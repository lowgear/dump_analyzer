using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;
using Newtonsoft.Json;

namespace DmpAnalyze.Issues
{
    public class DeadLockIssue : IIssue
    {
        // todo get rid of attribute and use configurable serialiser
        [JsonIgnore] public List<Tuple<ClrThread, BlockingObject>> Cycle { get; }

        public DeadLockIssue(IEnumerable<Tuple<ClrThread, BlockingObject>> lockCycle)
        {
            Cycle = lockCycle.ToList();
            Message = string.Join("\n",
                Cycle
                    .Select(t =>
                    {
                        var thread = t.Item1;
                        var blockingObj = t.Item2;
                        var blockingObjRef = blockingObj.Object;
                        var blockingObjType = thread.Runtime.Heap.GetObjectType(blockingObjRef);

                        return
                            $"Thread with id [{thread.OSThreadId}] locked object is blocked by object with ref [{blockingObjRef}] of type {blockingObjType}.";
                    }));
        }

        public string Title => $"Deadlock of {Cycle.Count} threads";

        public string Message { get; }
    }
}