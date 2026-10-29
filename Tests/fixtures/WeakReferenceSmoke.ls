using System;

public class Item
{
	public int32 Value;
	public this(int32 value) { Value = value; }
}

public class Program
{
	public static int32 Main()
	{
		Weak<Item>? weak = null;
		Weak<Item>? second = null;
		{
			Item target = new Item(42);
			weak = new Weak<Item>(target);
			second = new Weak<Item>(target);
			if (!weak!.IsAlive) return 1;
			Item? promoted = weak.Target;
			if (promoted == null || promoted.Value != 42) return 2;
		}
		Item? finalTarget = weak.Target;
		Item? secondTarget = second.Target;
		if (weak!.IsAlive || second!.IsAlive || finalTarget != null || secondTarget != null) return 3;
		Weak<string> literal = new Weak<string>("literal");
		if (!literal.IsAlive || literal.Target != "literal") return 4;
		Weak<string>? dynamicWeak = null;
		{
			string dynamic = new string('x', 3);
			dynamicWeak = new Weak<string>(dynamic);
		}
		string? dynamicTarget = dynamicWeak!.Target;
		if (dynamicWeak.IsAlive || dynamicTarget != null) return 5;
		Weak<int32[]>? weakArray = null;
		{
			int32[] array = new int32[2];
			weakArray = new Weak<int32[]>(array);
		}
		int32[]? arrayTarget = weakArray!.Target;
		if (weakArray.IsAlive || arrayTarget != null) return 6;
		Weak<Item> temporary = new Weak<Item>(new Item(7));
		if (temporary.IsAlive) return 7;
		Weak<object>? weakObject = null;
		{
			object boxed = 42;
			weakObject = new Weak<object>(boxed);
		}
		object? boxedTarget = weakObject!.Target;
		return !weakObject.IsAlive && boxedTarget == null ? 0 : 8;
	}
}
