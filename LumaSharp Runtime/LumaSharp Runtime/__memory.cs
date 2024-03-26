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
        public static void* Alloc(ref byte* stackPtr, in _TypeHandle type, bool stackAlloc = false)
        {
            // Check for zero
            if (type.TypeSize == 0)
                return null;

            // Size must include 4 bytes for reference counter and 4 bytes for type code
            uint fullSize = type.TypeSize + _MemoryHandle.Size;

            // Allocate
            void* mem;

            // Check for stack alloc
            if(stackAlloc == true)
            {
                // Allocate memory on stack
                mem = stackPtr;

                // Zero memory
                __memory.Zero(mem, fullSize);

                // Advance offset
                stackPtr += fullSize;
            }
            else
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
        public static void* AllocArray(ref byte* stackPtr, in _TypeHandle type, uint elementCount, bool stackAlloc = false)
        {
            // Check for zero
            if (type.TypeSize == 0)
                return null;

            // Size must include type info??
            uint fullSize = (type.TypeSize * elementCount) + _ArrayHandle.Size;

            void* mem;

            // Check for stack alloc
            if (stackAlloc == true)
            {
                // Allocate memory on stack
                mem = stackPtr;

                // Zero memory
                __memory.Zero(mem, fullSize);

                // Advance offset
                stackPtr += fullSize;
            }
            else
            {
                // Allocate memory on heap
                mem = NativeMemory.AllocZeroed(fullSize);

                // Register memory
                trackedMemory.Add((IntPtr)mem);
            }

            // Insert memory handle before data
            (*((_ArrayHandle*)mem)).ElementCount = elementCount;
            (*((_ArrayHandle*)mem)).MemoryHandle.TypeHandle = type;

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
                case TypeCode.I8: return *(sbyte*)ptr;
                case TypeCode.U8: return *(byte*)ptr;
                case TypeCode.I16: return *(short*)ptr;
                case TypeCode.U16: return *(ushort*)ptr;
                case TypeCode.I32: return *(int*)ptr;
                case TypeCode.U32: return *(uint*)ptr;
                case TypeCode.I64: return *(long*)ptr;
                case TypeCode.U64: return *(ulong*)ptr;
                case TypeCode.F32: return *(float*)ptr;
                case TypeCode.F64: return *(double*)ptr;

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
            int size = 0;

            // Select type
            switch(type->TypeCode)
            {
                case TypeCode.I8:
                    {
                        *(sbyte*)ptr = (sbyte)value;
                        size = sizeof(sbyte);
                        break;
                    }
                case TypeCode.U8:
                    {
                        *(byte*)ptr = (byte)value;
                        size = sizeof(byte);
                        break;
                    }
                case TypeCode.I16:
                    {
                        *(short*)ptr = (short)value;
                        size = sizeof(short);
                        break;
                    }
                case TypeCode.U16:
                    {
                        *(ushort*)ptr = (ushort)value;
                        size = sizeof(ushort);
                        break;
                    }
                case TypeCode.I32:
                    {
                        *(int*)ptr = (int)value;
                        size = sizeof(int);
                        break;
                    }
                case TypeCode.U32:
                    {
                        *(uint*)ptr = (uint)value;
                        size = sizeof(uint);
                        break;
                    }
                case TypeCode.I64:
                    {
                        *(long*)ptr = (long)value;
                        size = sizeof(long);
                        break;
                    }
                case TypeCode.U64:
                    {
                        *(ulong*)ptr = (ulong)value;
                        size = sizeof(ulong);
                        break;
                    }
                case TypeCode.F32:
                    {
                        *(float*)ptr = (float)value;
                        size = sizeof(float);
                        break;
                    }
                case TypeCode.F64:
                    {
                        *(double*)ptr = (double)value;
                        size = sizeof(double);
                        break;
                    }

                default:
                    throw new NotSupportedException();
            }

            // Increment ptr
            if (incrementPtr == true)
                mem = (byte*)mem + size;
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
