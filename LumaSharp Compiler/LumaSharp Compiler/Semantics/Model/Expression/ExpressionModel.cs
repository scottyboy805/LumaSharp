﻿using LumaSharp.Compiler.AST;

namespace LumaSharp.Compiler.Semantics.Model
{
    public abstract class ExpressionModel : SymbolModel
    {
        // Private
        private ExpressionSyntax syntax = null;

        // Properties
        /// <summary>
        /// Can the expression value or type be determined at compile time.
        /// </summary>
        public abstract bool IsStaticallyEvaluated { get; }

        public abstract ITypeReferenceSymbol EvaluatedTypeSymbol { get; }

        public IReferenceSymbol ParentSymbol
        {
            get
            {
                SymbolModel model = this;

                // Move up the hierarchy
                while ((model is IReferenceSymbol) == false && model.Parent != null)
                    model = model.Parent;

                // Get symbol
                return model as IReferenceSymbol;
            }
        }

        public SyntaxSource Source
        {
            get { return syntax.StartToken.Source; }
        }

        // Constructor
        internal ExpressionModel(SemanticModel model, SymbolModel parent, ExpressionSyntax syntax)
            : base(model, parent)
        {
            this.syntax = syntax;
        }

        // Methods
        public virtual ExpressionModel StaticallyEvaluateExpression(ISymbolProvider provider)
        {
            return this;
        }

        public virtual object GetStaticallyEvaluatedValue()
        {
            return null;
        }

        internal static ExpressionModel Any(SemanticModel model, SymbolModel parent, ExpressionSyntax syntax)
        {
            // Check for literal
            if(syntax is LiteralExpressionSyntax)
                return new ConstantModel(model, parent, syntax as  LiteralExpressionSyntax);

            // Check for type
            if (syntax is TypeReferenceSyntax)
                return new TypeReferenceModel(model, parent, syntax as TypeReferenceSyntax);

            // Check for variable
            if(syntax is VariableReferenceExpressionSyntax)
                return new VariableReferenceModel(model, parent, syntax as VariableReferenceExpressionSyntax);

            // Check for variable assign
            //if(syntax is VariableAssignExpressionSyntax)
            //    return new AssignModel(model, parent, syntax as )

            // Check for field
            if(syntax is FieldReferenceExpressionSyntax)
                return new FieldAccessorReferenceModel(model, parent, syntax as FieldReferenceExpressionSyntax);

            // Check for method
            if (syntax is MethodInvokeExpressionSyntax)
                return new MethodInvokeModel(model, parent, syntax as MethodInvokeExpressionSyntax);

            // Binary
            if(syntax is BinaryExpressionSyntax)
                return new BinaryModel(model, parent, syntax as BinaryExpressionSyntax);

            // This
            if (syntax is ThisExpressionSyntax)
                return new ThisModel(model, parent, syntax as ThisExpressionSyntax);

            // New
            if (syntax is NewExpressionSyntax)
                return new NewModel(model, parent, syntax as NewExpressionSyntax);

            throw new NotSupportedException("Specified syntax is not supported: " + syntax.GetType());
        }
    }
}
