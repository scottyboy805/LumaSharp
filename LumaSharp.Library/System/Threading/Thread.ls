using System;

namespace System.Threading;

public delegate void ThreadStart();

public delegate void ThreadStart<T>(T argument);

public unsafe class Thread
{
	private int handle;
	private bool isCurrentThread;
	private bool hasStarted;

	public this()
	{
		handle = 0;
		isCurrentThread = false;
		hasStarted = false;
	}

	public static Thread CurrentThread
	{
		get
		{
			return __threadcurrent();
		}
	}

	public bool IsCurrentThread
	{
		get => isCurrentThread;
	}

	public bool IsAlive
	{
		get
		{
			if (isCurrentThread)
				return true;

			if (handle == 0)
				return false;

			return !__threadjoin(handle, 0);
		}
	}

	~this()
	{
		if (handle != 0)
			__threadclose(handle);
	}

	public void Start(ThreadStart callback) mut
	{
		if (isCurrentThread)
			throw new InvalidOperationException();

		if (hasStarted)
			throw new InvalidOperationException();

		if (callback == null)
			throw new InvalidOperationException();

		handle = __threadstart(callback);
		if (handle == 0)
			throw new InvalidOperationException("The operating system could not start the thread.");
		hasStarted = true;
	}

	public void Start<T>(ThreadStart<T> callback, T argument) mut
	{
		if (isCurrentThread)
			throw new InvalidOperationException();

		if (hasStarted)
			throw new InvalidOperationException();

		object? boxedArgument = argument;
		handle = __threadstartarg(callback, boxedArgument);
		if (handle == 0)
			throw new InvalidOperationException("The operating system could not start the thread.");
		hasStarted = true;
	}

	public bool Join() mut
	{
		return Join(-1);
	}

	public bool Join(int32 millisecondsTimeout) mut
	{
		if (millisecondsTimeout < -1)
			throw new ArgumentOutOfRangeException();

		if (isCurrentThread)
			throw new InvalidOperationException();

		if (CurrentThread == this)
			throw new InvalidOperationException();

		if (handle == 0)
			return true;

		bool completed = __threadjoin(handle, millisecondsTimeout);
		if (completed)
		{
			__threadclose(handle);
			handle = 0;
		}

		return completed;
	}

	public void Abort() mut
	{
		if (isCurrentThread)
			throw new InvalidOperationException();

		if (CurrentThread == this)
			throw new InvalidOperationException();

		if (handle == 0)
			return;

		__threadabort(handle);
		handle = 0;
	}

	public static void Sleep(int32 millisecondsTimeout)
	{
		if (millisecondsTimeout < 0)
			throw new ArgumentOutOfRangeException();

		__threadsleep(millisecondsTimeout);
	}
}
