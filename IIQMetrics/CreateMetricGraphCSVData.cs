﻿using Prometheus;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace IIQCollector
{
    public class CreateMetricGraphCSVData
    {
    public static async void Create(string metricName, string helpText, string[] labelNames, bool forLNN, string filters, bool forPath)
        {
           // _ = Task.Run(async delegate
           // {
            var output = await Endpoints.GetCSVData.Get(Regex.Replace(metricName, "__.*", ""), filters, forLNN, forPath);
            var gauge = MetricsConfiguration.CreateOrGetGauge(metricName, helpText, labelNames);

            foreach (var cluster in output)
            {
                if (cluster == null)
                {
                    continue;
                }
                if (cluster.Count > 0)
                {

                    var a = cluster.OrderBy(o => o.Time);
                    var closestDataPoint = FindClosest(cluster.OrderBy(o => o.Time).ToList(), Program.lastDateUsed / 1000);
                    if (closestDataPoint == null)
                    {
                        Console.WriteLine("CSV error");
                    }
                    if (forLNN)
                    {
                        foreach (var item in closestDataPoint.Nodes)
                        {
                            gauge.WithLabels(closestDataPoint.ClusterGUID, closestDataPoint.ClusterName, item.Node.ToString()).Set((double)item.Data);
                        }
                    }
                    if (!forLNN && !forPath)
                    {
                        foreach (var item in closestDataPoint.Nodes)
                        {
                            gauge.WithLabels(closestDataPoint.ClusterGUID, closestDataPoint.ClusterName).Set((double)item.Data);
                        }
                    }
                    if (forPath)
                    {
                        foreach (var item in closestDataPoint.Nodes)
                        {
                            gauge.WithLabels(closestDataPoint.ClusterGUID, closestDataPoint.ClusterName, item.Path).Set((double)item.Data);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("CSV Empty for metric {0}", metricName);
                }
            }
           //});
        }

        public static JsonTypes.GraphCSVFormat.NodeData FindClosest(List<JsonTypes.GraphCSVFormat.NodeData> data, long targetTime)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Time >= (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Program.GatherOffsetUnixSeconds))
                {
                    if (i - 1 < 0)
                    {
                        return data[0];
                    }
                    return data[i];
                }
            }
            return data.LastOrDefault();
        }
    }
}
