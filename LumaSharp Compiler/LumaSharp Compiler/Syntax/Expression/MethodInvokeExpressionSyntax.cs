
using System.Security.Principal;

namespace LumaSharp_Compiler.Syntax.Expression
{
    public sealed class MethodInvokeExpressionSyntax : ExpressionSyntax
    {
        // Private
        private ExpressionSyntax accessExpression = null;
        private SyntaxToken identifier = null;
        private TypeReferenceSyntax[] genericArguments = null;
        private ExpressionSyntax[] arguments = null;

        // Properties
        public ExpressionSyntax AccessExpression
        {
            get { return accessExpression; }
        }

        public SyntaxToken Identifier
        {
            get { return identifier; }
        }

        public TypeReferenceSyntax[] GenericArguments
        {
            get { return genericArguments; }
        }

        public ExpressionSyntax[] Arguments
        {
            get { return arguments; }
        }

        public int GenericArgumentCount
        {
            get { return HasGenericArguments ? genericArguments.Length : 0; }
        }

        public int ArgumentCount
        {
            get { return HasArguments ? arguments.Length : 0; }
        }

        public bool HasGenericArguments
        {
            get { return genericArguments != null; }
        }

        public bool HasArguments
        {
            get { return arguments != null; }
        }

        public override SyntaxToken StartToken
        {
            get { return accessExpression.StartToken; }
        }

        public override SyntaxToken EndToken
        {
            get
            {
                if(HasArguments == true)
                    return arguments[arguments.Length - 1].EndToken;

                return identifier;
            }
        }

        // Methods
        public override void GetSourceText(TextWriter writer)
        {
            // Write access
            accessExpression.GetSourceText(writer);

            // Write dot 
            writer.Write('.');

            // Write method name
            writer.Write(identifier.ToString());

            // Write generics
            if(HasGenericArguments == true)
            {
                // Write lgen
                writer.Write('<');

                // Write all generics
                for(int i = 0; i < genericArguments.Length; i++)
                {
                    genericArguments[i].GetSourceText(writer);

                    // Write comma
                    if(i < genericArguments.Length - 1)
                        writer.Write(", ");
                }

                // Write rgen
                writer.Write('>');
            }

            // Write lparen
            writer.Write('(');

            // Write all arguments
            if (HasArguments == true)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    arguments[i].GetSourceText(writer);

                    // Write comma
                    if(i < arguments.Length - 1)
                        writer.Write(", ");
                }
            }

            // Write rparen
            writer.Write(')');
        }
    }
}
