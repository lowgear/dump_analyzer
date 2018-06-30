﻿using Microsoft.Diagnostics.Runtime;
using Vostok.Commons;

namespace DmpAnalyze
{
    public class TypeStat
    {
        public TypeStat(ClrType clrObjectType)
        {
            MethodTable = clrObjectType?.MethodTable;
        }

        public ulong? MethodTable { get; }
        public int Count { get; set; }
        public DataSize TotalSize { get; set; }
    }
}