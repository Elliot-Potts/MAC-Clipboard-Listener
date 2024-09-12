using System;
using System.Diagnostics;
using System.Linq;
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
            Debug.WriteLine("Inside GetMacDetails() for:  " + macAddress);
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

                //Debug.WriteLine("DBG Netdisco JSON Response: " + json.ToString());

                return new NetdiscoMacDetails
                {
                    IpAddress = json["ips"]?.FirstOrDefault()?["ip"]?.ToString(),
                    IpRouter = json["ips"]?.FirstOrDefault()?["router_ip"]?.ToString(),
                    SwitchHostname = json["sightings"]?.FirstOrDefault()?["device"]?["name"]?.ToString(),
                    SwitchIp = json["sightings"]?.FirstOrDefault()?["switch"]?.ToString(),
                    Switchport = json["sightings"]?.FirstOrDefault()?["port"]?.ToString()
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
        public string IpRouter { get; set; }
        public string SwitchHostname { get; set; }
        public string SwitchIp { get; set; }
        public string Switchport { get; set; }
    }
}