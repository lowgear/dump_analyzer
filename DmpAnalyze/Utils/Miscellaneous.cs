﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace DmpAnalyze.Utils
{
    public static class Miscellaneous
    {
        public static string EscapeNull(this string s) =>
            s ?? "<No representation>";

        public static int GetAggregatedHashCode<T>(this IEnumerable<T> enumerable)
        {
            return enumerable
                .Aggregate(37, 
                    (a, x) => (a * 32027 + x.GetHashCode()) % 32993);
        }
        
        public static string[] GetStackTraceRepr(this ClrThread thread) =>
            thread.StackTrace
                .Select(frame => frame.DisplayString.EscapeNull())
                .ToArray();
    }
}