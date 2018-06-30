using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using DmpAnalyze;
using DmpAnalyze.Issues;
using DmpAnalyze.Metrics;

namespace AnalyserApp
{
    public class HtmlRenderer
    {
        private static string CssDefinition { get; } = @"
<style type=""text/css"">
	table { border-collapse: collapse; display: block; width: 100%; overflow: auto; }
	td, th { padding: 6px 13px; border: 1px solid #ddd; }
	tr { background-color: #fff; border-top: 1px solid #ccc; }
	tr:nth-child(even) { background: #f8f8f8; }
</style>";

        private TextWriter Writer { get; }

        public HtmlRenderer(TextWriter writer)
        {
            Writer = writer;
        }

        public void Render(Report[] reports)
        {
            using (Tag("head"))
            {
                Writer.WriteLine(CssDefinition);
            }

            using (Tag("body"))
                foreach (var report in reports)
                    Render(report);
        }

        private void Render(Report report)
        {
            Writer.WriteLine(@"<h1>Report</h1>");

            Writer.WriteLine(@"<h2>Metrics</h2>");
            using (new DetailsTag(Writer))
                Render(report.Metrics);

            Writer.WriteLine(@"<h2>Issues</h2>");
            using (new DetailsTag(Writer))
                foreach (var issue in report.Issues)
                    Render(issue);

            Writer.WriteLine(@"<h2>Statistics</h2>");
            using (new DetailsTag(Writer))
                foreach (var stat in report.Stats)
                    Render(stat);
        }

        private void Render(IReadOnlyList<Metric> reportMetrics)
        {
            using (Tag("table"))
            {
                using (Tag("tr"))
                {
                    using (Tag("th"))
                        Write("Metric name");
                    using (Tag("th"))
                        Write("Value");
                }

                foreach (var metric in reportMetrics)
                    using (Tag("tr"))
                    {
                        using (Tag("td"))
                            Write(metric.Name);
                        using (Tag("td"))
                            Write(metric.Value.ToString());
                    }
            }
        }

        private void Write(string str)
        {
            Writer.WriteLine(
                string.Join("<br>", str.Split('\n')
                    .Select(HttpUtility.HtmlEncode)));
        }

        private void Render(Stat stat)
        {
            using (Tag("h3"))
                Write(stat.Title);
            using (Tag("p"))
                Write(stat.Description);

            // todo refactor shit
            // this is a temporary stab
            if (stat is StackTraceStat sts)
            {
                RenderTable(
                    sts.StackTraceInfos.OrderByDescending(s => s.ThreadsCount),
                    ("Threads count", s => Write(s.ThreadsCount.ToString())),
                    ("Thread ids", s => RenderPossiblyExpandableList(s.ThreadIds, ", ", 10, 5)),
                    ("Stack trace", s => RenderStackTrace(s.StackTrace)));
            }
            else if (stat is TypesStat ts)
            {
                using (Tag("h4"))
                    Write("Top 20 by objects count");
                RenderTypesStatsTable(ts.TypesStats.OrderByDescending(s => s.Value.Count).Take(20));

                using (Tag("h4"))
                    Write("Top 20 by total size");
                RenderTypesStatsTable(ts.TypesStats.OrderByDescending(s => s.Value.TotalSize).Take(20));
            }
            else
                throw new NotImplementedException();
        }

        private void RenderPossiblyExpandableList<T>(ICollection<T> elements, string separator, int threshold, int head)
        {
            var strings = elements
                .Select(e => HttpUtility.HtmlEncode(e.ToString()))
                .ToArray();

            if (elements.Count <= threshold)
            {
                Writer.Write(string.Join(
                    separator,
                    strings));
                return;
            }

            var title = string.Join(
                separator,
                strings.Take(head)
            );

            using (new DetailsTag(Writer, title))
                Writer.Write(string.Join(
                    separator,
                    strings.Skip(head)));
        }

        private void RenderStackTrace(string[] s)
        {
            RenderPossiblyExpandableList(s, "<br>", 7, 4);
//            Writer.Write(string.Join("<br>\n", s));
        }

        private void RenderTypesStatsTable(IEnumerable<KeyValuePair<string, TypeStat>> typesStats)
        {
            RenderTable(
                typesStats,
                ("Type name", s => Write(s.Key)),
                ("Method Table", s => Write(s.Value.MethodTable.ToString())),
                ("Objects count", s => Write(s.Value.Count.ToString())),
                ("Total objects size", s => Write(s.Value.TotalSize.ToString())));
        }

        private void RenderTable<T>(IEnumerable<T> elements, params ValueTuple<string, Action<T>>[] headAndSelector)
        {
            using (Tag("table"))
            {
                using (Tag("tr"))
                    foreach (var head in headAndSelector)
                        using (Tag("th"))
                            Write(head.Item1);

                foreach (var e in elements)
                    using (Tag("tr"))
                        foreach (var selector in headAndSelector)
                            using (Tag("td"))
                                selector.Item2(e);
            }
        }

        private void Render(IIssue issue)
        {
            using (Tag("h3"))
                Write(issue.Title);
            Write(issue.Message);

            if (issue is DeadLockIssue dlk)
            {
                using (Tag("h4"))
                    Write("Deadlock cycle");
                RenderTable(
                    dlk.Cycle.Zip(dlk.StackTraces, (c, s) => (c, s)),
                    ("Thread id", e => Write(e.Item1.Item1.ManagedThreadId.ToString())),
                    ("Stack trace", e => RenderStackTrace(e.Item2)));
            }
            else if (issue is UnhandledExceptionIssue ex)
            {
                using (Tag("h4"))
                    Write("Exception type");
                Write(ex.ExceptionType);

                using (Tag("h4"))
                    Write("Exception message");
                using (new DetailsTag(Writer))
                    Write(ex.ExceptionMessage);

                using (Tag("h4"))
                    Write("Stack trace");
                RenderStackTrace(ex.StackTrace);
            }
        }

        private Tag Tag(string tagName) => new Tag(Writer, tagName);
    }

    internal class Tag : IDisposable
    {
        private TextWriter Writer { get; }
        public string TagName { get; }

        public Tag(TextWriter writer, string tagName)
        {
            Writer = writer;
            TagName = tagName;

            Writer.WriteLine($"<{tagName}>");
        }

        public void Dispose()
        {
            Writer.WriteLine($"</{TagName}>");
        }
    }

    internal class DetailsTag : IDisposable
    {
        public TextWriter Writer { get; }

        public DetailsTag(TextWriter writer, string title = "Click to expand")
        {
            Writer = writer;

            Writer.WriteLine("<details>");
            Writer.WriteLine($"<summary>{title}</summary>");
        }

        public void Dispose()
        {
            Writer.WriteLine("</details>");
        }
    }
}