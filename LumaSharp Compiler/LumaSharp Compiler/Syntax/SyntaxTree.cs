
namespace LumaSharp_Compiler.Syntax
{
    public sealed class SyntaxTree : SyntaxNode
    {
        // Private
        private MemberSyntax[] rootMembers = null;
        private NamespaceSyntax[] namespaceMembers = null;

        // Properties
        public override SyntaxToken StartToken => throw new NotImplementedException();

        public override SyntaxToken EndToken => throw new NotImplementedException();

        // Constructor
        internal SyntaxTree(LumaSharpParser.CompilationUnitContext unit)
            : base(null, null)
        {
            // Get root members
            LumaSharpParser.RootMemberContext[] rootMembers = unit.rootMember();

            this.rootMembers = new MemberSyntax[rootMembers.Length];

            // Process all members
            for (int i = 0; i < rootMembers.Length; i++)
            {
                // Get all valid members
                LumaSharpParser.TypeDeclarationContext typeDef = rootMembers[i].typeDeclaration();
                LumaSharpParser.ContractDeclarationContext contractDef = rootMembers[i].contractDeclaration();
                LumaSharpParser.EnumDeclarationContext enumDef = rootMembers[i].enumDeclaration();

                // Check for type
                if (typeDef != null)
                    this.rootMembers[i] = new TypeSyntax(this, this, typeDef);

                // Check for contract

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
