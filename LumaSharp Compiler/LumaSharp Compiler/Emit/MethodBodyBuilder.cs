using LumaSharp.Compiler.Semantics.Visitor;
using LumaSharp.Compiler.Semantics.Model;
using LumaSharp.Compiler.AST;
using LumaSharp.Runtime;
using LumaSharp.Compiler.Semantics;

namespace LumaSharp.Compiler.Emit
{
    internal class MethodBodyBuilder : SemanticVisitor
    {
        // Type
        private enum ReferenceContext
        {
            Read,
            Write,
        }

        // Private
        private int parameterCount = 0;
        private StatementModel[] statements = null;
        private BytecodeBuilder builder = null;
        private Stack<ReferenceContext> referenceContextScope = new Stack<ReferenceContext>();

        // Properties
        public int MaxStack
        {
            get { return builder.MaxStack; }
        }

        // Constructor
        public MethodBodyBuilder(int parameterCount, StatementModel[] statements)
        {
            this.parameterCount = parameterCount;
            this.statements = statements;
            referenceContextScope.Push(ReferenceContext.Read);
        }

        // Methods
        public void EmitExecutionObject(BytecodeBuilder builder)
        {
            this.builder = builder;

            foreach (StatementModel statement in statements)
            {
                VisitStatement(statement);
            }

            if (statements.Length == 0 || statements[statements.Length - 1] is ReturnModel == false)
                builder.Emit(OpCode.Ret);
        }

        public override void VisitReturn(ReturnModel model)
        {
            base.VisitReturn(model);

            // Emit return instruction
            builder.Emit(OpCode.Ret);
        }

        #region Statement
        public override void VisitVariable(VariableModel model)
        {
            if (model.AssignModels != null)
            {
                // Visit all assign expressions
                foreach (AssignModel assign in model.AssignModels)
                {
                    assign.Accept(this);
                }
            }

            //foreach (model.VariableModels[0].
        }

        public override void VisitAssign(AssignModel model)
        {
            // Visit right
            referenceContextScope.Push(ReferenceContext.Read);
            {
                foreach (ExpressionModel expression in model.Right)
                    expression.Accept(this);
            }
            referenceContextScope.Pop();

            // Check for operation assign
            if (model.Operation != AssignOperation.Assign)
            {
                // Load the left value
                referenceContextScope.Push(ReferenceContext.Read);
                {
                    foreach (ExpressionModel expression in model.Left)
                        expression.Accept(this);
                }
                referenceContextScope.Pop();

                // Emit operation
                switch (model.Operation)
                {
                    case AssignOperation.AddAssign: builder.Emit(OpCode.Add); break;
                    case AssignOperation.SubtractAssign: builder.Emit(OpCode.Sub); break;
                    case AssignOperation.MultiplyAssign: builder.Emit(OpCode.Mul); break;
                    case AssignOperation.DivideAssign: builder.Emit(OpCode.Div); break;
                }
            }

            // Visit left
            referenceContextScope.Push(ReferenceContext.Write);
            {
                foreach (ExpressionModel expression in model.Left)
                    VisitExpression(expression);
            }
            referenceContextScope.Pop();
        }

        public override void VisitCondition(ConditionModel model)
        {
            BytecodeOperation jmp = default;
            bool didModifyJump = false;

            // Visit condition
            if (model.Condition != null)
            {
                referenceContextScope.Push(ReferenceContext.Read);
                {
                    // Generate condition
                    VisitExpression(model.Condition);

                    // Add jump instruction
                    jmp = builder.Emit(OpCode.Jmp_0, 0);
                }
                referenceContextScope.Pop();
            }

            BytecodeOperation conditionStart = builder.Last;

            // Visit body
            if (model.Statements != null && model.Statements.Length > 0)
            {
                foreach (StatementModel statement in model.Statements)
                    statement.Accept(this);
            }


            // Generate alternate
            if (model.Alternate != null)
            {
                // Check for additional condition elif - jump to next conditional check
                if ((model.Alternate != null || model.Alternate.Condition != null) && jmp.OpCode != OpCode.Nop)
                {
                    int finalOffset = builder.Last.Offset + builder.Last.OperandSize - conditionStart.Offset;
                    builder.ModifyOperand(jmp, finalOffset);
                    didModifyJump = true;
                }

                // Generate condition
                model.Alternate.Accept(this);
            }

            // Check for no else or else - Jump after else/single condition
            if (didModifyJump == false && (model.Alternate == null || model.Alternate.Condition == null) && jmp.OpCode != OpCode.Nop)
            {
                int finalOffset = builder.Last.Offset + builder.Last.OperandSize - conditionStart.Offset;// - conditionStart.dataSize);
                builder.ModifyOperand(jmp, finalOffset);
            }
        }

        public override void VisitFor(ForModel model)
        {
            // Emit variables
            if (model.Variable != null)
                model.Variable.Accept(this);

            // Jump unconditionally to loop condition checks
            BytecodeOperation jmpToCondition = builder.Emit(OpCode.Jmp, 0);

            // Visit body
            if (model.Statements != null && model.Statements.Length > 0)
            {
                foreach (StatementModel statement in model.Statements)
                    statement.Accept(this);
            }

            // Visit increments
            if (model.IncrementModels != null && model.IncrementModels.Length > 0)
            {
                foreach (ExpressionModel increment in model.IncrementModels)
                    increment.Accept(this);
            }


            // Get the jump offset
            int jumpOffset = builder.Last.EndOffset - jmpToCondition.Offset;
            jumpOffset += 2;

            int conditionStartOffset = builder.Last.EndOffset;

            // Visit condition
            if (model.Condition != null)
            {
                referenceContextScope.Push(ReferenceContext.Read);
                {
                    // Emit condition
                    model.Condition.Accept(this);
                }
                referenceContextScope.Pop();

                // Jump true
                builder.Emit(OpCode.Jmp_1, -jumpOffset);
            }
            else
            {
                // Jmp always
                builder.Emit(OpCode.Jmp, -jumpOffset);
            }

            // Get condition offset
            int conditionOffset = conditionStartOffset - jmpToCondition.Offset;

            builder.ModifyOperand(jmpToCondition, conditionOffset - 0);
        }
        #endregion

        #region Expression
        public override void VisitThis(ThisModel model)
        {
            // This object is always implicitly passed as var0
            builder.Emit(OpCode.Ld_Var_0);
        }

        public override void VisitConstant(ConstantModel model)
        {
            switch (model.EvaluatedTypeSymbol.PrimitiveType)
            {
                case PrimitiveType.Any:
                    {
                        builder.Emit(OpCode.Ld_Null);
                        break;
                    }

                case PrimitiveType.Bool:
                    {
                        // Get bool value
                        bool value = model.GetConstantValueAs<bool>();

                        // Check for true
                        if (value == true)
                        {
                            builder.Emit(OpCode.Ld_I4_1);
                        }
                        else
                        {
                            builder.Emit(OpCode.Ld_I4_0);
                        }
                        break;
                    }

                case PrimitiveType.I32:
                    {
                        // Get int value
                        int value = model.GetConstantValueAs<int>();

                        // Check for optimized instructions
                        if (value == 0)
                        {
                            builder.Emit(OpCode.Ld_I4_0);
                        }
                        else if (value == 1)
                        {
                            builder.Emit(OpCode.Ld_I4_1);
                        }
                        else if (value == -1)
                        {
                            builder.Emit(OpCode.Ld_I4_M1);
                        }
                        else
                        {
                            // Emit instruction with value
                            builder.Emit(OpCode.Ld_I4, value);
                        }
                        break;
                    }

                case PrimitiveType.U32:
                    {
                        // Get int value
                        uint value = model.GetConstantValueAs<uint>();

                        // Check for optimized instructions
                        if (value == 0)
                        {
                            builder.Emit(OpCode.Ld_I4_0);
                        }
                        else if (value == 1)
                        {
                            builder.Emit(OpCode.Ld_I4_1);
                        }
                        else
                        {
                            // Emit instruction with value
                            builder.Emit(OpCode.Ld_I4, value);
                        }

                        // Add convert instruction
                        builder.Emit(OpCode.Cast_I4);
                        break;
                    }

                case PrimitiveType.I64:
                    {
                        // Get long value
                        long value = model.GetConstantValueAs<long>();

                        // Write instruction
                        builder.Emit(OpCode.Ld_I8, value);
                        break;
                    }

                case PrimitiveType.U64:
                    {
                        // Get ulong value
                        ulong value = model.GetConstantValueAs<ulong>();

                        // Write instruction
                        builder.Emit(OpCode.Ld_I8, value);

                        // Add convert instruction
                        builder.Emit(OpCode.Cast_U8);
                        break;
                    }

                case PrimitiveType.F32:
                    {
                        // Get float value
                        float value = model.GetConstantValueAs<float>();

                        // Check for 0 optimized instruction
                        if (value == 0f)
                        {
                            // Write optimized instruction
                            builder.Emit(OpCode.Ld_F4_0);
                        }
                        else
                        {
                            // Write instruction
                            builder.Emit(OpCode.Ld_F4, value);
                        }
                        break;
                    }

                case PrimitiveType.F64:
                    {
                        // Get double value
                        double value = model.GetConstantValueAs<double>();

                        // Write instruction
                        builder.Emit(OpCode.Ld_F8, value);
                        break;
                    }
            }
        }

        public override void VisitBinary(BinaryModel model)
        {
            // Emit left
            model.Left.Accept(this);

            // Check for left promotion required
            if (model.IsLeftPromotionRequired == true || model.Left.EvaluatedTypeSymbol != model.EvaluatedTypeSymbol)
            {
                // Emit conversion instruction
                EmitConversion(model.Left.EvaluatedTypeSymbol, model.EvaluatedTypeSymbol);
            }

            // Emit right
            model.Right.Accept(this);

            // Check for right promotion required
            if (model.IsRightPromotionRequired == true || model.Right.EvaluatedTypeSymbol != model.EvaluatedTypeSymbol)
            {
                // Emit conversion instruction
                EmitConversion(model.Right.EvaluatedTypeSymbol, model.EvaluatedTypeSymbol);
            }


            // Emit op code
            switch (model.Operation)
            {
                case BinaryOperation.Equal:
                    {
                        builder.Emit(OpCode.Cmp_Eq);
                        break;
                    }
                case BinaryOperation.NotEqual:
                    {
                        builder.Emit(OpCode.Cmp_NEq);
                        break;
                    }
                case BinaryOperation.Add:
                    {
                        builder.Emit(OpCode.Add);
                        break;
                    }
                case BinaryOperation.Subtract:
                    {
                        builder.Emit(OpCode.Sub);
                        break;
                    }
                case BinaryOperation.Multiply:
                    {
                        builder.Emit(OpCode.Mul);
                        break;
                    }
                case BinaryOperation.Divide:
                    {
                        builder.Emit(OpCode.Div);
                        break;
                    }
                case BinaryOperation.Greater:
                    {
                        builder.Emit(OpCode.Cmp_G);
                        break;
                    }
                case BinaryOperation.GreaterEqual:
                    {
                        builder.Emit(OpCode.Cmp_Ge);
                        break;
                    }
                case BinaryOperation.Less:
                    {
                        builder.Emit(OpCode.Cmp_L);
                        break;
                    }
                case BinaryOperation.LessEqual:
                    {
                        builder.Emit(OpCode.Cmp_Le);
                        break;
                    }
            }
        }

        public override void VisitMethodInvoke(MethodInvokeModel model)
        {
            // Visit access expression
            model.AccessModelExpression.Accept(this);

            // Visit arguments
            if (model.ArgumentModelExpressions != null)
            {
                for (int i = 0; i < model.ArgumentModelExpressions.Length; i++)
                {
                    model.ArgumentModelExpressions[i].Accept(this);
                }
            }

            // Check for virtual
            if (model.MethodIdentifier.IsOverride == true)
            {
                // Emit virtual call instruction
                builder.EmitToken(OpCode.Call_Virt, model.MethodIdentifier);
            }
            else
            {
                // Emit call instruction
                builder.EmitToken(OpCode.Call, model.MethodIdentifier);
            }
        }

        public override void VisitVariableReference(VariableReferenceModel model)
        {
            // Get symbol
            ILocalIdentifierReferenceSymbol localSymbol = model.IdentifierSymbol as ILocalIdentifierReferenceSymbol;

            // Check for read
            if (referenceContextScope.Peek() == ReferenceContext.Read)
            {
                // Create load local/parameter instruction
                EmitLoadLocal(localSymbol, localSymbol.IsByReference);

                // Check for by reference
                // Should use the sequence: [ld_var.., ld_addr] to load value from address
                if (localSymbol.IsByReference == true)
                {
                    builder.Emit(OpCode.Ld_Addr);
                }
            }
            // Must be write
            else
            {
                // Check for by reference - need to load the address first
                if (localSymbol.IsByReference == true)
                {
                    EmitLoadLocal(localSymbol, true);
                    builder.Emit(OpCode.St_Addr);
                }
                else
                {
                    // Create store local/parameter instruction
                    EmitStoreLocal(localSymbol);
                }
            }
        }

        public override void VisitFieldAccessorReference(FieldAccessorReferenceModel model)
        {
            // Emit access expression
            base.VisitFieldAccessorReference(model);

            // Check for read
            if (referenceContextScope.Peek() == ReferenceContext.Read)
            {
                // Check for field
                if (model.IsFieldReference == true)
                {
                    // Get field symbol
                    IFieldReferenceSymbol fieldSymbol = model.FieldAccessorIdentifier as IFieldReferenceSymbol;

                    // Check for global
                    if (fieldSymbol.IsGlobal == false)
                    {
                        // Load field instruction
                        builder.EmitToken(OpCode.Ld_Fld, fieldSymbol);
                    }
                    else
                    {
                        // Load global field
                        builder.EmitToken(OpCode.Ld_Fld_G, fieldSymbol);
                    }
                }
                // Check for accessor
                else if (model.IsAccessorReference == true)
                {
                    // Get accessor symbol
                }
            }
            // Must be write
            else
            {
                // Check for field
                if (model.IsFieldReference == true)
                {
                    // Get field symbols
                    IFieldReferenceSymbol fieldSymbol = model.FieldAccessorIdentifier as IFieldReferenceSymbol;

                    // Check for global
                    if (fieldSymbol.IsGlobal == false)
                    {
                        // Store field instruction
                        builder.EmitToken(OpCode.St_Fld, fieldSymbol);
                    }
                    else
                    {
                        // Store global field
                        builder.EmitToken(OpCode.St_Fld_G, fieldSymbol);
                    }
                }
                // Check for accessor
                else if (model.IsAccessorReference == true)
                {

                }
            }
        }

        public override void VisitTypeReference(TypeReferenceModel model)
        {
            // Load type symbols
            builder.EmitToken(OpCode.Ld_Type, model.EvaluatedTypeSymbol);
        }

        private void EmitLoadLocal(ILocalIdentifierReferenceSymbol localSymbol, bool byReference)
        {
            int variableIndex = localSymbol.Index;

            // Check for local
            if (localSymbol.IsLocal == true)
                variableIndex += parameterCount;

            // Check for byref
            if (byReference == true)
            {
                if (localSymbol.Index < byte.MaxValue)
                {
                    builder.Emit(OpCode.Ld_Var_A, (byte)localSymbol.Index);
                }
                else
                {
                    builder.Emit(OpCode.Ld_Var_EA, (ushort)localSymbol.Index);
                }
                return;
            }

            switch (variableIndex)
            {
                case 0: builder.Emit(OpCode.Ld_Var_0); break;
                case 1: builder.Emit(OpCode.Ld_Var_1); break;
                case 2: builder.Emit(OpCode.Ld_Var_2); break;
                case 3: builder.Emit(OpCode.Ld_Var_3); break;
                default:
                    {
                        if (localSymbol.Index < byte.MaxValue)
                        {
                            builder.Emit(OpCode.Ld_Var, (byte)localSymbol.Index);
                        }
                        else
                        {
                            builder.Emit(OpCode.Ld_Var_E, (ushort)localSymbol.Index);
                        }
                        break;
                    }
            }
        }

        private void EmitStoreLocal(ILocalIdentifierReferenceSymbol localSymbol)
        {
            int variableIndex = localSymbol.Index;

            // Check for local
            if (localSymbol.IsLocal == true)
                variableIndex += parameterCount;

            switch (variableIndex)
            {
                case 0: builder.Emit(OpCode.St_Var_0); break;
                case 1: builder.Emit(OpCode.St_Var_1); break;
                case 2: builder.Emit(OpCode.St_Var_2); break;
                case 3: builder.Emit(OpCode.St_Var_3); break;
                default:
                    {
                        if (localSymbol.Index < byte.MaxValue)
                        {
                            builder.Emit(OpCode.St_Var, (byte)localSymbol.Index);
                        }
                        else
                        {
                            builder.Emit(OpCode.St_Var_E, (ushort)localSymbol.Index);
                        }
                        break;
                    }
            }
        }

        private void EmitConversion(ITypeReferenceSymbol fromSymbol, ITypeReferenceSymbol toSymbol)
        {
            // Check for objects
            if (fromSymbol.PrimitiveType == PrimitiveType.Any && toSymbol.PrimitiveType == PrimitiveType.Any)
            {
                builder.EmitToken(OpCode.Cast_Any, toSymbol);
            }
            // Check for boxed conversion to primitive
            else if (fromSymbol.PrimitiveType == PrimitiveType.Any)
            {
                builder.EmitToken(OpCode.Unbox_Any, toSymbol);
            }
            // Check for primitive conversion to boxed any
            else if (toSymbol.PrimitiveType == PrimitiveType.Any)
            {
                builder.EmitToken(OpCode.Box_Any, toSymbol);
            }
            else
            {
                switch (fromSymbol.PrimitiveType)
                {
                    // Not required???
                    //case PrimitiveType.I8:
                    //    {
                    //        instructions.EmitOpCode(OpCode.Cast_I1, (byte)toSymbol.PrimitiveType);
                    //        break;
                    //    }
                    //case PrimitiveType.U8:
                    //    {
                    //        instructions.EmitOpCode(OpCode.Cast_UI1, (byte)toSymbol.PrimitiveType);
                    //        break;
                    //    }
                    //case PrimitiveType.I16:
                    //    {
                    //        instructions.EmitOpCode(OpCode.Cast_I2, (byte)toSymbol.PrimitiveType);
                    //        break;
                    //    }
                    //case PrimitiveType.U16:
                    //    {
                    //        instructions.EmitOpCode(OpCode.Cast_UI2, (byte)toSymbol.PrimitiveType);
                    //        break;
                    //    }
                    case PrimitiveType.I32:
                        {
                            builder.Emit(OpCode.Cast_I4, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.U32:
                        {
                            builder.Emit(OpCode.Cast_U4, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.I64:
                        {
                            builder.Emit(OpCode.Cast_I8, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.U64:
                        {
                            builder.Emit(OpCode.Cast_U8, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.F32:
                        {
                            builder.Emit(OpCode.Cast_F4, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.F64:
                        {
                            builder.Emit(OpCode.Cast_F8, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                }
            }
        }
        #endregion
    }
}
