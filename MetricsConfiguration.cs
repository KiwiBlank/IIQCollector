using Prometheus;
using System.Text.Json;

namespace IIQCompare
{
    public class MetricsConfiguration
    {
        public static bool CheckSessionResponse(string json)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("message", out _))
                {
                    return false;
                }

                if (root.TryGetProperty("username", out _))
                {
                    return true;
                }
                return false;
            }
        }

        public static async Task GetSession()
        {
            HttpClient client = IIQCompare.Program.HTTPPrepare();
            string httpEndpoint = String.Format("{0}/insightiq/rest/security-iam/v1/auth/session", IIQCompare.Program.IIQHostAdress);
            string result = IIQCompare.Program.HTTPSend(client, httpEndpoint, false).Result;
            if (!CheckSessionResponse(result))
            {
                //Session expired, reauth
                AuthIIQ.AuthenticateIIQ();
            }
        }

        public static void PrometheusClient()
        {
            using var server = new KestrelMetricServer(port: Program.ExporterWebPort);
            server.Start();
            var clusterUsedBytesMetric = Metrics.CreateGauge("cluster_usage_bytes", "Cluster used bytes",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });

            var clusterDedupeRatioMetric = Metrics.CreateGauge("cluster_dedupe_ratio", "Cluster dedupe ratio",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterTroughputMetric = Metrics.CreateGauge("cluster_ifs_troughput", "Cluster ifs throughput",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterCPUMetric = Metrics.CreateGauge("cluster_cpu_usage", "Cluster cpu usage",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterNetThroughputMetric = Metrics.CreateGauge("cluster_net_troughput", "Cluster net throughput",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterTotalBytesMetric = Metrics.CreateGauge("cluster_total_bytes", "Cluster total bytes",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterWriteableTotalMetric = Metrics.CreateGauge("cluster_writeable_bytes", "Cluster writeable bytes",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterCPULNNMetric = Metrics.CreateGauge("cluster_cpu_usage_lnn", "Cluster cpu lnn",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name", "lnn" }
                });
            var clusterNetThroughputLNNMetric = Metrics.CreateGauge("cluster_net_troughput_lnn", "Cluster net throughput lnn",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name", "lnn" }
                });
            var clusterSIQNetThroughputMetric = Metrics.CreateGauge("cluster_siq_troughput", "Cluster siq throughput",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            // Example array of server objects
            _ = Task.Run(async delegate
            {
                while (true)
                {
                    await GetSession();
                    try
                    {
                        List<JsonTypes.Dedupe.Format> clusterDedupe = await Endpoints.GetDedupeStats.GetAllClusterDedupe();

                        List<JsonTypes.GraphFormat.Root> clusterCapacityUsed = await Endpoints.GetGraphData.Get("cluster_used");
                        List<JsonTypes.GraphFormat.Root> clusterThroughput = await Endpoints.GetGraphData.Get("ifs_total_rate");
                        List<JsonTypes.GraphFormat.Root> clusterCPU = await Endpoints.GetGraphData.Get("cpu");
                        List<JsonTypes.GraphFormat.Root> clusterNetThroughput = await Endpoints.GetGraphData.Get("ext_net");
                        List<JsonTypes.GraphFormat.Root> clusterCapacityTotal = await Endpoints.GetGraphData.Get("cluster_total");
                        List<JsonTypes.GraphFormat.Root> clusterWriteableTotal = await Endpoints.GetGraphData.Get("cluster_writeable");
                        CreateMetricGraphData.Create(clusterCapacityUsed, clusterUsedBytesMetric);
                        CreateMetricGraphData.Create(clusterThroughput, clusterTroughputMetric);
                        CreateMetricGraphData.Create(clusterCPU, clusterCPUMetric);
                        CreateMetricGraphData.Create(clusterNetThroughput, clusterNetThroughputMetric);
                        CreateMetricGraphData.Create(clusterCapacityTotal, clusterTotalBytesMetric);
                        CreateMetricGraphData.Create(clusterWriteableTotal, clusterWriteableTotalMetric);

                        if (Program.GetCSV)
                        {
                            
                            List<List<JsonTypes.GraphCSVFormat.NodeData>> clusterCPULNN = await Endpoints.GetCSVData.Get(String.Format("cpu%7Clnn%7C{0}",Program.NumBreakouts), true);
                            List<List<JsonTypes.GraphCSVFormat.NodeData>> clusterNetThroughputLNN = await Endpoints.GetCSVData.Get(String.Format("ext_net%7Clnn%7C{0}", Program.NumBreakouts), true);
                            List<List<JsonTypes.GraphCSVFormat.NodeData>> clusterSIQNetThroughput = await Endpoints.GetCSVData.Get("ext_net&filter=proto_name:siq", false);
                            CreateMetricGraphCSVData.Create(clusterCPULNN, clusterCPULNNMetric, true);
                            CreateMetricGraphCSVData.Create(clusterNetThroughputLNN, clusterNetThroughputLNNMetric, true);
                            CreateMetricGraphCSVData.Create(clusterSIQNetThroughput, clusterSIQNetThroughputMetric, false);
                        }

                        foreach (var cluster in clusterDedupe)
                        {
                            if (cluster == null)
                            {
                                continue;
                            }
                            clusterDedupeRatioMetric.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(((double)cluster.DedupeRatio));
                        }
                        Console.WriteLine("Written data.");
                    }
                    catch (Exception e)
                    {
                        if (Program.Debug)
                        {
                            Console.WriteLine(e.Message);
                        }
                        Console.WriteLine("Error processing metrics");

                        IIQCompare.Program.LogExceptionToFile(e);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(Program.PollingRate));
                }
            });

            Console.WriteLine("Open localhost:{0}/metrics in a web browser.", Program.ExporterWebPort);
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}