
namespace LumaSharp.Runtime.Handle
{
    public unsafe struct _FieldHandle
    {
        // Internal
        internal int FieldToken;
        internal uint FieldOffset;
        internal uint FieldSize;

        // Public
        public static readonly uint Size = (uint)sizeof(_FieldHandle);

        // Methods
        internal void Write(BinaryWriter writer)
        {
            writer.Write(FieldToken);
            writer.Write(FieldOffset);
            writer.Write(FieldSize);
        }

        internal void Read(BinaryReader reader)
        {
            FieldToken = reader.ReadInt32();
            FieldOffset = reader.ReadUInt32();
            FieldSize = reader.ReadUInt32();
        }
    }
}
