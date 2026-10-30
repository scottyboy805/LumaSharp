namespace System.Interop;

public unsafe class NativeLibrary
{
	private int handle;

	public this(string path)
	{
		handle = (int)__libopen(path);
	}

	public bool IsLoaded
	{
		get => handle != 0;
	}

	public void* GetProcAddress(string name)
	{
		if (handle == 0)
			return null;

		return __libsym((void*)handle, name);
	}

	public bool Unload() mut
	{
		if (handle == 0)
			return true;

		bool unloaded = __libclose((void*)handle);
		if (unloaded)
			handle = 0;

		return unloaded;
	}

	~this()
	{
		Unload();
	}
}
