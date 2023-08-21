using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumaSharp_Compiler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string input = "";
            AntlrInputStream inputStream = new AntlrInputStream(input);
            LumaSharpLexer lexer = new LumaSharpLexer(inputStream);
            LumaSharpParser parser = new LumaSharpParser( new CommonTokenStream(lexer));

            
        }
    }
}
