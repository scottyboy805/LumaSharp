using System.Threading;

class AsyncAwaitGate
{
}

class Program
{
	private static AsyncAwaitGate gate = new AsyncAwaitGate();
	private static int32 arrivals = 0;

	private static async int32 ArriveTogetherAsync(int32 value)
	{
		lock (gate)
			arrivals++;

		int32 attempts = 0;
		bool ready = false;
		while (!ready && attempts < 2000)
		{
			lock (gate)
				ready = arrivals >= 2;
			if (!ready)
				Thread.Sleep(1);
			attempts++;
		}
		return ready ? value : 0;
	}

	public static async int32 RunAsync()
	{
		let first, second = await (ArriveTogetherAsync(11), ArriveTogetherAsync(22));
		if (first != 11 || second != 22)
			return 1;

		lock (gate)
			arrivals = 0;

		int32 third, fourth;
		third, fourth = await (ArriveTogetherAsync(33), ArriveTogetherAsync(44));
		return third == 33 && fourth == 44 ? 0 : 2;
	}

	private static async int32 ImmediateAsync(int32 value)
	{
		return value;
	}

	private static async int32 SequentialAwaitsAsync()
	{
		let first, second = await ImmediateAsync(51), await ImmediateAsync(52);
		return first == 51 && second == 52 ? 0 : 3;
	}

	public static int32 Main()
	{
		int32 batchResult = RunAsync().Result;
		return batchResult != 0 ? batchResult : SequentialAwaitsAsync().Result;
	}
}
