using LumaSharp.Runtime.Handle;

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
        internal readonly AssemblyContext context = null;

        // Private
        private readonly MetaVariableFlags variableFlags = 0;
        private readonly int index = 0;
        private readonly StringReference nameReference;
        private readonly MemberReference<MetaType> variableTypeReference;

        // Properties
        public string Name
        {
            get { return nameReference.String; }
        }

        public int Index
        {
            get { return index; }
        }

        public MetaType VariableType
        {
            get { return variableTypeReference.Member; }
        }

        public _TokenHandle VariableTypeToken
        {
            get { return variableTypeReference.Token; }
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
        internal MetaVariable(AssemblyContext context, int index)
        {
            this.context = context;
            this.index = index;
        }

        internal MetaVariable(AssemblyContext context, int index, _TokenHandle nameToken, _TokenHandle variableTypeToken, MetaVariableFlags parameterFlags)
        {
            this.context = context;
            this.index = index;
            this.nameReference = new StringReference(context, nameToken);
            this.variableTypeReference = new MemberReference<MetaType>(context, variableTypeToken);
            this.variableFlags = parameterFlags;
        }
    }
}
