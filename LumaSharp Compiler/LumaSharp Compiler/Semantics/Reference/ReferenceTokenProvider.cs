using LumaSharp.Compiler.Semantics.Model;
using LumaSharp.Runtime.Handle;

namespace LumaSharp.Compiler.Semantics.Reference
{
    internal sealed class ReferenceTokenProvider
    {
        // Private
        private Dictionary<string, _TokenHandle> stringTable = new();
        private Dictionary<TypeModel, _TokenHandle> typeDefinitionTokens = new();
        private Dictionary<FieldModel, _TokenHandle> fieldDefinitionTokens = new();
        private Dictionary<AccessorModel, _TokenHandle> accessorDefinitionTokens = new();
        private Dictionary<MethodModel, _TokenHandle> methodDefinitionTokens = new();

        // Methods
        public IReadOnlyList<string> GetStrings()
        {
            return stringTable.Keys.ToArray();
        }

        public _TokenHandle GetSymbolToken(IReferenceSymbol symbol)
        {
            // Check for null
            if (symbol == null)
                return default;

            // Check for string definition
            if(symbol is StringModel str)
            {
                // Check for existing
                if (stringTable.TryGetValue(str.Text, out _TokenHandle stringToken) == true)
                    return stringToken;

                // Create new token
                stringToken = _TokenHandle.String(stringTable.Count);
                stringTable[str.Text] = stringToken;
                return stringToken;
            }
            // Check for type definition
            else if(symbol is TypeModel type)
            {
                // Check for existing
                if (typeDefinitionTokens.TryGetValue(type, out _TokenHandle typeToken) == true)
                    return typeToken;

                // Create new token
                typeToken = _TokenHandle.TypeDef(typeDefinitionTokens.Count);
                typeDefinitionTokens[type] = typeToken;
                return typeToken;
            }
            // Check for field definition
            else if (symbol is FieldModel field)
            {
                // Check for existing

            }

            // No token
            throw new NotSupportedException("Could not get token for symbol: " + symbol);
        }
    }
}
