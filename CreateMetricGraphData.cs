using Prometheus;

namespace IIQCompare
{
    public class CreateMetricGraphData
    {
        private string errorDataEmpty = "Error: Chartdata response is empty for {0}, cluster {1}";
        private static Dictionary<string, Gauge> _metrics = new Dictionary<string, Gauge>();

        // Method to dynamically create a metric if it doesn't already exist
        private static Gauge CreateOrGetGauge(string metricName, string helpText, string[] labelNames)
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

        public static async void Create(string metricName, string helpText, string[] labelNames)
        {
            var output = await Endpoints.GetGraphData.Get(metricName);
            string errorDataEmpty = "Error: Chartdata response is empty for {0}, cluster {1}";
            var gauge = CreateOrGetGauge(metricName, helpText, labelNames);
            // Update the metric values dynamically for each server
            foreach (var cluster in output)
            {
                if (cluster == null)
                {
                    continue;
                }
                if (cluster.ChartData[0].Data.Count > 0)
                {

                    var a = cluster.ChartData[0].Data.OrderBy(o => o.Date);
                    var closestDataPoint = FindClosest(cluster.ChartData[0].Data.OrderBy(o => o.Date).ToList(), Program.lastDateUsed);
                    if (closestDataPoint == null)
                    {
                        closestDataPoint = cluster.ChartData[0].Data.LastOrDefault();
                    }
                    //Console.WriteLine((double)closestDataPoint.Value);
                    gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set((double)closestDataPoint.Value);


                    // }
                }
                else
                {
                    Console.WriteLine(errorDataEmpty, cluster.ChartLabel, cluster.ClusterName);
                    gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Unpublish();
                }
            }
        }

        public static JsonTypes.GraphFormat.Datum FindClosest(List<JsonTypes.GraphFormat.Datum> data, long targetTime)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Date >= (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 600000))
                {
                    if (i - 1 < 0)
                    {
                        return data[0];
                    }
                    return data[i - 1];
                }
            }
            return data.LastOrDefault();
        }
    }
}