using System.Collections.Generic;

namespace MemoryLeak
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var list = new List<int>();
            while (true)
            {
                list.Add(0);
            }
        }
    }
}