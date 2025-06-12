using LumaSharp.Compiler.Parser;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.AST
{
    public sealed class SyntaxTree : SyntaxNode
    {
        // Private
        private List<SyntaxNode> rootElements = null;
        private CompileReport report = new CompileReport();

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                if (HasRootElements == true && RootElementCount > 0)
                    return rootElements[0].StartToken;

                return SyntaxToken.Invalid;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                if (HasRootElements == true && RootElementCount > 0)
                    return rootElements[rootElements.Count - 1].EndToken;

                return SyntaxToken.Invalid;
            }
        }

        public int RootElementCount
        {
            get { return HasRootElements ? rootElements.Count : 0; }
        }

        public int RootMemberCount
        {
            get { return HasRootMembers ? DescendantsOfType<MemberSyntax>().Count() : 0; }
        }

        public int NamespaceMemberCount
        {
            get { return HasNamespaceMembers ? DescendantsOfType<NamespaceSyntax>().Count() : 0; }
        }

        public bool HasRootElements
        {
            get { return rootElements != null; }
        }

        public bool HasRootMembers
        {
            get { return DescendantsOfType<MemberSyntax>().Any(); }
        }

        public bool HasNamespaceMembers
        {
            get { return DescendantsOfType<NamespaceSyntax>().Any(); }
        }

        public ICompileReportProvider Report
        {
            get { return report; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return rootElements; }
        }

        // Constructor
        internal SyntaxTree()
        {
            this.rootElements = new List<SyntaxNode>();
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public static SyntaxTree Create(params SyntaxNode[] rootNodes)
        {
            // Create new tree
            SyntaxTree result = new SyntaxTree();
            result.rootElements.AddRange(rootNodes);
            return result;
        }

        public static SyntaxTree Parse(InputSource source)
        {
            throw new NotImplementedException();
            // Create parser
            //ParserContext context = new ParserContext(source);

            //// Parse syntax tree
            //return context.ParseCompilationUnit();
        }

        public static SyntaxTree ParseStatement()
        {
            throw new NotImplementedException();
        }

        public static ExpressionSyntax ParseExpression(InputSource source)
        {
            return new ASTParser(new TokenParser(new TextView(source.Reader)).GetEnumerator(), new CompileReport())
                .ParseExpression();

            //// Create parser
            //ParserContext context = new ParserContext(source);
            
            //// Parse expression only
            //return context.ParseExpression();
        }
    }
}
