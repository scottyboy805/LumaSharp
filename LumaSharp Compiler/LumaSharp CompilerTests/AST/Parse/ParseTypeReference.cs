using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse
{
    [TestClass]
    public class ParseTypeReference
    {
        // Methods
        [DataTestMethod]
        [DataRow("void")]
        [DataRow("any")]
        [DataRow("char")]
        [DataRow("string")]
        [DataRow("bool")]
        [DataRow("i8")]
        [DataRow("u8")]
        [DataRow("i16")]
        [DataRow("u16")]
        [DataRow("i32")]
        [DataRow("u32")]
        [DataRow("i64")]
        [DataRow("u64")]
        [DataRow("f32")]
        [DataRow("f64")]
        [DataRow("identifier")]
        [DataRow("Namespace:identifier")]
        [DataRow("Name:Space:namedType")]
        [DataRow("myType.nested")]
        [DataRow("root.middle.end")]
        [DataRow("generic<i32>")]
        [DataRow("nested<string>.generic<i32>")]
        [DataRow("multiGeneric<i32, string, char>")]
        [DataRow("arr[]")]
        [DataRow("multiArr[,]")]
        [DataRow("finalArr[,,]")]
        public void ParseAsTypeReference(string input)
        {
            // Try to parse the tree
            TypeReferenceSyntax type = TestUtils.ParseTypeReference(input);

            Assert.IsNotNull(type);
            Assert.IsInstanceOfType(type, typeof(TypeReferenceSyntax));
        }
    }
}
