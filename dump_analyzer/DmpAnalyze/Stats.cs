using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze
{
    public class Stats
    {
        public IReadOnlyList<KeyValuePair<string, ulong>> Top10ConsumingTypes { get; }

        public Stats(ClrRuntime runtime)
        {
            Top10ConsumingTypes = runtime.Heap
                .EnumerateObjects()
                .Where(o => !o.Type.IsFree)
                .GroupBy(o => o.Type)
                .Select(g => new KeyValuePair<string, ulong>(g.Key.Name, g.Aggregate(0UL, (a, o) => a + o.Size)))
                .OrderByDescending(t => t.Value)
                .Take(10)
                .ToList();
            // TODO more stats
        }
    }
}