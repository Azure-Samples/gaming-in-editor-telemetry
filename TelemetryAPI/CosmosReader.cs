using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Diagnostics;
using System.Collections.Generic;
using TelemetryAPI.Query;

namespace TelemetryAPI
{
    public static class CosmosReader
    {
        static readonly DocumentClient _client = new DocumentClient(new Uri(Environment.GetEnvironmentVariable("CosmosDbUri")), Environment.GetEnvironmentVariable("CosmosDbAuthKey"));
        static readonly FeedOptions feedOptions = new FeedOptions() { MaxItemCount = 30000, EnableCrossPartitionQuery = true };
        static readonly string _collection = Environment.GetEnvironmentVariable("CosmosDataCollection") ?? "MyGame";
        static readonly string _database = Environment.GetEnvironmentVariable("CosmosDatabaseId") ?? "TelemetryDB";

        [FunctionName("CosmosReader")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "query")] HttpRequest req,
            ILogger log)
        {
            try
            {
                int.TryParse(req.Query["take"].FirstOrDefault() ?? "10000", out int take);

                List<SimpleEvent> query = null;

                long start = Stopwatch.GetTimestamp();

                if (req.Method == HttpMethods.Get)
                {
                    query = DoGet(take);
                }
                else
                {
                    query = await DoPost(take, req);
                }

                long end = Stopwatch.GetTimestamp();

                QueryResultHeader hdr = new QueryResultHeader() { Count = query?.Count ?? 0, QueryTime = (end - start) / 10000, Success = true };
                QueryResult result = new QueryResult() { Results = query, Header = hdr };

                return new OkObjectResult(result);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Error occurred when querying data.\n" + ex.StackTrace);
                return new BadRequestResult();
            }
        }

        private static List<SimpleEvent> DoGet(int take)
        {
            return _client.CreateDocumentQuery<SimpleEvent>(UriFactory.CreateDocumentCollectionUri(_database, _collection), feedOptions)
                    .OrderByDescending(a => a.ClientTimestamp).Take(take).ToList();
        }

        private static async Task<List<SimpleEvent>> DoPost(int take, HttpRequest req)
        {
            string body = await req.ReadAsStringAsync();
            QuerySpec q = JsonConvert.DeserializeObject<QuerySpec>(body);
            string tableAlias = "e";

            var whereClause = new QueryParser(q, tableAlias).Parse();

            return _client.CreateDocumentQuery<SimpleEvent>(UriFactory.CreateDocumentCollectionUri(_database, _collection), FormatQuery(tableAlias, whereClause, take), feedOptions).ToList();
        }

        private static string FormatQuery(string tableAlias, string whereClause, int limit = 10000)
        {
            return $"SELECT TOP {limit} * FROM SimpleEvents {tableAlias} WHERE {whereClause} ORDER BY e.client_ts DESC";
        }
    }
}
