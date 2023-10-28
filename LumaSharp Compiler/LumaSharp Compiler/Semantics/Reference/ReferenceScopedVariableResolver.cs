using LumaSharp_Compiler.AST;
using LumaSharp_Compiler.Reporting;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal sealed class ReferenceScopedVariableResolver
    {
        // Private
        private ICompileReportProvider report = null;

        // Constructor
        public ReferenceScopedVariableResolver(ICompileReportProvider report)
        {
            this.report = report;
        }

        // Methods
        public bool ResolveReferenceIdentifierSymbol(ReferenceLibrary library, IReferenceSymbol context, VariableReferenceExpressionSyntax reference, out IIdentifierReferenceSymbol resolvedSymbol)
        {
            // Try to resolve from context
            return ResolveReferenceIdentifierSymbolFromContext(context, reference, out resolvedSymbol);
        }

        private bool ResolveReferenceIdentifierSymbolFromContext(IReferenceSymbol context, VariableReferenceExpressionSyntax reference, out IIdentifierReferenceSymbol resolvedIdentifier)
        {
            // Check for no context
            if(context == null)
            {
                resolvedIdentifier = null;
                return false;
            }

            // Check for method parameters first
            if(context is IMethodReferenceSymbol)
            {
                // Check if we can resolve from parameters
                if (ResolveReferenceIdentifierSymbolFromMethodParameterContext((IMethodReferenceSymbol)context, reference, out resolvedIdentifier) == true)
                    return true;
            }

            // Check for local variables secondly
            if(context is IScopedReferenceSymbol)
            {
                // Still need to check for method parameters before all else, but a little more work is required to move up to the method scope if possible
                if (ResolveReferenceIdentifierSymbolFromMethodParameterScopedContext((IScopedReferenceSymbol)context, reference, out resolvedIdentifier) == true)
                    return true;

                // Check if we can resolve from local scope
                if (ResolveReferenceIdentifierSymbolFromScopeContext((IScopedReferenceSymbol)context, reference, out resolvedIdentifier) == true)
                    return true;
            }

            // Check for field variables lastly


            // Failed to resolve
            resolvedIdentifier = null;
            return false;
        }

        private bool ResolveReferenceIdentifierSymbolFromMethodParameterScopedContext(IScopedReferenceSymbol context, VariableReferenceExpressionSyntax reference, out IIdentifierReferenceSymbol resolvedIdentifier)
        {
            IMethodReferenceSymbol methodSymbol = context as IMethodReferenceSymbol;
            IScopedReferenceSymbol current = context;

            // Move up the chain
            while(methodSymbol == null && current != null)
            {
                current = current.ParentSymbol as IScopedReferenceSymbol;
                methodSymbol = current as IMethodReferenceSymbol;
            }

            // Check for method found
            if(methodSymbol != null)
                return ResolveReferenceIdentifierSymbolFromMethodParameterContext(methodSymbol, reference, out resolvedIdentifier);

            // Cannot resolve because we are not in a method context?
            resolvedIdentifier = null;
            return false;
        }

        private bool ResolveReferenceIdentifierSymbolFromMethodParameterContext(IMethodReferenceSymbol context, VariableReferenceExpressionSyntax reference, out IIdentifierReferenceSymbol resolvedIdentifier)
        {
            // Check for no parameters
            if(context.ParameterSymbols != null)
            {
                // Check for parameters matching variable
                foreach(ILocalIdentifierReferenceSymbol parameter in context.ParameterSymbols)
                {
                    // Check for matching identifier
                    if (parameter.IdentifierName == reference.Identifier.Text)
                    {
                        resolvedIdentifier = parameter;
                        return true;
                    }
                }
            }

            // Check for generic parameters
            if(context.GenericParameterSymbols != null)
            {
                // Check for generic parameters matching variable
                foreach(IGenericParameterIdentifierReferenceSymbol genericParameter in context.GenericParameterSymbols)
                {
                    // Check for matching identifier
                    if(genericParameter.IdentifierName == reference.Identifier.Text)
                    {
                        resolvedIdentifier = genericParameter;
                        return true;
                    }
                }
            }

            // Failed to resolve
            resolvedIdentifier = null;
            return false;
        }

        private bool ResolveReferenceIdentifierSymbolFromScopeContext(IScopedReferenceSymbol context, VariableReferenceExpressionSyntax reference, out IIdentifierReferenceSymbol resolvedIdentifier)
        {
            // Check for no locals
            if (context.LocalsInScope != null)
            {
                // Check scope for matching variable
                foreach (ILocalIdentifierReferenceSymbol local in context.LocalsInScope)
                {
                    // Check for matching identifier
                    if (local.IdentifierName == reference.Identifier.Text)
                    {
                        resolvedIdentifier = local;
                        return true;
                    }
                }
            }

            // Move up to the parent scope
            if(context.ParentSymbol != null && context.ParentSymbol is IScopedReferenceSymbol)
            {
                // Search in parent scope
                return ResolveReferenceIdentifierSymbolFromScopeContext((IScopedReferenceSymbol)context.ParentSymbol, reference, out resolvedIdentifier);
            }

            // Could not resolve from scope
            resolvedIdentifier = null;
            return false;
        }
    }
}
