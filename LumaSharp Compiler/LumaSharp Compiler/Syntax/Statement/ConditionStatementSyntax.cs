
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class ConditionStatementSyntax : StatementSyntax
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly ExpressionSyntax condition;
        private readonly StatementSyntax statement;

        private readonly ConditionStatementSyntax alternate;

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

        public ExpressionSyntax Condition
        {
            get { return condition; }
        }

        public StatementSyntax Statement
        {
            get { return statement; }
        }

        public ConditionStatementSyntax Alternate
        {
            get { return alternate; }
        }

        public bool HasCondition
        {
            get { return condition != null; }
        }

        public bool IsAlternate
        {
            get { return keyword.Kind != SyntaxTokenKind.IfKeyword; }
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
        internal ConditionStatementSyntax(SyntaxToken keyword, ExpressionSyntax condition, ConditionStatementSyntax alternate, StatementSyntax statement)
        {
            // Check kind
            if (keyword.Kind != SyntaxTokenKind.IfKeyword && keyword.Kind != SyntaxTokenKind.ElifKeyword && keyword.Kind != SyntaxTokenKind.ElseKeyword)
                throw new ArgumentException(nameof(keyword) + " must be a valid conditional kind");

            // Check null
            if(statement == null)
                throw new ArgumentNullException(nameof(statement));

            // Check alternate
            if (alternate != null && alternate.keyword.Kind == SyntaxTokenKind.IfKeyword)
                throw new ArgumentException(nameof(alternate) + " must be a valid alternate conditional kind");

            // Condition
            this.keyword = keyword;
            this.condition = condition;
            this.alternate = alternate;
            this.statement = statement;

            // Set parent
            if (condition != null) condition.parent = this;
            if (alternate != null) alternate.parent = this;
            statement.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitConditionStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitConditionStatement(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Keyword
            keyword.GetSourceText(writer);

            // Check for condition
            if(HasCondition == true)
            {
                // Condition
                condition.GetSourceText(writer);
            }

            // Write statement
            statement.GetSourceText(writer);

            // Write alternate
            if(HasAlternate == true)
            {
                alternate.GetSourceText(writer);
            }
        }
    }
}
