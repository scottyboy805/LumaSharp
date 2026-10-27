using LLVMSharp.Interop;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Emit.LLVM;
using LumaSharp.Compiler.Semantics;
using System.Diagnostics;

namespace LumaSharp_Compiler.Emit.LLVM
{
    internal class LLVMEmit
    {
        // Private
        private SemanticModel model;
        private LLVMCodeGen codeGen;

        // Constructor
        public LLVMEmit(SemanticModel semanticModel)
        {
            this.model = semanticModel;
            this.codeGen = new LLVMCodeGen(model);
        }

        // Methods
        public void Emit()
        {
            // Generate all compilation units
            foreach(SyntaxTree synatxTree in model.SyntaxTrees)
            {
                // Get root compilation unit
                CompilationUnitSyntax unit = synatxTree.Root as CompilationUnitSyntax;

                // Emit unit
                codeGen.EmitCompilationUnit(unit);
            }


            // Get the module
            LLVMModuleRef module = codeGen.Module;

            module.Target = "main";

            File.WriteAllText($"main.ll", module.ToString());

            if (module.TryVerify(LLVMVerifierFailureAction.LLVMPrintMessageAction, out string message) == false)
            {
                throw new Exception("LLVM verify failed: " + message);
            }
            else
            {
                File.WriteAllText($"main.ll", module.ToString());

                ProcessStartInfo clang = new ProcessStartInfo()
                {
                    FileName = @"clang.exe",//.\\Llvm\\bin\\clang.exe",
                    Arguments = $"\"{Environment.CurrentDirectory}\\main.ll\" --target=\"i686-pc-windows-msvc\" -O0 -o \"{Environment.CurrentDirectory}\\testing.exe\"",
                };

                Process.Start(clang).WaitForExit();
                Console.ReadKey();
            }
        }

        //public static void CompileLLMV()
        //{
        //    LLVMCodeGen codeGen = new LLVMCodeGen(new([SyntaxTree.Create()]));
        //    codeGen.TestEmit();

        //    // Get the module
        //    LLVMModuleRef module = codeGen.Module;

        //    module.Target = "main";

        //    File.WriteAllText($"main.ll", module.ToString());

        //    if (module.TryVerify(LLVMVerifierFailureAction.LLVMPrintMessageAction, out string message) == false)
        //    {
        //        throw new Exception("LLVM verify failed: " + message);
        //    }
        //    else
        //    {
        //        File.WriteAllText($"main.ll", module.ToString());

        //        ProcessStartInfo clang = new ProcessStartInfo()
        //        {
        //            FileName = @"clang.exe",//.\\Llvm\\bin\\clang.exe",
        //            Arguments = $"\"{Environment.CurrentDirectory}\\main.ll\" --target=\"i686-pc-windows-msvc\" -O0 -o \"{Environment.CurrentDirectory}\\testing.exe\"",
        //        };

        //        Process.Start(clang).WaitForExit();
        //        Console.ReadKey();
        //    }
        //}
    }
}
