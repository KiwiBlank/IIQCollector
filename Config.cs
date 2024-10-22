using System.Text.Json;
using System.Text.Json.Serialization;

namespace IIQCompare
{
    public class Config
    {

        public static void ConfigSettings()
        {
            string configFilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(configFilePath))
            {
                string defaultConfigContent = JsonSerializer.Serialize(new ConfigOptions(), SourceGeneratorContexti.Default.ConfigOptions);

                File.WriteAllText(configFilePath, defaultConfigContent);
                Console.WriteLine($"Created default config file at: {configFilePath}");
            }
            string jsonString = File.ReadAllText(configFilePath);
            ConfigOptions appSettings = JsonSerializer.Deserialize<ConfigOptions>(jsonString, SourceGeneratorContexti.Default.ConfigOptions);
            Program.IIQHostAdress = String.Format("{0}://{1}:{2}", appSettings.Protocol, appSettings.IIQHost, appSettings.IIQPort);
            Program.ExporterWebPort = appSettings.ExporterWebPort;
            Program.IIQUsername = appSettings.Username;
            Program.IIQPassword = appSettings.Password;
            Program.PollingRate = appSettings.PollingRateSeconds;
            Program.GetCSV = appSettings.GetCSV;
            Program.GatherOffsetUnixMilliseconds = appSettings.GatherOffsetUnixMilliseconds;
            Program.GatherOffsetUnixSeconds = appSettings.GatherOffsetUnixSeconds;
            Program.Debug = appSettings.Debug;

            //Program.PollingRate = 50;
        }
    }

    public class ConfigOptions
    {
        public int ExporterWebPort { get; set; } = 1234;
        public string IIQHost { get; set; } = "192.168.125.50";
        public int IIQPort { get; set; } = 8000;
        public string Username { get; set; } = "user";
        public string Password { get; set; } = "pass";
        public int PollingRateSeconds { get; set; } = 10;
        public string Protocol { get; set; } = "https";
        public bool GetCSV { get; set; } = true;
        public int GatherOffsetUnixSeconds { get; set; } = 600;
        public int GatherOffsetUnixMilliseconds { get; set; } = 600000;
        public bool Debug { get; set; } = false;

    }

    [JsonSerializable(typeof(ConfigOptions))]
    internal partial class SourceGeneratorContexti : JsonSerializerContext
    {
    }
}