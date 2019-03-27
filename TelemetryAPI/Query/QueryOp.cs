using System.Collections.Generic;

namespace TelemetryAPI.Query
{
    public static class QueryOp
    {
        public const string Eq = "eq";
        public const string Gt = "gt";
        public const string Gte = "gte";
        public const string Lt = "lt";
        public const string Lte = "lte";
        public const string Neq = "neq";
        public const string In = "in";
        public const string And = "and";
        public const string Or = "or";
        public const string Btwn = "btwn";

        public static Dictionary<string, string> Map = new Dictionary<string, string>
        {
            { Eq, "=" },
            { Gt, ">" },
            { Gte, ">=" },
            { Lt, "<" },
            { Lte, "<=" },
            { Neq, "!=" },
            { In, "in" },
            { And, "AND" },
            { Or, "OR" },
            { Btwn, "BETWEEN"}
        };
    }
}
