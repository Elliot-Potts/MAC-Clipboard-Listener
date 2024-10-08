﻿using MacClipListener.Properties;
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

            string[] lines = Resources.manuf.Split('\n');
            foreach (string line in lines)
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

        private string NormalizeMacPrefix(string macPrefix)
        {
            return Regex.Replace(macPrefix, "[.:-]", "").ToUpper();
        }

        public string LookupVendor(string macAddress)
        {
            string normalizedMac = NormalizeMacPrefix(macAddress);

            // Try 24-bit (6 characters) prefix first, then extended OUIs (28-bit, 36-bit)
            if (vendorDictionary.TryGetValue(normalizedMac.Substring(0, 6), out var vendor24))
                return vendor24;
            else if (vendorDictionary.TryGetValue(normalizedMac.Substring(0, 7), out var vendor28))
            {
                return vendor28;
            }
            else if (vendorDictionary.TryGetValue(normalizedMac.Substring(0, 9), out var vendor36))
            {
                return vendor36;
            }

            //Console.WriteLine($"length of Normalized MAC: {normalizedMac.Length}");
            //Console.WriteLine($"First 6 of Normalized MAC: {normalizedMac.Substring(0, 6)}");

            return "Unknown";
        }
    }
}