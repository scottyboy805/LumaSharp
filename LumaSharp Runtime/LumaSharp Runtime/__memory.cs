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

        private static int stackSize = 0;

        // Internal
        internal static IntPtr stackBasePtr = IntPtr.Zero;

        // Methods
        public static void InitStack(int size = 4096)// * 1024)    // 4mb by default
        {
            // Get stack size
            stackSize = size;

            // Allocate stack memory
            stackBasePtr = (IntPtr)NativeMemory.AllocZeroed((nuint)stackSize);
        }

        public static void CheckStack(void* addr)
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

            // Clear tracked memory
            trackedMemory.Clear();

            // Release stack
            NativeMemory.Free((void*)stackBasePtr);
            stackBasePtr = IntPtr.Zero;
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
