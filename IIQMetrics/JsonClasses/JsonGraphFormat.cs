using System.Text.Json.Serialization;

namespace JsonTypes.GraphFormat
{
    public class ChartFormat
    {
        [JsonPropertyName("breakouts_supported")]
        [JsonRequired]
        public string BreakoutsSupported { get; set; }

        [JsonPropertyName("cluster")]
        [JsonRequired]
        public string Cluster { get; set; }

        [JsonPropertyName("data")]
        [JsonRequired]
        public List<Datum> Data { get; set; }

        [JsonPropertyName("key")]
        [JsonRequired]
        public string Key { get; set; }

        [JsonPropertyName("label")]
        [JsonRequired]
        public string Label { get; set; }

        [JsonPropertyName("message")]
        [JsonRequired]
        public string Message { get; set; }

        [JsonPropertyName("supported")]
        [JsonRequired]
        public string Supported { get; set; }
    }

    public class Datum
    {
        [JsonPropertyName("date")]
        [JsonRequired]
        public long Date { get; set; }

        [JsonPropertyName("value")]
        [JsonRequired]
        public double Value { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("applicable_filters")]
        [JsonRequired]
        public List<string> ApplicableFilters { get; set; }

        [JsonPropertyName("applied_filters")]
        [JsonRequired]
        public List<object> AppliedFilters { get; set; }

        [JsonPropertyName("chart_data")]
        [JsonRequired]
        public List<ChartFormat> ChartData { get; set; }

        [JsonPropertyName("chart_label")]
        [JsonRequired]
        public string ChartLabel { get; set; }

        public string ClusterGUID { get; set; }

        public string ClusterName { get; set; }

        [JsonPropertyName("data_errors")]
        [JsonRequired]
        public List<object> DataErrors { get; set; }

        [JsonPropertyName("end_time")]
        [JsonRequired]
        public long EndTime { get; set; }

        [JsonPropertyName("errors")]
        [JsonRequired]
        public List<object> Errors { get; set; }
        [JsonPropertyName("formatter")]
        [JsonRequired]
        public string Formatter { get; set; }
        [JsonPropertyName("include_min_max")]
        [JsonRequired]
        public bool IncludeMinMax { get; set; }

        [JsonPropertyName("key")]
        [JsonRequired]
        public string Key { get; set; }

        [JsonPropertyName("label")]
        [JsonRequired]
        public string Label { get; set; }

        [JsonPropertyName("legend")]
        [JsonRequired]
        public object Legend { get; set; }

        [JsonPropertyName("legend_labels")]
        [JsonRequired]
        public object LegendLabels { get; set; }

        [JsonPropertyName("rejected_filters")]
        [JsonRequired]
        public List<object> RejectedFilters { get; set; }

        [JsonPropertyName("sample_rate")]
        [JsonRequired]
        public int SampleRate { get; set; }

        [JsonPropertyName("start_time")]
        [JsonRequired]
        public long StartTime { get; set; }
    }

    [JsonSerializable(typeof(Root))]
    internal partial class SourceGeneratorContextf : JsonSerializerContext
    {
    }
}