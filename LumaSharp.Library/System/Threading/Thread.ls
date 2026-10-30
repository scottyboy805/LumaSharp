using System;

namespace System.Threading;

public unsafe class Thread
{
	private ThreadStart? start;
	private int handle;
	private bool isCurrentThread;
	private bool hasStarted;

	public this()
	{
		start = null;
		handle = 0;
		isCurrentThread = false;
		hasStarted = false;
	}

	public this(ThreadStart start)
	{
		this.start = start;
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

	public bool IsCancellationRequested
	{
		get
		{
			if (isCurrentThread || handle == 0)
				return false;

			return __threadstoprequested(handle);
		}
	}

	public void Start() mut
	{
		if (isCurrentThread)
			throw new InvalidOperationException();

		if (hasStarted)
			throw new InvalidOperationException();

		ThreadStart? callback = start;
		if (callback == null)
			throw new InvalidOperationException();

		handle = __threadstart(callback!);
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

	public bool Join()
	{
		return Join(-1);
	}

	public bool Join(int32 millisecondsTimeout)
	{
		if (millisecondsTimeout < -1)
			throw new ArgumentOutOfRangeException();

		if (isCurrentThread)
			throw new InvalidOperationException();

		if (handle == 0)
			return true;

		return __threadjoin(handle, millisecondsTimeout);
	}

	public bool Stop(int32 millisecondsTimeout)
	{
		if (isCurrentThread)
			throw new InvalidOperationException();

		if (handle == 0)
			return true;

		__threadrequeststop(handle);
		return Join(millisecondsTimeout);
	}

	public void Close() mut
	{
		if (isCurrentThread || handle == 0)
			return;

		__threadclose(handle);
		handle = 0;
	}

	public static void Sleep(int32 millisecondsTimeout)
	{
		if (millisecondsTimeout < 0)
			throw new ArgumentOutOfRangeException();

		__threadsleep(millisecondsTimeout);
	}
}
