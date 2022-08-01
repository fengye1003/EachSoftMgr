using System;

namespace GetArgs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("No args are found");
                    break;
                default:
                    int num = 1;
                    foreach(string arg in args)
                    {
                        Console.WriteLine("\n#" + num + ":");
                        Console.WriteLine(arg);
                        num++;
                    }
                    break;
            }
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}