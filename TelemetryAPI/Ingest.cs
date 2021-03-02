using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TelemetryAPI.Utility;

namespace TelemetryAPI
{
    public static class Ingest
    {
        // The version of the algorithms used in this function - increment if something gets changed
        const string INGEST_VERSION = "1.0.0";

        // The version of the payload being pushed into event hub - increment if something is changed so downstream functions can detect
        const string PAYLOAD_VERSION = "1.0.0";

        const int MAX_EH_PAYLOAD_SIZE = 1 * 1024 * 1024; // 1MB

        static HashAlgorithm _md5 = MD5.Create();
        static JsonMergeSettings _mergeSettings = new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union, MergeNullValueHandling = MergeNullValueHandling.Merge };

        [FunctionName("ingest")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,
            "post",
            Route = null)] HttpRequestMessage req,
            [EventHub(
                "", // Not used if the name is in the connection string
                Connection = Config.EventHubConnectionStringConfigField)] ICollector<EventData> messages,
            ILogger log,
            ExecutionContext context)
        {
            using (var payload = BufferUtils.GetStream())
            {
                try
                {
                    string clientId = null;
                    string batchId = context.InvocationId.ToString("N");
                    string ingestTimestamp = DateTime.UtcNow.ToString("O");

                    // Extract content from body
                    string content = await GetContentAsStringAsync(req.Content);

                    using (StreamWriter writer = new StreamWriter(payload, new UTF8Encoding(false, true), 4 * 1024, true))
                    {
                        // Create a JSONL formatted payload (one json structure per line)
                        foreach (var se in ParseContent(content))
                        {
                            clientId = clientId ?? se["client_id"].Value<string>();

                            // Stamp event with ingestion metadata - Useful for debugging issues later
                            se.Add("ingest_ts", JToken.FromObject(ingestTimestamp));
                            se.Add("ingest_activity_id", JToken.FromObject(batchId));
                            se.Add("ingest_version", INGEST_VERSION);

                            // Check to make sure we can generate a unique id for this event
                            // Cosmos DB will reject creation of a new document with an existing id
                            Debug.Assert(se["client_id"] != null && se["client_ts"] != null && se["seq"] != null);

                            // The client can also generate a unique id for it, if it wishes
                            se["id"] = se["id"] ?? ComputeHash(se["client_id"], se["client_ts"], se["seq"]);

                            // Any newlines inside of values are automatically escaped.
                            string result = se.ToString(Formatting.None);
                            writer.WriteLine(result);

                            log.LogTrace(result);
                        }
                    }

                    // Choosing optimal because the expected data size isn't huge anyway
                    var compressedPayload = BufferUtils.GzipCompressToArray(payload, CompressionLevel.Optimal);

                    if (compressedPayload.Length > MAX_EH_PAYLOAD_SIZE)
                    {
                        return req.CreateErrorResponse(HttpStatusCode.RequestEntityTooLarge,
                            "The content after compression was larger than the max allowed Event Hub payload size.");
                    }

                    // Send payload on to event hub if not disabled
                    if (Config.EnableEventHub)
                    {
                        EventData batch = new EventData(compressedPayload);
                        batch.SetGzipEncoding();
                        batch.SetVersion(PAYLOAD_VERSION);

                        messages.Add(batch);
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Ingestion failed to send telemetry to event hub.\n" + ex.StackTrace);
                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Ingestion failed to send telemetry to event hub.");
                }

                return req.CreateResponse(HttpStatusCode.Accepted);
            }
        }

        /// <summary>
        /// Gets the content from the HttpContent, decompressing it if needed.
        /// </summary>
        /// <param name="httpContent"></param>
        /// <returns></returns>
        static async Task<string> GetContentAsStringAsync(HttpContent httpContent)
        {
            if (httpContent.Headers.ContentEncoding.Contains("gzip"))
            {
                var bodyStream = await httpContent.ReadAsStreamAsync();
                return await BufferUtils.GzipDecompressToString(bodyStream, Encoding.UTF8);
            }
            else
            {
                return await httpContent.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Computes a unique id from a set of json fields
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        static string ComputeHash(params JToken[] components)
        {
            StringBuilder sb = new StringBuilder();

            using (var ms = BufferUtils.GetStream())
            using (var sw = new StreamWriter(ms))
            {
                foreach (var c in components)
                {
                    sw.Write(c.Value<string>());
                }
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                byte[] result = _md5.ComputeHash(ms);

                for (int i = 0; i < 16; i++)
                {
                    sb.Append(result[i].ToString("x2"));
                }
            }
            return sb.ToString();
        }

        static IEnumerable<JObject> ParseContent(string content)
        {
            var batch = JsonConvert.DeserializeObject<SimpleEventBatch>(content);

            foreach (var ev in batch.Events)
            {
                ev.Merge(batch.Header, _mergeSettings);
            }
            return batch.Events;
        }
    }
}
