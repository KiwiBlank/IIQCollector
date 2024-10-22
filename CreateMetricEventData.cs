using Prometheus;

namespace IIQCompare
{
    public class CreateMetricEventData
    {
        private string errorDataEmpty = "Error: Event data response is empty for {0}, cluster {1}";

        public static void Create(List<JsonTypes.EventFormat.ActiveEvents> output, Gauge gaugeSev1, Gauge gaugeSev2, Gauge gaugeSev3, Gauge gaugeDiskBroken)
        {
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
                    //Console.WriteLine((double)closestDataPoint.Value);
                    gaugeSev1.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(severity1Count);
                    gaugeSev2.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(severity2Count);
                    gaugeSev3.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(severity3Count);
                    gaugeDiskBroken.WithLabels(cluster.ClusterGUID, cluster.ClusterName).Set(driveFailureCount);


                    // }
                }
            }
        }
    }
}