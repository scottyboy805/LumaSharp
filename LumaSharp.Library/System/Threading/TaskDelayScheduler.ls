using System;
using System.Diagnostics;

namespace System.Threading;

internal class TaskDelayNode
{
	internal Task item;
	internal int64 dueTimestampTicks;
	internal TaskDelayNode? next;

	public this(Task item, int64 dueTicks)
	{
		this.item = item;
		dueTimestampTicks = dueTicks;
		next = null;
	}
}

internal static class TaskDelayScheduler
{
	private static readonly object synchronization = new object();
	private static readonly StopWatch clock = StopWatch.StartNew();
	private static TaskDelayNode? first;
	private static Thread? timerThread;
	private static bool isRunning;

	internal static Task Schedule(int64 millisecondsDelay)
	{
		Task task = new Task("");
		int64 dueTicks = clock.Elapsed.Ticks + millisecondsDelay * TimeSpan.TicksPerMillisecond;
		TaskDelayNode node = new TaskDelayNode(task, dueTicks);
		lock (synchronization)
		{
			node.next = first;
			first = node;

			if (!isRunning)
			{
				if (timerThread != null)
				{
					timerThread!.Join();
					timerThread = null;
				}

				Thread worker = new Thread();
				timerThread = worker;
				isRunning = true;
				try
				{
					ThreadStart callback = TimerLoop;
					worker.Start(callback);
				}
				catch (Exception)
				{
					first = node.next;
					timerThread = null;
					isRunning = false;
					throw;
				}
			}
		}
		return task;
	}

	private static void TimerLoop()
	{
		while (true)
		{
			Thread.Sleep(10);
			int64 currentTicks = clock.Elapsed.Ticks;
			lock (synchronization)
			{
				TaskDelayNode? previous = null;
				TaskDelayNode? current = first;
				while (current != null)
				{
					TaskDelayNode? nextNode = current!.next;
					if (current!.dueTimestampTicks <= currentTicks)
					{
						if (previous == null)
							first = nextNode;
						else
							previous!.next = nextNode;
						current!.item.Complete(null);
					}
					else
						previous = current;

					current = nextNode;
				}

				if (first == null)
				{
					isRunning = false;
					return;
				}
			}
		}
	}
}
