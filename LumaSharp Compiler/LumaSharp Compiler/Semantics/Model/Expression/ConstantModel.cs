using LumaSharp_Compiler.Semantics.Model.Expression;
using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;
using System.Linq.Expressions;

namespace LumaSharp_Compiler.Semantics.Model
{
    public enum ConstantType
    {
        LiteralString,
        Hexadecimal,
        Hexadecimal_Long,
        Integer,
        Integer_Unsigned,
        Integer_Long,
        Integer_UnsignedLong,
        Decimal_Single,
        Decimal_Double,
        True,
        False,
        Null,
    }

    public sealed class ConstantModel : ExpressionModel
    {
        // Private
        private SyntaxToken literal;
        private SyntaxToken descriptor;
        private ITypeReferenceSymbol constantTypeSymbol = null;
        private object constantValue = null;

        // Properties
        public override bool IsStaticallyEvaluated
        {
            get { return constantTypeSymbol.PrimitiveType != PrimitiveType.Any; }
        }

        public override ITypeReferenceSymbol EvaluatedTypeSymbol
        {
            get { return constantTypeSymbol; }
        }

        public override IEnumerable<SymbolModel> Descendants
        {
            get { yield break; }
        }

        // Constructor
        public ConstantModel(SemanticModel model, SymbolModel parent, object value)
            : base(model, parent, null)
        {
            if(value == null)
            {
                literal = new SyntaxToken("null");
            }
            else if(value is string)
            {
                literal = new SyntaxToken("\"" + value + "\"");
            }
            else
            {
                switch(Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Byte:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        {
                            literal = new SyntaxToken(value.ToString());                            
                            break;
                        }
                    case TypeCode.UInt32:
                        {
                            literal = new SyntaxToken(value.ToString());
                            descriptor = new SyntaxToken("U");
                            break;
                        }
                    case TypeCode.Int64:
                        {
                            literal = new SyntaxToken(value.ToString());
                            descriptor = new SyntaxToken("L");
                            break;
                        }
                    case TypeCode.UInt64:
                        {
                            literal = new SyntaxToken(value.ToString());
                            descriptor = new SyntaxToken("UL");
                            break;
                        }
                    case TypeCode.Single:
                        {
                            literal = new SyntaxToken(value.ToString());
                            descriptor = new SyntaxToken("F");
                            break;
                        }
                    case TypeCode.Double:
                        {
                            literal = new SyntaxToken(value.ToString());
                            descriptor = new SyntaxToken("D");
                            break;
                        }
                }
            }
        }

        public ConstantModel(SemanticModel model, SymbolModel parent, LiteralExpressionSyntax literal) 
            : base(model, parent, literal)
        {
            this.literal = literal.Value;
            this.descriptor = literal.Descriptor;
        }

        // Methods
        public override void Accept(ISemanticVisitor visitor)
        {
            visitor.VisitConstant(this);
        }

        public override void ResolveSymbols(ISymbolProvider provider, ICompileReportProvider report)
        {
            // Get the constant type
            ConstantType constantType = ResolveConstantType(literal, descriptor);

            // Check for string
            if (constantType == ConstantType.LiteralString)
            {
                // Resolve symbol
                this.constantTypeSymbol = provider.ResolveTypeSymbol(null, new TypeReferenceSyntax("string"));
            }
            else
            {
                // Map to primitive value
                PrimitiveType primitiveType = constantType switch
                {
                    ConstantType.Null => PrimitiveType.Any,
                    ConstantType.LiteralString => PrimitiveType.Any,
                    ConstantType.Hexadecimal => PrimitiveType.I32,
                    ConstantType.Hexadecimal_Long => PrimitiveType.I64,
                    ConstantType.Integer => PrimitiveType.I32,
                    ConstantType.Integer_Unsigned => PrimitiveType.U32,
                    ConstantType.Integer_Long => PrimitiveType.I64,
                    ConstantType.Integer_UnsignedLong => PrimitiveType.U64,
                    ConstantType.Decimal_Single => PrimitiveType.F32,
                    ConstantType.Decimal_Double => PrimitiveType.F64,
                    ConstantType.True => PrimitiveType.Bool,
                    ConstantType.False => PrimitiveType.Bool,

                    _ => throw new NotImplementedException(),
                };

                // Resolve symbol
                this.constantTypeSymbol = provider.ResolveTypeSymbol(primitiveType, literal.Source);
            }
        }

        public override object GetStaticallyEvaluatedValue()
        {
            // Get the constant type
            ConstantType constantType = ResolveConstantType(literal, descriptor);

            // Check type
            switch (constantType)
            {
                case ConstantType.Hexadecimal:
                case ConstantType.Integer: return GetConstantValueAs<int>();
                case ConstantType.Integer_Unsigned: return GetConstantValueAs<uint>();
                case ConstantType.Hexadecimal_Long:
                case ConstantType.Integer_Long: return GetConstantValueAs<long>();
                case ConstantType.Integer_UnsignedLong: return GetConstantValueAs<ulong>();
                case ConstantType.Decimal_Single: return GetConstantValueAs<float>();
                case ConstantType.Decimal_Double: return GetConstantValueAs<double>();
                case ConstantType.True: return 1;
                case ConstantType.False: return 0;
            }
            return null;
        }

        public T GetConstantValueAs<T>()
        {
            return (T)constantValue;
        }

        private ConstantType ResolveConstantType(SyntaxToken token, SyntaxToken descriptor)
        {
            // Check for literal
            if (token.Text.Length >= 2 && token.Text[0] == '"' && token.Text[token.Text.Length - 1] == '"')
            {
                constantValue = token.Text.Substring(1, token.Text.Length - 2);
                return ConstantType.LiteralString;
            }

            // Check for true
            if (token.Text == "true")
            {
                constantValue = true;
                return ConstantType.True;
            }

            // Check for false
            if (token.Text == "false")
            {
                constantValue = false;
                return ConstantType.False;
            }

            // Check for null
            if (token.Text == "null")
            {
                constantValue = null;
                return ConstantType.Null;
            }

            // Check for hex
            if (token.Text.StartsWith("0x") == true)
            {
                // Check for number of characters
                if (token.Text.Remove(0, 2).Length > 8)
                    return ConstantType.Hexadecimal_Long;

                return ConstantType.Hexadecimal;
            }

            // Check for decimal
            if(token.Text.Contains('.') == true)
            {
                // Check for descriptor
                if (descriptor != null)
                {
                    // Check for D
                    if(descriptor.Text == "D")
                    {
                        double d;
                        if (double.TryParse(token.Text, out d) == false)
                            throw new InvalidOperationException("Constant expression decorated as 'double' cannot be expressed as a 64-bit value because it exceeds the maximum value: " + token.Text);

                        // Get as double
                        constantValue = d;
                        return ConstantType.Decimal_Double;
                    }
                    // Check for F
                    else if(descriptor.Text == "F")
                    {
                        float f;
                        if (float.TryParse(token.Text, out f) == false)
                            throw new InvalidOperationException("Constant expression decorated as `float` cannot be expressed as a 32-bit value because it exceeds the maximum value: " + token.Text);

                        constantValue = f;
                        return ConstantType.Decimal_Single;
                    }
                }
                else
                {
                    // Try to get as double
                    double d;
                    if (double.TryParse(token.Text, out d) == true)
                    {
                        constantValue = d;
                        return ConstantType.Decimal_Double;
                    }

                    // Try to get as float
                    float f;
                    if (float.TryParse(token.Text, out f) == true)
                    {
                        constantValue = f;
                        return ConstantType.Decimal_Single;
                    }
                }

                // Invalid
                throw new InvalidOperationException("Constant expression cannot be implicitly converted to single or double as it exceeds the maximum value: " + token.Text);
            }

            // Check for integer
            {
                if (descriptor != null)
                {
                    // Check for U
                    if (descriptor.Text == "U")
                    {
                        uint u;
                        if (uint.TryParse(token.Text, out u) == false)
                            throw new InvalidOperationException("Constant expression decorated as `u32` cannot be expressed as a 32-bit unsigned value because it exceeds the maximum value: " + token.Text);

                        constantValue = u;
                        return ConstantType.Integer_Unsigned;
                    }
                    // Check for L
                    else if(descriptor.Text == "L")
                    {
                        long l;
                        if (long.TryParse(token.Text, out l) == false)
                            throw new InvalidOperationException("Constant expression decorated as `i64` cannot be expressed as a 64-bit value because it exceeds the maximum value: " + token.Text);

                        constantValue = l;
                        return ConstantType.Integer_Long;
                    }
                    // Check for UL
                    else if(descriptor.Text == "UL")
                    {
                        ulong ul;
                        if (ulong.TryParse(token.Text, out ul) == false)
                            throw new InvalidOperationException("Constant expression decorated as `u64` cannot be expressed as a 64-bit unsigned value because it exceeds the maximum value: " + token.Text);

                        constantValue = ul;
                        return ConstantType.Integer_UnsignedLong;
                    }
                }
                else
                {
                    // Check for integer
                    int i;
                    if (int.TryParse(token.Text, out i) == true)
                    {
                        constantValue = i;
                        return ConstantType.Integer;
                    }

                    // Check for long int
                    long l;
                    if (long.TryParse(token.Text, out l) == true)
                    {
                        constantValue = l;
                        return ConstantType.Integer_Long;
                    }

                    // Check for unsigned long int
                    ulong ul;
                    if (ulong.TryParse(token.Text, out ul) == true)
                    {
                        constantValue = ul;
                        return ConstantType.Integer_UnsignedLong;
                    }
                }
            }

            throw new InvalidOperationException("Could not determine the compile time constant value of expression: " + token.Text);
        }
    }
}
