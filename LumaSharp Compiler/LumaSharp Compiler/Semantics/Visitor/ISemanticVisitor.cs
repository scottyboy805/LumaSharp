using LumaSharp_Compiler.Semantics.Model;
using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.Semantics.Model.Statement;

namespace LumaSharp_Compiler.Semantics
{
    public interface ISemanticVisitor
    {
        // Methods
        #region Root
        void VisitMember(MemberModel model);

        void VisitType(TypeModel model);

        void VisitField(FieldModel model);

        void VisitAccessor(AccessorModel model);

        void VisitMethod(MethodModel model);
        #endregion

        #region Statement
        void VisitStatement(StatementModel model);

        void VisitAssign(AssignModel model);

        void VisitReturn(ReturnModel model);

        void VisitVariable(VariableModel model);
        #endregion

        #region Expression
        void VisitExpression(ExpressionModel model);

        void VisitBinary(BinaryModel model);

        void VisitConstant(ConstantModel model);

        void VisitFieldAccessorReference(FieldAccessorReferenceModel model);

        void VisitThis(ThisReferenceModel model);

        void VisitTypeReference(TypeReferenceModel model);

        void VisitVariableReference(VariableReferenceModel model);
        #endregion
    }
}
