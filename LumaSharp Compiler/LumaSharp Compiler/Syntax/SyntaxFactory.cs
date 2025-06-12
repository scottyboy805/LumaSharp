
namespace LumaSharp.Compiler.AST
{ 
    public static class Syntax
    {
        // Methods
        public static SyntaxToken Identifier(string identifier)
        {
            return new SyntaxToken(SyntaxTokenKind.Identifier, identifier, default);
        }

        public static SyntaxToken Token(SyntaxTokenKind kind)
        {
            return new SyntaxToken(kind, default);
        }

        public static SyntaxToken Token(SyntaxTokenKind kind, string text)
        {
            return new SyntaxToken(kind, text, default);
        }

        #region Members
        internal static SeparatedTokenList NamespaceName(string[] identifiers) => new SeparatedTokenList(identifiers.Select(n => Identifier(n)).ToArray(), SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier);

        public static ImportSyntax Import(params SyntaxToken[] identifiers) => new ImportSyntax(new(identifiers, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier));
        //public static ImportSyntax ImportAlias(string alias, TypeReferenceSyntax aliasType, params string[] identifiers) => new ImportSyntax(null, alias, aliasType, identifiers);
        
        public static NamespaceSyntax Namespace(params SyntaxToken[] identifiers) => new NamespaceSyntax(new(identifiers, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier));
        public static TypeSyntax Type(SyntaxToken identifier) => new TypeSyntax(identifier, null, null, null, null, null, null);
        public static ContractSyntax Contract(SyntaxToken identifier) => new ContractSyntax( identifier, null, null, null, null, null);
        public static EnumSyntax Enum(SyntaxToken identifier, TypeReferenceSyntax underlyingType = null) => new EnumSyntax(identifier, null, null, new(new(SyntaxTokenKind.CommaSymbol, underlyingType)), null);
        public static FieldSyntax Field(SyntaxToken identifier, TypeReferenceSyntax fieldType, VariableAssignmentExpressionSyntax fieldAssignment = null) => new FieldSyntax(identifier, null, null, fieldType, fieldAssignment);
        public static EnumFieldSyntax EnumField(SyntaxToken identifier, VariableAssignmentExpressionSyntax enumAssignment = null) => new EnumFieldSyntax(identifier, enumAssignment);
        public static AccessorBodySyntax AccessorLambda(StatementSyntax statement) => new AccessorBodySyntax(Token(SyntaxTokenKind.ReadKeyword), statement);
        public static AccessorBodySyntax AccessorRead(StatementSyntax statement) => new AccessorBodySyntax(Token(SyntaxTokenKind.ReadKeyword), statement);
        public static AccessorBodySyntax AccessorWrite(StatementSyntax statement) => new AccessorBodySyntax(Token(SyntaxTokenKind.WriteKeyword), statement);
        public static AccessorSyntax Accessor(SyntaxToken identifier, TypeReferenceSyntax accessorType) => new AccessorSyntax(identifier, null, null, accessorType, null, null, null);

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
        public static GenericParameterSyntax GenericParameter(SyntaxToken identifier, params TypeReferenceSyntax[] constrainTypes) => new GenericParameterSyntax(identifier, default, new(SyntaxTokenKind.CommaSymbol, constrainTypes));
        public static GenericParameterListSyntax GenericParameterList(params GenericParameterSyntax[] genericParameters) => new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters));
        public static ParameterSyntax Parameter(TypeReferenceSyntax parameterType, SyntaxToken identifier, bool variableSizedList = false) => new ParameterSyntax(null, parameterType, identifier, null, variableSizedList ? Token(SyntaxTokenKind.EnumerableSymbol) : (SyntaxToken?)null);
        public static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters) => new ParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, parameters));
        public static GenericArgumentListSyntax GenericArgumentList(params TypeReferenceSyntax[] genericTypeArguments) => new GenericArgumentListSyntax(new(SyntaxTokenKind.CommaSymbol, genericTypeArguments));
        public static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argumentExpressions) => new ArgumentListSyntax(new(SyntaxTokenKind.CommaSymbol, argumentExpressions));
        public static AttributeSyntax Attribute(TypeReferenceSyntax attributeType, ArgumentListSyntax arguments = null) => new AttributeSyntax(attributeType, arguments);
        #endregion

        #region Statements
        public static VariableDeclarationSyntax Variable(TypeReferenceSyntax variableType, SyntaxToken identifier, VariableAssignmentExpressionSyntax assignment = null) => new VariableDeclarationSyntax(variableType, new(new [] { identifier }, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier), assignment);
        public static VariableDeclarationSyntax Variable(TypeReferenceSyntax variableType, SyntaxToken[] identifiers, VariableAssignmentExpressionSyntax assignment = null) => new VariableDeclarationSyntax(variableType, new(identifiers, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier), assignment);
        public static VariableDeclarationStatementSyntax LocalVariable(VariableDeclarationSyntax variable) => new VariableDeclarationStatementSyntax(variable);
        public static VariableDeclarationStatementSyntax LocalVariable(TypeReferenceSyntax variableType, SyntaxToken identifier, VariableAssignmentExpressionSyntax assignment = null) => new VariableDeclarationStatementSyntax(Variable(variableType, identifier, assignment));
        public static VariableDeclarationStatementSyntax LocalVariable(TypeReferenceSyntax variableType, SyntaxToken[] identifiers, VariableAssignmentExpressionSyntax assignment = null) => new VariableDeclarationStatementSyntax(Variable(variableType, identifiers, assignment));
        public static AssignStatementSyntax Assign(ExpressionSyntax left, VariableAssignmentExpressionSyntax variableAssign) => new AssignStatementSyntax(new(SyntaxTokenKind.CommaSymbol, new[] { left }), variableAssign);
        public static AssignStatementSyntax Assign(ExpressionSyntax[] left, VariableAssignmentExpressionSyntax variableAssign) => new AssignStatementSyntax(new(SyntaxTokenKind.CommaSymbol, left), variableAssign);
        public static BreakStatementSyntax Break() => new BreakStatementSyntax();
        public static ConditionStatementSyntax Condition(ExpressionSyntax condition = null) => new ConditionStatementSyntax(Token(SyntaxTokenKind.IfKeyword), condition, null, new EmptyStatementSyntax());
        public static ContinueStatementSyntax Continue() => new ContinueStatementSyntax();
        public static ForStatementSyntax For(VariableDeclarationSyntax variable, ExpressionSyntax condition, params ExpressionSyntax[] increments) => new ForStatementSyntax(variable, condition, new(SyntaxTokenKind.CommaSymbol, increments), new EmptyStatementSyntax());
        public static ReturnStatementSyntax Return(ExpressionSyntax expression = null) => new ReturnStatementSyntax(expression != null ? new(SyntaxTokenKind.CommaSymbol, new[] {expression}) : null);
        public static ReturnStatementSyntax Return(params ExpressionSyntax[] expressions) => new ReturnStatementSyntax(new(SyntaxTokenKind.CommaSymbol, expressions));
        public static MethodInvokeStatementSyntax MethodInvoke(MethodInvokeExpressionSyntax invokeExpression) => new MethodInvokeStatementSyntax(invokeExpression);
        #endregion

        #region Expressions
        public static LiteralExpressionSyntax LiteralNull() => new LiteralExpressionSyntax(Token(SyntaxTokenKind.NullKeyword));
        public static LiteralExpressionSyntax Literal(int val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString(), default));
        public static LiteralExpressionSyntax Literal(uint val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString(), default), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "U", default));
        public static LiteralExpressionSyntax Literal(long val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString(), default), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "L", default));
        public static LiteralExpressionSyntax Literal(ulong val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString(), default), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "UL", default));
        public static LiteralExpressionSyntax Literal(double val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, val.ToString(), default));
        public static LiteralExpressionSyntax Literal(string val) => new LiteralExpressionSyntax(new SyntaxToken(SyntaxTokenKind.Literal, "\"" + val + "\"", default));
        public static LiteralExpressionSyntax Literal(bool val) => new LiteralExpressionSyntax(new SyntaxToken(val == true ? SyntaxTokenKind.TrueKeyword : SyntaxTokenKind.FalseKeyword, val.ToString().ToLower(), default));
        
        public static IndexExpressionSyntax ArrayIndex(ExpressionSyntax accessExpression, params ExpressionSyntax[] indexExpressions) => new IndexExpressionSyntax(accessExpression, new(SyntaxTokenKind.CommaSymbol, indexExpressions));
        public static BaseExpressionSyntax Base() => new BaseExpressionSyntax();
        public static ThisExpressionSyntax This() => new ThisExpressionSyntax();
        public static TypeofExpressionSyntax Typeof(TypeReferenceSyntax typeReference) => new TypeofExpressionSyntax(typeReference);
        public static SizeofExpressionSyntax Sizeof(TypeReferenceSyntax typeReference) => new SizeofExpressionSyntax(typeReference);
        public static VariableAssignmentExpressionSyntax VariableAssignment(params ExpressionSyntax[] assignExpressions) => new VariableAssignmentExpressionSyntax(new(SyntaxTokenKind.CommaSymbol, assignExpressions));
        public static VariableAssignmentExpressionSyntax VariableAssignment(SyntaxToken assignOp, params ExpressionSyntax[] assignExpressions) => new VariableAssignmentExpressionSyntax(assignOp, new(SyntaxTokenKind.CommaSymbol, assignExpressions));
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
        // ### Members
        public static TypeSyntax WithMembers(this TypeSyntax type, params MemberSyntax[] members)
            => new TypeSyntax(type.Identifier, type.Attributes, type.AccessModifiers, type.GenericParameters, type.Override, type.BaseTypes, new MemberBlockSyntax(members));

        public static ContractSyntax WithMembers(this ContractSyntax contract, params MemberSyntax[] members)
            => new ContractSyntax(contract.Identifier, contract.Attributes, contract.AccessModifiers, contract.GenericParameters, contract.BaseTypes, new MemberBlockSyntax(members));

        public static EnumSyntax WithMembers(this EnumSyntax e, params EnumFieldSyntax[] fields)
            => new EnumSyntax(e.Identifier, e.Attributes, e.AccessModifiers, e.UnderlyingType, new EnumBlockSyntax(new SeparatedSyntaxList<EnumFieldSyntax>(SyntaxTokenKind.CommaSymbol, fields)));

        // ### Attributes
        public static TypeSyntax WithAttributes(this TypeSyntax type, params AttributeSyntax[] attributes)
            => new TypeSyntax(type.Identifier, attributes, type.AccessModifiers, type.GenericParameters, type.Override, type.BaseTypes, type.Members);

        public static ContractSyntax WithAttributes(this ContractSyntax contract, params AttributeSyntax[] attributes)
            => new ContractSyntax(contract.Identifier, attributes, contract.AccessModifiers, contract.GenericParameters, contract.BaseTypes, contract.Members);

        public static EnumSyntax WithAttributes(this EnumSyntax e, params AttributeSyntax[] attributes)
            => new EnumSyntax(e.Identifier, attributes, e.AccessModifiers, e.UnderlyingType, e.Body);

        public static FieldSyntax WithAttributes(this FieldSyntax field, params AttributeSyntax[] attributes)
            => new FieldSyntax(field.Identifier, attributes, field.AccessModifiers, field.FieldType, field.FieldAssignment);

        public static MethodSyntax WithAttributes(this MethodSyntax method, params AttributeSyntax[] attributes)
            => new MethodSyntax(method.Identifier, attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, method.Body, method.Lambda);

        public static ParameterSyntax WithAttributes(this ParameterSyntax parameter, params AttributeSyntax[] attributes)
            => new ParameterSyntax(attributes, parameter.ParameterType, parameter.Identifier, parameter.Assignment, parameter.Enumerable);


        // ### Modifiers
        public static TypeSyntax WithAccessModifiers(this TypeSyntax type, params SyntaxToken[] modifiers)
            => new TypeSyntax(type.Identifier, type.Attributes, modifiers, type.GenericParameters, type.Override, type.BaseTypes, type.Members);

        public static ContractSyntax WithAccessModifiers(this ContractSyntax contract, params SyntaxToken[] modifiers)
            => new ContractSyntax(contract.Identifier, contract.Attributes, modifiers, contract.GenericParameters, contract.BaseTypes, contract.Members);

        public static EnumSyntax WithAccessModifiers(this EnumSyntax e, params SyntaxToken[] modifiers)
            => new EnumSyntax(e.Identifier, e.Attributes, modifiers, e.UnderlyingType, e.Body);

        public static FieldSyntax WithAccessModifiers(this FieldSyntax field, params SyntaxToken[] modifiers)
            => new FieldSyntax(field.Identifier, field.Attributes, modifiers, field.FieldType, field.FieldAssignment);

        public static MethodSyntax WithAccessModifiers(this MethodSyntax method, params SyntaxToken[] modifiers)
            => new MethodSyntax(method.Identifier, method.Attributes, modifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, method.Body, method.Lambda);


        // Generic parameters
        public static TypeSyntax WithGenericParameters(this TypeSyntax type, params GenericParameterSyntax[] genericParameters)
            => type.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static TypeSyntax WithGenericParameters(this TypeSyntax type, GenericParameterListSyntax genericParameters)
            => new TypeSyntax(type.Identifier, type.Attributes, type.AccessModifiers, genericParameters, type.Override, type.BaseTypes, type.Members);

        public static ContractSyntax WithGenericParameters(this ContractSyntax contract, params GenericParameterSyntax[] genericParameters)
            => contract.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static ContractSyntax WithGenericParameters(this ContractSyntax contract, GenericParameterListSyntax genericParameters)
            => new ContractSyntax(contract.Identifier, contract.Attributes, contract.AccessModifiers, genericParameters, contract.BaseTypes, contract.Members);

        public static MethodSyntax WithGenericParameters(this MethodSyntax method, params GenericParameterSyntax[] genericParameters)
            => method.WithGenericParameters(new GenericParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, genericParameters)));

        public static MethodSyntax WithGenericParameters(this MethodSyntax method, GenericParameterListSyntax genericParameters)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, genericParameters, method.Parameters, method.Override, method.Body, method.Lambda);


        // Base type
        public static TypeSyntax WithBaseTypes(this TypeSyntax type, params TypeReferenceSyntax[] baseTypes)
            => new TypeSyntax(type.Identifier, type.Attributes, type.AccessModifiers, type.GenericParameters, type.Override, new(new(SyntaxTokenKind.CommaSymbol, baseTypes)), type.Members);

        public static ContractSyntax WithBaseTypes(this ContractSyntax contract, params TypeReferenceSyntax[] baseTypes)
            => new ContractSyntax(contract.Identifier, contract.Attributes, contract.AccessModifiers, contract.GenericParameters, new(new(SyntaxTokenKind.CommaSymbol, baseTypes)), contract.Members);

        public static EnumSyntax WithUnderlyingType(this EnumSyntax e, TypeReferenceSyntax underlyingType)
            => new EnumSyntax(e.Identifier, e.Attributes, e.AccessModifiers, new(new(SyntaxTokenKind.CommaSymbol, underlyingType)), e.Body);


        // Accessor
        public static AccessorSyntax WithAccessorLambda(this AccessorSyntax accessor, ExpressionSyntax expression)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.AccessModifiers, accessor.AccessorType, accessor.Override, null, new(expression));


        public static AccessorSyntax WithAccessorLambda(this AccessorSyntax accessor, StatementSyntax statement)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.AccessModifiers, accessor.AccessorType, accessor.Override, new[] { new AccessorBodySyntax(Token(SyntaxTokenKind.ReadKeyword), statement) }, null);

        public static AccessorSyntax WithAccessorBody(this AccessorSyntax accessor, params AccessorBodySyntax[] accessorBodies)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.AccessModifiers, accessor.AccessorType, accessor.Override, accessorBodies, null);


        // Methods
        public static MethodSyntax WithReturn(this MethodSyntax method, TypeReferenceSyntax returnType)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, new(SyntaxTokenKind.CommaSymbol, new[] { returnType }), method.GenericParameters, method.Parameters, method.Override, method.Body, method.Lambda);

        public static MethodSyntax WithParameters(this MethodSyntax method, params ParameterSyntax[] parameters)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, new ParameterListSyntax(new(SyntaxTokenKind.CommaSymbol, parameters)), method.Override, method.Body, method.Lambda);

        public static MethodSyntax WithBody(this MethodSyntax method, params StatementSyntax[] bodyStatements)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, new StatementBlockSyntax(bodyStatements), null);

        public static MethodSyntax WithBody(this MethodSyntax method, StatementBlockSyntax body)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, body, null);

        public static MethodSyntax WithLambda(this MethodSyntax method, StatementSyntax inlineStatement)
            => new MethodSyntax(method.Identifier, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.Override, null, new LambdaSyntax(inlineStatement));
        
        public static MethodSyntax WithLambda(this MethodSyntax method, LambdaSyntax lambda)
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
            => new ForStatementSyntax(forStatement.Keyword, forStatement.Variable, forStatement.VariableSemicolon, forStatement.Condition, forStatement.ConditionSemicolon, forStatement.Increments, new StatementBlockSyntax(statements));

        public static ForStatementSyntax WithStatementBlock(this ForStatementSyntax forStatement, StatementBlockSyntax block)
            => new ForStatementSyntax(forStatement.Keyword, forStatement.Variable, forStatement.VariableSemicolon, forStatement.Condition, forStatement.ConditionSemicolon, forStatement.Increments, block);

        public static ForStatementSyntax WithInlineStatement(this ForStatementSyntax forStatement, StatementSyntax statement)
            => new ForStatementSyntax(forStatement.Keyword, forStatement.Variable, forStatement.VariableSemicolon, forStatement.Condition, forStatement.ConditionSemicolon, forStatement.Increments, statement);

        #endregion

        private static void CheckSyntaxNode(object container)
        {
            // Check for syntax node
            if ((container is SyntaxNode) == false)
                throw new ArgumentException("Container must be a syntax node");
        }
    }
}
