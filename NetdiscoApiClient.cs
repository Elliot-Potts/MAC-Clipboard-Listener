using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MACAddressMonitor
{
    // TODO - Implement /search/node API endpoint
    public class NetdiscoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public NetdiscoApiClient()
        {
            _httpClient = new HttpClient();
            _apiKey = NetdiscoConfigManager.GetApiKey();
            _baseUrl = NetdiscoConfigManager.GetApiUrl();

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_baseUrl))
            {
                throw new InvalidOperationException("Netdisco API is not configured. Please configure Netdisco first.");
            }

            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", _apiKey);
        }

        public async Task<NetdiscoMacDetails> GetMacDetails(string macAddress)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/v1/search/node?q={macAddress}");
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