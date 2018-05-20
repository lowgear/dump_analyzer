using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze
{
    public class Stats
    {
        public Dictionary<string, TypeStat> TypesStats { get; private set; }

        public StackTraceStat[] StackTraceStats { get; private set; }

        public Stats(ClrRuntime runtime)
        {
            GetTypesStats(runtime);

            InitStackTraceStats(runtime);
        }

        private void InitStackTraceStats(ClrRuntime runtime)
        {
            var stackTraceStats = new Dictionary<string[], List<int>>(new StackTraceComparer());
            foreach (var thread in runtime.Threads)
            {
                var stackTrace = thread.StackTrace.Select(frame => frame?.DisplayString ?? "No representation")
                    .ToArray();
                if (!stackTraceStats.ContainsKey(stackTrace))
                    stackTraceStats[stackTrace] = new List<int>();
                stackTraceStats[stackTrace].Add(thread.ManagedThreadId);
            }

            StackTraceStats = stackTraceStats
                .Select(x => new StackTraceStat
                {
                    StackTrace = x.Key,
                    ThreadsCount = x.Value.Count,
                    ThreadIds = x.Value
                })
                .ToArray();
        }

        private void GetTypesStats(ClrRuntime runtime)
        {
            TypesStats = new Dictionary<string, TypeStat>();
            foreach (var clrObject in runtime.Heap.EnumerateObjects())
            {
                var typeName = clrObject.Type?.Name;

                if (typeName == null)
                    continue;
                if (!TypesStats.ContainsKey(typeName))
                    TypesStats[typeName] = new TypeStat();
                TypesStats[typeName].Count++;
                TypesStats[typeName].TotalSize += clrObject.Size;
            }
        }
    }

    public class StackTraceComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[] x, string[] y) => 
            x.Zip(y, (s, s1) => s1 == s).All(_ => _);

        public int GetHashCode(string[] obj)
        {
            return obj
                .Select(s => s.GetHashCode())
                .Aggregate(0, (x, y) => x ^ y);
        }
    }

    public class StackTraceStat
    {
        public string[] StackTrace { get; internal set; }
        public int ThreadsCount { get; internal set; }
        public List<int> ThreadIds { get; set; }


        public override int GetHashCode()
        {
            return StackTrace.Select(s => s.GetHashCode())
                .Aggregate((x, y) => x ^ y);
        }
    }

    public class TypeStat
    {
        public int Count { get; set; }
        public ulong TotalSize { get; set; }
    }
}