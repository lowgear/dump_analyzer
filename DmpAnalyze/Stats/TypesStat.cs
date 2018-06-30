using System.Collections.Generic;

namespace DmpAnalyze
{
    public class TypesStat : Stat
    {
        public Dictionary<string, TypeStat> TypesStats { get; }


        public TypesStat(string title, string description, Dictionary<string, TypeStat> typesStats) :
            base(title, description)
        {
            TypesStats = typesStats;
        }
    }
}