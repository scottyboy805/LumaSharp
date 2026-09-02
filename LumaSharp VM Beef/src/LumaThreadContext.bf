using System;
using System.Threading;
using internal LumaSharpVMBeef;

namespace LumaSharpVMBeef;

internal sealed class LumaThreadContext
{
	// Private
	private Thread thread;
	private uint64 stackSize;
	private uint8[] stackMemory;

	private volatile bool isThreadOwned = false;
	private volatile LumaStackContext stackContext;

	private uint8* spPush = null;
	private volatile bool start = false;
	private volatile bool executing = false;

	// Internal
	internal bool abort = false;

	// Properties
	public Thread Thread => thread;
	public int32 ThreadId => (int32)thread.Id;
	public bool IsThreadOwned => isThreadOwned;
	public bool IsCallingThread => thread == System.Threading.Thread.CurrentThread;
	public uint64 StackSize => stackSize;
	public uint8* StackBasePtr => stackMemory.Ptr;
	public uint8* StackMaxPtr => stackMemory.Ptr + stackSize;

	public bool IsExecuting => executing;
	public LumaMethodHandle* ExecutingMethod => stackContext.method;

	// Constructor
	public this(uint64 stackSize, bool standaloneThread = false)
	{
		this.stackSize = stackSize;
		this.stackMemory = new uint8[stackSize];

		// Create the thread
		thread = standaloneThread == true
			? new Thread(new => ExecuteOnThread)
			: System.Threading.Thread.CurrentThread;
		isThreadOwned = standaloneThread;
	}

	public ~this()
	{
		// Cleanup thread
		Abort();
		delete thread;

		// Cleanup stack
		delete stackMemory;
	}

	// Methods
	public void PrepareCall(LumaMethodHandle* method)
	{
		// Check for already executing
		if(executing == true || stackContext.method != null)
			Runtime.FatalError("Thread is already executing and cannot prepare a new call");

		// Check for no address
		if(method.rva == null)
			Runtime.FatalError("Method does not have a body");

		// Get base ptr
		uint8* spBase = StackBasePtr;

		// Prepare stack
		stackContext = LumaStackContext(method, StackBasePtr, StackBasePtr, StackMaxPtr);

		// Push call frame to stack
		*(LumaStackContext*)spBase = stackContext;
		spBase += sizeof(LumaStackContext);

		spPush = spBase;
	}

	public void PushArgument<T>(in T val) where T : struct
	{
		// Check for invalid state
		if(spPush == null)
			Runtime.FatalError("You must prepare a call before pushing any arguments");

		// Push value
		*(T*)spPush = val;
		spPush += sizeof(T);
	}

	public T PopReturn<T>() where T : struct
	{
		// Check for sp
		if(stackContext.sp == null)
			Runtime.FatalError("Stack pointer must be valid to get a return value");

		// Get value on stack
		return *(T*)(stackContext.sp - sizeof(T));
	}

	public void Execute(bool awaitResult = true)
	{
		// Check for thread running
		if(executing == true)
			Runtime.FatalError("Thread is already executing");

		// Setup flags
		start = true;
		executing = true;

		// Check for calling thread
		if(IsCallingThread == true)
		{
			// Call direct
			ExecuteOnThread();
		}
		// Call remote thread
		else
		{
			// Start the thread if not already running
			if(thread.IsAlive == false)
				thread.Start();
	
			// Wait for result
			if(awaitResult == true)
			{
				// Wait until exited
				while(executing == true)
					System.Threading.Thread.Sleep(10);
			}
		}
	}

	public void Abort()
	{
		if(thread.IsAlive == true && isThreadOwned == true)
		{
			abort = true;
			thread.Join();
		}
	}

	public void ExecuteOnThread()
	{
		// Wait for task to be allocated
		while(abort == false)
		{
			// Check for start
			if(start == true)
			{
				// Setup state
				start = false;
				abort = false;

				// Begin executing
				LumaInterpreter.Execute(&stackContext);

				// Reset flags
				executing = false;

				// Abort return
				if(isThreadOwned == false)
					break;
			}
			else
			{
				// Hang the thread until needed
				System.Threading.Thread.Sleep(25);
			}
		}
	}
}