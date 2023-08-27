
namespace LumaSharp_Compiler.Syntax
{
    public sealed class ParameterListSyntax : SyntaxNode
    {
        // Private
        private ParameterSyntax[] parameters = null;
        private SyntaxToken start = null;
        private SyntaxToken end = null;

        // Properties
        public ParameterSyntax[] Parameters
        {
            get { return parameters; }
        }

        public int ParameterCount
        {
            get { return HasParameters ? parameters.Length : 0; }
        }

        public bool HasParameters
        {
            get { return parameters != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return parameters; }
        }

        // Constructor
        internal ParameterListSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MethodParameterListContext paramsDef)
            : base(tree, parent, paramsDef)
        {
            // Get start and end
            this.start = new SyntaxToken(paramsDef.Start);
            this.end = new SyntaxToken(paramsDef.Stop);

            // Get all parameters
            LumaSharpParser.MethodParameterContext[] parameters = paramsDef.methodParameter();

            // Check for any
            if(parameters != null)
            {
                this.parameters = parameters.Select((p, i) => new ParameterSyntax(tree, this, p, i)).ToArray();
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
