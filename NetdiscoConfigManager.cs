using System;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MACAddressMonitor
{
    internal class NetdiscoConfigManager
    {
        private const string API_KEY_SETTING = "NetdiscoApiKey";
        private const string API_URL_SETTING = "NetdiscoApiUrl";
        private static readonly string ConfigFilePath;

        static NetdiscoConfigManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "MACAddressMonitor");
            Directory.CreateDirectory(appFolder);
            ConfigFilePath = Path.Combine(appFolder, "netdisco_config.xml");

            if (!File.Exists(ConfigFilePath))
            {
                CreateEmptyConfigFile();
            }
        }

        private static void CreateEmptyConfigFile()
        {
            var config = new XDocument(
                new XElement("configuration",
                    new XElement("appSettings")
                )
            );
            config.Save(ConfigFilePath);
        }

        public static string GetApiKey()
        {
            return GetSetting(API_KEY_SETTING);
        }

        public static string GetApiUrl()
        {
            return GetSetting(API_URL_SETTING);
        }

        public static async Task SaveApiConfig(string apiUrl, string username, string password)
        {
            string generatedApiKey = await GenerateApiKey(apiUrl, username, password);

            if (string.IsNullOrEmpty(generatedApiKey))
            {
                throw new InvalidOperationException("Failed to generate API key");
            }

            SaveSetting(API_KEY_SETTING, generatedApiKey);
            SaveSetting(API_URL_SETTING, apiUrl);

            Debug.WriteLine($"Configuration saved to: {ConfigFilePath}");
            Debug.WriteLine($"API Key: {generatedApiKey}");
            Debug.WriteLine($"API URL: {apiUrl}");
        }

        private static string GetSetting(string key)
        {
            var config = XDocument.Load(ConfigFilePath);
            var element = config.Root.Element("appSettings").Element(key);
            return element?.Value;
        }

        private static void SaveSetting(string key, string value)
        {
            var config = XDocument.Load(ConfigFilePath);
            var appSettings = config.Root.Element("appSettings");
            var element = appSettings.Element(key);

            if (element == null)
            {
                appSettings.Add(new XElement(key, value));
            }
            else
            {
                element.Value = value;
            }

            config.Save(ConfigFilePath);
        }

        public static bool IsConfigured()
        {
            Debug.WriteLine("Running check for IsConfigured()");
            return !string.IsNullOrEmpty(GetApiKey()) && !string.IsNullOrEmpty(GetApiUrl());
        }

        private static async Task<string> GenerateApiKey(string apiUrl, string username, string password)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/login");
                request.Headers.Add("Accept", "application/json");

                string authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                request.Headers.Add("Authorization", $"Basic {authToken}");

                try
                {
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Parse response to get API key
                    var jsonResponse = JObject.Parse(responseBody);
                    return jsonResponse["api_key"]?.ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error generating API key: {ex.Message}");
                    return null;
                }
            }
        }
    }
}