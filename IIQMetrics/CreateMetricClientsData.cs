using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IIQCollector
{
    public class CreateMetricClientsData
    {
        private string errorDataEmpty = "Error: clientdata response is empty for {0}, cluster {1}";
        public static async void Create(string metricName, string helpText, string[] labelNames, string valueType, List<JsonTypes.Clients.ClientStats> output)
        {
            var gauge = MetricsConfiguration.CreateOrGetGauge(metricName, helpText, labelNames);

            string errorDataEmpty = "Error: Event data response is empty for {0}, cluster {1}";

            // Update the metric values dynamically for each server
            foreach (var cluster in output)
            {
                if (cluster == null)
                {
                    continue;
                }
                if (cluster.Clients.Count > 0)
                {
                    foreach (var client in cluster.Clients)
                    {
                        //if (client.Protocols.Contains("smb2") || client.Protocols.Contains("nfs3") || client.Protocols.Contains("nfs4"))
                        //{
                            switch (valueType)
                            {
                                case "op_rate":
                                    gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName, client.ClientId).Set(client.OpRate);
                                    break;
                                case "write_rate":
                                    gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName, client.ClientId).Set(client.WriteRate);
                                    break;
                                case "read_rate":
                                    gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName, client.ClientId).Set(client.ReadRate);
                                    break;
                                case "byte_rate":
                                    gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName, client.ClientId).Set(client.ByteRate);
                                    break;
                                default:
                                    break;
                            }
                        //}
                    }
                }
            }
        }
    }
}
