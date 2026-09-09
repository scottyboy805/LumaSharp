
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{ 
    public static partial class Syntax
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

        #region Trivia
        public static SyntaxTrivia Trivia(SyntaxTriviaKind kind, string text)
        {
            return new SyntaxTrivia(kind, text, default);
        }

        internal static SyntaxTrivia Trivia(SyntaxTriviaKind kind, string text, SyntaxSpan span)
        {
            return new SyntaxTrivia(kind, text, span);
        }
        #endregion

        #region Transformation
        public static T NormalizeWhitespace<T>(this T node) where T : SyntaxNode
        {
            // Create rewriter
            WhitespaceRewriter rewriter = new WhitespaceRewriter();

            // Try to rewrite this node
            return rewriter.DefaultVisit(node);
        }

        public static T ReplaceSyntax<T, J>(this T node, J current, J replacement) where T : SyntaxNode where J : SyntaxNode
        {
            // Try to replace syntax
            return new SyntaxReplacer.SyntaxReplace(current, replacement)
                .DefaultVisit(node);
        }

        public static T ReplaceToken<T>(this T node, SyntaxToken current, SyntaxToken replacement) where T : SyntaxNode
        {
            // Try to replace token
            return new SyntaxReplacer.TokenReplace(current, replacement)
                .DefaultVisit(node);
        }

        public static T WithLeadingTrivia<T>(this T node, SyntaxTrivia trivia) where T : SyntaxNode
        {
            // Get start token
            SyntaxToken startToken = node.StartToken;

            // Get token with trivia
            SyntaxToken replaceToken = startToken.WithLeadingTrivia(trivia);

            // Replace the token
            return node.ReplaceToken(startToken, replaceToken);
        }

        public static T WithTrailingTrivia<T>(this T node, SyntaxTrivia trivia) where T : SyntaxNode
        {
            // Get end token
            SyntaxToken endToken = node.EndToken;

            // Get token with trivia
            SyntaxToken replaceToken = endToken.WithTrailingTrivia(trivia);

            // Replace the token
            return node.ReplaceToken(endToken, replaceToken);
        }
        #endregion

        #region Members
        internal static SeparatedTokenList NamespaceName(string[] identifiers) => new SeparatedTokenList(SyntaxTokenKind.ColonSymbol, identifiers.Select(n => Identifier(n)), SyntaxTokenKind.Identifier, true);

        
        
        public static NamespaceSyntax Namespace(params SyntaxToken[] identifiers) => new NamespaceSyntax(new(SyntaxTokenKind.ColonSymbol, identifiers, SyntaxTokenKind.Identifier));
        
        
        
        
        public static EnumFieldSyntax EnumField(SyntaxToken identifier, VariableAssignmentExpressionSyntax enumAssignment = null) => new EnumFieldSyntax(identifier, enumAssignment);
        public static AccessorBodySyntax AccessorLambda(StatementSyntax statement) => new AccessorBodySyntax(Token(SyntaxTokenKind.GetKeyword), statement);
        public static AccessorBodySyntax AccessorRead(StatementSyntax statement) => new AccessorBodySyntax(Token(SyntaxTokenKind.GetKeyword), statement);
        public static AccessorBodySyntax AccessorWrite(StatementSyntax statement) => new AccessorBodySyntax(Token(SyntaxTokenKind.SetKeyword), statement);
        public static AccessorSyntax Accessor(SyntaxToken identifier, TypeReferenceSyntax accessorType) => new AccessorSyntax(identifier, null, null, accessorType, null, null, null);

        
        #endregion

        #region CommonUse
        public static ParentTypeReferenceSyntax ParentTypeReference(SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null) => new ParentTypeReferenceSyntax(identifier, genericArguments);
        public static TypeReferenceSyntax TypeReference(ReservedType primitive, ArrayParametersSyntax arrayParameters = null) => new TypeReferenceSyntax(primitive, arrayParameters);
        public static TypeReferenceSyntax TypeReference(MemberSyntax member) => new TypeReferenceSyntax(member);
        public static TypeReferenceSyntax TypeReference(SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, null, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(NamespaceName(namespaceName), null, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(ParentTypeReferenceSyntax[] parentTypes, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, parentTypes, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(ParentTypeReferenceSyntax parentType, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(null, new[] { parentType }, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, ParentTypeReferenceSyntax[] parentTypes, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(NamespaceName(namespaceName), parentTypes, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        public static TypeReferenceSyntax TypeReference(string[] namespaceName, ParentTypeReferenceSyntax parentType, SyntaxToken identifier, GenericArgumentListSyntax genericArguments = null, int? arrayRank = null) => new TypeReferenceSyntax(NamespaceName(namespaceName), new[] { parentType }, identifier, genericArguments, arrayRank != null ? new ArrayParametersSyntax(arrayRank.Value) : null);
        
        
        
        
        #endregion

        #region Statements
        
        
        
        
        
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
        
        

        

        // ### Attributes
        
        

        

        

        
        


        // ### Modifiers
        

        

        

        

        

        // Generic parameters
        

        

        

        


        // Base type
        
        

        


        // Accessor
        public static AccessorSyntax WithAccessorLambda(this AccessorSyntax accessor, ExpressionSyntax expression)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.Modifiers, accessor.AccessorType, accessor.Override, null, new(expression));


        public static AccessorSyntax WithAccessorLambda(this AccessorSyntax accessor, StatementSyntax statement)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.Modifiers, accessor.AccessorType, accessor.Override, new[] { new AccessorBodySyntax(Token(SyntaxTokenKind.GetKeyword), statement) }, null);

        public static AccessorSyntax WithAccessorBody(this AccessorSyntax accessor, params AccessorBodySyntax[] accessorBodies)
            => new AccessorSyntax(accessor.Identifier, accessor.Attributes, accessor.Modifiers, accessor.AccessorType, accessor.Override, accessorBodies, null);


        // Methods
        
        


        // For
        
        #endregion

        private static void CheckSyntaxNode(object container)
        {
            // Check for syntax node
            if ((container is SyntaxNode) == false)
                throw new ArgumentException("Container must be a syntax node");
        }
    }
}
