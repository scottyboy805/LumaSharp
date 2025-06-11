using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;

namespace LumaSharp.Runtime
{
    internal sealed class MetaReader
    {
        // Private
        private readonly AppContext appContext = null;
        private readonly BinaryReader reader = null;

        // Constructor
        internal MetaReader(AppContext appContext, Stream inputStream)
        {
            this.appContext = appContext;
            this.reader = new BinaryReader(inputStream);
        }

        // Methods
        public AssemblyContext ReadAssembly(bool throwOnError = true)
        {
            // Check for magic
            if (reader.ReadInt32() != MetaAssembly.magic)
            {
                // Check for throw
                if (throwOnError == true)
                    throw new InvalidDataException("Stream does not contain a valid Luma assembly format");

                return null;
            }

            // Get hint path
            string hintPath = (reader.BaseStream is FileStream fs) ? fs.Name : null;

            // Get assembly name
            MetaAssemblyName assemblyName = ReadAssemblyName(throwOnError, hintPath);

            // TODO - Read reference assemblies here

            // Create assembly
            MetaAssembly assembly = new MetaAssembly(reader.BaseStream, assemblyName);

            // Create context
            AssemblyContext assemblyContext = new AssemblyContext(appContext, assembly);


            // Read members in order
            ReadTypeDefinitions(assemblyContext);
            ReadFieldDefinitions(assemblyContext);
            ReadMethodDefinitions(assemblyContext);


            return assemblyContext;
        }

        private MetaAssemblyName ReadAssemblyName(bool throwOnError = true, string hintPath = null)
        {
            try
            {
                // Create module name
                MetaAssemblyName assemblyName = new MetaAssemblyName(reader, hintPath);
                return assemblyName;
            }
            catch
            {
                // Check for throw
                if (throwOnError == true)
                    throw;
            }
            return null;
        }

        private void ReadTypeDefinitions(AssemblyContext assemblyContext)
        {
            // Read size
            int size = reader.ReadInt32();

            // Read all
            for (int i = 0; i < size; i++)
            {
                // Read the type
                ReadTypeDefinition(assemblyContext);
            }
        }

        private void ReadTypeDefinition(AssemblyContext assemblyContext)
        {
            // Read the type token
            _TokenHandle typeToken = ReadMetaToken();

            // Read type declaring type token
            _TokenHandle declaringTypeToken = ReadMetaToken();

            // Read flags
            MetaTypeFlags typeFlags = (MetaTypeFlags)reader.ReadUInt16();

            // Read namespace
            _TokenHandle namespaceToken = ReadMetaToken();

            // Read name
            _TokenHandle nameToken = ReadMetaToken();

            // Read base type token
            _TokenHandle baseTypeToken = ReadMetaToken();

            // Read contract tokens
            _TokenHandle[] contractTokens = null;

            if((typeFlags & MetaTypeFlags.Contract) != 0)
            {
                // Allocate array
                contractTokens = new _TokenHandle[reader.ReadUInt16()];

                // Read all
                for (int i = 0; i < contractTokens.Length; i++)
                    contractTokens[i] = ReadMetaToken();
            }

            // Read members
            _TokenHandle[] memberTokens = null;
            int membersSize = 0;

            if((membersSize = reader.ReadUInt16()) > 0)
            {
                // Allocate array
                memberTokens = new _TokenHandle[membersSize];

                // Read all
                for(int i = 0; i < memberTokens.Length; i++)
                    memberTokens[i] = ReadMetaToken();
            }

            // Get the type code from the token
            RuntimeTypeCode typeCode = (RuntimeTypeCode)typeToken;

            // Create type
            MetaType type = new MetaType(assemblyContext,
                typeToken,
                declaringTypeToken,
                namespaceToken,
                nameToken,
                typeCode,
                typeFlags,
                baseTypeToken,
                contractTokens,
                memberTokens);

            // Define the type
            assemblyContext.DefineMetaMember(type);
        }

        private void ReadFieldDefinitions(AssemblyContext assemblyContext)
        {
            // Read size
            int size = reader.ReadInt32();

            // Read all
            for(int i = 0;  i < size; i++)
            {
                // Read the field
                ReadFieldDefinition(assemblyContext);
            }
        }

        private void ReadFieldDefinition(AssemblyContext assemblyContext)
        {
            // Read field token
            _TokenHandle fieldToken = ReadMetaToken();

            // Read field declaring type token
            _TokenHandle declaringTypeToken = ReadMetaToken();

            // Read flags
            FieldFlags fieldFlags = (FieldFlags)reader.ReadUInt16();

            // Read name token
            _TokenHandle fieldNameToken = ReadMetaToken();

            // Read field type token
            _TokenHandle fieldTypeToken = ReadMetaToken();


            // Create the field
            MetaField field = new MetaField(assemblyContext, fieldToken, declaringTypeToken, fieldTypeToken, fieldNameToken, fieldFlags);

            // Register the member
            assemblyContext.DefineMetaMember(field);
        }

        private void ReadMethodDefinitions(AssemblyContext assemblyContext)
        {
            // Read size
            int size = reader.ReadInt32();

            // Read all
            for (int i = 0; i < size; i++)
            {
                // Read the method
                ReadMethodDefinition(assemblyContext);
            }
        }

        private void ReadMethodDefinition(AssemblyContext assemblyContext)
        {
            // Read method token
            _TokenHandle methodToken = ReadMetaToken();

            // Read the method declaring type token
            _TokenHandle declaringTypeToken = ReadMetaToken();

            // Read flags
            MetaMethodFlags methodFlags = (MetaMethodFlags)reader.ReadUInt16();

            // Read name
            _TokenHandle methodNameToken = ReadMetaToken();

            // Read return types
            _TokenHandle[] returnTypeTokens = null;

            if ((methodFlags & MetaMethodFlags.ReturnValue) != 0)
            {
                // Allocate array
                returnTypeTokens = new _TokenHandle[reader.ReadUInt16()];

                // Read all
                for (int i = 0; i < returnTypeTokens.Length; i++)
                    returnTypeTokens[i] = ReadMetaToken();
            }
            else
                returnTypeTokens = new[] { (_TokenHandle)RuntimeTypeCode.Void };

            // Read parameters
            MetaVariable[] parameters = null;

            if((methodFlags & MetaMethodFlags.ParamValues) != 0)
            {
                // Allocate array
                parameters = new MetaVariable[reader.ReadUInt16()];

                // Read all
                for (int i = 0; i < parameters.Length; i++)
                    parameters[i] = ReadVariableDefinition(assemblyContext, i);
            }

            // Read rva of executable
            int rva = reader.ReadInt32();


            // Create the method
            MetaMethod method = new MetaMethod(assemblyContext, methodToken, declaringTypeToken, methodNameToken, methodFlags, returnTypeTokens, parameters, rva);

            // Register the member
            assemblyContext.DefineMetaMember(method);
        }

        private MetaVariable ReadVariableDefinition(AssemblyContext assemblyContext, int index)
        {
            // Read flags
            MetaVariableFlags variableFlags = (MetaVariableFlags)reader.ReadUInt16();

            // Read name
            _TokenHandle nameToken = ReadMetaToken();

            // Read type token
            _TokenHandle variableTypeToken = ReadMetaToken();

            // Create variable
            MetaVariable variable = new MetaVariable(assemblyContext, index, nameToken, variableTypeToken, variableFlags);

            return variable;
        }

        private _TokenHandle ReadMetaToken()
        {
            // Read the token
            int token = reader.ReadInt32();

            // Create token
            return new _TokenHandle(token);
        }


        //public MetaMethod ReadMethodMeta(bool define = true)
        //{
        //    // Read metadata
        //    _TokenHandle methodToken = new _TokenHandle(reader.ReadInt32());

        //    // Read flags
        //    MetaMethodFlags methodFlags = (MetaMethodFlags)reader.ReadUInt16();

        //    // Read name
        //    string methodName = reader.ReadString();            

        //    // Read rva
        //    int rva = reader.ReadInt32();

        //    // Return types
        //    MemberReference<MetaType>[] returnTypes = ReadMethodReturnTypes();

        //    // Parameters
        //    MetaVariable[] parameters = ReadMethodParameters();

        //    // Build method
        //    MetaMethod method = new MetaMethod(appContext, methodToken, methodName, methodFlags, returnTypes, parameters);

        //    // Check for define
        //    if(define == true)
        //        appContext.DefineMetaMember(method);

        //    return method;
        //}

        //private MemberReference<MetaType>[] ReadMethodReturnTypes()
        //{
        //    // Get return types
        //    MemberReference<MetaType>[] returnTypes = new MemberReference<MetaType>
        //        [reader.ReadUInt16()];

        //    // Check for any
        //    if (returnTypes.Length > 0)
        //    {
        //        // Read all
        //        for (int i = 0; i < returnTypes.Length; i++)
        //            returnTypes[i] = new MemberReference<MetaType>(appContext, new _TokenHandle(reader.ReadInt32()));
        //    }
        //    return returnTypes;
        //}

        //private MetaVariable[] ReadMethodParameters()
        //{
        //    MetaVariable[] parameters = new MetaVariable[reader.ReadUInt16()];

        //    // Check for any
        //    if (parameters.Length > 0)
        //    {
        //        // Read all
        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            // Get parameter info
        //            _TokenHandle parameterTypeToken = new _TokenHandle(reader.ReadInt32());
                    
        //            // Read flags
        //            MetaVariableFlags parameterFlags = (MetaVariableFlags)reader.ReadUInt16();

        //            // Read name
        //            string parameterName = reader.ReadString();

        //            // Build parameter
        //            parameters[i] = new MetaVariable(appContext, parameterName, i, new MemberReference<MetaType>(appContext, parameterTypeToken), parameterFlags);
        //        }
        //    }
        //    return parameters;
        //}
    }
}
