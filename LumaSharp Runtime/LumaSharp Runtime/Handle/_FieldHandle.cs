
using System.Runtime.CompilerServices;

namespace LumaSharp.Runtime.Handle
{
    public unsafe readonly struct _FieldHandle
    {
        // Public
        public readonly _TokenHandle FieldToken;
        public readonly _TokenHandle DeclaringTypeToken;
        public readonly uint FieldOffset;
        public readonly _TypeHandle TypeHandle;

        // Public
        public static readonly uint Size = (uint)sizeof(_FieldHandle);

        // Constructor
        public _FieldHandle(_TokenHandle fieldToken, _TokenHandle declaringTypeToken, uint fieldOffset, _TypeHandle typeHandle)
        {
            this.FieldToken = fieldToken;
            this.DeclaringTypeToken = declaringTypeToken;
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
    }
}
