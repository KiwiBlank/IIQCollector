using Prometheus;

namespace IIQCompare
{
    public class CreateMetricGraphCSVData
    {
        private static Dictionary<string, Gauge> _metrics = new Dictionary<string, Gauge>();

        // Method to dynamically create a metric if it doesn't already exist
        private static Gauge CreateOrGetGauge(string metricName, string helpText, string[] labelNames)
        {
            if (!_metrics.TryGetValue(helpText, out var metric))
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
        public static async void Create(string metricName, string helpText, string[] labelNames, bool forLNN, string filters)
        { // &filter=proto_name:smb2
            var output = await Endpoints.GetCSVData.Get(metricName, filters, forLNN);
            var gauge = CreateOrGetGauge(metricName, helpText, labelNames);

            // Update the metric values dynamically for each server
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
                            //Console.WriteLine((double)item.Data);
                            gauge.WithLabels(closestDataPoint.ClusterGUID, closestDataPoint.ClusterName, item.Node.ToString()).Set((double)item.Data);
                        }
                    }
                    else
                    {
                        foreach (var item in closestDataPoint.Nodes)
                        {
                            //Console.WriteLine((double)item.Data);
                            gauge.WithLabels(closestDataPoint.ClusterGUID, closestDataPoint.ClusterName).Set((double)item.Data);
                        }
                    }



                }
                else
                {
                    Console.WriteLine("CSV Empty");
                }
            }
        }

        public static JsonTypes.GraphCSVFormat.NodeData FindClosest(List<JsonTypes.GraphCSVFormat.NodeData> data, long targetTime)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Time >= (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600))
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
