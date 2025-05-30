﻿using System.Collections;

namespace LumaSharp.Compiler.AST
{
    public class SeparatedListSyntax<T> : SyntaxNode, IEnumerable<T> where T : SyntaxNode
    {
        // Type
        private struct SyntaxSeparatedElement
        {
            // Public
            public SyntaxToken separator;
            public T syntax;
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
                    return syntaxList[0].syntax.StartToken;

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
                    return syntaxList[syntaxList.Count - 1].syntax.EndToken;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= syntaxList.Count)
                    throw new IndexOutOfRangeException();

                return syntaxList[index].syntax;
            }
        }

        public int Count
        {
            get { return syntaxList.Count; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                foreach (SyntaxSeparatedElement item in syntaxList)
                    yield return item.syntax;
            }
        }

        // Constructor
        internal SeparatedListSyntax(SyntaxNode parent, T[] items, SyntaxToken separator)
            : base(parent)
        {
            this.separatorKind = separator.Kind;

            // Add all with separator
            foreach (T item in items)
                AddElement(item, separator);
        }

        internal SeparatedListSyntax(SyntaxNode parent, SyntaxTokenKind separatorKind)
            : base(parent)
        {
            this.separatorKind = separatorKind;
        }

        // Methods
        public void AddElement(T syntaxElement, SyntaxToken? separator)
        {
            // Check kind
            if (separator != null && separator.Value.Kind != separatorKind)
                throw new ArgumentException("Separator must be of kind: " + separatorKind);

            // Check for count
            if (syntaxList.Count > 0 && separator == null)
                throw new ArgumentException("Separator must be provided for non-zero indexed elements");

            // Add to list
            syntaxList.Add(new SyntaxSeparatedElement
            {
                separator = separator != null ? separator.Value : default,
                syntax = syntaxElement,
            });
        }

        public override void GetSourceText(TextWriter writer)
        {
            // Process all elements
            for(int i = 0; i < syntaxList.Count; i++)
            {
                // Check for token
                if(i > 0)
                    syntaxList[i].separator.GetSourceText(writer);

                // Get syntax source
                syntaxList[i].syntax.GetSourceText(writer);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (SyntaxSeparatedElement item in syntaxList)
                yield return item.syntax;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
