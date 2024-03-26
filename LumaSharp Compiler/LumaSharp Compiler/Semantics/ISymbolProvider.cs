using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.AST.Expression;

namespace LumaSharp_Compiler.Semantics
{
    public interface ISymbolProvider
    {
        // Methods
        int GetDeclaredSymbolToken(IReferenceSymbol symbol);

        INamespaceReferenceSymbol ResolveNamespaceSymbol(NamespaceName namespaceName);

        ITypeReferenceSymbol ResolveTypeSymbol(PrimitiveType primitiveType, SyntaxSource source);

        ITypeReferenceSymbol ResolveTypeSymbol(IReferenceSymbol context, TypeReferenceSyntax reference);

        IIdentifierReferenceSymbol ResolveIdentifierSymbol(IReferenceSymbol context, VariableReferenceExpressionSyntax reference);

        IIdentifierReferenceSymbol ResolveFieldIdentifierSymbol(IReferenceSymbol context, FieldAccessorReferenceExpressionSyntax reference);

        IIdentifierReferenceSymbol ResolveMethodIdentifierSymbol(IReferenceSymbol context, MethodInvokeExpressionSyntax reference, ITypeReferenceSymbol[] argumentTypes = null);
    }
}
