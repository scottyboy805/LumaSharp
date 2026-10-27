using LLVMSharp.Interop;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal sealed partial class LLVMCodeGen
    {
        // Methods
        private void EmitMember(MemberSyntax member, LLVMTypeBuilder typeBuilder)
        {
            // Check kind
            switch(member)
            {
                case ClassSyntax _class: EmitClass(_class, typeBuilder); break;
                case MethodSyntax _method: EmitMethod(_method, typeBuilder);break;
            }
        }

        private void EmitClass(ClassSyntax _class, LLVMTypeBuilder typeBuilder)
        {
            // Emit type here
            LLVMTypeBuilder classType = null;

            // Emit all members
            if (_class.HasMembers == true)
            {
                foreach (MemberSyntax member in _class.Members.Members)
                {
                    // Emit the member
                    EmitMember(member, classType);
                }
            }
        }

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
