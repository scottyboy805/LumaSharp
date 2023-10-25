
namespace LumaSharp.Runtime.Reflection
{
    public sealed class Parameter
    {
        // Type
        internal enum ParameterFlags
        {
            Reference = 1,
            DefaultValue = 2,
            VariableLength = 4,
        }

        // Private
        private ParameterFlags parameterFlags = 0;
        private string name = "";
        private int index = 0;
        private Type parameterType = null;

        // Properties
        public string Name
        {
            get { return name; }
        }

        public int Index
        {
            get { return index; }
        }

        public Type ParameterType
        {
            get { return parameterType; }
        }

        public bool IsReference
        {
            get { return (parameterFlags & ParameterFlags.Reference) != 0; }
        }

        public bool IsDefaultValue
        {
            get { return (parameterFlags & ParameterFlags.DefaultValue) != 0; }
        }

        public bool IsVariableLength
        {
            get { return (parameterFlags & ParameterFlags.VariableLength) != 0; }
        }

        // Constructor
        internal Parameter(string name, int index, Type parameterType, ParameterFlags parameterFlags)
        {
            this.name = name;
            this.index = index;
            this.parameterType = parameterType;
            this.parameterFlags = parameterFlags;
        }
    }
}
