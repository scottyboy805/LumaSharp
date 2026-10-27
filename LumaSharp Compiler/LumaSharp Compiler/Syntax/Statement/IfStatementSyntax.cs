using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly SyntaxToken lParen;
        private readonly SyntaxToken rParen;
        private readonly ExpressionSyntax condition;
        private readonly StatementSyntax statement;
        private readonly ElseClauseSyntax alternate;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for alternate
                if (HasAlternate == true)
                    return alternate.EndToken;

                // Get statement
                return statement.EndToken;
            }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }        

        public SyntaxToken LParen
        {
            get { return lParen; }
        }

        public SyntaxToken RParen
        {
            get { return rParen; }
        }
        
        public ExpressionSyntax Condition
        {
            get { return condition; }
        }

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        public ElseClauseSyntax Alternate
        {
            get { return alternate; }
        }

        public bool HasAlternate
        {
            get { return alternate != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Check for condition
                if (condition != null)
                    yield return condition;

                // Statement
                yield return statement;

                // Alternate
                if(alternate != null) 
                    yield return alternate;
            }
        }

        // Constructor
        internal IfStatementSyntax(SyntaxToken keyword, SyntaxToken lParen, ExpressionSyntax condition, SyntaxToken rParen, StatementSyntax statement, ElseClauseSyntax alternate)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.IfKeyword)
                throw new ArgumentException(nameof(keyword) + " must be of kind: " + SyntaxTokenKind.IfKeyword);

            if(lParen.Kind != SyntaxTokenKind.LParenSymbol)
                throw new ArgumentException(nameof(lParen) + " must be of kind: " + SyntaxTokenKind.LParenSymbol);

            if(rParen.Kind != SyntaxTokenKind.RParenSymbol)
                throw new ArgumentException(nameof(rParen) + " must be of kind: " + SyntaxTokenKind.RParenSymbol);

            // Check null
            if(condition == null)
                throw new ArgumentNullException(nameof(condition));

            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            // Condition
            this.keyword = keyword;
            this.lParen = lParen;
            this.condition = condition;
            this.rParen = rParen;
            this.statement = statement;
            this.alternate = alternate;
            
            // Set parent
            if (condition != null) this.condition.parent = this;
            if (alternate != null) this.alternate.parent = this;
            this.statement.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIfStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // LParen
            lParen.GetSourceText(writer);

            // Condition
            condition.GetSourceText(writer);

            // RParen
            rParen.GetSourceText(writer);

            // Write statement
            statement.GetSourceText(writer);

            // Write alternate
            if(HasAlternate == true)
            {
                alternate.GetSourceText(writer);
            }
        }
    }

    public static partial class Syntax
    {
        // Methods
        public static IfStatementSyntax Condition(ExpressionSyntax condition) 
            => new(Token(SyntaxTokenKind.IfKeyword), Token(SyntaxTokenKind.LParenSymbol), condition, Token(SyntaxTokenKind.RParenSymbol), new EmptyStatementSyntax(), null);

        public static IfStatementSyntax If(SyntaxToken keyword, SyntaxToken lParen, ExpressionSyntax condition, SyntaxToken rParen, StatementSyntax statement, ElseClauseSyntax alternate = null)
            => new(keyword, lParen, condition, rParen, statement, alternate);


        public static IfStatementSyntax WithStatementBlock(this IfStatementSyntax ifStatement, params StatementSyntax[] statements)
            => new (ifStatement.Keyword, ifStatement.LParen, ifStatement.Condition, ifStatement.RParen, new StatementBlockSyntax(statements), ifStatement.Alternate);

        public static IfStatementSyntax WithStatementBlock(this IfStatementSyntax ifStatement, StatementBlockSyntax block)
            => new (ifStatement.Keyword, ifStatement.LParen, ifStatement.Condition, ifStatement.RParen, block, ifStatement.Alternate);

        public static IfStatementSyntax WithInlineStatement(this IfStatementSyntax ifStatement, StatementSyntax statement)
            => new (ifStatement.Keyword, ifStatement.LParen, ifStatement.Condition, ifStatement.RParen, statement, ifStatement.Alternate);

        public static IfStatementSyntax WithAlternate(this IfStatementSyntax ifStatement, ElseClauseSyntax alternate)
            => new (ifStatement.Keyword, ifStatement.LParen, ifStatement.Condition, ifStatement.RParen, ifStatement.Statement, alternate);

    }
}
