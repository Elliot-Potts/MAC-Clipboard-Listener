using MacClipListener.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace MACAddressMonitor
{
    // TODO - add sorting on fields
    public partial class MacDetailsForm : Form
    {
        private ContextMenuStrip cellContextMenu;

        public MacDetailsForm()
        {
            InitializeComponent();
            this.Icon = Resources.mac_monitor_icon;
            this.Resize += new EventHandler(MacDetailsForm_Resize);
            InitializeCellContextMenu();
        }

        private void InitializeComponent()
        {
            this.listViewMacs = new ListView();
            this.columnMac = new ColumnHeader();
            this.columnVendor = new ColumnHeader();
            this.columnAssociatedIP = new ColumnHeader();
            this.columnSwitchIP = new ColumnHeader();
            this.columnSwitch = new ColumnHeader();
            this.columnSwitchPort = new ColumnHeader();
            this.SuspendLayout();
            // 
            // listViewMacs
            // 
            this.listViewMacs.Columns.AddRange(new ColumnHeader[] {
            this.columnMac,
            this.columnVendor,
            this.columnAssociatedIP,
            this.columnSwitchIP,
            this.columnSwitch,
            this.columnSwitchPort});
            this.listViewMacs.Dock = DockStyle.Fill;
            this.listViewMacs.FullRowSelect = true;
            this.listViewMacs.GridLines = true;
            this.listViewMacs.HideSelection = false;
            this.listViewMacs.Location = new Point(0, 0);
            this.listViewMacs.Margin = new Padding(4, 5, 4, 5);
            this.listViewMacs.Name = "listViewMacs";
            this.listViewMacs.Size = new Size(876, 402);
            this.listViewMacs.TabIndex = 0;
            this.listViewMacs.UseCompatibleStateImageBehavior = false;
            this.listViewMacs.View = View.Details;
            this.listViewMacs.MouseClick += new MouseEventHandler(this.ListViewMacs_MouseClick);
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
            this.AutoScaleDimensions = new SizeF(9F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1100, 402);
            this.Controls.Add(this.listViewMacs);
            this.Margin = new Padding(4, 5, 4, 5);
            this.Name = "MacDetailsForm";
            this.Text = "MAC Address Details";
            this.ResumeLayout(false);

        }

        private void InitializeCellContextMenu()
        {
            cellContextMenu = new ContextMenuStrip();
            ToolStripMenuItem copyRowItem = new ToolStripMenuItem("Copy Row");
            ToolStripMenuItem copyCellItem = new ToolStripMenuItem("Copy Cell");
            copyRowItem.Click += CopyRowItem_Click;
            copyCellItem.Click += CopyMenuItem_Click;
            cellContextMenu.Items.Add(copyRowItem);
            cellContextMenu.Items.Add(copyCellItem);
        }

        private void ListViewMacs_MouseClick(object sender, MouseEventArgs e)
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

        private void CopyRowItem_Click(object sender, EventArgs e)
        {
            if (listViewMacs.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewMacs.SelectedItems[0];
                string rowText = string.Empty;

                foreach (ListViewItem.ListViewSubItem subItem in selectedItem.SubItems)
                {
                    rowText += subItem.Text + "\n";
                }

                Clipboard.SetText(rowText);
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

        private ListView listViewMacs;
        private ColumnHeader columnMac;
        private ColumnHeader columnVendor;
        private ColumnHeader columnAssociatedIP;
        private ColumnHeader columnSwitchIP;
        private ColumnHeader columnSwitch;
        private ColumnHeader columnSwitchPort;

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