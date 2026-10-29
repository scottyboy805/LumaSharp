namespace System.IO;

public class Path
{
	public static string Combine(string first, string second)
	{
		if (first.Length == 0)
			return second;
		if (second.Length == 0)
			return first;
		if (IsPathRooted(second))
			return second;

		char last = first[first.Length - 1];
		if (last == '/' || last == '\\')
			return first + second;
		return first + "/" + second;
	}

	public static string GetFullPath(string path)
	{
		if (path.Length == 0)
			return Directory.GetCurrentDirectory();
		if (IsPathRooted(path))
			return path;
		return Combine(Directory.GetCurrentDirectory(), path);
	}

	public static string GetTempPath()
	{
		return Directory.GetTempPath();
	}

	public static string GetFileName(string path)
	{
		int32 start = 0;
		for (int32 index = 0; index < path.Length; index++)
			if (path[index] == '/' || path[index] == '\\')
				start = index + 1;

		return path.Substring(start);
	}

	public static string GetExtension(string path)
	{
		string fileName = GetFileName(path);
		int32 extension = -1;
		for (int32 index = 0; index < fileName.Length; index++)
			if (fileName[index] == '.')
				extension = index;

		if (extension <= 0)
			return System.String.Empty;
		return fileName.Substring(extension);
	}

	public static string GetFileNameWithoutExtension(string path)
	{
		string fileName = GetFileName(path);
		string extension = GetExtension(path);
		if (extension.Length == 0)
			return fileName;
		return fileName.Substring(0, fileName.Length - extension.Length);
	}

	public static string? GetDirectoryName(string path)
	{
		int32 separator = -1;
		for (int32 index = 0; index < path.Length; index++)
			if (path[index] == '/' || path[index] == '\\')
				separator = index;

		if (separator < 0)
			return null;
		if (separator == 0)
			return path.Substring(0, 1);
		return path.Substring(0, separator);
	}

	public static string ChangeExtension(string path, string? extension)
	{
		string currentExtension = GetExtension(path);
		string withoutExtension = currentExtension.Length == 0
			? path
			: path.Substring(0, path.Length - currentExtension.Length);
		if (extension == null || extension.Length == 0)
			return withoutExtension;
		if (extension[0] != '.')
			extension = "." + extension;
		return withoutExtension + extension;
	}

	public static bool IsPathRooted(string path)
	{
		if (path.Length == 0)
			return false;
		return path[0] == '/' || path[0] == '\\' || path.Length > 1 && path[1] == ':';
	}
}
