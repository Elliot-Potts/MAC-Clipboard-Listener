using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MACAddressMonitor
{
    public class MacVendorLookup
    {
        private Dictionary<string, string> vendorDictionary = new Dictionary<string, string>();

        public MacVendorLookup()
        {
            LoadVendorData();
        }

        private void LoadVendorData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MacClipListener.manuf.txt";  // Loading embedded manuf text file from Wireshark 

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new Exception($"Resource {resourceName} not found.");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Skip comments in the manuf file...
                        if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line)) continue;

                        // Try and separate by tab
                        var parts = line.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        
                        // Skip invalid entries in manuf file (i.e. no MAC, shortname, fullname)
                        if (parts.Length < 3) continue;

                        // Normalize the MAC address, and trim any remaining whitespace
                        string macPrefix = NormalizeMacPrefix(parts[0].Trim());
                        string fullName = parts[2].Trim();

                        vendorDictionary[macPrefix] = fullName;
                    }
                }
            }
        }

        private string NormalizeMacPrefix(string macPrefix)
        {
            return Regex.Replace(macPrefix, "[.:-]", "").ToUpper();
        }

        public string LookupVendor(string macAddress)
        {
            Console.WriteLine($"Passed = {macAddress}");
            string normalizedMac = NormalizeMacPrefix(macAddress);
            Console.WriteLine($"Normalized = {normalizedMac}");

            // Try 24-bit (6 characters) prefix
            if (normalizedMac.Length >= 6 && vendorDictionary.TryGetValue(normalizedMac.Substring(0, 6), out var vendor24))
                return vendor24;

            // TODO -- improve support for extended OUIs (28/36 bit), current approach only focusses on 24 bit
            // Try 36-bit (9 characters) prefix
            //if (normalizedMac.Length >= 9 && vendorDictionary.TryGetValue(normalizedMac.Substring(0, 9), out var vendor36))
            //    return vendor36;

            // Try 28-bit (7 characters) prefix
            //if (normalizedMac.Length >= 7 && vendorDictionary.TryGetValue(normalizedMac.Substring(0, 7), out var vendor28))
            //    return vendor28;

            //Console.WriteLine($"length of Normalized MAC: {normalizedMac.Length}");
            //Console.WriteLine($"First 6 of Normalized MAC: {normalizedMac.Substring(0, 6)}");

            return "Unknown";
        }
    }
}