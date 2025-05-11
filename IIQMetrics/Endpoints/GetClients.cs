using IIQCollector;
using JsonTypes.Clients;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Endpoints
{
    public class GetClients
    {
        public static async Task<List<JsonTypes.Clients.ClientStats>> Get()
        {
            HttpClient client = IIQCollector.Program.HTTPPrepare();
            List<JsonTypes.Clients.ClientStats> clusterClients = new List<JsonTypes.Clients.ClientStats>();

            foreach (JsonTypes.Cluster.Result cluster in IIQCollector.Program.ClusterList)
            {
                string httpEndpoint = String.Format("{0}/insightiq/rest/reporting/v1/timeseries/summary/clients?cluster={1}", IIQCollector.Program.IIQHostAdress, cluster.Guid);
                string result = IIQCollector.Program.HTTPSend(client, httpEndpoint, false).Result;
                ClientStats parsedRoot = ParseJson(result, cluster.Guid, cluster.Name);
                clusterClients.Add(parsedRoot);
            }
            return clusterClients;
        }

        public static JsonTypes.Clients.ClientStats ParseJson(string json, string clusterGUID, string clusterName)
        {
            JsonTypes.Clients.ClientStats root = null;

            try
            {
                root = JsonSerializer.Deserialize<JsonTypes.Clients.ClientStats>(json, SourceGeneratorContextz.Default.ClientStats);

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

                IIQCollector.Program.LogExceptionToFile(e);
            }

            return root;
        }
    }
}
