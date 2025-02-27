using System.Text.Json.Serialization;

namespace JsonTypes.Cluster
{
    public class Monitoring
    {
        [JsonPropertyName("data size")]
        public long DataSize { get; set; }

        [JsonPropertyName("error_type")]
        public string ErrorType { get; set; }

        [JsonPropertyName("fsa_status")]
        public bool FsaStatus { get; set; }

        [JsonPropertyName("last_collection_time")]
        public long LastCollectionTime { get; set; }
        [JsonPropertyName("monitoring_status")]
        public string MonitoringStatus { get; set; }
        [JsonPropertyName("warning_type")]
        public string WarningType { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class Result
    {
        [JsonPropertyName("cluster_time")]
        public long ClusterTime { get; set; }

        [JsonPropertyName("dedupe_available")]
        public int DedupeAvailable { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("monitoring")]
        public Monitoring Monitoring { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("node_count")]
        public int NodeCount { get; set; }

        [JsonPropertyName("quotas_available")]
        public bool QuotasAvailable { get; set; }

        [JsonPropertyName("requested_state")]
        public string RequestedState { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("timezone")]
        public Timezone Timezone { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class Root
    {
        [JsonPropertyName("results")]
        public List<Result> Results { get; set; }
        public string ClusterGUID { get; set; }

        public string ClusterName { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class Timezone
    {
        [JsonPropertyName("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("custom")]
        public string Custom { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }

    [JsonSerializable(typeof(Root))]
    internal partial class SourceGeneratorContext : JsonSerializerContext
    {
    }
}