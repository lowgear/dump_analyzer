using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze.Issues
{
    public class DeadLockIssue : IIssue
    {
        private IReadOnlyList<ClrThread> Cycle { get; }

        public DeadLockIssue(IEnumerable<ClrThread> lockCycle)
        {
            Cycle = lockCycle.ToList();
        }

        public string Title => $"Deadlock of {Cycle.Count} threads";

        public string Message => string.Join("\n",
            Cycle
                .Select(t =>
                {
                    var blockingObjType = t.Runtime.Heap.GetObjectType(t.BlockingObjects.First().Object);
                    return
                        $"Thread with id {t.OSThreadId} locked on object of type {blockingObjType} in method {t.EnumerateStackTrace().First().Method.Name}";
                }));
    }
}