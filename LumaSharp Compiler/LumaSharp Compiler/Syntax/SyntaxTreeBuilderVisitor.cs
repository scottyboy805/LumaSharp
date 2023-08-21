using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumaSharp_Compiler.Syntax
{
    internal class SyntaxTreeBuilderVisitor : LumaSharpBaseVisitor<SyntaxNode>
    {
        public override SyntaxNode VisitFieldDeclaration([NotNull] LumaSharpParser.FieldDeclarationContext context)
        {
            return base.VisitFieldDeclaration(context);
        }
    }
}
