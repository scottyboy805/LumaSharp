using System;
using System.Diagnostics;

namespace System.Threading;

public delegate void TaskCallback();

public virtual class Task
{
	private const int64 maximumTimeSpanDelayMilliseconds = 4294967294;
	private readonly object synchronization = new object();
	private TaskCallback? callback;
	private Exception? exception;
	private OperationCanceledException? cancellationException;
	private string parentStackTrace;
	private bool isCompleted;
	private bool isRunning;
	private bool isCancellationRequested;
	private bool isCanceled;

	internal this(string parentTrace)
	{
		callback = null;
		exception = null;
		cancellationException = null;
		parentStackTrace = parentTrace;
		isCompleted = false;
		isRunning = false;
		isCancellationRequested = false;
		isCanceled = false;
	}

	internal this(TaskCallback callback, string parentTrace) : this(parentTrace)
	{
		this.callback = callback;
	}

	internal void SetCallback(TaskCallback callback) mut
	{
		this.callback = callback;
	}

	public bool IsCompleted
	{
		get
		{
			lock (synchronization)
				return isCompleted;
		}
	}

	public bool IsCancellationRequested
	{
		get
		{
			lock (synchronization)
				return isCancellationRequested;
		}
	}

	public bool IsCanceled
	{
		get
		{
			lock (synchronization)
				return isCanceled;
		}
	}

	public Exception? Exception
	{
		get
		{
			lock (synchronization)
				return exception;
		}
	}

	internal OperationCanceledException? CancellationException
	{
		get
		{
			lock (synchronization)
				return cancellationException;
		}
	}

	public void RequestCancellation() mut
	{
		lock (synchronization)
		{
			if (isCompleted)
				return;

			isCancellationRequested = true;
			if (!isRunning)
			{
				cancellationException = new OperationCanceledException();
				cancellationException!.AppendAsyncStackTrace(parentStackTrace);
				isCanceled = true;
				isCompleted = true;
				callback = null;
			}
		}
	}

	public void ThrowIfCancellationRequested()
	{
		if (IsCancellationRequested)
			throw new OperationCanceledException();
	}

	internal virtual void Execute() mut
	{
		TaskCallback? work;
		lock (synchronization)
		{
			if (isCompleted)
				return;

			if (isCancellationRequested)
			{
				cancellationException = new OperationCanceledException();
				cancellationException!.AppendAsyncStackTrace(parentStackTrace);
				isCanceled = true;
				isCompleted = true;
				callback = null;
				return;
			}

			isRunning = true;
			work = callback;
		}

		Exception? failure = null;
		OperationCanceledException? cancellation = null;
		try
		{
			if (work != null)
				work!();
		}
		catch (OperationCanceledException error)
		{
			error.AppendAsyncStackTrace(parentStackTrace);
			cancellation = error;
		}
		catch (Exception error)
		{
			error.AppendAsyncStackTrace(parentStackTrace);
			failure = error;
		}

		Complete(failure, cancellation);
	}

	internal void Complete(Exception? error) mut
	{
		Complete(error, null);
	}

	internal void Complete(Exception? error, OperationCanceledException? cancellation) mut
	{
		lock (synchronization)
		{
			if (isCompleted)
				return;

			exception = error;
			cancellationException = cancellation;
			isCanceled = cancellation != null;
			if (isCanceled)
				isCancellationRequested = true;
			callback = null;
			isRunning = false;
			isCompleted = true;
		}
	}

	internal void WaitForCompletion()
	{
		WaitForCompletion(null);
	}

	internal void WaitForCompletion(Task? cancellationContext)
	{
		if (IsCompleted)
			return;

		ThreadPool.BeginBlockingWait();
		try
		{
			while (!IsCompleted)
			{
				cancellationContext?.ThrowIfCancellationRequested();
				Thread.Sleep(1);
			}
		}
		finally
		{
			ThreadPool.EndBlockingWait();
		}
	}

	public void Wait()
	{
		WaitForCompletion();
	}

	public static Task CompletedTask
	{
		get => CreateCompletedTask();
	}

	public static Task Delay(int32 millisecondsDelay)
	{
		if (millisecondsDelay < -1)
			throw new ArgumentOutOfRangeException();

		if (millisecondsDelay == 0)
			return CompletedTask;
		if (millisecondsDelay == -1)
			return CreateInfiniteDelay();

		return TaskDelayScheduler.Schedule((int64)millisecondsDelay);
	}

	public static Task Delay(TimeSpan delay)
	{
		int64 ticks = delay.Ticks;
		if (ticks < 0 && ticks != -TimeSpan.TicksPerMillisecond)
			throw new ArgumentOutOfRangeException();

		if (ticks > maximumTimeSpanDelayMilliseconds * TimeSpan.TicksPerMillisecond)
			throw new ArgumentOutOfRangeException();

		int64 millisecondsDelay = ticks / TimeSpan.TicksPerMillisecond;
		if (millisecondsDelay == 0)
			return CompletedTask;
		if (millisecondsDelay == -1)
			return CreateInfiniteDelay();

		return TaskDelayScheduler.Schedule(millisecondsDelay);
	}

	private static Task CreateCompletedTask()
	{
		Task task = new Task("");
		task.Complete(null);
		return task;
	}

	private static Task CreateInfiniteDelay()
	{
		return new Task(new StackTrace(1).ToString());
	}

	internal static void GetAwaitedResult(Task task, string awaitFrame, Task cancellationContext)
	{
		task.WaitForCompletion(cancellationContext);
		if (task.IsCanceled)
		{
			OperationCanceledException cancellationError = task.CancellationException ?? new OperationCanceledException();
			cancellationError.AppendAsyncStackTrace(awaitFrame);
			__throwpreserve(cancellationError);
		}
		Exception? error = task.Exception;
		if (error != null)
		{
			error!.AppendAsyncStackTrace(awaitFrame);
			__throwpreserve(error!);
		}
	}

	internal static T GetAwaitedResult<T>(Task<T> task, string awaitFrame, Task cancellationContext)
	{
		return task.GetTaskResult(awaitFrame, cancellationContext);
	}

	public static Task Run(TaskCallback callback)
	{
		Task task = new Task(callback, new StackTrace(1).ToString());
		ThreadPool.QueueTask(task);
		return task;
	}

}

public class Task<T> : Task
{
	private T result;

	internal this(string parentTrace) : base(parentTrace)
	{
	}

	internal void SetResult(T value) mut
	{
		result = value;
	}

	internal T GetTaskResult()
	{
		return GetTaskResult("", null);
	}

	internal T GetTaskResult(string awaitFrame)
	{
		return GetTaskResult(awaitFrame, null);
	}

	internal T GetTaskResult(string awaitFrame, Task? cancellationContext)
	{
		WaitForCompletion(cancellationContext);
		if (IsCanceled)
		{
			OperationCanceledException cancelled = CancellationException ?? new OperationCanceledException();
			cancelled.AppendAsyncStackTrace(awaitFrame);
			__throwpreserve(cancelled);
		}
		Exception? error = Exception;
		if (error != null)
		{
			error!.AppendAsyncStackTrace(awaitFrame);
			__throwpreserve(error!);
		}
		return result;
	}

	public T Result
	{
		get => GetTaskResult();
	}

}
