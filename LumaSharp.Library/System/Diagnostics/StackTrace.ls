using System;

namespace System.Diagnostics;

public unsafe class StackTrace
{
	private string stackTrace;
	private int32 frameCount;

	public this()
	{
		// Runtime constructors are omitted from the live method frame chain,
		// so no caller frame needs to be skipped here.
		stackTrace = __stacktrace(0);
		frameCount = __stackframes(0);
	}

	public this(int32 skipFrames)
	{
		if (skipFrames < 0)
			throw new System.ArgumentOutOfRangeException();

		int32 intrinsicSkipFrames = skipFrames;

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
