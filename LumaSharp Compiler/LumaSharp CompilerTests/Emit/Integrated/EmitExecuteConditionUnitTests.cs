using LumaSharp_Compiler.AST.Factory;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Emit.Builder;
using LumaSharp_Compiler.Semantics.Model.Statement;
using LumaSharp_Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LumaSharp.Runtime;
using LumaSharp.Runtime.Emit;

namespace LumaSharp_CompilerTests.Emit.Integrated
{
    [TestClass]
    public unsafe sealed class EmitExecuteConditionUnitTests
    {
        [TestMethod]
        public void EmitExecuteCondition_Simple()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Type("Test").WithMembers(
                Syntax.Method("Test", Syntax.TypeReference(PrimitiveType.I32))
                .WithStatements(Syntax.Condition(Syntax.Literal(true))
                .WithInlineStatement(Syntax.Return(Syntax.Literal(5))))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);
            ConditionModel conditionModel = model.DescendantsOfType<ConditionModel>(true).FirstOrDefault();

            Assert.IsNotNull(model);
            Assert.IsNotNull(conditionModel);
            Assert.AreEqual(0, model.Report.MessageCount);

            // Build instructions
            MemoryStream stream = new MemoryStream();
            InstructionBuilder builder = new InstructionBuilder(new BinaryWriter(stream));
            new MethodBodyBuilder(new StatementModel[] { conditionModel }).BuildEmitObject(builder);

            Assert.IsTrue(builder.InstructionIndex >= 2);
            Assert.AreEqual(OpCode.Ld_I4_1, builder[0].opCode);
            Assert.AreEqual(OpCode.Jmp_0, builder[1].opCode);


            // Execute code
            __memory.InitStack();
            __interpreter.ExecuteBytecode(stream.ToArray());

            // Get value on top of stack
            Assert.AreEqual(5, __interpreter.FetchValue<int>());
        }
    }
}
