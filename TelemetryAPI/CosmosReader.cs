using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;

using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using TelemetryAPI.Query;

namespace TelemetryAPI
{
    public static class CosmosReader
    {
        static CosmosClient cosmosClient = new CosmosClient(Config.CosmosDbUri, Config.CosmosDbAuthKey);
        static Container container = cosmosClient.GetContainer(Config.CosmosDbId, Config.CosmosDbCollection);

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
                    query = await DoGet(take);
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
            catch (Exception ex)
            {
                log.LogError(ex, "Error occurred when querying data.\n" + ex.StackTrace);
                return new BadRequestResult();
            }
        }

        private static async Task<List<SimpleEvent>> DoGet(int take)
        {
            string query = $"SELECT TOP {take} * from c ORDER BY c.client_ts DESC";

            FeedIterator<SimpleEvent> resultSet = container.GetItemQueryIterator<SimpleEvent>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    MaxItemCount = -1 //better perf using -1 for dynamic count
                    //would be great to have /clientId value here for in-partition queries
                    //PartitionKey = new PartitionKey("0316126682083") 
                });

            List<SimpleEvent> simpleEvents = new List<SimpleEvent>();

            while (resultSet.HasMoreResults)
            {
                FeedResponse<SimpleEvent> response = await resultSet.ReadNextAsync();

                foreach(SimpleEvent simpleEvent in response)
                {
                    simpleEvents.Add(simpleEvent);
                }
            }

            return simpleEvents;
        }

        private static async Task<List<SimpleEvent>> DoPost(int take, HttpRequest req)
        {
            string body = await req.ReadAsStringAsync();
            QuerySpec q = JsonConvert.DeserializeObject<QuerySpec>(body);
            string tableAlias = "e";

            var whereClause = new QueryParser(q, tableAlias).Parse();

            string query = FormatQuery(tableAlias, whereClause, take);

            FeedIterator<SimpleEvent> resultSet = container.GetItemQueryIterator<SimpleEvent>(
                query,
                requestOptions: new QueryRequestOptions()
                {
                    MaxItemCount = -1 //dynamic max count 
                    //would be great to have /clientId value here
                    //PartitionKey = new PartitionKey("0316126682083")
                });

            List<SimpleEvent> simpleEvents = new List<SimpleEvent>();

            while (resultSet.HasMoreResults)
            {
                FeedResponse<SimpleEvent> response = await resultSet.ReadNextAsync();

                foreach (SimpleEvent simpleEvent in response)
                {
                    simpleEvents.Add(simpleEvent);
                }
            }

            return simpleEvents;

        }

        private static string FormatQuery(string tableAlias, string whereClause, int limit = 10000)
        {
            return $"SELECT TOP {limit} * FROM SimpleEvents {tableAlias} WHERE {whereClause} ORDER BY e.client_ts DESC";
        }
    }
}
