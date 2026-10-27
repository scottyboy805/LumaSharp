using LumaSharp.Compiler;
using LumaSharp.Compiler.AST;

Console.WriteLine("Hello, World!");


SyntaxTree tree = SyntaxTree.Parse(@"class Program
{
    int32 fib(int32 n)
    {
        if(n < 2)
        {   
            return n;
        }
        return fib(n - 1) + fib(n - 2);
    }

    int32 main()
    {
        return fib(15);   
    }
}");

// Create compilation
Compilation compilation = new Compilation(tree);
compilation.Emit();



//int32 x = 456;
//int32 a = 45 + x;
//return a;