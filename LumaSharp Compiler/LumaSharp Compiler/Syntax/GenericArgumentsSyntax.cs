
namespace LumaSharp_Compiler.Syntax
{
    public class GenericArgumentsSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken genericStart = null;
        private SyntaxToken genericEnd = null;
        private TypeReferenceSyntax[] genericTypes = null;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return genericStart; }
        }

        public override SyntaxToken EndToken
        {
            get { return genericEnd; }
        }

        public TypeReferenceSyntax[] GenericTypes
        {
            get { return genericTypes; }
        }

        public int GenericTypeCount
        {
            get { return genericTypes.Length; }
        }

        // Constructor
        internal GenericArgumentsSyntax(SyntaxTree tree, SyntaxNode parent, TypeReferenceSyntax[] genericTypes)
            : base(tree, parent)
        {
            this.genericStart = new SyntaxToken("<");
            this.genericEnd = new SyntaxToken(">");
            this.genericTypes = genericTypes;
        }

        internal GenericArgumentsSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.GenericArgumentsContext generics)
            : base(tree, parent)
        {
            genericStart = new SyntaxToken(generics.Start);
            genericEnd = new SyntaxToken(generics.Stop);

            // Get identifiers
            LumaSharpParser.TypeReferenceContext[] typeReferences = generics.typeReference();

            // Process all
            this.genericTypes = new TypeReferenceSyntax[typeReferences.Length];

            for(int i = 0; i < typeReferences.Length; i++)
                this.genericTypes[i] = new TypeReferenceSyntax(tree, this, typeReferences[i]);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Generic start
            writer.Write(genericStart.ToString());

            // Generic types
            for(int i = 0; i < genericTypes.Length; i++)
            {
                // Get source text
                writer.Write(genericTypes[i].GetSourceText());

                // Append separator
                if(i < genericTypes.Length - 1)
                    writer.Write(", ");
            }

            // Generic end
            writer.Write(genericEnd.ToString());
        }
    }
}
