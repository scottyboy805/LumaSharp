
namespace LumaSharp.Runtime.Reflection
{
    public sealed class Parameter
    {
        // Type
        [Flags]
        internal enum ParameterFlags : uint
        {
            Reference = 1,
            DefaultValue = 2,
            VariableLength = 4,
        }

        // Internal
        internal AppContext context = null;

        // Private
        private ParameterFlags parameterFlags = 0;
        private string name = "";
        private int index = 0;
        private MemberReference<Type> parameterType = null;

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
            get { return parameterType.Member; }
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
        internal Parameter(AppContext context, int index)
        {
            this.context = context;
            this.index = index;
        }

        internal Parameter(AppContext context, string name, int index, Type parameterType, ParameterFlags parameterFlags)
        {
            this.context = context;
            this.name = name;
            this.index = index;
            this.parameterType = new MemberReference<Type>(parameterType);
            this.parameterFlags = parameterFlags;
        }

        // Methods
        internal void LoadParameterMetadata(BinaryReader reader)
        {
            // Read parameter type
            this.parameterType = new MemberReference<Type>(
                context, reader.ReadInt32());

            // Read flags
            this.parameterFlags = (ParameterFlags)reader.ReadUInt32();
        }
    }
}
