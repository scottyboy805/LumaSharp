
using LumaSharp_Compiler.Syntax.Factory;

namespace LumaSharp_Compiler.Syntax
{
    public sealed class SyntaxTree : SyntaxNode//, IRootMemberSyntaxContainer
    {
        // Private
        private List<SyntaxNode> rootElements = null;

        // Properties
        public override SyntaxToken StartToken => throw new NotImplementedException();

        public override SyntaxToken EndToken => throw new NotImplementedException();

        public int RootElementCount
        {
            get { return HasRootElements ? rootElements.Count : 0; }
        }

        public int RootMemberCount
        {
            get { return HasRootMembers ? DescendantsOfType<MemberSyntax>().Count() : 0; }
        }

        public int NamespaceMemberCount
        {
            get { return HasNamespaceMembers ? DescendantsOfType<NamespaceSyntax>().Count() : 0; }
        }

        public bool HasRootElements
        {
            get { return rootElements != null; }
        }

        public bool HasRootMembers
        {
            get { return DescendantsOfType<MemberSyntax>().Any(); }
        }

        public bool HasNamespaceMembers
        {
            get { return DescendantsOfType<NamespaceSyntax>().Any(); }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return rootElements; }
        }

        // Constructor
        internal SyntaxTree(LumaSharpParser.CompilationUnitContext unit)
            : base(null, null)
        {
            // Get root members
            LumaSharpParser.ImportElementContext[] importElements = unit.importElement();
            LumaSharpParser.RootElementContext[] rootElements = unit.rootElement();

            if ((importElements != null && importElements.Length > 0) || (rootElements != null && rootElements.Length > 0))
            {
                // Create collection
                this.rootElements = new List<SyntaxNode>();
            }

            // Process all import elements
            if(importElements != null)
            {
                for(int i = 0; i < importElements.Length; i++)
                {
                    // Create import
                    this.rootElements.Add(new ImportSyntax(this, this, importElements[i]));
                }
            }

            // Process all root elements
            if (rootElements != null)
            {
                for (int i = 0; i < rootElements.Length; i++)
                {
                    // Get namespace or root member
                    LumaSharpParser.NamespaceDeclarationContext namespaceElement = rootElements[i].namespaceDeclaration();
                    LumaSharpParser.RootMemberContext rootMemberElement = rootElements[i].rootMember();

                    // Check for namespace
                    if (namespaceElement != null)
                    {
                        // Create namespace
                        this.rootElements.Add(new NamespaceSyntax(this, this, namespaceElement));
                    }
                    else if (rootMemberElement != null)
                    {
                        // Get all valid members
                        LumaSharpParser.TypeDeclarationContext typeDef = rootMemberElement.typeDeclaration();
                        LumaSharpParser.ContractDeclarationContext contractDef = rootMemberElement.contractDeclaration();
                        LumaSharpParser.EnumDeclarationContext enumDef = rootMemberElement.enumDeclaration();

                        // Check for type
                        if (typeDef != null)
                            this.rootElements.Add(new TypeSyntax(this, this, typeDef));

                        // Check for contract
                        if (contractDef != null)
                            this.rootElements.Add(new ContractSyntax(this, this, contractDef));

                        // Check for enum
                        if (enumDef != null)
                            this.rootElements.Add(new EnumSyntax(this, this, enumDef));
                    }
                }
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public static SyntaxTree Parse(InputSource source)
        {
            // Create parser
            ParserContext context = new ParserContext(source);

            // Parse syntax tree
            return context.ParseCompilationUnit();
        }

        public static SyntaxTree ParseStatement()
        {
            throw new NotImplementedException();
        }

        public static SyntaxTree ParseExpression()
        {
            throw new NotImplementedException();
        }
    }
}
