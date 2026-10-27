using LLVMSharp.Interop;
using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit.LLVM
{
    internal sealed partial class LLVMCodeGen
    {
        // Methods
        private void EmitStatement(StatementSyntax statement, LLVMFunctionBuilder functionBuilder)
        {
            switch(statement)
            {
                case EmptyStatementSyntax: return;
                case StatementBlockSyntax statementBlock:
                    {
                        // Emit all statements
                        foreach (StatementSyntax innerStatement in statementBlock)
                            EmitStatement(innerStatement, functionBuilder);
                        break;
                    }

                case VariableDeclarationStatementSyntax variableDeclarationStatement: EmitVariableStatement(variableDeclarationStatement, functionBuilder); break;
                case AssignStatementSyntax assignStatement: EmitAssignStatement(assignStatement, functionBuilder); break;
                case ReturnStatementSyntax returnStatement: EmitReturnStatement(returnStatement, functionBuilder); break;
                case IfStatementSyntax ifStatement: EmitIfStatement(ifStatement, functionBuilder); break;
                case WhileStatement whileStatement: EmitWhileStatement(whileStatement, functionBuilder); break;
            }
        }

        private void EmitVariableStatement(VariableDeclarationStatementSyntax variableDeclaration, LLVMFunctionBuilder functionBuilder)
        {
            // Get symbol
            SymbolInfo variableSymbol = model.GetSymbol(variableDeclaration.Variable);

            // Get type symbol
            ITypeSymbol typeSymbol = model.GetTypeSymbol(variableDeclaration.VariableType);

            // Get variable type
            LLVMTypeRef variableType = GetLLVMTypeRef(typeSymbol);

            // Process all identifiers
            for(int i = 0; i < variableDeclaration.Identifiers.Elements.Count; i++)
            {
                // Get the identifier name
                string identifier = variableDeclaration.Identifiers.Elements[i].Token.Text;

                // Create local variable
                LLVMValueRef localVar = builder.BuildAlloca(variableType, identifier);

                // Get symbol
                IVariableSymbol symbol = (variableSymbol.CandidateSymbols != null
                    ? variableSymbol.CandidateSymbols[i] : variableSymbol.Symbol) as IVariableSymbol;

                // Define the local
                functionBuilder.DeclareLocal(variableType, localVar, symbol, typeSymbol);

                // Check for assignment
                if (variableDeclaration.HasAssignment == true && i < variableDeclaration.Assignment.AssignExpressions.Count)
                {
                    // Get assign expression
                    LLVMValue localAssign = EmitExpression(variableDeclaration.Assignment.AssignExpressions[i], functionBuilder, false);

                    // Convert to type
                    LLVMValueRef localValue = GetLLVMCoerceValue(variableType, localAssign.Value, "local.convert");

                    // Store variable
                    builder.BuildStore(localValue, localVar);
                }
            }
        }

        private void EmitAssignStatement(AssignStatementSyntax assign, LLVMFunctionBuilder functionBuilder)
        {
            // Emit assign all
            for(int i = 0; i < assign.Left.Count; i++)
            {
                // Emit left
                LLVMValue leftValue = EmitExpression(assign.Left[i], functionBuilder, true);

                // Emit right
                LLVMValue rightValue = EmitExpression(assign.Right[i], functionBuilder, false);

                // Convert
                LLVMValueRef assignValue = GetLLVMCoerceValue(leftValue.Type, rightValue.Value, "assign.convert");

                // Emit assign
                builder.BuildStore(assignValue, leftValue.Value);
            }
        }

        private void EmitReturnStatement(ReturnStatementSyntax returnStatement, LLVMFunctionBuilder functionBuilder)
        {
            // Check for void
            if (returnStatement.HasExpression == true)
            {
                // Get the return expression
                LLVMValue returnVal = EmitExpression(returnStatement.Expression, functionBuilder, false);

                // Convert to return type
                LLVMValueRef value = GetLLVMCoerceValue(functionBuilder.FunctionValue.ReturnType, returnVal.Value, "return.convert");

                // Value return
                builder.BuildRet(value);
            }
            else 
            {
                // Void return
                builder.BuildRetVoid();
            }
        }

        private void EmitIfStatement(IfStatementSyntax ifStatement, LLVMFunctionBuilder functionBuilder)
        {
            // Emit expression
            LLVMValue conditionValue = EmitExpression(ifStatement.Condition, functionBuilder, false);

            // Check for bool result
            if(conditionValue.Type.IntWidth != 1)
            {
                // Convert to compare result
                conditionValue = new LLVMValue(
                    conditionValue.Type,
                    builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, conditionValue.Value, LLVMValueRef.CreateConstInt(conditionValue.Type, 0, false), "if.condition.convert"),
                    conditionValue.IsPointer);
            }

            // Create then block
            LLVMBasicBlockRef thenBlock = functionBuilder.FunctionValue.AppendBasicBlock("if.then");
            LLVMBasicBlockRef? elseBlock = ifStatement.HasAlternate == true ? functionBuilder.FunctionValue.AppendBasicBlock("if.else") : null;
            LLVMBasicBlockRef endBlock = functionBuilder.FunctionValue.AppendBasicBlock("if.end");

            // Build condition
            builder.BuildCondBr(conditionValue.Value, thenBlock, ifStatement.HasAlternate ? elseBlock.Value : endBlock);

            // Emit true body
            builder.PositionAtEnd(thenBlock);
            EmitStatement(ifStatement.Statement, functionBuilder);

            // Alternate
            if (builder.InsertBlock.Terminator == null)
                builder.BuildBr(endBlock);

            // Emit else
            if(ifStatement.HasAlternate == true)
            {
                // Emit alternate statement
                builder.PositionAtEnd(elseBlock.Value);
                EmitStatement(ifStatement.Alternate.Statement, functionBuilder);

                // Break to end
                if (builder.InsertBlock.Terminator == null)
                    builder.BuildBr(endBlock);
            }

            builder.PositionAtEnd(endBlock);
        }

        private void EmitWhileStatement(WhileStatement whileStatement, LLVMFunctionBuilder functionBuilder)
        {
            LLVMBasicBlockRef loopConditionBlock = functionBuilder.FunctionValue.AppendBasicBlock("while.condition");
            LLVMBasicBlockRef loopBodyBlock = functionBuilder.FunctionValue.AppendBasicBlock("while.body");
            LLVMBasicBlockRef loopEndBlock = functionBuilder.FunctionValue.AppendBasicBlock("while.end");

            // Jump to condition block
            builder.BuildBr(loopConditionBlock);
            builder.PositionAtEnd(loopConditionBlock);

            // Emit condition
            LLVMValue conditionalValue = EmitExpression(whileStatement.Expression, functionBuilder, false);
            builder.BuildCondBr(conditionalValue.Value, loopBodyBlock, loopEndBlock);

            // Emit body
            builder.PositionAtEnd(loopBodyBlock);
            EmitStatement(whileStatement.Statement, functionBuilder);

            // Jump back to condition
            if(loopBodyBlock.Terminator.Handle == IntPtr.Zero)
                builder.BuildBr(loopConditionBlock);

            // COntinue from end of loop
            builder.PositionAtEnd(loopEndBlock);
        }

        private void EmitForStatement(ForStatementSyntax forStatement, LLVMValueRef function)
        {
            LLVMBasicBlockRef loopConditionBlock = function.AppendBasicBlock("for.condition");
            LLVMBasicBlockRef loopBodyBlock = function.AppendBasicBlock("for.body");
            LLVMBasicBlockRef loopIncrementBlock = function.AppendBasicBlock("for.increment");
            LLVMBasicBlockRef loopEndBlock = function.AppendBasicBlock("for.end");
            
            // 

            // Emit variable
            //LLVMTypeRef variableType = GetLLVMTypeRef(forStatement.Variable.VariableType);

            // For conditional values
            //LLVMValueRef startValue = EmitExpression(forStatement.)

        }
    }
}
