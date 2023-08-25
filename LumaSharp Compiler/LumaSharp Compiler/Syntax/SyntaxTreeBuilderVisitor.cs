using Antlr4.Runtime.Misc;

namespace LumaSharp_Compiler.Syntax
{
    internal sealed class SyntaxTreeBuilderVisitor : LumaSharpBaseVisitor<SyntaxNode>
    {
        // Methods
        public override SyntaxNode VisitCompilationUnit([NotNull] LumaSharpParser.CompilationUnitContext context)
        {
            return base.VisitCompilationUnit(context);
        }

        public override SyntaxNode VisitNamespaceDeclaration([NotNull] LumaSharpParser.NamespaceDeclarationContext context)
        {
            return new NamespaceSyntax(null, null, context);
        }

        public override SyntaxNode VisitTypeDeclaration([NotNull] LumaSharpParser.TypeDeclarationContext context)
        {
            return new TypeSyntax(null, null, context);
        }

        public override SyntaxNode VisitFieldDeclaration([NotNull] LumaSharpParser.FieldDeclarationContext context)
        {
            //return FieldSyntax(context);
            throw new NotImplementedException();
        }
    }
}
