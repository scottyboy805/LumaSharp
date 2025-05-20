using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Parser
{
    internal sealed partial class ASTParser
    {
        // Methods
        internal BlockSyntax<MemberSyntax> ParseMemberBlock()
        {
            // Expect opening
            if(tokens.PeekKind() == SyntaxTokenKind.LBlockSymbol)
            {
                // Consume block
                SyntaxToken lBlock = tokens.Consume();

                MemberSyntax[] members = null;

                // Parse all members
                MemberSyntax member = ParseMember();

                // Check for valid
                while(member != null)
                {
                    if(members != null)
                    {
                        Array.Resize(ref members, members.Length + 1);
                    }
                    else
                    {
                        members = new MemberSyntax[1];
                    }

                    // Try to parse next member
                    member = ParseMember();
                }

                // Expect closing
                if (tokens.ConsumeExpect(SyntaxTokenKind.RBlockSymbol, out SyntaxToken rBlock) == false)
                    ; // TODO - Expected '}'

                // Create block
                return new BlockSyntax<MemberSyntax>(lBlock, members, rBlock);
            }
            return null;
        }

        internal MemberSyntax ParseMember()
        {
            //// Parse attributes
            //SeparatedListSyntax<AttributeReferenceSyntax> attributes = ParseSeparatedSyntaxList(ParseAttributeReference, SyntaxTokenKind.CommaSymbol);

            //// Parse access modifiers
            //SyntaxToken[] modifiers = ParseAccessModifiers();

            //// Check for member
            //MemberSyntax member = null;

            //// Check for type
            //if((member = ParseTypeDeclaration(attributes, modifiers)) != null)
            //{
            //    return member;
            //}
            return null;
        }

        internal TypeSyntax ParseTypeDeclaration()
        {
            // Check for type
            if(tokens.PeekKind() == SyntaxTokenKind.TypeKeyword)
            {
                // Consume keyword
                SyntaxToken typeToken = tokens.Consume();

                // Parse identifier
                if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
                {
                    // Expected identifier
                    report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                }

                // Parse generic parameters
                GenericParameterListSyntax genericParameters = ParseGenericParameterList();

                // Override keyword
                SyntaxToken overrideToken = default;

                if(tokens.PeekKind() == SyntaxTokenKind.OverrideKeyword)
                    overrideToken = tokens.Consume();

                // Inherit parameters
                SyntaxToken inheritToken = default;
                SeparatedSyntaxList<TypeReferenceSyntax> inheritTypes = null;

                // Check for inherit provided
                if(tokens.PeekKind() == SyntaxTokenKind.ColonSymbol)
                {
                    // Get the inherit token
                    inheritToken = tokens.Consume();

                    // Expect separated type inheritance list
                    inheritTypes = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.LBlockSymbol);
                }

                // Parse members
                BlockSyntax<MemberSyntax> members = ParseMemberBlock();

                // Create type declaration
            }
            return null;
        }

        internal ContractSyntax ParseContractDeclaration()
        {
            return null;
        }

        internal EnumSyntax ParseEnumDeclaration()
        {
            return null;
        }

        internal FieldSyntax ParseFieldDeclaration()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Parse optional attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Parse the field type
            TypeReferenceSyntax fieldType = ParseTypeReference();

            // Require that field type is valid at this point
            if(fieldType != null)
            {
                // Expect identifier next for the field name
                tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier);

                // Parse optional assignment
                VariableAssignExpressionSyntax assignement = null;

                // Create the field
                return new FieldSyntax(identifier, attributes, modifiers, fieldType, assignement);
            }

            // Retrace to starting position
            tokens.Retrace(initialPosition);
            return null;
        }

        internal AccessorSyntax ParseAccessorDeclaration()
        {
            return null;
        }

        internal MethodSyntax ParseMethodDeclaration()
        {
            // Store current position
            int initialPosition = tokens.Position;

            // Parse optional attributes
            AttributeReferenceSyntax[] attributes = ParseAttributes();

            // Parse access modifiers
            SyntaxToken[] modifiers = ParseAccessModifiers();

            // Parse method return values
            SeparatedSyntaxList<TypeReferenceSyntax> returnTypes = ParseSeparatedSyntaxList(ParseTypeReference, SyntaxTokenKind.CommaSymbol, SyntaxTokenKind.Identifier);

            // Check for return types available
            if(returnTypes != null)
            {
                // Expect identifier at this point
                if(tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == true)
                {
                    // Optional generic parameters
                    GenericParameterListSyntax genericParameters = ParseGenericParameterList();


                }
            }


            //new MethodSyntax()
            return null;
        }        

        internal SyntaxToken[] ParseAccessModifiers()
        {
            // Check for modifier
            SyntaxToken modifierToken = tokens.Peek();

            // Create modifiers
            SyntaxToken[] modifierList = null;

            // Check for modifier
            while(modifierToken.IsAccessModifier == true)
            {
                // Consume the token
                tokens.Consume();

                // Create array
                if(modifierList != null)
                {
                    Array.Resize(ref modifierList, modifierList.Length + 1);
                }
                else
                {
                    modifierList = new SyntaxToken[1];
                }

                // Add the modifier
                modifierList[modifierList.Length - 1] = modifierToken;

                // Check next token
                modifierToken = tokens.Peek();
            }
            return modifierList;
        }
    }
}
