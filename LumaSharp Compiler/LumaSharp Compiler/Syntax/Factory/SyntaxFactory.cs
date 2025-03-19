
namespace LumaSharp.Compiler.AST
{ 
    public static class Syntax
    {
        // Methods
        public static SyntaxToken Identifier(string identifier)
        {
            return new SyntaxToken(SyntaxTokenKind.Identifier, identifier);
        }

        public static SyntaxToken KeywordOrSymbol(SyntaxTokenKind kind)
        {
            return new SyntaxToken(kind);
        }

        public static SyntaxToken AssignOp(AssignOperation op)
        {
            return new SyntaxToken(SyntaxTokenKind.AssignOperator, op switch
            { 
                AssignOperation.Assign => "=",
                AssignOperation.AddAssign => "+=",
                AssignOperation.SubtractAssign => "-=",
                AssignOperation.MultiplyAssign => "*=",
                AssignOperation.DivideAssign => "/=",

                _ => throw new NotImplementedException(),
            });
        }

        public static SyntaxToken UnaryOp(UnaryOperation op)
        {
            return new SyntaxToken(SyntaxTokenKind.UnaryOperator, op switch
            { 
                UnaryOperation.PrefixNegate => "-",
                UnaryOperation.PrefixNot => "!",
                UnaryOperation.PrefixAdd => "++",
                UnaryOperation.PrefixSubtract => "--",
                UnaryOperation.PostfixAdd => "++",
                UnaryOperation.PostfixSubtract => "++",

                _ => throw new NotImplementedException(),
            });
        }

        public static SyntaxToken BinaryOp(BinaryOperation op)
        {
            return new SyntaxToken(SyntaxTokenKind.BinaryOperator, op switch
            {
                BinaryOperation.Add => "+",
                BinaryOperation.Subtract => "-",
                BinaryOperation.Multiply => "*",
                BinaryOperation.Divide => "/",
                BinaryOperation.Modulus => "%",
                BinaryOperation.Greater => ">",
                BinaryOperation.GreaterEqual => ">=",
                BinaryOperation.Less => "<",
                BinaryOperation.LessEqual => "<=",
                BinaryOperation.Equal => "==",
                BinaryOperation.NotEqual => "!=",
                BinaryOperation.And => "&&",
                BinaryOperation.Or => "||",

                _ => throw new NotImplementedException(),
            });
        }

        public static SyntaxToken Modifier(AccessModifier modifier)
        {
            return modifier switch
            {
                AccessModifier.Hidden => KeywordOrSymbol(SyntaxTokenKind.HiddenKeyword),
                AccessModifier.Internal => KeywordOrSymbol(SyntaxTokenKind.InternalKeyword),
                AccessModifier.Export => KeywordOrSymbol(SyntaxTokenKind.ExportKeyword),
                AccessModifier.Global => KeywordOrSymbol(SyntaxTokenKind.GlobalKeyword),

                _ => throw new NotImplementedException(),
            };
        }

        #region Members
        internal static SeparatedTokenList NamespaceName(string[] identifiers) => new SeparatedTokenList(null, identifiers.Select(n => new SyntaxToken(SyntaxTokenKind.Identifier, n)).ToArray(), SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier);

        public static ImportSyntax Import(params string[] identifiers) => new ImportSyntax(null, identifiers);
        public static ImportSyntax ImportAlias(string alias, TypeReferenceSyntax aliasType, params string[] identifiers) => new ImportSyntax(null, alias, aliasType, identifiers);
        
        public static NamespaceSyntax Namespace(params string[] identifiers) => new NamespaceSyntax(null, identifiers);
        public static TypeSyntax Type(string identifier) => new TypeSyntax(null, identifier, null, null, null, false, null, null);
        public static ContractSyntax Contract(string identifier) => new ContractSyntax(null, identifier, null, null, null, null, null);
        public static EnumSyntax Enum(string identifier, TypeReferenceSyntax underlyingType = null) => new EnumSyntax(null, identifier, null, null, underlyingType, null);
        public static FieldSyntax Field(string identifier, TypeReferenceSyntax fieldType, VariableAssignExpressionSyntax fieldAssignment = null, bool byReference = false) => new FieldSyntax(null, identifier, null, null, fieldType, fieldAssignment);
        public static AccessorBodySyntax AccessorLambda(StatementSyntax statement) => new AccessorBodySyntax(null, AccessorOperation.Read, statement);
        public static AccessorBodySyntax AccessorBody(AccessorOperation op, params StatementSyntax[] statements) => new AccessorBodySyntax(null, op, new BlockSyntax<StatementSyntax>(null, statements));
        public static AccessorBodySyntax AccessorBody(AccessorOperation op, BlockSyntax<StatementSyntax> block) => new AccessorBodySyntax(null, op, block);
        public static AccessorSyntax Accessor(string identifier, TypeReferenceSyntax accessorType) => new AccessorSyntax(null, identifier, null, null, accessorType, null, false);

        public static MethodSyntax Method(string identifier, TypeReferenceSyntax returnType) => new MethodSyntax(null, identifier, null, null, new SeparatedListSyntax<TypeReferenceSyntax>(null, new[] { returnType }, KeywordOrSymbol(SyntaxTokenKind.CommaSymbol)), null, null, false, null, null);
        public static MethodSyntax Method(string identifier) => new MethodSyntax(null, identifier, null, null, null, null, null, false, null, null);
        #endregion

        #region CommonUse
        public static ParentTypeReferenceSyntax ParentTypeReference(string identifier, GenericArgumentListSyntax genericArguments = null) => new ParentTypeReferenceSyntax(null, identifier, genericArguments);
        public static TypeReferenceSyntax TypeReference(PrimitiveType primitive) => new TypeReferenceSyntax(null, primitive);
        public static TypeReferenceSyntax TypeReference(MemberSyntax member) => new TypeReferenceSyntax(member);
        public static TypeReferenceSyntax TypeReference(string identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, null, null, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(null, arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, string identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, NamespaceName(namespaceName), null, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(null, arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(ParentTypeReferenceSyntax[] parentTypes, string identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, null, parentTypes, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(null, arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(ParentTypeReferenceSyntax parentType, string identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, null, new[] { parentType }, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(null, arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, ParentTypeReferenceSyntax[] parentTypes, string identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, NamespaceName(namespaceName), parentTypes, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(null, arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, ParentTypeReferenceSyntax parentType, string identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, NamespaceName(namespaceName), new[] { parentType }, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(null, arrayRank.Value) : null);
        public static GenericParameterSyntax GenericParameter(string identifier, params TypeReferenceSyntax[] constrainTypes) => new GenericParameterSyntax(null, identifier, 0, constrainTypes);
        public static GenericParameterListSyntax GenericParameterList(params GenericParameterSyntax[] genericParameters) => new GenericParameterListSyntax(null, genericParameters);
        public static ParameterSyntax Parameter(TypeReferenceSyntax parameterType, string identifier, bool variableSizedList = false) => new ParameterSyntax(null, null, parameterType, identifier, null, variableSizedList);
        public static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters) => new ParameterListSyntax(null, parameters);
        public static GenericArgumentListSyntax GenericArgumentList(params TypeReferenceSyntax[] genericTypeArguments) => new GenericArgumentListSyntax(null, genericTypeArguments);
        public static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argumentExpressions) => new ArgumentListSyntax(null, argumentExpressions);
        public static AttributeReferenceSyntax Attribute(TypeReferenceSyntax attributeType, ArgumentListSyntax arguments = null) => new AttributeReferenceSyntax(null, attributeType, arguments);
        #endregion

        #region Statements
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, params string[] identifiers) => new VariableDeclarationStatementSyntax(null, variableType, identifiers, null);
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, string identifier, VariableAssignExpressionSyntax assignment) => new VariableDeclarationStatementSyntax(null, variableType, new string[] { identifier }, assignment);
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, string[] identifiers, VariableAssignExpressionSyntax assignment) => new VariableDeclarationStatementSyntax(null, variableType, identifiers, assignment);
        public static AssignStatementSyntax Assign(ExpressionSyntax left, AssignOperation assignOp, ExpressionSyntax right) => new AssignStatementSyntax(null, assignOp, new[] { left }, new[] { right });
        public static AssignStatementSyntax Assign(ExpressionSyntax[] left, AssignOperation assignOp, ExpressionSyntax[] right) => new AssignStatementSyntax(null, assignOp, left, right);
        public static BreakStatementSyntax Break() => new BreakStatementSyntax(null);
        public static ConditionStatementSyntax Condition(ExpressionSyntax condition = null) => new ConditionStatementSyntax(null, condition, false, null, null, null);
        public static ContinueStatementSyntax Continue() => new ContinueStatementSyntax(null);
        public static ForStatementSyntax For(VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, params ExpressionSyntax[] increments) => new ForStatementSyntax(null, variable, condition, new SeparatedListSyntax<ExpressionSyntax>(null, increments, KeywordOrSymbol(SyntaxTokenKind.CommaSymbol)), null, null);
        public static ReturnStatementSyntax Return(ExpressionSyntax expression = null) => new ReturnStatementSyntax(null, expression != null ? new[] {expression} : null);
        public static ReturnStatementSyntax Return(params ExpressionSyntax[] expressions) => new ReturnStatementSyntax(null, expressions);
        public static MethodInvokeStatementSyntax MethodInvoke(MethodInvokeExpressionSyntax invokeExpression) => new MethodInvokeStatementSyntax(null, invokeExpression);
        #endregion

        #region Expressions
        public static LiteralExpressionSyntax LiteralNull() => new LiteralExpressionSyntax(null, new SyntaxToken(SyntaxTokenKind.NullKeyword));
        public static LiteralExpressionSyntax Literal(int val) => new LiteralExpressionSyntax(null, new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()));
        public static LiteralExpressionSyntax Literal(uint val) => new LiteralExpressionSyntax(null, new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "U"));
        public static LiteralExpressionSyntax Literal(long val) => new LiteralExpressionSyntax(null, new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "L"));
        public static LiteralExpressionSyntax Literal(ulong val) => new LiteralExpressionSyntax(null, new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()), new SyntaxToken(SyntaxTokenKind.LiteralDescriptor, "UL"));
        public static LiteralExpressionSyntax Literal(double val) => new LiteralExpressionSyntax(null, new SyntaxToken(SyntaxTokenKind.Literal, val.ToString()));
        public static LiteralExpressionSyntax Literal(string val) => new LiteralExpressionSyntax(null, new SyntaxToken(SyntaxTokenKind.Literal, "\"" + val + "\""));
        public static LiteralExpressionSyntax Literal(bool val) => new LiteralExpressionSyntax(null, new SyntaxToken(val == true ? SyntaxTokenKind.TrueKeyword : SyntaxTokenKind.FalseKeyword, val.ToString().ToLower()));
        
        public static ArrayIndexExpressionSyntax ArrayIndex(ExpressionSyntax accessExpression, params ExpressionSyntax[] indexExpressions) => new ArrayIndexExpressionSyntax(null, accessExpression, indexExpressions);
        public static BaseExpressionSyntax Base() => new BaseExpressionSyntax(null);
        public static ThisExpressionSyntax This() => new ThisExpressionSyntax(null);
        public static TypeExpressionSyntax TypeOp(TypeReferenceSyntax typeReference) => new TypeExpressionSyntax(null, typeReference);
        public static SizeExpressionSyntax SizeOp(TypeReferenceSyntax typeReference) => new SizeExpressionSyntax(null, typeReference);
        public static VariableAssignExpressionSyntax VariableAssignment(AssignOperation assignOp, params ExpressionSyntax[] assignExpressions) => new VariableAssignExpressionSyntax(null, assignOp, assignExpressions);
        public static VariableReferenceExpressionSyntax VariableReference(string identifier) => new VariableReferenceExpressionSyntax(null, identifier);
        public static FieldReferenceExpressionSyntax FieldReference(ExpressionSyntax accessExpression, string identifier) => new FieldReferenceExpressionSyntax(null, identifier, accessExpression);
        public static MethodInvokeExpressionSyntax MethodInvoke(ExpressionSyntax accessExpression, string identifier, ArgumentListSyntax arguments, GenericArgumentListSyntax genericArguments = null) => new MethodInvokeExpressionSyntax(null, identifier, accessExpression, genericArguments, arguments);
        public static NewExpressionSyntax New(TypeReferenceSyntax newType, ArgumentListSyntax arguments = null) => new NewExpressionSyntax(null, newType, arguments);
        public static TernaryExpressionSyntax Ternary(ExpressionSyntax condition, ExpressionSyntax trueExpression, ExpressionSyntax falseExpression) => new TernaryExpressionSyntax(null, condition, trueExpression, falseExpression);
        public static UnaryExpressionSyntax Unary(UnaryOperation op, ExpressionSyntax expression) => new UnaryExpressionSyntax(null, op, expression);
        public static BinaryExpressionSyntax Binary(ExpressionSyntax left, BinaryOperation op, ExpressionSyntax right) => new BinaryExpressionSyntax(null, left, op, right);
        public static MethodInvokeExpressionSyntax MethodInvoke(ExpressionSyntax accessExpression, string identifier, ArgumentListSyntax arguments = null) => new MethodInvokeExpressionSyntax(null, identifier, accessExpression, null, arguments);
        public static MethodInvokeExpressionSyntax MethodInvoke(ExpressionSyntax accessExpression, string identifier, GenericArgumentListSyntax genericArguments, ArgumentListSyntax arguments = null) => new MethodInvokeExpressionSyntax(null, identifier, accessExpression, genericArguments, arguments);
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
            => new TypeSyntax(type.Parent, type.Identifier.Text, attributes, type.AccessModifiers, type.GenericParameters, type.IsOverride, type.BaseTypes, type.MemberBlock);

        public static ContractSyntax WithAttributes(this ContractSyntax contract, params AttributeReferenceSyntax[] attributes)
            => new ContractSyntax(contract.Parent, contract.Identifier.Text, attributes, contract.AccessModifiers, contract.GenericParameters, contract.BaseTypes, contract.MemberBlock);

        public static EnumSyntax WithAttributes(this EnumSyntax e, params AttributeReferenceSyntax[] attributes)
            => new EnumSyntax(e.Parent, e.Identifier.Text, attributes, e.AccessModifiers, e.UnderlyingTypeReference, e.FieldBlock);

        public static FieldSyntax WithAttributes(this FieldSyntax field, params AttributeReferenceSyntax[] attributes)
            => new FieldSyntax(field.Parent, field.Identifier.Text, attributes, field.AccessModifiers, field.FieldType, field.FieldAssignment);

        public static MethodSyntax WithAttributes(this MethodSyntax method, params AttributeReferenceSyntax[] attributes)
            => new MethodSyntax(method.Parent, method.Identifier.Text, attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.IsOverride, method.Body, method.LambdaStatement);

        public static ParameterSyntax WithAttributes(this ParameterSyntax parameter, params AttributeReferenceSyntax[] attributes)
            => new ParameterSyntax(parameter.Parent, attributes, parameter.ParameterType, parameter.Identifier.Text, parameter.Assignment, parameter.HasVariableSizedList);


        // ### Modifiers
        public static TypeSyntax WithAccessModifiers(this TypeSyntax type, params SyntaxToken[] modifiers)
            => new TypeSyntax(type.Parent, type.Identifier.Text, type.Attributes, modifiers, type.GenericParameters, type.IsOverride, type.BaseTypes, type.MemberBlock);

        public static ContractSyntax WithAccessModifiers(this ContractSyntax contract, params SyntaxToken[] modifiers)
            => new ContractSyntax(contract.Parent, contract.Identifier.Text, contract.Attributes, modifiers, contract.GenericParameters, contract.BaseTypes, contract.MemberBlock);

        public static EnumSyntax WithAccessModifiers(this EnumSyntax e, params SyntaxToken[] modifiers)
            => new EnumSyntax(e.Parent, e.Identifier.Text, e.Attributes, modifiers, e.UnderlyingTypeReference, e.FieldBlock);

        public static FieldSyntax WithAccessModifiers(this FieldSyntax field, params SyntaxToken[] modifiers)
            => new FieldSyntax(field.Parent, field.Identifier.Text, field.Attributes, modifiers, field.FieldType, field.FieldAssignment);

        public static MethodSyntax WithAccessModifiers(this MethodSyntax method, params SyntaxToken[] modifiers)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, modifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.IsOverride, method.Body, method.LambdaStatement);


        // Generic parameters
        public static TypeSyntax WithGenericParameters(this TypeSyntax type, params GenericParameterSyntax[] genericParameters)
            => type.WithGenericParameters(new GenericParameterListSyntax(null, genericParameters));

        public static TypeSyntax WithGenericParameters(this TypeSyntax type, GenericParameterListSyntax genericParameters)
            => new TypeSyntax(type.Parent, type.Identifier.Text, type.Attributes, type.AccessModifiers, genericParameters, type.IsOverride, type.BaseTypes, type.MemberBlock);

        public static ContractSyntax WithGenericParameters(this ContractSyntax contract, params GenericParameterSyntax[] genericParameters)
            => contract.WithGenericParameters(new GenericParameterListSyntax(null, genericParameters));

        public static ContractSyntax WithGenericParameters(this ContractSyntax contract, GenericParameterListSyntax genericParameters)
            => new ContractSyntax(contract.Parent, contract.Identifier.Text, contract.Attributes, contract.AccessModifiers, genericParameters, contract.BaseTypes, contract.MemberBlock);

        public static MethodSyntax WithGenericParameters(this MethodSyntax method, params GenericParameterSyntax[] genericParameters)
            => method.WithGenericParameters(new GenericParameterListSyntax(null, genericParameters));

        public static MethodSyntax WithGenericParameters(this MethodSyntax method, GenericParameterListSyntax genericParameters)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, method.AccessModifiers, method.ReturnTypes, genericParameters, method.Parameters, method.IsOverride, method.Body, method.LambdaStatement);


        // Base type
        public static TypeSyntax WithBaseTypes(this TypeSyntax type, params TypeReferenceSyntax[] baseTypes)
            => new TypeSyntax(type.Parent, type.Identifier.Text, type.Attributes, type.AccessModifiers, type.GenericParameters, type.IsOverride, new SeparatedListSyntax<TypeReferenceSyntax>(null, baseTypes, KeywordOrSymbol(SyntaxTokenKind.CommaSymbol)), type.MemberBlock);

        public static ContractSyntax WithBaseTypes(this ContractSyntax contract, params TypeReferenceSyntax[] baseTypes)
            => new ContractSyntax(contract.Parent, contract.Identifier.Text, contract.Attributes, contract.AccessModifiers, contract.GenericParameters, new SeparatedListSyntax<TypeReferenceSyntax>(null, baseTypes, KeywordOrSymbol(SyntaxTokenKind.CommaSymbol)), contract.MemberBlock);

        public static EnumSyntax WithUnderlyingType(this EnumSyntax e, TypeReferenceSyntax underlyingType)
            => new EnumSyntax(e.Parent, e.Identifier.Text, e.Attributes, e.AccessModifiers, underlyingType, e.FieldBlock);


        // Accessor
        public static AccessorSyntax WithAccessorLambda(this AccessorSyntax accessor, StatementSyntax statement)
            => new AccessorSyntax(accessor.Parent, accessor.Identifier.Text, accessor.Attributes, accessor.AccessModifiers, accessor.AccessorType, new[] { new AccessorBodySyntax(null, AccessorOperation.Read, statement) }, accessor.IsOverride);

        public static AccessorSyntax WithAccessorBody(this AccessorSyntax accessor, params AccessorBodySyntax[] accessorBodies)
            => new AccessorSyntax(accessor.Parent, accessor.Identifier.Text, accessor.Attributes, accessor.AccessModifiers, accessor.AccessorType, accessorBodies, accessor.IsOverride);


        // Methods
        public static MethodSyntax WithReturn(this MethodSyntax method, TypeReferenceSyntax returnType)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, method.AccessModifiers, new SeparatedListSyntax<TypeReferenceSyntax>(null, new[] { returnType }, KeywordOrSymbol(SyntaxTokenKind.CommaSymbol)), method.GenericParameters, method.Parameters, method.IsOverride, method.Body, method.LambdaStatement);

        public static MethodSyntax WithParameters(this MethodSyntax method, params ParameterSyntax[] parameters)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, new ParameterListSyntax(null, parameters), method.IsOverride, method.Body, method.LambdaStatement);

        public static MethodSyntax WithBody(this MethodSyntax method, params StatementSyntax[] bodyStatements)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.IsOverride, new BlockSyntax<StatementSyntax>(null, bodyStatements), null);

        public static MethodSyntax WithBody(this MethodSyntax method, BlockSyntax<StatementSyntax> body)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.IsOverride, body, null);

        public static MethodSyntax WithLambda(this MethodSyntax method, StatementSyntax inlineStatement)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.IsOverride, null, new LambdaStatementSyntax(null, inlineStatement));
        
        public static MethodSyntax WithLambda(this MethodSyntax method, LambdaStatementSyntax lambda)
            => new MethodSyntax(method.Parent, method.Identifier.Text, method.Attributes, method.AccessModifiers, method.ReturnTypes, method.GenericParameters, method.Parameters, method.IsOverride, null, lambda);




        // Condition
        public static ConditionStatementSyntax WithStatementBlock(this ConditionStatementSyntax conditionStatement, params StatementSyntax[] statements)
            => new ConditionStatementSyntax(conditionStatement.Parent, conditionStatement.Condition, conditionStatement.IsAlternate, conditionStatement.Alternate, new BlockSyntax<StatementSyntax>(null, statements), null);

        public static ConditionStatementSyntax WithStatementBlock(this ConditionStatementSyntax conditionStatement, BlockSyntax<StatementSyntax> block)
            => new ConditionStatementSyntax(conditionStatement.Parent, conditionStatement.Condition, conditionStatement.IsAlternate, conditionStatement.Alternate, block, null);

        public static ConditionStatementSyntax WithInlineStatement(this ConditionStatementSyntax conditionStatement, StatementSyntax statement)
            => new ConditionStatementSyntax(conditionStatement.Parent, conditionStatement.Condition, conditionStatement.IsAlternate, conditionStatement.Alternate, null, statement);


        public static ConditionStatementSyntax WithAlternate(this ConditionStatementSyntax conditionStatement, ConditionStatementSyntax alternate)
            => new ConditionStatementSyntax(conditionStatement.parent, conditionStatement.Condition, conditionStatement.IsAlternate, alternate, conditionStatement.BlockStatement, alternate);


        // For
        public static ForStatementSyntax WithStatementBlock(this ForStatementSyntax forStatement, params StatementSyntax[] statements)
            => new ForStatementSyntax(forStatement.Parent, forStatement.Variable, forStatement.Condition, forStatement.Increments, new BlockSyntax<StatementSyntax>(null, statements), null);

        public static ForStatementSyntax WithStatementBlock(this ForStatementSyntax forStatement, BlockSyntax<StatementSyntax> block)
            => new ForStatementSyntax(forStatement.Parent, forStatement.Variable, forStatement.Condition, forStatement.Increments, block, null);

        public static ForStatementSyntax WithInlineStatement(this ForStatementSyntax forStatement, StatementSyntax statement)
            => new ForStatementSyntax(statement.Parent, forStatement.Variable, forStatement.Condition, forStatement.Increments, null, statement);


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

        public static MethodSyntax WithMethod<T>(this T node, string identifier) where T : SyntaxNode, IMemberSyntaxContainer
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
