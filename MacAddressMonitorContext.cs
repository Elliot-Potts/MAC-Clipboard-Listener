using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MacClipListener.Properties;
using System.Diagnostics;

namespace MACAddressMonitor
{
	public partial class MacAddressMonitorContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ClipboardMonitor clipboardMonitor;
        private ToolStripMenuItem configureNetdisco;
        private MacFormat selectedFormat = MacFormat.CiscoNotation;
        private bool ignoringNextClipboardUpdate = false;

        private ToolStripMenuItem ciscoNotationItem;
        private ToolStripMenuItem colonSeparatedItem;
        private ToolStripMenuItem hyphenSeparatedItem;

        public MacAddressMonitorContext()
        {
            InitializeComponent();
            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardUpdated += OnClipboardUpdated;
            ConfigManager.ApiKeyChanged += UpdateNetdiscoMenuItemText;
            LoadSavedMacFormat();
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

            configureNetdisco = new ToolStripMenuItem(null, null, ShowNetdiscoConfigForm);
            configureNetdisco.Text = ConfigManager.GetApiKey() != null ? "Configure Netdisco (Connected)" : "Configure Netdisco (Disconnected)";

            trayMenu.Items.Add(formatMenu);
            trayMenu.Items.Add(configureNetdisco);
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add("Exit", null, OnExit);

            trayIcon = new NotifyIcon()
            {
                Icon = Resources.mac_monitor_icon,
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

        private void LoadSavedMacFormat()
        {
            string savedFormat = ConfigManager.GetMacFormat();
            if (!string.IsNullOrEmpty(savedFormat) && Enum.TryParse(savedFormat, out MacFormat format))
            {
                SetMacFormat(format);
            }
        }

        private async void ShowNetdiscoConfigForm(object sender, EventArgs e)
        {
            var configForm = new NetdiscoConfigForm();

            if (configForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await ConfigManager.SaveApiConfig(configForm.NetdiscoUrl, configForm.Username, configForm.Password);
                    await ConfigManager.GenerateApiKey();
                    ShowNotification("Netdisco Configuration", "Netdisco settings have been saved and an API key has been generated. Please restart the application.");
                }
                catch (Exception ex)
                {
                    ShowNotification("Configuration Error", $"Failed to save Netdisco configuration or generate API key: {ex.Message}");
                }
            }
        }

        // This method updates the ToolStripMenuItem text
        private void UpdateNetdiscoMenuItemText(bool isConnected)
        {
            Debug.WriteLine($"UpdateNetdiscoMenuItemText isConnected = {isConnected}");
            configureNetdisco.Text = isConnected
                ? "Configure Netdisco (Connected)"
                : "Configure Netdisco (Disconnected)";
        }

        private void TrayIcon_Click(object sender, EventArgs e)
        {
            if (((MouseEventArgs)e).Button == MouseButtons.Left)
            {
                ShowMacDetailsForm();
            }
        }
                
        private void SetMacFormat(MacFormat format)
        {
            selectedFormat = format;
            UpdateFormatMenuCheckMarks();
            ConfigManager.SaveMacFormat(format.ToString());
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

        private MacDetailsForm macDetailsForm;
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
                string clipboardText = GetClipboardText();

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
                        await macAddressObject.FetchNetdiscoDetails();
                        macAddresses.Add(macAddressObject);
                    }
                }

                if (macAddresses.Any() && clipboardChanged)
                {
                    UpdateClipboardWithFormattedMacs(macAddresses);
                    ShowNotificationForMacs(macAddresses);

                    // Update the MacDetailsForm if it's open
                    if (macDetailsForm != null && !macDetailsForm.IsDisposed)
                    {
                        // Use Invoke to update UI from a different thread
                        macDetailsForm.Invoke((MethodInvoker)delegate
                        {
                            macDetailsForm.PopulateList(macAddresses);
                        });
                    }
                }

                // Clear macAddresses yet again - now removed for dialog/form
                //macAddresses.Clear();
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"An error occurred: {ex.Message}");
            }
        }

        private void UpdateClipboardWithFormattedMacs(List<MACAddress> macAddresses)
        {
            string formattedText = string.Join(Environment.NewLine, macAddresses.Select(m => m.MacAddress));
            ignoringNextClipboardUpdate = true;
            SetClipboardText(formattedText);
        }

        private void ShowNotificationForMacs(List<MACAddress> newMacAddresses)
        {
            string notificationText = string.Join(Environment.NewLine,
                newMacAddresses.Select(m => $"{m.MacAddress} - {m.Vendor}"));

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
                // Updated to check if a window is already open (fixes multiple windows opening)
                if (macDetailsForm == null || macDetailsForm.IsDisposed)
                {
                    macDetailsForm = new MacDetailsForm();
                    // Set/ensure macDetailsForm to null on closed
                    macDetailsForm.FormClosed += (s, args) => macDetailsForm = null;
                }

                if (!macDetailsForm.Visible)
                {
                    macDetailsForm.PopulateList(macAddresses);
                    macDetailsForm.Show();
                }
                else
                {
                    macDetailsForm.BringToFront();
                }
            }
            else
            {
                MessageBox.Show("No MAC addresses have been processed yet.", "MAC Address Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetClipboardText()
        {
            string text = string.Empty;
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
            return text;
        }

        private void SetClipboardText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
                // Clipboard was probably in use, we'll just skip setting it
            }
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
}