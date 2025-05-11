using IIQCollector;
using JsonTypes.Clients;
using System.Globalization;

namespace Endpoints
{

    public class GetCSVData
    {
        public static async Task<List<List<JsonTypes.GraphCSVFormat.NodeData>>> Get(string dataKey, string filters, bool forLNN, bool forPath)
        {

            HttpClient client = IIQCollector.Program.HTTPPrepare();
            List<List<JsonTypes.GraphCSVFormat.NodeData>> clusterReports = new List<List<JsonTypes.GraphCSVFormat.NodeData>>();

            foreach (JsonTypes.Cluster.Result cluster in IIQCollector.Program.ClusterList)
            {
                string httpEndpoint = String.Format("{0}/insightiq/rest/reporting/v1/timeseries/download_data?no_min_max=true&key={1}{2}&cluster={3}", IIQCollector.Program.IIQHostAdress, dataKey, filters, cluster.Guid);
                string result = IIQCollector.Program.HTTPSend(client, httpEndpoint, false).Result;
                try
                {
                    List<JsonTypes.GraphCSVFormat.NodeData> parsedRoot = ParseCSV(result, cluster.Guid, cluster.Name, forLNN, dataKey, forPath);
                    clusterReports.Add(parsedRoot);
                }
                catch (Exception e)
                {
                    if (Program.Debug)
                    {
                        Console.WriteLine("Error sending HTTP request");
                        Console.WriteLine(e.Message);
                    }

                    IIQCollector.Program.LogExceptionToFile(e);
                }
            }

            return clusterReports;
        }


        public static List<JsonTypes.GraphCSVFormat.NodeData> ParseCSV(string csv, string clusterGUID, string clusterName, bool forLNN, string dataKey, bool forPath)
        {
            var lines = csv.Trim().Split('\n');
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();

            // Check if the CSV contains error "ERROR: There is no data available at this time."
            if (headers.Any(h => h.Contains("ERROR: There is no data available at this time")))
            {
                Console.WriteLine("Error: CSV data contains an error message. No data available for cluster {0} on key {1}", clusterName, dataKey);
                return null;
            }

            List<JsonTypes.GraphCSVFormat.NodeData> nodeDataList = new List<JsonTypes.GraphCSVFormat.NodeData>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');

                if (values.Length < 2 || values.Length % 2 != 0)
                {
                    if (Program.Debug)
                    {
                        Console.WriteLine("Error: Row {0} has an incorrect number of columns for cluster {1} on key {2}", i, clusterName, dataKey);
                        continue;
                    }
                }

                double time;
                if (!double.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out time))
                {
                    if (Program.Debug)
                    {
                        Console.WriteLine("Error: Unable to parse time value at row {0} for cluster {1} on key {2}", i, clusterName, dataKey);
                        continue;
                    }
                }

                List<JsonTypes.GraphCSVFormat.NodeInfo> nodeValues = new List<JsonTypes.GraphCSVFormat.NodeInfo>();
                int nodeNumber = 0;
                string dataPath = "";

                for (int j = 1; j < values.Length; j += 2)
                {
                    if (forLNN)
                    {
                        string header = headers[j].Trim(); // Extract the header
                        if (!header.StartsWith("node:", StringComparison.OrdinalIgnoreCase))
                            continue;

                        string nodeNumberStr = header.Substring(5, header.IndexOf(" (") - 5); // Get the number part
                        if (!int.TryParse(nodeNumberStr, out nodeNumber))
                        {
                            if (Program.Debug)
                            {
                                Console.WriteLine("Error: Invalid node format at row {0}, column {1} for cluster {2} on key {3}", i, (j + 1), clusterName, dataKey);
                                continue;
                            }
                        }
                    }
                    if (forPath)
                    {
                        if (string.IsNullOrEmpty(values[j]))
                        {
                            break;
                        }
                        string header = headers[j].Trim(); // Extract the header
                        if (!header.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
                            continue;

                        dataPath = header.Substring(5, header.IndexOf(" (") - 5);

                    }
                    double nodePercent;
                    if (!double.TryParse(values[j], NumberStyles.Any, CultureInfo.InvariantCulture, out nodePercent))
                    {
                        if (Program.Debug)
                        {
                            Console.WriteLine("Error: Unable to parse node value for node {0} at row {1} for cluster {2} on key {3}", nodeNumber, i, clusterName, dataKey);
                        }
                        continue;
                    }

                    nodeValues.Add(new JsonTypes.GraphCSVFormat.NodeInfo { Node = nodeNumber, Data = nodePercent, Path = dataPath });
                }
                nodeValues = nodeValues.OrderBy(n => n.Node).ToList();
                nodeDataList.Add(new JsonTypes.GraphCSVFormat.NodeData { Time = time, Nodes = nodeValues, ClusterName = clusterName, ClusterGUID = clusterGUID });
            }

            return nodeDataList;
        }
    }
}
