using Prometheus;

namespace IIQCollector
{
    public class CreateMetricEventData
    {
        private string errorDataEmpty = "Error: Event data response is empty for {0}, cluster {1}";
        public static async void Create(string metricName, string helpText, string[] labelNames)
        {
            var output = await Endpoints.GetEvents.Get();
            var gauge = MetricsConfiguration.CreateOrGetGauge(metricName, helpText, labelNames);

            string errorDataEmpty = "Error: Event data response is empty for {0}, cluster {1}";

            // Update the metric values dynamically for each server
            foreach (var cluster in output)
            {
                if (cluster == null)
                {
                    continue;
                }
                if (cluster.ActiveEventsList.Count > 0)
                {
                    double severity1Count = cluster.ActiveEventsList.Count(e => e.Severity == 1);
                    double severity2Count = cluster.ActiveEventsList.Count(e => e.Severity == 2);
                    double severity3Count = cluster.ActiveEventsList.Count(e => e.Severity == 3);
                    double driveFailureCount = cluster.ActiveEventsList.Count(e => e.Message.Contains("One or more drives"));
                    if (metricName.Contains("one"))
                    {
                        gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(severity1Count);
                    }
                    if (metricName.Contains("two"))
                    {
                        gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(severity2Count);
                    }
                    if (metricName.Contains("three"))
                    {
                        gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(severity3Count);
                    }
                    if (metricName.Contains("broken"))
                    {
                        gauge.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(driveFailureCount);
                    }
                }
            }
        }
    }
}