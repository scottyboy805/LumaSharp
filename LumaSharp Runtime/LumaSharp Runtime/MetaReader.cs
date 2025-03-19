using LumaSharp.Runtime.Handle;
using LumaSharp.Runtime.Reflection;

namespace LumaSharp.Runtime
{
    internal sealed class MetaReader
    {
        // Private
        private AppContext appContext = null;
        private BinaryReader reader = null;

        // Methods
        public MetaMethod ReadMethodMeta()
        {
            // Read metadata
            _TokenHandle methodToken = new _TokenHandle(reader.ReadInt32());

            // Read flags
            MetaMethodFlags methodFlags = (MetaMethodFlags)reader.ReadUInt16();

            // Read name
            string methodName = reader.ReadString();            

            // Read rva
            int rva = reader.ReadInt32();

            // Return types
            MemberReference<MetaType>[] returnTypes = ReadMethodReturnTypes();

            // Parameters
            MetaVariable[] parameters = ReadMethodParameters();

            // Build method
            return new MetaMethod(appContext, methodToken, methodName, methodFlags, returnTypes, parameters);
        }

        private MemberReference<MetaType>[] ReadMethodReturnTypes()
        {
            // Get return types
            MemberReference<MetaType>[] returnTypes = new MemberReference<MetaType>
                [reader.ReadUInt16()];

            // Check for any
            if (returnTypes.Length > 0)
            {
                // Read all
                for (int i = 0; i < returnTypes.Length; i++)
                    returnTypes[i] = new MemberReference<MetaType>(appContext, new _TokenHandle(reader.ReadInt32()));
            }
            return returnTypes;
        }

        private MetaVariable[] ReadMethodParameters()
        {
            MetaVariable[] parameters = new MetaVariable[reader.ReadUInt16()];

            // Check for any
            if (parameters.Length > 0)
            {
                // Read all
                for (int i = 0; i < parameters.Length; i++)
                {
                    // Get parameter info
                    _TokenHandle parameterTypeToken = new _TokenHandle(reader.ReadInt32());
                    
                    // Read flags
                    MetaVariableFlags parameterFlags = (MetaVariableFlags)reader.ReadUInt16();

                    // Read name
                    string parameterName = reader.ReadString();

                    // Build parameter
                    parameters[i] = new MetaVariable(appContext, parameterName, i, new MemberReference<MetaType>(appContext, parameterTypeToken), parameterFlags);
                }
            }
            return parameters;
        }
    }
}
