using System.Collections.Generic;
using System.Linq;
using DmpAnalyze.Utils;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze.Issues
{
    public static class IssueDetectors
    {
        public const int ConvoyMinLength = 4;
        
        
//        public static IEnumerable<IIssue> DetectMemLeaks(ClrRuntime runtime, Report report)
//        {
//            var workingSet = report.Metrics.FirstOrDefault(m => m is WorkingSetMetric)?.Value ??
//                             MetricCollectors.CollectWorkingSetMetric(runtime).Value;
//            var suspiciousTypes = report.Stats.Top10ConsumingTypes.Where(kv => kv.Value > (ulong) (workingSet / 3));
//
//            return suspiciousTypes
//                .Select(kv =>
//                    new Issue($"Possible leak [{kv.Key}]",
//                        $"[{kv.Key}] objects consume {kv.Value * 100 / (ulong) workingSet}% of working set."));
//        }

        private class ClrThreadEqualityComparer : IEqualityComparer<ClrThread>
        {
            public bool Equals(ClrThread x, ClrThread y) => object.Equals(x, y);
            public int GetHashCode(ClrThread obj) => obj.GetHashCode();
        }

        public static IEnumerable<IIssue> DetectDeadLocks(ClrRuntime runtime, Report report)
        {            
            var blockedThreads = runtime.Heap
                .EnumerateBlockingObjects()
                .Where(o => o.Owner != null && o.Waiters.Count > 0)
                .Select(o => o.Owner);


            var lockCycles = Graphs
                .FindCycles(
                    blockedThreads,
                    thread => thread.BlockingObjects,
                    obj => obj.Owner,
                    new ClrThreadEqualityComparer());

            return lockCycles.Select(c => new DeadLockIssue(c));
        }

        public static IEnumerable<LockConvoyIssue> DetectLockConvoys(ClrRuntime runtime, Report report)
        {
            var convoyObjects = runtime.Heap.EnumerateBlockingObjects()
                .Where(o => o.Waiters.Count >= ConvoyMinLength);

            return convoyObjects
                .Select(o => new LockConvoyIssue(o));
        }

        public static IEnumerable<IIssue> DetectUnhandledExceptions(ClrRuntime runtime, Report report)
        {
            return runtime.Threads
                .Where(t => t.CurrentException != null)
                .Select(t => new UnhandledExceptionIssue(t));
        }
    }
}