
namespace LumaSharp.Compiler.AST
{
    public class BlockSyntax<T> : SyntaxNode, IMemberSyntaxContainer where T : SyntaxNode
    {
        // Private
        private readonly SyntaxToken lBlock;
        private readonly SyntaxToken rBlock;
        private readonly List<T> elements = new();

        // Properties
        public override SyntaxToken StartToken
        {
            get { return lBlock; }
        }

        public override SyntaxToken EndToken
        {
            get { return rBlock; }
        }

        public SyntaxToken LBlock
        {
            get { return lBlock; }
        }

        public SyntaxToken RBlock
        {
            get { return rBlock; }
        }

        public IReadOnlyList<T> Elements
        {
            get { return elements; }
        }

        public int ElementCount
        {
            get { return HasElements ? elements.Count : 0; }
        }

        public bool HasElements
        {
            get { return elements != null && elements.Count > 0; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return elements; }
        }

        // Constructor
        internal BlockSyntax(SyntaxNode parent, IEnumerable<T> elements)
        {
            this.lBlock = Syntax.Token(SyntaxTokenKind.LBlockSymbol);
            this.rBlock = Syntax.Token(SyntaxTokenKind.RBlockSymbol);

            if (elements != null)
                this.elements.AddRange(elements);
        }

        internal BlockSyntax(SyntaxToken lBlock, IEnumerable<T> elements, SyntaxToken rBlock)
        {
            // Check for kind
            if(lBlock.Kind != SyntaxTokenKind.LBlockSymbol)
                throw new ArgumentException(nameof(lBlock) + " must be of kind: " + SyntaxTokenKind.LBlockSymbol);

            if(rBlock.Kind != SyntaxTokenKind.RBlockSymbol)
                throw new ArgumentException(nameof(rBlock) + " must be of kind: " + SyntaxTokenKind.RBlockSymbol);

            this.lBlock = lBlock;
            this.rBlock = rBlock;

            if (elements != null)
                this.elements.AddRange(elements);
        }

        //internal BlockSyntax(SyntaxNode node, LumaSharpParser.RootMemberBlockContext rootMemberBlock)
        //    : base(node)
        //{
        //    this.lBlock = new SyntaxToken(SyntaxTokenKind.LBlockSymbol, rootMemberBlock.LBLOCK());
        //    this.rBlock = new SyntaxToken(SyntaxTokenKind.RBlockSymbol, rootMemberBlock.RBLOCK());

        //    // Get all members
        //    LumaSharpParser.RootMemberContext[] members = rootMemberBlock.rootMember();

        //    if (members.Length > 0)
        //    {
        //        for (int i = 0; i < members.Length; i++)
        //        {
        //            this.elements.Add(MemberSyntax.RootMember(this, members[i]) as T);
        //        }
        //    }
        //}

        //internal BlockSyntax(SyntaxNode parent, LumaSharpParser.MemberBlockContext memberBlock)
        //    : base(parent)
        //{
        //    this.lBlock = new SyntaxToken(SyntaxTokenKind.LBlockSymbol, memberBlock.LBLOCK());
        //    this.rBlock = new SyntaxToken(SyntaxTokenKind.RBlockSymbol, memberBlock.RBLOCK());

        //    // Get all members
        //    LumaSharpParser.MemberDeclarationContext[] members = memberBlock.memberDeclaration();

        //    if (members.Length > 0)
        //    {
        //        for (int i = 0; i < members.Length; i++)
        //        {
        //            this.elements.Add(MemberSyntax.Member(this, members[i]) as T);
        //        }
        //    }
        //}


        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write start
            lBlock.GetSourceText(writer);

            // Write all elements
            for(int i = 0; i < elements.Count; i++)
            {
                elements[i].GetSourceText(writer);
            }

            // Write end
            rBlock.GetSourceText(writer);
        }

        void IMemberSyntaxContainer.AddMember(MemberSyntax member)
        {
            if (member is T)
                elements.Add(member as T);
        }
    }
}
