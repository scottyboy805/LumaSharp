using LumaSharp.Compiler.AST.Visitor;
using System.Collections;

namespace LumaSharp.Compiler.AST
{    
    public class SeparatedSyntaxList<T> : SyntaxNode, IEnumerable<T> where T : SyntaxNode
    {
        // Type
        public readonly struct SyntaxSeparatedElement
        {
            // Public
            public readonly T Syntax;
            public readonly SyntaxToken? Separator;
            
            // Constructor
            public SyntaxSeparatedElement(T syntax, SyntaxToken? separator)
            {
                this.Syntax = syntax;
                this.Separator = separator;
            }
        }

        // Private
        private readonly List<SyntaxSeparatedElement> syntaxList = new();
        private readonly SyntaxTokenKind separatorKind;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for any
                if (syntaxList.Count > 0)
                    return syntaxList[0].Syntax.StartToken;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for any
                if (syntaxList.Count > 0)
                    return syntaxList[syntaxList.Count - 1].Syntax.EndToken;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        public T this[int index]
        {
            get { return index >= 0 && index < syntaxList.Count ? syntaxList[index].Syntax : null; }
        }

        public int Count
        {
            get { return syntaxList.Count; }
        }

        public IReadOnlyList<SyntaxSeparatedElement> Elements
        {
            get { return syntaxList; }
        }

        public SyntaxTokenKind SeparatorKind
        {
            get { return separatorKind; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                foreach (SyntaxSeparatedElement item in syntaxList)
                    yield return item.Syntax;
            }
        }

        // Constructor
        protected SeparatedSyntaxList(SeparatedSyntaxList<T> other)
        {
            this.separatorKind = other != null
                ? other.separatorKind
                : SyntaxTokenKind.Invalid;
            this.syntaxList = other != null && other.syntaxList != null
                ? other.syntaxList.ToList()
                : new();
        }

        internal SeparatedSyntaxList(SyntaxTokenKind separatorKind, T syntaxSingle)
        {
            // Check for null
            if(syntaxSingle == null)
                throw new ArgumentNullException(nameof(syntaxSingle));

            this.separatorKind = separatorKind;
            this.syntaxList = new();
            this.syntaxList.Add(new SyntaxSeparatedElement(syntaxSingle, null));
        }

        internal SeparatedSyntaxList(SyntaxTokenKind separatorKind, IEnumerable<T> elements)
            : this(separatorKind, BuildSeparatedSyntax(separatorKind, elements))
        {
        }

        internal SeparatedSyntaxList(SyntaxTokenKind separatorKind, IEnumerable<SyntaxSeparatedElement> elements)
        {
            this.separatorKind = separatorKind;
            this.syntaxList = new(elements != null
                ? elements.Count()
                : 0);

            // Check for any
            if (elements != null)
            {
                // Add all elements
                int current = 0;
                foreach (SyntaxSeparatedElement item in elements)
                {
                    // Add the element
                    this.syntaxList.Add(item);

                    // Check null
                    if (item.Syntax == null)
                        throw new ArgumentException("Syntax element is null at index: " + current);

                    // Expect separator?
                    if (current < syntaxList.Count - 1 && item.Separator == null)
                        throw new ArgumentException("A separator must be provided when syntax continues at index: " + current);

                    // Check kind
                    if (item.Separator != null && item.Separator.Value.Kind != separatorKind)
                        throw new ArgumentException("Separator must be of kind: " + separatorKind);

                    current++;
                }
            }
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSyntaxList(this);
        }

        public override J Accept<J>(SyntaxVisitor<J> visitor)
        {
            return visitor.VisitSyntaxList(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Process all elements
            for(int i = 0; i < syntaxList.Count; i++)
            {
                // Get syntax source
                syntaxList[i].Syntax.GetSourceText(writer);

                // Check for token
                if (i < syntaxList.Count - 1)
                    syntaxList[i].Separator?.GetSourceText(writer);
            }
        }

        public int IndexOf(T syntax)
        {
            return syntaxList.FindIndex(i => i.Syntax == syntax);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach(SyntaxSeparatedElement element in syntaxList)
                yield return element.Syntax;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static IEnumerable<SyntaxSeparatedElement> BuildSeparatedSyntax(SyntaxTokenKind separatorKind, IEnumerable<T> elements)
        {
            // Check for null
            if (elements == null)
                yield break;

            // Get count
            int count = elements.Count();
            int current = 0;

            // Get all
            foreach(T element in elements)
            {
                // Create the separator
                SyntaxToken? separator = current++ < count - 1
                    ? Syntax.Token(separatorKind)
                    : (SyntaxToken?)null;

                // Create the element
                yield return new SyntaxSeparatedElement(element, separator);
            }
        }
    }
}
