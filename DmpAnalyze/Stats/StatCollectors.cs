using System;
using System.Collections.Generic;
using System.Linq;
using DmpAnalyze.Utils;
using Microsoft.Diagnostics.Runtime;
using Vostok.Commons;

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
            var typesStats = TypesStats(
                runtime.Heap.EnumerateObjects(),
                o => o.Type?.Name);

            return new TypesStat(
                "Types statistics",
                "Numbers of objects and total sizes of each type.",
                typesStats);
        }

        public static Stat CollectStructStats(ClrRuntime runtime)
        {
            var typesStats = TypesStats(
                runtime.Heap.EnumerateObjects()
                    .Where(o => o.Type != null && o.Type.IsValueClass),
                o => o.Type?.Name);

            return new TypesStat(
                "Boxed structs statistics",
                "Numbers of boxed structs and total sizes of each struct type.",
                typesStats);
        }
        
        public static IEnumerable<Stat> CollectTypeByHeapGensStats(ClrRuntime runtime)
        {
            var typesStats = TypesStats(
                runtime.Heap.EnumerateObjects(),
                o => (o.Type?.Name, runtime.GetGenOrLOH(o.Address)));

            return typesStats
                .GroupBy(e => e.Key.Item2)
                .OrderBy(g => g.Key)
                .Select(g =>
                    new TypesStat(
                        $"Types statistics for {(g.Key == 3 ? "LOH" : $"generation {g.Key}")}",
                        "Numbers of objects and total sizes of each type.",
                        g.ToDictionary(e => e.Key.Item1, e => e.Value)));
        }

        private static Dictionary<T, TypeStat> TypesStats<T>(IEnumerable<ClrObject> objects,
            Func<ClrObject, T> descriminator)
        {
            var typesStats = new Dictionary<T, TypeStat>();
            foreach (var clrObject in objects.Where(o => o.Type != null))
            {
                var desc = descriminator(clrObject);

                if (desc == null)
                    continue;
                if (!typesStats.ContainsKey(desc))
                    typesStats[desc] = new TypeStat(clrObject.Type);
                typesStats[desc].Count++;
                typesStats[desc].TotalSize += DataSize.FromBytes((long) clrObject.Size);
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
}