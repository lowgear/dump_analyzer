﻿using System;
using System.Collections.Generic;
using System.Linq;
using DmpAnalyze;
using DmpAnalyze.Issues;
using DmpAnalyze.Metrics;
using Microsoft.Diagnostics.Runtime;
using Newtonsoft.Json;

namespace AnalyserApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {   
            var reporter = new Reporter();
            reporter
                .RegisterMetric(MetricCollectors.CollectWorkingSetMetric)
                .RegisterMetric(MetricCollectors.CollectThreadCountMetric)
                .RegisterMultiMetric(MetricCollectors.CollectHeapGenerationMetrics)
                .RegisterDetector(IssueDetectors.DetectMemLeaks);
//                .RegisterDetector(IssueDetectors.DetectDeadLocks);

            Report report;
            using (var dt = DataTarget.LoadCrashDump(args[0]))
            {
                var rt = dt.ClrVersions[0].CreateRuntime();
                
                report = reporter.Report(rt);
            }

            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Formatting = Formatting.Indented;
            jsonSerializer.Serialize(Console.Out, report);
        }
    }
}