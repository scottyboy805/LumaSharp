using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Parser
{
    internal sealed class TokenView
    {
        // Private
        private readonly IEnumerator<SyntaxToken> tokenProvider;
        private readonly List<SyntaxToken> buffer;
        private int position = 0;

        // Properties
        public int Position => position;
        public bool EOF
        {
            get
            {
                // Ensure buffer
                EnsureBuffer(position);

                // Check for end
                return position >= buffer.Count;
            }
        }

        // Constructor
        public TokenView(IEnumerator<SyntaxToken> tokenProvider, int bufferSize = 256)
        {
            // Check for null
            if(tokenProvider == null)
                throw new ArgumentNullException(nameof(tokenProvider));

            this.tokenProvider = tokenProvider;
            this.buffer = new(bufferSize);
        }

        // Methods
        public void Retrace(int retracePosition)
        {
            // Check bounds
            if (retracePosition < 0 || retracePosition > position)
                throw new InvalidOperationException("Retrace position must be a value less than the current position");

            // Check for equal - no need to do anything
            if (retracePosition == position)
                return;

            // Update position
            position = retracePosition;
        }

        public SyntaxToken Peek(int offset = 0)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            // Fill the buffer if required
            EnsureBuffer(position + offset);

            // Check for end of stream
            if (position + offset >= buffer.Count)
                return SyntaxToken.Invalid;

            // Get character at index
            return buffer[position + offset];
        }

        public SyntaxTokenKind PeekKind(int offset = 0)
        {
            return Peek(offset).Kind;
        }

        public SyntaxToken Consume()
        {
            // Get the current token
            SyntaxToken current = Peek();

            // Consume a single if we are not at the end
            if (EOF == false) 
                position++;

            return current;
        }

        public void Consume(int amount)
        {
            // Check for amount
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            // Fill the buffer
            EnsureBuffer(position + amount - 1);

            // Get the position
            position = Math.Min(position + amount, buffer.Count);
        }

        public bool ConsumeExpect(SyntaxTokenKind kind, out SyntaxToken actual)
        {
            actual = Consume();

            // Check for expected
            return actual.Kind == kind;
        }

        private void EnsureBuffer(int index)
        {
            // Repeat until we reach the index
            while (buffer.Count <= index)
            {
                // Try to move to next
                if (!tokenProvider.MoveNext())
                    break;

                // Cache buffered token
                buffer.Add(tokenProvider.Current);
            }
        }
    }
}
