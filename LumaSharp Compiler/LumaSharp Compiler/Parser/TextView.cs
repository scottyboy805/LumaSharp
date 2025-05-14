
namespace LumaSharp.Compiler.Parser
{
    internal sealed class TextView : IDisposable
    {
        // Private
        private readonly TextReader reader;
        private readonly char[] buffer;
        private int bufferStart = 0;
        private int bufferEnd = 0;
        private int bufferSize = 0;
        private int position = 0;

        // Properties
        public int Position => position;
        public bool EOF
        {
            get
            {
                // Try to fill at least 1 character if buffer is empty
                if (bufferStart == bufferEnd)
                    FillBuffer(1);

                // Check for end
                return bufferStart == bufferEnd;
            }
        }

        // Constructor
        public TextView(TextReader reader, int bufferSize = 1024)
        {
            this.reader = reader;
            this.buffer = new char[bufferSize];
        }

        // Methods
        public void Dispose()
        {
            reader.Dispose();
        }

        public char Peek(int offset = 0)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            // Fill the buffer if required
            int required = offset + 1;
            if ((bufferEnd - bufferStart) < required)
                FillBuffer(required - (bufferEnd - bufferStart));

            // Check for end of stream
            if (bufferStart + offset >= bufferEnd)
                return '\0';

            // Get character at index
            return buffer[bufferStart + offset];
        }

        public char Consume()
        {
            // Get the current character
            char current = Peek();
            
            // Consume a single
            Consume(1);
            return current;
        }

        public void Consume(int amount)
        {
            // Check for amount
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            // Fill the buffer
            FillBuffer(1);

            // Calculate the amount that can be read
            int toRead = Math.Min(amount, bufferEnd - bufferStart);

            // Advance pointer
            bufferStart += toRead;
            position += toRead;
        }

        private void FillBuffer(int minChars = 1)
        {
            // Shift buffer if needed
            if (bufferStart > 0 && bufferStart < bufferEnd)
            {
                Array.Copy(buffer, bufferStart, buffer, 0, bufferEnd - bufferStart);
                bufferEnd -= bufferStart;
                bufferStart = 0;
            }
            else if (bufferStart == bufferEnd)
            {
                bufferStart = bufferEnd = 0;
            }

            while ((bufferEnd - bufferStart) < minChars && bufferEnd < buffer.Length)
            {
                int read = reader.Read(buffer, bufferEnd, buffer.Length - bufferEnd);
                if (read == 0)
                    break;
                bufferEnd += read;
            }
        }
    }
}
