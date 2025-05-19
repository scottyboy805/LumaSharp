using LumaSharp.Compiler.AST;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics
{
    public interface ISymbolProvider
    {
        // Methods
        _TokenHandle GetDeclaredSymbolToken(IReferenceSymbol symbol);

        INamespaceReferenceSymbol ResolveNamespaceSymbol(SeparatedTokenList namespaceName);

        ITypeReferenceSymbol ResolveTypeSymbol(PrimitiveType primitiveType, SyntaxSource? source);

        ITypeReferenceSymbol ResolveTypeSymbol(IReferenceSymbol context, TypeReferenceSyntax reference);

        IIdentifierReferenceSymbol ResolveIdentifierSymbol(IReferenceSymbol context, VariableReferenceExpressionSyntax reference);

        IIdentifierReferenceSymbol ResolveFieldIdentifierSymbol(IReferenceSymbol context, MemberAccessExpressionSyntax reference);

        IIdentifierReferenceSymbol ResolveMethodIdentifierSymbol(IReferenceSymbol context, MethodInvokeExpressionSyntax reference, ITypeReferenceSymbol[] argumentTypes = null);
    }
}
