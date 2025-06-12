using LumaSharp.Compiler.AST.Visitor;
using System.Collections;

namespace LumaSharp.Compiler.AST
{
    public sealed class StatementBlockSyntax : StatementSyntax, IEnumerable<StatementSyntax>
    {
        // Private
        private readonly SyntaxToken lBlock;
        private readonly SyntaxToken rBlock;
        private readonly List<StatementSyntax> statements = new();

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
            get { return statements.Count; }
        }

        public IEnumerable<StatementSyntax> Statements
        {
            get { return statements; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return statements; }
        }

        // Constructor
        internal StatementBlockSyntax(IEnumerable<StatementSyntax> statements)
            : this(
                  Syntax.Token(SyntaxTokenKind.LBlockSymbol),
                  statements,
                  Syntax.Token(SyntaxTokenKind.RBlockSymbol))
        {
        }

        internal StatementBlockSyntax(SyntaxToken lBlock, IEnumerable<StatementSyntax> statements, SyntaxToken rBlock)
            : base()
        {
            // Check kind
            if (lBlock.Kind != SyntaxTokenKind.LBlockSymbol)
                throw new ArgumentException(nameof(lBlock) + " must be of kind: " + SyntaxTokenKind.LBlockSymbol);

            if (rBlock.Kind != SyntaxTokenKind.RBlockSymbol)
                throw new ArgumentException(nameof(rBlock) + " must be of kind: " + SyntaxTokenKind.RBlockSymbol);

            this.lBlock = lBlock;
            this.rBlock = rBlock;

            // Check for any
            if (statements != null)
            {
                this.statements.AddRange(statements.Where(s => s != null));

                // Set parent
                foreach (StatementSyntax s in this.statements)
                    s.parent = this;
            }
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitStatementBlock(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitStatementBlock(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // LBlock
            lBlock.GetSourceText(writer);

            // Statements
            foreach(StatementSyntax statement in statements)
                statement.GetSourceText();

            // RBlock
            rBlock.GetSourceText(writer);
        }

        public IEnumerator<StatementSyntax> GetEnumerator()
        {
            return statements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
