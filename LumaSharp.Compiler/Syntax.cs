namespace LumaSharp.Compiler;

public sealed record CompilationUnit(IReadOnlyList<TypeDeclaration> Types, SourceLocation Location,
    IReadOnlyList<UsingDirective>? Usings = null, string Namespace = "");
public sealed record UsingDirective(string Target, string? Alias, SourceLocation Location);
public sealed record TypeDeclaration(string Namespace, string Kind, string Name, IReadOnlyList<MemberDeclaration> Members, SourceLocation Location, bool HasTypeParameters = false, bool HasBaseList = false, IReadOnlyList<UsingDirective>? Usings = null, string? EnumUnderlyingType = null, IReadOnlyList<string>? BaseTypes = null, bool IsVirtual = false, bool IsStatic = false, string Accessibility = "internal", IReadOnlyList<string>? TypeParameters = null, IReadOnlyList<GenericConstraintDeclaration>? GenericConstraints = null, string? DelegateReturnType = null, IReadOnlyList<ParameterDeclaration>? DelegateParameters = null, string? ContainingTypeName = null);
public sealed record GenericConstraintDeclaration(string TypeParameter, IReadOnlyList<string> Constraints, SourceLocation Location);
public abstract record MemberDeclaration(SourceLocation Location);
public sealed record MethodDeclaration(string ContainingType, string Name, string ReturnType, IReadOnlyList<ParameterDeclaration> Parameters, BlockStatement? Body, bool IsStatic, bool IsPublic, bool IsMutating, bool HasTypeParameters, SourceLocation Location, bool IsUnsafe = false, bool IsExtern = false, bool IsOperator = false, string? Operator = null, bool IsVirtual = false, bool IsOverride = false, string Accessibility = "public", string? PropertyName = null, string? ExplicitInterfaceName = null, IReadOnlyList<string>? TypeParameters = null, IReadOnlyList<GenericConstraintDeclaration>? GenericConstraints = null) : MemberDeclaration(Location)
{
    public bool IsConversionOperator => IsOperator && Operator is "implicit" or "explicit";
}
public sealed record ConstructorDeclaration(string ContainingType, IReadOnlyList<ParameterDeclaration> Parameters, BlockStatement? Body, bool IsStatic, bool IsPublic, SourceLocation Location,
    string? InitializerKind = null, IReadOnlyList<Expression>? InitializerArguments = null, IReadOnlyList<string?>? InitializerArgumentNames = null,
    IReadOnlyList<string?>? InitializerArgumentModes = null, string Accessibility = "public") : MemberDeclaration(Location);
public sealed record FinalizerDeclaration(BlockStatement? Body, SourceLocation At) : MemberDeclaration(At);
public sealed record ConstantFieldDeclaration(string TypeName, string Name, Expression Initializer, bool IsPublic, SourceLocation At, string Accessibility = "private") : MemberDeclaration(At);
public sealed record FieldDeclaration(string TypeName, string Name, Expression? Initializer, bool IsStatic, bool IsPublic, bool IsReadOnly, SourceLocation At, string Accessibility = "private") : MemberDeclaration(At);
public sealed record ComputedPropertyDeclaration(string TypeName, string Name, BlockStatement? GetterBody, BlockStatement? SetterBody,
    bool IsStatic, bool IsVirtual, bool IsOverride, SourceLocation At, string Accessibility = "private", bool UsesBackingField = false,
    string? GetterAccessibility = null, string? SetterAccessibility = null, Expression? BackingInitializer = null, bool SetterIsAuto = false,
    string? ExplicitInterfaceName = null) : MemberDeclaration(At);
public sealed record ComputedIndexerDeclaration(string TypeName, IReadOnlyList<ParameterDeclaration> Parameters,
    BlockStatement? GetterBody, BlockStatement? SetterBody, bool IsVirtual, bool IsOverride, SourceLocation At,
    string Accessibility = "private", string? GetterAccessibility = null, string? SetterAccessibility = null,
    string? ExplicitInterfaceName = null) : MemberDeclaration(At);
public sealed record EnumMemberDeclaration(string Name, Expression? Initializer, SourceLocation At) : MemberDeclaration(At);
public sealed record UnsupportedMember(string Description, SourceLocation At) : MemberDeclaration(At);
public sealed record ParameterDeclaration(string TypeName, string Name, SourceLocation Location, Expression? DefaultValue = null, string PassingMode = "", bool IsReadOnly = false, bool IsVariadic = false, bool IsScoped = false);

public abstract record Statement(SourceLocation Location);
public sealed record BlockStatement(IReadOnlyList<Statement> Statements, SourceLocation At) : Statement(At);
public sealed record ReturnStatement(Expression? Value, SourceLocation At) : Statement(At);
public sealed record VariableStatement(string Name, bool Mutable, string? TypeName, Expression? Initializer, SourceLocation At, bool IsConstant = false) : Statement(At);
public sealed record VariableDeclarationStatement(IReadOnlyList<VariableStatement> Variables, SourceLocation At) : Statement(At);
public sealed record MultiAssignmentStatement(IReadOnlyList<Expression> Targets, IReadOnlyList<Expression> Values, SourceLocation At) : Statement(At);
public sealed record ExpressionStatement(Expression Value, SourceLocation At) : Statement(At);
public sealed record IfStatement(Expression Condition, Statement Then, Statement? Else, SourceLocation At) : Statement(At);
public sealed record WhileStatement(Expression Condition, Statement Body, SourceLocation At, string? Label = null) : Statement(At);
public sealed record DoWhileStatement(Statement Body, Expression Condition, SourceLocation At, string? Label = null) : Statement(At);
public sealed record ForStatement(Statement? Initializer, Expression? Condition, IReadOnlyList<Expression> Iterators, Statement Body, SourceLocation At, string? Label = null) : Statement(At);
public sealed record ForInStatement(string? TypeName, string Name, bool Mutable, Expression Collection, Statement Body, SourceLocation At, string? Label = null, bool IsUnsafeContext = false) : Statement(At);
public sealed record SwitchLabel(Expression? Value, bool IsDefault, SourceLocation At, Expression? Guard = null, bool IsNegated = false, bool IsDiscard = false,
    string? RelationalOperator = null, string? CombinedOperator = null, SwitchLabel? LeftPattern = null, SwitchLabel? RightPattern = null,
    string? TypePatternName = null, string? PatternVariableName = null)
{
    public BlockStatement GuardCaptureScope { get; init; } = new([], At);
    public bool IsUnsupportedPattern { get; init; }
}
public sealed record SwitchSection(IReadOnlyList<SwitchLabel> Labels, IReadOnlyList<Statement> Statements, SourceLocation At)
{
    public BlockStatement CaptureScope { get; init; } = new(Statements, At);
}
public sealed record SwitchStatement(Expression Value, IReadOnlyList<SwitchSection> Sections, SourceLocation At) : Statement(At);
public sealed record CatchClause(string? TypeName, string? VariableName, bool IsCatchAll, BlockStatement Body, SourceLocation At);
public sealed record TryStatement(BlockStatement Body, IReadOnlyList<CatchClause> Catches, BlockStatement? Finally, SourceLocation At) : Statement(At);
public sealed record ThrowStatement(Expression? Value, SourceLocation At) : Statement(At);
public sealed record BreakStatement(string? Label, bool IsContinue, SourceLocation At) : Statement(At);
public sealed record OverflowContextStatement(bool Checked, Statement Body, SourceLocation At) : Statement(At);
public sealed record EmptyStatement(SourceLocation At) : Statement(At);

public abstract record Expression(SourceLocation Location);
public sealed record LiteralExpression(string Value, SourceLocation At) : Expression(At);
public sealed record InlineArrayInitializerExpression(IReadOnlyList<Expression> Elements, SourceLocation At) : Expression(At);
public sealed record AllocationExpression(string? TypeName, IReadOnlyList<Expression> Dimensions, InlineArrayInitializerExpression? Initializer, bool IsScope, SourceLocation At,
    IReadOnlyList<Expression>? ConstructorArguments = null, IReadOnlyList<string?>? ConstructorArgumentNames = null, IReadOnlyList<string?>? ConstructorArgumentModes = null,
    bool IsTypeParameterConstruction = false) : Expression(At);
public sealed record NameExpression(string Name, SourceLocation At) : Expression(At);
public sealed record ThisExpression(SourceLocation At) : Expression(At);
public sealed record BaseExpression(SourceLocation At) : Expression(At);
public sealed record UnaryExpression(string Operator, Expression Operand, bool IsPostfix, SourceLocation At) : Expression(At);
public sealed record CastExpression(string TypeName, Expression Operand, SourceLocation At) : Expression(At);
public sealed record BinaryExpression(Expression Left, string Operator, Expression Right, SourceLocation At, string? PatternVariableName = null) : Expression(At)
{
    public BlockStatement PatternCaptureScope { get; init; } = new([], At);
    public IReadOnlySet<string> CapturedPatternNamesInRight { get; set; } = new HashSet<string>(StringComparer.Ordinal);
}
public sealed record ConditionalExpression(Expression Condition, Expression WhenTrue, Expression WhenFalse, SourceLocation At) : Expression(At);
public sealed record MemberAccessExpression(Expression Receiver, string Member, SourceLocation At, bool IsNullConditional = false, bool IsUnsafeContext = false) : Expression(At);
public sealed record IndexExpression(Expression Receiver, IReadOnlyList<Expression> Indices, SourceLocation At, bool IsUnsafeContext = false, bool IsNullConditional = false) : Expression(At);
public sealed record CallExpression(Expression Target, IReadOnlyList<Expression> Arguments, SourceLocation At, IReadOnlyList<string?>? ArgumentNames = null, IReadOnlyList<string?>? ArgumentModes = null, bool IsUnsafeContext = false, IReadOnlyList<string>? TypeArguments = null) : Expression(At);
public sealed record LambdaParameter(string? TypeName, string Name, SourceLocation Location);
public sealed record LambdaExpression(IReadOnlyList<LambdaParameter> Parameters, Expression? ExpressionBody, BlockStatement? BlockBody, SourceLocation At) : Expression(At);
public sealed record InlineOutVariableExpression(string? TypeName, string Name, SourceLocation At) : Expression(At);
public sealed record OverflowContextExpression(bool Checked, Expression Value, SourceLocation At) : Expression(At);
public sealed record DefaultValueExpression(string? TypeName, SourceLocation At, bool IsTypeParameterDefault = false) : Expression(At);
public sealed record SizeOfExpression(string TypeName, SourceLocation At) : Expression(At);
public sealed record UnsupportedExpression(string Description, SourceLocation At) : Expression(At);
public sealed record NullAssertionExpression(Expression Operand, SourceLocation At) : Expression(At);
