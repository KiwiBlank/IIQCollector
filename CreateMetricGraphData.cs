using Prometheus;

namespace IIQCompare
{
    public class CreateMetricGraphData
    {
        private string errorDataEmpty = "Error: Chartdata response is empty for {0}, cluster {1}";

        public static void Create(List<JsonTypes.GraphFormat.Root> output, Gauge gauge)
        {
            string errorDataEmpty = "Error: Chartdata response is empty for {0}, cluster {1}";

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