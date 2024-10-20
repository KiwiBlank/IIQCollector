using System.Text.Json.Serialization;

namespace JsonTypes.Dedupe
{
    public class Format
    {
        public string ClusterGUID { get; set; }

        public string ClusterName { get; set; }

        [JsonPropertyName("compression_ratio")]
        public decimal CompressionRatio { get; set; }

        [JsonPropertyName("data_reduction_logical")]
        public long DataReductionLogical { get; set; }

        [JsonPropertyName("data_reduction_ratio")]
        public decimal DataReductionRatio { get; set; }

        [JsonPropertyName("data_reduction_reduced")]
        public long DataReductionReduced { get; set; }

        [JsonPropertyName("dedupe_ratio")]
        public decimal DedupeRatio { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("errors")]
        public object Errors { get; set; }

        [JsonPropertyName("ifs_bytes_avail")]
        public long IfsBytesAvail { get; set; }

        [JsonPropertyName("ifs_bytes_free")]
        public long IfsBytesFree { get; set; }

        [JsonPropertyName("ifs_bytes_total")]
        public long IfsBytesTotal { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("vhs")]
        public long Vhs { get; set; }
    }

    [JsonSerializable(typeof(Format))]
    internal partial class SourceGeneratorContextd : JsonSerializerContext
    {
    }
}