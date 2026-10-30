using System;
using System.IO;

class Program 
{
	class Inner
	{
		public this()
		{
			Console.WriteLine("Inner Created!");
		}
		
		~this()
		{
			Console.WriteLine("Inner Destroyed");
		}
	}
	

    public static void Main() 
	{
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
    }
}