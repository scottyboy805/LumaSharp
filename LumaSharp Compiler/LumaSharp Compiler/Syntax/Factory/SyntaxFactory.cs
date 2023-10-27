
using LumaSharp_Compiler.AST.Expression;
using LumaSharp_Compiler.AST.Statement;

namespace LumaSharp_Compiler.AST.Factory
{
    public static class Syntax
    {
        // Methods
        #region Members
        public static ImportSyntax Import(params string[] identifiers) => new ImportSyntax(identifiers);
        public static ImportSyntax ImportAlias(string alias, TypeReferenceSyntax aliasType, params string[] identifiers) => new ImportSyntax(alias, aliasType, identifiers);
        public static NamespaceSyntax Namespace(params string[] identifiers) => new NamespaceSyntax(identifiers);
        public static TypeSyntax Type(string identifier) => new TypeSyntax(identifier);
        public static ContractSyntax Contract(string identifier) => new ContractSyntax(identifier);
        public static EnumSyntax Enum(string identifier) => new EnumSyntax(identifier);
        public static FieldSyntax Field(string identifier, TypeReferenceSyntax fieldType) => new FieldSyntax(identifier, fieldType);
        public static AccessorSyntax Accessor(string identifier, TypeReferenceSyntax accessorType) => new AccessorSyntax(identifier, accessorType);
        public static MethodSyntax Method(string identifier, TypeReferenceSyntax returnType = null) => new MethodSyntax(identifier, returnType);
        #endregion

        #region CommonUse
        public static TypeReferenceSyntax TypeReference(PrimitiveType primitive) => new TypeReferenceSyntax(primitive);
        public static TypeReferenceSyntax TypeReference(string identifier) => new TypeReferenceSyntax(identifier);
        public static GenericParameterSyntax GenericParameter(string identifier, params TypeReferenceSyntax[] constrainTypes) => new GenericParameterSyntax(identifier, constrainTypes);
        public static ParameterSyntax Parameter(TypeReferenceSyntax parameterType, string identifier, bool variableSizedList = false) => new ParameterSyntax(parameterType, identifier, variableSizedList);
        #endregion

        #region Statements
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, params string[] identifiers) => new VariableDeclarationStatementSyntax(variableType, identifiers, null);
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, string identifier, ExpressionSyntax assignExpression) => new VariableDeclarationStatementSyntax(variableType, new string[] { identifier }, new ExpressionSyntax[] { assignExpression });
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, string[] identifiers, ExpressionSyntax[] assignExpressions) => new VariableDeclarationStatementSyntax(variableType, identifiers, assignExpressions);
        public static AssignStatementSyntax Assign(ExpressionSyntax left, ExpressionSyntax right) => new AssignStatementSyntax(left, right);
        public static BreakStatementSyntax Break() => new BreakStatementSyntax();
        public static ConditionStatementSyntax Condition(ExpressionSyntax condition = null) => new ConditionStatementSyntax(condition);
        public static ContinueStatementSyntax Continue() => new ContinueStatementSyntax();
        public static ForeachStatementSyntax Foreach(TypeReferenceSyntax variableType, string identifier, ExpressionSyntax iterateExpression) => new ForeachStatementSyntax(variableType, identifier, iterateExpression);
        public static ForStatementSyntax For(VariableDeclarationStatementSyntax variable, ExpressionSyntax condition, params ExpressionSyntax[] increment) => new ForStatementSyntax(variable, condition, increment);
        public static ReturnStatementSyntax Return(ExpressionSyntax expression = null) => new ReturnStatementSyntax(expression);
        #endregion

        #region Expressions
        public static LiteralExpressionSyntax Literal(long val) => new LiteralExpressionSyntax(new SyntaxToken(val.ToString()));
        public static LiteralExpressionSyntax Literal(double val) => new LiteralExpressionSyntax(new SyntaxToken(val.ToString()));
        public static LiteralExpressionSyntax Literal(string val) => new LiteralExpressionSyntax(new SyntaxToken(val));
        public static LiteralExpressionSyntax Literal(bool val) => new LiteralExpressionSyntax(new SyntaxToken(val.ToString().ToLower()));
        public static VariableReferenceExpressionSyntax VariableReference(string identifier) => new VariableReferenceExpressionSyntax(identifier);

        public static BaseExpressionSyntax Base() => new BaseExpressionSyntax();
        public static ThisExpressionSyntax This() => new ThisExpressionSyntax();
        public static TypeExpressionSyntax TypeOp(TypeReferenceSyntax typeReference) => new TypeExpressionSyntax(typeReference);
        public static SizeExpressionSyntax SizeOp(TypeReferenceSyntax typeReference) => new SizeExpressionSyntax(typeReference);
        public static FieldAccessorReferenceExpressionSyntax FieldReference(string identifier, ExpressionSyntax accessExpression) => new FieldAccessorReferenceExpressionSyntax(identifier, accessExpression);
        public static MethodInvokeExpressionSyntax MethodInvoke(string identifier, ExpressionSyntax accessExpression) => new MethodInvokeExpressionSyntax(identifier, accessExpression);
        public static NewExpressionSyntax New(TypeReferenceSyntax newType, bool stackAlloc) => new NewExpressionSyntax(newType, stackAlloc);
        public static TernaryExpressionSyntax Ternary(ExpressionSyntax condition, ExpressionSyntax trueExpression, ExpressionSyntax falseExpression) => new TernaryExpressionSyntax(condition, trueExpression, falseExpression);
        public static BinaryExpressionSyntax Binary(ExpressionSyntax left, BinaryOperation op, ExpressionSyntax right) => new BinaryExpressionSyntax(left, op, right);
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

        public static T WithAttributes<T>(this T node, params AttributeSyntax[] attributes) where T : MemberSyntax
        {
            node.Attributes = attributes;
            return node;
        }

        public static T WithAccessModifiers<T>(this T node, params string[] modifiers) where T : MemberSyntax
        {
            node.AccessModifiers = modifiers.Select(m => new SyntaxToken(m)).ToArray();
            return node;
        }

        public static MethodSyntax WithParameters(this MethodSyntax method, params ParameterSyntax[] parameters)
        {
            method.Parameters = new ParameterListSyntax(parameters);
            return method;
        }


        public static MethodSyntax WithStatements(this MethodSyntax method, params StatementSyntax[] statements)
        {
            method.Body = new BlockSyntax<StatementSyntax>(statements);
            return method;
        }


        public static ConditionStatementSyntax WithStatements(this ConditionStatementSyntax condition, params StatementSyntax[] statements)
        {
            condition.BlockStatement = new BlockSyntax<StatementSyntax>(statements);
            return condition;
        }

        public static ConditionStatementSyntax WithInlineStatement(this ConditionStatementSyntax condition, StatementSyntax statement)
        {
            condition.InlineStatement = statement;
            return condition;
        }

        public static ConditionStatementSyntax WithAlternate(this ConditionStatementSyntax condition, ConditionStatementSyntax alternate = null)
        {
            condition.Alternate = alternate;
            alternate.MakeAlternate();
            return condition;
        }

        public static ForeachStatementSyntax WithStatements(this ForeachStatementSyntax foreachLoop, params StatementSyntax[] statements)
        {
            foreachLoop.BlockStatement = new BlockSyntax<StatementSyntax>(statements);
            return foreachLoop;
        }

        public static ForeachStatementSyntax WithInlineStatement(this ForeachStatementSyntax foreachLoop, StatementSyntax statement)
        {
            foreachLoop.InlineStatement = statement;
            return foreachLoop;
        }

        public static ForStatementSyntax WithStatements(this ForStatementSyntax forLoop, params StatementSyntax[] statements)
        {
            forLoop.BlockStatement = new BlockSyntax<StatementSyntax>(statements);
            return forLoop;
        }

        public static ForStatementSyntax WithInlineStatement(this ForStatementSyntax forLoop, StatementSyntax statement)
        {
            forLoop.InlineStatement = statement;
            return forLoop;
        }


        public static MethodInvokeExpressionSyntax WithGenericArguments(this MethodInvokeExpressionSyntax invoke, params TypeReferenceSyntax[] genericArguments)
        {
            invoke.GenericArguments = genericArguments;
            return invoke;
        }

        public static MethodInvokeExpressionSyntax WithArguments(this MethodInvokeExpressionSyntax invoke, params ExpressionSyntax[] arguments)
        {
            invoke.Arguments = arguments;
            return invoke;
        }


        public static NewExpressionSyntax WithArguments(this NewExpressionSyntax newExpr, params ExpressionSyntax[] arguments)
        {
            newExpr.ArgumentExpressions = arguments;
            return newExpr;
        }
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


        //#region Members

        //public static MethodSyntax WithMethod<T>(this T node, string identifier) where T : SyntaxNode, IMemberSyntaxContainer
        //{
        //    // Create method
        //    MethodSyntax method = new MethodSyntax(node.SyntaxTree, node, identifier);

        //    // Add member
        //    node.AddMember(method);
        //    return method;
        //}

        //public static MethodSyntax WithParameters(this MethodSyntax method, params ParameterSyntax[] parameters)
        //{
        //    // Create parameters
        //    ParameterListSyntax list = new ParameterListSyntax(method.SyntaxTree, method, parameters);

        //    // Update param list
        //    method.Parameters = list;
        //    return method;
        //}

        //public static MethodSyntax WithReturn(this MethodSyntax method, PrimitiveType returnType)
        //{
        //    // Create return type
        //    TypeReferenceSyntax reference = new TypeReferenceSyntax(method.SyntaxTree, method, returnType);

        //    // Update return type
        //    method.ReturnType = reference;
        //    return method;
        //}
        //#endregion

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
