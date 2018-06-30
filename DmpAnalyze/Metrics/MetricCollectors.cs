using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze.Metrics
{
    public static class MetricCollectors
    {
        public static Metric CollectThreadCountMetric(ClrRuntime runtime) =>
            new Metric("Threads count", runtime.Threads.Count);


        public static IEnumerable<Metric> CollectHeapGenerationMetrics(ClrRuntime runtime)
        {
//            var stat = new int[4];
//            foreach (var address in runtime.Heap.EnumerateObjectAddresses())
//            {
//                var segment = runtime.Heap.GetSegmentByAddress(address);
//                var gen = segment.IsLarge ? 3 : segment.GetGeneration(address);
//                stat[gen]++;
//            }
//
//            for (var i = 0; i < stat.Length; i++)
//                yield return new Metric($"Heap generation {i}", stat[i]);
//            
            return runtime.Heap
                .EnumerateObjectAddresses()
                .Select(a =>
                {
                    var g = runtime.Heap.GetGeneration(a);
                    if (g == 2 && runtime.Heap.GetSegmentByAddress(a).IsLarge)
                        g = 3;
                    return g;
                })
                .GroupBy(g => g)
                .OrderBy(g => g.Key)
                .Select(g => new Metric(
                    g.Key != 3 ? 
                        $"Heap generation {g.Key} objects count" : 
                        "Large Objects Heap objects count", 
                    g.Count()));
        }
    }
}