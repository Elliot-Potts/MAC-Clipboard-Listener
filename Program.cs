using System;
using System.Windows.Forms;
using System.Net.Http;
using System.Windows.Forms.VisualStyles;

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
}