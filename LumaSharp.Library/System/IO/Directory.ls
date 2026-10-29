namespace System.IO;

public unsafe class Directory
{
	public static string GetCurrentDirectory()
	{
		return NativeFileSystem.luma_fs_current_directory();
	}

	public static string GetTempPath()
	{
		string path = NativeFileSystem.luma_fs_temporary_directory();
		if (path.Length == 0)
			return path;
		char last = path[path.Length - 1];
		if (last == '/' || last == '\\')
			return path;
		return path + "/";
	}

	public static bool Exists(string path)
	{
		return NativeFileSystem.luma_fs_directory_exists(path.Ptr);
	}

	public static void CreateDirectory(string path)
	{
		if (Exists(path))
			return;

		if (NativeFileSystem.luma_fs_create_directory(path.Ptr) != 0)
			throw new IOException("Unable to create the directory.");
	}

	public static void Delete(string path)
	{
		Delete(path, false);
	}

	public static void Delete(string path, bool recursive)
	{
		if (!Exists(path))
			throw new IOException("The directory does not exist.");

		if (recursive)
			throw new System.NotSupportedException("Recursive directory deletion is not implemented yet.");

		if (NativeFileSystem.luma_fs_delete_directory(path.Ptr) != 0)
			throw new IOException("Unable to delete the directory.");
	}

	public static void Move(string source, string destination)
	{
		if (NativeFileSystem.luma_fs_move(source.Ptr, destination.Ptr, false) != 0)
			throw new IOException("Unable to move the directory.");
	}
}
