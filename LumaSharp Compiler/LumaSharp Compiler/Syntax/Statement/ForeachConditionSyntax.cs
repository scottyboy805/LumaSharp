
//namespace LumaSharp.Compiler.AST
//{
//    public sealed class ForeachConditionSyntax : SyntaxNode
//    {
//        // Private
//        private readonly TypeReferenceSyntax typeReference;
//        private readonly SyntaxToken identifier;
//        private readonly SyntaxToken inKeyword;
//        private readonly ExpressionSyntax expression;

//        // Properties
//        public override SyntaxToken StartToken
//        {
//            get { return typeReference.StartToken; }
//        }

//        public override SyntaxToken EndToken
//        {
//            get { return expression.EndToken; }
//        }

//        public TypeReferenceSyntax TypeReference
//        {
//            get { return typeReference; }
//        }

//        public SyntaxToken Identifier
//        {
//            get { return identifier; }
//        }

//        public SyntaxToken In
//        {
//            get { return inKeyword; }
//        }

//        public ExpressionSyntax Expression
//        {
//            get { return expression; }
//        }

//        internal override IEnumerable<SyntaxNode> Descendants
//        {
//            get
//            {
//                yield return typeReference;
//                yield return expression;
//            }
//        }

//        // Constructor
//        internal ForeachConditionSyntax(SyntaxNode parent, LumaSharpParser.ForeachConditionContext foreachCondition)
//            : base(parent)
//        {
//            // Type reference
//            this.typeReference = new TypeReferenceSyntax(this, null, foreachCondition.typeReference());

//            // Expression
//            this.expression = ExpressionSyntax.Any(this, foreachCondition.expression());

//            // Tokens
//            this.identifier = new SyntaxToken(SyntaxTokenKind.Identifier, foreachCondition.IDENTIFIER());
//            this.inKeyword = new SyntaxToken(SyntaxTokenKind.InKeyword, foreachCondition.IN());
//        }

//        // Methods
//        public override void GetSourceText(TextWriter writer)
//        {
//            // Type reference
//            typeReference.GetSourceText(writer);

//            // Identifier
//            identifier.GetSourceText(writer);

//            // In
//            inKeyword.GetSourceText(writer);

//            // Expression
//            expression.GetSourceText(writer);
//        }
//    }
//}
