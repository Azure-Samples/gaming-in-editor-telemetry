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
        static CosmosClient cosmosClient = new CosmosClient(Config.CosmosDbUri, Config.CosmosDbAuthKey);
        static Container container = cosmosClient.GetContainer(Config.CosmosDbId, Config.CosmosDbCollection);

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
                            //await collector.AddAsync(line);

                            SimpleEvent simpleEvent = JsonConvert.DeserializeObject<SimpleEvent>(line);
                            await container.CreateItemAsync<SimpleEvent>(simpleEvent, new PartitionKey(simpleEvent.ClientId));
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
