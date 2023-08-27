
namespace LumaSharp_Compiler.Syntax
{
    public class GenericParameterListSyntax : SyntaxNode
    {
        // Private        
        private GenericParameterSyntax[] genericParameters = null;

        // Properties
        public GenericParameterSyntax[] GenericParameters
        {
            get { return genericParameters; }
        }

        public int GenericParameterCount
        {
            get { return HasGenericParameters ? genericParameters.Length : 0; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameters != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { yield break; }
        }

        // Constructor
        //internal GenericParameterListSyntax(SyntaxTree tree, SyntaxNode parent, string[] genericNames)
        //    : base(tree, parent)
        //{
        //    this.start = new SyntaxToken("<");
        //    this.end = new SyntaxToken(">");
        //    this.genericParameters = new SyntaxToken[genericNames.Length];

        //    for(int i = 0; i < genericParameters.Length; i++)
        //        genericParameters[i] = new SyntaxToken(genericNames[i]);
        //}

        internal GenericParameterListSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.GenericParameterListContext generics)
            : base(tree, parent, generics)
        {
            // Parameters
            LumaSharpParser.GenericParameterContext[] parameters = generics.genericParameter();

            if(parameters != null)
            {
                this.genericParameters = parameters.Select((p, i) => new GenericParameterSyntax(tree, this, p, i)).ToArray();
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            //// Generic start
            //writer.Write(genericStart.ToString());

            //// Generic types
            //for(int i = 0; i < genericParameters.Length; i++)
            //{
            //    // Get source text
            //    writer.Write(genericParameters[i].ToString());

            //    // Append separator
            //    if(i < genericParameters.Length - 1)
            //        writer.Write(", ");
            //}

            //// Generic end
            //writer.Write(genericEnd.ToString());
        }
    }
}
