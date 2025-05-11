using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonTypes.Clients
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class ClientStats
    {
        [JsonPropertyName("clients")]
        public List<Client> Clients { get; set; }
        public string ClusterGUID { get; set; }

        public string ClusterName { get; set; }
    }
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class Client
    {
        [JsonPropertyName("byte_rate")]
        public double ByteRate { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("node_ids")]
        public List<NodeId> NodeIds { get; set; }

        [JsonPropertyName("op_rate")]
        public double OpRate { get; set; }

        [JsonPropertyName("protocols")]
        public List<string> Protocols { get; set; }

        [JsonPropertyName("read_rate")]
        public double ReadRate { get; set; }

        [JsonPropertyName("write_rate")]
        public double WriteRate { get; set; }
    }
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class NodeId
    {
        [JsonPropertyName("devid")]
        public int DevId { get; set; }

        [JsonPropertyName("lnn")]
        public int Lnn { get; set; }
    }
    [JsonSerializable(typeof(ClientStats))]
    internal partial class SourceGeneratorContextz : JsonSerializerContext
    {
    }
}
