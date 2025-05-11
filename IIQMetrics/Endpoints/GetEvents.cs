﻿using IIQCollector;
using JsonTypes.Clients;
using JsonTypes.EventFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Endpoints
{
    class GetEvents
    {
        public static async Task<List<JsonTypes.EventFormat.ActiveEvents>> Get()
        {
            HttpClient client = IIQCollector.Program.HTTPPrepare();
            List<JsonTypes.EventFormat.ActiveEvents> clusterReports = new List<JsonTypes.EventFormat.ActiveEvents>();

            foreach (JsonTypes.Cluster.Result cluster in IIQCollector.Program.ClusterList)
            {
                string httpEndpoint = String.Format("{0}/insightiq/rest/reporting/v1/timeseries/summary/active_events?cluster={1}", IIQCollector.Program.IIQHostAdress, cluster.Guid);
                string result = IIQCollector.Program.HTTPSend(client, httpEndpoint, false).Result;
                JsonTypes.EventFormat.ActiveEvents parsedRoot = ParseJson(result, cluster.Guid, cluster.Name);
                clusterReports.Add(parsedRoot);
            }

            return clusterReports;
        }

        public static JsonTypes.EventFormat.ActiveEvents ParseJson(string json, string clusterGUID, string clusterName)
        {
            JsonTypes.EventFormat.ActiveEvents root = null;

            try
            {
                root = JsonSerializer.Deserialize<JsonTypes.EventFormat.ActiveEvents>(json, SourceGeneratorContextt.Default.ActiveEvents);

                root.ClusterGUID = clusterGUID;
                root.ClusterName = clusterName;
            }
            catch (Exception e)
            {
                if (Program.Debug)
                {
                    Console.WriteLine("Error deserializing EventFormat for {0}", clusterName);
                    Console.WriteLine(e.Message);
                }

                IIQCollector.Program.LogExceptionToFile(e);
                Console.WriteLine("Error deserializing EventFormat for {0}", clusterName);

            }

            return root;
        }
    }
}
