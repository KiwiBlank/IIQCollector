using IIQCompare;
using System.Globalization;

namespace Endpoints
{

    public class GetCSVData
    {
        public static async Task<List<List<JsonTypes.GraphCSVFormat.NodeData>>> Get(string dataKey, bool forLNN)
        {

            HttpClient client = IIQCompare.Program.HTTPPrepare();
            List<List<JsonTypes.GraphCSVFormat.NodeData>> clusterReports = new List<List<JsonTypes.GraphCSVFormat.NodeData>>();

            foreach (JsonTypes.Cluster.Result cluster in IIQCompare.Program.ClusterList)
            {
                string httpEndpoint = String.Format("{0}/insightiq/rest/reporting/v1/timeseries/download_data?no_min_max=true&key={1}&cluster={2}", IIQCompare.Program.IIQHostAdress, dataKey, cluster.Guid);
                string result = IIQCompare.Program.HTTPSend(client, httpEndpoint, false).Result;
                try
                {
                    List<JsonTypes.GraphCSVFormat.NodeData> parsedRoot = ParseCSV(result, cluster.Guid, cluster.Name, forLNN);
                    clusterReports.Add(parsedRoot);
                }
                catch (Exception e)
                {
                    if (Program.Debug)
                    {
                        Console.WriteLine("Error sending HTTP request");
                        Console.WriteLine(e.Message);
                    }

                    IIQCompare.Program.LogExceptionToFile(e);
                }
            }

            return clusterReports;
        }


        public static List<JsonTypes.GraphCSVFormat.NodeData> ParseCSV(string csv, string clusterGUID, string clusterName, bool forLNN)
        {
            var lines = csv.Trim().Split('\n');
            var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();

            // Check if the CSV contains error "ERROR: There is no data available at this time."
            if (headers.Any(h => h.Contains("ERROR: There is no data available at this time")))
            {
                Console.WriteLine("Error: CSV data contains an error message. No data available for cluster {0}", clusterName);
                return null;
            }

            List<JsonTypes.GraphCSVFormat.NodeData> nodeDataList = new List<JsonTypes.GraphCSVFormat.NodeData>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');

                if (values.Length < 2 || values.Length % 2 != 0)
                {
                    Console.WriteLine("Error: Row {i} has an incorrect number of columns");
                    continue;
                }

                double time;
                if (!double.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out time))
                {
                    Console.WriteLine("Error: Unable to parse time value at row {i}");
                    continue;
                }

                List<JsonTypes.GraphCSVFormat.NodeInfo> nodeValues = new List<JsonTypes.GraphCSVFormat.NodeInfo>();
                int nodeNumber = 0;

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
                            Console.WriteLine("Error: Invalid node format at row {i}, column {j + 1}");
                            continue;
                        }
                    }
                    double nodePercent;
                    if (!double.TryParse(values[j], NumberStyles.Any, CultureInfo.InvariantCulture, out nodePercent))
                    {
                        Console.WriteLine("Error: Unable to parse node value for node {nodeNumber} at row {i}");
                        continue;
                    }

                    nodeValues.Add(new JsonTypes.GraphCSVFormat.NodeInfo { Node = nodeNumber, Data = nodePercent });
                }
                nodeValues = nodeValues.OrderBy(n => n.Node).ToList();
                nodeDataList.Add(new JsonTypes.GraphCSVFormat.NodeData { Time = time, Nodes = nodeValues, ClusterName = clusterName, ClusterGUID = clusterGUID });
            }

            return nodeDataList;
        }
    }
}
