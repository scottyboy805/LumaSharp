
using System.Runtime.CompilerServices;

namespace LumaSharp.Runtime.Handle
{
    public unsafe readonly struct _FieldHandle
    {
        // Public
        public readonly int FieldToken;
        public readonly uint FieldOffset;
        public readonly _TypeHandle TypeHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_FieldHandle);

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetFieldAddress(byte* inst)
        {
            return (inst + FieldOffset);
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(FieldToken);
            writer.Write(FieldOffset);
            //writer.Write(FieldSize);
        }

        internal void Read(BinaryReader reader)
        {
            //FieldToken = reader.ReadInt32();
            //FieldOffset = reader.ReadUInt32();
            //FieldSize = reader.ReadUInt32();
        }
    }
}
