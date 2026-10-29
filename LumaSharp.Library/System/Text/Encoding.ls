namespace System.Text;

public virtual class Encoding : System.Object
{
	public static Encoding UTF8
	{
		get => new UTF8Encoding();
	}

	public static Encoding Unicode
	{
		get => new UnicodeEncoding();
	}

	public virtual uint8[] GetBytes(ReadOnlySpan<char> chars);

	public virtual int32 GetCharCount(scope uint8[] bytes);

	public virtual char[] GetChars(scope uint8[] bytes);
}
