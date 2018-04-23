using System;
using System.Collections.Generic;
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
            var issueDetectors = new Func<ClrRuntime, Report, IEnumerable<IIssue>>[]
            {
                IssueDetectors.DetectMemLeaks
            };
            
            var metricCollector = new Func<ClrRuntime, Metric>[]
            {
                MetricCollectors.CollectWorkingSetMetric,
                MetricCollectors.CollectThreadCountMetric
            };

            Report report;
            using (var dt = DataTarget.LoadCrashDump(args[0]))
            {
                var rt = dt.ClrVersions[0].CreateRuntime();
                
                report = new Report(rt, issueDetectors, metricCollector);

//                Console.WriteLine("done");
            }

            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Formatting = Formatting.Indented;
            jsonSerializer.Serialize(Console.Out, report);
        }
    }
}