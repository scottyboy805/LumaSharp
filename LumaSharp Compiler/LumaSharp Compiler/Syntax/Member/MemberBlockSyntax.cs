
using LumaSharp.Compiler.AST.Visitor;
using System.Collections;

namespace LumaSharp.Compiler.AST
{
    public sealed class MemberBlockSyntax : SyntaxNode, IEnumerable<MemberSyntax>
    {
        // Private
        private readonly SyntaxToken lBlock;
        private readonly SyntaxToken rBlock;
        private readonly List<MemberSyntax> members = new();

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

        public int Count
        {
            get { return members.Count; }
        }

        public IEnumerable<MemberSyntax> Members
        {
            get { return members; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return members; }
        }

        // Constructor
        internal MemberBlockSyntax(IEnumerable<MemberSyntax> members)
            : this(
                  Syntax.Token(SyntaxTokenKind.LBlockSymbol),
                  members,
                  Syntax.Token(SyntaxTokenKind.RBlockSymbol))
        {
        }

        internal MemberBlockSyntax(SyntaxToken lBlock, IEnumerable<MemberSyntax> members, SyntaxToken rBlock)
        {
            // Check kind
            if (lBlock.Kind != SyntaxTokenKind.LBlockSymbol)
                throw new ArgumentException(nameof(lBlock) + " must be of kind: " + SyntaxTokenKind.LBlockSymbol);

            if (rBlock.Kind != SyntaxTokenKind.RBlockSymbol)
                throw new ArgumentException(nameof(rBlock) + " must be of kind: " + SyntaxTokenKind.RBlockSymbol);

            this.lBlock = lBlock;
            this.rBlock = rBlock;

            // Check for any
            if (members != null)
            {
                this.members.AddRange(members.Where(m => m != null));

                // Set parent
                foreach (MemberSyntax m in this.members)
                    m.parent = this;
            }
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMemberBlock(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitMemberBlock(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // LBlock
            lBlock.GetSourceText(writer);

            // Members
            foreach (MemberSyntax member in members)
                member.GetSourceText(writer);

            // RBlock
            rBlock.GetSourceText(writer);
        }

        public IEnumerator<MemberSyntax> GetEnumerator()
        {
            return members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
