using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Emit.LLVM;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics;
using LumaSharp_Compiler.Emit.LLVM;
using System;
using System.Collections.Generic;
using System.Text;

namespace LumaSharp.Compiler
{
    public sealed class Compilation
    {
        // Private
        private SyntaxTree[] syntaxTrees;
        private CompilerDiagnosticCollection diagnostics = new();

        // Properties
        public IReadOnlyList<SyntaxTree> SyntaxTrees => syntaxTrees;

        // Constructor
        public Compilation(SyntaxTree syntaxTree)
        {
            // Check for any
            if(syntaxTree == null)
                throw new ArgumentNullException(nameof(syntaxTree));

            this.syntaxTrees = [syntaxTree];
        }

        public Compilation(IEnumerable<SyntaxTree> syntaxTrees)
        {
            // Check for null
            if(syntaxTrees == null)
                throw new ArgumentNullException(nameof(syntaxTrees));

            // Get syntax trees
            this.syntaxTrees = syntaxTrees
                .Where(t => t != null)
                .ToArray();

            // Check for any
            if (this.syntaxTrees.Length == 0)
                throw new ArgumentException("No syntax trees provided");
        }

        // Methods
        public void Emit()
        {
            // Check for any errors

            // Create semantic model
            SemanticModel model = new(syntaxTrees);

            // Create semantic analysis
            SemanticAnalysis analysis = new SemanticAnalysis(model, diagnostics);

            // Analyze all syntax trees
            foreach (SyntaxTree syntaxTree in syntaxTrees)
                syntaxTree.Root.Accept(analysis);

            // Create llvm emit
            LLVMEmit llvm = new(model);

            // Emit the binary
            llvm.Emit();
        }
    }
}
