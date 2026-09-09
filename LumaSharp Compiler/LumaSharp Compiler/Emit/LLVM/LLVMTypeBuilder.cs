using LLVMSharp.Interop;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal sealed class LLVMField
    {
        // Private
        private readonly LLVMTypeRef type;
        private readonly IFieldSymbol symbol;
    }

    internal sealed class LLVMTypeBuilder
    {
        // Private
        private readonly LLVMTypeRef type;
        private readonly Dictionary<string, LLVMField> fields = new();

        // Properties
        public LLVMTypeRef TypeValue => type;

        // Constructor
        public LLVMTypeBuilder(LLVMModuleRef module, LLVMBuilderRef builder, string name)
        {
            
        }

        // Methods
        public LLVMField GetField(string identifier)
        {
            fields.TryGetValue(identifier, out var field);
            return field;
        }
    }
}
