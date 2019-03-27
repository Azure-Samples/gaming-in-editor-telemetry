using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TelemetryAPI.Query
{
    public interface INode
    {
        string Type { get; }

        string Operator { get; }
    }

    public interface IGroupNode : INode
    {
        List<QuerySpec> Children { get; }
    }

    public interface IComparisonNode : INode
    {
        string Column { get; }
        JValue Value { get; }
        JValue[] Values { get; }
    }

    public interface IQueryNode : IGroupNode, IComparisonNode
    {

    }

    public class QuerySpec : IQueryNode
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("op")]
        public string Operator { get; set; }

        [JsonProperty("children")]
        public List<QuerySpec> Children { get; set; }

        [JsonProperty("column")]
        public string Column { get; set; }

        [JsonProperty("value")]
        public JValue Value { get; set; }

        [JsonProperty("values")]
        public JValue[] Values { get; set; }
    }
}
