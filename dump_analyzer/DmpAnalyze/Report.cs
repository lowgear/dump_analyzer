using System;
using System.Collections.Generic;
using System.Linq;
using DmpAnalyze.Issues;
using DmpAnalyze.Metrics;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze
{
    public class Report
    {
        public IReadOnlyList<IIssue> Issues { get; }
        public IReadOnlyList<Metric> Metrics { get; }
        public Stats Stats { get; }

        public Report(ClrRuntime runtime, 
            IEnumerable<Func<ClrRuntime, Report, IEnumerable<IIssue>>> detectors,
            IEnumerable<Func<ClrRuntime, Metric>> metricColletors)
        {
            Stats = new Stats(runtime);

            Metrics = metricColletors?.Select(m => m(runtime)).ToList() ?? new List<Metric>();

            Issues = detectors?.SelectMany(d => d(runtime, this)).ToList() ?? new List<IIssue>();
        }
    }
}