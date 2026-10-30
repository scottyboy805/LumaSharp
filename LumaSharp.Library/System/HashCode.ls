namespace System;

public static class HashCode
{
	public static int32 Calculate(bool value)
	{
		return value ? 1 : 0;
	}

	public static int32 Calculate(char value)
	{
		return (int32)value;
	}

	public static int32 Calculate(int8 value)
	{
		return value;
	}

	public static int32 Calculate(uint8 value)
	{
		return value;
	}

	public static int32 Calculate(int16 value)
	{
		return value;
	}

	public static int32 Calculate(uint16 value)
	{
		return value;
	}

	public static int32 Calculate(int32 value)
	{
		return value;
	}

	public static int32 Calculate(uint32 value)
	{
		unsafe unchecked
		{
			return (int32)value;
		}
	}

	public static int32 Calculate(int64 value)
	{
		unsafe unchecked
		{
			return (int32)(value ^ (value >> 32));
		}
	}

	public static int32 Calculate(uint64 value)
	{
		unsafe unchecked
		{
			return (int32)(value ^ (value >> 32));
		}
	}

	public static int32 Calculate(int value)
	{
		return Calculate((int64)value);
	}

	public static int32 Calculate(uint value)
	{
		return Calculate((uint64)value);
	}

	public static unsafe int32 Calculate(float value)
	{
		if (Float.IsNaN(value))
			return 2143289344;
		if (value == 0.0f)
			return 0;

		uint32* bits = (uint32*)&value;
		unsafe unchecked
		{
			return (int32)*bits;
		}
	}

	public static unsafe int32 Calculate(double value)
	{
		if (Double.IsNaN(value))
			return 2146959360;
		if (value == 0.0)
			return 0;

		uint64* bits = (uint64*)&value;
		unsafe unchecked
		{
			return (int32)(*bits ^ (*bits >> 32));
		}
	}

	public static int32 Combine(int32 hashCode)
	{
		return hashCode;
	}

	public static int32 Combine(int32 first, int32 second)
	{
		unsafe unchecked
		{
			return first * 31 + second;
		}
	}

	public static int32 Combine(int32... hashCodes)
	{
		int32 hash = 0;
		for (let hashCode in hashCodes)
		{
		unsafe unchecked
		{
			hash = hash * 31 + hashCode;
		}
		}
		return hash;
	}
}
