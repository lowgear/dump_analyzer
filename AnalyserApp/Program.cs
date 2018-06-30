using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using DmpAnalyze;
using DmpAnalyze.Issues;
using DmpAnalyze.Metrics;
using Microsoft.Diagnostics.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AnalyserApp
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ProcessArguments, DumpArguments>(args)
                .MapResult(
                    (ProcessArguments a) => Run(a),
                    (DumpArguments a) => Run(a),
                    errs => 1);
        }

        private static int Run(Arguments options)
        {
            Report[] reports;
            using (var dt = options.GetDataTarget())
            {
                reports = dt.ClrVersions
                    .Select(cv => options.Reporter.Report(cv.CreateRuntime()))
                    .ToArray();
            }

            options.WriteReportsOut(reports);
            
//            if (options.Html)
//                new HtmlRenderer(Console.Out).Render(reports);
//            else
//            {
//                var jsonSerializer = new JsonSerializer
//                {
//                    Formatting = Formatting.Indented,
//                    Converters = {new DataSizeConverter()}
//                };
//                jsonSerializer.Serialize(Console.Out, reports);
//            }

            return 0;
        }
    }

    internal class Arguments
    {
        public Reporter Reporter { get; } = new Reporter();
        public Func<DataTarget> GetDataTarget { get; protected set; }

        public Action<Report[]> WriteReportsOut { get; protected set; } = reports =>
        {
            var jsonSerializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                Converters = {new DataSizeConverter()}
            };
            jsonSerializer.Serialize(Console.Out, reports);
        };
            
        [Option("dlk", Default = false, HelpText = "Check for deadlocks")]
        public bool Deadlocks
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterDetector(IssueDetectors.DetectDeadLocks);
            }
        }

        [Option("ex", Default = false, HelpText = "Check for unhandled exceptions")]
        public bool Exceptions
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterDetector(IssueDetectors.DetectUnhandledExceptions);
            }
        }

        [Option("tc", Default = false, HelpText = "Count threads")]
        public bool ThreadCount
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterMetrics(MetricCollectors.CollectThreadCountMetric);
            }
        }

        [Option("hg", Default = false, HelpText = "Generations counts and sizes")]
        public bool HeapGenerations
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterMultiMetric(MetricCollectors.CollectHeapGenerationMetrics);
            }
        }

        [Option("ts", Default = false, HelpText = "Object counts and sizes by types")]
        public bool TypesStats
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterStat(StatCollectors.CollectTypesStats);
            }
        }

        [Option("st", Default = false, HelpText = "Uniq stack traces and respective managed thread ids")]
        public bool StackTraces
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterStat(StatCollectors.CollectStackTraceStats);
            }
        }

        [Option("bxst", Default = false, HelpText = "Boxed structs counts and total sizes by types")]
        public bool Structs
        {
            get => false;
            set
            {
                if (value)
                    Reporter.RegisterStat(StatCollectors.CollectStructStats);
            }
        }

        [Option("html", Default = false, HelpText = "Render report to html instead of json")]
        public bool Html
        {
            get => false;
            set
            {
                if (value)
                    WriteReportsOut = reports => new HtmlRenderer(Console.Out).Render(reports);
            }
        }
    }

    [Verb("proc", HelpText = "Analyse live process")]
    internal class ProcessArguments : Arguments
    {
        [Value(0, HelpText = "Process id", Required = true)]
        public int ProcessId
        {
            get => 0;
            set => GetDataTarget = () => DataTarget.AttachToProcess(value, 10000, AttachFlag.Passive);
        }
    }

    [Verb("dump", HelpText = "Analyse dump file")]
    internal class DumpArguments : Arguments
    {
        [Value(0, HelpText = "Dump file path", Required = true)]
        public string DumpFile
        {
            get => null;
            set => GetDataTarget = () => DataTarget.LoadCrashDump(value);
        }
    }
}