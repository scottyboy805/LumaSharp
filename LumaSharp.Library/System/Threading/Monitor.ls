using System;
using System.Threading;

namespace System.Threading;

public static class Monitor
{
	public static void Enter(object syncRoot)
	{
		__monitorenter(syncRoot);
	}

	public static void Exit(object syncRoot)
	{
		if (!__monitorexit(syncRoot))
			throw new SynchronizationLockException();
	}
}
