﻿
namespace LumaSharp_Compiler.Syntax
{
    public sealed class ContractSyntax : MemberSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken start = null;
        private SyntaxToken end = null;
        private GenericParametersSyntax genericParameters = null;
        private TypeReferenceSyntax[] baseTypeReferences = null;

        private BlockSyntax<MemberSyntax> memberBlock = null;

        // Properties
        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public override SyntaxToken StartToken
        {
            get { return start; }
        }

        public override SyntaxToken EndToken
        {
            get { return end; }
        }

        public GenericParametersSyntax GenericParameters
        {
            get { return genericParameters; }
        }

        public TypeReferenceSyntax[] BaseTypeReferences
        {
            get { return baseTypeReferences; }
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
            get { return HasGenericParameters ? genericParameters.GenericTypeCount : 0; }
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
        internal ContractSyntax(SyntaxTree tree, SyntaxNode parent, string identifier)
            : base(identifier, tree, parent)
        {
            this.keyword = new SyntaxToken("contract");

            this.start = keyword;
            this.end = this.identifier;
        }

        internal ContractSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.ContractDeclarationContext contractDef)
            : base(contractDef.IDENTIFIER(), tree, parent, contractDef.accessModifier())
        {
            // Contract keyword
            this.keyword = new SyntaxToken(contractDef.CONTRACT());

            // Get generics
            LumaSharpParser.GenericParametersContext generics = contractDef.genericParameters();

            if(generics != null)
            {
                this.genericParameters = new GenericParametersSyntax(tree, this, generics);
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
            throw new NotImplementedException();
        }
    }
}