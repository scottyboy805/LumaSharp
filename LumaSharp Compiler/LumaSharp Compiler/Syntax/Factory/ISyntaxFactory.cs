
namespace LumaSharp_Compiler.Syntax.Factory
{
    public interface ISyntaxFactory
    {
        // Properties
        SyntaxTree Tree { get; }

        SyntaxNode Parent { get; }
    }
}
