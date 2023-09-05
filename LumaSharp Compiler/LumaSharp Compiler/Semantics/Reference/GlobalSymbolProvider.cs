using LumaSharp_Compiler.Syntax;
using LumaSharp_Compiler.Syntax.Expression;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal sealed class GlobalSymbolProvider : ISymbolProvider
    {
        // Private
        private ReferenceLibrary thisLibrary = null;

        // Constructor
        public GlobalSymbolProvider(ReferenceLibrary thisLibrary)
        {
            this.thisLibrary = thisLibrary;
        }

        // Methods
        public ITypeReferenceSymbol ResolveTypeSymbol(PrimitiveType primitiveType)
        {
            // Check for void
            if (primitiveType == 0)
                return Types._void;

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

            // Check for  simple types reference

            throw new NotImplementedException();
        }

        public IReferenceSymbol ResolveFieldIdentifierSymbol(IReferenceSymbol context, FieldAccessorReferenceExpressionSyntax reference)
        {
            // Check for type
            if(context is ITypeReferenceSymbol typeReference)
            {
                // Get all matches
                int matchCount = 0;
                IReferenceSymbol matchSymbol = null;

                // Get all fields
                foreach(IFieldReferenceSymbol field in typeReference.FieldMemberSymbols)
                {
                    // Check for matched field name
                    if(field.FieldName == reference.Identifier.Text)
                    {
                        matchCount++;
                        matchSymbol = field;
                    }
                }

                // Get all accessors
                foreach(IAccessorReferenceSymbol accessor in typeReference.AccessorMemberSymbols)
                {
                    // Check for matched accessor name
                    if(accessor.AccessorName == reference.Identifier.Text)
                    {
                        matchCount++;
                        matchSymbol = accessor;
                    }
                }

                // Check for ambiguous match
                if (matchCount > 1)
                    throw new Exception("Ambiguous match: " + typeReference);

                return matchSymbol;
            }

            // 
            throw new InvalidOperationException("Invalid context");
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
