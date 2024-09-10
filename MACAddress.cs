using System;

namespace MACAddressMonitor
{
    public class MACAddress
    {
        private static readonly MacVendorLookup vendorLookup;

        static MACAddress()
        {
            try
            {
                vendorLookup = new MacVendorLookup();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing MacVendorLookup: {ex.Message}");
                vendorLookup = null;
            }
        }

        public string MacAddress { get; private set; }
        public string Vendor { get; private set; }
        public string NdAssociatedIPAddress { get; private set; }
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
    }
}