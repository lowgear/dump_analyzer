﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LockConvoy
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var spinCount = int.Parse(Console.ReadLine());
            var lock1 = new object();
            object a = null;
            Task.WaitAll(
                Enumerable.Repeat(0, 4).Select(o =>
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            lock (lock1)
                            {
                                Thread.Sleep(100);
                                Thread.SpinWait(spinCount);
                            }
                        }
                    })).ToArray());
        }
    }
}