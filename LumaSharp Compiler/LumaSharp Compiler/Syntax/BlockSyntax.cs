
namespace LumaSharp_Compiler.Syntax
{
    public class BlockSyntax<T> : SyntaxNode where T : SyntaxNode
    {
        // Private
        private SyntaxToken start = null;
        private SyntaxToken end = null;
        private T[] elements = null;

        // Properties
        public T[] Elements
        {
            get { return elements; }
        }

        public int ElementCount
        {
            get { return HasElements ? elements.Length : 0; }
        }

        public bool HasElements
        {
            get { return elements != null; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return elements; }
        }

        // Constructor
        internal BlockSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.MemberBlockContext memberBlock)
            : base(tree, parent, memberBlock)
        {
            this.start = new SyntaxToken(memberBlock.Start);
            this.end = new SyntaxToken(memberBlock.Stop);

            // Get all members
            LumaSharpParser.MemberDeclarationContext[] members = memberBlock.memberDeclaration();

            if (members.Length > 0)
            {
                this.elements = new T[members.Length];

                for (int i = 0; i < members.Length; i++)
                {
                    var typeMember = members[i].typeDeclaration();
                    var contractMember = members[i].contractDeclaration();
                    var enumMember = members[i].enumDeclaration();
                    var fieldMember = members[i].fieldDeclaration();
                    var methodMember = members[i].methodDeclaration();

                    // Create type member
                    if (typeMember != null)
                        this.elements[i] = new TypeSyntax(tree, this, typeMember) as T;

                    // Create contract member
                    if (contractMember != null)
                        ;// this.elements[i] = new ContractSyntax(contractMember) as T;
                }

            }
        }

        internal BlockSyntax(SyntaxTree tree, SyntaxNode node, LumaSharpParser.RootMemberBlockContext rootMemberBlock)
            : base(tree, node, rootMemberBlock)
        {
            this.start = new SyntaxToken(rootMemberBlock.Start);
            this.end = new SyntaxToken(rootMemberBlock.Stop);

            // Get all members
            LumaSharpParser.RootMemberContext[] members = rootMemberBlock.rootMember();

            if (members.Length > 0)
            {
                this.elements = new T[members.Length];

                for (int i = 0; i < members.Length; i++)
                {
                    var typeMember = members[i].typeDeclaration();
                    var contractMember = members[i].contractDeclaration();
                    var enumMember = members[i].enumDeclaration();

                    // Create type member
                    if (typeMember != null)
                        this.elements[i] = new TypeSyntax(tree, this, typeMember) as T;

                    // Create contract member
                    if (contractMember != null)
                        ;// this.elements[i] = new ContractSyntax(contractMember) as T;
                }

            }
        }

        internal BlockSyntax(SyntaxTree tree, SyntaxNode node, LumaSharpParser.StatementBlockContext statementBlock)
            : base(tree, node, statementBlock)
        {
            this.start = new SyntaxToken(statementBlock.Start);
            this.end = new SyntaxToken(statementBlock.Stop);

            // Get all members
            LumaSharpParser.StatementContext[] members = statementBlock.statement();

            if (members.Length > 0)
            {
                this.elements = new T[members.Length];

                for (int i = 0; i < members.Length; i++)
                {
                    //var typeMember = members[i].typeDeclaration();
                    //var contractMember = members[i].contractDeclaration();
                    //var enumMember = members[i].enumDeclaration();
                    //var fieldMember = members[i].fieldDeclaration();
                    //var methodMember = members[i].methodDeclaration();

                    //// Create type member
                    //if (typeMember != null)
                    //    this.elements[i] = new TypeSyntax(typeMember) as T;

                    //// Create contract member
                    //if (contractMember != null)
                    //    ;// this.elements[i] = new ContractSyntax(contractMember) as T;
                }

            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write start
            writer.Write(start.ToString());

            // Write all elements
            for(int i = 0; i < elements.Length; i++)
            {
                elements[i].GetSourceText(writer);
            }

            // Write end
            writer.Write(end.ToString());
        }
    }
}
