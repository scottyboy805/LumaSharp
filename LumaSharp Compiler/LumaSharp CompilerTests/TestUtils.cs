using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.AST.Visitor;
using LumaSharp.Compiler.Parser;
using LumaSharp.Compiler.Reporting;
using System.Diagnostics;

namespace CompilerTests
{
    public static class TestUtils
    {
        // Methods
        public static CompilationUnitSyntax ParseInputStringCompilationUnit(string input)
        {
            return CreateParser(input, p => p.ParseCompilationUnit());
        }

        public static StatementSyntax ParseInputStringStatement(string input)
        {
            return CreateParser(input, p => p.ParseStatement());
        }

        internal static T ParseInputStringStatement<T>(string input, Func<ASTParser, T> parseStatement) where T : StatementSyntax
        {
            return CreateParser(input, p => parseStatement(p));
        }

        public static ExpressionSyntax ParseInputStringExpression(string input)
        {
            return CreateParser(input, p => p.ParseExpression());
        }

        internal static T ParseInputStringExpression<T>(string input, Func<ASTParser, T> parseExpression) where T : ExpressionSyntax
        {
            return CreateParser(input, p => parseExpression(p));
        }

        public static MemberSyntax ParseMemberDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseMember());
        }

        public static TypeSyntax ParseTypeDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseTypeDeclaration());
        }

        public static ContractSyntax ParseContractDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseContractDeclaration());
        }

        public static EnumSyntax ParseEnumDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseEnumDeclaration());
        }

        public static FieldSyntax ParseFieldDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseFieldDeclaration());
        }

        public static AccessorSyntax ParseAccessorDeclaration(string input)
        {
            return CreateParser(input, p => p.ParseAccessorDeclaration());
        }

        public static TypeReferenceSyntax ParseTypeReference(string input)
        {
            return CreateParser(input, p => p.ParseTypeReference());
        }

        public static SeparatedTokenList ParseSeparatedTokenList(string input, SyntaxTokenKind separatorKind, SyntaxTokenKind valueKind, bool requireTrailingSeparator)
        {
            return CreateParser(input, p => p.ParseSeparatedTokenList(separatorKind, valueKind, requireTrailingSeparator));
        }

        public static SeparatedSyntaxList<ExpressionSyntax> ParseSeparatedExpressionList(string input, SyntaxTokenKind separatorKind, SyntaxTokenKind endTokenKind = SyntaxTokenKind.Invalid)
        {
            return CreateParser(input, p => p.ParseSeparatedSyntaxList(p.ParseOptionalExpression, separatorKind, endTokenKind));
        }

        private static T CreateParser<T>(string input, Func<ASTParser, T> parse) where T : SyntaxNode
        {
            // Create the report
            CompileReport report = new CompileReport();

            // Create the text view
            TextView textView = new TextView(new StringReader(input));
            
            // Create the token parser
            TokenParser tokenParser = new TokenParser(textView, null);

            // Create parser
            ASTParser parser = new ASTParser(tokenParser.GetEnumerator(), report);

            // Try to parse
            T result = parse(parser);

            // Log the node
            if (result != null)
                Debug.WriteLine(result.GetSourceText());

            // Check for errors
            foreach(ICompileDiagnostic message in report.Diagnostics)
            {
                Debug.WriteLine(message);
            }

            // Write syntax
            if(result is ExpressionSyntax e)
            {
                PrintSyntaxTreeWalker print = new();
                e.Accept(print);
                Debug.WriteLine(print.Text);
            }

            return result;
        }
    }
}
