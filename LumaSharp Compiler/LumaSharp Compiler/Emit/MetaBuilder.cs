using LumaSharp.Compiler.Semantics;
using LumaSharp.Compiler.Semantics.Model;
using LumaSharp.Runtime.Reflection;

namespace LumaSharp.Compiler.Emit
{
    internal sealed class MetaBuilder : IDisposable
    {
        // Private
        private MemoryStream buffer = null;
        private BinaryWriter writer = null;

        // Constructor
        internal MetaBuilder(MemoryStream buffer = null)
        {
            // Check for null
            if(buffer == null)
                buffer = new MemoryStream();

            this.buffer = buffer;
            this.writer = new BinaryWriter(buffer);
        }

        // Methods
        public void Dispose()
        {
            writer.Dispose();
        }

        public void WriteTypeMeta(TypeModel type)
        {
            // Get flags
            MetaTypeFlags typeFlags = type.TypeFlags;

            // Write metadata
            writer.Write(type.SymbolToken.MetaToken);
            writer.Write(type.TypeName);
            writer.Write((ushort)typeFlags);

            // Generic parameters
            if ((typeFlags & MetaTypeFlags.Generic) != 0)
            {
                WriteGenericParameters(type.GenericParameterSymbols);
            }

            // Base type count
            writer.Write(type.BaseTypeSymbols.Length);

            foreach (ITypeReferenceSymbol baseType in type.BaseTypeSymbols)
            {
                // Write the type
                writer.Write(baseType.SymbolToken.MetaToken);
            }
        }

        public void WriteMethodMeta(MethodModel method, int rva)
        {
            // Get flags
            MetaMethodFlags methodFlags = method.MethodFlags;

            // Write metadata
            writer.Write(method.SymbolToken.MetaToken);
            writer.Write((ushort)methodFlags);
            writer.Write(method.MethodName);            

            // Rva - virtual address of method executable
            writer.Write(rva);

            // Return types
            if((methodFlags & MetaMethodFlags.ReturnValue) != 0)
            {
                WriteMethodReturnTypes(method.ReturnTypeSymbols);
            }

            // Generic parameters
            if((methodFlags & MetaMethodFlags.Generic) != 0)
            {
                WriteGenericParameters(method.GenericParameterSymbols);
            }

            // Parameter types
            if((methodFlags & MetaMethodFlags.ParamValues) != 0)
            {
                WriteMethodParameterMeta(method.ParameterSymbols);
            }
        }

        private void WriteMethodReturnTypes(ITypeReferenceSymbol[] returnTypes)
        {
            // Write count
            writer.Write((ushort)returnTypes.Length);

            // Write all
            for (int i = 0; i < returnTypes.Length; i++)
            {
                // Write type token
                writer.Write(returnTypes[i].SymbolToken.MetaToken);
            }
        }

        private void WriteMethodParameterMeta(ILocalIdentifierReferenceSymbol[] parameterSymbols)
        {
            // Write count
            writer.Write((ushort)parameterSymbols.Length);

            // Write all
            for (int i = 0; i < parameterSymbols.Length; i++)
            {
                // Build flags
                MetaVariableFlags parameterFlags = 0;
                if (parameterSymbols[i].IsByReference == true) parameterFlags |= MetaVariableFlags.ByReference;
                if (parameterSymbols[i].IsOptional == true) parameterFlags |= MetaVariableFlags.DefaultValue;

                // Write type token
                writer.Write(parameterSymbols[i].SymbolToken.MetaToken);

                // Write flags
                writer.Write((ushort)parameterFlags);

                // Write name
                writer.Write(parameterSymbols[i].IdentifierName);
            }
        }

        private void WriteGenericParameters(IGenericParameterIdentifierReferenceSymbol[] genericParameters)
        {
            // Write count
            writer.Write((ushort)genericParameters.Length);

            // Write all
            for(int i = 0; i < genericParameters.Length;i++)
            {
                // Write identifier
                writer.Write(genericParameters[i].TypeName);

                // Check for constraints
                writer.Write((ushort)genericParameters[i].TypeConstraintSymbols.Length);

                // Write all 
                for(int j = 0; j < genericParameters[i].TypeConstraintSymbols.Length; j++)
                {
                    // Write constraint type
                    writer.Write(genericParameters[i].TypeConstraintSymbols[j].SymbolToken.MetaToken);
                }
            }
        }

        //private void WriteTypeReferenceMeta(ITypeReferenceSymbol typeReference)
        //{
        //    writer.Write(typeReference.SymbolToken.MetaToken);
        //}

        //private void WriteGenericParameterReferenceMeta(IGenericParameterIdentifierReferenceSymbol genericParameter)
        //{
        //    // Write identifier
        //    writer.Write(genericParameter.TypeName);

        //    // Write constraints
        //    writer.Write((byte)genericParameter.TypeConstraintSymbols.Length);

        //    for(int i = 0; i < genericParameter.TypeConstraintSymbols.Length; i++)
        //    {
        //        // Write simple type symbol only
        //        WriteTypeReferenceMeta(genericParameter.TypeConstraintSymbols[i]);
        //    }
        //}

        //private void WriteLocalIdentifierReferenceMeta(ILocalIdentifierReferenceSymbol localIdentifierReference)
        //{
        //    // Write type
        //    WriteTypeReferenceMeta(localIdentifierReference.TypeSymbol);

        //    // Write name
        //    writer.Write(localIdentifierReference.IdentifierName);

        //    // Check for parameter
        //    if(localIdentifierReference.IsParameter == true)
        //    {
        //        // Build flags
        //        MetaVariableFlags variableFlags = 0;

        //        if (localIdentifierReference.IsByReference == true) variableFlags |= MetaVariableFlags.ByReference;
        //        if (localIdentifierReference.IsOptional == true) variableFlags |= MetaVariableFlags.DefaultValue;

        //        // Write the flags
        //        writer.Write((uint)variableFlags);

        //        // Write default value??
        //    }
        //}
    }
}
