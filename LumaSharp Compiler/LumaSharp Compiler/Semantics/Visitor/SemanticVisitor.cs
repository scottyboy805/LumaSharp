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
            model.Accept(this);
        }

        public virtual void VisitAssign(AssignModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitBinary(BinaryModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitConstant(ConstantModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitExpression(ExpressionModel model)
        {
            // Check for binary
            if(model is BinaryModel)
                VisitBinary((BinaryModel)model);

            // Check for constant
            if(model is ConstantModel)
                VisitConstant((ConstantModel)model);

            // Check for field
            if(model is FieldAccessorReferenceModel)
                VisitFieldAccessorReference((FieldAccessorReferenceModel)model);

            // Check for this
            if(model is ThisReferenceModel)
                VisitThis((ThisReferenceModel)model);

            // Check for type
            if(model is TypeReferenceModel)
                VisitTypeReference((TypeReferenceModel)model);

            // Check for variable
            if(model is VariableReferenceModel)
                VisitVariableReference((VariableReferenceModel)model);

            throw new NotSupportedException();
        }

        public virtual void VisitField(FieldModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitFieldAccessorReference(FieldAccessorReferenceModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitMember(MemberModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitMethod(MethodModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitReturn(ReturnModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitStatement(StatementModel model)
        {
            // Check for assign
            if (model is AssignModel)
                VisitAssign(model as AssignModel);

            // Check for retrun
            if(model is ReturnModel)
                VisitReturn(model as ReturnModel);

            // Check for variable
            if(model is VariableModel) 
                VisitVariable(model as VariableModel);

            throw new NotSupportedException();
        }

        public virtual void VisitThis(ThisReferenceModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitType(TypeModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitTypeReference(TypeReferenceModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitVariable(VariableModel model)
        {
            model.Accept(this);
        }

        public virtual void VisitVariableReference(VariableReferenceModel model)
        {
            model.Accept(this);
        }
    }
}
