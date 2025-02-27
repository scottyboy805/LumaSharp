using LumaSharp.Runtime.Handle;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LumaSharp.Runtime
{
    internal static unsafe class __memory
    {
        // Private
        private sealed class MemoryTracker
        {
            // Destructor
            ~MemoryTracker()
            {
                // Cleanup all memory
                __memory.Cleanup();
            }
        }

        // Private
        private static readonly MemoryTracker tracker = new MemoryTracker();
        private static HashSet<IntPtr> trackedMemory = new HashSet<IntPtr>();
        private static HashSet<IntPtr> trackedStackMemory = new HashSet<IntPtr>();

        // Methods
        public static byte* InitStack(uint size = 4096)// * 1024)    // 4mb by default
        {
            // Allocate stack memory
            IntPtr stackMemory = (IntPtr)NativeMemory.AllocZeroed(size);

            // Track the memory
            trackedStackMemory.Add(stackMemory);

            return (byte*)stackMemory;
        }

        public static void CheckStack(void* addr, void* stackBasePtr, uint stackSize)
        {
            if (addr > (byte*)stackBasePtr + stackSize)
                throw new StackOverflowException();
        }

        public static void Cleanup()
        {
            // Release heap memory in use
            foreach(void* ptr in trackedMemory)
            {
                NativeMemory.Free(ptr);
            }

            // Release stack memory in use
            foreach(void* ptr in trackedStackMemory)
            {
                NativeMemory.Free(ptr);
            }

            // Clear tracked memory
            trackedMemory.Clear();
            trackedStackMemory.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(_TypeHandle* type)
        {
            // Check for zero
            if (type->TypeSize == 0)
                return null;

            // Size must include 4 bytes for reference counter and 4 bytes for type code
            uint fullSize = type->TypeSize + _MemoryHandle.Size;

            // Allocate
            void* mem;

            // Check for stack alloc
            //if(stackAlloc == true)
            //{
            //    // Allocate memory on stack
            //    mem = stackPtr;

            //    // Zero memory
            //    __memory.Zero(mem, fullSize);

            //    // Advance offset
            //    stackPtr += fullSize;
            //}
            //else
            {
                // Allocate memory on the heap
                mem = NativeMemory.AllocZeroed(fullSize);

                // Register memory
                trackedMemory.Add((IntPtr)mem);
            }

            // Insert memory handle before data
            (*((_MemoryHandle*)mem)).TypeHandle = type;

            // Get data offset
            mem = ((byte*)mem) + _MemoryHandle.Size;

            return mem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocArray(_TypeHandle* elementType, uint elementCount)
        {
            // Check for zero
            if (elementType->TypeSize == 0)
                return null;

            // Size must include type info??
            uint fullSize = (elementType->TypeSize * elementCount) + _ArrayHandle.Size;

            void* mem;

            // Check for stack alloc
            //if (stackAlloc == true)
            //{
            //    // Allocate memory on stack
            //    mem = stackPtr;

            //    // Zero memory
            //    __memory.Zero(mem, fullSize);

            //    // Advance offset
            //    stackPtr += fullSize;
            //}
            //else
            {
                // Allocate memory on heap
                mem = NativeMemory.AllocZeroed(fullSize);

                // Register memory
                trackedMemory.Add((IntPtr)mem);
            }

            // Insert memory handle before data
            (*((_ArrayHandle*)mem)).ElementCount = elementCount;
            (*((_ArrayHandle*)mem)).MemoryHandle.TypeHandle = elementType;

            // Get data offset
            mem = ((byte*)mem) + _ArrayHandle.Size;

            return mem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* mem)
        {
            // Check for null
            if (mem == null)
                throw new ArgumentNullException("Cannot free null ptr");

            // Untrack memory
            trackedMemory.Remove((IntPtr)mem);

            // Account for the reference memory also and type token
            mem = ((int*)mem) - (sizeof(int) * 2);

            // Free memory
            NativeMemory.Free(mem);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(void* source, void* destination, long size)
        {
            Buffer.MemoryCopy(source, destination, size, size);

            //switch(size)
            //{
            //    //case 1:
            //    //    {
            //    //        *(byte*)destination = *(byte*)source;
            //    //        break;
            //    //    }
            //    //case 2:
            //    //    {
            //    //        *(ushort*)destination = *(ushort*)source;
            //    //        break;
            //    //    }
            //    //case 4:
            //    //    {
            //    //        *(uint*)destination = *(uint*)source;
            //    //        break;
            //    //    }
            //    //case 8:
            //    //    {
            //    //        *(ulong*)destination = *(ulong*)source;
            //    //        break;
            //    //    }

            //    default:
            //        {
            //            Buffer.MemoryCopy(source, destination, size, size);
            //            break;
            //        }
            //}            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Zero(void* mem, uint size)
        {
            NativeMemory.Clear(mem, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadAs<T>(void* mem, int offset = 0) where T : unmanaged
        {
            // Get offset
            byte* ptr = (byte*)mem + offset;

            // Get as T
            return *(T*)ptr;
        }

        public static object ReadAs(_TypeHandle type, void* mem, int offset = 0)
        {
            // Get offset
            byte* ptr = (byte*)mem + offset;

            // Select type
            switch(type.TypeCode)
            {
                case RuntimeTypeCode.I8: return *(sbyte*)ptr;
                case RuntimeTypeCode.U8: return *(byte*)ptr;
                case RuntimeTypeCode.I16: return *(short*)ptr;
                case RuntimeTypeCode.U16: return *(ushort*)ptr;
                case RuntimeTypeCode.I32: return *(int*)ptr;
                case RuntimeTypeCode.U32: return *(uint*)ptr;
                case RuntimeTypeCode.I64: return *(long*)ptr;
                case RuntimeTypeCode.U64: return *(ulong*)ptr;
                case RuntimeTypeCode.F32: return *(float*)ptr;
                case RuntimeTypeCode.F64: return *(double*)ptr;

                default:
                    throw new NotSupportedException();
            }
        }

        public static void WriteAs<T>(T value, ref byte* mem, int offset = 0, bool incrementPtr = true) where T : unmanaged
        {
            // Get offset
            byte* ptr = (byte*)mem + offset;

            // Store value
            *(T*)ptr = value;

            // Increment ptr
            if (incrementPtr == true)
                mem = (byte*)mem + sizeof(T);
        }

        public static void WriteAs(object value, _TypeHandle* type, ref byte* mem, int offset = 0, bool incrementPtr = true)
        {
            // Get offset
            byte* ptr = (byte*)mem + offset;

            // Select type
            switch(type->TypeCode)
            {
                case RuntimeTypeCode.I8:
                    {
                        *(sbyte*)ptr = (sbyte)value;
                        break;
                    }
                case RuntimeTypeCode.U8:
                    {
                        *(byte*)ptr = (byte)value;
                        break;
                    }
                case RuntimeTypeCode.I16:
                    {
                        *(short*)ptr = (short)value;
                        break;
                    }
                case RuntimeTypeCode.U16:
                    {
                        *(ushort*)ptr = (ushort)value;
                        break;
                    }
                case RuntimeTypeCode.I32:
                    {
                        *(int*)ptr = (int)value;
                        break;
                    }
                case RuntimeTypeCode.U32:
                    {
                        *(uint*)ptr = (uint)value;
                        break;
                    }
                case RuntimeTypeCode.I64:
                    {
                        *(long*)ptr = (long)value;
                        break;
                    }
                case RuntimeTypeCode.U64:
                    {
                        *(ulong*)ptr = (ulong)value;
                        break;
                    }
                case RuntimeTypeCode.F32:
                    {
                        *(float*)ptr = (float)value;
                        break;
                    }
                case RuntimeTypeCode.F64:
                    {
                        *(double*)ptr = (double)value;
                        break;
                    }
                case RuntimeTypeCode.Any:
                    {
                        *(IntPtr*)ptr = (IntPtr)value;
                        break;
                    }

                default:
                    throw new NotSupportedException();
            }

            // Increment ptr
            if (incrementPtr == true)
                mem = (byte*)mem + type->TypeSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReferenced(void* mem)
        {
            // Get start of object
            int referenceCount = (int)(((int*)mem) - sizeof(int));

            // Check for any
            return referenceCount > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReferenceIncrement(void* mem)
        {
            // Get start of object
            int referenceCount = (int)(((int*)mem) - sizeof(int));

            // Modify reference value
            *((int*)mem) = referenceCount + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReferenceDecrement(void* mem)
        {
            // Get start of object
            int referenceCount = (int)(((int*)mem) - sizeof(int));

            // Check for out of bounds
            if (referenceCount < 0)
                throw new InvalidOperationException("Cannot decrement reference count to a negative value");

            // Modify reference value
            *((int*)mem) = referenceCount - 1;
        }
    }
}
