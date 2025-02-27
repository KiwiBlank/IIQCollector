using IIQCompare;
using JsonTypes.Dedupe;
using System.Text.Json;

namespace Endpoints
{
    class GetDedupeStats
    {
        public static async Task<List<JsonTypes.Dedupe.Format>> Get()
        {
            HttpClient client = IIQCompare.Program.HTTPPrepare();
            List<JsonTypes.Dedupe.Format> clusterReports = new List<JsonTypes.Dedupe.Format>();

            foreach (JsonTypes.Cluster.Result cluster in IIQCompare.Program.ClusterList)
            {
                string httpEndpoint = String.Format("{0}/insightiq/rest/reporting/v1/drr/{1}/overview", IIQCompare.Program.IIQHostAdress, cluster.Guid);
                string result = IIQCompare.Program.HTTPSend(client, httpEndpoint, false).Result;
                Format parsedRoot = ParseJson(result, cluster.Guid, cluster.Name);
                clusterReports.Add(parsedRoot);
            }

            return clusterReports;
        }

        public static JsonTypes.Dedupe.Format ParseJson(string json, string clusterGUID, string clusterName)
        {
            JsonTypes.Dedupe.Format root = null;

            try
            {
                root = JsonSerializer.Deserialize<JsonTypes.Dedupe.Format>(json, SourceGeneratorContextd.Default.Format);

                root.ClusterGUID = clusterGUID;
                root.ClusterName = clusterName;
            }
            catch (Exception e)
            {
                if (Program.Debug)
                {
                    Console.WriteLine("Error sending HTTP request");
                    Console.WriteLine(e.Message);
                }

                IIQCompare.Program.LogExceptionToFile(e);
            }

            return root;
        }
    }
}