using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LumaSharp_CompilerTests.ParseTree.Statements
{
    [TestClass]
    public class LocalVariableStatementTests
    {
        [TestMethod]
        public void LocalVariable_Int()
        {
            string input = "i8 var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("i8", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_Int_Ref()
        {
            string input = "i8& var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("i8&", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_Int_Array_1()
        {
            string input = "i8[] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);
            
            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("i8[]", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_Int_Array_1_Ref()
        {
            string input = "i8[]& var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("i8[]&", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_String()
        {
            string input = "string var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("string", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_String_Ref()
        {
            string input = "string& var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("string&", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType()
        {
            string input = "MyType var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Ref()
        {
            string input = "MyType& var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType&", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserNestedType()
        {
            string input = "MyType.MySubType var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType.MySubType", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserNestedType_Ref()
        {
            string input = "MyType.MySubType& var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType.MySubType&", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_1()
        {
            string input = "MyType<T> var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType<T>", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_1_Ref()
        {
            string input = "MyType<T>& var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType<T>&", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_2()
        {
            string input = "MyType<T0, T1> var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            LumaSharpParser.TypeReferenceContext type = context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild<LumaSharpParser.TypeReferenceContext>(0);

            Assert.AreEqual("MyType", type.IDENTIFIER().GetText());
            Assert.AreEqual("T0", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(0).GetText());
            Assert.AreEqual("T1", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(1).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).IDENTIFIER().GetText());
        }

        //[TestMethod]
        //public void LocalVariable_UserType_Generic_2_Ref()
        //{
        //    string input = "MyType<T0, T1>& var1;";
        //    LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

        //    // Check for valid
        //    Assert.IsNotNull(context);
        //    LumaSharpParser.TypeReferenceContext type = context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild<LumaSharpParser.TypeReferenceContext>(0);

        //    Assert.AreEqual("MyType", type.IDENTIFIER().GetText());
        //    Assert.AreEqual("T0", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(0).GetText());
        //    Assert.AreEqual("T1", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(1).GetText());
        //    Assert.AreEqual("&", type.@ref.Text);
        //    Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).IDENTIFIER()[0].GetText());
        //}

        [TestMethod]
        public void LocalVariable_UserTypeNested_Generic_2()
        {
            string input = "MyType.MySubType<T0, T1> var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            LumaSharpParser.TypeReferenceContext type = context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild<LumaSharpParser.TypeReferenceContext>(0);

            Assert.AreEqual("MyType", type.parentTypeReference(0).GetText());
            Assert.AreEqual("MySubType", type.IDENTIFIER().GetText());
            Assert.AreEqual("T0", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(0).GetText());
            Assert.AreEqual("T1", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(1).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).IDENTIFIER().GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Array_0()
        {
            string input = "MyType[] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType[]", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Array_1()
        {
            string input = "MyType[,] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType[,]", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Array_2()
        {
            string input = "MyType[,,] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType[,,]", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_1_Array_1()
        {
            string input = "MyType<T>[] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType<T>[]", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_1_Array_2()
        {
            string input = "MyType<T>[,] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType<T>[,]", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_1_Array_3()
        {
            string input = "MyType<T>[,,] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType<T>[,,]", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_2_Array_1()
        {
            string input = "MyType<T0, T1>[] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            LumaSharpParser.TypeReferenceContext type = context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild<LumaSharpParser.TypeReferenceContext>(0);

            Assert.AreEqual("MyType", type.IDENTIFIER().GetText());
            Assert.AreEqual("T0", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(0).GetText());
            Assert.AreEqual("T1", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(1).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).IDENTIFIER().GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_2_Array_2()
        {
            string input = "MyType<T0, T1>[,] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            LumaSharpParser.TypeReferenceContext type = context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild<LumaSharpParser.TypeReferenceContext>(0);

            Assert.AreEqual("MyType", type.IDENTIFIER().GetText());
            Assert.AreEqual("T0", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(0).GetText());
            Assert.AreEqual("T1", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(1).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).IDENTIFIER().GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Generic_2_Array_3()
        {
            string input = "MyType<T0, T1>[,,] var1;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            LumaSharpParser.TypeReferenceContext type = context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild<LumaSharpParser.TypeReferenceContext>(0);

            Assert.AreEqual("MyType", type.IDENTIFIER().GetText());
            Assert.AreEqual("T0", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(0).GetText());
            Assert.AreEqual("T1", type.genericArgumentList().GetChild<LumaSharpParser.TypeReferenceContext>(1).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).IDENTIFIER().GetText());
        }

        [TestMethod]
        public void LocalVariable_Int_Assign()
        {
            string input = "i8 var1 = 5;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("i8", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
            Assert.AreEqual("5", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(2).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_String_Assign()
        {
            string input = @"string var1 = ""Hello"";";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("string", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
            Assert.AreEqual(@"""Hello""", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(2).GetChild(1).GetText());
        }

        [TestMethod]
        public void LocalVariable_UserType_Assign()
        {
            string input = "MyType var1 = SomeVar;";
            LumaSharpParser.StatementContext context = TestUtils.ParseInputStringStatement(input);

            // Check for valid
            Assert.IsNotNull(context);
            Assert.AreEqual("MyType", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(0).GetText());
            Assert.AreEqual("var1", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(1).GetText());
            Assert.AreEqual("SomeVar", context.GetChild<LumaSharpParser.LocalVariableStatementContext>(0).GetChild(2).GetChild(1).GetText());
        }
    }
}
