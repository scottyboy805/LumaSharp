
using LumaSharp_Compiler.AST.Statement;

namespace LumaSharp_Compiler.AST.Factory
{
    public static class Syntax
    {
        // Methods
        #region Members
        public static NamespaceSyntax Namespace(string identifier) => new NamespaceSyntax(identifier);
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
        public static VariableDeclarationStatementSyntax Variable(TypeReferenceSyntax variableType, string identifier) => new VariableDeclarationStatementSyntax(variableType, identifier);
        public static AssignStatementSyntax Assign(ExpressionSyntax left, ExpressionSyntax right) => new AssignStatementSyntax(left, right);
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
