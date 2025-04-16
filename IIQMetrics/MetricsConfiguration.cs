﻿using Prometheus;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;

namespace IIQCompare
{
    public class MetricsConfiguration
    {
        public static bool CheckSessionResponse(string json)
        {
            if (Program.Debug)
            {
                Console.WriteLine("Writing session response:{0}", json);
                Program.LogExceptionToFile(new Exception(String.Format("Writing session response after expiry:{0}", json)));
            } else
            {
                Console.WriteLine("Checking session status");
            }

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                //if (root.TryGetProperty("message", out _))
                //{
                //    return false;
                //}

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
                Console.WriteLine("Session detected as expired");
                Program.LogExceptionToFile(new Exception("Session detected as expired"));
                //Session expired, reauth
                await AuthIIQ.AuthenticateIIQ();
            }
        }

        public static void PrometheusClient()
        {
            using var server = new KestrelMetricServer(port: Program.ExporterWebPort);
            server.Start();

            var clusterDedupeRatioMetric = Metrics.CreateGauge("cluster_dedupe_ratio", "Cluster dedupe ratio",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterNodeCountMetric = Metrics.CreateGauge("cluster_node_count", "Cluster node count",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name" }
                });
            var clusterVersionMetric = Metrics.CreateGauge("cluster_version", "Cluster version",
                new GaugeConfiguration
                {
                    LabelNames = new[] { "cluster_guid", "cluster_name", "version" }
                });

            double itCounter = 0;
            // Example array of server objects
            _ = Task.Run(async delegate
            {
                while (true)
                {
                    await GetSession();
                    try
                    {
                        itCounter++;
                        if (itCounter > Program.RestartAfterCount && !Program.Debug)
                        {
                            Console.WriteLine("Restarting because of counter");
                            Program.LogExceptionToFile(new Exception("Restarting because of counter"));
                            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                            Environment.Exit(0);
                        }

                        if (itCounter % 10 == 0 || itCounter == 1)
                        {
                            if (Program.GetEvents)
                            {
                                List<JsonTypes.EventFormat.ActiveEvents> clusterEvents = await Endpoints.GetEvents.Get();
                                CreateMetricEventData.Create("cluster_event_sev_one", "Cluster sev 1 events", new[] { "cluster_guid", "cluster_name" });
                                CreateMetricEventData.Create("cluster_event_sev_two", "Cluster sev 2 events", new[] { "cluster_guid", "cluster_name" });
                                CreateMetricEventData.Create("cluster_event_sev_three", "Cluster sev 3 events", new[] { "cluster_guid", "cluster_name" });
                                CreateMetricEventData.Create("cluster_drive_broken", "Cluster drive broken events", new[] { "cluster_guid", "cluster_name" });
                                //CreateMetricGraphData.Create("active_job", "Cluster active jobs", new[] { "cluster_guid", "cluster_name" });
                            }
                        }

                        CreateMetricGraphData.Create("cluster_used", "Cluster used bytes", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("ifs_total_rate", "Cluster IFS throughput rate", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("cpu", "Cluster cpu usage", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("ext_net", "Cluster net throughput rate", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("cluster_total", "Cluster total bytes", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("cluster_writeable", "Cluster writeable bytes", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("active", "Cluster active clients", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("connected", "Cluster connected clients", new[] { "cluster_guid", "cluster_name" });
                        //CreateMetricGraphData.Create("pp_latency_read", "Cluster workload latency read", new[] { "cluster_guid", "cluster_name" });
                        //CreateMetricGraphData.Create("pp_latency_write", "Cluster workload latency write", new[] { "cluster_guid", "cluster_name" });
                        //CreateMetricGraphData.Create("ext_latency", "Cluster protocol latency", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("op_rate", "Cluster operation rate", new[] { "cluster_guid", "cluster_name" });
                        CreateMetricGraphData.Create("ext_errors", "Cluster net errors rate", new[] { "cluster_guid", "cluster_name" });



                        if (Program.GetCSV)
                        {
                            CreateMetricGraphCSVData.Create("cpu__2", "Cluster cpu usage lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));
                            CreateMetricGraphCSVData.Create("ext_net__2", "Cluster ext net lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));
                            CreateMetricGraphCSVData.Create("ext_net__3", "Cluster SIQ ext net", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=proto_name:siq&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("ext_net__4", "Cluster SMB ext net", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=proto_name:smb2&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("ext_net__5", "Cluster SIQ ext net out", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=proto_name:siq&filter=direction:out&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("op_rate__2", "Cluster SMB Op rate", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=proto_name:smb2&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("ext_latency", "Cluster SMB Latency", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=proto_name:smb2&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("ext_latency__2", "Cluster SIQ Latency out", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=proto_name:siq&filter=direction:out&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("ext_latency__3", "Cluster SIQ Latency in", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=proto_name:siq&filter=direction:in&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("cluster_used__2", "Cluster used bytes lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));
                            CreateMetricGraphCSVData.Create("cluster_total__2", "Cluster total bytes lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));
                            CreateMetricGraphCSVData.Create("cluster_writeable__2", "Cluster writeable bytes lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));
                            CreateMetricGraphCSVData.Create("active_job", "Cluster active flexprotect jobs", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=job_name:FlexProtect&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("active_job__2", "Cluster active flexprotectlin jobs", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=job_name:FlexProtectLin&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("active_job__3", "Cluster active collect jobs", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=job_name:Collect&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("active_job__4", "Cluster active smartpools jobs", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=job_name:SmartPools&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("active_job__5", "Cluster active autobalance jobs", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=job_name:AutoBalance&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("active_job__5", "Cluster active autobalancelin jobs", new[] { "cluster_guid", "cluster_name" }, false, String.Format("&filter=job_name:AutoBalanceLin&no_min_max=true"));
                            CreateMetricGraphCSVData.Create("disk_adv_busy", "Cluster disk activity busy lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));
                            CreateMetricGraphCSVData.Create("disk_adv_access_slow", "Cluster disk slow lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));
                            CreateMetricGraphCSVData.Create("disk_adv_access_latency", "Cluster disk latency lnn", new[] { "cluster_guid", "cluster_name", "lnn" }, true, String.Format("%7Clnn%7C{0}&no_min_max=true", Program.NumBreakouts));

                        }

                        foreach (var cluster in await Endpoints.GetDedupeStats.Get())
                        {
                            if (cluster == null)
                            {
                                continue;
                            }
                            clusterDedupeRatioMetric.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(((double)cluster.DedupeRatio));
                        }

                        foreach (var cluster in Program.ClusterList)
                        {
                            if (cluster == null)
                            {
                                continue;
                            }
                            clusterNodeCountMetric.WithLabels(cluster.Guid, cluster.Name).Set(((double)cluster.NodeCount));
                            //clusterVersionMetric.WithLabels(cluster.Guid, cluster.Name).Set(double.Parse(cluster.Version.Split('.')[0] + "." + cluster.Version.Split('.')[1], CultureInfo.InvariantCulture));
                            clusterVersionMetric.WithLabels(cluster.Guid, cluster.Name, cluster.Version).Set(1);

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
        public static Dictionary<string, Gauge> _metrics = new Dictionary<string, Gauge>();
        // Method to dynamically create a metric if it doesn't already exist
        public static Gauge CreateOrGetGauge(string metricName, string helpText, string[] labelNames)
        {
            if (!_metrics.TryGetValue(metricName, out var metric))
            {
                // If the metric doesn't exist, create it
                metric = Metrics.CreateGauge(metricName, helpText, new GaugeConfiguration
                {
                    LabelNames = labelNames
                });

                // Add it to the dictionary
                _metrics.Add(metricName, metric);
            }
            return metric;
        }
    }
}