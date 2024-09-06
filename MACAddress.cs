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
            GetNetdiscoDetails();
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

        private void GetNetdiscoDetails()
        {
            // API key stored in environment variable 'MACL_NDKEY'
            // TODO: Implement Netdisco API call to populate Associated values

            Console.WriteLine("Checking environment for 'MACL_NDKEY'");

            //string getNdAPIKey = Environment.GetEnvironmentVariable("MACL_NDKEY");

            //if (getNdAPIKey != null)
            //{
            //    Console.WriteLine($"Key found with value: {getNdAPIKey}");
            //}
            //else
            //{
            //    MessageBox.Show("Please make sure the environment variable 'MACL_NDKEY' is set.", "NetDisco API key not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            NdAssociatedIPAddress = "Not implemented";
            NdAssociatedSwitchHostname = "Not implemented";
            NdAssociatedSwitchIP = "Not implemented";
            NdAssociatedSwitchport = "Not implemented";
        }
    }
}