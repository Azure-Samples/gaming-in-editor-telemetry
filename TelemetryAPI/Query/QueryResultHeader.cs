namespace TelemetryAPI.Query
{
    /// <summary>
    /// Holds the results of a query made by a client
    /// </summary>
    public class QueryResultHeader
    {
        /// <summary>
        /// True if the query was executed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of results returned by the query
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Elapsed time in milliseconds of the query
        /// </summary>
        public long QueryTime { get; set; }
    }
}
