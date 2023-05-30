using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using TelemetryAPI.Utility;

namespace TelemetryAPI
{
    public static class CosmosSender
    {
        [FunctionName("CosmosSender")]
        public static async Task Run(
            // NOTE: If value is formatted as an '%environmentVariable%', then it will read the value at runtime
            [EventHubTrigger(
                "", // Not used if the name is in the connection string
                Connection = Config.EventHubConnectionStringConfigField, 
                ConsumerGroup = Config.ConsumerGroupEnvironmentVariable)] EventData[] messages,
            [CosmosDB(
                databaseName: Config.CosmosDbIdEnvironmentVariable,
                containerName: Config.CosmosDbCollectionEnvironmentVariable,
                CreateIfNotExists = true,
                Connection = Config.CosmosDbConnectionStringConfigField,
                PartitionKey = "/client_id")] IAsyncCollector<string> collector,
            ILogger log)
        {
            foreach (var message in messages)
            {
                byte[] payload = message.EventBody.ToArray();

                if (message.IsGzipCompressed())
                {
                    payload = await BufferUtils.GzipDecompressToArray(message.EventBody.ToArray());
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
                            await collector.AddAsync(line);
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
