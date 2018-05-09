﻿using System;
using System.Collections.Generic;
using System.Linq;
using DmpAnalyze.Issues;
using DmpAnalyze.Metrics;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze
{
    public class Reporter
    {
        private HashSet<Func<ClrRuntime, Metric>> MetricCollectors { get; } =
            new HashSet<Func<ClrRuntime, Metric>>();

        private HashSet<Func<ClrRuntime, IEnumerable<Metric>>> MultiMetricCollectors { get; } =
            new HashSet<Func<ClrRuntime, IEnumerable<Metric>>>();

        private HashSet<Func<ClrRuntime, Report, IEnumerable<IIssue>>> Detectors { get; } =
            new HashSet<Func<ClrRuntime, Report, IEnumerable<IIssue>>>();

        public Report Report(ClrRuntime runtime)
        {
            var report = new Report(runtime);

            report.Metrics = MetricCollectors
                .Select(m => m(runtime))
                .Concat(MultiMetricCollectors
                        .SelectMany(c => c(runtime)))
                .ToList();

            report.Issues = Detectors.SelectMany(d => d(runtime, report)).ToList();

            return report;
        }

        public Reporter RegisterMetric(Func<ClrRuntime, Metric> metricCollector)
        {
            MetricCollectors.Add(metricCollector);

            return this;
        }

        public Reporter RegisterMultiMetric(Func<ClrRuntime, IEnumerable<Metric>> multiMetricCollector)
        {
            MultiMetricCollectors.Add(multiMetricCollector);

            return this;
        }

        public Reporter RegisterDetector(Func<ClrRuntime, Report, IEnumerable<IIssue>> detector)
        {
            Detectors.Add(detector);

            return this;
        }
    }
}