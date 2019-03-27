using Newtonsoft.Json;
using System.Collections.Generic;

namespace TelemetryAPI.Query
{
    /// <summary>
    /// The result wrapper for a query
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// Stores information about the query results
        /// </summary>
        [JsonProperty("header")]
        public QueryResultHeader Header { get; set; }

        /// <summary>
        /// The results received from the query
        /// </summary>
        [JsonProperty("results")]
        public List<SimpleEvent> Results { get; set; }
    }
}
