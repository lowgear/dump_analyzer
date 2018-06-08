using System;
using System.Collections.Generic;
using System.Linq;
using DmpAnalyze.Utils;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze
{
    public static class StatCollectors
    {
        public static Stat CollectStackTraceStats(ClrRuntime runtime)
        {
//            var stackTraceStats = new Dictionary<string[], List<int>>(new StackTraceComparer());
//            foreach (var thread in runtime.Threads)
//            {
//                var stackTrace = thread.StackTrace
//                    .Select(frame => frame.DisplayString.EscapeNull())
//                    .ToArray();
//                if (!stackTraceStats.ContainsKey(stackTrace))
//                    stackTraceStats[stackTrace] = new List<int>();
//                stackTraceStats[stackTrace].Add(thread.ManagedThreadId);
//            }
//
//            var stackTraceStats = stackTraceStats
//                .Select(x => new StackTraceStat
//                {
//                    StackTrace = x.Key,
//                    ThreadsCount = x.Value.Count,
//                    ThreadIds = x.Value
//                })
//                .ToArray();

            var stackTraceStats = runtime.Threads
                .Select(t => Tuple.Create(t.ManagedThreadId, t.GetStackTraceRepr()))
                .GroupBy(t => t.Item2, t => t.Item1, new StackTraceComparer())
                .ToDictionary(g => g.Key, g => g.ToArray());

            return new StackTraceStat(stackTraceStats);
        }

        public static Stat CollectTypesStats(ClrRuntime runtime)
        {
            var typesStats = TypesStats(runtime.Heap.EnumerateObjects());

            return new TypesStat(
                "Types statistics",
                "Numbers of objects and total sizes of each type.",
                typesStats);
        }

        public static Stat CollectStructStats(ClrRuntime runtime)
        {
            var typesStats = TypesStats(runtime.Heap.EnumerateObjects()
                .Where(o => o.Type != null && o.Type.IsValueClass));

            return new TypesStat(
                "Boxed structs statistics",
                "Numbers of boxed structs and total sizes of each struct type.",
                typesStats);
        }

        private static Dictionary<string, TypeStat> TypesStats(IEnumerable<ClrObject> objects)
        {
            Dictionary<string, TypeStat> typesStats = new Dictionary<string, TypeStat>();
            foreach (var clrObject in objects)
            {
                var typeName = clrObject.Type?.Name;

                if (typeName == null)
                    continue;
                if (!typesStats.ContainsKey(typeName))
                    typesStats[typeName] = new TypeStat();
                typesStats[typeName].Count++;
                typesStats[typeName].TotalSize += clrObject.Size;
            }

            return typesStats;
        }
    }

    internal class StackTraceComparer : IEqualityComparer<string[]>
    {
        public bool Equals(string[] x, string[] y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            if (x.Length != y.Length)
                return false;
            for (var i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;
            return true;
        }

        public int GetHashCode(string[] obj) =>
            obj.GetAggregatedHashCode();
    }

    public class TypeStat
    {
        public int Count { get; set; }
        public ulong TotalSize { get; set; }
    }
}