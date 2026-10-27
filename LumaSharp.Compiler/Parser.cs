namespace LumaSharp.Compiler;

internal sealed class Parser(IReadOnlyList<Token> tokens, DiagnosticBag diagnostics, bool unsafeEnabled = false, CancellationToken cancellationToken = default)
{
    private static readonly HashSet<string> ReservedWords = [
        "bool", "char", "int", "uint", "int8", "int16", "int32", "int64", "uint8", "uint16", "uint32", "uint64", "float", "double", "object", "string", "void",
        "class", "struct", "interface", "enum", "delegate", "namespace", "using", "const", "let", "var", "extern", "operator",
        "public", "private", "protected", "internal", "static", "virtual", "override", "readonly", "mut", "unsafe", "ref", "in", "out",
        "if", "else", "switch", "case", "default", "while", "do", "for", "break", "continue", "return", "throw", "try", "catch", "finally",
        "new", "scope", "this", "base", "null", "true", "false", "is", "as", "sizeof", "checked", "unchecked", "async", "await", "lock"];
    private static readonly HashSet<string> TypeKeywords = [
        "bool", "char", "int", "uint", "int8", "int16", "int32", "int64", "uint8", "uint16", "uint32", "uint64", "float", "double", "object", "string", "void"];
    private int _position;
    private int _cancellationCheckCounter;
    private int _unsafeDepth;
    private int _pendingGenericClosers;
    private string _namespace = "";
    private readonly List<UsingDirective> _usingDirectives = [];
    private readonly HashSet<string> _usingAliases = new(StringComparer.Ordinal);
    private Token Current => tokens[Math.Min(_position, tokens.Count - 1)];
    private Token Peek(int n = 1) => tokens[Math.Min(_position + n, tokens.Count - 1)];
    private Token Take()
    {
        if ((++_cancellationCheckCounter & 0x3ff) == 0) cancellationToken.ThrowIfCancellationRequested();
        var token = Current;
        if (_position < tokens.Count - 1) _position++;
        return token;
    }
    private bool Is(string text) => Current.Text == text;
    private bool Eat(string text) { if (!Is(text)) return false; Take(); return true; }
    private Token Expect(string text) { if (Is(text)) return Take(); Error(Current, $"Expected '{text}', found '{Current.Text}'."); return new(TokenKind.Punctuation, text, Current.Location); }
    private void Error(Token token, string message) => diagnostics.Add("LS0001", DiagnosticSeverity.Error, message, token.Location);

    public CompilationUnit ParseCompilationUnit()
    {
        cancellationToken.ThrowIfCancellationRequested();
        var at = Current.Location; var types = new List<TypeDeclaration>();
        var sawNamespace = false;
        var sawTypeDeclaration = false;
        while (Current.Kind != TokenKind.End)
        {
            if (Is("using"))
            {
                var usingToken = Take();
                if (sawNamespace || sawTypeDeclaration)
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Using directives must appear before the namespace and all type declarations.", usingToken.Location);
                if (Eat("static"))
                {
                    var staticTarget = ReadQualifiedName(); Expect(";");
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Static using directive for '{staticTarget}' is not yet supported.", usingToken.Location);
                    continue;
                }
                string? alias = null;
                if (IsIdentifier(Current) && Peek().Text == "=") { alias = Take().Text.TrimStart('@'); Take(); }
                var target = alias is null ? ReadQualifiedName() : ParseTypeName(validateUnsafe: false) ?? string.Empty;
                if (target.Length == 0) Error(Current, "Expected a namespace or type name in using directive.");
                Expect(";");
                if (alias is not null && !_usingAliases.Add(alias)) diagnostics.Add("LS2001", DiagnosticSeverity.Error, $"Using alias '{alias}' is declared more than once.", usingToken.Location);
                _usingDirectives.Add(new(target, alias, usingToken.Location));
                continue;
            }
            if (Eat("namespace"))
            {
                var namespaceToken = tokens[Math.Max(0, _position - 1)];
                if (sawTypeDeclaration)
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "The file-scoped namespace declaration must precede every type declaration.", namespaceToken.Location);
                if (sawNamespace)
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A source file may declare only one file-scoped namespace.", namespaceToken.Location);
                sawNamespace = true;
                var name = ReadQualifiedName();
                if (Eat(";")) { _namespace = name; continue; }
                if (Eat("{"))
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Block-scoped namespace declarations are not supported; use one file-scoped namespace declaration.", namespaceToken.Location);
                    var previous = _namespace; _namespace = string.IsNullOrEmpty(previous) ? name : previous + "." + name;
                    ParseTypes(types, true); _namespace = previous; sawTypeDeclaration |= types.Count > 0; continue;
                }
                Error(Current, "Expected ';' or '{' after namespace name."); SynchronizeTop(); continue;
            }
            var before = _position;
            if (TryParseType(types)) { sawTypeDeclaration = true; continue; }
            if (_position == before) { Error(Current, $"Unexpected token '{Current.Text}' in compilation unit."); SynchronizeTop(); }
        }
        return new(types, at, _usingDirectives.ToArray(), _namespace);
    }
    private void ParseTypes(List<TypeDeclaration> types, bool untilClose)
    {
        while (Current.Kind != TokenKind.End && !(untilClose && Is("}")))
        {
            var before = _position;
            if (TryParseType(types)) continue;
            Error(Current, $"Expected a type declaration, found '{Current.Text}'."); SynchronizeTop();
            if (_position == before && Current.Kind != TokenKind.End) Take();
        }
        if (untilClose) Expect("}");
    }
    private bool TryParseType(List<TypeDeclaration> types, bool isNested = false)
    {
        var start = Current; var modifiers = ReadModifiers();
        var unsafeType = modifiers.Contains("unsafe");
        if (unsafeType && !unsafeEnabled) diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Unsafe code is disabled for this compilation.", start.Location);
        if (!(Is("class") || Is("struct") || Is("interface") || Is("enum") || Is("delegate"))) { _position = _position - modifiers.Count; return false; }
        var kind = Take().Text;
        if (kind == "delegate") return TryParseDelegateDeclaration(types, start, modifiers, isNested);
        var name = Take();
        if (!IsIdentifier(name)) { Error(name, "Expected a non-keyword type name (use @ to escape a keyword). "); SynchronizeTop(); return true; }
        ValidateModifiers(modifiers, start.Location, isTopLevelType: !isNested);
        ValidateAllowedModifiers(modifiers, kind switch
        {
            "class" => ["public", "private", "protected", "internal", "virtual", "static", "unsafe"],
            "struct" => ["public", "private", "protected", "internal", "unsafe"],
            _ => ["public", "private", "protected", "internal"]
        }, start.Location, $"{kind} declaration");
        if (kind == "enum") { ParseEnumType(types, name, start.Location, GetTypeAccessibility(modifiers, isNested), isNested ? _namespace : null); return true; }
        var typeParameters = ReadTypeParameterList();
        var generic = typeParameters.Count > 0; var hasBaseList = false; var baseTypes = new List<string>();
        if (Eat(":"))
        {
            hasBaseList = true;
            do
            {
                var baseType = ParseTypeName(validateUnsafe: false);
                if (baseType is null) { Error(Current, "Expected a base class or interface type."); break; }
                baseTypes.Add(baseType);
            } while (Eat(","));
        }
        var genericConstraints = ReadGenericConstraints(typeParameters);
        if (!Eat("{")) { Error(Current, "Expected '{' in type declaration."); SynchronizeTop(); return true; }
        var previousUnsafeDepth = _unsafeDepth; if (unsafeType) _unsafeDepth++;
        var members = new List<MemberDeclaration>();
        while (Current.Kind != TokenKind.End && !Is("}"))
        {
            var memberStart = Current; var memberModifiers = ReadModifiers();
            ValidateModifiers(memberModifiers, memberStart.Location, isTopLevelType: false);
            var unsafeMember = memberModifiers.Contains("unsafe");
            if (unsafeMember && !unsafeEnabled) diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Unsafe code is disabled for this compilation.", memberStart.Location);
            var previousMemberUnsafeDepth = _unsafeDepth; if (unsafeMember) _unsafeDepth++;
            if (Is("~"))
            {
                Take();
                if (!Eat("this")) Error(Current, "A finalizer must be written as '~this()'.");
                Expect("(");
                var finalizerParameters = ParseParameters();
                Expect(")");
                if (memberModifiers.Count > 0) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Finalizers cannot have modifiers or accessibility declarations.", memberStart.Location);
                if (kind != "class") diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Only classes may declare finalizers.", memberStart.Location);
                if (finalizerParameters.Count != 0) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A finalizer cannot declare parameters.", memberStart.Location);
                BlockStatement? finalizerBody = Is("{") ? ParseBlock() : null;
                if (finalizerBody is null) { Error(Current, "A finalizer requires a block body."); SkipMember(); }
                members.Add(new FinalizerDeclaration(finalizerBody, memberStart.Location));
                _unsafeDepth = previousMemberUnsafeDepth;
                continue;
            }
            if (Is("this") && Peek().Text == "(")
            {
                var staticConstructor = memberModifiers.Contains("static");
                ValidateAllowedModifiers(memberModifiers,
                    staticConstructor ? ["static"] : ["public", "private", "protected", "internal", "unsafe"],
                    memberStart.Location, staticConstructor ? "static constructor" : "constructor");
                Take(); Take(); var constructorParameters = ParseParameters(); Expect(")");
                if (staticConstructor && constructorParameters.Count > 0)
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A static constructor cannot declare parameters.", memberStart.Location);
                string? initializerKind = null;
                var initializerArguments = new List<Expression>();
                var initializerArgumentNames = new List<string?>();
                var initializerArgumentModes = new List<string?>();
                if (Eat(":"))
                {
                    if (Eat("this")) initializerKind = "this";
                    else if (Eat("base")) initializerKind = "base";
                    else Error(Current, "A constructor initializer must be ': this(...)' or ': base(...)'.");
                    if (Eat("("))
                    {
                        var sawNamedArgument = false;
                        while (!Is(")") && Current.Kind != TokenKind.End)
                        {
                            string? argumentName = null;
                            if (IsIdentifier(Current) && Peek().Text == ":")
                            {
                                argumentName = Take().Text.TrimStart('@'); Take(); sawNamedArgument = true;
                            }
                            else if (sawNamedArgument)
                                diagnostics.Add("LS0740", DiagnosticSeverity.Error, "A positional argument cannot follow a named argument.", Current.Location);
                            var argumentMode = Current.Text is "ref" or "in" or "out" ? Take().Text : null;
                            initializerArguments.Add(ParseExpression());
                            initializerArgumentNames.Add(argumentName);
                            initializerArgumentModes.Add(argumentMode);
                            if (!Eat(",")) break;
                        }
                        Expect(")");
                    }
                    else Error(Current, "A constructor initializer requires an argument list.");
                }
                BlockStatement? constructorBody = null;
                if (Is("{")) constructorBody = ParseBlock();
                else { Error(Current, "A constructor must have a block body."); SkipMember(); }
                if (memberModifiers.Contains("extern")) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Constructors cannot be extern.", memberStart.Location);
                var constructorAccessibility = string.Join(" ", memberModifiers.Where(modifier => modifier is "public" or "private" or "protected" or "internal"));
                if (constructorAccessibility.Length == 0) constructorAccessibility = "private";
                members.Add(new ConstructorDeclaration(name.Text.TrimStart('@'), constructorParameters, constructorBody, memberModifiers.Contains("static"), memberModifiers.Contains("public"), memberStart.Location,
                    initializerKind, initializerKind is null ? null : initializerArguments,
                    initializerKind is null || initializerArgumentNames.All(argumentName => argumentName is null) ? null : initializerArgumentNames,
                    initializerKind is null || initializerArgumentModes.All(argumentMode => argumentMode is null) ? null : initializerArgumentModes,
                    constructorAccessibility));
                _unsafeDepth = previousMemberUnsafeDepth; continue;
            }
            if (Is("class") || Is("struct") || Is("interface") || Is("enum") || Is("delegate"))
            {
                _unsafeDepth = previousMemberUnsafeDepth;
                _position -= memberModifiers.Count;
                var containingNamespace = _namespace;
                _namespace = string.IsNullOrEmpty(containingNamespace)
                    ? name.Text.TrimStart('@')
                    : containingNamespace + "." + name.Text.TrimStart('@');
                _ = TryParseType(types, isNested: true);
                _namespace = containingNamespace;
                continue;
            }
            if (Is("}")) { _unsafeDepth = previousMemberUnsafeDepth; break; }
            var returnType = ParseTypeName();
            if (returnType is null) { Error(Current, $"Unsupported member syntax beginning with '{Current.Text}'."); SkipMember(); members.Add(new UnsupportedMember("Unrecognized member", memberStart.Location)); _unsafeDepth = previousMemberUnsafeDepth; continue; }
            var memberName = Take();
            if (memberModifiers.Contains("const"))
            {
                ValidateAllowedModifiers(memberModifiers, ["public", "private", "protected", "internal", "const", "unsafe"], memberStart.Location, "const field");
                var fieldNames = new List<Token> { memberName };
                while (Eat(",")) fieldNames.Add(Take());
                if (!Eat("=")) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A const field must be initialized in its declaration.", memberStart.Location);
                var initializers = new List<Expression>();
                if (tokens[Math.Min(_position - 1, tokens.Count - 1)].Text == "=")
                {
                    initializers.Add(ParseExpression());
                    while (Eat(",")) initializers.Add(ParseExpression());
                }
                Expect(";");
                if (initializers.Skip(1).Any(e => e is BinaryExpression { Operator: "=" }))
                    diagnostics.Add("LS0214", DiagnosticSeverity.Error, "Multiple const fields use a single initializer list; list all field names before '='.", memberStart.Location);
                else if (initializers.Count != fieldNames.Count)
                    diagnostics.Add("LS0213", DiagnosticSeverity.Error, "Every const field in a multiple declaration must have an initializer.", memberStart.Location);
                for (var index = 0; index < Math.Min(fieldNames.Count, initializers.Count); index++)
                {
                    if (!IsIdentifier(fieldNames[index])) Error(fieldNames[index], "Expected a non-keyword const field name (use @ to escape a keyword).");
                    members.Add(new ConstantFieldDeclaration(returnType, fieldNames[index].Text.TrimStart('@'), initializers[index], memberModifiers.Contains("public"), memberStart.Location,
                        GetMemberAccessibility(memberModifiers)));
                }
                _unsafeDepth = previousMemberUnsafeDepth; continue;
            }
            string? explicitInterfaceName = null;
            if (Eat("."))
            {
                explicitInterfaceName = memberName.Text.TrimStart('@');
                memberName = Take();
                if (!IsIdentifier(memberName) && memberName.Text != "this") Error(memberName, "Expected a member name after the explicit interface qualifier.");
                if (kind is not ("class" or "struct"))
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Explicit interface implementations are only valid in classes and structs.", memberStart.Location);
                if (memberModifiers.Any(modifier => modifier != "unsafe"))
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An explicit interface implementation cannot declare accessibility or virtual/static modifiers.", memberStart.Location);
            }
            // Operator tokens such as '<' and '>>' immediately follow the member
            // name `operator`; they are never a method type-parameter list.
            IReadOnlyList<string> methodTypeParameters = memberName.Text == "operator" ? Array.Empty<string>() : ReadTypeParameterList();
            if (methodTypeParameters.Count > 0 && !Is("("))
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Type parameters may only appear on a method declaration.", memberStart.Location);
            if (memberName.Text == "this" && Is("["))
            {
                ValidateAllowedModifiers(memberModifiers, ["public", "private", "protected", "internal", "virtual", "override", "unsafe"], memberStart.Location, "indexer");
                var indexer = ParseIndexer(returnType, memberModifiers, kind, memberStart.Location);
                if (indexer is ComputedIndexerDeclaration computedIndexer && explicitInterfaceName is not null)
                    members.Add(computedIndexer with { ExplicitInterfaceName = explicitInterfaceName });
                else if (indexer is not null) members.Add(indexer);
                _unsafeDepth = previousMemberUnsafeDepth;
                continue;
            }
            if (Is("{") || Is("=>"))
            {
                ValidateAllowedModifiers(memberModifiers, ["public", "private", "protected", "internal", "static", "virtual", "override", "unsafe"], memberStart.Location, "property");
                var property = ParseProperty(returnType, memberName, memberModifiers, kind, memberStart.Location);
                if (property is ComputedPropertyDeclaration computedProperty && explicitInterfaceName is not null)
                    property = computedProperty with { ExplicitInterfaceName = explicitInterfaceName };
                if (property is ComputedPropertyDeclaration { UsesBackingField: true } backingProperty)
                    members.Add(new FieldDeclaration(backingProperty.TypeName, GetBackingFieldName(backingProperty.Name, backingProperty.ExplicitInterfaceName), backingProperty.BackingInitializer,
                        backingProperty.IsStatic, false, backingProperty.SetterBody is null, backingProperty.Location, "private"));
                if (property is not null) members.Add(property);
                else members.Add(new UnsupportedMember("Property accessor bodies", memberStart.Location));
                _unsafeDepth = previousMemberUnsafeDepth;
                continue;
            }
            if (memberName.Text == "operator")
            {
                ValidateAllowedModifiers(memberModifiers, ["public", "private", "protected", "internal", "static", "unsafe"], memberStart.Location, "operator");
                if (!memberModifiers.Contains("static"))
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An overloaded operator must be static.", memberStart.Location);
                var operatorToken = Take(); Expect("("); var operatorParameters = ParseParameters(); Expect(")");
                BlockStatement? operatorBody = null;
                if (Eat("=>")) { var expression = ParseExpression(); Expect(";"); operatorBody = new([new ReturnStatement(expression, expression.Location)], expression.Location); }
                else if (Is("{")) operatorBody = ParseBlock();
                else { Error(Current, "Expected an operator body or expression body."); SkipMember(); }
                if (operatorToken.Text == "!=")
                {
                    diagnostics.Add("LS0401", DiagnosticSeverity.Error, "The '!=' operator cannot be declared; it is defined as the negation of '=='.", operatorToken.Location);
                    members.Add(new UnsupportedMember("Invalid operator declaration", memberStart.Location));
                }
                else if (operatorToken.Text is not ("+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>" or "==" or "<" or ">" or "<=" or ">=" or "!" or "~" or "++" or "--"))
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Operator '{operatorToken.Text}' is recognized but is not yet supported.", operatorToken.Location);
                    members.Add(new UnsupportedMember("Unsupported operator declaration", memberStart.Location));
                }
                else members.Add(new MethodDeclaration(name.Text.TrimStart('@'), "$operator_" + OperatorMethodSuffix(operatorToken.Text), returnType, operatorParameters, operatorBody, memberModifiers.Contains("static"), memberModifiers.Contains("public"), false, false, memberStart.Location, unsafeMember, memberModifiers.Contains("extern"), true, operatorToken.Text,
                    Accessibility: GetMemberAccessibility(memberModifiers)));
                _unsafeDepth = previousMemberUnsafeDepth; continue;
            }
            if (!Is("(")) { ValidateAllowedModifiers(memberModifiers, ["public", "private", "protected", "internal", "static", "readonly", "const", "unsafe"], memberStart.Location, "field"); ParseFieldDeclaration(returnType, memberName, memberModifiers, memberStart.Location, members); _unsafeDepth = previousMemberUnsafeDepth; continue; }
            ValidateAllowedModifiers(memberModifiers, explicitInterfaceName is null
                ? ["public", "private", "protected", "internal", "static", "virtual", "override", "extern", "unsafe"]
                : ["unsafe"], memberStart.Location, "explicit interface method");
            Take(); var parameters = ParseParameters(); Expect(")");
            var hasTypeParameters = methodTypeParameters.Count > 0;
            var isMutating = Eat("mut");
            if (kind == "interface" && isMutating)
                diagnostics.Add("LS0824", DiagnosticSeverity.Error, "Interface methods cannot be declared mut; the implementing method may declare mut.", memberStart.Location);
            if (kind == "interface" && memberModifiers.Count > 0)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Interface methods cannot declare accessibility or implementation modifiers.", memberStart.Location);
            var methodConstraints = ReadGenericConstraints(methodTypeParameters);
            BlockStatement? body = null;
            if (Eat(";")) { }
            else if (Eat("=>")) { var expr = ParseExpression(); Expect(";"); body = new([returnType == "void" ? new ExpressionStatement(expr, expr.Location) : new ReturnStatement(expr, expr.Location)], expr.Location); }
            else if (Is("{")) body = ParseBlock();
            else { Error(Current, "Expected a method body, expression body, or ';'."); SkipMember(); }
            if (kind == "interface" && body is not null)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Interface methods cannot have a body.", memberStart.Location);
            _unsafeDepth = previousMemberUnsafeDepth;
            if (!IsIdentifier(memberName)) Error(memberName, "Expected a non-keyword method name (use @ to escape a keyword).");
            members.Add(new MethodDeclaration(name.Text.TrimStart('@'), memberName.Text.TrimStart('@'), returnType, parameters, body, memberModifiers.Contains("static"), memberModifiers.Contains("public"), isMutating, hasTypeParameters, memberStart.Location, unsafeMember, memberModifiers.Contains("extern"), IsVirtual: memberModifiers.Contains("virtual"), IsOverride: memberModifiers.Contains("override"), Accessibility: GetMemberAccessibility(memberModifiers), ExplicitInterfaceName: explicitInterfaceName, TypeParameters: methodTypeParameters, GenericConstraints: methodConstraints));
        }
        Expect("}"); _unsafeDepth = previousUnsafeDepth; types.Add(new(_namespace, kind, name.Text.TrimStart('@'), members, start.Location, generic, hasBaseList, _usingDirectives.ToArray(), BaseTypes: baseTypes, IsVirtual: modifiers.Contains("virtual"), IsStatic: modifiers.Contains("static"), Accessibility: GetTypeAccessibility(modifiers, isNested), TypeParameters: typeParameters, GenericConstraints: genericConstraints, ContainingTypeName: isNested ? _namespace : null)); return true;
    }
    private bool TryParseDelegateDeclaration(List<TypeDeclaration> types, Token start, IReadOnlyList<string> modifiers, bool isNested)
    {
        ValidateModifiers(modifiers, start.Location, isTopLevelType: !isNested);
        ValidateAllowedModifiers(modifiers, ["public", "private", "protected", "internal"], start.Location, "delegate declaration");
        var returnType = ParseTypeName();
        if (returnType is null)
        {
            Error(Current, "Expected a return type in delegate declaration.");
            SkipDeclaration();
            return true;
        }
        var name = Take();
        if (!IsIdentifier(name))
        {
            Error(name, "Expected a non-keyword delegate type name (use @ to escape a keyword).");
            SkipDeclaration();
            return true;
        }
        var typeParameters = ReadTypeParameterList();
        if (!Eat("("))
        {
            Error(Current, "Expected '(' after delegate type name.");
            SkipDeclaration();
            return true;
        }
        var parameters = ParseParameters();
        Expect(")");
        var constraints = ReadGenericConstraints(typeParameters);
        Expect(";");
        foreach (var parameter in parameters)
        {
            if (parameter.DefaultValue is not null)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Delegate parameters cannot be optional.", parameter.Location);
            if (parameter.IsVariadic)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Delegate parameters cannot be variadic.", parameter.Location);
        }
        types.Add(new(_namespace, "delegate", name.Text.TrimStart('@'), [], start.Location,
            HasTypeParameters: typeParameters.Count > 0, Usings: _usingDirectives.ToArray(),
            Accessibility: GetTypeAccessibility(modifiers, isNested), TypeParameters: typeParameters,
            GenericConstraints: constraints, DelegateReturnType: returnType, DelegateParameters: parameters,
            ContainingTypeName: isNested ? _namespace : null));
        return true;
    }
    private void ParseEnumType(List<TypeDeclaration> types, Token name, SourceLocation at, string accessibility, string? containingTypeName)
    {
        var underlying = "int32";
        if (Eat("<")) { diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Enums cannot have type parameters.", at); SkipBalanced("<", ">"); }
        if (Eat(":"))
        {
            underlying = ParseTypeName() ?? "?";
            if (underlying == "?") Error(Current, "Expected an integral enum underlying type.");
        }
        if (!Eat("{")) { Error(Current, "Expected '{' in enum declaration."); SynchronizeTop(); return; }
        var members = new List<MemberDeclaration>();
        while (Current.Kind != TokenKind.End && !Is("}"))
        {
            var member = Take();
            if (!IsIdentifier(member)) Error(member, "Expected an enum member name.");
            var initializer = Eat("=") ? ParseExpression() : null;
            members.Add(new EnumMemberDeclaration(member.Text.TrimStart('@'), initializer, member.Location));
            if (!Eat(",")) break;
        }
        Expect("}");
        types.Add(new TypeDeclaration(_namespace, "enum", name.Text.TrimStart('@'), members, at, Usings: _usingDirectives.ToArray(), EnumUnderlyingType: underlying, Accessibility: accessibility,
            ContainingTypeName: containingTypeName));
    }
    private void ParseFieldDeclaration(string typeName, Token firstName, IReadOnlyList<string> modifiers, SourceLocation at, List<MemberDeclaration> members)
    {
        var names = new List<Token> { firstName };
        while (Eat(",")) names.Add(Take());
        var initializers = new List<Expression>();
        if (Eat("="))
        {
            initializers.Add(SemanticAnalyzer.TryParseInlineArrayType(typeName, out _, out _) && Is("{")
                ? ParseInlineArrayInitializer() : ParseExpression());
            while (Eat(","))
                initializers.Add(SemanticAnalyzer.TryParseInlineArrayType(typeName, out _, out _) && Is("{")
                    ? ParseInlineArrayInitializer() : ParseExpression());
        }
        Expect(";");
        if (initializers.Skip(1).Any(expression => expression is BinaryExpression { Operator: "=" }))
            diagnostics.Add("LS0214", DiagnosticSeverity.Error, "Multiple fields use a single initializer list; list all field names before '='.", at);
        else if (initializers.Count > names.Count)
            diagnostics.Add("LS0212", DiagnosticSeverity.Error, "A field declaration has more initializers than identifiers.", at);
        for (var index = 0; index < names.Count; index++)
        {
            var name = names[index];
            if (!IsIdentifier(name)) Error(name, "Expected a non-keyword field name (use @ to escape a keyword).");
            members.Add(new FieldDeclaration(typeName, name.Text.TrimStart('@'), index < initializers.Count ? initializers[index] : null,
                modifiers.Contains("static"), modifiers.Contains("public"), modifiers.Contains("readonly"), at, GetMemberAccessibility(modifiers)));
        }
    }
    private static string OperatorMethodSuffix(string op) => op switch
    {
        "+" => "add", "-" => "subtract", "*" => "multiply", "/" => "divide", "%" => "remainder",
        "&" => "bitand", "|" => "bitor", "^" => "xor", "<<" => "leftshift", ">>" => "rightshift",
        "==" => "eq", "<" => "less", ">" => "greater", "<=" => "lessequal", ">=" => "greaterequal",
        "!" => "logicalnot", "~" => "complement", "++" => "increment", "--" => "decrement",
        _ => throw new ArgumentOutOfRangeException(nameof(op), op, "Unsupported overloaded operator.")
    };
    private static string GetBackingFieldName(string propertyName, string? explicitInterfaceName) =>
        "$backing_" + propertyName + (explicitInterfaceName is null ? "" : "$" + explicitInterfaceName);
    private MemberDeclaration? ParseProperty(string typeName, Token name, IReadOnlyList<string> modifiers, string containingTypeKind, SourceLocation at)
    {
        if (Eat("=>"))
        {
            var expression = ParseExpression();
            Expect(";");
            if (containingTypeKind == "interface")
            {
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Interface properties cannot have a body.", at);
                return new ComputedPropertyDeclaration(typeName, name.Text.TrimStart('@'),
                    new BlockStatement([], at), null, false, false, false, at, "public", GetterAccessibility: "public");
            }
            if (containingTypeKind is not ("class" or "struct"))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Computed properties are not supported in {containingTypeKind} declarations.", at);
                return null;
            }
            return new ComputedPropertyDeclaration(typeName, name.Text.TrimStart('@'),
                new([new ReturnStatement(expression, expression.Location)], expression.Location), null,
                modifiers.Contains("static"), modifiers.Contains("virtual"), modifiers.Contains("override"), at,
                GetMemberAccessibility(modifiers), ContainsName(expression, "field"), GetMemberAccessibility(modifiers));
        }

        Expect("{");
        var hasGetter = false;
        var hasSetter = false;
        var hasAutoAccessor = false;
        var hasExplicitAccessor = false;
        var usesBackingField = false;
        string? getterAccessibility = null;
        string? setterAccessibility = null;
        var getterIsAuto = false;
        var setterIsAuto = false;
        BlockStatement? getterBody = null;
        BlockStatement? setterBody = null;
        var supported = containingTypeKind is "class" or "struct" or "interface";
        while (Current.Kind != TokenKind.End && !Is("}"))
        {
            var accessorAt = Current.Location;
            var accessorModifiers = new List<string>();
            while (Current.Text is "public" or "private" or "protected" or "internal") accessorModifiers.Add(Take().Text);
            var accessor = Take();
            if (accessor.Text is not ("get" or "set"))
            {
                Error(accessor, "Expected 'get' or 'set' in a property accessor list.");
                supported = false;
                SkipMemberAfterHead();
                break;
            }

            if (accessor.Text == "get")
            {
                if (hasGetter) diagnostics.Add("LS2001", DiagnosticSeverity.Error, "A property cannot declare more than one get accessor.", accessor.Location);
                hasGetter = true;
            }
            else
            {
                if (hasSetter) diagnostics.Add("LS2001", DiagnosticSeverity.Error, "A property cannot declare more than one set accessor.", accessor.Location);
                hasSetter = true;
            }

            ValidateModifiers(accessorModifiers, accessorAt, isTopLevelType: false);
            var accessorAccessibility = accessorModifiers.Count == 0 ? null : GetMemberAccessibility(accessorModifiers);
            if (accessor.Text == "get") getterAccessibility = accessorAccessibility;
            else setterAccessibility = accessorAccessibility;

            BlockStatement? body;
            if (Eat(";"))
            {
                hasAutoAccessor = true;
                if (accessor.Text == "get") getterIsAuto = true;
                else setterIsAuto = true;
                continue;
            }
            hasExplicitAccessor = true;
            if (Eat("=>"))
            {
                var expression = ParseExpression();
                Expect(";");
                body = accessor.Text == "get"
                    ? new([new ReturnStatement(expression, expression.Location)], expression.Location)
                    : new([new ExpressionStatement(expression, expression.Location)], expression.Location);
            }
            else if (Is("{")) body = ParseBlock();
            else
            {
                Error(Current, "Expected ';', an expression body, or a block body after a property accessor.");
                SkipMemberAfterHead();
                break;
            }
            if (accessor.Text == "get") getterBody = body;
            else setterBody = body;
            usesBackingField |= ContainsName(body, "field");
        }
        Expect("}");
        var initializer = Eat("=") ? ParseExpression() : null;
        if (initializer is not null) Expect(";");

        if (!hasGetter && !hasSetter)
        {
            diagnostics.Add("LS2001", DiagnosticSeverity.Error, "A property must declare at least one accessor.", at);
            supported = false;
        }
        if (!supported)
        {
            if (containingTypeKind is not ("class" or "struct" or "interface"))
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Properties are not supported in {containingTypeKind} declarations.", at);
            return null;
        }
        if (containingTypeKind == "interface")
        {
            if (hasExplicitAccessor || initializer is not null || modifiers.Count > 0 || getterAccessibility is not null || setterAccessibility is not null)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Interface properties must be bodyless and cannot declare accessibility modifiers or initializers.", at);
            return new ComputedPropertyDeclaration(typeName, name.Text.TrimStart('@'),
                hasGetter ? new BlockStatement([], at) : null, hasSetter ? new BlockStatement([], at) : null,
                false, false, false, at, "public", GetterAccessibility: hasGetter ? "public" : null,
                SetterAccessibility: hasSetter ? "public" : null);
        }
        if (modifiers.Contains("virtual") || modifiers.Contains("override"))
        {
            // Auto accessors are lowered to ordinary virtual accessor methods below.
        }
        var propertyAccessibility = GetMemberAccessibility(modifiers);
        var effectiveGetterAccessibility = getterAccessibility ?? propertyAccessibility;
        var effectiveSetterAccessibility = setterAccessibility ?? propertyAccessibility;
        ValidateAccessorAccessibility(propertyAccessibility, getterAccessibility, at);
        ValidateAccessorAccessibility(propertyAccessibility, setterAccessibility, at);
        if (getterIsAuto)
        {
            var backing = new NameExpression("field", at);
            getterBody = new([new ReturnStatement(backing, at)], at);
            usesBackingField = true;
        }
        if (setterIsAuto)
        {
            var store = new BinaryExpression(new NameExpression("field", at), "=", new NameExpression("value", at), at);
            setterBody = new([new ExpressionStatement(store, at)], at);
            usesBackingField = true;
        }
        if (initializer is not null && !hasAutoAccessor && !usesBackingField)
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "A computed property without compiler backing storage cannot have an initializer; initialize storage in the accessor body.", initializer.Location);
        return new ComputedPropertyDeclaration(typeName, name.Text.TrimStart('@'), getterBody, setterBody,
            modifiers.Contains("static"), modifiers.Contains("virtual"), modifiers.Contains("override"), at,
            propertyAccessibility, usesBackingField, effectiveGetterAccessibility, effectiveSetterAccessibility,
            usesBackingField ? initializer : null, setterIsAuto);
    }
    private MemberDeclaration? ParseIndexer(string typeName, IReadOnlyList<string> modifiers, string containingTypeKind, SourceLocation at)
    {
        Expect("[");
        var parameters = ParseParameters("]");
        Expect("]");
        if (Eat("=>"))
        {
            var expression = ParseExpression();
            Expect(";");
            if (containingTypeKind == "interface")
            {
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Interface indexers cannot have a body.", at);
                return new ComputedIndexerDeclaration(typeName, parameters,
                    new BlockStatement([], at), null, false, false, at, "public", "public");
            }
            if (containingTypeKind is not ("class" or "struct"))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Indexers are not supported in {containingTypeKind} declarations.", at);
                return null;
            }
            if (parameters.Count == 0) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An indexer must declare at least one parameter.", at);
            if (modifiers.Contains("static")) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An indexer cannot be static.", at);
            var indexerAccessibility = GetMemberAccessibility(modifiers);
            return new ComputedIndexerDeclaration(typeName, parameters,
                new([new ReturnStatement(expression, expression.Location)], expression.Location), null,
                modifiers.Contains("virtual"), modifiers.Contains("override"), at, indexerAccessibility, indexerAccessibility);
        }
        if (!Eat("{"))
        {
            Error(Current, "Expected an accessor list after indexer parameters.");
            SkipMember();
            return null;
        }
        var hasGetter = false;
        var hasSetter = false;
        var hasBody = false;
        BlockStatement? getterBody = null;
        BlockStatement? setterBody = null;
        string? getterAccessibility = null;
        string? setterAccessibility = null;
        while (Current.Kind != TokenKind.End && !Is("}"))
        {
            var accessorAt = Current.Location;
            var accessorModifiers = new List<string>();
            while (Current.Text is "public" or "private" or "protected" or "internal") accessorModifiers.Add(Take().Text);
            var accessor = Take();
            if (accessor.Text is not ("get" or "set"))
            {
                Error(accessor, "Expected 'get' or 'set' in an indexer accessor list.");
                SkipMemberAfterHead();
                break;
            }
            if (accessor.Text == "get")
            {
                if (hasGetter) diagnostics.Add("LS2001", DiagnosticSeverity.Error, "An indexer cannot declare more than one get accessor.", accessor.Location);
                hasGetter = true;
            }
            else
            {
                if (hasSetter) diagnostics.Add("LS2001", DiagnosticSeverity.Error, "An indexer cannot declare more than one set accessor.", accessor.Location);
                hasSetter = true;
            }
            ValidateModifiers(accessorModifiers, accessorAt, isTopLevelType: false);
            var accessorAccessibility = accessorModifiers.Count == 0 ? null : GetMemberAccessibility(accessorModifiers);
            if (accessor.Text == "get") getterAccessibility = accessorAccessibility;
            else setterAccessibility = accessorAccessibility;
            if (Eat(";"))
            {
                if (containingTypeKind != "interface")
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Auto-indexer accessors are not supported; provide accessor bodies.", accessorAt);
                continue;
            }
            hasBody = true;
            BlockStatement body;
            if (Eat("=>"))
            {
                var expression = ParseExpression();
                Expect(";");
                body = accessor.Text == "get"
                    ? new([new ReturnStatement(expression, expression.Location)], expression.Location)
                    : new([new ExpressionStatement(expression, expression.Location)], expression.Location);
            }
            else if (Is("{")) body = ParseBlock();
            else
            {
                Error(Current, "Expected ';', an expression body, or a block body after the indexer accessor.");
                SkipMemberAfterHead();
                break;
            }
            if (accessor.Text == "get") getterBody = body;
            else setterBody = body;
        }
        Expect("}");
        if (!hasGetter && !hasSetter) diagnostics.Add("LS2001", DiagnosticSeverity.Error, "An indexer must declare at least one accessor.", at);
        if (containingTypeKind == "interface")
        {
            if (hasBody || modifiers.Count > 0 || getterAccessibility is not null || setterAccessibility is not null)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Interface indexers must be bodyless and cannot declare accessibility modifiers.", at);
            if (parameters.Count == 0) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An indexer must declare at least one parameter.", at);
            return new ComputedIndexerDeclaration(typeName, parameters,
                hasGetter ? new BlockStatement([], at) : null, hasSetter ? new BlockStatement([], at) : null,
                false, false, at, "public", hasGetter ? "public" : null, hasSetter ? "public" : null);
        }
        if (containingTypeKind is not ("class" or "struct"))
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Indexers are not supported in {containingTypeKind} declarations.", at);
            return null;
        }
        if (parameters.Count == 0) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An indexer must declare at least one parameter.", at);
        if (modifiers.Contains("static")) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An indexer cannot be static.", at);
        var memberAccessibility = GetMemberAccessibility(modifiers);
        ValidateAccessorAccessibility(memberAccessibility, getterAccessibility, at);
        ValidateAccessorAccessibility(memberAccessibility, setterAccessibility, at);
        if (!hasBody) return null;
        return new ComputedIndexerDeclaration(typeName, parameters, getterBody, setterBody,
            modifiers.Contains("virtual"), modifiers.Contains("override"), at, memberAccessibility,
            getterAccessibility ?? memberAccessibility, setterAccessibility ?? memberAccessibility);
    }
    private static bool ContainsName(BlockStatement body, string name) => body.Statements.Any(statement => ContainsName(statement, name));
    private static bool ContainsName(Statement statement, string name) => statement switch
    {
        BlockStatement block => block.Statements.Any(item => ContainsName(item, name)),
        ReturnStatement returned => returned.Value is not null && ContainsName(returned.Value, name),
        VariableStatement variable => variable.Initializer is not null && ContainsName(variable.Initializer, name),
        VariableDeclarationStatement declaration => declaration.Variables.Any(variable => variable.Initializer is not null && ContainsName(variable.Initializer, name)),
        MultiAssignmentStatement assignment => assignment.Targets.Any(target => ContainsName(target, name)) || assignment.Values.Any(value => ContainsName(value, name)),
        ExpressionStatement expression => ContainsName(expression.Value, name),
        IfStatement conditional => ContainsName(conditional.Condition, name) || ContainsName(conditional.Then, name) || conditional.Else is not null && ContainsName(conditional.Else, name),
        WhileStatement loop => ContainsName(loop.Condition, name) || ContainsName(loop.Body, name),
        DoWhileStatement loop => ContainsName(loop.Body, name) || ContainsName(loop.Condition, name),
        ForStatement loop => loop.Initializer is not null && ContainsName(loop.Initializer, name) || loop.Condition is not null && ContainsName(loop.Condition, name) || loop.Iterators.Any(item => ContainsName(item, name)) || ContainsName(loop.Body, name),
        ForInStatement loop => ContainsName(loop.Collection, name) || ContainsName(loop.Body, name),
        SwitchStatement selection => ContainsName(selection.Value, name) || selection.Sections.Any(section => section.Labels.Any(label => label.Value is not null && ContainsName(label.Value, name) || label.Guard is not null && ContainsName(label.Guard, name)) || section.Statements.Any(item => ContainsName(item, name))),
        TryStatement attempt => ContainsName(attempt.Body, name) || attempt.Catches.Any(clause => ContainsName(clause.Body, name)) || attempt.Finally is not null && ContainsName(attempt.Finally, name),
        ThrowStatement thrown => thrown.Value is not null && ContainsName(thrown.Value, name),
        OverflowContextStatement overflow => ContainsName(overflow.Body, name),
        _ => false
    };
    private static bool ContainsName(Expression expression, string name) => expression switch
    {
        NameExpression identifier => identifier.Name == name,
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(item => ContainsName(item, name)),
        AllocationExpression allocation => allocation.Dimensions.Any(item => ContainsName(item, name)) || allocation.Initializer is not null && ContainsName(allocation.Initializer, name) || allocation.ConstructorArguments?.Any(item => ContainsName(item, name)) == true,
        UnaryExpression unary => ContainsName(unary.Operand, name),
        CastExpression cast => ContainsName(cast.Operand, name),
        BinaryExpression binary => ContainsName(binary.Left, name) || ContainsName(binary.Right, name),
        ConditionalExpression conditional => ContainsName(conditional.Condition, name) || ContainsName(conditional.WhenTrue, name) || ContainsName(conditional.WhenFalse, name),
        MemberAccessExpression member => ContainsName(member.Receiver, name),
        IndexExpression index => ContainsName(index.Receiver, name) || index.Indices.Any(item => ContainsName(item, name)),
        CallExpression call => ContainsName(call.Target, name) || call.Arguments.Any(item => ContainsName(item, name)),
        OverflowContextExpression overflow => ContainsName(overflow.Value, name),
        NullAssertionExpression assertion => ContainsName(assertion.Operand, name),
        _ => false
    };
    private List<string> ReadModifiers()
    {
        var result = new List<string>();
        while (Current.Text is "public" or "private" or "protected" or "internal" or "static" or "virtual" or "override" or "extern" or "unsafe" or "readonly" or "const") result.Add(Take().Text);
        return result;
    }
    private static string GetMemberAccessibility(IReadOnlyList<string> modifiers)
    {
        var access = string.Join(" ", modifiers.Where(modifier => modifier is "public" or "private" or "protected" or "internal"));
        return access.Length == 0 ? "private" : access;
    }
    private void ValidateAccessorAccessibility(string memberAccessibility, string? accessorAccessibility, SourceLocation location)
    {
        if (accessorAccessibility is null) return;
        static int Rank(string accessibility) => accessibility switch
        {
            "public" => 4,
            "protected internal" => 3,
            "protected" or "internal" => 2,
            "private protected" => 1,
            "private" => 0,
            _ => 0
        };
        if (Rank(accessorAccessibility) >= Rank(memberAccessibility))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An accessor accessibility modifier must be more restrictive than the containing property or indexer.", location);
    }
    private static string GetTypeAccessibility(IReadOnlyList<string> modifiers, bool isNested = false)
    {
        var access = string.Join(" ", modifiers.Where(modifier => modifier is "public" or "private" or "protected" or "internal"));
        return access.Length == 0 ? isNested ? "private" : "internal" : access;
    }
    private void ValidateModifiers(IReadOnlyList<string> modifiers, SourceLocation location, bool isTopLevelType)
    {
        foreach (var duplicate in modifiers.GroupBy(modifier => modifier, StringComparer.Ordinal).Where(group => group.Count() > 1))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Modifier '{duplicate.Key}' cannot be repeated.", location);
        var access = modifiers.Where(modifier => modifier is "public" or "private" or "protected" or "internal").ToArray();
        var validPair = access.Length == 2 &&
            (access.Contains("protected", StringComparer.Ordinal) && access.Contains("internal", StringComparer.Ordinal) ||
             access.Contains("private", StringComparer.Ordinal) && access.Contains("protected", StringComparer.Ordinal));
        if (access.Length > 1 && !validPair)
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Accessibility modifiers cannot be combined in this form.", location);
        if (isTopLevelType && access.Any(modifier => modifier is "private" or "protected"))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A top-level type may be public or internal, but cannot be private or protected.", location);
        if (modifiers.Contains("virtual") && modifiers.Contains("override") ||
            modifiers.Contains("static") && (modifiers.Contains("virtual") || modifiers.Contains("override")))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "'static', 'virtual', and 'override' cannot be combined in this declaration.", location);
        if (modifiers.Contains("const") && modifiers.Contains("readonly"))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "'const' and 'readonly' cannot be combined.", location);
        if (modifiers.Contains("const") && modifiers.Contains("static"))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A const field is implicitly static; do not write 'static const'.", location);
    }
    private void ValidateAllowedModifiers(IReadOnlyList<string> modifiers, IReadOnlyCollection<string> allowed, SourceLocation location, string declarationKind)
    {
        foreach (var modifier in modifiers.Where(modifier => !allowed.Contains(modifier, StringComparer.Ordinal)).Distinct(StringComparer.Ordinal))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Modifier '{modifier}' is not valid on a {declarationKind}.", location);
    }
    private string? ParseTypeName(bool validateUnsafe = true, bool consumeArraySuffixes = true, bool consumeNullableSuffix = true)
    {
        if (Current.Kind != TokenKind.Identifier || (ReservedWords.Contains(Current.Text.TrimStart('@')) && !TypeKeywords.Contains(Current.Text.TrimStart('@')) && !Current.Text.StartsWith('@'))) return null;
        var type = Take().Text.TrimStart('@');
        if (Eat(".")) { if (!IsIdentifier(Current)) return type; type += "." + Take().Text.TrimStart('@'); while (Eat(".")) { if (IsIdentifier(Current)) type += "." + Take().Text.TrimStart('@'); else break; } }
        if (Eat("<"))
        {
            var arguments = new List<string>();
            do
            {
                var argument = ParseTypeName(validateUnsafe);
                if (argument is null)
                {
                    Error(Current, "Expected a type argument.");
                    break;
                }
                arguments.Add(argument);
            } while (Eat(","));
            if (!ConsumeGenericClose()) Error(Current, "Expected '>' after generic type arguments.");
            type += "<" + string.Join(",", arguments) + ">";
        }
        while (true)
        {
            // When the lexer combines adjacent generic closers into `>>`, the
            // inner type must leave any following nullable/array/pointer suffix
            // for the enclosing type after it consumes the deferred outer `>`.
            if (_pendingGenericClosers > 0) break;
            if (consumeNullableSuffix && Is("?[") )
            {
                Take(); type += "?[";
                while (Current.Kind != TokenKind.End && !Is("]")) type += Take().Text;
                Expect("]"); type += "]";
                continue;
            }
            if (consumeNullableSuffix && Eat("?")) { type += "?"; continue; }
            if (Eat("*")) { type += "*"; continue; }
            if (consumeArraySuffixes && Eat("["))
            {
                type += "[";
                while (Current.Kind != TokenKind.End && !Is("]")) type += Take().Text;
                Expect("]"); type += "]";
                continue;
            }
            break;
        }
        if (validateUnsafe && SemanticAnalyzer.TryParseInlineArrayType(type, out _, out _) && _unsafeDepth == 0)
        {
            if (unsafeEnabled) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Inline array types may only be declared in an unsafe context.", Current.Location);
            else diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Unsafe code is disabled for this compilation.", Current.Location);
        }
        if (validateUnsafe && type.EndsWith('*'))
        {
            if (_unsafeDepth == 0)
            {
                if (unsafeEnabled) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Pointer types may only be declared in an unsafe context.", Current.Location);
                else diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Unsafe code is disabled for this compilation.", Current.Location);
            }
        }
        return type;
    }
    private bool ConsumeGenericClose()
    {
        if (_pendingGenericClosers > 0)
        {
            _pendingGenericClosers--;
            return true;
        }
        if (Eat(">")) return true;
        if (Eat(">>"))
        {
            _pendingGenericClosers++;
            return true;
        }
        return false;
    }
    private IReadOnlyList<ParameterDeclaration> ParseParameters(string closingToken = ")")
    {
        var result = new List<ParameterDeclaration>();
        while (!Is(closingToken) && Current.Kind != TokenKind.End)
        {
            var at = Current.Location;
            var modifiers = new List<string>();
            while (Current.Text is "ref" or "in" or "out" or "readonly") modifiers.Add(Take().Text);
            var passingModes = modifiers.Where(modifier => modifier is "ref" or "in" or "out").Distinct(StringComparer.Ordinal).ToArray();
            if (passingModes.Length > 1 || modifiers.Count != modifiers.Distinct(StringComparer.Ordinal).Count()) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A parameter may have at most one of ref, in, or out and may not repeat modifiers.", at);
            var type = ParseTypeName(); if (type is null) { Error(Current, "Expected a parameter type."); break; }
            var isVariadic = Eat("...");
            if (isVariadic) type += "[]";
            var name = Take(); if (!IsIdentifier(name)) { Error(name, "Expected a non-keyword parameter name (use @ to escape a keyword)."); break; }
            var defaultValue = Eat("=") ? ParseExpression() : null;
            result.Add(new(type, name.Text.TrimStart('@'), at, defaultValue, passingModes.FirstOrDefault() ?? "", modifiers.Contains("readonly", StringComparer.Ordinal), isVariadic));
            if (!Eat(",")) break;
        }
        return result;
    }
    private void SkipBalanced(string open, string close)
    {
        var depth = 1; while (Current.Kind != TokenKind.End && depth > 0) { var t = Take(); if (t.Text == open) depth++; else if (t.Text == close) depth--; }
    }
    private IReadOnlyList<string> ReadTypeParameterList()
    {
        if (!Eat("<")) return [];
        var parameters = new List<string>();
        while (Current.Kind != TokenKind.End && !Is(">"))
        {
            var parameter = Take();
            if (!IsIdentifier(parameter))
            {
                Error(parameter, "Expected a type parameter name.");
                while (Current.Kind != TokenKind.End && !Is(",") && !Is(">")) Take();
            }
            else
            {
                var name = parameter.Text.TrimStart('@');
                if (parameters.Contains(name, StringComparer.Ordinal))
                    diagnostics.Add("LS2001", DiagnosticSeverity.Error, $"Type parameter '{name}' is declared more than once.", parameter.Location);
                else parameters.Add(name);
            }
            if (!Eat(",")) break;
        }
        Expect(">");
        if (parameters.Count == 0)
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A generic declaration must declare at least one type parameter.", Current.Location);
        return parameters;
    }
    private IReadOnlyList<GenericConstraintDeclaration> ReadGenericConstraints(IReadOnlyList<string> typeParameters)
    {
        var constraints = new List<GenericConstraintDeclaration>();
        var constrainedParameters = new HashSet<string>(StringComparer.Ordinal);
        while (Eat("where"))
        {
            var at = Current.Location;
            var parameterToken = Take();
            if (!IsIdentifier(parameterToken))
            {
                Error(parameterToken, "Expected a constrained type parameter name.");
                while (Current.Kind != TokenKind.End && !Is(":") && !Is("{") && !Is(";")) Take();
                if (!Eat(":")) continue;
            }
            var parameterName = parameterToken.Text.TrimStart('@');
            if (!typeParameters.Contains(parameterName, StringComparer.Ordinal))
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Constraint refers to undeclared type parameter '{parameterName}'.", parameterToken.Location);
            if (!constrainedParameters.Add(parameterName))
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Type parameter '{parameterName}' has more than one constraint clause.", parameterToken.Location);
            Expect(":");
            var items = new List<string>();
            do
            {
                if (Current.Text is "class" or "struct" or "enum")
                {
                    items.Add(Take().Text);
                    continue;
                }
                if (Eat("new"))
                {
                    Expect("(");
                    var constructorTypes = new List<string>();
                    while (!Is(")") && Current.Kind != TokenKind.End)
                    {
                        var constructorType = ParseTypeName(validateUnsafe: false);
                        if (constructorType is null)
                        {
                            Error(Current, "Expected a constructor constraint parameter type.");
                            break;
                        }
                        constructorTypes.Add(constructorType);
                        if (!Eat(",")) break;
                    }
                    Expect(")");
                    items.Add("new(" + string.Join(",", constructorTypes) + ")");
                    continue;
                }
                var typeConstraint = ParseTypeName(validateUnsafe: false);
                if (typeConstraint is null)
                {
                    Error(Current, "Expected a type constraint.");
                    break;
                }
                items.Add(typeConstraint);
            } while (Eat(","));
            if (items.Count == 0)
                Error(Current, "A constraint clause must contain at least one constraint.");
            var primaryConstraints = items.Select((item, index) => (item, index))
                .Where(entry => entry.item is "class" or "struct" or "enum").ToArray();
            if (primaryConstraints.Length > 1)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Type parameter '{parameterName}' may have only one primary constraint.", at);
            if (primaryConstraints.FirstOrDefault() is var primary && primary.index > 0)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"The primary constraint for '{parameterName}' must appear before type and constructor constraints.", at);
            var constructorConstraint = items.FindIndex(item => item.StartsWith("new(", StringComparison.Ordinal));
            if (constructorConstraint >= 0 && constructorConstraint != items.Count - 1)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"The constructor constraint for '{parameterName}' must be last.", at);
            if (items.Count != items.Distinct(StringComparer.Ordinal).Count())
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Type parameter '{parameterName}' has a duplicate constraint.", at);
            constraints.Add(new(parameterName, items, at));
        }
        return constraints;
    }
    private string ReadQualifiedName()
    {
        var parts = new List<string>(); if (IsIdentifier(Current)) parts.Add(Take().Text.TrimStart('@'));
        while (Eat(".")) { if (IsIdentifier(Current)) parts.Add(Take().Text.TrimStart('@')); else { Error(Current, "Expected an identifier after '.'."); break; } }
        return string.Join('.', parts);
    }
    private static bool IsIdentifier(Token token) => token.Kind == TokenKind.Identifier && (token.Text.StartsWith('@') || !ReservedWords.Contains(token.Text));
    private void SkipUntil(string token) { while (Current.Kind != TokenKind.End && !Is(token)) Take(); }
    private void SkipNestedType() { SkipUntil("{"); if (!Eat("{")) { SkipUntil(";"); Eat(";"); return; } var depth = 1; while (Current.Kind != TokenKind.End && depth > 0) { if (Eat("{")) depth++; else if (Eat("}")) depth--; else Take(); } }
    private void SkipDeclaration() { SkipUntil("{"); if (Eat("{")) { var depth = 1; while (Current.Kind != TokenKind.End && depth > 0) { if (Eat("{")) depth++; else if (Eat("}")) depth--; else Take(); } } else { SkipUntil(";"); Eat(";"); } }
    private void SkipMemberAfterHead() { var depth = 0; while (Current.Kind != TokenKind.End) { if (depth == 0 && Eat(";")) return; if (depth == 0 && Is("}")) return; if (Eat("{")) { depth++; continue; } if (Eat("}")) { if (--depth <= 0) return; continue; } Take(); } }
    private void SkipMember() { SkipMemberAfterHead(); }
    private void SynchronizeTop() { while (Current.Kind != TokenKind.End && !Is("class") && !Is("struct") && !Is("interface") && !Is("enum") && !Is("delegate")) Take(); }

    private BlockStatement ParseBlock()
    {
        var at = Expect("{").Location; var statements = new List<Statement>();
        while (Current.Kind != TokenKind.End && !Is("}")) { var before = _position; statements.Add(ParseStatement()); if (_position == before) Take(); }
        Expect("}"); return new(statements, at);
    }
    private Statement ParseStatement()
    {
        var at = Current.Location;
        if (Is("(") && LooksLikeTupleTypeDeclaration())
        {
            ReportUnsupportedTupleTypeDeclaration();
            return new EmptyStatement(at);
        }
        if (IsIdentifier(Current) && Peek().Text == ":")
        {
            var label = Take(); Take();
            if (Is("while") || Is("do") || Is("for"))
            {
                var loop = ParseStatement();
                return loop switch
                {
                    WhileStatement statement => statement with { Label = label.Text.TrimStart('@') },
                    DoWhileStatement statement => statement with { Label = label.Text.TrimStart('@') },
                    ForStatement statement => statement with { Label = label.Text.TrimStart('@') },
                    ForInStatement statement => statement with { Label = label.Text.TrimStart('@') },
                    _ => loop
                };
            }
            diagnostics.Add("LS0602", DiagnosticSeverity.Error, $"Loop label '{label.Text}' must be followed by a while, do, or for loop.", label.Location);
            return ParseStatement();
        }
        if (Is("{")) return ParseBlock();
        if (Eat(";")) return new EmptyStatement(at);
        if (Eat("return")) { var expr = Is(";") ? null : ParseExpression(); Expect(";"); return new ReturnStatement(expr, at); }
        if (Eat("throw")) { var expr = Is(";") ? null : ParseExpression(); Expect(";"); return new ThrowStatement(expr, at); }
        if (Eat("try")) return ParseTry(at);
        if (Eat("if")) { Expect("("); var condition = ParseExpression(); Expect(")"); var then = ParseStatement(); return new IfStatement(condition, then, Eat("else") ? ParseStatement() : null, at); }
        if (Eat("while")) { Expect("("); var condition = ParseExpression(); Expect(")"); return new WhileStatement(condition, ParseStatement(), at); }
        if (Eat("do")) { var body = ParseStatement(); Expect("while"); Expect("("); var condition = ParseExpression(); Expect(")"); Expect(";"); return new DoWhileStatement(body, condition, at); }
        if (Eat("for")) return ParseFor(at);
        if (Eat("break")) { var label = IsIdentifier(Current) ? Take().Text.TrimStart('@') : null; Expect(";"); return new BreakStatement(label, false, at); }
        if (Eat("continue")) { var label = IsIdentifier(Current) ? Take().Text.TrimStart('@') : null; Expect(";"); return new BreakStatement(label, true, at); }
        if (Is("unsafe"))
        {
            var token = Take(); if (!unsafeEnabled) diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Unsafe code is disabled for this compilation.", token.Location);
            if (!Is("{") && !(Is("unchecked") && Peek().Text == "{")) Error(Current, "An unsafe statement must contain a block.");
            _unsafeDepth++; var body = ParseStatement(); _unsafeDepth--; return body;
        }
        if (Is("checked") && Peek().Text == "{")
        {
            var context = Take(); return new OverflowContextStatement(true, ParseBlock(), context.Location);
        }
        if (Is("unchecked") && Peek().Text == "{")
        {
            var context = Take();
            if (_unsafeDepth == 0) diagnostics.Add("LS0501", DiagnosticSeverity.Error, "unchecked requires an unsafe context.", context.Location);
            return new OverflowContextStatement(false, ParseBlock(), context.Location);
        }
        if (Is("switch")) return ParseSwitch(at);
        if (Is("let") || Is("var") || Is("const"))
        {
            return ParseLocalDeclaration(at, expectSemicolon: true);
        }
        if (LooksLikeTypedLocal()) return ParseLocalDeclaration(at, expectSemicolon: true);
        if (LooksLikeMultiAssignment()) return ParseMultiAssignment(at);
        var expression = ParseExpression(); Expect(";"); return new ExpressionStatement(expression, at);
    }
    private bool LooksLikeTupleTypeDeclaration()
    {
        var close = FindClosingParenthesis(_position);
        return close > _position && HasTopLevelComma(_position, close) && close + 2 < tokens.Count &&
            IsIdentifier(tokens[close + 1]) && tokens[close + 2].Text is "=" or ";";
    }
    private void ReportUnsupportedTupleTypeDeclaration()
    {
        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Tuple types and deconstruction are not part of this language version.", Current.Location);
        Take();
        SkipBalanced("(", ")");
        while (Current.Kind != TokenKind.End && !Is(";") && !Is("}")) Take();
        Eat(";");
    }
    private int FindClosingParenthesis(int openIndex)
    {
        if (openIndex >= tokens.Count || tokens[openIndex].Text != "(") return -1;
        var depth = 0;
        for (var index = openIndex; index < tokens.Count; index++)
        {
            if (tokens[index].Text == "(") depth++;
            else if (tokens[index].Text == ")" && --depth == 0) return index;
        }
        return -1;
    }
    private bool HasTopLevelComma(int openIndex, int closeIndex)
    {
        var parentheses = 0;
        var brackets = 0;
        var braces = 0;
        var genericAngles = 0;
        for (var index = openIndex + 1; index < closeIndex; index++)
        {
            var text = tokens[index].Text;
            if (text == "(") parentheses++;
            else if (text == ")") parentheses--;
            else if (text == "[") brackets++;
            else if (text == "]") brackets--;
            else if (text == "{") braces++;
            else if (text == "}") braces--;
            else if (text == "<" && (genericAngles > 0 || HasMatchingGenericClose(index, closeIndex))) genericAngles++;
            else if (text == ">" && genericAngles > 0) genericAngles--;
            else if (text == ">>" && genericAngles > 0) genericAngles = Math.Max(0, genericAngles - 2);
            else if (text == "," && parentheses == 0 && brackets == 0 && braces == 0 && genericAngles == 0) return true;
        }
        return false;
    }
    private bool HasMatchingGenericClose(int openIndex, int closeIndex)
    {
        var depth = 0;
        for (var index = openIndex; index < closeIndex; index++)
        {
            if (tokens[index].Text == "<") depth++;
            else if (tokens[index].Text == ">" && depth > 0 && --depth == 0) return true;
            else if (tokens[index].Text == ">>" && depth > 0 && (depth -= 2) <= 0) return true;
        }
        return false;
    }
    private TryStatement ParseTry(SourceLocation at)
    {
        var body = Is("{") ? ParseBlock() : ParseMissingBlock("A try statement requires a block.");
        var catches = new List<CatchClause>();
        while (Eat("catch"))
        {
            var catchAt = tokens[_position - 1].Location;
            Expect("(");
            string? typeName = null;
            string? variableName = null;
            var catchAll = Eat("...");
            if (!catchAll)
            {
                typeName = ParseTypeName();
                if (typeName is null)
                    Error(Current, "Expected an exception type or '...' in catch clause.");
                else if (IsIdentifier(Current))
                    variableName = Take().Text.TrimStart('@');
            }
            Expect(")");
            if (Eat("when"))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Exception filters are not part of this language version.", catchAt);
                if (Eat("(")) SkipBalanced("(", ")");
                else Error(Current, "Expected '(' after the catch filter keyword.");
            }
            var catchBody = Is("{") ? ParseBlock() : ParseMissingBlock("A catch clause requires a block.");
            catches.Add(new(typeName, variableName, catchAll, catchBody, catchAt));
        }
        BlockStatement? finalizer = null;
        if (Eat("finally")) finalizer = Is("{") ? ParseBlock() : ParseMissingBlock("A finally clause requires a block.");
        if (catches.Count == 0 && finalizer is null)
            diagnostics.Add("LS1800", DiagnosticSeverity.Error, "A try statement must have at least one catch clause or a finally block.", at);
        return new(body, catches, finalizer, at);
    }
    private BlockStatement ParseMissingBlock(string message)
    {
        Error(Current, message);
        return new([], Current.Location);
    }
    private bool LooksLikeMultiAssignment()
    {
        var parens = 0; var brackets = 0; var sawTopLevelComma = false;
        for (var index = _position; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind == TokenKind.End || (token.Text == ";" && parens == 0 && brackets == 0)) return false;
            if (token.Text == "(") parens++;
            else if (token.Text == ")") { if (parens > 0) parens--; }
            else if (token.Text == "[") brackets++;
            else if (token.Text == "]") { if (brackets > 0) brackets--; }
            else if (parens == 0 && brackets == 0 && token.Text == ",") sawTopLevelComma = true;
            else if (parens == 0 && brackets == 0 && token.Text == "=") return sawTopLevelComma;
        }
        return false;
    }
    private MultiAssignmentStatement ParseMultiAssignment(SourceLocation at)
    {
        var targets = new List<Expression> { ParseExpression(2) };
        while (Eat(",")) targets.Add(ParseExpression(2));
        Expect("=");
        var values = new List<Expression> { ParseExpression() };
        while (Eat(",")) values.Add(ParseExpression());
        Expect(";");
        return new(targets, values, at);
    }
    private SwitchStatement ParseSwitch(SourceLocation at)
    {
        Expect("switch"); Expect("("); var value = ParseExpression(); Expect(")"); Expect("{");
        var sections = new List<SwitchSection>();
        while (Current.Kind != TokenKind.End && !Is("}"))
        {
            var sectionAt = Current.Location; var labels = new List<SwitchLabel>();
            while (Is("case") || Is("default"))
            {
                if (Eat("default")) labels.Add(new(null, true, sectionAt));
                else
                {
                    var caseToken = Take();
                    var firstCaseLabel = labels.Count;
                    labels.Add(ParseSwitchPattern(caseToken.Location));
                    while (Eat(",")) labels.Add(ParseSwitchPattern(Current.Location));
                    if (Eat("when"))
                    {
                        var guard = ParseExpression();
                        var guardCaptureScope = new BlockStatement([], guard.Location);
                        for (var labelIndex = firstCaseLabel; labelIndex < labels.Count; labelIndex++)
                            labels[labelIndex] = labels[labelIndex] with { Guard = guard, GuardCaptureScope = guardCaptureScope };
                    }
                }
                Expect(":");
            }
            if (labels.Count == 0)
            {
                Error(Current, "Expected a case or default label in switch.");
                if (Current.Kind != TokenKind.End) Take();
                continue;
            }
            var statements = new List<Statement>();
            while (Current.Kind != TokenKind.End && !Is("}") && !Is("case") && !Is("default"))
            {
                var before = _position; statements.Add(ParseStatement()); if (_position == before) Take();
            }
            sections.Add(new(labels, statements, sectionAt));
        }
        Expect("}");
        return new(value, sections, at);
    }
    private SwitchLabel ParseSwitchPattern(SourceLocation at, int minimumPrecedence = 1)
    {
        var left = ParseSwitchPatternAtom(at);
        while (Current.Text is "and" or "or")
        {
            var precedence = Current.Text == "and" ? 2 : 1;
            if (precedence < minimumPrecedence) break;
            var combinedOperator = Take().Text;
            var right = ParseSwitchPattern(Current.Location, precedence + 1);
            left = new(null, false, at, CombinedOperator: combinedOperator, LeftPattern: left, RightPattern: right);
        }
        return left;
    }
    private SwitchLabel ParseSwitchPatternAtom(SourceLocation at)
    {
        if (Is("("))
        {
            var close = FindClosingParenthesis(_position);
            if (close > _position && HasTopLevelComma(_position, close))
            {
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Tuple patterns are not part of this language version.", at);
                Take();
                SkipBalanced("(", ")");
                return new SwitchLabel(null, false, at) { IsUnsupportedPattern = true };
            }

            Take();
            var parenthesized = ParseSwitchPattern(Current.Location);
            Expect(")");
            return parenthesized with { At = at };
        }
        if (Eat("_")) return new(null, false, at, IsDiscard: true);
        if (Eat("not")) return new(null, false, at, IsNegated: true, LeftPattern: ParseSwitchPatternAtom(Current.Location));
        if (TryParseSwitchTypePattern(at, out var typePattern)) return typePattern;
        if (Current.Text is ">" or ">=" or "<" or "<=")
        {
            var relationalOperator = Take().Text;
            return new(ParseExpression(2), false, at, RelationalOperator: relationalOperator);
        }
        return new(ParseExpression(2), false, at);
    }
    private bool TryParseSwitchTypePattern(SourceLocation at, out SwitchLabel pattern)
    {
        pattern = null!;
        if (Current.Kind != TokenKind.Identifier) return false;
        var start = _position;
        var typeName = ParseTypeName(validateUnsafe: false);
        if (typeName is null) { _position = start; return false; }
        if (IsSwitchPatternBoundary(Current.Text))
        {
            Expression value = new NameExpression(typeName.Split('.')[0], at);
            foreach (var part in typeName.Split('.').Skip(1)) value = new MemberAccessExpression(value, part, at);
            pattern = new(value, false, at, TypePatternName: typeName);
            return true;
        }
        if (IsIdentifier(Current))
        {
            var variable = Take();
            if (IsSwitchPatternBoundary(Current.Text))
            {
                pattern = new(null, false, at, TypePatternName: typeName, PatternVariableName: variable.Text.TrimStart('@'));
                return true;
            }
        }
        _position = start;
        return false;
    }
    private static bool IsSwitchPatternBoundary(string token) => token is ":" or "," or "when" or "and" or "or";
    private Statement ParseFor(SourceLocation at)
    {
        Expect("(");
        if (LooksLikeForInHeader())
        {
            string? typeName = null;
            bool mutable;
            Token name;
            if (Is("let") || Is("var"))
            {
                mutable = Take().Text == "var";
                name = Take();
                if (!IsIdentifier(name)) Error(name, "Expected a loop variable name.");
            }
            else
            {
                typeName = ParseTypeName();
                mutable = true;
                name = Take();
                if (!IsIdentifier(name)) Error(name, "Expected a loop variable name.");
            }
            if (Eat("in"))
            {
                var collection = ParseExpression();
                Expect(")");
                return new ForInStatement(typeName, name.Text.TrimStart('@'), mutable, collection, ParseStatement(), at, IsUnsafeContext: _unsafeDepth > 0);
            }
            Error(Current, "Expected 'in' after a for-in loop variable.");
            SkipUntil(")");
            Eat(")");
            return new ForInStatement(typeName, name.Text.TrimStart('@'), mutable, new UnsupportedExpression("Malformed for-in collection", name.Location), ParseStatement(), at, IsUnsafeContext: _unsafeDepth > 0);
        }

        Statement? initializer = null;
        var tupleTypeInitializer = Is("(") && LooksLikeTupleTypeDeclaration();
        if (tupleTypeInitializer)
        {
            ReportUnsupportedTupleTypeDeclaration();
            initializer = new EmptyStatement(at);
        }
        if (!Is(";"))
        {
            if (!tupleTypeInitializer)
            {
                if (Is("let") || Is("var") || Is("const") || LooksLikeTypedLocal()) initializer = ParseLocalDeclaration(at, expectSemicolon: false);
                else initializer = new ExpressionStatement(ParseExpression(), at);
            }
        }
        if (!tupleTypeInitializer) Expect(";");
        var condition = Is(";") ? null : ParseExpression(); Expect(";"); var iterators = new List<Expression>();
        while (!Is(")") && Current.Kind != TokenKind.End) { iterators.Add(ParseExpression()); if (!Eat(",")) break; }
        Expect(")"); return new ForStatement(initializer, condition, iterators, ParseStatement(), at);
    }
    private bool LooksLikeForInHeader()
    {
        if (Is("let") || Is("var"))
            return _position + 2 < tokens.Count && IsIdentifier(tokens[_position + 1]) && tokens[_position + 2].Text == "in";
        if (Current.Kind != TokenKind.Identifier) return false;
        var nameIndex = _position + 1;
        while (nameIndex < tokens.Count)
        {
            if (tokens[nameIndex].Text is "*" or "?") { nameIndex++; continue; }
            if (tokens[nameIndex].Text == "?[")
            {
                nameIndex++;
                while (nameIndex < tokens.Count && tokens[nameIndex].Text != "]") nameIndex++;
                if (nameIndex >= tokens.Count) return false;
                nameIndex++;
                continue;
            }
            if (tokens[nameIndex].Text != "[") break;
            nameIndex++;
            while (nameIndex < tokens.Count && tokens[nameIndex].Text != "]") nameIndex++;
            if (nameIndex >= tokens.Count) return false;
            nameIndex++;
        }
        return nameIndex + 1 < tokens.Count && tokens[nameIndex].Kind == TokenKind.Identifier && tokens[nameIndex + 1].Text == "in";
    }
    private VariableDeclarationStatement ParseLocalDeclaration(SourceLocation at, bool expectSemicolon)
    {
        var mutable = true; var isVar = false; var isConstant = false; string? explicitType = null;
        if (Is("const"))
        {
            Take(); mutable = false; isConstant = true;
            if (Is("let") || Is("var")) Error(Current, "const requires an explicit type and cannot be combined with let or var.");
            else explicitType = ParseTypeName();
        }
        else if (Is("let") || Is("var"))
        {
            isVar = Take().Text == "var"; mutable = isVar;
            if (LooksLikeTypedLocal()) explicitType = ParseTypeName();
        }
        else explicitType = ParseTypeName();

        var names = new List<Token>();
        var first = Take();
        if (!IsIdentifier(first)) Error(first, "Expected a non-keyword local name (use @ to escape a keyword).");
        names.Add(first);
        while (Eat(","))
        {
            var name = Take();
            if (!IsIdentifier(name)) Error(name, "Expected a non-keyword local name (use @ to escape a keyword).");
            names.Add(name);
        }
        if (isVar && explicitType is not null) Error(first, "var does not accept an explicit type; use a typed mutable declaration.");

        var initializers = new List<Expression>();
        if (Eat("="))
        {
            initializers.Add(Is("{") ? ParseInlineArrayInitializer() : ParseExpression());
            while (Eat(",")) initializers.Add(Is("{") ? ParseInlineArrayInitializer() : ParseExpression());
        }
        if (expectSemicolon) Expect(";");

        if (initializers.Skip(1).Any(e => e is BinaryExpression { Operator: "=" }))
            diagnostics.Add("LS0214", DiagnosticSeverity.Error, "Multiple declarators use one initializer list; write all names before the single '='.", at);
        else if (initializers.Count > names.Count)
            diagnostics.Add("LS0212", DiagnosticSeverity.Error, "A multiple declaration has more initializers than identifiers.", at);

        if (isConstant && (initializers.Count != names.Count || initializers.Any(initializer => initializer is null)))
            diagnostics.Add("LS0213", DiagnosticSeverity.Error, "Every const local must have a constant initializer in its declaration.", at);

        var variables = names.Select((name, index) => new VariableStatement(
            name.Text.TrimStart('@'),
            mutable,
            explicitType,
            index < initializers.Count ? initializers[index] : null,
            name.Location,
            isConstant)).ToArray();
        return new(variables, at);
    }
    private bool LooksLikeTypedLocal()
    {
        if (Current.Kind != TokenKind.Identifier) return false;
        var nameIndex = _position + 1;
        while (nameIndex + 1 < tokens.Count && tokens[nameIndex].Text == "." && IsIdentifier(tokens[nameIndex + 1]))
            nameIndex += 2;
        while (nameIndex < tokens.Count)
        {
            if (tokens[nameIndex].Text == "<")
            {
                var depth = 0;
                do
                {
                    if (tokens[nameIndex].Text == "<") depth++;
                    else if (tokens[nameIndex].Text == ">") depth--;
                    else if (tokens[nameIndex].Text == ">>") depth -= 2;
                    nameIndex++;
                } while (nameIndex < tokens.Count && depth > 0);
                if (depth != 0) return false;
                continue;
            }
            if (tokens[nameIndex].Text is "*" or "?") { nameIndex++; continue; }
            if (tokens[nameIndex].Text == "?[")
            {
                nameIndex++;
                while (nameIndex < tokens.Count && tokens[nameIndex].Text != "]") nameIndex++;
                if (nameIndex >= tokens.Count) return false;
                nameIndex++;
                continue;
            }
            if (tokens[nameIndex].Text != "[") break;
            nameIndex++;
            while (nameIndex < tokens.Count && tokens[nameIndex].Text != "]") nameIndex++;
            if (nameIndex >= tokens.Count) return false;
            nameIndex++;
        }
        if (nameIndex >= tokens.Count || tokens[nameIndex].Kind != TokenKind.Identifier) return false;
        var afterName = nameIndex + 1;
        return afterName < tokens.Count && (tokens[afterName].Text is "=" or ";" or "," or "[");
    }
    private InlineArrayInitializerExpression ParseInlineArrayInitializer()
    {
        var at = Expect("{").Location;
        var elements = new List<Expression>();
        while (Current.Kind != TokenKind.End && !Is("}"))
        {
            elements.Add(Is("{") ? ParseInlineArrayInitializer() : ParseExpression());
            if (!Eat(",")) break;
        }
        Expect("}");
        return new(elements, at);
    }
    private void SkipStatement() { var depth = 0; while (Current.Kind != TokenKind.End) { if (depth == 0 && Eat(";")) return; if (Eat("{")) { depth++; continue; } if (Eat("}")) { if (depth == 0) return; depth--; if (depth == 0) return; continue; } Take(); } }

    private static readonly Dictionary<string, int> Precedence = new() { ["="] = 1, ["+="] = 1, ["-="] = 1, ["*="] = 1, ["/="] = 1, ["%="] = 1, ["&="] = 1, ["|="] = 1, ["^="] = 1, ["<<="] = 1, [">>="] = 1, ["??="] = 1, ["||"] = 2, ["??"] = 3, ["&&"] = 4, ["|"] = 5, ["^"] = 6, ["&"] = 7, ["=="] = 8, ["!="] = 8, ["<"] = 9, [">"] = 9, ["<="] = 9, [">="] = 9, ["is"] = 9, ["as"] = 9, ["<<"] = 10, [">>"] = 10, ["+"] = 11, ["-"] = 11, ["*"] = 12, ["/"] = 12, ["%"] = 12 };
    private Expression ParseExpression(int minPrecedence = 1)
    {
        var left = ParseUnary();
        while (Precedence.TryGetValue(Current.Text, out var precedence) && precedence >= minPrecedence)
        {
            var op = Take();
            var operatorText = op.Text == "is" && Eat("not") ? "is not" : op.Text;
            Expression right;
            string? patternVariableName = null;
            if (operatorText is "is" or "is not" or "as")
            {
                var targetLocation = Current.Location;
                var targetType = ParseTypeName(validateUnsafe: false, consumeNullableSuffix: false);
                if (targetType is null)
                {
                    Error(Current, $"Expected a type name after '{operatorText}'.");
                    right = new UnsupportedExpression($"Missing type after '{operatorText}'.", targetLocation);
                }
                else right = new NameExpression(targetType, targetLocation);
                if (targetType is not null && operatorText == "is" && Current.Kind == TokenKind.Identifier)
                    patternVariableName = Take().Text.TrimStart('@');
            }
            else right = ParseExpression(precedence + (precedence == 1 || op.Text == "??" ? 0 : 1));
            left = new BinaryExpression(left, operatorText, right, op.Location, patternVariableName);
        }
        if (minPrecedence == 1 && Eat("?")) { var at = left.Location; var whenTrue = ParseExpression(); Expect(":"); var whenFalse = ParseExpression(1); left = new ConditionalExpression(left, whenTrue, whenFalse, at); }
        return left;
    }
    private Expression ParseUnary()
    {
        var at = Current.Location;
        if (LooksLikeLambda()) return ParseLambdaExpression();
        if (LooksLikeCast())
        {
            Take(); var type = ParseTypeName()!; Expect(")");
            return new CastExpression(type, ParseUnary(), at);
        }
        if (Current.Text is "checked" or "unchecked")
        {
            var token = Take(); var checkedContext = token.Text == "checked";
            if (!checkedContext && _unsafeDepth == 0) diagnostics.Add("LS0501", DiagnosticSeverity.Error, "unchecked requires an unsafe context.", token.Location);
            Expect("("); var value = ParseExpression(); Expect(")");
            return new OverflowContextExpression(checkedContext, value, token.Location);
        }
        if (Current.Text is "+" or "-" or "!" or "~" or "++" or "--" or "&" or "*")
        {
            var op = Take();
            if (op.Text is "&" or "*" && _unsafeDepth == 0)
            {
                if (unsafeEnabled) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Pointer operations may only be used in an unsafe context.", op.Location);
                else diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Unsafe code is disabled for this compilation.", op.Location);
            }
            return new UnaryExpression(op.Text, ParseUnary(), false, at);
        }
        var expression = ParsePrimary();
        while (true)
        {
            IReadOnlyList<string>? explicitTypeArguments = null;
            if (Is("<") && LooksLikeGenericInvocation()) explicitTypeArguments = ReadTypeArgumentList();
            if (Eat("("))
            {
                var args = new List<Expression>(); var names = new List<string?>(); var modes = new List<string?>(); var sawNamed = false;
                while (!Is(")") && Current.Kind != TokenKind.End)
                {
                    string? argumentName = null;
                    if (IsIdentifier(Current) && Peek().Text == ":") { argumentName = Take().Text.TrimStart('@'); Take(); sawNamed = true; }
                    else if (sawNamed) diagnostics.Add("LS0740", DiagnosticSeverity.Error, "A positional argument cannot follow a named argument.", Current.Location);
                    var argumentMode = Current.Text is "ref" or "in" or "out" ? Take().Text : null;
                    if (argumentMode == "out" && Current.Kind == TokenKind.Identifier && Peek().Kind == TokenKind.Identifier)
                    {
                        var declarationAt = Current.Location;
                        string? declaredType;
                        if (Eat("var")) declaredType = null;
                        else declaredType = ParseTypeName();
                        var declaredName = Take();
                        if (!IsIdentifier(declaredName)) Error(declaredName, "Expected an inline out variable name.");
                        args.Add(new InlineOutVariableExpression(declaredType, declaredName.Text.TrimStart('@'), declarationAt));
                    }
                    else { args.Add(ParseExpression()); }
                    names.Add(argumentName); modes.Add(argumentMode);
                    if (!Eat(",")) break;
                }
                Expect(")");
                if (expression is not UnsupportedExpression) expression = new CallExpression(expression, args, at, names.All(name => name is null) ? null : names, modes.All(mode => mode is null) ? null : modes, _unsafeDepth > 0, explicitTypeArguments);
                continue;
            }
            if (Eat(".")) { var member = Take(); if (!IsIdentifier(member)) Error(member, "Expected a non-keyword member name after '.'."); if (expression is not UnsupportedExpression) expression = new MemberAccessExpression(expression, member.Text.TrimStart('@'), member.Location, IsUnsafeContext: _unsafeDepth > 0); continue; }
            if (Eat("?.")) { var member = Take(); if (!IsIdentifier(member)) Error(member, "Expected a non-keyword member name after '?.'."); expression = new MemberAccessExpression(expression, member.Text.TrimStart('@'), member.Location, IsNullConditional: true, IsUnsafeContext: _unsafeDepth > 0); continue; }
            if (Is("?[") || Is("["))
            {
                var nullConditional = Eat("?["); if (!nullConditional) Expect("[");
                var indices = new List<Expression>();
                if (!Is("]")) { indices.Add(ParseExpression()); while (Eat(",")) indices.Add(ParseExpression()); }
                Expect("]");
                expression = new IndexExpression(expression, indices, at, _unsafeDepth > 0, nullConditional);
                continue;
            }
            if (Eat("!")) { expression = new NullAssertionExpression(expression, at); continue; }
            if (Eat("++")) { expression = new UnaryExpression("++", expression, true, at); continue; }
            if (Eat("--")) { expression = new UnaryExpression("--", expression, true, at); continue; }
            break;
        }
        return expression;
    }
    private bool LooksLikeGenericInvocation()
    {
        var depth = 0;
        for (var index = _position; index < tokens.Count; index++)
        {
            // A generic argument list contains only type syntax. Stop at a
            // statement or expression boundary so a later relational '>' cannot
            // make an earlier comparison look like `Method<T>(...)`.
            if (index > _position && tokens[index].Text is ";" or "{" or "}" or ")" or "=>" or "&&" or "||" or "==" or "!=" or "<=" or ">=" or "+" or "-" or "/" or "%" or "|" or "^" or "=" or "?" or "!")
                return false;
            if (tokens[index].Text == "<") depth++;
            else if (tokens[index].Text == ">") depth--;
            else if (tokens[index].Text == ">>") depth -= 2;
            if (depth <= 0) return depth == 0 && index + 1 < tokens.Count && tokens[index + 1].Text == "(";
        }
        return false;
    }
    private IReadOnlyList<string> ReadTypeArgumentList()
    {
        Expect("<");
        var arguments = new List<string>();
        do
        {
            var argument = ParseTypeName(validateUnsafe: false);
            if (argument is null)
            {
                Error(Current, "Expected a generic method type argument.");
                break;
            }
            arguments.Add(argument);
        } while (Eat(","));
        if (!ConsumeGenericClose()) Error(Current, "Expected '>' after generic method type arguments.");
        return arguments;
    }
    private bool LooksLikeLambda()
    {
        if (Current.Kind == TokenKind.Identifier && Peek().Text == "=>") return true;
        if (!Is("(")) return false;
        var depth = 0;
        for (var index = _position; index < tokens.Count; index++)
        {
            if (tokens[index].Text == "(") depth++;
            else if (tokens[index].Text == ")" && --depth == 0)
                return index + 1 < tokens.Count && tokens[index + 1].Text == "=>";
        }
        return false;
    }
    private LambdaExpression ParseLambdaExpression()
    {
        var at = Current.Location;
        var parameters = new List<LambdaParameter>();
        if (Current.Kind == TokenKind.Identifier && Peek().Text == "=>")
        {
            var name = Take();
            parameters.Add(new(null, name.Text.TrimStart('@'), name.Location));
        }
        else
        {
            Expect("(");
            while (!Is(")") && Current.Kind != TokenKind.End)
            {
                var parameterAt = Current.Location;
                string? typeName = null;
                Token name;
                if (Current.Kind == TokenKind.Identifier && (Peek().Text is "," or ")")) name = Take();
                else
                {
                    typeName = ParseTypeName();
                    name = Take();
                    if (typeName is null) Error(name, "Expected a lambda parameter name.");
                    if (!IsIdentifier(name)) Error(name, "Expected a non-keyword lambda parameter name (use @ to escape a keyword).");
                }
                parameters.Add(new(typeName, name.Text.TrimStart('@'), parameterAt));
                if (!Eat(",")) break;
            }
            Expect(")");
        }
        Expect("=>");
        if (Is("{")) return new(parameters, null, ParseBlock(), at);
        return new(parameters, ParseExpression(), null, at);
    }
    private bool LooksLikeCast()
    {
        if (!Is("(") || Peek().Kind != TokenKind.Identifier) return false;
        var index = _position + 1;
        if (ReservedWords.Contains(tokens[index].Text.TrimStart('@')) && !TypeKeywords.Contains(tokens[index].Text.TrimStart('@')) && !tokens[index].Text.StartsWith('@')) return false;
        index++;
        while (index + 1 < tokens.Count && tokens[index].Text == "." && IsIdentifier(tokens[index + 1])) index += 2;
        if (index < tokens.Count && tokens[index].Text == "<")
        {
            var depth = 0;
            do { if (tokens[index].Text == "<") depth++; else if (tokens[index].Text == ">") depth--; index++; }
            while (index < tokens.Count && depth > 0);
            if (depth != 0) return false;
        }
        while (index < tokens.Count && tokens[index].Text is "?" or "*") index++;
        while (index + 1 < tokens.Count && tokens[index].Text == "[" && tokens[index + 1].Text == "]") index += 2;
        if (index >= tokens.Count || tokens[index].Text != ")") return false;
        index++;
        if (index >= tokens.Count) return false;
        var next = tokens[index];
        if (next.Kind is TokenKind.Identifier or TokenKind.Number or TokenKind.Character or TokenKind.String) return true;
        if (next.Text is "true" or "false" or "null" or "default" or "sizeof" or "new" or "scope" or "(" or "!" or "~" or "&" or "*" or "++" or "--") return true;
        return next.Text is "+" or "-" && TypeKeywords.Contains(tokens[_position + 1].Text.TrimStart('@'));
    }
    private Expression ParsePrimary()
    {
        var tokenIndex = _position;
        var token = Take();
        if (token.Text == "(")
        {
            var close = FindClosingParenthesis(tokenIndex);
            if (close > tokenIndex && HasTopLevelComma(tokenIndex, close))
            {
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Tuple expressions and deconstruction are not part of this language version.", token.Location);
                _position = close + 1;
                return new UnsupportedExpression("Tuple expressions are not supported", token.Location);
            }
            var expression = ParseExpression(); Expect(")"); return expression;
        }
        if (token.Kind is TokenKind.Number or TokenKind.String or TokenKind.Character || token.Text is "true" or "false" or "null") return new LiteralExpression(token.Text, token.Location);
        if (token.Text == "default")
        {
            string? type = null;
            if (Eat("(")) { type = ParseTypeName(); if (type is null) Error(Current, "Expected a type in default(T)."); Expect(")"); }
            return new DefaultValueExpression(type, token.Location);
        }
        if (token.Text == "sizeof")
        {
            if (_unsafeDepth == 0) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "sizeof requires an unsafe context.", token.Location);
            Expect("("); var type = ParseTypeName(); if (type is null) { Error(Current, "Expected a type in sizeof(T)."); type = "?"; } Expect(")");
            return new SizeOfExpression(type, token.Location);
        }
        if (token.Text is "new" or "scope") return ParseAllocationExpression(token);
        if (token.Text == "this") return new ThisExpression(token.Location);
        if (token.Text == "base") return new BaseExpression(token.Location);
        if (IsIdentifier(token) && LooksLikeGenericTypeQualifier(tokenIndex))
        {
            _position = tokenIndex;
            var constructedType = ParseTypeName(validateUnsafe: false, consumeArraySuffixes: false, consumeNullableSuffix: false);
            return new NameExpression(constructedType ?? token.Text.TrimStart('@'), token.Location);
        }
        if (IsIdentifier(token) || token.Kind == TokenKind.Identifier && !ReservedWords.Contains(token.Text)) return new NameExpression(token.Text.TrimStart('@'), token.Location);
        if (token.Kind == TokenKind.Identifier) { Error(token, $"Keyword '{token.Text}' cannot be used as an expression name."); return new LiteralExpression("0", token.Location); }
        Error(token, $"Expected an expression, found '{token.Text}'."); return new LiteralExpression("0", token.Location);
    }
    private bool LooksLikeGenericTypeQualifier(int startIndex)
    {
        var index = startIndex;
        if (index >= tokens.Count || tokens[index].Kind != TokenKind.Identifier) return false;
        index++;
        while (index + 1 < tokens.Count && tokens[index].Text == "." && tokens[index + 1].Kind == TokenKind.Identifier) index += 2;
        if (index >= tokens.Count || tokens[index].Text != "<") return false;
        var depth = 0;
        for (; index < tokens.Count; index++)
        {
            if (tokens[index].Text == "<") depth++;
            else if (tokens[index].Text == ">") depth--;
            else if (tokens[index].Text == ">>") depth -= 2;
            if (depth <= 0) return depth == 0 && index + 1 < tokens.Count && tokens[index + 1].Text == ".";
        }
        return false;
    }
    private AllocationExpression ParseAllocationExpression(Token keyword)
    {
        var isScope = keyword.Text == "scope";
        if (isScope && Eat("new")) { }

        string? allocatedType = null;
        var dimensions = new List<Expression>();
        if (!Is("{") && !Is("[")) allocatedType = ParseTypeName(validateUnsafe: false, consumeArraySuffixes: false, consumeNullableSuffix: false);

        // Consume nullable, pointer, and array suffixes in allocation types while
        // distinguishing an empty/rank-only array suffix from a dimension list.
        // `?[` is a combined lexer token (also used by null-conditional indexing),
        // so it can begin either a nullable element type suffix or its dimension
        // list, as in `new T?[n]` and `new T[]?[n]`.
        while (Is("[") || allocatedType is not null && (Is("?") || Is("?[") || Is("*")))
        {
            if (allocatedType is not null && Eat("?")) { allocatedType += "?"; continue; }
            if (allocatedType is not null && Eat("*")) { allocatedType += "*"; continue; }

            var nullableBracket = allocatedType is not null && Eat("?[");
            if (!nullableBracket && !Eat("[")) break;
            if (nullableBracket) allocatedType += "?";

            var bracketDimensions = new List<Expression>();
            var rank = 1;
            if (Is(","))
            {
                while (Eat(",")) rank++;
            }
            else if (!Is("]"))
            {
                while (!Is("]") && Current.Kind != TokenKind.End)
                {
                    bracketDimensions.Add(ParseExpression());
                    if (!Eat(",")) break;
                    rank++;
                }
            }
            Expect("]");
            if (allocatedType is not null) allocatedType += "[" + new string(',', rank - 1) + "]";
            dimensions.AddRange(bracketDimensions);
        }

        var constructorArguments = new List<Expression>();
        var constructorArgumentNames = new List<string?>();
        var constructorArgumentModes = new List<string?>();
        if (Eat("("))
        {
            while (!Is(")") && Current.Kind != TokenKind.End)
            {
                string? argumentName = null;
                if (IsIdentifier(Current) && Peek().Text == ":") { argumentName = Take().Text.TrimStart('@'); Take(); }
                var argumentMode = Current.Text is "ref" or "in" or "out" ? Take().Text : null;
                constructorArguments.Add(ParseExpression());
                constructorArgumentNames.Add(argumentName);
                constructorArgumentModes.Add(argumentMode);
                if (!Eat(",")) break;
            }
            Expect(")");
        }

        InlineArrayInitializerExpression? initializer = Is("{") ? ParseInlineArrayInitializer() : null;
        return new AllocationExpression(allocatedType, dimensions, initializer, isScope, keyword.Location,
            constructorArguments, constructorArgumentNames.All(name => name is null) ? null : constructorArgumentNames,
            constructorArgumentModes.All(mode => mode is null) ? null : constructorArgumentModes);
    }
}
