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

	public static extern int32 luma_fs_open(uint8* path, int32 mode, void** handle);
	public static extern int32 luma_fs_read(void* handle, uint8* buffer, int32 count, int32* bytesRead);
	public static extern int32 luma_fs_write(void* handle, uint8* buffer, int32 count, int32* bytesWritten);
	public static extern int32 luma_fs_seek(void* handle, int32 offset, int32 origin, int32* position);
	public static extern int32 luma_fs_set_length(void* handle, int32 length);
	public static extern int32 luma_fs_flush(void* handle);
	public static extern int32 luma_fs_close(void* handle);

	public static extern bool luma_fs_file_exists(uint8* path);
	public static extern bool luma_fs_directory_exists(uint8* path);
	public static extern int32 luma_fs_create_directory(uint8* path);
	public static extern int32 luma_fs_delete_file(uint8* path);
	public static extern int32 luma_fs_delete_directory(uint8* path);
	public static extern int32 luma_fs_move(uint8* source, uint8* destination, bool overwrite);
	public static extern int32 luma_fs_copy(uint8* source, uint8* destination, bool overwrite);
	public static extern string luma_fs_current_directory();
	public static extern string luma_fs_temporary_directory();
}
