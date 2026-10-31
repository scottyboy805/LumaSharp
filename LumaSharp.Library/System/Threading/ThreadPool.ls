using System;
namespace System.Threading;

internal class ThreadPoolWorkNode
{
	internal Task item;
	internal ThreadPoolWorkNode? next;

	public this(Task item)
	{
		this.item = item;
		next = null;
	}
}

internal class ThreadPoolWorker
{
	internal Thread thread;
	internal ThreadPoolWorker? next;

	public this()
	{
		thread = new Thread();
		next = null;
	}
}

internal static class ThreadPool
{
	private static readonly object synchronization = new object();
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
		lock (synchronization)
		{
			ThreadPoolWorkNode node = new ThreadPoolWorkNode(workItem);
			ThreadPoolWorkNode? previousLast = last;
			if (last == null)
				first = node;
			else
				last!.next = node;
			last = node;

			RemoveCompletedWorkers();
			if (idleWorkers == 0 && first != null && activeWorkers < maximumWorkers + blockingWaiters)
			{
				try
				{
					StartWorker();
				}
				catch (Exception)
				{
					if (previousLast == null)
						first = null;
					else
						previousLast!.next = null;
					last = previousLast;
					throw;
				}
			}
		}
	}

	internal static void BeginBlockingWait()
	{
		lock (synchronization)
		{
			blockingWaiters++;
			while (first != null && idleWorkers == 0 && activeWorkers < maximumWorkers + blockingWaiters)
				StartWorker();
		}
	}

	internal static void EndBlockingWait()
	{
		lock (synchronization)
		{
			if (blockingWaiters > 0)
				blockingWaiters--;
		}
	}

	private static void StartWorker()
	{
		ThreadPoolWorker worker = new ThreadPoolWorker();
		worker.next = workers;
		workers = worker;
		ThreadStart callback = WorkerLoop;
		activeWorkers++;
		try
		{
			worker.thread.Start(callback);
		}
		catch (Exception)
		{
			activeWorkers--;
			workers = worker.next;
			throw;
		}
	}

	private static void WorkerLoop()
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
					first = first!.next;
					if (first == null)
						last = null;
					node!.next = null;
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
				node!.item.Execute();
				idleTime = 0;
			}
			else
			{
				Thread.Sleep(10);
				idleTime += 10;
			}
		}
	}

	private static void RemoveCompletedWorkers()
	{
		ThreadPoolWorker? previous = null;
		ThreadPoolWorker? current = workers;
		while (current != null)
		{
			ThreadPoolWorker? next = current!.next;
			if (!current.thread.IsAlive)
			{
				if (previous == null)
					workers = next;
				else
					previous!.next = next;
			}
			else
				previous = current;
			current = next;
		}
	}
}
