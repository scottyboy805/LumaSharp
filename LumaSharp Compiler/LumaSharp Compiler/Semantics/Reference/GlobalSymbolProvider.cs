﻿using LumaSharp_Compiler.Syntax;
using LumaSharp_Compiler.Syntax.Expression;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal sealed class GlobalSymbolProvider : ISymbolProvider
    {
        // Methods
        public ITypeReferenceSymbol ResolveTypeSymbol(PrimitiveType primitiveType)
        {
            return primitiveType switch
            {
                PrimitiveType.Any => Types.any,
                PrimitiveType.Bool => Types._bool,
                PrimitiveType.Char => Types._char,
                PrimitiveType.String => Types._string,

                PrimitiveType.I8 => Types.i8,
                PrimitiveType.U8 => Types.u8,
                PrimitiveType.I16 => Types.i16,
                PrimitiveType.U16 => Types.u16,
                PrimitiveType.I32 => Types.i32,
                PrimitiveType.U32 => Types.u32,
                PrimitiveType.I64 => Types.i64,
                PrimitiveType.U64 => Types.u64,

                PrimitiveType.Float => Types._float,
                PrimitiveType.Double => Types._double,
            };
        }

        public ITypeReferenceSymbol ResolveTypeSymbol(IReferenceSymbol context, TypeReferenceSyntax reference)
        {
            // Check for primitive
            if (reference.IsPrimitiveType == true)
                return ResolveTypeSymbol(Enum.Parse<PrimitiveType>(reference.Identifier.Text));

            throw new NotImplementedException();
        }

        public IIdentifierReferenceSymbol ResolveFieldIdentifierSymbol(IReferenceSymbol context, FieldAccessorReferenceExpressionSyntax reference)
        {
            throw new NotImplementedException();
        }

        public IIdentifierReferenceSymbol ResolveIdentifierSymbol(IReferenceSymbol context, VariableReferenceExpressionSyntax reference)
        {
            throw new NotImplementedException();
        }

        public IIdentifierReferenceSymbol ResolveMethodIdentifierSymbol(IReferenceSymbol context, MethodInvokeExpressionSyntax reference)
        {
            throw new NotImplementedException();
        }
    }
}