using LumaSharp.Compiler.AST.Visitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumaSharp.Compiler.AST
{
    public sealed class AssignExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax left;
        private readonly SyntaxToken assign;
        private readonly ExpressionSyntax right;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return left.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return right.EndToken; }
        }

        public ExpressionSyntax Left
        {
            get { return left; }
        }

        public ExpressionSyntax Right
        {
            get { return right; }
        }

        public SyntaxToken Assign
        {
            get { return assign; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return left;
                yield return right;
            }
        }

        // Constructor
        public AssignExpressionSyntax(ExpressionSyntax left, SyntaxToken assign,  ExpressionSyntax right)
        {
            // Check null
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            // Check kind
            if (assign.IsAssign == false)
                throw new ArgumentException(nameof(assign) + " must be a valid assign token");

            this.left = left;
            this.assign = assign;
            this.right = right;

            // Set parent
            left.parent = this;
            right.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitAssignExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitAssignExpression(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Write left
            left.GetSourceText(writer);

            // Write assign
            assign.GetSourceText(writer);

            // Write right
            right.GetSourceText(writer);
        }
    }
}
