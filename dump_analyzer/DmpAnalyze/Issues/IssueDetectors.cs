using System.Collections.Generic;
using System.Linq;
using DmpAnalyze.Metrics;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze.Issues
{
    public static class IssueDetectors
    {
        public static IEnumerable<IIssue> DetectMemLeaks(this ClrRuntime runtime, Report report)
        {
            var workingSet = report.Metrics.FirstOrDefault(m => m is WorkingSetMetric)?.Value ??
                             runtime.CollectWorkingSetMetric().Value;
            var suspiciousTypes = report.Stats.Top10ConsumingTypes.Where(kv => kv.Value > (ulong) (workingSet / 3));

            return suspiciousTypes
                .Select(kv =>
                    new Issue($"Possible leak [{kv.Key}]",
                        $"[{kv.Key}] objects consume {kv.Value * 100 / (ulong) workingSet}% of working set."));
        }
    }
}