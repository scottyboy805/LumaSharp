using LLVMSharp.Interop;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal readonly struct LLVMParameter
    {
        // Private
        private readonly ISymbol declarationSymbol;
        private readonly ITypeSymbol typeSymbol;

        // Properties
        public readonly ISymbol DeclarationSymbol => declarationSymbol;
        public readonly ITypeSymbol TypeSymbol => typeSymbol;

        // Constructor
        public LLVMParameter(ISymbol declarationSymbol, ITypeSymbol typeSymbol)
        {
            this.declarationSymbol = declarationSymbol;
            this.typeSymbol = typeSymbol;
        }
    }

    internal readonly struct LLVMVariable
    {
        // Private
        private readonly LLVMTypeRef type;
        private readonly LLVMValueRef value;
        private readonly ISymbol declarationSymbol;
        private readonly ITypeSymbol typeSymbol;

        // Properties
        public readonly LLVMTypeRef TypeRef => type;
        public readonly LLVMValueRef ValueRef => value;
        public readonly ISymbol DeclarationSymbol => declarationSymbol;
        public readonly ITypeSymbol TypeSymbol => typeSymbol;

        // Constructor
        internal LLVMVariable(LLVMTypeRef type, LLVMValueRef value, ISymbol declarationSymbol, ITypeSymbol typeSymbol)
        {
            this.type = type;
            this.value = value;
            this.declarationSymbol = declarationSymbol;
            this.typeSymbol = typeSymbol;
        }
    }

    internal sealed class LLVMFunctionBuilder
    {
        // Private
        private readonly LLVMValueRef function;
        private readonly Dictionary<ISymbol, LLVMVariable> variables = new();

        // Properties
        public LLVMValueRef FunctionValue => function;

        // Constructor
        public LLVMFunctionBuilder(LLVMModuleRef module, LLVMBuilderRef builder, string name, LLVMTypeRef returnType, LLVMParameter[] parameters)
        {
            // Create the function
            LLVMTypeRef functionType = LLVMTypeRef.CreateFunction(returnType, parameters
                .Select(p => LLVMCodeGen.GetLLVMTypeRef(p.TypeSymbol))
                .ToArray(), false);

            // Add the function
            this.function = module.AddFunction(name, functionType);
            
            // Build parameters
            for (int i = 0; i < parameters.Length; i++)
            {
                // Get parameter type
                LLVMTypeRef paramTypeRef = LLVMCodeGen.GetLLVMTypeRef(parameters[i].TypeSymbol);

                // Get parameter value
                LLVMValueRef paramValueRef = function.GetParam((uint)i);

                // Create parameter
                LLVMVariable parameter = new (paramTypeRef, paramValueRef, parameters[i].DeclarationSymbol, parameters[i].TypeSymbol);

                // Register parameter
                variables[parameter.DeclarationSymbol] = parameter;
            }

            // Create body
            LLVMBasicBlockRef body = function.AppendBasicBlock("body");

            // Position at end
            builder.PositionAtEnd(body);
        }

        // Methods
        public void DeclareLocal(LLVMTypeRef variableType, LLVMValueRef variableValue, IVariableSymbol declarationSymbol, ITypeSymbol typeSymbol)
        {
            // Create the local
            LLVMVariable local = new(variableType, variableValue, declarationSymbol, typeSymbol);

            // Register variable
            variables[declarationSymbol] = local;
        }

        public LLVMVariable GetVariable(ISymbol symbol)
        {
            if (variables.TryGetValue(symbol, out var variable) == false)
                throw new Exception("Variable could not be resolved for symbol: " + symbol);

            return variable;
        }
    }
}
