using System;

namespace TelemetryAPI
{
    public class Config
    {
        /// <summary>
        /// The name of the field in the configuration that contains the Event Hub connection string.
        /// Used by the Event Hub Trigger Attribute.
        /// </summary>
        public const string EventHubConnectionStringConfigField = "EhConnString";

        /// <summary>
        /// The name of the field in the configuration that contains the Event Hub connection string.
        /// Used by the Event Hub Trigger Attribute.
        /// </summary>
        public const string CosmosDbConnectionStringConfigField = "CosmosDbConnectionString";

        /// <summary>
        /// The config field, formatted as an environment variable, that contains the name of the consumer group.
        /// Used by the Event Hub Trigger Attribute.
        /// </summary>
        public const string ConsumerGroupEnvironmentVariable = "%ConsumerGroup%";

        /// <summary>
        /// The config field, formatted as an environment variable, that contains the name of the consumer group.
        /// Used by the Event Hub Trigger Attribute.
        /// </summary>
        public const string CosmosDbIdEnvironmentVariable = "%CosmosDatabaseId%";

        /// <summary>
        /// The config field, formatted as an environment variable, that contains the name of the consumer group.
        /// Used by the Event Hub Trigger Attribute.
        /// </summary>
        public const string CosmosDbCollectionEnvironmentVariable = "%CosmosDataCollection%";

        /// <summary>
        /// Uri for access to Cosmos DB ( https://<accountid>.documents.azure.com:443/ )
        /// </summary>
        public static string CosmosDbUri => _Config.Value._cosmosDbUri;

        /// <summary>
        /// Base64 encoded account key string used to access Cosmos DB
        /// </summary>
        public static string CosmosDbAuthKey => _Config.Value._cosmosDbAuthKey;

        /// <summary>
        /// Cosmos DB Database Id that contains the collection telemetry will be ingested to
        /// </summary>
        public static string CosmosDbId => _Config.Value._cosmosDbId;

        /// <summary>
        /// Cosmos DB Collection name that telemetry will be ingested to
        /// </summary>
        public static string CosmosDbCollection => _Config.Value._cosmosDbCollection;

        /// <summary>
        /// Config value to enable or disable pushing data from event hub to Cosmos DB
        /// </summary>
        public static bool EnableCosmos => _Config.Value._enableCosmos;

        /// <summary>
        /// Config value to enable or disable pushing data from the ingest api to Event Hub
        /// </summary>
        public static bool EnableEventHub => _Config.Value._enableEventHub;

        private static Lazy<Config> _Config = new Lazy<Config>(true);
        private string _cosmosDbUri;
        private string _cosmosDbAuthKey;
        private string _cosmosDbId;
        private bool _enableEventHub;
        private bool _enableCosmos;
        private string _cosmosDbCollection;
        private string _consumerGroup;

        public Config()
        {
            _cosmosDbUri = Environment.GetEnvironmentVariable("CosmosDbUri");
            _cosmosDbAuthKey = Environment.GetEnvironmentVariable("CosmosDbAuthKey");
            _cosmosDbId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
            _enableEventHub = Boolean.Parse((Environment.GetEnvironmentVariable("EventHubIngest") ?? "true"));
            _enableCosmos = Boolean.Parse((Environment.GetEnvironmentVariable("CosmosUpload") ?? "true"));
            _cosmosDbCollection = Environment.GetEnvironmentVariable("CosmosDataCollection");
            _consumerGroup = Environment.GetEnvironmentVariable("ConsumerGroup");
        }
    }
}
