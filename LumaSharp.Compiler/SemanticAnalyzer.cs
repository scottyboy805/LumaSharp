using System.Numerics;

namespace LumaSharp.Compiler;

internal sealed record MethodSymbol(TypeDeclaration Owner, MethodDeclaration Declaration, bool IsStructConstructor = false, bool IsClassConstructor = false, bool IsClassFinalizer = false,
    ConstructorDeclaration? SourceConstructor = null, bool IsBuiltinObjectToString = false, bool IsGenericMethodSpecialization = false,
    bool IsTypeInitializer = false, MethodDeclaration? GenericTemplate = null)
{
    public string QualifiedName => (string.IsNullOrEmpty(Owner.Namespace) ? Owner.Name : Owner.Namespace + "." + Owner.Name) + "." + Declaration.Name;
    public bool IsStructInstanceMethod => Owner.Kind == "struct" && !Declaration.IsStatic && !IsStructConstructor;
    public bool IsClassInstanceMethod => Owner.Kind == "class" && !Declaration.IsStatic && !IsClassConstructor && !IsClassFinalizer;
    public bool IsStructEqualityOperator => Owner.Kind == "struct" && Declaration.IsOperator && Declaration.Operator == "==";
}
internal sealed record ConstantValue(string TypeName, BigInteger? Integer = null, double? Floating = null, bool? Boolean = null, string? Text = null);
internal sealed record ConstantFieldBinding(string TypeName, string Value, string? StringValue = null);
internal sealed record StaticFieldSymbol(TypeDeclaration Owner, FieldDeclaration Declaration, string GlobalName, string InitialValue,
    bool RequiresInitialization = false);
internal sealed record StaticFieldBinding(string TypeName, string GlobalName, bool Mutable, string OwnerType);
internal sealed record ClassToStringBinding(string TypeName, bool CallsBaseImplementation = false);
internal sealed record ComputedPropertySymbol(TypeDeclaration Owner, ComputedPropertyDeclaration Declaration, string TypeName, MethodSymbol? Getter, MethodSymbol? Setter);
internal sealed record ComputedIndexerSymbol(TypeDeclaration Owner, ComputedIndexerDeclaration Declaration, string TypeName, MethodSymbol? Getter, MethodSymbol? Setter);
internal sealed record StructFieldSymbol(TypeDeclaration Owner, FieldDeclaration Declaration, string TypeName, int Index);
internal sealed record StructFieldBinding(string OwnerType, string TypeName, int Index, bool IsReadOnly, bool ReceiverIsPointer = false,
    string? DeclaredOwnerType = null, bool IsScopeBound = false);
internal sealed record ClassTypeSymbol(TypeDeclaration Declaration, IReadOnlyList<StructFieldSymbol> Fields, string? BaseTypeName = null);
internal sealed record InterfaceMethodSlot(int Index, TypeDeclaration InterfaceType, MethodSymbol Contract);
internal sealed record ClassAllocationBinding(string TypeName, CallExpression ConstructorCall);
internal sealed record GenericConstructionBinding(string TypeName, CallExpression? ConstructorCall = null);
internal sealed record ConstructorInitializerBinding(CallExpression Call, bool DelegatesToThis);
internal sealed record ManagedArrayIndexBinding(string ElementType, int Rank);
internal sealed record DelegateMethodGroupBinding(string DelegateType, MethodSymbol Method, bool IsInstance = false, int? VirtualSlot = null,
    ClosureEnvironmentBinding? ClosureEnvironment = null);
internal sealed record ClosureEnvironmentBinding(string TypeName, IReadOnlyDictionary<string, int> CapturedFieldIndices,
    MethodSymbol EnclosingMethod, IReadOnlyDictionary<string, string>? ParentEnvironmentFields = null,
    IReadOnlyDictionary<string, string>? CapturedVariableEnvironments = null,
    IReadOnlySet<int>? BorrowedCaptureFieldIndices = null)
{
    public bool IsComposite => ParentEnvironmentFields is { Count: > 0 };
}
internal sealed record StructTypeSymbol(TypeDeclaration Declaration, IReadOnlyList<StructFieldSymbol> Fields, int SizeInBytes, int Alignment);
internal sealed record BoundCallArguments(IReadOnlyList<int> ParameterBySource, IReadOnlyList<Expression?> DefaultsByParameter,
    int VariadicParameterIndex = -1, bool UsesVariadicExpansion = false, IReadOnlyList<int>? VariadicSourceIndices = null);
internal sealed record CompilationAnalysis(
    IReadOnlyList<MethodSymbol> Methods,
    MethodSymbol? EntryPoint,
    IReadOnlyDictionary<VariableStatement, string> VariableTypes,
    IReadOnlyDictionary<ForInStatement, string> ForInTypes,
    IReadOnlyDictionary<ForInStatement, Statement> LoweredForInLoops,
    IReadOnlySet<VariableStatement> ScopeBoundVariables,
    IReadOnlyDictionary<CallExpression, MethodSymbol> CallTargets,
    IReadOnlyDictionary<CallExpression, string> EnumToStringCalls,
    IReadOnlyDictionary<CallExpression, ClassToStringBinding> ClassToStringCalls,
    IReadOnlyDictionary<CallExpression, string> StructToStringCalls,
    IReadOnlyDictionary<CallExpression, int> VirtualCallSlots,
    IReadOnlyDictionary<CallExpression, int> InterfaceCallSlots,
    IReadOnlyDictionary<string, TypeDeclaration> InterfaceTypes,
    IReadOnlyDictionary<string, TypeDeclaration> DelegateTypes,
    IReadOnlyDictionary<Expression, DelegateMethodGroupBinding> DelegateMethodGroups,
    IReadOnlyDictionary<MethodSymbol, ClosureEnvironmentBinding> ClosureEnvironments,
    IReadOnlyDictionary<ForStatement, ClosureEnvironmentBinding> ForHeaderClosureEnvironments,
    IReadOnlyDictionary<ForInStatement, ClosureEnvironmentBinding> ForInClosureEnvironments,
    IReadOnlyDictionary<BlockStatement, ClosureEnvironmentBinding> BlockClosureEnvironments,
    IReadOnlyDictionary<LambdaExpression, ClosureEnvironmentBinding> CompositeClosureEnvironments,
    IReadOnlyDictionary<CallExpression, TypeDeclaration> DelegateInvocations,
    IReadOnlyList<InterfaceMethodSlot> InterfaceMethodSlots,
    IReadOnlyDictionary<string, IReadOnlyList<MethodSymbol?>> InterfaceDispatchTables,
    IReadOnlySet<(string FromType, string ToType)> InterfaceConversions,
    IReadOnlyDictionary<DefaultValueExpression, string> DefaultTypes,
    IReadOnlyDictionary<MemberAccessExpression, ConstantFieldBinding> ConstantFields,
    IReadOnlyDictionary<NameExpression, ConstantFieldBinding> UnqualifiedConstantFields,
    IReadOnlyList<StaticFieldSymbol> StaticFields,
    IReadOnlyDictionary<MemberAccessExpression, StaticFieldBinding> FieldAccesses,
    IReadOnlyDictionary<MemberAccessExpression, CallExpression> PropertyGetterCalls,
    IReadOnlyDictionary<NameExpression, CallExpression> PropertyGetterNameCalls,
    IReadOnlyDictionary<BinaryExpression, CallExpression> PropertySetterCalls,
    IReadOnlyDictionary<MultiAssignmentStatement, IReadOnlyDictionary<int, BinaryExpression>> MultiAssignmentAccessorAssignments,
    IReadOnlyDictionary<IndexExpression, CallExpression> IndexerGetterCalls,
    IReadOnlyDictionary<NameExpression, StaticFieldBinding> UnqualifiedStaticFieldAccesses,
    IReadOnlyDictionary<string, StructTypeSymbol> StructTypes,
    IReadOnlyDictionary<string, ClassTypeSymbol> ClassTypes,
    IReadOnlyDictionary<MemberAccessExpression, StructFieldBinding> StructFieldAccesses,
    IReadOnlyDictionary<NameExpression, StructFieldBinding> UnqualifiedStructFields,
    IReadOnlyDictionary<MemberAccessExpression, StructFieldBinding> ClassFieldAccesses,
    IReadOnlyDictionary<NameExpression, StructFieldBinding> UnqualifiedClassFields,
    IReadOnlyDictionary<CallExpression, string> StructConstructors,
    IReadOnlyDictionary<AllocationExpression, ClassAllocationBinding> ClassAllocations,
    IReadOnlyDictionary<AllocationExpression, GenericConstructionBinding> GenericConstructions,
    IReadOnlyDictionary<Expression, MethodSymbol> OperatorTargets,
    IReadOnlyDictionary<(string SourceType, string TargetType), MethodSymbol> UserConversionOperators,
    IReadOnlyDictionary<string, string> EnumUnderlyingTypes,
    IReadOnlyDictionary<CastExpression, string> CastTypes,
    IReadOnlyDictionary<BinaryExpression, string> TypeOperatorTargets,
    IReadOnlyDictionary<SwitchLabel, string> SwitchTypePatterns,
    IReadOnlyDictionary<SizeOfExpression, string> SizeOfTypes,
    IReadOnlyDictionary<CallExpression, BoundCallArguments> CallArguments,
    IReadOnlyDictionary<MethodSymbol, ConstructorInitializerBinding> ConstructorInitializers,
    IReadOnlyDictionary<LiteralExpression, StringLiteralInfo> StringLiterals,
    IReadOnlyDictionary<MemberAccessExpression, bool> StringLengthAccesses,
    IReadOnlyDictionary<IndexExpression, bool> StringIndexAccesses,
    IReadOnlyDictionary<AllocationExpression, string> ManagedArrayAllocations,
    IReadOnlyDictionary<IndexExpression, ManagedArrayIndexBinding> ManagedArrayIndexAccesses,
    IReadOnlyDictionary<MemberAccessExpression, bool> ManagedArrayLengthAccesses,
    IReadOnlyDictionary<MemberAccessExpression, int> ManagedArrayRankAccesses,
    IReadOnlyDictionary<MemberAccessExpression, string> ManagedArrayPointerAccesses,
    IReadOnlyDictionary<MemberAccessExpression, string> GenericSpanEmptyAccesses,
    IReadOnlyDictionary<CallExpression, int> ManagedArrayGetLengthCalls,
    IReadOnlyDictionary<ForInStatement, string> ManagedArrayForInElementTypes,
    IReadOnlyDictionary<CatchClause, string> ExceptionCatchTypes,
    IReadOnlyDictionary<ThrowStatement, string> ThrowTypes,
    IReadOnlySet<string> ExceptionTypes,
    bool UsesExceptionHandling,
    IReadOnlyDictionary<string, StringLiteralInfo> StringLiteralPool,
    IReadOnlySet<string> StringifiedPrimitiveTypes,
    int NativePointerSize,
    string TargetTriple,
    IReadOnlyDictionary<ConditionalExpression, string> ConditionalExpressionTypes);

internal static class VirtualDispatchLayout
{
    private static readonly SourceLocation BuiltinLocation = new("<builtin>", 1, 1);
    private static readonly TypeDeclaration BuiltinObjectType = new("System", "class", "Object", [], BuiltinLocation, IsVirtual: true, Accessibility: "public");
    private static readonly TypeDeclaration BuiltinDelegateType = new("System", "class", "Delegate", [], BuiltinLocation,
        HasBaseList: true, BaseTypes: ["System.Object"], Accessibility: "public");
    private static readonly TypeDeclaration BuiltinArrayType = new("System", "class", "Array", [], BuiltinLocation,
        HasBaseList: true, BaseTypes: ["System.Object"], Accessibility: "public");
    private static readonly TypeDeclaration BuiltinValueType = new("System", "struct", "ValueType", [], BuiltinLocation,
        Accessibility: "public");
    public static TypeDeclaration ObjectType => BuiltinObjectType;
    public static TypeDeclaration DelegateType => BuiltinDelegateType;
    public static TypeDeclaration ArrayType => BuiltinArrayType;
    public static TypeDeclaration ValueType => BuiltinValueType;
    public static MethodSymbol ObjectToString { get; } = new(BuiltinObjectType,
        new MethodDeclaration("Object", "ToString", "string", [], null, false, true, false, false, BuiltinLocation,
            IsVirtual: true, Accessibility: "public"), IsBuiltinObjectToString: true);

    public static IReadOnlyList<MethodSymbol> GetMethods(string typeName,
        IReadOnlyDictionary<string, ClassTypeSymbol> classTypes, IReadOnlyList<MethodSymbol> methods)
    {
        var chain = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var current = typeName;
        while (classTypes.TryGetValue(current, out var type) && seen.Add(current))
        {
            chain.Add(current);
            if (type.BaseTypeName is null) break;
            current = type.BaseTypeName;
        }

        var slots = new List<MethodSymbol> { ObjectToString };
        var indices = new Dictionary<string, int>(StringComparer.Ordinal) { [Signature(ObjectToString)] = 0 };
        foreach (var owner in chain.AsEnumerable().Reverse())
        foreach (var method in methods.Where(candidate => candidate.IsClassInstanceMethod &&
                     QualifiedTypeName(candidate.Owner) == owner &&
                     (candidate.Declaration.IsVirtual || candidate.Declaration.IsOverride)))
        {
            var signature = Signature(method);
            if (indices.TryGetValue(signature, out var index)) slots[index] = method;
            else { indices.Add(signature, slots.Count); slots.Add(method); }
        }
        return slots;
    }

    public static int FindSlot(string typeName, MethodSymbol method,
        IReadOnlyDictionary<string, ClassTypeSymbol> classTypes, IReadOnlyList<MethodSymbol> methods)
    {
        var signature = Signature(method);
        var table = GetMethods(typeName, classTypes, methods);
        for (var index = 0; index < table.Count; index++)
            if (Signature(table[index]) == signature) return index;
        return -1;
    }

    public static string QualifiedTypeName(TypeDeclaration type) =>
        string.IsNullOrEmpty(type.Namespace) ? type.Name : type.Namespace + "." + type.Name;

    private static string Signature(MethodSymbol method) =>
        string.Join("\u001f", method.Declaration.Name,
            string.Join("\u001f", method.Declaration.Parameters.Select(parameter => parameter.TypeName)));
}

internal sealed class SemanticAnalyzer(CompilationOptions options, DiagnosticBag diagnostics, IReadOnlySet<string> runtimeSourcePaths, CancellationToken cancellationToken = default)
{
    private bool UnsafeEnabledAt(SourceLocation location) => options.UnsafeEnabled ||
        runtimeSourcePaths.Contains(Path.GetFullPath(location.File));

    private sealed class OutFlowLoop(string? label, int switchDepth)
    {
        public string? Label { get; } = label;
        public int SwitchDepth { get; } = switchDepth;
        public List<HashSet<string>> BreakStates { get; } = [];
        public List<HashSet<string>> ContinueStates { get; } = [];
    }

    private sealed class OutFlowSwitch(int depth)
    {
        public int Depth { get; } = depth;
        public List<HashSet<string>> BreakStates { get; } = [];
    }

    private sealed record OutFlowReturn(HashSet<string> Assigned, SourceLocation Location);
    private sealed record OutFlowExceptionState(string? ExceptionType, HashSet<string> Assigned);
    private enum OutFlowTransferKind { LoopBreak, LoopContinue, SwitchBreak }
    private sealed record OutFlowPendingTransfer(OutFlowTransferKind Kind, OutFlowLoop? Loop, OutFlowSwitch? Switch, HashSet<string> Assigned);

    private readonly record struct OutFlowTransferCounts(int[] LoopBreaks, int[] LoopContinues, int[] SwitchBreaks);

    private sealed record SwitchNumericInterval(ConstantValue? Lower, bool IncludeLower, ConstantValue? Upper, bool IncludeUpper);
    private static readonly string[] BuiltinExceptionNames = [
        "Exception", "InvalidOperationException", "IOException", "NullReferenceException",
        "InvalidCastException", "IndexOutOfRangeException", "OverflowException", "FormatException",
        "DivideByZeroException", "OutOfMemoryException", "TypeInitializationException"];
    private enum NullState { Null, NonNull, MaybeNull }
    private static readonly HashSet<string> IntegerTypes = ["int", "uint", "int8", "uint8", "int16", "uint16", "int32", "uint32", "int64", "uint64"];
    private static readonly HashSet<string> FloatingTypes = ["float", "double"];
    private static readonly HashSet<string> NumericTypes = [.. IntegerTypes, .. FloatingTypes, "char"];
    private static readonly HashSet<string> SupportedTypes = [.. NumericTypes, "bool", "char", "void", "string"];
    private IReadOnlyList<MethodSymbol> _methods = [];
    private List<MethodSymbol> _methodList = [];
    private int _nextLambdaMethodIndex;
    private int _nextClosureEnvironmentIndex;
    private readonly List<MethodSymbol> _genericMethodDefinitions = [];
    private readonly Dictionary<string, MethodSymbol> _genericMethodSpecializations = new(StringComparer.Ordinal);
    private readonly HashSet<string> _failedGenericMethodSpecializations = new(StringComparer.Ordinal);
    private IReadOnlyList<TypeDeclaration> _types = [];
    private IReadOnlyList<TypeDeclaration> _genericDefinitions = [];
    private readonly List<GenericTypeInstantiation> _genericTypeInstantiations = [];
    private GenericTypeSpecializer? _genericTypeSpecializer;
    private readonly HashSet<(string TypeName, string ContextName)> _genericTypeResolutionsInProgress = [];
    private readonly Dictionary<VariableStatement, string> _variableTypes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ForInStatement, string> _forInTypes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ForInStatement, Statement> _loweredForInLoops = new(ReferenceEqualityComparer.Instance);
    private int _forInIteratorCounter;
    private readonly HashSet<VariableStatement> _scopeBoundVariables = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<LambdaExpression, int> _scopeBoundLambdaDepths = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<LambdaExpression, int> _scopeBoundLambdaOrders = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<(string OwnerType, string FieldName), int> _scopeBoundClosureFields = new();
    private readonly HashSet<(string OwnerType, string FieldName)> _scopeBoundClassFields = [];
    private readonly HashSet<BinaryExpression> _scopeBoundLocalAssignments = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<BinaryExpression> _readOnlyLocalAssignments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MultiAssignmentStatement, HashSet<int>> _scopeBoundMultiAssignmentTargets = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MultiAssignmentStatement, HashSet<int>> _readOnlyMultiAssignmentTargets = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AllocationExpression, int> _scopeAllocationOrders = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, int> _scopeBoundClosureEnvironmentDepths = new(StringComparer.Ordinal);
    private readonly Dictionary<CallExpression, MethodSymbol> _callTargets = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, string> _enumToStringCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, ClassToStringBinding> _classToStringCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, string> _structToStringCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, int> _virtualCallSlots = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, int> _interfaceCallSlots = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Expression, DelegateMethodGroupBinding> _delegateMethodGroups = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MethodSymbol, ClosureEnvironmentBinding> _closureEnvironments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ForStatement, ClosureEnvironmentBinding> _forHeaderClosureEnvironments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ForInStatement, ClosureEnvironmentBinding> _forInClosureEnvironments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<BlockStatement, ClosureEnvironmentBinding> _blockClosureEnvironments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<LambdaExpression, ClosureEnvironmentBinding> _compositeClosureEnvironments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, MethodSymbol> _closureEnclosingMethods = new(StringComparer.Ordinal);
    private readonly Dictionary<CallExpression, TypeDeclaration> _delegateInvocations = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, BoundCallArguments> _callArguments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MethodSymbol, ConstructorInitializerBinding> _constructorInitializers = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<LiteralExpression, StringLiteralInfo> _stringLiterals = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, bool> _stringLengthAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IndexExpression, bool> _stringIndexAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AllocationExpression, string> _managedArrayAllocations = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IndexExpression, ManagedArrayIndexBinding> _managedArrayIndexAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, bool> _managedArrayLengthAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, int> _managedArrayRankAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, string> _managedArrayPointerAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, string> _genericSpanEmptyAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, int> _managedArrayGetLengthCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ForInStatement, string> _managedArrayForInElementTypes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CatchClause, string> _exceptionCatchTypes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ThrowStatement, string> _throwTypes = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<string> _exceptionTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, StringLiteralInfo> _stringLiteralPool = new(StringComparer.Ordinal);
    private readonly HashSet<string> _stringifiedPrimitiveTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<DefaultValueExpression, string> _defaultTypes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, (TypeDeclaration Owner, ConstantFieldBinding Binding, string Accessibility)> _constantFields = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (TypeDeclaration Owner, ConstantFieldDeclaration Declaration)> _pendingConstantFields = new(StringComparer.Ordinal);
    private readonly HashSet<string> _constantFieldsBeingEvaluated = new(StringComparer.Ordinal);
    private readonly HashSet<string> _constantFieldsEvaluated = new(StringComparer.Ordinal);
    private readonly HashSet<ConstantFieldDeclaration> _constantFieldDeclarationsVisited = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, ConstantFieldBinding> _fieldAccesses = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<NameExpression, ConstantFieldBinding> _boundUnqualifiedConstantFields = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, (TypeDeclaration Owner, StaticFieldSymbol Symbol, string Accessibility)> _staticFields = new(StringComparer.Ordinal);
    private readonly Dictionary<MemberAccessExpression, StaticFieldBinding> _boundStaticFields = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<NameExpression, StaticFieldBinding> _boundUnqualifiedStaticFields = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, ComputedPropertySymbol> _computedProperties = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<ComputedIndexerSymbol>> _computedIndexers = new(StringComparer.Ordinal);
    private readonly Dictionary<MemberAccessExpression, ComputedPropertySymbol> _boundComputedProperties = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, CallExpression> _propertyGetterCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<NameExpression, CallExpression> _propertyGetterNameCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<NameExpression, ComputedPropertySymbol> _boundUnqualifiedComputedProperties = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<BinaryExpression, CallExpression> _propertySetterCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MultiAssignmentStatement, Dictionary<int, BinaryExpression>> _multiAssignmentAccessorAssignments = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IndexExpression, CallExpression> _indexerGetterCalls = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IndexExpression, ComputedIndexerSymbol> _boundComputedIndexers = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, StructTypeSymbol> _structTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ClassTypeSymbol> _classTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TypeDeclaration> _interfaceTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TypeDeclaration> _delegateTypes = new(StringComparer.Ordinal);
    private IReadOnlyList<InterfaceMethodSlot> _interfaceMethodSlots = [];
    private IReadOnlySet<(string FromType, string ToType)> _interfaceConversions = new HashSet<(string, string)>();
    private IReadOnlyDictionary<string, IReadOnlyList<MethodSymbol?>> _interfaceDispatchTables =
        new Dictionary<string, IReadOnlyList<MethodSymbol?>>(StringComparer.Ordinal);
    private readonly Dictionary<MemberAccessExpression, StructFieldBinding> _boundStructFields = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<NameExpression, StructFieldBinding> _boundUnqualifiedStructFields = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<MemberAccessExpression, StructFieldBinding> _boundClassFields = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<NameExpression, StructFieldBinding> _boundUnqualifiedClassFields = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<CallExpression, string> _structConstructors = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AllocationExpression, ClassAllocationBinding> _classAllocations = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<AllocationExpression, GenericConstructionBinding> _genericConstructions = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<Expression, MethodSymbol> _operatorTargets = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<(string SourceType, string TargetType), MethodSymbol> _userConversionOperators = [];
    private readonly Dictionary<BinaryExpression, string> _typeOperatorTargets = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<ConditionalExpression, string> _conditionalExpressionTypes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<SwitchLabel, string> _switchTypePatterns = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<SwitchStatement> _exhaustiveSwitches = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, string> _enumUnderlyingTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<CastExpression, string> _castTypes = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<SizeOfExpression, string> _sizeOfTypes = new(ReferenceEqualityComparer.Instance);
    private readonly int _nativePointerSize = GetNativePointerSize(options.TargetTriple);
    private TypeDeclaration? _enumEvaluationOwner;
    private HashSet<string>? _currentDeclarationNames;
    private bool _checkedContext = true;
    private bool _isAnalyzingFieldInitializer;
    private Expression? _propertyAssignmentProbe;
    private int _catchDepth;
    private int _finallyDepth;
    private readonly Stack<(int LoopDepth, int SwitchDepth)> _finallyScopes = new();
    private readonly List<OutFlowLoop> _outFlowLoops = [];
    private readonly List<OutFlowSwitch> _outFlowSwitches = [];
    private readonly List<List<OutFlowReturn>> _outFlowReturnFrames = [];
    private readonly List<List<OutFlowExceptionState>> _outFlowExceptionFrames = [];
    private int _outFlowSwitchDepth;
    private int _outFlowSuppressReturnChecks;
    private bool _usesExceptionHandling;

    private static bool IsBuiltinExceptionName(string name) => BuiltinExceptionNames.Any(exception =>
        name == exception || name == "System." + exception || exception == "IOException" && name == "System.IO.IOException");

    private static string BuiltinExceptionNamespace(string name) => name == "IOException" ? "System.IO" : "System";

    private static IReadOnlyList<TypeDeclaration> CreateBuiltinExceptionTypes(IReadOnlyList<TypeDeclaration> sourceTypes)
    {
        var existing = sourceTypes.Select(GetQualifiedTypeName).ToHashSet(StringComparer.Ordinal);
        var location = new SourceLocation("<LumaSharp runtime>", 1, 1);
        return BuiltinExceptionNames
            .Where(name => !existing.Contains(BuiltinExceptionNamespace(name) + "." + name))
            .Select(name => new TypeDeclaration(BuiltinExceptionNamespace(name), "class", name,
                name == "Exception" ? CreateExceptionBaseMembers(location) : CreateBuiltinExceptionMembers(name, location), location,
                HasBaseList: name != "Exception",
                BaseTypes: name == "Exception" ? [] : ["System.Exception"], IsVirtual: true, Accessibility: "public"))
            .ToArray();
    }

    private static IReadOnlyList<MemberDeclaration> CreateExceptionBaseMembers(SourceLocation location)
    {
        return [
            new FieldDeclaration("string", "Message", null, IsStatic: false, IsPublic: true, IsReadOnly: true, location),
            new FieldDeclaration("System.Exception?", "InnerException", null, IsStatic: false, IsPublic: true, IsReadOnly: true, location),
            new ConstructorDeclaration("Exception", [], ExceptionConstructorBody(new LiteralExpression("\"\"", location), new LiteralExpression("null", location), location), IsStatic: false, IsPublic: true, location),
            new ConstructorDeclaration("Exception", [new ParameterDeclaration("string", "message", location)], ExceptionConstructorBody(new NameExpression("message", location), new LiteralExpression("null", location), location), IsStatic: false, IsPublic: true, location),
            new ConstructorDeclaration("Exception", [new ParameterDeclaration("string", "message", location), new ParameterDeclaration("System.Exception?", "innerException", location)], ExceptionConstructorBody(new NameExpression("message", location), new NameExpression("innerException", location), location), IsStatic: false, IsPublic: true, location)
        ];
    }

    private static IReadOnlyList<MemberDeclaration> CreateBuiltinExceptionMembers(string containingType, SourceLocation location) => [
        new ConstructorDeclaration(containingType, [], new BlockStatement([], location), IsStatic: false, IsPublic: true, location),
        new ConstructorDeclaration(containingType, [new ParameterDeclaration("string", "message", location)], ExceptionConstructorBody(new NameExpression("message", location), new LiteralExpression("null", location), location), IsStatic: false, IsPublic: true, location),
        new ConstructorDeclaration(containingType, [new ParameterDeclaration("string", "message", location), new ParameterDeclaration("System.Exception?", "innerException", location)], ExceptionConstructorBody(new NameExpression("message", location), new NameExpression("innerException", location), location), IsStatic: false, IsPublic: true, location)
    ];

    private static BlockStatement ExceptionConstructorBody(Expression message, Expression innerException, SourceLocation location)
    {
        var messageTarget = new MemberAccessExpression(new ThisExpression(location), "Message", location);
        var innerTarget = new MemberAccessExpression(new ThisExpression(location), "InnerException", location);
        var setMessage = new BinaryExpression(messageTarget, "=", message, location);
        var setInner = new BinaryExpression(innerTarget, "=", innerException, location);
        return new([new ExpressionStatement(setMessage, location), new ExpressionStatement(setInner, location)], location);
    }

    private static bool CompilationMayRaise(IReadOnlyList<TypeDeclaration> types)
    {
        foreach (var type in types)
        foreach (var member in type.Members)
        {
            var mayRaise = member switch
            {
                MethodDeclaration { Body: { } body } => StatementMayRaise(body),
                ConstructorDeclaration { Body: { } body } => StatementMayRaise(body),
                FinalizerDeclaration { Body: { } body } => StatementMayRaise(body),
                FieldDeclaration { Initializer: { } initializer } => ExpressionMayRaise(initializer),
                ConstantFieldDeclaration constant => ExpressionMayRaise(constant.Initializer),
                _ => false
            };
            if (mayRaise) return true;
        }
        return false;
    }

    private static bool StatementMayRaise(Statement statement) => statement switch
    {
        ThrowStatement or TryStatement => true,
        BlockStatement block => block.Statements.Any(StatementMayRaise),
        VariableStatement variable => variable.Initializer is not null && ExpressionMayRaise(variable.Initializer),
        VariableDeclarationStatement declaration => declaration.Variables.Any(StatementMayRaise),
        MultiAssignmentStatement assignment => assignment.Targets.Any(ExpressionMayRaise) || assignment.Values.Any(ExpressionMayRaise),
        ExpressionStatement expression => ExpressionMayRaise(expression.Value),
        ReturnStatement returned => returned.Value is not null && ExpressionMayRaise(returned.Value),
        IfStatement conditional => ExpressionMayRaise(conditional.Condition) || StatementMayRaise(conditional.Then) || conditional.Else is not null && StatementMayRaise(conditional.Else),
        WhileStatement loop => ExpressionMayRaise(loop.Condition) || StatementMayRaise(loop.Body),
        DoWhileStatement loop => StatementMayRaise(loop.Body) || ExpressionMayRaise(loop.Condition),
        ForStatement loop => loop.Initializer is not null && StatementMayRaise(loop.Initializer) || loop.Condition is not null && ExpressionMayRaise(loop.Condition) || loop.Iterators.Any(ExpressionMayRaise) || StatementMayRaise(loop.Body),
        ForInStatement loop => ExpressionMayRaise(loop.Collection) || StatementMayRaise(loop.Body),
        SwitchStatement selection => ExpressionMayRaise(selection.Value) || selection.Sections.Any(section => section.Labels.Any(label => SwitchPatternExpressions(label).Any(ExpressionMayRaise)) || section.Statements.Any(StatementMayRaise)),
        OverflowContextStatement overflow => StatementMayRaise(overflow.Body),
        _ => false
    };

    private static bool ExpressionMayRaise(Expression expression) => expression switch
    {
        // In checked context arithmetic and narrowing conversions raise OverflowException.
        // Division, allocation, indexing and explicit null assertions also have specified
        // run-time failures. The module shares one exception runtime, so a conservative
        // module-level scan is preferable to emitting unusable partial EH support.
        AllocationExpression => true,
        IndexExpression => true,
        NullAssertionExpression => true,
        CastExpression => true,
        BinaryExpression binary => binary.Operator is "+" or "-" or "*" or "/" or "%" or "+=" or "-=" or "*=" or "/=" or "%=" || ExpressionMayRaise(binary.Left) || ExpressionMayRaise(binary.Right),
        UnaryExpression unary => unary.Operator is "++" or "--" || ExpressionMayRaise(unary.Operand),
        CallExpression call => call.Target is MemberAccessExpression { Member: "GetLength" } || call.Arguments.Any(ExpressionMayRaise) || ExpressionMayRaise(call.Target),
        MemberAccessExpression member => ExpressionMayRaise(member.Receiver),
        ConditionalExpression conditional => ExpressionMayRaise(conditional.Condition) || ExpressionMayRaise(conditional.WhenTrue) || ExpressionMayRaise(conditional.WhenFalse),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(ExpressionMayRaise),
        OverflowContextExpression overflow => ExpressionMayRaise(overflow.Value),
        _ => false
    };

    private static bool TypeReferencesExceptionTypes(TypeDeclaration type)
    {
        if ((type.BaseTypes ?? []).Any(IsBuiltinExceptionName)) return true;
        foreach (var member in type.Members)
        {
            var referenced = member switch
            {
                FieldDeclaration field => IsBuiltinExceptionName(field.TypeName),
                ConstantFieldDeclaration field => IsBuiltinExceptionName(field.TypeName),
                MethodDeclaration method => IsBuiltinExceptionName(method.ReturnType) || method.Parameters.Any(parameter => IsBuiltinExceptionName(parameter.TypeName)) ||
                    method.Body is not null && StatementReferencesExceptionType(method.Body),
                ConstructorDeclaration constructor => constructor.Parameters.Any(parameter => IsBuiltinExceptionName(parameter.TypeName)) ||
                    constructor.Body is not null && StatementReferencesExceptionType(constructor.Body),
                FinalizerDeclaration { Body: { } body } => StatementReferencesExceptionType(body),
                _ => false
            };
            if (referenced) return true;
        }
        return false;
    }

    private static bool StatementReferencesExceptionType(Statement statement) => statement switch
    {
        ThrowStatement or TryStatement => true,
        BlockStatement block => block.Statements.Any(StatementReferencesExceptionType),
        VariableStatement variable => variable.TypeName is not null && IsBuiltinExceptionName(variable.TypeName) || variable.Initializer is not null && ExpressionReferencesExceptionType(variable.Initializer),
        VariableDeclarationStatement declaration => declaration.Variables.Any(variable => StatementReferencesExceptionType(variable)),
        MultiAssignmentStatement assignment => assignment.Values.Any(ExpressionReferencesExceptionType) || assignment.Targets.Any(ExpressionReferencesExceptionType),
        ExpressionStatement expression => ExpressionReferencesExceptionType(expression.Value),
        ReturnStatement returned => returned.Value is not null && ExpressionReferencesExceptionType(returned.Value),
        IfStatement conditional => ExpressionReferencesExceptionType(conditional.Condition) || StatementReferencesExceptionType(conditional.Then) || conditional.Else is not null && StatementReferencesExceptionType(conditional.Else),
        WhileStatement loop => ExpressionReferencesExceptionType(loop.Condition) || StatementReferencesExceptionType(loop.Body),
        DoWhileStatement loop => ExpressionReferencesExceptionType(loop.Condition) || StatementReferencesExceptionType(loop.Body),
        ForStatement loop => loop.Initializer is not null && StatementReferencesExceptionType(loop.Initializer) || loop.Condition is not null && ExpressionReferencesExceptionType(loop.Condition) || loop.Iterators.Any(ExpressionReferencesExceptionType) || StatementReferencesExceptionType(loop.Body),
        ForInStatement loop => loop.TypeName is not null && IsBuiltinExceptionName(loop.TypeName) || ExpressionReferencesExceptionType(loop.Collection) || StatementReferencesExceptionType(loop.Body),
        SwitchStatement selection => ExpressionReferencesExceptionType(selection.Value) || selection.Sections.Any(section => section.Labels.Any(label => SwitchPatternExpressions(label).Any(ExpressionReferencesExceptionType)) || section.Statements.Any(StatementReferencesExceptionType)),
        OverflowContextStatement overflow => StatementReferencesExceptionType(overflow.Body),
        _ => false
    };

    private static IEnumerable<Expression> SwitchPatternExpressions(SwitchLabel pattern)
    {
        if (pattern.Value is not null) yield return pattern.Value;
        if (pattern.Guard is not null) yield return pattern.Guard;
        if (pattern.LeftPattern is not null)
            foreach (var expression in SwitchPatternExpressions(pattern.LeftPattern)) yield return expression;
        if (pattern.RightPattern is not null)
            foreach (var expression in SwitchPatternExpressions(pattern.RightPattern)) yield return expression;
    }

    private static bool ExpressionReferencesExceptionType(Expression expression) => expression switch
    {
        AllocationExpression allocation => allocation.TypeName is not null && IsBuiltinExceptionName(allocation.TypeName) || allocation.ConstructorArguments?.Any(ExpressionReferencesExceptionType) == true || allocation.Dimensions.Any(ExpressionReferencesExceptionType),
        CastExpression cast => IsBuiltinExceptionName(cast.TypeName) || ExpressionReferencesExceptionType(cast.Operand),
        DefaultValueExpression value => value.TypeName is not null && IsBuiltinExceptionName(value.TypeName),
        BinaryExpression binary => binary.Operator == "is" && binary.Right is NameExpression typeName && IsBuiltinExceptionName(typeName.Name) || ExpressionReferencesExceptionType(binary.Left) || ExpressionReferencesExceptionType(binary.Right),
        MemberAccessExpression member => ExpressionReferencesExceptionType(member.Receiver),
        CallExpression call => ExpressionReferencesExceptionType(call.Target) || call.Arguments.Any(ExpressionReferencesExceptionType),
        IndexExpression index => ExpressionReferencesExceptionType(index.Receiver) || index.Indices.Any(ExpressionReferencesExceptionType),
        UnaryExpression unary => ExpressionReferencesExceptionType(unary.Operand),
        ConditionalExpression conditional => ExpressionReferencesExceptionType(conditional.Condition) || ExpressionReferencesExceptionType(conditional.WhenTrue) || ExpressionReferencesExceptionType(conditional.WhenFalse),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(ExpressionReferencesExceptionType),
        NullAssertionExpression assertion => ExpressionReferencesExceptionType(assertion.Operand),
        OverflowContextExpression overflow => ExpressionReferencesExceptionType(overflow.Value),
        _ => false
    };

    private static bool IsBuiltinExceptionType(TypeDeclaration type) => BuiltinExceptionNames.Contains(type.Name, StringComparer.Ordinal) && type.Namespace == BuiltinExceptionNamespace(type.Name);
    private static bool IsBuiltinExceptionType(MethodSymbol method) => IsBuiltinExceptionType(method.Owner);

    private bool IsExceptionType(string typeName) => GetClassChain(typeName).Contains("System.Exception", StringComparer.Ordinal);

    public CompilationAnalysis Analyze(IReadOnlyList<CompilationUnit> units)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var methods = new List<MethodSymbol>();
        var fieldDeclarations = new List<(TypeDeclaration Owner, FieldDeclaration Declaration)>();
        var structFieldDeclarations = new List<(TypeDeclaration Owner, FieldDeclaration Declaration)>();
        var classFieldDeclarations = new List<(TypeDeclaration Owner, FieldDeclaration Declaration)>();
        var staticConstructors = new Dictionary<TypeDeclaration, ConstructorDeclaration>(ReferenceEqualityComparer.Instance);
        var parsedSourceTypes = units.SelectMany(u => u.Types).ToArray();
        _genericTypeSpecializer = new GenericTypeSpecializer(diagnostics, _nativePointerSize, cancellationToken);
        var genericExpansion = _genericTypeSpecializer.Expand(parsedSourceTypes);
        _genericDefinitions = genericExpansion.GenericDefinitions;
        _genericTypeInstantiations.AddRange(genericExpansion.Instantiations);
        var sourceTypes = genericExpansion.ConcreteTypes.ToArray();
        var sourceObject = sourceTypes.FirstOrDefault(type => GetQualifiedTypeName(type) == "System.Object" && IsRuntimeLibraryDeclaration(type));
        foreach (var declaration in sourceTypes.Where(type => GetQualifiedTypeName(type) == "System.Object" && !IsRuntimeLibraryDeclaration(type)))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "'System.Object' is a predefined root type and cannot be declared in source.", declaration.Location);
        var sourceDelegate = sourceTypes.FirstOrDefault(type => GetQualifiedTypeName(type) == "System.Delegate" && IsRuntimeLibraryDeclaration(type));
        foreach (var declaration in sourceTypes.Where(type => GetQualifiedTypeName(type) == "System.Delegate" && !IsRuntimeLibraryDeclaration(type)))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "'System.Delegate' is a predefined type and cannot be declared in source.", declaration.Location);
        var sourceArray = sourceTypes.FirstOrDefault(type => GetQualifiedTypeName(type) == "System.Array" && IsRuntimeLibraryDeclaration(type));
        foreach (var declaration in sourceTypes.Where(type => GetQualifiedTypeName(type) == "System.Array" && !IsRuntimeLibraryDeclaration(type)))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "'System.Array' is a predefined type and cannot be declared in source.", declaration.Location);
        var sourceValueType = sourceTypes.FirstOrDefault(type => GetQualifiedTypeName(type) == "System.ValueType" && IsRuntimeLibraryDeclaration(type));
        foreach (var declaration in sourceTypes.Where(type => GetQualifiedTypeName(type) == "System.ValueType" && !IsRuntimeLibraryDeclaration(type)))
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "'System.ValueType' is a predefined type and cannot be declared in source.", declaration.Location);
        // Delegate wrappers and closure environments allocate managed objects even though
        // those allocations are implicit in source syntax; preserve the specified catchable
        // out-of-memory path whenever a source delegate type can be used.
        _usesExceptionHandling = CompilationMayRaise(sourceTypes) || sourceTypes.Any(type => type.Kind == "delegate" ||
            type.Members.OfType<ConstructorDeclaration>().Any(constructor => constructor.IsStatic));
        // Runtime source may provide the canonical declarations for compiler-known
        // types. Keep the synthetic declarations as fallbacks for standalone source
        // compilation, while letting runtime members participate in normal binding.
        _types = sourceTypes
            .Append(sourceObject is null ? VirtualDispatchLayout.ObjectType : null)
            .Append(sourceDelegate is null ? VirtualDispatchLayout.DelegateType : null)
            .Append(sourceArray is null ? VirtualDispatchLayout.ArrayType : null)
            .Append(sourceValueType is null ? VirtualDispatchLayout.ValueType : null)
            .Where(type => type is not null).Cast<TypeDeclaration>()
            .Concat(_usesExceptionHandling ? CreateBuiltinExceptionTypes(sourceTypes) : [])
            .ToArray();
        foreach (var unit in units)
        {
            // A using alias is a source-level type reference even if no expression
            // happens to use the alias. Model its file/namespace scope with a
            // synthetic top-level context so it cannot inherit private access from
            // one of the types declared in that file.
            var fileContext = new TypeDeclaration(unit.Namespace, "class", "$using", [], unit.Location,
                Usings: unit.Usings ?? []);
            foreach (var alias in unit.Usings?.Where(directive => directive.Alias is not null) ?? [])
                ValidateTypeAccessibility(alias.Target, fileContext, alias.Location);
        }
        foreach (var enumType in _types.Where(type => type.Kind == "enum"))
        {
            var underlying = enumType.EnumUnderlyingType ?? "int32";
            if (!IntegerTypes.Contains(underlying))
                diagnostics.Add("LS1101", DiagnosticSeverity.Error, $"Enum underlying type '{underlying}' must be an integral type.", enumType.Location);
            else _enumUnderlyingTypes[GetQualifiedTypeName(enumType)] = underlying;
        }
        foreach (var structType in _types.Where(type => type.Kind == "struct"))
            _structTypes[GetQualifiedTypeName(structType)] = new(structType, [], 0, 1);
        foreach (var classType in _types.Where(type => type.Kind == "class"))
            _classTypes[GetQualifiedTypeName(classType)] = new(classType, []);
        foreach (var interfaceType in _types.Where(type => type.Kind == "interface"))
            _interfaceTypes[GetQualifiedTypeName(interfaceType)] = interfaceType;
        foreach (var delegateType in _types.Where(type => type.Kind == "delegate" && !type.HasTypeParameters))
            _delegateTypes[GetQualifiedTypeName(delegateType)] = delegateType;
        foreach (var key in _delegateTypes.Keys.ToArray())
        {
            _delegateTypes[key] = NormalizeDelegateType(_delegateTypes[key]);
        }
        AnalyzeClassInheritance();
        var constFieldDeclarations = new List<(TypeDeclaration Owner, ConstantFieldDeclaration Declaration)>();
        foreach (var type in _types)
        {
            if (type.Kind == "enum") { continue; }
            if (type.Kind == "interface")
            {
                foreach (var invalidMember in type.Members.Where(member =>
                    member is not MethodDeclaration &&
                    member is not ComputedPropertyDeclaration && member is not ComputedIndexerDeclaration))
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Interfaces currently support method, property, and indexer declarations only.", invalidMember.Location);
                foreach (var contract in type.Members.OfType<MethodDeclaration>())
                {
                    if (contract.IsStatic)
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Static interface methods are not yet supported by the current backend.", contract.Location);
                    if (contract.HasTypeParameters)
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Generic interface methods are not yet supported by the current backend.", contract.Location);
                    ValidateDeclaredType(NormalizeEnumType(contract.ReturnType, type), "Interface method return", contract.Location, type);
                    foreach (var parameter in contract.Parameters)
                        ValidateDeclaredType(NormalizeEnumType(parameter.TypeName, type), "Interface method parameter", parameter.Location, type);
                }
                continue;
            }
            if (type.Kind == "delegate")
            {
                // Generic delegate declarations are parsed and retained for accurate
                // diagnostics, but their closed runtime representation is not yet lowered.
                if (type.HasTypeParameters) continue;
                if (type.DelegateReturnType is { } delegateReturnType)
                    ValidateDeclaredType(NormalizeEnumType(delegateReturnType, type), "Delegate return", type.Location, type);
                foreach (var parameter in type.DelegateParameters ?? [])
                    ValidateDeclaredType(NormalizeEnumType(parameter.TypeName, type), "Delegate parameter", parameter.Location, type);
                continue;
            }
            if (type.IsStatic && type.Kind != "class")
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Only classes may be declared static.", type.Location);
            if (type.IsStatic && type.Kind == "class")
            {
                if (type.HasBaseList || type.BaseTypes is { Count: > 0 })
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A static class cannot declare a base type.", type.Location);
                foreach (var member in type.Members)
                {
                    var invalid = member switch
                    {
                        FieldDeclaration field => !field.IsStatic,
                        MethodDeclaration method => !method.IsStatic || method.IsVirtual || method.IsOverride,
                        ConstructorDeclaration constructor => !constructor.IsStatic,
                        FinalizerDeclaration => true,
                        _ => false
                    };
                    if (invalid)
                        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A static class can contain only static fields, static methods, and a static constructor.", member.Location);
                }
            }
            if (type.HasTypeParameters) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Generic types are not yet supported.", type.Location);
            if (type.Kind is not ("class" or "struct") && type.HasBaseList) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Inheritance is not supported for {type.Kind} declarations.", type.Location);
            if (type.Kind is not ("class" or "struct")) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"{type.Kind} declarations are not supported by the current backend.", type.Location);
            foreach (var member in type.Members)
            {
                if (member is ConstantFieldDeclaration constField) { constFieldDeclarations.Add((type, constField)); continue; }
                if (member is FieldDeclaration field)
                {
                    if (type.Kind == "struct" && !field.IsStatic) structFieldDeclarations.Add((type, field));
                    else if (type.Kind == "class" && !field.IsStatic) classFieldDeclarations.Add((type, field));
                    else fieldDeclarations.Add((type, field));
                    continue;
                }
                if (member is ConstructorDeclaration constructor)
                {
                    if (constructor.IsStatic)
                    {
                        if (constructor.Parameters.Count != 0)
                            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A static constructor cannot declare parameters.", constructor.Location);
                        if (constructor.InitializerKind is not null)
                            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A static constructor cannot have a constructor initializer.", constructor.Location);
                        if (!staticConstructors.TryAdd(type, constructor))
                            diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Type '{GetQualifiedTypeName(type)}' may declare only one static constructor.", constructor.Location);
                        continue;
                    }
                    if (type.Kind == "struct" && constructor.Parameters.Count == 0)
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Structs cannot declare a parameterless constructor; S() always creates the default value.", constructor.Location);
                    var normalizedConstructorParameters = constructor.Parameters.Select(parameter => parameter with { TypeName = NormalizeEnumType(parameter.TypeName, type) }).ToArray();
                    var synthetic = new MethodDeclaration(type.Name, "$constructor", type.Kind == "struct" ? GetQualifiedTypeName(type) : "void", normalizedConstructorParameters,
                        constructor.Body, type.Kind == "struct", constructor.IsPublic, type.Kind == "class", false, constructor.Location,
                        Accessibility: constructor.Accessibility);
                    if (type.Kind != "class" && constructor.InitializerKind is not null)
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Constructor initializers are supported only for class constructors.", constructor.Location);
                    methods.Add(new(type, synthetic, IsStructConstructor: type.Kind == "struct", IsClassConstructor: type.Kind == "class", SourceConstructor: constructor));
                    continue;
                }
                if (member is FinalizerDeclaration finalizer)
                {
                    if (type.Kind != "class")
                    {
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Only classes may declare finalizers.", finalizer.Location);
                        continue;
                    }
                    if (methods.Any(candidate => candidate.Owner == type && candidate.IsClassFinalizer))
                    {
                        diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Class '{GetQualifiedTypeName(type)}' may declare only one finalizer.", finalizer.Location);
                        continue;
                    }
                    var synthetic = new MethodDeclaration(type.Name, "$finalizer", "void", [], finalizer.Body, false, false, true, false, finalizer.Location);
                    methods.Add(new(type, synthetic, IsClassFinalizer: true));
                    continue;
                }
                if (member is ComputedPropertyDeclaration property)
                {
                    var propertyType = NormalizeEnumType(property.TypeName, type);
                    var getterAccessibility = property.GetterAccessibility ?? property.Accessibility;
                    var setterAccessibility = property.SetterAccessibility ?? property.Accessibility;
                    if (propertyType == "void")
                        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A property cannot have type 'void'.", property.Location);
                    if (property.IsStatic && (property.IsVirtual || property.IsOverride) || type.Kind == "struct" && (property.IsVirtual || property.IsOverride))
                        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A computed property cannot be static virtual/override or a virtual/override struct member.", property.Location);
                    MethodSymbol? getter = null;
                    MethodSymbol? setter = null;
                    if (property.GetterBody is { } getterBody)
                    {
                        var getterDeclaration = new MethodDeclaration(type.Name, "$property_get_" + property.Name, propertyType, [], getterBody,
                            property.IsStatic, getterAccessibility.Contains("public", StringComparison.Ordinal), false, false, property.Location,
                            IsVirtual: property.IsVirtual, IsOverride: property.IsOverride, Accessibility: getterAccessibility, PropertyName: property.Name,
                            ExplicitInterfaceName: property.ExplicitInterfaceName);
                        getter = new(type, getterDeclaration);
                        methods.Add(getter);
                    }
                    if (property.SetterBody is { } setterBody)
                    {
                        var setterParameters = new[] { new ParameterDeclaration(propertyType, "value", property.Location) };
                        var setterDeclaration = new MethodDeclaration(type.Name, "$property_set_" + property.Name, propertyType, setterParameters,
                            CompletePropertySetterBody(setterBody, property.Location), property.IsStatic,
                            setterAccessibility.Contains("public", StringComparison.Ordinal), !property.IsStatic, false, property.Location,
                            IsVirtual: property.IsVirtual, IsOverride: property.IsOverride, Accessibility: setterAccessibility, PropertyName: property.Name,
                            ExplicitInterfaceName: property.ExplicitInterfaceName);
                        setter = new(type, setterDeclaration);
                        methods.Add(setter);
                    }
                    var propertyKey = GetQualifiedTypeName(type) + "." + property.Name +
                        (property.ExplicitInterfaceName is null ? "" : "$" + property.ExplicitInterfaceName);
                    if (type.Members.Any(candidate => candidate switch
                        {
                            FieldDeclaration field => field.Name == property.Name,
                            ConstantFieldDeclaration constant => constant.Name == property.Name,
                            _ => false
                        }))
                        diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Property '{propertyKey}' conflicts with a field or constant of the same name.", property.Location);
                    if (!_computedProperties.TryAdd(propertyKey, new(type, property, propertyType, getter, setter)))
                        diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Property '{propertyKey}' is declared more than once.", property.Location);
                    continue;
                }
                if (member is ComputedIndexerDeclaration indexer)
                {
                    var indexerType = NormalizeEnumType(indexer.TypeName, type);
                    var getterAccessibility = indexer.GetterAccessibility ?? indexer.Accessibility;
                    var setterAccessibility = indexer.SetterAccessibility ?? indexer.Accessibility;
                    var ownerKey = GetQualifiedTypeName(type);
                    if (!_computedIndexers.TryGetValue(ownerKey, out var ownerIndexers)) _computedIndexers[ownerKey] = ownerIndexers = [];
                    if (type.Kind == "struct" && (indexer.IsVirtual || indexer.IsOverride))
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Struct indexers cannot be virtual or override members.", indexer.Location);
                    if (indexerType == "void")
                        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "An indexer cannot have type 'void'.", indexer.Location);
                    if (indexer.Parameters.Any(parameter => parameter.DefaultValue is not null || parameter.PassingMode.Length > 0 || parameter.IsVariadic))
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Indexer parameters cannot be optional, ref, out, in, or variadic in the current implementation.", indexer.Location);
                    var normalizedParameters = indexer.Parameters.Select(parameter => parameter with
                    {
                        TypeName = NormalizeEnumType(parameter.TypeName, type),
                        DefaultValue = null,
                        PassingMode = "",
                        IsVariadic = false
                    }).ToArray();
                    var indexerSignature = string.Join("\u001f", normalizedParameters.Select(parameter => parameter.TypeName));
                    if (ownerIndexers.Any(candidate => candidate.Declaration.ExplicitInterfaceName == indexer.ExplicitInterfaceName &&
                        string.Join("\u001f", candidate.Declaration.Parameters.Select(parameter => NormalizeEnumType(parameter.TypeName, type))) == indexerSignature))
                    {
                        diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Indexer on '{ownerKey}' is declared more than once with the same parameter types.", indexer.Location);
                        continue;
                    }
                    MethodSymbol? getter = null;
                    MethodSymbol? setter = null;
                    if (indexer.GetterBody is { } getterBody)
                    {
                        var getterDeclaration = new MethodDeclaration(type.Name, "$indexer_get", indexerType, normalizedParameters, getterBody,
                            false, getterAccessibility.Contains("public", StringComparison.Ordinal), false, false, indexer.Location,
                            IsVirtual: indexer.IsVirtual, IsOverride: indexer.IsOverride, Accessibility: getterAccessibility,
                            ExplicitInterfaceName: indexer.ExplicitInterfaceName);
                        getter = new(type, getterDeclaration);
                        methods.Add(getter);
                    }
                    if (indexer.SetterBody is { } setterBody)
                    {
                        var setterParameters = normalizedParameters.Append(new ParameterDeclaration(indexerType, "value", indexer.Location)).ToArray();
                        var setterDeclaration = new MethodDeclaration(type.Name, "$indexer_set", indexerType, setterParameters, CompletePropertySetterBody(setterBody, indexer.Location),
                            false, setterAccessibility.Contains("public", StringComparison.Ordinal), true, false, indexer.Location,
                            IsVirtual: indexer.IsVirtual, IsOverride: indexer.IsOverride, Accessibility: setterAccessibility,
                            ExplicitInterfaceName: indexer.ExplicitInterfaceName);
                        setter = new(type, setterDeclaration);
                        methods.Add(setter);
                    }
                    ownerIndexers.Add(new(type, indexer, indexerType, getter, setter));
                    continue;
                }
                if (member is not MethodDeclaration method) { diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"{((UnsupportedMember)member).Description} are recognized but not supported yet.", member.Location); continue; }
                if (method.HasTypeParameters)
                {
                    if (method.IsVirtual || method.IsOverride)
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Generic methods cannot be virtual or override methods.", method.Location);
                    if (method.IsExtern)
                        diagnostics.Add("LS2101", DiagnosticSeverity.Error, "Extern methods cannot be generic.", method.Location);
                    var methodTypeParameters = (method.TypeParameters ?? []).ToHashSet(StringComparer.Ordinal);
                    string NormalizeGenericMethodType(string typeName) =>
                        GenericTypeSpecializer.ReferencesAnyTypeParameter(typeName, methodTypeParameters)
                            ? typeName
                            : NormalizeEnumType(typeName, type);
                    var genericMethod = method with
                    {
                        ReturnType = NormalizeGenericMethodType(method.ReturnType),
                        Parameters = method.Parameters.Select(parameter => parameter with { TypeName = NormalizeGenericMethodType(parameter.TypeName) }).ToArray()
                    };
                    _genericMethodDefinitions.Add(new(type, genericMethod));
                    continue;
                }
                var normalizedMethod = method with
                {
                    ReturnType = NormalizeEnumType(method.ReturnType, type),
                    Parameters = method.Parameters.Select(parameter => parameter with { TypeName = NormalizeEnumType(parameter.TypeName, type) }).ToArray()
                };
                methods.Add(new(type, normalizedMethod));
            }
        }
        foreach (var classType in _types.Where(type => type.Kind == "class" && !type.IsStatic && GetQualifiedTypeName(type) != "System.Delegate" &&
                     !type.Members.OfType<ConstructorDeclaration>().Any(constructor => !constructor.IsStatic)))
        {
            var location = classType.Location;
            var synthetic = new MethodDeclaration(classType.Name, "$constructor", "void", [], new BlockStatement([], location), false, !classType.IsVirtual, true, false, location);
            methods.Add(new(classType, synthetic, IsClassConstructor: true));
        }
        _methods = methods;
        _methodList = methods;
        BuildInterfaceContractLayout();
        ValidateInterfaceImplementations();
        _interfaceDispatchTables = BuildInterfaceDispatchTables();
        foreach (var type in _types)
            ValidateGenericConstraintAccessibility(type.GenericConstraints, type);
        foreach (var method in _genericMethodDefinitions)
            ValidateGenericConstraintAccessibility(method.Declaration.GenericConstraints, method.Owner);
        ValidateGenericTypeConstraints();
        ValidateInconsistentAccessibility(methods);
        ValidateVirtualMethods(methods);
        ValidateFinalizerCompletionCapacity(methods);
        AnalyzeEnumMembers();
        AnalyzeConstantFields(constFieldDeclarations);
        AnalyzeStructFields(structFieldDeclarations);
        AnalyzeClassFields(classFieldDeclarations);
        AnalyzeStaticFields(fieldDeclarations, staticConstructors);
        AddTypeInitializerMethods(_types, staticConstructors, methods);
        var methodSignatures = new HashSet<string>(StringComparer.Ordinal);
        foreach (var method in methods)
        {
            var signature = $"{method.QualifiedName}[{method.Declaration.HasTypeParameters}]:{method.Declaration.ExplicitInterfaceName}:{string.Join(",", method.Declaration.Parameters.Select(p => p.TypeName))}" +
                (method.Declaration.IsConversionOperator ? "->" + method.Declaration.ReturnType : "");
            if (!methodSignatures.Add(signature)) diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Method '{method.QualifiedName}' is declared more than once with the same parameter types.", method.Declaration.Location);
        }
        foreach (var symbolGroup in methods.Where(method => method.Declaration.IsExtern).GroupBy(method => method.Declaration.Name, StringComparer.Ordinal).Where(group => group.Count() > 1))
            foreach (var duplicate in symbolGroup.Skip(1)) diagnostics.Add("LS2101", DiagnosticSeverity.Error, $"Extern symbol '{symbolGroup.Key}' is declared more than once; native symbols cannot be overloaded.", duplicate.Declaration.Location);
        DiscoverPotentialScopeBoundClassFieldStores(methods);
        var mains = methods.Where(m => m.Declaration.Name == "Main" && IsSupportedEntryPoint(m)).ToList();
        if (!options.StaticLibrary && mains.Count > 1) diagnostics.Add("LS2201", DiagnosticSeverity.Error, "An executable must have exactly one entry point; multiple supported Main methods were found.", mains[1].Declaration.Location);
        for (var methodIndex = 0; methodIndex < methods.Count; methodIndex++)
        {
            var method = methods[methodIndex];
            var declaration = method.Declaration;
            ValidateOptionalParameters(method);
            ValidateVariadicParameters(method);
            if (!declaration.IsStatic && !method.IsStructConstructor && !method.IsClassConstructor && !method.IsClassFinalizer && !method.IsStructInstanceMethod && !method.IsClassInstanceMethod) diagnostics.Add(declaration.IsExtern ? "LS2101" : "LMC3000", DiagnosticSeverity.Error, declaration.IsExtern ? "Extern methods must be static." : "Only class and struct instance methods are supported.", declaration.Location);
            if (declaration.IsMutating && !method.IsStructInstanceMethod && !method.IsClassInstanceMethod && !method.IsClassConstructor && !method.IsClassFinalizer) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "The 'mut' modifier is supported only on instance methods.", declaration.Location);
            if (declaration.IsOperator)
            {
                if (declaration.IsConversionOperator)
                {
                    var expectedType = GetQualifiedTypeName(method.Owner);
                    var sourceType = declaration.Parameters.Count == 1
                        ? NormalizeEnumType(declaration.Parameters[0].TypeName, method.Owner)
                        : "?";
                    var targetType = NormalizeEnumType(declaration.ReturnType, method.Owner);
                    var containingTypeParticipates = sourceType == expectedType || targetType == expectedType;
                    var validParameters = declaration.Parameters.Count == 1 &&
                        declaration.Parameters[0].PassingMode.Length == 0 &&
                        !declaration.Parameters[0].IsVariadic &&
                        declaration.Parameters[0].DefaultValue is null;
                    if (method.Owner.Kind is not ("class" or "struct") || method.Owner.IsStatic ||
                        !declaration.IsStatic || !declaration.IsPublic || !validParameters || !containingTypeParticipates ||
                        declaration.ReturnType == "void" || sourceType == targetType)
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error,
                            $"A {method.Owner.Kind} conversion operator must be public static, have one by-value parameter, and involve its containing type.",
                            declaration.Location);
                }
                else
                {
                var supportedOperator = declaration.Operator is "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>" or "==" or "<" or ">" or "<=" or ">=" or "!" or "~" or "++" or "--";
                if (method.Owner.Kind is not ("struct" or "class") || !supportedOperator)
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "User-defined operators are supported only on classes and structs, and only for operators listed by the language specification.", declaration.Location);
                else
                {
                    var expectedType = GetQualifiedTypeName(method.Owner);
                    var unaryOperator = declaration.Operator is "!" or "~" or "++" or "--" || (declaration.Operator is "+" or "-") && declaration.Parameters.Count == 1;
                    var expectedArity = unaryOperator ? 1 : 2;
                    var parametersValid = declaration.Parameters.Count == expectedArity &&
                        (declaration.Parameters.Any(parameter => parameter.TypeName == expectedType) || declaration.Operator is "++" or "--" && declaration.ReturnType == expectedType) &&
                        declaration.Parameters.All(parameter => parameter.PassingMode.Length == 0 && !parameter.IsVariadic && parameter.DefaultValue is null);
                    if (unaryOperator && declaration.Parameters.FirstOrDefault()?.TypeName != expectedType) parametersValid = false;
                    var resultValid = declaration.Operator is "==" or "<" or ">" or "<=" or ">=" or "!"
                        ? declaration.ReturnType == "bool"
                        : declaration.Operator is "++" or "--" ? declaration.ReturnType == expectedType : declaration.ReturnType != "void";
                    if (!declaration.IsStatic || !declaration.IsPublic || !parametersValid || !resultValid)
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"A {method.Owner.Kind} '{declaration.Operator}' operator must be public static, use by-value operands with at least one containing-type operand, and return a valid operator result.", declaration.Location);
                    if (method.Owner.Kind == "struct" && declaration.Operator == "==" && declaration.Parameters.Any(parameter => parameter.TypeName != expectedType))
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A struct '==' operator must accept two values of its containing struct type.", declaration.Location);
                }
                }
            }
            if (declaration.IsExtern)
            {
                if (declaration.Body is not null) diagnostics.Add("LS2101", DiagnosticSeverity.Error, "Extern methods must not have a body.", declaration.Location);
                if (declaration.HasTypeParameters) diagnostics.Add("LS2101", DiagnosticSeverity.Error, "Extern methods cannot be generic.", declaration.Location);
                var isFileSystemStringResult = method.Owner.Namespace == "System.IO" && method.Owner.Name == "NativeFileSystem" &&
                    declaration.Name is "luma_fs_current_directory" or "luma_fs_temporary_directory" && declaration.ReturnType == "string";
                var isEnvironmentStringResult = method.Owner.Namespace == "System" && method.Owner.Name == "Environment" &&
                    declaration.Name is "luma_env_executable_path" or "luma_env_executable_directory" or "luma_env_executable_name" && declaration.ReturnType == "string";
                if (!isFileSystemStringResult && !isEnvironmentStringResult)
                    ValidateExternType(declaration.ReturnType, allowVoid: true, declaration.Location);
                foreach (var parameter in declaration.Parameters)
                {
                    ValidateExternType(parameter.TypeName, allowVoid: false, parameter.Location);
                    if (parameter.PassingMode.Length > 0) diagnostics.Add("LS2101", DiagnosticSeverity.Error, "Extern parameters must use their C ABI value representation and cannot use ref, in, or out.", parameter.Location);
                }
            }
            else if (declaration.Body is null && !(method.IsClassInstanceMethod && declaration.IsVirtual && method.Owner.IsVirtual))
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Bodyless methods must be extern or virtual members in a virtual class.", declaration.Location);
            ValidateDeclaredType(declaration.ReturnType, "Return", declaration.Location, method.Owner);
            foreach (var p in declaration.Parameters) ValidateDeclaredType(p.TypeName, "Parameter", p.Location, method.Owner);
            if (declaration.Body is not null) { AnalyzeMethod(method); AnalyzeOutParameterFlow(method); }
        }
        ValidateConstructorDelegationCycles();
        ValidateConstructorThisUsage();
        ValidateClassReferenceFieldInitialization();
        AnalyzeStrongReferenceCycles();
        foreach (var typeName in _classTypes.Keys)
            if (IsExceptionType(typeName)) _exceptionTypes.Add(typeName);
        if (_usesExceptionHandling)
            _exceptionTypes.UnionWith(BuiltinExceptionNames.Select(name => BuiltinExceptionNamespace(name) + "." + name));
        var compilationMethods = _usesExceptionHandling ? methods : methods.Where(method => !IsBuiltinExceptionType(method)).ToList();
        var compilationClassTypes = _usesExceptionHandling ? _classTypes : _classTypes.Where(pair => !IsBuiltinExceptionType(pair.Value.Declaration))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var classFieldAccesses = new Dictionary<MemberAccessExpression, StructFieldBinding>(ReferenceEqualityComparer.Instance);
        foreach (var (access, binding) in _boundClassFields)
            classFieldAccesses.Add(access, binding with
            {
                IsScopeBound = _scopeBoundClassFields.Contains((binding.DeclaredOwnerType ?? binding.OwnerType, access.Member))
            });
        var unqualifiedClassFields = new Dictionary<NameExpression, StructFieldBinding>(ReferenceEqualityComparer.Instance);
        foreach (var (name, binding) in _boundUnqualifiedClassFields)
            unqualifiedClassFields.Add(name, binding with
            {
                IsScopeBound = _scopeBoundClassFields.Contains((binding.DeclaredOwnerType ?? binding.OwnerType, name.Name))
            });
        var multiAssignmentAccessorAssignments = new Dictionary<MultiAssignmentStatement, IReadOnlyDictionary<int, BinaryExpression>>(ReferenceEqualityComparer.Instance);
        foreach (var (statement, assignments) in _multiAssignmentAccessorAssignments)
            multiAssignmentAccessorAssignments.Add(statement, assignments);
        return new(compilationMethods, options.StaticLibrary ? null : mains.FirstOrDefault(), _variableTypes, _forInTypes, _loweredForInLoops, _scopeBoundVariables, _callTargets, _enumToStringCalls, _classToStringCalls, _structToStringCalls, _virtualCallSlots, _interfaceCallSlots, _interfaceTypes, _delegateTypes, _delegateMethodGroups, _closureEnvironments, _forHeaderClosureEnvironments, _forInClosureEnvironments, _blockClosureEnvironments, _compositeClosureEnvironments, _delegateInvocations, _interfaceMethodSlots, _interfaceDispatchTables, _interfaceConversions, _defaultTypes, _fieldAccesses, _boundUnqualifiedConstantFields, _staticFields.Values.Select(f => f.Symbol).ToArray(), _boundStaticFields, _propertyGetterCalls, _propertyGetterNameCalls, _propertySetterCalls, multiAssignmentAccessorAssignments, _indexerGetterCalls, _boundUnqualifiedStaticFields, _structTypes, compilationClassTypes, _boundStructFields, _boundUnqualifiedStructFields, classFieldAccesses, unqualifiedClassFields, _structConstructors, _classAllocations, _genericConstructions, _operatorTargets, _userConversionOperators, _enumUnderlyingTypes, _castTypes, _typeOperatorTargets, _switchTypePatterns, _sizeOfTypes, _callArguments, _constructorInitializers, _stringLiterals, _stringLengthAccesses, _stringIndexAccesses, _managedArrayAllocations, _managedArrayIndexAccesses, _managedArrayLengthAccesses, _managedArrayRankAccesses, _managedArrayPointerAccesses, _genericSpanEmptyAccesses, _managedArrayGetLengthCalls, _managedArrayForInElementTypes, _exceptionCatchTypes, _throwTypes, _exceptionTypes, _usesExceptionHandling, _stringLiteralPool, _stringifiedPrimitiveTypes, _nativePointerSize, options.TargetTriple, _conditionalExpressionTypes);
    }

    private bool IsRuntimeLibraryDeclaration(TypeDeclaration type) =>
        runtimeSourcePaths.Contains(Path.GetFullPath(type.Location.File)) ||
        type.Location.File.Replace('\\', '/').Contains("LumaSharp.Library/", StringComparison.OrdinalIgnoreCase);

    private void DiscoverPotentialScopeBoundClassFieldStores(IReadOnlyList<MethodSymbol> methods)
    {
        var previousCount = -1;
        while (previousCount != _scopeBoundClassFields.Count)
        {
            previousCount = _scopeBoundClassFields.Count;
            foreach (var method in methods)
            {
                if (method.Declaration.Body is not { } body) continue;
                var scopes = new List<Dictionary<string, (string Type, bool IsScopeBound)>> { new(StringComparer.Ordinal) };
                VisitStatement(body, method, scopes, isRootBlock: true);
            }
        }

        void VisitStatement(Statement statement, MethodSymbol method,
            List<Dictionary<string, (string Type, bool IsScopeBound)>> scopes, bool isRootBlock = false)
        {
            switch (statement)
            {
                case BlockStatement block:
                    if (!isRootBlock) scopes.Add(new(StringComparer.Ordinal));
                    foreach (var child in block.Statements) VisitStatement(child, method, scopes);
                    if (!isRootBlock) scopes.RemoveAt(scopes.Count - 1);
                    break;
                case VariableStatement variable:
                    VisitExpression(variable.Initializer, method, scopes);
                    scopes[^1][variable.Name] = (GetStaticType(variable.Initializer, method, scopes, variable.TypeName) ?? "?",
                        IsPotentialScopeBound(variable.Initializer, method, scopes));
                    break;
                case VariableDeclarationStatement declaration:
                {
                    var values = declaration.Variables.Select(variable =>
                    {
                        VisitExpression(variable.Initializer, method, scopes);
                        return (Variable: variable,
                            Type: GetStaticType(variable.Initializer, method, scopes, variable.TypeName) ?? "?",
                            IsScopeBound: IsPotentialScopeBound(variable.Initializer, method, scopes));
                    }).ToArray();
                    foreach (var value in values) scopes[^1][value.Variable.Name] = (value.Type, value.IsScopeBound);
                    break;
                }
                case ExpressionStatement expression:
                    VisitExpression(expression.Value, method, scopes);
                    break;
                case MultiAssignmentStatement assignment:
                    foreach (var value in assignment.Values) VisitExpression(value, method, scopes);
                    foreach (var target in assignment.Targets) VisitExpression(target, method, scopes);
                    break;
                case ReturnStatement returned:
                    VisitExpression(returned.Value, method, scopes);
                    break;
                case ThrowStatement thrown:
                    VisitExpression(thrown.Value, method, scopes);
                    break;
                case IfStatement conditional:
                    VisitExpression(conditional.Condition, method, scopes);
                    VisitEmbedded(conditional.Then, method, scopes);
                    if (conditional.Else is not null) VisitEmbedded(conditional.Else, method, scopes);
                    break;
                case WhileStatement loop:
                    VisitExpression(loop.Condition, method, scopes);
                    VisitEmbedded(loop.Body, method, scopes);
                    break;
                case DoWhileStatement loop:
                    VisitEmbedded(loop.Body, method, scopes);
                    VisitExpression(loop.Condition, method, scopes);
                    break;
                case ForStatement loop:
                    scopes.Add(new(StringComparer.Ordinal));
                    if (loop.Initializer is not null) VisitStatement(loop.Initializer, method, scopes);
                    VisitExpression(loop.Condition, method, scopes);
                    foreach (var iterator in loop.Iterators) VisitExpression(iterator, method, scopes);
                    VisitEmbedded(loop.Body, method, scopes);
                    scopes.RemoveAt(scopes.Count - 1);
                    break;
                case ForInStatement loop:
                    VisitExpression(loop.Collection, method, scopes);
                    scopes.Add(new(StringComparer.Ordinal));
                    scopes[^1][loop.Name] = (loop.TypeName is null ? "?" : NormalizeEnumType(loop.TypeName, method.Owner), false);
                    VisitEmbedded(loop.Body, method, scopes);
                    scopes.RemoveAt(scopes.Count - 1);
                    break;
                case SwitchStatement selection:
                    VisitExpression(selection.Value, method, scopes);
                    foreach (var section in selection.Sections)
                    {
                        scopes.Add(new(StringComparer.Ordinal));
                        foreach (var label in section.Labels)
                        {
                            VisitExpression(label.Value, method, scopes);
                            VisitExpression(label.Guard, method, scopes);
                        }
                        foreach (var child in section.Statements) VisitStatement(child, method, scopes);
                        scopes.RemoveAt(scopes.Count - 1);
                    }
                    break;
                case TryStatement guarded:
                    VisitStatement(guarded.Body, method, scopes);
                    foreach (var clause in guarded.Catches)
                    {
                        scopes.Add(new(StringComparer.Ordinal));
                        if (clause.VariableName is { } exceptionName)
                            scopes[^1][exceptionName] = (clause.TypeName ?? "System.Exception", false);
                        VisitStatement(clause.Body, method, scopes);
                        scopes.RemoveAt(scopes.Count - 1);
                    }
                    if (guarded.Finally is not null) VisitStatement(guarded.Finally, method, scopes);
                    break;
                case OverflowContextStatement overflow:
                    VisitStatement(overflow.Body, method, scopes);
                    break;
            }
        }

        void VisitEmbedded(Statement statement, MethodSymbol method,
            List<Dictionary<string, (string Type, bool IsScopeBound)>> scopes)
        {
            scopes.Add(new(StringComparer.Ordinal));
            VisitStatement(statement, method, scopes);
            scopes.RemoveAt(scopes.Count - 1);
        }

        void VisitExpression(Expression? expression, MethodSymbol method,
            List<Dictionary<string, (string Type, bool IsScopeBound)>> scopes)
        {
            if (expression is null) return;
            switch (expression)
            {
                case BinaryExpression binary:
                    if (binary.Operator == "=" && IsPotentialScopeBound(binary.Right, method, scopes))
                    {
                        if (binary.Left is MemberAccessExpression member &&
                            GetStaticType(member.Receiver, method, scopes) is { } receiverType &&
                            TryGetClassField(receiverType, member.Member, out var field) && !field.Declaration.IsStatic && IsScopeBoundReferenceType(field.TypeName))
                            _scopeBoundClassFields.Add((GetQualifiedTypeName(field.Owner), field.Declaration.Name));
                        else if (binary.Left is NameExpression name && !TryLookup(name.Name, scopes, out _) && method.Owner.Kind == "class" &&
                                 TryGetClassField(GetQualifiedTypeName(method.Owner), name.Name, out var unqualifiedField) &&
                                 !unqualifiedField.Declaration.IsStatic && IsScopeBoundReferenceType(unqualifiedField.TypeName))
                            _scopeBoundClassFields.Add((GetQualifiedTypeName(unqualifiedField.Owner), unqualifiedField.Declaration.Name));
                    }
                    VisitExpression(binary.Left, method, scopes);
                    VisitExpression(binary.Right, method, scopes);
                    break;
                case UnaryExpression unary:
                    VisitExpression(unary.Operand, method, scopes);
                    break;
                case MemberAccessExpression member:
                    VisitExpression(member.Receiver, method, scopes);
                    break;
                case IndexExpression index:
                    VisitExpression(index.Receiver, method, scopes);
                    foreach (var item in index.Indices) VisitExpression(item, method, scopes);
                    break;
                case CallExpression call:
                    VisitExpression(call.Target, method, scopes);
                    foreach (var argument in call.Arguments) VisitExpression(argument, method, scopes);
                    break;
                case AllocationExpression allocation:
                    foreach (var argument in allocation.ConstructorArguments ?? []) VisitExpression(argument, method, scopes);
                    foreach (var dimension in allocation.Dimensions) VisitExpression(dimension, method, scopes);
                    if (allocation.Initializer is { } allocationInitializer) VisitExpression(allocationInitializer, method, scopes);
                    if (allocation.IsScope && allocation.TypeName is { } allocationType &&
                        (allocation.ConstructorArgumentNames ?? []).All(name => name is null))
                    {
                        var ownerType = NormalizeEnumType(allocationType, method.Owner);
                        var arguments = allocation.ConstructorArguments ?? [];
                        foreach (var constructor in methods.Where(candidate => candidate.IsClassConstructor &&
                                     GetQualifiedTypeName(candidate.Owner) == ownerType && candidate.Declaration.Parameters.Count == arguments.Count))
                        for (var index = 0; index < arguments.Count; index++)
                            if (IsPotentialScopeBound(arguments[index], method, scopes) &&
                                TryGetScopedConstructorParameterStores(constructor, constructor.Declaration.Parameters[index].Name, out var storedFields))
                                _scopeBoundClassFields.UnionWith(storedFields);
                    }
                    break;
                case InlineArrayInitializerExpression initializer:
                    foreach (var element in initializer.Elements) VisitExpression(element, method, scopes);
                    break;
                case ConditionalExpression conditional:
                    VisitExpression(conditional.Condition, method, scopes);
                    VisitExpression(conditional.WhenTrue, method, scopes);
                    VisitExpression(conditional.WhenFalse, method, scopes);
                    break;
                case NullAssertionExpression assertion:
                    VisitExpression(assertion.Operand, method, scopes);
                    break;
                case CastExpression cast:
                    VisitExpression(cast.Operand, method, scopes);
                    break;
                case OverflowContextExpression overflow:
                    VisitExpression(overflow.Value, method, scopes);
                    break;
                case LambdaExpression lambda:
                    VisitExpression(lambda.ExpressionBody, method, scopes);
                    if (lambda.BlockBody is not null) VisitStatement(lambda.BlockBody, method, scopes);
                    break;
            }
        }

        bool IsPotentialScopeBound(Expression? expression, MethodSymbol method,
            List<Dictionary<string, (string Type, bool IsScopeBound)>> scopes) => expression switch
        {
            AllocationExpression { IsScope: true } => true,
            NameExpression name when TryLookup(name.Name, scopes, out var local) => local.IsScopeBound,
            MemberAccessExpression member when _scopeBoundClassFields.Contains(GetPotentialFieldKey(member, method, scopes)) =>
                IsPotentialScopeBound(member.Receiver, method, scopes),
            IndexExpression index => IsPotentialScopeBound(index.Receiver, method, scopes),
            NullAssertionExpression assertion => IsPotentialScopeBound(assertion.Operand, method, scopes),
            CastExpression cast => IsPotentialScopeBound(cast.Operand, method, scopes),
            OverflowContextExpression overflow => IsPotentialScopeBound(overflow.Value, method, scopes),
            ConditionalExpression conditional => IsPotentialScopeBound(conditional.WhenTrue, method, scopes) || IsPotentialScopeBound(conditional.WhenFalse, method, scopes),
            BinaryExpression { Operator: "??" } coalesce => IsPotentialScopeBound(coalesce.Left, method, scopes) || IsPotentialScopeBound(coalesce.Right, method, scopes),
            _ => false
        };

        (string OwnerType, string FieldName) GetPotentialFieldKey(MemberAccessExpression member, MethodSymbol method,
            List<Dictionary<string, (string Type, bool IsScopeBound)>> scopes)
        {
            if (GetStaticType(member.Receiver, method, scopes) is { } receiverType &&
                TryGetClassField(receiverType, member.Member, out var field))
                return (GetQualifiedTypeName(field.Owner), field.Declaration.Name);
            return ("", "");
        }

        string? GetStaticType(Expression? expression, MethodSymbol method,
            List<Dictionary<string, (string Type, bool IsScopeBound)>> scopes, string? explicitType = null)
        {
            if (explicitType is not null) return NormalizeEnumType(explicitType, method.Owner);
            return expression switch
            {
                AllocationExpression { TypeName: { } typeName } => NormalizeEnumType(typeName, method.Owner),
                NameExpression name when TryLookup(name.Name, scopes, out var local) => local.Type,
                NameExpression name when IsClassType(NormalizeEnumType(name.Name, method.Owner)) => NormalizeEnumType(name.Name, method.Owner),
                MemberAccessExpression member when GetStaticType(member.Receiver, method, scopes) is { } receiverType &&
                    TryGetClassField(receiverType, member.Member, out var field) => NormalizeEnumType(field.TypeName, field.Owner),
                NullAssertionExpression assertion => GetStaticType(assertion.Operand, method, scopes),
                CastExpression cast => NormalizeEnumType(cast.TypeName, method.Owner),
                ConditionalExpression conditional => GetStaticType(conditional.WhenTrue, method, scopes) ?? GetStaticType(conditional.WhenFalse, method, scopes),
                _ => null
            };
        }

        bool TryLookup(string name, List<Dictionary<string, (string Type, bool IsScopeBound)>> scopes,
            out (string Type, bool IsScopeBound) local)
        {
            for (var index = scopes.Count - 1; index >= 0; index--)
                if (scopes[index].TryGetValue(name, out local)) return true;
            local = default;
            return false;
        }
    }

    private static int GetNativePointerSize(string targetTriple) => TargetTripleInfo.GetPointerSize(targetTriple, IntPtr.Size);

    private void ValidateOptionalParameters(MethodSymbol method)
    {
        var context = new MethodContext(method); var optionalSeen = false;
        foreach (var (parameter, parameterIndex) in method.Declaration.Parameters.Select((parameter, index) => (parameter, index)))
        {
            if (parameter.IsVariadic)
            {
                continue;
            }
            if (parameter.DefaultValue is null)
            {
                if (optionalSeen) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Required parameters must appear before optional parameters.", parameter.Location);
                continue;
            }
            if (parameter.PassingMode is "ref" or "in" or "out")
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"A '{parameter.PassingMode}' parameter cannot be optional.", parameter.Location);
            optionalSeen = true; BindTypeExpressions(parameter.DefaultValue, method.Owner);
            var actualType = AnalyzeExpression(parameter.DefaultValue, context, parameter.TypeName);
            if (!CanConvert(actualType, parameter.TypeName, parameter.DefaultValue))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Optional parameter default of type '{actualType}' cannot convert to '{parameter.TypeName}'.", parameter.DefaultValue.Location);
            if (!IsConstantExpression(parameter.DefaultValue, context))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Optional parameter defaults must be compile-time constants or target-typed default values.", parameter.DefaultValue.Location);
            else if (!TryEvaluateConstant(parameter.DefaultValue, out var constant, out var error))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, error ?? "Optional parameter default could not be evaluated at compile time.", parameter.DefaultValue.Location);
            else if (!TryConvertConstant(constant, parameter.TypeName, parameter.DefaultValue, out _))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Optional parameter default is not a representable constant of type '{parameter.TypeName}'.", parameter.DefaultValue.Location);
        }
    }

    private void ValidateVariadicParameters(MethodSymbol method)
    {
        foreach (var (parameter, parameterIndex) in method.Declaration.Parameters.Select((parameter, index) => (parameter, index)).Where(item => item.parameter.IsVariadic))
        {
            if (parameterIndex != method.Declaration.Parameters.Count - 1)
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A variadic parameter must be the final parameter.", parameter.Location);
            if (parameter.DefaultValue is not null)
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A variadic parameter cannot have a default value.", parameter.Location);
            if (parameter.PassingMode.Length > 0)
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A variadic parameter cannot use ref, in, or out.", parameter.Location);
            if (method.Declaration.IsExtern)
                diagnostics.Add("LS2101", DiagnosticSeverity.Error, "Variadic parameters are not supported by the C ABI extern implementation.", parameter.Location);
        }
    }

    private void AnalyzeEnumMembers()
    {
        foreach (var owner in _types.Where(type => type.Kind == "enum" && _enumUnderlyingTypes.ContainsKey(GetQualifiedTypeName(type))))
        {
            var previousEnumContext = _enumEvaluationOwner;
            _enumEvaluationOwner = owner;
            try
            {
                var underlying = _enumUnderlyingTypes[GetQualifiedTypeName(owner)];
                var next = BigInteger.Zero;
                foreach (var member in owner.Members.OfType<EnumMemberDeclaration>())
                {
                    if (member.Initializer is not null) BindTypeExpressions(member.Initializer, owner);
                    var key = GetQualifiedTypeName(owner) + "." + member.Name;
                    if (_constantFields.ContainsKey(key))
                    {
                        diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Enum member '{key}' is declared more than once.", member.Location);
                        continue;
                    }
                    BigInteger value;
                    if (member.Initializer is null)
                    {
                        value = next;
                        if (!Fits(value, underlying))
                        {
                            diagnostics.Add("LS1103", DiagnosticSeverity.Error, $"Implicit value for enum member '{member.Name}' overflows underlying type '{underlying}'.", member.Location);
                            continue;
                        }
                    }
                    else
                    {
                        var initializerType = ConstantExpressionType(member.Initializer);
                        if (initializerType is null || !(IntegerTypes.Contains(initializerType) || initializerType == "char"))
                        {
                            diagnostics.Add("LS1102", DiagnosticSeverity.Error, $"Enum member '{member.Name}' must have an integral constant value representable as '{underlying}'.", member.Initializer.Location);
                            continue;
                        }
                        if (!TryEvaluateConstant(member.Initializer, out var constant, out var error))
                        {
                            diagnostics.Add("LS1102", DiagnosticSeverity.Error, error ?? $"Enum member '{member.Name}' must have an integral constant value.", member.Initializer.Location);
                            continue;
                        }
                        if (constant.Integer is not { } integer || !Fits(integer, underlying))
                        {
                            diagnostics.Add("LS1102", DiagnosticSeverity.Error, $"Enum member '{member.Name}' is outside the range of underlying type '{underlying}'.", member.Initializer.Location);
                            continue;
                        }
                        value = integer;
                    }
                    _constantFields.Add(key, (owner, new(GetQualifiedTypeName(owner), value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "public"));
                    next = value + BigInteger.One;
                }
            }
            finally { _enumEvaluationOwner = previousEnumContext; }
        }
    }

    private bool TryGetEnumConstant(string enumName, string memberName, out ConstantFieldBinding binding)
    {
        binding = null!;
        if (_enumEvaluationOwner is not { } context) return false;
        if (enumName.Length == 0 && context.Kind != "enum") return false;
        TypeDeclaration[] candidates = enumName.Length == 0 ? [context] : FindTypes(enumName, context).Where(type => type.Kind == "enum").ToArray();
        if (candidates.Length != 1) return false;
        if (!_constantFields.TryGetValue(GetQualifiedTypeName(candidates[0]) + "." + memberName, out var stored)) return false;
        binding = stored.Binding; return true;
    }

    private bool TryGetEnumConstant(NameExpression name, out ConstantFieldBinding binding) => TryGetEnumConstant("", name.Name, out binding);

    private bool TryGetEnumConstant(MemberAccessExpression access, out ConstantFieldBinding binding)
    {
        binding = null!;
        return TryFlattenName(access.Receiver, out var enumName) && TryGetEnumConstant(enumName, access.Member, out binding);
    }

    private bool TryGetConstantField(NameExpression name, out ConstantFieldBinding binding)
    {
        binding = null!;
        if (_boundUnqualifiedConstantFields.TryGetValue(name, out binding!)) return true;
        if (_enumEvaluationOwner is not { } context) return TryGetEnumConstant(name, out binding);
        if (TryResolveConstantField(GetQualifiedTypeName(context) + "." + name.Name, name.Location, out binding)) return true;
        return TryGetEnumConstant(name, out binding);
    }

    private bool TryGetConstantField(MemberAccessExpression access, out ConstantFieldBinding binding)
    {
        if (_fieldAccesses.TryGetValue(access, out binding!)) return true;
        if (TryGetEnumConstant(access, out binding)) return true;
        binding = null!;
        if (_enumEvaluationOwner is not { } context || !TryFlattenName(access.Receiver, out var requestedType)) return false;
        var types = FindTypes(NormalizeEnumType(requestedType, context), context);
        if (types.Length != 1) return false;
        var key = GetQualifiedTypeName(types[0]) + "." + access.Member;
        if (!TryResolveConstantField(key, access.Location, out binding) || !_constantFields.TryGetValue(key, out var field)) return false;
        if (!IsAccessibilityAvailable(field.Accessibility, field.Owner, context))
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error,
                $"Constant field '{GetQualifiedTypeName(field.Owner)}.{access.Member}' is not accessible from this type.", access.Location);
        binding = field.Binding;
        return true;
    }

    private bool TryResolveConstantField(string key, SourceLocation location, out ConstantFieldBinding binding)
    {
        if (_constantFields.TryGetValue(key, out var resolved))
        { binding = resolved.Binding; return true; }
        binding = null!;
        if (!_pendingConstantFields.TryGetValue(key, out var pending)) return false;
        if (_constantFieldsBeingEvaluated.Contains(key))
        {
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Constant field dependency cycle includes '{key}'.", location);
            return false;
        }
        var previous = _enumEvaluationOwner;
        _enumEvaluationOwner = pending.Owner;
        try { AnalyzeConstantField(pending.Owner, pending.Declaration); }
        finally { _enumEvaluationOwner = previous; }
        if (!_constantFields.TryGetValue(key, out resolved)) return false;
        binding = resolved.Binding;
        return true;
    }

    private string ConstantFieldExpressionType(ConstantFieldBinding binding) =>
        _enumUnderlyingTypes.GetValueOrDefault(binding.TypeName, binding.TypeName);

    private bool TryEvaluateStoredConstant(ConstantFieldBinding binding, out ConstantValue value, out string? error)
    {
        error = null;
        var typeName = _enumUnderlyingTypes.GetValueOrDefault(binding.TypeName, binding.TypeName);
        if (typeName == "string" && binding.StringValue is { } text)
        { value = new(typeName, Text: text); return true; }
        if (typeName == "bool")
        { value = new(typeName, Boolean: binding.Value == "1"); return true; }
        if ((IntegerTypes.Contains(typeName) || typeName == "char") && BigInteger.TryParse(binding.Value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var integer))
        { value = new(typeName, Integer: integer); return true; }
        if (FloatingTypes.Contains(typeName) && double.TryParse(binding.Value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var floating))
        { value = new(typeName, Floating: floating); return true; }
        value = null!;
        error = $"Constant field value of type '{binding.TypeName}' cannot be evaluated by the current compiler.";
        return false;
    }

    private bool TryEvaluateEnumConstant(ConstantFieldBinding binding, out ConstantValue value, out string? error)
    {
        var underlying = _enumUnderlyingTypes.GetValueOrDefault(binding.TypeName);
        if (underlying is not null && BigInteger.TryParse(binding.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var integer))
        { value = new(underlying, Integer: integer); error = null; return true; }
        value = null!; error = "Enum member reference is not a previously declared integral constant."; return false;
    }

    private int GetTypeSize(string type)
    {
        if (IsNullableValueType(type)) return TryGetTypeLayout(type, [], out var nullableSize, out _) ? nullableSize : 0;
        if (_enumUnderlyingTypes.TryGetValue(type, out var underlying)) return GetTypeSize(underlying);
        if (TryParseInlineArrayType(type, out var elementType, out var count))
        {
            var byteCount = (long)GetTypeSize(elementType) * count;
            return byteCount <= int.MaxValue ? (int)byteCount : 0;
        }
        if (_structTypes.TryGetValue(type, out var structure)) return structure.SizeInBytes > 0 ? structure.SizeInBytes : TryGetTypeLayout(type, [], out var size, out _) ? size : 0;
        return type switch
        {
            "bool" or "int8" or "uint8" => 1,
            "int16" or "uint16" or "char" => 2,
            "int32" or "uint32" or "float" => 4,
            "int64" or "uint64" or "double" => 8,
            "int" or "uint" => _nativePointerSize,
            "string" => _nativePointerSize,
            var pointerType when IsPointerType(pointerType) => _nativePointerSize,
            _ => 0
        };
    }

    private bool IsSupportedType(string type) => SupportedTypes.Contains(type) || _enumUnderlyingTypes.ContainsKey(type) || _structTypes.ContainsKey(type) || _classTypes.ContainsKey(type) || _interfaceTypes.ContainsKey(type) || _delegateTypes.ContainsKey(type) || IsPointerType(type) && IsAllowedPointerType(type) ||
        IsNullableClassType(type) || IsNullableDelegateType(type) || IsNullableStringType(type) || IsNullableArrayType(type) || IsNullableValueType(type) ||
        TryParseManagedArrayType(type, out var arrayElementType, out var rank) && rank <= 3 && IsManagedArrayElementType(arrayElementType) ||
        TryParseInlineArrayType(type, out var elementType, out var count) && count > 0 && IsInlineArrayElementType(elementType);
    private bool IsManagedArrayElementType(string type) => NumericTypes.Contains(type) || type is "bool" or "string" or "string?" || _enumUnderlyingTypes.ContainsKey(type) ||
        IsStructType(type) ||
        IsClassReferenceType(type) || IsDelegateType(type) || IsNullableDelegateType(type) || IsNullableArrayType(type) || IsNullableValueType(type) ||
        TryParseManagedArrayType(type, out var nestedElementType, out var nestedRank) && nestedRank <= 3 && IsManagedArrayElementType(nestedElementType);
    private bool IsNonNullableManagedArrayElementReference(string type) => type == "string" || IsClassType(type) || IsDelegateType(type) ||
        !IsNullableArrayType(type) && TryParseManagedArrayType(type, out _, out _);
    private static bool IsPointerType(string type) =>
        (TypeNameSyntax.TryParse(type, out var parsed) && parsed.Suffixes.LastOrDefault() is PointerTypeNameSuffix) || type.EndsWith('*');
    internal static bool TryParseInlineArrayType(string type, out string elementType, out int count)
    {
        elementType = ""; count = 0;
        if (!TryGetInlineArrayParts(type, out elementType, out var lengthExpression)) return false;
        if (!TryEvaluateInlineArrayLength(lengthExpression, out count)) count = 0;
        return true;
    }
    private static bool TryGetInlineArrayParts(string type, out string elementType, out string lengthExpression)
    {
        elementType = ""; lengthExpression = "";
        if (!TypeNameSyntax.TryParse(type, out var parsed) || parsed.Suffixes.LastOrDefault() is not InlineArrayTypeNameSuffix inlineArray)
            return false;
        lengthExpression = inlineArray.LengthExpression;
        if (lengthExpression.Contains(',')) return false;
        elementType = (parsed with { Suffixes = parsed.Suffixes.Take(parsed.Suffixes.Count - 1).ToArray() }).ToString();
        return true;
    }
    internal static bool TryParseManagedArrayType(string type, out string elementType, out int rank)
    {
        elementType = ""; rank = 0;
        if (!TypeNameSyntax.TryParse(type, out var parsed)) return false;
        var suffixes = parsed.Suffixes;
        if (suffixes.LastOrDefault() is NullableTypeNameSuffix) suffixes = suffixes.Take(suffixes.Count - 1).ToArray();
        if (suffixes.LastOrDefault() is not ArrayRankTypeNameSuffix arrayRank) return false;
        elementType = (parsed with { Suffixes = suffixes.Take(suffixes.Count - 1).ToArray() }).ToString();
        rank = arrayRank.Rank;
        return elementType.Length > 0;
    }
    private static bool TryEvaluateInlineArrayLength(ReadOnlySpan<char> expression, out int length)
    {
        length = 0;
        if (!new InlineLengthParser(expression.ToString(), null).TryParse(out var value) || value < int.MinValue || value > int.MaxValue) return false;
        length = (int)value;
        return true;
    }
    private bool TryEvaluateInlineArrayLength(ReadOnlySpan<char> expression, TypeDeclaration context, out int length)
    {
        length = 0;
        if (!new InlineLengthParser(expression.ToString(), name => ResolveInlineArrayLengthName(name, context)).TryParse(out var value) ||
            value < int.MinValue || value > int.MaxValue) return false;
        length = (int)value;
        return true;
    }
    private BigInteger? ResolveInlineArrayLengthName(string name, TypeDeclaration context)
    {
        if (name.StartsWith("sizeof(", StringComparison.Ordinal) && name.EndsWith(')'))
        {
            var operandType = NormalizeEnumType(name[7..^1].Trim(), context);
            if (!SupportedTypes.Contains(operandType) && !_enumUnderlyingTypes.ContainsKey(operandType)) return null;
            var size = GetTypeSize(operandType);
            return size > 0 ? new BigInteger(size) : null;
        }
        var separator = name.LastIndexOf('.');
        if (separator < 0)
        {
            if (_constantFields.TryGetValue(GetQualifiedTypeName(context) + "." + name, out var localConstant))
                return ParseInlineArrayConstant(localConstant.Binding);
            if (context.Kind == "enum" && TryGetEnumConstant(context.Name, name, out var enumConstant))
                return ParseInlineArrayConstant(enumConstant);
            return null;
        }
        var typeName = name[..separator];
        var memberName = name[(separator + 1)..];
        var candidates = FindTypes(typeName, context);
        if (candidates.Length != 1 || !_constantFields.TryGetValue(GetQualifiedTypeName(candidates[0]) + "." + memberName, out var constant)) return null;
        return ParseInlineArrayConstant(constant.Binding);
    }
    private BigInteger? ParseInlineArrayConstant(ConstantFieldBinding binding)
    {
        var typeName = _enumUnderlyingTypes.GetValueOrDefault(binding.TypeName, binding.TypeName);
        if (!IntegerTypes.Contains(typeName) && typeName != "char") return null;
        return BigInteger.TryParse(binding.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private sealed class InlineLengthParser(string text, Func<string, BigInteger?>? resolveName)
    {
        private int _position;
        public bool TryParse(out BigInteger value)
        {
            value = BigInteger.Zero;
            try
            {
                value = ParseExpression(0);
                SkipWhitespace();
                return _position == text.Length;
            }
            catch (FormatException) { return false; }
            catch (DivideByZeroException) { return false; }
            catch (OverflowException) { return false; }
        }
        private BigInteger ParseExpression(int minimumPrecedence)
        {
            SkipWhitespace();
            BigInteger left;
            if (Eat("+")) left = ParseExpression(7);
            else if (Eat("-")) left = -ParseExpression(7);
            else if (Eat("~")) left = ~ParseExpression(7);
            else if (Eat("(")) { left = ParseExpression(0); if (!Eat(")")) throw new FormatException(); }
            else left = ParseLiteral();

            while (true)
            {
                SkipWhitespace();
                var (op, precedence) = CurrentOperator();
                if (op.Length == 0 || precedence < minimumPrecedence) return left;
                _position += op.Length;
                var right = ParseExpression(precedence + 1);
                left = op switch
                {
                    "|" => left | right,
                    "^" => left ^ right,
                    "&" => left & right,
                    "<<" when right >= 0 && right <= 31 => left << (int)right,
                    ">>" when right >= 0 && right <= 31 => left >> (int)right,
                    "+" => left + right,
                    "-" => left - right,
                    "*" => left * right,
                    "/" => left / right,
                    "%" => left % right,
                    "<<" or ">>" => throw new FormatException(),
                    _ => throw new FormatException()
                };
            }
        }
        private BigInteger ParseLiteral()
        {
            SkipWhitespace();
            if (text.AsSpan(_position).StartsWith("sizeof(", StringComparison.Ordinal))
            {
                var close = text.IndexOf(')', _position + "sizeof(".Length);
                if (close < 0) throw new FormatException();
                var type = text[(_position + "sizeof(".Length)..close].Trim();
                _position = close + 1;
                return resolveName?.Invoke("sizeof(" + type + ")") ?? throw new FormatException();
            }
            var start = _position;
            while (_position < text.Length && (char.IsLetterOrDigit(text[_position]) || text[_position] is '_' or '.')) _position++;
            if (start == _position) throw new FormatException();
            var token = text[start.._position];
            var literal = ParseInteger(token);
            if (literal.Value is { } integer) return integer;
            return resolveName?.Invoke(token) ?? throw new FormatException();
        }
        private (string Operator, int Precedence) CurrentOperator()
        {
            if (_position >= text.Length) return ("", -1);
            if (_position + 1 < text.Length && text.AsSpan(_position, 2).SequenceEqual("<<")) return ("<<", 4);
            if (_position + 1 < text.Length && text.AsSpan(_position, 2).SequenceEqual(">>")) return (">>", 4);
            return text[_position] switch
            {
                '|' => ("|", 1), '^' => ("^", 2), '&' => ("&", 3),
                '+' => ("+", 5), '-' => ("-", 5), '*' => ("*", 6), '/' => ("/", 6), '%' => ("%", 6),
                _ => ("", -1)
            };
        }
        private bool Eat(string token)
        {
            SkipWhitespace();
            if (!text.AsSpan(_position).StartsWith(token, StringComparison.Ordinal)) return false;
            _position += token.Length;
            return true;
        }
        private void SkipWhitespace() { while (_position < text.Length && char.IsWhiteSpace(text[_position])) _position++; }
    }
    private bool IsInlineArrayElementType(string type) => NumericTypes.Contains(type) || type == "bool" || IsEnumType(type) || IsStructType(type) || IsPointerType(type) && IsAllowedPointerType(type);
    private bool IsAllowedPointerType(string type)
    {
        if (!IsPointerType(type)) return false;
        var pointee = type[..^1];
        if (pointee == "void" || IsPointerType(pointee)) return pointee == "void" || IsAllowedPointerType(pointee);
        return SupportedTypes.Contains(pointee) && pointee is not ("void" or "string") ||
            _enumUnderlyingTypes.ContainsKey(pointee) || IsStructType(pointee) || IsClassType(pointee) || IsNullableType(pointee);
    }
    private void ValidateDeclaredType(string type, string role, SourceLocation location, TypeDeclaration? contextType = null)
    {
        if (contextType is not null) ValidateTypeAccessibility(type, contextType, location);
        var storageType = IsNullableArrayType(type) ? type[..^1] : type;
        if (type.EndsWith("?", StringComparison.Ordinal) && !IsNullableClassType(type) && !IsNullableDelegateType(type) && !IsNullableStringType(type) && !IsNullableArrayType(type) && !IsNullableValueType(type))
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Nullable type '{type}' is not yet supported by the current backend.", location);
            return;
        }
        if (TryParseInlineArrayType(storageType, out var elementType, out var count))
        {
            if (role is not ("Local" or "Struct field" or "Class field")) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Inline arrays are currently supported only as locals and class or struct fields.", location);
            if (count <= 0 || !IsInlineArrayElementType(elementType)) diagnostics.Add("LS1510", DiagnosticSeverity.Error, $"Inline array element type '{elementType}' or length '{count}' is invalid.", location);
        }
        else if (TryParseManagedArrayType(storageType, out var arrayElementType, out var arrayRank))
        {
            if (arrayRank > 3) diagnostics.Add("LS1503", DiagnosticSeverity.Error, "Rectangular arrays may have at most three dimensions.", location);
            else if (!IsManagedArrayElementType(arrayElementType)) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Managed arrays of element type '{arrayElementType}' are recognized but not yet supported by the current backend.", location);
            else if (role is not ("Local" or "Parameter" or "Return" or "Inline out local")) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Managed arrays are not yet supported as {role.ToLowerInvariant()}s.", location);
        }
        else if (IsPointerType(storageType) && !IsAllowedPointerType(storageType)) diagnostics.Add("LS1903", DiagnosticSeverity.Error, $"Pointer type '{type}' is not allowed by the current backend.", location);
        else if (!IsSupportedType(type)) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"{role} type '{type}' is not yet supported.", location);
    }
    private void ValidateExternType(string type, bool allowVoid, SourceLocation location)
    {
        if (allowVoid && type == "void" || NumericTypes.Contains(type) || type == "bool" || IsEnumType(type) || IsPointerType(type) && IsAllowedPointerType(type)) return;
        diagnostics.Add("LS2101", DiagnosticSeverity.Error, $"Type '{type}' does not have a supported C ABI representation for an extern signature.", location);
    }
    private bool IsEnumType(string type) => _enumUnderlyingTypes.ContainsKey(type);
    private bool IsStructType(string type) => _structTypes.ContainsKey(type);
    private string NormalizePointerMemberReceiverType(string pointerType, bool unsafeContext, SourceLocation location)
    {
        if (!IsPointerType(pointerType)) return pointerType;
        if (!UnsafeEnabledAt(location))
            diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Pointer member access requires unsafe code, but unsafe is disabled for this compilation.", location);
        else if (!unsafeContext)
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Pointer member access may only be used in an unsafe context.", location);

        var pointeeType = pointerType[..^1];
        if (pointeeType == "void")
        {
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A void pointer must be converted to a typed pointer before member access.", location);
            return "?";
        }
        if (IsPointerType(pointeeType))
        {
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Automatic member access dereferences one pointer level; explicitly dereference a pointer-to-pointer.", location);
            return "?";
        }
        return pointeeType;
    }
    private bool IsGenericValueType(string type) => NumericTypes.Contains(type) || type == "bool" || IsEnumType(type) || IsStructType(type) ||
        TryParseInlineArrayType(type, out _, out _);

    private bool StructContainsManagedReferences(string type) => StructContainsManagedReferences(type, new HashSet<string>(StringComparer.Ordinal));
    private bool StructContainsManagedReferences(string type, HashSet<string> visiting)
    {
        if (IsNullableValueType(type)) return StructContainsManagedReferences(NullableUnderlyingType(type), visiting);
        if (IsNullableStringType(type) || IsClassReferenceType(type) || IsNullableArrayType(type) || type is "string" or "object" ||
            TryParseManagedArrayType(type, out _, out _)) return true;
        if (TryParseInlineArrayType(type, out var inlineElementType, out _)) return StructContainsManagedReferences(inlineElementType, visiting);
        if (!_structTypes.TryGetValue(type, out var structure)) return false;
        if (!visiting.Add(type)) return true;
        var containsReferences = structure.Fields.Any(field => StructContainsManagedReferences(field.TypeName, visiting));
        visiting.Remove(type);
        return containsReferences;
    }
    private bool IsClassType(string type) => _classTypes.ContainsKey(type) || type == "object" && _classTypes.ContainsKey("System.Object");
    private bool IsDelegateType(string type) => _delegateTypes.ContainsKey(type);
    private bool IsNullableClassType(string type) => IsNullableTypeName(type) &&
        (_classTypes.ContainsKey(NullableUnderlyingType(type)) || NullableUnderlyingType(type) == "object" && _classTypes.ContainsKey("System.Object") || _interfaceTypes.ContainsKey(NullableUnderlyingType(type)));
    private bool IsNullableDelegateType(string type) => IsNullableTypeName(type) && _delegateTypes.ContainsKey(NullableUnderlyingType(type));
    private bool IsNullableArrayType(string type) =>
        TypeNameSyntax.TryParse(type, out var parsed) && parsed.Suffixes.LastOrDefault() is NullableTypeNameSuffix &&
        parsed.Suffixes.Take(parsed.Suffixes.Count - 1).LastOrDefault() is ArrayRankTypeNameSuffix;
    private bool IsManagedArrayReferenceType(string type) => IsNullableArrayType(type) || TryParseManagedArrayType(type, out _, out _);
    private static bool IsNullableStringType(string type) => type == "string?";
    private static bool IsStringType(string type) => type is "string" or "string?";
    private bool IsNullableValueType(string type)
    {
        if (!IsNullableTypeName(type)) return false;
        var underlying = NullableUnderlyingType(type);
        return underlying is not ("void" or "string") && (NumericTypes.Contains(underlying) || underlying == "bool" || _enumUnderlyingTypes.ContainsKey(underlying) || _structTypes.ContainsKey(underlying));
    }
    private bool IsNullableValuePayloadType(string type) => NumericTypes.Contains(type) || type == "bool" || _enumUnderlyingTypes.ContainsKey(type) || _structTypes.ContainsKey(type);
    private bool IsBoxableScalarOrEnum(string type) => NumericTypes.Contains(type) || type == "bool" || _enumUnderlyingTypes.ContainsKey(type);
    private bool IsBoxableValueType(string type) => IsBoxableScalarOrEnum(type) || IsStructType(type);
    private bool IsNullableType(string type) => IsNullableClassType(type) || IsNullableDelegateType(type) || IsNullableStringType(type) || IsNullableArrayType(type) || IsNullableValueType(type);
    private static string NullableUnderlyingType(string type) =>
        TypeNameSyntax.TryParse(type, out var parsed) && parsed.Suffixes.LastOrDefault() is NullableTypeNameSuffix
            ? (parsed with { Suffixes = parsed.Suffixes.Take(parsed.Suffixes.Count - 1).ToArray() }).ToString()
            : type.EndsWith("?", StringComparison.Ordinal) ? type[..^1] : type;
    private static bool IsNullableTypeName(string type) =>
        TypeNameSyntax.TryParse(type, out var parsed) && parsed.Suffixes.LastOrDefault() is NullableTypeNameSuffix ||
        type.EndsWith("?", StringComparison.Ordinal);

    private string RecordConditionalExpressionType(ConditionalExpression expression, string type)
    {
        _conditionalExpressionTypes[expression] = type;
        return type;
    }
    private bool IsClassReferenceType(string type) => IsClassType(type) || _interfaceTypes.ContainsKey(type) || IsNullableClassType(type);
    private bool IsScopeBoundReferenceType(string type) => IsClassReferenceType(type) ||
        IsDelegateType(NullableUnderlyingType(type)) || TryParseManagedArrayType(type, out _, out _);
    private string? GetClassReferenceTypeForAnalysis(string type) => IsClassReferenceType(type) || IsDelegateType(type) || IsNullableDelegateType(type)
        ? NullableUnderlyingType(type)
        : TryParseManagedArrayType(type, out _, out _) ? "System.Array" : IsStringType(type) ? "System.Object" : null;

    private TypeDeclaration? ResolveGenericInstance(string type, TypeDeclaration context)
    {
        if (!TrySplitGenericType(type, out var definitionName, out var arguments)) return null;
        var contextName = GetQualifiedTypeName(context);
        if (!_genericTypeResolutionsInProgress.Add((type, contextName))) return null;
        try
        {
            var definition = GenericTypeSpecializer.ResolveDefinition(definitionName, context, _genericDefinitions);
            if (definition is null) return null;
            var requestedArguments = arguments.Select(argument => NormalizeEnumType(argument, context)).ToArray();
            foreach (var instance in _genericTypeInstantiations)
            {
                if (GetQualifiedTypeName(instance.Definition) != GetQualifiedTypeName(definition) || instance.Arguments.Count != arguments.Count) continue;
                var instantiatedArguments = instance.Arguments.Select(argument => NormalizeEnumType(argument, instance.RequestContext)).ToArray();
                if (requestedArguments.SequenceEqual(instantiatedArguments, StringComparer.Ordinal)) return instance.Specialized;
            }
            if (definition.Kind == "delegate" && _genericTypeSpecializer?.EnsureClosedDelegate(definitionName, requestedArguments, context, context.Location) is { } closedDelegate)
            {
                if (!_genericTypeInstantiations.Any(instance => ReferenceEquals(instance.Specialized, closedDelegate.Specialized)))
                    _genericTypeInstantiations.Add(closedDelegate);
                var qualifiedName = GetQualifiedTypeName(closedDelegate.Specialized);
                _delegateTypes[qualifiedName] = NormalizeDelegateType(closedDelegate.Specialized);
                return closedDelegate.Specialized;
            }
            return null;
        }
        finally { _genericTypeResolutionsInProgress.Remove((type, contextName)); }
    }

    private static bool TrySplitGenericType(string type, out string definitionName, out IReadOnlyList<string> arguments)
    {
        definitionName = string.Empty;
        arguments = [];
        if (!TypeNameSyntax.TryParse(type, out var parsed) || parsed.Arguments.Count == 0 || parsed.Suffixes.Count != 0)
            return false;
        definitionName = parsed.Name;
        arguments = parsed.Arguments.Select(argument => argument.ToString()).ToArray();
        return true;
    }

    private string NormalizeEnumType(string type, TypeDeclaration context)
    {
        if (type.EndsWith("?", StringComparison.Ordinal)) return NormalizeEnumType(type[..^1], context) + "?";
        if (type is "object" or "System.Object") return "System.Object";
        if (context.Usings?.FirstOrDefault(directive => directive.Alias == type && directive.Target.Contains('<', StringComparison.Ordinal)) is { } constructedTypeAlias)
            return NormalizeEnumType(constructedTypeAlias.Target, context);
        if (IsPointerType(type)) return NormalizeEnumType(type[..^1], context) + "*";
        if (TryGetInlineArrayParts(type, out var elementType, out var lengthExpression))
        {
            var length = TryEvaluateInlineArrayLength(lengthExpression.AsSpan(), context, out var resolvedLength)
                ? resolvedLength
                : TryEvaluateInlineArrayLength(lengthExpression.AsSpan(), out var literalLength) ? literalLength : 0;
            var normalizedLength = length > 0 ? length.ToString(System.Globalization.CultureInfo.InvariantCulture) : lengthExpression;
            return NormalizeEnumType(elementType, context) + $"[{normalizedLength}]";
        }
        if (TryParseManagedArrayType(type, out var managedElementType, out var managedRank)) return NormalizeEnumType(managedElementType, context) + "[" + new string(',', managedRank - 1) + "]";
        if (ResolveGenericInstance(type, context) is { } genericInstance) return GetQualifiedTypeName(genericInstance);
        if (type.Contains('.', StringComparison.Ordinal) && _enumUnderlyingTypes.ContainsKey(type)) return type;
        var matches = FindTypes(type, context).Where(candidate => candidate.Kind is "enum" or "struct" or "class" or "interface" or "delegate").ToArray();
        return matches.Length == 1 ? GetQualifiedTypeName(matches[0]) : type;
    }

    private TypeDeclaration NormalizeDelegateType(TypeDeclaration declaration) => declaration with
    {
        DelegateReturnType = declaration.DelegateReturnType is null ? null : NormalizeEnumType(declaration.DelegateReturnType, declaration),
        DelegateParameters = (declaration.DelegateParameters ?? []).Select(parameter => parameter with
        {
            TypeName = NormalizeEnumType(parameter.TypeName, declaration)
        }).ToArray()
    };

    private bool TryBindStaticMethodGroup(NameExpression expression, string expectedType, MethodContext context) =>
        TryBindStaticMethodGroup(expression, expression.Name, expectedType, context, context.Method.Owner);

    private bool TryBindMethodGroup(Expression expression, string expectedType, MethodContext context) => expression switch
    {
        NameExpression name => TryBindStaticMethodGroup(name, expectedType, context) || TryBindUnqualifiedInstanceMethodGroup(name, expectedType, context),
        MemberAccessExpression member => TryBindStaticMethodGroup(member, expectedType, context),
        _ => false
    };

    private bool TryBindStaticMethodGroup(MemberAccessExpression expression, string expectedType, MethodContext context)
    {
        if (TryFlattenName(expression.Receiver, out var inaccessibleQualifier) &&
            IsInaccessibleTypeReference(inaccessibleQualifier, context.Method.Owner))
        {
            ValidateTypeAccessibility(inaccessibleQualifier, context.Method.Owner, expression.Receiver.Location);
            return true;
        }
        if (IsTypeReference(expression.Receiver, context) && TryFlattenName(expression.Receiver, out var requestedType))
        {
            var matchingTypes = FindTypes(NormalizeBuiltinTypeQualifier(NormalizeEnumType(requestedType, context.Method.Owner)), context.Method.Owner);
            if (matchingTypes.Length == 1)
                return TryBindStaticMethodGroup(expression, expression.Member, expectedType, context, matchingTypes[0]);
            return false;
        }

        var receiverType = AnalyzeExpression(expression.Receiver, context);
        var receiverBaseType = NullableUnderlyingType(receiverType);
        var receiverClassChain = _classTypes.ContainsKey(receiverBaseType)
            ? GetClassChain(receiverBaseType).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        var instanceCandidates = _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.Declaration.Name == expression.Member &&
            (candidate.IsClassInstanceMethod && receiverClassChain.Contains(GetQualifiedTypeName(candidate.Owner)) ||
             candidate.IsStructInstanceMethod && NormalizeEnumType(receiverType, context.Method.Owner) == GetQualifiedTypeName(candidate.Owner))).ToList();
        if (expression.Member == "ToString" && instanceCandidates.Count == 0 && receiverClassChain.Contains("System.Object"))
            instanceCandidates.Add(VirtualDispatchLayout.ObjectToString);
        if (instanceCandidates.Count == 0) return false;
        if (expression.IsNullConditional)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "A null-conditional method group cannot be converted to a delegate.", expression.Location);
            return true;
        }
        if (GetScopeBoundDepth(expression.Receiver, context) is not null)
            diagnostics.Add("LS0702", DiagnosticSeverity.Error, "A delegate cannot retain a scope-bound method-group receiver beyond its lifetime.", expression.Location);
        else if (IsReadOnlyReferenceExpression(expression.Receiver, context))
            diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A delegate cannot retain a readonly method-group receiver beyond its lifetime.", expression.Location);
        var isStructReceiver = _structTypes.ContainsKey(receiverBaseType);
        if (!isStructReceiver && !_classTypes.ContainsKey(receiverBaseType)) return false;
        if (!isStructReceiver && receiverType.EndsWith("?", StringComparison.Ordinal) && !IsDefinitelyNotNull(expression.Receiver, receiverType, context))
            diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable method-group receiver may be null; check or assert it before creating the delegate.", expression.Location);
        var accessible = instanceCandidates.Where(candidate => IsMethodAccessible(candidate, context.Method.Owner)).ToArray();
        if (accessible.Length == 0)
        {
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Instance method '{expression.Member}' is not accessible from this type.", expression.Location);
            return true;
        }
        return BindInstanceMethodGroup(expression, expression.Member, expectedType, context, accessible, receiverBaseType);
    }

    private bool TryBindUnqualifiedInstanceMethodGroup(NameExpression expression, string expectedType, MethodContext context)
    {
        if (context.Lookup(expression.Name) is not null || !(context.Method.IsClassInstanceMethod || context.Method.IsClassConstructor)) return false;
        var currentType = GetQualifiedTypeName(context.Method.Owner);
        var classChain = _classTypes.ContainsKey(currentType) ? GetClassChain(currentType).ToHashSet(StringComparer.Ordinal) : new HashSet<string>(StringComparer.Ordinal);
        var candidates = _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.IsClassInstanceMethod && candidate.Declaration.Name == expression.Name &&
            classChain.Contains(GetQualifiedTypeName(candidate.Owner))).ToList();
        if (expression.Name == "ToString" && candidates.Count == 0 && classChain.Contains("System.Object"))
            candidates.Add(VirtualDispatchLayout.ObjectToString);
        if (candidates.Count == 0) return false;
        var accessible = candidates.Where(candidate => IsMethodAccessible(candidate, context.Method.Owner)).ToArray();
        if (accessible.Length == 0)
        {
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Instance method '{expression.Name}' is not accessible from this type.", expression.Location);
            return true;
        }
        return BindInstanceMethodGroup(expression, expression.Name, expectedType, context, accessible, currentType);
    }

    private bool BindInstanceMethodGroup(Expression expression, string methodName, string expectedType, MethodContext context,
        IEnumerable<MethodSymbol> candidates, string virtualReceiverType)
    {
        var delegateName = NullableUnderlyingType(NormalizeEnumType(expectedType, context.Method.Owner));
        if (!_delegateTypes.TryGetValue(delegateName, out var delegateType) || delegateType.DelegateReturnType is null) return false;
        var delegateParameters = (delegateType.DelegateParameters ?? []).Select(parameter => parameter with
        {
            TypeName = NormalizeEnumType(parameter.TypeName, delegateType)
        }).ToArray();
        var delegateReturn = NormalizeEnumType(delegateType.DelegateReturnType, delegateType);
        var compatible = GetCompatibleMethodGroupCandidates(candidates, delegateParameters, delegateReturn, context,
            expression.Location, out var constraintFailures);
        if (compatible.Length != 1)
        {
            if (compatible.Length == 0 && constraintFailures.Count > 0)
            {
                foreach (var failure in constraintFailures)
                    diagnostics.Add(failure.Code, failure.Severity, failure.Message, failure.Location);
                return true;
            }
            diagnostics.Add(compatible.Length == 0 ? "LMC3010" : "LS0750", DiagnosticSeverity.Error,
                compatible.Length == 0
                    ? $"Instance method group '{methodName}' is not compatible with delegate '{delegateName}'."
                    : $"Instance method group '{methodName}' is ambiguous for delegate '{delegateName}'.",
                expression.Location);
            return true;
        }
        var receiver = expression is MemberAccessExpression member ? member.Receiver : new ThisExpression(expression.Location);
        if (compatible[0].Declaration.IsMutating && (HasThisRoot(receiver) || IsCapturedThisRoot(receiver, context)) && !CanMutateClassReceiver(context))
            diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A non-mutating class method cannot create a delegate to a mutating method on its receiver.", expression.Location);
        var isExplicitBase = expression is MemberAccessExpression { Receiver: BaseExpression };
        var virtualSlot = !isExplicitBase && (compatible[0].Declaration.IsVirtual || compatible[0].Declaration.IsOverride)
            ? VirtualDispatchLayout.FindSlot(virtualReceiverType, compatible[0], _classTypes, _methods)
            : -1;
        _delegateMethodGroups[expression] = new(delegateName, compatible[0], IsInstance: true,
            VirtualSlot: virtualSlot >= 0 ? virtualSlot : null);
        return true;
    }

    private bool TryBindStaticMethodGroup(Expression expression, string methodName, string expectedType, MethodContext context, TypeDeclaration owner)
    {
        if (expression is NameExpression name && context.Lookup(name.Name) is not null) return false;
        var delegateName = NullableUnderlyingType(NormalizeEnumType(expectedType, context.Method.Owner));
        if (!_delegateTypes.TryGetValue(delegateName, out var delegateType) || delegateType.DelegateReturnType is null) return false;
        var allNamedMethods = _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.Owner == owner &&
            candidate.Declaration.IsStatic && candidate.Declaration.Name == methodName &&
            IsMethodAccessible(candidate, context.Method.Owner)).ToArray();
        if (allNamedMethods.Length == 0) return false;
        var delegateParameters = (delegateType.DelegateParameters ?? []).Select(parameter => parameter with
        {
            TypeName = NormalizeEnumType(parameter.TypeName, delegateType)
        }).ToArray();
        var delegateReturn = NormalizeEnumType(delegateType.DelegateReturnType, delegateType);
        var compatible = GetCompatibleMethodGroupCandidates(allNamedMethods, delegateParameters, delegateReturn, context,
            expression.Location, out var constraintFailures);
        if (compatible.Length != 1)
        {
            if (compatible.Length == 0 && constraintFailures.Count > 0)
            {
                foreach (var failure in constraintFailures)
                    diagnostics.Add(failure.Code, failure.Severity, failure.Message, failure.Location);
                return true;
            }
            diagnostics.Add(compatible.Length == 0 ? "LMC3010" : "LS0750", DiagnosticSeverity.Error,
                compatible.Length == 0
                ? $"Static method group '{methodName}' is not compatible with delegate '{delegateName}'."
                    : $"Static method group '{methodName}' is ambiguous for delegate '{delegateName}'.",
                expression.Location);
            return true;
        }
        _delegateMethodGroups[expression] = new(delegateName, compatible[0]);
        return true;
    }

    private MethodSymbol[] GetCompatibleMethodGroupCandidates(IEnumerable<MethodSymbol> candidates,
        IReadOnlyList<ParameterDeclaration> delegateParameters, string delegateReturn, MethodContext context,
        SourceLocation location, out IReadOnlyList<Diagnostic> constraintFailures)
    {
        var compatible = new List<MethodSymbol>();
        var failures = new List<Diagnostic>();
        foreach (var candidate in candidates)
        {
            MethodSymbol? method;
            Diagnostic? failure = null;
            if (candidate.Declaration.HasTypeParameters)
                method = TrySpecializeGenericMethodGroup(candidate, delegateParameters, delegateReturn, context, location, out failure);
            else method = candidate;
            if (failure is not null) failures.Add(failure);
            if (method is null || method.Declaration.Parameters.Count != delegateParameters.Count) continue;
            var parametersMatch = method.Declaration.Parameters
                .Select(parameter => parameter with { TypeName = NormalizeEnumType(parameter.TypeName, method.Owner) })
                .Zip(delegateParameters)
                .All(pair => pair.First.TypeName == pair.Second.TypeName && pair.First.PassingMode == pair.Second.PassingMode);
            if (parametersMatch && CanConvert(NormalizeEnumType(method.Declaration.ReturnType, method.Owner), delegateReturn, null))
                compatible.Add(method);
        }
        constraintFailures = failures.Distinct().ToArray();
        var distinctCompatible = compatible.Distinct().ToArray();
        var nonGeneric = distinctCompatible.Where(method => !method.IsGenericMethodSpecialization &&
            !method.Declaration.HasTypeParameters).ToArray();
        if (nonGeneric.Length == 1 && distinctCompatible.All(method => ReferenceEquals(method, nonGeneric[0]) ||
            method.IsGenericMethodSpecialization && HasSameMethodGroupSignature(method, nonGeneric[0])))
            return nonGeneric;
        return distinctCompatible;
    }

    private bool HasSameMethodGroupSignature(MethodSymbol first, MethodSymbol second) =>
        NormalizeEnumType(first.Declaration.ReturnType, first.Owner) == NormalizeEnumType(second.Declaration.ReturnType, second.Owner) &&
        first.Declaration.Parameters.Count == second.Declaration.Parameters.Count &&
        first.Declaration.Parameters.Select(parameter => parameter with { TypeName = NormalizeEnumType(parameter.TypeName, first.Owner) })
            .Zip(second.Declaration.Parameters.Select(parameter => parameter with { TypeName = NormalizeEnumType(parameter.TypeName, second.Owner) }))
            .All(pair => pair.First.TypeName == pair.Second.TypeName && pair.First.PassingMode == pair.Second.PassingMode);

    private MethodSymbol? TrySpecializeGenericMethodGroup(MethodSymbol candidate,
        IReadOnlyList<ParameterDeclaration> delegateParameters, string delegateReturn, MethodContext context,
        SourceLocation location, out Diagnostic? constraintFailure)
    {
        constraintFailure = null;
        var typeParameters = candidate.Declaration.TypeParameters ?? [];
        if (typeParameters.Count == 0 || candidate.Declaration.Parameters.Count != delegateParameters.Count) return null;
        var inferred = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < delegateParameters.Count; index++)
        {
            var methodParameter = candidate.Declaration.Parameters[index];
            var delegateParameter = delegateParameters[index];
            if (methodParameter.PassingMode != delegateParameter.PassingMode ||
                !TryInferExactTypeParameter(NormalizeEnumType(methodParameter.TypeName, candidate.Owner), delegateParameter.TypeName,
                    typeParameters, inferred, candidate.Owner)) return null;
        }

        var methodReturnPattern = NormalizeEnumType(candidate.Declaration.ReturnType, candidate.Owner);
        if (methodReturnPattern != "void" && delegateReturn != "void" &&
            GenericTypeSpecializer.ReferencesAnyTypeParameter(methodReturnPattern, typeParameters.ToHashSet(StringComparer.Ordinal)) &&
            !TryInferExactTypeParameter(methodReturnPattern, delegateReturn, typeParameters, inferred, candidate.Owner)) return null;
        if (typeParameters.Any(parameter => !inferred.ContainsKey(parameter))) return null;

        foreach (var (methodParameter, delegateParameter) in candidate.Declaration.Parameters.Zip(delegateParameters))
        {
            var specializedParameterType = GenericTypeSpecializer.RewriteTypeName(
                NormalizeEnumType(methodParameter.TypeName, candidate.Owner), inferred);
            if (GenericTypeSpecializer.ReferencesAnyTypeParameter(specializedParameterType, typeParameters.ToHashSet(StringComparer.Ordinal)) ||
                specializedParameterType != delegateParameter.TypeName) return null;
        }
        var specializedReturnType = GenericTypeSpecializer.RewriteTypeName(methodReturnPattern, inferred);
        if (methodReturnPattern == "void" ? delegateReturn != "void" :
            delegateReturn == "void" || !CanConvert(specializedReturnType, delegateReturn, null)) return null;

        var substitutions = typeParameters.Zip(typeParameters.Select(parameter => inferred[parameter]))
            .ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
        foreach (var clause in candidate.Declaration.GenericConstraints ?? [])
        {
            var parameterIndex = Array.FindIndex(typeParameters.ToArray(), parameter => parameter == clause.TypeParameter);
            if (parameterIndex < 0) return null;
            foreach (var rawConstraint in clause.Constraints)
            {
                var constraint = GenericTypeSpecializer.RewriteTypeName(rawConstraint, substitutions);
                var normalizedConstraint = constraint is "class" or "struct" or "enum" || constraint.StartsWith("new(", StringComparison.Ordinal)
                    ? constraint
                    : NormalizeEnumType(constraint, candidate.Owner);
                if (!SatisfiesGenericConstraint(inferred[clause.TypeParameter], normalizedConstraint, context.Method.Owner))
                {
                    constraintFailure = new("LS1102", DiagnosticSeverity.Error,
                        $"Type argument '{inferred[clause.TypeParameter]}' does not satisfy constraint '{rawConstraint}' for type parameter '{clause.TypeParameter}' on method '{candidate.Declaration.Name}'.",
                        location);
                    return null;
                }
            }
        }
        return SpecializeGenericMethod(candidate, typeParameters.Select(parameter => inferred[parameter]).ToArray(), context, location);
    }

    private string AnalyzeDelegateInvocation(CallExpression call, TypeDeclaration delegateType, MethodContext context)
    {
        var parameters = (delegateType.DelegateParameters ?? []).Select(parameter => parameter with
        {
            TypeName = NormalizeEnumType(parameter.TypeName, delegateType)
        }).ToArray();
        if (call.Arguments.Count != parameters.Length)
            diagnostics.Add("LS0751", DiagnosticSeverity.Error, $"Delegate '{GetQualifiedTypeName(delegateType)}' requires {parameters.Length} arguments, but received {call.Arguments.Count}.", call.Location);

        var parameterBySource = new int[call.Arguments.Count];
        var supplied = new bool[parameters.Length];
        var sawNamed = false;
        for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
        {
            var argumentName = call.ArgumentNames is not null && sourceIndex < call.ArgumentNames.Count ? call.ArgumentNames[sourceIndex] : null;
            int parameterIndex;
            if (argumentName is not null)
            {
                sawNamed = true;
                parameterIndex = Array.FindIndex(parameters, parameter => parameter.Name == argumentName);
                if (parameterIndex < 0)
                {
                    diagnostics.Add("LS0740", DiagnosticSeverity.Error, $"Delegate '{GetQualifiedTypeName(delegateType)}' has no parameter named '{argumentName}'.", call.Arguments[sourceIndex].Location);
                    parameterBySource[sourceIndex] = Math.Min(sourceIndex, Math.Max(parameters.Length - 1, 0));
                    _ = AnalyzeExpression(call.Arguments[sourceIndex], context);
                    continue;
                }
            }
            else
            {
                if (sawNamed) diagnostics.Add("LS0740", DiagnosticSeverity.Error, "Positional arguments cannot follow named arguments.", call.Arguments[sourceIndex].Location);
                parameterIndex = sourceIndex;
                if (parameterIndex >= parameters.Length)
                {
                    parameterBySource[sourceIndex] = Math.Max(parameters.Length - 1, 0);
                    _ = AnalyzeExpression(call.Arguments[sourceIndex], context);
                    continue;
                }
            }
            parameterBySource[sourceIndex] = parameterIndex;
            if (supplied[parameterIndex]) diagnostics.Add("LS0740", DiagnosticSeverity.Error, $"Delegate parameter '{parameters[parameterIndex].Name}' is supplied more than once.", call.Arguments[sourceIndex].Location);
            supplied[parameterIndex] = true;
            var parameter = parameters[parameterIndex];
            var actualType = AnalyzeExpression(call.Arguments[sourceIndex], context, parameter.TypeName);
            var mode = call.ArgumentModes is not null && sourceIndex < call.ArgumentModes.Count ? call.ArgumentModes[sourceIndex] ?? "" : "";
            if (parameter.PassingMode is "ref" or "out")
            {
                if (mode != parameter.PassingMode || actualType != parameter.TypeName || !IsMutableVariableArgument(call.Arguments[sourceIndex], context))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Delegate argument for '{parameter.Name}' must use '{parameter.PassingMode}' with a mutable '{parameter.TypeName}' variable.", call.Arguments[sourceIndex].Location);
            }
            else if (mode is "ref" or "out" || parameter.PassingMode == "in" && mode is not ("" or "in") || parameter.PassingMode != "in" && mode == "in" ||
                !CanConvert(actualType, parameter.TypeName, call.Arguments[sourceIndex]))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert delegate argument from '{actualType}' to '{parameter.TypeName}'.", call.Arguments[sourceIndex].Location);
        }
        if (supplied.Any(value => !value)) diagnostics.Add("LS0751", DiagnosticSeverity.Error, $"Not all parameters of delegate '{GetQualifiedTypeName(delegateType)}' were supplied.", call.Location);
        _delegateInvocations[call] = delegateType;
        _callArguments[call] = new(parameterBySource, new Expression?[parameters.Length]);
        return NormalizeEnumType(delegateType.DelegateReturnType ?? "void", delegateType);
    }

    private void BindTypeExpressions(Expression expression, TypeDeclaration context)
    {
        switch (expression)
        {
            case SizeOfExpression sizeOf: _sizeOfTypes[sizeOf] = NormalizeEnumType(sizeOf.TypeName, context); break;
            case DefaultValueExpression defaultValue when defaultValue.TypeName is not null: _defaultTypes[defaultValue] = NormalizeEnumType(defaultValue.TypeName, context); break;
            case CastExpression cast: _castTypes[cast] = NormalizeEnumType(cast.TypeName, context); BindTypeExpressions(cast.Operand, context); break;
            case UnaryExpression unary: BindTypeExpressions(unary.Operand, context); break;
            case BinaryExpression binary: BindTypeExpressions(binary.Left, context); BindTypeExpressions(binary.Right, context); break;
            case ConditionalExpression conditional: BindTypeExpressions(conditional.Condition, context); BindTypeExpressions(conditional.WhenTrue, context); BindTypeExpressions(conditional.WhenFalse, context); break;
            case OverflowContextExpression overflow: BindTypeExpressions(overflow.Value, context); break;
        }
    }

    private void AnalyzeConstantFields(IEnumerable<(TypeDeclaration Owner, ConstantFieldDeclaration Declaration)> fields)
    {
        var declarations = fields.ToArray();
        foreach (var (owner, field) in declarations)
            _pendingConstantFields.TryAdd(GetQualifiedTypeName(owner) + "." + field.Name, (owner, field));
        foreach (var (owner, field) in declarations)
        {
            var previousConstantContext = _enumEvaluationOwner; _enumEvaluationOwner = owner;
            try { AnalyzeConstantField(owner, field); }
            finally { _enumEvaluationOwner = previousConstantContext; }
        }
    }

    private void AnalyzeConstantField(TypeDeclaration owner, ConstantFieldDeclaration field)
    {
        var key = GetQualifiedTypeName(owner) + "." + field.Name;
        if (!_constantFieldDeclarationsVisited.Add(field)) return;
        if (_pendingConstantFields.TryGetValue(key, out var primary) && !ReferenceEquals(primary.Declaration, field))
        {
            diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Constant field '{key}' is declared more than once.", field.Location);
            return;
        }
        if (_constantFieldsEvaluated.Contains(key)) return;
        if (!_constantFieldsBeingEvaluated.Add(key))
        {
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Constant field dependency cycle includes '{key}'.", field.Location);
            return;
        }
        try
        {
            BindTypeExpressions(field.Initializer, owner);
            if (!ValidateConstantStringLiterals(field.Initializer)) return;
            if (_constantFields.ContainsKey(key))
            {
                diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Constant field '{key}' is declared more than once.", field.Location);
                return;
            }
            if (field.TypeName is not ("bool" or "char" or "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double" or "string"))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Constant field type '{field.TypeName}' is not supported by the current backend.", field.Location);
                return;
            }
            if (field.Initializer is DefaultValueExpression defaultValue)
                _defaultTypes[defaultValue] = defaultValue.TypeName ?? field.TypeName;
            var initializerType = ConstantExpressionType(field.Initializer);
            if (initializerType is null && field.Initializer is DefaultValueExpression { TypeName: null }) initializerType = field.TypeName;
            if (initializerType is null)
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Constant field initializer must be a supported constant expression.", field.Initializer.Location);
                return;
            }
            if (!TryEvaluateConstant(field.Initializer, out var constant, out var evaluationError))
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, evaluationError ?? "Constant field initializer could not be evaluated at compile time.", field.Initializer.Location);
                return;
            }
            if (!TryConvertConstant(constant, field.TypeName, field.Initializer, out var converted))
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert constant value '{initializerType}' to '{field.TypeName}'.", field.Initializer.Location);
                return;
            }
            _constantFields.Add(key, (owner, new(field.TypeName, converted, constant.Text), GetFieldAccessibility(field)));
        }
        finally
        {
            _constantFieldsBeingEvaluated.Remove(key);
            _constantFieldsEvaluated.Add(key);
        }
    }

    private bool ValidateConstantStringLiterals(Expression expression) => expression switch
    {
        LiteralExpression literal when literal.Value.StartsWith('"') || literal.Value.StartsWith('@') => TryInternStringLiteral(literal, out _),
        UnaryExpression unary => ValidateConstantStringLiterals(unary.Operand),
        CastExpression cast => ValidateConstantStringLiterals(cast.Operand),
        BinaryExpression binary => ValidateConstantStringLiterals(binary.Left) && ValidateConstantStringLiterals(binary.Right),
        ConditionalExpression conditional => ValidateConstantStringLiterals(conditional.Condition) && ValidateConstantStringLiterals(conditional.WhenTrue) && ValidateConstantStringLiterals(conditional.WhenFalse),
        OverflowContextExpression overflow => ValidateConstantStringLiterals(overflow.Value),
        _ => true
    };

    private void AnalyzeStructFields(IEnumerable<(TypeDeclaration Owner, FieldDeclaration Declaration)> declarations)
    {
        var grouped = declarations.GroupBy(item => GetQualifiedTypeName(item.Owner), StringComparer.Ordinal);
        foreach (var group in grouped)
        {
            var fields = new List<StructFieldSymbol>();
            var typeDeclaration = group.First().Owner;
            var existingStaticNames = typeDeclaration.Members.OfType<FieldDeclaration>().Where(field => field.IsStatic).Select(field => field.Name)
                .Concat(typeDeclaration.Members.OfType<ConstantFieldDeclaration>().Select(field => field.Name));
            var names = new HashSet<string>(existingStaticNames, StringComparer.Ordinal);
            foreach (var (owner, declaration) in group)
            {
                if (!names.Add(declaration.Name) || _constantFields.ContainsKey(group.Key + "." + declaration.Name))
                {
                    diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Struct field '{group.Key}.{declaration.Name}' is declared more than once.", declaration.Location);
                    continue;
                }
                var fieldType = NormalizeEnumType(declaration.TypeName, owner);
                ValidateTypeAccessibility(declaration.TypeName, owner, declaration.Location);
                if (IsNonNullableReferenceField(fieldType, owner))
                    diagnostics.Add("LS0902", DiagnosticSeverity.Error, $"Struct field '{declaration.Name}' cannot have non-nullable reference type '{fieldType}'.", declaration.Location);
                else ValidateDeclaredType(fieldType, "Struct field", declaration.Location, owner);
                fields.Add(new(owner, declaration, fieldType, fields.Count));
            }
            if (_structTypes.TryGetValue(group.Key, out var structure))
                _structTypes[group.Key] = structure with { Fields = fields };
        }

        foreach (var (name, structure) in _structTypes.ToArray())
        {
            if (structure.Fields.All(field => IsSupportedType(field.TypeName)) &&
                !TryGetTypeLayout(name, [], out _, out _))
                diagnostics.Add("LS0900", DiagnosticSeverity.Error, $"Struct '{name}' has recursive or unrepresentable by-value containment.", structure.Declaration.Location);
        }
    }

    private void AnalyzeClassInheritance()
    {
        foreach (var (qualifiedName, symbol) in _classTypes.ToArray())
        {
            if (qualifiedName == "System.Object") continue;
            var bases = symbol.Declaration.BaseTypes ?? [];
            var inaccessibleBases = bases.Where(baseName =>
                ValidateTypeAccessibility(baseName, symbol.Declaration, symbol.Declaration.Location)).ToHashSet(StringComparer.Ordinal);
            var classBases = bases.Where(baseName => !inaccessibleBases.Contains(baseName) && baseName != "object" &&
                !ResolveInterfaceTypes(baseName, symbol.Declaration).Any()).ToArray();
            if (classBases.Length > 1)
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Class '{qualifiedName}' may have only one base class.", symbol.Declaration.Location);
            if (classBases.Length == 0)
            {
                if (!symbol.Declaration.IsStatic)
                    _classTypes[qualifiedName] = symbol with { BaseTypeName = "System.Object" };
                continue;
            }
            var baseName = classBases[0];
            // Base lists can name constructed generic classes. Resolve those through
            // the same normalization path used by ordinary type references so the
            // closed specialization participates in inheritance and member lookup.
            var matches = FindTypes(NormalizeEnumType(baseName, symbol.Declaration), symbol.Declaration)
                .Where(candidate => candidate.Kind == "class").ToArray();
            if (matches.Length != 1)
            {
                diagnostics.Add(matches.Length > 1 ? "LS2001" : "LS0200", DiagnosticSeverity.Error,
                    matches.Length > 1 ? $"Base type name '{baseName}' is ambiguous." : $"Base class '{baseName}' does not exist or is not a class.", symbol.Declaration.Location);
                continue;
            }
            var resolvedBase = GetQualifiedTypeName(matches[0]);
            if (matches[0].IsStatic)
                diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Static class '{resolvedBase}' cannot be used as a base class.", symbol.Declaration.Location);
            if (!matches[0].IsVirtual)
                diagnostics.Add("LS0830", DiagnosticSeverity.Error, $"Class '{qualifiedName}' cannot derive from non-virtual class '{resolvedBase}'.", symbol.Declaration.Location);
            _classTypes[qualifiedName] = symbol with { BaseTypeName = resolvedBase };
        }

        foreach (var type in _types.Where(type => type.Kind is "struct" or "interface"))
        {
            var hasValueTypeBase = false;
            foreach (var baseType in type.BaseTypes ?? [])
            {
                ValidateTypeAccessibility(baseType, type, type.Location);
                if (type.Kind != "struct") continue;
                var candidates = FindTypes(NormalizeEnumType(baseType, type), type);
                var isValueTypeBase = candidates.Any(candidate => candidate.Kind == "struct" && GetQualifiedTypeName(candidate) == "System.ValueType");
                var isInterfaceBase = candidates.Any(candidate => candidate.Kind == "interface");
                if (isValueTypeBase)
                {
                    if (hasValueTypeBase)
                        diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A struct may declare System.ValueType only once.", type.Location);
                    hasValueTypeBase = true;
                }
                else if (!isInterfaceBase)
                    diagnostics.Add("LS0001", DiagnosticSeverity.Error,
                        "A struct may implement interfaces and may declare System.ValueType as its special base, but cannot derive from another class.", type.Location);
            }
        }

        foreach (var name in _classTypes.Keys)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var current = name;
            while (_classTypes.TryGetValue(current, out var symbol) && symbol.BaseTypeName is { } baseName)
            {
                if (!seen.Add(current) || baseName == name || seen.Contains(baseName))
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Class inheritance cycle detected at '{name}'.", symbol.Declaration.Location);
                    _classTypes[name] = _classTypes[name] with { BaseTypeName = null };
                    break;
                }
                current = baseName;
            }
        }
    }

    private static string StripGenericArguments(string typeName)
    {
        var open = typeName.IndexOf('<');
        return open < 0 ? typeName : typeName[..open];
    }

    private TypeDeclaration[] ResolveInterfaceTypes(string typeName, TypeDeclaration context) =>
        FindTypes(NormalizeEnumType(typeName, context), context).Where(candidate => candidate.Kind == "interface").ToArray();

    private TypeDeclaration[] GetInterfaceHierarchy(TypeDeclaration root)
    {
        var hierarchy = new List<TypeDeclaration>();
        var pending = new Queue<TypeDeclaration>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        pending.Enqueue(root);
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            if (!visited.Add(GetQualifiedTypeName(current))) continue;
            hierarchy.Add(current);
            foreach (var baseType in current.BaseTypes ?? [])
                foreach (var inherited in ResolveInterfaceTypes(baseType, current)) pending.Enqueue(inherited);
        }
        return hierarchy.ToArray();
    }

    private void BuildInterfaceContractLayout()
    {
        var slots = new List<InterfaceMethodSlot>();
        foreach (var (interfaceName, interfaceType) in _interfaceTypes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (interfaceType.HasTypeParameters) continue;
            foreach (var contract in GetDeclaredInterfaceContracts(interfaceType)
                         .Where(method => !method.Declaration.IsStatic && !method.Declaration.HasTypeParameters)
                         .OrderBy(method => method.Declaration.Name, StringComparer.Ordinal)
                         .ThenBy(method => string.Join("\u001f", method.Declaration.Parameters.Select(parameter => parameter.TypeName + ":" + parameter.PassingMode)), StringComparer.Ordinal))
            {
                if (slots.Any(slot => InterfaceMethodIdentity(slot.Contract) == InterfaceMethodIdentity(contract)))
                {
                    diagnostics.Add("LMC3013", DiagnosticSeverity.Error,
                        $"Interface '{interfaceName}' declares method '{contract.Declaration.Name}' more than once with the same parameter types.", contract.Declaration.Location);
                    continue;
                }
                slots.Add(new(slots.Count, interfaceType, contract));
            }
        }
        _interfaceMethodSlots = slots;

        var conversions = new HashSet<(string FromType, string ToType)>();
        foreach (var className in _classTypes.Keys)
            foreach (var interfaceName in GetImplementedInterfaceNames(className)) conversions.Add((className, interfaceName));
        foreach (var structure in _structTypes.Values)
            foreach (var interfaceName in GetStructImplementedInterfaceNames(structure.Declaration))
                conversions.Add((GetQualifiedTypeName(structure.Declaration), interfaceName));
        foreach (var (interfaceName, interfaceType) in _interfaceTypes)
        {
            // Interfaces are reference types and therefore always have the implicit
            // conversion to the compiler-known root object type.
            conversions.Add((interfaceName, "System.Object"));
            foreach (var inherited in GetInterfaceHierarchy(interfaceType).Skip(1))
                conversions.Add((interfaceName, GetQualifiedTypeName(inherited)));
        }
        _interfaceConversions = conversions;
    }

    private IReadOnlyDictionary<string, IReadOnlyList<MethodSymbol?>> BuildInterfaceDispatchTables()
    {
        var tables = new Dictionary<string, IReadOnlyList<MethodSymbol?>>(StringComparer.Ordinal);
        foreach (var className in _classTypes.Keys.Order(StringComparer.Ordinal))
        {
            var implementedInterfaces = GetImplementedInterfaceNames(className).ToHashSet(StringComparer.Ordinal);
            var entries = new MethodSymbol?[_interfaceMethodSlots.Count];
            foreach (var slot in _interfaceMethodSlots)
            {
                var interfaceName = GetQualifiedTypeName(slot.InterfaceType);
                if (!implementedInterfaces.Contains(interfaceName)) continue;
                var requirement = slot.Contract.Declaration;
                entries[slot.Index] = GetClassInstanceMethods(className, requirement.Name)
                    .OrderBy(candidate => candidate.Declaration.ExplicitInterfaceName is null ? 1 : 0)
                    .FirstOrDefault(candidate =>
                    IsInterfaceImplementationFor(candidate, className, slot.InterfaceType) &&
                    candidate.Declaration.ReturnType == requirement.ReturnType &&
                    candidate.Declaration.Parameters.Count == requirement.Parameters.Count &&
                    candidate.Declaration.Parameters.Zip(requirement.Parameters).All(pair =>
                        pair.First.TypeName == pair.Second.TypeName && pair.First.PassingMode == pair.Second.PassingMode &&
                        pair.First.IsVariadic == pair.Second.IsVariadic));
            }
            tables[className] = entries;
        }
        foreach (var structure in _structTypes.Values.OrderBy(structure => GetQualifiedTypeName(structure.Declaration), StringComparer.Ordinal))
        {
            var structureName = GetQualifiedTypeName(structure.Declaration);
            var implementedInterfaces = GetStructImplementedInterfaceNames(structure.Declaration).ToHashSet(StringComparer.Ordinal);
            var entries = new MethodSymbol?[_interfaceMethodSlots.Count];
            foreach (var slot in _interfaceMethodSlots)
            {
                if (!implementedInterfaces.Contains(GetQualifiedTypeName(slot.InterfaceType))) continue;
                var requirement = slot.Contract.Declaration;
                entries[slot.Index] = _methods
                    .Where(candidate => candidate.IsStructInstanceMethod && GetQualifiedTypeName(candidate.Owner) == structureName &&
                        candidate.Declaration.Name == requirement.Name)
                    .OrderBy(candidate => candidate.Declaration.ExplicitInterfaceName is null ? 1 : 0)
                    .FirstOrDefault(candidate =>
                        (candidate.Declaration.ExplicitInterfaceName is null
                            ? candidate.Declaration.Accessibility == "public"
                            : IsExplicitInterfaceImplementationFor(structure.Declaration, candidate.Declaration.ExplicitInterfaceName, slot.InterfaceType)) &&
                        candidate.Declaration.ReturnType == requirement.ReturnType &&
                        candidate.Declaration.Parameters.Count == requirement.Parameters.Count &&
                        candidate.Declaration.Parameters.Zip(requirement.Parameters).All(pair =>
                            pair.First.TypeName == pair.Second.TypeName && pair.First.PassingMode == pair.Second.PassingMode &&
                            pair.First.IsVariadic == pair.Second.IsVariadic));
            }
            tables[structureName] = entries;
        }
        return tables;
    }

    private HashSet<string> GetStructImplementedInterfaceNames(TypeDeclaration structure)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var baseName in structure.BaseTypes ?? [])
            foreach (var directInterface in ResolveInterfaceTypes(baseName, structure))
                foreach (var inherited in GetInterfaceHierarchy(directInterface))
                    result.Add(GetQualifiedTypeName(inherited));
        return result;
    }

    private HashSet<string> GetImplementedInterfaceNames(string className)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (var ownerName in GetClassChain(className))
        {
            if (!_classTypes.TryGetValue(ownerName, out var owner)) continue;
            foreach (var baseName in owner.Declaration.BaseTypes ?? [])
                foreach (var directInterface in ResolveInterfaceTypes(baseName, owner.Declaration))
                    foreach (var inherited in GetInterfaceHierarchy(directInterface))
                        result.Add(GetQualifiedTypeName(inherited));
        }
        return result;
    }

    private MethodSymbol[] GetInterfaceInstanceMethods(string typeName, string methodName)
    {
        if (!_interfaceTypes.TryGetValue(typeName, out var interfaceType)) return [];
        var candidates = new List<MethodSymbol>();
        var seenSignatures = new HashSet<string>(StringComparer.Ordinal);
        foreach (var contractType in GetInterfaceHierarchy(interfaceType))
        foreach (var contract in GetDeclaredInterfaceContracts(contractType).Where(method =>
                     !method.Declaration.IsStatic && !method.Declaration.HasTypeParameters && method.Declaration.Name == methodName))
        {
            var signature = string.Join("\u001f", contract.Declaration.Name,
                string.Join("\u001f", contract.Declaration.Parameters.Select(parameter => parameter.TypeName + ":" + parameter.PassingMode)));
            if (seenSignatures.Add(signature)) candidates.Add(contract);
        }
        return candidates.ToArray();
    }

    private IEnumerable<MethodSymbol> GetDeclaredInterfaceContracts(TypeDeclaration interfaceType)
    {
        foreach (var method in interfaceType.Members.OfType<MethodDeclaration>())
            yield return new(interfaceType, NormalizeInterfaceMethod(method, interfaceType));
        foreach (var property in interfaceType.Members.OfType<ComputedPropertyDeclaration>())
        {
            var propertyType = NormalizeEnumType(property.TypeName, interfaceType);
            if (property.GetterBody is not null)
                yield return new(interfaceType, new(interfaceType.Name, "$property_get_" + property.Name, propertyType, [], null,
                    property.IsStatic, true, false, false, property.Location,
                    Accessibility: property.GetterAccessibility ?? property.Accessibility, PropertyName: property.Name));
            if (property.SetterBody is not null)
                yield return new(interfaceType, new(interfaceType.Name, "$property_set_" + property.Name, propertyType,
                    [new(propertyType, "value", property.Location)], null, property.IsStatic, true, false, false, property.Location,
                    Accessibility: property.SetterAccessibility ?? property.Accessibility, PropertyName: property.Name));
        }
        foreach (var indexer in interfaceType.Members.OfType<ComputedIndexerDeclaration>())
        {
            var indexerType = NormalizeEnumType(indexer.TypeName, interfaceType);
            var parameters = indexer.Parameters.Select(parameter => parameter with
            {
                TypeName = NormalizeEnumType(parameter.TypeName, interfaceType),
                DefaultValue = null
            }).ToArray();
            if (indexer.GetterBody is not null)
                yield return new(interfaceType, new(interfaceType.Name, "$indexer_get", indexerType, parameters, null,
                    false, true, false, false, indexer.Location,
                    Accessibility: indexer.GetterAccessibility ?? indexer.Accessibility));
            if (indexer.SetterBody is not null)
                yield return new(interfaceType, new(interfaceType.Name, "$indexer_set", indexerType,
                    parameters.Append(new ParameterDeclaration(indexerType, "value", indexer.Location)).ToArray(), null,
                    false, true, false, false, indexer.Location,
                    Accessibility: indexer.SetterAccessibility ?? indexer.Accessibility));
        }
    }

    private MethodDeclaration NormalizeInterfaceMethod(MethodDeclaration declaration, TypeDeclaration owner) => declaration with
    {
        ReturnType = NormalizeEnumType(declaration.ReturnType, owner),
        Parameters = declaration.Parameters.Select(parameter => parameter with { TypeName = NormalizeEnumType(parameter.TypeName, owner) }).ToArray()
    };

    private static string InterfaceMethodIdentity(MethodSymbol method) => GetQualifiedTypeName(method.Owner) + "\u001e" +
        method.Declaration.Name + "\u001e" + string.Join("\u001f", method.Declaration.Parameters.Select(parameter => parameter.TypeName + ":" + parameter.PassingMode));

    private int FindInterfaceMethodSlot(MethodSymbol method)
    {
        var identity = InterfaceMethodIdentity(method);
        return _interfaceMethodSlots.FirstOrDefault(slot => InterfaceMethodIdentity(slot.Contract) == identity)?.Index ?? -1;
    }

    private bool IsInterfaceType(string typeName) => _interfaceTypes.ContainsKey(typeName);

    private void ValidateGenericConstraintAccessibility(IReadOnlyList<GenericConstraintDeclaration>? constraints, TypeDeclaration accessingType)
    {
        foreach (var clause in constraints ?? [])
        foreach (var constraint in clause.Constraints)
        {
            if (constraint is "class" or "struct" or "enum" or "default") continue;
            if (constraint.StartsWith("new(", StringComparison.Ordinal) && constraint.EndsWith(')'))
            {
                foreach (var parameterType in ParseConstraintParameterTypes(constraint[4..^1]))
                    ValidateTypeAccessibility(parameterType, accessingType, clause.Location);
                continue;
            }
            ValidateTypeAccessibility(constraint, accessingType, clause.Location);
        }
    }

    private void ValidateGenericTypeConstraints()
    {
        foreach (var instance in _genericTypeInstantiations)
        {
            var parameters = instance.Definition.TypeParameters ?? [];
            if (parameters.Count != instance.Arguments.Count) continue;
            var substitutions = parameters.Zip(instance.Arguments).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
            foreach (var clause in instance.Definition.GenericConstraints ?? [])
            {
                var parameterIndex = Array.FindIndex(parameters.ToArray(), parameter => parameter == clause.TypeParameter);
                if (parameterIndex < 0 || parameterIndex >= instance.Arguments.Count) continue;
                var argumentType = NormalizeEnumType(instance.Arguments[parameterIndex], instance.RequestContext);
                foreach (var rawConstraint in clause.Constraints)
                {
                    var constraint = GenericTypeSpecializer.RewriteTypeName(rawConstraint, substitutions);
                    var normalizedConstraint = constraint is "class" or "struct" or "enum" || constraint.StartsWith("new(", StringComparison.Ordinal)
                        ? constraint
                        : NormalizeEnumType(constraint, instance.Definition);
                    if (!SatisfiesGenericConstraint(argumentType, normalizedConstraint, instance.RequestContext))
                        diagnostics.Add("LS1102", DiagnosticSeverity.Error,
                            $"Type argument '{instance.Arguments[parameterIndex]}' does not satisfy constraint '{rawConstraint}' for type parameter '{clause.TypeParameter}' on '{GetQualifiedTypeName(instance.Definition)}'.",
                            instance.Location);
                }
            }
        }
    }

    private bool SatisfiesGenericConstraint(string argumentType, string constraint, TypeDeclaration accessingType)
    {
        if (constraint == "default")
            return NumericTypes.Contains(argumentType) || argumentType == "bool" || _enumUnderlyingTypes.ContainsKey(argumentType) ||
                _structTypes.ContainsKey(argumentType) || TryParseInlineArrayType(argumentType, out _, out _) || IsNullableType(argumentType);
        if (argumentType.EndsWith("?", StringComparison.Ordinal)) return false;
        var isValueType = NumericTypes.Contains(argumentType) || argumentType == "bool" || _enumUnderlyingTypes.ContainsKey(argumentType) ||
            _structTypes.ContainsKey(argumentType) || TryParseInlineArrayType(argumentType, out _, out _);
        var isReferenceType = argumentType == "string" || _classTypes.ContainsKey(argumentType) || _interfaceTypes.ContainsKey(argumentType) ||
            _delegateTypes.ContainsKey(argumentType) ||
            TryParseManagedArrayType(argumentType, out _, out _);
        if (constraint == "class") return isReferenceType;
        if (constraint == "struct") return isValueType;
        if (constraint == "enum") return _enumUnderlyingTypes.ContainsKey(argumentType);
        if (constraint.StartsWith("new(", StringComparison.Ordinal) && constraint.EndsWith(')'))
        {
            // A constructor declaration alone does not make an abstract class
            // constructible. The generic `new` constraints promise that every
            // permitted type argument can actually be instantiated.
            if (_classTypes.ContainsKey(argumentType) && HasUnimplementedVirtualMembers(argumentType)) return false;
            var requestedParameters = ParseConstraintParameterTypes(constraint[4..^1])
                .Select(parameter => NormalizeEnumType(parameter, accessingType)).ToArray();
            if (requestedParameters.Length == 0 && isValueType) return true;
            return _methods.Any(method => (method.IsClassConstructor || method.IsStructConstructor) &&
                GetQualifiedTypeName(method.Owner) == argumentType && method.Declaration.Name == "$constructor" &&
                method.Declaration.Parameters.Select(parameter => parameter.TypeName).SequenceEqual(requestedParameters, StringComparer.Ordinal) &&
                IsAccessibilityAvailable(method.Declaration.Accessibility, method.Owner, accessingType));
        }
        if (constraint == argumentType) return true;
        if (_classTypes.ContainsKey(constraint) && _classTypes.ContainsKey(argumentType))
            return constraint == argumentType || IsBaseClassOf(constraint, argumentType);
        if (_interfaceTypes.ContainsKey(constraint))
        {
            if (_classTypes.ContainsKey(argumentType)) return GetImplementedInterfaceNames(argumentType).Contains(constraint, StringComparer.Ordinal);
            if (_structTypes.TryGetValue(argumentType, out var structure))
                return structure.Declaration.BaseTypes?.SelectMany(type => ResolveInterfaceTypes(type, structure.Declaration))
                    .Any(type => GetQualifiedTypeName(type) == constraint || GetInterfaceHierarchy(type).Any(parent => GetQualifiedTypeName(parent) == constraint)) == true;
            if (_interfaceTypes.TryGetValue(argumentType, out var interfaceType))
                return GetInterfaceHierarchy(interfaceType).Any(type => GetQualifiedTypeName(type) == constraint);
        }
        return false;
    }

    private static IReadOnlyList<string> ParseConstraintParameterTypes(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
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

    private static string GetPropertyBackingFieldName(ComputedPropertyDeclaration property) =>
        "$backing_" + property.Name + (property.ExplicitInterfaceName is null ? "" : "$" + property.ExplicitInterfaceName);

    private static string GetPropertyBackingFieldName(MethodDeclaration accessor) =>
        "$backing_" + accessor.PropertyName + (accessor.ExplicitInterfaceName is null ? "" : "$" + accessor.ExplicitInterfaceName);

    private bool IsInterfaceImplementationFor(MethodSymbol implementation, string className, TypeDeclaration contractType)
    {
        var explicitName = implementation.Declaration.ExplicitInterfaceName;
        if (explicitName is null) return implementation.Declaration.Accessibility == "public";
        if (implementation.Declaration.Body is null) return false;
        var implementedInterfaces = GetImplementedInterfaceNames(className);
        return ResolveInterfaceTypes(explicitName, implementation.Owner).Any(explicitType =>
        {
            var explicitTypeName = GetQualifiedTypeName(explicitType);
            var contractName = GetQualifiedTypeName(contractType);
            return implementedInterfaces.Contains(explicitTypeName) &&
                (explicitTypeName == contractName || _interfaceConversions.Contains((explicitTypeName, contractName)));
        });
    }

    private bool IsExplicitInterfaceImplementationFor(TypeDeclaration implementationType, string? explicitName, TypeDeclaration contractType)
    {
        if (explicitName is null) return false;
        var targets = ResolveInterfaceTypes(explicitName, implementationType);
        var declaredInterfaces = implementationType.Kind == "class"
            ? GetImplementedInterfaceNames(GetQualifiedTypeName(implementationType))
            : (implementationType.BaseTypes ?? []).SelectMany(baseName => ResolveInterfaceTypes(baseName, implementationType))
                .Select(GetQualifiedTypeName).ToHashSet(StringComparer.Ordinal);
        return targets.Any(target => declaredInterfaces.Contains(GetQualifiedTypeName(target)) &&
            GetInterfaceHierarchy(target).Any(inherited => GetQualifiedTypeName(inherited) == GetQualifiedTypeName(contractType)));
    }

    private bool IsReferenceAssignable(string targetType, string sourceType)
    {
        if (targetType == sourceType) return true;
        var targetReferenceType = NullableUnderlyingType(targetType);
        // Managed arrays are invariant, but every concrete descriptor derives from
        // the compiler-known Array and Object roots.
        if (!sourceType.EndsWith("?", StringComparison.Ordinal) && TryParseManagedArrayType(sourceType, out _, out _) &&
            targetReferenceType is ("System.Array" or "System.Object" or "object")) return true;
        if (_delegateTypes.ContainsKey(sourceType) && targetType is ("System.Delegate" or "System.Object")) return true;
        if (sourceType == "System.Delegate" && targetType == "System.Object") return true;
        if (IsClassType(sourceType) && IsClassType(targetType)) return IsBaseClassOf(targetType, sourceType) || targetType == sourceType;
        return _interfaceConversions.Contains((sourceType, targetType));
    }

    private IEnumerable<TMember> GetClassHierarchyMembers<TMember>(TypeDeclaration type) where TMember : MemberDeclaration
    {
        if (type.Kind != "class") return type.Members.OfType<TMember>();
        var result = new List<TMember>();
        foreach (var ownerName in GetClassChain(GetQualifiedTypeName(type)))
        {
            var owner = _types.FirstOrDefault(candidate => GetQualifiedTypeName(candidate) == ownerName);
            if (owner is not null) result.AddRange(owner.Members.OfType<TMember>());
        }
        return result;
    }

    private void ValidateInterfaceImplementations()
    {
        foreach (var implementationType in _types.Where(type => type.Kind is "class" or "struct"))
        {
            var implementationName = GetQualifiedTypeName(implementationType);
            var declaredInterfaces = implementationType.Kind == "class"
                ? GetImplementedInterfaceNames(implementationName)
                : (implementationType.BaseTypes ?? []).SelectMany(baseName => ResolveInterfaceTypes(baseName, implementationType))
                    .Select(GetQualifiedTypeName).ToHashSet(StringComparer.Ordinal);
            foreach (var explicitMember in implementationType.Members.Where(member => member switch
                     {
                         MethodDeclaration method => method.ExplicitInterfaceName is not null,
                         ComputedPropertyDeclaration property => property.ExplicitInterfaceName is not null,
                         ComputedIndexerDeclaration indexer => indexer.ExplicitInterfaceName is not null,
                         _ => false
                     }))
            {
                var explicitName = explicitMember switch
                {
                    MethodDeclaration method => method.ExplicitInterfaceName,
                    ComputedPropertyDeclaration property => property.ExplicitInterfaceName,
                    ComputedIndexerDeclaration indexer => indexer.ExplicitInterfaceName,
                    _ => null
                };
                if (explicitName is not null && !ResolveInterfaceTypes(explicitName, implementationType)
                        .Any(target => declaredInterfaces.Contains(GetQualifiedTypeName(target))))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error,
                        $"Explicit interface member qualifier '{explicitName}' is not an interface implemented by '{implementationName}'.", explicitMember.Location);
            }
            var checkedRequirements = new HashSet<string>(StringComparer.Ordinal);
            foreach (var declaredBase in implementationType.BaseTypes ?? [])
            foreach (var interfaceType in ResolveInterfaceTypes(declaredBase, implementationType))
            {
                if (interfaceType.HasTypeParameters) continue;
                foreach (var contract in GetInterfaceHierarchy(interfaceType).Where(contract => !contract.HasTypeParameters))
                {
                    foreach (var rawRequirement in contract.Members.OfType<MethodDeclaration>())
                    {
                        // Members on a specialized generic interface still contain source
                        // spellings such as IIterator<int32>. Compare canonical compiler
                        // type names here, as we do when constructing interface dispatch.
                        var requirement = NormalizeInterfaceMethod(rawRequirement, contract);
                        var key = "method:" + GetQualifiedTypeName(contract) + ":" + requirement.Name + ":" + requirement.ReturnType + ":" +
                            string.Join(",", requirement.Parameters.Select(parameter => parameter.TypeName + ":" + parameter.PassingMode));
                        if (!checkedRequirements.Add(key)) continue;
                        var ownerName = GetQualifiedTypeName(implementationType);
                        var candidates = implementationType.Kind == "class"
                            ? GetClassInstanceMethods(ownerName, requirement.Name).Select(candidate => candidate.Declaration).ToArray()
                            : _methods.Where(candidate => candidate.IsStructInstanceMethod && candidate.Owner == implementationType &&
                                candidate.Declaration.Name == requirement.Name).Select(candidate => candidate.Declaration).ToArray();
                        var implemented = candidates.Any(candidate =>
                            !candidate.IsStatic &&
                            (candidate.ExplicitInterfaceName is null && candidate.Accessibility == "public" ||
                                IsExplicitInterfaceImplementationFor(implementationType, candidate.ExplicitInterfaceName, contract)) &&
                            candidate.ReturnType == requirement.ReturnType &&
                            candidate.Parameters.Count == requirement.Parameters.Count &&
                            candidate.Parameters.Zip(requirement.Parameters).All(pair =>
                                pair.First.TypeName == pair.Second.TypeName && pair.First.PassingMode == pair.Second.PassingMode &&
                                pair.First.IsVariadic == pair.Second.IsVariadic)) ||
                            implementationType.Kind == "class" && implementationType.IsVirtual && candidates.Any(candidate =>
                                !candidate.IsStatic && candidate.Accessibility == "public" && candidate.IsVirtual && candidate.Body is null &&
                                candidate.ReturnType == requirement.ReturnType && candidate.Parameters.Count == requirement.Parameters.Count &&
                                candidate.Parameters.Zip(requirement.Parameters).All(pair => pair.First.TypeName == pair.Second.TypeName &&
                                    pair.First.PassingMode == pair.Second.PassingMode && pair.First.IsVariadic == pair.Second.IsVariadic));
                        if (!implemented)
                            diagnostics.Add("LS1001", DiagnosticSeverity.Error,
                                $"Type '{GetQualifiedTypeName(implementationType)}' does not implement interface member '{contract.Name}.{requirement.Name}'.",
                                implementationType.Location);
                    }
                    foreach (var requirement in contract.Members.OfType<ComputedPropertyDeclaration>())
                    {
                        var requirementType = NormalizeEnumType(requirement.TypeName, contract);
                        var key = "property:" + GetQualifiedTypeName(contract) + ":" + requirement.Name + ":" + requirement.TypeName + ":" +
                            (requirement.GetterBody is not null) + ":" + (requirement.SetterBody is not null);
                        if (!checkedRequirements.Add(key)) continue;
                        var implemented = GetClassHierarchyMembers<ComputedPropertyDeclaration>(implementationType).Any(candidate =>
                        {
                            var accessible = candidate.ExplicitInterfaceName is null
                                ? candidate.Accessibility == "public"
                                : IsExplicitInterfaceImplementationFor(implementationType, candidate.ExplicitInterfaceName, contract);
                            return candidate.Name == requirement.Name && NormalizeEnumType(candidate.TypeName, implementationType) == requirementType && !candidate.IsStatic && accessible &&
                                (requirement.GetterBody is null || candidate.GetterBody is not null &&
                                    (candidate.ExplicitInterfaceName is not null || candidate.GetterAccessibility == "public")) &&
                                (requirement.SetterBody is null || candidate.SetterBody is not null &&
                                    (candidate.ExplicitInterfaceName is not null || candidate.SetterAccessibility == "public"));
                        });
                        if (!implemented)
                            diagnostics.Add("LS1001", DiagnosticSeverity.Error,
                                $"Type '{GetQualifiedTypeName(implementationType)}' does not implement interface property '{contract.Name}.{requirement.Name}'.",
                                implementationType.Location);
                    }
                    foreach (var requirement in contract.Members.OfType<ComputedIndexerDeclaration>())
                    {
                        var requirementType = NormalizeEnumType(requirement.TypeName, contract);
                        var requirementParameters = requirement.Parameters.Select(parameter => NormalizeEnumType(parameter.TypeName, contract)).ToArray();
                        var key = "indexer:" + GetQualifiedTypeName(contract) + ":" + requirement.TypeName + ":" +
                            (requirement.GetterBody is not null) + ":" + (requirement.SetterBody is not null) + ":" +
                            string.Join(",", requirement.Parameters.Select(parameter => parameter.TypeName + ":" + parameter.PassingMode));
                        if (!checkedRequirements.Add(key)) continue;
                        var implemented = GetClassHierarchyMembers<ComputedIndexerDeclaration>(implementationType).Any(candidate =>
                            NormalizeEnumType(candidate.TypeName, implementationType) == requirementType &&
                            (candidate.ExplicitInterfaceName is null ? candidate.Accessibility == "public" :
                                IsExplicitInterfaceImplementationFor(implementationType, candidate.ExplicitInterfaceName, contract)) &&
                            candidate.Parameters.Count == requirement.Parameters.Count &&
                            candidate.Parameters.Zip(requirement.Parameters).Select((pair, index) =>
                                NormalizeEnumType(pair.First.TypeName, implementationType) == requirementParameters[index] &&
                                pair.First.PassingMode == pair.Second.PassingMode && pair.First.IsVariadic == pair.Second.IsVariadic).All(matches => matches) &&
                            (requirement.GetterBody is null || candidate.GetterBody is not null &&
                                (candidate.ExplicitInterfaceName is not null || candidate.GetterAccessibility == "public")) &&
                            (requirement.SetterBody is null || candidate.SetterBody is not null &&
                                (candidate.ExplicitInterfaceName is not null || candidate.SetterAccessibility == "public")));
                        if (!implemented)
                            diagnostics.Add("LS1001", DiagnosticSeverity.Error,
                                $"Type '{GetQualifiedTypeName(implementationType)}' does not implement an indexer from interface '{contract.Name}'.",
                                implementationType.Location);
                    }
                }
            }
        }
    }

    private BlockStatement CompletePropertySetterBody(BlockStatement body, SourceLocation location)
    {
        var rewritten = (BlockStatement)RewriteSetterReturns(body);
        if (AlwaysAbrupt(rewritten))
            return rewritten;

        return rewritten with
        {
            Statements = rewritten.Statements.Append(new ReturnStatement(new NameExpression("value", location), location)).ToArray()
        };

        Statement RewriteSetterReturns(Statement statement) => statement switch
        {
            ReturnStatement { Value: null } returned => returned with { Value = new NameExpression("value", returned.Location) },
            ReturnStatement { Value: not null } returned => InvalidValueReturn(returned),
            BlockStatement block => block with { Statements = block.Statements.Select(RewriteSetterReturns).ToArray() },
            IfStatement conditional => conditional with { Then = RewriteSetterReturns(conditional.Then), Else = conditional.Else is null ? null : RewriteSetterReturns(conditional.Else) },
            WhileStatement loop => loop with { Body = RewriteSetterReturns(loop.Body) },
            DoWhileStatement loop => loop with { Body = RewriteSetterReturns(loop.Body) },
            ForStatement loop => loop with { Initializer = loop.Initializer is null ? null : RewriteSetterReturns(loop.Initializer), Body = RewriteSetterReturns(loop.Body) },
            ForInStatement loop => loop with { Body = RewriteSetterReturns(loop.Body) },
            SwitchStatement selection => selection with { Sections = selection.Sections.Select(section =>
            {
                var statements = section.Statements.Select(RewriteSetterReturns).ToArray();
                return section with { Statements = statements, CaptureScope = new BlockStatement(statements, section.At) };
            }).ToArray() },
            TryStatement attempt => attempt with { Body = (BlockStatement)RewriteSetterReturns(attempt.Body), Catches = attempt.Catches.Select(clause => clause with { Body = (BlockStatement)RewriteSetterReturns(clause.Body) }).ToArray(), Finally = attempt.Finally is null ? null : (BlockStatement)RewriteSetterReturns(attempt.Finally) },
            OverflowContextStatement overflow => overflow with { Body = RewriteSetterReturns(overflow.Body) },
            _ => statement
        };

        Statement InvalidValueReturn(ReturnStatement returned)
        {
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, "A set accessor cannot return a value.", returned.Location);
            return new BlockStatement([
                new ExpressionStatement(returned.Value!, returned.Location),
                new ReturnStatement(new NameExpression("value", returned.Location), returned.Location)
            ], returned.Location);
        }
    }

    private IEnumerable<string> GetClassChain(string typeName)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var current = typeName;
        while (_classTypes.TryGetValue(current, out var symbol) && visited.Add(current))
        {
            yield return current;
            if (symbol.BaseTypeName is null) yield break;
            current = symbol.BaseTypeName;
        }
    }

    private bool IsBaseClassOf(string baseType, string derivedType) => GetClassChain(derivedType).Skip(1).Contains(baseType, StringComparer.Ordinal);

    private MethodSymbol[] GetClassInstanceMethods(string typeName, string methodName)
    {
        var visible = new List<MethodSymbol>();
        var signatures = new HashSet<string>(StringComparer.Ordinal);
        foreach (var owner in GetClassChain(typeName))
        foreach (var candidate in _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.IsClassInstanceMethod && GetQualifiedTypeName(candidate.Owner) == owner && candidate.Declaration.Name == methodName))
        {
            var signature = string.Join("\u001f", candidate.Declaration.Parameters.Select(parameter => parameter.TypeName)) +
                "\u001e" + candidate.Declaration.ExplicitInterfaceName;
            if (signatures.Add(signature)) visible.Add(candidate);
        }
        return visible.ToArray();
    }

    private MethodSymbol? FindInheritedMethod(MethodSymbol method)
    {
        var signature = string.Join("\u001f", method.Declaration.Parameters.Select(parameter => parameter.TypeName));
        foreach (var owner in GetClassChain(GetQualifiedTypeName(method.Owner)).Skip(1))
        {
            var inherited = _methods.FirstOrDefault(candidate => candidate.IsClassInstanceMethod && GetQualifiedTypeName(candidate.Owner) == owner &&
                candidate.Declaration.Name == method.Declaration.Name && string.Join("\u001f", candidate.Declaration.Parameters.Select(parameter => parameter.TypeName)) == signature);
            if (inherited is not null) return inherited;
        }
        if (method.Declaration.Name == "ToString" && method.Declaration.ReturnType == "string" && method.Declaration.Parameters.Count == 0)
            return VirtualDispatchLayout.ObjectToString;
        return null;
    }

    private void ValidateVirtualMethods(IReadOnlyList<MethodSymbol> methods)
    {
        foreach (var method in methods.Where(candidate => candidate.IsClassInstanceMethod))
        {
            var declaration = method.Declaration;
            var ownerType = _classTypes[GetQualifiedTypeName(method.Owner)].Declaration;
            var inherited = FindInheritedMethod(method);
            if (declaration.IsVirtual && !ownerType.IsVirtual)
                diagnostics.Add("LS0831", DiagnosticSeverity.Error, $"Virtual member '{method.QualifiedName}' is declared in a non-virtual class.", declaration.Location);
            if (declaration.IsVirtual && declaration.IsOverride)
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A member cannot be declared both virtual and override.", declaration.Location);
            if (declaration.IsOverride)
            {
                if (inherited is null || !inherited.Declaration.IsVirtual && !inherited.Declaration.IsOverride ||
                    inherited.Declaration.ReturnType != declaration.ReturnType || inherited.Declaration.Accessibility != declaration.Accessibility ||
                    !OverrideParameterContractsMatch(inherited.Declaration.Parameters, declaration.Parameters))
                    diagnostics.Add("LS0832", DiagnosticSeverity.Error, $"Override '{method.QualifiedName}' does not match an inherited virtual member's signature, parameter modifiers, return type, and accessibility.", declaration.Location);
            }
            else if (inherited is not null && (inherited.Declaration.IsVirtual || inherited.Declaration.IsOverride))
                diagnostics.Add("LS0833", DiagnosticSeverity.Error, $"Member '{method.QualifiedName}' must be marked override to replace an inherited virtual member.", declaration.Location);
            else if (inherited is not null)
                diagnostics.Add("LS3002", DiagnosticSeverity.Warning, $"Member '{method.QualifiedName}' hides an inherited non-virtual member.", declaration.Location);
        }

        foreach (var (typeName, symbol) in _classTypes)
            if (!symbol.Declaration.IsVirtual && HasUnimplementedVirtualMembers(typeName))
                diagnostics.Add("LS0835", DiagnosticSeverity.Error, $"Non-virtual class '{typeName}' leaves an inherited abstract virtual member unimplemented.", symbol.Declaration.Location);
    }

    private static bool OverrideParameterContractsMatch(
        IReadOnlyList<ParameterDeclaration> baseParameters,
        IReadOnlyList<ParameterDeclaration> overrideParameters)
    {
        if (baseParameters.Count != overrideParameters.Count)
            return false;

        for (var index = 0; index < baseParameters.Count; index++)
        {
            var baseParameter = baseParameters[index];
            var overrideParameter = overrideParameters[index];
            if (baseParameter.TypeName != overrideParameter.TypeName ||
                baseParameter.PassingMode != overrideParameter.PassingMode ||
                baseParameter.IsScoped != overrideParameter.IsScoped ||
                baseParameter.IsVariadic != overrideParameter.IsVariadic)
                return false;
        }

        return true;
    }

    private void ValidateFinalizerCompletionCapacity(IReadOnlyList<MethodSymbol> methods)
    {
        var finalizerOwners = methods.Where(method => method.IsClassFinalizer)
            .Select(method => GetQualifiedTypeName(method.Owner)).ToHashSet(StringComparer.Ordinal);
        foreach (var className in _classTypes.Keys)
        {
            var finalizerCount = GetClassChain(className).Count(finalizerOwners.Contains);
            if (finalizerCount > 29)
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                    $"Class hierarchy '{className}' has {finalizerCount} finalizers; the current object header can track completion for at most 29 finalizers in one hierarchy.",
                    _classTypes[className].Declaration.Location);
        }
    }

    private bool HasUnimplementedVirtualMembers(string typeName)
    {
        var implementations = new Dictionary<string, MethodSymbol>(StringComparer.Ordinal);
        foreach (var owner in GetClassChain(typeName).Reverse())
        foreach (var method in _methods.Where(method => method.IsClassInstanceMethod && GetQualifiedTypeName(method.Owner) == owner &&
                     (method.Declaration.IsVirtual || method.Declaration.IsOverride)))
            implementations[string.Join("\u001f", method.Declaration.Name, string.Join("\u001f", method.Declaration.Parameters.Select(parameter => parameter.TypeName)))] = method;
        return implementations.Values.Any(method => method.Declaration.Body is null);
    }

    private void AnalyzeClassFields(IEnumerable<(TypeDeclaration Owner, FieldDeclaration Declaration)> declarations)
    {
        foreach (var group in declarations.GroupBy(item => GetQualifiedTypeName(item.Owner), StringComparer.Ordinal))
        {
            var classFields = new List<StructFieldSymbol>();
            var ownerType = group.First().Owner;
            var names = new HashSet<string>(ownerType.Members.OfType<FieldDeclaration>().Where(field => field.IsStatic).Select(field => field.Name)
                .Concat(ownerType.Members.OfType<ConstantFieldDeclaration>().Select(field => field.Name)), StringComparer.Ordinal);
            foreach (var (owner, declaration) in group)
            {
                if (!names.Add(declaration.Name))
                {
                    diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Class field '{group.Key}.{declaration.Name}' is declared more than once.", declaration.Location);
                    continue;
                }
                var fieldType = NormalizeEnumType(declaration.TypeName, owner);
                ValidateTypeAccessibility(declaration.TypeName, owner, declaration.Location);
                var inlineArray = TryParseInlineArrayType(fieldType, out _, out _);
                if (inlineArray) ValidateDeclaredType(fieldType, "Class field", declaration.Location, owner);
                var supportedStorage = inlineArray || SupportedTypes.Contains(fieldType) && fieldType != "void" || fieldType == "string?" || _enumUnderlyingTypes.ContainsKey(fieldType) || _structTypes.ContainsKey(fieldType) ||
                    _classTypes.ContainsKey(fieldType) || _interfaceTypes.ContainsKey(fieldType) || _delegateTypes.ContainsKey(fieldType) || IsNullableClassType(fieldType) || IsNullableDelegateType(fieldType) || IsNullableArrayType(fieldType) || IsNullableValueType(fieldType) || TryParseManagedArrayType(fieldType, out var arrayElementType, out var arrayRank) && arrayRank <= 3 && IsManagedArrayElementType(arrayElementType);
                if (!supportedStorage)
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Class field type '{fieldType}' is not supported by the current class object runtime.", declaration.Location);
                if (classFields.Any(field => field.Declaration.Name == declaration.Name)) continue;
                classFields.Add(new(owner, declaration, fieldType, classFields.Count));
            }
            if (_classTypes.TryGetValue(group.Key, out var classType))
                _classTypes[group.Key] = classType with { Fields = classFields };
        }

        var declaredFields = _classTypes.ToDictionary(pair => pair.Key, pair => pair.Value.Fields, StringComparer.Ordinal);
        foreach (var name in _classTypes.Keys.OrderBy(typeName => GetClassChain(typeName).Count()))
        {
            var classType = _classTypes[name];
            var fields = classType.BaseTypeName is { } baseName && _classTypes.TryGetValue(baseName, out var baseType)
                ? baseType.Fields.ToList() : [];
            foreach (var field in declaredFields[name])
                fields.Add(field with { Index = fields.Count });
            _classTypes[name] = classType with { Fields = fields };
        }
    }

    private void ValidateClassReferenceFieldInitialization()
    {
        var writeCache = new Dictionary<MethodSymbol, HashSet<string>>(ReferenceEqualityComparer.Instance);
        var resolving = new HashSet<MethodSymbol>(ReferenceEqualityComparer.Instance);
        HashSet<string> GetGuaranteedWrites(MethodSymbol constructor)
        {
            if (writeCache.TryGetValue(constructor, out var cached)) return new(cached, StringComparer.Ordinal);
            if (!resolving.Add(constructor)) return [];
            var writes = CollectGuaranteedClassFieldWrites(constructor.Declaration.Body!, GetQualifiedTypeName(constructor.Owner));
            if (_constructorInitializers.TryGetValue(constructor, out var initializer) && initializer.DelegatesToThis &&
                _callTargets.TryGetValue(initializer.Call, out var delegated))
                writes.UnionWith(GetGuaranteedWrites(delegated));
            resolving.Remove(constructor);
            writeCache[constructor] = writes;
            return new(writes, StringComparer.Ordinal);
        }

        foreach (var (className, classType) in _classTypes)
        {
            if (IsClosureEnvironmentType(className)) continue;
            var constructors = _methods.Where(method => method.IsClassConstructor && GetQualifiedTypeName(method.Owner) == className).ToArray();
            foreach (var field in classType.Fields.Where(field => GetQualifiedTypeName(field.Owner) == className && field.Declaration.Initializer is null &&
                         (field.TypeName == "string" || IsClassReferenceType(field.TypeName) && !IsNullableClassType(field.TypeName) || IsDelegateType(field.TypeName) ||
                          TryParseManagedArrayType(field.TypeName, out _, out _) && !IsNullableArrayType(field.TypeName))))
            {
                var required = constructors.Length == 0 || constructors.Any(constructor =>
                    !GetGuaranteedWrites(constructor).Contains(field.Declaration.Name));
                if (required)
                    diagnostics.Add("LS0810", DiagnosticSeverity.Error, $"Non-nullable class field '{className}.{field.Declaration.Name}' must be initialized in its declaration or in every constructor.", field.Declaration.Location);
            }
        }
    }

    private void AnalyzeStrongReferenceCycles()
    {
        var nodes = _classTypes.Keys.Concat(_structTypes.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var edges = new List<(string From, string To, StructFieldSymbol Field)>();
        foreach (var (ownerName, classType) in _classTypes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        // Inherited storage exists on every derived instance, so include inherited
        // reference fields as edges from each derived type as well.
        foreach (var field in classType.Fields)
        {
            // Array edges are intentionally omitted by the v1 diagnostic contract.
            if (!IsClassType(field.TypeName)) continue;
            foreach (var target in _classTypes.Keys.Where(target => target == field.TypeName || IsBaseClassOf(field.TypeName, target)).Order(StringComparer.Ordinal))
                edges.Add((ownerName, target, field));
        }

        var edgesBySource = edges.GroupBy(edge => edge.From, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.OrderBy(edge => edge.To, StringComparer.Ordinal).ThenBy(edge => edge.Field.Declaration.Name, StringComparer.Ordinal).ToArray(), StringComparer.Ordinal);
        var nextIndex = 0;
        var indices = new Dictionary<string, int>(StringComparer.Ordinal);
        var lowLinks = new Dictionary<string, int>(StringComparer.Ordinal);
        var stack = new Stack<string>();
        var onStack = new HashSet<string>(StringComparer.Ordinal);
        var components = new List<string[]>();

        void Visit(string node)
        {
            indices[node] = nextIndex;
            lowLinks[node] = nextIndex++;
            stack.Push(node);
            onStack.Add(node);
            if (edgesBySource.TryGetValue(node, out var outgoing))
                foreach (var edge in outgoing)
                {
                    if (!indices.ContainsKey(edge.To))
                    {
                        Visit(edge.To);
                        lowLinks[node] = Math.Min(lowLinks[node], lowLinks[edge.To]);
                    }
                    else if (onStack.Contains(edge.To)) lowLinks[node] = Math.Min(lowLinks[node], indices[edge.To]);
                }
            if (lowLinks[node] != indices[node]) return;
            var component = new List<string>();
            string member;
            do
            {
                member = stack.Pop();
                onStack.Remove(member);
                component.Add(member);
            } while (member != node);
            components.Add(component.Order(StringComparer.Ordinal).ToArray());
        }

        foreach (var node in nodes)
            if (!indices.ContainsKey(node)) Visit(node);

        foreach (var component in components.OrderBy(component => component[0], StringComparer.Ordinal))
        {
            var members = component.ToHashSet(StringComparer.Ordinal);
            var cycleEdges = edges.Where(edge => members.Contains(edge.From) && members.Contains(edge.To)).ToArray();
            if (component.Length == 1 && !cycleEdges.Any(edge => edge.From == edge.To)) continue;
            var descriptions = cycleEdges
                .Select(edge => $"{edge.From}.{edge.Field.Declaration.Name} -> {edge.To}")
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal);
            var location = cycleEdges.OrderBy(edge => edge.From, StringComparer.Ordinal).ThenBy(edge => edge.Field.Declaration.Name, StringComparer.Ordinal).First().Field.Declaration.Location;
            diagnostics.Add("LS3301", DiagnosticSeverity.Warning,
                $"Potential strong reference cycle: {string.Join(", ", descriptions)}.", location);
        }
    }

    private HashSet<string> CollectGuaranteedClassFieldWrites(Statement statement, string ownerType)
    {
        switch (statement)
        {
            case BlockStatement block:
                var guaranteed = new HashSet<string>(StringComparer.Ordinal);
                foreach (var child in block.Statements)
                {
                    guaranteed.UnionWith(CollectGuaranteedClassFieldWrites(child, ownerType));
                    if (AlwaysReturns(child)) break;
                }
                return guaranteed;
            case ExpressionStatement { Value: BinaryExpression { Operator: "=" } assignment }:
                if (assignment.Left is NameExpression name && _boundUnqualifiedClassFields.TryGetValue(name, out var unqualified) && unqualified.OwnerType == ownerType) return [name.Name];
                if (assignment.Left is MemberAccessExpression { Receiver: ThisExpression } member && _boundClassFields.TryGetValue(member, out var bound) && bound.OwnerType == ownerType) return [member.Member];
                if (assignment.Left is NameExpression propertyName && _boundUnqualifiedComputedProperties.TryGetValue(propertyName, out var unqualifiedProperty) &&
                    unqualifiedProperty.Declaration.SetterIsAuto && GetQualifiedTypeName(unqualifiedProperty.Owner) == ownerType)
                    return [GetPropertyBackingFieldName(unqualifiedProperty.Declaration)];
                if (assignment.Left is MemberAccessExpression { Receiver: ThisExpression } propertyAccess && _boundComputedProperties.TryGetValue(propertyAccess, out var boundProperty) &&
                    boundProperty.Declaration.SetterIsAuto && GetQualifiedTypeName(boundProperty.Owner) == ownerType)
                    return [GetPropertyBackingFieldName(boundProperty.Declaration)];
                return [];
            case IfStatement conditional when conditional.Else is not null:
                var thenWrites = CollectGuaranteedClassFieldWrites(conditional.Then, ownerType);
                var elseWrites = CollectGuaranteedClassFieldWrites(conditional.Else, ownerType);
                if (AlwaysReturns(conditional.Then)) return elseWrites;
                if (AlwaysReturns(conditional.Else)) return thenWrites;
                thenWrites.IntersectWith(elseWrites);
                return thenWrites;
            case SwitchStatement selection when selection.Sections.Any(section => section.Labels.Any(label => label.IsDefault)):
                var sections = selection.Sections.Select(section => CollectGuaranteedClassFieldWrites(new BlockStatement(section.Statements, section.At), ownerType)).ToArray();
                if (sections.Length == 0) return [];
                var common = sections[0];
                foreach (var section in sections.Skip(1)) common.IntersectWith(section);
                return common;
            default:
                return [];
        }
    }

    private bool IsNonNullableReferenceField(string type, TypeDeclaration context)
    {
        if (type.EndsWith('?') || IsPointerType(type) || TryParseInlineArrayType(type, out _, out _)) return false;
        if (type is "string" or "object" || type.Contains('[')) return true;
        return FindTypes(type, context).Any(candidate => candidate.Kind is "class" or "interface" or "delegate");
    }

    private bool TryGetTypeLayout(string type, HashSet<string> active, out int size, out int alignment)
    {
        if (type is "string" or "string?" or "object" || IsClassType(type) || IsNullableClassType(type) ||
            IsDelegateType(type) || IsNullableDelegateType(type) || IsNullableStringType(type) || IsNullableArrayType(type) ||
            TryParseManagedArrayType(type, out _, out _))
        {
            size = alignment = _nativePointerSize;
            return true;
        }
        if (IsNullableValueType(type))
        {
            if (!TryGetTypeLayout(NullableUnderlyingType(type), active, out var valueSize, out var valueAlignment))
            { size = alignment = 0; return false; }
            var nullablePayloadOffset = AlignTo(1, valueAlignment);
            var total = AlignTo(nullablePayloadOffset + valueSize, valueAlignment);
            if (total > int.MaxValue) { size = alignment = 0; return false; }
            size = (int)total; alignment = valueAlignment; return true;
        }
        if (_enumUnderlyingTypes.TryGetValue(type, out var underlying)) return TryGetTypeLayout(underlying, active, out size, out alignment);
        if (TryParseInlineArrayType(type, out var elementType, out var count))
        {
            if (count <= 0 || !TryGetTypeLayout(elementType, active, out var elementSize, out alignment)) { size = alignment = 0; return false; }
            var total = (long)elementSize * count;
            if (total > int.MaxValue) { size = alignment = 0; return false; }
            size = (int)total;
            return true;
        }
        if (IsPointerType(type)) { size = alignment = _nativePointerSize; return true; }
        if (_structTypes.TryGetValue(type, out var structure))
        {
            if (structure.SizeInBytes > 0) { size = structure.SizeInBytes; alignment = structure.Alignment; return true; }
            if (!active.Add(type)) { size = alignment = 0; return false; }
            long offset = 0;
            var maxAlignment = 1;
            foreach (var field in structure.Fields)
            {
                if (!TryGetTypeLayout(field.TypeName, active, out var fieldSize, out var fieldAlignment))
                { active.Remove(type); size = alignment = 0; return false; }
                offset = AlignTo(offset, fieldAlignment) + fieldSize;
                maxAlignment = Math.Max(maxAlignment, fieldAlignment);
                if (offset > int.MaxValue) { active.Remove(type); size = alignment = 0; return false; }
            }
            active.Remove(type);
            size = structure.Fields.Count == 0 ? 1 : (int)AlignTo(offset, maxAlignment);
            alignment = maxAlignment;
            _structTypes[type] = structure with { SizeInBytes = size, Alignment = alignment };
            return true;
        }
        size = type switch
        {
            "bool" or "int8" or "uint8" => 1,
            "int16" or "uint16" or "char" => 2,
            "int32" or "uint32" or "float" => 4,
            "int64" or "uint64" or "double" => 8,
            "int" or "uint" => _nativePointerSize,
            "string" => _nativePointerSize,
            _ => 0
        };
        if (size == 0) { alignment = 0; return false; }
        alignment = size == 8 ? GetEightByteAlignment() : size;
        return true;
    }

    private int GetEightByteAlignment()
    {
        if (_nativePointerSize == 8) return 8;
        var arch = options.TargetTriple.Split('-', 2)[0].ToLowerInvariant();
        return arch is "i386" or "i486" or "i586" or "i686" or "x86" ? 4 : 8;
    }

    private static long AlignTo(long value, int alignment) => (value + alignment - 1) / alignment * alignment;

    private void AnalyzeStaticFields(IEnumerable<(TypeDeclaration Owner, FieldDeclaration Declaration)> fields,
        IReadOnlyDictionary<TypeDeclaration, ConstructorDeclaration> staticConstructors)
    {
        foreach (var (owner, field) in fields)
        {
            var previousConstantContext = _enumEvaluationOwner; _enumEvaluationOwner = owner;
            try { AnalyzeStaticField(owner, field, staticConstructors); }
            finally { _enumEvaluationOwner = previousConstantContext; }
        }
    }

    private void AddTypeInitializerMethods(IReadOnlyList<TypeDeclaration> types,
        IReadOnlyDictionary<TypeDeclaration, ConstructorDeclaration> staticConstructors, List<MethodSymbol> methods)
    {
        foreach (var type in types)
        {
            var dynamicFields = new List<StaticFieldSymbol>();
            foreach (var fieldDeclaration in type.Members.OfType<FieldDeclaration>())
                if (_staticFields.TryGetValue(GetQualifiedTypeName(type) + "." + fieldDeclaration.Name, out var entry) &&
                    entry.Symbol.RequiresInitialization && entry.Symbol.Declaration.Initializer is not null) dynamicFields.Add(entry.Symbol);
            var sourceConstructor = staticConstructors.GetValueOrDefault(type);
            if (dynamicFields.Count == 0 && sourceConstructor is null) continue;

            var location = sourceConstructor?.Location ?? dynamicFields[0].Declaration.At;
            var statements = new List<Statement>();
            foreach (var field in dynamicFields)
            {
                var at = field.Declaration.At;
                var target = new MemberAccessExpression(new NameExpression(type.Name, at), field.Declaration.Name, at);
                statements.Add(new ExpressionStatement(new BinaryExpression(target, "=", field.Declaration.Initializer!, at), at));
            }
            if (sourceConstructor?.Body is { } body) statements.AddRange(body.Statements);
            var initializerBody = new BlockStatement(statements, location);
            var declaration = new MethodDeclaration(type.Name, "$type_initializer", "void", [], initializerBody, true, false, false, false,
                location, Accessibility: "private");
            methods.Add(new(type, declaration, SourceConstructor: sourceConstructor, IsTypeInitializer: true));
        }
    }

    private void AnalyzeStaticField(TypeDeclaration owner, FieldDeclaration field,
        IReadOnlyDictionary<TypeDeclaration, ConstructorDeclaration> staticConstructors)
    {
        ValidateTypeAccessibility(field.TypeName, owner, field.Location);
        if (field.Initializer is not null) BindTypeExpressions(field.Initializer, owner);
        if (field.Initializer is not null && ContainsScopeAllocation(field.Initializer))
            diagnostics.Add("LS0803", DiagnosticSeverity.Error, "A scope allocation requires an enclosing method or block body and cannot appear in a field initializer.", field.Initializer.Location);
        var key = GetQualifiedTypeName(owner) + "." + field.Name;
        if (_staticFields.ContainsKey(key) || _constantFields.ContainsKey(key))
        {
            diagnostics.Add("LMC3013", DiagnosticSeverity.Error, $"Field '{key}' is declared more than once.", field.Location);
            return;
        }
        if (!field.IsStatic)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Instance fields are not yet supported by the current backend; mark this field static or use a primitive const field.", field.Location);
            return;
        }
        if ((field.IsReadOnly || IsNonNullableStaticReferenceType(field.TypeName)) && field.Initializer is null &&
            (!staticConstructors.TryGetValue(owner, out var staticConstructor) ||
             !HasAssignmentOnAllNormalStaticConstructorExits(staticConstructor.Body, owner, field.Name)))
        {
            if (field.IsReadOnly)
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "A static readonly field must have a compile-time initializer or be assigned on every normal exit from its static constructor.", field.Location);
            else
                diagnostics.Add("LS0810", DiagnosticSeverity.Error, $"Static field '{GetQualifiedTypeName(owner)}.{field.Name}' of non-nullable reference type '{field.TypeName}' must be initialized in its declaration or assigned by the static constructor.", field.Location);
            return;
        }
        var normalizedField = field with { TypeName = NormalizeEnumType(field.TypeName, owner) };
        if (!IsSupportedType(normalizedField.TypeName) || normalizedField.TypeName == "void")
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Static field type '{normalizedField.TypeName}' is not supported by the current backend.", field.Location);
            return;
        }
        if (_interfaceTypes.ContainsKey(normalizedField.TypeName) || normalizedField.TypeName.EndsWith("?", StringComparison.Ordinal) &&
            _interfaceTypes.ContainsKey(normalizedField.TypeName[..^1]))
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Static interface field '{key}' is not supported by the current static storage runtime.", field.Location);
            return;
        }

        var initialValue = DefaultLlvmValue(normalizedField.TypeName);
        var requiresInitialization = field.Initializer is null &&
            (field.IsReadOnly || IsNonNullableStaticReferenceType(normalizedField.TypeName));
        if (normalizedField.Initializer is not null)
        {
            if (normalizedField.TypeName is "string" or "string?")
            {
                if (normalizedField.Initializer is DefaultValueExpression defaultString)
                {
                    _defaultTypes[defaultString] = defaultString.TypeName ?? normalizedField.TypeName;
                    initialValue = normalizedField.TypeName == "string" ? "@luma_string_empty" : "null";
                }
                else if (normalizedField.Initializer is LiteralExpression stringLiteral && TryInternStringLiteral(stringLiteral, out var stringInfo))
                    initialValue = "@" + stringInfo.Symbol;
                else if (normalizedField.Initializer is LiteralExpression { Value: "null" } && normalizedField.TypeName == "string?")
                    initialValue = "null";
                else if (CanRuntimeInitializeStaticField(normalizedField.TypeName))
                    requiresInitialization = true;
                else
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "This static string initializer cannot be lowered by the runtime initialization path.", normalizedField.Initializer.Location);
                    return;
                }
            }
            else
            {
                if (normalizedField.Initializer is DefaultValueExpression defaultValue)
                {
                    _defaultTypes[defaultValue] = defaultValue.TypeName ?? normalizedField.TypeName;
                }
                else if (TryEvaluateConstantForOwner(normalizedField.Initializer, owner, out var constant, out _))
                {
                    var expressionType = ConstantExpressionTypeForOwner(normalizedField.Initializer, owner) ?? constant.TypeName;
                    if (!TryConvertConstant(constant, normalizedField.TypeName, normalizedField.Initializer, out initialValue))
                    {
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert static field initializer '{expressionType}' to '{normalizedField.TypeName}'.", normalizedField.Initializer.Location);
                        return;
                    }
                }
                else if (normalizedField.Initializer is LiteralExpression { Value: "null" } && normalizedField.TypeName.EndsWith("?", StringComparison.Ordinal))
                    initialValue = "null";
                else if (CanRuntimeInitializeStaticField(normalizedField.TypeName))
                {
                    requiresInitialization = true;
                }
                else
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "This static field initializer cannot be lowered by the current runtime initialization path.", normalizedField.Initializer.Location);
                    return;
                }
            }
        }

        var global = "luma_global_" + string.Concat(key.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_'));
        var symbol = new StaticFieldSymbol(owner, normalizedField, global, initialValue, requiresInitialization);
        _staticFields.Add(key, (owner, symbol, GetFieldAccessibility(normalizedField)));
    }

    private bool CanRuntimeInitializeStaticField(string typeName) =>
        typeName is "bool" or "char" or "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or
            "int64" or "uint64" or "float" or "double" || _enumUnderlyingTypes.ContainsKey(typeName) ||
        typeName is "string" or "string?" || _classTypes.ContainsKey(typeName) ||
        typeName.EndsWith("?", StringComparison.Ordinal) && _classTypes.ContainsKey(typeName[..^1]) ||
        TryParseManagedArrayType(typeName, out _, out var rank) && rank <= 3 ||
        IsStructType(typeName) || TryParseInlineArrayType(typeName, out _, out _) || typeName.EndsWith('*');

    private bool TryEvaluateConstantForOwner(Expression expression, TypeDeclaration owner, out ConstantValue value, out string? error)
    {
        var previous = _enumEvaluationOwner;
        _enumEvaluationOwner = owner;
        try { return TryEvaluateConstant(expression, out value, out error); }
        finally { _enumEvaluationOwner = previous; }
    }

    private string? ConstantExpressionTypeForOwner(Expression expression, TypeDeclaration owner)
    {
        var previous = _enumEvaluationOwner;
        _enumEvaluationOwner = owner;
        try { return ConstantExpressionType(expression); }
        finally { _enumEvaluationOwner = previous; }
    }

    private bool IsNonNullableStaticReferenceType(string typeName) =>
        _classTypes.ContainsKey(typeName) || _interfaceTypes.ContainsKey(typeName) ||
        TryParseManagedArrayType(typeName, out _, out _) && !typeName.EndsWith("?", StringComparison.Ordinal);

    private sealed record StaticAssignmentTransfer(bool IsContinue, string? Label, HashSet<bool> States);
    private sealed record StaticAssignmentFlow(HashSet<bool> NormalStates, bool HasUnassignedReturn,
        IReadOnlyList<StaticAssignmentTransfer> Transfers);

    private bool HasAssignmentOnAllNormalStaticConstructorExits(BlockStatement? body, TypeDeclaration owner, string fieldName)
    {
        if (body is null) return false;
        var flow = AnalyzeStaticAssignmentFlow(body, [false], new(StringComparer.Ordinal), owner, fieldName);
        return !flow.HasUnassignedReturn && flow.NormalStates.All(assigned => assigned);
    }

    private StaticAssignmentFlow AnalyzeStaticAssignmentFlow(Statement statement, HashSet<bool> incoming,
        HashSet<string> locals, TypeDeclaration owner, string fieldName)
    {
        switch (statement)
        {
            case BlockStatement block:
            {
                var scopedLocals = new HashSet<string>(locals, StringComparer.Ordinal);
                var states = incoming;
                var missing = false;
                var transfers = new List<StaticAssignmentTransfer>();
                foreach (var child in block.Statements)
                {
                    var childFlow = AnalyzeStaticAssignmentFlow(child, states, scopedLocals, owner, fieldName);
                    states = childFlow.NormalStates;
                    missing |= childFlow.HasUnassignedReturn;
                    transfers.AddRange(childFlow.Transfers);
                    if (child is VariableStatement variable) scopedLocals.Add(variable.Name);
                    if (child is VariableDeclarationStatement declaration)
                        foreach (var declared in declaration.Variables) scopedLocals.Add(declared.Name);
                }
                return new(states, missing, transfers);
            }
            case IfStatement conditional:
            {
                if (TryEvaluateConstant(conditional.Condition, out var constantCondition, out _) && constantCondition.Boolean is { } knownCondition)
                    return knownCondition
                        ? AnalyzeStaticAssignmentFlow(conditional.Then, new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName)
                        : conditional.Else is null
                            ? new(new(incoming), false, [])
                            : AnalyzeStaticAssignmentFlow(conditional.Else, new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName);
                var thenFlow = AnalyzeStaticAssignmentFlow(conditional.Then, new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName);
                var elseFlow = conditional.Else is null
                    ? new StaticAssignmentFlow(new(incoming), false, [])
                    : AnalyzeStaticAssignmentFlow(conditional.Else, new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName);
                return new(thenFlow.NormalStates.Union(elseFlow.NormalStates).ToHashSet(),
                    thenFlow.HasUnassignedReturn || elseFlow.HasUnassignedReturn,
                    [.. thenFlow.Transfers, .. elseFlow.Transfers]);
            }
            case SwitchStatement selection:
            {
                var states = new HashSet<bool>();
                var missing = false;
                var transfers = new List<StaticAssignmentTransfer>();
                foreach (var section in selection.Sections)
                {
                    var branch = AnalyzeStaticAssignmentFlow(new BlockStatement(section.Statements, section.At),
                        new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName);
                    states.UnionWith(branch.NormalStates);
                    missing |= branch.HasUnassignedReturn;
                    foreach (var transfer in branch.Transfers)
                    {
                        if (!transfer.IsContinue && transfer.Label is null) states.UnionWith(transfer.States);
                        else transfers.Add(transfer);
                    }
                }
                if (!selection.Sections.Any(section => section.Labels.Any(label => label.IsDefault))) states.UnionWith(incoming);
                return new(states, missing, transfers);
            }
            case TryStatement attempt:
            {
                var alternatives = new List<StaticAssignmentFlow>
                {
                    AnalyzeStaticAssignmentFlow(attempt.Body, new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName)
                };
                alternatives.AddRange(attempt.Catches.Select(clause => AnalyzeStaticAssignmentFlow(clause.Body,
                    new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName)));
                var states = alternatives.SelectMany(alternative => alternative.NormalStates).ToHashSet();
                var missing = alternatives.Any(alternative => alternative.HasUnassignedReturn);
                var transfers = alternatives.SelectMany(alternative => alternative.Transfers).ToList();
                if (attempt.Finally is not null)
                {
                    var normalFinally = AnalyzeStaticAssignmentFlow(attempt.Finally, states, new(locals, StringComparer.Ordinal), owner, fieldName);
                    states = normalFinally.NormalStates;
                    missing |= normalFinally.HasUnassignedReturn;
                    var pendingTransfers = transfers.ToArray();
                    transfers = [.. normalFinally.Transfers];
                    foreach (var transfer in pendingTransfers)
                    {
                        var transferFinally = AnalyzeStaticAssignmentFlow(attempt.Finally, transfer.States,
                            new(locals, StringComparer.Ordinal), owner, fieldName);
                        missing |= transferFinally.HasUnassignedReturn;
                        transfers.AddRange(transferFinally.Transfers);
                        if (transferFinally.NormalStates.Count > 0)
                            transfers.Add(transfer with { States = transferFinally.NormalStates });
                    }
                }
                return new(states, missing, transfers);
            }
            case WhileStatement loop:
            {
                return AnalyzeStaticLoopFlow(loop.Body, loop.Condition, loop.Label, incoming, locals, owner, fieldName);
            }
            case DoWhileStatement loop:
            {
                return AnalyzeStaticLoopFlow(loop.Body, loop.Condition, loop.Label, incoming, locals, owner, fieldName,
                    executesAtLeastOnce: true);
            }
            case ForStatement loop:
            {
                var initializer = loop.Initializer is null
                    ? new StaticAssignmentFlow(new(incoming), false, [])
                    : AnalyzeStaticAssignmentFlow(loop.Initializer, new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName);
                var loopLocals = new HashSet<string>(locals, StringComparer.Ordinal);
                if (loop.Initializer is VariableStatement variable) loopLocals.Add(variable.Name);
                if (loop.Initializer is VariableDeclarationStatement declaration)
                    foreach (var declared in declaration.Variables) loopLocals.Add(declared.Name);
                var loopFlow = AnalyzeStaticLoopFlow(loop.Body, loop.Condition, loop.Label, initializer.NormalStates,
                    loopLocals, owner, fieldName, iterators: loop.Iterators);
                return new(loopFlow.NormalStates, initializer.HasUnassignedReturn || loopFlow.HasUnassignedReturn,
                    [.. initializer.Transfers, .. loopFlow.Transfers]);
            }
            case ForInStatement loop:
            {
                var body = AnalyzeStaticAssignmentFlow(loop.Body, new(incoming), new(locals, StringComparer.Ordinal), owner, fieldName);
                var exits = new HashSet<bool>(incoming);
                exits.UnionWith(body.NormalStates);
                var transfers = new List<StaticAssignmentTransfer>();
                foreach (var transfer in body.Transfers)
                {
                    if (!transfer.IsContinue && (transfer.Label is null || transfer.Label == loop.Label)) exits.UnionWith(transfer.States);
                    else if (transfer.IsContinue && (transfer.Label is null || transfer.Label == loop.Label)) exits.UnionWith(transfer.States);
                    else transfers.Add(transfer);
                }
                return new(exits, body.HasUnassignedReturn, transfers);
            }
            case OverflowContextStatement overflow:
                return AnalyzeStaticAssignmentFlow(overflow.Body, incoming, new(locals, StringComparer.Ordinal), owner, fieldName);
            case ReturnStatement:
                return new([], incoming.Contains(false), []);
            case ThrowStatement:
                return new([], false, []);
            case ExpressionStatement { Value: BinaryExpression { Operator: "=", Left: var target } } when IsStaticFieldTarget(target, locals, owner, fieldName):
                return new(incoming.Select(_ => true).ToHashSet(), false, []);
            case MultiAssignmentStatement assignment:
                return new(assignment.Targets.Any(target => IsStaticFieldTarget(target, locals, owner, fieldName))
                    ? incoming.Select(_ => true).ToHashSet() : new(incoming), false, []);
            case BreakStatement transfer:
                return new([], false, [new(transfer.IsContinue, transfer.Label, new(incoming))]);
            default:
                return new(new(incoming), false, []);
        }
    }

    private StaticAssignmentFlow AnalyzeStaticLoopFlow(Statement body, Expression? condition, string? label,
        HashSet<bool> incoming, HashSet<string> locals, TypeDeclaration owner, string fieldName,
        IReadOnlyList<Expression>? iterators = null, bool executesAtLeastOnce = false)
    {
        bool? knownCondition = condition is null ? true :
            TryEvaluateConstant(condition, out var constant, out _) ? constant.Boolean : null;
        if (!executesAtLeastOnce && knownCondition == false)
            return new(new(incoming), false, []);

        var head = new HashSet<bool>(incoming);
        var exits = new HashSet<bool>();
        var transfers = new List<StaticAssignmentTransfer>();
        var missingReturn = false;
        var firstIteration = true;
        while (true)
        {
            var bodyFlow = AnalyzeStaticAssignmentFlow(body, new(head), new(locals, StringComparer.Ordinal), owner, fieldName);
            missingReturn |= bodyFlow.HasUnassignedReturn;
            var backStates = new HashSet<bool>(bodyFlow.NormalStates);
            var currentBreaks = new HashSet<bool>();
            var currentContinues = new HashSet<bool>();
            foreach (var transfer in bodyFlow.Transfers)
            {
                if (transfer.Label is null || transfer.Label == label)
                {
                    if (transfer.IsContinue) currentContinues.UnionWith(transfer.States);
                    else currentBreaks.UnionWith(transfer.States);
                }
                else transfers.Add(transfer);
            }
            backStates.UnionWith(currentContinues);
            if (iterators is not null && backStates.Count > 0)
            {
                foreach (var iterator in iterators)
                    backStates = AnalyzeStaticAssignmentFlow(new ExpressionStatement(iterator, iterator.Location), backStates,
                        new(locals, StringComparer.Ordinal), owner, fieldName).NormalStates;
            }

            exits.UnionWith(currentBreaks);
            if (knownCondition is null)
            {
                if (executesAtLeastOnce || !firstIteration) exits.UnionWith(backStates);
                else exits.UnionWith(head);
            }
            else if (knownCondition == false && executesAtLeastOnce)
            {
                exits.UnionWith(backStates);
            }

            if (knownCondition == false || backStates.Count == 0) break;
            var nextHead = new HashSet<bool>(head);
            nextHead.UnionWith(backStates);
            if (nextHead.SetEquals(head)) break;
            head = nextHead;
            firstIteration = false;
        }

        return new(exits, missingReturn, transfers);
    }

    private static bool IsStaticFieldTarget(Expression target, HashSet<string> locals, TypeDeclaration owner, string fieldName)
    {
        if (target is NameExpression name) return name.Name == fieldName && !locals.Contains(name.Name);
        return target is MemberAccessExpression access && access.Member == fieldName &&
            TryFlattenName(access.Receiver, out var receiver) && receiver == GetQualifiedTypeName(owner);
    }

    private static bool ContainsScopeAllocation(Expression expression) => expression switch
    {
        AllocationExpression allocation => allocation.IsScope || allocation.Dimensions.Any(ContainsScopeAllocation) ||
            allocation.Initializer is not null && allocation.Initializer.Elements.Any(ContainsScopeAllocation) ||
            (allocation.ConstructorArguments ?? []).Any(ContainsScopeAllocation),
        MemberAccessExpression access => ContainsScopeAllocation(access.Receiver),
        IndexExpression index => ContainsScopeAllocation(index.Receiver) || index.Indices.Any(ContainsScopeAllocation),
        CallExpression call => ContainsScopeAllocation(call.Target) || call.Arguments.Any(ContainsScopeAllocation),
        BinaryExpression binary => ContainsScopeAllocation(binary.Left) || ContainsScopeAllocation(binary.Right),
        UnaryExpression unary => ContainsScopeAllocation(unary.Operand),
        CastExpression cast => ContainsScopeAllocation(cast.Operand),
        NullAssertionExpression assertion => ContainsScopeAllocation(assertion.Operand),
        ConditionalExpression conditional => ContainsScopeAllocation(conditional.Condition) || ContainsScopeAllocation(conditional.WhenTrue) || ContainsScopeAllocation(conditional.WhenFalse),
        OverflowContextExpression overflow => ContainsScopeAllocation(overflow.Value),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(ContainsScopeAllocation),
        _ => false
    };

    private string DefaultLlvmValue(string type) => type == "string" ? "@luma_string_empty" :
        type == "string?" || type.EndsWith("?", StringComparison.Ordinal) &&
        (_classTypes.ContainsKey(type[..^1]) || _interfaceTypes.ContainsKey(type[..^1]) || _delegateTypes.ContainsKey(type[..^1]) || TryParseManagedArrayType(type[..^1], out _, out _)) ? "null" :
        type.EndsWith('*') || _classTypes.ContainsKey(type) || _interfaceTypes.ContainsKey(type) || _delegateTypes.ContainsKey(type) || TryParseManagedArrayType(type, out _, out _) ? "null" :
        type is "float" or "double" ? "0.0" : IsStructType(type) || TryParseInlineArrayType(type, out _, out _) ? "zeroinitializer" : "0";

    private bool TryInternStringLiteral(LiteralExpression literal, out StringLiteralInfo info)
    {
        if (_stringLiterals.TryGetValue(literal, out info!)) return true;
        if (!StringLiteralCodec.TryParse(literal.Value, out info!, out var error))
        {
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, error ?? "Malformed string literal.", literal.Location);
            return false;
        }
        _stringLiterals[literal] = info;
        _stringLiteralPool.TryAdd(info.Text, info);
        return true;
    }

    private bool TryInternStringText(string text, out StringLiteralInfo info)
    {
        if (_stringLiteralPool.TryGetValue(text, out info!)) return true;
        if (!StringLiteralCodec.TryCreate(text, out info!, out var error))
        {
            diagnostics.Add("LS0001", DiagnosticSeverity.Error, error ?? "String value cannot be represented as valid UTF-8.");
            return false;
        }
        _stringLiteralPool.Add(text, info);
        return true;
    }

    private static string LiteralType(LiteralExpression literal)
    {
        if (literal.Value.StartsWith('"') || literal.Value.StartsWith('@')) return "string";
        if (literal.Value is "true" or "false") return "bool";
        if (literal.Value.StartsWith('\'') && TryParseCharacter(literal.Value, out _)) return "char";
        if (IsFloatingLiteral(literal.Value)) return ParseFloating(literal.Value).Type;
        return ParseInteger(literal.Value).Type;
    }

    private string? ConstantExpressionType(Expression expression) => expression switch
    {
        LiteralExpression literal => LiteralType(literal) is "?" ? null : LiteralType(literal),
        NameExpression name when TryGetConstantField(name, out var binding) => ConstantFieldExpressionType(binding),
        MemberAccessExpression access when TryGetConstantField(access, out var binding) => ConstantFieldExpressionType(binding),
        UnaryExpression unary when unary.Operator is "+" or "-" => ConstantExpressionType(unary.Operand) is { } operand && NumericTypes.Contains(operand) ? PromoteSmallInteger(operand) : null,
        UnaryExpression { Operator: "!" } unary when ConstantExpressionType(unary.Operand) == "bool" => "bool",
        UnaryExpression { Operator: "~" } unary when ConstantExpressionType(unary.Operand) is { } operand && IntegerTypes.Contains(operand) => PromoteSmallInteger(operand),
        BinaryExpression binary when binary.Operator is "&&" or "||" && ConstantExpressionType(binary.Left) == "bool" && ConstantExpressionType(binary.Right) == "bool" => "bool",
        BinaryExpression binary when binary.Operator is "==" or "!=" or "<" or ">" or "<=" or ">=" &&
            ConstantExpressionType(binary.Left) is { } left && ConstantExpressionType(binary.Right) is { } right &&
            (left == right && (left != "string" || binary.Operator is "==" or "!=") || NumericTypes.Contains(left) && NumericTypes.Contains(right)) => "bool",
        BinaryExpression binary when binary.Operator is "&" or "|" or "^" && ConstantExpressionType(binary.Left) == "bool" && ConstantExpressionType(binary.Right) == "bool" => "bool",
        BinaryExpression binary when binary.Operator is "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" &&
            ConstantExpressionType(binary.Left) is { } left && ConstantExpressionType(binary.Right) is { } right && NumericTypes.Contains(left) && NumericTypes.Contains(right) => NumericPromotionType(left, right),
        BinaryExpression { Operator: "+" } binary when ConstantExpressionType(binary.Left) == "string" && ConstantExpressionType(binary.Right) == "string" => "string",
        BinaryExpression binary when binary.Operator is "<<" or ">>" && ConstantExpressionType(binary.Left) is { } left &&
            ConstantExpressionType(binary.Right) is { } right && IntegerTypes.Contains(left) && IntegerTypes.Contains(right) => PromoteSmallInteger(left),
        ConditionalExpression conditional when ConstantExpressionType(conditional.Condition) == "bool" &&
            ConstantExpressionType(conditional.WhenTrue) is { } whenTrue && ConstantExpressionType(conditional.WhenFalse) == whenTrue => whenTrue,
        OverflowContextExpression overflow => ConstantExpressionType(overflow.Value),
        SizeOfExpression sizeOf when sizeOf.TypeName != "void" && IsSupportedType(_sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName)) => "int32",
        DefaultValueExpression defaultValue when defaultValue.TypeName is { } type && IsSupportedType(type) => type,
        CastExpression cast when ConstantExpressionType(cast.Operand) is { } operand && NumericTypes.Contains(operand) && NumericTypes.Contains(cast.TypeName) => cast.TypeName,
        _ => null
    };

    private bool TryEvaluateConstant(Expression expression, out ConstantValue value, out string? error, bool checkedContext = true)
    {
        error = null;
        switch (expression)
        {
            case NameExpression name when TryGetConstantField(name, out var nameBinding):
                return TryEvaluateStoredConstant(nameBinding, out value, out error);
            case MemberAccessExpression access when TryGetConstantField(access, out var accessBinding):
                return TryEvaluateStoredConstant(accessBinding, out value, out error);
            case LiteralExpression literal:
                if (literal.Value.StartsWith('"') || literal.Value.StartsWith('@'))
                {
                    if (StringLiteralCodec.TryParse(literal.Value, out var stringLiteral, out error))
                    { value = new("string", Text: stringLiteral.Text); return true; }
                    value = null!; return false;
                }
                if (literal.Value is "true" or "false") { value = new("bool", Boolean: literal.Value == "true"); return true; }
                if (literal.Value.StartsWith('\'') && TryParseCharacter(literal.Value, out var character)) { value = new("char", Integer: character); return true; }
                if (IsFloatingLiteral(literal.Value))
                {
                    var floating = ParseFloating(literal.Value);
                    if (floating.Type != "?") { value = new(floating.Type, Floating: floating.Value); return true; }
                }
                else
                {
                    var integer = ParseInteger(literal.Value);
                    if (integer.Type != "?" && integer.Value is { } parsed) { value = new(integer.Type, Integer: parsed); return true; }
                }
                value = null!; error = "Constant expression contains a malformed literal."; return false;
            case DefaultValueExpression defaultValue:
                if (_defaultTypes.TryGetValue(defaultValue, out var defaultType) && (IntegerTypes.Contains(defaultType) || defaultType == "char")) { value = new(defaultType, Integer: BigInteger.Zero); return true; }
                if (_defaultTypes.TryGetValue(defaultValue, out defaultType) && defaultType == "bool") { value = new("bool", Boolean: false); return true; }
                if (_defaultTypes.TryGetValue(defaultValue, out defaultType) && defaultType == "string") { value = new("string", Text: ""); return true; }
                if (_defaultTypes.TryGetValue(defaultValue, out defaultType) && FloatingTypes.Contains(defaultType)) { value = new(defaultType, Floating: 0.0); return true; }
                if (_defaultTypes.TryGetValue(defaultValue, out defaultType) && _enumUnderlyingTypes.TryGetValue(defaultType, out var defaultEnumUnderlying)) { value = new(defaultEnumUnderlying, Integer: BigInteger.Zero); return true; }
                value = null!; error = "Only primitive default values are supported in constants."; return false;
            case SizeOfExpression sizeOf when ConstantExpressionType(sizeOf) is not null:
                var sizeType = _sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName);
                value = new("int32", Integer: GetTypeSize(sizeType));
                return true;
            case OverflowContextExpression overflow:
                return TryEvaluateConstant(overflow.Value, out value, out error, overflow.Checked);
            case CastExpression cast:
                if (!TryEvaluateConstant(cast.Operand, out var castOperand, out error, checkedContext)) { value = null!; return false; }
                if ((IntegerTypes.Contains(cast.TypeName) || cast.TypeName == "char") && castOperand.Integer is { } integerOperand)
                    return FinishInteger(integerOperand, cast.TypeName, checkedContext, out value, out error);
                if ((IntegerTypes.Contains(cast.TypeName) || cast.TypeName == "char") && castOperand.Floating is { } floatingOperand)
                {
                    var truncated = Math.Truncate(floatingOperand); var width = Width(cast.TypeName); var unsigned = IsUnsigned(cast.TypeName);
                    var lower = unsigned ? 0.0 : -Math.Pow(2.0, width - 1); var upper = Math.Pow(2.0, unsigned ? width : width - 1);
                    if (!double.IsFinite(truncated) || truncated < lower || truncated >= upper)
                    {
                        if (checkedContext) { value = null!; error = $"Constant conversion overflows '{cast.TypeName}'."; return false; }
                        value = new(cast.TypeName, Integer: BigInteger.Zero); return true;
                    }
                    return FinishInteger(new BigInteger(truncated), cast.TypeName, checkedContext, out value, out error);
                }
                if (cast.TypeName is "float" or "double")
                {
                    var number = castOperand.Floating ?? (double)(castOperand.Integer ?? BigInteger.Zero);
                    if (cast.TypeName == "float") number = (float)number;
                    value = new(cast.TypeName, Floating: number); return true;
                }
                value = null!; error = $"Constant conversion to '{cast.TypeName}' is not supported."; return false;
            case ConditionalExpression conditional:
                if (!TryEvaluateConstant(conditional.Condition, out var condition, out error, checkedContext) || condition.Boolean is null)
                { value = null!; error ??= "A constant conditional requires a bool condition."; return false; }
                return TryEvaluateConstant(condition.Boolean.Value ? conditional.WhenTrue : conditional.WhenFalse, out value, out error, checkedContext);
            case UnaryExpression unary:
                if (!TryEvaluateConstant(unary.Operand, out var operand, out error, checkedContext)) { value = null!; return false; }
                if (unary.Operator == "!" && operand.Boolean is { } boolean) { value = new("bool", Boolean: !boolean); return true; }
                if (unary.Operator is "+" or "-" && operand.Floating is { } floatValue)
                { value = new(operand.TypeName, Floating: unary.Operator == "-" ? -floatValue : floatValue); return true; }
                if (unary.Operator is "+" or "-" or "~" && operand.Integer is { } integerValue)
                {
                    var resultType = PromoteSmallInteger(operand.TypeName);
                    var result = unary.Operator switch { "-" => -integerValue, "~" => ~integerValue, _ => integerValue };
                    return FinishInteger(result, resultType, checkedContext, out value, out error);
                }
                value = null!; error = $"Unary operator '{unary.Operator}' is not valid in this constant expression."; return false;
            case BinaryExpression binary:
                if (!TryEvaluateConstant(binary.Left, out var left, out error, checkedContext) || !TryEvaluateConstant(binary.Right, out var right, out error, checkedContext)) { value = null!; return false; }
                var resultTypeName = ConstantExpressionType(binary);
                if (resultTypeName is null) { value = null!; error = $"Operator '{binary.Operator}' is not valid in a primitive constant expression."; return false; }
                if (binary.Operator is "&&" or "||" && left.Boolean is { } leftBool && right.Boolean is { } rightBool)
                { value = new("bool", Boolean: binary.Operator == "&&" ? leftBool && rightBool : leftBool || rightBool); return true; }
                if (binary.Operator is "&" or "|" or "^" && left.Boolean is { } leftBit && right.Boolean is { } rightBit)
                {
                    value = new("bool", Boolean: binary.Operator switch { "&" => leftBit & rightBit, "|" => leftBit | rightBit, _ => leftBit ^ rightBit }); return true;
                }
                if (left.Text is { } leftText && right.Text is { } rightText)
                {
                    if (binary.Operator == "+") { value = new("string", Text: leftText + rightText); return true; }
                    if (binary.Operator is "==" or "!=") { value = new("bool", Boolean: binary.Operator == "==" ? leftText == rightText : leftText != rightText); return true; }
                    value = null!; error = $"String operator '{binary.Operator}' is not valid in a constant expression."; return false;
                }
                if (binary.Operator is "==" or "!=" or "<" or ">" or "<=" or ">=")
                { value = new("bool", Boolean: CompareConstantValues(binary.Operator, left, right)); return true; }
                if (left.Floating is not null || right.Floating is not null)
                {
                    var leftNumber = left.Floating ?? (double)(left.Integer ?? BigInteger.Zero);
                    var rightNumber = right.Floating ?? (double)(right.Integer ?? BigInteger.Zero);
                    var result = binary.Operator switch { "+" => leftNumber + rightNumber, "-" => leftNumber - rightNumber, "*" => leftNumber * rightNumber, "/" => leftNumber / rightNumber, "%" => leftNumber % rightNumber, _ => double.NaN };
                    if (!double.IsFinite(result)) { value = null!; error = "Floating constant expression produced a non-finite value."; return false; }
                    if (resultTypeName == "float") result = (float)result;
                    value = new(resultTypeName, Floating: result); return true;
                }
                if (left.Integer is { } leftInteger && right.Integer is { } rightInteger)
                {
                    var width = Width(resultTypeName);
                    var shift = (int)(rightInteger & Math.Max(0, width - 1));
                    var integerResult = binary.Operator switch
                    {
                        "+" => leftInteger + rightInteger, "-" => leftInteger - rightInteger, "*" => leftInteger * rightInteger,
                        "/" when rightInteger != 0 => leftInteger / rightInteger, "%" when rightInteger != 0 => leftInteger % rightInteger,
                        "&" => leftInteger & rightInteger, "|" => leftInteger | rightInteger, "^" => leftInteger ^ rightInteger,
                        "<<" => leftInteger << shift, ">>" => leftInteger >> shift,
                        _ => BigInteger.Zero
                    };
                    if (binary.Operator is "/" or "%" && rightInteger == 0) { value = null!; error = "Division by zero in constant expression."; return false; }
                    return FinishInteger(integerResult, resultTypeName, checkedContext, out value, out error);
                }
                value = null!; error = $"Operator '{binary.Operator}' cannot be evaluated for these constant operands."; return false;
            default:
                value = null!; error = "Expression is not a primitive constant expression."; return false;
        }
    }

    private static bool CompareConstantValues(string op, ConstantValue left, ConstantValue right)
    {
        if (left.Text is { } leftText && right.Text is { } rightText)
            return op switch { "==" => leftText == rightText, "!=" => leftText != rightText, _ => false };
        if (left.Boolean is { } leftBoolean && right.Boolean is { } rightBoolean)
            return op switch { "==" => leftBoolean == rightBoolean, "!=" => leftBoolean != rightBoolean, _ => false };
        if (left.Floating is { } || right.Floating is { })
        {
            var leftFloat = left.Floating ?? (double)(left.Integer ?? BigInteger.Zero);
            var rightFloat = right.Floating ?? (double)(right.Integer ?? BigInteger.Zero);
            return op switch { "==" => leftFloat == rightFloat, "!=" => leftFloat != rightFloat, "<" => leftFloat < rightFloat, ">" => leftFloat > rightFloat, "<=" => leftFloat <= rightFloat, _ => leftFloat >= rightFloat };
        }
        var leftInteger = left.Integer ?? BigInteger.Zero; var rightInteger = right.Integer ?? BigInteger.Zero;
        return op switch { "==" => leftInteger == rightInteger, "!=" => leftInteger != rightInteger, "<" => leftInteger < rightInteger, ">" => leftInteger > rightInteger, "<=" => leftInteger <= rightInteger, _ => leftInteger >= rightInteger };
    }

    private bool FinishInteger(BigInteger integer, string type, bool checkedContext, out ConstantValue value, out string? error)
    {
        error = null; value = null!;
        if (!IntegerTypes.Contains(type) && type != "char") { error = $"Integer result is not valid for type '{type}'."; return false; }
        if (Fits(integer, type)) { value = new(type, Integer: integer); return true; }
        if (checkedContext) { error = $"Constant arithmetic overflows '{type}'."; return false; }
        var width = Width(type); var modulus = BigInteger.One << width; var wrapped = ((integer % modulus) + modulus) % modulus;
        if (!IsUnsigned(type) && wrapped >= (BigInteger.One << (width - 1))) wrapped -= modulus;
        value = new(type, Integer: wrapped); return true;
    }

    private bool TryConvertConstant(ConstantValue value, string targetType, Expression source, out string literal)
    {
        literal = "";
        if (_enumUnderlyingTypes.TryGetValue(targetType, out var underlyingType)) targetType = underlyingType;
        if (targetType == "string" && value.Text is { } stringValue && value.TypeName == "string" && TryInternStringText(stringValue, out var stringLiteral))
        { literal = "@" + stringLiteral.Symbol; return true; }
        if (targetType == "bool" && value.Boolean is { } boolean) { literal = boolean ? "1" : "0"; return true; }
        if ((IntegerTypes.Contains(targetType) || targetType == "char") && value.Integer is { } integer)
        {
            if (Fits(integer, targetType)) { literal = integer.ToString(System.Globalization.CultureInfo.InvariantCulture); return true; }
            return false;
        }
        if (targetType is "float" or "double")
        {
            if (!CanConvert(value.TypeName, targetType, source)) return false;
            var number = value.Floating ?? (value.Integer is { } integerValue ? (double)integerValue : double.NaN);
            if (!double.IsFinite(number)) return false;
            if (targetType == "float") number = (float)number;
            literal = number.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
            if (!literal.Contains('.') && !literal.Contains('e') && !literal.Contains('E')) literal += ".0";
            return true;
        }
        return false;
    }

    private static bool IsSupportedEntryPoint(MethodSymbol method)
    {
        if (!method.Declaration.IsStatic || !method.Declaration.IsPublic || method.Declaration.IsExtern ||
            method.Declaration.HasTypeParameters || method.Owner.HasTypeParameters || method.Declaration.ReturnType is not ("void" or "int32"))
            return false;

        var parameters = method.Declaration.Parameters;
        return parameters.Count == 0 || parameters is
        [
            { TypeName: "string[]", PassingMode: "", DefaultValue: null, IsReadOnly: false, IsVariadic: false, IsScoped: false }
        ];
    }

    private void AnalyzeMethod(MethodSymbol method)
    {
        if (method.Declaration.Body is null) return;
        _catchDepth = 0;
        _finallyDepth = 0;
        _finallyScopes.Clear();
        var context = new MethodContext(method);
        foreach (var parameter in method.Declaration.Parameters)
        {
            context.Declare(parameter.Name, parameter.TypeName, parameter.PassingMode != "in" && !parameter.IsReadOnly,
                parameter.Location, scopeBoundDepth: parameter.IsScoped && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) ? context.ScopeDepth : null,
                isReadOnlyReference: parameter.PassingMode == "in" && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) || parameter.IsReadOnly);
            if (IsNullableType(parameter.TypeName)) context.SetNullState(parameter.Name, NullState.MaybeNull);
        }
        if (method.IsClassConstructor) BindConstructorInitializer(method, context);
        if (method.IsStructConstructor && _structTypes.TryGetValue(GetQualifiedTypeName(method.Owner), out var structTypeWithInitializers))
        {
            foreach (var field in structTypeWithInitializers.Fields.Where(field => field.Declaration.Initializer is not null))
            {
                var initializer = field.Declaration.Initializer!;
                var previousInitializerState = _isAnalyzingFieldInitializer;
                _isAnalyzingFieldInitializer = true;
                string actualType;
                try { actualType = AnalyzeExpression(initializer, context, field.TypeName); }
                finally { _isAnalyzingFieldInitializer = previousInitializerState; }
                if (!CanConvert(actualType, field.TypeName, initializer))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert struct field initializer from '{actualType}' to '{field.TypeName}'.", initializer.Location);
            }
        }
        if (method.IsClassConstructor && ReferenceEquals(_methods.First(candidate => candidate.IsClassConstructor && candidate.Owner == method.Owner), method) &&
            _classTypes.TryGetValue(GetQualifiedTypeName(method.Owner), out var classType))
        {
            foreach (var field in classType.Fields.Where(field => GetQualifiedTypeName(field.Owner) == GetQualifiedTypeName(method.Owner) && field.Declaration.Initializer is not null))
            {
                var initializer = field.Declaration.Initializer!;
                var previousInitializerState = _isAnalyzingFieldInitializer;
                _isAnalyzingFieldInitializer = true;
                string actualType;
                try { actualType = AnalyzeExpression(initializer, context, field.TypeName); }
                finally { _isAnalyzingFieldInitializer = previousInitializerState; }
                if (!CanConvert(actualType, field.TypeName, initializer))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert class field initializer from '{actualType}' to '{field.TypeName}'.", initializer.Location);
            }
        }
        AnalyzeStatement(method.Declaration.Body, context);
        if (method.IsClassFinalizer) ValidateFinalizerThisEscape(method);
        if (!method.IsStructConstructor && method.Declaration.ReturnType != "void" && !AlwaysReturns(method.Declaration.Body))
            diagnostics.Add("LS0630", DiagnosticSeverity.Error, $"Not all code paths in '{method.Declaration.Name}' return a value.", method.Declaration.Location);
    }

    private void BindConstructorInitializer(MethodSymbol method, MethodContext context)
    {
        var ownerName = GetQualifiedTypeName(method.Owner);
        var source = method.SourceConstructor;
        var initializerKind = source?.InitializerKind;
        var classType = _classTypes.GetValueOrDefault(ownerName);
        var delegatesToThis = initializerKind == "this";
        string? targetType;
        if (delegatesToThis) targetType = ownerName;
        else if (initializerKind == "base" || initializerKind is null) targetType = classType?.BaseTypeName;
        else
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Unknown constructor initializer '{initializerKind}'.", source!.Location);
            return;
        }

        if (initializerKind == "base" && targetType is null)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Class '{ownerName}' has no base class to initialize.", source!.Location);
            return;
        }
        if (targetType is null) return;

        var arguments = (IReadOnlyList<Expression>)(source?.InitializerArguments ?? []);
        foreach (var argument in arguments)
            if (ContainsThisReference(argument))
                diagnostics.Add("LS0812", DiagnosticSeverity.Error, "A constructor initializer argument cannot refer to 'this' or 'base'.", argument.Location);

        var location = source?.Location ?? method.Declaration.Location;
        var call = new CallExpression(new NameExpression("$constructor", location), arguments, location,
            source?.InitializerArgumentNames, source?.InitializerArgumentModes);
        var candidates = _methods.Where(candidate => candidate.IsClassConstructor && GetQualifiedTypeName(candidate.Owner) == targetType &&
            (delegatesToThis || IsConstructorAccessible(candidate, method.Owner))).ToArray();
        var noMatchCode = delegatesToThis ? "LS0751" : "LS0811";
        if (SelectConstructor(call, candidates, context, targetType, noMatchCode) is not null)
            _constructorInitializers[method] = new(call, delegatesToThis);
    }

    private static bool ContainsThisReference(Expression expression) => expression switch
    {
        ThisExpression or BaseExpression => true,
        UnaryExpression unary => ContainsThisReference(unary.Operand),
        CastExpression cast => ContainsThisReference(cast.Operand),
        BinaryExpression binary => ContainsThisReference(binary.Left) || ContainsThisReference(binary.Right),
        ConditionalExpression conditional => ContainsThisReference(conditional.Condition) || ContainsThisReference(conditional.WhenTrue) || ContainsThisReference(conditional.WhenFalse),
        MemberAccessExpression member => ContainsThisReference(member.Receiver),
        IndexExpression index => ContainsThisReference(index.Receiver) || index.Indices.Any(ContainsThisReference),
        CallExpression call => ContainsThisReference(call.Target) || call.Arguments.Any(ContainsThisReference),
        AllocationExpression allocation => allocation.Dimensions.Any(ContainsThisReference) || (allocation.ConstructorArguments?.Any(ContainsThisReference) ?? false) ||
            (allocation.Initializer?.Elements.Any(ContainsThisReference) ?? false),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(ContainsThisReference),
        NullAssertionExpression assertion => ContainsThisReference(assertion.Operand),
        OverflowContextExpression overflow => ContainsThisReference(overflow.Value),
        _ => false
    };

    private void ValidateConstructorDelegationCycles()
    {
        var state = new Dictionary<MethodSymbol, byte>(ReferenceEqualityComparer.Instance);
        var stack = new List<MethodSymbol>();
        var reported = new HashSet<MethodSymbol>(ReferenceEqualityComparer.Instance);
        void Visit(MethodSymbol constructor)
        {
            state[constructor] = 1;
            stack.Add(constructor);
            if (_constructorInitializers.TryGetValue(constructor, out var initializer) && initializer.DelegatesToThis &&
                _callTargets.TryGetValue(initializer.Call, out var target))
            {
                if (!state.TryGetValue(target, out var targetState)) Visit(target);
                else if (targetState == 1)
                {
                    var cycleStart = stack.FindIndex(candidate => ReferenceEquals(candidate, target));
                    foreach (var member in stack.Skip(Math.Max(cycleStart, 0)))
                        if (reported.Add(member))
                            diagnostics.Add("LMC3014", DiagnosticSeverity.Error, $"Constructor delegation cycle in '{GetQualifiedTypeName(member.Owner)}'.", member.SourceConstructor?.Location ?? member.Declaration.Location);
                }
            }
            stack.RemoveAt(stack.Count - 1);
            state[constructor] = 2;
        }

        foreach (var constructor in _methods.Where(method => method.IsClassConstructor))
            if (!state.ContainsKey(constructor)) Visit(constructor);
    }

    private void ValidateConstructorThisUsage()
    {
        foreach (var constructor in _methods.Where(method => method.IsClassConstructor && method.Declaration.Body is not null))
        {
            if (_constructorInitializers.TryGetValue(constructor, out var initializer) && initializer.DelegatesToThis) continue;
            var ownerType = GetQualifiedTypeName(constructor.Owner);
            if (!_classTypes.TryGetValue(ownerType, out var classType)) continue;
            var required = classType.Fields.Where(field => GetQualifiedTypeName(field.Owner) == ownerType && field.Declaration.Initializer is null &&
                    (IsClassReferenceType(field.TypeName) && !IsNullableClassType(field.TypeName) ||
                     TryParseManagedArrayType(field.TypeName, out _, out _) && !IsNullableArrayType(field.TypeName)))
                .Select(field => field.Declaration.Name).ToHashSet(StringComparer.Ordinal);
            if (required.Count == 0) continue;
            var constructorContext = new MethodContext(constructor);
            foreach (var parameter in constructor.Declaration.Parameters)
                constructorContext.Declare(parameter.Name, parameter.TypeName, parameter.PassingMode != "in" && !parameter.IsReadOnly,
                    parameter.Location, scopeBoundDepth: parameter.IsScoped && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) ? constructorContext.ScopeDepth : null,
                    isReadOnlyReference: parameter.PassingMode == "in" && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) || parameter.IsReadOnly);

            void Report(SourceLocation location, string message) =>
                diagnostics.Add("LS0812", DiagnosticSeverity.Error, message, location);

            string? DirectCurrentFieldWrite(Expression target) => target switch
            {
                NameExpression name when _boundUnqualifiedClassFields.TryGetValue(name, out var field) && field.OwnerType == ownerType => name.Name,
                MemberAccessExpression { Receiver: ThisExpression } member when _boundClassFields.TryGetValue(member, out var field) && field.OwnerType == ownerType => member.Member,
                _ => null
            };

            void CheckExpression(Expression expression, HashSet<string> assigned)
            {
                var allAssigned = required.IsSubsetOf(assigned);
                switch (expression)
                {
                    case ThisExpression value:
                        if (!allAssigned) Report(value.Location, "'this' cannot be used as a value until all non-nullable reference fields declared by this class are assigned.");
                        break;
                    case LambdaExpression lambda when LambdaBodyReferencesThis(lambda) || LambdaReferencesImplicitInstanceMember(lambda, constructorContext):
                        if (!allAssigned) Report(lambda.Location, "'this' cannot be captured by a lambda until all non-nullable reference fields declared by this class are assigned.");
                        break;
                    case BaseExpression value:
                        if (!allAssigned) Report(value.Location, "'base' members cannot be used until all non-nullable reference fields declared by this class are assigned.");
                        break;
                    case NameExpression name:
                        if (!allAssigned && required.Contains(name.Name) &&
                            _boundUnqualifiedClassFields.TryGetValue(name, out var unqualified) && unqualified.OwnerType == ownerType && !assigned.Contains(name.Name))
                            Report(name.Location, $"Field '{name.Name}' cannot be read until it is assigned in this constructor.");
                        break;
                    case MemberAccessExpression member:
                        if (_boundClassFields.TryGetValue(member, out var field) && field.OwnerType == ownerType && member.Receiver is ThisExpression)
                        {
                            if (!allAssigned && required.Contains(member.Member) && !assigned.Contains(member.Member))
                                Report(member.Location, $"Field '{member.Member}' cannot be read until it is assigned in this constructor.");
                            break;
                        }
                        if (member.Receiver is BaseExpression)
                        {
                            if (!allAssigned) Report(member.Location, "'base' members cannot be used until all non-nullable reference fields declared by this class are assigned.");
                            break;
                        }
                        if (member.Receiver is ThisExpression && !allAssigned)
                        {
                            Report(member.Location, "Instance members cannot be used until all non-nullable reference fields declared by this class are assigned.");
                            break;
                        }
                        CheckExpression(member.Receiver, assigned);
                        break;
                    case CallExpression call:
                        var implicitThisCall = _callTargets.TryGetValue(call, out var callTarget) && callTarget.IsClassInstanceMethod && call.Target is NameExpression;
                        var explicitThisCall = call.Target is MemberAccessExpression { Receiver: ThisExpression or BaseExpression };
                        if (!allAssigned && (implicitThisCall || explicitThisCall))
                            Report(call.Location, "Instance methods cannot be called until all non-nullable reference fields declared by this class are assigned.");
                        else if (!explicitThisCall) CheckExpression(call.Target, assigned);
                        foreach (var argument in call.Arguments) CheckExpression(argument, assigned);
                        break;
                    case BinaryExpression { Operator: "=" } assignment when DirectCurrentFieldWrite(assignment.Left) is { } writtenField:
                        CheckExpression(assignment.Right, assigned);
                        assigned.Add(writtenField);
                        break;
                    case UnaryExpression unary: CheckExpression(unary.Operand, assigned); break;
                    case CastExpression cast: CheckExpression(cast.Operand, assigned); break;
                    case BinaryExpression binary: CheckExpression(binary.Left, assigned); CheckExpression(binary.Right, assigned); break;
                    case ConditionalExpression conditional:
                        CheckExpression(conditional.Condition, assigned);
                        var trueState = new HashSet<string>(assigned, StringComparer.Ordinal);
                        var falseState = new HashSet<string>(assigned, StringComparer.Ordinal);
                        CheckExpression(conditional.WhenTrue, trueState);
                        CheckExpression(conditional.WhenFalse, falseState);
                        trueState.IntersectWith(falseState);
                        assigned.IntersectWith(trueState);
                        assigned.UnionWith(trueState);
                        break;
                    case IndexExpression index:
                        CheckExpression(index.Receiver, assigned);
                        foreach (var subscript in index.Indices) CheckExpression(subscript, assigned);
                        break;
                    case AllocationExpression allocation:
                        foreach (var dimension in allocation.Dimensions) CheckExpression(dimension, assigned);
                        foreach (var argument in allocation.ConstructorArguments ?? []) CheckExpression(argument, assigned);
                        if (allocation.Initializer is { } initializerExpression) CheckExpression(initializerExpression, assigned);
                        break;
                    case InlineArrayInitializerExpression inlineInitializer:
                        foreach (var element in inlineInitializer.Elements) CheckExpression(element, assigned);
                        break;
                    case NullAssertionExpression assertion: CheckExpression(assertion.Operand, assigned); break;
                    case OverflowContextExpression overflow: CheckExpression(overflow.Value, assigned); break;
                }
            }

            void CheckStatement(Statement statement, HashSet<string> assigned)
            {
                switch (statement)
                {
                    case BlockStatement block:
                        foreach (var child in block.Statements)
                        {
                            CheckStatement(child, assigned);
                            if (AlwaysReturns(child)) break;
                        }
                        break;
                    case ExpressionStatement expression:
                        CheckExpression(expression.Value, assigned);
                        break;
                    case VariableStatement variable when variable.Initializer is not null:
                        CheckExpression(variable.Initializer, assigned);
                        break;
                    case VariableDeclarationStatement declaration:
                        foreach (var variable in declaration.Variables)
                            if (variable.Initializer is not null) CheckExpression(variable.Initializer, assigned);
                        break;
                    case MultiAssignmentStatement assignment:
                        foreach (var value in assignment.Values) CheckExpression(value, assigned);
                        foreach (var target in assignment.Targets) CheckExpression(target, assigned);
                        break;
                    case ReturnStatement { Value: { } value }:
                        CheckExpression(value, assigned);
                        break;
                    case ThrowStatement { Value: { } value }:
                        CheckExpression(value, assigned);
                        break;
                    case IfStatement conditional:
                        CheckExpression(conditional.Condition, assigned);
                        var thenState = new HashSet<string>(assigned, StringComparer.Ordinal);
                        var elseState = new HashSet<string>(assigned, StringComparer.Ordinal);
                        CheckStatement(conditional.Then, thenState);
                        if (conditional.Else is not null) CheckStatement(conditional.Else, elseState);
                        var thenContinues = !AlwaysReturns(conditional.Then);
                        var elseContinues = conditional.Else is null || !AlwaysReturns(conditional.Else);
                        if (thenContinues && elseContinues)
                        {
                            thenState.IntersectWith(elseState);
                            assigned.Clear();
                            assigned.UnionWith(thenState);
                        }
                        else if (thenContinues)
                        {
                            assigned.Clear();
                            assigned.UnionWith(thenState);
                        }
                        else if (elseContinues)
                        {
                            assigned.Clear();
                            assigned.UnionWith(elseState);
                        }
                        break;
                    case SwitchStatement selection:
                        CheckExpression(selection.Value, assigned);
                        var states = new List<HashSet<string>>();
                        foreach (var section in selection.Sections)
                        {
                            var sectionState = new HashSet<string>(assigned, StringComparer.Ordinal);
                            foreach (var label in section.Labels)
                                if (label.Guard is not null) CheckExpression(label.Guard, sectionState);
                            foreach (var child in section.Statements) CheckStatement(child, sectionState);
                            states.Add(sectionState);
                        }
                        if (!selection.Sections.Any(section => section.Labels.Any(label => label.IsDefault)))
                            states.Add(new HashSet<string>(assigned, StringComparer.Ordinal));
                        if (states.Count > 0)
                        {
                            var common = states[0];
                            foreach (var state in states.Skip(1)) common.IntersectWith(state);
                            assigned.IntersectWith(common);
                            assigned.UnionWith(common);
                        }
                        break;
                    case WhileStatement loop:
                        CheckExpression(loop.Condition, assigned);
                        CheckStatement(loop.Body, new HashSet<string>(assigned, StringComparer.Ordinal));
                        break;
                    case DoWhileStatement loop:
                        var doState = new HashSet<string>(assigned, StringComparer.Ordinal);
                        CheckStatement(loop.Body, doState);
                        CheckExpression(loop.Condition, doState);
                        break;
                    case ForStatement loop:
                        if (loop.Initializer is not null) CheckStatement(loop.Initializer, assigned);
                        if (loop.Condition is not null) CheckExpression(loop.Condition, assigned);
                        var forState = new HashSet<string>(assigned, StringComparer.Ordinal);
                        CheckStatement(loop.Body, forState);
                        foreach (var iterator in loop.Iterators) CheckExpression(iterator, forState);
                        break;
                    case ForInStatement loop:
                        CheckExpression(loop.Collection, assigned);
                        CheckStatement(loop.Body, new HashSet<string>(assigned, StringComparer.Ordinal));
                        break;
                    case TryStatement tryStatement:
                        var tryState = new HashSet<string>(assigned, StringComparer.Ordinal);
                        CheckStatement(tryStatement.Body, tryState);
                        var completionStates = new List<HashSet<string>> { tryState };
                        foreach (var clause in tryStatement.Catches)
                        {
                            var catchState = new HashSet<string>(assigned, StringComparer.Ordinal);
                            CheckStatement(clause.Body, catchState);
                            completionStates.Add(catchState);
                        }
                        if (tryStatement.Catches.Count == 0) completionStates.Add(new HashSet<string>(assigned, StringComparer.Ordinal));
                        var merged = completionStates[0];
                        foreach (var state in completionStates.Skip(1)) merged.IntersectWith(state);
                        if (tryStatement.Finally is not null) CheckStatement(tryStatement.Finally, merged);
                        assigned.IntersectWith(merged);
                        assigned.UnionWith(merged);
                        break;
                    case OverflowContextStatement overflow:
                        CheckStatement(overflow.Body, assigned);
                        break;
                }
            }

            CheckStatement(constructor.Declaration.Body!, new HashSet<string>(StringComparer.Ordinal));
        }
    }

    private void ValidateFinalizerThisEscape(MethodSymbol method)
    {
        var context = new MethodContext(method);
        foreach (var parameter in method.Declaration.Parameters)
            context.Declare(parameter.Name, parameter.TypeName, parameter.PassingMode != "in" && !parameter.IsReadOnly, parameter.Location,
                scopeBoundDepth: parameter.IsScoped && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) ? context.ScopeDepth : null,
                isReadOnlyReference: parameter.PassingMode == "in" && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) || parameter.IsReadOnly);
        void CheckExpression(Expression expression)
        {
            switch (expression)
            {
                case ThisExpression value:
                    diagnostics.Add("LS0701", DiagnosticSeverity.Error, "'this' cannot escape a finalizer.", value.Location);
                    break;
                case LambdaExpression lambda when LambdaBodyReferencesThis(lambda) || LambdaReferencesImplicitInstanceMember(lambda, context):
                    diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A lambda in a finalizer cannot capture 'this'.", lambda.Location);
                    break;
                case MemberAccessExpression member:
                    if (member.Receiver is not ThisExpression) CheckExpression(member.Receiver);
                    break;
                case CallExpression call:
                    if (call.Target is not MemberAccessExpression { Receiver: ThisExpression }) CheckExpression(call.Target);
                    for (var index = 0; index < call.Arguments.Count; index++)
                    {
                        var isReadOnlyParameter = _callTargets.TryGetValue(call, out var target) && _callArguments.TryGetValue(call, out var binding) &&
                            index < binding.ParameterBySource.Count && (target.Declaration.Parameters[binding.ParameterBySource[index]].PassingMode == "in" ||
                                target.Declaration.Parameters[binding.ParameterBySource[index]].IsReadOnly);
                        if (isReadOnlyParameter && call.Arguments[index] is ThisExpression) continue;
                        CheckExpression(call.Arguments[index]);
                    }
                    break;
                case BinaryExpression binary: CheckExpression(binary.Left); CheckExpression(binary.Right); break;
                case UnaryExpression unary: CheckExpression(unary.Operand); break;
                case NullAssertionExpression assertion:
                    if (assertion.Operand is not ThisExpression) CheckExpression(assertion.Operand);
                    break;
                case CastExpression cast: CheckExpression(cast.Operand); break;
                case ConditionalExpression conditional: CheckExpression(conditional.Condition); CheckExpression(conditional.WhenTrue); CheckExpression(conditional.WhenFalse); break;
                case IndexExpression index:
                    if (index.Receiver is not ThisExpression) CheckExpression(index.Receiver);
                    foreach (var subscript in index.Indices) CheckExpression(subscript);
                    break;
                case AllocationExpression allocation:
                    foreach (var argument in allocation.ConstructorArguments ?? []) CheckExpression(argument);
                    foreach (var dimension in allocation.Dimensions) CheckExpression(dimension);
                    break;
                case InlineArrayInitializerExpression initializer:
                    foreach (var element in initializer.Elements) CheckExpression(element);
                    break;
                case OverflowContextExpression overflow: CheckExpression(overflow.Value); break;
            }
        }
        void CheckStatement(Statement statement)
        {
            switch (statement)
            {
                case BlockStatement block: foreach (var child in block.Statements) CheckStatement(child); break;
                case ReturnStatement { Value: { } value }: CheckExpression(value); break;
                case VariableStatement { Initializer: { } value }: CheckExpression(value); break;
                case VariableDeclarationStatement declaration: foreach (var variable in declaration.Variables) if (variable.Initializer is not null) CheckExpression(variable.Initializer); break;
                case MultiAssignmentStatement assignment: foreach (var value in assignment.Values) CheckExpression(value); break;
                case ExpressionStatement expression: CheckExpression(expression.Value); break;
                case IfStatement conditional:
                    CheckExpression(conditional.Condition); CheckStatement(conditional.Then); if (conditional.Else is not null) CheckStatement(conditional.Else); break;
                case WhileStatement loop: CheckExpression(loop.Condition); CheckStatement(loop.Body); break;
                case DoWhileStatement loop: CheckStatement(loop.Body); CheckExpression(loop.Condition); break;
                case ForStatement loop:
                    if (loop.Initializer is not null) CheckStatement(loop.Initializer); if (loop.Condition is not null) CheckExpression(loop.Condition);
                    foreach (var iterator in loop.Iterators) CheckExpression(iterator); CheckStatement(loop.Body); break;
                case ForInStatement loop: CheckExpression(loop.Collection); CheckStatement(loop.Body); break;
                case SwitchStatement selection:
                    CheckExpression(selection.Value);
                    foreach (var section in selection.Sections) { foreach (var label in section.Labels) if (label.Value is not null) CheckExpression(label.Value); foreach (var child in section.Statements) CheckStatement(child); }
                    break;
                case OverflowContextStatement overflow: CheckStatement(overflow.Body); break;
            }
        }
        CheckStatement(method.Declaration.Body!);
    }

    private bool MethodLetsThisEscape(MethodSymbol method, string? receiverType = null, HashSet<MethodSymbol>? visiting = null, bool allowVirtualDispatch = true)
    {
        receiverType ??= GetQualifiedTypeName(method.Owner);
        if (allowVirtualDispatch && method.IsClassInstanceMethod && (method.Declaration.IsVirtual || method.Declaration.IsOverride))
        {
            if (_classTypes.TryGetValue(receiverType, out var receiverClass) && receiverClass.Declaration.IsVirtual)
                return true;

            var slot = VirtualDispatchLayout.FindSlot(receiverType, method, _classTypes, _methods);
            var dispatchTable = VirtualDispatchLayout.GetMethods(receiverType, _classTypes, _methods);
            if (slot >= 0 && slot < dispatchTable.Count)
                method = dispatchTable[slot] ?? method;
        }
        if (method.Declaration.Body is null) return true;
        if (MethodContainsThisCapture(method)) return true;
        visiting ??= [];
        if (!visiting.Add(method)) return false;
        try { return StatementLetsThisEscape(method.Declaration.Body, receiverType, visiting); }
        finally { visiting.Remove(method); }
    }

    private bool MethodContainsThisCapture(MethodSymbol method)
    {
        if (method.Owner.Kind != "class" || method.Declaration.IsStatic) return false;
        var context = new MethodContext(method);
        foreach (var parameter in method.Declaration.Parameters)
            context.Declare(parameter.Name, parameter.TypeName, parameter.PassingMode != "in" && !parameter.IsReadOnly, parameter.Location,
                scopeBoundDepth: parameter.IsScoped && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) ? context.ScopeDepth : null,
                isReadOnlyReference: parameter.PassingMode == "in" && (IsScopeBoundReferenceType(parameter.TypeName) || IsManagedReferenceValue(parameter.TypeName)) || parameter.IsReadOnly);
        bool ExpressionContainsCapture(Expression expression) => expression switch
        {
            LambdaExpression lambda => LambdaBodyReferencesThis(lambda) || LambdaReferencesImplicitInstanceMember(lambda, context),
            InlineArrayInitializerExpression initializer => initializer.Elements.Any(ExpressionContainsCapture),
            AllocationExpression allocation => allocation.Dimensions.Any(ExpressionContainsCapture) || allocation.Initializer is not null && ExpressionContainsCapture(allocation.Initializer) ||
                allocation.ConstructorArguments is not null && allocation.ConstructorArguments.Any(ExpressionContainsCapture),
            UnaryExpression unary => ExpressionContainsCapture(unary.Operand),
            CastExpression cast => ExpressionContainsCapture(cast.Operand),
            BinaryExpression binary => ExpressionContainsCapture(binary.Left) || ExpressionContainsCapture(binary.Right),
            ConditionalExpression conditional => ExpressionContainsCapture(conditional.Condition) || ExpressionContainsCapture(conditional.WhenTrue) || ExpressionContainsCapture(conditional.WhenFalse),
            MemberAccessExpression member => ExpressionContainsCapture(member.Receiver),
            IndexExpression index => ExpressionContainsCapture(index.Receiver) || index.Indices.Any(ExpressionContainsCapture),
            CallExpression call => ExpressionContainsCapture(call.Target) || call.Arguments.Any(ExpressionContainsCapture),
            NullAssertionExpression assertion => ExpressionContainsCapture(assertion.Operand),
            OverflowContextExpression overflow => ExpressionContainsCapture(overflow.Value),
            _ => false
        };
        bool StatementContainsCapture(Statement statement) => statement switch
        {
            BlockStatement block => block.Statements.Any(StatementContainsCapture),
            ReturnStatement returned => returned.Value is not null && ExpressionContainsCapture(returned.Value),
            VariableStatement variable => variable.Initializer is not null && ExpressionContainsCapture(variable.Initializer),
            VariableDeclarationStatement declaration => declaration.Variables.Any(variable => variable.Initializer is not null && ExpressionContainsCapture(variable.Initializer)),
            MultiAssignmentStatement assignment => assignment.Targets.Any(ExpressionContainsCapture) || assignment.Values.Any(ExpressionContainsCapture),
            ExpressionStatement expression => ExpressionContainsCapture(expression.Value),
            IfStatement conditional => ExpressionContainsCapture(conditional.Condition) || StatementContainsCapture(conditional.Then) || conditional.Else is not null && StatementContainsCapture(conditional.Else),
            WhileStatement loop => ExpressionContainsCapture(loop.Condition) || StatementContainsCapture(loop.Body),
            DoWhileStatement loop => StatementContainsCapture(loop.Body) || ExpressionContainsCapture(loop.Condition),
            ForStatement loop => loop.Initializer is not null && StatementContainsCapture(loop.Initializer) || loop.Condition is not null && ExpressionContainsCapture(loop.Condition) ||
                loop.Iterators.Any(ExpressionContainsCapture) || StatementContainsCapture(loop.Body),
            ForInStatement loop => ExpressionContainsCapture(loop.Collection) || StatementContainsCapture(loop.Body),
            SwitchStatement selection => ExpressionContainsCapture(selection.Value) || selection.Sections.Any(section =>
                section.Labels.Any(label => label.Value is not null && ExpressionContainsCapture(label.Value) || label.Guard is not null && ExpressionContainsCapture(label.Guard)) ||
                section.Statements.Any(StatementContainsCapture)),
            TryStatement tryStatement => StatementContainsCapture(tryStatement.Body) || tryStatement.Catches.Any(clause => StatementContainsCapture(clause.Body)) ||
                tryStatement.Finally is not null && StatementContainsCapture(tryStatement.Finally),
            ThrowStatement thrown => thrown.Value is not null && ExpressionContainsCapture(thrown.Value),
            OverflowContextStatement overflow => StatementContainsCapture(overflow.Body),
            _ => false
        };
        return method.Declaration.Body is { } body && StatementContainsCapture(body);
    }

    private bool StatementLetsThisEscape(Statement statement, string receiverType, HashSet<MethodSymbol> visiting) => statement switch
    {
        BlockStatement block => block.Statements.Any(child => StatementLetsThisEscape(child, receiverType, visiting)),
        ReturnStatement { Value: { } value } => ExpressionContainsThis(value, receiverType, visiting),
        VariableStatement { Initializer: { } initializer } => ExpressionContainsThis(initializer, receiverType, visiting),
        VariableDeclarationStatement declaration => declaration.Variables.Any(variable => variable.Initializer is not null && ExpressionContainsThis(variable.Initializer, receiverType, visiting)),
        MultiAssignmentStatement assignment => assignment.Targets.Any(target => ExpressionContainsThis(target, receiverType, visiting)) ||
            assignment.Values.Any(value => ExpressionContainsThis(value, receiverType, visiting)),
        ExpressionStatement expression => ExpressionContainsThis(expression.Value, receiverType, visiting),
        IfStatement conditional => ExpressionContainsThis(conditional.Condition, receiverType, visiting) || StatementLetsThisEscape(conditional.Then, receiverType, visiting) || conditional.Else is not null && StatementLetsThisEscape(conditional.Else, receiverType, visiting),
        WhileStatement loop => ExpressionContainsThis(loop.Condition, receiverType, visiting) || StatementLetsThisEscape(loop.Body, receiverType, visiting),
        DoWhileStatement loop => StatementLetsThisEscape(loop.Body, receiverType, visiting) || ExpressionContainsThis(loop.Condition, receiverType, visiting),
        ForStatement loop => loop.Initializer is not null && StatementLetsThisEscape(loop.Initializer, receiverType, visiting) ||
            loop.Condition is not null && ExpressionContainsThis(loop.Condition, receiverType, visiting) || loop.Iterators.Any(expression => ExpressionContainsThis(expression, receiverType, visiting)) || StatementLetsThisEscape(loop.Body, receiverType, visiting),
        ForInStatement loop => ExpressionContainsThis(loop.Collection, receiverType, visiting) || StatementLetsThisEscape(loop.Body, receiverType, visiting),
        SwitchStatement selection => ExpressionContainsThis(selection.Value, receiverType, visiting) || selection.Sections.Any(section =>
            section.Labels.Any(label => label.Value is not null && ExpressionContainsThis(label.Value, receiverType, visiting) ||
                label.Guard is not null && ExpressionContainsThis(label.Guard, receiverType, visiting)) ||
            section.Statements.Any(child => StatementLetsThisEscape(child, receiverType, visiting))),
        TryStatement attempt => StatementLetsThisEscape(attempt.Body, receiverType, visiting) ||
            attempt.Catches.Any(clause => StatementLetsThisEscape(clause.Body, receiverType, visiting)) ||
            attempt.Finally is not null && StatementLetsThisEscape(attempt.Finally, receiverType, visiting),
        ThrowStatement { Value: { } value } => ExpressionContainsThis(value, receiverType, visiting),
        OverflowContextStatement overflow => StatementLetsThisEscape(overflow.Body, receiverType, visiting),
        _ => false
    };

    private bool ExpressionContainsThis(Expression expression, string receiverType, HashSet<MethodSymbol> visiting)
    {
        switch (expression)
        {
            case ThisExpression or BaseExpression: return true;
            case NameExpression name:
                return _boundUnqualifiedClassFields.TryGetValue(name, out var unqualifiedField) &&
                    ThisCanBeStoredInType(unqualifiedField.TypeName, receiverType);
            case MemberAccessExpression access:
                if (!ExpressionContainsThis(access.Receiver, receiverType, visiting)) return false;
                return ThisCanBeStoredInType(GetReceiverEscapeValueType(access), receiverType);
            case IndexExpression index: return ExpressionContainsThis(index.Receiver, receiverType, visiting) || index.Indices.Any(item => ExpressionContainsThis(item, receiverType, visiting));
            case CallExpression call:
                var argumentMayEscape = false;
                if (_callTargets.TryGetValue(call, out var target) && _callArguments.TryGetValue(call, out var binding))
                {
                    for (var index = 0; index < call.Arguments.Count; index++)
                    {
                        if (!ExpressionContainsThis(call.Arguments[index], receiverType, visiting)) continue;
                        var isReadOnly = target.Declaration.Parameters[binding.ParameterBySource[index]].PassingMode == "in" ||
                            target.Declaration.Parameters[binding.ParameterBySource[index]].IsReadOnly;
                        if (!isReadOnly || call.Arguments[index] is not (ThisExpression or BaseExpression)) return true;
                    }
                    argumentMayEscape = false;
                    var receiverIsThis = call.Target is MemberAccessExpression memberCall && ExpressionContainsThis(memberCall.Receiver, receiverType, visiting) ||
                        call.Target is NameExpression && target.IsClassInstanceMethod;
                    var directBaseCall = call.Target is MemberAccessExpression { Receiver: BaseExpression };
                    if (receiverIsThis && target.IsClassInstanceMethod && MethodLetsThisEscape(target, receiverType, visiting, allowVirtualDispatch: !directBaseCall)) return true;
                }
                else argumentMayEscape = call.Arguments.Any(argument => ExpressionContainsThis(argument, receiverType, visiting));
                var receiverWasCheckedAsCurrentThis = call.Target is MemberAccessExpression && _callTargets.TryGetValue(call, out var callTarget) &&
                    callTarget.IsClassInstanceMethod && ExpressionContainsThis(((MemberAccessExpression)call.Target).Receiver, receiverType, visiting);
                return argumentMayEscape || call.Target is not MemberAccessExpression && ExpressionContainsThis(call.Target, receiverType, visiting) ||
                    call.Target is MemberAccessExpression && !receiverWasCheckedAsCurrentThis &&
                    ExpressionContainsThis(call.Target, receiverType, visiting);
            case BinaryExpression binary:
                if (binary.Operator is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=")
                {
                    if (_propertySetterCalls.TryGetValue(binary, out var setterCall))
                        return ExpressionContainsThis(setterCall, receiverType, visiting) || ExpressionContainsThis(binary.Right, receiverType, visiting);
                    if (IsCurrentThisFieldTarget(binary.Left))
                        return ExpressionContainsThis(binary.Right, receiverType, visiting);
                }
                return ExpressionContainsThis(binary.Left, receiverType, visiting) || ExpressionContainsThis(binary.Right, receiverType, visiting);
            case UnaryExpression unary: return ExpressionContainsThis(unary.Operand, receiverType, visiting);
            case CastExpression cast: return ExpressionContainsThis(cast.Operand, receiverType, visiting);
            case NullAssertionExpression assertion: return ExpressionContainsThis(assertion.Operand, receiverType, visiting);
            case ConditionalExpression conditional: return ExpressionContainsThis(conditional.Condition, receiverType, visiting) || ExpressionContainsThis(conditional.WhenTrue, receiverType, visiting) || ExpressionContainsThis(conditional.WhenFalse, receiverType, visiting);
            case OverflowContextExpression overflow: return ExpressionContainsThis(overflow.Value, receiverType, visiting);
            case AllocationExpression allocation: return (allocation.ConstructorArguments ?? []).Any(argument => ExpressionContainsThis(argument, receiverType, visiting)) || allocation.Initializer is not null && allocation.Initializer.Elements.Any(element => ExpressionContainsThis(element, receiverType, visiting));
            case InlineArrayInitializerExpression initializer: return initializer.Elements.Any(element => ExpressionContainsThis(element, receiverType, visiting));
            default: return false;
        }
    }

    private bool IsCurrentThisFieldTarget(Expression target) => target switch
    {
        NameExpression name => _boundUnqualifiedClassFields.ContainsKey(name),
        MemberAccessExpression access when access.Receiver is ThisExpression or BaseExpression => _boundClassFields.ContainsKey(access),
        _ => false
    };

    private string? GetReceiverEscapeValueType(Expression expression) => expression switch
    {
        NameExpression name when _boundUnqualifiedClassFields.TryGetValue(name, out var field) => field.TypeName,
        NameExpression name when _boundUnqualifiedStructFields.TryGetValue(name, out var field) => field.TypeName,
        MemberAccessExpression access when _boundClassFields.TryGetValue(access, out var field) => field.TypeName,
        MemberAccessExpression access when _boundStructFields.TryGetValue(access, out var field) => field.TypeName,
        MemberAccessExpression access when _boundStaticFields.TryGetValue(access, out var field) => field.TypeName,
        MemberAccessExpression access when _propertyGetterCalls.TryGetValue(access, out var getter) && _callTargets.TryGetValue(getter, out var method) => method.Declaration.ReturnType,
        NameExpression name when _propertyGetterNameCalls.TryGetValue(name, out var getter) && _callTargets.TryGetValue(getter, out var method) => method.Declaration.ReturnType,
        CallExpression call when _callTargets.TryGetValue(call, out var method) => method.Declaration.ReturnType,
        IndexExpression index when _managedArrayIndexAccesses.TryGetValue(index, out var arrayIndex) => arrayIndex.ElementType,
        IndexExpression index when _indexerGetterCalls.TryGetValue(index, out var getter) && _callTargets.TryGetValue(getter, out var method) => method.Declaration.ReturnType,
        CastExpression cast when _castTypes.TryGetValue(cast, out var castType) => castType,
        NullAssertionExpression assertion => GetReceiverEscapeValueType(assertion.Operand),
        UnaryExpression unary => GetReceiverEscapeValueType(unary.Operand),
        _ => null
    };

    private bool ThisCanBeStoredInType(string? valueType, string receiverType)
    {
        if (valueType is null || valueType == "?") return true;
        var targetType = NullableUnderlyingType(valueType);
        return IsClassReferenceType(targetType) && CanConvert(receiverType, targetType, null);
    }

    private void AnalyzeOutParameterFlow(MethodSymbol method)
    {
        if (method.Declaration.Body is null) return;
        var outParameters = method.Declaration.Parameters.Where(parameter => parameter.PassingMode == "out").ToDictionary(parameter => parameter.Name, StringComparer.Ordinal);
        if (outParameters.Count == 0) return;
        _outFlowLoops.Clear();
        _outFlowSwitches.Clear();
        _outFlowReturnFrames.Clear();
        _outFlowExceptionFrames.Clear();
        _outFlowSwitchDepth = 0;
        _outFlowSuppressReturnChecks = 0;
        var (assigned, fallsThrough) = AnalyzeOutFlowStatement(method.Declaration.Body, new HashSet<string>(StringComparer.Ordinal), outParameters);
        if (fallsThrough) ReportUnassignedOutParameters(assigned, outParameters, method.Declaration.Location);
    }

    private OutFlowTransferCounts CaptureOutFlowTransferCounts() => new(
        _outFlowLoops.Select(loop => loop.BreakStates.Count).ToArray(),
        _outFlowLoops.Select(loop => loop.ContinueStates.Count).ToArray(),
        _outFlowSwitches.Select(selection => selection.BreakStates.Count).ToArray());

    private void RestoreOutFlowTransferCounts(OutFlowTransferCounts counts)
    {
        for (var index = 0; index < Math.Min(counts.LoopBreaks.Length, _outFlowLoops.Count); index++)
        {
            Trim(_outFlowLoops[index].BreakStates, counts.LoopBreaks[index]);
            Trim(_outFlowLoops[index].ContinueStates, counts.LoopContinues[index]);
        }
        for (var index = 0; index < Math.Min(counts.SwitchBreaks.Length, _outFlowSwitches.Count); index++)
            Trim(_outFlowSwitches[index].BreakStates, counts.SwitchBreaks[index]);

        static void Trim<T>(List<T> items, int count)
        {
            if (items.Count > count) items.RemoveRange(count, items.Count - count);
        }
    }

    private void AnalyzeDeadOutFlowStatement(Statement statement, HashSet<string> assigned,
        IReadOnlyDictionary<string, ParameterDeclaration> outParameters)
    {
        var transfers = CaptureOutFlowTransferCounts();
        var exceptionCounts = _outFlowExceptionFrames.Select(frame => frame.Count).ToArray();
        _outFlowSuppressReturnChecks++;
        try { _ = AnalyzeOutFlowStatement(statement, new HashSet<string>(assigned, StringComparer.Ordinal), outParameters); }
        finally
        {
            _outFlowSuppressReturnChecks--;
            RestoreOutFlowTransferCounts(transfers);
            for (var index = 0; index < Math.Min(exceptionCounts.Length, _outFlowExceptionFrames.Count); index++)
            {
                var frame = _outFlowExceptionFrames[index];
                if (frame.Count > exceptionCounts[index]) frame.RemoveRange(exceptionCounts[index], frame.Count - exceptionCounts[index]);
            }
        }
    }

    private void RecordOutFlowException(HashSet<string> assigned, string? exceptionType = null)
    {
        if (_outFlowExceptionFrames.Count > 0)
            _outFlowExceptionFrames[^1].Add(new(exceptionType, new HashSet<string>(assigned, StringComparer.Ordinal)));
    }

    private bool OutFlowExceptionMayMatchCatch(OutFlowExceptionState thrown, CatchClause clause)
    {
        if (clause.IsCatchAll || thrown.ExceptionType is null) return true;
        var catchType = GetOutFlowCatchType(clause);
        if (catchType is null) return false;
        if (thrown.ExceptionType == catchType || IsBaseClassOf(catchType, thrown.ExceptionType)) return true;

        // A virtual class value may hold a more derived runtime type. Such a catch
        // is possible, but does not consume the whole path unless it catches the
        // statically known base type or one of its bases.
        return _classTypes.TryGetValue(thrown.ExceptionType, out var staticType) &&
            staticType.Declaration.IsVirtual && IsBaseClassOf(thrown.ExceptionType, catchType);
    }

    private bool OutFlowCatchConsumesException(OutFlowExceptionState thrown, CatchClause clause)
    {
        if (clause.IsCatchAll) return true;
        if (thrown.ExceptionType is null) return GetOutFlowCatchType(clause) == "System.Exception";
        var catchType = GetOutFlowCatchType(clause);
        return catchType is not null && (thrown.ExceptionType == catchType || IsBaseClassOf(catchType, thrown.ExceptionType));
    }

    private string? GetOutFlowCatchType(CatchClause clause) =>
        _exceptionCatchTypes.TryGetValue(clause, out var catchType) ? catchType : clause.TypeName;

    private List<OutFlowPendingTransfer> TakeOutFlowTransfersSince(OutFlowTransferCounts counts)
    {
        var transfers = new List<OutFlowPendingTransfer>();
        for (var index = 0; index < Math.Min(counts.LoopBreaks.Length, _outFlowLoops.Count); index++)
        {
            var loop = _outFlowLoops[index];
            transfers.AddRange(loop.BreakStates.Skip(counts.LoopBreaks[index])
                .Select(state => new OutFlowPendingTransfer(OutFlowTransferKind.LoopBreak, loop, null, state)));
            transfers.AddRange(loop.ContinueStates.Skip(counts.LoopContinues[index])
                .Select(state => new OutFlowPendingTransfer(OutFlowTransferKind.LoopContinue, loop, null, state)));
        }
        for (var index = 0; index < Math.Min(counts.SwitchBreaks.Length, _outFlowSwitches.Count); index++)
        {
            var selection = _outFlowSwitches[index];
            transfers.AddRange(selection.BreakStates.Skip(counts.SwitchBreaks[index])
                .Select(state => new OutFlowPendingTransfer(OutFlowTransferKind.SwitchBreak, null, selection, state)));
        }
        RestoreOutFlowTransferCounts(counts);
        return transfers;
    }

    private void ApplyOutFlowFinallyToTransfers(BlockStatement finallyBody, IEnumerable<OutFlowPendingTransfer> transfers,
        IReadOnlyDictionary<string, ParameterDeclaration> outParameters)
    {
        foreach (var transfer in transfers)
        {
            var transferCounts = CaptureOutFlowTransferCounts();
            _outFlowSuppressReturnChecks++;
            (HashSet<string> Assigned, bool FallsThrough) cleanupFlow;
            try
            {
                cleanupFlow = AnalyzeOutFlowStatement(finallyBody,
                    new HashSet<string>(transfer.Assigned, StringComparer.Ordinal), outParameters);
            }
            finally
            {
                _outFlowSuppressReturnChecks--;
                RestoreOutFlowTransferCounts(transferCounts);
            }
            if (!cleanupFlow.FallsThrough) continue;
            switch (transfer.Kind)
            {
                case OutFlowTransferKind.LoopBreak: transfer.Loop!.BreakStates.Add(cleanupFlow.Assigned); break;
                case OutFlowTransferKind.LoopContinue: transfer.Loop!.ContinueStates.Add(cleanupFlow.Assigned); break;
                case OutFlowTransferKind.SwitchBreak: transfer.Switch!.BreakStates.Add(cleanupFlow.Assigned); break;
            }
        }
    }

    private static HashSet<string> MergeOutAssignmentPaths(IEnumerable<HashSet<string>> paths)
    {
        using var iterator = paths.GetEnumerator();
        if (!iterator.MoveNext()) return new(StringComparer.Ordinal);
        var merged = new HashSet<string>(iterator.Current, StringComparer.Ordinal);
        while (iterator.MoveNext()) merged.IntersectWith(iterator.Current);
        return merged;
    }

    private void CompleteOutFlowReturn(HashSet<string> assigned, IReadOnlyDictionary<string, ParameterDeclaration> outParameters, SourceLocation location)
    {
        if (_outFlowSuppressReturnChecks > 0) return;
        if (_outFlowReturnFrames.Count > 0)
            _outFlowReturnFrames[^1].Add(new(new HashSet<string>(assigned, StringComparer.Ordinal), location));
        else
            ReportUnassignedOutParameters(assigned, outParameters, location);
    }

    private (HashSet<string> Assigned, bool FallsThrough) AnalyzeOutLoop(
        Statement body, string? label, HashSet<string> entryState, IReadOnlyDictionary<string, ParameterDeclaration> outParameters,
        Func<bool> canExitNormally, Action<HashSet<string>>? advance = null)
    {
        var loop = new OutFlowLoop(label, _outFlowSwitchDepth);
        _outFlowLoops.Add(loop);
        var bodyFlow = AnalyzeOutFlowStatement(body, new HashSet<string>(entryState, StringComparer.Ordinal), outParameters);
        _outFlowLoops.RemoveAt(_outFlowLoops.Count - 1);

        var exits = new List<HashSet<string>>(loop.BreakStates);
        if (canExitNormally()) exits.Add(new HashSet<string>(entryState, StringComparer.Ordinal));
        var backPaths = new List<HashSet<string>>();
        if (bodyFlow.FallsThrough) backPaths.Add(bodyFlow.Assigned);
        backPaths.AddRange(loop.ContinueStates);
        if (advance is not null)
        {
            foreach (var backPath in backPaths)
            {
                var exitState = new HashSet<string>(backPath, StringComparer.Ordinal);
                advance(exitState);
                if (canExitNormally()) exits.Add(exitState);
            }
        }
        if (exits.Count == 0) return (new HashSet<string>(entryState, StringComparer.Ordinal), false);
        return (MergeOutAssignmentPaths(exits), true);
    }

    private (HashSet<string> Assigned, bool FallsThrough) AnalyzeOutFlowStatement(Statement statement, HashSet<string> assigned, IReadOnlyDictionary<string, ParameterDeclaration> outParameters)
    {
        switch (statement)
        {
            case BlockStatement block:
                var state = new HashSet<string>(assigned, StringComparer.Ordinal); var fallsThrough = true;
                foreach (var child in block.Statements)
                {
                    if (!fallsThrough)
                    {
                        AnalyzeDeadOutFlowStatement(child, state, outParameters);
                        continue;
                    }
                    (state, fallsThrough) = AnalyzeOutFlowStatement(child, state, outParameters);
                }
                return (state, fallsThrough);
            case ExpressionStatement expression:
                AnalyzeOutFlowExpression(expression.Value, assigned, outParameters); return (assigned, true);
            case VariableStatement variable:
                if (variable.Initializer is not null) AnalyzeOutFlowExpression(variable.Initializer, assigned, outParameters);
                return (assigned, true);
            case VariableDeclarationStatement declaration:
                foreach (var variableDeclaration in declaration.Variables)
                    if (variableDeclaration.Initializer is not null) AnalyzeOutFlowExpression(variableDeclaration.Initializer, assigned, outParameters);
                return (assigned, true);
            case MultiAssignmentStatement multiple:
                foreach (var value in multiple.Values) AnalyzeOutFlowExpression(value, assigned, outParameters);
                foreach (var target in multiple.Targets)
                    if (target is not NameExpression) AnalyzeOutFlowExpression(target, assigned, outParameters);
                foreach (var target in multiple.Targets) if (target is NameExpression targetName && outParameters.ContainsKey(targetName.Name)) assigned.Add(targetName.Name);
                return (assigned, true);
            case ReturnStatement returned:
                if (returned.Value is not null) AnalyzeOutFlowExpression(returned.Value, assigned, outParameters);
                CompleteOutFlowReturn(assigned, outParameters, returned.Location);
                return (assigned, false);
            case IfStatement conditional:
                AnalyzeOutFlowExpression(conditional.Condition, assigned, outParameters);
                if (TryEvaluateConstant(conditional.Condition, out var constantCondition, out _) && constantCondition.Boolean is { } knownBranch)
                {
                    AnalyzeDeadOutFlowStatement(knownBranch ? conditional.Else ?? new EmptyStatement(conditional.Location) : conditional.Then,
                        assigned, outParameters);
                    return knownBranch
                        ? AnalyzeOutFlowStatement(conditional.Then, new HashSet<string>(assigned, StringComparer.Ordinal), outParameters)
                        : conditional.Else is null
                            ? (new HashSet<string>(assigned, StringComparer.Ordinal), true)
                            : AnalyzeOutFlowStatement(conditional.Else, new HashSet<string>(assigned, StringComparer.Ordinal), outParameters);
                }
                var thenFlow = AnalyzeOutFlowStatement(conditional.Then, new HashSet<string>(assigned, StringComparer.Ordinal), outParameters);
                var elseFlow = conditional.Else is null
                    ? (new HashSet<string>(assigned, StringComparer.Ordinal), true)
                    : AnalyzeOutFlowStatement(conditional.Else, new HashSet<string>(assigned, StringComparer.Ordinal), outParameters);
                return MergeOutFlows(thenFlow, elseFlow);
            case WhileStatement whileLoop:
                AnalyzeOutFlowExpression(whileLoop.Condition, assigned, outParameters);
                var whileEntry = new HashSet<string>(assigned, StringComparer.Ordinal);
                if (GetKnownBoolean(whileLoop.Condition) == false)
                {
                    _outFlowLoops.Add(new(whileLoop.Label, _outFlowSwitchDepth));
                    AnalyzeDeadOutFlowStatement(whileLoop.Body, whileEntry, outParameters);
                    _outFlowLoops.RemoveAt(_outFlowLoops.Count - 1);
                    return (whileEntry, true);
                }
                return AnalyzeOutLoop(whileLoop.Body, whileLoop.Label, whileEntry, outParameters,
                    () => !IsAlwaysTrueCondition(whileLoop.Condition),
                    state => AnalyzeOutFlowExpression(whileLoop.Condition, state, outParameters));
            case DoWhileStatement doLoop:
                var doEntry = new HashSet<string>(assigned, StringComparer.Ordinal);
                var doLoopFlow = new OutFlowLoop(doLoop.Label, _outFlowSwitchDepth);
                _outFlowLoops.Add(doLoopFlow);
                var doBodyFlow = AnalyzeOutFlowStatement(doLoop.Body, new HashSet<string>(doEntry, StringComparer.Ordinal), outParameters);
                _outFlowLoops.RemoveAt(_outFlowLoops.Count - 1);
                var doExits = new List<HashSet<string>>(doLoopFlow.BreakStates);
                if (!IsAlwaysTrueCondition(doLoop.Condition))
                {
                    var doBackPaths = new List<HashSet<string>>();
                    if (doBodyFlow.FallsThrough) doBackPaths.Add(doBodyFlow.Assigned);
                    doBackPaths.AddRange(doLoopFlow.ContinueStates);
                    foreach (var backPath in doBackPaths)
                    {
                        var conditionState = new HashSet<string>(backPath, StringComparer.Ordinal);
                        AnalyzeOutFlowExpression(doLoop.Condition, conditionState, outParameters);
                        doExits.Add(conditionState);
                    }
                }
                return doExits.Count == 0 ? (doEntry, false) : (MergeOutAssignmentPaths(doExits), true);
            case ForStatement forLoop:
                if (forLoop.Initializer is not null) (assigned, _) = AnalyzeOutFlowStatement(forLoop.Initializer, assigned, outParameters);
                if (forLoop.Condition is not null) AnalyzeOutFlowExpression(forLoop.Condition, assigned, outParameters);
                var forEntry = new HashSet<string>(assigned, StringComparer.Ordinal);
                if (forLoop.Condition is not null && GetKnownBoolean(forLoop.Condition) == false)
                {
                    _outFlowLoops.Add(new(forLoop.Label, _outFlowSwitchDepth));
                    AnalyzeDeadOutFlowStatement(forLoop.Body, forEntry, outParameters);
                    _outFlowLoops.RemoveAt(_outFlowLoops.Count - 1);
                    return (forEntry, true);
                }
                return AnalyzeOutLoop(forLoop.Body, forLoop.Label, forEntry, outParameters,
                    () => !IsAlwaysTrueCondition(forLoop.Condition),
                    state =>
                    {
                        foreach (var iterator in forLoop.Iterators) AnalyzeOutFlowExpression(iterator, state, outParameters);
                        if (forLoop.Condition is not null) AnalyzeOutFlowExpression(forLoop.Condition, state, outParameters);
                    });
            case ForInStatement forIn:
                AnalyzeOutFlowExpression(forIn.Collection, assigned, outParameters);
                RecordOutFlowException(assigned);
                var forInLoop = new OutFlowLoop(forIn.Label, _outFlowSwitchDepth);
                _outFlowLoops.Add(forInLoop);
                _ = AnalyzeOutFlowStatement(forIn.Body, new HashSet<string>(assigned, StringComparer.Ordinal), outParameters);
                _outFlowLoops.RemoveAt(_outFlowLoops.Count - 1);
                return (new HashSet<string>(assigned, StringComparer.Ordinal), true);
            case SwitchStatement selection:
                AnalyzeOutFlowExpression(selection.Value, assigned, outParameters);
                var switchFlow = new OutFlowSwitch(++_outFlowSwitchDepth);
                _outFlowSwitches.Add(switchFlow);
                var branches = new List<(HashSet<string> Assigned, bool FallsThrough)>(); var hasDefault = false;
                foreach (var section in selection.Sections)
                {
                    hasDefault |= _exhaustiveSwitches.Contains(selection) || section.Labels.Any(label => label.IsDefault || label.Guard is null && IsCatchAllSwitchPattern(label));
                    branches.Add(AnalyzeOutFlowStatement(new BlockStatement(section.Statements, section.At), new HashSet<string>(assigned, StringComparer.Ordinal), outParameters));
                }
                _outFlowSwitches.RemoveAt(_outFlowSwitches.Count - 1);
                _outFlowSwitchDepth--;
                branches.AddRange(switchFlow.BreakStates.Select(state => (state, true)));
                if (!hasDefault) branches.Add((new HashSet<string>(assigned, StringComparer.Ordinal), true));
                if (branches.Count == 0) return (assigned, true);
                return branches.Aggregate(MergeOutFlows);
            case TryStatement attempt:
                var transferStart = CaptureOutFlowTransferCounts();
                var pendingReturns = new List<OutFlowReturn>();
                var pendingExceptions = new List<OutFlowExceptionState>();
                _outFlowReturnFrames.Add(pendingReturns);
                _outFlowExceptionFrames.Add(pendingExceptions);
                var tryFlow = AnalyzeOutFlowStatement(attempt.Body, new HashSet<string>(assigned, StringComparer.Ordinal), outParameters);
                _outFlowExceptionFrames.RemoveAt(_outFlowExceptionFrames.Count - 1);
                var catchFlows = new List<(HashSet<string> Assigned, bool FallsThrough)>();
                var fullyCaught = new bool[pendingExceptions.Count];
                foreach (var clause in attempt.Catches)
                {
                    var matchingStates = new List<HashSet<string>>();
                    for (var exceptionIndex = 0; exceptionIndex < pendingExceptions.Count; exceptionIndex++)
                    {
                        var thrown = pendingExceptions[exceptionIndex];
                        if (fullyCaught[exceptionIndex] || !OutFlowExceptionMayMatchCatch(thrown, clause)) continue;
                        matchingStates.Add(thrown.Assigned);
                        if (OutFlowCatchConsumesException(thrown, clause)) fullyCaught[exceptionIndex] = true;
                    }

                    if (matchingStates.Count > 0)
                    {
                        var catchEntry = MergeOutAssignmentPaths(matchingStates);
                        catchFlows.Add(AnalyzeOutFlowStatement(clause.Body,
                            new HashSet<string>(catchEntry, StringComparer.Ordinal), outParameters));
                    }
                    else
                    {
                        AnalyzeDeadOutFlowStatement(clause.Body, assigned, outParameters);
                    }
                }
                for (var exceptionIndex = 0; exceptionIndex < pendingExceptions.Count; exceptionIndex++)
                    if (!fullyCaught[exceptionIndex])
                    {
                        var exceptionState = pendingExceptions[exceptionIndex];
                        RecordOutFlowException(exceptionState.Assigned, exceptionState.ExceptionType);
                    }
                List<OutFlowPendingTransfer> pendingTransfers = attempt.Finally is null ? [] : TakeOutFlowTransfersSince(transferStart);
                _outFlowReturnFrames.RemoveAt(_outFlowReturnFrames.Count - 1);

                if (attempt.Finally is not null)
                    ApplyOutFlowFinallyToTransfers(attempt.Finally, pendingTransfers, outParameters);

                foreach (var pendingReturn in pendingReturns)
                {
                    if (attempt.Finally is null)
                    {
                        CompleteOutFlowReturn(pendingReturn.Assigned, outParameters, pendingReturn.Location);
                        continue;
                    }
                    _outFlowSuppressReturnChecks++;
                    (HashSet<string> Assigned, bool FallsThrough) finalReturnFlow;
                    try
                    {
                        finalReturnFlow = AnalyzeOutFlowStatement(attempt.Finally,
                            new HashSet<string>(pendingReturn.Assigned, StringComparer.Ordinal), outParameters);
                    }
                    finally { _outFlowSuppressReturnChecks--; }
                    if (finalReturnFlow.FallsThrough)
                        CompleteOutFlowReturn(finalReturnFlow.Assigned, outParameters, pendingReturn.Location);
                }

                var normalFlows = new List<(HashSet<string> Assigned, bool FallsThrough)>();
                if (tryFlow.FallsThrough) normalFlows.Add(tryFlow);
                normalFlows.AddRange(catchFlows.Where(flow => flow.FallsThrough));
                if (normalFlows.Count == 0) return (new HashSet<string>(assigned, StringComparer.Ordinal), false);
                var normalFlow = normalFlows.Aggregate(MergeOutFlows);
                if (attempt.Finally is null) return normalFlow;
                _outFlowSuppressReturnChecks++;
                try { return AnalyzeOutFlowStatement(attempt.Finally, normalFlow.Assigned, outParameters); }
                finally { _outFlowSuppressReturnChecks--; }
            case OverflowContextStatement overflow:
                return AnalyzeOutFlowStatement(overflow.Body, assigned, outParameters);
            case BreakStatement transfer:
                if (transfer.IsContinue)
                {
                    var loopTarget = transfer.Label is null
                        ? _outFlowLoops.LastOrDefault()
                        : _outFlowLoops.LastOrDefault(candidate => candidate.Label == transfer.Label);
                    loopTarget?.ContinueStates.Add(new HashSet<string>(assigned, StringComparer.Ordinal));
                    return (assigned, false);
                }
                if (transfer.Label is not null)
                {
                    _outFlowLoops.LastOrDefault(candidate => candidate.Label == transfer.Label)?.BreakStates.Add(new HashSet<string>(assigned, StringComparer.Ordinal));
                    return (assigned, false);
                }
                var nearestLoopSwitchDepth = _outFlowLoops.Count == 0 ? -1 : _outFlowLoops[^1].SwitchDepth;
                if (_outFlowSwitches.Count > 0 && _outFlowSwitches[^1].Depth > nearestLoopSwitchDepth)
                    _outFlowSwitches[^1].BreakStates.Add(new HashSet<string>(assigned, StringComparer.Ordinal));
                else if (_outFlowLoops.Count > 0)
                    _outFlowLoops[^1].BreakStates.Add(new HashSet<string>(assigned, StringComparer.Ordinal));
                return (assigned, false);
            case ThrowStatement thrown:
                if (thrown.Value is not null) AnalyzeOutFlowExpression(thrown.Value, assigned, outParameters);
                var thrownType = _throwTypes.GetValueOrDefault(thrown);
                RecordOutFlowException(assigned, thrownType is "<rethrow>" ? null : thrownType);
                return (assigned, false);
            case EmptyStatement:
                return (assigned, true);
            default:
                return (assigned, true);
        }
    }

    private (HashSet<string> Assigned, bool FallsThrough) MergeOutFlows((HashSet<string> Assigned, bool FallsThrough) first, (HashSet<string> Assigned, bool FallsThrough) second)
    {
        var falling = new[] { first, second }.Where(flow => flow.FallsThrough).ToArray();
        if (falling.Length == 0) return (new HashSet<string>(first.Assigned, StringComparer.Ordinal), false);
        var merged = new HashSet<string>(falling[0].Assigned, StringComparer.Ordinal);
        foreach (var flow in falling.Skip(1)) merged.IntersectWith(flow.Assigned);
        return (merged, true);
    }

    private void ReportUnassignedOutParameters(IReadOnlySet<string> assigned, IReadOnlyDictionary<string, ParameterDeclaration> outParameters, SourceLocation location)
    {
        foreach (var parameter in outParameters.Values.Where(parameter => !assigned.Contains(parameter.Name)))
            diagnostics.Add("LS0720", DiagnosticSeverity.Error, $"Out parameter '{parameter.Name}' must be assigned before returning normally.", location);
    }

    private void AnalyzeOutFlowExpression(Expression expression, HashSet<string> assigned, IReadOnlyDictionary<string, ParameterDeclaration> outParameters)
    {
        switch (expression)
        {
            case NameExpression readName when outParameters.ContainsKey(readName.Name):
                if (!assigned.Contains(readName.Name)) diagnostics.Add("LS0720", DiagnosticSeverity.Error, $"Out parameter '{readName.Name}' is read before it is assigned.", readName.Location);
                break;
            case BinaryExpression { Operator: "=" } assignment when assignment.Left is NameExpression writeName && outParameters.ContainsKey(writeName.Name):
                AnalyzeOutFlowExpression(assignment.Right, assigned, outParameters); assigned.Add(writeName.Name); break;
            case BinaryExpression assignment:
                AnalyzeOutFlowExpression(assignment.Left, assigned, outParameters); AnalyzeOutFlowExpression(assignment.Right, assigned, outParameters);
                if (assignment.Operator is "+" or "-" or "*" or "/" or "%" or "<<" or ">>" or "??" or "??=")
                    RecordOutFlowException(assigned);
                if (assignment.Operator == "=" && assignment.Left is not NameExpression)
                    RecordOutFlowException(assigned);
                if ((assignment.Operator is "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=") && assignment.Left is NameExpression compoundName && outParameters.ContainsKey(compoundName.Name)) assigned.Add(compoundName.Name);
                break;
            case UnaryExpression { Operator: "++" or "--" } increment when increment.Operand is NameExpression incrementName && outParameters.ContainsKey(incrementName.Name):
                AnalyzeOutFlowExpression(increment.Operand, assigned, outParameters); RecordOutFlowException(assigned); assigned.Add(incrementName.Name); break;
            case UnaryExpression unary:
                AnalyzeOutFlowExpression(unary.Operand, assigned, outParameters);
                if (unary.Operator is "-" or "++" or "--") RecordOutFlowException(assigned);
                break;
            case CastExpression cast: AnalyzeOutFlowExpression(cast.Operand, assigned, outParameters); RecordOutFlowException(assigned); break;
            case NullAssertionExpression assertion: AnalyzeOutFlowExpression(assertion.Operand, assigned, outParameters); RecordOutFlowException(assigned); break;
            case ConditionalExpression conditional:
                AnalyzeOutFlowExpression(conditional.Condition, assigned, outParameters);
                var baseState = new HashSet<string>(assigned, StringComparer.Ordinal);
                var trueState = new HashSet<string>(baseState, StringComparer.Ordinal); var falseState = new HashSet<string>(baseState, StringComparer.Ordinal);
                AnalyzeOutFlowExpression(conditional.WhenTrue, trueState, outParameters); AnalyzeOutFlowExpression(conditional.WhenFalse, falseState, outParameters);
                trueState.IntersectWith(falseState); assigned.Clear(); assigned.UnionWith(trueState); break;
            case MemberAccessExpression member:
                AnalyzeOutFlowExpression(member.Receiver, assigned, outParameters);
                RecordOutFlowException(assigned);
                break;
            case IndexExpression index:
                AnalyzeOutFlowExpression(index.Receiver, assigned, outParameters);
                foreach (var item in index.Indices) AnalyzeOutFlowExpression(item, assigned, outParameters);
                RecordOutFlowException(assigned);
                break;
            case CallExpression call:
                AnalyzeOutFlowExpression(call.Target, assigned, outParameters);
                var outArguments = new List<string>();
                for (var index = 0; index < call.Arguments.Count; index++)
                {
                    var mode = call.ArgumentModes is not null && index < call.ArgumentModes.Count ? call.ArgumentModes[index] : null;
                    if (mode == "out" && call.Arguments[index] is NameExpression outName && outParameters.ContainsKey(outName.Name)) outArguments.Add(outName.Name);
                    else AnalyzeOutFlowExpression(call.Arguments[index], assigned, outParameters);
                }
                RecordOutFlowException(assigned);
                assigned.UnionWith(outArguments);
                break;
            case AllocationExpression allocation:
                foreach (var dimension in allocation.Dimensions) AnalyzeOutFlowExpression(dimension, assigned, outParameters);
                foreach (var argument in allocation.ConstructorArguments ?? []) AnalyzeOutFlowExpression(argument, assigned, outParameters);
                if (allocation.Initializer is { } initializer)
                    foreach (var element in initializer.Elements) AnalyzeOutFlowExpression(element, assigned, outParameters);
                RecordOutFlowException(assigned);
                break;
            case OverflowContextExpression overflow: AnalyzeOutFlowExpression(overflow.Value, assigned, outParameters); break;
        }
    }
    private bool AlwaysReturns(Statement statement) => statement switch
    {
        ReturnStatement or ThrowStatement => true,
        OverflowContextStatement c => AlwaysReturns(c.Body),
        BlockStatement b => b.Statements.Any(AlwaysReturns),
        IfStatement i => GetKnownBoolean(i.Condition) switch
        {
            true => AlwaysReturns(i.Then),
            false => i.Else is not null && AlwaysReturns(i.Else),
            null => i.Else is not null && AlwaysReturns(i.Then) && AlwaysReturns(i.Else)
        },
        WhileStatement loop => IsUnbreakableInfiniteLoop(loop),
        DoWhileStatement loop => IsUnbreakableInfiniteLoop(loop),
        ForStatement loop => IsUnbreakableInfiniteLoop(loop),
        TryStatement t => AlwaysReturns(t.Body) && (t.Catches.Count == 0 || t.Catches.All(c => AlwaysReturns(c.Body))),
        SwitchStatement s => (_exhaustiveSwitches.Contains(s) || s.Sections.Any(section => section.Labels.Any(label => label.IsDefault || label.Guard is null && IsCatchAllSwitchPattern(label)))) && s.Sections.All(section => AlwaysReturns(new BlockStatement(section.Statements, section.At))),
        _ => false
    };

    private bool IsUnbreakableInfiniteLoop(WhileStatement loop) =>
        IsAlwaysTrueCondition(loop.Condition) && !ContainsBreakTargetingLoop(loop.Body, loop.Label);

    private bool IsUnbreakableInfiniteLoop(DoWhileStatement loop) =>
        IsAlwaysTrueCondition(loop.Condition) && !ContainsBreakTargetingLoop(loop.Body, loop.Label);

    private bool IsUnbreakableInfiniteLoop(ForStatement loop) =>
        IsAlwaysTrueCondition(loop.Condition) && !ContainsBreakTargetingLoop(loop.Body, loop.Label);

    private bool ContainsBreakTargetingLoop(Statement statement, string? targetLabel, int nestedBreakableDepth = 0)
    {
        switch (statement)
        {
            case BreakStatement { IsContinue: false, Label: { } label }:
                return targetLabel is not null && label == targetLabel;
            case BreakStatement { IsContinue: false }:
                return nestedBreakableDepth == 0;
            case BlockStatement block:
                foreach (var child in block.Statements)
                {
                    if (ContainsBreakTargetingLoop(child, targetLabel, nestedBreakableDepth)) return true;
                    if (AlwaysReturns(child)) break;
                }
                return false;
            case IfStatement conditional:
                if (TryEvaluateConstant(conditional.Condition, out var constant, out _) && constant.Boolean is { } value)
                    return ContainsBreakTargetingLoop(value ? conditional.Then : conditional.Else ?? new EmptyStatement(conditional.Location), targetLabel, nestedBreakableDepth);
                return ContainsBreakTargetingLoop(conditional.Then, targetLabel, nestedBreakableDepth) ||
                    conditional.Else is not null && ContainsBreakTargetingLoop(conditional.Else, targetLabel, nestedBreakableDepth);
            case WhileStatement loop:
                return ContainsBreakTargetingLoop(loop.Body, targetLabel, nestedBreakableDepth + 1);
            case DoWhileStatement loop:
                return ContainsBreakTargetingLoop(loop.Body, targetLabel, nestedBreakableDepth + 1);
            case ForStatement loop:
                return ContainsBreakTargetingLoop(loop.Body, targetLabel, nestedBreakableDepth + 1);
            case ForInStatement loop:
                return ContainsBreakTargetingLoop(loop.Body, targetLabel, nestedBreakableDepth + 1);
            case SwitchStatement selection:
                return selection.Sections.Any(section => ContainsBreakTargetingLoop(
                    new BlockStatement(section.Statements, section.At), targetLabel, nestedBreakableDepth + 1));
            case TryStatement attempt:
                return ContainsBreakTargetingLoop(attempt.Body, targetLabel, nestedBreakableDepth) ||
                    attempt.Catches.Any(clause => ContainsBreakTargetingLoop(clause.Body, targetLabel, nestedBreakableDepth)) ||
                    attempt.Finally is not null && ContainsBreakTargetingLoop(attempt.Finally, targetLabel, nestedBreakableDepth);
            case OverflowContextStatement overflow:
                return ContainsBreakTargetingLoop(overflow.Body, targetLabel, nestedBreakableDepth);
            default:
                return false;
        }
    }
    private bool AlwaysAbrupt(Statement statement, bool breakTerminatesCurrentConstruct = true) => statement switch
    {
        ReturnStatement or ThrowStatement => true,
        BreakStatement transfer => transfer.IsContinue || breakTerminatesCurrentConstruct,
        OverflowContextStatement overflow => AlwaysAbrupt(overflow.Body, breakTerminatesCurrentConstruct),
        BlockStatement block => block.Statements.Any(child => AlwaysAbrupt(child, breakTerminatesCurrentConstruct)),
        IfStatement conditional => GetKnownBoolean(conditional.Condition) switch
        {
            true => AlwaysAbrupt(conditional.Then, breakTerminatesCurrentConstruct),
            false => conditional.Else is not null && AlwaysAbrupt(conditional.Else, breakTerminatesCurrentConstruct),
            null => conditional.Else is not null &&
                AlwaysAbrupt(conditional.Then, breakTerminatesCurrentConstruct) &&
                AlwaysAbrupt(conditional.Else, breakTerminatesCurrentConstruct)
        },
        TryStatement attempt => attempt.Finally is not null && AlwaysAbrupt(attempt.Finally, breakTerminatesCurrentConstruct) ||
            AlwaysAbrupt(attempt.Body, breakTerminatesCurrentConstruct) &&
            (attempt.Catches.Count == 0 || attempt.Catches.All(clause => AlwaysAbrupt(clause.Body, breakTerminatesCurrentConstruct))),
        SwitchStatement selection => _exhaustiveSwitches.Contains(selection) && selection.Sections.All(section =>
            section.Statements.Any(child => AlwaysAbrupt(child, breakTerminatesCurrentConstruct: false))),
        _ => false
    };

    private bool IsAlwaysTrueCondition(Expression? condition) => condition is null ||
        TryEvaluateConstant(condition, out var value, out _) && value.Boolean == true;

    private bool? GetKnownBoolean(Expression expression) =>
        TryEvaluateConstant(expression, out var value, out _) ? value.Boolean : null;

    private void AnalyzeStatement(Statement statement, MethodContext context)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (statement)
        {
            case BlockStatement block:
                var isMethodScopeBlock = context.ScopeDepth == 1 && context.CaptureBlockDepth == 0;
                context.EnterCaptureBlock(block, isMethodScopeBlock);
                context.Push(); var reachable = true;
                foreach (var child in block.Statements)
                {
                    if (!reachable) diagnostics.Add("LS3001", DiagnosticSeverity.Warning, "Unreachable code.", child.Location);
                    var beforeChild = reachable ? context.CaptureNullStates() : null;
                    AnalyzeStatement(child, context);
                    if (reachable && AlwaysAbrupt(child))
                    {
                        if (beforeChild is not null) context.RestoreNullStates(beforeChild);
                        reachable = false;
                    }
                }
                context.Pop(); context.ExitCaptureBlock(block); break;
            case VariableStatement variable:
                AnalyzeVariableDeclarations([variable], context); break;
            case VariableDeclarationStatement declaration:
                AnalyzeVariableDeclarations(declaration.Variables, context); break;
            case MultiAssignmentStatement assignment:
                AnalyzeMultiAssignment(assignment, context); break;
            case ReturnStatement ret:
                if (_finallyDepth > 0)
                    diagnostics.Add("LS0610", DiagnosticSeverity.Error, "A return statement cannot leave a finally block.", ret.Location);
                var expected = context.Method.Declaration.ReturnType;
                if (context.Method.IsStructConstructor)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "A struct constructor cannot return; let execution reach the end of its body.", ret.Location);
                    break;
                }
                if (ret.Value is null && expected != "void") diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A value must be returned from this method.", ret.Location);
                else if (ret.Value is not null && expected == "void") diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A void method cannot return a value.", ret.Location);
                else if (ret.Value is not null)
                {
                    var actual = AnalyzeExpression(ret.Value, context, expected);
                    if (GetScopeBoundDepth(ret.Value, context) is not null || IsReadOnlyReferenceExpression(ret.Value, context))
                        diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A scope-bound or readonly reference cannot be returned from its lexical scope.", ret.Value.Location);
                    if (!CanConvertInContext(actual, expected, ret.Value, context)) AddReferenceConversionDiagnostic(actual, expected, ret.Value, context, ret.Location);
                }
                break;
            case OverflowContextStatement overflow:
                var priorChecked = _checkedContext; _checkedContext = overflow.Checked;
                try { AnalyzeStatement(overflow.Body, context); } finally { _checkedContext = priorChecked; }
                break;
            case IfStatement conditional:
                RequireBool(conditional.Condition, context);
                var knownCondition = GetKnownBoolean(conditional.Condition);
                if (knownCondition == false)
                    diagnostics.Add("LS3001", DiagnosticSeverity.Warning, "Unreachable code.", conditional.Then.Location);
                else if (knownCondition == true && conditional.Else is not null)
                    diagnostics.Add("LS3001", DiagnosticSeverity.Warning, "Unreachable code.", conditional.Else.Location);
                var beforeIf = context.CaptureNullStates();
                RefineNullCondition(conditional.Condition, context, whenTrue: true);
                context.Push();
                var ifPatternCaptureScopes = DeclarePositiveTypePatternVariables(conditional.Condition, context);
                var discardedThenTransfers = knownCondition == false ? context.CaptureLoopTransferCounts() : null;
                AnalyzeStatement(conditional.Then, context);
                if (discardedThenTransfers is not null) context.DiscardLoopTransfersSince(discardedThenTransfers);
                var afterThen = context.CaptureNullStates();
                ExitCaptureScopes(context, ifPatternCaptureScopes);
                context.Pop();
                context.RestoreNullStates(beforeIf);
                RefineNullCondition(conditional.Condition, context, whenTrue: false);
                var discardedElseTransfers = knownCondition == true && conditional.Else is not null
                    ? context.CaptureLoopTransferCounts()
                    : null;
                if (conditional.Else is not null) AnalyzeStatement(conditional.Else, context);
                if (discardedElseTransfers is not null) context.DiscardLoopTransfersSince(discardedElseTransfers);
                var afterElse = context.CaptureNullStates();
                var thenAbrupt = AlwaysAbrupt(conditional.Then);
                var elseAbrupt = conditional.Else is not null && AlwaysAbrupt(conditional.Else);
                if (knownCondition == true) context.RestoreNullStates(afterThen);
                else if (knownCondition == false) context.RestoreNullStates(afterElse);
                else if (thenAbrupt && elseAbrupt) context.RestoreNullStates(beforeIf);
                else if (thenAbrupt) context.RestoreNullStates(afterElse);
                else if (elseAbrupt) context.RestoreNullStates(afterThen);
                else context.RestoreNullStates(MergeNullStates(afterThen, afterElse));
                break;
            case WhileStatement loop:
                RequireBool(loop.Condition, context);
                var whileEntryState = context.CaptureNullStates();
                if (GetKnownBoolean(loop.Condition) == false)
                {
                    diagnostics.Add("LS3001", DiagnosticSeverity.Warning, "Unreachable code.", loop.Body.Location);
                    RefineNullCondition(loop.Condition, context, whenTrue: true);
                    var discardedLoopTransfers = context.CaptureLoopTransferCounts();
                    _ = AnalyzeLoopIteration(loop.Body, context, loop.Label, loop.Location, loop.Condition);
                    context.DiscardLoopTransfersSince(discardedLoopTransfers);
                    context.RestoreNullStates(whileEntryState);
                    RequireBool(loop.Condition, context);
                    RefineNullCondition(loop.Condition, context, whenTrue: false);
                }
                else
                {
                    var whileHeadState = whileEntryState;
                    var whileBreakStates = new List<IReadOnlyDictionary<string, NullState>>();
                    while (true)
                    {
                        context.RestoreNullStates(whileHeadState);
                        RequireBool(loop.Condition, context);
                        RefineNullCondition(loop.Condition, context, whenTrue: true);
                        var whileIteration = AnalyzeLoopIteration(loop.Body, context, loop.Label, loop.Location, loop.Condition);
                        if (whileIteration.Flow is not null) whileBreakStates.AddRange(whileIteration.Flow.BreakStates);
                        var backStates = new List<IReadOnlyDictionary<string, NullState>>();
                        if (!AlwaysAbrupt(loop.Body)) backStates.Add(whileIteration.BodyState);
                        if (whileIteration.Flow is not null) backStates.AddRange(whileIteration.Flow.ContinueStates);
                        var nextHead = MergeNullStatePaths([whileEntryState, .. backStates]);
                        if (NullStateMapsEqual(whileHeadState, nextHead))
                        {
                            whileHeadState = nextHead;
                            break;
                        }
                        whileHeadState = nextHead;
                    }
                    context.RestoreNullStates(whileHeadState);
                    RequireBool(loop.Condition, context);
                    RefineNullCondition(loop.Condition, context, whenTrue: false);
                    var whileNormalExit = context.CaptureNullStates();
                    var whileFlow = CreateLoopExitFlow(context, loop.Label, whileBreakStates);
                    MergeLoopExitStates(context, whileFlow, whileNormalExit, !IsAlwaysTrueCondition(loop.Condition));
                }
                break;
            case DoWhileStatement loop:
                var doEntryState = context.CaptureNullStates();
                var doHeadState = doEntryState;
                var doBreakStates = new List<IReadOnlyDictionary<string, NullState>>();
                IReadOnlyDictionary<string, NullState>? doConditionState = null;
                var reachesDoCondition = false;
                while (true)
                {
                    context.RestoreNullStates(doHeadState);
                    var doIteration = AnalyzeLoopIteration(loop.Body, context, loop.Label, loop.Location);
                    if (doIteration.Flow is not null) doBreakStates.AddRange(doIteration.Flow.BreakStates);
                    var backStates = new List<IReadOnlyDictionary<string, NullState>>();
                    if (!AlwaysAbrupt(loop.Body)) backStates.Add(doIteration.BodyState);
                    if (doIteration.Flow is not null) backStates.AddRange(doIteration.Flow.ContinueStates);
                    reachesDoCondition = backStates.Count > 0;
                    if (!reachesDoCondition) break;
                    context.RestoreNullStates(MergeNullStatePaths(backStates));
                    RequireBool(loop.Condition, context);
                    doConditionState = context.CaptureNullStates();
                    RefineNullCondition(loop.Condition, context, whenTrue: true);
                    var nextHead = MergeNullStatePaths([doEntryState, context.CaptureNullStates()]);
                    if (NullStateMapsEqual(doHeadState, nextHead))
                    {
                        doHeadState = nextHead;
                        break;
                    }
                    doHeadState = nextHead;
                }
                if (reachesDoCondition && doConditionState is not null) context.RestoreNullStates(doConditionState);
                RefineNullCondition(loop.Condition, context, whenTrue: false);
                var doNormalExit = context.CaptureNullStates();
                var doWhileFlow = CreateLoopExitFlow(context, loop.Label, doBreakStates);
                MergeLoopExitStates(context, doWhileFlow, doNormalExit, reachesDoCondition && !IsAlwaysTrueCondition(loop.Condition));
                break;
            case ForStatement loop:
                context.EnterForStatement(loop);
                context.Push();
                if (loop.Initializer is not null)
                {
                    context.ForInitializerOwner = loop;
                    try { AnalyzeStatement(loop.Initializer, context); }
                    finally { context.ForInitializerOwner = null; }
                }
                if (loop.Condition is not null) RequireBool(loop.Condition, context);
                var forEntryState = context.CaptureNullStates();
                if (loop.Condition is not null && GetKnownBoolean(loop.Condition) == false)
                {
                    diagnostics.Add("LS3001", DiagnosticSeverity.Warning, "Unreachable code.", loop.Body.Location);
                    RefineNullCondition(loop.Condition, context, whenTrue: true);
                    var discardedLoopTransfers = context.CaptureLoopTransferCounts();
                    _ = AnalyzeLoopIteration(loop.Body, context, loop.Label, loop.Location, loop.Condition);
                    context.DiscardLoopTransfersSince(discardedLoopTransfers);
                    context.RestoreNullStates(forEntryState);
                    RequireBool(loop.Condition, context);
                    RefineNullCondition(loop.Condition, context, whenTrue: false);
                }
                else
                {
                    var forHeadState = forEntryState;
                    var forBreakStates = new List<IReadOnlyDictionary<string, NullState>>();
                    while (true)
                    {
                        context.RestoreNullStates(forHeadState);
                        if (loop.Condition is not null)
                        {
                            RequireBool(loop.Condition, context);
                            RefineNullCondition(loop.Condition, context, whenTrue: true);
                        }
                        var forIteration = AnalyzeLoopIteration(loop.Body, context, loop.Label, loop.Location, loop.Condition);
                        if (forIteration.Flow is not null) forBreakStates.AddRange(forIteration.Flow.BreakStates);
                        var backStates = new List<IReadOnlyDictionary<string, NullState>>();
                        if (!AlwaysAbrupt(loop.Body)) backStates.Add(forIteration.BodyState);
                        if (forIteration.Flow is not null) backStates.AddRange(forIteration.Flow.ContinueStates);
                        IReadOnlyDictionary<string, NullState> nextHead = forEntryState;
                        if (backStates.Count > 0)
                        {
                            context.RestoreNullStates(MergeNullStatePaths(backStates));
                            foreach (var iterator in loop.Iterators) AnalyzeExpression(iterator, context);
                            nextHead = MergeNullStatePaths([forEntryState, context.CaptureNullStates()]);
                        }
                        if (NullStateMapsEqual(forHeadState, nextHead))
                        {
                            forHeadState = nextHead;
                            break;
                        }
                        forHeadState = nextHead;
                    }
                    context.RestoreNullStates(forHeadState);
                    if (loop.Condition is not null) RequireBool(loop.Condition, context);
                    if (loop.Condition is not null) RefineNullCondition(loop.Condition, context, whenTrue: false);
                    var forNormalExit = context.CaptureNullStates();
                    var forFlow = CreateLoopExitFlow(context, loop.Label, forBreakStates);
                    MergeLoopExitStates(context, forFlow, forNormalExit, !IsAlwaysTrueCondition(loop.Condition));
                }
                context.Pop();
                context.ExitForStatement(loop);
                break;
            case ForInStatement loop:
                AnalyzeForIn(loop, context); break;
            case SwitchStatement selection:
                AnalyzeSwitch(selection, context); break;
            case BreakStatement transfer:
                if (_finallyScopes.TryPeek(out var finallyScope) && context.TransferLeavesFinally(transfer, finallyScope.LoopDepth, finallyScope.SwitchDepth))
                    diagnostics.Add("LS0610", DiagnosticSeverity.Error, "A break or continue statement cannot leave a finally block.", transfer.Location);
                if (transfer.Label is not null && !context.HasLoopLabel(transfer.Label)) diagnostics.Add("LS0603", DiagnosticSeverity.Error, $"Loop label '{transfer.Label}' is not visible here.", transfer.Location);
                else if (transfer.IsContinue && context.LoopDepth == 0) diagnostics.Add("LS0605", DiagnosticSeverity.Error, "'continue' is not inside a loop.", transfer.Location);
                else if (!transfer.IsContinue && transfer.Label is null && context.LoopDepth == 0 && context.SwitchDepth == 0) diagnostics.Add("LS0605", DiagnosticSeverity.Error, "'break' is not inside a loop or switch.", transfer.Location);
                else if (!transfer.IsContinue) context.RecordLoopBreak(transfer.Label);
                else context.RecordLoopContinue(transfer.Label);
                break;
            case ThrowStatement thrown:
                if (thrown.Value is null)
                {
                    if (_catchDepth == 0)
                        diagnostics.Add("LS1800", DiagnosticSeverity.Error, "A bare 'throw;' is valid only inside a catch block.", thrown.Location);
                    else
                        _throwTypes[thrown] = "<rethrow>";
                    break;
                }
                var thrownType = AnalyzeExpression(thrown.Value, context);
                var nonNullableThrownType = NullableUnderlyingType(thrownType);
                if (thrownType.EndsWith("?", StringComparison.Ordinal))
                    diagnostics.Add("LS1800", DiagnosticSeverity.Error, "A thrown exception must be non-nullable; use the null-forgiving operator if it may be null.", thrown.Value.Location);
                if (!IsClassType(nonNullableThrownType) || !IsExceptionType(nonNullableThrownType))
                    diagnostics.Add("LS1800", DiagnosticSeverity.Error, $"A thrown value must be a non-nullable class deriving from System.Exception, not '{thrownType}'.", thrown.Value.Location);
                else
                {
                    _throwTypes[thrown] = nonNullableThrownType;
                    if (GetScopeBoundDepth(thrown.Value, context) is not null || IsReadOnlyReferenceExpression(thrown.Value, context))
                        diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A scope-bound or readonly reference cannot escape by being thrown.", thrown.Value.Location);
                }
                break;
            case TryStatement tryStatement:
                var catchTypes = new List<string>();
                var priorWasCatchAll = false;
                foreach (var clause in tryStatement.Catches)
                {
                    if (clause.IsCatchAll)
                    {
                        if (clause.VariableName is not null)
                            diagnostics.Add("LS1800", DiagnosticSeverity.Error, "A catch-all clause cannot declare an exception variable.", clause.At);
                        if (priorWasCatchAll || clause != tryStatement.Catches[^1])
                            diagnostics.Add("LS1801", DiagnosticSeverity.Error, "A catch-all clause must be the last catch clause.", clause.At);
                        if (catchTypes.Contains("System.Exception", StringComparer.Ordinal))
                            diagnostics.Add("LS1801", DiagnosticSeverity.Error, "The catch-all clause is unreachable because an earlier clause catches System.Exception.", clause.At);
                        priorWasCatchAll = true;
                        continue;
                    }
                    if (clause.TypeName is null) continue;
                    var catchType = NormalizeEnumType(clause.TypeName, context.Method.Owner);
                    var inaccessibleCatchType = ValidateTypeAccessibility(clause.TypeName, context.Method.Owner, clause.At);
                    if (inaccessibleCatchType)
                    {
                        // Keep the invalid catch type bound for its variable and
                        // reachability analysis, while suppressing a secondary
                        // "not an exception" error caused only by access filtering.
                        if (_classTypes.ContainsKey(catchType) && IsExceptionType(catchType))
                        {
                            _exceptionCatchTypes[clause] = catchType;
                            catchTypes.Add(catchType);
                        }
                        else
                            diagnostics.Add("LS1800", DiagnosticSeverity.Error, $"Catch type '{clause.TypeName}' must be a class deriving from System.Exception.", clause.At);
                        continue;
                    }
                    if (!IsClassType(catchType) || !IsExceptionType(catchType))
                    {
                        diagnostics.Add("LS1800", DiagnosticSeverity.Error, $"Catch type '{clause.TypeName}' must be a class deriving from System.Exception.", clause.At);
                        continue;
                    }
                    _exceptionCatchTypes[clause] = catchType;
                    if (catchTypes.Any(previous => previous == catchType || IsBaseClassOf(previous, catchType)))
                        diagnostics.Add("LS1801", DiagnosticSeverity.Error, $"Catch clause for '{catchType}' is unreachable because an earlier clause catches it.", clause.At);
                    catchTypes.Add(catchType);
                }
                var beforeTry = context.CaptureNullStates();
                AnalyzeStatement(tryStatement.Body, context);
                var tryOutcomes = new List<IReadOnlyDictionary<string, NullState>> { context.CaptureNullStates() };
                foreach (var clause in tryStatement.Catches)
                {
                    context.RestoreNullStates(beforeTry);
                    context.Push();
                    if (clause.VariableName is { } catchVariable && _exceptionCatchTypes.TryGetValue(clause, out var boundCatchType))
                        context.Declare(catchVariable, boundCatchType, mutable: false, clause.At, blockOwner: clause.Body);
                    _catchDepth++;
                    AnalyzeStatement(clause.Body, context);
                    _catchDepth--;
                    tryOutcomes.Add(context.CaptureNullStates());
                    context.Pop();
                }
                var mergedTryState = tryOutcomes[0];
                foreach (var outcome in tryOutcomes.Skip(1)) mergedTryState = MergeNullStates(mergedTryState, outcome);
                context.RestoreNullStates(mergedTryState);
                if (tryStatement.Finally is not null)
                {
                    _finallyDepth++;
                    _finallyScopes.Push((context.LoopDepth, context.SwitchDepth));
                    AnalyzeStatement(tryStatement.Finally, context);
                    _finallyScopes.Pop();
                    _finallyDepth--;
                }
                break;
            case ExpressionStatement expression:
                if (expression.Value is not (CallExpression or AllocationExpression or BinaryExpression { Operator: "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=" } or UnaryExpression { Operator: "++" or "--" })) diagnostics.Add("LS0601", DiagnosticSeverity.Error, "An expression statement must be an invocation, allocation, assignment, or increment/decrement.", expression.Location);
                AnalyzeExpression(expression.Value, context); break;
            case EmptyStatement: break;
        }
    }

    private static void MergeLoopExitStates(MethodContext context, MethodContext.LoopFlow? loop,
        IReadOnlyDictionary<string, NullState> normalExitState, bool hasNormalExit)
    {
        var exitStates = new List<IReadOnlyDictionary<string, NullState>>();
        if (hasNormalExit) exitStates.Add(normalExitState);
        if (loop is not null)
            exitStates.AddRange(loop.BreakStates.Select(context.FilterVisibleNullStates));
        if (exitStates.Count == 0)
        {
            context.RestoreNullStates(context.CaptureNullStates().Keys.ToDictionary(name => name,
                _ => NullState.MaybeNull, StringComparer.Ordinal));
            return;
        }
        context.RestoreNullStates(MergeNullStatePaths(exitStates));
    }

    private (MethodContext.LoopFlow? Flow, IReadOnlyDictionary<string, NullState> BodyState) AnalyzeLoopIteration(
        Statement body, MethodContext context, string? label, SourceLocation location, Expression? condition = null)
    {
        context.Push();
        var patternCaptureScopes = condition is null ? [] : DeclarePositiveTypePatternVariables(condition, context);
        EnterLoop(context, label, location);
        AnalyzeStatement(body, context);
        var flow = context.ExitLoop();
        var bodyState = context.CaptureNullStates();
        var breakStates = flow?.BreakStates.ToArray() ?? [];
        var continueStates = flow?.ContinueStates.ToArray() ?? [];
        ExitCaptureScopes(context, patternCaptureScopes);
        context.Pop();
        if (flow is not null)
        {
            flow.BreakStates.Clear();
            flow.BreakStates.AddRange(breakStates.Select(context.FilterVisibleNullStates));
            flow.ContinueStates.Clear();
            flow.ContinueStates.AddRange(continueStates.Select(context.FilterVisibleNullStates));
        }
        return (flow, context.FilterVisibleNullStates(bodyState));
    }

    private static MethodContext.LoopFlow CreateLoopExitFlow(MethodContext context, string? label,
        IEnumerable<IReadOnlyDictionary<string, NullState>> breakStates)
    {
        var flow = new MethodContext.LoopFlow(label, context.SwitchDepth);
        flow.BreakStates.AddRange(breakStates);
        return flow;
    }

    private static bool NullStateMapsEqual(IReadOnlyDictionary<string, NullState> first,
        IReadOnlyDictionary<string, NullState> second) => first.Count == second.Count &&
        first.All(pair => second.TryGetValue(pair.Key, out var state) && pair.Value == state);

    private static IReadOnlyDictionary<string, NullState> MergeNullStatePaths(
        IEnumerable<IReadOnlyDictionary<string, NullState>> states)
    {
        using var iterator = states.GetEnumerator();
        if (!iterator.MoveNext()) return new Dictionary<string, NullState>(StringComparer.Ordinal);
        var merged = new Dictionary<string, NullState>(iterator.Current, StringComparer.Ordinal);
        while (iterator.MoveNext()) merged = new Dictionary<string, NullState>(MergeNullStates(merged, iterator.Current), StringComparer.Ordinal);
        return merged;
    }
    private void AnalyzeForIn(ForInStatement loop, MethodContext context)
    {
        var sourceType = AnalyzeExpression(loop.Collection, context);
        if (sourceType == "?") return;
        if (IsNullableClassType(sourceType) || IsNullableArrayType(sourceType))
        {
            diagnostics.Add("LS1303", DiagnosticSeverity.Error, "A for-in collection must be non-nullable; check the value before iterating.", loop.Collection.Location);
            return;
        }
        var isManagedArray = TryParseManagedArrayType(sourceType, out var elementType, out var arrayRank) && arrayRank <= 3;
        if (isManagedArray)
            _managedArrayForInElementTypes[loop] = elementType;
        else if (TryGetEnumerableElementTypes(sourceType, context.Method.Owner, out var enumerableElementTypes))
        {
            var candidates = enumerableElementTypes;
            if (loop.TypeName is not null)
            {
                var requestedType = NormalizeEnumType(loop.TypeName, context.Method.Owner);
                candidates = candidates.Where(candidate => CanConvert(candidate, requestedType, null)).ToArray();
                if (candidates.Length > 1)
                    diagnostics.Add("LS0640", DiagnosticSeverity.Error, "The collection implements IEnumerable<T> for multiple element types; the explicit loop-variable type must select exactly one.", loop.Location);
                else if (candidates.Length == 0)
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"No IEnumerable<T> element type can be converted to loop variable type '{requestedType}'.", loop.Location);
                else
                    LowerEnumerableForIn(loop, sourceType, candidates[0], requestedType, context);
            }
            else if (enumerableElementTypes.Length > 1)
                diagnostics.Add("LS0640", DiagnosticSeverity.Error, "The collection implements IEnumerable<T> for multiple element types; specify a loop-variable type that selects exactly one.", loop.Location);
            else
                LowerEnumerableForIn(loop, sourceType, enumerableElementTypes[0], null, context);
            return;
        }
        else if (!TryParseInlineArrayType(sourceType, out elementType, out var length) || length <= 0 || !IsInlineArrayLocation(loop.Collection, context))
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "The current backend supports for-in over managed arrays and addressable inline arrays.", loop.Collection.Location);
            return;
        }
        if (!isManagedArray && !UnsafeEnabledAt(loop.Location))
            diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Inline array iteration requires unsafe code, but unsafe is disabled for this compilation.", loop.Location);
        else if (!isManagedArray && !loop.IsUnsafeContext)
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Inline array iteration may only be used in an unsafe context.", loop.Location);

        elementType = NormalizeEnumType(elementType, context.Method.Owner);
        var loopType = loop.TypeName is null ? elementType : NormalizeEnumType(loop.TypeName, context.Method.Owner);
        _forInTypes[loop] = loopType;
        if (loop.TypeName is not null)
        {
            ValidateDeclaredType(loopType, "Local", loop.Location, context.Method.Owner);
            if (!CanConvert(elementType, loopType, null))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert inline array element '{elementType}' to loop variable type '{loopType}'.", loop.Location);
        }
        if (context.Exists(loop.Name))
            diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Loop variable '{loop.Name}' conflicts with a name in an enclosing scope.", loop.Location);

        var beforeLoop = context.CaptureNullStates();
        context.EnterForInStatement(loop);
        context.Push();
        context.Declare(loop.Name, loopType, loop.Mutable, loop.Location, forInOwner: loop);
        var loopHead = beforeLoop;
        var loopBreakStates = new List<IReadOnlyDictionary<string, NullState>>();
        var iterationExitStates = new List<IReadOnlyDictionary<string, NullState>>();
        while (true)
        {
            context.RestoreNullStates(loopHead);
            if (IsNullableType(loopType)) context.SetNullState(loop.Name, NullState.MaybeNull);
            var loopIteration = AnalyzeLoopIteration(loop.Body, context, loop.Label, loop.Location);
            if (loopIteration.Flow is not null) loopBreakStates.AddRange(loopIteration.Flow.BreakStates);
            var backStates = new List<IReadOnlyDictionary<string, NullState>>();
            if (!AlwaysAbrupt(loop.Body)) backStates.Add(loopIteration.BodyState);
            if (loopIteration.Flow is not null) backStates.AddRange(loopIteration.Flow.ContinueStates);
            iterationExitStates.AddRange(backStates.Select(state => RemoveLoopVariableNullState(state, loop.Name)));
            var nextHead = MergeNullStatePaths([beforeLoop, .. backStates.Select(state => RemoveLoopVariableNullState(state, loop.Name))]);
            if (NullStateMapsEqual(loopHead, nextHead))
            {
                loopHead = nextHead;
                break;
            }
            loopHead = nextHead;
        }
        context.Pop();
        context.ExitForInStatement(loop);
        var normalExitStates = new List<IReadOnlyDictionary<string, NullState>> { beforeLoop };
        normalExitStates.AddRange(iterationExitStates.Select(context.FilterVisibleNullStates));
        var loopFlow = CreateLoopExitFlow(context, loop.Label, loopBreakStates);
        MergeLoopExitStates(context, loopFlow, MergeNullStatePaths(normalExitStates), hasNormalExit: true);
    }

    private static IReadOnlyDictionary<string, NullState> RemoveLoopVariableNullState(
        IReadOnlyDictionary<string, NullState> state, string variableName) => state
        .Where(pair => pair.Key != variableName && !pair.Key.StartsWith("path:" + variableName + ".", StringComparison.Ordinal))
        .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    private void LowerEnumerableForIn(ForInStatement loop, string sourceType, string elementType,
        string? requestedElementType, MethodContext context)
    {
        // Rewrite to ordinary calls and a while loop so the existing binder, virtual/interface
        // dispatch, exception unwinding, and reference-counted local cleanup implement the
        // specified iterator semantics without a second backend-specific loop path.
        var availableInterfaces = new HashSet<string>(StringComparer.Ordinal);
        if (_classTypes.ContainsKey(sourceType))
            availableInterfaces.UnionWith(GetImplementedInterfaceNames(sourceType));
        else if (_interfaceTypes.TryGetValue(sourceType, out var sourceInterface))
            availableInterfaces.UnionWith(GetInterfaceHierarchy(sourceInterface).Select(GetQualifiedTypeName));
        else if (_structTypes.TryGetValue(sourceType, out var sourceStruct))
            foreach (var declaredBase in sourceStruct.Declaration.BaseTypes ?? [])
                foreach (var implemented in ResolveInterfaceTypes(declaredBase, sourceStruct.Declaration))
                    foreach (var inherited in GetInterfaceHierarchy(implemented)) availableInterfaces.Add(GetQualifiedTypeName(inherited));

        var enumerableInstance = _genericTypeInstantiations.FirstOrDefault(instance =>
            availableInterfaces.Contains(GetQualifiedTypeName(instance.Specialized)) &&
            instance.Arguments.Count == 1 && IsEnumerableDefinition(instance.Definition) &&
            NormalizeEnumType(instance.Arguments[0], context.Method.Owner) == elementType);
        if (enumerableInstance is null)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                "The IEnumerable<T> iteration protocol is recognized, but its closed interface contract could not be resolved.", loop.Collection.Location);
            return;
        }
        var enumerableInterface = enumerableInstance.Specialized;
        var iterate = enumerableInterface.Members.OfType<MethodDeclaration>().FirstOrDefault(method =>
            method.Name == "Iterate" && !method.IsStatic && method.Parameters.Count == 0);
        var iteratorTypeName = iterate is null ? null : NormalizeEnumType(iterate.ReturnType, enumerableInterface);
        var iteratorTypes = iteratorTypeName is null ? [] : FindTypes(iteratorTypeName, enumerableInterface)
            .Where(type => type.Kind == "interface").ToArray();
        var iteratorInstance = iteratorTypes.Length == 1
            ? _genericTypeInstantiations.FirstOrDefault(instance =>
                GetQualifiedTypeName(instance.Specialized) == GetQualifiedTypeName(iteratorTypes[0]) &&
                IsIteratorDefinition(instance.Definition) && instance.Arguments.Count == 1 &&
                NormalizeEnumType(instance.Arguments[0], context.Method.Owner) == elementType)
            : null;
        if (iterate is null || iteratorTypes.Length != 1 || iteratorInstance is null)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                "The IEnumerable<T> iteration protocol requires an instance Iterate() method returning IIterator<T>.", loop.Collection.Location);
            return;
        }
        var iteratorInterface = iteratorTypes[0];
        var iteratorType = GetQualifiedTypeName(iteratorInterface);
        var hasNext = iteratorInterface.Members.OfType<MethodDeclaration>().Any(method =>
            method.Name == "Next" && !method.IsStatic && method.ReturnType == "bool" && method.Parameters.Count == 0);
        var current = iteratorInterface.Members.OfType<ComputedPropertyDeclaration>().FirstOrDefault(property =>
            property.Name == "Current" && property.GetterBody is not null &&
            NormalizeEnumType(property.TypeName, iteratorInterface) == elementType);
        if (!hasNext || current is null)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                $"The IIterator<T> contract '{iteratorType}' must provide bool Next() and a readable T Current property.", loop.Collection.Location);
            return;
        }

        var iteratorName = "__ls_forin_iterator_" + _forInIteratorCounter++;
        while (context.Exists(iteratorName)) iteratorName = "__ls_forin_iterator_" + _forInIteratorCounter++;
        var at = loop.Location;
        var cast = new CastExpression(GetQualifiedTypeName(enumerableInterface), loop.Collection, at);
        var iterateCall = new CallExpression(new MemberAccessExpression(cast, "Iterate", at), [], at);
        var iteratorDeclaration = new VariableStatement(iteratorName, true, iteratorType, iterateCall, at);
        var iteratorForNext = new NameExpression(iteratorName, at);
        var nextCall = new CallExpression(new MemberAccessExpression(iteratorForNext, "Next", at), [], at);
        var currentRead = new MemberAccessExpression(new NameExpression(iteratorName, at), "Current", at);
        var iterationVariable = new VariableStatement(loop.Name, loop.Mutable, requestedElementType, currentRead, loop.Location);
        var iterationStatements = loop.Body is BlockStatement bodyBlock
            ? new[] { iterationVariable }.Concat(bodyBlock.Statements).ToArray()
            : [iterationVariable, loop.Body];
        var loweredBody = new BlockStatement(iterationStatements, loop.Body.Location);
        var lowered = new BlockStatement([
            iteratorDeclaration,
            new WhileStatement(nextCall, loweredBody, loop.Location, loop.Label)
        ], loop.Location);
        _loweredForInLoops[loop] = lowered;
        AnalyzeStatement(lowered, context);
    }

    private static bool IsEnumerableDefinition(TypeDeclaration definition) => definition.Name == "IEnumerable" &&
        definition.Kind == "interface" && definition.Namespace is "" or "System.Collections" or "System.Collections.Generic";

    private static bool IsIteratorDefinition(TypeDeclaration definition) => definition.Name == "IIterator" &&
        definition.Kind == "interface" && definition.Namespace is "" or "System.Collections" or "System.Collections.Generic";

    private bool TryGetEnumerableElementTypes(string sourceType, TypeDeclaration context, out string[] elementTypes)
    {
        var found = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Queue<TypeDeclaration>(FindTypes(sourceType, context).Where(type => type.Kind is "class" or "struct" or "interface"));
        var visited = new HashSet<string>(StringComparer.Ordinal);
        void AddEnumerableElementType(string declaredType)
        {
            var baseName = StripGenericArguments(declaredType);
            if (!(baseName is "IEnumerable" or "System.Collections.IEnumerable" or "System.Collections.Generic.IEnumerable") || !declaredType.Contains('<', StringComparison.Ordinal)) return;
            var open = declaredType.IndexOf('<');
            var close = declaredType.LastIndexOf('>');
            if (open >= 0 && close > open)
                found.Add(NormalizeEnumType(declaredType[(open + 1)..close], context));
        }
        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            var currentName = GetQualifiedTypeName(current);
            if (!visited.Add(currentName)) continue;
            var currentInstantiation = _genericTypeInstantiations.FirstOrDefault(instance =>
                GetQualifiedTypeName(instance.Specialized) == currentName && IsEnumerableDefinition(instance.Definition) && instance.Arguments.Count == 1);
            if (currentInstantiation is not null)
                found.Add(NormalizeEnumType(currentInstantiation.Arguments[0], context));
            foreach (var declaredBase in current.BaseTypes ?? [])
            {
                var baseName = StripGenericArguments(declaredBase);
                AddEnumerableElementType(declaredBase);
                foreach (var interfaceType in ResolveInterfaceTypes(declaredBase, current))
                {
                    pending.Enqueue(interfaceType);
                foreach (var inheritedInterface in GetInterfaceHierarchy(interfaceType))
                {
                    pending.Enqueue(inheritedInterface);
                foreach (var inheritedBase in inheritedInterface.BaseTypes ?? [])
                    AddEnumerableElementType(inheritedBase);
                }
                }
                foreach (var classBase in FindTypes(baseName, current).Where(type => type.Kind == "class")) pending.Enqueue(classBase);
            }
            if (_classTypes.TryGetValue(currentName, out var classSymbol) && classSymbol.BaseTypeName is { } resolvedBase &&
                _classTypes.TryGetValue(resolvedBase, out var resolvedBaseSymbol)) pending.Enqueue(resolvedBaseSymbol.Declaration);
        }
        elementTypes = found.OrderBy(type => type, StringComparer.Ordinal).ToArray();
        return elementTypes.Length > 0;
    }

    private void AnalyzeSwitch(SwitchStatement statement, MethodContext context)
    {
        var switchType = AnalyzeExpression(statement.Value, context);
        var supportsReferenceOrNullablePatterns = IsNullableType(switchType) || IsClassType(switchType) || IsInterfaceType(switchType) || IsDelegateType(switchType) || IsNullableClassType(switchType) || IsManagedArrayReferenceType(switchType);
        if (switchType != "?" && IsSupportedType(switchType) && !(NumericTypes.Contains(switchType) || switchType is "bool" or "string" || IsEnumType(switchType) || IsStructType(switchType) || supportsReferenceOrNullablePatterns))
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"A switch expression must have a numeric, bool, string, nullable, or reference type, not '{switchType}'.", statement.Value.Location);

        var hasDefault = false; var unconditionalCatchAll = false; var unconditionalLiteralKeys = new HashSet<string>(StringComparer.Ordinal);
        var unconditionalNumericIntervals = new List<SwitchNumericInterval>();
        var unconditionalClassTypes = new List<string>();
        var unconditionalNonNullCatchAll = false;
        var relationalType = IsNullableValueType(switchType) ? switchType[..^1] : switchType;
        var governingReference = IsClassReferenceType(switchType) || IsDelegateType(switchType) || IsNullableDelegateType(switchType) || IsStringType(switchType) || IsManagedArrayReferenceType(switchType)
            ? NullableUnderlyingType(switchType)
            : null;
        foreach (var section in statement.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label.IsUnsupportedPattern) continue;
                if (label.IsDefault)
                {
                    if (hasDefault) diagnostics.Add("LS0622", DiagnosticSeverity.Error, "A switch may contain only one default label.", label.At);
                    else if (unconditionalCatchAll || unconditionalNonNullCatchAll && (!IsNullableType(switchType) || unconditionalLiteralKeys.Contains("null")))
                        diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This switch label is subsumed by earlier unguarded patterns.", label.At);
                    hasDefault = true;
                    unconditionalCatchAll = true;
                    continue;
                }
                if (unconditionalCatchAll)
                {
                    diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This switch label is subsumed by an earlier catch-all pattern.", label.At);
                    continue;
                }
                else if (unconditionalNonNullCatchAll && !IsNullSwitchPattern(label))
                {
                    diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This switch label is subsumed by earlier unguarded patterns that match every non-null value.", label.At);
                    continue;
                }
                ValidateSwitchPattern(label, switchType, relationalType, context, trackSubsumption: true, recordForLater: label.Guard is null, unconditionalLiteralKeys, unconditionalNumericIntervals, unconditionalClassTypes);
                if (label.CombinedOperator is not null && !TryGetSwitchPatternBindings(label, out _))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error,
                        "Pattern variables must be definitely assigned on every matching alternative; 'or' alternatives must bind the same names and types, and an 'and' pattern cannot declare a name twice.", label.At);
                if (label.Guard is not null)
                {
                    var hasGuardScope = false;
                    context.EnterCaptureBlock(label.GuardCaptureScope, isMethodScope: false);
                    if (TryGetSwitchPatternBindings(label, out var guardBindings) && guardBindings.Count > 0)
                    {
                        context.Push();
                        hasGuardScope = true;
                        foreach (var (guardVariable, guardVariableType) in guardBindings)
                        {
                            if (context.Exists(guardVariable)) diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Pattern variable '{guardVariable}' conflicts with an existing name in the current context.", label.At);
                            else context.Declare(guardVariable, guardVariableType, mutable: false, label.At, blockOwner: label.GuardCaptureScope);
                        }
                    }
                    var guardType = AnalyzeExpression(label.Guard, context);
                    if (guardType != "bool")
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"A switch guard must have type 'bool', not '{guardType}'.", label.Guard.Location);
                    if (hasGuardScope) context.Pop();
                    context.ExitCaptureBlock(label.GuardCaptureScope);
                }
                else if (IsCatchAllSwitchPattern(label)) unconditionalCatchAll = true;
                else
                {
                    var coversAllNonNullReferences = governingReference is not null &&
                        (IsClassType(governingReference) || IsInterfaceType(governingReference) || IsDelegateType(governingReference) || IsManagedArrayReferenceType(governingReference) || governingReference == "string") &&
                        _switchTypePatterns.TryGetValue(label, out var patternType) &&
                        (patternType == governingReference || IsReferenceAssignable(patternType, governingReference));
                    var governingValueType = NullableUnderlyingType(switchType);
                    var coversAllNonNullBoxedValues = IsBoxableValueType(governingValueType) &&
                        _switchTypePatterns.TryGetValue(label, out var boxedPatternType) && boxedPatternType == governingValueType;
                    var coversAllNonNullValues = IsNonNullCatchAllSwitchPattern(label) || coversAllNonNullReferences || coversAllNonNullBoxedValues ||
                        IsFiniteSwitchDomainExhaustive(switchType, unconditionalLiteralKeys, unconditionalNumericIntervals, [], requireNull: false);
                    if (coversAllNonNullValues)
                    {
                        unconditionalNonNullCatchAll = true;
                        if (!IsNullableType(switchType) || unconditionalLiteralKeys.Contains("null")) unconditionalCatchAll = true;
                    }
                    if (IsNullSwitchPattern(label) && unconditionalNonNullCatchAll) unconditionalCatchAll = true;
                }
            }
        }
        if (governingReference is not null && (IsClassType(governingReference) || IsInterfaceType(governingReference) || IsDelegateType(governingReference) || IsManagedArrayReferenceType(governingReference) || governingReference == "string"))
        {
            var unguardedLabels = statement.Sections.SelectMany(section => section.Labels).Where(label => label.Guard is null).ToArray();
            var allNonNullValuesMatch = unguardedLabels.Any(label => _switchTypePatterns.TryGetValue(label, out var patternType) &&
                (patternType == governingReference || IsReferenceAssignable(patternType, governingReference))) || unguardedLabels.Any(IsNonNullCatchAllSwitchPattern);
            var nullIsCovered = unguardedLabels.Any(IsNullSwitchPattern);
            var nullableReference = IsNullableClassType(switchType) || IsNullableDelegateType(switchType) || IsNullableStringType(switchType) || IsNullableArrayType(switchType);
            if (allNonNullValuesMatch && (!nullableReference || nullIsCovered)) _exhaustiveSwitches.Add(statement);
        }
        else if (IsBoxableValueType(NullableUnderlyingType(switchType)) &&
            statement.Sections.SelectMany(section => section.Labels).Any(label => label.Guard is null &&
                _switchTypePatterns.TryGetValue(label, out var patternType) && patternType == NullableUnderlyingType(switchType)) &&
            (!IsNullableValueType(switchType) || statement.Sections.SelectMany(section => section.Labels).Any(IsNullSwitchPattern)) ||
            IsFiniteSwitchDomainExhaustive(switchType, unconditionalLiteralKeys, unconditionalNumericIntervals, statement.Sections.SelectMany(section => section.Labels)))
        {
            _exhaustiveSwitches.Add(statement);
        }
        if (IsManagedArrayReferenceType(switchType))
        {
            var arrayType = NullableUnderlyingType(switchType);
            var unguardedLabels = statement.Sections.SelectMany(section => section.Labels).Where(label => label.Guard is null).ToArray();
            var coversNonNullArrays = unguardedLabels.Any(label =>
                _switchTypePatterns.TryGetValue(label, out var patternType) &&
                (patternType == arrayType || patternType is "System.Array" or "System.Object" or "object")) ||
                unguardedLabels.Any(IsNonNullCatchAllSwitchPattern);
            var coversNull = unguardedLabels.Any(IsNullSwitchPattern);
            if (coversNonNullArrays && (!IsNullableArrayType(switchType) || coversNull)) _exhaustiveSwitches.Add(statement);
        }
        context.SwitchDepth++;
        foreach (var section in statement.Sections)
        {
            if (section.Statements.Count == 0) diagnostics.Add("LS0620", DiagnosticSeverity.Error, "A switch section must contain at least one statement.", section.At);
            var sectionBindings = TryGetSwitchSectionBindings(section.Labels, out var bindings)
                ? bindings
                : new Dictionary<string, string>(StringComparer.Ordinal);
            if (section.Labels.Count > 1 && section.Labels.Any(label => GetSwitchPatternVariableNodes(label).Any()) &&
                section.Labels.All(label => TryGetSwitchPatternBindings(label, out _)) &&
                !TryGetSwitchSectionBindings(section.Labels, out _))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error,
                    "Pattern variables in every label of a switch section must have the same names and types.", section.At);
            context.EnterCaptureBlock(section.CaptureScope, isMethodScope: false);
            context.Push();
            foreach (var (variableName, variableType) in sectionBindings)
            {
                if (context.Exists(variableName)) diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Pattern variable '{variableName}' conflicts with an existing name in the current context.", section.At);
                else context.Declare(variableName, variableType, mutable: false, section.At, blockOwner: section.CaptureScope);
            }
            foreach (var child in section.Statements) AnalyzeStatement(child, context);
            context.Pop();
            context.ExitCaptureBlock(section.CaptureScope);
        }
        context.SwitchDepth--;
    }

    private bool IsFiniteSwitchDomainExhaustive(string switchType, HashSet<string> unconditionalLiteralKeys,
        IReadOnlyList<SwitchNumericInterval> unconditionalNumericIntervals, IEnumerable<SwitchLabel> labels, bool requireNull = true)
    {
        var nullable = IsNullableValueType(switchType);
        var valueType = nullable ? switchType[..^1] : switchType;
        var labelArray = labels.Where(label => label.Guard is null).ToArray();
        var nullIsCovered = unconditionalLiteralKeys.Contains("null") || labelArray.Any(IsNullSwitchPattern);
        if (nullable && requireNull && !nullIsCovered) return false;
        if (nullable && labelArray.Any(IsNonNullCatchAllSwitchPattern)) return true;

        if (valueType == "bool")
        {
            var coversNonNullValues = unconditionalLiteralKeys.Contains("bool:true") && unconditionalLiteralKeys.Contains("bool:false");
            return coversNonNullValues;
        }

        if (IntegerTypes.Contains(valueType) || valueType == "char")
        {
            var width = Width(valueType);
            if (width == 0) return false;
            var minimum = IsUnsigned(valueType) ? BigInteger.Zero : -(BigInteger.One << (width - 1));
            var maximum = IsUnsigned(valueType) ? (BigInteger.One << width) - BigInteger.One : (BigInteger.One << (width - 1)) - BigInteger.One;
            return IntegralSwitchIntervalUnionContains(unconditionalNumericIntervals, minimum, maximum);
        }

        if (!IsEnumType(valueType)) return false;

        var requiredValues = _constantFields.Values
            .Where(field => GetQualifiedTypeName(field.Owner) == valueType && field.Binding.TypeName == valueType)
            .Select(field => "number:" + field.Binding.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (requiredValues.Length == 0 || requiredValues.Any(value => !unconditionalLiteralKeys.Contains(value))) return false;
        return true;
    }

    private static bool IntegralSwitchIntervalUnionContains(IReadOnlyList<SwitchNumericInterval> intervals, BigInteger minimum, BigInteger maximum)
    {
        var normalized = new List<(BigInteger Lower, BigInteger Upper)>();
        foreach (var interval in intervals)
        {
            if (interval.Lower is { } lowerValue && lowerValue.Integer is null || interval.Upper is { } upperValue && upperValue.Integer is null) return false;
            var lower = interval.Lower is { } lowerBound
                ? lowerBound.Integer.GetValueOrDefault() + (interval.IncludeLower ? BigInteger.Zero : BigInteger.One)
                : minimum;
            var upper = interval.Upper is { } upperBound
                ? upperBound.Integer.GetValueOrDefault() - (interval.IncludeUpper ? BigInteger.Zero : BigInteger.One)
                : maximum;
            lower = BigInteger.Max(lower, minimum);
            upper = BigInteger.Min(upper, maximum);
            if (lower <= upper) normalized.Add((lower, upper));
        }

        var nextRequired = minimum;
        foreach (var interval in normalized.OrderBy(interval => interval.Lower).ThenBy(interval => interval.Upper))
        {
            if (interval.Upper < nextRequired) continue;
            if (interval.Lower > nextRequired) return false;
            nextRequired = interval.Upper + BigInteger.One;
            if (nextRequired > maximum) return true;
        }
        return nextRequired > maximum;
    }

    private static SwitchLabel[] GetSwitchPatternVariableNodes(SwitchLabel pattern)
    {
        var variables = new List<SwitchLabel>();
        if (pattern.PatternVariableName is not null) variables.Add(pattern);
        if (pattern.LeftPattern is not null) variables.AddRange(GetSwitchPatternVariableNodes(pattern.LeftPattern));
        if (pattern.RightPattern is not null) variables.AddRange(GetSwitchPatternVariableNodes(pattern.RightPattern));
        return variables.ToArray();
    }

    private bool TryGetSwitchSectionBindings(IReadOnlyList<SwitchLabel> labels, out Dictionary<string, string> bindings)
    {
        bindings = new Dictionary<string, string>(StringComparer.Ordinal);
        if (labels.Count == 0) return true;
        if (!TryGetSwitchPatternBindings(labels[0], out bindings)) return false;
        for (var index = 1; index < labels.Count; index++)
        {
            if (!TryGetSwitchPatternBindings(labels[index], out var alternative) || !HaveSameBindings(bindings, alternative))
                return false;
        }
        return true;
    }

    private bool TryGetSwitchPatternBindings(SwitchLabel pattern, out Dictionary<string, string> bindings)
    {
        bindings = new Dictionary<string, string>(StringComparer.Ordinal);
        if (pattern.IsNegated)
            return !GetSwitchPatternVariableNodes(pattern).Any();

        if (pattern.PatternVariableName is { } name)
        {
            if (!_switchTypePatterns.TryGetValue(pattern, out var type)) return false;
            bindings.Add(name, type);
        }

        if (pattern.CombinedOperator is { } combinedOperator)
        {
            if (pattern.LeftPattern is null || pattern.RightPattern is null ||
                !TryGetSwitchPatternBindings(pattern.LeftPattern, out var left) ||
                !TryGetSwitchPatternBindings(pattern.RightPattern, out var right)) return false;
            if (combinedOperator == "and")
            {
                foreach (var (variable, type) in right)
                {
                    if (bindings.ContainsKey(variable) || left.ContainsKey(variable)) return false;
                    bindings.Add(variable, type);
                }
                foreach (var (variable, type) in left)
                {
                    if (bindings.TryGetValue(variable, out var existingType) && existingType != type) return false;
                    bindings.TryAdd(variable, type);
                }
                return true;
            }
            if (combinedOperator == "or")
            {
                if (!HaveSameBindings(left, right)) return false;
                foreach (var (variable, type) in left)
                {
                    if (bindings.TryGetValue(variable, out var existingType) && existingType != type) return false;
                    bindings.TryAdd(variable, type);
                }
                return true;
            }
            return false;
        }

        if (pattern.LeftPattern is not null)
        {
            if (!TryGetSwitchPatternBindings(pattern.LeftPattern, out var nested)) return false;
            foreach (var (variable, type) in nested)
            {
                if (bindings.TryGetValue(variable, out var existingType) && existingType != type) return false;
                bindings.TryAdd(variable, type);
            }
        }
        return true;
    }

    private static bool HaveSameBindings(IReadOnlyDictionary<string, string> left, IReadOnlyDictionary<string, string> right) =>
        left.Count == right.Count && left.All(binding => right.TryGetValue(binding.Key, out var type) && type == binding.Value);

    private void ValidateSwitchPattern(SwitchLabel pattern, string switchType, string relationalType, MethodContext context, bool trackSubsumption, bool recordForLater,
        HashSet<string> unconditionalLiteralKeys, List<SwitchNumericInterval> unconditionalNumericIntervals, List<string> unconditionalClassTypes)
    {
        if (pattern.IsUnsupportedPattern) return;
        if (pattern.TypePatternName is { } typePatternName && TryBindSwitchClassTypePattern(pattern, typePatternName, switchType, context, trackSubsumption, recordForLater, unconditionalClassTypes))
            return;
        if (pattern.CombinedOperator is { } combinedOperator)
        {
            if (combinedOperator is not ("and" or "or") || pattern.LeftPattern is null || pattern.RightPattern is null)
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Malformed combined switch pattern.", pattern.At);
                return;
            }
            var trackAlternatives = trackSubsumption && combinedOperator == "or";
            ValidateSwitchPattern(pattern.LeftPattern, switchType, relationalType, context, trackAlternatives, recordForLater && trackAlternatives, unconditionalLiteralKeys, unconditionalNumericIntervals, unconditionalClassTypes);
            ValidateSwitchPattern(pattern.RightPattern, switchType, relationalType, context, trackAlternatives, recordForLater && trackAlternatives, unconditionalLiteralKeys, unconditionalNumericIntervals, unconditionalClassTypes);
            if (combinedOperator == "and" && trackSubsumption &&
                TryGetSwitchNumericIntervals(pattern, relationalType, out var intersections))
            {
                if (intersections.Count > 0 && intersections.All(interval => SwitchIntervalUnionContains(unconditionalNumericIntervals, interval)))
                    diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This combined numeric switch pattern is subsumed by earlier unguarded numeric patterns.", pattern.At);
                if (recordForLater) unconditionalNumericIntervals.AddRange(intersections);
            }
            return;
        }
        if (pattern.LeftPattern is not null)
        {
            ValidateSwitchPattern(pattern.LeftPattern, switchType, relationalType, context, trackSubsumption: false, recordForLater: false, unconditionalLiteralKeys, unconditionalNumericIntervals, unconditionalClassTypes);
            if (pattern.IsNegated && trackSubsumption && TryGetSwitchNumericIntervals(pattern, relationalType, out var negatedIntervals))
            {
                if (negatedIntervals.Count > 0 && negatedIntervals.All(interval => SwitchIntervalUnionContains(unconditionalNumericIntervals, interval)))
                    diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This negated numeric switch pattern is subsumed by earlier unguarded numeric patterns.", pattern.At);
                if (recordForLater) unconditionalNumericIntervals.AddRange(negatedIntervals);
            }
            return;
        }
        if (pattern.IsDiscard) return;
        if (pattern.Value is null)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "A switch pattern is missing its value.", pattern.At);
            return;
        }

        var patternType = AnalyzeExpression(pattern.Value, context);
        var isNullPattern = pattern.Value is LiteralExpression { Value: "null" };
        if (pattern.RelationalOperator is { } relationalOperator)
        {
            if (!NumericTypes.Contains(relationalType))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Relational switch patterns require a numeric governing type, not '{switchType}'.", pattern.At);
            if (!IsConstantExpression(pattern.Value, context))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A relational switch pattern requires a numeric constant.", pattern.At);
            if (!NumericTypes.Contains(patternType) || !CanConvert(patternType, relationalType, pattern.Value))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Relational pattern value of type '{patternType}' cannot convert to numeric switch type '{relationalType}'.", pattern.At);
            if (trackSubsumption && TryCreateSwitchNumericInterval(pattern.Value, relationalType, relationalOperator, out var interval))
            {
                if (SwitchIntervalUnionContains(unconditionalNumericIntervals, interval))
                    diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This relational switch pattern is subsumed by an earlier unguarded numeric pattern.", pattern.At);
                if (recordForLater) unconditionalNumericIntervals.Add(interval);
            }
            return;
        }

        if (!isNullPattern && !IsConstantExpression(pattern.Value, context))
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A switch case label must be a constant expression or a supported pattern.", pattern.At);
        if (!CanConvert(patternType, switchType, pattern.Value))
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Case value of type '{patternType}' cannot be converted to switch type '{switchType}'.", pattern.At);
        if (!trackSubsumption || pattern.IsNegated) return;
        var duplicateConstant = patternType == "string" && unconditionalClassTypes.Contains("string", StringComparer.Ordinal);
        if (duplicateConstant)
            diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This string constant is subsumed by an earlier unguarded string type pattern.", pattern.At);
        if (TryGetSwitchConstantKey(pattern.Value, out var key))
        {
            if (unconditionalLiteralKeys.Contains(key))
            {
                diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This switch case duplicates or is subsumed by an earlier unguarded constant label.", pattern.At);
                duplicateConstant = true;
            }
            if (recordForLater) unconditionalLiteralKeys.Add(key);
        }
        if (TryCreateSwitchNumericInterval(pattern.Value, relationalType, null, out var point))
        {
            if (!duplicateConstant && SwitchIntervalUnionContains(unconditionalNumericIntervals, point))
                diagnostics.Add("LS0622", DiagnosticSeverity.Error, "This constant switch case is subsumed by an earlier unguarded numeric pattern.", pattern.At);
            if (recordForLater) unconditionalNumericIntervals.Add(point);
        }
    }
    private static bool IsCatchAllSwitchPattern(SwitchLabel pattern)
    {
        if (pattern.IsDiscard) return true;
        if (pattern.IsNegated) return false;
        if (pattern.CombinedOperator == "or") return pattern.LeftPattern is not null && IsCatchAllSwitchPattern(pattern.LeftPattern) || pattern.RightPattern is not null && IsCatchAllSwitchPattern(pattern.RightPattern);
        if (pattern.CombinedOperator == "and") return pattern.LeftPattern is not null && IsCatchAllSwitchPattern(pattern.LeftPattern) && pattern.RightPattern is not null && IsCatchAllSwitchPattern(pattern.RightPattern);
        return false;
    }
    private static bool IsNullSwitchPattern(SwitchLabel pattern)
    {
        if (pattern.IsNegated) return false;
        if (pattern.Value is LiteralExpression { Value: "null" }) return true;
        return pattern.CombinedOperator == "or" &&
            (pattern.LeftPattern is not null && IsNullSwitchPattern(pattern.LeftPattern) || pattern.RightPattern is not null && IsNullSwitchPattern(pattern.RightPattern));
    }
    private static bool IsNonNullCatchAllSwitchPattern(SwitchLabel pattern)
    {
        if (pattern.IsNegated && pattern.LeftPattern is { Value: LiteralExpression { Value: "null" } }) return true;
        return pattern.CombinedOperator == "or" &&
            (pattern.LeftPattern is not null && IsNonNullCatchAllSwitchPattern(pattern.LeftPattern) || pattern.RightPattern is not null && IsNonNullCatchAllSwitchPattern(pattern.RightPattern));
    }
    private bool TryBindSwitchClassTypePattern(SwitchLabel pattern, string typePatternName, string switchType, MethodContext context,
        bool trackSubsumption, bool recordForLater, List<string> unconditionalClassTypes)
    {
        if (ValidateTypeAccessibility(typePatternName, context.Method.Owner, pattern.At)) return true;
        if (NumericTypes.Contains(typePatternName) || typePatternName == "bool")
            return BindSwitchValueTypePattern(pattern, typePatternName, switchType, trackSubsumption, recordForLater, unconditionalClassTypes);
        if (typePatternName is "string" or "System.String")
        {
            if (switchType is not ("string" or "string?") && GetClassReferenceTypeForAnalysis(switchType) is null) return false;
            _switchTypePatterns[pattern] = "string";
            if (trackSubsumption)
            {
                if (unconditionalClassTypes.Contains("string", StringComparer.Ordinal))
                    diagnostics.Add("LS0622", DiagnosticSeverity.Error, "String type pattern is subsumed by an earlier unguarded string type pattern.", pattern.At);
                if (recordForLater) unconditionalClassTypes.Add("string");
            }
            return true;
        }
        if (TryParseManagedArrayType(typePatternName, out _, out var arrayRank))
        {
            if (arrayRank > 3)
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Array type patterns of rank {arrayRank} are not supported; the backend supports ranks one through three.", pattern.At);
                return true;
            }
            var targetArrayType = typePatternName.EndsWith("?", StringComparison.Ordinal)
                ? typePatternName[..^1] : typePatternName;
            var governingArrayType = NullableUnderlyingType(switchType);
            var governingIsArray = TryParseManagedArrayType(governingArrayType, out _, out _);
            var governingIsObject = GetClassReferenceTypeForAnalysis(switchType) is "System.Object" or "object";
            if (!governingIsArray && !governingIsObject)
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error,
                    $"Array type pattern '{typePatternName}' requires an array or object-reference switch value, not '{switchType}'.", pattern.At);
                return true;
            }
            _switchTypePatterns[pattern] = targetArrayType;
            if (governingIsArray && governingArrayType != targetArrayType)
                diagnostics.Add("LS0622", DiagnosticSeverity.Error,
                    $"Array type pattern '{typePatternName}' cannot match governing array type '{switchType}'.", pattern.At);
            if (trackSubsumption)
            {
                if (unconditionalClassTypes.Contains(targetArrayType, StringComparer.Ordinal))
                    diagnostics.Add("LS0622", DiagnosticSeverity.Error, $"Array type pattern '{typePatternName}' is subsumed by an earlier unguarded array type pattern.", pattern.At);
                if (recordForLater) unconditionalClassTypes.Add(targetArrayType);
            }
            return true;
        }
        var matches = FindTypes(typePatternName, context.Method.Owner);
        if (matches.Length == 0)
        {
            if (pattern.PatternVariableName is not null)
            {
                if (IsSupportedType(typePatternName) || TryParseManagedArrayType(typePatternName, out _, out _) || TryParseInlineArrayType(typePatternName, out _, out _))
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Type patterns for '{typePatternName}' require boxing or runtime descriptors that are not implemented.", pattern.At);
                else
                    diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Pattern type '{typePatternName}' does not exist in the current compilation.", pattern.At);
                return true;
            }
            if (IsSupportedType(typePatternName) || TryParseManagedArrayType(typePatternName, out _, out _) || TryParseInlineArrayType(typePatternName, out _, out _))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Type patterns for '{typePatternName}' require boxing or runtime descriptors that are not implemented.", pattern.At);
                return true;
            }
            return false;
        }
        if (matches.Length > 1)
        {
            diagnostics.Add("LS2001", DiagnosticSeverity.Error, $"Pattern type name '{typePatternName}' is ambiguous.", pattern.At);
            return true;
        }
        var resolvedType = matches[0];
        if (resolvedType.Kind == "enum")
            return BindSwitchValueTypePattern(pattern, GetQualifiedTypeName(resolvedType), switchType, trackSubsumption, recordForLater, unconditionalClassTypes);
        if (resolvedType.Kind == "struct")
        {
            var valueType = GetQualifiedTypeName(resolvedType);
            return BindSwitchValueTypePattern(pattern, valueType, switchType, trackSubsumption, recordForLater, unconditionalClassTypes);
        }
        if (resolvedType.Kind is not ("class" or "interface" or "delegate"))
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Switch type patterns for {resolvedType.Kind} '{typePatternName}' require boxing or runtime type descriptors.", pattern.At);
            return true;
        }
        var governingReference = IsClassReferenceType(switchType) || IsDelegateType(switchType) || IsNullableDelegateType(switchType) || IsStringType(switchType) || IsManagedArrayReferenceType(switchType)
            ? NullableUnderlyingType(switchType)
            : null;
        if (governingReference is null || !(IsClassType(governingReference) || IsInterfaceType(governingReference) || IsDelegateType(governingReference) || IsManagedArrayReferenceType(governingReference) || governingReference == "string"))
        {
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Class type pattern '{typePatternName}' requires a class-reference, interface-reference, or delegate-reference switch value, not '{switchType}'.", pattern.At);
            return true;
        }

        var targetType = GetQualifiedTypeName(resolvedType);
        _switchTypePatterns[pattern] = targetType;
        var isClassInterfacePair = IsClassType(governingReference) && IsInterfaceType(targetType) ||
            IsInterfaceType(governingReference) && IsClassType(targetType);
        var targetIsArrayBase = targetType is "System.Array" or "System.Object" or "object";
        var governingIsArrayBase = governingReference is "System.Array" or "System.Object" or "object";
        var isArrayBasePatternPair =
            (IsManagedArrayReferenceType(governingReference) && targetIsArrayBase) ||
            (IsManagedArrayReferenceType(targetType) && governingIsArrayBase) ||
            (governingIsArrayBase && targetIsArrayBase);
        var mayShareRuntimeValue = IsReferenceAssignable(governingReference, targetType) || IsReferenceAssignable(targetType, governingReference) ||
            isArrayBasePatternPair || (isClassInterfacePair && (IsClassType(governingReference)
                ? _classTypes.TryGetValue(governingReference, out var governingClass) && governingClass.Declaration.IsVirtual
                : _classTypes.TryGetValue(targetType, out var patternClass) && patternClass.Declaration.IsVirtual));
        if (governingReference != targetType && !mayShareRuntimeValue)
            diagnostics.Add("LS0622", DiagnosticSeverity.Error, $"Class type pattern '{typePatternName}' cannot match values of governing type '{switchType}'.", pattern.At);
        if (trackSubsumption)
        {
            if (unconditionalClassTypes.Any(previous => previous == targetType || IsReferenceAssignable(previous, targetType)))
                diagnostics.Add("LS0622", DiagnosticSeverity.Error, $"Class type pattern '{typePatternName}' is subsumed by an earlier unguarded type pattern.", pattern.At);
            if (recordForLater) unconditionalClassTypes.Add(targetType);
        }
        return true;
    }
    private bool BindSwitchValueTypePattern(SwitchLabel pattern, string targetType, string switchType,
        bool trackSubsumption, bool recordForLater, List<string> unconditionalClassTypes)
    {
        var governingValueType = NullableUnderlyingType(switchType);
        var governingIsValueType = IsBoxableValueType(governingValueType);
        var governingIsObjectReference = GetClassReferenceTypeForAnalysis(switchType) is "System.Object" or "object";
        if (!governingIsValueType && !governingIsObjectReference)
        {
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error,
                $"Value type pattern '{targetType}' requires the same value type or an object-reference switch value, not '{switchType}'.", pattern.At);
            return true;
        }
        if (governingIsValueType && governingValueType != targetType)
            diagnostics.Add("LS0622", DiagnosticSeverity.Error,
                $"Value type pattern '{targetType}' cannot match governing value type '{switchType}'.", pattern.At);
        _switchTypePatterns[pattern] = targetType;
        if (trackSubsumption && unconditionalClassTypes.Contains(targetType, StringComparer.Ordinal))
            diagnostics.Add("LS0622", DiagnosticSeverity.Error,
                $"Value type pattern '{targetType}' is subsumed by an earlier unguarded type pattern.", pattern.At);
        if (trackSubsumption && recordForLater) unconditionalClassTypes.Add(targetType);
        return true;
    }
    private bool TryCreateSwitchNumericInterval(Expression expression, string numericType, string? relationalOperator, out SwitchNumericInterval interval)
    {
        interval = null!;
        if (!NumericTypes.Contains(numericType) || !TryEvaluateConstant(expression, out var constant, out _) ||
            !TryConvertConstant(constant, numericType, expression, out var convertedText)) return false;
        ConstantValue converted;
        if (FloatingTypes.Contains(numericType))
        {
            if (!double.TryParse(convertedText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number)) return false;
            converted = new(numericType, Floating: number);
        }
        else
        {
            if (!BigInteger.TryParse(convertedText, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var integer)) return false;
            converted = new(numericType, Integer: integer);
        }
        interval = relationalOperator switch
        {
            ">" => new(converted, false, null, false),
            ">=" => new(converted, true, null, false),
            "<" => new(null, false, converted, false),
            "<=" => new(null, false, converted, true),
            _ => new(converted, true, converted, true)
        };
        return true;
    }
    private bool TryGetSwitchNumericIntervals(SwitchLabel pattern, string numericType, out IReadOnlyList<SwitchNumericInterval> intervals)
    {
        intervals = [];
        if (pattern.IsDiscard) return false;
        if (pattern.IsNegated && pattern.LeftPattern is { } negatedPattern &&
            TryGetSwitchNumericIntervals(negatedPattern, numericType, out var negatedIntervals) && negatedIntervals.Count == 1)
        {
            var inner = negatedIntervals[0];
            var complement = new List<SwitchNumericInterval>();
            if (inner.Lower is not null)
                complement.Add(new SwitchNumericInterval(null, false, inner.Lower, !inner.IncludeLower));
            if (inner.Upper is not null)
                complement.Add(new SwitchNumericInterval(inner.Upper, !inner.IncludeUpper, null, false));
            intervals = complement;
            return true;
        }
        if (pattern.CombinedOperator == "or" && pattern.LeftPattern is { } leftOr && pattern.RightPattern is { } rightOr &&
            TryGetSwitchNumericIntervals(leftOr, numericType, out var leftUnion) &&
            TryGetSwitchNumericIntervals(rightOr, numericType, out var rightUnion))
        {
            intervals = leftUnion.Concat(rightUnion).ToArray();
            return true;
        }
        if (pattern.CombinedOperator == "and" && pattern.LeftPattern is { } leftAnd && pattern.RightPattern is { } rightAnd &&
            TryGetSwitchNumericIntervals(leftAnd, numericType, out var leftIntervals) &&
            TryGetSwitchNumericIntervals(rightAnd, numericType, out var rightIntervals))
        {
            var intersections = new List<SwitchNumericInterval>();
            foreach (var leftInterval in leftIntervals)
            foreach (var rightInterval in rightIntervals)
                if (TryIntersectSwitchIntervals(leftInterval, rightInterval, out var intersection)) intersections.Add(intersection);
            intervals = intersections;
            return true;
        }
        if (pattern.RelationalOperator is { } relationalOperator && pattern.Value is { } relationalValue &&
            TryCreateSwitchNumericInterval(relationalValue, numericType, relationalOperator, out var relationalInterval))
        {
            intervals = [relationalInterval];
            return true;
        }
        if (pattern.Value is { } value && value is not LiteralExpression { Value: "null" } &&
            TryCreateSwitchNumericInterval(value, numericType, null, out var point))
        {
            intervals = [point];
            return true;
        }
        return false;
    }
    private static bool TryIntersectSwitchIntervals(SwitchNumericInterval first, SwitchNumericInterval second, out SwitchNumericInterval intersection)
    {
        var lower = first.Lower;
        var includeLower = first.IncludeLower;
        if (second.Lower is { } secondLower)
        {
            if (lower is null || CompareSwitchNumbers(secondLower, lower) > 0)
            {
                lower = secondLower;
                includeLower = second.IncludeLower;
            }
            else if (CompareSwitchNumbers(secondLower, lower) == 0)
                includeLower &= second.IncludeLower;
        }

        var upper = first.Upper;
        var includeUpper = first.IncludeUpper;
        if (second.Upper is { } secondUpper)
        {
            if (upper is null || CompareSwitchNumbers(secondUpper, upper) < 0)
            {
                upper = secondUpper;
                includeUpper = second.IncludeUpper;
            }
            else if (CompareSwitchNumbers(secondUpper, upper) == 0)
                includeUpper &= second.IncludeUpper;
        }

        intersection = new(lower, includeLower, upper, includeUpper);
        if (lower is null || upper is null) return true;
        var comparison = CompareSwitchNumbers(lower, upper);
        return comparison < 0 || comparison == 0 && includeLower && includeUpper;
    }
    private static bool SwitchIntervalUnionContains(IReadOnlyList<SwitchNumericInterval> intervals, SwitchNumericInterval target)
    {
        var ordered = intervals.OrderBy(interval => interval.Lower is null ? 0 : 1)
            .ThenBy(interval => interval.Lower, SwitchNumericBoundComparer.Instance)
            .ThenByDescending(interval => interval.IncludeLower)
            .ToArray();
        SwitchNumericInterval? coverage = null;
        foreach (var interval in ordered)
        {
            if (coverage is null)
            {
                if (!ContainsIntervalStart(interval, target)) continue;
                coverage = interval;
            }
            else
            {
                if (coverage.Upper is null) return true;
                if (interval.Lower is null) continue;
                var gap = CompareSwitchNumbers(interval.Lower, coverage.Upper);
                if (gap > 0 || gap == 0 && !coverage.IncludeUpper && !interval.IncludeLower) return false;
                if (interval.Upper is null)
                    coverage = coverage with { Upper = null, IncludeUpper = false };
                else if (CompareSwitchNumbers(interval.Upper, coverage.Upper) > 0)
                    coverage = coverage with { Upper = interval.Upper, IncludeUpper = interval.IncludeUpper };
                else if (CompareSwitchNumbers(interval.Upper, coverage.Upper) == 0)
                    coverage = coverage with { IncludeUpper = coverage.IncludeUpper || interval.IncludeUpper };
            }
            if (ContainsUpperBound(coverage, target)) return true;
        }
        return false;
    }
    private static bool ContainsIntervalStart(SwitchNumericInterval outer, SwitchNumericInterval inner)
    {
        if (!ContainsLowerBound(outer, inner)) return false;
        if (inner.Lower is null) return outer.Lower is null;
        if (outer.Upper is null) return true;
        var comparison = CompareSwitchNumbers(outer.Upper, inner.Lower);
        return comparison > 0 || comparison == 0 && (!inner.IncludeLower || outer.IncludeUpper);
    }
    private sealed class SwitchNumericBoundComparer : IComparer<ConstantValue?>
    {
        public static SwitchNumericBoundComparer Instance { get; } = new();
        public int Compare(ConstantValue? first, ConstantValue? second)
        {
            if (first is null) return second is null ? 0 : -1;
            if (second is null) return 1;
            return CompareSwitchNumbers(first, second);
        }
    }
    private static bool SwitchIntervalContains(SwitchNumericInterval outer, SwitchNumericInterval inner) =>
        ContainsLowerBound(outer, inner) && ContainsUpperBound(outer, inner);
    private static bool ContainsLowerBound(SwitchNumericInterval outer, SwitchNumericInterval inner)
    {
        if (outer.Lower is null) return true;
        if (inner.Lower is null) return false;
        var comparison = CompareSwitchNumbers(outer.Lower, inner.Lower);
        return comparison < 0 || comparison == 0 && (!inner.IncludeLower || outer.IncludeLower);
    }
    private static bool ContainsUpperBound(SwitchNumericInterval outer, SwitchNumericInterval inner)
    {
        if (outer.Upper is null) return true;
        if (inner.Upper is null) return false;
        var comparison = CompareSwitchNumbers(outer.Upper, inner.Upper);
        return comparison > 0 || comparison == 0 && (!inner.IncludeUpper || outer.IncludeUpper);
    }
    private static int CompareSwitchNumbers(ConstantValue first, ConstantValue second) =>
        first.Integer is { } firstInteger && second.Integer is { } secondInteger
            ? firstInteger.CompareTo(secondInteger)
            : (first.Floating ?? (double)first.Integer!.Value).CompareTo(second.Floating ?? (double)second.Integer!.Value);
    private bool TryGetSwitchConstantKey(Expression expression, out string key)
    {
        switch (expression)
        {
            case LiteralExpression { Value: "null" }: key = "null"; return true;
            case MemberAccessExpression access when _fieldAccesses.TryGetValue(access, out var binding) && binding.TypeName == "string" && binding.StringValue is { } text: key = "string:" + text; return true;
            case LiteralExpression literal when (literal.Value.StartsWith('"') || literal.Value.StartsWith('@')) && StringLiteralCodec.TryParse(literal.Value, out var stringValue, out _): key = "string:" + stringValue.Text; return true;
            case LiteralExpression { Value: "true" or "false" } literal: key = "bool:" + literal.Value; return true;
            case LiteralExpression literal when literal.Value.StartsWith('\'') && TryParseCharacter(literal.Value, out var character): key = "number:" + character; return true;
            case LiteralExpression literal when !IsFloatingLiteral(literal.Value) && ParseInteger(literal.Value).Value is { } integer: key = "number:" + integer; return true;
            case MemberAccessExpression access when _fieldAccesses.TryGetValue(access, out var binding) && IsEnumType(binding.TypeName) && BigInteger.TryParse(binding.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var enumValue):
                key = "number:" + enumValue.ToString(System.Globalization.CultureInfo.InvariantCulture); return true;
            case NameExpression name when _boundUnqualifiedConstantFields.TryGetValue(name, out var unqualifiedBinding) && IsEnumType(unqualifiedBinding.TypeName) && BigInteger.TryParse(unqualifiedBinding.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var unqualifiedEnumValue):
                key = "number:" + unqualifiedEnumValue.ToString(System.Globalization.CultureInfo.InvariantCulture); return true;
            default: key = ""; return false;
        }
    }
    private void EnterLoop(MethodContext context, string? label, SourceLocation at)
    {
        if (label is not null && context.HasLoopLabel(label)) diagnostics.Add("LS0604", DiagnosticSeverity.Error, $"Loop label '{label}' is already used by an enclosing loop.", at);
        context.EnterLoop(label);
    }
    private void AnalyzeMultiAssignment(MultiAssignmentStatement assignment, MethodContext context)
    {
        if (assignment.Targets.Count != assignment.Values.Count)
            diagnostics.Add("LS0210", DiagnosticSeverity.Error, $"Multiple assignment has {assignment.Targets.Count} targets but {assignment.Values.Count} values.", assignment.Location);

        // Bind target types before visiting the values so target-typed lambdas and default expressions
        // receive the same context as an ordinary assignment. This is a semantic-only pass; lowering
        // still emits all right-hand sides before evaluating any target sub-expressions. Analyze on
        // a flow snapshot so target-side assertions and calls cannot make an earlier RHS appear safe.
        var targetContext = context.CloneForInference();
        var targetTypes = new string[assignment.Targets.Count];
        var targetBindingDiagnosticStart = diagnostics.Items.Count;
        for (var index = 0; index < assignment.Targets.Count; index++)
        {
            var target = assignment.Targets[index];
            if (target is NameExpression localName && targetContext.Lookup(localName.Name) is { } local)
                targetTypes[index] = local.Type;
            else
            {
                var previousPropertyAssignmentProbe = _propertyAssignmentProbe;
                _propertyAssignmentProbe = target;
                try { targetTypes[index] = AnalyzeExpression(target, targetContext); }
                finally { _propertyAssignmentProbe = previousPropertyAssignmentProbe; }
            }
        }
        diagnostics.DiscardSince(targetBindingDiagnosticStart);
        var valueTypes = assignment.Values.Select((value, index) =>
        {
            var targetType = index < targetTypes.Length && targetTypes[index] != "?" ? targetTypes[index] : null;
            return AnalyzeExpression(value, context, targetType);
        }).ToArray();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < assignment.Targets.Count; index++)
        {
            var target = assignment.Targets[index];
            MethodContext.Local? local = null;
            var previousPropertyAssignmentProbe = _propertyAssignmentProbe;
            string targetType;
            _propertyAssignmentProbe = target;
            try { targetType = AnalyzeExpression(target, context); }
            finally { _propertyAssignmentProbe = previousPropertyAssignmentProbe; }
            var isManagedArrayElement = false;
            var isFieldTarget = false;
            if (target is NameExpression name)
            {
                if (!seen.Add(name.Name)) diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Variable '{name.Name}' appears more than once as a multiple-assignment target.", target.Location);
                local = context.Lookup(name.Name);
                if (local is null)
                {
                    if (_boundUnqualifiedStaticFields.TryGetValue(name, out var staticField))
                    {
                        isFieldTarget = true;
                        if (!staticField.Mutable && !(context.Method.IsTypeInitializer && staticField.OwnerType == GetQualifiedTypeName(context.Method.Owner)))
                            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A multiple-assignment static-field target must be mutable.", name.Location);
                    }
                    else if (_boundUnqualifiedStructFields.TryGetValue(name, out var structField))
                    {
                        isFieldTarget = true;
                        var mayInitializeReadOnly = CanInitializeReadOnlyStructField(structField, new ThisExpression(name.Location), context);
                        if (structField.IsReadOnly && !mayInitializeReadOnly || !IsMutableStructLocation(new ThisExpression(name.Location), context))
                            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A multiple-assignment struct-field target must be mutable.", name.Location);
                    }
                    else if (_boundUnqualifiedClassFields.TryGetValue(name, out var classField))
                    {
                        isFieldTarget = true;
                        if (classField.IsReadOnly && !CanInitializeReadOnlyClassField(classField, new ThisExpression(name.Location), context) ||
                            !IsMutableClassFieldLocation(new ThisExpression(name.Location), context))
                            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A multiple-assignment class-field target must be mutable.", name.Location);
                    }
                    else if (_boundUnqualifiedComputedProperties.TryGetValue(name, out var property) && index < assignment.Values.Count)
                    {
                        var syntheticAssignment = new BinaryExpression(name, "=", assignment.Values[index], assignment.Location);
                        BindComputedPropertyWrite(syntheticAssignment, null, property, context);
                        if (!_multiAssignmentAccessorAssignments.TryGetValue(assignment, out var accessorAssignments))
                            _multiAssignmentAccessorAssignments[assignment] = accessorAssignments = [];
                        accessorAssignments[index] = syntheticAssignment;
                    }
                    else
                    {
                        diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"The name '{name.Name}' does not exist in the current context.", name.Location);
                        continue;
                    }
                }
                else if (!local.Mutable) diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Cannot assign to immutable local '{name.Name}'.", name.Location);
            }
            else if (target is IndexExpression indexTarget)
            {
                isManagedArrayElement = _managedArrayIndexAccesses.ContainsKey(indexTarget);
                if (isManagedArrayElement && (indexTarget.IsNullConditional || !IsMutableIndexTarget(indexTarget, context)))
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "Multiple-assignment index targets must be mutable managed-array elements.", target.Location);
                else if (_boundComputedIndexers.TryGetValue(indexTarget, out var indexer))
                {
                    if (index < assignment.Values.Count)
                    {
                        var syntheticAssignment = new BinaryExpression(indexTarget, "=", assignment.Values[index], assignment.Location);
                        BindComputedIndexerWrite(syntheticAssignment, indexTarget, indexer, context);
                        if (!_multiAssignmentAccessorAssignments.TryGetValue(assignment, out var accessorAssignments))
                            _multiAssignmentAccessorAssignments[assignment] = accessorAssignments = [];
                        accessorAssignments[index] = syntheticAssignment;
                    }
                }
                else if (!isManagedArrayElement && (indexTarget.IsNullConditional || targetType == "?" || !IsMutableIndexTarget(indexTarget, context)))
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "Multiple-assignment index targets must be mutable array or pointer elements, or indexers with a set accessor.", target.Location);
            }
            else if (target is MemberAccessExpression memberTarget)
            {
                if (_boundStaticFields.TryGetValue(memberTarget, out var staticField))
                {
                    isFieldTarget = true;
                    var mayInitializeReadOnly = context.Method.IsTypeInitializer && staticField.OwnerType == GetQualifiedTypeName(context.Method.Owner);
                    if (!staticField.Mutable && !mayInitializeReadOnly)
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A multiple-assignment static-field target must be mutable.", target.Location);
                }
                else if (_boundStructFields.TryGetValue(memberTarget, out var structField))
                {
                    isFieldTarget = true;
                    var mayInitializeReadOnly = CanInitializeReadOnlyStructField(structField, memberTarget.Receiver, context);
                    if (structField.IsReadOnly && !mayInitializeReadOnly || !IsMutableStructLocation(memberTarget.Receiver, context))
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A multiple-assignment struct-field target must be mutable.", target.Location);
                }
                else if (_boundClassFields.TryGetValue(memberTarget, out var classField))
                {
                    isFieldTarget = true;
                    if (classField.IsReadOnly && !CanInitializeReadOnlyClassField(classField, memberTarget.Receiver, context) ||
                        !IsMutableClassFieldLocation(memberTarget.Receiver, context))
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A multiple-assignment class-field target must be mutable.", target.Location);
                }
                else if (_boundComputedProperties.TryGetValue(memberTarget, out var property) && index < assignment.Values.Count)
                {
                    var syntheticAssignment = new BinaryExpression(memberTarget, "=", assignment.Values[index], assignment.Location);
                    BindComputedPropertyWrite(syntheticAssignment, memberTarget.Receiver, property, context,
                        memberTarget.IsNullConditional, memberTarget.IsUnsafeContext);
                    if (!_multiAssignmentAccessorAssignments.TryGetValue(assignment, out var accessorAssignments))
                        _multiAssignmentAccessorAssignments[assignment] = accessorAssignments = [];
                    accessorAssignments[index] = syntheticAssignment;
                }
                else
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                        "Multiple assignment requires a mutable field or a property with an accessible set accessor.", target.Location);
                    continue;
                }
            }
            else if (target is UnaryExpression { Operator: "*" } pointerTarget)
            {
                if (IsClassReferenceType(targetType))
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A class-object pointer dereference is not an assignable multiple-assignment target.", target.Location);
            }
            else
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Multiple-assignment targets must be mutable fields, locals, managed-array elements, or unsafe pointer dereferences.", target.Location);
                continue;
            }

            if (index < valueTypes.Length && !CanConvert(valueTypes[index], targetType, assignment.Values[index]))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert '{valueTypes[index]}' to '{targetType}'.", assignment.Values[index].Location);

            if (index >= assignment.Values.Count) continue;
            var value = assignment.Values[index];
            var assignedScopeDepth = GetScopeBoundDepth(value, context);
            var assignedBorrowedDepth = GetScopeBoundOrBorrowedDepth(value, context);
            var assignedReadOnlyReference = IsReadOnlyReferenceExpression(value, context);
            if (local is null)
            {
                var mayStoreScopedArrayElement = target is IndexExpression arrayTarget && isManagedArrayElement &&
                    CanStoreScopeBoundArrayElement(arrayTarget.Receiver, value, context) && IsScopeBoundReferenceType(targetType);
                var mayStoreScopedField = target is MemberAccessExpression scopedFieldTarget && isFieldTarget &&
                    IsScopeBoundReferenceType(targetType) &&
                    GetScopeBoundDepth(scopedFieldTarget.Receiver, context) is { } ownerScopeDepth &&
                    GetScopeBoundOrder(scopedFieldTarget.Receiver, context) is { } ownerScopeOrder &&
                    assignedScopeDepth is { } childScopeDepth &&
                    GetScopeBoundOrder(value, context) is { } childScopeOrder &&
                    ownerScopeDepth >= childScopeDepth &&
                    (ownerScopeDepth > childScopeDepth || ownerScopeOrder > childScopeOrder);
                if (mayStoreScopedField && target is MemberAccessExpression boundFieldTarget &&
                    _boundClassFields.TryGetValue(boundFieldTarget, out var scopedFieldBinding))
                    _scopeBoundClassFields.Add((scopedFieldBinding.DeclaredOwnerType ?? scopedFieldBinding.OwnerType, boundFieldTarget.Member));
                if (assignedScopeDepth is not null && !mayStoreScopedArrayElement && !mayStoreScopedField || assignedReadOnlyReference)
                    diagnostics.Add("LS0701", DiagnosticSeverity.Error,
                        "A scope-bound or readonly reference cannot be moved into a multiple-assignment field or array target whose lifetime is not proven safe.", value.Location);
                continue;
            }

            var scopedAssignments = _scopeBoundMultiAssignmentTargets.GetValueOrDefault(assignment);
            var readOnlyAssignments = _readOnlyMultiAssignmentTargets.GetValueOrDefault(assignment);
            var isFirstScopedLocalAssignment = local.ScopeBoundDepth is null && local.Declaration is { } localDeclaration &&
                (localDeclaration.Initializer is null || localDeclaration.Initializer is LiteralExpression { Value: "null" }) &&
                assignedScopeDepth is { } localValueDepth && local.ScopeDepth >= localValueDepth &&
                IsScopeBoundReferenceType(local.Type);
            var mayAssignScopedLocal = isFirstScopedLocalAssignment || scopedAssignments?.Contains(index) == true;
            if (isFirstScopedLocalAssignment && assignedScopeDepth is { } assignedLocalDepth && local.Declaration is { } assignedLocalDeclaration)
            {
                if (scopedAssignments is null)
                    _scopeBoundMultiAssignmentTargets[assignment] = scopedAssignments = [];
                scopedAssignments.Add(index);
                _scopeBoundVariables.Add(assignedLocalDeclaration);
                context.SetScopeBound(assignedLocalDeclaration.Name, assignedLocalDepth, GetScopeBoundOrder(value, context));
            }
            var isFirstReadOnlyLocalAssignment = !local.IsReadOnlyReference && local.ScopeBoundDepth is null && local.Declaration is { } readOnlyDeclaration &&
                (readOnlyDeclaration.Initializer is null || readOnlyDeclaration.Initializer is LiteralExpression { Value: "null" }) &&
                assignedReadOnlyReference && assignedBorrowedDepth is { } readOnlyValueDepth &&
                local.ScopeDepth >= readOnlyValueDepth && IsManagedReferenceValue(local.Type);
            var mayAssignReadOnlyLocal = isFirstReadOnlyLocalAssignment || readOnlyAssignments?.Contains(index) == true;
            if (isFirstReadOnlyLocalAssignment && local.Declaration is { } readOnlyLocalDeclaration)
            {
                if (readOnlyAssignments is null)
                    _readOnlyMultiAssignmentTargets[assignment] = readOnlyAssignments = [];
                readOnlyAssignments.Add(index);
                _scopeBoundVariables.Add(readOnlyLocalDeclaration);
                context.SetReadOnlyReference(readOnlyLocalDeclaration.Name);
            }
            var invalidScopedLocalReplacement = local.ScopeBoundDepth is not null && assignedScopeDepth is null &&
                value is not LiteralExpression { Value: "null" };
            var invalidReadOnlyLocalReplacement = local.IsReadOnlyReference && !assignedReadOnlyReference &&
                value is not LiteralExpression { Value: "null" };
            if ((assignedScopeDepth is not null && !mayAssignScopedLocal || assignedReadOnlyReference && !mayAssignReadOnlyLocal ||
                 invalidScopedLocalReplacement || invalidReadOnlyLocalReplacement) &&
                (IsScopeBoundReferenceType(local.Type) || IsManagedReferenceValue(local.Type)))
                diagnostics.Add("LS0701", DiagnosticSeverity.Error,
                    "A scope-bound or readonly reference can only be assigned through multiple assignment to a local alias with the same or a shorter lexical lifetime.", value.Location);
        }
    }
    private int[] AnalyzeManagedArrayInitializer(InlineArrayInitializerExpression initializer, string elementType, int rank, MethodContext context,
        bool allowScopeBoundElements = false)
    {
        if (rank <= 1)
        {
            foreach (var element in initializer.Elements)
            {
                if (TryParseManagedArrayType(elementType, out var nestedElementType, out var nestedRank) && element is InlineArrayInitializerExpression nestedInitializer)
                {
                    _ = AnalyzeManagedArrayInitializer(nestedInitializer, nestedElementType, nestedRank, context, allowScopeBoundElements);
                    continue;
                }
                if (element is InlineArrayInitializerExpression)
                {
                    if (!TryParseManagedArrayType(elementType, out _, out _))
                    {
                        diagnostics.Add("LS1504", DiagnosticSeverity.Error, "A one-dimensional value array initializer cannot contain nested initializer lists.", element.Location);
                        continue;
                    }
                }
                var actualType = AnalyzeExpression(element, context, elementType);
                var scopeBoundDepth = GetScopeBoundDepth(element, context);
                var canStoreScopeBoundElement = allowScopeBoundElements && scopeBoundDepth is { } depth &&
                    context.ScopeDepth >= depth && IsScopeBoundReferenceType(elementType);
                if (scopeBoundDepth is not null && !canStoreScopeBoundElement || IsReadOnlyReferenceExpression(element, context))
                    diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A scope-bound or readonly reference cannot be stored in an array initializer.", element.Location);
                if (!CanConvert(actualType, elementType, element))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert array element from '{actualType}' to '{elementType}'.", element.Location);
            }
            return [initializer.Elements.Count];
        }

        var nestedShapes = new List<int[]>();
        foreach (var element in initializer.Elements)
        {
            if (element is InlineArrayInitializerExpression nested)
                nestedShapes.Add(AnalyzeManagedArrayInitializer(nested, elementType, rank - 1, context, allowScopeBoundElements));
            else
            {
                diagnostics.Add("LS1504", DiagnosticSeverity.Error, $"A rank-{rank} array initializer requires a nested initializer list at this level.", element.Location);
                _ = AnalyzeExpression(element, context);
            }
        }
        if (nestedShapes.Count > 1 && nestedShapes.Skip(1).Any(shape => !shape.SequenceEqual(nestedShapes[0])))
            diagnostics.Add("LS1504", DiagnosticSeverity.Error, "Rectangular array initializer sub-lists must have identical dimensions.", initializer.Location);
        var childShape = nestedShapes.Count == 0 ? Enumerable.Repeat(0, rank - 1).ToArray() : nestedShapes[0];
        return [initializer.Elements.Count, .. childShape];
    }
    private void RequireBool(Expression expression, MethodContext context)
    {
        var type = AnalyzeExpression(expression, context); if (type is not ("bool" or "?")) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Condition must have type bool, not '{type}'.", expression.Location);
    }

    private MethodSymbol? SelectConstructor(CallExpression call, MethodSymbol[] candidates, MethodContext context, string typeName,
        string noMatchCode = "LS0751", AllocationExpression? scopedOwner = null)
    {
        var argumentTypes = call.Arguments.Select(argument => AnalyzeExpression(argument, context)).ToArray();
        var applicable = new List<(MethodSymbol Method, BoundCallArguments Binding)>();
        foreach (var candidate in candidates)
        {
            if (!TryBindCallArguments(call, candidate, argumentTypes, out var binding)) continue;
            var valid = true;
            for (var index = 0; index < argumentTypes.Length; index++)
            {
                var parameter = candidate.Declaration.Parameters[binding.ParameterBySource[index]];
                var parameterType = GetParameterTypeForSource(candidate, binding, index);
                var mode = call.ArgumentModes is not null && index < call.ArgumentModes.Count ? call.ArgumentModes[index] ?? "" : "";
                if (parameter.PassingMode is "ref" or "out")
                {
                    if (mode != parameter.PassingMode || argumentTypes[index] != parameterType || !IsMutableVariableArgument(call.Arguments[index], context)) { valid = false; break; }
                    continue;
                }
                if (mode is "ref" or "out" || parameter.PassingMode == "in" && mode is not ("" or "in") || parameter.PassingMode != "in" && mode == "in" ||
                    argumentTypes[index] != "?" && !CanConvert(argumentTypes[index], parameterType, call.Arguments[index]))
                { valid = false; break; }
            }
            if (valid) applicable.Add((candidate, binding));
        }
        if (applicable.Count == 0)
        {
            if (noMatchCode == "LS0811")
                diagnostics.Add("LS0811", DiagnosticSeverity.Error, $"Class '{typeName}' has no accessible constructor matching the base initializer.", call.Location);
            else diagnostics.Add("LS0751", DiagnosticSeverity.Error, $"No applicable constructor overload for '{typeName}'.", call.Location);
            return null;
        }
        var best = applicable.Where(candidate => applicable.All(other => ReferenceEquals(candidate.Method, other.Method) ||
            CompareCandidates(candidate.Method, other.Method, argumentTypes, call.Arguments, candidate.Binding, other.Binding) <= 0)).ToArray();
        if (best.Length > 1)
        {
            var minDefaults = best.Min(candidate => candidate.Binding.DefaultsByParameter.Count(value => value is not null));
            best = best.Where(candidate => candidate.Binding.DefaultsByParameter.Count(value => value is not null) == minDefaults).ToArray();
        }
        if (best.Length > 1)
        {
            var minVariadicPenalty = best.Min(candidate => VariadicBindingPenalty(candidate.Binding));
            best = best.Where(candidate => VariadicBindingPenalty(candidate.Binding) == minVariadicPenalty).ToArray();
        }
        if (best.Length != 1)
        {
            diagnostics.Add("LS0750", DiagnosticSeverity.Error, $"Call to constructor '{typeName}' is ambiguous.", call.Location);
            return null;
        }
        var selected = best[0];
        _callTargets[call] = selected.Method;
        _callArguments[call] = selected.Binding;
        var scopedOwnerDepth = scopedOwner is null ? null : GetScopeBoundDepth(scopedOwner, context);
        var scopedOwnerOrder = scopedOwner is null ? null : GetScopeBoundOrder(scopedOwner, context);
        for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
        {
            var parameterIndex = selected.Binding.ParameterBySource[sourceIndex];
            var parameter = selected.Method.Declaration.Parameters[parameterIndex];
            var argument = call.Arguments[sourceIndex];
            var argumentScopeDepth = GetScopeBoundDepth(argument, context);
            var mayStoreScopedConstructorArgument = false;
            if (scopedOwner is { IsScope: true } && argumentScopeDepth is { } childDepth &&
                GetScopeBoundOrder(argument, context) is { } childOrder &&
                scopedOwnerDepth is { } ownerDepth && scopedOwnerOrder is { } ownerOrder &&
                ownerDepth >= childDepth && (ownerDepth > childDepth || ownerOrder > childOrder) &&
                !parameter.IsScoped && !IsReadOnlyReferenceExpression(argument, context) &&
                TryGetScopedConstructorParameterStores(selected.Method, parameter.Name, out var storedFields))
            {
                mayStoreScopedConstructorArgument = true;
                _scopeBoundClassFields.UnionWith(storedFields);
            }
            if ((argumentScopeDepth is not null || IsReadOnlyReferenceExpression(argument, context)) &&
                parameter.PassingMode != "in" && !parameter.IsReadOnly && !parameter.IsScoped && !mayStoreScopedConstructorArgument)
                diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A scope-bound reference may only be passed to a scope or in parameter.", call.Arguments[sourceIndex].Location);
        }
        for (var parameterIndex = 0; parameterIndex < selected.Binding.DefaultsByParameter.Count; parameterIndex++)
            if (selected.Binding.DefaultsByParameter[parameterIndex] is { } defaultArgument)
                _ = AnalyzeExpression(defaultArgument, context, selected.Method.Declaration.Parameters[parameterIndex].TypeName);
        return selected.Method;
    }

    private bool TryGetScopedConstructorParameterStores(MethodSymbol constructor, string parameterName,
        out IReadOnlySet<(string OwnerType, string FieldName)> storedFields)
    {
        var fields = new HashSet<(string OwnerType, string FieldName)>();
        storedFields = fields;
        if (!constructor.IsClassConstructor || constructor.Declaration.Body is not { } body || body.Statements.Count == 0)
            return false;
        if (constructor.SourceConstructor is { } source &&
            (source.InitializerKind == "this" || (source.InitializerArguments ?? []).Count > 0))
            return false;

        var parameterNames = constructor.Declaration.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        if (!constructor.Declaration.Parameters.Any(parameter => parameter.Name == parameterName &&
                parameter.PassingMode.Length == 0 && !parameter.IsReadOnly))
            return false;
        var targetStoresParameter = false;
        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatement { Value: BinaryExpression { Operator: "=", Right: NameExpression value } assignment } ||
                !parameterNames.Contains(value.Name))
                return false;

            string? fieldName = assignment.Left switch
            {
                MemberAccessExpression { Receiver: ThisExpression } member => member.Member,
                NameExpression name when !parameterNames.Contains(name.Name) => name.Name,
                _ => null
            };
            if (fieldName is null || !TryGetClassField(GetQualifiedTypeName(constructor.Owner), fieldName, out var field) || field.Declaration.IsStatic)
                return false;
            if (value.Name != parameterName) continue;
            if (!IsScopeBoundReferenceType(field.TypeName)) return false;
            fields.Add((GetQualifiedTypeName(field.Owner), field.Declaration.Name));
            targetStoresParameter = true;
        }
        return targetStoresParameter;
    }

    private string AnalyzeExpression(Expression expression, MethodContext context, string? expectedType = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (expression)
        {
            case ThisExpression thisExpression:
                if (!context.Method.IsStructConstructor && !context.Method.IsStructInstanceMethod && !context.Method.IsClassConstructor && !context.Method.IsClassInstanceMethod && !context.Method.IsClassFinalizer)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "'this' is supported only inside instance constructors and methods.", thisExpression.Location);
                    return "?";
                }
                var thisOwner = GetQualifiedTypeName(context.Method.Owner);
                if (thisOwner == "System.String") return "string";
                return PrimitiveKeywordAlias(thisOwner) ?? thisOwner;
            case BaseExpression baseExpression:
                var baseContextOwner = context.Method.Owner;
                var enclosingOwners = new HashSet<string>(StringComparer.Ordinal);
                while (_closureEnclosingMethods.TryGetValue(GetQualifiedTypeName(baseContextOwner), out var enclosingMethod) &&
                    enclosingOwners.Add(GetQualifiedTypeName(baseContextOwner)))
                    baseContextOwner = enclosingMethod.Owner;
                if (baseContextOwner.Kind != "class" || !context.Method.IsClassConstructor && !context.Method.IsClassInstanceMethod && !context.Method.IsClassFinalizer)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "'base' is supported only inside class constructors and instance methods.", baseExpression.Location);
                    return "?";
                }
                if (!_classTypes.TryGetValue(GetQualifiedTypeName(baseContextOwner), out var baseOwner) || baseOwner.BaseTypeName is null)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Class '{GetQualifiedTypeName(baseContextOwner)}' has no base class.", baseExpression.Location);
                    return "?";
                }
                return baseOwner.BaseTypeName;
            case NullAssertionExpression assertion:
                var assertedType = AnalyzeExpression(assertion.Operand, context);
                if (IsNullableStringType(assertedType))
                {
                    if (assertion.Operand is NameExpression nullableStringName) context.SetNullState(nullableStringName.Name, NullState.NonNull);
                    return "string";
                }
                if (IsNullableArrayType(assertedType))
                {
                    if (assertion.Operand is NameExpression nullableArrayName) context.SetNullState(nullableArrayName.Name, NullState.NonNull);
                    return NullableUnderlyingType(assertedType);
                }
                if (IsNullableValueType(assertedType))
                {
                    if (assertion.Operand is NameExpression nullableValueName) context.SetNullState(nullableValueName.Name, NullState.NonNull);
                    return NullableUnderlyingType(assertedType);
                }
                if (IsNullableClassType(assertedType))
                {
                    if (assertion.Operand is NameExpression nullableName) context.SetNullState(nullableName.Name, NullState.NonNull);
                    return NullableUnderlyingType(assertedType);
                }
                if (IsNullableDelegateType(assertedType))
                {
                    if (assertion.Operand is NameExpression nullableDelegateName) context.SetNullState(nullableDelegateName.Name, NullState.NonNull);
                    return NullableUnderlyingType(assertedType);
                }
                if (IsClassReferenceType(assertedType) && !IsNullableClassType(assertedType))
                {
                    diagnostics.Add("LS3103", DiagnosticSeverity.Warning, "Null assertion has no effect on a non-nullable reference.", assertion.Location);
                    return assertedType;
                }
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Null assertion requires a nullable reference, not '{assertedType}'.", assertion.Location);
                return "?";
            case LiteralExpression literal:
                if (literal.Value is "true" or "false") return "bool";
                if (literal.Value == "null") return "null";
                if (literal.Value.StartsWith('"') || literal.Value.StartsWith('@'))
                    return TryInternStringLiteral(literal, out _) ? "string" : "?";
                if (literal.Value.StartsWith('\''))
                {
                    if (!TryParseCharacter(literal.Value, out _)) diagnostics.Add("LS0001", DiagnosticSeverity.Error, "Character literal must represent exactly one UTF-16 code unit.", literal.Location);
                    return "char";
                }
                if (IsFloatingLiteral(literal.Value))
                {
                    var floating = ParseFloating(literal.Value);
                    if (floating.Type == "?") diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Malformed floating-point literal '{literal.Value}'.", literal.Location);
                    return floating.Type;
                }
                var number = ParseInteger(literal.Value);
                if (number.Type == "?") diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Malformed or out-of-range integer literal '{literal.Value}'.", literal.Location);
                return number.Type;
            case NameExpression name:
                if (_currentDeclarationNames?.Contains(name.Name) == true) return "?";
                if (context.Lookup(name.Name) is { } local) return local.Type;
                if (TryBindUnqualifiedConstantField(name, context, out var unqualifiedConstant)) return unqualifiedConstant.TypeName;
                if (expectedType is not null && TryBindMethodGroup(name, expectedType, context))
                    return NormalizeEnumType(expectedType, context.Method.Owner);
                if (name.Name == "field" && context.Method.Declaration.PropertyName is { } currentPropertyName)
                {
                    var ownerName = GetQualifiedTypeName(context.Method.Owner);
                    var backingName = GetPropertyBackingFieldName(context.Method.Declaration);
                    if (context.Method.Declaration.IsStatic && _staticFields.TryGetValue(ownerName + "." + backingName, out var staticBacking))
                    {
                        var binding = new StaticFieldBinding(staticBacking.Symbol.Declaration.TypeName, staticBacking.Symbol.GlobalName,
                            !staticBacking.Symbol.Declaration.IsReadOnly, GetQualifiedTypeName(staticBacking.Owner));
                        _boundUnqualifiedStaticFields[name] = binding;
                        return binding.TypeName;
                    }
                    if (!context.Method.Declaration.IsStatic && context.Method.Owner.Kind == "class" && TryGetClassField(ownerName, backingName, out var classBacking))
                    {
                        _boundUnqualifiedClassFields[name] = new(ownerName, classBacking.TypeName, classBacking.Index, classBacking.Declaration.IsReadOnly,
                            DeclaredOwnerType: GetQualifiedTypeName(classBacking.Owner));
                        return classBacking.TypeName;
                    }
                    if (!context.Method.Declaration.IsStatic && context.Method.Owner.Kind == "struct" && TryGetStructField(ownerName, backingName, out var structBacking))
                    {
                        _boundUnqualifiedStructFields[name] = new(ownerName, structBacking.TypeName, structBacking.Index, structBacking.Declaration.IsReadOnly);
                        return structBacking.TypeName;
                    }
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "The contextual 'field' keyword requires a property with compiler-generated backing storage.", name.Location);
                    return "?";
                }
                if ((context.Method.IsClassConstructor || context.Method.IsClassInstanceMethod || context.Method.IsClassFinalizer) && TryGetClassField(GetQualifiedTypeName(context.Method.Owner), name.Name, out var instanceClassField))
                {
                    if (!IsAccessibilityAvailable(GetFieldAccessibility(instanceClassField.Declaration), instanceClassField.Owner, context.Method.Owner))
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Class field '{GetQualifiedTypeName(instanceClassField.Owner)}.{name.Name}' is not accessible from this type.", name.Location);
                    _boundUnqualifiedClassFields[name] = new(GetQualifiedTypeName(context.Method.Owner), instanceClassField.TypeName, instanceClassField.Index,
                        instanceClassField.Declaration.IsReadOnly, DeclaredOwnerType: GetQualifiedTypeName(instanceClassField.Owner));
                    return instanceClassField.TypeName;
                }
                if ((context.Method.IsStructConstructor || context.Method.IsStructInstanceMethod) && TryGetStructField(GetQualifiedTypeName(context.Method.Owner), name.Name, out var instanceField))
                {
                    if (!IsAccessibilityAvailable(GetFieldAccessibility(instanceField.Declaration), instanceField.Owner, context.Method.Owner))
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Struct field '{GetQualifiedTypeName(instanceField.Owner)}.{name.Name}' is not accessible from this type.", name.Location);
                    _boundUnqualifiedStructFields[name] = new(GetQualifiedTypeName(context.Method.Owner), instanceField.TypeName, instanceField.Index, instanceField.Declaration.IsReadOnly);
                    return instanceField.TypeName;
                }
                var currentTypeName = GetQualifiedTypeName(context.Method.Owner);
                IEnumerable<string> staticFieldOwners = context.Method.Owner.Kind == "class" ? GetClassChain(currentTypeName) : [currentTypeName];
                foreach (var staticFieldOwner in staticFieldOwners)
                    if (_staticFields.TryGetValue(staticFieldOwner + "." + name.Name, out var unqualifiedStaticField))
                    {
                        if (!IsAccessibilityAvailable(unqualifiedStaticField.Accessibility, unqualifiedStaticField.Owner, context.Method.Owner))
                            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Static field '{staticFieldOwner}.{name.Name}' is not accessible from this type.", name.Location);
                        var binding = new StaticFieldBinding(unqualifiedStaticField.Symbol.Declaration.TypeName,
                            unqualifiedStaticField.Symbol.GlobalName, !unqualifiedStaticField.Symbol.Declaration.IsReadOnly,
                            GetQualifiedTypeName(unqualifiedStaticField.Owner));
                        _boundUnqualifiedStaticFields[name] = binding;
                        return binding.TypeName;
                    }
                if (TryGetComputedProperty(GetQualifiedTypeName(context.Method.Owner), name.Name) is { } unqualifiedProperty)
                {
                    Expression receiver;
                    if (unqualifiedProperty.Declaration.IsStatic)
                        receiver = new NameExpression(context.Method.Owner.Name, name.Location);
                    else if (context.Method.IsClassConstructor || context.Method.IsClassInstanceMethod || context.Method.IsClassFinalizer ||
                             context.Method.IsStructConstructor || context.Method.IsStructInstanceMethod)
                        receiver = new ThisExpression(name.Location);
                    else
                    {
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Instance property '{unqualifiedProperty.Declaration.Name}' requires an object receiver in a static method.", name.Location);
                        return unqualifiedProperty.TypeName;
                    }
                    var syntheticAccess = new MemberAccessExpression(receiver, name.Name, name.Location);
                    _boundUnqualifiedComputedProperties[name] = unqualifiedProperty;
                    var propertyType = BindComputedPropertyRead(syntheticAccess, unqualifiedProperty, context);
                    if (_propertyGetterCalls.TryGetValue(syntheticAccess, out var propertyGetterCall))
                        _propertyGetterNameCalls[name] = propertyGetterCall;
                    return propertyType;
                }
                diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"The name '{name.Name}' does not exist in the current context.", name.Location); return "?";
            case UnaryExpression unary:
                var operand = AnalyzeExpression(unary.Operand, context);
                if (unary.Operator == "!" && (IsStructType(operand) || IsClassType(operand)))
                {
                    var logicalNot = FindUserUnaryOperator("!", operand, unary.Operand, context);
                    if (logicalNot is not null) { _operatorTargets[unary] = logicalNot; return NormalizeEnumType(logicalNot.Declaration.ReturnType, logicalNot.Owner); }
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Type '{operand}' does not define a public static '!' operator.", unary.Location);
                    return "?";
                }
                if (unary.Operator == "!") { if (operand != "bool") diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Logical negation requires bool.", unary.Location); return "bool"; }
                if (unary.Operator is "++" or "--")
                {
                    var readonlyStructTarget = unary.Operand is NameExpression readonlyStructName && _boundUnqualifiedStructFields.TryGetValue(readonlyStructName, out var readonlyUnqualifiedStruct) && readonlyUnqualifiedStruct.IsReadOnly ||
                        unary.Operand is MemberAccessExpression readonlyStructAccess && _boundStructFields.TryGetValue(readonlyStructAccess, out var readonlyBoundStruct) && readonlyBoundStruct.IsReadOnly;
                    var readonlyClassTarget = unary.Operand is NameExpression readonlyClassName && _boundUnqualifiedClassFields.TryGetValue(readonlyClassName, out var readonlyUnqualifiedClass) && readonlyUnqualifiedClass.IsReadOnly ||
                        unary.Operand is MemberAccessExpression readonlyClassAccess && _boundClassFields.TryGetValue(readonlyClassAccess, out var readonlyBoundClass) && readonlyBoundClass.IsReadOnly;
                    var mayInitializeReadonlyTarget = CanInitializeReadOnlyFieldTarget(unary.Operand, context);
                    var mutableTarget = unary.Operand is NameExpression n && context.Lookup(n.Name) is { Mutable: true } ||
                        unary.Operand is NameExpression unqualifiedStaticName && _boundUnqualifiedStaticFields.TryGetValue(unqualifiedStaticName, out var unqualifiedStaticIncrementField) && unqualifiedStaticIncrementField.Mutable ||
                        unary.Operand is NameExpression unqualifiedFieldName && _boundUnqualifiedStructFields.TryGetValue(unqualifiedFieldName, out var unqualifiedIncrementField) &&
                            (!unqualifiedIncrementField.IsReadOnly || CanInitializeReadOnlyStructField(unqualifiedIncrementField, new ThisExpression(unary.Location), context)) && IsMutableStructLocation(new ThisExpression(unary.Location), context) ||
                        unary.Operand is MemberAccessExpression access && _boundStaticFields.TryGetValue(access, out var mutableBoundField) && mutableBoundField.Mutable ||
                        unary.Operand is MemberAccessExpression structAccess && _boundStructFields.TryGetValue(structAccess, out var incrementStructField) && (!incrementStructField.IsReadOnly || CanInitializeReadOnlyStructField(incrementStructField, structAccess.Receiver, context)) && IsMutableStructLocation(structAccess.Receiver, context) ||
                        unary.Operand is NameExpression unqualifiedClassName && _boundUnqualifiedClassFields.TryGetValue(unqualifiedClassName, out var unqualifiedClassIncrementField) && (!unqualifiedClassIncrementField.IsReadOnly || CanInitializeReadOnlyClassField(unqualifiedClassIncrementField, new ThisExpression(unary.Location), context)) && IsMutableClassFieldLocation(new ThisExpression(unary.Location), context) ||
                        unary.Operand is MemberAccessExpression classAccess && _boundClassFields.TryGetValue(classAccess, out var incrementClassField) && (!incrementClassField.IsReadOnly || CanInitializeReadOnlyClassField(incrementClassField, classAccess.Receiver, context)) && IsMutableClassFieldLocation(classAccess.Receiver, context) ||
                        unary.Operand is UnaryExpression { Operator: "*" } || unary.Operand is IndexExpression indexed && IsMutableIndexTarget(indexed, context);
                    if (!mutableTarget)
                    {
                        if ((readonlyStructTarget || readonlyClassTarget) && !mayInitializeReadonlyTarget)
                            diagnostics.Add("LS0231", DiagnosticSeverity.Error, "A readonly field may only be assigned in its declaring type's constructor or initializer.", unary.Location);
                        else diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "Increment and decrement require a mutable local or supported static field.", unary.Location);
                    }
                    if (IsStructType(operand) || IsClassType(operand))
                    {
                        var incrementOperator = FindUserUnaryOperator(unary.Operator, operand, unary.Operand, context);
                        if (incrementOperator is not null) { _operatorTargets[unary] = incrementOperator; return operand; }
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Type '{operand}' does not define a public static '{unary.Operator}' operator.", unary.Location);
                        return "?";
                    }
                    return operand;
                }
                if (unary.Operator == "&")
                {
                    if (unary.Operand is NameExpression addressName && context.Lookup(addressName.Name) is { } addressedLocal)
                    {
                        if (!addressedLocal.Mutable) diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a mutable local or parameter.", unary.Location);
                        return addressedLocal.Type + "*";
                    }
                    if (unary.Operand is NameExpression unqualifiedStaticAddressName &&
                        _boundUnqualifiedStaticFields.TryGetValue(unqualifiedStaticAddressName, out var unqualifiedStaticAddressField))
                    {
                        if (!unqualifiedStaticAddressField.Mutable)
                            diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a mutable field target.", unary.Location);
                        return unqualifiedStaticAddressField.TypeName + "*";
                    }
                    if (unary.Operand is IndexExpression indexedTarget)
                    {
                        if (!IsMutableIndexTarget(indexedTarget, context)) diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator cannot target an element of an immutable inline array.", unary.Location);
                        return operand + "*";
                    }
                    if (unary.Operand is MemberAccessExpression staticFieldTarget &&
                        _boundStaticFields.TryGetValue(staticFieldTarget, out var addressedStaticField))
                    {
                        if (!addressedStaticField.Mutable)
                            diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a mutable field target.", unary.Location);
                        return addressedStaticField.TypeName + "*";
                    }
                    if (unary.Operand is MemberAccessExpression fieldTarget && _boundStructFields.TryGetValue(fieldTarget, out var boundStructField))
                    {
                        if (boundStructField.IsReadOnly || !IsMutableStructLocation(fieldTarget.Receiver, context))
                            diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a mutable struct field target.", unary.Location);
                        return operand + "*";
                    }
                    if (unary.Operand is MemberAccessExpression classFieldTarget && _boundClassFields.TryGetValue(classFieldTarget, out var boundClassField))
                    {
                        if (boundClassField.IsReadOnly || !IsMutableClassFieldLocation(classFieldTarget.Receiver, context))
                            diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a mutable class field target.", unary.Location);
                        return operand + "*";
                    }
                    if (unary.Operand is NameExpression unqualifiedClassFieldTarget &&
                        _boundUnqualifiedClassFields.TryGetValue(unqualifiedClassFieldTarget, out var unqualifiedAddressClassField))
                    {
                        if (unqualifiedAddressClassField.IsReadOnly || !IsMutableClassFieldLocation(new ThisExpression(unary.Location), context))
                            diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a mutable class field target.", unary.Location);
                        return operand + "*";
                    }
                    if (unary.Operand is NameExpression unqualifiedFieldTarget && _boundUnqualifiedStructFields.TryGetValue(unqualifiedFieldTarget, out var unqualifiedAddressField))
                    {
                        if (unqualifiedAddressField.IsReadOnly || !IsMutableStructLocation(new ThisExpression(unary.Location), context))
                            diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a mutable struct field target.", unary.Location);
                        return operand + "*";
                    }
                    diagnostics.Add("LS1902", DiagnosticSeverity.Error, "The address-of operator requires a supported mutable local or parameter.", unary.Location);
                    return "?";
                }
                if (unary.Operator == "*")
                {
                    if (!IsPointerType(operand)) { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot dereference non-pointer type '{operand}'.", unary.Location); return "?"; }
                    var pointee = operand[..^1];
                    if (pointee == "void") { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A void pointer must be converted to a typed pointer before dereferencing.", unary.Location); return "?"; }
                    return pointee;
                }
                if ((IsStructType(operand) || IsClassType(operand)) && (unary.Operator is "+" or "-" or "~"))
                {
                    var overloaded = FindUserUnaryOperator(unary.Operator, operand, unary.Operand, context);
                    if (overloaded is not null) { _operatorTargets[unary] = overloaded; return NormalizeEnumType(overloaded.Declaration.ReturnType, overloaded.Owner); }
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Type '{operand}' does not define a public static '{unary.Operator}' operator.", unary.Location);
                    return "?";
                }
                if (IsNullableValueType(operand) && unary.Operator is "+" or "-" or "~")
                {
                    var nullableUnderlying = NullableUnderlyingType(operand);
                    if (!NumericTypes.Contains(nullableUnderlying) || unary.Operator == "~" && !IntegerTypes.Contains(nullableUnderlying))
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Unary '{unary.Operator}' is not defined for nullable type '{operand}'.", unary.Location);
                    return operand;
                }
                if (IsEnumType(operand) && unary.Operator != "~") { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Unary '{unary.Operator}' is not defined for enum values.", unary.Location); return operand; }
                if (!NumericTypes.Contains(operand) && !IsEnumType(operand) || (unary.Operator == "~" && !IntegerTypes.Contains(operand) && !IsEnumType(operand))) { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Unary '{unary.Operator}' requires a compatible numeric operand.", unary.Location); return operand; }
                return PromoteSmallInteger(operand);
            case CastExpression cast:
                var inaccessibleCastTarget = ValidateTypeAccessibility(cast.TypeName, context.Method.Owner, cast.Location);
                var sourceType = AnalyzeExpression(cast.Operand, context);
                if (inaccessibleCastTarget) return "?";
                var targetType = NormalizeEnumType(cast.TypeName, context.Method.Owner);
                var sourceClass = GetClassReferenceTypeForAnalysis(sourceType);
                var targetClass = GetClassReferenceTypeForAnalysis(targetType);
                var explicitUnboxing = sourceClass is not null && IsBoxableValueType(targetType);
                var explicitNullableUnboxing = sourceClass is not null && IsNullableValueType(targetType) && IsBoxableValueType(NullableUnderlyingType(targetType));
                var explicitClassConversion = sourceClass is not null && targetClass is not null &&
                    (IsClassType(sourceClass) && IsClassType(targetClass)
                        ? IsBaseClassOf(sourceClass, targetClass) || IsBaseClassOf(targetClass, sourceClass) || sourceClass == targetClass
                        : IsClassReferenceType(sourceClass) && IsClassReferenceType(targetClass) &&
                          (IsInterfaceType(sourceClass) || IsInterfaceType(targetClass)));
                var explicitStructInterfaceBoxing = IsStructType(sourceType) && targetClass is not null && IsInterfaceType(targetClass) &&
                    _interfaceConversions.Contains((sourceType, targetClass));
                var userConversion = FindUserConversionOperator(sourceType, targetType, allowExplicit: true) ??
                    FindArraySpanConversionMethod(sourceType, targetType);
                if (!IsSupportedType(targetType) || targetType == "void" ||
                    !(sourceType == targetType || NumericTypes.Contains(sourceType) && NumericTypes.Contains(targetType) ||
                      sourceType == "null" && (IsNullableValueType(targetType) || IsNullableStringType(targetType) || IsNullableArrayType(targetType)) ||
                      sourceType == "null" && IsNullableClassType(targetType) || explicitClassConversion || explicitStructInterfaceBoxing || explicitUnboxing || explicitNullableUnboxing ||
                      sourceType == "string" && IsNullableStringType(targetType) || IsNullableStringType(sourceType) && targetType == "string" ||
                      IsNullableArrayType(targetType) && sourceType == NullableUnderlyingType(targetType) ||
                      IsNullableArrayType(sourceType) && targetType == NullableUnderlyingType(sourceType) ||
                      IsNullableValueType(targetType) && sourceType == NullableUnderlyingType(targetType) ||
                      IsNullableValueType(sourceType) && targetType == NullableUnderlyingType(sourceType) ||
                      IsEnumType(sourceType) && IntegerTypes.Contains(targetType) || IntegerTypes.Contains(sourceType) && IsEnumType(targetType) ||
                      IsEnumType(sourceType) && IsEnumType(targetType) ||
                      IsPointerType(sourceType) && (IsPointerType(targetType) || IntegerTypes.Contains(targetType)) ||
                      IntegerTypes.Contains(sourceType) && IsPointerType(targetType) || userConversion is not null))
                {
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Explicit conversion from '{sourceType}' to '{targetType}' is not supported.", cast.Location);
                    return "?";
                }
                if (userConversion is not null)
                    _userConversionOperators[(sourceType, targetType)] = userConversion;
                _castTypes[cast] = targetType;
                if (_checkedContext && IsConstantExpression(cast.Operand, context) && CanEvaluateConstantNow(cast.Operand) && !TryEvaluateConstant(cast with { TypeName = targetType }, out _, out var constantError))
                    diagnostics.Add("LS0301", DiagnosticSeverity.Error, constantError ?? $"Constant conversion overflows '{targetType}'.", cast.Location);
                return targetType;
            case BinaryExpression binary:
                return AnalyzeBinary(binary, context);
            case ConditionalExpression conditional:
                RequireBool(conditional.Condition, context);
                var knownConditional = GetKnownBoolean(conditional.Condition);
                var beforeConditional = context.CaptureNullStates();
                RefineNullCondition(conditional.Condition, context, whenTrue: true);
                context.Push();
                var conditionalPatternCaptureScopes = DeclarePositiveTypePatternVariables(conditional.Condition, context);
                var t = AnalyzeExpression(conditional.WhenTrue, context, expectedType);
                var trueBranchNonNull = IsDefinitelyNotNull(conditional.WhenTrue, t, context);
                var afterTrue = context.CaptureNullStates();
                ExitCaptureScopes(context, conditionalPatternCaptureScopes);
                context.Pop();
                context.RestoreNullStates(beforeConditional);
                RefineNullCondition(conditional.Condition, context, whenTrue: false);
                context.Push();
                var f = AnalyzeExpression(conditional.WhenFalse, context, expectedType);
                var falseBranchNonNull = IsDefinitelyNotNull(conditional.WhenFalse, f, context);
                var afterFalse = context.CaptureNullStates();
                context.Pop();
                if (knownConditional == true) context.RestoreNullStates(afterTrue);
                else if (knownConditional == false) context.RestoreNullStates(afterFalse);
                else context.RestoreNullStates(MergeNullStates(afterTrue, afterFalse));
                if (t == f) return RecordConditionalExpressionType(conditional, t);
                if (t == "null" && IsStringType(f) || f == "null" && IsStringType(t)) return RecordConditionalExpressionType(conditional, "string?");
                if (IsStringType(t) && IsStringType(f)) return RecordConditionalExpressionType(conditional, IsNullableStringType(t) || IsNullableStringType(f) ? "string?" : "string");
                if (t == "null" && (IsNullableType(f) || IsManagedReferenceValue(f) || IsNullableValuePayloadType(f))) return RecordConditionalExpressionType(conditional, NullableUnderlyingType(f) + "?");
                if (f == "null" && (IsNullableType(t) || IsManagedReferenceValue(t) || IsNullableValuePayloadType(t))) return RecordConditionalExpressionType(conditional, NullableUnderlyingType(t) + "?");
                if (TryFindConditionalNumericCommonType(t, conditional.WhenTrue, f, conditional.WhenFalse, out var commonNumericType))
                    return RecordConditionalExpressionType(conditional, commonNumericType);
                if (IsGenericInferenceReferenceType(t) && IsGenericInferenceReferenceType(f))
                {
                    var trueType = trueBranchNonNull ? NullableUnderlyingType(t) : t;
                    var falseType = falseBranchNonNull ? NullableUnderlyingType(f) : f;
                    var trueUnderlying = NormalizeEnumType(NullableUnderlyingType(trueType), context.Method.Owner);
                    var falseUnderlying = NormalizeEnumType(NullableUnderlyingType(falseType), context.Method.Owner);
                    var commonType = GetClassChain(trueUnderlying).FirstOrDefault(candidate =>
                        candidate == falseUnderlying || IsBaseClassOf(candidate, falseUnderlying));
                    if (commonType == "System.Object") commonType = null;
                    commonType ??= TryFindCommonReferenceInferenceType(trueType, falseType, context.Method.Owner, out var inferredReference)
                        ? NullableUnderlyingType(inferredReference)
                        : null;
                    if (commonType is not null)
                    {
                        if (trueBranchNonNull && falseBranchNonNull) return RecordConditionalExpressionType(conditional, commonType);
                        return RecordConditionalExpressionType(conditional, IsNullableReferenceInferenceType(commonType) ? commonType : commonType + "?");
                    }
                }
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Conditional expression branches must have a common supported type.", conditional.Location);
                return RecordConditionalExpressionType(conditional, "?");
            case CallExpression call:
                if (call.Target is MemberAccessExpression qualifiedCall && !HasLocalRoot(qualifiedCall.Receiver, context) &&
                    TryFlattenName(qualifiedCall.Receiver, out var inaccessibleQualifier) &&
                    IsInaccessibleTypeReference(inaccessibleQualifier, context.Method.Owner))
                {
                    ValidateTypeAccessibility(inaccessibleQualifier, context.Method.Owner, qualifiedCall.Receiver.Location);
                    foreach (var argument in call.Arguments) _ = AnalyzeExpression(argument, context);
                    return "?";
                }
                if (call.Target is NameExpression delegateName && context.Lookup(delegateName.Name) is { } delegateLocal &&
                    _delegateTypes.TryGetValue(NullableUnderlyingType(delegateLocal.Type), out var localDelegateType))
                {
                    if (IsNullableDelegateType(delegateLocal.Type) && !IsDefinitelyNotNull(call.Target, delegateLocal.Type, context))
                    {
                        diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable delegate may be null; use a null check or null assertion before invocation.", call.Target.Location);
                        return "?";
                    }
                    return AnalyzeDelegateInvocation(call, localDelegateType, context);
                }
                if (call.Target is MemberAccessExpression delegateFieldAccess)
                {
                    var isTypeReceiver = IsTypeReference(delegateFieldAccess.Receiver, context);
                    if (isTypeReceiver && TryBindStaticField(delegateFieldAccess, context, out var delegateStaticField) &&
                        _delegateTypes.TryGetValue(NullableUnderlyingType(delegateStaticField.TypeName), out var staticFieldDelegate))
                    {
                        if (IsNullableDelegateType(delegateStaticField.TypeName) && !IsDefinitelyNotNull(delegateFieldAccess, delegateStaticField.TypeName, context))
                        {
                            diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable delegate may be null; use a null check or null assertion before invocation.", delegateFieldAccess.Location);
                            return "?";
                        }
                        return AnalyzeDelegateInvocation(call, staticFieldDelegate, context);
                    }
                    if (!isTypeReceiver)
                    {
                        var delegateFieldReceiverType = AnalyzeExpression(delegateFieldAccess.Receiver, context);
                        var resolvedReceiverType = NullableUnderlyingType(delegateFieldReceiverType);
                        if (IsClassType(resolvedReceiverType) && TryGetClassField(resolvedReceiverType, delegateFieldAccess.Member, out var delegateClassField) &&
                            _delegateTypes.TryGetValue(NullableUnderlyingType(delegateClassField.TypeName), out var classFieldDelegate))
                        {
                            _ = AnalyzeExpression(delegateFieldAccess, context);
                            if (IsNullableDelegateType(delegateClassField.TypeName) && !IsDefinitelyNotNull(delegateFieldAccess, delegateClassField.TypeName, context))
                            {
                                diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable delegate may be null; use a null check or null assertion before invocation.", delegateFieldAccess.Location);
                                return "?";
                            }
                            return AnalyzeDelegateInvocation(call, classFieldDelegate, context);
                        }
                    }
                }
                if (call.Target is NullAssertionExpression assertedDelegate)
                {
                    var nullAssertedDelegateType = AnalyzeExpression(assertedDelegate, context);
                    if (_delegateTypes.TryGetValue(nullAssertedDelegateType, out var assertedDelegateType))
                        return AnalyzeDelegateInvocation(call, assertedDelegateType, context);
                }
                if (call.Target is CallExpression or IndexExpression)
                {
                    var delegateExpressionTargetType = AnalyzeExpression(call.Target, context);
                    if (_delegateTypes.TryGetValue(NullableUnderlyingType(delegateExpressionTargetType), out var expressionDelegateType))
                    {
                        if (IsNullableDelegateType(delegateExpressionTargetType) && !IsDefinitelyNotNull(call.Target, delegateExpressionTargetType, context))
                        {
                            diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable delegate may be null; use a null check or null assertion before invocation.", call.Target.Location);
                            return "?";
                        }
                        return AnalyzeDelegateInvocation(call, expressionDelegateType, context);
                    }
                }
                if (call.Target is MemberAccessExpression arrayMember && arrayMember.Member == "GetLength")
                {
                    var arrayType = AnalyzeExpression(arrayMember.Receiver, context);
                    var nullableGetLengthArrayBase = IsNullableClassType(arrayType) && NullableUnderlyingType(arrayType) == "System.Array";
                    var nullableGetLengthArray = nullableGetLengthArrayBase || IsNullableArrayType(arrayType);
                    var isArrayBase = arrayType == "System.Array" || nullableGetLengthArrayBase;
                    var rank = 0;
                    var isManagedArray = TryParseManagedArrayType(NullableUnderlyingType(arrayType), out _, out rank);
                    if (isArrayBase || isManagedArray)
                    {
                        var resolvedRank = isArrayBase ? -1 : rank;
                        if (resolvedRank > 3) diagnostics.Add("LS1503", DiagnosticSeverity.Error, "Rectangular arrays may have at most three dimensions.", call.Location);
                        if (nullableGetLengthArray && !arrayMember.IsNullConditional && !IsDefinitelyNotNull(arrayMember.Receiver, arrayType, context))
                            diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable array instance may be null; use a null check or null assertion before calling GetLength.", arrayMember.Location);
                        if (call.Arguments.Count != 1) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Array.GetLength requires exactly one dimension argument.", call.Location);
                        foreach (var argument in call.Arguments)
                        {
                            var argumentType = AnalyzeExpression(argument, context);
                            if (!CanConvert(argumentType, "int32", argument)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "An array dimension index must be convertible to int32.", argument.Location);
                        }
                        _managedArrayGetLengthCalls[call] = resolvedRank;
                        return nullableGetLengthArray && arrayMember.IsNullConditional ? "int32?" : "int32";
                    }
                }
                if (call.Target is NameExpression structName && context.Lookup(structName.Name) is null && !_methods.Any(candidate => candidate.Owner == context.Method.Owner && candidate.Declaration.Name == structName.Name && candidate.Declaration.IsStatic))
                {
                    var structMatches = FindTypes(structName.Name, context.Method.Owner).Where(candidate => candidate.Kind == "struct").ToArray();
                    var genericStructDefinition = call.TypeArguments is { Count: > 0 }
                        ? GenericTypeSpecializer.ResolveDefinition(structName.Name, context.Method.Owner, _genericDefinitions)
                        : null;
                    var structDefinition = structMatches.Length == 1
                        ? structMatches[0]
                        : genericStructDefinition is { Kind: "struct" } ? genericStructDefinition : null;
                    if (structDefinition is not null)
                    {
                        var structType = GetQualifiedTypeName(structDefinition);
                        if (call.TypeArguments is { } structTypeArguments && structDefinition.HasTypeParameters)
                        {
                            var typeParameters = structDefinition.TypeParameters ?? [];
                            if (structTypeArguments.Count != typeParameters.Count)
                            {
                                diagnostics.Add("LS1101", DiagnosticSeverity.Error,
                                    $"Generic struct '{structType}' requires {typeParameters.Count} type argument(s), but {structTypeArguments.Count} were supplied.", call.Location);
                                return "?";
                            }
                            var normalizedArguments = structTypeArguments
                                .Select(argument => NormalizeEnumType(argument, context.Method.Owner))
                                .ToArray();
                            var constructedName = $"{structType}<{string.Join(",", normalizedArguments)}>";
                            if (ResolveGenericInstance(constructedName, context.Method.Owner) is not { } specializedStruct)
                            {
                                diagnostics.Add("LS1101", DiagnosticSeverity.Error,
                                    $"Generic struct construction '{constructedName}' was not expanded.", call.Location);
                                return "?";
                            }
                            structType = GetQualifiedTypeName(specializedStruct);
                        }
                        if (call.Arguments.Count == 0) { _structConstructors[call] = structType; return structType; }
                        var constructorCandidates = _methods.Where(candidate => candidate.IsStructConstructor && GetQualifiedTypeName(candidate.Owner) == structType &&
                            IsConstructorAccessible(candidate, context.Method.Owner)).ToArray();
                        var argumentTypes = call.Arguments.Select(argument => argument switch
                        {
                            InlineOutVariableExpression { TypeName: null } => "?",
                            InlineOutVariableExpression inline => NormalizeEnumType(inline.TypeName!, context.Method.Owner),
                            DefaultValueExpression { TypeName: null } => "?",
                            _ => AnalyzeExpression(argument, context)
                        }).ToArray();
                        var applicableConstructors = new List<(MethodSymbol Method, BoundCallArguments Binding)>();
                        foreach (var candidate in constructorCandidates)
                        {
                            if (!TryBindCallArguments(call, candidate, argumentTypes, out var binding)) continue;
                            var valid = true;
                            for (var index = 0; index < argumentTypes.Length; index++)
                            {
                                var parameter = candidate.Declaration.Parameters[binding.ParameterBySource[index]];
                                var parameterType = GetParameterTypeForSource(candidate, binding, index);
                                var mode = call.ArgumentModes is not null && index < call.ArgumentModes.Count ? call.ArgumentModes[index] ?? "" : "";
                                if (call.Arguments[index] is InlineOutVariableExpression inlineOut)
                                {
                                    var declaredType = inlineOut.TypeName is null ? parameterType : argumentTypes[index];
                                    if (mode != "out" || parameter.PassingMode != "out" || declaredType != parameterType) { valid = false; break; }
                                    continue;
                                }
                                if (parameter.PassingMode is "ref" or "out")
                                {
                                    if (mode != parameter.PassingMode || argumentTypes[index] != parameterType || !IsMutableVariableArgument(call.Arguments[index], context)) { valid = false; break; }
                                    continue;
                                }
                                if (mode is "ref" or "out" || parameter.PassingMode == "in" && mode is not ("" or "in") || parameter.PassingMode != "in" && mode == "in" ||
                                    argumentTypes[index] != "?" && !CanConvert(argumentTypes[index], parameterType, call.Arguments[index]))
                                { valid = false; break; }
                            }
                            if (valid) applicableConstructors.Add((candidate, binding));
                        }
                        if (applicableConstructors.Count == 0)
                        {
                            diagnostics.Add("LS0751", DiagnosticSeverity.Error, $"No applicable constructor overload for '{structType}'.", call.Location);
                            return "?";
                        }
                        var bestConstructors = applicableConstructors.Where(candidate => applicableConstructors.All(other =>
                            ReferenceEquals(candidate.Method, other.Method) || CompareCandidates(candidate.Method, other.Method, argumentTypes, call.Arguments, candidate.Binding, other.Binding) <= 0)).ToArray();
                        if (bestConstructors.Length > 1)
                        {
                            var minDefaults = bestConstructors.Min(candidate => candidate.Binding.DefaultsByParameter.Count(value => value is not null));
                            bestConstructors = bestConstructors.Where(candidate => candidate.Binding.DefaultsByParameter.Count(value => value is not null) == minDefaults).ToArray();
                        }
                        if (bestConstructors.Length > 1)
                        {
                            var minVariadicPenalty = bestConstructors.Min(candidate => VariadicBindingPenalty(candidate.Binding));
                            bestConstructors = bestConstructors.Where(candidate => VariadicBindingPenalty(candidate.Binding) == minVariadicPenalty).ToArray();
                        }
                        if (bestConstructors.Length != 1)
                        {
                            diagnostics.Add("LS0750", DiagnosticSeverity.Error, $"Call to constructor '{structType}' is ambiguous.", call.Location);
                            return "?";
                        }
                        var selected = bestConstructors[0];
                        _structConstructors[call] = structType;
                        _callTargets[call] = selected.Method;
                        _callArguments[call] = selected.Binding;
                        for (var index = 0; index < call.Arguments.Count; index++)
                        {
                            if (call.Arguments[index] is InlineOutVariableExpression inlineOut)
                            {
                                var parameter = selected.Method.Declaration.Parameters[selected.Binding.ParameterBySource[index]];
                                var inlineType = inlineOut.TypeName is null ? parameter.TypeName : argumentTypes[index];
                                ValidateDeclaredType(inlineType, "Inline out local", inlineOut.Location, context.Method.Owner);
                                if (context.Exists(inlineOut.Name)) diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Inline out local '{inlineOut.Name}' conflicts with an existing local.", inlineOut.Location);
                                else context.Declare(inlineOut.Name, inlineType, true, inlineOut.Location,
                                    forHeaderOwner: context.ForInitializerOwner,
                                    blockOwner: context.ForInitializerOwner is null ? context.CurrentCaptureBlock : null);
                            }
                        }
                        for (var parameterIndex = 0; parameterIndex < selected.Binding.DefaultsByParameter.Count; parameterIndex++)
                            if (selected.Binding.DefaultsByParameter[parameterIndex] is { } defaultArgument)
                                _ = AnalyzeExpression(defaultArgument, context, selected.Method.Declaration.Parameters[parameterIndex].TypeName);
                        return structType;
                    }
                }
                Expression? instanceReceiver = null;
                string? virtualReceiverType = null;
                string? interfaceReceiverType = null;
                var isBaseCall = call.Target is MemberAccessExpression { Receiver: BaseExpression };
                var isNullConditionalCall = call.Target is MemberAccessExpression { IsNullConditional: true };
                MethodSymbol[]? candidates;
                if (call.Target is NameExpression unqualifiedCall && context.Lookup(unqualifiedCall.Name) is null &&
                    (context.Method.IsStructConstructor || context.Method.IsStructInstanceMethod || context.Method.IsClassConstructor || context.Method.IsClassInstanceMethod || context.Method.IsClassFinalizer))
                {
                    if (context.Method.Owner.Kind == "struct")
                    {
                        var instanceCandidates = _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.IsStructInstanceMethod && candidate.Owner == context.Method.Owner &&
                            candidate.Declaration.Name == unqualifiedCall.Name).ToArray();
                        var staticCandidates = _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.Owner == context.Method.Owner && candidate.Declaration.IsStatic &&
                            candidate.Declaration.Name == unqualifiedCall.Name).ToArray();
                        candidates = [.. instanceCandidates, .. staticCandidates];
                        if (instanceCandidates.Length > 0) instanceReceiver = new ThisExpression(unqualifiedCall.Location);
                    }
                    else
                    {
                        var className = GetQualifiedTypeName(context.Method.Owner);
                        var instanceCandidates = GetClassInstanceMethods(className, unqualifiedCall.Name)
                            .Where(candidate => IsMethodAccessible(candidate, context.Method.Owner)).ToArray();
                        var staticCandidates = _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.Owner == context.Method.Owner && candidate.Declaration.IsStatic &&
                            candidate.Declaration.Name == unqualifiedCall.Name).ToArray();
                        candidates = [.. instanceCandidates, .. staticCandidates];
                        if (instanceCandidates.Length > 0)
                        {
                            instanceReceiver = new ThisExpression(unqualifiedCall.Location);
                            virtualReceiverType = className;
                        }
                    }
                    if (candidates.Length == 0) candidates = ResolveCallCandidates(call, context);
                }
                else if (call.Target is MemberAccessExpression instanceCall && !IsTypeReference(instanceCall.Receiver, context))
                {
                    var instanceReceiverType = AnalyzeExpression(instanceCall.Receiver, context);
                    if (IsPointerType(instanceReceiverType))
                        instanceReceiverType = NormalizePointerMemberReceiverType(instanceReceiverType, call.IsUnsafeContext, instanceCall.Location);
                    if (instanceCall.Member == "ToString" && call.Arguments.Count == 0 &&
                        (IsStringType(instanceReceiverType) || instanceReceiverType is "bool" or "char" || IntegerTypes.Contains(instanceReceiverType) || FloatingTypes.Contains(instanceReceiverType)))
                    {
                        _structToStringCalls[call] = instanceReceiverType;
                        RecordStringification(instanceReceiverType);
                        return "string";
                    }
                    if (IsStringType(instanceReceiverType))
                    {
                        instanceReceiver = instanceCall.Receiver;
                        candidates = _methods.Concat(_genericMethodDefinitions)
                            .Where(candidate => candidate.IsClassInstanceMethod &&
                                GetQualifiedTypeName(candidate.Owner) == "System.String" &&
                                candidate.Declaration.Name == instanceCall.Member)
                            .ToArray();
                        if (candidates.Length == 0)
                        {
                            diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"String has no instance method named '{instanceCall.Member}'.", instanceCall.Location);
                            return "?";
                        }
                    }
                    else if (IsEnumType(instanceReceiverType))
                    {
                        if (instanceCall.Member == "ToString" && call.Arguments.Count == 0)
                        {
                            _enumToStringCalls[call] = instanceReceiverType;
                            RecordStringification(instanceReceiverType);
                            return "string";
                        }
                        diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Enum '{instanceReceiverType}' has no instance method named '{instanceCall.Member}'.", instanceCall.Location);
                        return "?";
                    }
                    else if (IsStructType(instanceReceiverType) || IsStructType(NormalizeBuiltinTypeQualifier(instanceReceiverType)))
                    {
                        instanceReceiver = instanceCall.Receiver;
                        var normalizedStructType = IsStructType(instanceReceiverType)
                            ? instanceReceiverType
                            : NormalizeBuiltinTypeQualifier(instanceReceiverType);
                        candidates = _methods.Concat(_genericMethodDefinitions).Where(candidate => candidate.IsStructInstanceMethod && GetQualifiedTypeName(candidate.Owner) == normalizedStructType && candidate.Declaration.Name == instanceCall.Member).ToArray();
                        if (candidates.Length == 0)
                        {
                            if (instanceCall.Member == "ToString" && call.Arguments.Count == 0 && !HasCustomStructToString(normalizedStructType))
                            {
                                _structToStringCalls[call] = normalizedStructType;
                                RecordStringification(normalizedStructType);
                                return "string";
                            }
                            diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Struct '{normalizedStructType}' has no instance method named '{instanceCall.Member}'.", instanceCall.Location);
                            return "?";
                        }
                    }
                    else if (IsNullableClassType(instanceReceiverType))
                    {
                        if (!instanceCall.IsNullConditional && !IsDefinitelyNotNull(instanceCall.Receiver, instanceReceiverType, context))
                        {
                            diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable class instance may be null; use a null check or null assertion before member access.", instanceCall.Location);
                            return "?";
                        }
                        instanceReceiver = instanceCall.Receiver;
                        var classTypeName = NullableUnderlyingType(instanceReceiverType);
                        if (IsInterfaceType(classTypeName))
                        {
                            interfaceReceiverType = classTypeName;
                            candidates = GetInterfaceInstanceMethods(classTypeName, instanceCall.Member);
                        }
                        else
                        {
                            virtualReceiverType = classTypeName;
                            candidates = GetClassInstanceMethods(classTypeName, instanceCall.Member).Where(candidate => IsMethodAccessible(candidate, context.Method.Owner)).ToArray();
                        }
                        if (candidates.Length == 0)
                        {
                            if (instanceCall.Member == "ToString" && call.Arguments.Count == 0 &&
                                (_classTypes.ContainsKey(classTypeName) || _interfaceTypes.ContainsKey(classTypeName)))
                            {
                                _classToStringCalls[call] = new(classTypeName);
                                return instanceCall.IsNullConditional ? "string?" : "string";
                            }
                            diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Reference type '{classTypeName}' has no accessible instance method named '{instanceCall.Member}'.", instanceCall.Location);
                            return "?";
                        }
                    }
                    else if (IsInterfaceType(instanceReceiverType))
                    {
                        instanceReceiver = instanceCall.Receiver;
                        interfaceReceiverType = instanceReceiverType;
                        candidates = GetInterfaceInstanceMethods(instanceReceiverType, instanceCall.Member);
                        if (candidates.Length == 0)
                        {
                            if (instanceCall.Member == "ToString" && call.Arguments.Count == 0)
                            {
                                _classToStringCalls[call] = new(instanceReceiverType);
                                return "string";
                            }
                            diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Interface '{instanceReceiverType}' has no instance method named '{instanceCall.Member}'.", instanceCall.Location);
                            return "?";
                        }
                    }
                    else if (IsClassType(instanceReceiverType))
                    {
                        instanceReceiver = instanceCall.Receiver;
                        virtualReceiverType = instanceReceiverType;
                        candidates = GetClassInstanceMethods(instanceReceiverType, instanceCall.Member).Where(candidate => IsMethodAccessible(candidate, context.Method.Owner)).ToArray();
                        if (candidates.Length == 0)
                        {
                            if (instanceCall.Member == "ToString" && call.Arguments.Count == 0)
                            {
                                _classToStringCalls[call] = new(instanceReceiverType, isBaseCall);
                                return "string";
                            }
                            diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Class '{instanceReceiverType}' has no accessible instance method named '{instanceCall.Member}'.", instanceCall.Location);
                            return "?";
                        }
                    }
                    else candidates = ResolveCallCandidates(call, context);
                }
                else candidates = ResolveCallCandidates(call, context);
                if (candidates is null) return "?";
                var args = new string[call.Arguments.Count];
                for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
                {
                    var argument = call.Arguments[sourceIndex];
                    // A lambda has no type until a delegate target is known. Let ordinary
                    // arguments first infer generic method arguments, then specialize the
                    // candidate and use its closed parameter type to bind the lambda.
                    if (argument is LambdaExpression)
                    {
                        args[sourceIndex] = "?";
                        continue;
                    }

                    if ((argument is NameExpression or MemberAccessExpression) && candidates.Length > 0)
                    {
                        var expectedTypes = new List<string>();
                        foreach (var candidate in candidates)
                        {
                            if (TryBindCallArguments(call, candidate, Enumerable.Repeat("?", call.Arguments.Count).ToArray(), out var argumentBinding) &&
                                sourceIndex < argumentBinding.ParameterBySource.Count)
                                expectedTypes.Add(NormalizeEnumType(GetParameterTypeForSource(candidate, argumentBinding, sourceIndex), candidate.Owner));
                        }
                        var distinctExpectedTypes = expectedTypes.Distinct(StringComparer.Ordinal).ToArray();
                        var expectedTypeDependsOnMethodParameter = distinctExpectedTypes.Length == 1 && candidates.Any(candidate =>
                            candidate.Declaration.TypeParameters is { Count: > 0 } methodTypeParameters &&
                            TryBindCallArguments(call, candidate, Enumerable.Repeat("?", call.Arguments.Count).ToArray(), out var candidateBinding) &&
                            GenericTypeSpecializer.ReferencesAnyTypeParameter(
                                GetParameterTypeForSource(candidate, candidateBinding, sourceIndex),
                                methodTypeParameters.ToHashSet(StringComparer.Ordinal)));
                        var potentialMethodGroup = argument switch
                        {
                            NameExpression name => IsPotentialMethodGroupForInference(name, context),
                            MemberAccessExpression member => IsPotentialMethodGroupForInference(member, context),
                            _ => false
                        };
                        if (expectedTypeDependsOnMethodParameter && potentialMethodGroup)
                        {
                            args[sourceIndex] = "?";
                            continue;
                        }
                        if (distinctExpectedTypes.Length == 1 && expectedTypes.Count == candidates.Length &&
                            !expectedTypeDependsOnMethodParameter && TryBindMethodGroup(argument, distinctExpectedTypes[0], context))
                        {
                            args[sourceIndex] = distinctExpectedTypes[0];
                            continue;
                        }
                    }

                    args[sourceIndex] = argument switch
                    {
                        InlineOutVariableExpression { TypeName: null } => "?",
                        InlineOutVariableExpression inline => NormalizeEnumType(inline.TypeName!, context.Method.Owner),
                        DefaultValueExpression { TypeName: null } => "?",
                        _ => AnalyzeExpression(argument, context)
                    };
                }
                var candidateCountBeforeGenericPreparation = candidates.Length;
                candidates = PrepareGenericCallCandidates(call, candidates, args, context);
                if (candidates.Length == 0)
                {
                    if (candidateCountBeforeGenericPreparation == 0)
                    {
                        var missingCallName = call.Target switch { NameExpression n => n.Name, MemberAccessExpression m => m.Member, _ => "<expression>" };
                        diagnostics.Add("LS0751", DiagnosticSeverity.Error, $"No applicable overload for '{missingCallName}'.", call.Location);
                    }
                    return "?";
                }
                for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
                {
                    if (call.Arguments[sourceIndex] is not LambdaExpression lambda) continue;
                    var expectedTypes = new List<string>();
                    foreach (var candidate in candidates)
                    {
                        if (TryBindCallArguments(call, candidate, args, out var argumentBinding) &&
                            sourceIndex < argumentBinding.ParameterBySource.Count)
                            expectedTypes.Add(NormalizeEnumType(GetParameterTypeForSource(candidate, argumentBinding, sourceIndex), candidate.Owner));
                    }
                    var distinctExpectedTypes = expectedTypes.Distinct(StringComparer.Ordinal).ToArray();
                    args[sourceIndex] = distinctExpectedTypes.Length == 1 && expectedTypes.Count == candidates.Length
                        ? AnalyzeExpression(lambda, context, distinctExpectedTypes[0])
                        : AnalyzeExpression(lambda, context);
                }
                for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
                {
                    if (args[sourceIndex] != "?") continue;
                    var argument = call.Arguments[sourceIndex];
                    if (argument is not (NameExpression or MemberAccessExpression)) continue;
                    var expectedTypes = new List<string>();
                    foreach (var candidate in candidates)
                    {
                        if (TryBindCallArguments(call, candidate, args, out var argumentBinding) &&
                            sourceIndex < argumentBinding.ParameterBySource.Count)
                            expectedTypes.Add(NormalizeEnumType(GetParameterTypeForSource(candidate, argumentBinding, sourceIndex), candidate.Owner));
                    }
                    var distinctExpectedTypes = expectedTypes.Distinct(StringComparer.Ordinal).ToArray();
                    if (distinctExpectedTypes.Length == 1 && expectedTypes.Count == candidates.Length &&
                        TryBindMethodGroup(argument, distinctExpectedTypes[0], context))
                        args[sourceIndex] = distinctExpectedTypes[0];
                }
                var applicable = new List<(MethodSymbol Method, BoundCallArguments Binding)>();
                foreach (var candidate in candidates)
                {
                    if (!TryBindCallArguments(call, candidate, args, out var binding)) continue;
                    var converts = true;
                    for (var index = 0; index < args.Length; index++)
                    {
                        var parameter = candidate.Declaration.Parameters[binding.ParameterBySource[index]];
                        var parameterType = GetParameterTypeForSource(candidate, binding, index);
                        var argumentMode = call.ArgumentModes is not null && index < call.ArgumentModes.Count ? call.ArgumentModes[index] ?? "" : "";
                        if (call.Arguments[index] is InlineOutVariableExpression inlineOut)
                        {
                            var declaredType = inlineOut.TypeName is null ? parameterType : args[index];
                            if (argumentMode != "out" || parameter.PassingMode != "out" || declaredType != parameterType) { converts = false; break; }
                            continue;
                        }
                        if (parameter.PassingMode is "ref" or "out")
                        {
                            if (argumentMode != parameter.PassingMode || args[index] != parameterType || !IsMutableVariableArgument(call.Arguments[index], context)) { converts = false; break; }
                            continue;
                        }
                        if (argumentMode is "ref" or "out" || parameter.PassingMode == "in" && argumentMode is not ("" or "in") || parameter.PassingMode != "in" && argumentMode == "in")
                        { converts = false; break; }
                        if (args[index] == "?" && call.Arguments[index] is DefaultValueExpression { TypeName: null }) continue;
                        if (!CanConvertInContext(args[index], parameterType, call.Arguments[index], context)) { converts = false; break; }
                    }
                    if (converts) applicable.Add((candidate, binding));
                }
                var callName = call.Target switch { NameExpression n => n.Name, MemberAccessExpression m => m.Member, _ => "<expression>" };
                if (applicable.Count == 0) { diagnostics.Add("LS0751", DiagnosticSeverity.Error, $"No applicable overload for '{callName}'.", call.Location); return "?"; }
                var best = applicable.Where(candidate => applicable.All(other => ReferenceEquals(candidate.Method, other.Method) || CompareCandidates(candidate.Method, other.Method, args, call.Arguments, candidate.Binding, other.Binding) <= 0)).ToArray();
                if (best.Length > 1)
                {
                    var minDefaults = best.Min(candidate => candidate.Binding.DefaultsByParameter.Count(value => value is not null));
                    best = best.Where(candidate => candidate.Binding.DefaultsByParameter.Count(value => value is not null) == minDefaults).ToArray();
                }
                if (best.Length > 1)
                {
                    var minVariadicPenalty = best.Min(candidate => VariadicBindingPenalty(candidate.Binding));
                    best = best.Where(candidate => VariadicBindingPenalty(candidate.Binding) == minVariadicPenalty).ToArray();
                }
                if (best.Length != 1) { diagnostics.Add("LS0750", DiagnosticSeverity.Error, $"Call to '{callName}' is ambiguous.", call.Location); return "?"; }
                if (best[0].Method.Declaration.IsExtern && !call.IsUnsafeContext)
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Calling extern method '{best[0].Method.Declaration.Name}' requires an unsafe context.", call.Location);
                if (best[0].Method.IsStructInstanceMethod && instanceReceiver is not null && best[0].Method.Declaration.IsMutating && !IsMutableStructLocation(instanceReceiver, context))
                {
                    if (context.Method.IsStructInstanceMethod && !context.Method.Declaration.IsMutating && HasThisRoot(instanceReceiver))
                        diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A non-mutating struct method cannot call a mutating method on its receiver.", call.Location);
                    else diagnostics.Add("LS0823", DiagnosticSeverity.Error, "A mutating struct method requires a mutable variable receiver.", call.Location);
                }
                if (best[0].Method.IsClassInstanceMethod && instanceReceiver is not null && best[0].Method.Declaration.IsMutating &&
                    (HasThisRoot(instanceReceiver) || IsCapturedThisRoot(instanceReceiver, context)) && !CanMutateClassReceiver(context))
                    diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A non-mutating class method cannot call a mutating method on its receiver.", call.Location);
                if (isBaseCall && best[0].Method.Declaration.Body is null)
                    diagnostics.Add("LS0836", DiagnosticSeverity.Error, $"Abstract base method '{best[0].Method.QualifiedName}' has no implementation to call.", call.Location);
                _callTargets[call] = best[0].Method; _callArguments[call] = best[0].Binding;
                if (interfaceReceiverType is not null)
                {
                    var slot = FindInterfaceMethodSlot(best[0].Method);
                    if (slot < 0)
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Interface method '{best[0].Method.QualifiedName}' has no supported dispatch slot.", call.Location);
                    else _interfaceCallSlots[call] = slot;
                }
                for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
                {
                    var parameter = best[0].Method.Declaration.Parameters[best[0].Binding.ParameterBySource[sourceIndex]];
                    if ((GetScopeBoundDepth(call.Arguments[sourceIndex], context) is not null || IsReadOnlyReferenceExpression(call.Arguments[sourceIndex], context)) &&
                        parameter.PassingMode != "in" && !parameter.IsReadOnly && !parameter.IsScoped)
                        diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A scope-bound reference may only be passed to a scope or in parameter.", call.Arguments[sourceIndex].Location);
                }
                if (!isBaseCall && virtualReceiverType is not null && best[0].Method.IsClassInstanceMethod &&
                    (best[0].Method.Declaration.IsVirtual || best[0].Method.Declaration.IsOverride))
                {
                    var slot = VirtualDispatchLayout.FindSlot(virtualReceiverType, best[0].Method, _classTypes, _methods);
                    if (slot >= 0) _virtualCallSlots[call] = slot;
                }
                var receiverIsScopeBound = instanceReceiver is not null && GetScopeBoundDepth(instanceReceiver, context) is not null;
                var receiverIsReadOnly = instanceReceiver is not null && IsReadOnlyReferenceExpression(instanceReceiver, context);
                if ((receiverIsScopeBound || receiverIsReadOnly) && (best[0].Method.IsClassInstanceMethod || interfaceReceiverType is not null))
                {
                    var hasEscapingTarget = false;
                    var hasOpenTargetSet = false;
                    var openTargetReason = "";
                    if (interfaceReceiverType is not null)
                    {
                        if (!_interfaceCallSlots.TryGetValue(call, out var interfaceSlot))
                        {
                            hasOpenTargetSet = true;
                            openTargetReason = "no interface dispatch slot was resolved";
                        }
                        else
                        {
                            var possibleImplementations = _interfaceDispatchTables
                                .Where(entry => interfaceSlot < entry.Value.Count && entry.Value[interfaceSlot] is not null)
                                .Select(entry => entry.Key)
                                .ToArray();
                            if (possibleImplementations.Length == 0)
                            {
                                hasOpenTargetSet = true;
                                openTargetReason = $"no implementations of '{interfaceReceiverType}' are known";
                            }
                            foreach (var possibleType in possibleImplementations)
                            {
                                if (_classTypes.TryGetValue(possibleType, out var implementationClass) && implementationClass.Declaration.IsVirtual)
                                {
                                    hasOpenTargetSet = true;
                                    openTargetReason = $"'{possibleType}' is an extensible implementation";
                                }
                                var implementation = _interfaceDispatchTables[possibleType][interfaceSlot]!;
                                if (MethodLetsThisEscape(implementation, possibleType)) hasEscapingTarget = true;
                            }
                        }
                    }
                    else
                    {
                        var possibleTargets = new HashSet<MethodSymbol> { best[0].Method };
                        if (virtualReceiverType is not null && _virtualCallSlots.TryGetValue(call, out var virtualSlot))
                        {
                            foreach (var possibleType in _classTypes.Keys.Where(candidate => candidate == virtualReceiverType || IsBaseClassOf(virtualReceiverType, candidate)))
                            {
                                var table = VirtualDispatchLayout.GetMethods(possibleType, _classTypes, _methods);
                                if (virtualSlot < table.Count) possibleTargets.Add(table[virtualSlot]);
                            }
                            if (_classTypes.TryGetValue(virtualReceiverType, out var receiverClass) && receiverClass.Declaration.IsVirtual)
                                hasOpenTargetSet = true;
                        }
                        hasEscapingTarget = possibleTargets.Any(target => MethodLetsThisEscape(target, virtualReceiverType ?? GetQualifiedTypeName(target.Owner)));
                    }
                    if (hasOpenTargetSet || hasEscapingTarget)
                        diagnostics.Add(receiverIsScopeBound ? "LS0702" : "LS0701", DiagnosticSeverity.Error,
                            hasOpenTargetSet
                                ? $"A virtual or interface call on a borrowed receiver has an open set of possible targets and cannot be proven non-escaping ({openTargetReason})."
                                : $"Method '{best[0].Method.Declaration.Name}' may let its receiver escape and cannot be called on this borrowed object.",
                            call.Location);
                }
                for (var index = 0; index < call.Arguments.Count; index++)
                {
                    if (call.Arguments[index] is InlineOutVariableExpression inlineOut)
                    {
                        var parameter = best[0].Method.Declaration.Parameters[best[0].Binding.ParameterBySource[index]];
                        var inlineType = inlineOut.TypeName is null ? parameter.TypeName : args[index];
                        if (!IsSupportedType(inlineType)) ValidateDeclaredType(inlineType, "Inline out local", inlineOut.Location, context.Method.Owner);
                        if (context.Exists(inlineOut.Name)) diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Inline out local '{inlineOut.Name}' conflicts with an existing local.", inlineOut.Location);
                        else context.Declare(inlineOut.Name, inlineType, true, inlineOut.Location,
                            forHeaderOwner: context.ForInitializerOwner,
                            blockOwner: context.ForInitializerOwner is null ? context.CurrentCaptureBlock : null);
                        continue;
                    }
                    if (call.Arguments[index] is DefaultValueExpression { TypeName: null } targetTypedDefault)
                        _ = AnalyzeExpression(targetTypedDefault, context, best[0].Method.Declaration.Parameters[best[0].Binding.ParameterBySource[index]].TypeName);
                }
                var callReturnType = best[0].Method.Declaration.ReturnType;
                context.InvalidateMemberNullStates();
                if (isNullConditionalCall && callReturnType != "void")
                {
                    if (IsClassReferenceType(callReturnType)) return NullableUnderlyingType(callReturnType) + "?";
                    if (IsDelegateType(callReturnType) || IsNullableDelegateType(callReturnType)) return NullableUnderlyingType(callReturnType) + "?";
                    if (IsStringType(callReturnType)) return "string?";
                    if (IsNullableValueType(callReturnType)) return callReturnType;
                    if (IsNullableValuePayloadType(callReturnType)) return callReturnType + "?";
                    if (IsManagedArrayReferenceType(callReturnType)) return IsNullableArrayType(callReturnType) ? callReturnType : callReturnType + "?";
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Null-conditional call results of type '{callReturnType}' are not supported.", call.Location);
                    return "?";
                }
                return callReturnType;
            case OverflowContextExpression overflow:
                var priorChecked = _checkedContext; _checkedContext = overflow.Checked;
                try { return AnalyzeExpression(overflow.Value, context, expectedType); } finally { _checkedContext = priorChecked; }
            case DefaultValueExpression defaultValue:
                var defaultType = NormalizeEnumType(defaultValue.TypeName ?? expectedType ?? "?", context.Method.Owner);
                if (defaultType == "?") defaultType = null;
                if (defaultType is null) { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "The default expression requires a target type.", defaultValue.Location); return "?"; }
                if (defaultValue.TypeName is not null && ValidateTypeAccessibility(defaultType, context.Method.Owner, defaultValue.Location)) return "?";
                if (!IsSupportedType(defaultType)) { diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Default values for '{defaultType}' are not yet supported.", defaultValue.Location); return "?"; }
                if (TryParseManagedArrayType(defaultType, out _, out _)) diagnostics.Add("LS0810", DiagnosticSeverity.Error, $"Managed array '{defaultType}' has no default value; initialize it explicitly.", defaultValue.Location);
                if (!defaultValue.IsTypeParameterDefault && (IsClassType(defaultType) || defaultType == "string")) diagnostics.Add("LS0810", DiagnosticSeverity.Error, $"Non-nullable reference type '{defaultType}' has no default value.", defaultValue.Location);
                _defaultTypes[defaultValue] = defaultType;
                return defaultType;
            case SizeOfExpression sizeOf:
                var sizeType = NormalizeEnumType(sizeOf.TypeName, context.Method.Owner);
                if (ValidateTypeAccessibility(sizeType, context.Method.Owner, sizeOf.Location)) return "int32";
                _sizeOfTypes[sizeOf] = sizeType;
                if (!IsSupportedType(sizeType) || sizeOf.TypeName == "void")
                { diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"sizeof is not supported for type '{sizeOf.TypeName}'.", sizeOf.Location); return "int32"; }
                return "int32";
            case InlineArrayInitializerExpression initializer:
                var normalizedInitializerType = expectedType is null ? null : NormalizeEnumType(expectedType, context.Method.Owner);
                if (normalizedInitializerType is not null && TryParseManagedArrayType(normalizedInitializerType, out var managedInitializerElementType, out var managedInitializerRank))
                {
                    if (managedInitializerRank > 3) diagnostics.Add("LS1503", DiagnosticSeverity.Error, "Rectangular arrays may have at most three dimensions.", initializer.Location);
                    if (!IsManagedArrayElementType(managedInitializerElementType))
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Collection initializer for managed array type '{normalizedInitializerType}' is not yet supported by the current backend.", initializer.Location);
                    _ = AnalyzeManagedArrayInitializer(initializer, managedInitializerElementType, managedInitializerRank, context);
                    return normalizedInitializerType;
                }
                if (normalizedInitializerType is null || !TryParseInlineArrayType(normalizedInitializerType, out var initializerElementType, out var initializerLength))
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "An inline array initializer requires a declared inline array type.", initializer.Location);
                    return "?";
                }
                if (initializer.Elements.Count > initializerLength)
                    diagnostics.Add("LS1504", DiagnosticSeverity.Error, $"Inline array initializer has {initializer.Elements.Count} elements but the array length is {initializerLength}.", initializer.Location);
                foreach (var element in initializer.Elements)
                {
                    var elementType = AnalyzeExpression(element, context, initializerElementType);
                    if (!CanConvert(elementType, initializerElementType, element))
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert inline array element from '{elementType}' to '{initializerElementType}'.", element.Location);
                }
                return normalizedInitializerType;
            case AllocationExpression allocation:
                if (allocation.IsScope && !_scopeAllocationOrders.ContainsKey(allocation))
                    _scopeAllocationOrders.Add(allocation, context.NextScopeAllocationOrder());
                if (allocation.TypeName is not null && ValidateTypeAccessibility(allocation.TypeName, context.Method.Owner, allocation.Location))
                {
                    foreach (var argument in allocation.ConstructorArguments ?? []) _ = AnalyzeExpression(argument, context);
                    foreach (var dimension in allocation.Dimensions) _ = AnalyzeExpression(dimension, context);
                    if (allocation.Initializer is { } inaccessibleInitializer)
                        foreach (var element in inaccessibleInitializer.Elements) _ = AnalyzeExpression(element, context);
                    return "?";
                }
                var allocationType = allocation.TypeName is null
                    ? expectedType is null ? null : NormalizeEnumType(expectedType, context.Method.Owner)
                    : NormalizeEnumType(allocation.TypeName, context.Method.Owner);
                var normalizedExpectedType = expectedType is null ? null : NormalizeEnumType(expectedType, context.Method.Owner);
                if (allocation.TypeName is null && allocationType is null)
                {
                    diagnostics.Add("LS0802", DiagnosticSeverity.Error, "A target-typed allocation requires an expected type in this context.", allocation.Location);
                    foreach (var argument in allocation.ConstructorArguments ?? []) _ = AnalyzeExpression(argument, context);
                    foreach (var dimension in allocation.Dimensions) _ = AnalyzeExpression(dimension, context);
                    if (allocation.Initializer is { } untypedInitializer)
                        foreach (var element in untypedInitializer.Elements) _ = AnalyzeExpression(element, context);
                    return "?";
                }
                if (allocation.Initializer is not null && normalizedExpectedType is not null &&
                    TryParseInlineArrayType(normalizedExpectedType, out var expectedInlineElementType, out _) &&
                    (allocationType is null || TryParseManagedArrayType(allocationType, out var allocatedArrayElementType, out _) && allocatedArrayElementType == expectedInlineElementType))
                {
                    diagnostics.Add("LS1505", DiagnosticSeverity.Error, "An inline array cannot be created with an allocation expression; use a direct collection initializer.", allocation.Location);
                    foreach (var element in allocation.Initializer.Elements)
                    {
                        var elementType = AnalyzeExpression(element, context, expectedInlineElementType);
                        if (!CanConvert(elementType, expectedInlineElementType, element))
                            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert inline array element from '{elementType}' to '{expectedInlineElementType}'.", element.Location);
                    }
                    return normalizedExpectedType;
                }
                if (allocationType == "System.Array")
                {
                    diagnostics.Add("LS0834", DiagnosticSeverity.Error, "The compiler-known System.Array base cannot be instantiated directly; allocate a concrete array type instead.", allocation.Location);
                    foreach (var argument in allocation.ConstructorArguments ?? []) _ = AnalyzeExpression(argument, context);
                    foreach (var dimension in allocation.Dimensions) _ = AnalyzeExpression(dimension, context);
                    return "?";
                }
                if (allocationType is "string" or "System.String")
                {
                    var constructorArguments = allocation.ConstructorArguments ?? [];
                    var factoryName = constructorArguments.Count == 2 &&
                        constructorArguments[0] is not InlineOutVariableExpression &&
                        AnalyzeExpression(constructorArguments[0], context) == "char"
                            ? "Repeat"
                            : "FromChars";
                    var factoryCall = new CallExpression(
                        new MemberAccessExpression(new NameExpression("System.String", allocation.Location), factoryName, allocation.Location),
                        constructorArguments,
                        allocation.Location,
                        allocation.ConstructorArgumentNames,
                        allocation.ConstructorArgumentModes);
                    var constructedStringType = AnalyzeExpression(factoryCall, context);
                    if (constructedStringType != "string")
                        return "?";
                    _classAllocations[allocation] = new("System.String", factoryCall);
                    return "string";
                }
                if (allocationType is not null && TryParseManagedArrayType(allocationType, out var managedElementType, out var managedRank))
                {
                    if (allocation.IsScope && _isAnalyzingFieldInitializer)
                        diagnostics.Add("LS0803", DiagnosticSeverity.Error, "A scope allocation requires an enclosing method or block body and cannot appear in a field initializer.", allocation.Location);
                    _managedArrayAllocations[allocation] = allocationType;
                    if (managedRank > 3) diagnostics.Add("LS1503", DiagnosticSeverity.Error, "Rectangular arrays may have at most three dimensions.", allocation.Location);
                    if (!IsManagedArrayElementType(managedElementType))
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Managed array type '{allocationType}' is recognized but not yet supported by the current backend.", allocation.Location);
                    if (allocation.Dimensions.Count > 0 && allocation.Dimensions.Count != managedRank)
                        diagnostics.Add("LS1503", DiagnosticSeverity.Error, $"Array type '{allocationType}' has rank {managedRank}, but {allocation.Dimensions.Count} dimension lengths were provided.", allocation.Location);
                    foreach (var dimension in allocation.Dimensions)
                    {
                        var dimensionType = AnalyzeExpression(dimension, context);
                        if (!CanConvert(dimensionType, "int32", dimension)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"An array length of type '{dimensionType}' cannot be converted to int32.", dimension.Location);
                    }
                    if (allocation.Dimensions.Count == 0 && allocation.Initializer is null)
                        diagnostics.Add("LS1501", DiagnosticSeverity.Error, "An array creation without a dimension length requires an initializer.", allocation.Location);
                    if (allocation.Initializer is { } managedInitializer)
                    {
                        var shape = AnalyzeManagedArrayInitializer(managedInitializer, managedElementType, managedRank, context,
                            allowScopeBoundElements: allocation.IsScope);
                        if (allocation.Dimensions.Count == managedRank)
                        {
                            for (var dimensionIndex = 0; dimensionIndex < managedRank; dimensionIndex++)
                                if (allocation.Dimensions[dimensionIndex] is LiteralExpression dimensionLiteral && ParseInteger(dimensionLiteral.Value).Value is { } constantLength && constantLength >= 0 && constantLength != shape[dimensionIndex])
                                    diagnostics.Add("LS1504", DiagnosticSeverity.Error, $"Array dimension {dimensionIndex} has length {constantLength}, but its initializer has length {shape[dimensionIndex]}.", managedInitializer.Location);
                        }
                    }
                    else if (IsNonNullableManagedArrayElementReference(managedElementType) &&
                        !allocation.Dimensions.Any(dimension => dimension is LiteralExpression literal && ParseInteger(literal.Value).Value == 0))
                        diagnostics.Add("LS1501", DiagnosticSeverity.Error, "An array of non-nullable reference elements requires an initializer that supplies every element.", allocation.Location);
                    return allocationType;
                }
                if (allocation.IsTypeParameterConstruction && allocationType is not null && IsGenericValueType(allocationType))
                {
                    if (allocation.IsScope)
                    {
                        diagnostics.Add("LS0801", DiagnosticSeverity.Error, $"'scope' cannot be applied to value type '{allocationType}'.", allocation.Location);
                        return allocationType;
                    }
                    var constructorArguments = allocation.ConstructorArguments ?? [];
                    if (constructorArguments.Count == 0)
                    {
                        _genericConstructions[allocation] = new(allocationType);
                        return allocationType;
                    }
                    if (IsStructType(allocationType))
                    {
                        var constructorCall = new CallExpression(new NameExpression(allocationType, allocation.Location), constructorArguments,
                            allocation.Location, allocation.ConstructorArgumentNames, allocation.ConstructorArgumentModes);
                        var constructedType = AnalyzeExpression(constructorCall, context);
                        if (constructedType == allocationType)
                        {
                            _genericConstructions[allocation] = new(allocationType, constructorCall);
                            return allocationType;
                        }
                        return "?";
                    }
                    foreach (var argument in constructorArguments) _ = AnalyzeExpression(argument, context);
                    diagnostics.Add("LS0751", DiagnosticSeverity.Error,
                        $"Value type parameter construction for '{allocationType}' does not define a matching constructor.", allocation.Location);
                    return "?";
                }
                if (allocationType is not null && IsClassType(allocationType))
                {
                    if (_classTypes[allocationType].Declaration.IsStatic)
                    {
                        diagnostics.Add("LS0001", DiagnosticSeverity.Error, $"Static class '{allocationType}' cannot be instantiated.", allocation.Location);
                        return "?";
                    }
                    if (allocation.IsScope && _isAnalyzingFieldInitializer)
                        diagnostics.Add("LS0803", DiagnosticSeverity.Error, "A scope allocation requires an enclosing method or block body and cannot appear in a field initializer.", allocation.Location);
                    if (HasUnimplementedVirtualMembers(allocationType))
                        diagnostics.Add("LS0834", DiagnosticSeverity.Error, $"Abstract class '{allocationType}' cannot be instantiated.", allocation.Location);
                    if (allocation.Dimensions.Count != 0) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Class allocation does not accept array dimensions.", allocation.Location);
                    if (allocation.Initializer is not null)
                    {
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Class object initializers are not yet supported.", allocation.Initializer.Location);
                        foreach (var element in allocation.Initializer.Elements) _ = AnalyzeExpression(element, context);
                    }
                    var arguments = allocation.ConstructorArguments ?? [];
                    var call = new CallExpression(new NameExpression(allocationType, allocation.Location), arguments, allocation.Location,
                        allocation.ConstructorArgumentNames, allocation.ConstructorArgumentModes);
                    var constructors = _methods.Where(candidate => candidate.IsClassConstructor && GetQualifiedTypeName(candidate.Owner) == allocationType &&
                        IsConstructorAccessible(candidate, context.Method.Owner)).ToArray();
                    if (SelectConstructor(call, constructors, context, allocationType, scopedOwner: allocation) is null) return "?";
                    _classAllocations[allocation] = new(allocationType, call);
                    return allocationType;
                }
                if (allocationType is not null && IsStructType(allocationType))
                {
                    diagnostics.Add("LS0801", DiagnosticSeverity.Error, allocation.IsScope
                        ? $"'scope' cannot be applied to struct '{allocationType}'; structs are value types."
                        : $"'new' cannot be applied to struct '{allocationType}'; use '{allocationType}()' or default.", allocation.Location);
                    if (allocation.Initializer is not null)
                        foreach (var element in allocation.Initializer.Elements) _ = AnalyzeExpression(element, context);
                    return allocationType;
                }
                var allocationElementType = "";
                var inlineAllocation = allocationType is not null && TryParseInlineArrayType(allocationType, out allocationElementType, out _);
                if (allocation.Initializer is { } allocationInitializer)
                {
                    if (inlineAllocation)
                        diagnostics.Add("LS1505", DiagnosticSeverity.Error, "An inline array cannot be created with an allocation expression; use a direct collection initializer.", allocation.Location);
                    else
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Allocated collection initializers are not yet supported by the current backend.", allocation.Location);
                    foreach (var element in allocationInitializer.Elements)
                        _ = AnalyzeExpression(element, context, inlineAllocation ? allocationElementType : null);
                }
                else
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, allocation.IsScope ? "Scoped allocation expressions are not yet supported by the current backend." : "Heap allocation expressions are not yet supported by the current backend.", allocation.Location);
                return allocationType ?? "?";
            case IndexExpression index:
                var receiverType = AnalyzeExpression(index.Receiver, context);
                var indexTypes = index.Indices.Select(item => AnalyzeExpression(item, context)).ToArray();
                if (IsPointerType(receiverType) &&
                    (IsClassType(receiverType[..^1]) || IsStructType(receiverType[..^1])))
                    receiverType = NormalizePointerMemberReceiverType(receiverType, index.IsUnsafeContext, index.Location);
                if (receiverType is "string" or "string?")
                {
                    if (receiverType == "string?" && !index.IsNullConditional && !IsDefinitelyNotNull(index.Receiver, receiverType, context))
                    {
                        diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable string instance may be null; use a null check or null assertion before indexing.", index.Location);
                        return "?";
                    }
                    if (index.Indices.Count != 1)
                    { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "String indexing accepts exactly one index.", index.Location); return "?"; }
                    if (indexTypes.Length > 0 && !CanConvert(indexTypes[0], "int32", index.Indices[0]))
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A string index must be convertible to int32.", index.Indices[0].Location);
                    _stringIndexAccesses[index] = true;
                    return index.IsNullConditional ? "char?" : "char";
                }
                var nullableManagedArray = IsNullableArrayType(receiverType);
                if (nullableManagedArray && !index.IsNullConditional && !IsDefinitelyNotNull(index.Receiver, receiverType, context))
                {
                    diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable array instance may be null; use a null check or null assertion before indexing.", index.Location);
                    return "?";
                }
                if (nullableManagedArray) receiverType = NullableUnderlyingType(receiverType);
                if (TryParseManagedArrayType(receiverType, out var managedIndexedElementType, out var managedArrayRank))
                {
                    _managedArrayIndexAccesses[index] = new(managedIndexedElementType, managedArrayRank);
                    if (managedArrayRank > 3) diagnostics.Add("LS1503", DiagnosticSeverity.Error, "Rectangular arrays may have at most three dimensions.", index.Location);
                    if (index.Indices.Count != managedArrayRank) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Array type '{receiverType}' requires {managedArrayRank} index value(s).", index.Location);
                    foreach (var item in index.Indices)
                        if (!CanConvert(AnalyzeExpression(item, context), "int32", item)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "An array index must be convertible to int32.", item.Location);
                    if (!index.IsNullConditional) return managedIndexedElementType;
                    if (IsStringType(managedIndexedElementType)) return "string?";
                    if (IsNullableType(managedIndexedElementType)) return managedIndexedElementType;
                    if (IsNullableValuePayloadType(managedIndexedElementType) || IsEnumType(managedIndexedElementType)) return managedIndexedElementType + "?";
                    if (IsManagedArrayReferenceType(managedIndexedElementType) || IsClassReferenceType(managedIndexedElementType) || IsDelegateType(managedIndexedElementType))
                        return managedIndexedElementType + "?";
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Null-conditional indexing of array element type '{managedIndexedElementType}' is not yet supported.", index.Location);
                    return "?";
                }
                var nullableIndexerReceiver = IsNullableClassType(receiverType);
                if (nullableIndexerReceiver && !index.IsNullConditional && !IsDefinitelyNotNull(index.Receiver, receiverType, context))
                {
                    diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable class instance may be null; use a null check or null assertion before indexing.", index.Location);
                    return "?";
                }
                if (nullableIndexerReceiver) receiverType = NullableUnderlyingType(receiverType);
                if (IsStructType(receiverType) || IsClassType(receiverType) || IsInterfaceType(receiverType))
                {
                    var indexers = GetComputedIndexers(receiverType);
                    if (indexers.Count > 0)
                    {
                        if (index.IsNullConditional && IsStructType(receiverType))
                            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Null-conditional indexing requires a class receiver.", index.Location);

                        if (ReferenceEquals(index, _propertyAssignmentProbe))
                        {
                            var setOnly = SelectComputedIndexer(indexers.Where(candidate => candidate.Getter is null && candidate.Setter is not null).ToArray(), indexTypes, index.Indices, context, index.Location, reportAmbiguity: false);
                            if (setOnly is not null)
                            {
                                _boundComputedIndexers[index] = setOnly;
                                return setOnly.TypeName;
                            }
                        }

                        var getterCandidates = indexers.Where(candidate => candidate.Getter is not null).ToArray();
                        if (getterCandidates.Length == 0)
                        {
                            var matchingIndexer = SelectComputedIndexer(indexers, indexTypes, index.Indices, context, index.Location);
                            if (matchingIndexer is not null)
                            {
                                _boundComputedIndexers[index] = matchingIndexer;
                                if (!ReferenceEquals(index, _propertyAssignmentProbe))
                                    diagnostics.Add("LS0200", DiagnosticSeverity.Error, "The matching indexer has no get accessor.", index.Location);
                                return matchingIndexer.TypeName;
                            }
                        }

                        var indexerGetter = new CallExpression(
                            new MemberAccessExpression(index.Receiver, "$indexer_get", index.Location, index.IsNullConditional, index.IsUnsafeContext),
                            index.Indices, index.Location, IsUnsafeContext: index.IsUnsafeContext);
                        var getterType = AnalyzeExpression(indexerGetter, context);
                        if (_callTargets.TryGetValue(indexerGetter, out var selectedGetter))
                        {
                            var selectedGetterIdentity = InterfaceMethodIdentity(selectedGetter);
                            var selectedIndexer = indexers.FirstOrDefault(candidate => candidate.Getter is { } getter &&
                                InterfaceMethodIdentity(getter) == selectedGetterIdentity);
                            if (selectedIndexer is not null) _boundComputedIndexers[index] = selectedIndexer;
                            _indexerGetterCalls[index] = indexerGetter;
                        }
                        else if (ReferenceEquals(index, _propertyAssignmentProbe))
                        {
                            var setIndexer = SelectComputedIndexer(indexers.Where(candidate => candidate.Setter is not null).ToArray(), indexTypes, index.Indices, context, index.Location, reportAmbiguity: false);
                            if (setIndexer is not null) _boundComputedIndexers[index] = setIndexer;
                        }
                        return getterType;
                    }
                }
                var isInlineArray = TryParseInlineArrayType(receiverType, out var indexedElementType, out _);
                if (!isInlineArray && !IsPointerType(receiverType))
                {
                    if (receiverType != "?") diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Array and indexer access are not yet supported by the current backend.", index.Location);
                    return "?";
                }
                if (!UnsafeEnabledAt(index.Location)) diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Pointer and inline array indexing require unsafe code, but unsafe is disabled for this compilation.", index.Location);
                else if (!index.IsUnsafeContext) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Pointer and inline array indexing may only be used in an unsafe context.", index.Location);
                if (index.Indices.Count != 1)
                { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Pointer indexing accepts exactly one index.", index.Location); return "?"; }
                if (indexTypes.Length > 0 && !(IntegerTypes.Contains(indexTypes[0]) || indexTypes[0] == "char"))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A pointer index must have an integral type.", index.Indices[0].Location);
                var indexedType = isInlineArray ? indexedElementType : receiverType[..^1];
                if (indexedType == "void") { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A void pointer cannot be indexed; convert it to a typed pointer first.", index.Location); return "?"; }
                return indexedType;
            case LambdaExpression lambda:
                foreach (var parameter in lambda.Parameters)
                    if (context.Exists(parameter.Name))
                        diagnostics.Add("LS0201", DiagnosticSeverity.Error,
                            $"Lambda parameter '{parameter.Name}' conflicts with a name in an enclosing scope.", parameter.Location);
                foreach (var parameter in context.Method.Declaration.Parameters)
                {
                    if (parameter.PassingMode is not ("ref" or "in" or "out") ||
                        lambda.Parameters.Any(lambdaParameter => lambdaParameter.Name == parameter.Name))
                        continue;
                    if (LambdaBodyReferencesName(lambda, parameter.Name))
                        diagnostics.Add("LS0730", DiagnosticSeverity.Error,
                            $"A lambda cannot capture the '{parameter.PassingMode}' parameter '{parameter.Name}'.", lambda.Location);
                }
                if (expectedType is null or "?")
                {
                    diagnostics.Add("LS1701", DiagnosticSeverity.Error,
                        "A lambda expression requires a target delegate type.", lambda.Location);
                    return "?";
                }
                var lambdaDelegateName = NullableUnderlyingType(NormalizeEnumType(expectedType, context.Method.Owner));
                if (!_delegateTypes.TryGetValue(lambdaDelegateName, out var lambdaDelegate) || lambdaDelegate.DelegateReturnType is null)
                {
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"A lambda requires a delegate target, but '{expectedType}' is not a supported delegate type.", lambda.Location);
                    return expectedType;
                }
                if (_delegateMethodGroups.ContainsKey(lambda)) return lambdaDelegateName;
                var delegateParameters = (lambdaDelegate.DelegateParameters ?? []).Select(parameter => parameter with
                {
                    TypeName = NormalizeEnumType(parameter.TypeName, lambdaDelegate)
                }).ToArray();
                if (lambda.Parameters.Count != delegateParameters.Length)
                    diagnostics.Add("LS0751", DiagnosticSeverity.Error, $"Delegate '{lambdaDelegateName}' requires {delegateParameters.Length} lambda parameter(s), but received {lambda.Parameters.Count}.", lambda.Location);
                var generatedParameters = new List<ParameterDeclaration>();
                for (var parameterIndex = 0; parameterIndex < Math.Min(lambda.Parameters.Count, delegateParameters.Length); parameterIndex++)
                {
                    var sourceParameter = lambda.Parameters[parameterIndex];
                    var targetParameter = delegateParameters[parameterIndex];
                    var actualType = sourceParameter.TypeName is null ? targetParameter.TypeName : NormalizeEnumType(sourceParameter.TypeName, context.Method.Owner);
                    if (sourceParameter.TypeName is not null && actualType != targetParameter.TypeName)
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Lambda parameter '{sourceParameter.Name}' has type '{actualType}', but delegate parameter '{targetParameter.Name}' requires '{targetParameter.TypeName}'.", sourceParameter.Location);
                    generatedParameters.Add(new(actualType, sourceParameter.Name, sourceParameter.Location,
                        PassingMode: targetParameter.PassingMode, IsReadOnly: targetParameter.IsReadOnly,
                        IsScoped: targetParameter.IsScoped));
                }
                var capturedNames = FindCapturedLambdaVariables(lambda, context);
                var scopeBoundCaptures = capturedNames.Where(name => context.Lookup(name) is { } local &&
                    (local.ScopeBoundDepth is not null || local.IsReadOnlyReference)).ToArray();
                var unsupportedCapture = false;
                foreach (var capturedName in capturedNames)
                {
                    var capturedLocal = context.Lookup(capturedName)!;
                    var parameter = context.Method.Declaration.Parameters.FirstOrDefault(candidate => candidate.Name == capturedName);
                    if (parameter is { PassingMode.Length: > 0 })
                    {
                        diagnostics.Add("LS0730", DiagnosticSeverity.Error, $"Lambda cannot capture {parameter.PassingMode} parameter '{capturedName}'.", lambda.Location);
                        unsupportedCapture = true;
                    }
                    else if (capturedLocal.ScopeDepth > 2 && !capturedLocal.IsForHeader && capturedLocal.BlockOwner is null && capturedLocal.ForInOwner is null)
                    {
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                            $"Capturing local '{capturedName}' from this nested scope is not yet supported; method-scope, nested-block, C-style for-header, and for-in captures are supported.", lambda.Location);
                        unsupportedCapture = true;
                    }
                }
                var capturesThis = LambdaBodyReferencesThis(lambda) || LambdaReferencesImplicitInstanceMember(lambda, context);
                var scopeBoundLambdaDepth = scopeBoundCaptures
                    .Select(name =>
                    {
                        var local = context.Lookup(name)!;
                        return local.ScopeBoundDepth ?? (local.IsReadOnlyReference ? local.ScopeDepth : (int?)null);
                    })
                    .Where(depth => depth is not null)
                    .Select(depth => depth!.Value)
                    .DefaultIfEmpty(-1)
                    .Max();
                if (capturesThis && _scopeBoundClosureEnvironmentDepths.TryGetValue(GetQualifiedTypeName(context.Method.Owner), out var capturedEnvironmentDepth))
                    scopeBoundLambdaDepth = Math.Max(scopeBoundLambdaDepth, capturedEnvironmentDepth);
                if (scopeBoundLambdaDepth >= 0) _scopeBoundLambdaDepths[lambda] = scopeBoundLambdaDepth;
                var scopeBoundCaptureOrders = scopeBoundCaptures
                    .Select(name => context.Lookup(name)?.ScopeBoundOrder)
                    .ToArray();
                // A stored closure must not outlive any scoped value it borrows. The
                // latest captured allocation is the order needed by container checks;
                // Scoped and in parameters without a local allocation order stay conservative.
                if (scopeBoundCaptureOrders.Length > 0 && scopeBoundCaptureOrders.All(order => order is not null))
                    _scopeBoundLambdaOrders[lambda] = scopeBoundCaptureOrders.Max(order => order!.Value);
                if (capturesThis && (context.Method.Owner.Kind is not ("class" or "struct") || context.Method.Declaration.IsStatic || context.Method.IsStructConstructor))
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                        "Capturing 'this' is supported in class and struct instance methods; static receivers and struct constructors cannot be captured.", lambda.Location);
                    unsupportedCapture = true;
                }
                else if (capturesThis && !IsClosureEnvironmentType(GetQualifiedTypeName(context.Method.Owner)) &&
                    !context.Method.Declaration.IsMutating && LambdaMutatesEnclosingInstance(lambda, context))
                {
                    diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A lambda in a non-mutating method cannot modify or call mutating members on the captured receiver.", lambda.Location);
                    unsupportedCapture = true;
                }
                var capturedForHeaderLocals = capturedNames
                    .Select(name => context.Lookup(name)!)
                    .Where(local => local.ForHeaderOwner is not null)
                    .Select(local => local.ForHeaderOwner!)
                    .Distinct<ForStatement>(ReferenceEqualityComparer.Instance)
                    .ToArray();
                var capturedBlockLocals = capturedNames
                    .Select(name => context.Lookup(name)!)
                    .Where(local => local.BlockOwner is not null)
                    .Select(local => local.BlockOwner!)
                    .Distinct<BlockStatement>(ReferenceEqualityComparer.Instance)
                    .ToArray();
                var capturedForInLocals = capturedNames
                    .Select(name => context.Lookup(name)!)
                    .Where(local => local.ForInOwner is not null)
                    .Select(local => local.ForInOwner!)
                    .Distinct<ForInStatement>(ReferenceEqualityComparer.Instance)
                    .ToArray();
                ClosureEnvironmentBinding? closureEnvironment = null;
                var inactiveCaptureScope = capturedForHeaderLocals.Any(owner => !context.IsForStatementActive(owner)) ||
                    capturedForInLocals.Any(owner => !context.IsForInStatementActive(owner)) ||
                    capturedBlockLocals.Any(owner => !context.IsCaptureBlockActive(owner));
                var methodCapturedNames = capturedNames.Where(name =>
                {
                    var local = context.Lookup(name)!;
                    return local.ForHeaderOwner is null && local.ForInOwner is null && local.BlockOwner is null;
                }).ToArray();
                var scopeCount = capturedForHeaderLocals.Length + capturedForInLocals.Length + capturedBlockLocals.Length;
                var requiresCompositeEnvironment = scopeCount > 1 ||
                    scopeCount > 0 && (methodCapturedNames.Length > 0 || capturesThis);
                if (inactiveCaptureScope)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                        "A lambda cannot outlive the lexical scope of a captured for-header, for-in, or nested-block variable.", lambda.Location);
                    unsupportedCapture = true;
                }
                else if (requiresCompositeEnvironment && !unsupportedCapture)
                {
                    closureEnvironment = GetOrCreateCompositeClosureEnvironment(lambda, capturedNames, context, capturesThis,
                        capturedForHeaderLocals, capturedForInLocals, capturedBlockLocals, methodCapturedNames);
                }
                else if (!unsupportedCapture && capturedForHeaderLocals.Length > 0)
                {
                    var owner = capturedForHeaderLocals[0];
                    if (capturedForHeaderLocals.Length != 1 || capturedNames.Any(name => context.Lookup(name)?.ForHeaderOwner != owner))
                        unsupportedCapture = ReportMixedCaptureEnvironment(lambda);
                    else closureEnvironment = GetOrCreateClosureEnvironment(context.Method, capturedNames, context, captureThis: false, forHeaderOwner: owner);
                }
                else if (!unsupportedCapture && capturedForInLocals.Length > 0)
                {
                    var owner = capturedForInLocals[0];
                    if (capturedForInLocals.Length != 1 || capturedNames.Any(name => context.Lookup(name)?.ForInOwner != owner))
                        unsupportedCapture = ReportMixedCaptureEnvironment(lambda);
                    else closureEnvironment = GetOrCreateClosureEnvironment(context.Method, capturedNames, context, captureThis: false, forInOwner: owner);
                }
                else if (!unsupportedCapture && capturedBlockLocals.Length > 0)
                {
                    var owner = capturedBlockLocals[0];
                    if (capturedBlockLocals.Length != 1 || capturedNames.Any(name => context.Lookup(name)?.BlockOwner != owner))
                        unsupportedCapture = ReportMixedCaptureEnvironment(lambda);
                    else closureEnvironment = GetOrCreateClosureEnvironment(context.Method, capturedNames, context, captureThis: false, blockOwner: owner);
                }
                else if ((capturedNames.Count > 0 || capturesThis) && !unsupportedCapture)
                    closureEnvironment = GetOrCreateClosureEnvironment(context.Method, capturedNames, context, capturesThis);

                if (!unsupportedCapture && scopeBoundCaptures.Length > 0)
                    MarkScopeBoundCaptureFields(scopeBoundCaptures, context);

                bool ReportMixedCaptureEnvironment(LambdaExpression captureLambda)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                        "A lambda cannot mix captures from multiple lexical environments; use separate closures for each environment.", captureLambda.Location);
                    return true;
                }

                var returnType = NormalizeEnumType(lambdaDelegate.DelegateReturnType, lambdaDelegate);
                var loweredLambda = closureEnvironment is { IsComposite: true }
                    ? RewriteLambdaForClosure(lambda, context, closureEnvironment)
                    : capturesThis && closureEnvironment is not null ? RewriteLambdaForClosure(lambda, context) : lambda;
                var lambdaBody = loweredLambda.BlockBody ?? new BlockStatement(
                    returnType == "void"
                        ? [new ExpressionStatement(loweredLambda.ExpressionBody ?? new UnsupportedExpression("missing lambda expression body", lambda.Location), lambda.Location)]
                        : [new ReturnStatement(loweredLambda.ExpressionBody, lambda.Location)], lambda.Location);
                var generatedLambdaBody = unsupportedCapture
                    ? returnType == "void"
                        ? new BlockStatement([], lambda.Location)
                        : new BlockStatement([new ReturnStatement(new UnsupportedExpression("body omitted after an unsupported capture", lambda.Location), lambda.Location)], lambda.Location)
                    : lambdaBody;
                var generatedName = "$lambda" + _nextLambdaMethodIndex++;
                var lambdaOwner = closureEnvironment is not null
                    ? _classTypes[closureEnvironment.TypeName].Declaration
                    : context.Method.Owner;
                var declaration = new MethodDeclaration(lambdaOwner.Name, generatedName, returnType, generatedParameters,
                    generatedLambdaBody,
                    IsStatic: closureEnvironment is null, IsPublic: false, IsMutating: false, HasTypeParameters: false, lambda.Location,
                    IsUnsafe: context.Method.Declaration.IsUnsafe);
                var generatedMethod = new MethodSymbol(lambdaOwner, declaration);
                _delegateMethodGroups[lambda] = new(lambdaDelegateName, generatedMethod, IsInstance: closureEnvironment is not null,
                    ClosureEnvironment: closureEnvironment);
                _methodList.Add(generatedMethod);
                return lambdaDelegateName;
            case UnsupportedExpression:
                return "?";
            case MemberAccessExpression access:
                if (!HasLocalRoot(access.Receiver, context) && TryFlattenName(access.Receiver, out var qualifiedTypeName) &&
                    IsInaccessibleTypeReference(qualifiedTypeName, context.Method.Owner))
                {
                    ValidateTypeAccessibility(qualifiedTypeName, context.Method.Owner, access.Receiver.Location);
                    return "?";
                }
                if (access.Member == "Empty" &&
                    TryFlattenName(access.Receiver, out var emptyTypeName) &&
                    TrySplitGenericType(emptyTypeName, out var emptyDefinition, out var emptyArguments) && emptyArguments.Count == 1 &&
                    emptyDefinition is "Span" or "System.Span" or "ReadOnlySpan" or "System.ReadOnlySpan")
                {
                    // Open generic type qualifiers cannot be resolved to a constructed type here.
                    // Both runtime Empty properties are defined as the default span value, so bind
                    // them to the closed expected type and let the emitter materialize that value.
                    var emptyType = expectedType is not null &&
                        (expectedType.StartsWith("System.Span__g_", StringComparison.Ordinal) || expectedType.StartsWith("System.ReadOnlySpan__g_", StringComparison.Ordinal))
                        ? expectedType
                        : NormalizeEnumType(emptyTypeName, context.Method.Owner);
                    _genericSpanEmptyAccesses[access] = emptyType;
                    return emptyType;
                }
                if (expectedType is not null && TryBindMethodGroup(access, expectedType, context))
                    return NormalizeEnumType(expectedType, context.Method.Owner);
                if (TryBindConstantField(access, context, out var field)) return field.TypeName;
                if (TryBindStaticField(access, context, out var staticField)) return staticField.TypeName;
                var lengthReceiverType = access.Member is "Length" or "Rank" or "Ptr" ? AnalyzeExpression(access.Receiver, context) : null;
                if (access.Member == "Ptr" && lengthReceiverType is not null && TryParseManagedArrayType(lengthReceiverType, out var pointerElementType, out var pointerArrayRank) && pointerArrayRank <= 3)
                {
                    var nullablePointerArray = IsNullableArrayType(lengthReceiverType);
                    if (nullablePointerArray && !access.IsNullConditional && !IsDefinitelyNotNull(access.Receiver, lengthReceiverType, context))
                    {
                        diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable array instance may be null; use a null check or null assertion before member access.", access.Location);
                        return "?";
                    }
                    if (nullablePointerArray && access.IsNullConditional)
                    {
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "The Ptr property cannot be used with conditional access because it returns a pointer.", access.Location);
                        return "?";
                    }
                    if (!UnsafeEnabledAt(access.Location)) diagnostics.Add("LS1901", DiagnosticSeverity.Error, "Managed array Ptr access requires unsafe code, but unsafe is disabled for this compilation.", access.Location);
                    else if (!access.IsUnsafeContext) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Managed array Ptr access may only be used in an unsafe context.", access.Location);
                    _managedArrayPointerAccesses[access] = pointerElementType;
                    return pointerElementType + "*";
                }
                if (access.Member == "Ptr" && lengthReceiverType is not null && IsStringType(lengthReceiverType))
                {
                    if (!UnsafeEnabledAt(access.Location)) diagnostics.Add("LS1901", DiagnosticSeverity.Error, "String Ptr access requires unsafe code, but unsafe is disabled for this compilation.", access.Location);
                    else if (!access.IsUnsafeContext) diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "String Ptr access may only be used in an unsafe context.", access.Location);
                    _managedArrayPointerAccesses[access] = "$string";
                    return "uint8*";
                }
                if (lengthReceiverType is not null && TryParseInlineArrayType(lengthReceiverType, out _, out _) && access.Member is "Length" or "Rank") return "int32";
                var nullableLengthArray = lengthReceiverType is not null && IsNullableArrayType(lengthReceiverType);
                var nullableArrayBase = lengthReceiverType is not null && IsNullableClassType(lengthReceiverType) && NullableUnderlyingType(lengthReceiverType) == "System.Array";
                if ((nullableLengthArray || nullableArrayBase) && !access.IsNullConditional && !IsDefinitelyNotNull(access.Receiver, lengthReceiverType!, context))
                {
                    diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable array instance may be null; use a null check or null assertion before member access.", access.Location);
                    return "?";
                }
                if (nullableLengthArray || nullableArrayBase) lengthReceiverType = NullableUnderlyingType(lengthReceiverType!);
                if (lengthReceiverType is not null && TryParseManagedArrayType(lengthReceiverType, out _, out var arrayRank) && arrayRank <= 3)
                {
                    if (access.Member == "Length") _managedArrayLengthAccesses[access] = true;
                    else _managedArrayRankAccesses[access] = arrayRank;
                    return (nullableLengthArray || nullableArrayBase) && access.IsNullConditional ? "int32?" : "int32";
                }
                if (lengthReceiverType == "System.Array")
                {
                    if (access.Member == "Length") _managedArrayLengthAccesses[access] = true;
                    else _managedArrayRankAccesses[access] = -1;
                    return nullableArrayBase && access.IsNullConditional ? "int32?" : "int32";
                }
                if (lengthReceiverType == "string")
                {
                    _stringLengthAccesses[access] = true;
                    return "int32";
                }
                if (lengthReceiverType == "string?" && access.IsNullConditional)
                {
                    _stringLengthAccesses[access] = true;
                    return "int32?";
                }
                if (lengthReceiverType == "string?")
                {
                    if (!IsDefinitelyNotNull(access.Receiver, lengthReceiverType, context))
                    {
                        diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable string instance may be null; use a null check or null assertion before member access.", access.Location);
                        return "?";
                    }
                    _stringLengthAccesses[access] = true;
                    return "int32";
                }
                if (!HasLocalRoot(access.Receiver, context) && TryFlattenName(access.Receiver, out var requestedType))
                {
                    var matchingTypes = FindTypes(NormalizeEnumType(requestedType, context.Method.Owner), context.Method.Owner);
                    if (matchingTypes.Length > 1) { diagnostics.Add("LS2001", DiagnosticSeverity.Error, $"The type name '{requestedType}' is ambiguous.", access.Receiver.Location); return "?"; }
                    if (matchingTypes.Length == 1)
                    {
                        var staticOwner = GetQualifiedTypeName(matchingTypes[0]);
                        var property = TryGetComputedProperty(staticOwner, access.Member);
                        if (property is not null)
                        {
                            if (!property.Declaration.IsStatic)
                            {
                                diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Instance property '{staticOwner}.{access.Member}' requires an object receiver.", access.Location);
                                return "?";
                            }
                            return BindComputedPropertyRead(access, property, context);
                        }
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Only primitive const fields are currently supported for static member access.", access.Location);
                        return "?";
                    }
                }
                var structReceiverType = lengthReceiverType ?? AnalyzeExpression(access.Receiver, context);
                var pointerReceiver = IsPointerType(structReceiverType);
                if (pointerReceiver)
                    structReceiverType = NormalizePointerMemberReceiverType(structReceiverType, access.IsUnsafeContext, access.Location);
                if (IsNullableClassType(structReceiverType))
                {
                    if (!access.IsNullConditional && !IsDefinitelyNotNull(access.Receiver, structReceiverType, context))
                    {
                        diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable class instance may be null; use a null check or null assertion before member access.", access.Location);
                        return "?";
                    }
                    structReceiverType = NullableUnderlyingType(structReceiverType);
                }
                else if (IsNullableValueType(structReceiverType) && IsStructType(NullableUnderlyingType(structReceiverType)))
                {
                    if (!IsDefinitelyNotNull(access.Receiver, structReceiverType, context))
                    {
                        diagnostics.Add("LS1303", DiagnosticSeverity.Error, "The nullable struct value may be null; use a null check or null assertion before member access.", access.Location);
                        return "?";
                    }
                    structReceiverType = NullableUnderlyingType(structReceiverType);
                }
                var computedProperty = TryGetComputedProperty(structReceiverType, access.Member);
                if (computedProperty is not null)
                {
                    if (computedProperty.Declaration.IsStatic)
                    {
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Static property '{GetQualifiedTypeName(computedProperty.Owner)}.{access.Member}' requires a type receiver.", access.Location);
                        return "?";
                    }
                    return BindComputedPropertyRead(access, computedProperty, context);
                }
                if (IsStructType(structReceiverType) && TryGetStructField(structReceiverType, access.Member, out var resolvedStructField))
                {
                    if (!IsAccessibilityAvailable(GetFieldAccessibility(resolvedStructField.Declaration), resolvedStructField.Owner, context.Method.Owner))
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Struct field '{structReceiverType}.{access.Member}' is not accessible from this type.", access.Location);
                    var binding = new StructFieldBinding(structReceiverType, resolvedStructField.TypeName, resolvedStructField.Index,
                        resolvedStructField.Declaration.IsReadOnly, pointerReceiver);
                    _boundStructFields[access] = binding;
                    return binding.TypeName;
                }
                if (IsClassType(structReceiverType) && TryGetClassField(structReceiverType, access.Member, out var resolvedClassField))
                {
                    if (!IsAccessibilityAvailable(GetFieldAccessibility(resolvedClassField.Declaration), resolvedClassField.Owner, context.Method.Owner))
                        diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Class field '{structReceiverType}.{access.Member}' is not accessible from this type.", access.Location);
                    var declaredOwnerType = GetQualifiedTypeName(resolvedClassField.Owner);
                    var binding = new StructFieldBinding(structReceiverType, resolvedClassField.TypeName, resolvedClassField.Index,
                        resolvedClassField.Declaration.IsReadOnly, pointerReceiver, declaredOwnerType,
                        _scopeBoundClassFields.Contains((declaredOwnerType, access.Member)));
                    _boundClassFields[access] = binding;
                    if (access.IsNullConditional)
                    {
                        if (IsClassReferenceType(binding.TypeName)) return NullableUnderlyingType(binding.TypeName) + "?";
                        if (IsStringType(binding.TypeName)) return "string?";
                        if (IsManagedArrayReferenceType(binding.TypeName)) return IsNullableArrayType(binding.TypeName) ? binding.TypeName : binding.TypeName + "?";
                        if (IsNullableValueType(binding.TypeName)) return binding.TypeName;
                        if (IsNullableValuePayloadType(binding.TypeName)) return binding.TypeName + "?";
                        else
                        {
                            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Null-conditional access to '{binding.TypeName}' requires nullable value/string support, which is not yet implemented.", access.Location);
                            return "?";
                        }
                    }
                    return binding.TypeName;
                }
                if (IsStructType(structReceiverType)) diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Struct '{structReceiverType}' has no field named '{access.Member}'.", access.Location);
                else if (IsClassType(structReceiverType)) diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Class '{structReceiverType}' has no field named '{access.Member}'.", access.Location);
                else if (structReceiverType != "?") diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Instance and value member access is not yet supported for this value type.", access.Location);
                return "?";
            default: diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Expression is not supported yet.", expression.Location); return "?";
        }
    }

    private void AnalyzeVariableDeclarations(IReadOnlyList<VariableStatement> variables, MethodContext context)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var variable in variables)
        {
            if (!names.Add(variable.Name)) diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Local '{variable.Name}' is declared more than once in this statement.", variable.Location);
            if (context.Exists(variable.Name)) diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Local '{variable.Name}' conflicts with a name in an enclosing scope.", variable.Location);
        }

        var previousNames = _currentDeclarationNames;
        _currentDeclarationNames = names;
        var initializerTypes = new string?[variables.Count];
        try
        {
            for (var index = 0; index < variables.Count; index++)
            {
                var initializer = variables[index].Initializer;
                if (initializer is null) continue;
                if (ContainsDeclaredName(initializer, names)) diagnostics.Add("LS0201", DiagnosticSeverity.Error, "An initializer cannot refer to a variable declared by the same statement.", initializer.Location);
                initializerTypes[index] = AnalyzeExpression(initializer, context, variables[index].TypeName);
                if (variables[index].IsConstant && !IsConstantExpression(initializer, context))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A const initializer must be a constant expression composed of literals, earlier const locals, and constant operators.", initializer.Location);
            }
        }
        finally { _currentDeclarationNames = previousNames; }

        var inferredTypes = initializerTypes.Where(t => t is not null).Distinct(StringComparer.Ordinal).ToArray();
        var inferredType = inferredTypes.FirstOrDefault() ?? "?";
        var hasInferredType = variables.Any(v => v.TypeName is null);
        if (hasInferredType && (initializerTypes.Any(t => t is null) || inferredTypes.Length > 1))
            diagnostics.Add("LS0213", DiagnosticSeverity.Error, "An inferred multi-declaration requires an initializer for every name and one common type.", variables[0].Location);
        else if (!hasInferredType && variables.Any(v => !v.Mutable) && initializerTypes.Any(t => t is null))
            diagnostics.Add("LS0213", DiagnosticSeverity.Error, "Every immutable local in a multi-declaration must be initialized.", variables[0].Location);

        for (var index = 0; index < variables.Count; index++)
        {
            var variable = variables[index];
            var type = variable.TypeName is null ? inferredType : NormalizeEnumType(variable.TypeName, context.Method.Owner);
                if (variable.TypeName is not null) ValidateDeclaredType(type, "Local", variable.Location, context.Method.Owner);
            if (variable.IsConstant && type is not ("bool" or "char" or "int" or "uint" or "int8" or "uint8" or "int16" or "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double" or "string"))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Type '{type}' is not permitted for a local constant.", variable.Location);
            var initializerType = initializerTypes[index];
            if (variable.TypeName is not null && initializerType is not null && type != initializerType && !CanConvertInContext(initializerType, type, variable.Initializer, context))
                AddReferenceConversionDiagnostic(initializerType, type, variable.Initializer, context, variable.Location);
            if (type == "void") diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A local cannot have type void.", variable.Location);
            if (initializerType is null && TryParseManagedArrayType(type, out _, out _)) diagnostics.Add("LS0810", DiagnosticSeverity.Error, $"Managed array '{type}' must be initialized before it is read.", variable.Location);
            if (initializerType is null && (type == "string" || IsClassReferenceType(type) && !IsNullableClassType(type) || IsDelegateType(type))) diagnostics.Add("LS0810", DiagnosticSeverity.Error, $"Non-nullable reference local '{type}' must be initialized.", variable.Location);
            _variableTypes[variable] = type;
            var scopeBoundDepth = variable.Initializer is null ? null : GetScopeBoundDepth(variable.Initializer, context);
            var scopeBoundOrder = variable.Initializer is null ? null : GetScopeBoundOrder(variable.Initializer, context);
            var isReadOnlyReference = variable.Initializer is not null && IsReadOnlyReferenceExpression(variable.Initializer, context);
            if (scopeBoundDepth is not null || isReadOnlyReference) _scopeBoundVariables.Add(variable);
            context.Declare(variable.Name, type, variable.Mutable, variable.Location, variable.IsConstant, scopeBoundDepth, isReadOnlyReference,
                forHeaderOwner: context.ForInitializerOwner,
                blockOwner: context.ForInitializerOwner is null ? context.CurrentCaptureBlock : null,
                scopeBoundOrder: scopeBoundOrder,
                declaration: variable);
            if (IsNullableType(type)) context.SetNullState(variable.Name, initializerType is null ? NullState.Null : GetNullState(variable.Initializer!, initializerType, context));
        }
    }

    private int? GetScopeBoundDepth(Expression expression, MethodContext context) => expression switch
    {
        AllocationExpression { IsScope: true } => context.ScopeDepth,
        LambdaExpression lambda when _scopeBoundLambdaDepths.TryGetValue(lambda, out var lambdaDepth) => lambdaDepth,
        NameExpression name when _boundUnqualifiedClassFields.TryGetValue(name, out var unqualifiedField) &&
            _scopeBoundClosureFields.TryGetValue((unqualifiedField.OwnerType, name.Name), out var unqualifiedCaptureDepth) => unqualifiedCaptureDepth,
        NameExpression name => context.Lookup(name.Name)?.ScopeBoundDepth,
        MemberAccessExpression access when _propertyGetterCalls.TryGetValue(access, out var getterCall) =>
            GetScopeBoundDepth(getterCall, context),
        MemberAccessExpression access when _boundClassFields.TryGetValue(access, out var classField) &&
            _scopeBoundClassFields.Contains((classField.DeclaredOwnerType ?? classField.OwnerType, access.Member)) &&
            GetScopeBoundOrBorrowedDepth(access.Receiver, context) is { } classReceiverDepth => classReceiverDepth,
        MemberAccessExpression access when _boundClassFields.TryGetValue(access, out var memberField) &&
            _scopeBoundClosureFields.TryGetValue((memberField.OwnerType, access.Member), out var memberCaptureDepth) => memberCaptureDepth,
        CallExpression call when _callTargets.TryGetValue(call, out var calledMethod) &&
            IsScopeBoundReferenceType(calledMethod.Declaration.ReturnType) && call.Target is MemberAccessExpression callTarget &&
            GetScopeBoundOrBorrowedDepth(callTarget.Receiver, context) is { } callReceiverDepth => callReceiverDepth,
        IndexExpression index when _indexerGetterCalls.TryGetValue(index, out var indexerGetterCall) =>
            GetScopeBoundDepth(indexerGetterCall, context),
        IndexExpression index when _managedArrayIndexAccesses.TryGetValue(index, out var binding) &&
            IsScopeBoundReferenceType(binding.ElementType) =>
                GetScopeBoundDepth(index.Receiver, context),
        NullAssertionExpression assertion => GetScopeBoundDepth(assertion.Operand, context),
        CastExpression cast => GetScopeBoundDepth(cast.Operand, context),
        OverflowContextExpression overflow => GetScopeBoundDepth(overflow.Value, context),
        ConditionalExpression conditional => ShortestScopeLifetime(
            GetScopeBoundDepth(conditional.WhenTrue, context), GetScopeBoundDepth(conditional.WhenFalse, context)),
        BinaryExpression { Operator: "??" } coalesce => ShortestScopeLifetime(
            GetScopeBoundDepth(coalesce.Left, context), GetScopeBoundDepth(coalesce.Right, context)),
        _ => null
    };

    private int? GetScopeBoundOrBorrowedDepth(Expression expression, MethodContext context)
    {
        var scopeBoundDepth = GetScopeBoundDepth(expression, context);
        if (scopeBoundDepth is not null) return scopeBoundDepth;
        return expression switch
        {
            NameExpression name when context.Lookup(name.Name) is { IsReadOnlyReference: true } local => local.ScopeDepth,
            NullAssertionExpression assertion => GetScopeBoundOrBorrowedDepth(assertion.Operand, context),
            CastExpression cast => GetScopeBoundOrBorrowedDepth(cast.Operand, context),
            OverflowContextExpression overflow => GetScopeBoundOrBorrowedDepth(overflow.Value, context),
            ConditionalExpression conditional => LaterScopeOrder(
                GetScopeBoundOrBorrowedDepth(conditional.WhenTrue, context), GetScopeBoundOrBorrowedDepth(conditional.WhenFalse, context)),
            BinaryExpression { Operator: "??" } coalesce => LaterScopeOrder(
                GetScopeBoundOrBorrowedDepth(coalesce.Left, context), GetScopeBoundOrBorrowedDepth(coalesce.Right, context)),
            _ => null
        };
    }

    private int? GetScopeBoundOrder(Expression expression, MethodContext context) => expression switch
    {
        AllocationExpression { IsScope: true } allocation => _scopeAllocationOrders.GetValueOrDefault(allocation),
        LambdaExpression lambda when _scopeBoundLambdaOrders.TryGetValue(lambda, out var lambdaOrder) => lambdaOrder,
        NameExpression name => context.Lookup(name.Name)?.ScopeBoundOrder,
        MemberAccessExpression access when _propertyGetterCalls.TryGetValue(access, out var getterCall) =>
            GetScopeBoundOrder(getterCall, context),
        MemberAccessExpression access when _boundClassFields.TryGetValue(access, out var field) &&
            _scopeBoundClassFields.Contains((field.DeclaredOwnerType ?? field.OwnerType, access.Member)) =>
                GetScopeBoundOrder(access.Receiver, context),
        CallExpression call when _callTargets.TryGetValue(call, out var calledMethod) &&
            IsScopeBoundReferenceType(calledMethod.Declaration.ReturnType) && call.Target is MemberAccessExpression callTarget =>
                GetScopeBoundOrder(callTarget.Receiver, context),
        IndexExpression index when _indexerGetterCalls.TryGetValue(index, out var indexerGetterCall) =>
            GetScopeBoundOrder(indexerGetterCall, context),
        IndexExpression index when _managedArrayIndexAccesses.TryGetValue(index, out var binding) &&
            IsScopeBoundReferenceType(binding.ElementType) =>
                GetScopeBoundOrder(index.Receiver, context),
        NullAssertionExpression assertion => GetScopeBoundOrder(assertion.Operand, context),
        CastExpression cast => GetScopeBoundOrder(cast.Operand, context),
        OverflowContextExpression overflow => GetScopeBoundOrder(overflow.Value, context),
        ConditionalExpression conditional => LaterScopeOrder(
            GetScopeBoundOrder(conditional.WhenTrue, context), GetScopeBoundOrder(conditional.WhenFalse, context)),
        BinaryExpression { Operator: "??" } coalesce => LaterScopeOrder(
            GetScopeBoundOrder(coalesce.Left, context), GetScopeBoundOrder(coalesce.Right, context)),
        _ => null
    };

    private bool CanStoreScopeBoundArrayElement(Expression array, Expression value, MethodContext context)
    {
        if (GetScopeBoundDepth(array, context) is not { } arrayDepth ||
            GetScopeBoundDepth(value, context) is not { } valueDepth || arrayDepth < valueDepth)
            return false;

        if (arrayDepth > valueDepth || value is AllocationExpression { IsScope: true })
            return true;

        return GetScopeBoundOrder(array, context) is { } arrayOrder &&
            GetScopeBoundOrder(value, context) is { } valueOrder && arrayOrder > valueOrder;
    }

    private static int? LaterScopeOrder(int? first, int? second) =>
        first is null ? second : second is null ? first : Math.Max(first.Value, second.Value);

    private bool IsReadOnlyReferenceExpression(Expression expression, MethodContext context) => expression switch
    {
        NameExpression name => context.Lookup(name.Name)?.IsReadOnlyReference == true,
        NullAssertionExpression assertion => IsReadOnlyReferenceExpression(assertion.Operand, context),
        CastExpression cast => IsReadOnlyReferenceExpression(cast.Operand, context),
        OverflowContextExpression overflow => IsReadOnlyReferenceExpression(overflow.Value, context),
        ConditionalExpression conditional => IsReadOnlyReferenceExpression(conditional.WhenTrue, context) || IsReadOnlyReferenceExpression(conditional.WhenFalse, context),
        BinaryExpression { Operator: "??" } coalesce => IsReadOnlyReferenceExpression(coalesce.Left, context) || IsReadOnlyReferenceExpression(coalesce.Right, context),
        _ => false
    };

    private bool IsManagedReferenceValue(string type) => IsClassReferenceType(type) || IsDelegateType(type) || IsNullableDelegateType(type) ||
        IsStringType(type) || TryParseManagedArrayType(NullableUnderlyingType(type), out _, out _);

    private static int? ShortestScopeLifetime(int? first, int? second) =>
        first is null ? second : second is null ? first : Math.Max(first.Value, second.Value);

    private bool IsConstantExpression(Expression expression, MethodContext context) => expression switch
    {
        LiteralExpression literal => literal.Value is "true" or "false" || (literal.Value.StartsWith('"') || literal.Value.StartsWith('@')) && StringLiteralCodec.TryParse(literal.Value, out _, out _) || literal.Value.StartsWith('\'') && TryParseCharacter(literal.Value, out _) ||
            (IsFloatingLiteral(literal.Value) ? ParseFloating(literal.Value).Type != "?" : ParseInteger(literal.Value).Type != "?"),
        NameExpression name => context.Lookup(name.Name) is { IsConstant: true } || TryGetConstantField(name, out _),
        UnaryExpression unary when unary.Operator is "+" or "-" or "~" or "!" => IsConstantExpression(unary.Operand, context),
        CastExpression cast => IsConstantExpression(cast.Operand, context),
        BinaryExpression binary when binary.Operator is not ("=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=") =>
            IsConstantExpression(binary.Left, context) && IsConstantExpression(binary.Right, context),
        ConditionalExpression conditional => IsConstantExpression(conditional.Condition, context) && IsConstantExpression(conditional.WhenTrue, context) && IsConstantExpression(conditional.WhenFalse, context),
        OverflowContextExpression overflow => IsConstantExpression(overflow.Value, context),
        SizeOfExpression sizeOf => IntegerTypes.Contains(_sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName)) || FloatingTypes.Contains(_sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName)) || _sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName) is "bool" or "char" || IsEnumType(_sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName)) || IsPointerType(_sizeOfTypes.GetValueOrDefault(sizeOf, sizeOf.TypeName)),
        DefaultValueExpression defaultValue => _defaultTypes.ContainsKey(defaultValue),
        MemberAccessExpression access => TryGetConstantField(access, out _),
        _ => false
    };

    private static bool CanEvaluateConstantNow(Expression expression) => expression switch
    {
        LiteralExpression or SizeOfExpression or DefaultValueExpression => true,
        UnaryExpression unary when unary.Operator is "+" or "-" or "~" or "!" => CanEvaluateConstantNow(unary.Operand),
        CastExpression cast => CanEvaluateConstantNow(cast.Operand),
        BinaryExpression binary when binary.Operator is not ("=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=") =>
            CanEvaluateConstantNow(binary.Left) && CanEvaluateConstantNow(binary.Right),
        ConditionalExpression conditional => CanEvaluateConstantNow(conditional.Condition) && CanEvaluateConstantNow(conditional.WhenTrue) && CanEvaluateConstantNow(conditional.WhenFalse),
        OverflowContextExpression overflow => CanEvaluateConstantNow(overflow.Value),
        _ => false
    };

    private bool TryBindConstantField(MemberAccessExpression access, MethodContext context, out ConstantFieldBinding binding)
    {
        binding = null!;
        if (!TryFlattenName(access.Receiver, out var requestedType)) return false;
        var matches = FindTypes(NormalizeBuiltinTypeQualifier(NormalizeEnumType(requestedType, context.Method.Owner)), context.Method.Owner);
        if (matches.Length != 1) return false;
        var owner = matches[0]; var key = GetQualifiedTypeName(owner) + "." + access.Member;
        if (!_constantFields.TryGetValue(key, out var symbol)) return false;
        if (!IsAccessibilityAvailable(symbol.Accessibility, symbol.Owner, context.Method.Owner))
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Constant field '{key}' is not accessible from this type.", access.Location);
        binding = GetPrimitiveConstantBinding(key, symbol.Binding); _fieldAccesses[access] = binding; return true;
    }

    private bool TryBindUnqualifiedConstantField(NameExpression name, MethodContext context, out ConstantFieldBinding binding)
    {
        binding = null!;
        var ownerName = GetQualifiedTypeName(context.Method.Owner);
        IEnumerable<string> owners = context.Method.Owner.Kind == "class" ? GetClassChain(ownerName) : [ownerName];
        foreach (var constantOwner in owners)
        {
            if (!_constantFields.TryGetValue(constantOwner + "." + name.Name, out var field)) continue;
            if (!IsAccessibilityAvailable(field.Accessibility, field.Owner, context.Method.Owner))
                diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Constant field '{constantOwner}.{name.Name}' is not accessible from this type.", name.Location);
            binding = GetPrimitiveConstantBinding(constantOwner + "." + name.Name, field.Binding);
            _boundUnqualifiedConstantFields[name] = binding;
            return true;
        }
        return false;
    }

    private MethodSymbol? FindUserConversionOperator(string sourceType, string targetType, bool allowExplicit)
    {
        var candidates = _methods.Concat(_genericMethodDefinitions)
            .Where(method => method.Declaration.IsConversionOperator &&
                (method.Declaration.Operator == "implicit" || allowExplicit && method.Declaration.Operator == "explicit") &&
                method.Declaration.Parameters.Count == 1 &&
                NormalizeEnumType(method.Declaration.Parameters[0].TypeName, method.Owner) == sourceType &&
                NormalizeEnumType(method.Declaration.ReturnType, method.Owner) == targetType)
            .ToArray();
        var implicitCandidate = candidates.FirstOrDefault(method => method.Declaration.Operator == "implicit");
        return implicitCandidate ?? (candidates.Length == 1 ? candidates[0] : null);
    }

    private MethodSymbol? FindArraySpanConversionMethod(string sourceType, string targetType)
    {
        if (!TryParseManagedArrayType(sourceType, out var elementType, out var rank) || rank != 1)
            return null;

        var targetDefinition = targetType.StartsWith("System.Span__g_", StringComparison.Ordinal)
            ? "System.Span"
            : targetType.StartsWith("System.ReadOnlySpan__g_", StringComparison.Ordinal)
                ? "System.ReadOnlySpan"
                : null;
        if (targetDefinition is null || !_genericTypeInstantiations.Any(instance =>
                GetQualifiedTypeName(instance.Definition) == targetDefinition &&
                GetQualifiedTypeName(instance.Specialized) == targetType &&
                instance.Arguments.Count == 1 &&
                NormalizeEnumType(instance.Arguments[0], instance.RequestContext) == elementType))
            return null;

        var helperName = targetDefinition == "System.Span" ? "ToSpan" : "ToReadOnlySpan";
        var template = _genericMethodDefinitions.FirstOrDefault(method =>
            GetQualifiedTypeName(method.Owner) == "System.Array" &&
            method.Declaration.Name == helperName &&
            method.Declaration.HasTypeParameters &&
            method.Declaration.TypeParameters is { Count: 1 });
        if (template is null)
            return null;

        var cacheKey = $"array-span-conversion:{template.QualifiedName}:{elementType}";
        if (_genericMethodSpecializations.TryGetValue(cacheKey, out var cached))
            return cached;

        var typeParameter = template.Declaration.TypeParameters![0];
        var substitutions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [typeParameter] = elementType
        };
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(elementType)).AsSpan(0, 6)).ToLowerInvariant();
        var declaration = (MethodDeclaration)GenericTypeSpecializer.RewriteMember(
            template.Declaration, template.Owner.Name, substitutions);
        declaration = declaration with
        {
            Name = $"{helperName}__array_g_{hash}",
            HasTypeParameters = false,
            TypeParameters = null,
            GenericConstraints = null,
            ReturnType = NormalizeEnumType(declaration.ReturnType, template.Owner),
            Parameters = declaration.Parameters.Select(parameter => parameter with
            {
                TypeName = NormalizeEnumType(parameter.TypeName, template.Owner)
            }).ToArray()
        };
        var specialized = template with
        {
            Declaration = declaration,
            IsGenericMethodSpecialization = true,
            GenericTemplate = template.Declaration
        };
        _genericMethodSpecializations.Add(cacheKey, specialized);
        _methodList.Add(specialized);
        return specialized;
    }

    private ConstantFieldBinding GetPrimitiveConstantBinding(string key, ConstantFieldBinding fallback)
    {
        var bits = _nativePointerSize * 8;
        if (key == "System.Int.MinValue") return new("int", (-(BigInteger.One << (bits - 1))).ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (key == "System.Int.MaxValue") return new("int", ((BigInteger.One << (bits - 1)) - 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
        if (key == "System.UInt.MinValue") return new("uint", "0");
        if (key == "System.UInt.MaxValue") return new("uint", ((BigInteger.One << bits) - 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
        return fallback;
    }

    private bool TryBindStaticField(MemberAccessExpression access, MethodContext context, out StaticFieldBinding binding)
    {
        binding = null!;
        if (!TryFlattenName(access.Receiver, out var requestedType)) return false;
        var matches = FindTypes(NormalizeBuiltinTypeQualifier(NormalizeEnumType(requestedType, context.Method.Owner)), context.Method.Owner);
        if (matches.Length != 1) return false;
        var owner = matches[0]; var key = GetQualifiedTypeName(owner) + "." + access.Member;
        if (!_staticFields.TryGetValue(key, out var symbol)) return false;
        if (!IsAccessibilityAvailable(symbol.Accessibility, symbol.Owner, context.Method.Owner))
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Static field '{key}' is not accessible from this type.", access.Location);
        binding = new(symbol.Symbol.Declaration.TypeName, symbol.Symbol.GlobalName, !symbol.Symbol.Declaration.IsReadOnly,
            GetQualifiedTypeName(symbol.Owner));
        _boundStaticFields[access] = binding; return true;
    }

    private bool TryGetStructField(string typeName, string fieldName, out StructFieldSymbol field)
    {
        if (_structTypes.TryGetValue(typeName, out var structure))
        {
            var match = structure.Fields.FirstOrDefault(candidate => candidate.Declaration.Name == fieldName);
            if (match is not null) { field = match; return true; }
        }
        field = null!;
        return false;
    }

    private bool TryGetClassField(string typeName, string fieldName, out StructFieldSymbol field)
    {
        if (_classTypes.TryGetValue(typeName, out var classType))
        {
            var match = classType.Fields.LastOrDefault(candidate => candidate.Declaration.Name == fieldName);
            if (match is not null) { field = match; return true; }
        }
        field = null!;
        return false;
    }

    private ComputedPropertySymbol? TryGetComputedProperty(string typeName, string propertyName)
    {
        if (IsStructType(typeName))
            return _computedProperties.GetValueOrDefault(typeName + "." + propertyName);
        if (IsClassType(typeName))
            foreach (var ownerType in GetClassChain(typeName))
                if (_computedProperties.TryGetValue(ownerType + "." + propertyName, out var property)) return property;
        if (_interfaceTypes.TryGetValue(typeName, out var interfaceType))
            foreach (var contractType in GetInterfaceHierarchy(interfaceType))
            {
                var declaration = contractType.Members.OfType<ComputedPropertyDeclaration>().FirstOrDefault(candidate => candidate.Name == propertyName);
                if (declaration is null) continue;
                var getterName = "$property_get_" + propertyName;
                var setterName = "$property_set_" + propertyName;
                var contracts = GetDeclaredInterfaceContracts(contractType).ToArray();
                var getter = contracts.FirstOrDefault(contract => contract.Declaration.Name == getterName);
                var setter = contracts.FirstOrDefault(contract => contract.Declaration.Name == setterName);
                return new(contractType, declaration, NormalizeEnumType(declaration.TypeName, contractType), getter, setter);
            }
        return null;
    }

    private IReadOnlyList<ComputedIndexerSymbol> GetComputedIndexers(string typeName)
    {
        if (IsStructType(typeName)) return (_computedIndexers.GetValueOrDefault(typeName) ?? [])
            .Where(indexer => indexer.Declaration.ExplicitInterfaceName is null).ToArray();
        if (_interfaceTypes.TryGetValue(typeName, out var interfaceType))
        {
            var interfaceIndexers = new List<ComputedIndexerSymbol>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var contractType in GetInterfaceHierarchy(interfaceType))
            foreach (var declaration in contractType.Members.OfType<ComputedIndexerDeclaration>())
            {
                var parameters = declaration.Parameters.Select(parameter => parameter with
                {
                    TypeName = NormalizeEnumType(parameter.TypeName, contractType),
                    DefaultValue = null
                }).ToArray();
                var signature = string.Join("\u001f", parameters.Select(parameter => parameter.TypeName));
                if (!seen.Add(signature)) continue;
                var contracts = GetDeclaredInterfaceContracts(contractType).ToArray();
                var getter = contracts.FirstOrDefault(contract => contract.Declaration.Name == "$indexer_get" &&
                    contract.Declaration.Parameters.Count == parameters.Length &&
                    contract.Declaration.Parameters.Select(parameter => parameter.TypeName).SequenceEqual(parameters.Select(parameter => parameter.TypeName)));
                var setter = contracts.FirstOrDefault(contract => contract.Declaration.Name == "$indexer_set" &&
                    contract.Declaration.Parameters.Count == parameters.Length + 1 &&
                    contract.Declaration.Parameters.Take(parameters.Length).Select(parameter => parameter.TypeName).SequenceEqual(parameters.Select(parameter => parameter.TypeName)));
                interfaceIndexers.Add(new(contractType, declaration, NormalizeEnumType(declaration.TypeName, contractType), getter, setter));
            }
            return interfaceIndexers;
        }
        if (!IsClassType(typeName)) return [];
        var result = new List<ComputedIndexerSymbol>();
        var signatures = new HashSet<string>(StringComparer.Ordinal);
        foreach (var ownerType in GetClassChain(typeName))
        foreach (var indexer in (_computedIndexers.GetValueOrDefault(ownerType) ?? [])
                     .Where(indexer => indexer.Declaration.ExplicitInterfaceName is null))
        {
            var signature = string.Join("\u001f", indexer.Declaration.Parameters.Select(parameter => NormalizeEnumType(parameter.TypeName, indexer.Owner)));
            if (signatures.Add(signature)) result.Add(indexer);
        }
        return result;
    }

    private ComputedIndexerSymbol? SelectComputedIndexer(IReadOnlyList<ComputedIndexerSymbol> candidates,
        IReadOnlyList<string> argumentTypes, IReadOnlyList<Expression> arguments, MethodContext context, SourceLocation location,
        bool reportAmbiguity = true)
    {
        var applicable = new List<(ComputedIndexerSymbol Indexer, int Rank)>();
        foreach (var candidate in candidates)
        {
            if (candidate.Declaration.Parameters.Count != argumentTypes.Count) continue;
            var rank = 0;
            var valid = true;
            for (var index = 0; index < argumentTypes.Count; index++)
            {
                var parameterType = NormalizeEnumType(candidate.Declaration.Parameters[index].TypeName, candidate.Owner);
                if (!CanConvertInContext(argumentTypes[index], parameterType, arguments[index], context)) { valid = false; break; }
                rank += ConversionRank(argumentTypes[index], parameterType, arguments[index]);
            }
            if (valid) applicable.Add((candidate, rank));
        }
        if (applicable.Count == 0) return null;
        var bestRank = applicable.Min(candidate => candidate.Rank);
        var best = applicable.Where(candidate => candidate.Rank == bestRank).ToArray();
        if (best.Length != 1)
        {
            if (reportAmbiguity) diagnostics.Add("LS0750", DiagnosticSeverity.Error, "Indexer access is ambiguous between multiple indexer overloads.", location);
            return null;
        }
        return best[0].Indexer;
    }

    private string BindComputedPropertyRead(MemberAccessExpression access, ComputedPropertySymbol property, MethodContext context)
    {
        if (!IsAccessibilityAvailable(property.Declaration.Accessibility, property.Owner, context.Method.Owner))
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Property '{GetQualifiedTypeName(property.Owner)}.{property.Declaration.Name}' is not accessible from this type.", access.Location);
        _boundComputedProperties[access] = property;
        if (property.Getter is null)
        {
            if (!ReferenceEquals(access, _propertyAssignmentProbe))
                diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"Property '{GetQualifiedTypeName(property.Owner)}.{property.Declaration.Name}' has no get accessor.", access.Location);
            return property.TypeName;
        }

        var target = new MemberAccessExpression(access.Receiver, property.Getter.Declaration.Name, access.Location,
            access.IsNullConditional, access.IsUnsafeContext);
        var getterCall = new CallExpression(target, [], access.Location, IsUnsafeContext: access.IsUnsafeContext);
        var resultType = AnalyzeExpression(getterCall, context);
        _propertyGetterCalls[access] = getterCall;
        return resultType;
    }

    private void BindComputedPropertyWrite(BinaryExpression assignment, Expression? receiver,
        ComputedPropertySymbol property, MethodContext context, bool isNullConditional = false, bool isUnsafeContext = false)
    {
        if (property.Setter is null)
        {
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Property '{GetQualifiedTypeName(property.Owner)}.{property.Declaration.Name}' has no set accessor.", assignment.Left.Location);
            return;
        }
        if (isNullConditional)
        {
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A null-conditional property access cannot be assigned.", assignment.Left.Location);
            return;
        }
        var setter = property.Setter;
        if (!IsAccessibilityAvailable(setter.Declaration.Accessibility, setter.Owner, context.Method.Owner))
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Property '{GetQualifiedTypeName(property.Owner)}.{property.Declaration.Name}' has no accessible set accessor.", assignment.Left.Location);
        if (GetScopeBoundDepth(assignment.Right, context) is not null || IsReadOnlyReferenceExpression(assignment.Right, context))
            diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A scope-bound reference cannot escape through a property assignment.", assignment.Right.Location);

        var mutationReceiver = receiver ?? new ThisExpression(assignment.Location);
        if (setter.IsStructInstanceMethod && setter.Declaration.IsMutating && !IsMutableStructLocation(mutationReceiver, context))
        {
            if (context.Method.IsStructInstanceMethod && !context.Method.Declaration.IsMutating && HasThisRoot(mutationReceiver))
                diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A non-mutating struct method cannot assign a property on its receiver.", assignment.Location);
            else diagnostics.Add("LS0823", DiagnosticSeverity.Error, "A property set accessor requires a mutable struct receiver.", assignment.Location);
        }
        if (setter.IsClassInstanceMethod && (HasThisRoot(mutationReceiver) || IsCapturedThisRoot(mutationReceiver, context)) && !CanMutateClassReceiver(context))
            diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A non-mutating class method cannot assign a property on its receiver.", assignment.Location);

        Expression callTarget = receiver is null
            ? new NameExpression(setter.Declaration.Name, assignment.Location)
            : new MemberAccessExpression(receiver, setter.Declaration.Name, assignment.Location, IsUnsafeContext: isUnsafeContext);
        var setterCall = new CallExpression(callTarget, [assignment.Right], assignment.Location, IsUnsafeContext: isUnsafeContext);
        _callTargets[setterCall] = setter;
        _callArguments[setterCall] = new([0], [null]);
        if (setter.Owner.Kind == "interface" && FindInterfaceMethodSlot(setter) is var interfaceSlot && interfaceSlot >= 0)
            _interfaceCallSlots[setterCall] = interfaceSlot;
        if (setter.IsClassInstanceMethod && receiver is not BaseExpression && (setter.Declaration.IsVirtual || setter.Declaration.IsOverride))
        {
            var receiverType = receiver is null ? GetQualifiedTypeName(context.Method.Owner) : AnalyzeExpression(receiver, context);
            if (IsNullableClassType(receiverType)) receiverType = NullableUnderlyingType(receiverType);
            if (IsPointerType(receiverType) && IsClassType(receiverType[..^1])) receiverType = receiverType[..^1];
            var slot = VirtualDispatchLayout.FindSlot(receiverType, setter, _classTypes, _methods);
            if (slot >= 0) _virtualCallSlots[setterCall] = slot;
        }
        _propertySetterCalls[assignment] = setterCall;
    }

    private void BindComputedIndexerWrite(BinaryExpression assignment, IndexExpression index,
        ComputedIndexerSymbol indexer, MethodContext context)
    {
        var setter = indexer.Setter;
        if (setter is null)
        {
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "The matching indexer has no set accessor.", index.Location);
            return;
        }
        if (!IsAccessibilityAvailable(setter.Declaration.Accessibility, setter.Owner, context.Method.Owner))
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "The matching indexer set accessor is not accessible from this type.", index.Location);
        if (index.IsNullConditional)
        {
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A null-conditional index is a value and cannot be used as an assignment target.", index.Location);
            return;
        }
        if (setter.IsStructInstanceMethod && !IsMutableStructLocation(index.Receiver, context))
        {
            if (context.Method.IsStructInstanceMethod && !context.Method.Declaration.IsMutating && HasThisRoot(index.Receiver))
                diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A non-mutating struct method cannot assign an indexer on its receiver.", assignment.Location);
            else diagnostics.Add("LS0823", DiagnosticSeverity.Error, "An indexer set accessor requires a mutable struct receiver.", assignment.Location);
        }
        if (setter.IsClassInstanceMethod && (HasThisRoot(index.Receiver) || IsCapturedThisRoot(index.Receiver, context)) && !CanMutateClassReceiver(context))
            diagnostics.Add("LS0821", DiagnosticSeverity.Error, "A non-mutating class method cannot assign an indexer on its receiver.", assignment.Location);
        if (GetScopeBoundDepth(assignment.Right, context) is not null || IsReadOnlyReferenceExpression(assignment.Right, context))
            diagnostics.Add("LS0701", DiagnosticSeverity.Error, "A scope-bound reference cannot escape through an indexer assignment.", assignment.Right.Location);

        var setterArguments = index.Indices.Append(assignment.Right).ToArray();
        var setterCall = new CallExpression(new MemberAccessExpression(index.Receiver, setter.Declaration.Name, index.Location,
            IsUnsafeContext: index.IsUnsafeContext), setterArguments, assignment.Location, IsUnsafeContext: index.IsUnsafeContext);
        _callTargets[setterCall] = setter;
        _callArguments[setterCall] = new(Enumerable.Range(0, setterArguments.Length).ToArray(),
            Enumerable.Repeat<Expression?>(null, setter.Declaration.Parameters.Count).ToArray());
        if (setter.Owner.Kind == "interface" && FindInterfaceMethodSlot(setter) is var interfaceSlot && interfaceSlot >= 0)
            _interfaceCallSlots[setterCall] = interfaceSlot;
        if (setter.IsClassInstanceMethod && index.Receiver is not BaseExpression && (setter.Declaration.IsVirtual || setter.Declaration.IsOverride))
        {
            var slot = VirtualDispatchLayout.FindSlot(GetQualifiedTypeName(indexer.Owner), setter, _classTypes, _methods);
            if (slot >= 0) _virtualCallSlots[setterCall] = slot;
        }
        _propertySetterCalls[assignment] = setterCall;
    }

    private static bool IsBasePropertyReceiver(Expression receiver) => receiver is BaseExpression;

    private bool HasLocalRoot(Expression expression, MethodContext context) => expression switch
    {
        NameExpression name => context.Lookup(name.Name) is not null,
        MemberAccessExpression member => HasLocalRoot(member.Receiver, context),
        ThisExpression or BaseExpression => context.Method.IsStructConstructor || context.Method.IsStructInstanceMethod || context.Method.IsClassConstructor || context.Method.IsClassInstanceMethod || context.Method.IsClassFinalizer,
        _ => false
    };

    private bool IsTypeReference(Expression expression, MethodContext context)
    {
        if (HasLocalRoot(expression, context)) return false;
        return TryFlattenName(expression, out var name) && FindTypes(NormalizeBuiltinTypeQualifier(NormalizeEnumType(name, context.Method.Owner)), context.Method.Owner).Length > 0;
    }

    private bool IsInaccessibleTypeReference(string requestedType, TypeDeclaration accessingType)
    {
        var candidates = FindTypeCandidates(NormalizeEnumType(requestedType, accessingType), accessingType);
        return candidates.Length > 0 && candidates.All(candidate => !IsTypeAccessible(candidate, accessingType));
    }

    private static bool HasThisRoot(Expression expression) => expression switch
    {
        ThisExpression or BaseExpression => true,
        MemberAccessExpression member => HasThisRoot(member.Receiver),
        IndexExpression index => HasThisRoot(index.Receiver),
        _ => false
    };

    private bool IsMutableStructLocation(Expression expression, MethodContext context) => expression switch
    {
        NameExpression { Name: "$this" } when _closureEnclosingMethods.TryGetValue(GetQualifiedTypeName(context.Method.Owner), out var enclosingMethod) &&
            enclosingMethod.IsStructInstanceMethod => enclosingMethod.Declaration.IsMutating,
        NameExpression name => context.Lookup(name.Name) is { Mutable: true } ||
            context.Lookup(name.Name) is { Type: var pointerType } && IsPointerType(pointerType) && IsStructType(pointerType[..^1]),
        NullAssertionExpression assertion => IsMutableStructLocation(assertion.Operand, context),
        MemberAccessExpression member when _boundStructFields.TryGetValue(member, out var field) => !field.IsReadOnly && IsMutableStructLocation(member.Receiver, context),
        MemberAccessExpression member when _boundStaticFields.TryGetValue(member, out var staticField) => staticField.Mutable,
        IndexExpression index => IsMutableIndexTarget(index, context),
        ThisExpression => context.Method.IsStructConstructor || context.Method.IsStructInstanceMethod && context.Method.Declaration.IsMutating,
        _ => false
    };

    private bool IsMutableClassFieldLocation(Expression expression, MethodContext context)
    {
        if (IsCapturedThisRoot(expression, context)) return CanMutateClassReceiver(context);
        if (!HasThisRoot(expression)) return true;
        if (IsClosureEnvironmentType(GetQualifiedTypeName(context.Method.Owner))) return true;
        return CanMutateClassReceiver(context);
    }

    private bool CanMutateClassReceiver(MethodContext context) => IsClosureEnvironmentType(GetQualifiedTypeName(context.Method.Owner)) ||
        context.Method.IsClassConstructor || context.Method.IsClassFinalizer ||
        context.Method.IsClassInstanceMethod && context.Method.Declaration.IsMutating ||
        _closureEnclosingMethods.TryGetValue(GetQualifiedTypeName(context.Method.Owner), out var enclosingMethod) && enclosingMethod.Declaration.IsMutating;

    private bool IsCapturedThisRoot(Expression expression, MethodContext context) => expression switch
    {
        NameExpression { Name: "$this" } => _closureEnclosingMethods.ContainsKey(GetQualifiedTypeName(context.Method.Owner)),
        MemberAccessExpression member => IsCapturedThisRoot(member.Receiver, context),
        IndexExpression index => IsCapturedThisRoot(index.Receiver, context),
        NullAssertionExpression assertion => IsCapturedThisRoot(assertion.Operand, context),
        _ => false
    };

    private bool IsClosureEnvironmentType(string typeName) => _closureEnvironments.Values.Any(environment => environment.TypeName == typeName) ||
        _forHeaderClosureEnvironments.Values.Any(environment => environment.TypeName == typeName) ||
        _forInClosureEnvironments.Values.Any(environment => environment.TypeName == typeName) ||
        _blockClosureEnvironments.Values.Any(environment => environment.TypeName == typeName) ||
        _compositeClosureEnvironments.Values.Any(environment => environment.TypeName == typeName);

    private static bool CanInitializeReadOnlyStructField(StructFieldBinding field, Expression receiver, MethodContext context) =>
        context.Method.IsStructConstructor && receiver is ThisExpression && GetQualifiedTypeName(context.Method.Owner) == field.OwnerType;

    private static bool CanInitializeReadOnlyClassField(StructFieldBinding field, Expression receiver, MethodContext context) =>
        context.Method.IsClassConstructor && receiver is ThisExpression && GetQualifiedTypeName(context.Method.Owner) == field.OwnerType;

    private bool CanInitializeReadOnlyFieldTarget(Expression target, MethodContext context)
    {
        if (target is NameExpression name && _boundUnqualifiedStructFields.TryGetValue(name, out var structLocalField))
            return CanInitializeReadOnlyStructField(structLocalField, new ThisExpression(target.Location), context);
        if (target is MemberAccessExpression structMember && _boundStructFields.TryGetValue(structMember, out var structMemberField))
            return CanInitializeReadOnlyStructField(structMemberField, structMember.Receiver, context);
        if (target is NameExpression className && _boundUnqualifiedClassFields.TryGetValue(className, out var classLocalField))
            return CanInitializeReadOnlyClassField(classLocalField, new ThisExpression(target.Location), context);
        if (target is MemberAccessExpression classMember && _boundClassFields.TryGetValue(classMember, out var classMemberField))
            return CanInitializeReadOnlyClassField(classMemberField, classMember.Receiver, context);
        return false;
    }

    private bool IsInlineArrayLocation(Expression expression, MethodContext context) => expression switch
    {
        NameExpression name => context.Lookup(name.Name) is { } local && TryParseInlineArrayType(local.Type, out _, out _) ||
            _boundUnqualifiedClassFields.TryGetValue(name, out var unqualifiedClassField) && TryParseInlineArrayType(unqualifiedClassField.TypeName, out _, out _),
        MemberAccessExpression access => _boundStructFields.TryGetValue(access, out var structField) && TryParseInlineArrayType(structField.TypeName, out _, out _) ||
            _boundClassFields.TryGetValue(access, out var classField) && TryParseInlineArrayType(classField.TypeName, out _, out _) ||
            _boundStaticFields.TryGetValue(access, out var staticField) && TryParseInlineArrayType(staticField.TypeName, out _, out _),
        _ => false
    };

    private TypeDeclaration[] FindTypes(string requestedType, TypeDeclaration contextType)
    {
        contextType = GetLexicalAccessingType(contextType);
        return FindTypeCandidates(requestedType, contextType).Where(candidate => IsTypeAccessible(candidate, contextType)).ToArray();
    }

    private TypeDeclaration[] FindTypeCandidates(string requestedType, TypeDeclaration contextType)
    {
        contextType = GetLexicalAccessingType(contextType);
        if (requestedType == "object") requestedType = "System.Object";
        foreach (var alias in contextType.Usings?.Where(usingDirective => usingDirective.Alias is not null) ?? [])
        {
            var aliasName = alias.Alias!;
            if (requestedType == aliasName) { requestedType = alias.Target; break; }
            if (requestedType.StartsWith(aliasName + ".", StringComparison.Ordinal))
            { requestedType = alias.Target + requestedType[aliasName.Length..]; break; }
        }
        var exact = _types.Where(type => GetQualifiedTypeName(type) == requestedType).ToArray();
        if (exact.Length > 0) return exact;
        if (requestedType.Contains('.', StringComparison.Ordinal))
        {
            var scoped = _types.Where(type => GetQualifiedTypeName(type) == contextType.Namespace + "." + requestedType).ToArray();
            if (scoped.Length > 0) return scoped;
            var importedQualified = contextType.Usings?.Where(usingDirective => usingDirective.Alias is null)
                .SelectMany(usingDirective => _types.Where(type => GetQualifiedTypeName(type) == usingDirective.Target + "." + requestedType)).ToArray() ?? [];
            if (importedQualified.Length > 0) return importedQualified;
            return [];
        }
        var nested = _types.Where(type => type.Name == requestedType && type.Namespace == GetQualifiedTypeName(contextType)).ToArray();
        if (nested.Length > 0) return nested;
        var inNamespace = _types.Where(type => type.Name == requestedType && type.Namespace == contextType.Namespace).ToArray();
        if (inNamespace.Length > 0) return inNamespace;
        var imports = contextType.Usings?.Where(usingDirective => usingDirective.Alias is null)
            .SelectMany(usingDirective => _types.Where(type => type.Name == requestedType && type.Namespace == usingDirective.Target)).ToArray() ?? [];
        if (imports.Length > 0) return imports;
        var global = _types.Where(type => type.Name == requestedType && type.Namespace.Length == 0).ToArray();
        if (global.Length > 0) return global;
        if (requestedType == "Array") return _types.Where(type => type.Name == "Array" && type.Namespace == "System").ToArray();
        if (!IsBuiltinExceptionName(requestedType)) return [];
        var builtinName = requestedType switch
        {
            "System.IO.IOException" or "IOException" => "IOException",
            _ when requestedType.StartsWith("System.", StringComparison.Ordinal) => requestedType["System.".Length..],
            _ => requestedType
        };
        return _types.Where(type => type.Name == builtinName && type.Namespace == BuiltinExceptionNamespace(builtinName)).ToArray();
    }

    private static string NormalizeBuiltinTypeQualifier(string typeName) => typeName switch
    {
        "bool" => "System.Bool", "char" => "System.Char",
        "int8" => "System.Int8", "uint8" => "System.UInt8",
        "int16" => "System.Int16", "uint16" => "System.UInt16",
        "int32" => "System.Int32", "uint32" => "System.UInt32",
        "int64" => "System.Int64", "uint64" => "System.UInt64",
        "int" => "System.Int", "uint" => "System.UInt",
        "float" => "System.Float", "double" => "System.Double",
        "string" => "System.String",
        _ => typeName
    };

    private static string? PrimitiveKeywordAlias(string typeName) => typeName switch
    {
        "System.Bool" => "bool", "System.Char" => "char",
        "System.Int8" => "int8", "System.UInt8" => "uint8",
        "System.Int16" => "int16", "System.UInt16" => "uint16",
        "System.Int32" => "int32", "System.UInt32" => "uint32",
        "System.Int64" => "int64", "System.UInt64" => "uint64",
        "System.Int" => "int", "System.UInt" => "uint",
        "System.Float" => "float", "System.Double" => "double",
        _ => null
    };

    private IEnumerable<string> EnumerateTypeConstituents(string typeName)
    {
        if (TypeNameSyntax.TryParse(typeName, out var parsedType))
        {
            foreach (var node in parsedType.DescendantsAndSelf()) yield return node.Name;
            yield break;
        }

        // Keep tolerant handling for constraint annotations and legacy type
        // fragments until those are represented by dedicated syntax nodes.
        var normalized = typeName.Trim();
        while (normalized.EndsWith('?') || normalized.EndsWith('*'))
            normalized = normalized[..^1];
        if (TryParseManagedArrayType(normalized, out var arrayElementType, out _))
        {
            foreach (var constituent in EnumerateTypeConstituents(arrayElementType)) yield return constituent;
            yield break;
        }
        if (TryParseInlineArrayType(normalized, out var inlineElementType, out _))
        {
            foreach (var constituent in EnumerateTypeConstituents(inlineElementType)) yield return constituent;
            yield break;
        }

        var genericOpen = normalized.IndexOf('<');
        if (genericOpen >= 0)
        {
            var depth = 0;
            var genericClose = -1;
            for (var index = genericOpen; index < normalized.Length; index++)
            {
                if (normalized[index] == '<') depth++;
                else if (normalized[index] == '>' && --depth == 0) { genericClose = index; break; }
            }
            if (genericClose > genericOpen)
            {
                var genericName = normalized[..genericOpen].Trim();
                if (genericName.Length > 0) yield return genericName;
                var arguments = normalized[(genericOpen + 1)..genericClose];
                var argumentStart = 0;
                depth = 0;
                for (var index = 0; index <= arguments.Length; index++)
                {
                    if (index < arguments.Length && arguments[index] == '<') depth++;
                    else if (index < arguments.Length && arguments[index] == '>') depth--;
                    if (index != arguments.Length && (arguments[index] != ',' || depth != 0)) continue;
                    foreach (var constituent in EnumerateTypeConstituents(arguments[argumentStart..index])) yield return constituent;
                    argumentStart = index + 1;
                }
                var suffix = normalized[(genericClose + 1)..];
                if (!string.IsNullOrWhiteSpace(suffix))
                    foreach (var constituent in EnumerateTypeConstituents(genericName + suffix)) yield return constituent;
                yield break;
            }
        }
        if (normalized.Length > 0) yield return normalized;
    }

    private bool ValidateTypeAccessibility(string typeName, TypeDeclaration accessingType, SourceLocation location)
    {
        var inaccessibleTypes = EnumerateTypeConstituents(typeName)
            .Distinct(StringComparer.Ordinal)
            .SelectMany(component => FindTypeCandidates(component, accessingType))
            .Where(candidate => !IsTypeAccessible(candidate, accessingType))
            .DistinctBy(GetQualifiedTypeName)
            .ToArray();
        foreach (var inaccessible in inaccessibleTypes)
            diagnostics.Add("LMC3012", DiagnosticSeverity.Error,
                $"Type '{GetQualifiedTypeName(inaccessible)}' is inaccessible from '{GetQualifiedTypeName(accessingType)}'.", location);
        return inaccessibleTypes.Length > 0;
    }

    private static bool LambdaBodyReferencesName(LambdaExpression lambda, string name) =>
        lambda.ExpressionBody is not null && LambdaExpressionReferencesName(lambda.ExpressionBody, name) ||
        lambda.BlockBody is not null && LambdaStatementReferencesName(lambda.BlockBody, name);

    private IReadOnlyList<string> FindCapturedLambdaVariables(LambdaExpression lambda, MethodContext context)
    {
        var lambdaLocals = lambda.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        if (lambda.BlockBody is { } lambdaBody) CollectLocalNames(lambdaBody, lambdaLocals);
        var candidates = new List<string>();
        var candidateSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var parameter in context.Method.Declaration.Parameters)
            if (candidateSet.Add(parameter.Name)) candidates.Add(parameter.Name);
        if (context.Method.Declaration.Body is { } methodBody)
        {
            var locals = new HashSet<string>(StringComparer.Ordinal);
            CollectLocalNames(methodBody, locals);
            foreach (var name in locals)
                if (candidateSet.Add(name)) candidates.Add(name);
        }
        return candidates.Where(name => !lambdaLocals.Contains(name) && context.Lookup(name) is not null &&
            LambdaBodyReferencesName(lambda, name)).ToArray();
    }

    private ClosureEnvironmentBinding GetOrCreateCompositeClosureEnvironment(LambdaExpression lambda,
        IReadOnlyList<string> capturedNames, MethodContext context, bool captureThis,
        IReadOnlyList<ForStatement> forHeaderOwners, IReadOnlyList<ForInStatement> forInOwners,
        IReadOnlyList<BlockStatement> blockOwners, IReadOnlyList<string> methodCapturedNames)
    {
        var sourceEnvironments = new Dictionary<string, ClosureEnvironmentBinding>(StringComparer.Ordinal);
        var capturedVariableEnvironments = new Dictionary<string, string>(StringComparer.Ordinal);

        void AddSource(ClosureEnvironmentBinding environment, IEnumerable<string> names)
        {
            sourceEnvironments[environment.TypeName] = environment;
            foreach (var name in names) capturedVariableEnvironments[name] = environment.TypeName;
        }

        if (methodCapturedNames.Count > 0 || captureThis)
        {
            var environment = GetOrCreateClosureEnvironment(context.Method, methodCapturedNames, context, captureThis);
            AddSource(environment, methodCapturedNames);
            if (captureThis) capturedVariableEnvironments["$this"] = environment.TypeName;
        }
        foreach (var owner in forHeaderOwners)
        {
            var names = capturedNames.Where(name => ReferenceEquals(context.Lookup(name)?.ForHeaderOwner, owner)).ToArray();
            AddSource(GetOrCreateClosureEnvironment(context.Method, names, context, captureThis: false, forHeaderOwner: owner), names);
        }
        foreach (var owner in forInOwners)
        {
            var names = capturedNames.Where(name => ReferenceEquals(context.Lookup(name)?.ForInOwner, owner)).ToArray();
            AddSource(GetOrCreateClosureEnvironment(context.Method, names, context, captureThis: false, forInOwner: owner), names);
        }
        foreach (var owner in blockOwners)
        {
            var names = capturedNames.Where(name => ReferenceEquals(context.Lookup(name)?.BlockOwner, owner)).ToArray();
            AddSource(GetOrCreateClosureEnvironment(context.Method, names, context, captureThis: false, blockOwner: owner), names);
        }

        string name;
        do name = "__LumaClosure_" + _nextClosureEnvironmentIndex++;
        while (_types.Any(type => type.Namespace == context.Method.Owner.Namespace && type.Name == name));
        var declaration = new TypeDeclaration(context.Method.Owner.Namespace, "class", name, [], context.Method.Declaration.Location,
            BaseTypes: ["System.Object"], Accessibility: "private");
        _types = _types.Append(declaration).ToArray();
        var qualifiedName = GetQualifiedTypeName(declaration);
        var typeSymbol = new ClassTypeSymbol(declaration, [], "System.Object");
        _classTypes[qualifiedName] = typeSymbol;

        var parentEnvironmentFields = new Dictionary<string, string>(StringComparer.Ordinal);
        var parentFieldIndices = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var sourceType in sourceEnvironments.Keys)
        {
            var fieldName = "$scope" + parentEnvironmentFields.Count;
            var field = new FieldDeclaration(sourceType, fieldName, null, false, false, true,
                context.Method.Declaration.Location, "private");
            var index = typeSymbol.Fields.Count;
            var updatedDeclaration = typeSymbol.Declaration with { Members = typeSymbol.Declaration.Members.Append(field).ToArray() };
            var updatedFields = typeSymbol.Fields.Append(new StructFieldSymbol(updatedDeclaration, field, sourceType, index)).ToArray();
            _types = _types.Select(type => ReferenceEquals(type, typeSymbol.Declaration) ? updatedDeclaration : type).ToArray();
            typeSymbol = typeSymbol with { Declaration = updatedDeclaration, Fields = updatedFields };
            _classTypes[qualifiedName] = typeSymbol;
            parentEnvironmentFields[sourceType] = fieldName;
            parentFieldIndices[fieldName] = index;
        }

        var environmentBinding = new ClosureEnvironmentBinding(qualifiedName, parentFieldIndices, context.Method,
            parentEnvironmentFields, capturedVariableEnvironments);
        _closureEnclosingMethods[qualifiedName] = context.Method;
        _compositeClosureEnvironments[lambda] = environmentBinding;
        return environmentBinding;
    }

    private ClosureEnvironmentBinding GetOrCreateClosureEnvironment(MethodSymbol method, IReadOnlyList<string> capturedNames,
        MethodContext context, bool captureThis, ForStatement? forHeaderOwner = null,
        ForInStatement? forInOwner = null, BlockStatement? blockOwner = null)
    {
        ClosureEnvironmentBinding? environment;
        var hasEnvironment = forHeaderOwner is not null
            ? _forHeaderClosureEnvironments.TryGetValue(forHeaderOwner, out environment)
            : forInOwner is not null
                ? _forInClosureEnvironments.TryGetValue(forInOwner, out environment)
                : blockOwner is not null
                    ? _blockClosureEnvironments.TryGetValue(blockOwner, out environment)
                    : _closureEnvironments.TryGetValue(method, out environment);
        if (!hasEnvironment)
        {
            string name;
            do name = "__LumaClosure_" + _nextClosureEnvironmentIndex++;
            while (_types.Any(type => type.Namespace == method.Owner.Namespace && type.Name == name));
            var declaration = new TypeDeclaration(method.Owner.Namespace, "class", name, [], method.Declaration.Location,
                BaseTypes: ["System.Object"], Accessibility: "private");
            _types = _types.Append(declaration).ToArray();
            _classTypes[GetQualifiedTypeName(declaration)] = new(declaration, [], "System.Object");
            environment = new(GetQualifiedTypeName(declaration), new Dictionary<string, int>(StringComparer.Ordinal), method);
            _closureEnclosingMethods[environment.TypeName] = method;
        }

        if (environment is null) throw new InvalidOperationException("Closure environment lookup produced no environment.");
        var typeSymbol = _classTypes[environment.TypeName];
        var capturedFields = new Dictionary<string, int>(environment.CapturedFieldIndices, StringComparer.Ordinal);
        if (captureThis && !capturedFields.ContainsKey("$this"))
        {
            var thisField = new FieldDeclaration(GetQualifiedTypeName(method.Owner), "$this", null, false, false, true, method.Declaration.Location, "public");
            var thisIndex = typeSymbol.Fields.Count;
            var updatedDeclaration = typeSymbol.Declaration with { Members = typeSymbol.Declaration.Members.Append(thisField).ToArray() };
            var updatedFields = typeSymbol.Fields.Append(new StructFieldSymbol(updatedDeclaration, thisField, thisField.TypeName, thisIndex)).ToArray();
            _types = _types.Select(type => ReferenceEquals(type, typeSymbol.Declaration) ? updatedDeclaration : type).ToArray();
            typeSymbol = typeSymbol with { Declaration = updatedDeclaration, Fields = updatedFields };
            _classTypes[environment.TypeName] = typeSymbol;
            capturedFields["$this"] = thisIndex;
        }
        foreach (var name in capturedNames)
        {
            if (capturedFields.ContainsKey(name)) continue;
            var local = context.Lookup(name)!;
            var field = new FieldDeclaration(local.Type, name, null, false, false, !local.Mutable, method.Declaration.Location, "public");
            var index = typeSymbol.Fields.Count;
            var updatedDeclaration = typeSymbol.Declaration with { Members = typeSymbol.Declaration.Members.Append(field).ToArray() };
            var updatedFields = typeSymbol.Fields.Append(new StructFieldSymbol(updatedDeclaration, field, local.Type, index)).ToArray();
            _types = _types.Select(type => ReferenceEquals(type, typeSymbol.Declaration) ? updatedDeclaration : type).ToArray();
            typeSymbol = typeSymbol with { Declaration = updatedDeclaration, Fields = updatedFields };
            _classTypes[environment.TypeName] = typeSymbol;
            capturedFields[name] = index;
        }
        environment = environment with { CapturedFieldIndices = capturedFields };
        if (forHeaderOwner is not null) _forHeaderClosureEnvironments[forHeaderOwner] = environment;
        else if (forInOwner is not null) _forInClosureEnvironments[forInOwner] = environment;
        else if (blockOwner is not null) _blockClosureEnvironments[blockOwner] = environment;
        else _closureEnvironments[method] = environment;
        return environment;
    }

    private void MarkScopeBoundCaptureFields(IReadOnlyList<string> capturedNames, MethodContext context)
    {
        foreach (var name in capturedNames)
        {
            var local = context.Lookup(name);
            if (local is null) continue;
            var depth = local.ScopeBoundDepth ?? (local.IsReadOnlyReference ? local.ScopeDepth : (int?)null);
            if (depth is null) continue;
            ClosureEnvironmentBinding? environment = local.ForHeaderOwner is { } forOwner
                ? _forHeaderClosureEnvironments.GetValueOrDefault(forOwner)
                : local.ForInOwner is { } forInOwner
                    ? _forInClosureEnvironments.GetValueOrDefault(forInOwner)
                    : local.BlockOwner is { } blockOwner
                        ? _blockClosureEnvironments.GetValueOrDefault(blockOwner)
                        : _closureEnvironments.GetValueOrDefault(context.Method);
            if (environment is null || !environment.CapturedFieldIndices.TryGetValue(name, out var fieldIndex))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                    $"The scope-bound capture '{name}' could not be represented in its closure environment.", context.Method.Declaration.Location);
                continue;
            }

            var borrowedFields = new HashSet<int>(environment.BorrowedCaptureFieldIndices ?? new HashSet<int>());
            if (IsManagedReferenceValue(local.Type))
            {
                // A captured parameter is borrowed from its caller, but a captured local
                // alias owns the reference acquired when that local is initialized or
                // assigned. The closure environment must release the latter with its
                // other managed fields.
                if (local.Declaration is null) borrowedFields.Add(fieldIndex);
                else borrowedFields.Remove(fieldIndex);
                _scopeBoundClosureFields[(environment.TypeName, name)] = depth.Value;
            }
            var updated = environment with { BorrowedCaptureFieldIndices = borrowedFields };
            ReplaceEnvironment(_closureEnvironments, environment.TypeName, updated);
            ReplaceEnvironment(_forHeaderClosureEnvironments, environment.TypeName, updated);
            ReplaceEnvironment(_forInClosureEnvironments, environment.TypeName, updated);
            ReplaceEnvironment(_blockClosureEnvironments, environment.TypeName, updated);
            _scopeBoundClosureEnvironmentDepths[environment.TypeName] = Math.Max(
                _scopeBoundClosureEnvironmentDepths.GetValueOrDefault(environment.TypeName, -1), depth.Value);

            foreach (var composite in _compositeClosureEnvironments.Values.Where(candidate =>
                         candidate.CapturedVariableEnvironments?.GetValueOrDefault(name) == environment.TypeName))
                _scopeBoundClosureEnvironmentDepths[composite.TypeName] = Math.Max(
                    _scopeBoundClosureEnvironmentDepths.GetValueOrDefault(composite.TypeName, -1), depth.Value);
        }

        static void ReplaceEnvironment<TKey>(Dictionary<TKey, ClosureEnvironmentBinding> environments,
            string typeName, ClosureEnvironmentBinding replacement) where TKey : notnull
        {
            foreach (var key in environments.Where(pair => pair.Value.TypeName == typeName).Select(pair => pair.Key).ToArray())
                environments[key] = replacement;
        }
    }

    private LambdaExpression RewriteLambdaForClosure(LambdaExpression lambda, MethodContext context,
        ClosureEnvironmentBinding? compositeEnvironment = null)
    {
        var shadowed = lambda.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        if (lambda.BlockBody is { } body) CollectLocalNames(body, shadowed);
        var capturedLocals = FindCapturedLambdaVariables(lambda, context).ToHashSet(StringComparer.Ordinal);
        var enclosingTypeName = GetQualifiedTypeName(context.Method.Owner);

        bool IsInstanceMember(string name)
        {
            if (name == "ToString") return true;
            if (_structTypes.TryGetValue(enclosingTypeName, out var structure) &&
                (structure.Fields.Any(field => !field.Declaration.IsStatic && field.Declaration.Name == name) ||
                 _methods.Concat(_genericMethodDefinitions).Any(method => method.IsStructInstanceMethod &&
                     GetQualifiedTypeName(method.Owner) == enclosingTypeName && method.Declaration.Name == name) ||
                 _computedProperties.ContainsKey(enclosingTypeName + "." + name))) return true;
            foreach (var ownerName in GetClassChain(enclosingTypeName))
            {
                if (_classTypes.TryGetValue(ownerName, out var type) && type.Fields.Any(field => !field.Declaration.IsStatic && field.Declaration.Name == name)) return true;
                if (_methods.Any(method => method.IsClassInstanceMethod && GetQualifiedTypeName(method.Owner) == ownerName && method.Declaration.Name == name)) return true;
                if (_computedProperties.ContainsKey(ownerName + "." + name) || _computedIndexers.ContainsKey(ownerName + "." + name)) return true;
            }
            return false;
        }

        bool IsStaticMember(string name) => _staticFields.ContainsKey(enclosingTypeName + "." + name) ||
            _constantFields.ContainsKey(enclosingTypeName + "." + name) ||
            _methods.Any(method => method.Declaration.IsStatic && GetQualifiedTypeName(method.Owner) == enclosingTypeName && method.Declaration.Name == name);

        Expression CompositeCapture(string sourceEnvironmentType, string capturedName, SourceLocation location)
        {
            if (compositeEnvironment?.ParentEnvironmentFields is not { } parentFields ||
                !parentFields.TryGetValue(sourceEnvironmentType, out var parentFieldName))
                throw new InvalidOperationException($"Composite closure environment has no parent field for '{sourceEnvironmentType}'.");
            return new MemberAccessExpression(
                new MemberAccessExpression(new ThisExpression(location), parentFieldName, location),
                capturedName, location);
        }

        Expression RewriteExpression(Expression expression) => expression switch
        {
            ThisExpression self when compositeEnvironment?.CapturedVariableEnvironments?.TryGetValue("$this", out var thisEnvironment) == true
                => CompositeCapture(thisEnvironment, "$this", self.Location),
            ThisExpression self => new NameExpression("$this", self.Location),
            NameExpression name when name.Name == "$this" && compositeEnvironment?.CapturedVariableEnvironments?.TryGetValue("$this", out var thisEnvironment) == true
                => CompositeCapture(thisEnvironment, "$this", name.Location),
            NameExpression name when compositeEnvironment?.CapturedVariableEnvironments?.TryGetValue(name.Name, out var sourceEnvironment) == true
                => CompositeCapture(sourceEnvironment, name.Name, name.Location),
            NameExpression name when name.Name != "$this" && !shadowed.Contains(name.Name) && !capturedLocals.Contains(name.Name) && context.Lookup(name.Name) is null && IsInstanceMember(name.Name)
                => new MemberAccessExpression(new NameExpression("$this", name.Location), name.Name, name.Location),
            NameExpression name when name.Name != "$this" && !shadowed.Contains(name.Name) && !capturedLocals.Contains(name.Name) && context.Lookup(name.Name) is null && IsStaticMember(name.Name)
                => new MemberAccessExpression(new NameExpression(context.Method.Owner.Name, name.Location), name.Name, name.Location),
            InlineArrayInitializerExpression initializer => initializer with { Elements = initializer.Elements.Select(RewriteExpression).ToArray() },
            AllocationExpression allocation => allocation with
            {
                Dimensions = allocation.Dimensions.Select(RewriteExpression).ToArray(),
                Initializer = allocation.Initializer is null ? null : (InlineArrayInitializerExpression)RewriteExpression(allocation.Initializer),
                ConstructorArguments = allocation.ConstructorArguments?.Select(RewriteExpression).ToArray()
            },
            UnaryExpression unary => unary with { Operand = RewriteExpression(unary.Operand) },
            CastExpression cast => cast with { Operand = RewriteExpression(cast.Operand) },
            BinaryExpression binary => binary with { Left = RewriteExpression(binary.Left), Right = RewriteExpression(binary.Right) },
            ConditionalExpression conditional => conditional with
            {
                Condition = RewriteExpression(conditional.Condition), WhenTrue = RewriteExpression(conditional.WhenTrue), WhenFalse = RewriteExpression(conditional.WhenFalse)
            },
            MemberAccessExpression member => member with { Receiver = RewriteExpression(member.Receiver) },
            IndexExpression index => index with { Receiver = RewriteExpression(index.Receiver), Indices = index.Indices.Select(RewriteExpression).ToArray() },
            CallExpression call => call with { Target = RewriteExpression(call.Target), Arguments = call.Arguments.Select(RewriteExpression).ToArray() },
            OverflowContextExpression overflow => overflow with { Value = RewriteExpression(overflow.Value) },
            NullAssertionExpression assertion => assertion with { Operand = RewriteExpression(assertion.Operand) },
            _ => expression
        };

        SwitchLabel RewriteLabel(SwitchLabel label) => label with
        {
            Value = label.Value is null ? null : RewriteExpression(label.Value),
            Guard = label.Guard is null ? null : RewriteExpression(label.Guard),
            LeftPattern = label.LeftPattern is null ? null : RewriteLabel(label.LeftPattern),
            RightPattern = label.RightPattern is null ? null : RewriteLabel(label.RightPattern)
        };

        Statement RewriteStatement(Statement statement) => statement switch
        {
            BlockStatement block => block with { Statements = block.Statements.Select(RewriteStatement).ToArray() },
            ReturnStatement returned => returned with { Value = returned.Value is null ? null : RewriteExpression(returned.Value) },
            VariableStatement variable => variable with { Initializer = variable.Initializer is null ? null : RewriteExpression(variable.Initializer) },
            VariableDeclarationStatement declaration => declaration with { Variables = declaration.Variables.Select(variable => (VariableStatement)RewriteStatement(variable)).ToArray() },
            MultiAssignmentStatement assignment => assignment with
            {
                Targets = assignment.Targets.Select(RewriteExpression).ToArray(), Values = assignment.Values.Select(RewriteExpression).ToArray()
            },
            ExpressionStatement expressionStatement => expressionStatement with { Value = RewriteExpression(expressionStatement.Value) },
            IfStatement conditional => conditional with
            {
                Condition = RewriteExpression(conditional.Condition), Then = RewriteStatement(conditional.Then),
                Else = conditional.Else is null ? null : RewriteStatement(conditional.Else)
            },
            WhileStatement loop => loop with { Condition = RewriteExpression(loop.Condition), Body = RewriteStatement(loop.Body) },
            DoWhileStatement loop => loop with { Body = RewriteStatement(loop.Body), Condition = RewriteExpression(loop.Condition) },
            ForStatement loop => loop with
            {
                Initializer = loop.Initializer is null ? null : RewriteStatement(loop.Initializer),
                Condition = loop.Condition is null ? null : RewriteExpression(loop.Condition),
                Iterators = loop.Iterators.Select(RewriteExpression).ToArray(), Body = RewriteStatement(loop.Body)
            },
            ForInStatement loop => loop with { Collection = RewriteExpression(loop.Collection), Body = RewriteStatement(loop.Body) },
            SwitchStatement selection => selection with
            {
                Value = RewriteExpression(selection.Value),
                Sections = selection.Sections.Select(section =>
                {
                    var statements = section.Statements.Select(RewriteStatement).ToArray();
                    return section with
                    {
                        Labels = section.Labels.Select(RewriteLabel).ToArray(),
                        Statements = statements,
                        CaptureScope = new BlockStatement(statements, section.At)
                    };
                }).ToArray()
            },
            TryStatement tryStatement => tryStatement with
            {
                Body = (BlockStatement)RewriteStatement(tryStatement.Body),
                Catches = tryStatement.Catches.Select(clause => clause with { Body = (BlockStatement)RewriteStatement(clause.Body) }).ToArray(),
                Finally = tryStatement.Finally is null ? null : (BlockStatement)RewriteStatement(tryStatement.Finally)
            },
            ThrowStatement thrown => thrown with { Value = thrown.Value is null ? null : RewriteExpression(thrown.Value) },
            OverflowContextStatement overflow => overflow with { Body = RewriteStatement(overflow.Body) },
            _ => statement
        };

        return lambda with
        {
            ExpressionBody = lambda.ExpressionBody is null ? null : RewriteExpression(lambda.ExpressionBody),
            BlockBody = lambda.BlockBody is null ? null : (BlockStatement)RewriteStatement(lambda.BlockBody)
        };
    }

    private bool LambdaReferencesImplicitInstanceMember(LambdaExpression lambda, MethodContext context)
    {
        if (context.Method.Owner.Kind is not ("class" or "struct") || context.Method.Declaration.IsStatic) return false;
        var lambdaParameters = lambda.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        if (lambda.BlockBody is { } lambdaBody) CollectLocalNames(lambdaBody, lambdaParameters);
        var typeName = GetQualifiedTypeName(context.Method.Owner);
        var ownerNames = context.Method.Owner.Kind == "class" ? GetClassChain(typeName) : [typeName];
        foreach (var ownerName in ownerNames)
        {
            var hasInstanceClassField = _classTypes.TryGetValue(ownerName, out var classType) && classType.Fields.Any(field =>
                    !field.Declaration.IsStatic && !lambdaParameters.Contains(field.Declaration.Name) &&
                    context.Lookup(field.Declaration.Name) is null && LambdaBodyReferencesName(lambda, field.Declaration.Name));
            var hasInstanceStructField = _structTypes.TryGetValue(ownerName, out var structType) && structType.Fields.Any(field =>
                    !field.Declaration.IsStatic && !lambdaParameters.Contains(field.Declaration.Name) &&
                    context.Lookup(field.Declaration.Name) is null && LambdaBodyReferencesName(lambda, field.Declaration.Name));
            if (hasInstanceClassField || hasInstanceStructField)
                return true;
            if (_methods.Any(method => (method.IsClassInstanceMethod || method.IsStructInstanceMethod) && GetQualifiedTypeName(method.Owner) == ownerName &&
                    !lambdaParameters.Contains(method.Declaration.Name) && LambdaBodyReferencesName(lambda, method.Declaration.Name)))
                return true;
            if (ownerName != "System.Object" && !lambdaParameters.Contains("ToString") && context.Lookup("ToString") is null &&
                LambdaBodyReferencesName(lambda, "ToString"))
                return true;
            if (_computedProperties.Keys.Any(member => member.StartsWith(ownerName + ".", StringComparison.Ordinal) &&
                    !lambdaParameters.Contains(member[(member.LastIndexOf('.') + 1)..]) &&
                    LambdaBodyReferencesName(lambda, member[(member.LastIndexOf('.') + 1)..])))
                return true;
        }
        return false;
    }

    private bool LambdaMutatesEnclosingInstance(LambdaExpression lambda, MethodContext context)
    {
        var ownerName = GetQualifiedTypeName(context.Method.Owner);
        var ownerNames = context.Method.Owner.Kind == "class" ? GetClassChain(ownerName) : [ownerName];
        var lambdaNames = lambda.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        if (lambda.BlockBody is { } body) CollectLocalNames(body, lambdaNames);
        bool IsInstanceName(string name) => !lambdaNames.Contains(name) && context.Lookup(name) is null &&
            ownerNames.Any(owner =>
                _classTypes.TryGetValue(owner, out var classType) && classType.Fields.Any(field => !field.Declaration.IsStatic && field.Declaration.Name == name) ||
                _structTypes.TryGetValue(owner, out var structType) && structType.Fields.Any(field => !field.Declaration.IsStatic && field.Declaration.Name == name) ||
                _methods.Concat(_genericMethodDefinitions).Any(method => (method.IsClassInstanceMethod || method.IsStructInstanceMethod) && GetQualifiedTypeName(method.Owner) == owner && method.Declaration.Name == name) ||
                _computedProperties.ContainsKey(owner + "." + name));
        bool IsReceiverMember(Expression expression) => expression switch
        {
            MemberAccessExpression member => member.Receiver is ThisExpression or BaseExpression || IsReceiverMember(member.Receiver),
            IndexExpression index => index.Receiver is ThisExpression or BaseExpression || IsReceiverMember(index.Receiver),
            NameExpression name => IsInstanceName(name.Name),
            _ => false
        };
        bool IsMutatingMethod(string name) => ownerNames.Any(owner => _methods.Concat(_genericMethodDefinitions).Any(method =>
            (method.IsClassInstanceMethod || method.IsStructInstanceMethod) && method.Declaration.IsMutating &&
            GetQualifiedTypeName(method.Owner) == owner && method.Declaration.Name == name));
        bool ExpressionMutates(Expression expression) => expression switch
        {
            BinaryExpression binary => (binary.Operator is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "??=" && IsReceiverMember(binary.Left)) ||
                ExpressionMutates(binary.Left) || ExpressionMutates(binary.Right),
            UnaryExpression unary => (unary.Operator is "++" or "--" && IsReceiverMember(unary.Operand)) || ExpressionMutates(unary.Operand),
            CallExpression call => call.Target switch
            {
                NameExpression name when IsInstanceName(name.Name) && IsMutatingMethod(name.Name) => true,
                MemberAccessExpression { Receiver: ThisExpression or BaseExpression, Member: var memberName } when IsMutatingMethod(memberName) => true,
                MemberAccessExpression member => ExpressionMutates(member.Receiver),
                _ => ExpressionMutates(call.Target)
            } || call.Arguments.Any(ExpressionMutates),
            MemberAccessExpression member => ExpressionMutates(member.Receiver),
            IndexExpression index => ExpressionMutates(index.Receiver) || index.Indices.Any(ExpressionMutates),
            CastExpression cast => ExpressionMutates(cast.Operand),
            ConditionalExpression conditional => ExpressionMutates(conditional.Condition) || ExpressionMutates(conditional.WhenTrue) || ExpressionMutates(conditional.WhenFalse),
            InlineArrayInitializerExpression initializer => initializer.Elements.Any(ExpressionMutates),
            AllocationExpression allocation => allocation.Dimensions.Any(ExpressionMutates) || allocation.Initializer is not null && ExpressionMutates(allocation.Initializer) ||
                allocation.ConstructorArguments is not null && allocation.ConstructorArguments.Any(ExpressionMutates),
            NullAssertionExpression assertion => ExpressionMutates(assertion.Operand),
            OverflowContextExpression overflow => ExpressionMutates(overflow.Value),
            _ => false
        };
        bool StatementMutates(Statement statement) => statement switch
        {
            BlockStatement block => block.Statements.Any(StatementMutates),
            ReturnStatement returned => returned.Value is not null && ExpressionMutates(returned.Value),
            VariableStatement variable => variable.Initializer is not null && ExpressionMutates(variable.Initializer),
            VariableDeclarationStatement declaration => declaration.Variables.Any(variable => variable.Initializer is not null && ExpressionMutates(variable.Initializer)),
            MultiAssignmentStatement assignment => assignment.Targets.Any(ExpressionMutates) || assignment.Values.Any(ExpressionMutates),
            ExpressionStatement expression => ExpressionMutates(expression.Value),
            IfStatement conditional => ExpressionMutates(conditional.Condition) || StatementMutates(conditional.Then) || conditional.Else is not null && StatementMutates(conditional.Else),
            WhileStatement loop => ExpressionMutates(loop.Condition) || StatementMutates(loop.Body),
            DoWhileStatement loop => StatementMutates(loop.Body) || ExpressionMutates(loop.Condition),
            ForStatement loop => loop.Initializer is not null && StatementMutates(loop.Initializer) || loop.Condition is not null && ExpressionMutates(loop.Condition) ||
                loop.Iterators.Any(ExpressionMutates) || StatementMutates(loop.Body),
            ForInStatement loop => ExpressionMutates(loop.Collection) || StatementMutates(loop.Body),
            SwitchStatement selection => ExpressionMutates(selection.Value) || selection.Sections.Any(section =>
                section.Labels.Any(label => label.Value is not null && ExpressionMutates(label.Value) || label.Guard is not null && ExpressionMutates(label.Guard)) ||
                section.Statements.Any(StatementMutates)),
            TryStatement tryStatement => StatementMutates(tryStatement.Body) || tryStatement.Catches.Any(clause => StatementMutates(clause.Body)) ||
                tryStatement.Finally is not null && StatementMutates(tryStatement.Finally),
            ThrowStatement thrown => thrown.Value is not null && ExpressionMutates(thrown.Value),
            OverflowContextStatement overflow => StatementMutates(overflow.Body),
            _ => false
        };
        return lambda.ExpressionBody is not null && ExpressionMutates(lambda.ExpressionBody) || lambda.BlockBody is not null && StatementMutates(lambda.BlockBody);
    }

    private static void CollectLocalNames(Statement statement, HashSet<string> names)
    {
        switch (statement)
        {
            case BlockStatement block:
                foreach (var child in block.Statements) CollectLocalNames(child, names);
                break;
            case VariableStatement variable:
                names.Add(variable.Name);
                if (variable.Initializer is not null) CollectTypePatternNames(variable.Initializer, names);
                break;
            case VariableDeclarationStatement declaration:
                foreach (var variable in declaration.Variables)
                {
                    names.Add(variable.Name);
                    if (variable.Initializer is not null) CollectTypePatternNames(variable.Initializer, names);
                }
                break;
            case ReturnStatement returned when returned.Value is not null:
                CollectTypePatternNames(returned.Value, names);
                break;
            case ExpressionStatement expression:
                CollectTypePatternNames(expression.Value, names);
                break;
            case ThrowStatement thrown when thrown.Value is not null:
                CollectTypePatternNames(thrown.Value, names);
                break;
            case MultiAssignmentStatement assignment:
                foreach (var target in assignment.Targets) CollectTypePatternNames(target, names);
                foreach (var value in assignment.Values) CollectTypePatternNames(value, names);
                break;
            case IfStatement conditional:
                CollectTypePatternNames(conditional.Condition, names);
                CollectLocalNames(conditional.Then, names);
                if (conditional.Else is not null) CollectLocalNames(conditional.Else, names);
                break;
            case WhileStatement loop: CollectTypePatternNames(loop.Condition, names); CollectLocalNames(loop.Body, names); break;
            case DoWhileStatement loop: CollectLocalNames(loop.Body, names); CollectTypePatternNames(loop.Condition, names); break;
            case ForStatement loop:
                if (loop.Initializer is not null) CollectLocalNames(loop.Initializer, names);
                if (loop.Condition is not null) CollectTypePatternNames(loop.Condition, names);
                foreach (var iterator in loop.Iterators) CollectTypePatternNames(iterator, names);
                CollectLocalNames(loop.Body, names);
                break;
            case ForInStatement loop:
                if (loop.Name is { } loopName) names.Add(loopName);
                CollectTypePatternNames(loop.Collection, names);
                CollectLocalNames(loop.Body, names);
                break;
            case SwitchStatement selection:
                CollectTypePatternNames(selection.Value, names);
                foreach (var section in selection.Sections)
                {
                    foreach (var label in section.Labels)
                    {
                        if (label.Value is not null) CollectTypePatternNames(label.Value, names);
                        if (label.Guard is not null) CollectTypePatternNames(label.Guard, names);
                        foreach (var pattern in GetSwitchPatternVariableNodes(label))
                            if (pattern.PatternVariableName is { } patternName) names.Add(patternName);
                    }
                    foreach (var child in section.Statements) CollectLocalNames(child, names);
                }
                break;
            case TryStatement tryStatement:
                CollectLocalNames(tryStatement.Body, names);
                foreach (var clause in tryStatement.Catches)
                {
                    if (clause.VariableName is { } catchName) names.Add(catchName);
                    CollectLocalNames(clause.Body, names);
                }
                if (tryStatement.Finally is not null) CollectLocalNames(tryStatement.Finally, names);
                break;
            case OverflowContextStatement overflow: CollectLocalNames(overflow.Body, names); break;
        }
    }

    private static void CollectTypePatternNames(Expression expression, HashSet<string> names)
    {
        switch (expression)
        {
            case InlineOutVariableExpression inlineOut:
                names.Add(inlineOut.Name);
                break;
            case BinaryExpression binary:
                if (binary.Operator == "is" && binary.PatternVariableName is { } patternName) names.Add(patternName);
                CollectTypePatternNames(binary.Left, names);
                CollectTypePatternNames(binary.Right, names);
                break;
            case UnaryExpression unary: CollectTypePatternNames(unary.Operand, names); break;
            case CastExpression cast: CollectTypePatternNames(cast.Operand, names); break;
            case ConditionalExpression conditional:
                CollectTypePatternNames(conditional.Condition, names);
                CollectTypePatternNames(conditional.WhenTrue, names);
                CollectTypePatternNames(conditional.WhenFalse, names);
                break;
            case MemberAccessExpression member: CollectTypePatternNames(member.Receiver, names); break;
            case IndexExpression index:
                CollectTypePatternNames(index.Receiver, names);
                foreach (var item in index.Indices) CollectTypePatternNames(item, names);
                break;
            case CallExpression call:
                CollectTypePatternNames(call.Target, names);
                foreach (var argument in call.Arguments) CollectTypePatternNames(argument, names);
                break;
            case AllocationExpression allocation:
                foreach (var dimension in allocation.Dimensions) CollectTypePatternNames(dimension, names);
                if (allocation.Initializer is not null) CollectTypePatternNames(allocation.Initializer, names);
                if (allocation.ConstructorArguments is not null)
                    foreach (var argument in allocation.ConstructorArguments) CollectTypePatternNames(argument, names);
                break;
            case InlineArrayInitializerExpression initializer:
                foreach (var element in initializer.Elements) CollectTypePatternNames(element, names);
                break;
            case NullAssertionExpression assertion: CollectTypePatternNames(assertion.Operand, names); break;
            case OverflowContextExpression overflow: CollectTypePatternNames(overflow.Value, names); break;
        }
    }

    private static bool LambdaBodyReferencesThis(LambdaExpression lambda) =>
        lambda.ExpressionBody is not null && LambdaExpressionReferencesThis(lambda.ExpressionBody) ||
        lambda.BlockBody is not null && LambdaStatementReferencesThis(lambda.BlockBody);

    private static bool LambdaStatementReferencesThis(Statement statement) => statement switch
    {
        BlockStatement block => block.Statements.Any(LambdaStatementReferencesThis),
        ReturnStatement ret => ret.Value is not null && LambdaExpressionReferencesThis(ret.Value),
        VariableStatement variable => variable.Initializer is not null && LambdaExpressionReferencesThis(variable.Initializer),
        VariableDeclarationStatement declaration => declaration.Variables.Any(variable => variable.Initializer is not null && LambdaExpressionReferencesThis(variable.Initializer)),
        MultiAssignmentStatement assignment => assignment.Targets.Any(LambdaExpressionReferencesThis) || assignment.Values.Any(LambdaExpressionReferencesThis),
        ExpressionStatement expression => LambdaExpressionReferencesThis(expression.Value),
        IfStatement conditional => LambdaExpressionReferencesThis(conditional.Condition) || LambdaStatementReferencesThis(conditional.Then) || conditional.Else is not null && LambdaStatementReferencesThis(conditional.Else),
        WhileStatement loop => LambdaExpressionReferencesThis(loop.Condition) || LambdaStatementReferencesThis(loop.Body),
        DoWhileStatement loop => LambdaExpressionReferencesThis(loop.Condition) || LambdaStatementReferencesThis(loop.Body),
        ForStatement loop => loop.Condition is not null && LambdaExpressionReferencesThis(loop.Condition) || loop.Iterators.Any(LambdaExpressionReferencesThis) ||
            loop.Initializer is not null && LambdaStatementReferencesThis(loop.Initializer) || LambdaStatementReferencesThis(loop.Body),
        ForInStatement loop => LambdaExpressionReferencesThis(loop.Collection) || LambdaStatementReferencesThis(loop.Body),
        SwitchStatement selection => LambdaExpressionReferencesThis(selection.Value) || selection.Sections.Any(section =>
            section.Labels.Any(label => label.Value is not null && LambdaExpressionReferencesThis(label.Value) || label.Guard is not null && LambdaExpressionReferencesThis(label.Guard)) ||
            section.Statements.Any(LambdaStatementReferencesThis)),
        TryStatement tryStatement => LambdaStatementReferencesThis(tryStatement.Body) || tryStatement.Catches.Any(clause => LambdaStatementReferencesThis(clause.Body)) ||
            tryStatement.Finally is not null && LambdaStatementReferencesThis(tryStatement.Finally),
        ThrowStatement thrown => thrown.Value is not null && LambdaExpressionReferencesThis(thrown.Value),
        OverflowContextStatement overflow => LambdaStatementReferencesThis(overflow.Body),
        _ => false
    };

    private static bool LambdaExpressionReferencesThis(Expression expression) => expression switch
    {
        ThisExpression or BaseExpression => true,
        LambdaExpression nested => LambdaBodyReferencesThis(nested),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(LambdaExpressionReferencesThis),
        AllocationExpression allocation => allocation.Dimensions.Any(LambdaExpressionReferencesThis) || allocation.Initializer is not null && LambdaExpressionReferencesThis(allocation.Initializer) ||
            allocation.ConstructorArguments is not null && allocation.ConstructorArguments.Any(LambdaExpressionReferencesThis),
        UnaryExpression unary => LambdaExpressionReferencesThis(unary.Operand),
        CastExpression cast => LambdaExpressionReferencesThis(cast.Operand),
        BinaryExpression binary => LambdaExpressionReferencesThis(binary.Left) || LambdaExpressionReferencesThis(binary.Right),
        ConditionalExpression conditional => LambdaExpressionReferencesThis(conditional.Condition) || LambdaExpressionReferencesThis(conditional.WhenTrue) || LambdaExpressionReferencesThis(conditional.WhenFalse),
        MemberAccessExpression access => LambdaExpressionReferencesThis(access.Receiver),
        IndexExpression index => LambdaExpressionReferencesThis(index.Receiver) || index.Indices.Any(LambdaExpressionReferencesThis),
        CallExpression call => LambdaExpressionReferencesThis(call.Target) || call.Arguments.Any(LambdaExpressionReferencesThis),
        OverflowContextExpression overflow => LambdaExpressionReferencesThis(overflow.Value),
        NullAssertionExpression assertion => LambdaExpressionReferencesThis(assertion.Operand),
        _ => false
    };

    private static bool LambdaStatementReferencesName(Statement statement, string name) => statement switch
    {
        BlockStatement block => block.Statements.Any(child => LambdaStatementReferencesName(child, name)),
        ReturnStatement ret => ret.Value is not null && LambdaExpressionReferencesName(ret.Value, name),
        VariableStatement variable => variable.Initializer is not null && LambdaExpressionReferencesName(variable.Initializer, name),
        VariableDeclarationStatement declaration => declaration.Variables.Any(variable => LambdaStatementReferencesName(variable, name)),
        MultiAssignmentStatement assignment => assignment.Targets.Any(target => LambdaExpressionReferencesName(target, name)) || assignment.Values.Any(value => LambdaExpressionReferencesName(value, name)),
        ExpressionStatement expression => LambdaExpressionReferencesName(expression.Value, name),
        IfStatement conditional => LambdaExpressionReferencesName(conditional.Condition, name) || LambdaStatementReferencesName(conditional.Then, name) || conditional.Else is not null && LambdaStatementReferencesName(conditional.Else, name),
        WhileStatement loop => LambdaExpressionReferencesName(loop.Condition, name) || LambdaStatementReferencesName(loop.Body, name),
        DoWhileStatement loop => LambdaStatementReferencesName(loop.Body, name) || LambdaExpressionReferencesName(loop.Condition, name),
        ForStatement loop => loop.Initializer is not null && LambdaStatementReferencesName(loop.Initializer, name) ||
            loop.Condition is not null && LambdaExpressionReferencesName(loop.Condition, name) ||
            loop.Iterators.Any(iterator => LambdaExpressionReferencesName(iterator, name)) || LambdaStatementReferencesName(loop.Body, name),
        ForInStatement loop => LambdaExpressionReferencesName(loop.Collection, name) || loop.Name != name && LambdaStatementReferencesName(loop.Body, name),
        SwitchStatement @switch => LambdaExpressionReferencesName(@switch.Value, name) || @switch.Sections.Any(section =>
            section.Labels.Any(label => LambdaSwitchLabelReferencesName(label, name)) || section.Statements.Any(child => LambdaStatementReferencesName(child, name))),
        TryStatement @try => LambdaStatementReferencesName(@try.Body, name) || @try.Catches.Any(@catch =>
            (@catch.VariableName != name && LambdaStatementReferencesName(@catch.Body, name))) ||
            @try.Finally is not null && LambdaStatementReferencesName(@try.Finally, name),
        ThrowStatement thrown => thrown.Value is not null && LambdaExpressionReferencesName(thrown.Value, name),
        OverflowContextStatement overflow => LambdaStatementReferencesName(overflow.Body, name),
        _ => false
    };

    private static bool LambdaSwitchLabelReferencesName(SwitchLabel label, string name) =>
        label.Value is not null && LambdaExpressionReferencesName(label.Value, name) ||
        label.Guard is not null && LambdaExpressionReferencesName(label.Guard, name) ||
        label.LeftPattern is not null && LambdaSwitchLabelReferencesName(label.LeftPattern, name) ||
        label.RightPattern is not null && LambdaSwitchLabelReferencesName(label.RightPattern, name);

    private static bool LambdaExpressionReferencesName(Expression expression, string name) => expression switch
    {
        NameExpression reference => reference.Name == name,
        LambdaExpression nested => !nested.Parameters.Any(parameter => parameter.Name == name) && LambdaBodyReferencesName(nested, name),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(element => LambdaExpressionReferencesName(element, name)),
        AllocationExpression allocation => allocation.Dimensions.Any(dimension => LambdaExpressionReferencesName(dimension, name)) ||
            allocation.Initializer is not null && LambdaExpressionReferencesName(allocation.Initializer, name) ||
            allocation.ConstructorArguments is not null && allocation.ConstructorArguments.Any(argument => LambdaExpressionReferencesName(argument, name)),
        UnaryExpression unary => LambdaExpressionReferencesName(unary.Operand, name),
        CastExpression cast => LambdaExpressionReferencesName(cast.Operand, name),
        BinaryExpression { Operator: "is" or "is not" or "as" } binary => LambdaExpressionReferencesName(binary.Left, name),
        BinaryExpression binary => LambdaExpressionReferencesName(binary.Left, name) || LambdaExpressionReferencesName(binary.Right, name),
        ConditionalExpression conditional => LambdaExpressionReferencesName(conditional.Condition, name) ||
            LambdaExpressionReferencesName(conditional.WhenTrue, name) || LambdaExpressionReferencesName(conditional.WhenFalse, name),
        MemberAccessExpression access => LambdaExpressionReferencesName(access.Receiver, name),
        IndexExpression index => LambdaExpressionReferencesName(index.Receiver, name) || index.Indices.Any(item => LambdaExpressionReferencesName(item, name)),
        CallExpression call => LambdaExpressionReferencesName(call.Target, name) || call.Arguments.Any(argument => LambdaExpressionReferencesName(argument, name)),
        OverflowContextExpression overflow => LambdaExpressionReferencesName(overflow.Value, name),
        NullAssertionExpression assertion => LambdaExpressionReferencesName(assertion.Operand, name),
        _ => false
    };

    private static bool ContainsDeclaredName(Expression expression, IReadOnlySet<string> names) => expression switch
    {
            InlineOutVariableExpression inlineOut => names.Contains(inlineOut.Name),
        InlineArrayInitializerExpression initializer => initializer.Elements.Any(element => ContainsDeclaredName(element, names)),
        AllocationExpression allocation => allocation.Dimensions.Any(dimension => ContainsDeclaredName(dimension, names)) || allocation.Initializer is not null && allocation.Initializer.Elements.Any(element => ContainsDeclaredName(element, names)),
        NameExpression name => names.Contains(name.Name),
        UnaryExpression unary => ContainsDeclaredName(unary.Operand, names),
        CastExpression cast => ContainsDeclaredName(cast.Operand, names),
        BinaryExpression binary => ContainsDeclaredName(binary.Left, names) || ContainsDeclaredName(binary.Right, names),
        ConditionalExpression conditional => ContainsDeclaredName(conditional.Condition, names) || ContainsDeclaredName(conditional.WhenTrue, names) || ContainsDeclaredName(conditional.WhenFalse, names),
            MemberAccessExpression access => ContainsDeclaredName(access.Receiver, names),
            IndexExpression index => ContainsDeclaredName(index.Receiver, names) || index.Indices.Any(item => ContainsDeclaredName(item, names)),
        CallExpression call => ContainsDeclaredName(call.Target, names) || call.Arguments.Any(arg => ContainsDeclaredName(arg, names)),
        OverflowContextExpression overflow => ContainsDeclaredName(overflow.Value, names),
        _ => false
    };

    private MethodSymbol[] PrepareGenericCallCandidates(CallExpression call, MethodSymbol[] candidates,
        IReadOnlyList<string> argumentTypes, MethodContext context)
    {
        var genericCandidates = candidates.Where(candidate => candidate.Declaration.HasTypeParameters).ToArray();
        if (call.TypeArguments is { } explicitTypeArguments)
        {
            if (genericCandidates.Length == 0)
            {
                diagnostics.Add("LS1101", DiagnosticSeverity.Error, "Explicit type arguments were supplied to a non-generic method.", call.Location);
                return [];
            }
            return genericCandidates.Select(candidate => SpecializeGenericMethod(candidate, explicitTypeArguments, context, call.Location))
                .Where(candidate => candidate is not null).Cast<MethodSymbol>().Distinct().ToArray();
        }
        if (genericCandidates.Length == 0) return candidates;

        var inferred = new List<MethodSymbol>();
        foreach (var candidate in genericCandidates)
            if (TryInferGenericMethodArguments(call, candidate, argumentTypes, context, out var typeArguments))
                if (SpecializeGenericMethod(candidate, typeArguments, context, call.Location) is { } specialized)
                    inferred.Add(specialized);

        var nonGenericCandidates = candidates.Where(candidate => !candidate.Declaration.HasTypeParameters);
        if (inferred.Count == 0 && !nonGenericCandidates.Any())
            diagnostics.Add("LS1101", DiagnosticSeverity.Error,
                "Generic method type arguments could not be inferred from the call arguments; supply them explicitly.", call.Location);
        return nonGenericCandidates.Concat(inferred).Distinct().ToArray();
    }

    private bool TryInferGenericMethodArguments(CallExpression call, MethodSymbol candidate,
        IReadOnlyList<string> argumentTypes, MethodContext inferenceContext, out IReadOnlyList<string> typeArguments)
    {
        var parameters = candidate.Declaration.TypeParameters ?? [];
        var inferred = new Dictionary<string, string>(StringComparer.Ordinal);
        var lowerBounds = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var exactParameters = new HashSet<string>(StringComparer.Ordinal);
        var nullableLowerBoundParameters = new HashSet<string>(StringComparer.Ordinal);
        if (parameters.Count == 0 || !TryBindCallArguments(call, candidate, argumentTypes, out var binding))
        {
            typeArguments = [];
            return false;
        }
        for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
        {
            var actualType = argumentTypes[sourceIndex];
            var parameterIndex = binding.ParameterBySource[sourceIndex];
            var patternType = candidate.Declaration.Parameters[parameterIndex].TypeName;
            if (binding.UsesVariadicExpansion && parameterIndex == binding.VariadicParameterIndex && patternType.EndsWith("[]", StringComparison.Ordinal))
                patternType = patternType[..^2];
            if (actualType == "null")
            {
                if (parameters.Contains(patternType, StringComparer.Ordinal)) nullableLowerBoundParameters.Add(patternType);
                continue;
            }
            if (actualType == "?") continue;
            // By-reference inputs use exact inference: unlike a by-value argument, a
            // ref/in/out storage location cannot be widened to a common base or numeric
            // promotion and still denote the caller's original variable.
            var inferredSuccessfully = candidate.Declaration.Parameters[parameterIndex].PassingMode is "ref" or "in" or "out"
                ? TryInferExactTypeParameter(patternType, actualType, parameters, inferred, candidate.Owner, exactParameters, lowerBounds)
                : TryInferTypeParameter(patternType, actualType, parameters, inferred, candidate.Owner, lowerBounds, exactParameters);
            if (!inferredSuccessfully)
            {
                typeArguments = [];
                return false;
            }
        }
        for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
        {
            if (call.Arguments[sourceIndex] is not LambdaExpression lambda) continue;
            var parameterIndex = binding.ParameterBySource[sourceIndex];
            var patternType = candidate.Declaration.Parameters[parameterIndex].TypeName;
            if (binding.UsesVariadicExpansion && parameterIndex == binding.VariadicParameterIndex && patternType.EndsWith("[]", StringComparison.Ordinal))
                patternType = patternType[..^2];
            if (!TryInferGenericMethodArgumentsFromLambda(lambda, patternType, parameters, inferred, candidate.Owner,
                inferenceContext, false, lowerBounds, exactParameters))
            {
                typeArguments = [];
                return false;
            }
        }
        for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
        {
            if (argumentTypes[sourceIndex] != "?") continue;
            var argument = call.Arguments[sourceIndex];
            var isMethodGroup = argument switch
            {
                NameExpression name => IsPotentialMethodGroupForInference(name, inferenceContext),
                MemberAccessExpression access => IsPotentialMethodGroupForInference(access, inferenceContext),
                _ => false
            };
            if (!isMethodGroup) continue;
            var parameterIndex = binding.ParameterBySource[sourceIndex];
            var patternType = candidate.Declaration.Parameters[parameterIndex].TypeName;
            if (binding.UsesVariadicExpansion && parameterIndex == binding.VariadicParameterIndex && patternType.EndsWith("[]", StringComparison.Ordinal))
                patternType = patternType[..^2];
            if (!TryInferGenericMethodArgumentsFromMethodGroup(argument, patternType, parameters, inferred, candidate.Owner,
                inferenceContext, lowerBounds, exactParameters))
            {
                typeArguments = [];
                return false;
            }
        }
        for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
        {
            if (call.Arguments[sourceIndex] is not LambdaExpression lambda) continue;
            var parameterIndex = binding.ParameterBySource[sourceIndex];
            var patternType = candidate.Declaration.Parameters[parameterIndex].TypeName;
            if (binding.UsesVariadicExpansion && parameterIndex == binding.VariadicParameterIndex && patternType.EndsWith("[]", StringComparison.Ordinal))
                patternType = patternType[..^2];
            if (!TryInferGenericMethodArgumentsFromLambda(lambda, patternType, parameters, inferred, candidate.Owner,
                inferenceContext, true, lowerBounds, exactParameters))
            {
                typeArguments = [];
                return false;
            }
        }
        foreach (var (parameter, bounds) in lowerBounds)
        {
            if (exactParameters.Contains(parameter))
            {
                if (!inferred.TryGetValue(parameter, out var exactType) || bounds.Any(bound => !CanConvert(bound, exactType, null)))
                {
                    typeArguments = [];
                    return false;
                }
                continue;
            }
            if (bounds.All(IsNumericInferenceBound))
            {
                if (!TryFindCommonNumericInferenceType(bounds, out var numericType))
                {
                    typeArguments = [];
                    return false;
                }
                inferred[parameter] = numericType;
            }
        }
        foreach (var parameter in nullableLowerBoundParameters)
            if (inferred.TryGetValue(parameter, out var inferredType) && !inferredType.EndsWith("?", StringComparison.Ordinal))
                inferred[parameter] = inferredType + "?";
        if (parameters.Any(parameter => !inferred.ContainsKey(parameter)))
        {
            typeArguments = [];
            return false;
        }
        typeArguments = parameters.Select(parameter => inferred[parameter]).ToArray();
        return true;
    }

    private bool TryInferGenericMethodArgumentsFromMethodGroup(Expression methodGroup, string delegatePattern,
        IReadOnlyList<string> methodTypeParameters, IDictionary<string, string> inferred, TypeDeclaration owner,
        MethodContext inferenceContext, IDictionary<string, List<string>> lowerBounds, ISet<string> exactParameters)
    {
        if (!TrySplitGenericType(delegatePattern, out var definitionName, out var delegateArguments)) return true;
        var definition = GenericTypeSpecializer.ResolveDefinition(definitionName, owner, _genericDefinitions);
        if (definition is not { Kind: "delegate", TypeParameters: { } delegateTypeParameters, DelegateParameters: { } delegateParameters } ||
            delegateArguments.Count != delegateTypeParameters.Count)
            return true;

        var delegateSubstitutions = delegateTypeParameters.Zip(delegateArguments)
            .ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
        var solutions = new List<(Dictionary<string, string> Inferred, Dictionary<string, List<string>> LowerBounds,
            HashSet<string> ExactParameters)>();
        foreach (var candidate in GetMethodGroupInferenceCandidates(methodGroup, inferenceContext))
        {
            if (candidate.Declaration.HasTypeParameters || candidate.Declaration.Parameters.Count != delegateParameters.Count) continue;
            var trial = new Dictionary<string, string>(inferred, StringComparer.Ordinal);
            var trialLowerBounds = CloneInferenceLowerBounds(lowerBounds);
            var trialExactParameters = new HashSet<string>(exactParameters, StringComparer.Ordinal);
            var compatible = true;
            for (var index = 0; index < delegateParameters.Count; index++)
            {
                var delegateParameter = delegateParameters[index];
                var methodParameter = candidate.Declaration.Parameters[index];
                if (methodParameter.PassingMode != delegateParameter.PassingMode)
                {
                    compatible = false;
                    break;
                }
                var pattern = GenericTypeSpecializer.RewriteTypeName(delegateParameter.TypeName, delegateSubstitutions);
                var actual = NormalizeEnumType(methodParameter.TypeName, candidate.Owner);
                if (!TryInferExactTypeParameter(pattern, actual, methodTypeParameters, trial, owner,
                    trialExactParameters, trialLowerBounds))
                {
                    compatible = false;
                    break;
                }
            }
            if (!compatible) continue;

            var delegateReturnPattern = GenericTypeSpecializer.RewriteTypeName(definition.DelegateReturnType ?? "void", delegateSubstitutions);
            var methodReturnType = NormalizeEnumType(candidate.Declaration.ReturnType, candidate.Owner);
            if (delegateReturnPattern == "void")
            {
                if (methodReturnType != "void") continue;
            }
            else
            {
                if (!TryInferTypeParameter(delegateReturnPattern, methodReturnType, methodTypeParameters, trial, owner,
                    trialLowerBounds, trialExactParameters)) continue;
                var closedReturnType = GenericTypeSpecializer.RewriteTypeName(delegateReturnPattern, trial);
                if (GenericTypeSpecializer.ReferencesAnyTypeParameter(closedReturnType, methodTypeParameters.ToHashSet(StringComparer.Ordinal)) ||
                    !CanConvert(methodReturnType, closedReturnType, null)) continue;
            }

            var parameters = methodTypeParameters.ToHashSet(StringComparer.Ordinal);
            if (delegateParameters.Select((parameter, index) => (parameter, index)).Any(pair =>
            {
                var closedParameterType = GenericTypeSpecializer.RewriteTypeName(
                    GenericTypeSpecializer.RewriteTypeName(pair.parameter.TypeName, delegateSubstitutions), trial);
                return GenericTypeSpecializer.ReferencesAnyTypeParameter(closedParameterType, parameters) ||
                    closedParameterType != NormalizeEnumType(candidate.Declaration.Parameters[pair.index].TypeName, candidate.Owner);
            }))
                continue;

            solutions.Add((trial, trialLowerBounds, trialExactParameters));
        }

        if (solutions.Count == 0) return true;
        var distinctSolutions = solutions
            .GroupBy(solution => string.Join(";", methodTypeParameters.Where(solution.Inferred.ContainsKey)
                    .Select(parameter => $"{parameter}={solution.Inferred[parameter]}")) + "|" +
                string.Join(";", solution.LowerBounds.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => $"{pair.Key}={string.Join(",", pair.Value)}")) + "|" +
                string.Join(";", solution.ExactParameters.OrderBy(parameter => parameter, StringComparer.Ordinal)), StringComparer.Ordinal)
            .Select(group => group.First()).ToArray();
        if (distinctSolutions.Length != 1) return false;
        foreach (var (parameter, type) in distinctSolutions[0].Inferred) inferred[parameter] = type;
        ReplaceInferenceLowerBounds(lowerBounds, distinctSolutions[0].LowerBounds);
        exactParameters.UnionWith(distinctSolutions[0].ExactParameters);
        return true;
    }

    private static Dictionary<string, List<string>> CloneInferenceLowerBounds(IDictionary<string, List<string>> source) =>
        source.ToDictionary(pair => pair.Key, pair => new List<string>(pair.Value), StringComparer.Ordinal);

    private static void ReplaceInferenceLowerBounds(IDictionary<string, List<string>> destination,
        IReadOnlyDictionary<string, List<string>> source)
    {
        destination.Clear();
        foreach (var (parameter, bounds) in source)
            destination[parameter] = new List<string>(bounds);
    }

    private IEnumerable<MethodSymbol> GetMethodGroupInferenceCandidates(Expression expression, MethodContext context)
    {
        if (expression is NameExpression name)
        {
            if (context.Lookup(name.Name) is not null) return [];
            var candidates = _methods.Concat(_genericMethodDefinitions).Where(method => method.Owner == context.Method.Owner &&
                method.Declaration.Name == name.Name && method.Declaration.IsStatic).ToList();
            if (context.Method.IsClassInstanceMethod || context.Method.IsClassConstructor || context.Method.IsClassFinalizer)
                candidates.AddRange(GetClassInstanceMethods(GetQualifiedTypeName(context.Method.Owner), name.Name));
            if (context.Method.IsStructInstanceMethod || context.Method.IsStructConstructor)
                candidates.AddRange(_methods.Concat(_genericMethodDefinitions).Where(method => method.IsStructInstanceMethod &&
                    method.Owner == context.Method.Owner && method.Declaration.Name == name.Name));
            return candidates.Where(method => IsMethodAccessible(method, context.Method.Owner)).Distinct().ToArray();
        }

        if (expression is not MemberAccessExpression access) return [];
        if (IsTypeReference(access.Receiver, context) && TryFlattenName(access.Receiver, out var requestedType))
        {
            var matchingTypes = FindTypes(NormalizeBuiltinTypeQualifier(NormalizeEnumType(requestedType, context.Method.Owner)), context.Method.Owner);
            return matchingTypes.Length == 1
                ? _methods.Concat(_genericMethodDefinitions).Where(method => method.Owner == matchingTypes[0] &&
                    method.Declaration.IsStatic && method.Declaration.Name == access.Member && IsMethodAccessible(method, context.Method.Owner)).ToArray()
                : [];
        }

        var receiverType = GetMethodGroupReceiverType(access.Receiver, context);
        if (receiverType is null) return [];
        if (IsClassType(receiverType))
            return GetClassInstanceMethods(receiverType, access.Member).Where(method => IsMethodAccessible(method, context.Method.Owner)).ToArray();
        if (IsInterfaceType(receiverType)) return GetInterfaceInstanceMethods(receiverType, access.Member).ToArray();
        if (IsStructType(receiverType))
            return _methods.Concat(_genericMethodDefinitions).Where(method => method.IsStructInstanceMethod &&
                GetQualifiedTypeName(method.Owner) == receiverType && method.Declaration.Name == access.Member).ToArray();
        return [];
    }

    private bool TryInferGenericMethodArgumentsFromLambda(LambdaExpression lambda, string delegatePattern,
        IReadOnlyList<string> methodTypeParameters, IDictionary<string, string> inferred, TypeDeclaration context,
        MethodContext inferenceContext, bool inferLambdaOutputs, IDictionary<string, List<string>> lowerBounds,
        ISet<string> exactParameters)
    {
        if (!TrySplitGenericType(delegatePattern, out var definitionName, out var delegateArguments)) return true;
        var definition = GenericTypeSpecializer.ResolveDefinition(definitionName, context, _genericDefinitions);
        if (definition is not { Kind: "delegate", TypeParameters: { } delegateTypeParameters, DelegateParameters: { } delegateParameters } ||
            delegateArguments.Count != delegateTypeParameters.Count || lambda.Parameters.Count != delegateParameters.Count)
            return true;

        var delegateSubstitutions = delegateTypeParameters.Zip(delegateArguments)
            .ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
        var methodTypeParameterSet = methodTypeParameters.ToHashSet(StringComparer.Ordinal);
        for (var index = 0; index < lambda.Parameters.Count; index++)
        {
            var lambdaParameter = lambda.Parameters[index];
            if (lambdaParameter.TypeName is null) continue;
            var parameterPattern = GenericTypeSpecializer.RewriteTypeName(delegateParameters[index].TypeName, delegateSubstitutions);
            if (!GenericTypeSpecializer.ReferencesAnyTypeParameter(parameterPattern, methodTypeParameterSet)) continue;
            var actualType = NormalizeEnumType(lambdaParameter.TypeName, context);
            if (!TryInferExactTypeParameter(parameterPattern, actualType, methodTypeParameters, inferred, context,
                exactParameters, lowerBounds)) return false;
        }

        var returnPattern = GenericTypeSpecializer.RewriteTypeName(definition.DelegateReturnType ?? "void", delegateSubstitutions);
        if (inferLambdaOutputs && GenericTypeSpecializer.ReferencesAnyTypeParameter(returnPattern, methodTypeParameterSet))
        {
            var outputExpressions = lambda.ExpressionBody is { } expressionBody
                ? new[] { expressionBody }
                : lambda.BlockBody is { } blockBody ? GetLambdaReturnExpressions(blockBody).ToArray() : [];
            var outputTypes = new List<string>(outputExpressions.Length);
            foreach (var body in outputExpressions)
            {
                if (body is LambdaExpression nestedLambda)
                {
                    if (!TryInferGenericMethodArgumentsFromLambda(nestedLambda, returnPattern, methodTypeParameters,
                        inferred, context, inferenceContext, true, lowerBounds, exactParameters)) return false;
                    continue;
                }
                var outputType = TryInferLambdaOutputExpressionType(body, lambda, delegateParameters, delegateSubstitutions,
                    methodTypeParameters, inferred, context, inferenceContext);
                if (outputType is not null) outputTypes.Add(outputType);
            }
            foreach (var outputType in outputTypes.Where(outputType => outputType is not ("?" or "null")))
                if (!TryInferTypeParameter(returnPattern, outputType, methodTypeParameters, inferred, context,
                    lowerBounds, exactParameters)) return false;
        }
        return true;
    }

    private string? TryInferLambdaOutputExpressionType(Expression outputExpression, LambdaExpression lambda,
        IReadOnlyList<ParameterDeclaration> delegateParameters, IReadOnlyDictionary<string, string> delegateSubstitutions,
        IReadOnlyList<string> methodTypeParameters, IDictionary<string, string> inferred, TypeDeclaration owner,
        MethodContext inferenceContext)
    {
        var methodTypeParameterSet = methodTypeParameters.ToHashSet(StringComparer.Ordinal);
        if (ContainsTargetDependentInferenceExpression(outputExpression, inferenceContext, methodTypeParameterSet)) return null;

        var lambdaContext = inferenceContext.CloneForInference();
        lambdaContext.Push();
        var inferredArguments = new Dictionary<string, string>(inferred, StringComparer.Ordinal);
        for (var index = 0; index < lambda.Parameters.Count; index++)
        {
            var sourceParameter = lambda.Parameters[index];
            if (lambdaContext.Exists(sourceParameter.Name)) return null;
            var parameterType = sourceParameter.TypeName is null
                ? GenericTypeSpecializer.RewriteTypeName(delegateParameters[index].TypeName, delegateSubstitutions)
                : sourceParameter.TypeName;
            parameterType = GenericTypeSpecializer.RewriteTypeName(parameterType, inferredArguments);
            if (GenericTypeSpecializer.ReferencesAnyTypeParameter(parameterType, methodTypeParameterSet)) return null;
            parameterType = NormalizeEnumType(parameterType, owner);
            lambdaContext.Declare(sourceParameter.Name, parameterType, mutable: false, sourceParameter.Location);
        }

        var initialNullStates = lambdaContext.CaptureNullStates();
        var diagnosticCount = diagnostics.Items.Count;
        var clonedExpression = GenericTypeSpecializer.CloneExpressionForAnalysis(outputExpression);
        var outputType = AnalyzeExpression(clonedExpression, lambdaContext);
        lambdaContext.RestoreNullStates(initialNullStates);
        var producedError = diagnostics.Items.Skip(diagnosticCount).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        diagnostics.DiscardSince(diagnosticCount);
        if (producedError) return null;
        if (GenericTypeSpecializer.ReferencesAnyTypeParameter(outputType, methodTypeParameterSet)) return null;
        return outputType;
    }

    private bool ContainsTargetDependentInferenceExpression(Expression expression, MethodContext context,
        IReadOnlySet<string> methodTypeParameters) => expression switch
    {
        LambdaExpression => true,
        DefaultValueExpression { TypeName: null } => true,
        AllocationExpression { TypeName: null } => true,
        InlineArrayInitializerExpression => true,
        UnsupportedExpression => true,
        NameExpression name => IsPotentialMethodGroupForInference(name, context),
        UnaryExpression unary => ContainsTargetDependentInferenceExpression(unary.Operand, context, methodTypeParameters),
        CastExpression cast => GenericTypeSpecializer.ReferencesAnyTypeParameter(cast.TypeName, methodTypeParameters) ||
            ContainsTargetDependentInferenceExpression(cast.Operand, context, methodTypeParameters),
        BinaryExpression binary => ContainsTargetDependentInferenceExpression(binary.Left, context, methodTypeParameters) ||
            ContainsTargetDependentInferenceExpression(binary.Right, context, methodTypeParameters),
        ConditionalExpression conditional => ContainsTargetDependentInferenceExpression(conditional.Condition, context, methodTypeParameters) ||
            ContainsTargetDependentInferenceExpression(conditional.WhenTrue, context, methodTypeParameters) ||
            ContainsTargetDependentInferenceExpression(conditional.WhenFalse, context, methodTypeParameters),
        MemberAccessExpression access => IsPotentialMethodGroupForInference(access, context) ||
            ContainsTargetDependentInferenceExpression(access.Receiver, context, methodTypeParameters),
        IndexExpression index => ContainsTargetDependentInferenceExpression(index.Receiver, context, methodTypeParameters) ||
            index.Indices.Any(item => ContainsTargetDependentInferenceExpression(item, context, methodTypeParameters)),
        CallExpression call => ContainsTargetDependentCallTarget(call.Target, context, methodTypeParameters) ||
            call.Arguments.Any(argument => ContainsTargetDependentInferenceExpression(argument, context, methodTypeParameters)),
        OverflowContextExpression overflow => ContainsTargetDependentInferenceExpression(overflow.Value, context, methodTypeParameters),
        NullAssertionExpression assertion => ContainsTargetDependentInferenceExpression(assertion.Operand, context, methodTypeParameters),
        AllocationExpression allocation => allocation.Dimensions.Any(dimension => ContainsTargetDependentInferenceExpression(dimension, context, methodTypeParameters)) ||
            allocation.ConstructorArguments?.Any(argument => ContainsTargetDependentInferenceExpression(argument, context, methodTypeParameters)) == true ||
            allocation.Initializer is not null && ContainsTargetDependentInferenceExpression(allocation.Initializer, context, methodTypeParameters),
        _ => false
    };

    private bool ContainsTargetDependentCallTarget(Expression target, MethodContext context,
        IReadOnlySet<string> methodTypeParameters) => target switch
    {
        NameExpression name when IsPotentialMethodGroupForInference(name, context) => false,
        MemberAccessExpression access when IsPotentialMethodGroupForInference(access, context) =>
            ContainsTargetDependentInferenceExpression(access.Receiver, context, methodTypeParameters),
        _ => ContainsTargetDependentInferenceExpression(target, context, methodTypeParameters)
    };

    private bool IsPotentialMethodGroupForInference(NameExpression expression, MethodContext context) =>
        context.Lookup(expression.Name) is null && _methods.Concat(_genericMethodDefinitions).Any(method =>
            method.Owner == context.Method.Owner && method.Declaration.Name == expression.Name);

    private bool IsPotentialMethodGroupForInference(MemberAccessExpression expression, MethodContext context)
    {
        if (IsTypeReference(expression.Receiver, context) && TryFlattenName(expression.Receiver, out var requestedType))
        {
            var matchingTypes = FindTypes(NormalizeBuiltinTypeQualifier(NormalizeEnumType(requestedType, context.Method.Owner)), context.Method.Owner);
            return matchingTypes.Length == 1 && _methods.Concat(_genericMethodDefinitions).Any(method =>
                method.Owner == matchingTypes[0] && method.Declaration.Name == expression.Member && method.Declaration.IsStatic);
        }

        var receiverType = GetMethodGroupReceiverType(expression.Receiver, context);
        if (receiverType is null) return false;
        if (IsClassType(receiverType)) return GetClassInstanceMethods(receiverType, expression.Member).Length > 0;
        if (IsInterfaceType(receiverType)) return GetInterfaceInstanceMethods(receiverType, expression.Member).Length > 0;
        return _methods.Concat(_genericMethodDefinitions).Any(method => method.IsStructInstanceMethod &&
            GetQualifiedTypeName(method.Owner) == receiverType && method.Declaration.Name == expression.Member);
    }

    private string? GetMethodGroupReceiverType(Expression receiver, MethodContext context)
    {
        var knownType = receiver switch
        {
            ThisExpression => GetQualifiedTypeName(context.Method.Owner),
            BaseExpression => context.Method.Owner.BaseTypes?.FirstOrDefault(),
            NameExpression name when context.Lookup(name.Name) is { } local => NullableUnderlyingType(local.Type),
            _ => null
        };
        if (knownType is not null) return knownType;

        var probeContext = context.CloneForInference();
        var probeNullStates = probeContext.CaptureNullStates();
        var diagnosticCount = diagnostics.Items.Count;
        var analyzedType = AnalyzeExpression(GenericTypeSpecializer.CloneExpressionForAnalysis(receiver), probeContext);
        probeContext.RestoreNullStates(probeNullStates);
        var producedError = diagnostics.Items.Skip(diagnosticCount).Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
        diagnostics.DiscardSince(diagnosticCount);
        return producedError || analyzedType == "?" ? null : NullableUnderlyingType(analyzedType);
    }

    private static IEnumerable<Expression> GetLambdaReturnExpressions(Statement statement)
    {
        switch (statement)
        {
            case ReturnStatement { Value: { } value }:
                yield return value;
                break;
            case BlockStatement block:
                foreach (var child in block.Statements)
                    foreach (var value in GetLambdaReturnExpressions(child)) yield return value;
                break;
            case IfStatement conditional:
                foreach (var value in GetLambdaReturnExpressions(conditional.Then)) yield return value;
                if (conditional.Else is not null)
                    foreach (var value in GetLambdaReturnExpressions(conditional.Else)) yield return value;
                break;
            case WhileStatement loop:
                foreach (var value in GetLambdaReturnExpressions(loop.Body)) yield return value;
                break;
            case DoWhileStatement loop:
                foreach (var value in GetLambdaReturnExpressions(loop.Body)) yield return value;
                break;
            case ForStatement loop:
                if (loop.Initializer is not null)
                    foreach (var value in GetLambdaReturnExpressions(loop.Initializer)) yield return value;
                foreach (var value in GetLambdaReturnExpressions(loop.Body)) yield return value;
                break;
            case ForInStatement loop:
                foreach (var value in GetLambdaReturnExpressions(loop.Body)) yield return value;
                break;
            case SwitchStatement selection:
                foreach (var section in selection.Sections)
                    foreach (var child in section.Statements)
                        foreach (var value in GetLambdaReturnExpressions(child)) yield return value;
                break;
            case TryStatement attempt:
                foreach (var value in GetLambdaReturnExpressions(attempt.Body)) yield return value;
                foreach (var clause in attempt.Catches)
                    foreach (var value in GetLambdaReturnExpressions(clause.Body)) yield return value;
                if (attempt.Finally is not null)
                    foreach (var value in GetLambdaReturnExpressions(attempt.Finally)) yield return value;
                break;
            case OverflowContextStatement overflow:
                foreach (var value in GetLambdaReturnExpressions(overflow.Body)) yield return value;
                break;
        }
    }

    private bool TryInferExactTypeParameter(string pattern, string actual, IReadOnlyList<string> typeParameters,
        IDictionary<string, string> inferred, TypeDeclaration context, ISet<string>? exactParameters = null,
        IDictionary<string, List<string>>? lowerBounds = null)
    {
        if (typeParameters.Contains(pattern, StringComparer.Ordinal))
        {
            if (inferred.TryGetValue(pattern, out var previous))
            {
                if (previous == actual)
                {
                    exactParameters?.Add(pattern);
                    return true;
                }
                if (lowerBounds is not null && lowerBounds.TryGetValue(pattern, out var bounds) &&
                    bounds.All(bound => CanConvert(bound, actual, null)))
                {
                    inferred[pattern] = actual;
                    exactParameters?.Add(pattern);
                    return true;
                }
                return false;
            }
            inferred[pattern] = actual;
            exactParameters?.Add(pattern);
            return true;
        }
        if (pattern.EndsWith("?", StringComparison.Ordinal))
            return actual.EndsWith("?", StringComparison.Ordinal) &&
                TryInferExactTypeParameter(pattern[..^1], actual[..^1], typeParameters, inferred, context, exactParameters, lowerBounds);
        if (TryParseManagedArrayType(pattern, out var patternElement, out var patternRank))
            return TryParseManagedArrayType(actual, out var actualElement, out var actualRank) && patternRank == actualRank &&
                TryInferExactTypeParameter(patternElement, actualElement, typeParameters, inferred, context, exactParameters, lowerBounds);
        if (pattern.EndsWith("*", StringComparison.Ordinal))
            return actual.EndsWith("*", StringComparison.Ordinal) &&
                TryInferExactTypeParameter(pattern[..^1], actual[..^1], typeParameters, inferred, context, exactParameters, lowerBounds);
        if (TryParseInlineArrayType(pattern, out var patternInlineElement, out var patternInlineLength))
            return TryParseInlineArrayType(actual, out var actualInlineElement, out var actualInlineLength) &&
                patternInlineLength == actualInlineLength &&
                TryInferExactTypeParameter(patternInlineElement, actualInlineElement, typeParameters, inferred, context, exactParameters, lowerBounds);
        if (TrySplitGenericType(pattern, out var patternDefinitionName, out var patternArguments))
        {
            var definition = GenericTypeSpecializer.ResolveDefinition(patternDefinitionName, context, _genericDefinitions);
            if (definition is null || !TryGetExactGenericTypeArguments(actual, definition, context, out var actualArguments) ||
                patternArguments.Count != actualArguments.Count) return false;
            for (var index = 0; index < patternArguments.Count; index++)
                if (!TryInferExactTypeParameter(patternArguments[index], actualArguments[index], typeParameters, inferred,
                    context, exactParameters, lowerBounds)) return false;
            return true;
        }
        return pattern == actual;
    }

    private bool TryGetExactGenericTypeArguments(string actualType, TypeDeclaration expectedDefinition,
        TypeDeclaration context, out IReadOnlyList<string> arguments)
    {
        var actualUnderlying = NullableUnderlyingType(actualType);
        if (TrySplitGenericType(actualUnderlying, out var actualDefinitionName, out var syntacticArguments) &&
            GenericTypeSpecializer.ResolveDefinition(actualDefinitionName, context, _genericDefinitions) is { } actualDefinition &&
            GetQualifiedTypeName(actualDefinition) == GetQualifiedTypeName(expectedDefinition))
        {
            arguments = syntacticArguments;
            return true;
        }
        var instances = _genericTypeInstantiations.Where(instance =>
            GetQualifiedTypeName(instance.Specialized) == actualUnderlying &&
            GetQualifiedTypeName(instance.Definition) == GetQualifiedTypeName(expectedDefinition)).ToArray();
        if (instances.Length == 1)
        {
            arguments = instances[0].Arguments.Select(argument => NormalizeEnumType(argument, instances[0].RequestContext)).ToArray();
            return true;
        }
        arguments = [];
        return false;
    }

    private bool TryInferTypeParameter(string pattern, string actual, IReadOnlyList<string> typeParameters,
        IDictionary<string, string> inferred, TypeDeclaration context,
        IDictionary<string, List<string>>? lowerBounds = null, ISet<string>? exactParameters = null)
    {
        if (typeParameters.Contains(pattern, StringComparer.Ordinal))
        {
            if (exactParameters?.Contains(pattern) == true)
                return inferred.TryGetValue(pattern, out var exactType) && CanConvert(actual, exactType, null);

            if (lowerBounds is not null && (IsGenericInferenceReferenceType(actual) || IsNumericInferenceBound(actual)) &&
                (!inferred.ContainsKey(pattern) || lowerBounds.ContainsKey(pattern)))
            {
                if (!lowerBounds.TryGetValue(pattern, out var bounds))
                {
                    bounds = [];
                    lowerBounds.Add(pattern, bounds);
                }
                bounds.Add(actual);
                if (bounds.All(IsGenericInferenceReferenceType))
                {
                    if (!TryFindCommonReferenceInferenceType(bounds, context, out var inferredReference)) return false;
                    inferred[pattern] = inferredReference;
                    return true;
                }
                if (bounds.All(IsNumericInferenceBound))
                {
                    inferred[pattern] = TryFindCommonNumericInferenceType(bounds, out var numericType)
                        ? numericType
                        : GetProvisionalNumericInferenceType(bounds);
                    return true;
                }
                return false;
            }

            if (!inferred.TryGetValue(pattern, out var previous)) { inferred[pattern] = actual; return true; }
            if (previous == actual) return true;
            if (IsNullableValueType(previous) || IsNullableValueType(actual))
            {
                var previousUnderlying = NormalizeEnumType(NullableUnderlyingType(previous), context);
                var actualUnderlying = NormalizeEnumType(NullableUnderlyingType(actual), context);
                if (previousUnderlying == actualUnderlying)
                {
                    var nullableType = previousUnderlying + "?";
                    if ((nullableType == previous || nullableType == actual) &&
                        CanConvert(previous, nullableType, null) && CanConvert(actual, nullableType, null))
                    {
                        inferred[pattern] = nullableType;
                        return true;
                    }
                }
                if (NumericTypes.Contains(previousUnderlying) && NumericTypes.Contains(actualUnderlying))
                {
                    var nullableType = NumericPromotionType(previousUnderlying, actualUnderlying) + "?";
                    if ((nullableType == previous || nullableType == actual) &&
                        CanConvert(previous, nullableType, null) && CanConvert(actual, nullableType, null))
                    {
                        inferred[pattern] = nullableType;
                        return true;
                    }
                }
            }
            if (NumericTypes.Contains(previous) && NumericTypes.Contains(actual))
            {
                var promotedType = NumericPromotionType(previous, actual);
                if ((promotedType == previous || promotedType == actual) &&
                    CanConvert(previous, promotedType, null) && CanConvert(actual, promotedType, null))
                {
                    inferred[pattern] = promotedType;
                    return true;
                }
            }
            if (IsGenericInferenceReferenceType(previous) && IsGenericInferenceReferenceType(actual))
            {
                var previousUnderlying = NormalizeEnumType(NullableUnderlyingType(previous), context);
                var actualUnderlying = NormalizeEnumType(NullableUnderlyingType(actual), context);
                var commonBase = GetClassChain(previousUnderlying).FirstOrDefault(candidate =>
                    candidate == actualUnderlying || IsBaseClassOf(candidate, actualUnderlying));
                string? commonType = commonBase is not null && commonBase != "System.Object" ? commonBase : null;
                if (commonType is null && TryFindCommonReferenceInferenceType(previous, actual, context, out var commonReference))
                    commonType = commonReference;
                commonType ??= commonBase;
                if (commonType is not null || _classTypes.ContainsKey("System.Object"))
                {
                    commonType ??= "System.Object";
                    if ((IsNullableReferenceInferenceType(previous) || IsNullableReferenceInferenceType(actual)) &&
                        !commonType.EndsWith("?", StringComparison.Ordinal))
                        commonType += "?";
                    inferred[pattern] = commonType;
                    return true;
                }
            }
            return false;
        }
        if (pattern.EndsWith("?", StringComparison.Ordinal))
        {
            var patternUnderlying = pattern[..^1];
            var actualUnderlying = actual.EndsWith("?", StringComparison.Ordinal) ? actual[..^1] : actual;
            if (patternUnderlying != pattern && TryInferTypeParameter(patternUnderlying, actualUnderlying, typeParameters, inferred,
                context, lowerBounds, exactParameters)) return true;
        }
        if (TryParseManagedArrayType(pattern, out var patternElement, out var patternRank))
        {
            if (!TryParseManagedArrayType(actual, out var actualElement, out var actualRank) || patternRank != actualRank) return false;
            // LumaSharp arrays are invariant, so their element type contributes an exact
            // bound even when the element itself is a reference type.
            return TryInferExactTypeParameter(patternElement, actualElement, typeParameters, inferred, context, exactParameters, lowerBounds);
        }
        if (pattern.EndsWith("*", StringComparison.Ordinal))
            return actual.EndsWith("*", StringComparison.Ordinal) &&
                TryInferExactTypeParameter(pattern[..^1], actual[..^1], typeParameters, inferred, context, exactParameters, lowerBounds);
        if (TryParseInlineArrayType(pattern, out var patternInlineElement, out var patternInlineLength))
            return TryParseInlineArrayType(actual, out var actualInlineElement, out var actualInlineLength) &&
                patternInlineLength == actualInlineLength &&
                TryInferExactTypeParameter(patternInlineElement, actualInlineElement, typeParameters, inferred, context,
                    exactParameters, lowerBounds);
        if (TrySplitGenericType(pattern, out var patternDefinitionName, out var patternArguments))
        {
            var definition = GenericTypeSpecializer.ResolveDefinition(patternDefinitionName, context, _genericDefinitions);
            if (definition is null) return false;

            if (TryGetExactGenericTypeArguments(actual, definition, context, out var exactArguments))
            {
                if (patternArguments.Count != exactArguments.Count) return false;
                for (var index = 0; index < patternArguments.Count; index++)
                    if (!TryInferExactTypeParameter(patternArguments[index], exactArguments[index], typeParameters, inferred,
                        context, exactParameters, lowerBounds)) return false;
                return true;
            }

            var convertibleTypes = new HashSet<string>(StringComparer.Ordinal) { actual };
            foreach (var baseType in GetClassChain(actual)) convertibleTypes.Add(baseType);
            foreach (var conversion in _interfaceConversions.Where(conversion => conversion.FromType == actual))
                convertibleTypes.Add(conversion.ToType);
            var actualInstances = _genericTypeInstantiations.Where(instance =>
                    convertibleTypes.Contains(GetQualifiedTypeName(instance.Specialized)) &&
                    GetQualifiedTypeName(instance.Definition) == GetQualifiedTypeName(definition))
                .DistinctBy(instance => GetQualifiedTypeName(instance.Specialized), StringComparer.Ordinal)
                .ToArray();
            if (actualInstances.Length != 1 || patternArguments.Count != actualInstances[0].Arguments.Count) return false;
            var actualInstance = actualInstances[0];
            for (var index = 0; index < patternArguments.Count; index++)
            {
                var actualArgument = NormalizeEnumType(actualInstance.Arguments[index], actualInstance.RequestContext);
                // The constructed interface/base conversion supplies the matching
                // generic type, but its arguments remain invariant in LumaSharp.
                if (!TryInferExactTypeParameter(patternArguments[index], actualArgument, typeParameters, inferred,
                    context, exactParameters, lowerBounds)) return false;
            }
            return true;
        }
        return pattern == actual;
    }

    private bool IsGenericInferenceReferenceType(string type)
    {
        var underlying = NullableUnderlyingType(type);
        return IsClassReferenceType(underlying) || IsDelegateType(underlying) || IsStringType(underlying) ||
            TryParseManagedArrayType(underlying, out _, out _);
    }

    private bool IsNullableReferenceInferenceType(string type) =>
        IsNullableClassType(type) || IsNullableDelegateType(type) || IsNullableStringType(type) || IsNullableArrayType(type);

    private static bool IsNumericInferenceBound(string type) => NumericTypes.Contains(NullableUnderlyingType(type));

    private bool TryFindConditionalNumericCommonType(string first, Expression firstExpression, string second,
        Expression secondExpression, out string commonType)
    {
        var firstUnderlying = NullableUnderlyingType(first);
        var secondUnderlying = NullableUnderlyingType(second);
        if (!NumericTypes.Contains(firstUnderlying) || !NumericTypes.Contains(secondUnderlying))
        {
            commonType = "";
            return false;
        }

        var nullable = IsNullableValueType(first) || IsNullableValueType(second);
        var promoted = NumericPromotionType(firstUnderlying, secondUnderlying) + (nullable ? "?" : "");
        if (CanConvert(first, promoted, firstExpression) && CanConvert(second, promoted, secondExpression))
        {
            commonType = promoted;
            return true;
        }

        // Conditional expressions use an actual implicit-conversion target, not an
        // arithmetic-only conversion. For example, int32 and float have double as a
        // common type even though binary arithmetic promotion would select float.
        string[] candidates = ["int8", "uint8", "int16", "uint16", "int32", "uint32", "int64", "uint64", "float", "double", "char"];
        var viable = candidates
            .Select(candidate => candidate + (nullable ? "?" : ""))
            .Where(candidate => CanConvert(first, candidate, firstExpression) && CanConvert(second, candidate, secondExpression))
            .ToArray();
        var mostSpecific = viable.Where(candidate => viable.All(other =>
                candidate == other || CanConvert(candidate, other, null)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (mostSpecific.Length == 1)
        {
            commonType = mostSpecific[0];
            return true;
        }

        commonType = "";
        return false;
    }

    private bool TryFindCommonNumericInferenceType(IReadOnlyList<string> lowerBounds, out string commonType)
    {
        var candidates = lowerBounds.Distinct(StringComparer.Ordinal)
            .Where(IsNumericInferenceBound)
            .Where(candidate => lowerBounds.All(bound => CanConvert(bound, candidate, null)))
            .ToArray();
        var mostSpecific = candidates.Where(candidate => candidates.All(other =>
                candidate == other || CanConvert(candidate, other, null)))
            .ToArray();
        if (mostSpecific.Length == 1)
        {
            commonType = mostSpecific[0];
            return true;
        }
        commonType = "";
        return false;
    }

    private string GetProvisionalNumericInferenceType(IReadOnlyList<string> lowerBounds)
    {
        var result = lowerBounds[0];
        foreach (var bound in lowerBounds.Skip(1))
        {
            var nullable = IsNullableValueType(result) || IsNullableValueType(bound);
            result = NumericPromotionType(NullableUnderlyingType(result), NullableUnderlyingType(bound)) + (nullable ? "?" : "");
        }
        return result;
    }

    private bool TryFindCommonReferenceInferenceType(string previous, string actual, TypeDeclaration context, out string commonType)
        => TryFindCommonReferenceInferenceType([previous, actual], context, out commonType);

    private bool TryFindCommonReferenceInferenceType(IReadOnlyList<string> lowerBounds, TypeDeclaration context, out string commonType)
    {
        if (lowerBounds.Count == 0)
        {
            commonType = "";
            return false;
        }
        var normalizedBounds = lowerBounds.Select(bound => NormalizeEnumType(NullableUnderlyingType(bound), context)).ToArray();
        var preserveNullability = lowerBounds.Any(IsNullableReferenceInferenceType);
        var candidates = _classTypes.Keys.Concat(_interfaceTypes.Keys).Concat(_delegateTypes.Keys)
            .Append("string").Concat(normalizedBounds)
            .Where(IsGenericInferenceReferenceType)
            .Distinct(StringComparer.Ordinal)
            .Select(candidate => preserveNullability ? candidate + "?" : candidate)
            .Where(candidate => lowerBounds.All(bound => CanConvert(bound, candidate, null)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var mostSpecific = candidates.Where(candidate => candidates.All(other =>
                candidate == other || CanConvert(candidate, other, null)))
            .ToArray();
        if (mostSpecific.Length == 1)
        {
            commonType = mostSpecific[0];
            return true;
        }

        // When unrelated interfaces share the same lower bounds there may be no unique
        // best interface. The root object remains a valid conservative common type.
        var objectType = preserveNullability ? "System.Object?" : "System.Object";
        if (candidates.Contains(objectType, StringComparer.Ordinal))
        {
            commonType = objectType;
            return true;
        }
        commonType = "";
        return false;
    }

    private MethodSymbol? SpecializeGenericMethod(MethodSymbol template, IReadOnlyList<string> rawTypeArguments,
        MethodContext context, SourceLocation callLocation)
    {
        var declaration = template.Declaration;
        var typeParameters = declaration.TypeParameters ?? [];
        var cacheKey = template.QualifiedName + "@" + declaration.Location.File + ":" + declaration.Location.Line + ":" + declaration.Location.Column +
            "<" + string.Join(",", rawTypeArguments) + ">";
        if (_genericMethodSpecializations.TryGetValue(cacheKey, out var cached)) return cached;
        if (_failedGenericMethodSpecializations.Contains(cacheKey)) return null;
        if (rawTypeArguments.Count != typeParameters.Count)
        {
            _failedGenericMethodSpecializations.Add(cacheKey);
            diagnostics.Add("LS1101", DiagnosticSeverity.Error,
                $"Generic method '{declaration.Name}' requires {typeParameters.Count} type argument(s), but {rawTypeArguments.Count} were supplied.", callLocation);
            return null;
        }

        // Explicit method type arguments are source-level type references just like
        // parameter, return, and constructed-type arguments. Validate them before
        // specialization so a private nested type cannot leak through a generic call.
        foreach (var typeArgument in rawTypeArguments)
            ValidateTypeAccessibility(typeArgument, context.Method.Owner, callLocation);

        var typeArguments = rawTypeArguments.Select(argument => NormalizeEnumType(argument, context.Method.Owner)).ToArray();
        var substitutions = typeParameters.Zip(typeArguments).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal);
        foreach (var clause in declaration.GenericConstraints ?? [])
        {
            var parameterIndex = Array.FindIndex(typeParameters.ToArray(), parameter => parameter == clause.TypeParameter);
            if (parameterIndex < 0 || parameterIndex >= typeArguments.Length) continue;
            foreach (var rawConstraint in clause.Constraints)
            {
                var constraint = GenericTypeSpecializer.RewriteTypeName(rawConstraint, substitutions);
                var normalizedConstraint = constraint is "class" or "struct" or "enum" || constraint.StartsWith("new(", StringComparison.Ordinal)
                    ? constraint
                    : NormalizeEnumType(constraint, template.Owner);
                if (!SatisfiesGenericConstraint(typeArguments[parameterIndex], normalizedConstraint, context.Method.Owner))
                {
                    _failedGenericMethodSpecializations.Add(cacheKey);
                    diagnostics.Add("LS1102", DiagnosticSeverity.Error,
                        $"Type argument '{rawTypeArguments[parameterIndex]}' does not satisfy constraint '{rawConstraint}' for type parameter '{clause.TypeParameter}' on method '{declaration.Name}'.",
                        callLocation);
                    return null;
                }
            }
        }

        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
            cacheKey + "|" + string.Join(",", typeArguments))).AsSpan(0, 10)).ToLowerInvariant();
        var specializedName = declaration.Name + "__g_" + hash;
        var rewritten = (MethodDeclaration)GenericTypeSpecializer.RewriteMember(declaration, template.Owner.Name, substitutions);
        rewritten = rewritten with
        {
            Name = specializedName,
            HasTypeParameters = false,
            TypeParameters = null,
            GenericConstraints = null,
            ReturnType = NormalizeEnumType(rewritten.ReturnType, template.Owner),
            Parameters = rewritten.Parameters.Select(parameter => parameter with { TypeName = NormalizeEnumType(parameter.TypeName, template.Owner) }).ToArray()
        };
        if (GetQualifiedTypeName(template.Owner) == "System.Enum" && declaration.Name == "GetValues")
            rewritten = rewritten with { Body = CreateEnumGetValuesBody(typeArguments[0], callLocation) };
        var specialized = template with
        {
            Declaration = rewritten,
            IsGenericMethodSpecialization = true,
            GenericTemplate = template.Declaration
        };
        _genericMethodSpecializations.Add(cacheKey, specialized);
        _methodList.Add(specialized);
        return specialized;
    }

    private BlockStatement CreateEnumGetValuesBody(string enumTypeName, SourceLocation location)
    {
        var enumType = _types.FirstOrDefault(type => type.Kind == "enum" && GetQualifiedTypeName(type) == enumTypeName);
        if (enumType is null || !_enumUnderlyingTypes.TryGetValue(enumTypeName, out var underlyingType))
            return new([new ReturnStatement(new UnsupportedExpression("enum value metadata could not be resolved", location), location)], location);

        var width = underlyingType switch
        {
            "int8" or "uint8" => 8,
            "int16" or "uint16" or "char" => 16,
            "int32" or "uint32" => 32,
            "int64" or "uint64" => 64,
            "int" or "uint" => _nativePointerSize * 8,
            _ => 0
        };
        var values = enumType.Members.OfType<EnumMemberDeclaration>()
            .Select((member, index) =>
            {
                var key = enumTypeName + "." + member.Name;
                var text = _constantFields.TryGetValue(key, out var constant) ? constant.Binding.Value : "0";
                var value = BigInteger.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
                var unsignedValue = value.Sign < 0 && width > 0 ? value + (BigInteger.One << width) : value;
                return (Member: member, Value: value, SortValue: unsignedValue, Index: index);
            })
            .OrderBy(item => item.SortValue)
            .ThenBy(item => item.Index)
            .ToArray();

        var enumNameParts = enumTypeName.Split('.');
        Expression CreateEnumReceiver()
        {
            Expression receiver = new NameExpression(enumNameParts[0], location);
            foreach (var namePart in enumNameParts.Skip(1))
                receiver = new MemberAccessExpression(receiver, namePart, location);
            return receiver;
        }
        var elements = values.Select(item => (Expression)new MemberAccessExpression(CreateEnumReceiver(), item.Member.Name, location)).ToArray();
        var initializer = new InlineArrayInitializerExpression(elements, location);
        var allocation = new AllocationExpression(enumTypeName + "[]", [], initializer, false, location);
        return new([new ReturnStatement(allocation, location)], location);
    }

    private bool TryBindCallArguments(CallExpression call, MethodSymbol candidate, IReadOnlyList<string> argumentTypes, out BoundCallArguments binding)
    {
        var parameters = candidate.Declaration.Parameters;
        var variadicIndex = parameters.Select((parameter, index) => (parameter, index)).Where(item => item.parameter.IsVariadic).Select(item => item.index).DefaultIfEmpty(-1).Single();
        var parameterBySource = new int[call.Arguments.Count]; var assigned = new bool[parameters.Count]; var namedSeen = false; var nextPositional = 0;
        var variadicSourceIndices = new List<int>(); var usesVariadicExpansion = false;
        for (var sourceIndex = 0; sourceIndex < call.Arguments.Count; sourceIndex++)
        {
            var name = call.ArgumentNames is not null && sourceIndex < call.ArgumentNames.Count ? call.ArgumentNames[sourceIndex] : null;
            int parameterIndex;
            if (name is null)
            {
                if (namedSeen) { binding = null!; return false; }
                while (nextPositional < assigned.Length && assigned[nextPositional] && nextPositional != variadicIndex) nextPositional++;
                if (nextPositional >= parameters.Count)
                {
                    if (variadicIndex < 0) { binding = null!; return false; }
                    parameterIndex = variadicIndex;
                }
                else if (nextPositional == variadicIndex) parameterIndex = variadicIndex;
                else parameterIndex = nextPositional++;
            }
            else
            {
                namedSeen = true;
                parameterIndex = -1;
                for (var index = 0; index < parameters.Count; index++)
                    if (parameters[index].Name == name) { parameterIndex = index; break; }
                if (parameterIndex < 0) { binding = null!; return false; }
            }
            if (parameterIndex == variadicIndex)
            {
                if (assigned[parameterIndex] && (!usesVariadicExpansion || name is not null)) { binding = null!; return false; }
                var parameter = parameters[parameterIndex];
                var hasNormalForm = !usesVariadicExpansion && !assigned[parameterIndex] &&
                    sourceIndex < argumentTypes.Count &&
                    (name is not null || call.Arguments.Count - sourceIndex == 1) &&
                    CanConvert(argumentTypes[sourceIndex], parameter.TypeName, call.Arguments[sourceIndex]);
                if (hasNormalForm)
                {
                    if (assigned[parameterIndex]) { binding = null!; return false; }
                    assigned[parameterIndex] = true;
                }
                else
                {
                    if (name is not null) { binding = null!; return false; }
                    assigned[parameterIndex] = true;
                    usesVariadicExpansion = true;
                    variadicSourceIndices.Add(sourceIndex);
                }
            }
            else
            {
                if (assigned[parameterIndex]) { binding = null!; return false; }
                assigned[parameterIndex] = true;
            }
            parameterBySource[sourceIndex] = parameterIndex;
        }

        if (variadicIndex >= 0 && !assigned[variadicIndex]) usesVariadicExpansion = true;
        var defaults = new Expression?[parameters.Count];
        for (var parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
        {
            if (assigned[parameterIndex]) continue;
            if (parameters[parameterIndex].IsVariadic) { usesVariadicExpansion = true; continue; }
            if (parameters[parameterIndex].DefaultValue is not { } defaultValue) { binding = null!; return false; }
            defaults[parameterIndex] = defaultValue;
        }
        binding = new(parameterBySource, defaults, variadicIndex, usesVariadicExpansion, variadicSourceIndices); return true;
    }

    private static string GetParameterTypeForSource(MethodSymbol method, BoundCallArguments binding, int sourceIndex)
    {
        var parameterIndex = binding.ParameterBySource[sourceIndex];
        var parameter = method.Declaration.Parameters[parameterIndex];
        return parameter.IsVariadic && binding.UsesVariadicExpansion ? parameter.TypeName[..^2] : parameter.TypeName;
    }

    private bool IsMutableVariableArgument(Expression argument, MethodContext context) => argument switch
    {
        NameExpression name => context.Lookup(name.Name) is { Mutable: true } ||
            _boundUnqualifiedStructFields.TryGetValue(name, out var field) && !field.IsReadOnly && IsMutableStructLocation(new ThisExpression(name.Location), context),
        MemberAccessExpression access => _boundStaticFields.TryGetValue(access, out var field) && field.Mutable ||
            _boundStructFields.TryGetValue(access, out var structField) && !structField.IsReadOnly && IsMutableStructLocation(access.Receiver, context),
        UnaryExpression { Operator: "*" } => true,
        IndexExpression index => IsMutableIndexTarget(index, context),
        _ => false
    };
    private bool IsMutableIndexTarget(IndexExpression index, MethodContext context)
    {
        if (_stringIndexAccesses.ContainsKey(index)) return false;
        if (index.Receiver is NameExpression name && context.Lookup(name.Name) is { } local && TryParseInlineArrayType(local.Type, out _, out _))
            return local.Mutable;
        if (index.Receiver is MemberAccessExpression access)
        {
            if (_boundStructFields.TryGetValue(access, out var structField) && TryParseInlineArrayType(structField.TypeName, out _, out _))
                return !structField.IsReadOnly && IsMutableStructLocation(access.Receiver, context);
            if (_boundStaticFields.TryGetValue(access, out var staticField) && TryParseInlineArrayType(staticField.TypeName, out _, out _))
                return staticField.Mutable;
        }
        return true;
    }

    private MethodSymbol[]? ResolveCallCandidates(CallExpression call, MethodContext context)
    {
        if (call.Target is NameExpression unqualified)
            return _methods.Concat(_genericMethodDefinitions).Where(m => m.Owner == context.Method.Owner && m.Declaration.Name == unqualified.Name && m.Declaration.IsStatic).ToArray();

        if (call.Target is not MemberAccessExpression access || !TryFlattenName(access.Receiver, out var requestedType))
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Only calls to static methods on a type or within the current type are supported.", call.Location);
            return null;
        }
        if (access.Receiver is NameExpression receiverName && context.Lookup(receiverName.Name) is not null)
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "Instance method calls are not yet supported by the current backend.", call.Location);
            return null;
        }

        var matchingTypes = FindTypes(NormalizeBuiltinTypeQualifier(NormalizeEnumType(requestedType, context.Method.Owner)), context.Method.Owner);
        if (matchingTypes.Length == 0)
        {
            diagnostics.Add("LS0200", DiagnosticSeverity.Error, $"The type '{requestedType}' does not exist in the current compilation.", access.Receiver.Location);
            return null;
        }
        if (matchingTypes.Length > 1)
        {
            diagnostics.Add("LS2001", DiagnosticSeverity.Error, $"The type name '{requestedType}' is ambiguous.", access.Receiver.Location);
            return null;
        }

        var owner = matchingTypes[0];
        return _methods.Concat(_genericMethodDefinitions).Where(m => m.Owner == owner && m.Declaration.Name == access.Member && m.Declaration.IsStatic && IsMethodAccessible(m, context.Method.Owner)).ToArray();
    }

    private static string GetQualifiedTypeName(TypeDeclaration type) => string.IsNullOrEmpty(type.Namespace) ? type.Name : type.Namespace + "." + type.Name;
    private bool IsConstructorAccessible(MethodSymbol constructor, TypeDeclaration accessingType)
    {
        var accessibility = constructor.SourceConstructor?.Accessibility ??
            (constructor.Declaration.IsPublic ? "public" : constructor.Owner.IsVirtual ? "protected" : "private");
        return IsAccessibilityAvailable(accessibility, constructor.Owner, accessingType);
    }
    private bool IsMethodAccessible(MethodSymbol method, TypeDeclaration accessingType) =>
        method.Declaration.ExplicitInterfaceName is null &&
        IsAccessibilityAvailable(method.Declaration.Accessibility, method.Owner, accessingType);

    private TypeDeclaration GetLexicalAccessingType(TypeDeclaration accessingType) =>
        _closureEnclosingMethods.TryGetValue(GetQualifiedTypeName(accessingType), out var enclosingMethod) ? enclosingMethod.Owner : accessingType;

    private static string GetFieldAccessibility(FieldDeclaration field) => field.IsPublic ? "public" : field.Accessibility;
    private static string GetFieldAccessibility(ConstantFieldDeclaration field) => field.IsPublic ? "public" : field.Accessibility;

    private bool IsAccessibilityAvailable(string accessibility, TypeDeclaration declaredOn, TypeDeclaration accessingType)
    {
        if (GetQualifiedTypeName(declaredOn) == GetQualifiedTypeName(accessingType)) return true;
        accessingType = GetLexicalAccessingType(accessingType);
        if (accessibility.Contains("public", StringComparison.Ordinal) || accessibility.Contains("internal", StringComparison.Ordinal)) return true;
        var declaringName = GetQualifiedTypeName(declaredOn);
        var accessingName = GetQualifiedTypeName(accessingType);
        if (declaringName == accessingName) return true;
        if (IsLexicallyWithinType(accessingType, declaringName) || IsLexicallyWithinType(declaredOn, accessingName)) return true;
        return accessibility.Contains("protected", StringComparison.Ordinal) && IsBaseClassOf(declaringName, accessingName);
    }

    private bool IsTypeAccessible(TypeDeclaration declaredType, TypeDeclaration accessingType)
    {
        accessingType = GetLexicalAccessingType(accessingType);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var current = declaredType;
        while (visited.Add(GetQualifiedTypeName(current)))
        {
            var accessibility = current.Accessibility;
            var accessingName = GetQualifiedTypeName(accessingType);
            var currentAccessible = accessibility.Contains("public", StringComparison.Ordinal) ||
                accessibility.Contains("internal", StringComparison.Ordinal) ||
                accessibility.Contains("private", StringComparison.Ordinal) &&
                (accessingName == GetQualifiedTypeName(current) || current.ContainingTypeName is { } privateContainer &&
                 IsLexicallyWithinType(accessingType, privateContainer)) ||
                accessibility.Contains("protected", StringComparison.Ordinal) && current.ContainingTypeName is { } protectedContainer &&
                IsBaseClassOf(protectedContainer, accessingName);
            if (!currentAccessible) return false;

            if (current.ContainingTypeName is not { } containingTypeName) return true;
            var containingType = _types.FirstOrDefault(candidate => GetQualifiedTypeName(candidate) == containingTypeName);
            if (containingType is null) return true;
            current = containingType;
        }
        return false;
    }

    private bool IsLexicallyWithinType(TypeDeclaration accessingType, string containingTypeName)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var current = accessingType;
        while (visited.Add(GetQualifiedTypeName(current)))
        {
            if (GetQualifiedTypeName(current) == containingTypeName) return true;
            if (current.ContainingTypeName is not { } parentName) return false;
            if (parentName == containingTypeName) return true;
            var parent = _types.FirstOrDefault(candidate => GetQualifiedTypeName(candidate) == parentName);
            if (parent is null) return false;
            current = parent;
        }
        return false;
    }

    private void ValidateInconsistentAccessibility(IReadOnlyList<MethodSymbol> methods)
    {
        static int TypeAccessRank(TypeDeclaration type) => type.Accessibility.Contains("public", StringComparison.Ordinal) ? 2 : 1;
        static int MemberAccessRank(string accessibility)
        {
            if (accessibility.Contains("private", StringComparison.Ordinal) && accessibility.Contains("protected", StringComparison.Ordinal)) return 1;
            if (accessibility.Contains("public", StringComparison.Ordinal) || accessibility.Contains("protected", StringComparison.Ordinal)) return 2;
            return accessibility.Contains("internal", StringComparison.Ordinal) ? 1 : 0;
        }

        void CheckTypeReference(TypeDeclaration owner, string typeName, int exposedRank, SourceLocation location, string memberDescription)
        {
            if (exposedRank == 0) return;
            var unwrapped = typeName;
            while (true)
            {
                if (unwrapped.EndsWith('?') || unwrapped.EndsWith('*')) { unwrapped = unwrapped[..^1]; continue; }
                if (TryParseManagedArrayType(unwrapped, out var arrayElement, out _)) { unwrapped = arrayElement; continue; }
                if (TryParseInlineArrayType(unwrapped, out var inlineElement, out _)) { unwrapped = inlineElement; continue; }
                break;
            }
            var matches = FindTypes(unwrapped, owner);
            if (matches.Length != 1 || TypeAccessRank(matches[0]) >= exposedRank) return;
            diagnostics.Add("LS0840", DiagnosticSeverity.Error,
                $"Type '{GetQualifiedTypeName(matches[0])}' is less accessible than {memberDescription}.", location);
        }

        foreach (var type in _types)
        {
            var typeRank = TypeAccessRank(type);
            foreach (var baseType in type.BaseTypes ?? [])
                CheckTypeReference(type, baseType, typeRank, type.Location, $"base type '{GetQualifiedTypeName(type)}'");

            foreach (var member in type.Members)
            {
                switch (member)
                {
                    case FieldDeclaration field:
                        CheckTypeReference(type, field.TypeName, Math.Min(typeRank, MemberAccessRank(field.IsPublic ? "public" : field.Accessibility)),
                            field.Location, $"field '{GetQualifiedTypeName(type)}.{field.Name}'");
                        break;
                    case ConstantFieldDeclaration constant:
                        CheckTypeReference(type, constant.TypeName, Math.Min(typeRank, MemberAccessRank(constant.IsPublic ? "public" : constant.Accessibility)),
                            constant.Location, $"constant '{GetQualifiedTypeName(type)}.{constant.Name}'");
                        break;
                    case MethodDeclaration method:
                        var methodRank = Math.Min(typeRank, MemberAccessRank(method.Accessibility));
                        CheckTypeReference(type, method.ReturnType, methodRank, method.Location, $"return type of method '{method.Name}'");
                        foreach (var parameter in method.Parameters)
                            CheckTypeReference(type, parameter.TypeName, methodRank, parameter.Location, $"parameter '{parameter.Name}' of method '{method.Name}'");
                        break;
                    case ComputedPropertyDeclaration property:
                        var propertyRank = Math.Min(typeRank, MemberAccessRank(property.Accessibility));
                        CheckTypeReference(type, property.TypeName, propertyRank, property.Location, $"property '{GetQualifiedTypeName(type)}.{property.Name}'");
                        break;
                    case ComputedIndexerDeclaration indexer:
                        var indexerRank = Math.Min(typeRank, MemberAccessRank(indexer.Accessibility));
                        CheckTypeReference(type, indexer.TypeName, indexerRank, indexer.Location, $"indexer on '{GetQualifiedTypeName(type)}'");
                        foreach (var parameter in indexer.Parameters)
                            CheckTypeReference(type, parameter.TypeName, indexerRank, parameter.Location, $"indexer parameter '{parameter.Name}' on '{GetQualifiedTypeName(type)}'");
                        break;
                    case ConstructorDeclaration constructor:
                        var constructorRank = Math.Min(typeRank, MemberAccessRank(constructor.Accessibility));
                        foreach (var parameter in constructor.Parameters)
                            CheckTypeReference(type, parameter.TypeName, constructorRank, parameter.Location, $"parameter '{parameter.Name}' of constructor '{GetQualifiedTypeName(type)}'");
                        break;
                }
            }
        }
    }
    private static bool TryFlattenName(Expression expression, out string name)
    {
        switch (expression)
        {
            case NameExpression identifier: name = identifier.Name; return true;
            case MemberAccessExpression member when TryFlattenName(member.Receiver, out var parent): name = parent + "." + member.Member; return true;
            default: name = ""; return false;
        }
    }
    // -1 means first candidate is better, +1 means second is better, and 0 means tied/incomparable.
    private int CompareCandidates(MethodSymbol first, MethodSymbol second, IReadOnlyList<string> argumentTypes, IReadOnlyList<Expression> arguments, BoundCallArguments firstBinding, BoundCallArguments secondBinding)
    {
        var overall = 0;
        for (var index = 0; index < argumentTypes.Count; index++)
        {
            if (arguments[index] is DefaultValueExpression { TypeName: null } or InlineOutVariableExpression) continue;
            var firstTarget = GetParameterTypeForSource(first, firstBinding, index);
            var secondTarget = GetParameterTypeForSource(second, secondBinding, index);
            if (firstTarget == secondTarget) continue;
            var firstRank = ConversionRank(argumentTypes[index], firstTarget, arguments[index]);
            var secondRank = ConversionRank(argumentTypes[index], secondTarget, arguments[index]);
            var current = firstRank == secondRank ? CompareTargetSpecificity(firstTarget, secondTarget) : Math.Sign(firstRank - secondRank);
            if (current == 0) continue;
            if (overall != 0 && overall != current) return 0;
            overall = current;
        }
        if (overall != 0) return overall;
        if (first.IsGenericMethodSpecialization != second.IsGenericMethodSpecialization)
            return first.IsGenericMethodSpecialization ? 1 : -1;
        if (first.IsGenericMethodSpecialization && second.IsGenericMethodSpecialization)
        {
            var firstMoreSpecific = IsMoreSpecificGenericMethod(first, second);
            var secondMoreSpecific = IsMoreSpecificGenericMethod(second, first);
            if (firstMoreSpecific != secondMoreSpecific) return firstMoreSpecific ? -1 : 1;
        }
        return 0;
    }

    private bool IsMoreSpecificGenericMethod(MethodSymbol candidate, MethodSymbol general)
    {
        if (candidate.GenericTemplate is not { } specificTemplate || general.GenericTemplate is not { } generalTemplate ||
            specificTemplate.Parameters.Count != generalTemplate.Parameters.Count)
            return false;

        var substitutions = new Dictionary<string, string>(StringComparer.Ordinal);
        var generalTypeParameters = (generalTemplate.TypeParameters ?? []).ToHashSet(StringComparer.Ordinal);
        for (var index = 0; index < specificTemplate.Parameters.Count; index++)
        {
            var specificParameter = specificTemplate.Parameters[index];
            var generalParameter = generalTemplate.Parameters[index];
            if (specificParameter.PassingMode != generalParameter.PassingMode ||
                specificParameter.IsVariadic != generalParameter.IsVariadic ||
                !CanSubstituteGenericParameterType(generalParameter.TypeName, specificParameter.TypeName,
                    generalTypeParameters, substitutions, general.Owner, candidate.Owner))
                return false;
        }
        return true;
    }

    private bool CanSubstituteGenericParameterType(string generalType, string specificType,
        IReadOnlySet<string> generalTypeParameters, IDictionary<string, string> substitutions,
        TypeDeclaration generalOwner, TypeDeclaration specificOwner)
    {
        if (generalTypeParameters.Contains(generalType))
        {
            if (substitutions.TryGetValue(generalType, out var previous)) return previous == specificType;
            substitutions[generalType] = specificType;
            return true;
        }

        if (generalType.EndsWith("?", StringComparison.Ordinal))
            return specificType.EndsWith("?", StringComparison.Ordinal) &&
                CanSubstituteGenericParameterType(generalType[..^1], specificType[..^1], generalTypeParameters,
                    substitutions, generalOwner, specificOwner);
        if (TryParseManagedArrayType(generalType, out var generalElement, out var generalRank))
            return TryParseManagedArrayType(specificType, out var specificElement, out var specificRank) &&
                generalRank == specificRank && CanSubstituteGenericParameterType(generalElement, specificElement,
                    generalTypeParameters, substitutions, generalOwner, specificOwner);
        if (generalType.EndsWith("*", StringComparison.Ordinal))
            return specificType.EndsWith("*", StringComparison.Ordinal) &&
                CanSubstituteGenericParameterType(generalType[..^1], specificType[..^1], generalTypeParameters,
                    substitutions, generalOwner, specificOwner);
        if (TryParseInlineArrayType(generalType, out var generalInlineElement, out var generalInlineLength))
            return TryParseInlineArrayType(specificType, out var specificInlineElement, out var specificInlineLength) &&
                generalInlineLength == specificInlineLength &&
                CanSubstituteGenericParameterType(generalInlineElement, specificInlineElement, generalTypeParameters,
                    substitutions, generalOwner, specificOwner);

        if (TrySplitGenericType(generalType, out var generalDefinitionName, out var generalArguments))
        {
            if (!TrySplitGenericType(specificType, out var specificDefinitionName, out var specificArguments) ||
                generalArguments.Count != specificArguments.Count)
                return false;

            var generalDefinition = GenericTypeSpecializer.ResolveDefinition(generalDefinitionName, generalOwner, _genericDefinitions);
            var specificDefinition = GenericTypeSpecializer.ResolveDefinition(specificDefinitionName, specificOwner, _genericDefinitions);
            var sameDefinition = generalDefinition is not null && specificDefinition is not null
                ? GetQualifiedTypeName(generalDefinition) == GetQualifiedTypeName(specificDefinition)
                : NormalizeEnumType(generalDefinitionName, generalOwner) == NormalizeEnumType(specificDefinitionName, specificOwner);
            if (!sameDefinition) return false;

            for (var index = 0; index < generalArguments.Count; index++)
                if (!CanSubstituteGenericParameterType(generalArguments[index], specificArguments[index], generalTypeParameters,
                    substitutions, generalOwner, specificOwner))
                    return false;
            return true;
        }

        return NormalizeEnumType(generalType, generalOwner) == NormalizeEnumType(specificType, specificOwner);
    }

    private static int VariadicBindingPenalty(BoundCallArguments binding) => binding.UsesVariadicExpansion ? 1 : 0;
    private int ConversionRank(string from, string to, Expression expression)
    {
        if (from == to) return 0;
        if (!CanConvert(from, to, expression)) return int.MaxValue;
        if (IntegerTypes.Contains(from) && IntegerTypes.Contains(to) && IsImplicitIntegerConversion(from, to)) return 1;
        if (from == "char" && IntegerTypes.Contains(to) || from == "float" && to == "double") return 1;
        if ((IntegerTypes.Contains(from) || from == "char") && FloatingTypes.Contains(to)) return 2;
        return CanConvert(from, to, null) ? 2 : 3;
    }
    private int CompareTargetSpecificity(string first, string second)
    {
        var firstToSecond = CanConvert(first, second, null);
        var secondToFirst = CanConvert(second, first, null);
        if (firstToSecond && !secondToFirst) return -1;
        if (secondToFirst && !firstToSecond) return 1;
        if (IntegerTypes.Contains(first) && IntegerTypes.Contains(second))
        {
            var firstWidth = Width(first); var secondWidth = Width(second);
            if (firstWidth < secondWidth) return -1;
            if (secondWidth < firstWidth) return 1;
        }
        return 0;
    }
    private MethodSymbol? FindUserUnaryOperator(string op, string operandType, Expression operand, MethodContext context)
    {
        var applicable = _methods
            .Where(candidate => candidate.Owner.Kind is "struct" or "class" && candidate.Declaration.IsOperator &&
                candidate.Declaration.Operator == op && candidate.Declaration.Parameters.Count == 1)
            .Select(candidate => (Method: candidate, Parameter: candidate.Declaration.Parameters[0].TypeName))
            .Where(candidate => CanConvertInContext(operandType, candidate.Parameter, operand, context))
            .Select(candidate => (candidate.Method, Rank: ConversionRank(operandType, candidate.Parameter, operand)))
            .ToArray();
        if (applicable.Length == 0) return null;
        var bestRank = applicable.Min(candidate => candidate.Rank);
        var best = applicable.Where(candidate => candidate.Rank == bestRank).ToArray();
        if (best.Length == 1) return best[0].Method;
        diagnostics.Add("LS0750", DiagnosticSeverity.Error, $"Unary operator '{op}' is ambiguous for operand type '{operandType}'.", operand.Location);
        return null;
    }

    private MethodSymbol? FindUserBinaryOperator(string op, string leftType, string rightType,
        Expression left, Expression right, MethodContext context, SourceLocation location)
    {
        var declaredOperator = op == "!=" ? "==" : op;
        var applicable = _methods
            .Where(candidate => candidate.Owner.Kind is "struct" or "class" && candidate.Declaration.IsOperator &&
                candidate.Declaration.Operator == declaredOperator && candidate.Declaration.Parameters.Count == 2)
            .Select(candidate => (Method: candidate, Left: candidate.Declaration.Parameters[0].TypeName, Right: candidate.Declaration.Parameters[1].TypeName))
            .Where(candidate => CanConvertInContext(leftType, candidate.Left, left, context) && CanConvertInContext(rightType, candidate.Right, right, context))
            .Select(candidate => (candidate.Method,
                Rank: ConversionRank(leftType, candidate.Left, left) + ConversionRank(rightType, candidate.Right, right)))
            .ToArray();
        if (applicable.Length == 0) return null;
        var bestRank = applicable.Min(candidate => candidate.Rank);
        var best = applicable.Where(candidate => candidate.Rank == bestRank).ToArray();
        if (best.Length == 1) return best[0].Method;
        diagnostics.Add("LS0750", DiagnosticSeverity.Error, $"Operator '{op}' is ambiguous for operands '{leftType}' and '{rightType}'.", location);
        return null;
    }

    private void ReportConstantTypeTestWarning(BinaryExpression binary, string op, string staticType, bool alwaysMatches)
    {
        var constantResult = op == "is" ? alwaysMatches : !alwaysMatches;
        diagnostics.Add("LS3101", DiagnosticSeverity.Warning,
            $"This '{op}' type test is always {(constantResult ? "true" : "false")} for static type '{staticType}'.", binary.Location);
    }

    private bool TryGetConstantReferenceTypeTestResult(string sourceType, string targetType, out bool alwaysMatches)
    {
        alwaysMatches = false;
        var sourceIsNullable = IsNullableClassType(sourceType) || IsNullableDelegateType(sourceType) ||
            IsNullableStringType(sourceType) || IsNullableArrayType(sourceType);
        var source = NullableUnderlyingType(sourceType);

        if (source == targetType)
        {
            alwaysMatches = !sourceIsNullable;
            return !sourceIsNullable;
        }

        if (IsClassType(source))
        {
            var sourceIsObjectRoot = source is "System.Object" or "object";
            if (targetType is "System.Object" or "object")
            {
                alwaysMatches = !sourceIsNullable;
                return !sourceIsNullable;
            }
            if (sourceIsObjectRoot) return false;

            if (IsClassType(targetType))
            {
                if (IsBaseClassOf(targetType, source))
                {
                    alwaysMatches = true;
                    return !sourceIsNullable;
                }
                if (IsBaseClassOf(source, targetType))
                {
                    if (_classTypes.TryGetValue(source, out var sourceClass) && sourceClass.Declaration.IsVirtual)
                        return false;
                    return true;
                }

                // Two unrelated class branches cannot have a common runtime class.
                return true;
            }

            if (IsInterfaceType(targetType))
            {
                if (GetImplementedInterfaceNames(source).Contains(targetType, StringComparer.Ordinal))
                {
                    alwaysMatches = true;
                    return !sourceIsNullable;
                }
                if (_classTypes.TryGetValue(source, out var sourceClass) && sourceClass.Declaration.IsVirtual)
                    return false;
                return true;
            }

            if (IsDelegateType(targetType)) return true;
            return false;
        }

        if (IsInterfaceType(source))
        {
            if (targetType is "System.Object" or "object")
            {
                alwaysMatches = !sourceIsNullable;
                return !sourceIsNullable;
            }
            if (!IsInterfaceType(targetType)) return false;

            var sourceDeclaration = _interfaceTypes[source];
            if (GetInterfaceHierarchy(sourceDeclaration).Any(type => GetQualifiedTypeName(type) == targetType))
            {
                alwaysMatches = !sourceIsNullable;
                return !sourceIsNullable;
            }
            // A runtime class may implement both unrelated interfaces.
            return false;
        }

        if (IsDelegateType(source))
        {
            if (targetType is "System.Object" or "object" or "System.Delegate")
            {
                alwaysMatches = !sourceIsNullable;
                return !sourceIsNullable;
            }
            if (IsClassType(targetType) || IsInterfaceType(targetType) || IsDelegateType(targetType))
                return true;
            return false;
        }

        if (IsStringType(source))
        {
            if (targetType is "System.Object" or "object")
            {
                alwaysMatches = !sourceIsNullable;
                return !sourceIsNullable;
            }
            return true;
        }

        if (TryParseManagedArrayType(source, out _, out _))
        {
            if (targetType is "System.Object" or "object" or "System.Array")
            {
                alwaysMatches = !sourceIsNullable;
                return !sourceIsNullable;
            }
            if (TryParseManagedArrayType(targetType, out _, out _)) return true;
            return IsClassType(targetType) || IsInterfaceType(targetType) || IsDelegateType(targetType);
        }

        return false;
    }

    private string AnalyzeBinary(BinaryExpression binary, MethodContext context)
    {
        var op = binary.Operator;
        var previousPropertyAssignmentProbe = _propertyAssignmentProbe;
        if (op == "=" && binary.Left is (MemberAccessExpression or IndexExpression))
            _propertyAssignmentProbe = binary.Left;
        string left;
        try { left = AnalyzeExpression(binary.Left, context); }
        finally { _propertyAssignmentProbe = previousPropertyAssignmentProbe; }
        if (op is "is" or "is not" or "as")
        {
            if ((op is "is" or "is not") && binary.Right is LiteralExpression { Value: "null" })
            {
                if (left != "null" && !IsClassReferenceType(left) && !IsDelegateType(left) && !IsNullableDelegateType(left) && !IsNullableStringType(left) && !IsNullableArrayType(left))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"A null type test requires a reference operand, not '{left}'.", binary.Location);
                _typeOperatorTargets[binary] = "null";
                return "bool";
            }

            if (binary.Right is not NameExpression && !TryFlattenName(binary.Right, out _))
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"The right operand of '{op}' must be a supported type name.", binary.Right.Location);
                return op == "as" ? "?" : "bool";
            }
            var targetName = binary.Right is NameExpression targetIdentifier ? targetIdentifier.Name : TryFlattenName(binary.Right, out var flattenedTarget) ? flattenedTarget : "?";
            var normalizedTargetName = NormalizeEnumType(targetName, context.Method.Owner);
            if (ValidateTypeAccessibility(targetName, context.Method.Owner, binary.Right.Location))
                return op == "as" ? "?" : "bool";
            if (targetName is "string" or "System.String")
            {
                _typeOperatorTargets[binary] = "string";
                if (op is "is" or "is not")
                {
                    ValidateTypePatternVariable(binary, op, context);
                    if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null && !IsStringType(left))
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Runtime string type tests from '{left}' require a reference value.", binary.Left.Location);
                    if (left == "string") ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: true);
                    else if (left == "null" || TryParseManagedArrayType(left, out _, out _))
                        ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: false);
                    return "bool";
                }
                if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null && !IsStringType(left))
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Conditional casts to string from '{left}' require a reference value.", binary.Left.Location);
                return left == "string" ? "string" : "string?";
            }
            if (IsBoxableScalarOrEnum(normalizedTargetName))
            {
                _typeOperatorTargets[binary] = normalizedTargetName;
                ValidateTypePatternVariable(binary, op, context);
                if (op == "as")
                {
                    if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null)
                    {
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Conditional unboxing from '{left}' requires a class reference value.", binary.Left.Location);
                        return "?";
                    }
                    return normalizedTargetName + "?";
                }
                if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null && !IsBoxableScalarOrEnum(left) && !IsNullableValueType(left))
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Runtime value type tests from '{left}' require a class reference value or a boxable value.", binary.Left.Location);
                if (left == "null" || IsStringType(left) || TryParseManagedArrayType(left, out _, out _))
                    ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: false);
                else if (IsBoxableScalarOrEnum(left))
                    ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: left == normalizedTargetName);
                else if (IsNullableValueType(left) && NullableUnderlyingType(left) != normalizedTargetName)
                    ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: false);
                return "bool";
            }
            var valueTypeMatches = FindTypes(targetName, context.Method.Owner);
            if (valueTypeMatches.Length == 1 && valueTypeMatches[0].Kind == "struct")
            {
                var targetStruct = GetQualifiedTypeName(valueTypeMatches[0]);
                _typeOperatorTargets[binary] = targetStruct;
                ValidateTypePatternVariable(binary, op, context);
                if (op == "as")
                {
                    if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null)
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                            $"Conditional unboxing from '{left}' requires an object-reference value.", binary.Left.Location);
                    return targetStruct + "?";
                }
                if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null &&
                    !IsNullableValueType(left) && !IsBoxableScalarOrEnum(left) && !IsStructType(left))
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                        $"Runtime struct type tests from '{left}' require a value or object-reference operand.", binary.Left.Location);
                if (left == "null" || IsBoxableScalarOrEnum(left) || IsStringType(left) ||
                    TryParseManagedArrayType(left, out _, out _) || IsStructType(left) && left != targetStruct ||
                    IsNullableValueType(left) && NullableUnderlyingType(left) != targetStruct)
                    ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: false);
                else if (left == targetStruct)
                    ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: true);
                return "bool";
            }
            if (TryParseManagedArrayType(normalizedTargetName, out _, out var arrayRank))
            {
                if (arrayRank > 3)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Runtime type tests for array rank {arrayRank} are not supported; the backend supports ranks one through three.", binary.Right.Location);
                    return op == "as" ? "?" : "bool";
                }
                var targetArrayType = normalizedTargetName.EndsWith("?", StringComparison.Ordinal)
                    ? normalizedTargetName[..^1] : normalizedTargetName;
                _typeOperatorTargets[binary] = targetArrayType;
                ValidateTypePatternVariable(binary, op, context);
                if (op == "as")
                {
                    if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null && !IsStringType(left))
                    {
                        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Conditional casts from '{left}' to an array require a reference value.", binary.Left.Location);
                        return targetArrayType + "?";
                    }
                    var exactNonNullableArray = TryParseManagedArrayType(left, out _, out _) &&
                        NullableUnderlyingType(left) == targetArrayType && !IsNullableArrayType(left);
                    return exactNonNullableArray ? targetArrayType : targetArrayType + "?";
                }
                if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null && !IsStringType(left))
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Runtime array type tests from '{left}' require a reference value.", binary.Left.Location);
                if (TryParseManagedArrayType(left, out _, out _))
                {
                    if (NullableUnderlyingType(left) != targetArrayType)
                        ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: false);
                    else if (!IsNullableArrayType(left))
                        ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: true);
                }
                else if (IsStringType(left) || left == "null")
                    ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: false);
                return "bool";
            }
            var targetMatches = FindTypes(targetName, context.Method.Owner);
            if (targetMatches.Length != 1)
            {
                diagnostics.Add(targetMatches.Length == 0 ? "LS0200" : "LS2001", DiagnosticSeverity.Error,
                    targetMatches.Length == 0 ? $"The type '{targetName}' does not exist in the current compilation." : $"The type name '{targetName}' is ambiguous.", binary.Right.Location);
                return op == "as" ? "?" : "bool";
            }
            if (targetMatches[0].Kind is not ("class" or "interface" or "delegate"))
            {
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"'{op}' currently supports class reference types; '{targetName}' requires boxing or interface runtime support.", binary.Right.Location);
                return op == "as" ? "?" : "bool";
            }
            var targetType = GetQualifiedTypeName(targetMatches[0]);
            if (op == "as")
            {
                if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null)
                {
                    diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Conditional casts from '{left}' require class reference runtime type information.", binary.Left.Location);
                    return "?";
                }
                _typeOperatorTargets[binary] = targetType;
                ValidateTypePatternVariable(binary, op, context);
                var sourceType = GetClassReferenceTypeForAnalysis(left);
                var provenUpcast = sourceType is not null && IsReferenceAssignable(targetType, sourceType);
                return provenUpcast ? targetType : targetType + "?";
            }

            if (left != "null" && GetClassReferenceTypeForAnalysis(left) is null)
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Runtime type tests for '{left}' require class reference runtime type information.", binary.Left.Location);
            _typeOperatorTargets[binary] = targetType;
            ValidateTypePatternVariable(binary, op, context);
            if (left == "null")
                ReportConstantTypeTestWarning(binary, op, left, alwaysMatches: false);
            else if (TryGetConstantReferenceTypeTestResult(left, targetType, out var constantMatch))
                ReportConstantTypeTestWarning(binary, op, left, constantMatch);
            return "bool";
        }
        if (op is "&&" or "||")
        {
            if (left != "bool") diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Operator '{op}' requires bool operands.", binary.Left.Location);
            var afterLeft = context.CaptureNullStates();
            RefineNullCondition(binary.Left, context, whenTrue: op == "&&");
            context.Push();
            var rightPatternCaptureScopes = op == "&&" ? DeclarePositiveTypePatternVariables(binary.Left, context) : [];
            var rightLogical = AnalyzeExpression(binary.Right, context);
            if (rightLogical != "bool") diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Operator '{op}' requires bool operands.", binary.Right.Location);
            var afterRight = context.CaptureNullStates();
            if (op == "&&")
            {
                binary.CapturedPatternNamesInRight = GetPositiveTypePatternNodes(binary.Left)
                    .Select(pattern => pattern.PatternVariableName)
                    .Where(name => name is not null && GetPatternCaptureEnvironment(binary.Left, name) is not null)
                    .Select(name => name!)
                    .ToHashSet(StringComparer.Ordinal);
            }
            ExitCaptureScopes(context, rightPatternCaptureScopes);
            context.Pop();
            context.RestoreNullStates(afterLeft);
            RefineNullCondition(binary.Left, context, whenTrue: op == "||");
            var skippedRight = context.CaptureNullStates();
            context.RestoreNullStates(MergeNullStates(afterRight, skippedRight));
            return "bool";
        }
        var hasNullStateKey = TryGetNullStateKey(binary.Left, context, out var assignmentFlowKey);
        var coalesceAssignmentState = op == "??=" && hasNullStateKey ? context.GetNullState(assignmentFlowKey) : NullState.MaybeNull;
        if (op == "??=" && IsNullableType(left) && hasNullStateKey) context.SetNullState(assignmentFlowKey, NullState.Null);
        var rightExpectedType = op is "=" or "??=" ? left : op == "??" && IsNullableType(left) ? NullableUnderlyingType(left) : null;
        var right = AnalyzeExpression(binary.Right, context, rightExpectedType);
        if (left == "?" || right == "?") return "?";
        if (op == "??")
        {
            if (!IsNullableType(left))
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"The left operand of '??' must be a nullable value or class reference, not '{left}'.", binary.Left.Location);
                return "?";
            }
            var nonNullableType = NullableUnderlyingType(left);
            if (!CanConvertInContext(right, nonNullableType + "?", binary.Right, context))
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"The right operand of '??' cannot be converted to '{nonNullableType}?'.", binary.Right.Location);
                return "?";
            }
            return IsNullableType(right) || right == "null" ? nonNullableType + "?" : nonNullableType;
        }
        if (op is "=" or "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=" or "??=")
        {
            string targetType; bool mutable;
            var isReadOnlyFieldTarget = false;
            var mayInitializeReadOnlyField = false;
            MethodContext.Local? assignmentLocal = null;
            ComputedPropertySymbol? assignmentProperty = null;
            Expression? assignmentPropertyReceiver = null;
            var assignmentPropertyIsNullConditional = false;
            var assignmentPropertyIsUnsafeContext = false;
            ComputedIndexerSymbol? assignmentIndexer = null;
            if (binary.Left is NameExpression name && context.Lookup(name.Name) is { } local) { targetType = local.Type; mutable = local.Mutable; assignmentLocal = local; }
            else if (binary.Left is NameExpression staticFieldName && _boundUnqualifiedStaticFields.TryGetValue(staticFieldName, out var unqualifiedStaticField))
            {
                targetType = unqualifiedStaticField.TypeName;
                isReadOnlyFieldTarget = !unqualifiedStaticField.Mutable;
                mayInitializeReadOnlyField = context.Method.IsTypeInitializer && unqualifiedStaticField.OwnerType == GetQualifiedTypeName(context.Method.Owner);
                mutable = unqualifiedStaticField.Mutable || mayInitializeReadOnlyField;
            }
            else if (binary.Left is NameExpression fieldName && _boundUnqualifiedStructFields.TryGetValue(fieldName, out var unqualifiedField))
            {
                targetType = unqualifiedField.TypeName;
                isReadOnlyFieldTarget = unqualifiedField.IsReadOnly;
                mayInitializeReadOnlyField = CanInitializeReadOnlyStructField(unqualifiedField, new ThisExpression(fieldName.Location), context);
                mutable = (!isReadOnlyFieldTarget || mayInitializeReadOnlyField) && IsMutableStructLocation(new ThisExpression(fieldName.Location), context);
                if (!mutable && context.Method.IsStructInstanceMethod && !context.Method.Declaration.IsMutating)
                    diagnostics.Add("LS0820", DiagnosticSeverity.Error, "A non-mutating struct method cannot modify its receiver.", binary.Location);
            }
            else if (binary.Left is NameExpression classFieldName && _boundUnqualifiedClassFields.TryGetValue(classFieldName, out var unqualifiedClassField))
            {
                targetType = unqualifiedClassField.TypeName;
                isReadOnlyFieldTarget = unqualifiedClassField.IsReadOnly;
                mayInitializeReadOnlyField = CanInitializeReadOnlyClassField(unqualifiedClassField, new ThisExpression(classFieldName.Location), context);
                mutable = (!isReadOnlyFieldTarget || mayInitializeReadOnlyField) && IsMutableClassFieldLocation(new ThisExpression(classFieldName.Location), context);
                if (!mutable && context.Method.IsClassInstanceMethod && !context.Method.Declaration.IsMutating)
                    diagnostics.Add("LS0820", DiagnosticSeverity.Error, "A non-mutating class method cannot modify its receiver.", binary.Location);
            }
            else if (binary.Left is NameExpression propertyName && _boundUnqualifiedComputedProperties.TryGetValue(propertyName, out assignmentProperty))
            {
                targetType = assignmentProperty.TypeName;
                mutable = assignmentProperty.Setter is not null && IsAccessibilityAvailable(assignmentProperty.Setter.Declaration.Accessibility, assignmentProperty.Setter.Owner, context.Method.Owner);
                if (!mutable)
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Property '{GetQualifiedTypeName(assignmentProperty.Owner)}.{assignmentProperty.Declaration.Name}' has no accessible set accessor.", propertyName.Location);
            }
            else if (binary.Left is MemberAccessExpression access && _boundStaticFields.TryGetValue(access, out var field))
            {
                targetType = field.TypeName;
                isReadOnlyFieldTarget = !field.Mutable;
                mayInitializeReadOnlyField = context.Method.IsTypeInitializer && field.OwnerType == GetQualifiedTypeName(context.Method.Owner);
                mutable = field.Mutable || mayInitializeReadOnlyField;
            }
            else if (binary.Left is MemberAccessExpression structAccess && _boundStructFields.TryGetValue(structAccess, out var structField))
            {
                targetType = structField.TypeName;
                isReadOnlyFieldTarget = structField.IsReadOnly;
                mayInitializeReadOnlyField = CanInitializeReadOnlyStructField(structField, structAccess.Receiver, context);
                mutable = (!isReadOnlyFieldTarget || mayInitializeReadOnlyField) && IsMutableStructLocation(structAccess.Receiver, context);
                if (!mutable && HasThisRoot(structAccess.Receiver) && context.Method.IsStructInstanceMethod && !context.Method.Declaration.IsMutating)
                    diagnostics.Add("LS0820", DiagnosticSeverity.Error, "A non-mutating struct method cannot modify its receiver.", binary.Location);
            }
            else if (binary.Left is MemberAccessExpression classAccess && _boundClassFields.TryGetValue(classAccess, out var classField))
            {
                targetType = classField.TypeName;
                isReadOnlyFieldTarget = classField.IsReadOnly;
                mayInitializeReadOnlyField = CanInitializeReadOnlyClassField(classField, classAccess.Receiver, context);
                mutable = (!isReadOnlyFieldTarget || mayInitializeReadOnlyField) && IsMutableClassFieldLocation(classAccess.Receiver, context);
                if (!mutable && (HasThisRoot(classAccess.Receiver) || IsCapturedThisRoot(classAccess.Receiver, context)) && !CanMutateClassReceiver(context))
                    diagnostics.Add("LS0820", DiagnosticSeverity.Error, "A non-mutating class method cannot modify its receiver.", binary.Location);
            }
            else if (binary.Left is MemberAccessExpression propertyAccess && _boundComputedProperties.TryGetValue(propertyAccess, out assignmentProperty))
            {
                targetType = assignmentProperty.TypeName;
                assignmentPropertyReceiver = propertyAccess.Receiver;
                assignmentPropertyIsNullConditional = propertyAccess.IsNullConditional;
                assignmentPropertyIsUnsafeContext = propertyAccess.IsUnsafeContext;
                mutable = assignmentProperty.Setter is not null && IsAccessibilityAvailable(assignmentProperty.Setter.Declaration.Accessibility, assignmentProperty.Setter.Owner, context.Method.Owner);
                if (!mutable)
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, $"Property '{GetQualifiedTypeName(assignmentProperty.Owner)}.{assignmentProperty.Declaration.Name}' has no accessible set accessor.", propertyAccess.Location);
            }
            else if (binary.Left is UnaryExpression { Operator: "*" })
            {
                targetType = left;
                mutable = !IsClassType(left);
            }
            else if (binary.Left is IndexExpression computedIndexerTarget && _boundComputedIndexers.TryGetValue(computedIndexerTarget, out assignmentIndexer))
            {
                targetType = assignmentIndexer.TypeName;
                mutable = assignmentIndexer.Setter is not null && IsAccessibilityAvailable(assignmentIndexer.Setter.Declaration.Accessibility, assignmentIndexer.Setter.Owner, context.Method.Owner);
                if (!mutable)
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "The matching indexer has no accessible set accessor.", computedIndexerTarget.Location);
                if (computedIndexerTarget.IsNullConditional)
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A null-conditional index is a value and cannot be used as an assignment target.", computedIndexerTarget.Location);
            }
            else if (binary.Left is IndexExpression indexedTarget)
            {
                targetType = left; mutable = IsMutableIndexTarget(indexedTarget, context);
                if (indexedTarget.IsNullConditional)
                    diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "A null-conditional index is a value and cannot be used as an assignment target.", indexedTarget.Location);
            }
            else { diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "Assignment target must be a mutable local, parameter, or supported static field.", binary.Location); return left; }
            if (!mutable)
            {
                if (assignmentProperty is not null) { }
                else if (isReadOnlyFieldTarget && !mayInitializeReadOnlyField)
                    diagnostics.Add("LS0231", DiagnosticSeverity.Error, "A readonly field may only be assigned in its declaring type's constructor or initializer.", binary.Location);
                else diagnostics.Add("LMC3012", DiagnosticSeverity.Error, "Cannot assign to an immutable local or readonly field.", binary.Location);
            }
            var assignedScopeDepth = GetScopeBoundDepth(binary.Right, context);
            var assignedBorrowedDepth = GetScopeBoundOrBorrowedDepth(binary.Right, context);
            var assignedReadOnlyReference = IsReadOnlyReferenceExpression(binary.Right, context);
            var isFirstScopedLocalAssignment = assignmentLocal is { ScopeBoundDepth: null, Declaration: { } localDeclaration } &&
                (localDeclaration.Initializer is null || localDeclaration.Initializer is LiteralExpression { Value: "null" }) &&
                assignedScopeDepth is { } localValueDepth && assignmentLocal.ScopeDepth >= localValueDepth &&
                IsScopeBoundReferenceType(targetType);
            var mayAssignScopedLocal = isFirstScopedLocalAssignment || _scopeBoundLocalAssignments.Contains(binary);
            if (isFirstScopedLocalAssignment && assignmentLocal is not null && assignedScopeDepth is { } assignedLocalDepth &&
                assignmentLocal.Declaration is { } assignedLocalDeclaration)
            {
                _scopeBoundLocalAssignments.Add(binary);
                _scopeBoundVariables.Add(assignedLocalDeclaration);
                context.SetScopeBound(assignedLocalDeclaration.Name, assignedLocalDepth, GetScopeBoundOrder(binary.Right, context));
            }
            var isFirstReadOnlyLocalAssignment = assignmentLocal is { IsReadOnlyReference: false, ScopeBoundDepth: null, Declaration: { } readOnlyDeclaration } &&
                (readOnlyDeclaration.Initializer is null || readOnlyDeclaration.Initializer is LiteralExpression { Value: "null" }) &&
                assignedReadOnlyReference && assignedBorrowedDepth is { } readOnlyValueDepth &&
                assignmentLocal.ScopeDepth >= readOnlyValueDepth && IsManagedReferenceValue(targetType);
            var mayAssignReadOnlyLocal = isFirstReadOnlyLocalAssignment || _readOnlyLocalAssignments.Contains(binary);
            if (isFirstReadOnlyLocalAssignment && assignmentLocal?.Declaration is { } readOnlyLocalDeclaration)
            {
                _readOnlyLocalAssignments.Add(binary);
                _scopeBoundVariables.Add(readOnlyLocalDeclaration);
                context.SetReadOnlyReference(readOnlyLocalDeclaration.Name);
            }
            var mayStoreScopedArrayElement = binary.Left is IndexExpression indexedArrayTarget &&
                _managedArrayIndexAccesses.ContainsKey(indexedArrayTarget) &&
                CanStoreScopeBoundArrayElement(indexedArrayTarget.Receiver, binary.Right, context) &&
                IsScopeBoundReferenceType(targetType);
            var mayStoreScopedClassField = false;
            if (op == "=" && binary.Left is MemberAccessExpression scopedFieldTarget &&
                _boundClassFields.TryGetValue(scopedFieldTarget, out var scopedFieldBinding) &&
                IsScopeBoundReferenceType(targetType) &&
                GetScopeBoundDepth(scopedFieldTarget.Receiver, context) is { } ownerScopeDepth &&
                GetScopeBoundOrder(scopedFieldTarget.Receiver, context) is { } ownerScopeOrder &&
                assignedScopeDepth is { } childScopeDepth &&
                GetScopeBoundOrder(binary.Right, context) is { } childScopeOrder &&
                ownerScopeDepth >= childScopeDepth &&
                (ownerScopeDepth > childScopeDepth || ownerScopeOrder > childScopeOrder))
            {
                mayStoreScopedClassField = true;
                _scopeBoundClassFields.Add((scopedFieldBinding.DeclaredOwnerType ?? scopedFieldBinding.OwnerType, scopedFieldTarget.Member));
            }
            var invalidScopedLocalReplacement = assignmentLocal?.ScopeBoundDepth is not null && assignedScopeDepth is null &&
                binary.Right is not LiteralExpression { Value: "null" };
            var invalidReadOnlyLocalReplacement = assignmentLocal?.IsReadOnlyReference == true && !assignedReadOnlyReference &&
                binary.Right is not LiteralExpression { Value: "null" };
            if (assignedScopeDepth is not null && !mayAssignScopedLocal && !mayStoreScopedArrayElement && !mayStoreScopedClassField ||
                assignedReadOnlyReference && !mayAssignReadOnlyLocal || invalidScopedLocalReplacement || invalidReadOnlyLocalReplacement)
                diagnostics.Add("LS0701", DiagnosticSeverity.Error,
                    assignmentLocal is null ? "A scope-bound or readonly reference cannot be stored in a field, static, array, or other heap-owned location." : "A scope-bound or readonly reference cannot be assigned after its declaration; declare an alias in the same or an inner lexical scope.",
                    binary.Right.Location);
            if (assignmentProperty is not null)
                BindComputedPropertyWrite(binary, assignmentPropertyReceiver, assignmentProperty, context,
                    assignmentPropertyIsNullConditional, assignmentPropertyIsUnsafeContext);
            if (assignmentIndexer is not null)
                BindComputedIndexerWrite(binary, (IndexExpression)binary.Left, assignmentIndexer, context);
            var compoundOperator = op switch
            {
                "+=" => "+", "-=" => "-", "*=" => "*", "/=" => "/", "%=" => "%",
                "&=" => "&", "|=" => "|", "^=" => "^", "<<=" => "<<", ">>=" => ">>",
                _ => null
            };
            if (compoundOperator is not null && (IsStructType(targetType) || IsClassType(targetType)))
            {
                var overloadedCompound = FindUserBinaryOperator(compoundOperator, targetType, right, binary.Left, binary.Right, context, binary.Location);
                if (overloadedCompound?.Declaration.ReturnType != targetType) overloadedCompound = null;
                if (overloadedCompound is null)
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Compound assignment '{op}' requires a matching '{compoundOperator}' operator returning '{targetType}'.", binary.Location);
                else _operatorTargets[binary] = overloadedCompound;
                return targetType;
            }
            if (op == "=" && !CanConvertInContext(right, targetType, binary.Right, context)) AddReferenceConversionDiagnostic(right, targetType, binary.Right, context, binary.Location);
            else if (op == "??=" && !IsNullableType(targetType))
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"The left operand of '??=' must be a nullable value or class reference, not '{targetType}'.", binary.Left.Location);
            else if (op == "??=" && !CanConvertInContext(right, targetType, binary.Right, context)) AddReferenceConversionDiagnostic(right, targetType, binary.Right, context, binary.Location);
            else if (IsNullableValueType(targetType) && op is "+=" or "-=" or "*=" or "/=" or "%=" or "&=" or "|=" or "^=" or "<<=" or ">>=")
            {
                var underlying = NullableUnderlyingType(targetType);
                var rightUnderlying = IsNullableValueType(right) ? NullableUnderlyingType(right) : right;
                if (IsNullableValueType(right) && rightUnderlying != underlying || !CanConvert(rightUnderlying, underlying, binary.Right))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Compound operator '{op}' requires an operand compatible with '{underlying}'.", binary.Right.Location);
                else if (op is "+=" or "-=" or "*=" or "/=" or "%=")
                {
                    if (!NumericTypes.Contains(underlying) || !NumericTypes.Contains(rightUnderlying)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Compound operator '{op}' requires numeric operands.", binary.Location);
                }
                else if (op is "&=" or "|=" or "^=")
                {
                    if (!(IntegerTypes.Contains(underlying) || underlying is "char" or "bool") || !(IntegerTypes.Contains(rightUnderlying) || rightUnderlying is "char" or "bool"))
                        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Compound operator '{op}' requires integral or bool operands.", binary.Location);
                }
                else if (IsNullableValueType(right) || !(IntegerTypes.Contains(underlying) || underlying == "char") || !(IntegerTypes.Contains(rightUnderlying) || rightUnderlying == "char"))
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A lifted shift compound assignment requires an integral nullable value and a non-nullable integral count.", binary.Location);
            }
            else if (IsStringType(targetType) && op == "+=" && CanStringify(right)) { RecordStringification(right); }
            else if (IsStringType(targetType) && op == "+=") diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"String concatenation cannot convert '{right}' to text; supported values include strings, enums, primitive bool, char, integer, and floating-point values, plus classes/structs with default ToString.", binary.Location);
            else if (IsPointerType(left) && (op is "+=" or "-=") && (IntegerTypes.Contains(right) || right == "char")) { }
            else if (IsEnumType(left) && (op is "+=" or "-=") && (IntegerTypes.Contains(right) || right == "char")) { }
            else if (IsEnumType(left) && (op is "&=" or "|=" or "^=") && right == left) { }
            else if (op is "+=" or "-=" or "*=" or "/=" or "%=") _ = PromoteNumeric(left, right, binary.Left, binary.Right, binary.Location);
            else if (op is "&=" or "|=" or "^=")
            {
                if (left == "bool" && right == "bool") { }
                else if (IntegerTypes.Contains(left) || left == "char") { _ = PromoteNumeric(left, right, binary.Left, binary.Right, binary.Location); }
                else diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Operator '{op}' requires integral or bool operands.", binary.Location);
            }
            else if (op is "<<=" or ">>=")
            {
                if (!(IntegerTypes.Contains(left) || left == "char") || !(IntegerTypes.Contains(right) || right == "char")) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Shift operands must be integers.", binary.Location);
                else if (!CanConvert(right, "int32", binary.Right)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A shift count must be convertible to int32.", binary.Right.Location);
            }
            if (op is "=" or "??=" && IsNullableType(targetType) && hasNullStateKey)
            {
                var assignedState = GetNullState(binary.Right, right, context);
                context.SetNullState(assignmentFlowKey, op == "=" ? assignedState : coalesceAssignmentState == NullState.NonNull ? NullState.NonNull : coalesceAssignmentState == NullState.Null ? assignedState : MergeNullState(NullState.NonNull, assignedState));
                if (binary.Left is NameExpression assignedName && context.Lookup(assignedName.Name) is not null)
                    context.InvalidateMemberNullStates("path:" + assignedName.Name + ".");
                else if (binary.Left is MemberAccessExpression)
                    context.InvalidateMemberNullStates(assignmentFlowKey + ".");
            }
            return targetType;
        }
        if (op is "&&" or "||") { if (left != "bool" || right != "bool") diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Operator '{op}' requires bool operands.", binary.Location); return "bool"; }
        if (op is "==" or "!=")
        {
            var leftDelegate = NullableUnderlyingType(left);
            var rightDelegate = NullableUnderlyingType(right);
            if (IsDelegateType(leftDelegate) || IsDelegateType(rightDelegate))
            {
                var compatibleDelegates = IsDelegateType(leftDelegate) && leftDelegate == rightDelegate;
                if (compatibleDelegates || left == "null" && (IsDelegateType(rightDelegate) || IsNullableDelegateType(right)) ||
                    right == "null" && (IsDelegateType(leftDelegate) || IsNullableDelegateType(left))) return "bool";
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Delegate equality requires values of the same delegate type or null.", binary.Location);
                return "bool";
            }
            if (IsNullableValueType(left) || IsNullableValueType(right))
            {
                var nullableValue = IsNullableValueType(left) ? left : right;
                var underlyingValue = NullableUnderlyingType(nullableValue);
                var compatibleValues = IsNullableValueType(left) && IsNullableValueType(right) && NullableUnderlyingType(left) == NullableUnderlyingType(right) ||
                    IsNullableValueType(left) && right == underlyingValue || IsNullableValueType(right) && left == underlyingValue;
                if (left == "null" && IsNullableValueType(right) || right == "null" && IsNullableValueType(left)) return "bool";
                if (compatibleValues && IsStructType(underlyingValue))
                {
                    var equalityOperator = _methods.FirstOrDefault(candidate => candidate.IsStructEqualityOperator && GetQualifiedTypeName(candidate.Owner) == underlyingValue &&
                        candidate.Declaration.ReturnType == "bool" && candidate.Declaration.Parameters.Count == 2 && candidate.Declaration.Parameters.All(parameter => parameter.TypeName == underlyingValue));
                    if (equalityOperator is not null) { _operatorTargets[binary] = equalityOperator; return "bool"; }
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Nullable struct equality requires a valid user-defined '==' operator.", binary.Location);
                    return "bool";
                }
                if (compatibleValues)
                    return "bool";
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Nullable value equality requires null or compatible values of the same underlying type.", binary.Location);
                return "bool";
            }
            if (IsNullableStringType(left) || IsNullableStringType(right))
            {
                if ((IsStringType(left) || left == "null") && (IsStringType(right) || right == "null")) return "bool";
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Nullable string equality requires string or null operands.", binary.Location);
                return "bool";
            }
            if (IsNullableClassType(left) || IsNullableClassType(right) || left == "null" || right == "null")
            {
                var referenceType = IsNullableClassType(left) ? NullableUnderlyingType(left) : IsNullableClassType(right) ? NullableUnderlyingType(right) :
                    IsClassType(left) ? left : IsClassType(right) ? right : null;
                if (referenceType is not null &&
                    (left == "null" && IsClassReferenceType(right) || right == "null" && IsClassReferenceType(left) ||
                     left != "null" && right != "null" && IsClassType(NullableUnderlyingType(left)) && IsClassType(NullableUnderlyingType(right)) &&
                     (NullableUnderlyingType(left) == NullableUnderlyingType(right) || IsBaseClassOf(NullableUnderlyingType(left), NullableUnderlyingType(right)) || IsBaseClassOf(NullableUnderlyingType(right), NullableUnderlyingType(left))))) return "bool";
            }
            if (IsStringType(left) || IsStringType(right))
            {
                if (left == "string" && right == "string") return "bool";
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "String equality requires two non-nullable string operands; use a nullable string for null comparisons.", binary.Location);
                return "bool";
            }
            if (IsNullableArrayType(left) || IsNullableArrayType(right) || left == "null" && IsManagedArrayReferenceType(right) || right == "null" && IsManagedArrayReferenceType(left))
            {
                var nullableArray = IsNullableArrayType(left) ? left : IsNullableArrayType(right) ? right : null;
                if (nullableArray is not null && (left == "null" && IsManagedArrayReferenceType(right) || right == "null" && IsManagedArrayReferenceType(left) ||
                    IsManagedArrayReferenceType(left) && IsManagedArrayReferenceType(right) && NullableUnderlyingType(left) == NullableUnderlyingType(right))) return "bool";
            }
            if (IsManagedArrayReferenceType(left) || IsManagedArrayReferenceType(right))
            {
                var sameArrayType = IsManagedArrayReferenceType(left) && IsManagedArrayReferenceType(right) &&
                    NullableUnderlyingType(left) == NullableUnderlyingType(right);
                var arrayToClass = IsManagedArrayReferenceType(left) && IsClassReferenceType(NullableUnderlyingType(right)) &&
                    CanConvert(left, right, binary.Left);
                var classToArray = IsManagedArrayReferenceType(right) && IsClassReferenceType(NullableUnderlyingType(left)) &&
                    CanConvert(right, left, binary.Right);
                if (!sameArrayType && !arrayToClass && !classToArray)
                    diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Array equality requires the same invariant array type or a compatible base-class reference.", binary.Location);
                return "bool";
            }
            if (IsClassType(left) || IsClassType(right))
            {
                var overloadedEquality = FindUserBinaryOperator(op, left, right, binary.Left, binary.Right, context, binary.Location);
                if (overloadedEquality is not null)
                {
                    _operatorTargets[binary] = overloadedEquality;
                    return "bool";
                }
                if (left != right || !IsClassType(left)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Class equality requires references to the same class type.", binary.Location);
                return "bool";
            }
            if (IsStructType(left) || IsStructType(right))
            {
                var equalityOperator = left == right ? _methods.FirstOrDefault(candidate => candidate.IsStructEqualityOperator && GetQualifiedTypeName(candidate.Owner) == left &&
                    candidate.Declaration.ReturnType == "bool" && candidate.Declaration.Parameters.Count == 2 && candidate.Declaration.Parameters.All(parameter => parameter.TypeName == left)) : null;
                if (equalityOperator is not null) _operatorTargets[binary] = equalityOperator;
                else diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Struct equality requires a valid user-defined '==' operator; aggregate equality is not implicit.", binary.Location);
                return "bool";
            }
            if (TryParseInlineArrayType(left, out _, out _) || TryParseInlineArrayType(right, out _, out _))
            {
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Inline arrays do not have built-in equality.", binary.Location);
                return "bool";
            }
            if (IsPointerType(left) || IsPointerType(right))
            {
                if (left == right || IsPointerType(left) && right == "null" || IsPointerType(right) && left == "null" ||
                    IsPointerType(left) && IsPointerType(right) && (left == "void*" || right == "void*")) return "bool";
                diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Pointer equality operands '{left}' and '{right}' are incompatible.", binary.Location); return "bool";
            }
            if (left != right && (!NumericTypes.Contains(left) || !NumericTypes.Contains(right))) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Equality operands must have compatible types.", binary.Location);
            else if (NumericTypes.Contains(left) && NumericTypes.Contains(right)) _ = PromoteNumeric(left, right, binary.Left, binary.Right, binary.Location);
            return "bool";
        }
        if (op == "+" && (left == "string" || right == "string"))
        {
            if (CanStringify(left) && CanStringify(right))
            {
                RecordStringification(left);
                RecordStringification(right);
                return "string";
            }
            var unsupported = left == "string" ? right : left;
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error,
                $"String concatenation cannot convert '{unsupported}' to text; supported values include strings, enums, primitive bool, char, integer, and floating-point values, plus classes/structs with ToString.", binary.Location);
            return "string";
        }
        if ((op is "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>" or "<" or ">" or "<=" or ">=") &&
            (IsStructType(left) || IsStructType(right) || IsClassType(left) || IsClassType(right)))
        {
            var overloaded = FindUserBinaryOperator(op, left, right, binary.Left, binary.Right, context, binary.Location);
            if (overloaded is not null)
            {
                _operatorTargets[binary] = overloaded;
                return NormalizeEnumType(overloaded.Declaration.ReturnType, overloaded.Owner);
            }
            diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Operands '{left}' and '{right}' do not define a matching public static '{op}' operator.", binary.Location);
            return "?";
        }
        if ((IsNullableValueType(left) || IsNullableValueType(right)) && op is "<" or ">" or "<=" or ">=" or "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>")
        {
            var leftUnderlying = IsNullableValueType(left) ? NullableUnderlyingType(left) : left;
            var rightUnderlying = IsNullableValueType(right) ? NullableUnderlyingType(right) : right;
            if (op is "&" or "|" or "^")
            {
                if (leftUnderlying == "bool" && rightUnderlying == "bool") return "bool?";
                if (!(IntegerTypes.Contains(leftUnderlying) || leftUnderlying == "char") || !(IntegerTypes.Contains(rightUnderlying) || rightUnderlying == "char"))
                { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Lifted operator '{op}' requires integral or bool operands.", binary.Location); return "?"; }
            }
            else if (op is "<<" or ">>")
            {
                if (IsNullableValueType(right) || !(IntegerTypes.Contains(leftUnderlying) || leftUnderlying == "char") || !(IntegerTypes.Contains(rightUnderlying) || rightUnderlying == "char"))
                { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A lifted shift requires a nullable integral operand and a non-nullable integral count.", binary.Location); return "?"; }
                if (!CanConvert(rightUnderlying, "int32", binary.Right)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A shift count must be convertible to int32.", binary.Right.Location);
                return PromoteSmallInteger(leftUnderlying) + "?";
            }
            else if (!NumericTypes.Contains(leftUnderlying) || !NumericTypes.Contains(rightUnderlying))
            { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Lifted operator '{op}' requires numeric operands.", binary.Location); return "?"; }
            if (IsNullableValueType(left) && IsNullableValueType(right) && leftUnderlying != rightUnderlying &&
                (!NumericTypes.Contains(leftUnderlying) || !NumericTypes.Contains(rightUnderlying)))
            { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Lifted operator '{op}' requires compatible nullable operands, not '{left}' and '{right}'.", binary.Location); return "?"; }
            var promoted = NumericTypes.Contains(leftUnderlying) && NumericTypes.Contains(rightUnderlying)
                ? NumericPromotionType(leftUnderlying, rightUnderlying) : leftUnderlying;
            if (op is "<" or ">" or "<=" or ">=") return "bool";
            return promoted + "?";
        }
        if (op is "<" or ">" or "<=" or ">=")
        {
            if (IsPointerType(left) && left == right) return "bool";
            if (left == "char" && right == "char" || IsEnumType(left) && left == right) return "bool";
            _ = PromoteNumeric(left, right, binary.Left, binary.Right, binary.Location); return "bool";
        }
        if (op is "+" or "-")
        {
            if (op == "+" && (left == "string" || right == "string") && CanStringify(left) && CanStringify(right))
            {
                RecordStringification(left);
                RecordStringification(right);
                return "string";
            }
            if (op == "+" && (left == "string" || right == "string"))
            {
                var unsupported = left == "string" ? right : left;
                diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"String concatenation cannot convert '{unsupported}' to text; supported values include strings, enums, primitive bool, char, integer, and floating-point values, plus classes/structs with ToString.", binary.Location);
                return "string";
            }
            if (IsPointerType(left) && (IntegerTypes.Contains(right) || right == "char")) return left;
            if (op == "+" && IsPointerType(right) && (IntegerTypes.Contains(left) || left == "char")) return right;
            if (op == "-" && IsPointerType(left) && left == right) return "int64";
            if (IsPointerType(left) || IsPointerType(right)) { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Pointer arithmetic is not defined for '{left}' and '{right}'.", binary.Location); return "?"; }
            if (IsEnumType(left) && (IntegerTypes.Contains(right) || right == "char")) return left;
            if (op == "-" && IsEnumType(left) && left == right) { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Subtracting enum values is not supported; convert explicitly to the underlying integer type.", binary.Location); return "?"; }
            return PromoteNumeric(left, right, binary.Left, binary.Right, binary.Location);
        }
        if (op is "*" or "/" or "%") return PromoteNumeric(left, right, binary.Left, binary.Right, binary.Location);
        if (op is "&" or "|" or "^")
        {
            if (left == "bool" && right == "bool") return "bool";
            if (IsEnumType(left) && left == right) return left;
            if (!(IntegerTypes.Contains(left) || left == "char") || !(IntegerTypes.Contains(right) || right == "char"))
            { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Operator '{op}' requires integral or bool operands.", binary.Location); return "?"; }
            return PromoteNumeric(left, right, binary.Left, binary.Right, binary.Location);
        }
        if (op is "<<" or ">>")
        {
            if (!(IntegerTypes.Contains(left) || left == "char") || !(IntegerTypes.Contains(right) || right == "char"))
            { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Shift operands must be integers and the left operand must be integral.", binary.Location); return "?"; }
            if (!CanConvert(right, "int32", binary.Right)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "A shift count must be convertible to int32.", binary.Right.Location);
            return PromoteSmallInteger(left);
        }
        diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Operator '{op}' is not supported.", binary.Location); return "?";
    }
    private void RequireSameNumeric(string left, string right, BinaryExpression expression)
    {
        if (left == "char" && right == "char") return;
        if (FloatingTypes.Contains(left) || FloatingTypes.Contains(right))
        {
            if (left != right || !FloatingTypes.Contains(left)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Floating operands must have the same type; found '{left}' and '{right}'.", expression.Location);
            return;
        }
        if (!IntegerTypes.Contains(left) || !IntegerTypes.Contains(right)) diagnostics.Add("LMC3010", DiagnosticSeverity.Error, "Operator requires numeric operands.", expression.Location);
        else if (left != right)
        {
            if (IsUnsigned(left) != IsUnsigned(right)) diagnostics.Add("LS0301", DiagnosticSeverity.Error, "Operands of mixed signedness require an explicit cast.", expression.Location);
            else diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Operands must have the same integer type; found '{left}' and '{right}'.", expression.Location);
        }
    }
    private bool CanStringify(string type) => type is "string" or "string?" or "null" or "bool" or "char" || IntegerTypes.Contains(type) || FloatingTypes.Contains(type) || IsEnumType(type) || CanUseDefaultClassToString(type) || CanUseDefaultStructToString(type) || GetCustomStructToString(type) is not null;
    private void RecordStringification(string type)
    {
        if (type is "char" || IntegerTypes.Contains(type) || FloatingTypes.Contains(type))
            _stringifiedPrimitiveTypes.Add(type);
        if (IsEnumType(type) && StringLiteralCodec.TryCreate(type, out var enumName, out _))
            _stringLiteralPool.TryAdd(enumName.Text, enumName);
        if (_structTypes.ContainsKey(type) && StringLiteralCodec.TryCreate(type, out var structName, out _))
            _stringLiteralPool.TryAdd(structName.Text, structName);
    }
    private bool CanUseDefaultClassToString(string type)
    {
        var classType = IsNullableClassType(type) ? NullableUnderlyingType(type) : type;
        return _classTypes.ContainsKey(classType) || _interfaceTypes.ContainsKey(classType);
    }
    private bool CanUseDefaultStructToString(string type) => _structTypes.ContainsKey(type) && GetCustomStructToString(type) is null;
    private bool HasCustomStructToString(string type) => GetCustomStructToString(type) is not null;
    private MethodSymbol? GetCustomStructToString(string type) => _methods.FirstOrDefault(candidate => candidate.IsStructInstanceMethod &&
        GetQualifiedTypeName(candidate.Owner) == type && candidate.Declaration.Name == "ToString" &&
        candidate.Declaration.IsPublic && !candidate.Declaration.IsMutating && candidate.Declaration.ReturnType == "string" &&
        candidate.Declaration.Parameters.Count == 0);

    private string PromoteNumeric(string left, string right, Expression leftExpression, Expression rightExpression, SourceLocation at)
    {
        if (!NumericTypes.Contains(left) || !NumericTypes.Contains(right))
        { diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Numeric operands required; found '{left}' and '{right}'.", at); return "int32"; }
        if (left == "double" || right == "double") return "double";
        if (left == "float" || right == "float") return "float";
        if ((left == "uint32" && right is "uint8" or "uint16") || (right == "uint32" && left is "uint8" or "uint16")) return "uint32";
        left = PromoteSmallInteger(left); right = PromoteSmallInteger(right);
        if (left == "uint64" || right == "uint64")
        {
            var other = left == "uint64" ? right : left; var otherExpression = left == "uint64" ? rightExpression : leftExpression;
            if (other == "uint64" || IsUnsigned(other) || IsNonNegativeIntegerConstant(otherExpression)) return "uint64";
            diagnostics.Add("LS0301", DiagnosticSeverity.Error, "A signed operand cannot be combined with uint64 unless it is a non-negative constant.", at); return "uint64";
        }
        if (left == "int64" || right == "int64") return "int64";
        if (left == "uint32" || right == "uint32")
        {
            var other = left == "uint32" ? right : left; var otherExpression = left == "uint32" ? rightExpression : leftExpression;
            if (other == "uint32" || IsUnsigned(other)) return "uint32";
            if (other == "int32" && IsNonNegativeIntegerConstant(otherExpression)) return "uint32";
            return "int64";
        }
        return "int32";
    }
    internal static string PromoteSmallInteger(string type) => type is "int8" or "uint8" or "int16" or "uint16" or "char" ? "int32" : type;
    internal static string NumericPromotionType(string left, string right)
    {
        if (left == "double" || right == "double") return "double";
        if (left == "float" || right == "float") return "float";
        if ((left == "uint32" && right is "uint8" or "uint16") || (right == "uint32" && left is "uint8" or "uint16")) return "uint32";
        left = PromoteSmallInteger(left); right = PromoteSmallInteger(right);
        if (left == "uint64" || right == "uint64") return "uint64";
        if (left == "int64" || right == "int64") return "int64";
        if (left == "uint32" || right == "uint32") return left == "uint32" && right == "uint32" || left == "uint32" && IsUnsigned(right) || right == "uint32" && IsUnsigned(left) ? "uint32" : "int64";
        return "int32";
    }
    private static bool IsNonNegativeIntegerConstant(Expression expression) => expression is LiteralExpression l && (ParseInteger(l.Value).Value is { } value && value >= BigInteger.Zero || TryParseCharacter(l.Value, out _));
    private bool CanConvert(string from, string to, Expression? expression)
    {
        if (from == to || from == "?" || to == "?") return true;
        if (TryParseManagedArrayType(from, out _, out _) &&
            (to is "System.Array" or "System.Object" or "object" || IsNullableClassType(to) && NullableUnderlyingType(to) is "System.Array" or "System.Object"))
            return !IsNullableArrayType(from) || IsNullableClassType(to);
        if (TryParseManagedArrayType(to, out _, out _) && from == "null") return IsNullableArrayType(to);
        if (IsStringType(from) &&
            (to is "System.Object" or "object" || IsNullableClassType(to) && NullableUnderlyingType(to) == "System.Object"))
            return !IsNullableStringType(from) || IsNullableClassType(to);
        if (IsStructType(from) && IsInterfaceType(NullableUnderlyingType(to)) &&
            _interfaceConversions.Contains((from, NullableUnderlyingType(to)))) return true;
        if (IsNullableValueType(from) && IsNullableClassType(to) && IsInterfaceType(NullableUnderlyingType(to)) &&
            IsStructType(NullableUnderlyingType(from)) &&
            _interfaceConversions.Contains((NullableUnderlyingType(from), NullableUnderlyingType(to)))) return true;
        if ((to is "System.Object" or "object") || IsNullableClassType(to) && NullableUnderlyingType(to) == "System.Object")
        {
            var boxedType = IsNullableValueType(from) ? NullableUnderlyingType(from) : from;
            if (IsBoxableValueType(boxedType)) return true;
        }
        if (from == "null" && IsNullableType(to)) return true;
        if (IsNullableArrayType(to) && from == NullableUnderlyingType(to)) return true;
        if (IsNullableDelegateType(to) && from == NullableUnderlyingType(to)) return true;
        var sourceDelegateType = NullableUnderlyingType(from);
        var targetReferenceType = NullableUnderlyingType(to);
        if (_delegateTypes.ContainsKey(sourceDelegateType) && targetReferenceType is ("System.Delegate" or "System.Object") &&
            (!IsNullableDelegateType(from) || IsNullableClassType(to))) return true;
        if (from == "string" && IsNullableStringType(to)) return true;
        if (IsClassReferenceType(from) && to == from + "?") return true;
        var fromClass = NullableUnderlyingType(from); var toClass = NullableUnderlyingType(to);
        if (IsClassReferenceType(fromClass) && IsClassReferenceType(toClass) && IsReferenceAssignable(toClass, fromClass) &&
            (!IsNullableClassType(from) || IsNullableClassType(to))) return true;
        if (IsNullableValueType(to) && CanConvert(from, NullableUnderlyingType(to), expression)) return true;
        if (IsNullableValueType(from) && IsNullableValueType(to) && CanConvert(NullableUnderlyingType(from), NullableUnderlyingType(to), expression)) return true;
        if (from == "null" && IsPointerType(to)) return true;
        if (IsPointerType(from) && IsPointerType(to)) return to == "void*";
        if (_enumUnderlyingTypes.TryGetValue(from, out var enumUnderlying)) return CanConvert(enumUnderlying, to, expression);
        if (IsEnumType(to)) return false;
        if (from == "char" && to is "uint16" or "int32" or "uint32" or "int64" or "uint64" or "float" or "double") return true;
        if (from == "char" && to == "int16") return false;
        if (from == "char" && expression is LiteralExpression character && TryParseCharacter(character.Value, out var codeUnit)) return Fits(codeUnit, to);
        if (from == "float" && to == "double") return true;
        if ((from is "int32" or "uint32") && to == "double") return true;
        if ((from is "int8" or "uint8" or "int16" or "uint16") && (to is "float" or "double")) return true;
        if (IntegerTypes.Contains(from) && IntegerTypes.Contains(to) && IsImplicitIntegerConversion(from, to)) return true;
        if (IntegerTypes.Contains(from) && IntegerTypes.Contains(to) && expression is not null && TryEvaluateConstant(expression, out var integerConstant, out _) && integerConstant.Integer is { } constantInteger)
        {
            return Fits(constantInteger, to);
        }
        if (IntegerTypes.Contains(from) && to == "char" && expression is LiteralExpression charTargetLiteral)
        {
            var parsed = ParseInteger(charTargetLiteral.Value); return parsed.Value.HasValue && Fits(parsed.Value.Value, "char");
        }
        if (from == "char" && (to is "int8" or "uint8" or "int16" or "int32" or "uint32" or "int64" or "uint64" or "int" or "uint")) return true;
        if (FindArraySpanConversionMethod(from, to) is { } arraySpanConversion)
        {
            _userConversionOperators[(from, to)] = arraySpanConversion;
            return true;
        }
        var userConversion = FindUserConversionOperator(from, to, allowExplicit: false);
        if (userConversion is not null)
        {
            _userConversionOperators[(from, to)] = userConversion;
            return true;
        }
        return false;
    }

    private bool CanConvertInContext(string from, string to, Expression? expression, MethodContext context)
    {
        if (CanConvert(from, to, expression)) return true;
        return IsNullableType(from) && IsDefinitelyNotNull(expression, from, context) && CanConvert(NullableUnderlyingType(from), to, expression);
    }

    private void AddReferenceConversionDiagnostic(string from, string to, Expression? expression, MethodContext context, SourceLocation location)
    {
        if (from == "null" && (IsClassReferenceType(to) || to == "string" || TryParseManagedArrayType(to, out _, out _)))
        {
            diagnostics.Add("LS1301", DiagnosticSeverity.Error, $"Null cannot be assigned to non-nullable reference type '{to}'.", location);
            return;
        }
        if (IsNullableType(from) && CanConvert(NullableUnderlyingType(from), to, expression))
        {
            diagnostics.Add("LS1302", DiagnosticSeverity.Error, $"Nullable value or reference '{from}' may be null and cannot be converted to '{to}' here.", location);
            return;
        }
        var boxedType = IsNullableValueType(from) ? NullableUnderlyingType(from) : from;
        if ((to is "System.Object" or "object") && RequiresBoxingForObject(from) && !IsBoxableValueType(boxedType))
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, $"Converting value type '{from}' to 'System.Object' requires boxing, which is not yet supported by the current backend.", location);
            return;
        }
        if (IsManagedArrayCovarianceAttempt(from, to))
        {
            diagnostics.Add("LS1502", DiagnosticSeverity.Error, $"Managed arrays are invariant; '{from}' cannot be converted to '{to}'.", location);
            return;
        }
        diagnostics.Add("LMC3010", DiagnosticSeverity.Error, $"Cannot convert '{from}' to '{to}'.", location);
    }

    private bool RequiresBoxingForObject(string type) =>
        IsStructType(type) || IsEnumType(type) || type is "bool" or "char" || IntegerTypes.Contains(type) || FloatingTypes.Contains(type) ||
        IsNullableValueType(type);

    private bool IsManagedArrayCovarianceAttempt(string from, string to)
    {
        if (!TryParseManagedArrayType(from, out var fromElement, out var fromRank) ||
            !TryParseManagedArrayType(to, out var toElement, out var toRank) || fromRank != toRank)
            return false;
        return IsManagedArrayElementCovarianceAttempt(fromElement, toElement);
    }

    private bool IsManagedArrayElementCovarianceAttempt(string from, string to)
    {
        if (from == to) return false;
        if (TryParseManagedArrayType(from, out var fromElement, out var fromRank) &&
            TryParseManagedArrayType(to, out var toElement, out var toRank) && fromRank == toRank)
            return IsManagedArrayElementCovarianceAttempt(fromElement, toElement);
        var fromNullableClass = IsNullableClassType(from);
        var fromClass = fromNullableClass ? NullableUnderlyingType(from) : from;
        var toNullableClass = IsNullableClassType(to);
        var toClass = toNullableClass ? NullableUnderlyingType(to) : to;
        return IsClassReferenceType(fromClass) && IsClassReferenceType(toClass) && IsReferenceAssignable(toClass, fromClass) &&
            (!fromNullableClass || toNullableClass);
    }

    private bool IsDefinitelyNotNull(Expression? expression, string expressionType, MethodContext context)
    {
        if (!IsNullableType(expressionType)) return IsClassReferenceType(expressionType);
        if (expression is NullAssertionExpression) return true;
        if (expression is not null && TryGetNullStateKey(expression, context, out var key))
            return context.GetNullState(key) == NullState.NonNull;
        return false;
    }

    private bool TryGetNullStateKey(Expression expression, MethodContext context, out string key)
    {
        switch (expression)
        {
            case NameExpression name when context.Lookup(name.Name) is not null:
                key = name.Name; return true;
            case NameExpression name when _boundUnqualifiedClassFields.ContainsKey(name) || _boundUnqualifiedStructFields.ContainsKey(name):
                key = "path:this." + name.Name; return true;
            case ThisExpression:
                key = "path:this"; return true;
            case MemberAccessExpression member when TryGetNullStateKey(member.Receiver, context, out var receiverKey):
                key = (receiverKey.StartsWith("path:", StringComparison.Ordinal) ? receiverKey : "path:" + receiverKey) + "." + member.Member;
                return true;
            default:
                key = ""; return false;
        }
    }

    private NullState GetNullState(Expression expression, string expressionType, MethodContext context)
    {
        if (expression is LiteralExpression { Value: "null" }) return NullState.Null;
        if (!IsNullableType(expressionType)) return NullState.NonNull;
        return expression switch
        {
            NullAssertionExpression => NullState.NonNull,
            _ when TryGetNullStateKey(expression, context, out var key) => context.GetNullState(key),
            BinaryExpression { Operator: "??" } => NullState.MaybeNull,
            _ => NullState.MaybeNull
        };
    }

    private void ValidateTypePatternVariable(BinaryExpression expression, string op, MethodContext context)
    {
        if (expression.PatternVariableName is not { } name) return;
        if (op != "is")
        {
            diagnostics.Add("LMC3000", DiagnosticSeverity.Error, "A type pattern variable is supported only with the 'is' operator.", expression.Location);
            return;
        }
        if (context.Exists(name))
            diagnostics.Add("LS0201", DiagnosticSeverity.Error, $"Pattern variable '{name}' conflicts with an existing name in the current context.", expression.Location);
    }

    private IReadOnlyList<BlockStatement> DeclarePositiveTypePatternVariables(Expression condition, MethodContext context, bool allowCapture = true)
    {
        if (condition is BinaryExpression { Operator: "&&" } conjunction)
        {
            return DeclarePositiveTypePatternVariables(conjunction.Left, context, allowCapture)
                .Concat(DeclarePositiveTypePatternVariables(conjunction.Right, context, allowCapture)).ToArray();
        }
        if (condition is not BinaryExpression { Operator: "is", PatternVariableName: { } name } typePattern ||
            !_typeOperatorTargets.TryGetValue(typePattern, out var targetType) || context.Exists(name)) return [];
        var captureScope = allowCapture ? typePattern.PatternCaptureScope : null;
        context.Declare(name, targetType, mutable: false, typePattern.Location, blockOwner: captureScope);
        if (captureScope is null) return [];
        context.EnterCaptureBlock(captureScope, isMethodScope: false);
        return [captureScope];
    }

    private static BinaryExpression[] GetPositiveTypePatternNodes(Expression condition)
    {
        if (condition is BinaryExpression { Operator: "&&" } conjunction)
            return GetPositiveTypePatternNodes(conjunction.Left).Concat(GetPositiveTypePatternNodes(conjunction.Right)).ToArray();
        return condition is BinaryExpression { Operator: "is", PatternVariableName: not null } pattern ? [pattern] : [];
    }

    private ClosureEnvironmentBinding? GetPatternCaptureEnvironment(Expression condition, string? name)
    {
        if (name is null) return null;
        foreach (var pattern in GetPositiveTypePatternNodes(condition))
            if (pattern.PatternVariableName == name && _blockClosureEnvironments.TryGetValue(pattern.PatternCaptureScope, out var environment) &&
                environment.CapturedFieldIndices.ContainsKey(name))
                return environment;
        return null;
    }

    private static void ExitCaptureScopes(MethodContext context, IReadOnlyList<BlockStatement> scopes)
    {
        for (var index = scopes.Count - 1; index >= 0; index--) context.ExitCaptureBlock(scopes[index]);
    }

    private void RefineNullCondition(Expression condition, MethodContext context, bool whenTrue)
    {
        if (condition is UnaryExpression { Operator: "!" } negation)
        {
            RefineNullCondition(negation.Operand, context, !whenTrue);
            return;
        }
        if (condition is BinaryExpression { Operator: "&&" } conjunction && whenTrue)
        {
            RefineNullCondition(conjunction.Left, context, true);
            RefineNullCondition(conjunction.Right, context, true);
            return;
        }
        if (condition is BinaryExpression { Operator: "||" } disjunction && !whenTrue)
        {
            RefineNullCondition(disjunction.Left, context, false);
            RefineNullCondition(disjunction.Right, context, false);
            return;
        }
        if (condition is BinaryExpression typeTest && (typeTest.Operator is "is" or "is not") &&
            _typeOperatorTargets.TryGetValue(typeTest, out var typeTestTarget) && typeTestTarget != "null" &&
            TryGetNullStateKey(typeTest.Left, context, out var testedKey))
        {
            var testedType = GetNullFlowExpressionType(typeTest.Left, context);
            var matchedBranch = typeTest.Operator == "is" ? whenTrue : !whenTrue;
            if (matchedBranch && IsNullableClassType(testedType)) context.SetNullState(testedKey, NullState.NonNull);
            return;
        }
        if (condition is not BinaryExpression { Operator: "==" or "!=" } comparison) return;
        Expression? candidate = comparison.Left is not LiteralExpression { Value: "null" } && comparison.Right is LiteralExpression { Value: "null" } ? comparison.Left :
            comparison.Right is not LiteralExpression { Value: "null" } && comparison.Left is LiteralExpression { Value: "null" } ? comparison.Right : null;
        if (candidate is null || !TryGetNullStateKey(candidate, context, out var key)) return;
        var candidateType = GetNullFlowExpressionType(candidate, context);
        if (!IsNullableType(candidateType)) return;
        var notEqual = comparison.Operator == "!=";
        context.SetNullState(key, whenTrue == notEqual ? NullState.NonNull : NullState.Null);
    }

    private string GetNullFlowExpressionType(Expression expression, MethodContext context) => expression switch
    {
        NameExpression name when context.Lookup(name.Name) is { } local => local.Type,
        NameExpression name when _boundUnqualifiedClassFields.TryGetValue(name, out var field) => field.TypeName,
        NameExpression name when _boundUnqualifiedStructFields.TryGetValue(name, out var field) => field.TypeName,
        MemberAccessExpression access when _boundClassFields.TryGetValue(access, out var field) => field.TypeName,
        MemberAccessExpression access when _boundStructFields.TryGetValue(access, out var field) => field.TypeName,
        MemberAccessExpression access when _boundStaticFields.TryGetValue(access, out var field) => field.TypeName,
        _ => "?"
    };

    private static IReadOnlyDictionary<string, NullState> MergeNullStates(
        IReadOnlyDictionary<string, NullState> first,
        IReadOnlyDictionary<string, NullState> second)
    {
        var merged = new Dictionary<string, NullState>(StringComparer.Ordinal);
        foreach (var name in first.Keys.Union(second.Keys, StringComparer.Ordinal))
        {
            var left = first.GetValueOrDefault(name, NullState.MaybeNull);
            var right = second.GetValueOrDefault(name, NullState.MaybeNull);
            merged[name] = left == right ? left : NullState.MaybeNull;
        }
        return merged;
    }
    private static NullState MergeNullState(NullState first, NullState second) => first == second ? first : NullState.MaybeNull;
    private static bool IsUnsigned(string type) => type.StartsWith("u", StringComparison.Ordinal) || type == "char";
    private bool IsImplicitIntegerConversion(string from, string to)
    {
        var sourceWidth = Width(from); var targetWidth = Width(to);
        if (sourceWidth == targetWidth) return IsUnsigned(from) == IsUnsigned(to);
        return sourceWidth < targetWidth && (IsUnsigned(from) == IsUnsigned(to) || (IsUnsigned(from) && !IsUnsigned(to)));
    }
    private int Width(string type) => type switch { "int8" or "uint8" => 8, "int16" or "uint16" or "char" => 16, "int32" or "uint32" => 32, "int64" or "uint64" => 64, "int" or "uint" => _nativePointerSize * 8, _ => 0 };
    private bool Fits(BigInteger value, string type)
    {
        var bits = Width(type);
        if (bits == 0) return false; var min = IsUnsigned(type) ? BigInteger.Zero : -(BigInteger.One << (bits - 1)); var max = IsUnsigned(type) ? (BigInteger.One << bits) - 1 : (BigInteger.One << (bits - 1)) - 1; return value >= min && value <= max;
    }
    private static bool FitsLiteral(BigInteger value, string type)
    {
        var bits = type switch { "int8" or "uint8" => 8, "int16" or "uint16" or "char" => 16, "int" or "uint" => IntPtr.Size * 8, "int32" or "uint32" => 32, "int64" or "uint64" => 64, _ => 0 };
        if (bits == 0) return false; var min = IsUnsigned(type) ? BigInteger.Zero : -(BigInteger.One << (bits - 1)); var max = IsUnsigned(type) ? (BigInteger.One << bits) - 1 : (BigInteger.One << (bits - 1)) - 1; return value >= min && value <= max;
    }
    internal static (string Type, BigInteger? Value) ParseInteger(string raw)
    {
        var suffixStart = raw.Length;
        while (suffixStart > 0 && raw[suffixStart - 1] is 'u' or 'U' or 'l' or 'L') suffixStart--;
        var suffix = raw[suffixStart..].ToLowerInvariant();
        if (suffix is not ("" or "u" or "l" or "ul")) return ("?", null);

        var digits = raw[..suffixStart]; var radix = 10;
        if (digits.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) { radix = 16; digits = digits[2..]; }
        else if (digits.StartsWith("0b", StringComparison.OrdinalIgnoreCase)) { radix = 2; digits = digits[2..]; }
        if (digits.Length == 0) return ("?", null);

        var value = BigInteger.Zero;
        for (var i = 0; i < digits.Length; i++)
        {
            var c = digits[i];
            if (c == '_')
            {
                if (i == 0 || i == digits.Length - 1) return ("?", null);
                var before = DigitValue(digits[i - 1]); var after = DigitValue(digits[i + 1]);
                if (before < 0 || before >= radix || after < 0 || after >= radix) return ("?", null);
                continue;
            }
            var digit = DigitValue(c);
            if (digit < 0 || digit >= radix) return ("?", null);
            value = value * radix + digit;
        }

        var type = suffix switch
        {
            "u" => FitsLiteral(value, "uint32") ? "uint32" : FitsLiteral(value, "uint64") ? "uint64" : "?",
            "l" => FitsLiteral(value, "int64") ? "int64" : "?",
            "ul" => FitsLiteral(value, "uint64") ? "uint64" : "?",
            _ => FitsLiteral(value, "int32") ? "int32" : FitsLiteral(value, "uint32") ? "uint32" : FitsLiteral(value, "int64") ? "int64" : FitsLiteral(value, "uint64") ? "uint64" : "?"
        };
        return type == "?" ? ("?", null) : (type, value);
    }
    private static int DigitValue(char c) => char.ToLowerInvariant(c) switch { >= '0' and <= '9' => c - '0', >= 'a' and <= 'f' => char.ToLowerInvariant(c) - 'a' + 10, _ => -1 };
    internal static bool IsFloatingLiteral(string raw)
    {
        if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || raw.StartsWith("0b", StringComparison.OrdinalIgnoreCase)) return false;
        return raw.Contains('.') || raw.Contains('e') || raw.Contains('E') || raw.EndsWith('f') || raw.EndsWith('F') || raw.EndsWith('d') || raw.EndsWith('D');
    }
    internal static (string Type, double Value) ParseFloating(string raw)
    {
        var isFloat = raw.EndsWith('f') || raw.EndsWith('F'); var hasSuffix = isFloat || raw.EndsWith('d') || raw.EndsWith('D');
        var digits = hasSuffix ? raw[..^1] : raw;
        for (var i = 0; i < digits.Length; i++)
            if (digits[i] == '_' && (i == 0 || i == digits.Length - 1 || !char.IsAsciiDigit(digits[i - 1]) || !char.IsAsciiDigit(digits[i + 1]))) return ("?", 0);
        digits = digits.Replace("_", "", StringComparison.Ordinal);
        if (isFloat) return float.TryParse(digits, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f) && float.IsFinite(f) ? ("float", f) : ("?", 0);
        return double.TryParse(digits, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d) && double.IsFinite(d) ? ("double", d) : ("?", 0);
    }
    internal static bool TryParseCharacter(string raw, out ushort value)
    {
        value = 0; if (raw.Length < 3 || raw[0] != '\'' || raw[^1] != '\'') return false;
        var body = raw[1..^1]; if (body.Length == 1) { value = body[0]; return true; }
        if (!body.StartsWith('\\')) return false;
        if (body.Length == 2) { value = body[1] switch { '0' => (ushort)0, 'a' => '\a', 'b' => '\b', 'f' => '\f', 'n' => '\n', 'r' => '\r', 't' => '\t', 'v' => '\v', '\\' => '\\', '\'' => '\'', '"' => '"', _ => ushort.MaxValue }; return value != ushort.MaxValue; }
        var kind = body[1]; var text = body[2..];
        if (kind == 'x') { if (text.Length is < 1 or > 4) return false; return ushort.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out value); }
        if (kind == 'u') { if (text.Length != 4) return false; return ushort.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out value); }
        if (kind == 'U' && text.Length == 8 && uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var codepoint) && codepoint <= ushort.MaxValue) { value = (ushort)codepoint; return true; }
        return false;
    }
    private sealed class MethodContext(MethodSymbol method)
    {
        private readonly List<Dictionary<string, Local>> _scopes = [new(StringComparer.Ordinal)];
        private readonly List<HashSet<string>> _nullStateScopes = [new(StringComparer.Ordinal)];
        private readonly Dictionary<string, NullState> _nullStates = new(StringComparer.Ordinal);
        private readonly List<string?> _loopLabels = [];
        private readonly List<LoopFlow> _loopFlows = [];
        private readonly List<ForStatement> _activeForStatements = [];
        private readonly List<ForInStatement> _activeForInStatements = [];
        private readonly List<BlockStatement?> _captureBlocks = [];
        private int _nextScopeAllocationOrder;
        public MethodSymbol Method { get; } = method;
        public int LoopDepth => _loopLabels.Count;
        public int ScopeDepth => _scopes.Count;
        public ForStatement? ForInitializerOwner { get; set; }
        public int CaptureBlockDepth => _captureBlocks.Count;
        public int NextScopeAllocationOrder() => ++_nextScopeAllocationOrder;
        public BlockStatement? CurrentCaptureBlock => _captureBlocks.Count == 0 ? null : _captureBlocks[^1];
        public void EnterCaptureBlock(BlockStatement statement, bool isMethodScope) => _captureBlocks.Add(isMethodScope ? null : statement);
        public void ExitCaptureBlock(BlockStatement statement)
        {
            if (_captureBlocks.Count > 0 && (_captureBlocks[^1] is null || ReferenceEquals(_captureBlocks[^1], statement)))
                _captureBlocks.RemoveAt(_captureBlocks.Count - 1);
        }
        public bool IsCaptureBlockActive(BlockStatement statement) => _captureBlocks.Contains(statement, ReferenceEqualityComparer.Instance);
        public void EnterForStatement(ForStatement statement) => _activeForStatements.Add(statement);
        public void ExitForStatement(ForStatement statement)
        {
            if (_activeForStatements.Count > 0 && ReferenceEquals(_activeForStatements[^1], statement))
                _activeForStatements.RemoveAt(_activeForStatements.Count - 1);
        }
        public bool IsForStatementActive(ForStatement statement) => _activeForStatements.Contains(statement, ReferenceEqualityComparer.Instance);
        public void EnterForInStatement(ForInStatement statement) => _activeForInStatements.Add(statement);
        public void ExitForInStatement(ForInStatement statement)
        {
            if (_activeForInStatements.Count > 0 && ReferenceEquals(_activeForInStatements[^1], statement))
                _activeForInStatements.RemoveAt(_activeForInStatements.Count - 1);
        }
        public bool IsForInStatementActive(ForInStatement statement) => _activeForInStatements.Contains(statement, ReferenceEqualityComparer.Instance);
        public int SwitchDepth { get; set; }
        public bool HasLoopLabel(string label) => _loopLabels.Contains(label, StringComparer.Ordinal);
        public bool TransferLeavesFinally(BreakStatement transfer, int entryLoopDepth, int entrySwitchDepth)
        {
            if (transfer.Label is { } label)
            {
                var target = _loopLabels.FindLastIndex(candidate => candidate == label);
                return target < 0 || target + 1 <= entryLoopDepth;
            }
            if (transfer.IsContinue) return _loopLabels.Count <= entryLoopDepth;
            if (SwitchDepth > entrySwitchDepth) return false;
            return _loopLabels.Count <= entryLoopDepth;
        }
        public void EnterLoop(string? label) { _loopLabels.Add(label); _loopFlows.Add(new(label, SwitchDepth)); }
        public LoopFlow? ExitLoop()
        {
            if (_loopLabels.Count > 0) _loopLabels.RemoveAt(_loopLabels.Count - 1);
            if (_loopFlows.Count == 0) return null;
            var completed = _loopFlows[^1]; _loopFlows.RemoveAt(_loopFlows.Count - 1); return completed;
        }
        public (int BreakCount, int ContinueCount)[] CaptureLoopTransferCounts() =>
            _loopFlows.Select(flow => (flow.BreakStates.Count, flow.ContinueStates.Count)).ToArray();
        public void DiscardLoopTransfersSince(IReadOnlyList<(int BreakCount, int ContinueCount)> counts)
        {
            var flowCount = Math.Min(_loopFlows.Count, counts.Count);
            for (var index = 0; index < flowCount; index++)
            {
                var flow = _loopFlows[index];
                var (breakCount, continueCount) = counts[index];
                if (flow.BreakStates.Count > breakCount)
                    flow.BreakStates.RemoveRange(breakCount, flow.BreakStates.Count - breakCount);
                if (flow.ContinueStates.Count > continueCount)
                    flow.ContinueStates.RemoveRange(continueCount, flow.ContinueStates.Count - continueCount);
            }
        }
        public void RecordLoopBreak(string? label)
        {
            for (var index = _loopFlows.Count - 1; index >= 0; index--)
            {
                var loop = _loopFlows[index];
                if (label is null)
                {
                    if (SwitchDepth > loop.SwitchDepth) return;
                    loop.BreakStates.Add(CaptureNullStates()); return;
                }
                if (loop.Label == label) { loop.BreakStates.Add(CaptureNullStates()); return; }
            }
        }
        public void RecordLoopContinue(string? label)
        {
            for (var index = _loopFlows.Count - 1; index >= 0; index--)
            {
                var loop = _loopFlows[index];
                if (label is null)
                {
                    loop.ContinueStates.Add(CaptureNullStates()); return;
                }
                if (loop.Label == label) { loop.ContinueStates.Add(CaptureNullStates()); return; }
            }
        }
        public MethodContext CloneForInference()
        {
            var clone = new MethodContext(Method);
            clone._scopes.Clear();
            foreach (var scope in _scopes)
                clone._scopes.Add(new Dictionary<string, Local>(scope, StringComparer.Ordinal));
            clone._nullStateScopes.Clear();
            foreach (var scope in _nullStateScopes)
                clone._nullStateScopes.Add(new HashSet<string>(scope, StringComparer.Ordinal));
            foreach (var (name, state) in _nullStates) clone._nullStates.Add(name, state);
            clone.ForInitializerOwner = ForInitializerOwner;
            clone.SwitchDepth = SwitchDepth;
            clone._nextScopeAllocationOrder = _nextScopeAllocationOrder;
            return clone;
        }
        public void Push() { _scopes.Add(new(StringComparer.Ordinal)); _nullStateScopes.Add(new(StringComparer.Ordinal)); }
        public void Pop()
        {
            if (_scopes.Count <= 1) return;
            foreach (var name in _nullStateScopes[^1]) _nullStates.Remove(name);
            _scopes.RemoveAt(_scopes.Count - 1); _nullStateScopes.RemoveAt(_nullStateScopes.Count - 1);
        }
        public bool Exists(string name) => _scopes.Any(s => s.ContainsKey(name));
        public void Declare(string name, string type, bool mutable, SourceLocation location, bool isConstant = false, int? scopeBoundDepth = null,
            bool isReadOnlyReference = false, ForStatement? forHeaderOwner = null, BlockStatement? blockOwner = null,
            ForInStatement? forInOwner = null, int? scopeBoundOrder = null, VariableStatement? declaration = null)
        {
            if (!_scopes[^1].TryAdd(name, new(type, mutable, isConstant, _scopes.Count, scopeBoundDepth, isReadOnlyReference,
                    forHeaderOwner, blockOwner, forInOwner, scopeBoundOrder, declaration))) return;
            if (!type.EndsWith("?", StringComparison.Ordinal)) return;
            _nullStates[name] = NullState.MaybeNull;
            _nullStateScopes[^1].Add(name);
        }
        public void SetScopeBound(string name, int scopeBoundDepth, int? scopeBoundOrder)
        {
            for (var index = _scopes.Count - 1; index >= 0; index--)
                if (_scopes[index].TryGetValue(name, out var local))
                {
                    _scopes[index][name] = local with { ScopeBoundDepth = scopeBoundDepth, ScopeBoundOrder = scopeBoundOrder };
                    return;
                }
        }
        public void SetReadOnlyReference(string name)
        {
            for (var index = _scopes.Count - 1; index >= 0; index--)
                if (_scopes[index].TryGetValue(name, out var local))
                {
                    _scopes[index][name] = local with { IsReadOnlyReference = true };
                    return;
                }
        }
        public Local? Lookup(string name) { for (var i = _scopes.Count - 1; i >= 0; i--) if (_scopes[i].TryGetValue(name, out var value)) return value; return null; }
        public NullState GetNullState(string name) => _nullStates.GetValueOrDefault(name, NullState.MaybeNull);
        public void SetNullState(string name, NullState state)
        {
            if (!_nullStates.ContainsKey(name) && !name.StartsWith("path:", StringComparison.Ordinal)) _nullStateScopes[^1].Add(name);
            _nullStates[name] = state;
        }
        public void InvalidateMemberNullStates(string? prefix = null)
        {
            foreach (var key in _nullStates.Keys.Where(key => key.StartsWith("path:", StringComparison.Ordinal) && (prefix is null || key.StartsWith(prefix, StringComparison.Ordinal))).ToArray())
                _nullStates[key] = NullState.MaybeNull;
        }
        public IReadOnlyDictionary<string, NullState> CaptureNullStates() => new Dictionary<string, NullState>(_nullStates, StringComparer.Ordinal);
        public IReadOnlyDictionary<string, NullState> FilterVisibleNullStates(IReadOnlyDictionary<string, NullState> states)
        {
            var visible = new Dictionary<string, NullState>(StringComparer.Ordinal);
            foreach (var (name, state) in states)
            {
                var localName = name.StartsWith("path:", StringComparison.Ordinal) ? name[5..].Split('.', 2)[0] : name;
                if (_scopes.Any(scope => scope.ContainsKey(localName))) visible[name] = state;
            }
            return visible;
        }
        public void RestoreNullStates(IReadOnlyDictionary<string, NullState> states)
        {
            foreach (var name in _nullStates.Keys.Where(name => !states.ContainsKey(name)).ToArray()) _nullStates.Remove(name);
            foreach (var (name, state) in states)
            {
                if (!_nullStates.ContainsKey(name) && !name.StartsWith("path:", StringComparison.Ordinal)) _nullStateScopes[^1].Add(name);
                _nullStates[name] = state;
            }
        }
        public sealed record Local(string Type, bool Mutable, bool IsConstant = false, int ScopeDepth = 0, int? ScopeBoundDepth = null,
            bool IsReadOnlyReference = false, ForStatement? ForHeaderOwner = null, BlockStatement? BlockOwner = null,
            ForInStatement? ForInOwner = null, int? ScopeBoundOrder = null, VariableStatement? Declaration = null)
        {
            public bool IsForHeader => ForHeaderOwner is not null;
        }
        public sealed class LoopFlow(string? label, int switchDepth)
        {
            public string? Label { get; } = label;
            public int SwitchDepth { get; } = switchDepth;
            public List<IReadOnlyDictionary<string, NullState>> BreakStates { get; } = [];
            public List<IReadOnlyDictionary<string, NullState>> ContinueStates { get; } = [];
        }
    }
}
