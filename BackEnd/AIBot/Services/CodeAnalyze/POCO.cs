using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Services.CodeAnalyze
{
    public enum InfoLevel
    {
        error,
        warning,
        style,
        performance,
        portability,
        information
    }

    public class CppCheckResult
    {
        public string Category { get; set; }
        public InfoLevel Level { get; set; }
        public Location Location { get; set; }
        public string Symbol { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            var location = Location == null ? "null" : Location.ToString();
            var symbol = Symbol == null ? "null" : Symbol;
            return $"CppCheckResult{{category={Category}, level={Level.ToString()}, Symbol={symbol}, location={location}}}";
        }
    }

    public class Location
    {
        public string FileName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public override string ToString()
        {
            return $"Location{{fileName={FileName}, line={Line}, colum={Column}}}";
        }
    }
}
