
namespace LumaSharp.Runtime
{
    public unsafe struct _StackHandle
    {
        // Internal
        internal _TypeHandle TypeHandle;
        internal uint StackOffset;

        // Methods
        public override string ToString()
        {
            return string.Format("_StackHandle(type = {0}, offset = {1}, size = {2})", TypeHandle.TypeCode, StackOffset, TypeHandle.TypeSize);
        }

        internal void Write(BinaryWriter writer)
        {
            TypeHandle.Write(writer);
            writer.Write(StackOffset);
        }

        internal void Read(BinaryReader reader)
        {
            TypeHandle.Read(reader);
            StackOffset = reader.ReadUInt32();
        }
    }
}
