using System;
using System.IO;

class Program 
{
	class Inner
	{
		public string Field = "This is a field";
		public this()
		{
			Console.WriteLine("Inner Created!");
		}
		
		~this()
		{
			Console.WriteLine("Inner Destroyed");
		}
	}
	
	struct St
	{
		public int32 myField;
	}
	

    public static void Main() 
	{
		Type t = typeof(int32);
		object inst = t.New();
		Console.WriteLine(((int32)inst).ToString());
		
		Inner value = (Inner)typeof(Inner).New();
		Console.WriteLine(value.Field);
	
		Console.WriteLine(nameof(System.IO));
		
		int32 num = 123;
		object boxedNum = num;
		let type = num.GetType();
		let boxedType = boxedNum.GetType();
		Console.WriteLine("Int32 type = " + type.ToString());
		Console.WriteLine("Boxed Int32 type = " + type.ToString());
	
		St[] arr = new { default, default };
		let stRef = ref arr[0];
		
		stRef.myField = 1234;
		
		Console.WriteLine("Field val = " + arr[0].myField);
	
		//File.Create("MyFile.txt");
		
		Stream fs = new FileStream("MyFile.txt", FileMode.Create);
		
		uint8[] bytes = new { 1, 2, 3, 4 };
		fs.Write(bytes);
		
		
		{
		Inner? inner = scope Inner();
		Console.WriteLine("After create inner");
		inner = null;
		}
	
		let count = 5;
		
		for(var i = 0; i < count; i++)
			Console.WriteLine("Counting to : " + i.ToString());			
			
		Console.WriteLine("Finished counting!");
		
		try
		{
			Other();
		}
		catch(Exception e)
		{
			//Console.WriteLine(e.Message);
			//Console.WriteLine(e.StackTrace);
			Console.WriteLine(e.ToString());
		}
		
    }
	
	static void Other()
	{
		throw new Exception("This is a message from exception!");
	}
}