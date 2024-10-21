using System;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace MACAddressMonitor
{
    internal class ConfigManager
    {
        private const string USERNAME_SETTING = "NetdiscoUsername";
        private const string PASSWORD_SETTING = "NetdiscoPassword";
        private const string API_URL_SETTING = "NetdiscoApiUrl";
        private const string MAC_FORMAT_SETTING = "MacFormat";
        private static readonly string ConfigFilePath;
        private static string _apiKey;

        // Used to update tray button text
        public static event Action<bool> ApiKeyChanged;

        // Create the path and initial XML config file
        static ConfigManager()
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

        // Create an empty XML configuration file
        private static void CreateEmptyConfigFile()
        {
            var config = new XDocument(
                new XElement("configuration",
                    new XElement("appSettings")
                )
            );
            config.Save(ConfigFilePath);
        }

        public static string GetUsername()
        {
            return GetSetting(USERNAME_SETTING);
        }

        public static string GetPassword()
        {
            return GetSetting(PASSWORD_SETTING);
        }

        public static string GetApiUrl()
        {
            return GetSetting(API_URL_SETTING);
        }

        public static string GetApiKey()
        {
            return _apiKey;
        }

        public static string GetMacFormat()
        {
            return GetSetting(MAC_FORMAT_SETTING);
        }

        public static void SaveMacFormat(string format)
        {
            SaveSetting(MAC_FORMAT_SETTING, format);
        }

        public static async Task SaveApiConfig(string apiUrl, string username, string password)
        {
            SaveSetting(API_URL_SETTING, apiUrl);
            SaveSetting(USERNAME_SETTING, username);
            SaveSetting(PASSWORD_SETTING, password);

            await GenerateApiKey();

            Debug.WriteLine($"Configuration saved to: {ConfigFilePath}");
            Debug.WriteLine($"API URL: {apiUrl}");
            Debug.WriteLine($"Username: {username}");
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
            return !string.IsNullOrEmpty(GetUsername()) && !string.IsNullOrEmpty(GetPassword()) && !string.IsNullOrEmpty(GetApiUrl());
        }

        public static async Task GenerateApiKey()
        {
            string apiUrl = GetApiUrl();
            string username = GetUsername();
            string password = GetPassword();

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

                    var jsonResponse = JObject.Parse(responseBody);
                    _apiKey = jsonResponse["api_key"]?.ToString();
                    ApiKeyChanged?.Invoke(!string.IsNullOrEmpty(_apiKey));

                    if (string.IsNullOrEmpty(_apiKey))
                    {
                        throw new Exception("API key not found in the response");
                    }

                    Debug.WriteLine($"New API key generated: {_apiKey}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error generating API key: {ex.Message}");
                    throw;
                }
            }
        }
    }
}