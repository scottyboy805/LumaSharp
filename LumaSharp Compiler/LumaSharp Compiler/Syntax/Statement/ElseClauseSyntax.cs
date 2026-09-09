using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ElseClauseSyntax : SyntaxNode
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly StatementSyntax statement;      // Can be either IfStatement or inline statement or scope

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return statement.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return statement;
            }
        }

        // Constructor
        internal ElseClauseSyntax(SyntaxToken keyword, StatementSyntax statement)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.ElseKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.ElseKeyword);

            // Check null
            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            this.keyword = keyword;
            this.statement = statement;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Write statement
            statement.GetSourceText(writer);
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static ElseClauseSyntax Else(StatementSyntax statement = null) 
            => new (Token(SyntaxTokenKind.ElseKeyword), statement ?? new EmptyStatementSyntax());

        public static ElseClauseSyntax Else(SyntaxToken keyword, StatementSyntax statement = null)
            => new(keyword, statement ?? new EmptyStatementSyntax());


        public static ElseClauseSyntax WithStatement(this ElseClauseSyntax _else, StatementSyntax statement)
            => new ElseClauseSyntax(_else.Keyword, statement);
    }
}
