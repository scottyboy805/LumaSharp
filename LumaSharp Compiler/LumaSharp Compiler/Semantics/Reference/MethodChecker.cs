
namespace LumaSharp_Compiler.Semantics.Reference
{
    internal static class MethodChecker
    {
        // Methods
        public static bool IsMethodInvokable(IMethodReferenceSymbol method, params ITypeReferenceSymbol[] argumentTypes)
        {
            int parameterOffset = (method.IsGlobal == false) ? 1 : 0;

            // Check for trivial case
            if((method.ParameterSymbols == null || method.ParameterSymbols.Length == parameterOffset) &&
                (argumentTypes == null || argumentTypes.Length == 0))
            {
                // Parameter less method can be invoked
                return true;
            }

            // Check all arguments
            for(int i = parameterOffset, j = 0; i < method.ParameterSymbols.Length; i++, j++)
            {
                // Get parameter
                ILocalIdentifierReferenceSymbol parameter = method.ParameterSymbols[i];

                // Check for argument provided
                if(argumentTypes != null && argumentTypes.Length > j)
                {
                    // Check if type is assignable
                    if (TypeChecker.IsTypeAssignable(argumentTypes[j], parameter.TypeSymbol) == false)
                        return false;
                }
                else
                {
                    // Check if parameter is optional
                    if (parameter.IsOptional == false)
                        return false;
                }
            }

            // Can be invoked with provided argument list
            return true;
        }

        public static int GetMethodInvokableScore(IMethodReferenceSymbol method, params ITypeReferenceSymbol[] argumentTypes)
        {
            // Check for invokable first
            if (IsMethodInvokable(method, argumentTypes) == false)
                return 0;

            int parameterOffset = (method.IsGlobal == false) ? 1 : 0;

            // Calculate score
            int score = 0;

            for(int i = parameterOffset, j = 0; i < method.ParameterSymbols.Length; i++, j++)
            {
                // Get parameter
                ILocalIdentifierReferenceSymbol parameter = method.ParameterSymbols[i];

                // Check for argument provided
                if(argumentTypes != null && argumentTypes.Length > j)
                {
                    // Get type match score
                    score += TypeChecker.GetTypeMatchScore(argumentTypes[j], parameter.TypeSymbol);
                }
            }

            return score;
        }

        public static int GetBestMatchingMethodOverload(IReadOnlyList<IMethodReferenceSymbol> potentialMethods, params ITypeReferenceSymbol[] argumentTypes)
        {
            // Check for no matches
            if (potentialMethods.Count == 0)
                return -1;

            // Check for trivial case
            if (potentialMethods.Count == 1 && IsMethodInvokable(potentialMethods[0], argumentTypes) == true)
                return 0;

            int bestMatchingIndex = 0;
            int bestMatchingScore = 0;
            bool hasMatch = false;
            bool ambiguousMatch = false;

            // Check all provided methods
            for(int i = 0; i <  potentialMethods.Count; i++)
            {
                // Get the method score
                int methodScore = GetMethodInvokableScore(potentialMethods[i], argumentTypes);

                // Check for better score
                if(methodScore > bestMatchingScore)
                {
                    bestMatchingScore = methodScore;
                    bestMatchingIndex = i;
                    hasMatch = true;
                    ambiguousMatch = false;
                }
                else if(methodScore == bestMatchingScore && bestMatchingScore > 0)
                {
                    // Set ambiguous match
                    ambiguousMatch = true;
                }
            }

            // Check for no match
            if (hasMatch == false)
                return -1;

            // Check for ambiguous
            if (ambiguousMatch == true)
                return -2;

            return bestMatchingIndex;
        }
    }
}
