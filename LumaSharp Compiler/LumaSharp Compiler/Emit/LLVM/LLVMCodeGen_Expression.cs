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
                case MethodInvokeExpressionSyntax invoke: return EmitMethodInvokeExpression(invoke, functionBuilder);
                case BinaryExpressionSyntax binary: return EmitBinaryExpression(binary, functionBuilder, asPointer);
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
            if (asPointer == true || identifierSymbol is IParameterSymbol)
                return new LLVMValue(variable.TypeRef, variable.ValueRef, true);

            // Load value
            return new LLVMValue(variable.TypeRef, builder.BuildLoad2(variable.TypeRef, variable.ValueRef, identifierSymbol.Name + "_value"), false);
        }

        private LLVMValue EmitMethodInvokeExpression(MethodInvokeExpressionSyntax methodInvoke, LLVMFunctionBuilder functionBuilder)
        {
            // Get symbol
            IMethodSymbol methodSymbol = model.GetSymbol(methodInvoke).Symbol as IMethodSymbol;

            // Get return type
            ITypeSymbol returnTypeSymbol = methodSymbol.GetReturnType();

            // Get parameters
            IReadOnlyList<IParameterSymbol> parameters = methodSymbol.GetParameters();

            // Create arguments
            LLVMValue[] arguments = parameters.Count > 0
                ? new LLVMValue[parameters.Count]
                : [] ;

            // Process arguments
            for(int i = 0; i < parameters.Count; i++)
            {
                // Get parameter symbol
                IParameterSymbol parameterSymbol = parameters[i];

                // Check for by ref
                bool byRef = false;// parameterSymbol.IsIn || parameterSymbol.IsOut || parameterSymbol.IsRef;

                // Check if argument was provided
                bool hasArgument = i <= methodInvoke.ArgumentCount;

                // Get argument or default value
                arguments[i] = hasArgument == true
                    ? EmitExpression(methodInvoke.ArgumentList[i], functionBuilder, byRef)
                    : default;
            }

            // Get return type
            LLVMTypeRef returnType = GetLLVMTypeRef(returnTypeSymbol);

            // Build argument types
            LLVMTypeRef[] parameterTypes = parameters.Select(p => GetLLVMTypeRef(p.GetParameterType())).ToArray();

            // Build argument ref values
            LLVMValueRef[] argumentValues = arguments.Select(a => a.Value).ToArray();

            // Create function type
            LLVMTypeRef functionType = LLVMTypeRef.CreateFunction(returnType, parameterTypes, false);

            // Get the function to call
            LLVMValueRef functionInstance = module.GetNamedFunction(methodSymbol.Name);

            // Build call
            return new LLVMValue(returnType,
                builder.BuildCall2(functionType, functionInstance, argumentValues, methodSymbol.Name + "_invoke"));
        }

        private LLVMValue EmitBinaryExpression(BinaryExpressionSyntax binary, LLVMFunctionBuilder functionBuilder, bool asPointer)
        {
            // Get operation
            IOperation operation = model.GetOperation(binary);

            // Get type
            ITypeSymbol operationTypeSymbol = operation.GetOperationType();

            // Check for floating point
            bool isFloatingPoint = operationTypeSymbol.ReservedType == ReservedType.Float || operationTypeSymbol.ReservedType == ReservedType.Double;
            bool isSignedType = GetLLVMTypeIsSigned(operationTypeSymbol);

            // Get left and right
            LLVMValue left = EmitExpression(binary.Left, functionBuilder, false);
            LLVMValue right = EmitExpression(binary.Right, functionBuilder, false);

            // Get the operation type
            LLVMTypeRef operationType = GetLLVMTypeRef(operationTypeSymbol);

            // Check kind
            switch (operation.Kind)
            {
                case OperationKind.BinaryAdd:
                    {
                        // Check for float
                        return new LLVMValue(operationType, isFloatingPoint == true
                            ? builder.BuildFAdd(left.Value, right.Value, "add.float")
                            : builder.BuildAdd(left.Value, right.Value, "add.integer"));
                    }
                case OperationKind.BinarySubtract:
                    {
                        // Check for float
                        return new LLVMValue(operationType, isFloatingPoint == true
                            ? builder.BuildFSub(left.Value, right.Value, "subtract.float")
                            : builder.BuildSub(left.Value, right.Value, "subtract.integer"));
                    }
                case OperationKind.BinaryLess:
                    {
                        return new LLVMValue(operationType, isFloatingPoint == true
                            ? builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, left.Value, right.Value, "clt.float")
                            : builder.BuildICmp(isSignedType == true ? LLVMIntPredicate.LLVMIntSLT : LLVMIntPredicate.LLVMIntULT, left.Value, right.Value, "clt.integer"));
                    }
            }
            throw new NotSupportedException(operation.Kind.ToString());
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

            // Check for small int - promote to 32bit
            if (constant.ConstantKind == ConstantKind.Integer && typeRef.IntWidth < 32)
                typeRef = LLVMTypeRef.Int32;
            
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
        }
    }
}
