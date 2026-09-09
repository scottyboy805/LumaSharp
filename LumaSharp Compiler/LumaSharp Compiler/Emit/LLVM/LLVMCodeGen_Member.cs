using LLVMSharp.Interop;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal sealed partial class LLVMCodeGen
    {
        // Methods
        private void EmitMethod(MethodSyntax method, LLVMTypeBuilder typeBuilder)
        {
            // Get the method symbol
            IMethodSymbol symbol = model.GetSymbol(method).Symbol as IMethodSymbol;

            // Get the parameter symbols
            IReadOnlyList<IParameterSymbol> parameterSymbols = symbol.GetParameters();

            // Create return
            LLVMTypeRef returnType = GetLLVMTypeRef(symbol.GetReturnType());

            // Create parameters
            int parameterCount = symbol.ParameterCount;

            // Create parameters
            LLVMParameter[] parameters = new LLVMParameter[parameterCount];

            // Init parameters
            for(int i = 0; i < parameterCount; i++)
            {
                // Create the parameter
                parameters[i] = new LLVMParameter(parameterSymbols[i], parameterSymbols[i].GetParameterType());
            }

            // Create function builder
            LLVMFunctionBuilder functionBuilder = new (module, builder, symbol.Name, returnType, parameters);

            // Emit all statements
            if(method.HasBody == true)
            {
                // Emit the body
                EmitStatement(method.Body, functionBuilder);
            }

            // Check for lambda
            //if(method.HasLambda == true)
            //{
            //    method.Lambda.
            //}
        }
    }
}
