using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Http;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;

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

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MacAddressMonitorContext());
        }
    }

    public class MacAddressMonitorContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ClipboardMonitor clipboardMonitor;
        private MacFormat selectedFormat = MacFormat.CiscoNotation;
        private bool ignoringNextClipboardUpdate = false;
        private Icon customIcon;

        private ToolStripMenuItem ciscoNotationItem;
        private ToolStripMenuItem colonSeparatedItem;
        private ToolStripMenuItem hyphenSeparatedItem;

        private enum MacFormat
        {
            CiscoNotation,
            ColonSeparated,
            HyphenSeparated
        }

        public MacAddressMonitorContext()
        {
            InitializeComponent();
            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardUpdated += OnClipboardUpdated;
        }

        private void InitializeComponent()
        {
            trayMenu = new ContextMenuStrip();

            ToolStripMenuItem formatMenu = new ToolStripMenuItem("Select MAC Format");
            ciscoNotationItem = new ToolStripMenuItem("Cisco Notation", null, (s, e) => SetMacFormat(MacFormat.CiscoNotation));
            colonSeparatedItem = new ToolStripMenuItem("Colon Separated", null, (s, e) => SetMacFormat(MacFormat.ColonSeparated));
            hyphenSeparatedItem = new ToolStripMenuItem("Hyphen Separated", null, (s, e) => SetMacFormat(MacFormat.HyphenSeparated));

            formatMenu.DropDownItems.Add(ciscoNotationItem);
            formatMenu.DropDownItems.Add(colonSeparatedItem);
            formatMenu.DropDownItems.Add(hyphenSeparatedItem);

            trayMenu.Items.Add(formatMenu);
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add("Exit", null, OnExit);

            customIcon = LoadIconFromResources("MacClipListener.mac_monitor_icon.ico");

            trayIcon = new NotifyIcon()
            {
                Icon = customIcon,
                ContextMenuStrip = trayMenu,
                Text = "MAC Address Monitor",
                Visible = true
            };

            // Click event for opening MAC details
            trayIcon.Click += TrayIcon_Click;

            // Set initial check mark
            UpdateFormatMenuCheckMarks();

            // Show initial listening message
            ShowNotification("MAC Clip Listener", "The application has started and is listening for MAC addresses.");
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                //Console.WriteLine($"DBG Length Count of MACOBJ array: {macAddresses.Count}");
                ShowMacDetailsForm();
            }
        }

        private Icon LoadIconFromResources(string resourceName)
        {
            // Load Icon from embedded resource
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        return new Icon(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Icon Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Fallback to default icon on failure
            return SystemIcons.Application;
        }

        private void SetMacFormat(MacFormat format)
        {
            selectedFormat = format;
            UpdateFormatMenuCheckMarks();
            ShowNotification("MAC Format Updated", $"Selected format: {format}");
        }

        private void UpdateFormatMenuCheckMarks()
        {
            ciscoNotationItem.Checked = (selectedFormat == MacFormat.CiscoNotation);
            colonSeparatedItem.Checked = (selectedFormat == MacFormat.ColonSeparated);
            hyphenSeparatedItem.Checked = (selectedFormat == MacFormat.HyphenSeparated);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                trayIcon.Dispose();
                clipboardMonitor.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        List<MACAddress> macAddresses = new List<MACAddress>();

        private async void OnClipboardUpdated()
        {
            Console.WriteLine("[DBG] OnClipboardUpdated called");
            if (ignoringNextClipboardUpdate)
            {
                ignoringNextClipboardUpdate = false;
                return;
            }

            try
            {
                string clipboardText = await Task.Run(() => GetClipboardText());

                string[] splitLines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                bool clipboardChanged = false;

                // Empty the existing MAC addresses list
                macAddresses.Clear();

                foreach (string line in splitLines)
                {
                    if (IsMACAddress(line.Trim()))
                    {
                        string formattedMac = ConvertMacFormat(line.Trim(), selectedFormat);
                        if (formattedMac != line.Trim())
                        {
                            clipboardChanged = true;
                        }
                        var macAddressObject = new MACAddress(formattedMac);
                        macAddresses.Add(macAddressObject);
                    }
                }

                if (macAddresses.Any() && clipboardChanged)
                {
                    await UpdateClipboardWithFormattedMacs(macAddresses);
                    ShowNotificationForMacs(macAddresses);
                }

                // Clear macAddresses yet again - now removed for dialog/form
                //macAddresses.Clear();
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"An error occurred: {ex.Message}");
            }
        }

        private async Task UpdateClipboardWithFormattedMacs(List<MACAddress> macAddresses)
        {
            string formattedText = string.Join(Environment.NewLine, macAddresses.Select(m => m.MacAddress));
            ignoringNextClipboardUpdate = true;
            await Task.Run(() => SetClipboardText(formattedText));
        }

        private void ShowNotificationForMacs(List<MACAddress> newMacAddresses)
        {
            string notificationText = string.Join(Environment.NewLine,
                newMacAddresses.Select(m => $"{m.MacAddress} - {m.Vendor}"));

            //ShowNotification("MAC Addresses Processed",
            //    $"Processed {newMacAddresses.Count} MAC address(es):{Environment.NewLine}{notificationText}");

            trayIcon.BalloonTipClicked += TrayIcon_BalloonTipClicked;
            trayIcon.ShowBalloonTip(3000, "MAC Addresses Processed",
                $"Processed {newMacAddresses.Count} MAC address(es). Click for details.", ToolTipIcon.Info);
        }

        private void TrayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            ShowMacDetailsForm();
            trayIcon.BalloonTipClicked -= TrayIcon_BalloonTipClicked;
        }

        private void ShowMacDetailsForm()
        {
            if (macAddresses.Any())
            {
                var detailsForm = new MacDetailsForm();
                detailsForm.PopulateList(macAddresses);
                detailsForm.Show();
            }
            else
            {
                MessageBox.Show("No MAC addresses have been processed yet.", "MAC Address Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetClipboardText()
        {
            string text = string.Empty;
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        text = Clipboard.GetText();
                    }
                }
                catch (Exception)
                {
                    // Clipboard was probably in use, we'll just return empty string
                }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
            return text;
        }

        private void SetClipboardText(string text)
        {
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    Clipboard.SetText(text);
                }
                catch (Exception)
                {
                    // Clipboard was probably in use, we'll just skip setting it
                }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        private void ShowNotification(string title, string message)
        {
            trayIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.None);
        }

        private bool IsMACAddress(string text)
        {
            string pattern = @"^(?i)(([0-9A-F]{2}[:-]){5}([0-9A-F]{2})|([0-9A-F]{4}\.){2}([0-9A-F]{4}))$";
            return Regex.IsMatch(text, pattern);
        }

        private string ConvertMacFormat(string mac, MacFormat format)
        {
            // Reformat the MAC address based on toolmenu selection
            string cleanMac = Regex.Replace(mac.ToUpper(), "[.:-]", "");

            switch (format)
            {
                case MacFormat.CiscoNotation:
                    return Regex.Replace(cleanMac, "(.{4})(.{4})(.{4})", "$1.$2.$3");
                case MacFormat.ColonSeparated:
                    return Regex.Replace(cleanMac, "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})", "$1:$2:$3:$4:$5:$6");
                case MacFormat.HyphenSeparated:
                    return Regex.Replace(cleanMac, "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})", "$1-$2-$3-$4-$5-$6");
                default:
                    return mac;
            }
        }
    }

    public class ClipboardMonitor : NativeWindow, IDisposable
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        public event Action ClipboardUpdated;

        public ClipboardMonitor()
        {
            CreateHandle(new CreateParams());
            AddClipboardFormatListener(this.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                ClipboardUpdated?.Invoke();
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            RemoveClipboardFormatListener(this.Handle);
            this.DestroyHandle();
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    }
}