using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Http;
using System.Drawing;
using System.IO;


namespace MACAddressMonitor
{
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
        private HttpClient httpClient;
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
            httpClient = new HttpClient();
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
            trayMenu.Items.Add("Show Notification", null, OnShowNotification);
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

            // Set initial check mark
            UpdateFormatMenuCheckMarks();
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
                httpClient.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnShowNotification(object sender, EventArgs e)
        {
            ShowNotification("MAC Address Monitor", "This is a test notification.");
        }

        private async void OnClipboardUpdated()
        {
            if (ignoringNextClipboardUpdate)
            {
                ignoringNextClipboardUpdate = false;
                return;
            }

            try
            {
                string clipboardText = await Task.Run(() => GetClipboardText());

                if (IsMACAddress(clipboardText))
                {
                    string formattedMac = ConvertMacFormat(clipboardText, selectedFormat);
                    string vendor = await LookupVendorAsync(formattedMac);

                    if (formattedMac != clipboardText)
                    {
                        ignoringNextClipboardUpdate = true;
                        await Task.Run(() => SetClipboardText(formattedMac));
                        ShowNotification("MAC Address Formatted", $"{formattedMac}\nVendor: {vendor}");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("Error", $"An error occurred: {ex.Message}");
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
            return Regex.IsMatch(text.Trim(), pattern);
        }

        private string ConvertMacFormat(string mac, MacFormat format)
        {
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

        private async Task<string> LookupVendorAsync(string mac)
        {
            try
            {
                string url = $"https://api.macvendors.com/{Uri.EscapeDataString(mac)}";
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return "Not Found";
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Lookup Failed: {ex.Message}";
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