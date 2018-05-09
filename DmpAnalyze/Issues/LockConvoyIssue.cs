using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze.Issues
{
    public class LockConvoyIssue : IIssue
    {
        public LockConvoyIssue(BlockingObject blockingObject)
        {
            Title = $"Lock convoy of {blockingObject.Waiters} waiting and one locking threads";

            var objRef = blockingObject.Object;

            Message = string.Join("\n", new[]
                {
                    $"Lock convoy to object ref [{objRef}] of type {blockingObject.Owner.Runtime.Heap.GetObjectType(objRef)}",
                    $"Locking thread id [{blockingObject.Owner.OSThreadId}]",
                    "Waiting threads ids:"
                }
                .Concat(blockingObject.Waiters
                    .Select(t => t.OSThreadId.ToString())));
        }

        public string Title { get; }
        public string Message { get; }
    }
}