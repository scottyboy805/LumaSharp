using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;
using LumaSharp.Compiler.Semantics.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.Semantic.Reporting
{
    [TestClass]
    public sealed class ReportMemberMessagesUnitTests
    {
        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_ContractGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_ContractExport()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_ContractHidden()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.HiddenKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_ContractInternal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Contract("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }


        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_EnumGlobal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.GlobalKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_EnumExport()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.ExportKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_EnumHidden()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.HiddenKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }

        [TestMethod]
        public void ReportMemberMessages_ModifierInvalid_EnumInternal()
        {
            SyntaxTree tree = SyntaxTree.Create(
                Syntax.Enum("Test").WithMembers(Syntax.Field("myField", Syntax.TypeReference("Test"))
                .WithAccessModifiers(Syntax.Token(SyntaxTokenKind.InternalKeyword))));

            // Create model
            SemanticModel model = SemanticModel.BuildModel("Test", new SyntaxTree[] { tree }, null);

            Console.WriteLine(model.Report);

            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Report.MessageCount);
            Assert.AreEqual((int)Code.AccessModifierNotValid, model.Report.Messages.First().Code);
        }
    }
}
