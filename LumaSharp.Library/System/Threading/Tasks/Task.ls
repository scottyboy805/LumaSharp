using System;
using System.Diagnostics;
using System.Threading;

namespace System.Threading.Tasks;

public delegate void TaskCallback();

public virtual class Task
{
	private readonly object synchronization = new object();
	private TaskCallback? callback;
	private Exception? exception;
	private string parentStackTrace;
	private bool isCompleted;

	internal this(string parentTrace)
	{
		callback = null;
		exception = null;
		parentStackTrace = parentTrace;
		isCompleted = false;
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

	public Exception? Exception
	{
		get
		{
			lock (synchronization)
				return exception;
		}
	}

	internal virtual void Execute() mut
	{
		Exception? failure = null;
		try
		{
			if (callback != null)
				callback!();
		}
		catch (Exception error)
		{
			error.AppendAsyncStackTrace(parentStackTrace);
			failure = error;
		}

		callback = null;
		Complete(failure);
	}

	private void Complete(Exception? error) mut
	{
		lock (synchronization)
		{
			exception = error;
			isCompleted = true;
		}
	}

	internal void WaitForCompletion()
	{
		if (IsCompleted)
			return;

		ThreadPool.BeginBlockingWait();
		try
		{
			while (!IsCompleted)
				Thread.Sleep(1);
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

	internal static void GetAwaitedResult(Task task, string awaitFrame)
	{
		task.WaitForCompletion();
		Exception? error = task.Exception;
		if (error != null)
		{
			error!.AppendAsyncStackTrace(awaitFrame);
			__throwpreserve(error!);
		}
	}

	internal static T GetAwaitedResult<T>(Task<T> task, string awaitFrame)
	{
		return task.GetTaskResult(awaitFrame);
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
		return GetTaskResult("");
	}

	internal T GetTaskResult(string awaitFrame)
	{
		WaitForCompletion();
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
