﻿
namespace LumaSharp_Compiler.Syntax
{
    public sealed class EnumSyntax : MemberSyntax
    {
        // Private
        private SyntaxToken keyword = null;
        private SyntaxToken start = null;
        private SyntaxToken end = null;
        private TypeReferenceSyntax underlyingTypeReference = null;

        private BlockSyntax<FieldSyntax> fieldBlock = null;

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

        public TypeReferenceSyntax UnderlyingTypeReference
        {
            get { return underlyingTypeReference; }
        }

        public BlockSyntax<FieldSyntax> FieldBlock
        {
            get { return fieldBlock; }
        }

        public IEnumerable<FieldSyntax> Fields
        {
            get { return fieldBlock.Elements; }
        }

        public int FieldCount
        {
            get { return fieldBlock.ElementCount; }
        }

        public bool HasFields
        {
            get { return fieldBlock.HasElements; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return fieldBlock.Descendants; }
        }

        // Constructor
        internal EnumSyntax(SyntaxTree tree, SyntaxNode parent, string identifier)
            : base(identifier, tree, parent)
        {
            this.keyword = new SyntaxToken("enum");

            this.start = this.keyword;
            this.end = this.identifier;
        }

        internal EnumSyntax(SyntaxTree tree, SyntaxNode parent, LumaSharpParser.EnumDeclarationContext enumDef)
            : base(enumDef.IDENTIFIER(), tree, parent, enumDef.accessModifier())
        {
            // Enum keyword
            this.keyword = new SyntaxToken(enumDef.ENUM());

            // Get fields
            LumaSharpParser.FieldBlockContext block = enumDef.fieldBlock();

            //this.fieldBlock = new BlockSyntax<FieldSyntax>(tree, this, block);
        }

        public override void GetSourceText(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}