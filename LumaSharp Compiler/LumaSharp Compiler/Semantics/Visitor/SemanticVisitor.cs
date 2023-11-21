using LumaSharp_Compiler.Semantics.Model;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model.Statement;

namespace LumaSharp_Compiler.Semantics.Visitor
{
    public abstract class SemanticVisitor : ISemanticVisitor
    {
        // Methods
        public virtual void VisitAccessor(AccessorModel model)
        {

        }

        public virtual void VisitAssign(AssignModel model)
        {
            VisitExpression(model.Right);
            VisitExpression(model.Left);
        }

        public virtual void VisitBinary(BinaryModel model)
        {
            VisitExpression(model.Right);
            VisitExpression(model.Left);
        }

        public virtual void VisitConstant(ConstantModel model)
        {
        }

        public virtual void VisitExpression(ExpressionModel model)
        {
            // Check for binary
            if (model is BinaryModel)
            {
                VisitBinary((BinaryModel)model);
            }
            // Check for constant
            else if (model is ConstantModel)
            {
                VisitConstant((ConstantModel)model);
            }
            // Check for field
            else if (model is FieldAccessorReferenceModel)
            {
                VisitFieldAccessorReference((FieldAccessorReferenceModel)model);
            }
            // Check for this
            else if (model is ThisModel)
            {
                VisitThis((ThisModel)model);
            }
            // Check for type
            else if (model is TypeReferenceModel)
            {
                VisitTypeReference((TypeReferenceModel)model);
            }
            // Check for variable
            else if (model is VariableReferenceModel)
            {
                VisitVariableReference((VariableReferenceModel)model);
            }
            // Check for method
            else if(model is MethodInvokeModel)
            {
                VisitMethodInvoke((MethodInvokeModel)model);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public virtual void VisitField(FieldModel model)
        {
        }

        public virtual void VisitFieldAccessorReference(FieldAccessorReferenceModel model)
        {
            VisitExpression(model.AccessModelExpression);
        }

        public virtual void VisitMethodInvoke(MethodInvokeModel model)
        {
            VisitExpression(model.AccessModelExpression);

            // Visit arguments
            if (model.ArgumentModelExpressions != null)
            {
                for (int i = 0; i < model.ArgumentModelExpressions.Length; i++)
                {
                    VisitExpression(model.ArgumentModelExpressions[i]);
                }
            }
        }

        public virtual void VisitMember(MemberModel model)
        {
            // Check for type
            if (model is TypeModel)
            {
                VisitType((TypeModel)model);
            }
            else if (model is FieldModel)
            {
                VisitField((FieldModel)model);
            }
            else if (model is AccessorModel)
            {
                VisitAccessor((AccessorModel)model);
            }
            else if (model is MethodModel)
            {
                VisitMethod((MethodModel)model);
            }
            throw new NotSupportedException();
        }

        public virtual void VisitMethod(MethodModel model)
        {
            foreach (StatementModel statement in model.DescendantsOfType<StatementModel>())
                VisitStatement(statement);
        }

        public virtual void VisitReturn(ReturnModel model)
        {
            if (model.HasReturnExpression == true)
                VisitExpression(model.ReturnModelExpression);
        }

        public virtual void VisitCondition(ConditionModel model)
        {
            if (model.Condition != null)
                VisitExpression(model.Condition);

            // Visit statements
            if(model.Statements != null)
            {
                foreach(StatementModel statement in model.Statements)
                    VisitStatement(statement);
            }

            if(model.Alternate != null)
                VisitCondition(model.Alternate);
        }

        public virtual void VisitFor(ForModel model)
        {
            if(model.Variable != null)
                VisitVariable(model.Variable);

            if(model.Condition != null)
                VisitExpression(model.Condition);

            if(model.IncrementModels != null)
            {
                foreach(ExpressionModel expression in model.IncrementModels)
                    VisitExpression(expression);
            }

            if(model.Statements != null)
            {
                foreach(StatementModel statement in model.Statements)
                    VisitStatement(statement);
            }
        }

        public virtual void VisitMethodInvoke(MethodInlineInvokeModel model)
        {
            VisitMethodInvoke(model.InvokeModel);
        }

        public virtual void VisitStatement(StatementModel model)
        {
            // Check for assign
            if (model is AssignModel)
            {
                VisitAssign(model as AssignModel);
            }
            // Check for retrun
            else if (model is ReturnModel)
            {
                VisitReturn(model as ReturnModel);
            }
            // Check for variable
            else if (model is VariableModel)
            {
                VisitVariable(model as VariableModel);
            }
            // Check for condition
            else if(model is ConditionModel)
            {
                VisitCondition(model as ConditionModel);
            }
            // Check for for
            else if(model is ForModel)
            {
                VisitFor(model as ForModel);
            }
            // Check for method invoke
            else if(model is MethodInlineInvokeModel)
            {
                VisitMethodInvoke(model as MethodInlineInvokeModel);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public virtual void VisitThis(ThisModel model)
        {
        }

        public virtual void VisitType(TypeModel model)
        {
            foreach (MemberModel member in model.DescendantsOfType<MemberModel>())
                VisitMember(member);
        }

        public virtual void VisitTypeReference(TypeReferenceModel model)
        {
        }

        public virtual void VisitVariable(VariableModel model)
        {
        }

        public virtual void VisitVariableReference(VariableReferenceModel model)
        {
        }

        public virtual void VisitNew(NewModel model)
        {
            VisitTypeReference(model.NewTypeExpression);
        }

        public virtual void VisitSize(SizeModel model)
        {
            VisitTypeReference(model.SizeTypeExpression);
        }

        public virtual void VisitTypeToken(TypeTokenModel model)
        {
            VisitTypeReference(model.TypeTokenExpression);
        }
    }
}
