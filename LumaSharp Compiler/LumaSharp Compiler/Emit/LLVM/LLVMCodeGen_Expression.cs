using LLVMSharp.Interop;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal sealed partial class LLVMCodeGen
    {
        // Methods
        private LLVMValue EmitExpression(ExpressionSyntax expression, LLVMFunctionBuilder functionBuilder, bool asPointer)
        {
            switch(expression)
            {
                case VariableReferenceExpressionSyntax variable: return EmitVariableReferenceExpression(variable, functionBuilder, asPointer);
                case SizeofExpressionSyntax size: return EmitSizeofExpression(size);
                case LiteralExpressionSyntax literal: return EmitLiteralExpression(literal);
                default: throw new NotSupportedException(expression.ToString());
            }
        }

        private LLVMValue EmitVariableReferenceExpression(VariableReferenceExpressionSyntax variableReference, LLVMFunctionBuilder functionBuilder, bool asPointer)
        {
            // Get symbol
            ISymbol identifierSymbol = model.GetSymbol(variableReference).Symbol;

            // Resolve variable
            LLVMVariable variable = functionBuilder.GetVariable(identifierSymbol);

            // Check for pointer
            if (asPointer == true)
                return new LLVMValue(variable.TypeRef, variable.ValueRef, true);

            // Load value
            return new LLVMValue(variable.TypeRef, builder.BuildLoad2(variable.TypeRef, variable.ValueRef, identifierSymbol.Name + "_value"), false);

            //// Get identifier
            //string identifier = variableReference.Identifier.Text;

            //// Check for parameter first
            //LLVMParameter parameter = null;
            //if((parameter = functionBuilder.GetParameter(identifier)) != null)
            //{
            //    // Check for pointer
            //    if (asPointer == true)
            //        return new LLVMValue(parameter.ParameterType, parameter.ParameterValue, true);

            //    // Bind to parameter
            //    return new LLVMValue(
            //        parameter.ParameterType,
            //        builder.BuildLoad2(parameter.ParameterType, parameter.ParameterValue, identifier + "_value"));
            //}

            //// Check for local
            //LLVMVariable local = null;
            //if((local = functionBuilder.GetVariable(identifier)) != null)
            //{
            //    // Check for pointer
            //    if (asPointer == true)
            //        return new LLVMValue(local.TypeRef, local.ValueRef);

            //    // Bind to local
            //    return new LLVMValue(
            //        local.TypeRef,
            //        builder.BuildLoad2(local.TypeRef, local.ValueRef, identifier + "_value"));
            //}
            throw new NotSupportedException();
        }

        //private LLVMValue EmitIndexExpression(IndexExpressionSyntax indexExpression, LLVMFunctionBuilder functionBuilder, bool asPointer)
        //{
        //    // Get access expression
        //    LLVMValue accessValue = EmitExpression(indexExpression.AccessExpression, functionBuilder, true);

        //    // Get index
        //    LLVMValueRef[] indexValues = indexExpression.IndexExpressions.Select(i => EmitExpression(i, functionBuilder, false).Value).ToArray();

        //    // Get bounds checked access
        //    return new LLVMValue(default, 
        //        builder.BuildInBoundsGEP2(default,
        //        accessValue.Value, indexValues, "array.index");
        //}

        private LLVMValue EmitSizeofExpression(SizeofExpressionSyntax sizeofExpression)
        {
            // Get type symbol
            ITypeSymbol typeSymbol = model.GetTypeSymbol(sizeofExpression);

            // Get the type
            LLVMTypeRef typeRef = GetLLVMTypeRef(typeSymbol);

            // Emit sizeof
            return new LLVMValue(typeRef.SizeOf.TypeOf, typeRef.SizeOf);
        }

        private LLVMValue EmitLiteralExpression(LiteralExpressionSyntax literalExpression)
        {
            // Get literal type
            ITypeSymbol type = model.GetTypeSymbol(literalExpression);

            // Get literal symbol
            IConstantSymbol constant = model.GetConstantSymbol(literalExpression);

            // Get llvm type
            LLVMTypeRef typeRef = GetLLVMTypeRef(type);

            // Create value
            LLVMValueRef valueRef = constant.ConstantKind switch
            {
                ConstantKind.Boolean => LLVMValueRef.CreateConstInt(typeRef, constant.BooleanValue.Value ? 1ul : 0),
                ConstantKind.Integer => LLVMValueRef.CreateConstInt(typeRef, constant.IntegerValue.Value),
                ConstantKind.Decimal => LLVMValueRef.CreateConstReal(typeRef, constant.DecimalValue.Value),
                ConstantKind.String => builder.BuildGlobalStringPtr(constant.StringValue),

                _ => throw new NotSupportedException(constant.ConstantKind.ToString()),
            };

            // Create value
            return new LLVMValue(typeRef, valueRef, constant.ConstantKind == ConstantKind.String);

            // Create the literal type

            //// Check kind
            //switch(constant.ConstantKind)
            //{
            //    //case ConstantKind.Null:
            //    //    {
            //    //        return LLVMValueRef.CreateConstNull();
            //    //    }
            //    case ConstantKind.Boolean:
            //        {
            //            // Get bool value
            //            bool value = constant.BooleanValue.Value;

            //            // Create const int I1
            //            return new LLVMValue(
            //                LLVMTypeRef.Int1,
            //                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, value == true ? 1ul : 0));
            //        }
            //    case ConstantKind.Integer:
            //        {
            //            // Get int value
            //            ulong integerValue = constant.IntegerValue.Value;

            //            // Check for int8
            //            if (integerValue < byte.MaxValue)
            //            {
            //                return new LLVMValue(
            //                    LLVMTypeRef.Int8,
            //                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, integerValue));
            //            }

            //            // Check for int 16
            //            if (integerValue < ushort.MaxValue)
            //            {
            //                return new LLVMValue(
            //                    LLVMTypeRef.Int16,
            //                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, integerValue));
            //            }

            //            // Check for int 32
            //            if (integerValue < uint.MaxValue)
            //            {
            //                return new LLVMValue(
            //                    LLVMTypeRef.Int32,
            //                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, integerValue));
            //            }

            //            // Must be 64
            //            return new LLVMValue(
            //                LLVMTypeRef.Int64,
            //                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, integerValue));
            //        }
            //    case ConstantKind.Decimal:
            //        {
            //            // Get decimal value
            //            double decimalValue = constant.DecimalValue.Value;

            //            // Check for float
            //            if (decimalValue >= float.MinValue && decimalValue <= float.MaxValue)
            //            {
            //                return new LLVMValue(
            //                    LLVMTypeRef.Float,
            //                    LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, decimalValue));
            //            }

            //            // Must be 64
            //            return new LLVMValue(
            //                LLVMTypeRef.Double,
            //                LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, decimalValue));
            //        }
            //    case ConstantKind.String:
            //        {
            //            // Create global string
            //            return new LLVMValue(
            //                LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
            //                builder.BuildGlobalStringPtr(literalExpression.Value.Text), true);
            //        }
            //}

            
        }
    }
}
