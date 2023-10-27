
namespace LumaSharp_Compiler.AST
{
    public sealed class ParameterListSyntax : SyntaxNode
    {
        // Private
        private ParameterSyntax[] parameters = null;
        private SyntaxToken lparen = null;
        private SyntaxToken rparen = null;
        private SyntaxToken comma = null;

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
        internal ParameterListSyntax(ParameterSyntax[] parameters)
            : base(parameters.Length > 0 ? parameters[0].StartToken : null, parameters.Length > 0 ? parameters[parameters.Length - 1].EndToken : null)
        {
            this.parameters = parameters;
            this.lparen = SyntaxToken.LParen();
            this.rparen = SyntaxToken.RParen();
            this.comma = SyntaxToken.Comma();
        }

        internal ParameterListSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MethodParameterListContext paramsDef)
            : base(tree, parent, paramsDef)
        {
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
            // Lparen
            lparen.GetSourceText(writer);

            // Write all
            for(int i = 0; i < parameters.Length; i++)
            {
                // Write parameter
                parameters[i].GetSourceText(writer);

                // Comma
                if (i < parameters.Length - 1)
                    comma.GetSourceText(writer);
            }

            // Rparen
            rparen.GetSourceText(writer);
        }
    }
}
