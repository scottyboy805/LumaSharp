
namespace LumaSharp.Runtime.Reflection
{
    [Flags]
    internal enum MetaVariableFlags : ushort
    {
        ByReference = 1 << 1,
        DefaultValue = 1 << 2,
        VariableLength = 1 << 3,
    }

    public sealed class MetaVariable
    {
        // Internal
        internal AppContext context = null;

        // Private
        private MetaVariableFlags variableFlags = 0;
        private string name = "";
        private int index = 0;
        private MemberReference<MetaType> variableType = null;

        // Properties
        public string Name
        {
            get { return name; }
        }

        public int Index
        {
            get { return index; }
        }

        public MetaType VariableType
        {
            get { return variableType.Member; }
        }

        public bool IsReference
        {
            get { return (variableFlags & MetaVariableFlags.ByReference) != 0; }
        }

        public bool IsDefaultValue
        {
            get { return (variableFlags & MetaVariableFlags.DefaultValue) != 0; }
        }

        public bool IsVariableLength
        {
            get { return (variableFlags & MetaVariableFlags.VariableLength) != 0; }
        }

        // Constructor
        internal MetaVariable(AppContext context, int index)
        {
            this.context = context;
            this.index = index;
        }

        internal MetaVariable(AppContext context, string name, int index, MemberReference<MetaType> parameterType, MetaVariableFlags parameterFlags)
        {
            this.context = context;
            this.name = name;
            this.index = index;
            this.variableType = parameterType;
            this.variableFlags = parameterFlags;
        }

        // Methods
        //internal void LoadParameterMetadata(BinaryReader reader)
        //{
        //    // Read parameter type
        //    this.variableType = new MemberReference<MetaType>(
        //        context, reader.ReadInt32());

        //    // Read flags
        //    this.variableFlags = (MetaVariableFlags)reader.ReadUInt32();
        //}
    }
}
