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
        public static void* Alloc(int size)
        {
            // Check for size
            if (size < 0)
                throw new ArgumentException("size must be 0 or greater");

            // Check for zero
            if (size == 0)
                return null;

            // Size must include 4 bytes for reference counter
            int fullSize = size + sizeof(int);

            // Allocate
            void* mem = NativeMemory.AllocZeroed((nuint)fullSize);

            // Register memory
            trackedMemory.Add((IntPtr)mem);

            // Get data offset
            mem = ((int*)mem) + sizeof(int);

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

            // Account for the reference memory also
            mem = ((int*)mem) - sizeof(int);

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
