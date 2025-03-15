
using System.Runtime.CompilerServices;

namespace LumaSharp.Runtime.Handle
{
    public unsafe readonly struct _FieldHandle
    {
        // Public
        public readonly int FieldToken;
        public readonly int DeclaringTypeToken;
        public readonly uint FieldOffset;
        public readonly _TypeHandle TypeHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_FieldHandle);

        // Constructor
        public _FieldHandle(int token, uint fieldOffset, _TypeHandle typeHandle)
        {
            this.FieldToken = token;
            this.FieldOffset = fieldOffset;
            this.TypeHandle = typeHandle;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetFieldAddress(byte* inst)
        {
            return (inst + FieldOffset);
        }

        internal static void SetFieldValue(_FieldHandle* field, IntPtr inst, StackData* value)
        {
            // Get field address
            byte* fieldMem = field->GetFieldAddress((byte*)inst);

            // Copy to memory
            StackData.CopyToMemory(value, fieldMem, field->TypeHandle.TypeCode);
        }

        internal static void GetFieldValue(_FieldHandle* field, IntPtr inst, StackData* value)
        {
            // Get field address
            byte* fieldMem = field->GetFieldAddress((byte*)inst);

            // Copy from memory
            StackData.CopyFromMemory(value, fieldMem, field->TypeHandle.TypeCode);
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
