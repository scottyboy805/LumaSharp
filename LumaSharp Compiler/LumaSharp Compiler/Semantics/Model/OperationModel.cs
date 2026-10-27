using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class OperationModel : IOperation
    {
        // Private
        private readonly SemanticModel model;
        private readonly ExpressionSyntax operation;
        private readonly OperationKind operationKind;

        // Properties
        public OperationKind Kind => operationKind;
        public bool IsBinary => operation is BinaryExpressionSyntax;
        public bool IsUnary => operation is UnaryExpressionSyntax;
        public string Name => "operation";
        public SyntaxSpan? Source => operation.GetSpan();

        // COnstructor
        public OperationModel(SemanticModel model, ExpressionSyntax operation)
        {
            // Check kind
            if (operation is not BinaryExpressionSyntax && operation is not UnaryExpressionSyntax)
                throw new ArgumentException("Must be binary or unary expression");

            this.model = model;
            this.operation = operation;
            this.operationKind = GetOperationKind();
        }

        // Methods
        public ITypeSymbol GetOperationType()
        {
            // Check for unary - should always be the same type
            if (operation is UnaryExpressionSyntax unary)
                return model.GetTypeSymbol(unary.Expression);

            // Get binary
            BinaryExpressionSyntax binary = (BinaryExpressionSyntax)operation;

            switch(operationKind)
            {
                case OperationKind.BinaryOr:
                case OperationKind.BinaryAnd:

                case OperationKind.BinaryEqual:
                case OperationKind.BinaryNotEqual:
                case OperationKind.BinaryLess:
                case OperationKind.BinaryLessEqual:
                case OperationKind.BinaryGreater:
                case OperationKind.BinaryGreaterEqual:
                    {
                        // Convert to boolean kind
                        return model.Binder.GetReservedType(ReservedType.Bool);
                    }

                case OperationKind.BinaryBitwiseAnd:
                case OperationKind.BinaryBitwiseOr:
                case OperationKind.BinaryBitwiseXOr:
                case OperationKind.BinaryBitshiftLeft:
                case OperationKind.BinaryBitshiftRight:
                    {
                        // Use left type as result
                        return model.GetTypeSymbol(binary.Left);
                    }

                case OperationKind.BinaryAdd:
                case OperationKind.BinarySubtract:
                    {
                        // Check for matching type
                        ITypeSymbol left = model.GetTypeSymbol(binary.Left);
                        ITypeSymbol right = model.GetTypeSymbol(binary.Right);

                        // Check for matching kind
                        if (left == right)
                            return left;

                        // Type needs to be inferred 
                        return null;
                    }
            }
            throw new NotSupportedException(operationKind.ToString());
        }

        public ISymbol GetLeft()
        {
            // Check for binary
            if(operation is BinaryExpressionSyntax binary)
                return model.GetSymbol(binary.Left).Symbol;

            // No symbol
            return null;
        }

        public ISymbol GetRight()
        {
            // Check for binary
            if (operation is BinaryExpressionSyntax binary)
                return model.GetSymbol(binary.Right).Symbol;

            // No symbol
            return null;
        }

        public ISymbol GetSingle()
        {
            // Check for unary
            if(operation is UnaryExpressionSyntax unary)
                return model.GetSymbol(unary.Expression).Symbol;

            // No symbol
            return null;
        }

        private OperationKind GetOperationKind()
        {
            // Get operation token
            SyntaxToken operationToken = operation switch
            {
                BinaryExpressionSyntax binary => binary.Operation,
                UnaryExpressionSyntax unary => unary.Operation,
                _ => throw new Exception("Unable to determine operation kind: " + operation),
            };

            // Map kind
            return operationToken.Kind switch
            {
                // Binary
                SyntaxTokenKind.OrSymbol => OperationKind.BinaryOr,
                SyntaxTokenKind.AndSymbol => OperationKind.BinaryAnd,

                SyntaxTokenKind.BitwiseAndSymbol => OperationKind.BinaryBitwiseAnd,
                SyntaxTokenKind.BitwiseOrSymbol => OperationKind.BinaryBitwiseOr,
                SyntaxTokenKind.BitwiseXOrSymbol => OperationKind.BinaryBitwiseXOr,

                SyntaxTokenKind.EqualitySymbol => OperationKind.BinaryEqual,
                SyntaxTokenKind.NonEqualitySymbol => OperationKind.BinaryNotEqual,
                SyntaxTokenKind.LessSymbol => OperationKind.BinaryLess,
                SyntaxTokenKind.LessEqualSymbol => OperationKind.BinaryLessEqual,
                SyntaxTokenKind.GreaterSymbol => OperationKind.BinaryGreater,
                SyntaxTokenKind.GreaterEqualSymbol => OperationKind.BinaryGreaterEqual,
                SyntaxTokenKind.BitShiftLeftSymbol => OperationKind.BinaryBitshiftLeft,
                SyntaxTokenKind.BitShiftRightSymbol => OperationKind.BinaryBitshiftRight,
                SyntaxTokenKind.AddSymbol => OperationKind.BinaryAdd,
                SyntaxTokenKind.SubtractSymbol => OperationKind.BinarySubtract,
                SyntaxTokenKind.MultiplySymbol => OperationKind.BinaryMultiply,
                SyntaxTokenKind.DivideSymbol => OperationKind.BinaryDivide,
                SyntaxTokenKind.ModulusSymbol => OperationKind.BinaryModulus,

                _ => throw new NotSupportedException(operationToken.Kind.ToString()),
            };
        }
    }
}
