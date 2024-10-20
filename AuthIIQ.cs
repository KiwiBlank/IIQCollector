using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IIQCompare
{
    public class AuthIIQ
    {
        public static async Task AuthenticateIIQ()
        {
            HttpClient httpClient = new HttpClient(Program.HandlerClient);
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new LoginFormat
                {
                    username = Program.IIQUsername,
                    password = Program.IIQPassword
                }, SourceGeneratorContextn.Default.LoginFormat),
                Encoding.UTF8,
                "application/json");
            HttpResponseMessage response;

            try
            {
                //Console.WriteLine(Program.IIQHostAdress);
                string httpEndpoint = String.Format("{0}/insightiq/rest/security-iam/v1/auth/login", Program.IIQHostAdress);
                response = await httpClient.PostAsync(httpEndpoint, jsonContent);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error authenticating to IIQ");
                Console.WriteLine(e.Message);
                Program.LogExceptionToFile(e);
                throw;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonResponses = response.Headers.GetValues;

            List<String> headersList = response.Headers.GetValues("Set-Cookie").ToList();
            Program.AuthKey = ParseAuthKey(headersList[0]);
            Program.AuthCSRF = ParseCSRFKey(headersList[1]);
            //Console.WriteLine("Authkey: {0}", Program.AuthKey);
            //Console.WriteLine("CSRFKey: {0}", Program.AuthCSRF);
        }

        public static string ParseAuthKey(string authKey)
        {
            string key = "insightiq_auth=";
            int startIndex = authKey.IndexOf(key) + key.Length;
            int endIndex = authKey.IndexOf(';', startIndex);
            string insightiqAuthValue = authKey.Substring(startIndex, endIndex - startIndex);
            return insightiqAuthValue;
        }

        public static string ParseCSRFKey(string csrfKey)
        {
            string key = "csrf_token=";
            int startIndex = csrfKey.IndexOf(key) + key.Length;
            int endIndex = csrfKey.IndexOf(';', startIndex);
            string csrfTokenValue = csrfKey.Substring(startIndex, endIndex - startIndex);
            return csrfTokenValue;
        }
    }

    public class LoginFormat
    {
        public string password { get; set; }
        public string username { get; set; }
    }

    [JsonSerializable(typeof(LoginFormat))]
    internal partial class SourceGeneratorContextn : JsonSerializerContext
    {
    }
}