namespace System;

public static unsafe class Environment
{
	public static string ExecutablePath
	{
		get
		{
			return __execpath();
		}
	}

	public static string ExecutableDirectory
	{
		get
		{
			return __execdir();
		}
	}

	public static string ExecutableName
	{
		get
		{
			return __execname();
		}
	}

	public static int32 ProcessorCount
	{
		get
		{
			int32 count = __processors();
			return count > 0 ? count : 1;
		}
	}

	public static bool IsLittleEndian
	{
		get
		{
			return __endianness();
		}
	}

	public static Platform Platform
	{
		get
		{
			return (Platform)__platform();
		}
	}

	public static Architecture Architecture
	{
		get
		{
			return (Architecture)__architecture();
		}
	}

}

public enum Platform : int32
{
	Unknown = 0,
	Windows = 1,
	Linux = 2,
	MacOS = 3
}

public enum Architecture : int32
{
	Unknown = 0,
	X86 = 1,
	X64 = 2,
	Arm = 3,
	Arm64 = 4
}
