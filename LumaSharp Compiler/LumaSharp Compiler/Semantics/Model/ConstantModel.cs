using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    internal sealed class ConstantModel : IConstantSymbol
    {
        // Private
        private readonly SemanticModel model;
        private readonly LiteralExpressionSyntax literal;

        // Properties
        public string Name => literal.Value.Text;
        public SyntaxSpan? Source => literal.GetSpan();

        public ConstantKind ConstantKind
        {
            get
            {
                switch (literal.Value.Kind)
                {
                    case SyntaxTokenKind.NullKeyword: return ConstantKind.Null;
                    case SyntaxTokenKind.TrueKeyword:
                    case SyntaxTokenKind.FalseKeyword: return ConstantKind.Boolean;
                }

                // Check for decimal
                if (literal.Value.Text.Contains('.') == true && double.TryParse(literal.Value.Text, out _) == true)
                    return ConstantKind.Decimal;

                // Check for integer
                if (ulong.TryParse(literal.Value.Text, out _) == true)
                    return ConstantKind.Integer;

                // Must be string?
                return ConstantKind.String;
            }
        }
        
        public bool? BooleanValue
        {
            get
            {
                // Check for bool kind
                if (ConstantKind == ConstantKind.Boolean)
                    return literal.Value.Kind == SyntaxTokenKind.TrueKeyword ? true : false;

                // Not a bool
                return null;
            }
        }

        public ulong? IntegerValue
        {
            get
            {
                // Check for integer kind
                if(ConstantKind == ConstantKind.Integer)
                    return ulong.Parse(literal.Value.Text);

                // Not an integer
                return null;
            }
        }

        public double? DecimalValue
        {
            get
            {
                // Check for decimal kind
                if(ConstantKind == ConstantKind.Decimal)
                    return double.Parse(literal.Value.Text);

                // Not a decimal
                return null;
            }
        }

        public string StringValue
        {
            get
            {
                // Check for string kind
                if(ConstantKind == ConstantKind.String)
                    return literal.Value.Text;

                // Not a string
                return null;
            }
        }

        // Constructor
        public ConstantModel(SemanticModel model, LiteralExpressionSyntax literal)
        {
            this.model = model;
            this.literal = literal;
        }
    }
}
