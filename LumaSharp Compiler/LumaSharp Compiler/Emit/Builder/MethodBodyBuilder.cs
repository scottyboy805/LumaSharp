﻿using LumaSharp_Compiler.Semantics.Model.Statement;
using LumaSharp_Compiler.Semantics.Visitor;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model;
using LumaSharp_Compiler.AST;
using LumaSharp.Runtime;
using LumaSharp_Compiler.Semantics;
using LumaSharp.Runtime.Emit;
using LumaSharp.Runtime.Handle;
using System.Diagnostics;
using TypeCode = LumaSharp.Runtime.TypeCode;

namespace LumaSharp_Compiler.Emit.Builder
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
        private StatementModel[] statements = null;
        private InstructionBuilder instructions = null;
        private Stack<ReferenceContext> referenceContextScope = new Stack<ReferenceContext>();

        // Constructor
        public MethodBodyBuilder(StatementModel[] statements)
        {
            this.statements = statements;
            this.referenceContextScope.Push(ReferenceContext.Read);
        }

        // Methods
        public void EmitExecutionObject(InstructionBuilder instructions)
        {
            this.instructions = instructions;

            foreach (StatementModel statement in statements)
            {
                VisitStatement(statement);
            }

            if(statements.Length == 0 || (statements[statements.Length - 1] is ReturnModel) == false)
                instructions.EmitOpCode(OpCode.Ret);
        }

        public override void VisitReturn(ReturnModel model)
        {
            base.VisitReturn(model);

            // Emit return instruction
            instructions.EmitOpCode(OpCode.Ret);
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
                model.Right.Accept(this);
            }
            referenceContextScope.Pop();

            // Check for operation assign
            if(model.Operation != AssignOperation.Assign)
            {
                // Load the left value
                referenceContextScope.Push(ReferenceContext.Read);
                {
                    model.Left.Accept(this);
                }
                referenceContextScope.Pop();

                // Emit operation
                switch(model.Operation)
                {
                    case AssignOperation.AddAssign: instructions.EmitOpCode(OpCode.Add, (byte)model.Left.EvaluatedTypeSymbol.PrimitiveType); break;
                    case AssignOperation.SubtractAssign: instructions.EmitOpCode(OpCode.Sub, (byte)model.Left.EvaluatedTypeSymbol.PrimitiveType); break;
                    case AssignOperation.MultiplyAssign: instructions.EmitOpCode(OpCode.Mul, (byte)model.Left.EvaluatedTypeSymbol.PrimitiveType); break;
                    case AssignOperation.DivideAssign: instructions.EmitOpCode(OpCode.Div, (byte)model.Left.EvaluatedTypeSymbol.PrimitiveType); break;
                }
            }

            // Visit left
            referenceContextScope.Push(ReferenceContext.Write);
            {
                VisitExpression(model.Left);
            }
            referenceContextScope.Pop();
        }

        public override void VisitCondition(ConditionModel model)
        {
            Instruction jmp = default;
            bool didModifyJump = false;

            // Visit condition
            if (model.Condition != null)
            {
                referenceContextScope.Push(ReferenceContext.Read);
                {
                    // Generate condition
                    VisitExpression(model.Condition);

                    // Add jump instruction
                    jmp = instructions.EmitOpCode(OpCode.Jmp_0, 0);
                }
                referenceContextScope.Pop();
            }

            Instruction conditionStart = instructions.Last;

            // Visit body
            if(model.Statements != null && model.Statements.Length > 0)
            {
                foreach (StatementModel statement in model.Statements)
                    statement.Accept(this);
            }


            // Generate alternate
            if (model.Alternate != null)
            {
                // Check for additional condition elif - jump to next conditional check
                if((model.Alternate != null || model.Alternate.Condition != null) && jmp.opCode != OpCode.Nop)
                {
                    int finalOffset = (instructions.Last.offset + instructions.Last.dataSize) - (conditionStart.offset);
                    instructions.ModifyOpCode(jmp, finalOffset);
                    didModifyJump = true;
                }

                // Generate condition
                model.Alternate.Accept(this);
            }

            // Check for no else or else - Jump after else/single condition
            if (didModifyJump == false && (model.Alternate == null || model.Alternate.Condition == null) && jmp.opCode != OpCode.Nop)
            {
                int finalOffset = (instructions.Last.offset + instructions.Last.dataSize) - (conditionStart.offset);// - conditionStart.dataSize);
                instructions.ModifyOpCode(jmp, finalOffset);
            }
        }

        public override void VisitFor(ForModel model)
        {
            // Emit variables
            if(model.Variable != null)
                model.Variable.Accept(this);

            // Jump unconditionally to loop condition checks
            Instruction jmpToCondition = instructions.EmitOpCode(OpCode.Jmp, 0);

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
            int jumpOffset = instructions.Last.EndOffset - jmpToCondition.offset;
            jumpOffset += 2;

            int conditionStartOffset = instructions.Last.EndOffset;

            // Visit condition
            if(model.Condition != null)
            {
                referenceContextScope.Push(ReferenceContext.Read);
                {
                    // Emit condition
                    model.Condition.Accept(this);
                }
                referenceContextScope.Pop();

                // Jump true
                instructions.EmitOpCode(OpCode.Jmp_1, -jumpOffset);
            }
            else
            {
                // Jmp always
                instructions.EmitOpCode(OpCode.Jmp, -jumpOffset);
            }

            // Get condition offset
            int conditionOffset = conditionStartOffset - jmpToCondition.offset;

            instructions.ModifyOpCode(jmpToCondition, conditionOffset - 0);
        }
        #endregion

        #region Expression
        public override void VisitThis(ThisModel model)
        {
            // This object is always implicitly passed as arg0
            instructions.EmitOpCode(OpCode.Ld_Arg_0);
        }

        public override void VisitConstant(ConstantModel model)
        {
            switch(model.EvaluatedTypeSymbol.PrimitiveType)
            {
                case PrimitiveType.Any:
                    {
                        instructions.EmitOpCode(OpCode.Ld_Null);
                        break;
                    }

                case PrimitiveType.Bool:
                    {
                        // Get bool value
                        bool value = model.GetConstantValueAs<bool>();

                        // Check for true
                        if (value == true)
                        {
                            instructions.EmitOpCode(OpCode.Ld_I4_1);
                        }
                        else
                        {
                            instructions.EmitOpCode(OpCode.Ld_I4_0);
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
                            instructions.EmitOpCode(OpCode.Ld_I4_0);
                        }
                        else if (value == 1)
                        {
                            instructions.EmitOpCode(OpCode.Ld_I4_1);
                        }
                        else if (value == -1)
                        {
                            instructions.EmitOpCode(OpCode.Ld_I4_M1);
                        }
                        else
                        {
                            // Emit instruction with value
                            instructions.EmitOpCode(OpCode.Ld_I4, value);
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
                            instructions.EmitOpCode(OpCode.Ld_I4_0);
                        }
                        else if (value == 1)
                        {
                            instructions.EmitOpCode(OpCode.Ld_I4_1);
                        }
                        else
                        {
                            // Emit instruction with value
                            instructions.EmitOpCode(OpCode.Ld_I4, value);
                        }

                        // Add convert instruction
                        instructions.EmitOpCode(OpCode.Cast_I4, (byte)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }

                case PrimitiveType.I64:
                    {
                        // Get long value
                        long value = model.GetConstantValueAs<long>();

                        // Write instruction
                        instructions.EmitOpCode(OpCode.Ld_I8, value);
                        break;
                    }

                case PrimitiveType.U64:
                    {
                        // Get ulong value
                        ulong value = model.GetConstantValueAs<ulong>();

                        // Write instruction
                        instructions.EmitOpCode(OpCode.Ld_I8, value);

                        // Add convert instruction
                        instructions.EmitOpCode(OpCode.Cast_I8, (byte)model.EvaluatedTypeSymbol.PrimitiveType);
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
                            instructions.EmitOpCode(OpCode.Ld_F4_0);
                        }
                        else
                        {
                            // Write instruction
                            instructions.EmitOpCode(OpCode.Ld_F4, value);
                        }
                        break;
                    }

                case PrimitiveType.F64:
                    {
                        // Get double value
                        double value = model.GetConstantValueAs<double>();

                        // Write instruction
                        instructions.EmitOpCode(OpCode.Ld_F8, value);
                        break;
                    }
            }
        }

        public override void VisitBinary(BinaryModel model)
        {
            // Emit left
            model.Left.Accept(this);

            // Check for left promotion required
            if(model.IsLeftPromotionRequired == true || model.Left.EvaluatedTypeSymbol != model.EvaluatedTypeSymbol)
            {
                // Emit conversion instruction
                EmitConversion(model.Left.EvaluatedTypeSymbol, model.EvaluatedTypeSymbol);
            }

            // Emit right
            model.Right.Accept(this);

            // Check for right promotion required
            if(model.IsRightPromotionRequired == true || model.Right.EvaluatedTypeSymbol != model.EvaluatedTypeSymbol)
            {
                // Emit conversion instruction
                EmitConversion(model.Right.EvaluatedTypeSymbol, model.EvaluatedTypeSymbol);
            }
                        

            // Emit op code
            switch(model.Operation)
            {
                case BinaryOperation.Equal:
                    {
                        instructions.EmitOpCode(OpCode.Cmp_Eq, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.NotEqual:
                    {
                        instructions.EmitOpCode(OpCode.Cmp_NEq, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.Add:
                    {
                        instructions.EmitOpCode(OpCode.Add, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.Subtract:
                    {
                        instructions.EmitOpCode(OpCode.Sub, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.Multiply:
                    {
                        instructions.EmitOpCode(OpCode.Mul, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.Divide:
                    {
                        instructions.EmitOpCode(OpCode.Div, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.Greater:
                    {
                        instructions.EmitOpCode(OpCode.Cmp_G, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.GreaterEqual:
                    {
                        instructions.EmitOpCode(OpCode.Cmp_Ge, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.Less:
                    {
                        instructions.EmitOpCode(OpCode.Cmp_L, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
                case BinaryOperation.LessEqual:
                    {
                        instructions.EmitOpCode(OpCode.Cmp_Le, (TypeCode)model.EvaluatedTypeSymbol.PrimitiveType);
                        break;
                    }
            }
        }

        public override void VisitMethodInvoke(MethodInvokeModel model)
        {
            // Visit access expression
            model.AccessModelExpression.Accept(this);

            // Visit arguments
            if(model.ArgumentModelExpressions != null)
            {
                for(int i = 0; i < model.ArgumentModelExpressions.Length; i++)
                {
                    model.ArgumentModelExpressions[i].Accept(this);
                }
            }

            // Emit call instruction
            instructions.EmitOpCode(OpCode.Call, model.MethodIdentifier.SymbolToken);
        }

        public override void VisitVariableReference(VariableReferenceModel model)
        {
            // Get symbol
            ILocalIdentifierReferenceSymbol localSymbol = model.IdentifierSymbol as ILocalIdentifierReferenceSymbol;

            // Check for read
            if (referenceContextScope.Peek() == ReferenceContext.Read)
            {
                // Create load local/parameter instruction
                EmitLoadLocal(localSymbol);

                // Check for by reference
                // Should use the sequence: [ld_arg.., ld_addr] to load value from address
                if (localSymbol.IsByReference == true)
                {
                    EmitLoadAddress(localSymbol.TypeSymbol);
                }
            }
            // Must be write
            else
            {
                // Check for by reference - need to load the address first
                if (localSymbol.IsByReference == true)
                {
                    EmitLoadLocal(localSymbol);
                    EmitStoreAddress(localSymbol.TypeSymbol);
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
            if(referenceContextScope.Peek() == ReferenceContext.Read)
            {
                // Check for field
                if(model.IsFieldReference == true)
                {
                    // Get field symbol
                    IFieldReferenceSymbol fieldSymbol = model.FieldAccessorIdentifier as IFieldReferenceSymbol;

                    // Load field instruction
                    instructions.EmitOpCode(OpCode.Ld_Fld, fieldSymbol.SymbolToken);
                }
                // Check for accessor
                else if(model.IsAccessorReference == true)
                {
                    // Get accessor symbol
                }
            }
            // Must be write
            else
            {
                // Check for field
                if(model.IsFieldReference == true)
                {
                    // Get field symbols
                    IFieldReferenceSymbol fieldSymbol = model.FieldAccessorIdentifier as IFieldReferenceSymbol;

                    // Store field instruction
                    instructions.EmitOpCode(OpCode.St_Fld, fieldSymbol.SymbolToken);
                }
                // Check for accessor
                else if(model.IsAccessorReference == true)
                {

                }
            }
        }

        public override void VisitTypeReference(TypeReferenceModel model)
        {
            // Load type symbols
            instructions.EmitOpCode(OpCode.Ld_Type, model.EvaluatedTypeSymbol.SymbolToken);
        }

        private void EmitLoadLocal(ILocalIdentifierReferenceSymbol localSymbol)
        {
            // Check for parameter
            if (localSymbol.IsParameter == true)
            {
                switch (localSymbol.Index)
                {
                    case 0: instructions.EmitOpCode(OpCode.Ld_Arg_0); break;
                    case 1: instructions.EmitOpCode(OpCode.Ld_Arg_1); break;
                    case 2: instructions.EmitOpCode(OpCode.Ld_Arg_2); break;
                    case 3: instructions.EmitOpCode(OpCode.Ld_Arg_3); break;
                    default:
                        {
                            if (localSymbol.Index < byte.MaxValue)
                            {
                                instructions.EmitOpCode(OpCode.Ld_Arg, (byte)localSymbol.Index);
                            }
                            else
                            {
                                instructions.EmitOpCode(OpCode.Ld_Arg_E, (ushort)localSymbol.Index);
                            }
                            break;
                        }
                }
            }
            // Check for local
            else if (localSymbol.IsLocal == true)
            {
                switch (localSymbol.Index)
                {
                    case 0: instructions.EmitOpCode(OpCode.Ld_Loc_0); break;
                    case 1: instructions.EmitOpCode(OpCode.Ld_Loc_1); break;
                    case 2: instructions.EmitOpCode(OpCode.Ld_Loc_2); break;
                    default:
                        {
                            if (localSymbol.Index < byte.MaxValue)
                            {
                                instructions.EmitOpCode(OpCode.Ld_Loc, (byte)localSymbol.Index);
                            }
                            else
                            {
                                instructions.EmitOpCode(OpCode.Ld_Loc_E, (ushort)localSymbol.Index);
                            }
                            break;
                        }
                }
            }
        }

        private void EmitStoreLocal(ILocalIdentifierReferenceSymbol localSymbol)
        {
            // Check for parameter
            if (localSymbol.IsParameter == true)
            {
                switch (localSymbol.Index)
                {
                    case 0:
                        {
                            instructions.EmitOpCode(OpCode.Ld_Arg_0);
                            EmitStoreAddress(localSymbol.TypeSymbol);
                            break;
                        }
                    case 1:
                        {
                            instructions.EmitOpCode(OpCode.Ld_Arg_1);
                            EmitStoreAddress(localSymbol.TypeSymbol);
                            break;
                        }
                    case 2:
                        {
                            instructions.EmitOpCode(OpCode.Ld_Arg_2);
                            EmitStoreAddress(localSymbol.TypeSymbol);
                            break;
                        }
                    case 3:
                        {
                            instructions.EmitOpCode(OpCode.Ld_Arg_3);
                            EmitStoreAddress(localSymbol.TypeSymbol);
                            break;
                        }
                    default:
                        {
                            if (localSymbol.Index < byte.MaxValue)
                            {
                                instructions.EmitOpCode(OpCode.Ld_Arg, (byte)localSymbol.Index);
                                EmitStoreAddress(localSymbol.TypeSymbol);
                            }
                            else
                            {
                                instructions.EmitOpCode(OpCode.Ld_Arg_E, (ushort)localSymbol.Index);
                                EmitStoreAddress(localSymbol.TypeSymbol);
                            }
                            break;
                        }
                }
            }
            // Check for local
            else if (localSymbol.IsLocal == true)
            {
                switch (localSymbol.Index)
                {
                    case 0: instructions.EmitOpCode(OpCode.St_Loc_0); break;
                    case 1: instructions.EmitOpCode(OpCode.St_Loc_1); break;
                    case 2: instructions.EmitOpCode(OpCode.St_Loc_2); break;
                    default:
                        {
                            if (localSymbol.Index < byte.MaxValue)
                            {
                                instructions.EmitOpCode(OpCode.St_Loc, (byte)localSymbol.Index);
                            }
                            else
                            {
                                instructions.EmitOpCode(OpCode.St_Loc_E, (ushort)localSymbol.Index);
                            }
                            break;
                        }
                }
            }
        }

        private void EmitLoadAddress(ITypeReferenceSymbol typeSymbol)
        {
            switch(typeSymbol.PrimitiveType)
            {
                case PrimitiveType.I8:
                case PrimitiveType.U8: instructions.EmitOpCode(OpCode.Ld_Addr_I1); break;
                case PrimitiveType.I16:
                case PrimitiveType.U16: instructions.EmitOpCode(OpCode.Ld_Addr_I2); break;
                case PrimitiveType.I32:
                case PrimitiveType.U32: instructions.EmitOpCode(OpCode.Ld_Addr_I4); break;
                case PrimitiveType.I64:
                case PrimitiveType.U64: instructions.EmitOpCode (OpCode.Ld_Addr_I8); break;
                case PrimitiveType.F32: instructions.EmitOpCode(OpCode.Ld_Addr_F4); break;
                case PrimitiveType.F64: instructions.EmitOpCode(OpCode.Ld_Addr_F8); break;
                default:
                    {
                        instructions.EmitOpCode(OpCode.Ld_Addr_Any, typeSymbol.SymbolToken);
                        break;
                    }
            }
        }

        private void EmitStoreAddress(ITypeReferenceSymbol typeSymbol)
        {
            switch (typeSymbol.PrimitiveType)
            {
                case PrimitiveType.I8:
                case PrimitiveType.U8: instructions.EmitOpCode(OpCode.St_Addr_I1); break;
                case PrimitiveType.I16:
                case PrimitiveType.U16: instructions.EmitOpCode(OpCode.St_Addr_I2); break;
                case PrimitiveType.I32:
                case PrimitiveType.U32: instructions.EmitOpCode(OpCode.St_Addr_I4); break;
                case PrimitiveType.I64:
                case PrimitiveType.U64: instructions.EmitOpCode(OpCode.St_Addr_I8); break;
                case PrimitiveType.F32: instructions.EmitOpCode(OpCode.St_Addr_F4); break;
                case PrimitiveType.F64: instructions.EmitOpCode(OpCode.St_Addr_F8); break;
                default:
                    {
                        instructions.EmitOpCode(OpCode.St_Addr_Any, typeSymbol.SymbolToken);
                        break;
                    }
            }
        }

        private void EmitConversion(ITypeReferenceSymbol fromSymbol, ITypeReferenceSymbol toSymbol)
        {
            // Check for objects
            if (fromSymbol.PrimitiveType == PrimitiveType.Any && toSymbol.PrimitiveType == PrimitiveType.Any)
            {
                instructions.EmitOpCode(OpCode.Cast_Any, toSymbol.SymbolToken);
            }
            // Check for boxed conversion to primitive
            else if (fromSymbol.PrimitiveType == PrimitiveType.Any)
            {
                instructions.EmitOpCode(OpCode.From_Any, toSymbol.SymbolToken);
            }
            // Check for primitive conversion to boxed any
            else if (toSymbol.PrimitiveType == PrimitiveType.Any)
            {
                instructions.EmitOpCode(OpCode.As_Any, toSymbol.SymbolToken);
            }
            else
            {
                switch (fromSymbol.PrimitiveType)
                {
                    case PrimitiveType.I8:
                        {
                            instructions.EmitOpCode(OpCode.Cast_I1, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.U8:
                        {
                            instructions.EmitOpCode(OpCode.Cast_UI1, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.I16:
                        {
                            instructions.EmitOpCode(OpCode.Cast_I2, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.U16:
                        {
                            instructions.EmitOpCode(OpCode.Cast_UI2, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.I32:
                        {
                            instructions.EmitOpCode(OpCode.Cast_I4, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.U32:
                        {
                            instructions.EmitOpCode(OpCode.Cast_UI4, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.I64:
                        {
                            instructions.EmitOpCode(OpCode.Cast_I8, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.U64:
                        {
                            instructions.EmitOpCode(OpCode.Cast_UI8, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.F32:
                        {
                            instructions.EmitOpCode(OpCode.Cast_F4, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                    case PrimitiveType.F64:
                        {
                            instructions.EmitOpCode(OpCode.Cast_F8, (byte)toSymbol.PrimitiveType);
                            break;
                        }
                }
            }
        }
        #endregion
    }
}
