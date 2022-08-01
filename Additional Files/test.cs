using System;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Success!");
			if(args!=null)
			{
				Console.WriteLine(args[0]);
			}
		}
	}
}