using Azure.Messaging.EventHubs;

namespace TelemetryAPI.Utility
{
    public static class EventHubExtensions
    {
        const string VERSION = "version";
        const string CONTENT_ENCODING = "content-encoding";
        const string GZIP = "gzip";

        /// <summary>
        /// Reads the EventData header to see if the data is compressed
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool IsGzipCompressed(this EventData data)
        {
            if (data.Properties.TryGetValue(CONTENT_ENCODING, out object value))
            {
                string encoding = (value as string) ?? "";
                return encoding == GZIP;
            }

            return false;
        }

        /// <summary>
        /// Adds a header to the Event Data that indicates that we compressed the payload
        /// </summary>
        /// <param name="data"></param>
        public static void SetGzipEncoding(this EventData data)
        {
            data.Properties.Add(CONTENT_ENCODING, GZIP);
        }

        /// <summary>
        /// Adds a header to the Event Data that holds the version of our payload
        /// </summary>
        /// <param name="data"></param>
        /// <param name="version"></param>
        public static void SetVersion(this EventData data, string version)
        {
            data.Properties.Add(VERSION, version);
        }
    }
}
