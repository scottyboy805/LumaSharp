﻿
namespace LumaSharp.Compiler.AST
{
    public sealed class TypeSyntax : MemberSyntax, IMemberSyntaxContainer
    {
        // Private
        private readonly SyntaxToken keyword;
        private readonly GenericParameterListSyntax genericParameters;
        private readonly SeparatedListSyntax<TypeReferenceSyntax> baseTypes;
        private readonly SyntaxToken colon;
        private readonly SyntaxToken overrideKeyword;

        private readonly BlockSyntax<MemberSyntax> memberBlock;

        // Properties
        public override SyntaxToken StartToken
        {
            get { return keyword; }
        }

        public override SyntaxToken EndToken
        {
            get { return memberBlock.EndToken; }
        }

        public SyntaxToken Keyword
        {
            get { return keyword; }
        }

        public SeparatedTokenList Namespace
        {
            get 
            {
                SyntaxNode current = Parent;

                // Move up until end or namespace
                while(current != null && (current is NamespaceSyntax) == false)
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
        }

        public SeparatedListSyntax<TypeReferenceSyntax> BaseTypes
        {
            get { return baseTypes; }
        }

        public SyntaxToken Colon
        {
            get { return colon; }
        }

        public SyntaxToken Override
        {
            get { return overrideKeyword; }
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
            get { return HasGenericParameters ? genericParameters.Count : 0; }
        }

        public int BaseTypeCount
        {
            get { return HasBaseTypes ? baseTypes.Count : 0; }
        }

        public int MemberCount
        {
            get { return HasMembers ? memberBlock.ElementCount: 0; }
        }        

        public bool HasGenericParameters
        {
            get { return genericParameters != null; }
        }

        public bool HasBaseTypes
        {
            get { return baseTypes != null; }
        }

        public bool HasMembers
        {
            get { return memberBlock.HasElements; }
        }

        public bool IsAttribute
        {
            get { return keyword.Kind == SyntaxTokenKind.AttributeKeyword; }
        }

        public bool IsOverride
        {
            get { return overrideKeyword.Kind != SyntaxTokenKind.Invalid; }
        }

        internal override IEnumerable<SyntaxNode> Descendants
        {
            get { return memberBlock.Descendants; }
        }

        // Constructor
        internal TypeSyntax(SyntaxNode parent, string identifier, AttributeReferenceSyntax[] attributes, SyntaxToken[] accessModifiers, GenericParameterListSyntax genericParameters, bool isOverride, SeparatedListSyntax<TypeReferenceSyntax> baseTypes, BlockSyntax<MemberSyntax> memberBlock)
            : base(parent, identifier, attributes, accessModifiers)
        {
            // Members
            this.keyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.TypeKeyword);
            this.genericParameters = genericParameters;

            // Check for override
            if (isOverride == true)
                this.overrideKeyword = Syntax.KeywordOrSymbol(SyntaxTokenKind.OverrideKeyword);

            if (baseTypes != null)
            {
                this.colon = Syntax.KeywordOrSymbol(SyntaxTokenKind.ColonSymbol);
                this.baseTypes = baseTypes;
            }

            this.memberBlock = memberBlock;

            // Check for null block
            if (memberBlock == null)
                this.memberBlock = new BlockSyntax<MemberSyntax>(this, Enumerable.Empty<MemberSyntax>());
        }

        internal TypeSyntax(SyntaxNode parent, LumaSharpParser.TypeDeclarationContext typeDef)
            : base(typeDef.IDENTIFIER(), parent, typeDef.attributeReference(), typeDef.accessModifier())
        {
            // Type keyword
            this.keyword = new SyntaxToken(SyntaxTokenKind.TypeKeyword, typeDef.TYPE());

            // Get generics
            if (typeDef.genericParameterList() != null)
            {
                this.genericParameters = new GenericParameterListSyntax(this, typeDef.genericParameterList());
            }

            // Override
            if(typeDef.OVERRIDE() != null)
            {
                this.overrideKeyword = new SyntaxToken(SyntaxTokenKind.OverrideKeyword, typeDef.OVERRIDE());
            }

            // Get base
            LumaSharpParser.InheritParametersContext inherit = typeDef.inheritParameters();

            if (inherit != null)
            {
                // Inherit symbol
                this.colon = new SyntaxToken(SyntaxTokenKind.ColonSymbol, inherit.COLON());

                // Base types
                this.baseTypes = ExpressionSyntax.List(this, inherit.typeReferenceList());
            }

            // Get members
            LumaSharpParser.MemberBlockContext block = typeDef.memberBlock();

            this.memberBlock = new BlockSyntax<MemberSyntax>(this, block);
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
                // Generic parameters
                genericParameters.GetSourceText(writer);
            }

            // Check for override
            if(IsOverride == true)
                overrideKeyword.GetSourceText(writer);

            // Check for base types
            if(HasBaseTypes == true)
            {
                // Colon
                colon.GetSourceText(writer);

                // Base types
                baseTypes.GetSourceText(writer);
            }

            // Write block
            memberBlock.GetSourceText(writer);
        }

        void IMemberSyntaxContainer.AddMember(MemberSyntax member)
        {
            ((IMemberSyntaxContainer)memberBlock).AddMember(member);

            // Update hierarchy
            member.parent = this;
        }
    }
}
