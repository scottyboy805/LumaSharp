﻿
using System.ComponentModel.DataAnnotations;

namespace LumaSharp_Compiler.AST
{
    public sealed class ContractSyntax : MemberSyntax, IMemberSyntaxContainer
    {
        // Private
        private SyntaxToken keyword = null;
        private GenericParameterListSyntax genericParameters = null;
        private TypeReferenceSyntax[] baseTypeReferences = null;
        private SyntaxToken colon = null;
        private SyntaxToken comma = null;

        private BlockSyntax<MemberSyntax> memberBlock = null;

        // Properties
        public override SyntaxToken EndToken
        {
            get { return memberBlock.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public NamespaceName Namespace
        {
            get
            {
                SyntaxNode current = Parent;

                // Move up until end or namespace
                while (current != null && (current is NamespaceSyntax) == false)
                    current = current.Parent;

                // Try to get namespace
                NamespaceSyntax ns = current as NamespaceSyntax;

                // Get the name
                if (ns != null)
                    return ns.Name;

                return null;
            }
        }

        public GenericParameterListSyntax GenericParameters
        {
            get { return genericParameters; }
            internal set { genericParameters = value; }
        }

        public TypeReferenceSyntax[] BaseTypeReferences
        {
            get { return baseTypeReferences; }
            internal set { baseTypeReferences = value; }
        }

        public BlockSyntax<MemberSyntax> MemberBlock
        {
            get { return memberBlock; }
        }

        public IEnumerable<MemberSyntax> Members
        {
            get { return memberBlock.Elements; }
        }

        public int GenericParameterCount
        {
            get { return HasGenericParameters ? genericParameters.GenericParameterCount : 0; }
        }

        public int BaseTypeCount
        {
            get { return HasBaseTypes ? baseTypeReferences.Length : 0; }
        }

        public int MemberCount
        {
            get { return HasMembers ? memberBlock.ElementCount : 0; }
        }

        public bool HasGenericParameters
        {
            get { return genericParameters != null; }
        }

        public bool HasBaseTypes
        {
            get { return baseTypeReferences != null; }
        }

        public bool HasMembers
        {
            get { return memberBlock.HasElements; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return memberBlock.Descendants; }
        }

        // Constructor
        //internal ContractSyntax(SyntaxTree tree, SyntaxNode parent, string identifier)
        //    : base(identifier, tree, parent)
        //{
        //    this.keyword = new SyntaxToken("contract");

        //    this.start = keyword;
        //    this.end = this.identifier;
        //}

        internal ContractSyntax(string identifier)
            : base(identifier, SyntaxToken.Contract(), null)
        {
            this.keyword = base.StartToken.WithTrailingWhitespace(" ");
            this.memberBlock = new BlockSyntax<MemberSyntax>();
            this.colon = SyntaxToken.Colon();
            this.comma = SyntaxToken.Comma();
        }

        internal ContractSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ContractDeclarationContext contractDef)
            : base(contractDef.IDENTIFIER(), tree, parent, contractDef, contractDef.attributeDeclaration(), contractDef.accessModifier())
        {
            // Contract keyword
            this.keyword = new SyntaxToken(contractDef.CONTRACT());

            // Get generics
            if (contractDef.genericParameterList() != null)
            {
                this.genericParameters = new GenericParameterListSyntax(tree, this, contractDef.genericParameterList());
            }
            
            // Get base
            LumaSharpParser.InheritParametersContext inherit = contractDef.inheritParameters();

            if(inherit != null)
            {
                this.baseTypeReferences = inherit.typeReference().Select(t => new TypeReferenceSyntax(tree, this, t)).ToArray();
            }

            // Get members
            LumaSharpParser.MemberBlockContext block = contractDef.memberBlock();

            this.memberBlock = new BlockSyntax<MemberSyntax>(tree, this, block);
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Generate attributes
            base.GetSourceText(writer);

            // Keyword
            keyword.GetSourceText(writer);

            // Identifier
            identifier.GetSourceText(writer);

            // Generics
            if(HasGenericParameters == true)
            {
                genericParameters.GetSourceText(writer);
            }

            // Check for base types
            if(HasBaseTypes == true)
            {
                // Colon
                colon.GetSourceText(writer);

                // Base types
                for(int i = 0; i < baseTypeReferences.Length; i++)
                {
                    // Write type
                    baseTypeReferences[i].GetSourceText(writer);

                    // Comma
                    if(i <  baseTypeReferences.Length - 1)
                        comma.GetSourceText(writer);
                }
            }

            // Write block
            memberBlock.GetSourceText(writer);
        }

        public void AddMember(MemberSyntax member)
        {
            ((IMemberSyntaxContainer)memberBlock).AddMember(member);

            // Update hierarchy
            member.tree = tree;
            member.parent = this;
        }
    }
}
