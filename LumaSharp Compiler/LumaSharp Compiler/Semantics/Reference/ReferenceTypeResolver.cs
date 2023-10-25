using LumaSharp_Compiler.AST;

namespace LumaSharp_Compiler.Semantics.Reference
{
    internal sealed class ReferenceTypeResolver
    {
        // Methods
        public bool ResolveReferenceTypeSymbol(ReferenceLibrary library, IReferenceSymbol context, TypeReferenceSyntax reference, out ITypeReferenceSymbol resolvedType)
        {
            // Try to resolve from context
            if(ResolveReferenceTypeSymbolFromContext(context, reference, out resolvedType) == true) 
                return true;

            // Check for namespace provided
            if (reference.HasNamespace == true)
            {
                // Check the library for named types
                foreach (ITypeReferenceSymbol typeSymbol in library.GetDeclaredNamedTypes(reference.Namespace.Identifiers.Select(i => i.Text).ToArray()))
                {
                    // Check for matched
                    if (IsReferenceTypeSymbolMatch(typeSymbol, reference) == true)
                    {
                        resolvedType = typeSymbol;
                        return true;
                    }
                }
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
            if(context is ITypeReferenceSymbol && ((ITypeReferenceSymbol)context).DeclaringTypeSymbol != null)
            {
                // Move up the hierarchy to parent type (Sub type context)
                return ResolveReferenceTypeSymbolFromContext(((ITypeReferenceSymbol)context).DeclaringTypeSymbol, reference, out resolvedType);
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
                // Move up the hierarchy to declaring type
                return ResolveReferenceTypeSymbolFromContext(((IMethodReferenceSymbol)context).DeclaringTypeSymbol, reference, out resolvedType);
            }

            // Could not resolve from context
            resolvedType = null;
            return false;
        }

        private bool IsReferenceTypeSymbolMatch(ITypeReferenceSymbol typeSymbol, TypeReferenceSyntax reference)
        {
            // Check for nested type
            if (reference.HasParentTypeIdentifiers == true)
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
