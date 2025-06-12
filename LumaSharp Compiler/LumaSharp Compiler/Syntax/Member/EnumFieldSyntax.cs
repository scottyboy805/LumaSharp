
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class EnumFieldSyntax : MemberSyntax
    {
        // Private
        private readonly VariableAssignmentExpressionSyntax fieldAssignment;

        // Properties
        public override SyntaxToken EndToken
        {
            get
            {
                if(HasFieldAssignment == true)
                    return fieldAssignment.EndToken;

                return identifier;
            }
        }

        public VariableAssignmentExpressionSyntax FieldAssignment
        {
            get { return fieldAssignment; }
        }

        public bool HasFieldAssignment
        {
            get { return fieldAssignment != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                // Field assignment
                if (HasFieldAssignment == true)
                    yield return fieldAssignment;
            }
        }

        // Constructor
        internal EnumFieldSyntax(SyntaxToken identifier, VariableAssignmentExpressionSyntax fieldAssignment)
            : base(identifier, null, null)
        {
            this.fieldAssignment = fieldAssignment;

            // Set parent
            if (fieldAssignment != null) fieldAssignment.parent = this;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEnumField(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEnumField(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Identifier
            identifier.GetSourceText(writer);

            // Assign
            if (HasFieldAssignment == true)
            {
                // Assign expression
                fieldAssignment.GetSourceText(writer);
            }
        }
    }
}
