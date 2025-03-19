using LumaSharp.Compiler.AST;
using LumaSharp.Compiler.Reporting;

namespace LumaSharp.Compiler.Semantics.Reference
{
    internal sealed class ReferenceTypeResolver
    {
        // Private
        private ISymbolProvider provider = null;
        private ICompileReportProvider report = null;

        // Constructor
        public ReferenceTypeResolver(ISymbolProvider provider, ICompileReportProvider report)
        {
            this.provider = provider;
            this.report = report;
        }

        // Methods
        public bool CheckVoidTypeUsage(TypeReferenceSyntax voidType)
        {
            // Check for invalid use of generic
            if (voidType.IsGenericType == true)
            {
                report.ReportMessage(Code.TypeGenericPrimitive, MessageSeverity.Error, voidType.GenericArguments.StartToken.Source, voidType.Identifier.Text);
                return false;
            }

            // Check for invalid use of  array indexing
            if(voidType.IsArrayType == true)
            {
                report.ReportMessage(Code.TypeArrayPrimitive, MessageSeverity.Error, voidType.ArrayParameters.StartToken.Source, voidType.Identifier.Text);
                return false;
            }
            return true;
        }

        public bool ResolveReferenceTypeSymbol(ReferenceLibrary library, IReferenceSymbol context, TypeReferenceSyntax reference, out ITypeReferenceSymbol resolvedType)
        {
            // Try to resolve from context
            if(ResolveReferenceTypeSymbolFromContext(context, reference, out resolvedType) == true) 
                return true;

            // Check for namespace provided
            if (reference.HasNamespace == true)
            {
                // Resolve the namespace
                INamespaceReferenceSymbol resolvedNamespace = provider.ResolveNamespaceSymbol(reference.Namespace);

                // Check for no namespace
                if(resolvedNamespace == null)
                {
                    resolvedType = null;
                    return false;
                }

                // Search in namespace for type
                //foreach(ITypeReferenceSymbol )

                //// Check the library for named types
                //foreach (ITypeReferenceSymbol typeSymbol in library.GetDeclaredNamedTypes(reference.Namespace.Identifiers.Select(i => i.Text).ToArray()))
                //{
                //    // Check for matched
                //    if (IsReferenceTypeSymbolMatch(typeSymbol, reference) == true)
                //    {
                //        resolvedType = typeSymbol;
                //        return true;
                //    }
                //}
            }
            else
            {
                // Check the library
                foreach (ITypeReferenceSymbol typeSymbol in library.GetDeclaredRootTypes())
                {
                    // Check for matched
                    if (IsReferenceTypeSymbolMatch(typeSymbol, reference) == true)
                    {
                        resolvedType = typeSymbol;
                        return true;
                    }
                }
            }

            resolvedType = null;
            return false;
        }

        private bool ResolveReferenceTypeSymbolFromContext(IReferenceSymbol context, TypeReferenceSyntax reference, out ITypeReferenceSymbol resolvedType)
        {
            // Check for no context
            if(context == null)
            {
                resolvedType = null;
                return false;
            }

            // Check for trivial case
            if (context is ITypeReferenceSymbol && IsReferenceTypeSymbolMatch((ITypeReferenceSymbol)context, reference) == true)
            {
                resolvedType = (ITypeReferenceSymbol)context;
                return true;
            }

            // Check for parent type context - recursive up search
            if (context is ITypeReferenceSymbol)
            {
                // Try to resolve from method generic parameters
                if (ResolveReferenceTypeSymbolFromTypeGenericContext((ITypeReferenceSymbol)context, reference, out resolvedType) == true)
                    return true;

                // Check for declaring type
                if (((ITypeReferenceSymbol)context).DeclaringTypeSymbol != null)
                {
                    // Move up the hierarchy to parent type (Sub type context)
                    return ResolveReferenceTypeSymbolFromContext(((ITypeReferenceSymbol)context).DeclaringTypeSymbol, reference, out resolvedType);
                }
            }

            // Check for field context
            if(context is IFieldReferenceSymbol)
            {
                // Move up the hierarchy to declaring type
                return ResolveReferenceTypeSymbolFromContext(((IFieldReferenceSymbol)context).DeclaringTypeSymbol, reference, out resolvedType);
            }

            // Check for accessor context
            if(context is IAccessorReferenceSymbol)
            {
                // Move up the hierarchy to declaring type
                return ResolveReferenceTypeSymbolFromContext(((IAccessorReferenceSymbol)context).DeclaringTypeSymbol, reference, out resolvedType);
            }

            // Check for method context
            if(context is IMethodReferenceSymbol)
            {
                // Try to resolve from method generic parameters
                if (ResolveReferenceTypeSymbolFromMethodGenericContext((IMethodReferenceSymbol)context, reference, out resolvedType) == true)
                    return true;

                // Move up the hierarchy to declaring type
                return ResolveReferenceTypeSymbolFromContext(((IMethodReferenceSymbol)context).DeclaringTypeSymbol, reference, out resolvedType);
            }

            // Could not resolve from context
            resolvedType = null;
            return false;
        }

        private bool ResolveReferenceTypeSymbolFromTypeGenericContext(ITypeReferenceSymbol typeContext, TypeReferenceSyntax reference, out ITypeReferenceSymbol resolvedType)
        {
            // Check for generic parameters
            if(typeContext.GenericParameterSymbols != null)
            {
                for(int i = 0; i <  typeContext.GenericParameterSymbols.Length; i++)
                {
                    // Check for matching symbol
                    if (IsReferenceTypeSymbolMatch(typeContext.GenericParameterSymbols[i], reference) == true)
                    {
                        resolvedType = typeContext.GenericParameterSymbols[i];
                        return true;
                    }
                }
            }

            // Failed to resolve
            resolvedType = null;
            return false;
        }

        private bool ResolveReferenceTypeSymbolFromMethodGenericContext(IMethodReferenceSymbol methodContext, TypeReferenceSyntax reference, out ITypeReferenceSymbol resolvedType)
        {
            // Check for generic parameters
            if(methodContext.GenericParameterSymbols != null)
            {
                for(int i = 0; i < methodContext.GenericParameterSymbols.Length; i++)
                {
                    // Check for matching symbol
                    if (IsReferenceTypeSymbolMatch(methodContext.GenericParameterSymbols[i], reference) == true)
                    {
                        resolvedType = methodContext.GenericParameterSymbols[i];
                        return true;
                    }
                }
            }

            // Failed to resolve
            resolvedType = null;
            return false;
        }

        private bool IsReferenceTypeSymbolMatch(ITypeReferenceSymbol typeSymbol, TypeReferenceSyntax reference)
        {
            // Check for nested type
            if (reference.HasParentType == true)
            {

            }
            else
            {
                // Check for matched type name
                if (typeSymbol.TypeName == reference.Identifier.Text)
                    return true;
            }
            // Not a match
            return false;
        }
    }
}
