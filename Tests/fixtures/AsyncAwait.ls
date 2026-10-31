using System;
using System.Threading;

class CounterGate
{
}

public interface IAsyncValue
{
	async int32 GetValue();
}

public class AsyncValue : IAsyncValue
{
	public async int32 GetValue()
	{
		return 17;
	}
}

class Program
{
	private static CounterGate gate = new CounterGate();
	private static CounterGate signalGate = new CounterGate();
	private static int32 value = 0;
	private static bool enteredCriticalRegion = false;
	private static int32 gateEvaluations = 0;

	private static CounterGate GetGate()
	{
		gateEvaluations++;
		return gate;
	}

	private static void CriticalWorker()
	{
		lock (gate)
		{
			lock (signalGate)
				enteredCriticalRegion = true;
			Thread.Sleep(50);
		}

		while (true)
			Thread.Sleep(1);
	}

	public static async int32 IncrementAsync()
	{
		int32 next;
		lock (gate)
		{
			lock (gate)
			{
				int32 observed = value;
				Thread.Sleep(2);
				next = observed + 1;
				value = next;
			}
		}
		return next;
	}

	public static async T IdentityAsync<T>(T input)
	{
		return input;
	}

	public static async int32 CountdownAsync(int32 depth)
	{
		if (depth == 0)
			return 1;
		return 1 + await CountdownAsync(depth - 1);
	}

	public static async int32 SumAsync()
	{
		Thread caller = Thread.CurrentThread;
		int32 first = await IncrementAsync();
		if (Thread.CurrentThread != caller)
			return -1;
		int32 second = await IncrementAsync();
		if (Thread.CurrentThread != caller)
			return -2;
		return first + second;
	}

	public static async void AddAsync(int32 amount)
	{
		lock (gate)
			value += amount;
	}

	public static async int32 AddAndReadAsync()
	{
		await AddAsync(2);
		return value;
	}

	public static async int32 FaultAsync()
	{
		throw new InvalidOperationException("async failure");
	}

	public static async int32 FaultThroughAsync()
	{
		return await FaultAsync();
	}

	public static async int32 FaultTwoLevelsAsync()
	{
		return await FaultThroughAsync();
	}

	public static async int32 CatchAwaitFaultAsync()
	{
		Thread observerThread = Thread.CurrentThread;
		try
		{
			return await FaultTwoLevelsAsync();
		}
		catch (InvalidOperationException error)
		{
			if (Thread.CurrentThread != observerThread)
				return -2;
			string? caughtTrace = error.StackTrace;
			string expectedTrace = "at Program.FaultAsync()\n\n--- End of stack trace from previous location ---\nat Program.FaultThroughAsync()\n\n--- End of stack trace from previous location ---\nat Program.FaultTwoLevelsAsync()\n\n--- End of stack trace from previous location ---\nat Program.CatchAwaitFaultAsync()\n";
			if (error.Message != "async failure" || caughtTrace != expectedTrace)
				return -1;
			return 23;
		}
	}

	public static async void FaultVoidAsync()
	{
		throw new InvalidOperationException("void async failure");
	}

	public static async void FaultVoidThroughAsync()
	{
		await FaultVoidAsync();
	}

	public static async int32 CatchAwaitVoidFaultAsync()
	{
		Thread observerThread = Thread.CurrentThread;
		try
		{
			await FaultVoidThroughAsync();
			return -1;
		}
		catch (InvalidOperationException error)
		{
			if (Thread.CurrentThread != observerThread)
				return -3;
			string? caughtTrace = error.StackTrace;
			string expectedTrace = "at Program.FaultVoidAsync()\n\n--- End of stack trace from previous location ---\nat Program.FaultVoidThroughAsync()\n\n--- End of stack trace from previous location ---\nat Program.CatchAwaitVoidFaultAsync()\n";
			if (error.Message != "void async failure" || caughtTrace != expectedTrace)
				return -2;
			return 24;
		}
	}

	public static int32 Main()
	{
		Task<int32> first = IncrementAsync();
		Task<int32> second = IncrementAsync();
		int32 firstResult = first.Result;
		int32 secondResult = second.Result;
		int32 sumResult = SumAsync().Result;
		Task<int32> identity = IdentityAsync<int32>(42);
		if (firstResult + secondResult != 3 || sumResult != 7 || value != 4 || identity.Result != 42 || CountdownAsync(66).Result != 67 || !first.IsCompleted)
			return 1;
		IAsyncValue provider = new AsyncValue();
		if (provider.GetValue().Result != 17)
			return 10;
		if (AddAndReadAsync().Result != 6 || value != 6)
			return 9;

		try
		{
			int32 ignored = FaultAsync().Result;
			return 2;
		}
		catch (Exception error)
		{
			if (error.Message != "async failure" || error.StackTrace == null)
				return 3;
			string trace = error.StackTrace!;
			if (trace != "at Program.FaultAsync()\nat Program.Main()\n")
				return 4;
		}

		try
		{
			int32 ignored = FaultThroughAsync().Result;
			return 5;
		}
		catch (Exception error)
		{
			string trace = error.StackTrace!;
			string expectedTrace = "at Program.FaultAsync()\n\n--- End of stack trace from previous location ---\nat Program.FaultThroughAsync()\nat Program.Main()\n";
			if (trace != expectedTrace)
				return 6;
		}

		try
		{
			int32 ignored = FaultTwoLevelsAsync().Result;
			return 14;
		}
		catch (Exception error)
		{
			string expectedTrace = "at Program.FaultAsync()\n\n--- End of stack trace from previous location ---\nat Program.FaultThroughAsync()\n\n--- End of stack trace from previous location ---\nat Program.FaultTwoLevelsAsync()\nat Program.Main()\n";
			if (error.StackTrace != expectedTrace)
				return 15;
		}

		int32 caughtResult = CatchAwaitFaultAsync().Result;
		if (caughtResult != 23)
			return 16;
		if (CatchAwaitVoidFaultAsync().Result != 24)
			return 17;
		Task faulted = FaultAsync();
		faulted.Wait();
		if (faulted.Exception == null)
			return 13;

		Thread aborted = new Thread();
		ThreadStart criticalCallback = CriticalWorker;
		aborted.Start(criticalCallback);
		bool isEntered = false;
		while (!isEntered)
		{
			lock (signalGate)
				isEntered = enteredCriticalRegion;
			if (isEntered)
				break;
			Thread.Sleep(1);
		}
		aborted.Abort();
		if (aborted.IsAlive)
			return 7;
		lock (gate)
			value++;
		if (value != 7)
			return 8;

		lock (GetGate())
		{
		}
		if (gateEvaluations != 1)
			return 11;

		try
		{
			lock (gate)
				throw new InvalidOperationException("release monitor on exception");
		}
		catch (InvalidOperationException)
		{
			lock (gate)
				value++;
		}
		if (value != 8)
			return 12;

		CounterGate?[] manyGates = new CounterGate?[5000];
		for (int32 i = 0; i < manyGates.Length; i++)
			manyGates[i] = new CounterGate();
		for (int32 i = 0; i < manyGates.Length; i++)
			Monitor.Enter(manyGates[i]!);
		for (int32 i = manyGates.Length - 1; i >= 0; i--)
			Monitor.Exit(manyGates[i]!);

		return 0;
	}
}
