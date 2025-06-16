using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Parser
{
    internal sealed partial class ASTParser
    {
        // Private
        private readonly TokenView tokens;
        private readonly ICompileReportProvider report;

        // Constructor
        public ASTParser(IEnumerator<SyntaxToken> tokenProvider, ICompileReportProvider report)
        {
            // Check for null
            if (tokenProvider == null)
                throw new ArgumentNullException(nameof(tokenProvider));

            // Create token view
            this.tokens = new TokenView(tokenProvider);
            this.report = report;
        }

        // Methods
        internal CompilationUnitSyntax ParseCompilationUnit()
        {
            // Parse imports
            ImportSyntax[] imports = null;

            while(tokens.PeekKind() == SyntaxTokenKind.ImportKeyword)
            {
                // Parse the import
                ImportSyntax import = ParseImport();

                // Check for null
                if (import == null)
                    continue;

                // Add to collection
                if(imports != null)
                {
                    Array.Resize(ref imports, imports.Length + 1);
                }
                else
                {
                    imports = new ImportSyntax[1];
                }

                // Add import
                imports[imports.Length - 1] = import;
            }

            // Parse namespace or members
            SyntaxNode[] namespaceOrMembers = null;

            while(tokens.EOF == false)
            {
                // Parse the namespace
                SyntaxNode namespaceOrMember = null;// ParseNamespaceReference();

                // Parse root member otherwise
                if (namespaceOrMember == null)
                    namespaceOrMember = ParseRootMember();

                // Check for any
                if (namespaceOrMember == null)
                    break;

                // Add to collection
                if(namespaceOrMembers != null)
                {
                    Array.Resize(ref namespaceOrMembers, namespaceOrMembers.Length + 1);
                }
                else
                {
                    namespaceOrMembers = new SyntaxNode[1];
                }

                // Add member
                namespaceOrMembers[namespaceOrMembers.Length - 1] = namespaceOrMember;
            }

            // Create compilation unit
            return new CompilationUnitSyntax(imports, namespaceOrMembers);
        }

        internal ImportSyntax ParseImport()
        {
            // Check for import
            if(tokens.PeekKind() == SyntaxTokenKind.ImportKeyword)
            {
                // Consume the token
                SyntaxToken importToken = tokens.Consume();

                // Check for alias
                if(tokens.PeekKind() == SyntaxTokenKind.Identifier && tokens.PeekKind(1) == SyntaxTokenKind.AsKeyword)
                {
                    // We are parsing an alias
                    SyntaxToken identifier = tokens.Consume();
                    SyntaxToken asToken = tokens.Consume();

                    // Parse the type reference
                    TypeReferenceSyntax importType = ParseTypeReference();

                    // Expected type
                    if(importType == null)
                    {
                        // Expected type
                        report.ReportDiagnostic(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Span);
                        return RecoverFromImportError();
                    }

                    // Expect semicolon
                    if (tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                    {
                        // Expected ';'
                        report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                        return RecoverFromImportError();
                    }

                    // Create the alias
                    return new ImportAliasSyntax(importToken, identifier, asToken, importType, semicolon);
                }
                else
                {
                    // We are parsing a standard import
                    SeparatedTokenList importName = ParseSeparatedTokenList(SyntaxTokenKind.ColonSymbol, SyntaxTokenKind.Identifier, false); //ParseNamespaceReference();

                    // Expect semicolon
                    if(tokens.ConsumeExpect(SyntaxTokenKind.SemicolonSymbol, out SyntaxToken semicolon) == false)
                    {
                        // Expected ';'
                        report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.SemicolonSymbol));
                        return RecoverFromImportError();
                    }

                    // Create the import
                    return new ImportSyntax(importToken, importName, semicolon);
                }
            }
            return null;
        }

        //internal SeparatedTokenList ParseNamespaceReference()
        //{
        //    SeparatedTokenList namespaceName = null;

        //    // Expect identifier followed by colon
        //    if(tokens.PeekKind() == SyntaxTokenKind.Identifier && tokens.PeekKind(1) == SyntaxTokenKind.ColonSymbol)
        //    {
        //        // Consume the first tokens
        //        SyntaxToken identifierToken = tokens.Consume();
        //        SyntaxToken colonToken = tokens.Consume();

        //        // We have a valid namespace - create the list
        //        namespaceName = new(SyntaxTokenKind.ColonSymbol, SyntaxTokenKind.Identifier);

        //        // Add the first name group
        //        namespaceName.AddElement(identifierToken, colonToken);

        //        // Repeat for following namespace names - Note look ahead further this time because we should not consume the last colon
        //        while(tokens.PeekKind() == SyntaxTokenKind.Identifier && tokens.PeekKind(1) == SyntaxTokenKind.ColonSymbol)
        //        {
        //            // Consume the last color
        //            tokens.Consume();

        //            // Consume the additional tokens
        //            identifierToken = tokens.Consume();
        //            colonToken = tokens.Consume();

        //            // Add the additional name group
        //            namespaceName.AddElement(identifierToken, colonToken);
        //        }
        //    }
        //    return namespaceName;
        //}

        internal AttributeSyntax[] ParseAttributes()
        {
            // Parse an attribute
            AttributeSyntax attribute = ParseAttribute();

            // Create array
            AttributeSyntax[] attributes = null;

            // Check for not null
            while (attribute != null)
            {
                // Update the array
                if (attributes != null)
                {
                    Array.Resize(ref attributes, attributes.Length + 1);
                }
                else
                {
                    attributes = new AttributeSyntax[1];
                }

                // Update element
                attributes[attributes.Length - 1] = attribute;

                // Parse next
                attribute = ParseAttribute();
            }
            return attributes;
        }

        internal AttributeSyntax ParseAttribute()
        {
            // Check for hash
            if(tokens.PeekKind() == SyntaxTokenKind.HashSymbol)
            {
                // Get the hash token
                SyntaxToken hashToken = tokens.Consume();

                // Get the type reference
                TypeReferenceSyntax attributeTypeReference = ParseFullTypeReference();

                // Check for type
                if (attributeTypeReference == null)
                {
                    // Expected type reference
                    report.ReportDiagnostic(Code.ExpectedType, MessageSeverity.Error, tokens.Peek().Span);
                    return null; // Cannot recover from here - need to process each following token manually
                }

                // Get the argument list
                ArgumentListSyntax argumentList = ParseArgumentList();

                // Create attribute
                return new AttributeSyntax(hashToken, attributeTypeReference, argumentList);
            }
            return null;
        }

        internal ArrayParametersSyntax ParseArrayParameters()
        {
            // Check for open array
            if (tokens.PeekKind() == SyntaxTokenKind.LArraySymbol)
            {
                // Get the open array
                SyntaxToken lArray = tokens.Consume();

                // Parse separators
                SyntaxToken[] separators = null;

                // Check for separators
                while (tokens.PeekKind() == SyntaxTokenKind.CommaSymbol)
                {
                    // Update array
                    if (separators != null)
                    {
                        // resize existing
                        Array.Resize(ref separators, separators.Length + 1);
                    }
                    else
                    {
                        // Create new
                        separators = new SyntaxToken[1];
                    }

                    // Append token
                    separators[separators.Length - 1] = tokens.Consume();
                }

                // Expect comma or end array
                if (tokens.ConsumeExpect(SyntaxTokenKind.RArraySymbol, out SyntaxToken rArray) == false)
                {
                    // Expected ']'
                    report.ReportDiagnostic(Code.ExpectedToken, MessageSeverity.Error, tokens.Peek().Span, SyntaxToken.GetText(SyntaxTokenKind.RArraySymbol));
                    return RecoverFromArrayParameterError();
                }

                // Create array parameters
                return new ArrayParametersSyntax(lArray, separators, rArray);
            }
            return null;
        }

        

        internal TypeReferenceSyntax ParseTypeReference()
        {
            // Check for primitive
            TypeReferenceSyntax typeReference = ParsePrimitiveTypeReference();

            // Check for null
            if (typeReference != null)
                return typeReference;

            // Check for identifier
            return ParseFullTypeReference();
        }

        internal TypeReferenceSyntax ParsePrimitiveTypeReference()
        {
            // Check for any primitive type
            SyntaxTokenKind kind = tokens.PeekKind();

            // Check for primitive token
            switch (kind)
            {
                case SyntaxTokenKind.AnyKeyword:
                case SyntaxTokenKind.BoolKeyword:
                case SyntaxTokenKind.CharKeyword:
                case SyntaxTokenKind.I8Keyword:
                case SyntaxTokenKind.U8Keyword:
                case SyntaxTokenKind.I16Keyword:
                case SyntaxTokenKind.U16Keyword:
                case SyntaxTokenKind.I32Keyword:
                case SyntaxTokenKind.U32Keyword:
                case SyntaxTokenKind.I64Keyword:
                case SyntaxTokenKind.U64Keyword:
                case SyntaxTokenKind.F32Keyword:
                case SyntaxTokenKind.F64Keyword:
                case SyntaxTokenKind.StringKeyword:
                case SyntaxTokenKind.VoidKeyword:
                    {
                        // Get the keyword token
                        SyntaxToken primitiveToken = tokens.Consume();

                        // Create reference
                        return new TypeReferenceSyntax(primitiveToken,
                            ParseArrayParameters());
                    }
            }
            return null;
        }

        internal TypeReferenceSyntax ParseFullTypeReference()
        {
            // Parse the namespace name first
            SeparatedTokenList namespaceName = ParseSeparatedTokenList(SyntaxTokenKind.ColonSymbol, SyntaxTokenKind.Identifier, true);// ParseNamespaceReference();

            // Parse parent name?
            ParentTypeReferenceSyntax[] parentTypes = ParseParentTypeReferences();

            // Parse identifier
            if (tokens.ConsumeExpect(SyntaxTokenKind.Identifier, out SyntaxToken identifier) == false)
            {
                // Expected identifier
                //report.ReportDiagnostic(Code.ExpectedIdentifier, MessageSeverity.Error, tokens.Peek().Source);
                return null;
            }

            // Parse the generic arguments
            GenericArgumentListSyntax genericArguments = ParseGenericArgumentList();

            // Parse array parameters
            ArrayParametersSyntax arrayParameters = ParseArrayParameters();

            // Create type reference
            return new TypeReferenceSyntax(namespaceName, parentTypes, identifier, genericArguments, arrayParameters);
        }

        internal ParentTypeReferenceSyntax[] ParseParentTypeReferences()
        {
            int position = 0;
            ParentTypeReferenceSyntax[] parentTypeReferences = null;

            // Try to parse all references
            while(tokens.EOF == false && tokens.PeekKind() == SyntaxTokenKind.Identifier)
            {
                // Store the current position
                position = tokens.Position;

                // Read the identifier
                SyntaxToken identifier = tokens.Consume();

                // Optional generic arguments
                GenericArgumentListSyntax genericArguments = ParseGenericArgumentList();

                // Now expect dot separator to confirm we are parsing a parent type
                if(tokens.PeekKind() != SyntaxTokenKind.DotSymbol)
                {
                    // Return to prior state
                    tokens.Retrace(position);
                    break;
                }

                // Consume the dot
                SyntaxToken dot = tokens.Consume();

                // Add the reference
                if(parentTypeReferences != null)
                {
                    Array.Resize(ref parentTypeReferences, parentTypeReferences.Length + 1);
                }
                else
                {
                    parentTypeReferences = new ParentTypeReferenceSyntax[1];
                }

                // Add the reference
                parentTypeReferences[parentTypeReferences.Length - 1] = new ParentTypeReferenceSyntax(identifier, genericArguments, dot);
            }

            return parentTypeReferences;
        }


        private ImportSyntax RecoverFromImportError()
        {
            // An error occurred while parsing an import - To recover we should consume all tokens until we reach a semicolon or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.SemicolonSymbol)
                tokens.Consume();

            // Consume the semicolon
            if (tokens.EOF == false)
                tokens.Consume();

            // Always null - just means we can return from calling control easily
            return null;
        }

        private ArrayParametersSyntax RecoverFromArrayParameterError()
        {
            // An error occurred while parsing a array parameters - To recover we should consume all tokens until we reach a rArray or end of stream
            while (tokens.EOF == false && tokens.PeekKind() != SyntaxTokenKind.GreaterSymbol)
                tokens.Consume();

            // Consume the rArray
            if (tokens.EOF == false)
                tokens.Consume();

            // Create error
            return new ArrayParametersSyntax(null);
        }
    }
}
