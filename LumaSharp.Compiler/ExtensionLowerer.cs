namespace LumaSharp.Compiler;

internal static class ExtensionLowerer
{
    public static (IReadOnlyList<TypeDeclaration> Helpers, IReadOnlyDictionary<string, string> Targets) Lower(
        IReadOnlyList<CompilationUnit> units, IReadOnlyList<TypeDeclaration> sourceTypes, DiagnosticBag diagnostics)
    {
        var helpers = new List<TypeDeclaration>();
        var targets = new Dictionary<string, string>(StringComparer.Ordinal);
        var methodSignatures = new HashSet<string>(StringComparer.Ordinal);
        var propertySignatures = new HashSet<string>(StringComparer.Ordinal);
        var ordinal = 0;
        foreach (var unit in units)
        foreach (var extension in unit.Extensions ?? [])
        {
            var target = ResolveTarget(extension, sourceTypes);
            if (target is null)
            {
                diagnostics.Add("LS0200", DiagnosticSeverity.Error,
                    extension.TargetType.Contains("<", StringComparison.Ordinal)
                        ? $"Constructed generic extension target '{extension.TargetType}' could not be resolved; generic extension targets are not yet supported."
                        : $"Extension target type '{extension.TargetType}' does not exist or is ambiguous.", extension.Location);
                continue;
            }
            if (target.Kind is not ("class" or "struct" or "interface"))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                    $"Extensions can target classes, structs, and interfaces; '{extension.TargetType}' is a {target.Kind}.", extension.Location);
                continue;
            }
            if (target.HasTypeParameters)
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                    $"Extensions on open generic type '{GetQualifiedTypeName(target)}' are not yet supported.", extension.Location);
                continue;
            }
            var helperName = "$Extension_" + (ordinal++).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var helperNamespace = extension.Namespace;
            var memberNames = target.Members.Select(member => member switch
            {
                MethodDeclaration method => method.Name,
                FieldDeclaration field => field.Name,
                ConstantFieldDeclaration constant => constant.Name,
                ComputedPropertyDeclaration property => property.Name,
                _ => ""
            }).Where(name => name.Length > 0).Concat(extension.Members.OfType<MethodDeclaration>().Select(method => method.Name))
              .Concat(extension.Members.OfType<ComputedPropertyDeclaration>().Select(property => property.Name))
              .ToHashSet(StringComparer.Ordinal);
            var rewritten = new List<MemberDeclaration>();
            foreach (var member in extension.Members)
            {
                if (member is ComputedPropertyDeclaration property)
                {
                    var propertySignature = GetQualifiedTypeName(target) + "." + property.Name;
                    if (!propertySignatures.Add(propertySignature))
                        diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Extension property '{propertySignature}' is declared more than once.", property.Location);
                    if (property.TypeName == "void")
                        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An extension property cannot have type 'void'.", property.Location);
                    if (property.GetterBody is { } getterBody)
                    {
                        var getterLocals = new HashSet<string>(StringComparer.Ordinal) { "$this" };
                        foreach (var name in GetDeclaredNames(getterBody)) getterLocals.Add(name);
                        rewritten.Add(new MethodDeclaration(helperName, "$property_get_" + property.Name, property.TypeName,
                            [new ParameterDeclaration(ExtensionReceiverName(target), "$this", property.Location, IsScoped: true)],
                            RewriteBlock(getterBody, memberNames, getterLocals), true, property.Accessibility == "public", false, false,
                            property.Location, Accessibility: extension.Accessibility, PropertyName: property.Name));
                    }
                    if (property.SetterBody is { } setterBody)
                    {
                        var setterLocals = new HashSet<string>(StringComparer.Ordinal) { "$this", "value" };
                        foreach (var name in GetDeclaredNames(setterBody)) setterLocals.Add(name);
                        var rewrittenSetter = RewriteBlock(setterBody, memberNames, setterLocals);
                        rewritten.Add(new MethodDeclaration(helperName, "$property_set_" + property.Name, property.TypeName,
                            [new ParameterDeclaration(ExtensionReceiverName(target), "$this", property.Location,
                                PassingMode: target.Kind == "struct" ? "ref" : "", IsScoped: true), new ParameterDeclaration(property.TypeName, "value", property.Location)],
                            CompleteSetterBody(rewrittenSetter, property.Location, diagnostics), true, property.Accessibility == "public", false, false,
                            property.Location, Accessibility: extension.Accessibility, PropertyName: property.Name));
                    }
                    continue;
                }
                if (member is not MethodDeclaration method) continue;
                if (method.HasTypeParameters)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                        "Generic extension methods are not yet supported.", method.Location);
                    continue;
                }
                var signature = GetQualifiedTypeName(target) + "." + method.Name + "(" +
                    string.Join(",", method.Parameters.Select(parameter => parameter.TypeName)) + ")";
                if (!methodSignatures.Add(signature))
                    diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Extension method '{signature}' is declared more than once.", method.Location);
                var receiver = new ParameterDeclaration(ExtensionReceiverName(target), "$this", method.Location, IsScoped: true);
                var localNames = method.Parameters.Select(parameter => parameter.Name).Append("$this").ToHashSet(StringComparer.Ordinal);
                if (method.Body is not null)
                    foreach (var name in GetDeclaredNames(method.Body)) localNames.Add(name);
                var body = method.Body is null ? null : RewriteBlock(method.Body, memberNames, localNames);
                rewritten.Add(method with
                {
                    ContainingType = helperName,
                    Parameters = [receiver, .. method.Parameters],
                    Body = body,
                    IsStatic = true,
                    Accessibility = extension.Accessibility,
                    IsPublic = extension.Accessibility == "public"
                });
            }
            var helper = new TypeDeclaration(helperNamespace, "class", helperName, rewritten, extension.Location,
                IsStatic: true, Accessibility: extension.Accessibility, Usings: extension.Usings);
            helpers.Add(helper);
            targets[GetQualifiedTypeName(helper)] = GetQualifiedTypeName(target);
        }
        return (helpers, targets);
    }

    public static string ExtensionReceiverName(TypeDeclaration target)
    {
        var name = string.IsNullOrEmpty(target.Namespace) ? target.Name : target.Namespace + "." + target.Name;
        return target.TypeParameters is { Count: > 0 } ? name : name;
    }

    private static TypeDeclaration? ResolveTarget(ExtensionDeclaration extension, IReadOnlyList<TypeDeclaration> types)
    {
        var targetName = extension.TargetType.Trim();
        var alias = extension.Usings?.FirstOrDefault(directive => directive.Alias == targetName);
        if (alias is not null) targetName = alias.Target;
        var exact = types.Where(type => Qualified(type) == targetName).ToArray();
        if (exact.Length == 1) return exact[0];
        var inNamespace = types.Where(type => type.Namespace == extension.Namespace && type.Name == targetName).ToArray();
        if (inNamespace.Length == 1) return inNamespace[0];
        var imported = extension.Usings?.Where(directive => directive.Alias is null)
            .SelectMany(directive => types.Where(type => type.Namespace == directive.Target && type.Name == targetName)).ToArray() ?? [];
        if (imported.Length == 1) return imported[0];
        var global = types.Where(type => type.Name == targetName).ToArray();
        return global.Length == 1 ? global[0] : null;
    }

    private static string Qualified(TypeDeclaration type) => string.IsNullOrEmpty(type.Namespace) ? type.Name : type.Namespace + "." + type.Name;
    private static string GetQualifiedTypeName(TypeDeclaration type) => Qualified(type);

    private static IEnumerable<string> GetDeclaredNames(BlockStatement block)
    {
        foreach (var statement in block.Statements)
        {
            switch (statement)
            {
                case VariableStatement variable: yield return variable.Name; break;
                case VariableDeclarationStatement declaration:
                    foreach (var variable in declaration.Variables) yield return variable.Name;
                    break;
                case BlockStatement nested:
                    foreach (var name in GetDeclaredNames(nested)) yield return name;
                    break;
                case IfStatement conditional:
                    foreach (var name in GetDeclaredNames(conditional.Then)) yield return name;
                    if (conditional.Else is not null) foreach (var name in GetDeclaredNames(conditional.Else)) yield return name;
                    break;
                case WhileStatement loop:
                    foreach (var name in GetDeclaredNames(loop.Body)) yield return name;
                    break;
                case DoWhileStatement loop:
                    foreach (var name in GetDeclaredNames(loop.Body)) yield return name;
                    break;
                case ForStatement loop:
                    if (loop.Initializer is not null) foreach (var name in GetDeclaredNames(loop.Initializer)) yield return name;
                    foreach (var name in GetDeclaredNames(loop.Body)) yield return name;
                    break;
                case ForInStatement loop:
                    if (loop.TypeName is not null) yield return loop.Name;
                    foreach (var name in GetDeclaredNames(loop.Body)) yield return name;
                    break;
                case SwitchStatement selection:
                    foreach (var section in selection.Sections)
                    foreach (var statementItem in section.Statements)
                        foreach (var name in GetDeclaredNames(statementItem)) yield return name;
                    break;
                case TryStatement attempt:
                    foreach (var name in GetDeclaredNames(attempt.Body)) yield return name;
                    foreach (var clause in attempt.Catches)
                    {
                        if (clause.VariableName is not null) yield return clause.VariableName;
                        foreach (var name in GetDeclaredNames(clause.Body)) yield return name;
                    }
                    if (attempt.Finally is not null) foreach (var name in GetDeclaredNames(attempt.Finally)) yield return name;
                    break;
                case OverflowContextStatement overflow:
                    foreach (var name in GetDeclaredNames(overflow.Body)) yield return name;
                    break;
            }
        }
    }

    private static IEnumerable<string> GetDeclaredNames(Statement statement) =>
        GetDeclaredNames(statement as BlockStatement ?? new BlockStatement([statement], statement.Location));

    private static BlockStatement RewriteBlock(BlockStatement block, HashSet<string> members, HashSet<string> locals) =>
        block with { Statements = block.Statements.Select(statement => RewriteStatement(statement, members, locals)).ToArray() };

    private static BlockStatement CompleteSetterBody(BlockStatement body, SourceLocation location, DiagnosticBag diagnostics)
    {
        var rewritten = (BlockStatement)RewriteReturns(body);
        if (AlwaysAbrupt(rewritten)) return rewritten;
        return rewritten with { Statements = rewritten.Statements.Append(new ReturnStatement(new NameExpression("value", location), location)).ToArray() };

        Statement RewriteReturns(Statement statement) => statement switch
        {
            ReturnStatement { Value: null } returned => returned with { Value = new NameExpression("value", returned.Location) },
            ReturnStatement { Value: not null } returned => InvalidReturn(returned),
            BlockStatement block => block with { Statements = block.Statements.Select(RewriteReturns).ToArray() },
            IfStatement conditional => conditional with { Then = RewriteReturns(conditional.Then), Else = conditional.Else is null ? null : RewriteReturns(conditional.Else) },
            WhileStatement loop => loop with { Body = RewriteReturns(loop.Body) },
            DoWhileStatement loop => loop with { Body = RewriteReturns(loop.Body) },
            ForStatement loop => loop with { Body = RewriteReturns(loop.Body) },
            ForInStatement loop => loop with { Body = RewriteReturns(loop.Body) },
            OverflowContextStatement overflow => overflow with { Body = RewriteReturns(overflow.Body) },
            _ => statement
        };

        Statement InvalidReturn(ReturnStatement returned)
        {
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error,
                "An extension property setter cannot return a value; use 'return;' to finish the accessor.", returned.Location);
            return returned with { Value = new NameExpression("value", returned.Location) };
        }

        static bool AlwaysAbrupt(Statement statement) => statement switch
        {
            ReturnStatement or ThrowStatement => true,
            BlockStatement block => block.Statements.LastOrDefault() is { } last && AlwaysAbrupt(last),
            IfStatement { Else: not null } conditional => AlwaysAbrupt(conditional.Then) && AlwaysAbrupt(conditional.Else),
            OverflowContextStatement overflow => AlwaysAbrupt(overflow.Body),
            _ => false
        };
    }

    private static Statement RewriteStatement(Statement statement, HashSet<string> members, HashSet<string> locals) => statement switch
    {
        BlockStatement block => RewriteBlock(block, members, locals),
        ReturnStatement value => value with { Value = value.Value is null ? null : RewriteExpression(value.Value, members, locals) },
        VariableStatement variable => variable with { Initializer = variable.Initializer is null ? null : RewriteExpression(variable.Initializer, members, locals) },
        VariableDeclarationStatement declaration => declaration with { Variables = declaration.Variables.Select(variable => (VariableStatement)RewriteStatement(variable, members, locals)).ToArray() },
        MultiAssignmentStatement assignment => assignment with { Targets = assignment.Targets.Select(value => RewriteExpression(value, members, locals)).ToArray(), Values = assignment.Values.Select(value => RewriteExpression(value, members, locals)).ToArray() },
        ExpressionStatement expression => expression with { Value = RewriteExpression(expression.Value, members, locals) },
        IfStatement conditional => conditional with { Condition = RewriteExpression(conditional.Condition, members, locals), Then = RewriteStatement(conditional.Then, members, locals), Else = conditional.Else is null ? null : RewriteStatement(conditional.Else, members, locals) },
        WhileStatement loop => loop with { Condition = RewriteExpression(loop.Condition, members, locals), Body = RewriteStatement(loop.Body, members, locals) },
        DoWhileStatement loop => loop with { Condition = RewriteExpression(loop.Condition, members, locals), Body = RewriteStatement(loop.Body, members, locals) },
        ForStatement loop => loop with { Initializer = loop.Initializer is null ? null : RewriteStatement(loop.Initializer, members, locals), Condition = loop.Condition is null ? null : RewriteExpression(loop.Condition, members, locals), Iterators = loop.Iterators.Select(value => RewriteExpression(value, members, locals)).ToArray(), Body = RewriteStatement(loop.Body, members, locals) },
        ForInStatement loop => loop with { Collection = RewriteExpression(loop.Collection, members, locals), Body = RewriteStatement(loop.Body, members, locals) },
        SwitchStatement selection => selection with { Value = RewriteExpression(selection.Value, members, locals), Sections = selection.Sections.Select(section => section with { Labels = section.Labels.Select(label => RewriteLabel(label, members, locals)).ToArray(), Statements = section.Statements.Select(item => RewriteStatement(item, members, locals)).ToArray() }).ToArray() },
        ThrowStatement thrown => thrown with { Value = thrown.Value is null ? null : RewriteExpression(thrown.Value, members, locals) },
        TryStatement attempt => attempt with { Body = RewriteBlock(attempt.Body, members, locals), Catches = attempt.Catches.Select(clause => clause with { Body = RewriteBlock(clause.Body, members, locals) }).ToArray(), Finally = attempt.Finally is null ? null : RewriteBlock(attempt.Finally, members, locals) },
        OverflowContextStatement overflow => overflow with { Body = RewriteStatement(overflow.Body, members, locals) },
        _ => statement
    };

    private static Expression Receiver(SourceLocation at) => new NameExpression("$this", at);

    private static Expression RewriteExpression(Expression expression, HashSet<string> members, HashSet<string> locals) => expression switch
    {
        NameOfExpression nameOf => nameOf with { Operand = RewriteExpression(nameOf.Operand, members, locals) },
        ThisExpression self => Receiver(self.Location),
        NameExpression name when members.Contains(name.Name) && !locals.Contains(name.Name) => new MemberAccessExpression(Receiver(name.Location), name.Name, name.Location),
        UnaryExpression unary => unary with { Operand = RewriteExpression(unary.Operand, members, locals) },
        CastExpression cast => cast with { Operand = RewriteExpression(cast.Operand, members, locals) },
        BinaryExpression binary => binary with { Left = RewriteExpression(binary.Left, members, locals), Right = RewriteExpression(binary.Right, members, locals) },
        ConditionalExpression conditional => conditional with { Condition = RewriteExpression(conditional.Condition, members, locals), WhenTrue = RewriteExpression(conditional.WhenTrue, members, locals), WhenFalse = RewriteExpression(conditional.WhenFalse, members, locals) },
        MemberAccessExpression access => access with { Receiver = RewriteExpression(access.Receiver, members, locals) },
        IndexExpression index => index with { Receiver = RewriteExpression(index.Receiver, members, locals), Indices = index.Indices.Select(value => RewriteExpression(value, members, locals)).ToArray() },
        CallExpression call => call with { Target = RewriteExpression(call.Target, members, locals), Arguments = call.Arguments.Select(value => RewriteExpression(value, members, locals)).ToArray() },
        AllocationExpression allocation => allocation with { Dimensions = allocation.Dimensions.Select(value => RewriteExpression(value, members, locals)).ToArray(), Initializer = allocation.Initializer is null ? null : (InlineArrayInitializerExpression)RewriteExpression(allocation.Initializer, members, locals), ConstructorArguments = allocation.ConstructorArguments?.Select(value => RewriteExpression(value, members, locals)).ToArray() },
        InlineArrayInitializerExpression initializer => initializer with { Elements = initializer.Elements.Select(value => RewriteExpression(value, members, locals)).ToArray() },
        NullAssertionExpression assertion => assertion with { Operand = RewriteExpression(assertion.Operand, members, locals) },
        AwaitExpression awaitExpression => awaitExpression with { Operand = RewriteExpression(awaitExpression.Operand, members, locals) },
        AwaitBatchExpression batch => batch with { Tasks = batch.Tasks.Select(value => RewriteExpression(value, members, locals)).ToArray() },
        LambdaExpression lambda => RewriteLambda(lambda, members, locals),
        OverflowContextExpression overflow => overflow with { Value = RewriteExpression(overflow.Value, members, locals) },
        _ => expression
    };

    private static SwitchLabel RewriteLabel(SwitchLabel label, HashSet<string> members, HashSet<string> locals) => label with
    {
        Value = label.Value is null ? null : RewriteExpression(label.Value, members, locals),
        Guard = label.Guard is null ? null : RewriteExpression(label.Guard, members, locals),
        LeftPattern = label.LeftPattern is null ? null : RewriteLabel(label.LeftPattern, members, locals),
        RightPattern = label.RightPattern is null ? null : RewriteLabel(label.RightPattern, members, locals)
    };

    private static LambdaExpression RewriteLambda(LambdaExpression lambda, HashSet<string> members, HashSet<string> outerLocals)
    {
        var locals = new HashSet<string>(outerLocals, StringComparer.Ordinal);
        foreach (var parameter in lambda.Parameters) locals.Add(parameter.Name);
        if (lambda.BlockBody is not null)
            foreach (var name in GetDeclaredNames(lambda.BlockBody)) locals.Add(name);
        return lambda with
        {
            ExpressionBody = lambda.ExpressionBody is null ? null : RewriteExpression(lambda.ExpressionBody, members, locals),
            BlockBody = lambda.BlockBody is null ? null : RewriteBlock(lambda.BlockBody, members, locals)
        };
    }
}
