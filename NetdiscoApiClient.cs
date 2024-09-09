using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MACAddressMonitor
{
    public class NetdiscoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public NetdiscoApiClient()
        {
            _httpClient = new HttpClient();
            _baseUrl = NetdiscoConfigManager.GetApiUrl();

            if (string.IsNullOrEmpty(_baseUrl))
            {
                throw new InvalidOperationException("Netdisco API is not configured. Please configure Netdisco first.");
            }
        }

        public async Task<NetdiscoMacDetails> GetMacDetails(string macAddress)
        {
            try
            {
                string apiKey = NetdiscoConfigManager.GetApiKey();
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("API key is not available. Please ensure the application has generated an API key.");
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/v1/search/node?q={macAddress}");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", apiKey);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(content);

                return new NetdiscoMacDetails
                {
                    IpAddress = json["ip"]?.ToString(),
                    SwitchHostname = json["device"]?["name"]?.ToString(),
                    SwitchIp = json["device"]?["ip"]?.ToString(),
                    Switchport = json["port"]?["port"]?.ToString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching MAC details from Netdisco: {ex.Message}");
                return null;
            }
        }
    }

    public class NetdiscoMacDetails
    {
        public string IpAddress { get; set; }
        public string SwitchHostname { get; set; }
        public string SwitchIp { get; set; }
        public string Switchport { get; set; }
    }
}