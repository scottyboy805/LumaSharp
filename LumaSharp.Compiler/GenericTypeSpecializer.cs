using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LumaSharp.Compiler;

internal sealed record GenericTypeInstantiation(TypeDeclaration Definition, IReadOnlyList<string> Arguments, TypeDeclaration RequestContext, SourceLocation Location, TypeDeclaration Specialized);
internal sealed record GenericExpansionResult(
    IReadOnlyList<TypeDeclaration> ConcreteTypes,
    IReadOnlyList<TypeDeclaration> GenericDefinitions,
    IReadOnlyList<GenericTypeInstantiation> Instantiations);

/// <summary>
/// Expands closed generic class, struct, interface, and delegate uses into ordinary
/// concrete declarations before semantic binding. Generic delegates may also be
/// closed lazily when specializing a generic method signature.
/// </summary>
internal sealed class GenericTypeSpecializer(DiagnosticBag diagnostics, int nativePointerSize, CancellationToken cancellationToken = default)
{
    private const int MaximumNestedGenericArgumentDepth = 64;
    private static readonly HashSet<string> TypeStringProperties = new(StringComparer.Ordinal)
    {
        "TypeName", "ReturnType", "BaseTypes", "EnumUnderlyingType", "TypePatternName", "ExplicitInterfaceName", "Constraints", "TypeArguments", "Name"
    };

    private readonly List<GenericTypeInstantiation> _instantiations = [];
    private readonly int _nativePointerSize = nativePointerSize;
    private readonly Dictionary<string, TypeDeclaration> _generatedByKey = new(StringComparer.Ordinal);
    private readonly HashSet<string> _reportedProblems = new(StringComparer.Ordinal);
    private IReadOnlyList<TypeDeclaration> _genericDefinitions = [];

    public GenericExpansionResult Expand(IReadOnlyList<TypeDeclaration> sourceTypes)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var genericDefinitions = sourceTypes.Where(type => type.HasTypeParameters).ToArray();
        _genericDefinitions = genericDefinitions;
        foreach (var unsupported in genericDefinitions.Where(type =>
                     type.Kind is not ("class" or "struct" or "interface" or "delegate")))
            AddOnce("unsupported:" + QualifiedName(unsupported), "LMC3000",
                $"Generic {unsupported.Kind} declarations are not yet supported by the current backend.", unsupported.Location);

        foreach (var malformed in genericDefinitions.Where(type => type.TypeParameters is null or { Count: 0 }))
            AddOnce("parameters:" + QualifiedName(malformed), "LMC3000",
                $"Generic type '{QualifiedName(malformed)}' has no supported type parameter list.", malformed.Location);

        foreach (var definition in genericDefinitions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateGenericBodyRestrictions(definition.TypeParameters ?? [], definition.GenericConstraints ?? [], definition.Members,
                definition.Kind, definition.Location, sourceTypes, definition);
            foreach (var method in definition.Members.OfType<MethodDeclaration>().Where(method => method.HasTypeParameters))
                ValidateGenericBodyRestrictions(method.TypeParameters ?? [], method.GenericConstraints ?? [], [method], definition.Kind,
                    method.Location, sourceTypes, definition);
        }
        foreach (var type in sourceTypes.Where(type => !type.HasTypeParameters))
        foreach (var method in type.Members.OfType<MethodDeclaration>().Where(method => method.HasTypeParameters))
            ValidateGenericBodyRestrictions(method.TypeParameters ?? [], method.GenericConstraints ?? [], [method], "class", method.Location,
                sourceTypes, type);
        foreach (var type in sourceTypes)
        foreach (var method in type.Members.OfType<MethodDeclaration>())
            if (type.HasTypeParameters || method.HasTypeParameters)
                ValidateGenericConstraintMemberUsage(type, method, sourceTypes);

        var definitions = genericDefinitions
            .Where(type => (type.Kind is "class" or "struct" or "interface" or "delegate") && type.TypeParameters is { Count: > 0 })
            .ToArray();
        var concreteTypes = sourceTypes.Where(type => !type.HasTypeParameters).ToList();
        var queue = new Queue<TypeDeclaration>(concreteTypes);
        var scanned = new HashSet<TypeDeclaration>(ReferenceEqualityComparer.Instance);

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = queue.Dequeue();
            if (!scanned.Add(current)) continue;
            var typeReferences = EnumerateTypeStrings(current).ToList();
            var currentInstantiation = _instantiations.FirstOrDefault(instance => ReferenceEquals(instance.Specialized, current));
            var genericLookupContext = currentInstantiation?.Definition ?? current;
            var openGenericMethodCalls = current.Members.OfType<MethodDeclaration>()
                .Where(method => method.HasTypeParameters && method.Body is not null)
                .SelectMany(method => EnumerateNodes(method.Body!).OfType<CallExpression>()
                    .Where(call => call.TypeArguments?.Any(argument =>
                        ReferencesAnyTypeParameter(argument, (method.TypeParameters ?? []).ToHashSet(StringComparer.Ordinal))) == true))
                .ToHashSet(ReferenceEqualityComparer.Instance);
            foreach (var call in EnumerateNodes(current).OfType<CallExpression>())
            {
                if (openGenericMethodCalls.Contains(call)) continue;
                if (call.Target is not NameExpression typeName || call.TypeArguments is not { Count: > 0 }) continue;
                if (genericLookupContext.Members.OfType<MethodDeclaration>().Any(method => method.HasTypeParameters && method.Name == typeName.Name))
                    continue;
                var definition = ResolveDefinition(typeName.Name, genericLookupContext, definitions);
                if (definition?.Kind == "struct" && definition.TypeParameters?.Count == call.TypeArguments.Count)
                    typeReferences.Add(($"{typeName.Name}<{string.Join(",", call.TypeArguments)}>", call.Location));
            }
            var referencedTypeText = typeReferences.Select(reference => reference.TypeText).ToArray();
            typeReferences.AddRange((current.Usings ?? []).Where(directive => directive.Alias is { } alias &&
                    directive.Target.Contains('<', StringComparison.Ordinal) &&
                    referencedTypeText.Any(typeText => Regex.IsMatch(typeText,
                        $"(?<![A-Za-z0-9_]){Regex.Escape(alias)}(?![A-Za-z0-9_])")))
                .Select(directive => (directive.Target, directive.Location)));
            foreach (var (typeText, location) in typeReferences)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var use in EnumerateGenericUses(typeText))
                {
                var definition = ResolveDefinition(use.Name, current, definitions);
                if (definition is null && currentInstantiation is not null)
                    definition = ResolveDefinition(use.Name, currentInstantiation.Definition, definitions);
                if (definition is null) continue;
                var parameters = definition.TypeParameters!;
                var key = MakeKey(definition, use.Arguments);
                if (parameters.Count != use.Arguments.Count)
                {
                    AddOnce("arity:" + key + ":" + location.File + ":" + location.Line + ":" + location.Column,
                        "LMC3000", $"Generic type '{QualifiedName(definition)}' requires {parameters.Count} type argument(s), but {use.Arguments.Count} were supplied.", location);
                    continue;
                }

                if (currentInstantiation is not null &&
                    QualifiedName(currentInstantiation.Definition) == QualifiedName(definition) &&
                    IsExpansiveRecursiveInstantiation(currentInstantiation.Arguments, use.Arguments))
                {
                    AddOnce("expansive-recursion:" + QualifiedName(definition) + ":" + location.File + ":" + location.Line + ":" + location.Column,
                        "LMC3000", $"Expansive recursive generic type '{QualifiedName(definition)}' cannot be monomorphized because its type arguments grow without reaching a closed set.", location);
                    continue;
                }

                if (use.Arguments.Any(argument => GetGenericArgumentDepth(argument) > MaximumNestedGenericArgumentDepth))
                {
                    AddOnce("depth:" + key + ":" + location.File + ":" + location.Line + ":" + location.Column,
                        "LMC3000", $"Generic type expansion exceeds the supported nesting limit of {MaximumNestedGenericArgumentDepth}.", location);
                    continue;
                }

                if (_generatedByKey.TryGetValue(key, out var existing))
                {
                    if (!scanned.Contains(existing)) queue.Enqueue(existing);
                    continue;
                }

                var specializedName = GetSpecializedName(definition, use.Arguments);
                var substitutions = parameters.Zip(use.Arguments).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
                var specialized = RewriteType(definition, specializedName, substitutions);
                _generatedByKey.Add(key, specialized);
                _instantiations.Add(new(definition, use.Arguments.ToArray(), current, location, specialized));
                concreteTypes.Add(specialized);
                queue.Enqueue(specialized);
                }
            }
        }

        var closedTypeNames = BuildClosedTypeNameMap(_instantiations);
        if (closedTypeNames.Count > 0)
            concreteTypes = concreteTypes.Select(type => RewriteClosedTypeReferences(type, closedTypeNames)).ToList();

        return new(concreteTypes, genericDefinitions, _instantiations.ToArray());
    }

    private static Dictionary<string, string> BuildClosedTypeNameMap(IReadOnlyList<GenericTypeInstantiation> instantiations)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var ambiguousSimpleUses = new HashSet<string>(StringComparer.Ordinal);
        foreach (var instantiation in instantiations)
        {
            var qualifiedDefinition = QualifiedName(instantiation.Definition);
            var qualifiedUse = qualifiedDefinition + "<" + string.Join(",", instantiation.Arguments) + ">";
            var qualifiedReplacement = QualifiedName(instantiation.Specialized);
            result[qualifiedUse] = qualifiedReplacement;

            var simpleUse = instantiation.Definition.Name + "<" + string.Join(",", instantiation.Arguments) + ">";
            if (ambiguousSimpleUses.Contains(simpleUse))
                continue;

            if (!result.TryGetValue(simpleUse, out var existing) || existing == qualifiedReplacement)
                result[simpleUse] = qualifiedReplacement;
            else
            {
                result.Remove(simpleUse);
                ambiguousSimpleUses.Add(simpleUse);
            }
        }

        return result;
    }

    private static TypeDeclaration RewriteClosedTypeReferences(TypeDeclaration type, IReadOnlyDictionary<string, string> substitutions) => type with
    {
        BaseTypes = type.BaseTypes?.Select(baseType => RewriteTypeName(baseType, substitutions)).ToArray(),
        EnumUnderlyingType = type.EnumUnderlyingType is null ? null : RewriteTypeName(type.EnumUnderlyingType, substitutions),
        DelegateReturnType = type.DelegateReturnType is null ? null : RewriteTypeName(type.DelegateReturnType, substitutions),
        DelegateParameters = type.DelegateParameters?.Select(parameter => RewriteParameter(parameter, substitutions)).ToArray(),
        Members = type.Members.Select(member => RewriteMember(member, type.Name, substitutions)).ToArray()
    };

    internal GenericTypeInstantiation? EnsureClosedDelegate(string definitionName, IReadOnlyList<string> arguments,
        TypeDeclaration context, SourceLocation location)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var definition = ResolveDefinition(definitionName, context, _genericDefinitions);
        if (definition is null || definition.Kind != "delegate" || definition.TypeParameters is not { } parameters || parameters.Count != arguments.Count)
            return null;

        var key = MakeKey(definition, arguments);
        if (_generatedByKey.TryGetValue(key, out var existing))
            return _instantiations.FirstOrDefault(instance => ReferenceEquals(instance.Specialized, existing));

        var specializedName = GetSpecializedName(definition, arguments);
        var substitutions = parameters.Zip(arguments).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
        var specialized = RewriteType(definition, specializedName, substitutions);
        var instantiation = new GenericTypeInstantiation(definition, arguments.ToArray(), context, location, specialized);
        _generatedByKey.Add(key, specialized);
        _instantiations.Add(instantiation);
        return instantiation;
    }

    private void AddOnce(string key, string code, string message, SourceLocation location)
    {
        if (_reportedProblems.Add(key)) diagnostics.Add(code, DiagnosticSeverity.Error, message, location);
    }

    private static string NumericPromotionTypeForTarget(string left, string right, int nativePointerSize) =>
        SemanticAnalyzer.NumericPromotionType(ExpandNativeIntegerType(left, nativePointerSize), ExpandNativeIntegerType(right, nativePointerSize));

    private static bool IsImplicitNumericConversionForTarget(string from, string to, int nativePointerSize)
    {
        from = ExpandNativeIntegerType(from, nativePointerSize);
        to = ExpandNativeIntegerType(to, nativePointerSize);
        if (from == to) return true;
        if (from == "float") return to == "double";
        if (from is "double" or "uint64" or "int64") return false;
        return from switch
        {
            "int8" => to is "int16" or "int32" or "int64" or "float" or "double",
            "uint8" => to is "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double",
            "int16" => to is "int32" or "int64" or "float" or "double",
            "uint16" => to is "int32" or "uint32" or "int64" or "uint64" or "float" or "double",
            "int32" => to is "int64" or "double",
            "uint32" => to is "int64" or "uint64" or "double",
            "char" => to is "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double",
            _ => false
        };
    }

    private static string ExpandNativeIntegerType(string typeName, int nativePointerSize) => typeName switch
    {
        "int" => nativePointerSize == 4 ? "int32" : "int64",
        "uint" => nativePointerSize == 4 ? "uint32" : "uint64",
        _ => typeName
    };

    private void ValidateGenericBodyRestrictions(IReadOnlyList<string> parameters,
        IReadOnlyList<GenericConstraintDeclaration> constraints, IReadOnlyList<MemberDeclaration> members,
        string containingKind, SourceLocation location, IReadOnlyList<TypeDeclaration> sourceTypes, TypeDeclaration contextType)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (parameters.Count == 0) return;
        var valueParameters = constraints.Where(clause => clause.Constraints.Any(item => item is "struct" or "enum"))
            .Select(clause => clause.TypeParameter).ToHashSet(StringComparer.Ordinal);
        var defaultParameters = constraints.Where(clause => clause.Constraints.Any(item => item is "default" or "struct" or "enum"))
            .Select(clause => clause.TypeParameter).ToHashSet(StringComparer.Ordinal);
        var constraintParameters = constraints.GroupBy(clause => clause.TypeParameter, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.SelectMany(clause => clause.Constraints).ToArray(), StringComparer.Ordinal);
        var knownVariableTypes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var method in members.OfType<MethodDeclaration>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var parameter in method.Parameters) knownVariableTypes[parameter.Name] = parameter.TypeName;
        }
        foreach (var variable in EnumerateNodes(members).OfType<VariableStatement>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (variable.TypeName is not null) knownVariableTypes[variable.Name] = variable.TypeName;
        }
        var inferredVariables = new List<VariableStatement>();
        foreach (var variable in EnumerateNodes(members).OfType<VariableStatement>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (variable.TypeName is null && variable.Initializer is not null) inferredVariables.Add(variable);
        }
        for (var pass = 0; pass < inferredVariables.Count; pass++)
            foreach (var variable in inferredVariables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (KnownExpressionType(variable.Initializer!) is { } inferredType && inferredType != "?")
                    knownVariableTypes[variable.Name] = inferredType;
            }

        foreach (var parameter in parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (containingKind == "struct" && members.OfType<FieldDeclaration>().Any(field => field.TypeName == parameter) && !defaultParameters.Contains(parameter))
                Report($"Generic struct field '{parameter}' requires a 'default', 'struct', or 'enum' constraint.", location);

            foreach (var node in EnumerateNodes(members))
            {
                cancellationToken.ThrowIfCancellationRequested();
                switch (node)
                {
                    case VariableStatement { TypeName: { } typeName, Initializer: null } variable when typeName == parameter && !defaultParameters.Contains(parameter):
                        Report($"Uninitialized local of type parameter '{parameter}' requires a 'default', 'struct', or 'enum' constraint.", variable.Location);
                        break;
                    case AllocationExpression { TypeName: { } typeName, Dimensions.Count: 0 } allocation when typeName == parameter:
                        ValidateTypeParameterConstruction(parameter, allocation);
                        break;
                    case AllocationExpression { TypeName: { } arrayType, Dimensions.Count: > 0 } allocation
                        when SemanticAnalyzer.TryParseManagedArrayType(arrayType, out var elementType, out _) &&
                             elementType == parameter && !defaultParameters.Contains(parameter) &&
                             !allocation.Dimensions.Any(dimension => dimension is LiteralExpression { Value: "0" }):
                        Report($"Allocating an array of type parameter '{parameter}' requires a 'default', 'struct', or 'enum' constraint; use '{parameter}?[]' for nullable elements.", allocation.Location);
                        break;
                    case DefaultValueExpression { TypeName: { } typeName } defaultValue when typeName == parameter && !defaultParameters.Contains(parameter):
                        Report($"default({parameter}) requires a 'default', 'struct', or 'enum' constraint.", defaultValue.Location);
                        break;
                    case VariableStatement { TypeName: { } typeName, Initializer: DefaultValueExpression { TypeName: null } } variable
                        when typeName == parameter && !defaultParameters.Contains(parameter):
                        Report($"A target-typed default for '{parameter}' requires a 'default', 'struct', or 'enum' constraint.", variable.Location);
                        break;
                }
            }
            if (!defaultParameters.Contains(parameter))
                foreach (var method in members.OfType<MethodDeclaration>().Where(method => method.ReturnType == parameter && method.Body is not null))
                    foreach (var returnedDefault in EnumerateNodes(method.Body!).OfType<ReturnStatement>()
                                 .Where(statement => statement.Value is DefaultValueExpression { TypeName: null }))
                        Report($"A target-typed default for '{parameter}' requires a 'default', 'struct', or 'enum' constraint.", returnedDefault.Location);
        }

        void Report(string message, SourceLocation at) => AddOnce("generic-body:" + message + ":" + at.File + ":" + at.Line + ":" + at.Column,
            "LMC3000", message, at);

        void ValidateTypeParameterConstruction(string parameter, AllocationExpression allocation)
        {
            var constructorArguments = allocation.ConstructorArguments ?? [];
            var constraintItems = constraintParameters.GetValueOrDefault(parameter) ?? [];
            if (constructorArguments.Count == 0)
            {
                if (valueParameters.Contains(parameter) || constraintItems.Contains("new()", StringComparer.Ordinal)) return;
                if (constraintItems.Any(item => item.StartsWith("new(", StringComparison.Ordinal) && item != "new()"))
                    Report($"Parameterless construction of type parameter '{parameter}' requires a 'struct', 'enum', or 'new()' constraint.", allocation.Location);
                else Report($"Construction of type parameter '{parameter}' requires a 'struct', 'enum', or 'new()' constraint.", allocation.Location);
                return;
            }

            var constructorConstraints = constraintItems.Where(item => item.StartsWith("new(", StringComparison.Ordinal) && item != "new()")
                .Select(item => item[4..^1]).ToArray();
            if (constructorConstraints.Length == 0)
            {
                ReportConstructor($"Construction of type parameter '{parameter}' with arguments requires a matching 'new(types)' constraint.", allocation.Location);
                return;
            }
            if (allocation.ConstructorArgumentNames?.Any(name => name is not null) == true)
            {
                ReportConstructor($"Named arguments cannot be validated against the type-only constructor constraint on '{parameter}'.", allocation.Location);
                return;
            }
            var actualTypes = constructorArguments.Select(KnownExpressionType).ToArray();
            var hasMatchingConstraint = constructorConstraints.Any(text =>
            {
                var expectedTypes = SplitArguments(text);
                if (expectedTypes.Count != actualTypes.Length) return false;
                for (var index = 0; index < expectedTypes.Count; index++)
                    if (actualTypes[index] is { } actualType && !ConstraintArgumentMayConvert(actualType, expectedTypes[index])) return false;
                return true;
            });
            if (!hasMatchingConstraint)
                ReportConstructor($"Arguments to construction of type parameter '{parameter}' do not match any 'new(types)' constraint.", allocation.Location);
        }

        string? KnownExpressionType(Expression expression) => expression switch
        {
            NameExpression name when knownVariableTypes.TryGetValue(name.Name, out var typeName) => typeName,
            LiteralExpression { Value: "true" or "false" } => "bool",
            LiteralExpression { Value: "null" } => "null",
            LiteralExpression literal when literal.Value.StartsWith('"') || literal.Value.StartsWith("@\"", StringComparison.Ordinal) => "string",
            LiteralExpression literal when literal.Value.StartsWith('\'') => "char",
            LiteralExpression literal when IsFloatingPointLiteral(literal.Value) => literal.Value.EndsWith('f') || literal.Value.EndsWith('F') ? "float" : "double",
            LiteralExpression literal => SemanticAnalyzer.ParseInteger(literal.Value).Type,
            CastExpression cast => cast.TypeName,
            DefaultValueExpression { TypeName: { } defaultType } => defaultType,
            AllocationExpression { TypeName: { } allocationType } => allocationType,
            _ => null
        };

        bool ConstraintArgumentMayConvert(string from, string to)
        {
            if (from == to) return true;
            if (from == "null") return to.EndsWith("?", StringComparison.Ordinal);
            if (to.EndsWith("?", StringComparison.Ordinal))
                return ConstraintArgumentMayConvert(from.EndsWith("?", StringComparison.Ordinal) ? from[..^1] : from, to[..^1]);
            if (from.EndsWith("?", StringComparison.Ordinal)) return false;
            if ((to is "object" or "System.Object") && IsPotentialReferenceType(from)) return true;
            if (to == "System.Array" && SemanticAnalyzer.TryParseManagedArrayType(from, out _, out _)) return true;
            return IsImplicitNumericConversionForTarget(from, to, _nativePointerSize) ||
                IsAssignableConstraintType(from, to, new HashSet<string>(StringComparer.Ordinal));
        }

        bool IsPotentialReferenceType(string typeName)
        {
            var nonNullableType = typeName.EndsWith("?", StringComparison.Ordinal) ? typeName[..^1] : typeName;
            return nonNullableType is "string" or "object" or "System.Object" ||
                ResolveDefinition(nonNullableType, contextType, sourceTypes) is { Kind: "class" or "interface" or "delegate" } ||
                EnumerateGenericUses(nonNullableType).Any(use => ResolveDefinition(use.Name, contextType, sourceTypes) is { Kind: "class" or "interface" or "delegate" });
        }

        bool IsAssignableConstraintType(string from, string to, HashSet<string> visited)
        {
            if (from == to) return true;
            if (!visited.Add(from)) return false;
            var definition = ResolveDefinition(from, contextType, sourceTypes);
            if (definition is null) return false;
            foreach (var baseType in definition.BaseTypes ?? [])
                if (baseType == to || IsAssignableConstraintType(baseType, to, visited)) return true;
            return false;
        }

        static bool IsFloatingPointLiteral(string value) =>
            !value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("0b", StringComparison.OrdinalIgnoreCase) &&
            (value.Contains('.') || value.Contains('e') || value.Contains('E'));

        void ReportConstructor(string message, SourceLocation at) => AddOnce("generic-constructor:" + message + ":" + at.File + ":" + at.Line + ":" + at.Column,
            "LS0751", message, at);
    }

    private void ValidateGenericConstraintMemberUsage(TypeDeclaration owner, MethodDeclaration method,
        IReadOnlyList<TypeDeclaration> sourceTypes)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (method.Body is null) return;
        var typeParameters = (owner.TypeParameters ?? []).Concat(method.TypeParameters ?? []).ToHashSet(StringComparer.Ordinal);
        if (typeParameters.Count == 0) return;

        var constraints = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var clause in owner.GenericConstraints ?? []) constraints[clause.TypeParameter] = clause.Constraints;
        foreach (var clause in method.GenericConstraints ?? []) constraints[clause.TypeParameter] = clause.Constraints;

        var variables = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var field in owner.Members.OfType<FieldDeclaration>()) variables[field.Name] = field.TypeName;
        foreach (var parameter in method.Parameters) variables[parameter.Name] = parameter.TypeName;
        var localVariables = new List<VariableStatement>();
        foreach (var variable in EnumerateNodes(method.Body).OfType<VariableStatement>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            localVariables.Add(variable);
        }
        foreach (var variable in localVariables.Where(variable => variable.TypeName is not null))
            variables[variable.Name] = variable.TypeName!;
        for (var pass = 0; pass < localVariables.Count; pass++)
        foreach (var variable in localVariables.Where(variable => variable.TypeName is null && variable.Initializer is not null))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (KnownExpressionType(variable.Initializer!) is { } inferredType && inferredType != "?")
                variables[variable.Name] = inferredType;
        }

        var callsByTarget = new Dictionary<MemberAccessExpression, CallExpression>(ReferenceEqualityComparer.Instance);
        foreach (var call in EnumerateNodes(method.Body).OfType<CallExpression>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (call.Target is MemberAccessExpression target) callsByTarget.TryAdd(target, call);
        }
        foreach (var access in EnumerateNodes(method.Body).OfType<MemberAccessExpression>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var receiverType = KnownExpressionType(access.Receiver);
            if (receiverType is null || !typeParameters.Contains(receiverType.TrimEnd('?'))) continue;
            receiverType = receiverType.TrimEnd('?');

            callsByTarget.TryGetValue(access, out var call);
            var (readsMember, writesMember, assignedValue) = GetMemberAccessUse(access);
            if (HasConstrainedMember(receiverType, access.Member, call, readsMember, writesMember, assignedValue,
                new HashSet<string>(StringComparer.Ordinal))) continue;

            var description = call is not null ? "method" : "member";
            diagnostics.Add("LS0200", DiagnosticSeverity.Error,
                $"Type parameter '{receiverType}' has no {description} '{access.Member}' available from its constraints.", access.Location);
        }

        foreach (var index in EnumerateNodes(method.Body).OfType<IndexExpression>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var receiverType = KnownExpressionType(index.Receiver);
            if (receiverType is null || !typeParameters.Contains(receiverType.TrimEnd('?'))) continue;
            receiverType = receiverType.TrimEnd('?');
            var (readsIndexer, writesIndexer, assignedValue) = GetIndexerAccessUse(index);
            if (HasConstrainedIndexer(receiverType, index, readsIndexer, writesIndexer, assignedValue,
                new HashSet<string>(StringComparer.Ordinal))) continue;
            diagnostics.Add("LS0200", DiagnosticSeverity.Error,
                $"Type parameter '{receiverType}' has no indexer with the supplied arguments available from its constraints.", index.Location);
        }

        foreach (var binary in EnumerateNodes(method.Body).OfType<BinaryExpression>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var constraintOperator = GetConstraintOperatorName(binary.Operator);
            if (constraintOperator is null) continue;
            var leftType = KnownExpressionType(binary.Left);
            var rightType = KnownExpressionType(binary.Right);
            if (leftType is null || rightType is null) continue;
            var constrainedParameter = GetTypeParameterOperand(leftType) ?? GetTypeParameterOperand(rightType);
            if (constrainedParameter is null) continue;
            if (HasConstraintOperator(constrainedParameter, constraintOperator, [leftType, rightType], new HashSet<string>(StringComparer.Ordinal))) continue;
            diagnostics.Add("LS0751", DiagnosticSeverity.Error,
                $"Operator '{binary.Operator}' is not available for type parameter '{constrainedParameter}' from its constraints.", binary.Location);
        }

        foreach (var unary in EnumerateNodes(method.Body).OfType<UnaryExpression>())
        {
            if (unary.Operator is not ("+" or "-" or "!" or "~" or "++" or "--")) continue;
            var operandType = KnownExpressionType(unary.Operand);
            var constrainedParameter = operandType is null ? null : GetTypeParameterOperand(operandType);
            if (constrainedParameter is null) continue;
            if (HasConstraintOperator(constrainedParameter, unary.Operator, [operandType!], new HashSet<string>(StringComparer.Ordinal))) continue;
            diagnostics.Add("LS0751", DiagnosticSeverity.Error,
                $"Operator '{unary.Operator}' is not available for type parameter '{constrainedParameter}' from its constraints.", unary.Location);
        }

        bool HasConstrainedMember(string parameter, string memberName, CallExpression? call, bool readsMember, bool writesMember,
            Expression? assignedValue, HashSet<string> visitedParameters)
        {
            if (!visitedParameters.Add(parameter)) return false;
            if (!constraints.TryGetValue(parameter, out var items)) return false;
            if (items.Contains("class", StringComparer.Ordinal) && memberName == "ToString" && (call is null || call.Arguments.Count == 0)) return true;
            foreach (var item in items)
            {
                if (item is "class" or "struct" or "enum" || item.StartsWith("new(", StringComparison.Ordinal)) continue;
                if (typeParameters.Contains(item) && HasConstrainedMember(item, memberName, call, readsMember, writesMember, assignedValue, visitedParameters)) return true;
                if (HasMemberOnConstraint(item, memberName, call, readsMember, writesMember, assignedValue,
                    new HashSet<string>(StringComparer.Ordinal))) return true;
            }
            return false;
        }

        bool HasMemberOnConstraint(string constraintType, string memberName, CallExpression? call, bool readsMember, bool writesMember,
            Expression? assignedValue, HashSet<string> visitedTypes)
        {
            var baseName = constraintType;
            var substitutions = new Dictionary<string, string>(StringComparer.Ordinal);
            var genericUse = EnumerateGenericUses(constraintType).FirstOrDefault();
            if (genericUse is not null)
            {
                baseName = genericUse.Name;
                var definition = ResolveDefinition(baseName, owner, sourceTypes);
                if (definition?.TypeParameters is { } parameters && parameters.Count == genericUse.Arguments.Count)
                    substitutions = parameters.Zip(genericUse.Arguments).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
            }
            if (!visitedTypes.Add(baseName)) return false;
            var declaredType = ResolveDefinition(baseName, owner, sourceTypes);
            if (declaredType is null) return false;
            var members = declaredType.Members;
            if (call is not null)
            {
                if (members.OfType<MethodDeclaration>().Any(candidate => !candidate.IsStatic && candidate.Name == memberName &&
                    (declaredType.Kind == "interface" || IsConstraintMemberAccessible(candidate.Accessibility)) &&
                    ConstraintCallMatches(call, candidate, substitutions))) return true;
            }
            else if (members.Any(candidate => candidate switch
                     {
                         FieldDeclaration field => field.Name == memberName && !field.IsStatic && IsConstraintMemberAccessible(field.Accessibility) &&
                             (!writesMember || !field.IsReadOnly && CanAssignToConstraintMember(assignedValue, field.TypeName, substitutions)),
                         ComputedPropertyDeclaration property => property.Name == memberName &&
                             ConstraintAccessorAccessible(declaredType, property.Accessibility, property.GetterAccessibility) &&
                             ConstraintAccessorAccessible(declaredType, property.Accessibility, property.SetterAccessibility) &&
                             (!readsMember || property.GetterBody is not null) && (!writesMember || property.SetterBody is not null &&
                                 CanAssignToConstraintMember(assignedValue, property.TypeName, substitutions)),
                         _ => false
                     })) return true;

            foreach (var baseType in declaredType.BaseTypes ?? [])
                if (HasMemberOnConstraint(RewriteTypeName(baseType, substitutions), memberName, call, readsMember, writesMember,
                    assignedValue, visitedTypes)) return true;
            return false;
        }

        bool HasConstrainedIndexer(string parameter, IndexExpression index, bool readsIndexer, bool writesIndexer,
            Expression? assignedValue, HashSet<string> visitedParameters)
        {
            if (!visitedParameters.Add(parameter) || !constraints.TryGetValue(parameter, out var items)) return false;
            foreach (var item in items)
            {
                if (item is "class" or "struct" or "enum" || item.StartsWith("new(", StringComparison.Ordinal)) continue;
                if (typeParameters.Contains(item))
                {
                    if (HasConstrainedIndexer(item, index, readsIndexer, writesIndexer, assignedValue, visitedParameters)) return true;
                }
                else if (HasIndexerOnConstraint(item, index, readsIndexer, writesIndexer, assignedValue,
                    new HashSet<string>(StringComparer.Ordinal))) return true;
            }
            return false;
        }

        bool HasConstraintOperator(string parameter, string operatorName, IReadOnlyList<string> operandTypes, HashSet<string> visitedParameters)
        {
            if ((operatorName is "==" or "!=") &&
                operandTypes.All(type => GetTypeParameterOperand(type) == parameter || type is "null" or "object" or "System.Object")) return true;
            if (!visitedParameters.Add(parameter) || !constraints.TryGetValue(parameter, out var items)) return false;
            foreach (var item in items)
            {
                if (item is "class" or "struct" or "enum" || item.StartsWith("new(", StringComparison.Ordinal)) continue;
                if (typeParameters.Contains(item))
                {
                    if (HasConstraintOperator(item, operatorName, operandTypes, visitedParameters)) return true;
                    continue;
                }
                if (HasOperatorOnConstraint(item, parameter, operatorName, operandTypes, new HashSet<string>(StringComparer.Ordinal))) return true;
            }
            return false;
        }

        bool HasOperatorOnConstraint(string constraintType, string typeParameter, string operatorName,
            IReadOnlyList<string> operandTypes, HashSet<string> visitedTypes)
        {
            var baseName = constraintType;
            var substitutions = new Dictionary<string, string>(StringComparer.Ordinal);
            var genericUse = EnumerateGenericUses(constraintType).FirstOrDefault();
            if (genericUse is not null)
            {
                baseName = genericUse.Name;
                var definition = ResolveDefinition(baseName, owner, sourceTypes);
                if (definition?.TypeParameters is { } parameters && parameters.Count == genericUse.Arguments.Count)
                    substitutions = parameters.Zip(genericUse.Arguments).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
            }
            if (!visitedTypes.Add(baseName)) return false;
            var declaredType = ResolveDefinition(baseName, owner, sourceTypes);
            if (declaredType is null) return false;
            var declaredOperatorName = operatorName == "!=" ? "==" : operatorName;
            var matchingOperator = declaredType.Members.OfType<MethodDeclaration>().Any(candidate => candidate.IsOperator && candidate.IsStatic &&
                candidate.Operator == declaredOperatorName && candidate.Parameters.Count == operandTypes.Count &&
                (declaredType.Kind == "interface" || IsConstraintMemberAccessible(candidate.Accessibility)) &&
                candidate.Parameters.Select(parameter => RewriteTypeName(parameter.TypeName, substitutions))
                    .Zip(operandTypes, (expected, actual) => ConstraintOperatorOperandMayConvert(typeParameter, actual, expected))
                    .All(matches => matches));
            if (matchingOperator) return true;
            foreach (var baseType in declaredType.BaseTypes ?? [])
                if (HasOperatorOnConstraint(RewriteTypeName(baseType, substitutions), typeParameter, operatorName, operandTypes, visitedTypes)) return true;
            return false;
        }

        bool ConstraintOperatorOperandMayConvert(string parameter, string actual, string expected)
        {
            if (actual == "null" && expected == parameter && constraints.GetValueOrDefault(parameter)?.Contains("class", StringComparer.Ordinal) == true)
                return true;
            if (typeParameters.Contains(actual))
            {
                if (actual == expected) return true;
                var actualConstraints = constraints.GetValueOrDefault(actual) ?? [];
                return actualConstraints.Any(item => item == expected || IsAssignableConstraintType(item, expected, new HashSet<string>(StringComparer.Ordinal)));
            }
            return ConstraintArgumentMayConvert(actual, expected);
        }

        string? GetTypeParameterOperand(string typeName) => typeParameters.Contains(typeName.TrimEnd('?')) ? typeName.TrimEnd('?') : null;

        static string? GetConstraintOperatorName(string operatorName) => operatorName switch
        {
            "+=" => "+", "-=" => "-", "*=" => "*", "/=" => "/", "%=" => "%", "&=" => "&", "|=" => "|", "^=" => "^",
            "<<=" => "<<", ">>=" => ">>",
            "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>" or "==" or "!=" or "<" or ">" or "<=" or ">=" => operatorName,
            _ => null
        };

        bool HasIndexerOnConstraint(string constraintType, IndexExpression index, bool readsIndexer, bool writesIndexer,
            Expression? assignedValue, HashSet<string> visitedTypes)
        {
            var baseName = constraintType;
            var substitutions = new Dictionary<string, string>(StringComparer.Ordinal);
            var genericUse = EnumerateGenericUses(constraintType).FirstOrDefault();
            if (genericUse is not null)
            {
                baseName = genericUse.Name;
                var definition = ResolveDefinition(baseName, owner, sourceTypes);
                if (definition?.TypeParameters is { } parameters && parameters.Count == genericUse.Arguments.Count)
                    substitutions = parameters.Zip(genericUse.Arguments).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
            }
            if (!visitedTypes.Add(baseName)) return false;
            var declaredType = ResolveDefinition(baseName, owner, sourceTypes);
            if (declaredType is null) return false;
            var syntheticCall = new CallExpression(new NameExpression("$constraint-indexer", index.Location), index.Indices, index.Location);
            var matchingIndexer = declaredType.Members.OfType<ComputedIndexerDeclaration>().Any(candidate =>
                ConstraintCallMatches(syntheticCall, new MethodDeclaration(owner.Name, "$constraint-indexer", candidate.TypeName,
                    candidate.Parameters, null, false, true, false, false, candidate.Location), substitutions) &&
                ConstraintAccessorAccessible(declaredType, candidate.Accessibility, candidate.GetterAccessibility) &&
                ConstraintAccessorAccessible(declaredType, candidate.Accessibility, candidate.SetterAccessibility) &&
                (!readsIndexer || candidate.GetterBody is not null) && (!writesIndexer || candidate.SetterBody is not null &&
                    CanAssignToConstraintMember(assignedValue, candidate.TypeName, substitutions)));
            if (matchingIndexer) return true;
            foreach (var baseType in declaredType.BaseTypes ?? [])
                if (HasIndexerOnConstraint(RewriteTypeName(baseType, substitutions), index, readsIndexer, writesIndexer,
                    assignedValue, visitedTypes)) return true;
            return false;
        }

        (bool Reads, bool Writes, Expression? AssignedValue) GetMemberAccessUse(MemberAccessExpression access)
        {
            foreach (var assignment in EnumerateNodes(method.Body).OfType<BinaryExpression>())
            {
                if (!ReferenceEquals(assignment.Left, access) || assignment.Operator is not ("=" or "+=" or "-=" or "*=" or "/=" or "%=" or
                    "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=")) continue;
                return assignment.Operator == "=" ? (false, true, assignment.Right) : (true, true, null);
            }
            foreach (var unary in EnumerateNodes(method.Body).OfType<UnaryExpression>())
                if (ReferenceEquals(unary.Operand, access) && unary.Operator is "++" or "--") return (true, true, null);
            return (true, false, null);
        }

        (bool Reads, bool Writes, Expression? AssignedValue) GetIndexerAccessUse(IndexExpression index)
        {
            foreach (var assignment in EnumerateNodes(method.Body).OfType<BinaryExpression>())
            {
                if (!ReferenceEquals(assignment.Left, index) || assignment.Operator is not ("=" or "+=" or "-=" or "*=" or "/=" or "%=" or
                    "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=")) continue;
                return assignment.Operator == "=" ? (false, true, assignment.Right) : (true, true, null);
            }
            foreach (var unary in EnumerateNodes(method.Body).OfType<UnaryExpression>())
                if (ReferenceEquals(unary.Operand, index) && unary.Operator is "++" or "--") return (true, true, null);
            return (true, false, null);
        }

        bool CanAssignToConstraintMember(Expression? value, string memberType, IReadOnlyDictionary<string, string> substitutions)
        {
            if (value is null) return true;
            var actualType = KnownExpressionType(value);
            return actualType is null || ConstraintArgumentMayConvert(actualType, RewriteTypeName(memberType, substitutions));
        }

        static bool ConstraintAccessorAccessible(TypeDeclaration declaredType, string propertyAccessibility, string? accessorAccessibility) =>
            declaredType.Kind == "interface" || IsConstraintMemberAccessible(accessorAccessibility ?? propertyAccessibility);

        bool ConstraintCallMatches(CallExpression call, MethodDeclaration candidate, IReadOnlyDictionary<string, string> substitutions)
        {
            var argumentNames = call.ArgumentNames ?? [];
            var argumentToParameter = new int[call.Arguments.Count];
            var assigned = new HashSet<int>();
            var nextPositional = 0;
            for (var argumentIndex = 0; argumentIndex < call.Arguments.Count; argumentIndex++)
            {
                var suppliedName = argumentIndex < argumentNames.Count ? argumentNames[argumentIndex] : null;
                var parameterIndex = suppliedName is null
                    ? FindNextPositionalParameter(candidate.Parameters, assigned, ref nextPositional)
                    : FindParameterByName(candidate.Parameters, suppliedName);
                if (parameterIndex < 0 || parameterIndex >= candidate.Parameters.Count) return false;
                if (!candidate.Parameters[parameterIndex].IsVariadic && !assigned.Add(parameterIndex)) return false;
                assigned.Add(parameterIndex);
                argumentToParameter[argumentIndex] = parameterIndex;
            }

            for (var parameterIndex = 0; parameterIndex < candidate.Parameters.Count; parameterIndex++)
            {
                if (assigned.Contains(parameterIndex)) continue;
                var parameter = candidate.Parameters[parameterIndex];
                if (parameter.DefaultValue is not null || parameter.IsVariadic) continue;
                return false;
            }

            var argumentModes = call.ArgumentModes ?? [];
            for (var argumentIndex = 0; argumentIndex < call.Arguments.Count; argumentIndex++)
            {
                var parameter = candidate.Parameters[argumentToParameter[argumentIndex]];
                var suppliedMode = argumentIndex < argumentModes.Count ? argumentModes[argumentIndex] ?? "" : "";
                if (suppliedMode != parameter.PassingMode) return false;
                var actualType = KnownExpressionType(call.Arguments[argumentIndex]);
                var expectedType = RewriteTypeName(parameter.TypeName, substitutions);
                if (actualType is null)
                {
                    if (call.Arguments[argumentIndex] is LambdaExpression lambda &&
                        LambdaConvertsToDelegate(lambda, expectedType) is false) return false;
                    if (MethodGroupConvertsToDelegate(call.Arguments[argumentIndex], expectedType) is false) return false;
                    continue;
                }
                if (suppliedMode is "ref" or "in" or "out")
                {
                    if (actualType != expectedType) return false;
                }
                else if (!ConstraintArgumentMayConvert(actualType, expectedType)) return false;
            }
            return true;
        }

        bool? LambdaConvertsToDelegate(LambdaExpression lambda, string expectedType)
        {
            var delegateTypeText = expectedType.EndsWith("?", StringComparison.Ordinal) ? expectedType[..^1] : expectedType;
            var genericUse = EnumerateGenericUses(delegateTypeText).FirstOrDefault();
            var delegateName = genericUse?.Name ?? delegateTypeText;
            var delegateType = ResolveDefinition(delegateName, owner, sourceTypes);
            if (delegateType is null) return null;
            if (delegateType.Kind != "delegate") return false;

            var delegateTypeParameters = delegateType.TypeParameters ?? [];
            var delegateTypeArguments = genericUse?.Arguments ?? [];
            if (delegateTypeParameters.Count != delegateTypeArguments.Count) return false;
            var delegateSubstitutions = delegateTypeParameters.Zip(delegateTypeArguments)
                .ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
            var delegateParameters = delegateType.DelegateParameters ?? [];
            if (lambda.Parameters.Count != delegateParameters.Count) return false;

            for (var index = 0; index < lambda.Parameters.Count; index++)
            {
                var explicitType = lambda.Parameters[index].TypeName;
                var delegateParameterType = RewriteTypeName(delegateParameters[index].TypeName, delegateSubstitutions);
                if (explicitType is not null && explicitType != delegateParameterType) return false;
            }

            var previousParameterTypes = new Dictionary<string, string?>(StringComparer.Ordinal);
            try
            {
                for (var index = 0; index < lambda.Parameters.Count; index++)
                {
                    var lambdaParameter = lambda.Parameters[index];
                    var delegateParameterType = RewriteTypeName(delegateParameters[index].TypeName, delegateSubstitutions);
                    if (!previousParameterTypes.ContainsKey(lambdaParameter.Name))
                        previousParameterTypes[lambdaParameter.Name] = variables.TryGetValue(lambdaParameter.Name, out var previousType) ? previousType : null;
                    variables[lambdaParameter.Name] = delegateParameterType;
                }

                var delegateReturnType = RewriteTypeName(delegateType.DelegateReturnType ?? "void", delegateSubstitutions);
                var returnExpressions = lambda.ExpressionBody is not null
                    ? [lambda.ExpressionBody]
                    : lambda.BlockBody is not null ? EnumerateLambdaReturns(lambda.BlockBody).ToArray() : [];
                if (delegateReturnType == "void")
                    return lambda.ExpressionBody is not null || returnExpressions.All(expression => expression is null);
                if (returnExpressions.Length == 0) return false;

                foreach (var expression in returnExpressions)
                {
                    if (expression is null) return false;
                    var actualReturnType = KnownExpressionType(expression);
                    if (actualReturnType is not null && !ConstraintArgumentMayConvert(actualReturnType, delegateReturnType)) return false;
                }
                return true;
            }
            finally
            {
                foreach (var (name, previousType) in previousParameterTypes)
                {
                    if (previousType is null) variables.Remove(name);
                    else variables[name] = previousType;
                }
            }
        }

        bool? MethodGroupConvertsToDelegate(Expression expression, string expectedType)
        {
            if (expression is not NameExpression methodName || variables.ContainsKey(methodName.Name) ||
                EnumerateNodes(method.Body).OfType<VariableStatement>().Any(variable => variable.Name == methodName.Name)) return null;

            var genericUse = EnumerateGenericUses(expectedType.EndsWith("?", StringComparison.Ordinal)
                ? expectedType[..^1] : expectedType).FirstOrDefault();
            var delegateName = genericUse?.Name ?? expectedType;
            if (delegateName.EndsWith("?", StringComparison.Ordinal)) delegateName = delegateName[..^1];
            var delegateType = ResolveDefinition(delegateName, owner, sourceTypes);
            if (delegateType is null) return null;
            if (delegateType.Kind != "delegate") return false;

            var delegateTypeParameters = delegateType.TypeParameters ?? [];
            var delegateTypeArguments = genericUse?.Arguments ?? [];
            if (delegateTypeParameters.Count != delegateTypeArguments.Count) return false;
            var delegateSubstitutions = delegateTypeParameters.Zip(delegateTypeArguments)
                .ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
            var delegateParameters = delegateType.DelegateParameters ?? [];
            var candidates = owner.Members.OfType<MethodDeclaration>()
                .Where(candidate => candidate.Name == methodName.Name).ToArray();
            if (candidates.Length == 0) return null;

            var compatible = candidates.Where(candidate => !candidate.HasTypeParameters &&
                    (method.IsStatic || !candidate.IsStatic) && candidate.Parameters.Count == delegateParameters.Count &&
                    candidate.Parameters.Select((parameter, index) =>
                    {
                        var delegateParameter = delegateParameters[index];
                        var expectedParameterType = RewriteTypeName(delegateParameter.TypeName, delegateSubstitutions);
                        return parameter.PassingMode == delegateParameter.PassingMode && parameter.TypeName == expectedParameterType;
                    }).All(matches => matches))
                .Any(candidate => delegateType.DelegateReturnType == "void"
                    ? candidate.ReturnType == "void"
                    : ConstraintArgumentMayConvert(candidate.ReturnType,
                        RewriteTypeName(delegateType.DelegateReturnType ?? "void", delegateSubstitutions)));
            if (compatible) return true;
            return candidates.Any(candidate => candidate.HasTypeParameters) ? null : false;
        }

        static IEnumerable<Expression?> EnumerateLambdaReturns(Statement statement) => statement switch
        {
            ReturnStatement ret => [ret.Value],
            BlockStatement block => block.Statements.SelectMany(EnumerateLambdaReturns),
            IfStatement conditional => EnumerateLambdaReturns(conditional.Then)
                .Concat(conditional.Else is null ? [] : EnumerateLambdaReturns(conditional.Else)),
            WhileStatement loop => EnumerateLambdaReturns(loop.Body),
            DoWhileStatement loop => EnumerateLambdaReturns(loop.Body),
            ForStatement loop => EnumerateLambdaReturns(loop.Body),
            ForInStatement loop => EnumerateLambdaReturns(loop.Body),
            SwitchStatement selection => selection.Sections.SelectMany(section => section.Statements).SelectMany(EnumerateLambdaReturns),
            TryStatement attempt => EnumerateLambdaReturns(attempt.Body)
                .Concat(attempt.Catches.SelectMany(clause => EnumerateLambdaReturns(clause.Body)))
                .Concat(attempt.Finally is null ? [] : EnumerateLambdaReturns(attempt.Finally)),
            OverflowContextStatement overflow => EnumerateLambdaReturns(overflow.Body),
            _ => []
        };

        static int FindNextPositionalParameter(IReadOnlyList<ParameterDeclaration> parameters, HashSet<int> assigned, ref int next)
        {
            if (parameters.Count > 0 && parameters[^1].IsVariadic && next >= parameters.Count) return parameters.Count - 1;
            while (next < parameters.Count && assigned.Contains(next)) next++;
            if (parameters.Count > 0 && parameters[^1].IsVariadic && (next >= parameters.Count || assigned.Contains(parameters.Count - 1)))
                return parameters.Count - 1;
            return next < parameters.Count ? next++ : -1;
        }

        static int FindParameterByName(IReadOnlyList<ParameterDeclaration> parameters, string name)
        {
            for (var index = 0; index < parameters.Count; index++)
                if (parameters[index].Name == name) return index;
            return -1;
        }

        string? KnownExpressionType(Expression expression) => expression switch
        {
            NameExpression name when variables.TryGetValue(name.Name, out var variableType) => variableType,
            MemberAccessExpression access => GetConstrainedMemberType(access, null),
            CallExpression { Target: MemberAccessExpression access } call => GetConstrainedMemberType(access, call),
            CallExpression { Target: NameExpression target } call => GetUnqualifiedCallResultType(target.Name, call),
            IndexExpression index => GetConstrainedIndexerType(index),
            ThisExpression => QualifiedName(owner),
            BaseExpression => owner.BaseTypes?.FirstOrDefault(),
            LiteralExpression { Value: "true" or "false" } => "bool",
            LiteralExpression { Value: "null" } => "null",
            LiteralExpression literal when literal.Value.StartsWith('"') || literal.Value.StartsWith("@\"", StringComparison.Ordinal) => "string",
            LiteralExpression literal when literal.Value.StartsWith('\'') => "char",
            LiteralExpression literal when IsFloatingPointLiteral(literal.Value) => literal.Value.EndsWith('f') || literal.Value.EndsWith('F') ? "float" : "double",
            LiteralExpression literal => SemanticAnalyzer.ParseInteger(literal.Value).Type,
            CastExpression cast => cast.TypeName,
            DefaultValueExpression { TypeName: { } typeName } => typeName,
            AllocationExpression { TypeName: { } typeName } => typeName,
            UnaryExpression { Operator: "-", Operand: LiteralExpression literal } => SemanticAnalyzer.ParseInteger(literal.Value).Type,
            UnaryExpression unary => GetUnaryResultType(unary),
            BinaryExpression binary => GetBinaryResultType(binary),
            ConditionalExpression conditional => GetCommonExpressionType(
                KnownExpressionType(conditional.WhenTrue), KnownExpressionType(conditional.WhenFalse)),
            NullAssertionExpression assertion => KnownExpressionType(assertion.Operand) is { } assertedType
                ? assertedType.EndsWith('?') ? assertedType[..^1] : assertedType
                : null,
            OverflowContextExpression overflow => KnownExpressionType(overflow.Value),
            SizeOfExpression => "int32",
            _ => null
        };

        string? GetUnqualifiedCallResultType(string name, CallExpression call)
        {
            var returnTypes = owner.Members.OfType<MethodDeclaration>()
                .Where(candidate => candidate.Name == name)
                .Select(candidate => GetMethodCallResultType(call, candidate))
                .Where(returnType => returnType is not null)
                .Distinct(StringComparer.Ordinal).ToArray();
            return returnTypes.Length == 1 ? returnTypes[0] : null;
        }

        string? GetMethodCallResultType(CallExpression call, MethodDeclaration candidate,
            IReadOnlyDictionary<string, string>? inheritedSubstitutions = null)
        {
            var substitutions = inheritedSubstitutions?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
                ?? new Dictionary<string, string>(StringComparer.Ordinal);
            if (candidate.HasTypeParameters)
            {
                var typeParameters = candidate.TypeParameters!;
                if (call.TypeArguments is { } typeArguments)
                {
                    if (typeArguments.Count != typeParameters.Count) return null;
                    foreach (var (parameter, argument) in typeParameters.Zip(typeArguments))
                        substitutions[parameter] = RewriteTypeName(argument, inheritedSubstitutions ?? new Dictionary<string, string>(StringComparer.Ordinal));
                }
                else if (!TryInferMethodTypeArguments(call, candidate, typeParameters, substitutions, inheritedSubstitutions))
                {
                    return null;
                }
            }
            else if (call.TypeArguments is { Count: > 0 })
            {
                return null;
            }

            return ConstraintCallMatches(call, candidate, substitutions)
                ? RewriteTypeName(candidate.ReturnType, substitutions)
                : null;
        }

        bool TryInferMethodTypeArguments(CallExpression call, MethodDeclaration candidate,
            IReadOnlyList<string> typeParameters, Dictionary<string, string> substitutions,
            IReadOnlyDictionary<string, string>? inheritedSubstitutions)
        {
            var argumentNames = call.ArgumentNames ?? [];
            var argumentToParameter = new int[call.Arguments.Count];
            var assigned = new HashSet<int>();
            var nextPositional = 0;
            for (var argumentIndex = 0; argumentIndex < call.Arguments.Count; argumentIndex++)
            {
                var suppliedName = argumentIndex < argumentNames.Count ? argumentNames[argumentIndex] : null;
                var parameterIndex = suppliedName is null
                    ? FindNextPositionalParameter(candidate.Parameters, assigned, ref nextPositional)
                    : FindParameterByName(candidate.Parameters, suppliedName);
                if (parameterIndex < 0 || parameterIndex >= candidate.Parameters.Count) return false;
                if (!candidate.Parameters[parameterIndex].IsVariadic && !assigned.Add(parameterIndex)) return false;
                assigned.Add(parameterIndex);
                argumentToParameter[argumentIndex] = parameterIndex;
            }

            var genericParameters = typeParameters.ToHashSet(StringComparer.Ordinal);
            for (var argumentIndex = 0; argumentIndex < call.Arguments.Count; argumentIndex++)
            {
                var actualType = KnownExpressionType(call.Arguments[argumentIndex]);
                if (actualType is null or "null") continue;
                var parameterType = RewriteTypeName(candidate.Parameters[argumentToParameter[argumentIndex]].TypeName,
                    inheritedSubstitutions ?? new Dictionary<string, string>(StringComparer.Ordinal));
                if (!InferMethodTypeArgument(parameterType, actualType, genericParameters, substitutions)) return false;
                if (!genericParameters.Contains(parameterType) || !substitutions.TryGetValue(parameterType, out var inferredType) || inferredType == actualType)
                    continue;

                if (candidate.Parameters[argumentToParameter[argumentIndex]].PassingMode is "ref" or "in" or "out") return false;
                var commonType = GetCommonExpressionType(inferredType, actualType);
                if (commonType is null || !ConstraintArgumentMayConvert(inferredType, commonType) ||
                    !ConstraintArgumentMayConvert(actualType, commonType)) return false;
                substitutions[parameterType] = commonType;
            }

            return typeParameters.All(substitutions.ContainsKey);
        }

        bool InferMethodTypeArgument(string parameterType, string argumentType, HashSet<string> genericParameters,
            Dictionary<string, string> substitutions)
        {
            if (parameterType.EndsWith("?", StringComparison.Ordinal))
            {
                if (argumentType == "null") return true;
                parameterType = parameterType[..^1];
                if (argumentType.EndsWith("?", StringComparison.Ordinal)) argumentType = argumentType[..^1];
            }

            if (genericParameters.Contains(parameterType))
            {
                if (substitutions.ContainsKey(parameterType)) return true;
                substitutions.Add(parameterType, argumentType);
                return true;
            }

            var isParameterArray = SemanticAnalyzer.TryParseManagedArrayType(parameterType, out var parameterElement, out var parameterRank);
            var isArgumentArray = SemanticAnalyzer.TryParseManagedArrayType(argumentType, out var argumentElement, out var argumentRank);
            if (isParameterArray || isArgumentArray)
                return isParameterArray && isArgumentArray && parameterRank == argumentRank &&
                    InferMethodTypeArgument(parameterElement, argumentElement, genericParameters, substitutions);

            var parameterUse = EnumerateGenericUses(parameterType).FirstOrDefault();
            var argumentUse = EnumerateGenericUses(argumentType).FirstOrDefault();
            if (parameterUse is not null && argumentUse is not null &&
                SameGenericDefinition(parameterUse.Name, argumentUse.Name) &&
                parameterUse.Arguments.Count == argumentUse.Arguments.Count)
            {
                for (var index = 0; index < parameterUse.Arguments.Count; index++)
                    if (!InferMethodTypeArgument(parameterUse.Arguments[index], argumentUse.Arguments[index], genericParameters, substitutions))
                        return false;
            }

            return true;
        }

        bool SameGenericDefinition(string parameterName, string argumentName)
        {
            if (parameterName == argumentName) return true;
            var parameterDefinition = ResolveDefinition(parameterName, owner, sourceTypes);
            return parameterDefinition is not null && ReferenceEquals(parameterDefinition,
                ResolveDefinition(argumentName, owner, sourceTypes));
        }

        string? GetUnaryResultType(UnaryExpression unary)
        {
            var operandType = KnownExpressionType(unary.Operand);
            if (operandType is null) return null;
            if (unary.Operator == "!") return "bool";
            if (unary.Operator is "&" or "*") return unary.Operator == "&" ? operandType + "*" : null;
            if (unary.Operator is "+" or "-" or "~" or "++" or "--")
            {
                var underlying = operandType.EndsWith('?') ? operandType[..^1] : operandType;
                if (IsNumericConstraintType(underlying)) return SemanticAnalyzer.PromoteSmallInteger(underlying);
                if (typeParameters.Contains(underlying)) return GetConstrainedUnaryOperatorResult(underlying, unary.Operator);
            }
            return operandType;
        }

        string? GetBinaryResultType(BinaryExpression binary)
        {
            var left = KnownExpressionType(binary.Left);
            var right = KnownExpressionType(binary.Right);
            if (binary.Operator is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or
                "<<=" or ">>=" or "??=") return left;
            if (binary.Operator is "&&" or "||" or "==" or "!=" or "<" or ">" or "<=" or ">=" or "is" or "is not") return "bool";
            if (binary.Operator == "??")
            {
                if (left is null || right is null) return null;
                var nonNullableLeft = left.EndsWith('?') ? left[..^1] : left;
                return GetCommonExpressionType(nonNullableLeft, right);
            }
            if (left is null || right is null) return null;
            if (binary.Operator == "+" && (left == "string" || right == "string" || left == "null" || right == "null")) return "string";
            if (IsNumericConstraintType(left) && IsNumericConstraintType(right) &&
                binary.Operator is "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>")
                return NumericPromotionTypeForTarget(left, right, _nativePointerSize);
            if ((binary.Operator is "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>") &&
                typeParameters.Contains(left) && left == right)
                return GetConstrainedBinaryOperatorResult(left, binary.Operator, [left, right]);
            return null;
        }

        string? GetCommonExpressionType(string? left, string? right)
        {
            if (left is null || right is null) return null;
            if (left == right) return left;
            if (left == "null") return IsPotentialReferenceType(right) ? right.EndsWith('?') ? right : right + "?" : null;
            if (right == "null") return IsPotentialReferenceType(left) ? left.EndsWith('?') ? left : left + "?" : null;
            if (left.EndsWith('?') && left[..^1] == right) return left;
            if (right.EndsWith('?') && right[..^1] == left) return right;
            if (IsNumericConstraintType(left) && IsNumericConstraintType(right)) return NumericPromotionTypeForTarget(left, right, _nativePointerSize);
            if (ConstraintArgumentMayConvert(left, right)) return right;
            if (ConstraintArgumentMayConvert(right, left)) return left;
            return null;
        }

        bool IsPotentialReferenceType(string typeName)
        {
            var nonNullableType = typeName.EndsWith('?') ? typeName[..^1] : typeName;
            return nonNullableType is "string" or "object" or "System.Object" ||
                ResolveDefinition(nonNullableType, owner, sourceTypes) is { Kind: "class" or "interface" or "delegate" } ||
                EnumerateGenericUses(nonNullableType).Any(use => ResolveDefinition(use.Name, owner, sourceTypes) is { Kind: "class" or "interface" or "delegate" });
        }

        string? GetConstrainedUnaryOperatorResult(string parameter, string operatorName)
        {
            foreach (var item in constraints.GetValueOrDefault(parameter) ?? [])
            {
                if (item is "class" or "struct" or "enum" || item.StartsWith("new(", StringComparison.Ordinal)) continue;
                var definitionName = EnumerateGenericUses(item).FirstOrDefault()?.Name ?? item;
                var definition = ResolveDefinition(definitionName, owner, sourceTypes);
                var resultTypes = definition?.Members.OfType<MethodDeclaration>()
                    .Where(candidate => candidate.IsOperator && candidate.Operator == operatorName && candidate.Parameters.Count == 1 &&
                        (definition.Kind == "interface" || IsConstraintMemberAccessible(candidate.Accessibility)))
                    .Select(candidate => candidate.ReturnType).Distinct(StringComparer.Ordinal).ToArray() ?? [];
                if (resultTypes.Length == 1) return resultTypes[0];
            }
            return null;
        }

        string? GetConstrainedBinaryOperatorResult(string parameter, string operatorName, IReadOnlyList<string> operandTypes)
        {
            foreach (var item in constraints.GetValueOrDefault(parameter) ?? [])
            {
                if (item is "class" or "struct" or "enum" || item.StartsWith("new(", StringComparison.Ordinal)) continue;
                var definitionName = EnumerateGenericUses(item).FirstOrDefault()?.Name ?? item;
                var definition = ResolveDefinition(definitionName, owner, sourceTypes);
                var resultTypes = definition?.Members.OfType<MethodDeclaration>()
                    .Where(candidate => candidate.IsOperator && candidate.IsStatic && candidate.Operator == operatorName &&
                        candidate.Parameters.Count == operandTypes.Count &&
                        (definition.Kind == "interface" || IsConstraintMemberAccessible(candidate.Accessibility)) &&
                        candidate.Parameters.Select(parameter => parameter.TypeName).Zip(operandTypes,
                            (expected, actual) => expected == actual || typeParameters.Contains(actual) &&
                                (constraints.GetValueOrDefault(actual) ?? []).Any(constraint => constraint == expected)).All(matches => matches))
                    .Select(candidate => candidate.ReturnType).Distinct(StringComparer.Ordinal).ToArray() ?? [];
                if (resultTypes.Length == 1) return resultTypes[0];
            }
            return null;
        }

        string? GetConstrainedMemberType(MemberAccessExpression access, CallExpression? call)
        {
            var receiverType = KnownExpressionType(access.Receiver);
            var isStaticAccess = false;
            if (receiverType is null && access.Receiver is NameExpression typeName && !variables.ContainsKey(typeName.Name) &&
                ResolveDefinition(typeName.Name, owner, sourceTypes) is { } typeReference)
            {
                receiverType = QualifiedName(typeReference);
                isStaticAccess = true;
            }
            if (receiverType is null) return null;
            var baseType = receiverType.TrimEnd('?');
            if (typeParameters.Contains(baseType))
                return GetMemberTypeFromConstraints(baseType, access.Member, call, new HashSet<string>(StringComparer.Ordinal),
                    new HashSet<string>(StringComparer.Ordinal));
            return GetMemberTypeFromDeclaredType(baseType, access.Member, call, new Dictionary<string, string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal), isStaticAccess);
        }

        string? GetMemberTypeFromConstraints(string parameter, string memberName, CallExpression? call, HashSet<string> visitedParameters,
            HashSet<string> visitedTypes)
        {
            if (!visitedParameters.Add(parameter) || !constraints.TryGetValue(parameter, out var items)) return null;
            if (call is not null && items.Contains("class", StringComparer.Ordinal) && memberName == "ToString" && call.Arguments.Count == 0)
                return "string";
            foreach (var item in items)
            {
                if (item is "class" or "struct" or "enum" || item.StartsWith("new(", StringComparison.Ordinal)) continue;
                if (typeParameters.Contains(item))
                {
                    var nested = GetMemberTypeFromConstraints(item, memberName, call, visitedParameters, visitedTypes);
                    if (nested is not null) return nested;
                }
                else
                {
                    var memberType = GetMemberTypeFromDeclaredType(item, memberName, call,
                        new Dictionary<string, string>(StringComparer.Ordinal), visitedTypes, requireStatic: false);
                    if (memberType is not null) return memberType;
                }
            }
            return null;
        }

        string? GetMemberTypeFromDeclaredType(string declaredTypeName, string memberName, CallExpression? call,
            IReadOnlyDictionary<string, string> inheritedSubstitutions, HashSet<string> visitedTypes, bool requireStatic)
        {
            var baseName = declaredTypeName;
            var substitutions = inheritedSubstitutions.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            var genericUse = EnumerateGenericUses(declaredTypeName).FirstOrDefault();
            if (genericUse is not null)
            {
                baseName = genericUse.Name;
                var definition = ResolveDefinition(baseName, owner, sourceTypes);
                if (definition?.TypeParameters is { } parameters && parameters.Count == genericUse.Arguments.Count)
                    foreach (var pair in parameters.Zip(genericUse.Arguments)) substitutions[pair.First] = RewriteTypeName(pair.Second, inheritedSubstitutions);
            }
            if (!visitedTypes.Add(baseName)) return null;
            var declaredType = ResolveDefinition(baseName, owner, sourceTypes);
            if (declaredType is null) return null;
            if (call is not null)
            {
                var returnTypes = declaredType.Members.OfType<MethodDeclaration>()
                    .Where(candidate => candidate.IsStatic == requireStatic && candidate.Name == memberName &&
                        (declaredType.Kind == "interface" || IsConstraintMemberAccessible(candidate.Accessibility)))
                    .Select(candidate => GetMethodCallResultType(call, candidate, substitutions))
                    .Where(returnType => returnType is not null)
                    .Distinct(StringComparer.Ordinal).ToArray();
                if (returnTypes.Length == 1) return returnTypes[0];
                if (returnTypes.Length > 1) return null;
            }
            var field = declaredType.Members.OfType<FieldDeclaration>().FirstOrDefault(candidate => candidate.Name == memberName &&
                candidate.IsStatic == requireStatic && IsConstraintMemberAccessible(candidate.Accessibility));
            if (field is not null) return RewriteTypeName(field.TypeName, substitutions);
            var property = declaredType.Members.OfType<ComputedPropertyDeclaration>().FirstOrDefault(candidate => candidate.Name == memberName &&
                candidate.IsStatic == requireStatic &&
                candidate.GetterBody is not null && ConstraintAccessorAccessible(declaredType, candidate.Accessibility, candidate.GetterAccessibility));
            if (property is not null) return RewriteTypeName(property.TypeName, substitutions);
            foreach (var baseType in declaredType.BaseTypes ?? [])
            {
                var inherited = GetMemberTypeFromDeclaredType(RewriteTypeName(baseType, substitutions), memberName, call,
                    substitutions, visitedTypes, requireStatic);
                if (inherited is not null) return inherited;
            }
            return null;
        }

        string? GetConstrainedIndexerType(IndexExpression index)
        {
            var receiverType = KnownExpressionType(index.Receiver);
            if (receiverType is null) return null;
            receiverType = receiverType.TrimEnd('?');
            if (typeParameters.Contains(receiverType))
                return GetIndexerTypeFromConstraints(receiverType, index, new HashSet<string>(StringComparer.Ordinal),
                    new HashSet<string>(StringComparer.Ordinal));
            return GetIndexerTypeFromDeclaredType(receiverType, index, new Dictionary<string, string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal));
        }

        string? GetIndexerTypeFromConstraints(string parameter, IndexExpression index, HashSet<string> visitedParameters,
            HashSet<string> visitedTypes)
        {
            if (!visitedParameters.Add(parameter) || !constraints.TryGetValue(parameter, out var items)) return null;
            foreach (var item in items)
            {
                if (item is "class" or "struct" or "enum" || item.StartsWith("new(", StringComparison.Ordinal)) continue;
                if (typeParameters.Contains(item))
                {
                    var nested = GetIndexerTypeFromConstraints(item, index, visitedParameters, visitedTypes);
                    if (nested is not null) return nested;
                }
                else
                {
                    var resultType = GetIndexerTypeFromDeclaredType(item, index, new Dictionary<string, string>(StringComparer.Ordinal), visitedTypes);
                    if (resultType is not null) return resultType;
                }
            }
            return null;
        }

        string? GetIndexerTypeFromDeclaredType(string declaredTypeName, IndexExpression index,
            IReadOnlyDictionary<string, string> inheritedSubstitutions, HashSet<string> visitedTypes)
        {
            var baseName = declaredTypeName;
            var substitutions = inheritedSubstitutions.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            var genericUse = EnumerateGenericUses(declaredTypeName).FirstOrDefault();
            if (genericUse is not null)
            {
                baseName = genericUse.Name;
                var definition = ResolveDefinition(baseName, owner, sourceTypes);
                if (definition?.TypeParameters is { } parameters && parameters.Count == genericUse.Arguments.Count)
                    foreach (var pair in parameters.Zip(genericUse.Arguments)) substitutions[pair.First] = RewriteTypeName(pair.Second, inheritedSubstitutions);
            }
            if (!visitedTypes.Add(baseName)) return null;
            var declaredType = ResolveDefinition(baseName, owner, sourceTypes);
            if (declaredType is null) return null;
            var syntheticCall = new CallExpression(new NameExpression("$constraint-indexer", index.Location), index.Indices, index.Location);
            var resultTypes = declaredType.Members.OfType<ComputedIndexerDeclaration>()
                .Where(candidate => candidate.GetterBody is not null &&
                    ConstraintAccessorAccessible(declaredType, candidate.Accessibility, candidate.GetterAccessibility) &&
                    ConstraintCallMatches(syntheticCall, new MethodDeclaration(owner.Name, "$constraint-indexer", candidate.TypeName,
                        candidate.Parameters, null, false, true, false, false, candidate.Location), substitutions))
                .Select(candidate => RewriteTypeName(candidate.TypeName, substitutions)).Distinct(StringComparer.Ordinal).ToArray();
            if (resultTypes.Length == 1) return resultTypes[0];
            if (resultTypes.Length > 1) return null;
            foreach (var baseType in declaredType.BaseTypes ?? [])
            {
                var inherited = GetIndexerTypeFromDeclaredType(RewriteTypeName(baseType, substitutions), index,
                    substitutions, visitedTypes);
                if (inherited is not null) return inherited;
            }
            return null;
        }

        bool ConstraintArgumentMayConvert(string from, string to)
        {
            if (from == to) return true;
            if (from == "null") return to.EndsWith("?", StringComparison.Ordinal);
            if (to.EndsWith("?", StringComparison.Ordinal))
                return ConstraintArgumentMayConvert(from.EndsWith("?", StringComparison.Ordinal) ? from[..^1] : from, to[..^1]);
            if (from.EndsWith("?", StringComparison.Ordinal)) return false;
            if ((to is "object" or "System.Object") && IsPotentialReferenceType(from)) return true;
            if (to == "System.Array" && SemanticAnalyzer.TryParseManagedArrayType(from, out _, out _)) return true;
            return IsImplicitNumericConversionForTarget(from, to, _nativePointerSize) ||
                IsAssignableConstraintType(from, to, new HashSet<string>(StringComparer.Ordinal));
        }

        bool IsAssignableConstraintType(string from, string to, HashSet<string> visited)
        {
            if (from == to) return true;
            if (!visited.Add(from)) return false;
            var definition = ResolveDefinition(from, owner, sourceTypes);
            if (definition is null) return false;
            foreach (var baseType in definition.BaseTypes ?? [])
            {
                if (baseType == to || IsAssignableConstraintType(baseType, to, visited)) return true;
            }
            return false;
        }

        static bool IsNumericConstraintType(string typeName) => typeName is "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or
            "int32" or "uint32" or "int64" or "uint64" or "char" or "float" or "double";

        static bool IsFloatingPointLiteral(string value) =>
            !value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("0b", StringComparison.OrdinalIgnoreCase) &&
            (value.Contains('.') || value.Contains('e') || value.Contains('E'));

        static bool IsConstraintMemberAccessible(string accessibility) =>
            accessibility.Contains("public", StringComparison.Ordinal) || accessibility.Contains("internal", StringComparison.Ordinal);
    }

    private static IEnumerable<object> EnumerateNodes(object root)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var nodes = new List<object>();
        Visit(root);
        return nodes;

        void Visit(object? value)
        {
            if (value is null || value is string || value is SourceLocation) return;
            if (value is IEnumerable sequence)
            {
                foreach (var element in sequence) Visit(element);
                return;
            }
            var runtimeType = value.GetType();
            if (runtimeType.Namespace != typeof(TypeDeclaration).Namespace || !runtimeType.IsClass || !visited.Add(value)) return;
            nodes.Add(value);
            foreach (var property in runtimeType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                if (property.CanRead && property.GetIndexParameters().Length == 0 && property.Name is not ("Location" or "Usings"))
                    Visit(property.GetValue(value));
        }
    }

    private static IEnumerable<(string TypeText, SourceLocation Location)> EnumerateTypeStrings(TypeDeclaration root)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var results = new List<(string, SourceLocation)>();
        Visit(root, root.Location);
        return results;

        void Visit(object? value, SourceLocation inheritedLocation, string? propertyName = null, IReadOnlySet<string>? excludedTypeParameters = null)
        {
            if (value is null) return;
            if (value is string text)
            {
                if (propertyName is not null && TypeStringProperties.Contains(propertyName) &&
                    (excludedTypeParameters is null || !ReferencesAnyTypeParameter(text, excludedTypeParameters)))
                    results.Add((text, inheritedLocation));
                return;
            }
            if (value is SourceLocation) return;
            if (value is IEnumerable sequence)
            {
                foreach (var element in sequence) Visit(element, inheritedLocation, propertyName, excludedTypeParameters);
                return;
            }
            var runtimeType = value.GetType();
            if (runtimeType.Namespace != typeof(TypeDeclaration).Namespace || !runtimeType.IsClass || !visited.Add(value)) return;
            var location = runtimeType.GetProperty("Location", BindingFlags.Instance | BindingFlags.Public)?.GetValue(value) as SourceLocation
                ?? inheritedLocation;
            if (value is MethodDeclaration { HasTypeParameters: true, TypeParameters: { Count: > 0 } methodTypeParameters })
                excludedTypeParameters = (excludedTypeParameters ?? new HashSet<string>(StringComparer.Ordinal))
                    .Concat(methodTypeParameters).ToHashSet(StringComparer.Ordinal);
            foreach (var property in runtimeType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0) continue;
                if (property.Name is "Location" or "Usings" or "Namespace" or "ContainingType" or "TypeParameters") continue;
                Visit(property.GetValue(value), location, property.Name, excludedTypeParameters);
            }
        }
    }

    internal static bool ReferencesAnyTypeParameter(string typeName, IReadOnlySet<string> typeParameters) => typeParameters.Any(parameter =>
        TypeNameSyntax.TryParse(typeName, out var parsed)
            ? parsed.DescendantsAndSelf().Any(node => node.Name == parameter)
            : Regex.IsMatch(typeName, $"(?<![A-Za-z0-9_$.]){Regex.Escape(parameter)}(?![A-Za-z0-9_])"));

    private static IEnumerable<GenericUse> EnumerateGenericUses(string text)
    {
        if (TypeNameSyntax.TryParse(text, out var parsed))
        {
            foreach (var node in parsed.DescendantsAndSelf())
            {
                if (node.Arguments.Count > 0)
                    yield return new(node.Name, node.Arguments.Select(argument => argument.ToString()).ToArray());
            }
            yield break;
        }

        // Some legacy declaration forms carry non-type annotations in the same
        // string field. Keep the tolerant scanner for those until those forms
        // move to dedicated syntax nodes.
        for (var open = 0; open < text.Length; open++)
        {
            if (text[open] != '<') continue;
            var end = FindMatchingAngle(text, open);
            if (end < 0) continue;
            var nameStart = open - 1;
            while (nameStart >= 0 && (char.IsLetterOrDigit(text[nameStart]) || text[nameStart] is '_' or '.' or '$')) nameStart--;
            nameStart++;
            if (nameStart == open || !char.IsLetter(text[nameStart]) && text[nameStart] != '_') continue;
            var name = text[nameStart..open];
            var argumentText = text[(open + 1)..end];
            var arguments = SplitArguments(argumentText);
            if (arguments.Count > 0 && arguments.All(argument => argument.Length > 0))
                yield return new(name, arguments);
        }
    }

    private static int GetGenericArgumentDepth(string typeName)
    {
        var depth = 0;
        var maximum = 0;
        foreach (var character in typeName)
        {
            if (character == '<') maximum = Math.Max(maximum, ++depth);
            else if (character == '>') depth = Math.Max(0, depth - 1);
        }
        return maximum;
    }

    private static bool IsExpansiveRecursiveInstantiation(IReadOnlyList<string> currentArguments, IReadOnlyList<string> nextArguments)
    {
        if (currentArguments.Count != nextArguments.Count ||
            nextArguments.Sum(argument => argument.Length) <= currentArguments.Sum(argument => argument.Length)) return false;
        return currentArguments.All(currentArgument => nextArguments.Any(nextArgument =>
            nextArgument.Equals(currentArgument, StringComparison.Ordinal) ||
            nextArgument.Contains("<" + currentArgument, StringComparison.Ordinal) ||
            nextArgument.Contains("," + currentArgument, StringComparison.Ordinal)));
    }

    private static int FindMatchingAngle(string text, int open)
    {
        var depth = 0;
        for (var index = open; index < text.Length; index++)
        {
            if (text[index] == '<') depth++;
            else if (text[index] == '>' && --depth == 0) return index;
        }
        return -1;
    }

    private static IReadOnlyList<string> SplitArguments(string text)
    {
        var result = new List<string>();
        var depth = 0;
        var start = 0;
        for (var index = 0; index < text.Length; index++)
        {
            if (text[index] == '<') depth++;
            else if (text[index] == '>') depth--;
            else if (text[index] == ',' && depth == 0)
            {
                result.Add(text[start..index].Trim());
                start = index + 1;
            }
        }
        result.Add(text[start..].Trim());
        return result;
    }

    internal static TypeDeclaration? ResolveDefinition(string requestedName, TypeDeclaration context, IReadOnlyList<TypeDeclaration> definitions)
    {
        var name = requestedName;
        foreach (var alias in context.Usings?.Where(directive => directive.Alias is not null) ?? [])
        {
            var aliasName = alias.Alias!;
            if (name == aliasName) { name = alias.Target; break; }
            if (name.StartsWith(aliasName + ".", StringComparison.Ordinal))
            { name = alias.Target + name[aliasName.Length..]; break; }
        }

        IEnumerable<TypeDeclaration> candidates;
        if (name.Contains('.', StringComparison.Ordinal))
        {
            candidates = definitions.Where(type => QualifiedName(type) == name);
            if (!candidates.Any()) candidates = definitions.Where(type => QualifiedName(type) == Join(context.Namespace, name));
            if (!candidates.Any())
                candidates = context.Usings?.Where(directive => directive.Alias is null)
                    .SelectMany(directive => definitions.Where(type => QualifiedName(type) == Join(directive.Target, name))) ?? [];
        }
        else
        {
            candidates = definitions.Where(type => type.Name == name && type.Namespace == context.Namespace);
            if (!candidates.Any()) candidates = definitions.Where(type => type.Name == name && type.Namespace == QualifiedName(context));
            if (!candidates.Any())
                candidates = context.Usings?.Where(directive => directive.Alias is null)
                    .SelectMany(directive => definitions.Where(type => type.Name == name && type.Namespace == directive.Target)) ?? [];
            if (!candidates.Any()) candidates = definitions.Where(type => type.Name == name && type.Namespace.Length == 0);
        }
        var matches = candidates.GroupBy(QualifiedName, StringComparer.Ordinal).Select(group => group.First()).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static string MakeKey(TypeDeclaration definition, IReadOnlyList<string> arguments) =>
        QualifiedName(definition) + "<" + string.Join(",", arguments) + ">";

    private static string GetSpecializedName(TypeDeclaration definition, IReadOnlyList<string> arguments)
    {
        var payload = Encoding.UTF8.GetBytes(string.Join("\u001f", arguments));
        var digest = Convert.ToHexString(SHA256.HashData(payload).AsSpan(0, 10)).ToLowerInvariant();
        return definition.Name + "__g_" + digest;
    }

    private static TypeDeclaration RewriteType(TypeDeclaration definition, string specializedName, IReadOnlyDictionary<string, string> substitutions)
    {
        var members = definition.Members.Select(member => RewriteMember(member, specializedName, substitutions)).ToArray();
        return definition with
        {
            Name = specializedName,
            HasTypeParameters = false,
            TypeParameters = null,
            GenericConstraints = RewriteConstraints(definition.GenericConstraints, substitutions),
            Members = members,
            BaseTypes = definition.BaseTypes?.Select(type => RewriteTypeName(type, substitutions)).ToArray(),
            DelegateReturnType = definition.DelegateReturnType is null ? null : RewriteTypeName(definition.DelegateReturnType, substitutions),
            DelegateParameters = definition.DelegateParameters?.Select(parameter => RewriteParameter(parameter, substitutions)).ToArray()
        };
    }

    internal static MemberDeclaration RewriteMember(MemberDeclaration member, string ownerName, IReadOnlyDictionary<string, string> substitutions) => member switch
    {
        FieldDeclaration field => field with { TypeName = RewriteTypeName(field.TypeName, substitutions), Initializer = RewriteOptional(field.Initializer, substitutions) },
        ConstantFieldDeclaration field => field with { TypeName = RewriteTypeName(field.TypeName, substitutions), Initializer = RewriteExpression(field.Initializer, substitutions) },
        MethodDeclaration method => method with
        {
            ContainingType = ownerName,
            ReturnType = RewriteTypeName(method.ReturnType, substitutions),
            Parameters = method.Parameters.Select(parameter => RewriteParameter(parameter, substitutions)).ToArray(),
            Body = method.Body is null ? null : (BlockStatement)RewriteStatement(method.Body, substitutions, method.ReturnType),
            ExplicitInterfaceName = RewriteOptionalType(method.ExplicitInterfaceName, substitutions),
            GenericConstraints = RewriteConstraints(method.GenericConstraints, substitutions)
        },
        ConstructorDeclaration constructor => constructor with
        {
            ContainingType = ownerName,
            Parameters = constructor.Parameters.Select(parameter => RewriteParameter(parameter, substitutions)).ToArray(),
            Body = RewriteOptional(constructor.Body, substitutions),
            InitializerArguments = constructor.InitializerArguments?.Select(expression => RewriteExpression(expression, substitutions)).ToArray()
        },
        FinalizerDeclaration finalizer => finalizer with { Body = RewriteOptional(finalizer.Body, substitutions) },
        ComputedPropertyDeclaration property => property with
        {
            TypeName = RewriteTypeName(property.TypeName, substitutions),
            GetterBody = RewriteOptional(property.GetterBody, substitutions),
            SetterBody = RewriteOptional(property.SetterBody, substitutions),
            BackingInitializer = RewriteOptional(property.BackingInitializer, substitutions),
            ExplicitInterfaceName = RewriteOptionalType(property.ExplicitInterfaceName, substitutions)
        },
        ComputedIndexerDeclaration indexer => indexer with
        {
            TypeName = RewriteTypeName(indexer.TypeName, substitutions),
            Parameters = indexer.Parameters.Select(parameter => RewriteParameter(parameter, substitutions)).ToArray(),
            GetterBody = RewriteOptional(indexer.GetterBody, substitutions),
            SetterBody = RewriteOptional(indexer.SetterBody, substitutions),
            ExplicitInterfaceName = RewriteOptionalType(indexer.ExplicitInterfaceName, substitutions)
        },
        _ => member
    };

    internal static Expression CloneExpressionForAnalysis(Expression expression) =>
        RewriteExpression(expression, new Dictionary<string, string>(StringComparer.Ordinal));

    private static ParameterDeclaration RewriteParameter(ParameterDeclaration parameter, IReadOnlyDictionary<string, string> substitutions) =>
        parameter with { TypeName = RewriteTypeName(parameter.TypeName, substitutions), DefaultValue = RewriteOptional(parameter.DefaultValue, substitutions) };

    private static IReadOnlyList<GenericConstraintDeclaration>? RewriteConstraints(IReadOnlyList<GenericConstraintDeclaration>? constraints, IReadOnlyDictionary<string, string> substitutions) =>
        constraints?.Select(constraint => constraint with { Constraints = constraint.Constraints.Select(type => RewriteTypeName(type, substitutions)).ToArray() }).ToArray();

    internal static string RewriteTypeName(string typeName, IReadOnlyDictionary<string, string> substitutions)
    {
        if (TypeNameSyntax.TryParse(typeName, out var parsed))
            return parsed.Rewrite(substitutions).ToString();

        // Constraints such as "new(...)" are not type names. Preserve their
        // established rewriting behavior while ordinary types use syntax-aware
        // identifier substitution above.
        foreach (var (parameter, argument) in substitutions)
            typeName = Regex.Replace(typeName, $"(?<![A-Za-z0-9_$.]){Regex.Escape(parameter)}(?![A-Za-z0-9_])", _ => argument);
        return typeName;
    }

    private static string? RewriteOptionalType(string? typeName, IReadOnlyDictionary<string, string> substitutions) =>
        typeName is null ? null : RewriteTypeName(typeName, substitutions);

    private static T? RewriteOptional<T>(T? node, IReadOnlyDictionary<string, string> substitutions) where T : class => node switch
    {
        Statement statement => (T)(object)RewriteStatement(statement, substitutions),
        Expression expression => (T)(object)RewriteExpression(expression, substitutions),
        _ => node
    };

    private static Statement RewriteStatement(Statement statement, IReadOnlyDictionary<string, string> substitutions, string? returnType = null) => statement switch
    {
        BlockStatement block => block with { Statements = block.Statements.Select(child => RewriteStatement(child, substitutions, returnType)).ToArray() },
        ReturnStatement ret => ret with { Value = ret.Value is DefaultValueExpression { TypeName: null } defaultValue && returnType is not null && substitutions.ContainsKey(returnType)
            ? defaultValue with { IsTypeParameterDefault = true }
            : RewriteOptional(ret.Value, substitutions) },
        VariableStatement variable => variable with
        {
            TypeName = RewriteOptionalType(variable.TypeName, substitutions),
            Initializer = variable.Initializer is DefaultValueExpression { TypeName: null } defaultValue && variable.TypeName is not null && substitutions.ContainsKey(variable.TypeName)
                ? defaultValue with { IsTypeParameterDefault = true }
                : RewriteOptional(variable.Initializer, substitutions)
        },
        VariableDeclarationStatement declaration => declaration with { Variables = declaration.Variables.Select(variable => (VariableStatement)RewriteStatement(variable, substitutions, returnType)).ToArray() },
        MultiAssignmentStatement assignment => assignment with { Targets = assignment.Targets.Select(expression => RewriteExpression(expression, substitutions)).ToArray(), Values = assignment.Values.Select(expression => RewriteExpression(expression, substitutions)).ToArray() },
        ExpressionStatement expression => expression with { Value = RewriteExpression(expression.Value, substitutions) },
        IfStatement conditional => conditional with { Condition = RewriteExpression(conditional.Condition, substitutions), Then = RewriteStatement(conditional.Then, substitutions, returnType), Else = RewriteOptional(conditional.Else, substitutions) },
        WhileStatement loop => loop with { Condition = RewriteExpression(loop.Condition, substitutions), Body = RewriteStatement(loop.Body, substitutions, returnType) },
        DoWhileStatement loop => loop with { Body = RewriteStatement(loop.Body, substitutions, returnType), Condition = RewriteExpression(loop.Condition, substitutions) },
        ForStatement loop => loop with { Initializer = RewriteOptional(loop.Initializer, substitutions), Condition = RewriteOptional(loop.Condition, substitutions), Iterators = loop.Iterators.Select(expression => RewriteExpression(expression, substitutions)).ToArray(), Body = RewriteStatement(loop.Body, substitutions, returnType) },
        ForInStatement loop => loop with { TypeName = RewriteOptionalType(loop.TypeName, substitutions), Collection = RewriteExpression(loop.Collection, substitutions), Body = RewriteStatement(loop.Body, substitutions) },
        SwitchStatement selection => selection with
        {
            Value = RewriteExpression(selection.Value, substitutions),
            Sections = selection.Sections.Select(section =>
            {
                var statements = section.Statements.Select(child => RewriteStatement(child, substitutions)).ToArray();
                return section with
                {
                    Labels = section.Labels.Select(label => RewriteSwitchLabel(label, substitutions)).ToArray(),
                    Statements = statements,
                    CaptureScope = new BlockStatement(statements, section.At)
                };
            }).ToArray()
        },
        TryStatement attempt => attempt with
        {
            Body = (BlockStatement)RewriteStatement(attempt.Body, substitutions),
            Catches = attempt.Catches.Select(clause => clause with { TypeName = RewriteOptionalType(clause.TypeName, substitutions), Body = (BlockStatement)RewriteStatement(clause.Body, substitutions) }).ToArray(),
            Finally = RewriteOptional(attempt.Finally, substitutions)
        },
        ThrowStatement thrown => thrown with { Value = RewriteOptional(thrown.Value, substitutions) },
        OverflowContextStatement overflow => overflow with { Body = RewriteStatement(overflow.Body, substitutions) },
        _ => statement
    };

    private static SwitchLabel RewriteSwitchLabel(SwitchLabel label, IReadOnlyDictionary<string, string> substitutions) => label with
    {
        Value = RewriteOptional(label.Value, substitutions),
        Guard = RewriteOptional(label.Guard, substitutions),
        TypePatternName = RewriteOptionalType(label.TypePatternName, substitutions),
        LeftPattern = label.LeftPattern is null ? null : RewriteSwitchLabel(label.LeftPattern, substitutions),
        RightPattern = label.RightPattern is null ? null : RewriteSwitchLabel(label.RightPattern, substitutions)
    };

    private static Expression RewriteExpression(Expression expression, IReadOnlyDictionary<string, string> substitutions) => expression switch
    {
        // Each closed generic type gets its own bound AST. Semantic bindings are stored by
        // syntax-node identity, so even leaves which have no type name to rewrite must be
        // cloned. Reusing NameExpression nodes across specializations lets the last analyzed
        // instantiation overwrite the binding used by every earlier one.
        LiteralExpression literal => literal with { },
        NameExpression name => name with { },
        ThisExpression @this => @this with { },
        BaseExpression @base => @base with { },
        UnsupportedExpression unsupported => unsupported with { },
        InlineArrayInitializerExpression initializer => initializer with { Elements = initializer.Elements.Select(element => RewriteExpression(element, substitutions)).ToArray() },
        AllocationExpression allocation => allocation with
        {
            TypeName = RewriteOptionalType(allocation.TypeName, substitutions),
            Dimensions = allocation.Dimensions.Select(dimension => RewriteExpression(dimension, substitutions)).ToArray(),
            Initializer = RewriteOptional(allocation.Initializer, substitutions),
            ConstructorArguments = allocation.ConstructorArguments?.Select(argument => RewriteExpression(argument, substitutions)).ToArray(),
            IsTypeParameterConstruction = allocation.IsTypeParameterConstruction || allocation.TypeName is not null &&
                substitutions.ContainsKey(allocation.TypeName)
        },
        UnaryExpression unary => unary with { Operand = RewriteExpression(unary.Operand, substitutions) },
        CastExpression cast => cast with { TypeName = RewriteTypeName(cast.TypeName, substitutions), Operand = RewriteExpression(cast.Operand, substitutions) },
        BinaryExpression binary => binary with { Left = RewriteExpression(binary.Left, substitutions), Right = RewriteExpression(binary.Right, substitutions) },
        ConditionalExpression conditional => conditional with { Condition = RewriteExpression(conditional.Condition, substitutions), WhenTrue = RewriteExpression(conditional.WhenTrue, substitutions), WhenFalse = RewriteExpression(conditional.WhenFalse, substitutions) },
        MemberAccessExpression access => access with { Receiver = RewriteExpression(access.Receiver, substitutions) },
        IndexExpression index => index with { Receiver = RewriteExpression(index.Receiver, substitutions), Indices = index.Indices.Select(item => RewriteExpression(item, substitutions)).ToArray() },
        CallExpression call => call with
        {
            Target = RewriteExpression(call.Target, substitutions),
            Arguments = call.Arguments.Select(argument => RewriteExpression(argument, substitutions)).ToArray(),
            TypeArguments = call.TypeArguments?.Select(type => RewriteTypeName(type, substitutions)).ToArray()
        },
        LambdaExpression lambda => lambda with
        {
            Parameters = lambda.Parameters.Select(parameter => parameter with { TypeName = RewriteOptionalType(parameter.TypeName, substitutions) }).ToArray(),
            ExpressionBody = RewriteOptional(lambda.ExpressionBody, substitutions),
            BlockBody = RewriteOptional(lambda.BlockBody, substitutions)
        },
        InlineOutVariableExpression inline => inline with { TypeName = RewriteOptionalType(inline.TypeName, substitutions) },
        OverflowContextExpression overflow => overflow with { Value = RewriteExpression(overflow.Value, substitutions) },
        DefaultValueExpression defaultValue => defaultValue with
        {
            TypeName = RewriteOptionalType(defaultValue.TypeName, substitutions),
            IsTypeParameterDefault = defaultValue.IsTypeParameterDefault || defaultValue.TypeName is not null &&
                substitutions.ContainsKey(defaultValue.TypeName)
        },
        SizeOfExpression sizeOf => sizeOf with { TypeName = RewriteTypeName(sizeOf.TypeName, substitutions) },
        NullAssertionExpression assertion => assertion with { Operand = RewriteExpression(assertion.Operand, substitutions) },
        _ => expression
    };

    private static string QualifiedName(TypeDeclaration type) => Join(type.Namespace, type.Name);
    private static string Join(string left, string right) => string.IsNullOrEmpty(left) ? right : string.IsNullOrEmpty(right) ? left : left + "." + right;
    private sealed record GenericUse(string Name, IReadOnlyList<string> Arguments);
}
