using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

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

        public MacAddressMonitorContext()
        {
            InitializeComponent();
            clipboardMonitor = new ClipboardMonitor();
            clipboardMonitor.ClipboardUpdated += OnClipboardUpdated;
        }

        private void InitializeComponent()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show Notification", null, OnShowNotification);
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add("Exit", null, OnExit);

            trayIcon = new NotifyIcon()
            {
                Icon = System.Drawing.SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Text = "MAC Address Monitor",
                Visible = true
            };
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

        private void OnShowNotification(object sender, EventArgs e)
        {
            trayIcon.ShowBalloonTip(3000, "MAC Address Monitor", "MAC address found in clipboard.\nVendor: Example-HP", ToolTipIcon.Info);
        }

        private void OnClipboardUpdated()
        {
            string clipboardText = GetClipboardText();
            if (IsMACAddress(clipboardText))
            {
                trayIcon.ShowBalloonTip(3000, "MAC Address Detected", clipboardText, ToolTipIcon.Info);
            }
        }

        private string GetClipboardText()
        {
            if (Clipboard.ContainsText())
            {
                return Clipboard.GetText();
            }
            return string.Empty;
        }

        private bool IsMACAddress(string text)
        {
            string pattern = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
            return Regex.IsMatch(text, pattern);
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