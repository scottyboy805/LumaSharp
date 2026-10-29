namespace System.IO;

// These declarations are compiler-emitted LLVM runtime shims, not C ABI imports.
// Their names and signatures are part of the compiler/runtime ABI.
internal static unsafe class NativeFileSystem
{
	public const int32 OpenMode = 0;
	public const int32 CreateMode = 1;
	public const int32 AppendMode = 2;
	public const int32 OpenOrCreateMode = 3;
	public const int32 CreateNewMode = 5;

	public static extern int32 luma_fs_open(char* path, int32 mode, void** handle);
	public static extern void luma_fs_console_write(char* buffer, int32 length, bool appendNewLine);
	public static extern int32 luma_fs_read(void* handle, uint8* buffer, int32 count, int32* bytesRead);
	public static extern int32 luma_fs_write(void* handle, uint8* buffer, int32 count, int32* bytesWritten);
	public static extern int32 luma_fs_seek(void* handle, int32 offset, int32 origin, int32* position);
	public static extern int32 luma_fs_set_length(void* handle, int32 length);
	public static extern int32 luma_fs_flush(void* handle);
	public static extern int32 luma_fs_close(void* handle);

	public static extern bool luma_fs_file_exists(char* path);
	public static extern bool luma_fs_directory_exists(char* path);
	public static extern int32 luma_fs_create_directory(char* path);
	public static extern int32 luma_fs_delete_file(char* path);
	public static extern int32 luma_fs_delete_directory(char* path);
	public static extern int32 luma_fs_move(char* source, char* destination, bool overwrite);
	public static extern int32 luma_fs_copy(char* source, char* destination, bool overwrite);
	public static extern string luma_fs_current_directory();
	public static extern string luma_fs_temporary_directory();
}
