using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MACAddressMonitor
{
    public partial class MacDetailsForm : Form
    {
        public MacDetailsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.listViewMacs = new System.Windows.Forms.ListView();
            this.columnMac = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnVendor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewMacs
            // 
            this.listViewMacs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.columnMac,
                this.columnVendor
            });
            this.listViewMacs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewMacs.FullRowSelect = true;
            this.listViewMacs.GridLines = true;
            this.listViewMacs.Location = new System.Drawing.Point(0, 0);
            this.listViewMacs.Name = "listViewMacs";
            this.listViewMacs.Size = new System.Drawing.Size(384, 261);
            this.listViewMacs.TabIndex = 0;
            this.listViewMacs.UseCompatibleStateImageBehavior = false;
            this.listViewMacs.View = System.Windows.Forms.View.Details;
            // 
            // columnMac
            // 
            this.columnMac.Text = "MAC Address";
            this.columnMac.Width = 150;
            // 
            // columnVendor
            // 
            this.columnVendor.Text = "Vendor";
            this.columnVendor.Width = 200;
            // 
            // MacDetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Controls.Add(this.listViewMacs);
            this.Name = "MacDetailsForm";
            this.Text = "MAC Address Details";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ListView listViewMacs;
        private System.Windows.Forms.ColumnHeader columnMac;
        private System.Windows.Forms.ColumnHeader columnVendor;

        public void PopulateList(List<MACAddress> macAddresses)
        {
            listViewMacs.Items.Clear();
            foreach (var mac in macAddresses)
            {
                var item = new ListViewItem(new[] { mac.MacAddress, mac.Vendor });
                listViewMacs.Items.Add(item);
            }
        }
    }
}