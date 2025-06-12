
using LumaSharp.Compiler.AST.Visitor;

namespace LumaSharp.Compiler.AST
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        // Private
        private readonly ImportSyntax[] imports;
        private readonly SyntaxNode[] namespaceOrMembers;

        // Properties
        public override SyntaxToken StartToken
        {
            get
            {
                // Check for imports
                if (imports != null)
                    return imports[0].StartToken;

                // Check for namespace or members
                if(namespaceOrMembers != null)
                    return namespaceOrMembers[0].StartToken;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                // Check for namespace or members
                if (namespaceOrMembers != null)
                    return namespaceOrMembers[namespaceOrMembers.Length - 1].EndToken;

                // Check for imports
                if (imports != null)
                    return imports[imports.Length - 1].EndToken;

                // Not valid
                return SyntaxToken.Invalid;
            }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get
            {
                if(imports != null)
                {
                    foreach (ImportSyntax i in imports)
                        yield return i;
                }

                if(namespaceOrMembers != null)
                {
                    foreach(SyntaxNode n in namespaceOrMembers)
                        yield return n;
                }
            }
        }

        // Constructor
        internal CompilationUnitSyntax(ImportSyntax[] imports, SyntaxNode[] namespaceOrMembers)
        {
            this.imports = imports;
            this.namespaceOrMembers = namespaceOrMembers;
        }

        // Methods
        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitCompilationUnit(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitCompilationUnit(this);
        }

        public override void GetSourceText(TextWriter writer)
        {
            if (imports != null)
            {
                foreach (ImportSyntax i in imports)
                    i.GetSourceText(writer);
            }

            if (namespaceOrMembers != null)
            {
                foreach (SyntaxNode n in namespaceOrMembers)
                    n.GetSourceText(writer);
            }
        }
    }
}
