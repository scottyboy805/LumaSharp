
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class MethodInvokeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax accessExpression;
        private readonly GenericArgumentListSyntax genericArgumentList;
        private readonly ArgumentListSyntax argumentList;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return accessExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get { return argumentList.EndToken; }
        }

        public SyntaxToken Identifier
        {
            get
            {
                // Check for member
                if (accessExpression is MemberAccessExpressionSyntax memberAccess)
                    return memberAccess.Identifier;

                // Check for variable
                if(accessExpression is VariableReferenceExpressionSyntax variableReference)
                    return variableReference.Identifier;

                throw new InvalidOperationException("Access expression is not valid");
            }
        }

        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public GenericArgumentListSyntax GenericArgumentList
        {
            get { return genericArgumentList; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return argumentList; }
        }

        public int GenericArgumentCount
        {
            get { return HasGenericArguments ? genericArgumentList.Count : 0; }
        }

        public int ArgumentCount
        {
            get { return argumentList.Count; }
        }

        public bool HasGenericArguments
        {
            get { return genericArgumentList != null; }         // Possible to have empty generics Type<>
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                yield return accessExpression;

                // Get generics
                if(HasGenericArguments == true)
                {
                    foreach (SyntaxNode node in genericArgumentList.Descendants)
                        yield return node;
                }

                // Get arguments
                foreach(SyntaxNode node in argumentList.Descendants)
                    yield return node;
            }
        }

        // Constructor
        internal MethodInvokeExpressionSyntax(ExpressionSyntax accessExpression, GenericArgumentListSyntax genericArguments, ArgumentListSyntax arguments)
        {
            // Check for null
            if(accessExpression == null)
                throw new ArgumentNullException(nameof(accessExpression));

            this.accessExpression = accessExpression;
            this.genericArgumentList = genericArguments;
            this.argumentList = arguments;

            // Set parent
            accessExpression.parent = this;
            if(genericArgumentList != null) genericArgumentList.parent = this;
            if(argumentList != null) argumentList.parent = this;
        }
        
        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write access
            accessExpression.GetSourceText(writer);

            // Write generics
            if(HasGenericArguments == true)
            {
                // Build generic arguments
                genericArgumentList.GetSourceText(writer);
            }

            // Build arguments
            argumentList.GetSourceText(writer);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitMethodInvokeExpression(this);
        }
    }
}
