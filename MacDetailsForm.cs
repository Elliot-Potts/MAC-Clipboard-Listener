using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MACAddressMonitor
{
    // TODO - add sorting on Vendor
    // TODO - add ability to copy fields
    public partial class MacDetailsForm : Form
    {
        private ContextMenuStrip cellContextMenu;

        public MacDetailsForm()
        {
            InitializeComponent();
            SetFormIcon();
            this.Resize += new EventHandler(MacDetailsForm_Resize);
            InitializeCellContextMenu();
        }

        private void InitializeComponent()
        {
            this.listViewMacs = new System.Windows.Forms.ListView();
            this.columnMac = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnVendor = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAssociatedIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnSwitchIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnSwitch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnSwitchPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listViewMacs
            // 
            this.listViewMacs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnMac,
            this.columnVendor,
            this.columnAssociatedIP,
            this.columnSwitchIP,
            this.columnSwitch,
            this.columnSwitchPort});
            this.listViewMacs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewMacs.FullRowSelect = true;
            this.listViewMacs.GridLines = true;
            this.listViewMacs.HideSelection = false;
            this.listViewMacs.Location = new System.Drawing.Point(0, 0);
            this.listViewMacs.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.listViewMacs.Name = "listViewMacs";
            this.listViewMacs.Size = new System.Drawing.Size(876, 402);
            this.listViewMacs.TabIndex = 0;
            this.listViewMacs.UseCompatibleStateImageBehavior = false;
            this.listViewMacs.View = System.Windows.Forms.View.Details;
            this.listViewMacs.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewMacs_MouseClick);
            // 
            // columnMac
            // 
            this.columnMac.Text = "MAC Address";
            this.columnMac.Width = 120;
            // 
            // columnVendor
            // 
            this.columnVendor.Text = "Vendor";
            this.columnVendor.Width = 150;
            // 
            // columnAssociatedIP
            // 
            this.columnAssociatedIP.Text = "Associated IP";
            this.columnAssociatedIP.Width = 100;
            // 
            // columnSwitchIP
            // 
            this.columnSwitchIP.Text = "Switch IP";
            this.columnSwitchIP.Width = 100;
            // 
            // columnSwitch
            // 
            this.columnSwitch.Text = "Switch";
            this.columnSwitch.Width = 150;
            // 
            // columnSwitchPort
            // 
            this.columnSwitchPort.Text = "Switch Port";
            this.columnSwitchPort.Width = 80;
            // 
            // MacDetailsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 402);
            this.Controls.Add(this.listViewMacs);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MacDetailsForm";
            this.Text = "MAC Address Details";
            this.ResumeLayout(false);

        }

        private void InitializeCellContextMenu()
        {
            cellContextMenu = new ContextMenuStrip();
            ToolStripMenuItem copyMenuItem = new ToolStripMenuItem("Copy");
            copyMenuItem.Click += CopyMenuItem_Click;
            cellContextMenu.Items.Add(copyMenuItem);
        }

        private void listViewMacs_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewHitTestInfo hitTest = listViewMacs.HitTest(e.Location);
                if (hitTest.Item != null)
                {
                    cellContextMenu.Show(listViewMacs, e.Location);
                }
            }
        }

        private void CopyMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewMacs.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewMacs.SelectedItems[0];
                Point mousePosition = listViewMacs.PointToClient(Control.MousePosition);
                int columnIndex = GetColumnIndexAtPoint(mousePosition);

                if (columnIndex != -1 && columnIndex < selectedItem.SubItems.Count)
                {
                    string cellValue = selectedItem.SubItems[columnIndex].Text;
                    Clipboard.SetText(cellValue);
                }
            }
        }

        private System.Windows.Forms.ListView listViewMacs;
        private System.Windows.Forms.ColumnHeader columnMac;
        private System.Windows.Forms.ColumnHeader columnVendor;
        private System.Windows.Forms.ColumnHeader columnAssociatedIP;
        private System.Windows.Forms.ColumnHeader columnSwitchIP;
        private System.Windows.Forms.ColumnHeader columnSwitch;
        private System.Windows.Forms.ColumnHeader columnSwitchPort;

        private int GetColumnIndexAtPoint(Point point)
        {
            int columnIndex = -1;
            int x = 0;

            for (int i = 0; i < listViewMacs.Columns.Count; i++)
            {
                x += listViewMacs.Columns[i].Width;
                if (point.X < x)
                {
                    columnIndex = i;
                    break;
                }
            }

            return columnIndex;
        }

        public void PopulateList(List<MACAddress> macAddresses)
        {
            listViewMacs.Items.Clear();
            foreach (var mac in macAddresses)
            {
                var item = new ListViewItem(new[] {
                    mac.MacAddress,
                    mac.Vendor,
                    mac.NdAssociatedIPAddress,
                    mac.NdAssociatedSwitchIP,
                    mac.NdAssociatedSwitchHostname,
                    mac.NdAssociatedSwitchport
                });
                listViewMacs.Items.Add(item);
            }
        }

        private void SetFormIcon()
        {
            try
            {
                string resourceName = "MacClipListener.mac_monitor_icon.ico";
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        this.Icon = new Icon(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Icon Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MacDetailsForm_Resize(object sender, EventArgs e)
        {
            ResizeColumns();
        }

        private void ResizeColumns()
        {
            int totalWidth = listViewMacs.ClientSize.Width;
            int[] columnWidths = { 15, 20, 15, 20, 15, 10 }; // Percentage widths

            for (int i = 0; i < listViewMacs.Columns.Count; i++)
            {
                listViewMacs.Columns[i].Width = (totalWidth * columnWidths[i]) / 100;
            }
        }
    }
}