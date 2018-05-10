using System.Threading.Tasks;

namespace DeadLock
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var lock1 = new object();
            var lock2 = new object();
            Task.Run(() =>
            {
                while (true)
                {
                    lock (lock1)
                    {
                        lock (lock2)
                        {
                        }
                    }
                }
            });

            while (true)
            {
                lock (lock2)
                {
                    lock (lock1)
                    {
                    }
                }
            }
        }
    }
}