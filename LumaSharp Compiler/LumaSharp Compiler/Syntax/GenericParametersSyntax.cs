using Antlr4.Runtime.Tree;

namespace LumaSharp_Compiler.Syntax
{
    public class GenericParametersSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken genericStart = null;
        private SyntaxToken genericEnd = null;
        private SyntaxToken[] genericIdentifiers = null;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return genericStart; }
        }

        public override SyntaxToken EndToken
        {
            get { return genericEnd; }
        }

        public SyntaxToken[] GenericTypes
        {
            get { return genericIdentifiers; }
        }

        public int GenericTypeCount
        {
            get { return genericIdentifiers.Length; }
        }

        // Constructor
        internal GenericParametersSyntax(SyntaxTree tree, SyntaxNode parent, string[] genericNames)
            : base(tree, parent)
        {
            this.genericStart = new SyntaxToken("<");
            this.genericEnd = new SyntaxToken(">");
            this.genericIdentifiers = new SyntaxToken[genericNames.Length];

            for(int i = 0; i < genericIdentifiers.Length; i++)
                genericIdentifiers[i] = new SyntaxToken(genericNames[i]);
        }

        internal GenericParametersSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.GenericParametersContext generics)
            : base(tree, parent)
        {
            genericStart = new SyntaxToken(generics.Start);
            genericEnd = new SyntaxToken(generics.Stop);

            // Get identifiers
            ITerminalNode[] genericNames = generics.IDENTIFIER();

            // Process all
            this.genericIdentifiers = new SyntaxToken[genericNames.Length];

            for(int i = 0; i < genericIdentifiers.Length; i++)
                this.genericIdentifiers[i] = new SyntaxToken(genericNames[i]);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Generic start
            writer.Write(genericStart.ToString());

            // Generic types
            for(int i = 0; i < genericIdentifiers.Length; i++)
            {
                // Get source text
                writer.Write(genericIdentifiers[i].ToString());

                // Append separator
                if(i < genericIdentifiers.Length - 1)
                    writer.Write(", ");
            }

            // Generic end
            writer.Write(genericEnd.ToString());
        }
    }
}
