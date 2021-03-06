﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace TelemetryAPI
{
    public class SimpleEvent
    {
        /// <summary>
        /// Unique id of a particular event record.  Used to de-duplicate records when inserting into the database.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Name of the event. 
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Version of the event.  Used by later consumers of the event to understand how to process the event.
        /// </summary>
        [JsonProperty("e_ver")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// UTC Timestamp of the ingestion.  This is more likely to be correct, but all events in the batch will share the same timestamp.
        /// </summary>
        [JsonProperty("ingest_ts")]
        public DateTime IngestTimestamp { get; set; }

        /// <summary>
        /// A unique id to represent a client.  Usually an advertising id, or an id generated on install of the application.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// UTC Timestamp generated by the client.  While usually accurate, it can be affected by clients which change their clocks
        /// </summary>
        [JsonProperty("client_ts")]
        public DateTime ClientTimestamp { get; set; }

        /// <summary>
        /// Session Id for the event.  Used to differentiate one play session from another on the same client device.
        /// </summary>
        [JsonProperty("session_id")]
        public string SessionId { get; set; }

        /// <summary>
        /// Sequence of the event.  All events from the client should be stamped with an incrementing value to indicate order.
        /// </summary>
        [JsonProperty("seq")]
        public uint Sequence { get; set; }

        /// <summary>
        /// This field will catch all fields that don't have a dedicated field.
        /// When serialized to json, any fields in this map will be parented at the root level.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> data { get; set; }
    }

    public class SimpleEventBatch
    {
        /// <summary>
        /// Any properties in the header are considered to be common to *all* events in the batch.
        /// On ingestion, these fields will be copied to each event in the batch.
        /// This enables saving of space in transmission from the client.
        /// </summary>
        [JsonProperty("header")]
        public JObject Header { get; set; }

        /// <summary>
        /// The invidivual events from the client.
        /// </summary>
        [JsonProperty("events")]
        public JObject[] Events { get; set; }
    }


    // NOTE: This is for example only!
    
    /// <summary>
    /// This class is not used, but is left as an example of what might go into the header object.
    /// In the SimpleEventBatch, it is stored as a JObject.
    /// </summary>
    public class SimpleEventHeader
    {
        /// <summary>
        /// Version of the header component
        /// </summary>
        [JsonProperty("h_ver")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// A unique id to represent a client.  Usually an advertising id, or an id generated on install of the application.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// Session Id for the event.  Used to differentiate one play session from another on the same client device.
        /// </summary>
        [JsonProperty("session_id")]
        public string SessionId { get; set; }
    }   
}
