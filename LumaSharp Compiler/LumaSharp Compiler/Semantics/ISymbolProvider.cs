using LumaSharp_Compiler.Syntax;
using LumaSharp_Compiler.Syntax.Expression;

namespace LumaSharp_Compiler.Semantics
{
    public interface ISymbolProvider
    {
        // Methods
        ITypeReferenceSymbol ResolveTypeSymbol(PrimitiveType primitiveType);

        ITypeReferenceSymbol ResolveTypeSymbol(IReferenceSymbol context, TypeReferenceSyntax reference);

        IIdentifierReferenceSymbol ResolveIdentifierSymbol(IReferenceSymbol context, VariableReferenceExpressionSyntax reference);

        IIdentifierReferenceSymbol ResolveFieldIdentifierSymbol(IReferenceSymbol context, FieldAccessorReferenceExpressionSyntax reference);

        IIdentifierReferenceSymbol ResolveMethodIdentifierSymbol(IReferenceSymbol context, MethodInvokeExpressionSyntax reference);
    }
}
