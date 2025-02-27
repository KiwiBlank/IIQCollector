using System.Text.Json.Serialization;

namespace JsonTypes.GraphCSVFormat
{

    public class NodeData
    {
        public double Time { get; set; }
        public List<NodeInfo> Nodes { get; set; }
        public string ClusterGUID { get; set; }
        public string ClusterName { get; set; }
    }

    public class NodeInfo
    {
        public int Node { get; set; }
        public double Data { get; set; }
    }
    [JsonSerializable(typeof(NodeData))]
    internal partial class SourceGeneratorContextv : JsonSerializerContext
    {
    }
}
