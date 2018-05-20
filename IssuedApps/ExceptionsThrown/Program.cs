using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExceptionsThrown
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var t = new Thread(() => throw new Exception("we throw in other thread"));
            
            t.Start();

            throw new Exception("we throw in main thread");
            
            Console.WriteLine("Done.");
        }
    }
}