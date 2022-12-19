/*
 * Created by SharpDevelop.
 * User: srossmann
 * Date: 25.11.2015
 * Time: 06:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace EasyModbusAdvancedClient
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.addFunctionCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFunctionCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editFunctionCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.startSingleJobToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopCurrentJobToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startAllJobsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopAllJobsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.updateValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateAllValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button2 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column5 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.workspaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveWorkspaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeWorkspaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addConnectionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteConnectionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editConnectionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.addFunctionCodeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteFunctionCodeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.editFunctionCodeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.startAllJobsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.stopAllJobsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.updateAllValuessingleReadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button9);
            this.splitContainer1.Panel1.Controls.Add(this.button10);
            this.splitContainer1.Panel1.Controls.Add(this.button8);
            this.splitContainer1.Panel1.Controls.Add(this.button7);
            this.splitContainer1.Panel1.Controls.Add(this.button6);
            this.splitContainer1.Panel1.Controls.Add(this.button5);
            this.splitContainer1.Panel1.Controls.Add(this.button4);
            this.splitContainer1.Panel1.Controls.Add(this.button3);
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.button2);
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Size = new System.Drawing.Size(1284, 742);
            this.splitContainer1.SplitterDistance = 409;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer1SplitterMoved);
            // 
            // button9
            // 
            this.button9.Image = ((System.Drawing.Image)(resources.GetObject("button9.Image")));
            this.button9.Location = new System.Drawing.Point(256, 28);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(35, 35);
            this.button9.TabIndex = 8;
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.Button9Click);
            // 
            // button10
            // 
            this.button10.Image = ((System.Drawing.Image)(resources.GetObject("button10.Image")));
            this.button10.Location = new System.Drawing.Point(220, 28);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(35, 35);
            this.button10.TabIndex = 7;
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.Button10Click);
            // 
            // button8
            // 
            this.button8.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab;
            this.button8.Location = new System.Drawing.Point(184, 28);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(35, 35);
            this.button8.TabIndex = 5;
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.Button8Click);
            // 
            // button7
            // 
            this.button7.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab_close;
            this.button7.Location = new System.Drawing.Point(148, 28);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(35, 35);
            this.button7.TabIndex = 4;
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.Button7Click);
            // 
            // button6
            // 
            this.button6.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab_new;
            this.button6.Location = new System.Drawing.Point(112, 28);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(35, 35);
            this.button6.TabIndex = 2;
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.Button6Click);
            // 
            // button5
            // 
            this.button5.Image = ((System.Drawing.Image)(resources.GetObject("button5.Image")));
            this.button5.Location = new System.Drawing.Point(76, 28);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(35, 35);
            this.button5.TabIndex = 3;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.Button5Click);
            // 
            // button4
            // 
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.Location = new System.Drawing.Point(40, 28);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(35, 35);
            this.button4.TabIndex = 2;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Button4Click);
            // 
            // button3
            // 
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.Location = new System.Drawing.Point(4, 28);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(35, 35);
            this.button3.TabIndex = 1;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3Click);
            // 
            // treeView1
            // 
            this.treeView1.ContextMenuStrip = this.contextMenuStrip1;
            this.treeView1.ForeColor = System.Drawing.Color.Black;
            this.treeView1.ItemHeight = 16;
            this.treeView1.Location = new System.Drawing.Point(3, 69);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(403, 670);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1AfterSelect);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addConnectionToolStripMenuItem,
            this.deleteConnectionToolStripMenuItem,
            this.editConnectionToolStripMenuItem,
            this.toolStripSeparator1,
            this.addFunctionCodeToolStripMenuItem,
            this.deleteFunctionCodeToolStripMenuItem,
            this.editFunctionCodeToolStripMenuItem,
            this.toolStripSeparator2,
            this.startSingleJobToolStripMenuItem,
            this.stopCurrentJobToolStripMenuItem,
            this.startAllJobsToolStripMenuItem,
            this.stopAllJobsToolStripMenuItem,
            this.toolStripSeparator3,
            this.updateValuesToolStripMenuItem,
            this.updateAllValuesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(281, 286);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1Opening);
            // 
            // addConnectionToolStripMenuItem
            // 
            this.addConnectionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("addConnectionToolStripMenuItem.Image")));
            this.addConnectionToolStripMenuItem.Name = "addConnectionToolStripMenuItem";
            this.addConnectionToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.addConnectionToolStripMenuItem.Text = "Add connection";
            this.addConnectionToolStripMenuItem.Click += new System.EventHandler(this.AddConnectionToolStripMenuItemClick);
            // 
            // deleteConnectionToolStripMenuItem
            // 
            this.deleteConnectionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deleteConnectionToolStripMenuItem.Image")));
            this.deleteConnectionToolStripMenuItem.Name = "deleteConnectionToolStripMenuItem";
            this.deleteConnectionToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.deleteConnectionToolStripMenuItem.Text = "Delete connection";
            this.deleteConnectionToolStripMenuItem.Visible = false;
            this.deleteConnectionToolStripMenuItem.Click += new System.EventHandler(this.DeleteConnectionToolStripMenuItemClick);
            // 
            // editConnectionToolStripMenuItem
            // 
            this.editConnectionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("editConnectionToolStripMenuItem.Image")));
            this.editConnectionToolStripMenuItem.Name = "editConnectionToolStripMenuItem";
            this.editConnectionToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.editConnectionToolStripMenuItem.Text = "Edit connection";
            this.editConnectionToolStripMenuItem.Visible = false;
            this.editConnectionToolStripMenuItem.Click += new System.EventHandler(this.EditConnectionToolStripMenuItemClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(277, 6);
            this.toolStripSeparator1.Visible = false;
            // 
            // addFunctionCodeToolStripMenuItem
            // 
            this.addFunctionCodeToolStripMenuItem.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab_new;
            this.addFunctionCodeToolStripMenuItem.Name = "addFunctionCodeToolStripMenuItem";
            this.addFunctionCodeToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.addFunctionCodeToolStripMenuItem.Text = "Add function code";
            this.addFunctionCodeToolStripMenuItem.Visible = false;
            this.addFunctionCodeToolStripMenuItem.Click += new System.EventHandler(this.AddFunctionCodeToolStripMenuItemClick);
            // 
            // deleteFunctionCodeToolStripMenuItem
            // 
            this.deleteFunctionCodeToolStripMenuItem.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab_close;
            this.deleteFunctionCodeToolStripMenuItem.Name = "deleteFunctionCodeToolStripMenuItem";
            this.deleteFunctionCodeToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.deleteFunctionCodeToolStripMenuItem.Text = "Delete function code";
            this.deleteFunctionCodeToolStripMenuItem.Visible = false;
            this.deleteFunctionCodeToolStripMenuItem.Click += new System.EventHandler(this.DeleteFunctionCodeToolStripMenuItemClick);
            // 
            // editFunctionCodeToolStripMenuItem
            // 
            this.editFunctionCodeToolStripMenuItem.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab;
            this.editFunctionCodeToolStripMenuItem.Name = "editFunctionCodeToolStripMenuItem";
            this.editFunctionCodeToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.editFunctionCodeToolStripMenuItem.Text = "Edit function Code";
            this.editFunctionCodeToolStripMenuItem.Visible = false;
            this.editFunctionCodeToolStripMenuItem.Click += new System.EventHandler(this.EditFunctionCodeToolStripMenuItemClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(277, 6);
            this.toolStripSeparator2.Visible = false;
            // 
            // startSingleJobToolStripMenuItem
            // 
            this.startSingleJobToolStripMenuItem.Name = "startSingleJobToolStripMenuItem";
            this.startSingleJobToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.startSingleJobToolStripMenuItem.Text = "Start current job (continuous read)";
            this.startSingleJobToolStripMenuItem.Visible = false;
            this.startSingleJobToolStripMenuItem.Click += new System.EventHandler(this.StartSingleJobToolStripMenuItemClick);
            // 
            // stopCurrentJobToolStripMenuItem
            // 
            this.stopCurrentJobToolStripMenuItem.Name = "stopCurrentJobToolStripMenuItem";
            this.stopCurrentJobToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.stopCurrentJobToolStripMenuItem.Text = "Stop current job";
            this.stopCurrentJobToolStripMenuItem.Visible = false;
            this.stopCurrentJobToolStripMenuItem.Click += new System.EventHandler(this.stopCurrentJobToolStripMenuItem_Click);
            // 
            // startAllJobsToolStripMenuItem
            // 
            this.startAllJobsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("startAllJobsToolStripMenuItem.Image")));
            this.startAllJobsToolStripMenuItem.Name = "startAllJobsToolStripMenuItem";
            this.startAllJobsToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.startAllJobsToolStripMenuItem.Text = "Start all jobs (continuous read)";
            this.startAllJobsToolStripMenuItem.Visible = false;
            this.startAllJobsToolStripMenuItem.Click += new System.EventHandler(this.StartAllJobsToolStripMenuItemClick);
            // 
            // stopAllJobsToolStripMenuItem
            // 
            this.stopAllJobsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("stopAllJobsToolStripMenuItem.Image")));
            this.stopAllJobsToolStripMenuItem.Name = "stopAllJobsToolStripMenuItem";
            this.stopAllJobsToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.stopAllJobsToolStripMenuItem.Text = "Stop all jobs";
            this.stopAllJobsToolStripMenuItem.Visible = false;
            this.stopAllJobsToolStripMenuItem.Click += new System.EventHandler(this.StopAllJobsToolStripMenuItemClick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(277, 6);
            this.toolStripSeparator3.Visible = false;
            // 
            // updateValuesToolStripMenuItem
            // 
            this.updateValuesToolStripMenuItem.Name = "updateValuesToolStripMenuItem";
            this.updateValuesToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.updateValuesToolStripMenuItem.Text = "Update values current job (single read) ";
            this.updateValuesToolStripMenuItem.Visible = false;
            this.updateValuesToolStripMenuItem.Click += new System.EventHandler(this.UpdateValuesToolStripMenuItemClick);
            // 
            // updateAllValuesToolStripMenuItem
            // 
            this.updateAllValuesToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("updateAllValuesToolStripMenuItem.Image")));
            this.updateAllValuesToolStripMenuItem.Name = "updateAllValuesToolStripMenuItem";
            this.updateAllValuesToolStripMenuItem.Size = new System.Drawing.Size(280, 22);
            this.updateAllValuesToolStripMenuItem.Text = "Update all values (single read)";
            this.updateAllValuesToolStripMenuItem.Visible = false;
            this.updateAllValuesToolStripMenuItem.Click += new System.EventHandler(this.UpdateAllValuesToolStripMenuItemClick);
            // 
            // button2
            // 
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button2.Location = new System.Drawing.Point(3, 262);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(63, 58);
            this.button2.TabIndex = 2;
            this.button2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(72, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(787, 702);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(779, 676);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Tag View";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column5,
            this.Column1,
            this.Column2,
            this.Column4,
            this.Column3});
            this.dataGridView1.Location = new System.Drawing.Point(17, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(766, 662);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView1CellClick);
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Connection";
            this.Column5.Name = "Column5";
            this.Column5.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column5.Width = 150;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Address";
            this.Column1.Name = "Column1";
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Tag (optional)";
            this.Column2.Name = "Column2";
            this.Column2.Width = 200;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "Datatype";
            this.Column4.Items.AddRange(new object[] {
            "BOOL (FALSE...TRUE)",
            "INT16 (-32768...32767)",
            "UINT16 (0...65535)",
            "BOOL (FALSE...TRUE)",
            "INT16 (-32768...32767)",
            "WORD16 (0...65535)",
            "ASCII"});
            this.Column4.Name = "Column4";
            this.Column4.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Column4.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.Column4.Width = 150;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "Value";
            this.Column3.Name = "Column3";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(779, 676);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Raw Data View";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(4, 4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(769, 666);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button1.Location = new System.Drawing.Point(3, 167);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(63, 58);
            this.button1.TabIndex = 1;
            this.button1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.workspaceToolStripMenuItem,
            this.connectionManagerToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1284, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // workspaceToolStripMenuItem
            // 
            this.workspaceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveWorkspaceToolStripMenuItem,
            this.changeWorkspaceToolStripMenuItem,
            this.loadToolStripMenuItem});
            this.workspaceToolStripMenuItem.Name = "workspaceToolStripMenuItem";
            this.workspaceToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.workspaceToolStripMenuItem.Text = "Workspace";
            // 
            // saveWorkspaceToolStripMenuItem
            // 
            this.saveWorkspaceToolStripMenuItem.Name = "saveWorkspaceToolStripMenuItem";
            this.saveWorkspaceToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.saveWorkspaceToolStripMenuItem.Text = "Save Workspace";
            this.saveWorkspaceToolStripMenuItem.Click += new System.EventHandler(this.saveWorkspaceToolStripMenuItem_Click);
            // 
            // changeWorkspaceToolStripMenuItem
            // 
            this.changeWorkspaceToolStripMenuItem.Name = "changeWorkspaceToolStripMenuItem";
            this.changeWorkspaceToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.changeWorkspaceToolStripMenuItem.Text = "Change Workspace";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // connectionManagerToolStripMenuItem
            // 
            this.connectionManagerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addConnectionToolStripMenuItem1,
            this.deleteConnectionToolStripMenuItem1,
            this.editConnectionToolStripMenuItem1,
            this.toolStripSeparator4,
            this.addFunctionCodeToolStripMenuItem1,
            this.deleteFunctionCodeToolStripMenuItem1,
            this.editFunctionCodeToolStripMenuItem1,
            this.toolStripSeparator5,
            this.startAllJobsToolStripMenuItem1,
            this.stopAllJobsToolStripMenuItem1,
            this.toolStripSeparator6,
            this.updateAllValuessingleReadToolStripMenuItem});
            this.connectionManagerToolStripMenuItem.Name = "connectionManagerToolStripMenuItem";
            this.connectionManagerToolStripMenuItem.Size = new System.Drawing.Size(131, 20);
            this.connectionManagerToolStripMenuItem.Text = "Connection Manager";
            // 
            // addConnectionToolStripMenuItem1
            // 
            this.addConnectionToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.network_connect;
            this.addConnectionToolStripMenuItem1.Name = "addConnectionToolStripMenuItem1";
            this.addConnectionToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.addConnectionToolStripMenuItem1.Text = "Add connection";
            this.addConnectionToolStripMenuItem1.Click += new System.EventHandler(this.AddConnectionToolStripMenuItemClick);
            // 
            // deleteConnectionToolStripMenuItem1
            // 
            this.deleteConnectionToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.network_disconnect_2;
            this.deleteConnectionToolStripMenuItem1.Name = "deleteConnectionToolStripMenuItem1";
            this.deleteConnectionToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.deleteConnectionToolStripMenuItem1.Text = "Delete connection";
            this.deleteConnectionToolStripMenuItem1.Visible = false;
            this.deleteConnectionToolStripMenuItem1.Click += new System.EventHandler(this.DeleteConnectionToolStripMenuItemClick);
            // 
            // editConnectionToolStripMenuItem1
            // 
            this.editConnectionToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.network_connect_2;
            this.editConnectionToolStripMenuItem1.Name = "editConnectionToolStripMenuItem1";
            this.editConnectionToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.editConnectionToolStripMenuItem1.Text = "Edit connection";
            this.editConnectionToolStripMenuItem1.Visible = false;
            this.editConnectionToolStripMenuItem1.Click += new System.EventHandler(this.EditConnectionToolStripMenuItemClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(232, 6);
            this.toolStripSeparator4.Visible = false;
            // 
            // addFunctionCodeToolStripMenuItem1
            // 
            this.addFunctionCodeToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab_new;
            this.addFunctionCodeToolStripMenuItem1.Name = "addFunctionCodeToolStripMenuItem1";
            this.addFunctionCodeToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.addFunctionCodeToolStripMenuItem1.Text = "Add function code";
            this.addFunctionCodeToolStripMenuItem1.Visible = false;
            this.addFunctionCodeToolStripMenuItem1.Click += new System.EventHandler(this.AddFunctionCodeToolStripMenuItemClick);
            // 
            // deleteFunctionCodeToolStripMenuItem1
            // 
            this.deleteFunctionCodeToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab_close;
            this.deleteFunctionCodeToolStripMenuItem1.Name = "deleteFunctionCodeToolStripMenuItem1";
            this.deleteFunctionCodeToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.deleteFunctionCodeToolStripMenuItem1.Text = "Delete function code";
            this.deleteFunctionCodeToolStripMenuItem1.Visible = false;
            this.deleteFunctionCodeToolStripMenuItem1.Click += new System.EventHandler(this.DeleteFunctionCodeToolStripMenuItemClick);
            // 
            // editFunctionCodeToolStripMenuItem1
            // 
            this.editFunctionCodeToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.tab;
            this.editFunctionCodeToolStripMenuItem1.Name = "editFunctionCodeToolStripMenuItem1";
            this.editFunctionCodeToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.editFunctionCodeToolStripMenuItem1.Text = "Edit function code";
            this.editFunctionCodeToolStripMenuItem1.Visible = false;
            this.editFunctionCodeToolStripMenuItem1.Click += new System.EventHandler(this.EditFunctionCodeToolStripMenuItemClick);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(232, 6);
            this.toolStripSeparator5.Visible = false;
            // 
            // startAllJobsToolStripMenuItem1
            // 
            this.startAllJobsToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.view_refresh_2;
            this.startAllJobsToolStripMenuItem1.Name = "startAllJobsToolStripMenuItem1";
            this.startAllJobsToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.startAllJobsToolStripMenuItem1.Text = "Start all jobs (continuous read)";
            this.startAllJobsToolStripMenuItem1.Visible = false;
            this.startAllJobsToolStripMenuItem1.Click += new System.EventHandler(this.StartAllJobsToolStripMenuItemClick);
            // 
            // stopAllJobsToolStripMenuItem1
            // 
            this.stopAllJobsToolStripMenuItem1.Image = global::EasyModbusAdvancedClient.Properties.Resources.process_stop_2;
            this.stopAllJobsToolStripMenuItem1.Name = "stopAllJobsToolStripMenuItem1";
            this.stopAllJobsToolStripMenuItem1.Size = new System.Drawing.Size(235, 22);
            this.stopAllJobsToolStripMenuItem1.Text = "Stop all jobs";
            this.stopAllJobsToolStripMenuItem1.Visible = false;
            this.stopAllJobsToolStripMenuItem1.Click += new System.EventHandler(this.StopAllJobsToolStripMenuItemClick);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(232, 6);
            this.toolStripSeparator6.Visible = false;
            // 
            // updateAllValuessingleReadToolStripMenuItem
            // 
            this.updateAllValuessingleReadToolStripMenuItem.Name = "updateAllValuessingleReadToolStripMenuItem";
            this.updateAllValuessingleReadToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.updateAllValuessingleReadToolStripMenuItem.Text = "Update all Values (single read)";
            this.updateAllValuessingleReadToolStripMenuItem.Visible = false;
            this.updateAllValuessingleReadToolStripMenuItem.Click += new System.EventHandler(this.UpdateAllValuesToolStripMenuItemClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 742);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "EasyModbus Advanced Client";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.SizeChanged += new System.EventHandler(this.MainFormSizeChanged);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		
		
				private System.Windows.Forms.ToolStripMenuItem deleteConnectionToolStripMenuItem;
			private System.Windows.Forms.ToolStripMenuItem editConnectionToolStripMenuItem;
			private System.Windows.Forms.ToolStripMenuItem deleteFunctionCodeToolStripMenuItem;
			private System.Windows.Forms.ToolStripMenuItem editFunctionCodeToolStripMenuItem;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.ToolStripMenuItem updateAllValuesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem updateValuesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startAllJobsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem startSingleJobToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addFunctionCodeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addConnectionToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.ToolStripMenuItem stopCurrentJobToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem stopAllJobsToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem workspaceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveWorkspaceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem changeWorkspaceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem connectionManagerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addConnectionToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem deleteConnectionToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem editConnectionToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem addFunctionCodeToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem deleteFunctionCodeToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem editFunctionCodeToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem startAllJobsToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem stopAllJobsToolStripMenuItem1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem updateAllValuessingleReadToolStripMenuItem;
	
	
		
		void ContextMenuStrip1Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			
		}
		
		void AddConnectionToolStripMenuItemClick(object sender, System.EventArgs e)
		{
			AddConnectionForm addConnectionForm = new AddConnectionForm(easyModbusManager);
			addConnectionForm.ShowDialog();
		}

        private System.Windows.Forms.DataGridViewComboBoxColumn Column5;
        private System.Windows.Forms.DataGridViewComboBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewComboBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox1;
    }
}
	

