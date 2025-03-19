
using Antlr4.Runtime;
using System.Text;

namespace LumaSharp.Compiler
{
    public sealed class InputSource : IDisposable
    {
        // Private
        private TextReader reader = null;
        private string documentName = "Unknown";
        private string documentPath = "";

        // Properties


        // Constructor
        private InputSource(TextReader reader, string name = null, string path = null)
        {
            this.reader = reader;
            if(name != null) this.documentName = name;
            if(path != null) this.documentPath = path;
        }

        // Methods
        public void Dispose()
        {
            reader.Dispose();
        }

        internal AntlrInputStream GetInputStream()
        {
            return new AntlrInputStream(reader);
        }

        public static InputSource FromSourceText(string lumaSharpSource)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(lumaSharpSource) == true)
                throw new ArgumentException("Input source cannot be null or empty");

            // Create reader
            return new InputSource(new StringReader(lumaSharpSource));
        }

        public static InputSource FromTextReader(TextReader reader)
        {
            // Check for null
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            return new InputSource(reader);
        }

        public static InputSource FromFile(string filePath, Encoding encoding = null)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(filePath) == true)
                throw new ArgumentException("File path cannot be null or empty");

            // Check for file
            if (File.Exists(filePath) == false)
                throw new ArgumentException("File path does not exist");

            // Create reader
            return new InputSource(new StreamReader(filePath, encoding),
                Path.GetFileNameWithoutExtension(filePath),
                filePath);
        }
    }
}
