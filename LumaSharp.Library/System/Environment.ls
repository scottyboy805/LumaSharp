namespace System;

public static unsafe class Environment
{
	public static string ExecutablePath
	{
		get
		{
			return luma_env_executable_path();
		}
	}

	public static string ExecutableDirectory
	{
		get
		{
			return luma_env_executable_directory();
		}
	}

	public static string ExecutableName
	{
		get
		{
			return luma_env_executable_name();
		}
	}

	public static int32 ProcessorCount
	{
		get
		{
			int32 count = luma_env_processor_count();
			return count > 0 ? count : 1;
		}
	}

	public static bool IsLittleEndian
	{
		get
		{
			return luma_env_is_little_endian();
		}
	}

	public static Platform Platform
	{
		get
		{
			return (Platform)luma_env_platform();
		}
	}

	public static Architecture Architecture
	{
		get
		{
			return (Architecture)luma_env_architecture();
		}
	}

	private static extern string luma_env_executable_path();
	private static extern string luma_env_executable_directory();
	private static extern string luma_env_executable_name();
	private static extern int32 luma_env_processor_count();
	private static extern bool luma_env_is_little_endian();
	private static extern int32 luma_env_platform();
	private static extern int32 luma_env_architecture();
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
