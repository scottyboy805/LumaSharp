using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics
{
    internal sealed partial class SemanticAnalysis
    {
        // Methods
        private void CheckImplicitConversion(ITypeSymbol fromType, ITypeSymbol toType, SyntaxSpan? span)
        {
            // Check for same
            if (fromType == toType)
                return;

            bool hasImplicitConversion = false;

            // Check for built in
            if(fromType.IsReserved == true && toType.IsReserved == true)
            {
                // Get reserved types
                ReservedType fromReservedType = fromType.ReservedType;
                ReservedType toReservedType = toType.ReservedType;

                // Check for conversion
                hasImplicitConversion = fromReservedType switch
                {
                    ReservedType.Int8 => toReservedType == ReservedType.Int16 
                        || toReservedType == ReservedType.Int32
                        || toReservedType == ReservedType.Int64,
                    ReservedType.Int16 => toReservedType == ReservedType.Int32
                        || toReservedType == ReservedType.Int64,
                    ReservedType.Int32 => toReservedType == ReservedType.Int64,
                    ReservedType.UInt8 => toReservedType == ReservedType.UInt16
                        || toReservedType == ReservedType.UInt32
                        || toReservedType == ReservedType.UInt64,
                    ReservedType.UInt16 => toReservedType == ReservedType.UInt32
                        || toReservedType == ReservedType.UInt64,
                    ReservedType.UInt32 => toReservedType == ReservedType.UInt64,
                    ReservedType.Float => toReservedType == ReservedType.Double,
                    _ => false,
                };
            }
            // Check implicit operators
            else
            {
                // Try to get implicit cast
                hasImplicitConversion = toType.GetSpecialOperation(SpecialOperation.ImplicitConvert, toType, fromType) != null;

                // Check for none
                if (hasImplicitConversion == false && fromType.IsReserved == false)
                    hasImplicitConversion = fromType.GetSpecialOperation(SpecialOperation.ImplicitConvert, toType, fromType) != null;
            }

            // Check for implicit conversion
            if (hasImplicitConversion == false)
                diagnostics.Report(Code.NoImplicitConversion, DiagnosticSeverity.Error, span, fromType.Name, toType.Name);
        }
    }
}
