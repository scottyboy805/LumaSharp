
namespace LumaSharp.Runtime.Reflection
{
    public sealed class MetaVariable
    {
        // Type
        [Flags]
        internal enum VariableFlags : uint
        {
            Reference = 1,
            DefaultValue = 2,
            VariableLength = 4,
        }

        // Internal
        internal AppContext context = null;

        // Private
        private VariableFlags parameterFlags = 0;
        private string name = "";
        private int index = 0;
        private MemberReference<MetaType> parameterType = null;

        // Properties
        public string Name
        {
            get { return name; }
        }

        public int Index
        {
            get { return index; }
        }

        public MetaType ParameterType
        {
            get { return parameterType.Member; }
        }

        public bool IsReference
        {
            get { return (parameterFlags & VariableFlags.Reference) != 0; }
        }

        public bool IsDefaultValue
        {
            get { return (parameterFlags & VariableFlags.DefaultValue) != 0; }
        }

        public bool IsVariableLength
        {
            get { return (parameterFlags & VariableFlags.VariableLength) != 0; }
        }

        // Constructor
        internal MetaVariable(AppContext context, int index)
        {
            this.context = context;
            this.index = index;
        }

        internal MetaVariable(AppContext context, string name, int index, MetaType parameterType, VariableFlags parameterFlags)
        {
            this.context = context;
            this.name = name;
            this.index = index;
            this.parameterType = new MemberReference<MetaType>(parameterType);
            this.parameterFlags = parameterFlags;
        }

        // Methods
        internal void LoadParameterMetadata(BinaryReader reader)
        {
            // Read parameter type
            this.parameterType = new MemberReference<MetaType>(
                context, reader.ReadInt32());

            // Read flags
            this.parameterFlags = (VariableFlags)reader.ReadUInt32();
        }
    }
}
