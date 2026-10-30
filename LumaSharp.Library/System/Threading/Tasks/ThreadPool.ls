using System;
using System.Threading;

namespace System.Threading.Tasks;

internal class ThreadPoolWorkNode
{
	public Task Item;
	public ThreadPoolWorkNode? Next;

	public this(Task item)
	{
		Item = item;
		Next = null;
	}
}

internal class ThreadPoolWorker
{
	public Thread Thread;
	public ThreadPoolWorker? Next;

	public this()
	{
		Thread = new Thread();
		Next = null;
	}
}

public static class ThreadPool
{
	private static TaskGate synchronization = new TaskGate();
	private static ThreadPoolWorkNode? first;
	private static ThreadPoolWorkNode? last;
	private static ThreadPoolWorker? workers;
	private static int32 activeWorkers = 0;
	private static int32 idleWorkers = 0;
	private static int32 blockingWaiters = 0;
	private const int32 maximumWorkers = 64;
	private const int32 idleTimeoutMilliseconds = 30000;

	internal static void QueueTask(Task workItem)
	{
		if (workItem == null)
			throw new ArgumentNullException();

		lock (synchronization)
		{
			ThreadPoolWorkNode node = new ThreadPoolWorkNode(workItem);
			ThreadPoolWorkNode? previousLast = last;
			if (last == null)
				first = node;
			else
				last!.Next = node;
			last = node;

			hidden_RemoveCompletedWorkers();
			if (idleWorkers == 0 && first != null && activeWorkers < maximumWorkers + blockingWaiters)
			{
				try
				{
					hidden_StartWorker();
				}
				catch (Exception)
				{
					if (previousLast == null)
						first = null;
				else
						previousLast!.Next = null;
					last = previousLast;
					throw;
				}
			}
		}
	}

	internal static void hidden_BeginBlockingWait()
	{
		lock (synchronization)
		{
			blockingWaiters++;
			while (first != null && idleWorkers == 0 && activeWorkers < maximumWorkers + blockingWaiters)
				hidden_StartWorker();
		}
	}

	internal static void hidden_EndBlockingWait()
	{
		lock (synchronization)
		{
			if (blockingWaiters > 0)
				blockingWaiters--;
		}
	}

	private static void hidden_StartWorker()
	{
		ThreadPoolWorker worker = new ThreadPoolWorker();
		worker.Next = workers;
		workers = worker;
		ThreadStart callback = hidden_WorkerLoop;
		activeWorkers++;
		try
		{
			worker.Thread.Start(callback);
		}
		catch (Exception)
		{
			activeWorkers--;
			workers = worker.Next;
			throw;
		}
	}

	private static void hidden_WorkerLoop()
	{
		bool isIdle = false;
		int32 idleTime = 0;
		while (true)
		{
			ThreadPoolWorkNode? node = null;
			bool shouldExit = false;
			lock (synchronization)
			{
				if (first != null)
				{
					node = first;
					first = first!.Next;
					if (first == null)
						last = null;
					node!.Next = null;
					if (isIdle)
					{
						idleWorkers--;
						isIdle = false;
					}
				}
				else if (idleTime >= idleTimeoutMilliseconds)
				{
					if (isIdle)
						idleWorkers--;
					activeWorkers--;
					shouldExit = true;
				}
				else if (!isIdle)
				{
					idleWorkers++;
					isIdle = true;
				}
			}

			if (shouldExit)
				return;

			if (node != null)
			{
				node!.Item.hidden_Execute();
				idleTime = 0;
			}
			else
			{
				Thread.Sleep(10);
				idleTime += 10;
			}
		}
	}

	private static void hidden_RemoveCompletedWorkers()
	{
		ThreadPoolWorker? previous = null;
		ThreadPoolWorker? current = workers;
		while (current != null)
		{
			ThreadPoolWorker? next = current!.Next;
			if (!current.Thread.IsAlive)
			{
				if (previous == null)
					workers = next;
				else
					previous!.Next = next;
			}
			else
				previous = current;
			current = next;
		}
	}
}
