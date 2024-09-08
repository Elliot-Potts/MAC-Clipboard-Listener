using System;
using System.Windows.Forms;
using System.Drawing;

namespace MACAddressMonitor
{
    public partial class NetdiscoConfigForm : Form
    {
        public string NetdiscoUrl { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        private Label lblStatus;
        private TextBox txtUrl;
        private TextBox txtUsername;
        private TextBox txtPassword;

        public NetdiscoConfigForm()
        {
            InitializeComponent();
            LoadExistingConfig();
        }

        private void InitializeComponent()
        {
            this.Text = "Configure Netdisco";
            this.Size = new System.Drawing.Size(300, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblStatus = new Label() { Text = "Checking configuration...", Left = 20, Top = 20, Width = 260 };

            Label lblUrl = new Label() { Text = "Netdisco URL:", Left = 20, Top = 50 };
            txtUrl = new TextBox() { Left = 120, Top = 50, Width = 150 };

            Label lblUsername = new Label() { Text = "Username:", Left = 20, Top = 80 };
            txtUsername = new TextBox() { Left = 120, Top = 80, Width = 150 };

            Label lblPassword = new Label() { Text = "Password:", Left = 20, Top = 110 };
            txtPassword = new TextBox() { Left = 120, Top = 110, Width = 150, PasswordChar = '*' };

            Button btnSave = new Button() { Text = "Save", Left = 100, Top = 150, Width = 75 };
            btnSave.Click += (sender, e) => {
                NetdiscoUrl = txtUrl.Text;
                Username = txtUsername.Text;
                Password = txtPassword.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            Button btnCancel = new Button() { Text = "Cancel", Left = 190, Top = 150, Width = 75 };
            btnCancel.Click += (sender, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.AddRange(new Control[] { lblStatus, lblUrl, txtUrl, lblUsername, txtUsername, lblPassword, txtPassword, btnSave, btnCancel });
        }

        private void LoadExistingConfig()
        {
            string existingUrl = NetdiscoConfigManager.GetApiUrl();
            string existingKey = NetdiscoConfigManager.GetApiKey();

            if (!string.IsNullOrEmpty(existingUrl))
            {
                txtUrl.Text = existingUrl;
            }

            // Check if both URL and API key are configured, and set status label accordingly
            if (!string.IsNullOrEmpty(existingUrl) && !string.IsNullOrEmpty(existingKey))
            {
                lblStatus.Text = "API Key and URL are configured.";
                lblStatus.ForeColor = Color.Green;
            }
            else if (!string.IsNullOrEmpty(existingUrl) && string.IsNullOrEmpty(existingKey))
            {
                lblStatus.Text = "URL is configured, but API Key is missing.";
                lblStatus.ForeColor = Color.Red;
            }
            else if (string.IsNullOrEmpty(existingUrl) && !string.IsNullOrEmpty(existingKey))
            {
                lblStatus.Text = "API Key is configured, but URL is missing.";
                lblStatus.ForeColor = Color.Red;
            }
            else
            {
                lblStatus.Text = "API Key and URL are missing.";
                lblStatus.ForeColor = Color.Red;
            }
        }
    }
}