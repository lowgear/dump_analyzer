using System;
using System.Linq;
using Castle.MicroKernel;
using DmpAnalyze;
using DmpAnalyze.Issues;
using DmpAnalyze.Metrics;
using Fclp;
using Microsoft.Diagnostics.Runtime;
using Newtonsoft.Json;
using FluentJsonNet;

namespace AnalyserApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var fclp = new FluentCommandLineParser();

            var dumpCmd = fclp.SetupCommand<DumpArguments>("dump")
                .OnSuccess(HandleDump);

            dumpCmd.Setup(a => a.File)
                .As('f', "file")
                .Required();
            
            var processCmd = fclp.SetupCommand<ProcessArguments>("proc")
                .OnSuccess(HandleProc);

            processCmd.Setup(a => a.ProcessId)
                .As('i', "id")
                .Required();

            fclp.Parse(args);
        }

        private static void HandleProc(ProcessArguments args)
        {
            var reporter = new Reporter();
            reporter
                .RegisterMetric(MetricCollectors.CollectWorkingSetMetric)
                .RegisterMetric(MetricCollectors.CollectThreadCountMetric)
                .RegisterMultiMetric(MetricCollectors.CollectHeapGenerationMetrics)
                .RegisterDetector(IssueDetectors.DetectMemLeaks)
                .RegisterDetector(IssueDetectors.DetectDeadLocks)
                .RegisterDetector(IssueDetectors.DetectLockConvoys);

            Report[] reports;
            using (var dt = DataTarget.AttachToProcess(args.ProcessId, 10000)) // todo timeout for what? if out?
            {
                reports = dt.ClrVersions
                    .Select(cv => reporter.Report(cv.CreateRuntime()))
                    .ToArray();
            }

            var jsonSerializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            jsonSerializer.Serialize(Console.Out, reports);
        }

        private static void HandleDump(DumpArguments args)
        {
            var reporter = new Reporter();
            reporter
                .RegisterMetric(MetricCollectors.CollectWorkingSetMetric)
                .RegisterMetric(MetricCollectors.CollectThreadCountMetric)
                .RegisterMultiMetric(MetricCollectors.CollectHeapGenerationMetrics)
                .RegisterDetector(IssueDetectors.DetectMemLeaks)
                .RegisterDetector(IssueDetectors.DetectDeadLocks)
                .RegisterDetector(IssueDetectors.DetectLockConvoys);

            Report[] reports;
            using (var dt = DataTarget.LoadCrashDump(args.File))
            {
                reports = dt.ClrVersions
                    .Select(cv => reporter.Report(cv.CreateRuntime()))
                    .ToArray();
            }

            var jsonSerializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            jsonSerializer.Serialize(Console.Out, reports);
        }
    }

    internal class ProcessArguments
    {
        public int ProcessId { get; set; }
    }

    internal class DumpArguments
    {
        public string File { get; set; }
    }
}