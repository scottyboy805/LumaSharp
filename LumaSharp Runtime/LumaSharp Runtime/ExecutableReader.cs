﻿using LumaSharp.Runtime.Handle;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    internal unsafe sealed class ExecutableReader
    {
        // Private
        private AppContext appContext = null;
        private BinaryReader reader = null;
        private byte* executableMemory = null;

        // Constructor
        internal ExecutableReader(AppContext appContext, Stream inputStream, long size = -1)
        {
            this.appContext = appContext;
            this.reader = new BinaryReader(inputStream);

            // Get size
            if (size == -1)
                size = inputStream.Length;

            // Allocate memory
            executableMemory = (byte*)NativeMemory.AllocZeroed((nuint)size * 2);

            // Read all memory
            inputStream.Read(new Span<byte>(executableMemory, (int)size));
        }

        ~ExecutableReader()
        {
            NativeMemory.Free(executableMemory);
            executableMemory = null;
        }

        // Methods
        public _MethodHandle* ReadMethodExecutable(int rva, bool define = true)
        {
            // Seek to rva
            reader.BaseStream.Seek(rva, SeekOrigin.Begin);

            // Read tokens
            _TokenHandle methodToken = new _TokenHandle(reader.ReadInt32());
            _TokenHandle declaringTypeToken = new _TokenHandle(reader.ReadInt32());

            // Read signature
            _MethodSignatureFlags signatureFlags = (_MethodSignatureFlags)reader.ReadUInt16();
            ushort parameterCount = reader.ReadUInt16();
            ushort returnCount = reader.ReadUInt16();

            // Read body
            ushort maxStack = reader.ReadUInt16();
            ushort localCount = reader.ReadUInt16();
            uint instructionsSize = reader.ReadUInt32();

            // Get base ptr
            byte* executableMethod = (executableMemory + rva);

            // Get handle
            _MethodHandle* methodHandle = (_MethodHandle*)executableMemory;

            // Get pointers
            byte* returnPtr = executableMethod + (sizeof(_MethodHandle) + instructionsSize);
            byte* parameterPtr = returnPtr + (sizeof(_VariableHandle) * returnCount);
            byte* variablePtr = parameterPtr + (sizeof(_VariableHandle) * parameterCount);

            // Store new method
            *methodHandle = new _MethodHandle(
                methodToken,
                declaringTypeToken,
                new _MethodSignatureHandle(parameterCount, returnCount, (_VariableHandle*)parameterPtr, (_VariableHandle*)returnPtr, signatureFlags),
                new _MethodBodyHandle(maxStack, (_VariableHandle*)variablePtr, localCount, instructionsSize));

            // Read instructions
            reader.BaseStream.Read(new Span<byte>(executableMethod + sizeof(_MethodHandle), (int)instructionsSize));

            // Read variables
            reader.BaseStream.Read(new Span<byte>(executableMethod + sizeof(_MethodHandle) + instructionsSize, sizeof(_VariableHandle) * (returnCount + parameterCount + localCount)));

            // Define
            if(define == true)
                appContext.DefineExecutableMethod(methodHandle);

            return methodHandle;
        }
    }
}
