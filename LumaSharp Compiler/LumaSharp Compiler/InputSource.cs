using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("LumaSharp CompilerTests")]

namespace LumaSharp.Compiler
{
    public sealed class InputSource : IDisposable
    {
        // Private
        private readonly TextReader reader = null;
        private readonly string document = null;

        // Properties
        public TextReader Reader => reader;
        public string Document => document;

        // Constructor
        private InputSource(TextReader reader, string document = null)
        {
            this.reader = reader;
            this.document = document;

            // Check for null
            if (document == null && reader is StreamReader sr && sr.BaseStream is FileStream fs)
                this.document = fs.Name;
        }

        // Methods
        public void Dispose()
        {
            reader.Dispose();
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
                filePath);
        }
    }
}
