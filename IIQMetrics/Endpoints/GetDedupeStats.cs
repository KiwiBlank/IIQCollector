﻿using IIQCollector;
using JsonTypes.Clients;
using System.Text.Json;

namespace Endpoints
{
    class GetDedupeStats
    {
        public static async Task<List<JsonTypes.Dedupe.Format>> Get()
        {
            HttpClient client = IIQCollector.Program.HTTPPrepare();
            List<JsonTypes.Dedupe.Format> clusterReports = new List<JsonTypes.Dedupe.Format>();

            foreach (JsonTypes.Cluster.Result cluster in IIQCollector.Program.ClusterList)
            {
                string httpEndpoint = String.Format("{0}/insightiq/rest/reporting/v1/drr/{1}/overview", IIQCollector.Program.IIQHostAdress, cluster.Guid);
                string result = IIQCollector.Program.HTTPSend(client, httpEndpoint, false).Result;
                JsonTypes.Dedupe.Format parsedRoot = ParseJson(result, cluster.Guid, cluster.Name);
                clusterReports.Add(parsedRoot);
            }

            return clusterReports;
        }

        public static JsonTypes.Dedupe.Format ParseJson(string json, string clusterGUID, string clusterName)
        {
            JsonTypes.Dedupe.Format root = null;

            try
            {
                root = JsonSerializer.Deserialize<JsonTypes.Dedupe.Format>(json, JsonTypes.Dedupe.SourceGeneratorContextd.Default.Format);

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