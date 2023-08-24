using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumaSharp_Compiler.Syntax
{
    public abstract class StatementSyntax : SyntaxNode
    {
        // Private
        private SyntaxToken end = null;     // Semicolon
    }
}
