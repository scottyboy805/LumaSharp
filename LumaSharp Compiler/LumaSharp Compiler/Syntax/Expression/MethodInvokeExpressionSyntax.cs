
namespace LumaSharp.Compiler.AST
{
    public sealed class MethodInvokeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private readonly ExpressionSyntax accessExpression;
        private readonly SyntaxToken identifier;
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

        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
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
        internal MethodInvokeExpressionSyntax(SyntaxToken identifier, ExpressionSyntax accessExpression, GenericArgumentListSyntax genericArguments, ArgumentListSyntax arguments)
        {
            this.identifier = identifier;
            this.accessExpression = accessExpression;
            this.genericArgumentList = genericArguments;
            this.argumentList = arguments;
        }
        
        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            if (accessExpression != null)
            {
                // Write access
                accessExpression.GetSourceText(writer);

                // Write dot 
                //dot.GetSourceText(writer);
            }

            // Write method name
            writer.Write(identifier.ToString());

            // Write generics
            if(HasGenericArguments == true)
            {
                // Build generic arguments
                genericArgumentList.GetSourceText(writer);
            }

            // Build arguments
            argumentList.GetSourceText(writer);
        }
    }
}
