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
                    if (ConfigManager.IsConfigured())
                    {
                        await ConfigManager.GenerateApiKey();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to generate API key. Please check your Netdisco settings. Exception: {ex.Message}", "Netdisco Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }).Wait();

            Application.Run(new MacAddressMonitorContext());
        }
    }
}