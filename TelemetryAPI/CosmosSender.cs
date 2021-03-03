using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using TelemetryAPI.Utility;
using Newtonsoft.Json;

namespace TelemetryAPI
{
    public static class CosmosSender
    {
        static readonly string _cosmosUri = Config.CosmosDbUri;
        static readonly string _authKey = Config.CosmosDbAuthKey;
        static readonly string _databaseId = Config.CosmosDbId;
        static readonly string _containerId = Config.CosmosDbCollection;

        static CosmosClient _cosmosClient = new CosmosClient(
            _cosmosUri,
            _authKey,
            new CosmosClientOptions
            {
                //bulk mode to better saturate throughput for high volume writes
                AllowBulkExecution = true
            });
        
        static Container _container = _cosmosClient.GetContainer(_databaseId, _containerId);

        [FunctionName("CosmosSender")]
        public static async Task Run(
            [EventHubTrigger("", 
            Connection = Config.EventHubConnectionStringConfigField,
            ConsumerGroup = Config.ConsumerGroupEnvironmentVariable),] EventData[] messages, ILogger log)
        {
            foreach (var message in messages)
            {
                byte[] payload = message.Body.Array;

                if (message.IsGzipCompressed())
                {
                    payload = await BufferUtils.GzipDecompressToArray(message.Body.Array);
                }

                // Payload is expected to be a JSONL (one line per json record) formatted body
                var ev = Encoding.UTF8.GetString(payload);

                var split = ev.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                if (Config.EnableCosmos)
                {
                    foreach (var line in split)
                    {
                        if (String.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            
                            SimpleEvent simpleEvent = JsonConvert.DeserializeObject<SimpleEvent>(line);

                            await _container.CreateItemAsync<SimpleEvent>(
                                simpleEvent, 
                                new PartitionKey(simpleEvent.ClientId),
                                new ItemRequestOptions
                                {
                                    //optimize bandwidth for high write volume
                                    EnableContentResponseOnWrite = false
                                });
                        }
                        catch (Exception ex)
                        {
                            log.LogWarning(ex, "Exception occurred while inserting a document.\n" + ex.StackTrace);
                        }
                    }
                    log.LogDebug($"Inserted {split.Length} events.");
                }
            }
        }
    }
}
