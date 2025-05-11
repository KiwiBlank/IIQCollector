using System.Net;
using System.Text.Json;

namespace IIQCollector
{
    public class Program
    {
        public static string AuthCSRF;
        public static string AuthKey;
        public static HttpClient ClientPublic;
        public static List<JsonTypes.Cluster.Result> ClusterList;
        public static int ExporterWebPort;
        public static bool foundFirstDate = false;
        public static HttpClientHandler HandlerClient;
        public static string IIQHostAdress;
        public static string IIQPassword;
        public static string IIQUsername;
        public static long lastDateUsed;
        public static int PollingRate;
        public static long unixMinusFive;
        public static bool GetCSV;
        public static int GatherOffsetUnixMilliseconds;
        public static int GatherOffsetUnixSeconds;
        public static bool Debug;
        public static int NumBreakouts;
        public static int HTTPTimeoutMinutes;
        public static int RestartAfterCount;
        public static bool GetEvents;
        public static bool GetClients;
        public static bool GetHeat;

        public static HttpClient HTTPPrepare()
        {
            HttpClient httpClient = new HttpClient(HandlerClient);
            httpClient.Timeout = TimeSpan.FromMinutes(Program.HTTPTimeoutMinutes);

            var baseAddress = new Uri(IIQHostAdress);
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(baseAddress, new Cookie("TOK", "insightiq_auth={0}", AuthKey));
            httpClient.DefaultRequestHeaders.Add("x-csrf-token", AuthCSRF);
            return httpClient;
        }

        public static async Task<string> HTTPSend(HttpClient client, string httpEndpoint, bool checkStatusCode)
        {
            try
            {
                using HttpResponseMessage response = await client.GetAsync(httpEndpoint);
                if (checkStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                };
                string readJson = await response.Content.ReadAsStringAsync();
                return readJson;
            }
            catch (Exception e)
            {
                if (Program.Debug)
                {
                    Console.WriteLine("Error sending HTTP request");
                    Console.WriteLine(e.Message);
                }

                LogExceptionToFile(e);
            }
            return "";
        }

        public static void LogExceptionToFile(Exception ex)
        {
            // Path to the log file
            string logFilePath = "error_log.txt";

            if (File.Exists(logFilePath))
            {
                FileInfo fileInfo = new FileInfo(logFilePath);
                const long maxFileSize = 5L * 1024 * 1024 * 1024; // 5 GB in bytes

                if (fileInfo.Length > maxFileSize)
                {
                    // Delete the file if it exceeds 5GB
                    File.Delete(logFilePath);
                }
            }

            // Append the exception details to the log file
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine("Exception occurred at: " + DateTime.Now);
                writer.WriteLine("Exception Message: " + ex.Message);
                writer.WriteLine("Stack Trace: " + ex.StackTrace);
                writer.WriteLine("---------------------------------------------------");
            }

            // Optionally, also write to the console
            Console.WriteLine("Exception. Details have been written to: " + logFilePath);
        }

        private static async Task GetClusters()
        {
            HttpClient client = HTTPPrepare();
            HttpResponseMessage response = null;
            int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    string httpEndpoint = String.Format("{0}/insightiq/rest/clustermanager/v1/clusters", IIQHostAdress);
                    response = await client.GetAsync(httpEndpoint);
                    response.EnsureSuccessStatusCode();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error getting clusters from IIQ");
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine("Maximum retry attempts reached");
                        Console.WriteLine(e.Message);
                        LogExceptionToFile(e);
                        throw;
                    }
                    Console.WriteLine("Trying to get clusters again");
                    await Task.Delay(2000); // Wait before retrying
                }

            }

            List<JsonTypes.Cluster.Result> resultsList;
            string readJson = await response.Content.ReadAsStringAsync();
            try
            {
                JsonTypes.Cluster.Root root = JsonSerializer.Deserialize<JsonTypes.Cluster.Root>(readJson, JsonTypes.Cluster.SourceGeneratorContext.Default.Root);
                resultsList = root.Results;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting clusters from IIQ");
                Console.WriteLine(e.Message);
                LogExceptionToFile(e);
                throw;
            }
            if (resultsList.Count < 1)
            {
                Console.WriteLine("Error: No clusters found.");
                throw new InvalidOperationException("Error: No clusters found.");
            }
            ClusterList = resultsList;
        }

        private static async Task Main(string[] args)
        {
            Config.ConfigSettings();
            var httpClientHandler = new HttpClientHandler();
            HandlerClient = httpClientHandler;
            HandlerClient.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            //await Task.Delay(2000);
            await AuthIIQ.AuthenticateIIQ();
            await GetClusters();
            long currentUnixTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Subtract 5 minutes (5 minutes = 5 * 60 * 1000 milliseconds)
            //unixMinusFive = currentUnixTimeMillis - (2 * 60 * 1000);
            unixMinusFive = currentUnixTimeMillis;
            Program.lastDateUsed = currentUnixTimeMillis - Program.GatherOffsetUnixMilliseconds;
            MetricsConfiguration.PrometheusClient();
        }
    }
}