using JsonTypes.GraphCSVFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonTypes.EventFormat
{
    public class Event
    {
        [JsonPropertyName("severity")]
        public int Severity { get; set; }

        [JsonPropertyName("start_time")]
        public long StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public long EndTime { get; set; }

        [JsonPropertyName("event_id")]
        public string EventId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("instance_id")]
        public string InstanceId { get; set; }
    }

    public class ActiveEvents
    {
        [JsonPropertyName("active_events")]
        public List<Event> ActiveEventsList { get; set; }
        public string ClusterGUID { get; set; }

        public string ClusterName { get; set; }
    }
    [JsonSerializable(typeof(ActiveEvents))]
    internal partial class SourceGeneratorContextt : JsonSerializerContext
    {
    }
}
