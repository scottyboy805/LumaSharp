using System;
using System.Threading;

namespace System.Threading;

public static class Monitor
{
	public static void Enter(object syncRoot)
	{
		if (syncRoot == null)
			throw new ArgumentNullException();

		__monitorenter(syncRoot);
	}

	public static void Exit(object syncRoot)
	{
		if (syncRoot == null)
			throw new ArgumentNullException();

		if (!__monitorexit(syncRoot))
			throw new SynchronizationLockException();
	}
}
