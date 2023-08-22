
namespace LumaSharp_Compiler.Syntax
{
    public abstract class SyntaxNode
    {
        // Properties
        public abstract SyntaxToken StartToken { get; }

        public abstract SyntaxToken EndToken { get; }

        // Methods
        public abstract void GetSourceText(TextWriter writer);

        public string GetSourceText()
        {
            // Create the writer
            using(StringWriter writer = new StringWriter())
            {
                // Write the text
                GetSourceText(writer);

                // Get full string
                return writer.ToString();
            }
        }
    }
}
