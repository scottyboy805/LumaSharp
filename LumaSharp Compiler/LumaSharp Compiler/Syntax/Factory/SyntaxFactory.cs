
namespace LumaSharp.Compiler.AST
{ 
    public static class Syntax
    {
        // Methods
        public static SyntaxToken Identifier(string identifier)
        {
            return new SyntaxToken(SyntaxTokenKind.Identifier, identifier);
        }

        public static SyntaxToken Token(SyntaxTokenKind kind)
        {
            return new SyntaxToken(kind);
        }

        #region Members
        internal static SeparatedTokenList NamespaceName(string[] identifiers) => new SeparatedTokenList(identifiers.Select(n => new SyntaxToken(SyntaxTokenKind.Identifier, n)).ToArray(), SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier);

        public static ImportSyntax Import(params SyntaxToken[] identifiers) => new ImportSyntax(new(identifiers, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier));
        //public static ImportSyntax ImportAlias(string alias, TypeReferenceSyntax aliasType, params string[] identifiers) => new ImportSyntax(null, alias, aliasType, identifiers);
        
        public static NamespaceSyntax Namespace(params SyntaxToken[] identifiers) => new NamespaceSyntax(new(identifiers, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier));
        public static TypeSyntax Type(SyntaxToken identifier) => new TypeSyntax(identifier, null, null, null, false, null, null);
        public static ContractSyntax Contract(SyntaxToken identifier) => new ContractSyntax( identifier, null, null, null, null, null);
        public static EnumSyntax Enum(SyntaxToken identifier, TypeReferenceSyntax underlyingType = null) => new EnumSyntax(identifier, null, null, underlyingType, null);
        public static FieldSyntax Field(SyntaxToken identifier, TypeReferenceSyntax fieldType, VariableAssignExpressionSyntax fieldAssignment = null, bool byReference = false) => new FieldSyntax(identifier, null, null, fieldType, fieldAssignment);
        public static AccessorBodySyntax AccessorLambda(StatementSyntax statement) => new AccessorBodySyntax(null, AccessorOperation.Read, statement);
        public static AccessorBodySyntax AccessorBody(AccessorOperation op, params StatementSyntax[] statements) => new AccessorBodySyntax(null, op, new BlockSyntax<StatementSyntax>(null, statements));
        public static AccessorBodySyntax AccessorBody(AccessorOperation op, BlockSyntax<StatementSyntax> block) => new AccessorBodySyntax(null, op, block);
        public static AccessorSyntax Accessor(SyntaxToken identifier, TypeReferenceSyntax accessorType) => new AccessorSyntax(identifier, null, null, accessorType, null, false);

        public static MethodSyntax Method(SyntaxToken identifier, TypeReferenceSyntax returnType) => new MethodSyntax(identifier, null, null, new SeparatedSyntaxList<TypeReferenceSyntax>(SyntaxTokenKind.CommaSymbol, new[] { returnType }), null, null, null, null, null);
        public static MethodSyntax Method(SyntaxToken identifier) => new MethodSyntax(identifier, null, null, null, null, null, null, null, null);
        #endregion

        #region CommonUse
        public static ParentTypeReferenceSyntax ParentTypeReference(SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null) => new ParentTypeReferenceSyntax(identifier, genericArguments);
        public static TypeReferenceSyntax TypeReference(PrimitiveType primitive, ArrayParametersSyntax arrayParameters = null) => new TypeReferenceSyntax(primitive, arrayParameters);
        public static TypeReferenceSyntax TypeReference(MemberSyntax member) => new TypeReferenceSyntax(member);
        public static TypeReferenceSyntax TypeReference(SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, null, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(NamespaceName(namespaceName), null, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(ParentTypeReferenceSyntax[] parentTypes, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, parentTypes, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(ParentTypeReferenceSyntax parentType, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, new[] { parentType }, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, ParentTypeReferenceSyntax[] parentTypes, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(NamespaceName(namespaceName), parentTypes, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, ParentTypeReferenceSyntax parentType, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(NamespaceName(namespaceName), new[] { parentType }, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static GenericParameterSyntax GenericParameter(SyntaxToken identifier, params TypeReferenceSyntax[] constrainTypes) => new GenericParameterSyntax(identifier, 0, default, new(SyntaxTokenKind.CommaSymbol, constrainTypes));
        public static GenericParameterListSyntax GenericParameterList(params GenericParameterSyntax[] genericParameters) => new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters));
        public static ParameterSyntax Parameter(TypeReferenceSyntax parameterType, SyntaxToken identifier, bool variableSizedList = false) => new ParameterSyntax(null, parameterType, identifier, null, variableSizedList ? new SyntaxToken(SyntaxTokenKind.EnumerableSymbol) : default);
        public static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters) => new ParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, parameters));
        public static GenericArgumentListSyntax GenericArgumentList(params TypeReferenceSyntax[] genericTypeArguments) => new GenericArgumentListSyntax(new(SyntaxTokenKind.CommaSymbol, genericTypeArguments));
        public static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argumentExpressions) => new ArgumentListSyntax(new(SyntaxTokenKind.CommaSymbol, argumentExpressions));
        public static AttributeReferenceSyntax Attribute(TypeReferenceSyntax attributeType, ArgumentListSyntax arguments = null) => new AttributeReferenceSyntax(attributeType, arguments);
        #endregion

        #region Statements
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, params SyntaxToken[] identifiers) => new VariableDeclarationStatementSyntax(variableType, new(identifiers, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier), null);
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, SyntaxToken identifier, VariableAssignExpressionSyntax assignment) => new VariableDeclarationStatementSyntax(variableType, new(new [] { identifier }, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier), assignment);
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, SyntaxToken[] identifiers, VariableAssignExpressionSyntax assignment) => new VariableDeclarationStatementSyntax(variableType, new(identifiers, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier), assignment);
        public static AssignStatementSyntax Assign(ExpressionSyntax left, VariableAssignExpressionSyntax variableAssign) => new AssignStatementSyntax(new(SyntaxTokenKind.CommaSymbol, new[] { left }), variableAssign);
        public static AssignStatementSyntax Assign(ExpressionSyntax[] left, VariableAssignExpressionSyntax variableAssign) => new AssignStatementSyntax(new(SyntaxTokenKind.CommaSymbol, left), variableAssign);
        public static BreakStatementSyntax Break() => new BreakStatementSyntax();
        public static ConditionStatementSyntax Condition(ExpressionSyntax condition = null) => new ConditionStatementSyntax(Token(SyntaxTokenKind.IfKeyword), condition, null, new EmptyStatementSyntax());
        public static ContinueStatementSyntax Continue() => new ContinueStatementSyntax();
        public static ForStatementSyntax For(VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, params ExpressionSyntax[] increments) => new ForStatementSyntax(variable, condition, new(SyntaxTokenKind.CommaSymbol, increments), new EmptyStatementSyntax());
        public static ReturnStatementSyntax Return(ExpressionSyntax expression = null) => new ReturnStatementSyntax(expression != null ? new(SyntaxTokenKind.CommaSymbol, new[] {expression}) : null);
        public static ReturnStatementSyntax Return(params ExpressionSyntax[] expressions) => new ReturnStatementSyntax(new(SyntaxTokenKind.CommaSymbol, expressions));
        public static MethodInvokeStatementSyntax MethodInvoke(MethodInvokeExpressionSyntax invokeExpression) => new MethodInvokeStatementSyntax(invokeExpression);
        #endregion

        #region Expressions
        public static LiteralExpressionSyntax LiteralNull() => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.NullKeyword));
        public static LiteralExpressionSyntax Literal(int val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()));
        public static LiteralExpressionSyntax Literal(uint val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "U"));
        public static LiteralExpressionSyntax Literal(long val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "L"));
        public static LiteralExpressionSyntax Literal(ulong val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "UL"));
        public static LiteralExpressionSyntax Literal(double val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()));
        public static LiteralExpressionSyntax Literal(string val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, "\"" + val + "\""));
        public static LiteralExpressionSyntax Literal(bool val) => new LiteralExpressionSyntax(new SyntaxToken(val == true ? SyntaxTokenKind.TrueKeyword : SyntaxTokenKind.FalseKeyword, val.ToString().ToLower()));
        
        public static IndexExpressionSyntax ArrayIndex(ExpressionSyntax accessExpression, params ExpressionSyntax[] indexExpressions) => new IndexExpressionSyntax(accessExpression, new(SyntaxTokenKind.CommaSymbol, indexExpressions));
        public static BaseExpressionSyntax Base() => new BaseExpressionSyntax();
        public static ThisExpressionSyntax This() => new ThisExpressionSyntax();
        public static TypeofExpressionSyntax TypeOp(TypeReferenceSyntax typeReference) => new TypeofExpressionSyntax(typeReference);
        public static SizeofExpressionSyntax SizeOp(TypeReferenceSyntax typeReference) => new SizeofExpressionSyntax(typeReference);
        public static VariableAssignExpressionSyntax VariableAssignment(params ExpressionSyntax[] assignExpressions) => new VariableAssignExpressionSyntax(new(SyntaxTokenKind.CommaSymbol, assignExpressions));
        public static VariableAssignExpressionSyntax VariableAssignment(SyntaxToken assignOp, params ExpressionSyntax[] assignExpressions) => new VariableAssignExpressionSyntax(assignOp, new(SyntaxTokenKind.CommaSymbol, assignExpressions));
        public static VariableReferenceExpressionSyntax VariableReference(SyntaxToken identifier) => new VariableReferenceExpressionSyntax(identifier);
        public static MemberAccessExpressionSyntax MemberReference(ExpressionSyntax accessExpression, SyntaxToken identifier) => new MemberAccessExpressionSyntax(accessExpression, identifier);
        public static MethodInvokeExpressionSyntax MethodInvoke(ExpressionSyntax accessExpression, ArgumentListSyntax arguments, GenericArgumentListSyntax genericArguments = null) => new MethodInvokeExpressionSyntax(accessExpression, genericArguments, arguments);
        public static NewExpressionSyntax New(TypeReferenceSyntax newType, ArgumentListSyntax arguments = null) => new NewExpressionSyntax(newType, arguments);
        public static TernaryExpressionSyntax Ternary(ExpressionSyntax condition, ExpressionSyntax trueExpression, ExpressionSyntax falseExpression) => new TernaryExpressionSyntax(condition, trueExpression, falseExpression);
        public static UnaryExpressionSyntax UnaryPrefix(SyntaxToken op, ExpressionSyntax expression) => new UnaryExpressionSyntax(expression, op, true);
        public static UnaryExpressionSyntax UnaryPostfix(SyntaxToken op, ExpressionSyntax expression) => new UnaryExpressionSyntax(expression, op, false);
        public static BinaryExpressionSyntax Binary(ExpressionSyntax left, SyntaxToken op, ExpressionSyntax right) => new BinaryExpressionSyntax(left, op, right);
        public static MethodInvokeExpressionSyntax MethodInvoke(ExpressionSyntax accessExpression, ArgumentListSyntax arguments = null) => new MethodInvokeExpressionSyntax(accessExpression, null, arguments);
        public static MethodInvokeExpressionSyntax MethodInvoke(ExpressionSyntax accessExpression, GenericArgumentListSyntax genericArguments, ArgumentListSyntax arguments = null) => new MethodInvokeExpressionSyntax(accessExpression, genericArguments, arguments);
        #endregion



        #region Expanders
        public static T WithRootMembers<T>(this T node, params MemberSyntax[] members) where T : SyntaxNode, IRootSyntaxContainer
        {
            foreach (MemberSyntax member in members)
                node.AddRootSyntax(member);

            return node;
        }

        public static T WithMembers<T>(this T node, params MemberSyntax[] members) where T : SyntaxNode, IMemberSyntaxContainer
        {
            foreach (MemberSyntax member in members)
                node.AddMember(member);

            return node;
        }

        // ### Attributes
        public static TypeSyntax WithAttributes(this TypeSyntax type, params AttributeReferenceSyntax[] attributes)
            => new TypeSyntax(type.Identifier, attributes, type.AccessModifiers, type.GenericParameters, type.IsOverride, type.BaseTypes, type.MemberBlock);

        public static ContractSyntax WithAttributes(this ContractSyntax contract, params AttributeReferenceSyntax[] attributes)
            => new ContractSyntax(contract.Identifier, attributes, contract.AccessModifiers, contract.GenericParameters, contract.BaseTypes, contract.MemberBlock);

        public static EnumSyntax WithAttributes(this EnumSyntax e, params AttributeReferenceSyntax[] attributes)
            => new EnumSyntax(e.Identifier, attributes, e.AccessModifiers, e.UnderlyingTypeReference, e.FieldBlock);

        public static FieldSyntax WithAttributes(this FieldSyntax field, params AttributeReferenceSyntax[] attributes)
            => new FieldSyntax(field.Identifier, attributes, field.AccessModifiers, field.FieldType, field.FieldAssignment);

        public static MethodSyntax WithAttributes(this MethodSyntax method, params AttributeReferenceSyntax[] attributes)
            => new MethodSyntax(method.Identifier, attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, method.Body, method.LambdaStatement);

        public static ParameterSyntax WithAttributes(this ParameterSyntax parameter, params AttributeReferenceSyntax[] attributes)
            => new ParameterSyntax(attributes, parameter.ParameterType, parameter.Identifier, parameter.Assignment, parameter.Enumerable);


        // ### Modifiers
        public static TypeSyntax WithAccessModifiers(this TypeSyntax type, params SyntaxToken[] modifiers)
            => new TypeSyntax(type.Identifier, type.Attributes, modifiers, type.GenericParameters, type.IsOverride, type.BaseTypes, type.MemberBlock);

        public static ContractSyntax WithAccessModifiers(this ContractSyntax contract, params SyntaxToken[] modifiers)
            => new ContractSyntax(contract.Identifier, contract.Attributes, modifiers, contract.GenericParameters, contract.BaseTypes, contract.MemberBlock);

        public static EnumSyntax WithAccessModifiers(this EnumSyntax e, params SyntaxToken[] modifiers)
            => new EnumSyntax(e.Identifier, e.Attributes, modifiers, e.UnderlyingTypeReference, e.FieldBlock);

        public static FieldSyntax WithAccessModifiers(this FieldSyntax field, params SyntaxToken[] modifiers)
            => new FieldSyntax(field.Identifier, field.Attributes, modifiers, field.FieldType, field.FieldAssignment);

        public static MethodSyntax WithAccessModifiers(this MethodSyntax method, params SyntaxToken[] modifiers)
            => new MethodSyntax(method.Identifier, method.Attributes, modifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, method.Body, method.LambdaStatement);


        // Generic parameters
        public static TypeSyntax WithGenericParameters(this TypeSyntax type, params GenericParameterSyntax[] genericParameters)
            => type.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static TypeSyntax WithGenericParameters(this TypeSyntax type, GenericParameterListSyntax genericParameters)
            => new TypeSyntax(type.Identifier, type.Attributes, type.AccessModifiers, genericParameters, type.IsOverride, type.BaseTypes, type.MemberBlock);

        public static ContractSyntax WithGenericParameters(this ContractSyntax contract, params GenericParameterSyntax[] genericParameters)
            => contract.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static ContractSyntax WithGenericParameters(this ContractSyntax contract, GenericParameterListSyntax genericParameters)
            => new ContractSyntax(contract.Identifier, contract.Attributes, contract.AccessModifiers, genericParameters, contract.BaseTypes, contract.MemberBlock);

        public static MethodSyntax WithGenericParameters(this MethodSyntax method, params GenericParameterSyntax[] genericParameters)
            => method.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static MethodSyntax WithGenericParameters(this MethodSyntax method, GenericParameterListSyntax genericParameters)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, genericParameters, method.Parameters, method.Override, method.Body, method.LambdaStatement);


        // Base type
        public static TypeSyntax WithBaseTypes(this TypeSyntax type, params TypeReferenceSyntax[] baseTypes)
            => new TypeSyntax(type.Identifier, type.Attributes, type.AccessModifiers, type.GenericParameters, type.IsOverride, new(SyntaxTokenKind.CommaSymbol, baseTypes), type.MemberBlock);

        public static ContractSyntax WithBaseTypes(this ContractSyntax contract, params TypeReferenceSyntax[] baseTypes)
            => new ContractSyntax(contract.Identifier, contract.Attributes, contract.AccessModifiers, contract.GenericParameters, new(SyntaxTokenKind.CommaSymbol, baseTypes), contract.MemberBlock);

        public static EnumSyntax WithUnderlyingType(this EnumSyntax e, TypeReferenceSyntax underlyingType)
            => new EnumSyntax(e.Identifier, e.Attributes, e.AccessModifiers, underlyingType, e.FieldBlock);


        // Accessor
        public static AccessorSyntax WithAccessorLambda(this AccessorSyntax accessor, StatementSyntax statement)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.AccessModifiers, accessor.AccessorType, new[] { new AccessorBodySyntax(null, AccessorOperation.Read, statement) }, accessor.IsOverride);

        public static AccessorSyntax WithAccessorBody(this AccessorSyntax accessor, params AccessorBodySyntax[] accessorBodies)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.AccessModifiers, accessor.AccessorType, accessorBodies, accessor.IsOverride);


        // Methods
        public static MethodSyntax WithReturn(this MethodSyntax method, TypeReferenceSyntax returnType)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, new(SyntaxTokenKind.CommaSymbol, new[] { returnType }), method.GenericParameters, method.Parameters, method.Override, method.Body, method.LambdaStatement);

        public static MethodSyntax WithParameters(this MethodSyntax method, params ParameterSyntax[] parameters)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, new ParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, parameters)), method.Override, method.Body, method.LambdaStatement);

        public static MethodSyntax WithBody(this MethodSyntax method, params StatementSyntax[] bodyStatements)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, new BlockSyntax<StatementSyntax>(null, bodyStatements), null);

        public static MethodSyntax WithBody(this MethodSyntax method, BlockSyntax<StatementSyntax> body)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, body, null);

        public static MethodSyntax WithLambda(this MethodSyntax method, StatementSyntax inlineStatement)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, null, new LambdaStatementSyntax(inlineStatement));
        
        public static MethodSyntax WithLambda(this MethodSyntax method, LambdaStatementSyntax lambda)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, null, lambda);




        // Condition
        public static ConditionStatementSyntax WithStatementBlock(this ConditionStatementSyntax conditionStatement, params StatementSyntax[] statements)
            => new ConditionStatementSyntax(conditionStatement.Keyword, conditionStatement.Condition, conditionStatement.Alternate, new StatementBlockSyntax(statements));

        public static ConditionStatementSyntax WithStatementBlock(this ConditionStatementSyntax conditionStatement, StatementBlockSyntax block)
            => new ConditionStatementSyntax(conditionStatement.Keyword, conditionStatement.Condition, conditionStatement.Alternate, block);

        public static ConditionStatementSyntax WithInlineStatement(this ConditionStatementSyntax conditionStatement, StatementSyntax statement)
            => new ConditionStatementSyntax(conditionStatement.Keyword, conditionStatement.Condition, conditionStatement.Alternate, statement);


        public static ConditionStatementSyntax WithAlternate(this ConditionStatementSyntax conditionStatement, ConditionStatementSyntax alternate)
            => new ConditionStatementSyntax(conditionStatement.Keyword, conditionStatement.Condition, alternate, conditionStatement.Statement);


        // For
        public static ForStatementSyntax WithStatementBlock(this ForStatementSyntax forStatement, params StatementSyntax[] statements)
            => new ForStatementSyntax(forStatement.Keyword, forStatement.Variable, forStatement.Condition, forStatement.Increments, new StatementBlockSyntax(statements));

        public static ForStatementSyntax WithStatementBlock(this ForStatementSyntax forStatement, StatementBlockSyntax block)
            => new ForStatementSyntax(forStatement.Keyword, forStatement.Variable, forStatement.Condition, forStatement.Increments, block);

        public static ForStatementSyntax WithInlineStatement(this ForStatementSyntax forStatement, StatementSyntax statement)
            => new ForStatementSyntax(forStatement.Keyword, forStatement.Variable, forStatement.Condition, forStatement.Increments, statement);


        //public static AccessorSyntax WithReadStatement(this AccessorSyntax accessor, StatementSyntax statement)
        //{
        //    accessor.AssignExpression = null;
        //    accessor.ReadBody = new AccessorBodySyntax(SyntaxToken.Read(), statement);
        //    return accessor;
        //}

        //public static AccessorSyntax WithReadStatements(this AccessorSyntax accessor, params StatementSyntax[] readStatements)
        //{
        //    accessor.AssignExpression = null;
        //    accessor.ReadBody = new AccessorBodySyntax(SyntaxToken.Read(), readStatements);
        //    return accessor;
        //}

        //public static AccessorSyntax WithWriteStatement(this AccessorSyntax accessor, StatementSyntax statement)
        //{
        //    accessor.AssignExpression = null;
        //    accessor.WriteBody = new AccessorBodySyntax(SyntaxToken.Write(), statement);
        //    return accessor;
        //}

        //public static AccessorSyntax WithWriteStatements(this AccessorSyntax accessor, params StatementSyntax[] writeStatements)
        //{
        //    accessor.AssignExpression = null;
        //    accessor.WriteBody = new AccessorBodySyntax(SyntaxToken.Write(), writeStatements);
        //    return accessor;
        //}


        //public static MethodSyntax WithParameters(this MethodSyntax method, params ParameterSyntax[] parameters)
        //{
        //    method.Parameters = new ParameterListSyntax(parameters);
        //    return method;
        //}


        //public static MethodSyntax WithStatements(this MethodSyntax method, params StatementSyntax[] statements)
        //{
        //    method.Body = new BlockSyntax<StatementSyntax>(statements);
        //    return method;
        //}


        //public static ConditionStatementSyntax WithStatements(this ConditionStatementSyntax condition, params StatementSyntax[] statements)
        //{
        //    condition.BlockStatement = new BlockSyntax<StatementSyntax>(statements);
        //    return condition;
        //}

        //public static ConditionStatementSyntax WithInlineStatement(this ConditionStatementSyntax condition, StatementSyntax statement)
        //{
        //    condition.InlineStatement = statement;
        //    return condition;
        //}

        //public static ConditionStatementSyntax WithAlternate(this ConditionStatementSyntax condition, ConditionStatementSyntax alternate = null)
        //{
        //    condition.Alternate = alternate;
        //    alternate.MakeAlternate();
        //    return condition;
        //}

        //public static ForeachStatementSyntax WithStatements(this ForeachStatementSyntax foreachLoop, params StatementSyntax[] statements)
        //{
        //    foreachLoop.BlockStatement = new BlockSyntax<StatementSyntax>(statements);
        //    return foreachLoop;
        //}

        //public static ForeachStatementSyntax WithInlineStatement(this ForeachStatementSyntax foreachLoop, StatementSyntax statement)
        //{
        //    foreachLoop.InlineStatement = statement;
        //    return foreachLoop;
        //}

        //public static ForStatementSyntax WithStatements(this ForStatementSyntax forLoop, params StatementSyntax[] statements)
        //{
        //    forLoop.BlockStatement = new BlockSyntax<StatementSyntax>(statements);
        //    return forLoop;
        //}

        //public static ForStatementSyntax WithInlineStatement(this ForStatementSyntax forLoop, StatementSyntax statement)
        //{
        //    forLoop.InlineStatement = statement;
        //    return forLoop;
        //}


        //public static MethodInvokeExpressionSyntax WithGenericArguments(this MethodInvokeExpressionSyntax invoke, params TypeReferenceSyntax[] genericArguments)
        //{
        //    invoke.GenericArguments = genericArguments;
        //    return invoke;
        //}

        //public static MethodInvokeExpressionSyntax WithArguments(this MethodInvokeExpressionSyntax invoke, params ExpressionSyntax[] arguments)
        //{
        //    invoke.Arguments = arguments;
        //    return invoke;
        //}


        //public static NewExpressionSyntax WithArguments(this NewExpressionSyntax newExpr, params ExpressionSyntax[] arguments)
        //{
        //    newExpr.ArgumentExpressions = arguments;
        //    return newExpr;
        //}

        //public static TypeReferenceSyntax WithNamespaceQualifier(this TypeReferenceSyntax type, params string[] namespaceIdentifiers)
        //{
        //    if (type.HasParentTypeIdentifier == true)
        //        throw new InvalidOperationException("Nested type cannot have a namespace qualifier");

        //    type.Namespace = new NamespaceName(namespaceIdentifiers);
        //    return type;
        //}

        //public static TypeReferenceSyntax WithGenericArguments(this TypeReferenceSyntax type, params TypeReferenceSyntax[] genericArguments)
        //{
        //    type.GenericArguments = new GenericArgumentListSyntax(genericArguments);
        //    return type;
        //}

        //public static TypeReferenceSyntax WithArrayQualifier(this TypeReferenceSyntax type, int rank)
        //{
        //    type.ArrayParameters = new ArrayParametersSyntax(rank);
        //    return type;
        //}
        #endregion

        //public static NamespaceSyntax Namespace(string identifier)
        //{
        //    return SyntaxTree.Create()
        //        .WithNamespace(identifier);
        //}

        //public static TypeSyntax Type(string identifier)
        //{
        //    return SyntaxTree.Create()
        //        .WithType(identifier);
        //}

        //public static ContractSyntax Contract(string identifier)
        //{
        //    return SyntaxTree.Create()
        //        .WithContract(identifier);
        //}

        //public static EnumSyntax Enum(string identifier)
        //{
        //    return SyntaxTree.Create()
        //        .WithEnum(identifier);
        //}

        //public static MethodSyntax Method(string identifier)
        //{
        //    return SyntaxTree.Create().WithMethod(identifier);
        //}


        //public static NamespaceSyntax WithNamespace<T>(this T node, string identifier) where T : SyntaxNode, IRootSyntaxContainer
        //{
        //    // Create namespace 
        //    NamespaceSyntax ns = new NamespaceSyntax(node.tree, node, identifier);

        //    // Add member
        //    node.AddRootSyntax(ns);
        //    return ns;
        //}

        //public static TypeSyntax WithType<T>(this T node, string identifier) where T : SyntaxNode, IMemberSyntaxContainer
        //{
        //    // Create type
        //    TypeSyntax type = new TypeSyntax(node.SyntaxTree, node, identifier);

        //    // Add member
        //    node.AddMember(type);
        //    return type;
        //}

        //public static ContractSyntax WithContract<T>(this T node, string identifier) where T : SyntaxNode, IMemberSyntaxContainer
        //{
        //    // Create contract
        //    ContractSyntax contract = new ContractSyntax(node.SyntaxTree, node, identifier);

        //    // Add member
        //    node.AddMember(contract);
        //    return contract;
        //}

        //public static EnumSyntax WithEnum<T>(this T node, string identifier) where T : SyntaxNode, IMemberSyntaxContainer
        //{
        //    // Create contract
        //    EnumSyntax e = new EnumSyntax(node.SyntaxTree, node, identifier);

        //    // Add member
        //    node.AddMember(e);
        //    return e;
        //}


        #region Members

        public static MethodSyntax WithMethod<T>(this T node, SyntaxToken identifier) where T : SyntaxNode, IMemberSyntaxContainer
        {
            // Create method
            MethodSyntax method = Method(identifier);

            // Add member
            node.AddMember(method);
            return method;
        }
        #endregion

        //#region Statements
        //public static T WithStatement<T>(this T node, StatementSyntax statement)
        //{

        //}
        //#endregion

        //#region RootMembers
        //public static TypeSyntax WithTypeSyntax(this IRootMemberSyntaxContainer container, string identifier)
        //{
        //    // Check container
        //    CheckSyntaxNode(container);

        //    // Create type
        //    return new TypeSyntax(container.Tree, container as SyntaxNode, identifier);
        //}
        //#endregion

        //#region Members
        //public static TypeSyntax WithTypeSyntax(this IMemberSyntaxContainer container, string identifier)
        //{
        //    // Check container
        //    CheckSyntaxNode(container);

        //    // Create type
        //    return new TypeSyntax(container.Tree, container as SyntaxNode, identifier);
        //}
        //#endregion

        //#region Statements
        //public static BreakStatementSyntax WithBreakSyntax(this IStatementSyntaxContainer container)
        //{
        //    // Check container
        //    CheckSyntaxNode(container);

        //    // Create break
        //    return new BreakStatementSyntax(container.Tree, container as SyntaxNode);
        //}

        //public static ContinueStatementSyntax WithContinueSyntax(this IStatementSyntaxContainer container)
        //{
        //    // Check container
        //    CheckSyntaxNode(container);

        //    // Create continue
        //    return new ContinueStatementSyntax(container.Tree, container as SyntaxNode);
        //}

        //public static ReturnStatementSyntax WithReturnSyntax(this IStatementSyntaxContainer container, ExpressionSyntax returnExpression = null)
        //{
        //    // Check container
        //    CheckSyntaxNode(container);

        //    // Create return
        //    return new ReturnStatementSyntax(container.Tree, container as SyntaxNode, returnExpression);
        //}
        //#endregion

        private static void CheckSyntaxNode(object container)
        {
            // Check for syntax node
            if ((container is SyntaxNode) == false)
                throw new ArgumentException("Container must be a syntax node");
        }
    }
}
