using LumaSharp.Compiler.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTests.AST.Parse.Complete
{
    [TestClass]
    public class BubbleSort
    {
        // Private
        private const string source = @"
import Collections:Generic;

export type Main
{
    List<i32> unsortedValues;

    global void BubbleSort(List<i32> values)
    {
        i32 temp = 0;
        
        for i32 i = 0; i < values.Count; i++
        {
            for i32 j = 0; j < values.Count - 1; j++
            {
                if values[j] > values[j + 1]
                {
                    temp = values[j + 1];
                    values[j + 1] = values[j];
                    values[j] = temp;
                }
            }
        }
    }
}";

        // Methods
        [DataTestMethod]
        [DataRow(source)]
        public void ParseAsBubbleSort(string input)
        {
            // Try to parse the tree
            CompilationUnitSyntax unit = TestUtils.ParseInputStringCompilationUnit(input);

            Assert.IsNotNull(unit);
            Assert.IsInstanceOfType(unit, typeof(CompilationUnitSyntax));
        }
    }
}
