using JsonTypes.GraphFormat;
using System.Text.Json;

namespace Endpoints
{
    public class GetGraphData
    {
        public static async Task<List<JsonTypes.GraphFormat.Root>> Get(string dataKey)
        {
            HttpClient client = IIQCompare.Program.HTTPPrepare();
            List<JsonTypes.GraphFormat.Root> clusterReports = new List<JsonTypes.GraphFormat.Root>();

            foreach (JsonTypes.Cluster.Result cluster in IIQCompare.Program.ClusterList)
            {
                string httpEndpoint = String.Format("{0}/insightiq/rest/reporting/v1/timeseries/graph_data?no_min_max=true&key={1}&cluster={2}", IIQCompare.Program.IIQHostAdress, dataKey, cluster.Guid);
                string result = IIQCompare.Program.HTTPSend(client, httpEndpoint, false).Result;

                JsonTypes.GraphFormat.Root parsedRoot = ParseJson(result, cluster.Guid, cluster.Name);
                clusterReports.Add(parsedRoot);
            }

            return clusterReports;
        }

        public static bool IsChartDataEmty(JsonElement element)
        {
            if (element.TryGetProperty("chart_data", out JsonElement chartElement) && chartElement.ValueKind == JsonValueKind.Array)
            {
                return !chartElement.EnumerateArray().Any();
            }

            return false;
        }

        public static JsonTypes.GraphFormat.Root ParseJson(string json, string clusterGUID, string clusterName)
        {
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement rootElement = document.RootElement;

                // Recursively check for null values in the JSON structure
                bool hasNullValues = IsChartDataEmty(rootElement);

                if (hasNullValues)
                {
                    Console.WriteLine("chart_data in json structure is empty. Cluster not initialized?");
                }
            }

            JsonTypes.GraphFormat.Root root = null;
            try
            {
                root = JsonSerializer.Deserialize<JsonTypes.GraphFormat.Root>(json, SourceGeneratorContextf.Default.Root);

                root.ClusterGUID = clusterGUID;
                root.ClusterName = clusterName;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing json");
                Console.WriteLine(e);
                IIQCompare.Program.LogExceptionToFile(e);
            }

            return root;
        }
    }
}