namespace System.IO;

public unsafe class File
{
	public static FileStream Open(string path, FileMode mode)
	{
		return new FileStream(path, mode);
	}

	public static FileStream OpenRead(string path)
	{
		return Open(path, FileMode.Open);
	}

	public static FileStream Create(string path)
	{
		return Open(path, FileMode.Create);
	}

	public static FileStream OpenAppend(string path)
	{
		return Open(path, FileMode.Append);
	}

	public static FileStream OpenOrCreate(string path)
	{
		return Open(path, FileMode.OpenOrCreate);
	}

	public static uint8[] ReadAllBytes(string path)
	{
		FileStream stream = OpenRead(path);
		int32 length = stream.Length;
		uint8[] result = new uint8[length];
		int32 offset = 0;
		while (offset < length)
		{
			int32 read = stream.Read(result, offset, length - offset);
			if (read == 0)
				throw new EndOfStreamException();
			offset += read;
		}
		stream.Close();
		return result;
	}

	public static void WriteAllBytes(string path, uint8[] contents)
	{
		FileStream stream = Create(path);
		stream.Write(contents, 0, contents.Length);
		stream.Close();
	}

	public static bool Exists(string path)
	{
		return NativeFileSystem.luma_fs_file_exists(path.Utf8Ptr);
	}

	public static void Delete(string path)
	{
		if (NativeFileSystem.luma_fs_file_exists(path.Utf8Ptr))
			if (NativeFileSystem.luma_fs_delete_file(path.Utf8Ptr) != 0)
				throw new IOException("Unable to delete the file.");
	}

	public static void Copy(string source, string destination)
	{
		Copy(source, destination, false);
	}

	public static void Copy(string source, string destination, bool overwrite)
	{
		if (NativeFileSystem.luma_fs_copy(source.Utf8Ptr, destination.Utf8Ptr, overwrite) != 0)
			throw new IOException("Unable to copy the file.");
	}

	public static void Move(string source, string destination)
	{
		Move(source, destination, false);
	}

	public static void Move(string source, string destination, bool overwrite)
	{
		if (NativeFileSystem.luma_fs_move(source.Utf8Ptr, destination.Utf8Ptr, overwrite) != 0)
			throw new IOException("Unable to move the file.");
	}
}
