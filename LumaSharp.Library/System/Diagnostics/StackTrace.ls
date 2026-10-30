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

	public this(int32 skipFrames)
	{
		if (skipFrames < 0)
			throw new System.ArgumentOutOfRangeException();

		int32 intrinsicSkipFrames = skipFrames;
		if (intrinsicSkipFrames < int32.MaxValue)
			intrinsicSkipFrames++;

		stackTrace = __stacktrace(intrinsicSkipFrames);
		frameCount = __stackframes(intrinsicSkipFrames);
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
