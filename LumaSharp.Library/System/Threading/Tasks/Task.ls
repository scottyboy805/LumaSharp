using System;
using System.Diagnostics;
using System.Threading;

namespace System.Threading.Tasks;

internal class TaskGate
{
}

public virtual class Task
{
	private TaskGate synchronization;
	private Action? action;
	private Exception? exception;
	protected string parentStackTrace;
	private bool isCompleted;

	internal this(string parentTrace)
	{
		synchronization = new TaskGate();
		action = null;
		exception = null;
		parentStackTrace = parentTrace;
		isCompleted = false;
	}

	internal this(Action callback, string parentTrace) : this(parentTrace)
	{
		if (callback == null)
			throw new ArgumentNullException();
		action = callback;
	}

	internal void SetAction(Action callback) mut
	{
		if (callback == null)
			throw new ArgumentNullException();
		action = callback;
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

	public virtual void hidden_Execute() mut
	{
		Exception? failure = null;
		try
		{
			if (action != null)
				action!();
		}
		catch (Exception error)
		{
			error.hidden_AppendAsyncStackTrace(parentStackTrace);
			failure = error;
		}

		action = null;
		hidden_Complete(failure);
	}

	protected void hidden_Complete(Exception? error) mut
	{
		lock (synchronization)
		{
			exception = error;
			isCompleted = true;
		}
	}

	public void hidden_Wait()
	{
		if (IsCompleted)
			return;

		ThreadPool.hidden_BeginBlockingWait();
		try
		{
			while (!IsCompleted)
				Thread.Sleep(1);
		}
		finally
		{
			ThreadPool.hidden_EndBlockingWait();
		}
	}

	public void Wait()
	{
		hidden_Wait();
	}

	internal static void hidden_GetResult(Task task)
	{
		if (task == null)
			throw new ArgumentNullException();

		task.hidden_Wait();
		Exception? error = task.Exception;
		if (error != null)
			__throwpreserve(error!);
	}

	internal static T hidden_GetResult<T>(Task<T> task)
	{
		if (task == null)
			throw new ArgumentNullException();

		return task.hidden_GetTaskResult();
	}

	public static Task Run(Action callback)
	{
		if (callback == null)
			throw new ArgumentNullException();

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

	internal T hidden_GetTaskResult()
	{
		hidden_Wait();
		Exception? error = Exception;
		if (error != null)
			__throwpreserve(error!);
		return result;
	}

	public T Result
	{
		get => hidden_GetTaskResult();
	}

}
