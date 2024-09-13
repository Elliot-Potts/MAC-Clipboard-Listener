using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MACAddressMonitor
{
    public class MACAddress
    {
        private static readonly MacVendorLookup vendorLookup;
        private static readonly NetdiscoApiClient netdiscoClient;

        static MACAddress()
        {
            vendorLookup = new MacVendorLookup();
            try
            {
                netdiscoClient = new NetdiscoApiClient();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing MacVendorLookup or NetdiscoApiClient: {ex.Message}");
                netdiscoClient = null;
            }
        }

        public string MacAddress { get; private set; }
        public string Vendor { get; private set; }
        public string NdAssociatedIPAddress { get; private set; }
        public string NdAssociatedIPRouter { get; private set; }
        public string NdAssociatedSwitchHostname { get; private set; }
        public string NdAssociatedSwitchIP { get; private set; }
        public string NdAssociatedSwitchport { get; private set; }

        public MACAddress(string macAddress)
        {
            MacAddress = macAddress;
            Vendor = LookupVendor(macAddress);
        }

        private string LookupVendor(string macAddress)
        {
            Console.WriteLine($"Performing vendor lookup for: {macAddress}");
            if (vendorLookup != null)
            {
                try
                {
                    return vendorLookup.LookupVendor(macAddress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error looking up vendor: {ex.Message}");
                }
            }
            return "Unknown";
        }

        public async Task FetchNetdiscoDetails()
        {
            if (netdiscoClient != null)
            {
                try
                {
                    var details = await netdiscoClient.GetMacDetails(MacAddress);
                    Debug.WriteLine("Fetched details...");
                    Debug.WriteLine(details);
                    if (details != null)
                    {
                        NdAssociatedIPAddress = details.IpAddress;
                        NdAssociatedIPRouter = details.IpRouter;
                        NdAssociatedSwitchHostname = details.SwitchHostname;
                        NdAssociatedSwitchIP = details.SwitchIp;
                        NdAssociatedSwitchport = details.Switchport;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching Netdisco details: {ex.Message}");
                }
            }
        }
    }
}