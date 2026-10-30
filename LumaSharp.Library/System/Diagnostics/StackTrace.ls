using System;

namespace System.Diagnostics;

public unsafe class StackTrace
{
	private string stackTrace;
	private int32 frameCount;

	public this()
	{
		stackTrace = __stacktrace(1);
		frameCount = __stackframes(1);
	}

	public int32 FrameCount
	{
		get => frameCount;
	}

	public override string ToString()
	{
		return stackTrace;
	}

}
