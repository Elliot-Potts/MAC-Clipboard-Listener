using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace MACAddressMonitor
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Generate API key on startup
            Task.Run(async () =>
            {
                try
                {
                    if (NetdiscoConfigManager.IsConfigured())
                    {
                        await NetdiscoConfigManager.GenerateApiKey();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to generate API key: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }).Wait();

            Application.Run(new MacAddressMonitorContext());
        }
    }
}