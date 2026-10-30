using System;

namespace System.Diagnostics;

public unsafe class StackTrace
{
	private string stackTrace;
	private int32 frameCount;

	public this()
	{
		stackTrace = CaptureRuntime(1);
		frameCount = FrameCountRuntime(1);
	}

	public int32 FrameCount
	{
		get => frameCount;
	}

	public override string ToString()
	{
		return stackTrace;
	}

	private static extern string CaptureRuntime(int32 framesToSkip);
	private static extern int32 FrameCountRuntime(int32 framesToSkip);
}
