namespace System.Interop;

public unsafe class NativeLibrary
{
	private int handle;

	public this(string path)
	{
		handle = (int)NativeLibraryRuntime.luma_interop_library_load(path.Ptr);
	}

	public bool IsLoaded
	{
		get => handle != 0;
	}

	public void* GetProcAddress(string name)
	{
		if (handle == 0)
			return null;

		return NativeLibraryRuntime.luma_interop_library_get_proc_address((void*)handle, name.Ptr);
	}

	public bool Unload() mut
	{
		if (handle == 0)
			return true;

		bool unloaded = NativeLibraryRuntime.luma_interop_library_unload((void*)handle);
		if (unloaded)
			handle = 0;

		return unloaded;
	}

	~this()
	{
		Unload();
	}
}

internal static unsafe class NativeLibraryRuntime
{
	public static extern void* luma_interop_library_load(char* path);
	public static extern void* luma_interop_library_get_proc_address(void* handle, char* name);
	public static extern bool luma_interop_library_unload(void* handle);
}
