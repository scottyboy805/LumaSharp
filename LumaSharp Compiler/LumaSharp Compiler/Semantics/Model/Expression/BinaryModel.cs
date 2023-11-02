using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using LumaSharp_Compiler.Semantics.Reference;

namespace LumaSharp_Compiler.Semantics.Model.Expression
{
    public sealed class BinaryModel : ExpressionModel
    {
        // Private
        private BinaryExpressionSyntax syntax = null;
        private ExpressionModel left = null;
        private ExpressionModel right = null;
        private ITypeReferenceSymbol inferredTypeSymbol = null;
        private IMethodReferenceSymbol inferredMethodOperatorSymbol = null;

        // Properties
        public ExpressionModel Left
        {
            get { return left; }
        }

        public ExpressionModel Right
        {
            get { return right; }
        }

        public bool IsLeftPromotionRequired
        {
            get { return left.EvaluatedTypeSymbol != null && IsSmallPrimitive(left.EvaluatedTypeSymbol); }
        }

        public bool IsRightPromotionRequired
        {
            get { return right.EvaluatedTypeSymbol != null && IsSmallPrimitive(right.EvaluatedTypeSymbol); }
        }

        public override bool IsStaticallyEvaluated
        {
            get { return left.IsStaticallyEvaluated == true && right.IsStaticallyEvaluated == true; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get 
            {
                // Check for operator method available
                if (inferredMethodOperatorSymbol != null)
                    return inferredMethodOperatorSymbol.ReturnTypeSymbol;

                return inferredTypeSymbol; 
            }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get
            {
                yield return left;
                yield return right;
            }
        }

        // Constructor
        internal BinaryModel(SemanticModel model, SymbolModel parent, BinaryExpressionSyntax syntax)
            : base(model, parent, syntax)
        {
            this.syntax = syntax;
            this.left = Any(model, this, syntax.Left);
            this.right = Any(model, this, syntax.Right);
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitBinary(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Resolve left
            left.ResolveSymbols(provider, report);

            // Resolve right
            right.ResolveSymbols(provider, report);


            // Try to get the target operation
            BinaryOperation operation = 0;
            try
            {
                operation = syntax.BinaryOperation;
            }
            catch(NotSupportedException)
            {
                report.ReportMessage(Code.InvalidOperation, MessageSeverity.Error, syntax.Operation.Source);
                return;
            }

            // Check for symbols resolved
            if (left.EvaluatedTypeSymbol != null && right.EvaluatedTypeSymbol != null)
            {
                // Get left/right types
                PrimitiveType leftTypeCode = GetPromotedTypeCode(left.EvaluatedTypeSymbol);
                PrimitiveType rightTypeCode = GetPromotedTypeCode(right.EvaluatedTypeSymbol);

                // Check for both primitive
                if (leftTypeCode != PrimitiveType.Any && rightTypeCode != PrimitiveType.Any)
                {
                    PrimitiveType opReturnType = 0;

                    // Check for operation defined
                    switch (operation)
                    {
                        case BinaryOperation.Add:
                            {
                                // Check for defined
                                opReturnType = OpTable.GetAddOperationReturnType(leftTypeCode, rightTypeCode);
                                break;
                            }
                    }

                    // Check for invalid
                    if(opReturnType == 0)
                    {
                        report.ReportMessage(Code.NoBuiltInOperation, MessageSeverity.Error, syntax.Operation.Source, syntax.Operation.Text, left.EvaluatedTypeSymbol, right.EvaluatedTypeSymbol);
                    }
                    else
                    {
                        // We can resolve the inferred return type
                        inferredTypeSymbol = provider.ResolveTypeSymbol(opReturnType);
                    }
                }
                // Left and or right must be a custom type
                else
                {
                    // Check for primitive left
                    if(leftTypeCode != PrimitiveType.Any)
                    {
                        // We know that built in primitive types cannot support operation on user types
                        report.ReportMessage(Code.NoBuiltInOperation, MessageSeverity.Error, syntax.Operation.Source, syntax.Operation.Text, left.EvaluatedTypeSymbol, right.EvaluatedTypeSymbol);
                    }
                    else
                    {
                        // Must both be user types - check for operator defined on left
                    }
                }
            }
        }

        private bool IsSmallPrimitive(ITypeReferenceSymbol type)
        {
            if(type.IsPrimitive == true)
            {
                switch(type.PrimitiveType)
                {
                    // Note - bool does not require promotion because it is represented as i32 by the runtime
                    case PrimitiveType.I8:
                    case PrimitiveType.U8:
                    case PrimitiveType.I16:
                    case PrimitiveType.U16:
                    case PrimitiveType.Char:
                        return true;
                }
            }
            return false;
        }

        private PrimitiveType GetPromotedTypeCode(ITypeReferenceSymbol type)
        {
            if(type.IsPrimitive == true)
            {
                switch(type.PrimitiveType)
                {
                    case PrimitiveType.I8:
                    case PrimitiveType.U8:
                    case PrimitiveType.I16:
                    case PrimitiveType.U16:
                    case PrimitiveType.Char:
                    case PrimitiveType.Bool:
                        return PrimitiveType.I32;
                }
            }
            return type.PrimitiveType;
        }
    }
}
