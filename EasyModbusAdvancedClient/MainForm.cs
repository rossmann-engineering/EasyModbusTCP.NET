/*
Copyright (c) 2018-2020 Rossmann-Engineering
Permission is hereby granted, free of charge, 
to any person obtaining a copy of this software
and associated documentation files (the "Software"),
to deal in the Software without restriction, 
including without limitation the rights to use, 
copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit 
persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission 
notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EasyModbusAdvancedClient
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		EasyModbusManager easyModbusManager = new EasyModbusManager();
		public MainForm()
		{
			
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
            easyModbusManager.connectionPropertiesListChanged += new EasyModbusManager.ConnectionPropertiesListChanged(UpdateListBox);
            easyModbusManager.valuesChanged += new EasyModbusManager.ValuesChanged(UpdateDataGridView);
            //*************************************************create tooltips
            ToolTip toolTip1 = new ToolTip();
   			toolTip1.AutoPopDelay=5000;
    		toolTip1.InitialDelay=100;
    		toolTip1.ReshowDelay=500;
    		toolTip1.ShowAlways=true;
    		toolTip1.SetToolTip(this.button3, "Add connection");
    		
    		ToolTip toolTip2 = new ToolTip();
   			toolTip2.AutoPopDelay=5000;
    		toolTip2.InitialDelay=100;
    		toolTip2.ReshowDelay=500;
    		toolTip2.ShowAlways=true;
    		toolTip2.SetToolTip(this.button4, "Remove connection");
    		
    		ToolTip toolTip3 = new ToolTip();
   			toolTip3.AutoPopDelay=5000;
    		toolTip3.InitialDelay=100;
    		toolTip3.ReshowDelay=500;
    		toolTip3.ShowAlways=true;
    		toolTip3.SetToolTip(this.button5, "Edit connection");
    		
    		ToolTip toolTip4 = new ToolTip();
   			toolTip4.AutoPopDelay=5000;
    		toolTip4.InitialDelay=100;
    		toolTip4.ReshowDelay=500;
    		toolTip4.ShowAlways=true;
    		toolTip4.SetToolTip(this.button6, "Add Function code");
    		
    		ToolTip toolTip5 = new ToolTip();
   			toolTip5.AutoPopDelay=5000;
    		toolTip5.InitialDelay=100;
    		toolTip5.ReshowDelay=500;
    		toolTip5.ShowAlways=true;
    		toolTip5.SetToolTip(this.button7, "Remove Function code");
    		
    		ToolTip toolTip6 = new ToolTip();
   			toolTip6.AutoPopDelay=5000;
    		toolTip6.InitialDelay=100;
    		toolTip6.ReshowDelay=500;
    		toolTip6.ShowAlways=true;
    		toolTip6.SetToolTip(this.button8, "Edit Function code");
    		
    		    		
    		ToolTip toolTip7 = new ToolTip();
   			toolTip7.AutoPopDelay=5000;
    		toolTip7.InitialDelay=100;
    		toolTip7.ReshowDelay=500;
    		toolTip7.ShowAlways=true;
    		toolTip7.SetToolTip(this.button9, "Stop all jobs");
       		    		
    		ToolTip toolTip8 = new ToolTip();
   			toolTip8.AutoPopDelay=5000;
    		toolTip8.InitialDelay=100;
    		toolTip8.ReshowDelay=500;
    		toolTip8.ShowAlways=true;
    		toolTip8.SetToolTip(this.button10, "Start all jobs (continuous reading)");
		}
		

        private void UpdateListBox(object sender)
        {
            treeView1.Nodes.Clear();
            TreeNode rootNode = new TreeNode("Modbus connections");
            treeView1.Nodes.Add(rootNode);
            foreach (ConnectionProperties connectionProperty in easyModbusManager.connectionPropertiesList)
            {
            	TreeNode treeNode = null;
            	if (connectionProperty.ModbusTypeProperty == ModbusType.ModbusTCP) 
            		treeNode = new TreeNode("Modbus-TCP Connection: " + connectionProperty.ConnectionName +"; IP-Address: "+connectionProperty.ModbusTCPAddress + "; Port: "+connectionProperty.Port);
            	else
            		treeNode = new TreeNode("Modbus-RTU Connection: " + connectionProperty.ConnectionName +"; COM-Port: "+connectionProperty.ComPort);
            	foreach (FunctionProperties functionProperty in connectionProperty.FunctionPropertiesList)
            	{
            		treeNode.Nodes.Add("Function code: " + functionProperty.FunctionCodeRead + "; Starting Address: "+functionProperty.StartingAdress + "; Quantity: "+functionProperty.Quantity);
            	}
            	rootNode.Nodes.Add(treeNode);
            
            }
            treeView1.ExpandAll();
            this.UpdateDataGridViewItems();
            
        }
        
        private void UpdateDataGridView(object sender)
        {
        	for (int i = 0; i < dataGridView1.RowCount-1; i++)
        	{
        			foreach (ConnectionProperties connectionProperty in easyModbusManager.connectionPropertiesList)
					{
        				if (dataGridView1[0,i].Value != null)
        				if (connectionProperty.ConnectionName.Equals(dataGridView1[0,i].Value.ToString()))
        				{
        					foreach (FunctionProperties functionProperty in connectionProperty.FunctionPropertiesList)
        					{

        						if (dataGridView1[1,i].Value != null)
                                    
                                    for (int j = 0; j < functionProperty.Quantity; j++)
        								if (EasyModbusManager.getAddress(functionProperty.FunctionCodeRead, functionProperty.StartingAdress, functionProperty.Quantity, j).Equals(dataGridView1[1,i].Value.ToString()))
        								{
                                            functionProperty.DataGridRow = i;
                                            if (functionProperty.values.GetType().Equals(typeof(Boolean[])))
        										dataGridView1[4,i].Value=((bool[]) functionProperty.values)[j].ToString();
        									else
        									{
                                                if (dataGridView1[3, i].Value != null)
                                                    if (dataGridView1[3, i].Value.Equals("UINT16 (0...65535)"))
                                                        if (((int[])functionProperty.values)[j] < 0)
                                                            dataGridView1[4, i].Value = (65536 + ((int[])functionProperty.values)[j]).ToString();
                                                        else
                                                            dataGridView1[4, i].Value = ((int[])functionProperty.values)[j].ToString();
                                                    else if (dataGridView1[3, i].Value.Equals("ASCII"))
                                                    {
                                                        
                                                        string str = "";
                                                        for (int tt = 0; tt < ((int[])functionProperty.values).Length; tt++)
                                                        {
                                                            int value = ((int[])functionProperty.values)[tt];
                                                            str += "" + (char)((value & 0xff00) >> 8) + (char)((value & 0x00ff));
                                                        }
                                                        dataGridView1[4, i].Value = "" + str;
                                                    } 
                                                    else
                                                        dataGridView1[4, i].Value = ((int[])functionProperty.values)[j].ToString();
                                                else
                                                    dataGridView1[4, i].Value = ((int[])functionProperty.values)[j].ToString();
        									}
        								}
        					}
					   	
        				}
					}
        	}
        }
        


		void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
		{			
			if (treeView1.SelectedNode.Level == 1)	
			{
                //conext menu strip
				stopCurrentJobToolStripMenuItem.Visible = true;
				stopAllJobsToolStripMenuItem.Visible = true;
				startAllJobsToolStripMenuItem.Visible = true;
				startSingleJobToolStripMenuItem.Visible = true;
				startAllJobsToolStripMenuItem.Visible = true;
				startSingleJobToolStripMenuItem.Visible = true;
				addFunctionCodeToolStripMenuItem.Visible = true;	
				deleteConnectionToolStripMenuItem.Visible = true;
				editConnectionToolStripMenuItem.Visible = true;
				addConnectionToolStripMenuItem.Visible = true;
				deleteFunctionCodeToolStripMenuItem.Visible = false;
				editFunctionCodeToolStripMenuItem.Visible = false;
				updateAllValuesToolStripMenuItem.Visible = true;
				updateValuesToolStripMenuItem.Visible = true;
				toolStripSeparator1.Visible = true;
				toolStripSeparator2.Visible = true;
				toolStripSeparator3.Visible = true;
                //tool strip
                addConnectionToolStripMenuItem1.Visible = true;
                editConnectionToolStripMenuItem1.Visible = true;
                deleteConnectionToolStripMenuItem1.Visible = true;
                addFunctionCodeToolStripMenuItem1.Visible = true;
                editFunctionCodeToolStripMenuItem1.Visible = false;
                deleteFunctionCodeToolStripMenuItem1.Visible = false;
                startAllJobsToolStripMenuItem1.Visible = true;
                stopAllJobsToolStripMenuItem1.Visible = true;
                updateAllValuessingleReadToolStripMenuItem.Visible = true;
                toolStripSeparator4.Visible = true;
                toolStripSeparator5.Visible = true;
                toolStripSeparator6.Visible = true;

            }
			else if (treeView1.SelectedNode.Level == 2)
			{
				stopCurrentJobToolStripMenuItem.Visible = true;
				stopAllJobsToolStripMenuItem.Visible = true;
				startAllJobsToolStripMenuItem.Visible = true;
				startSingleJobToolStripMenuItem.Visible = true;
				addConnectionToolStripMenuItem.Visible = true;
				editConnectionToolStripMenuItem.Visible = false;
				deleteConnectionToolStripMenuItem.Visible = false;
				editFunctionCodeToolStripMenuItem.Visible = true;
				deleteFunctionCodeToolStripMenuItem.Visible = true;
				startAllJobsToolStripMenuItem.Visible = true;
				startSingleJobToolStripMenuItem.Visible = true;
				updateAllValuesToolStripMenuItem.Visible = true;
				updateValuesToolStripMenuItem.Visible = true;
				toolStripSeparator1.Visible = true;
				toolStripSeparator2.Visible = true;
				toolStripSeparator3.Visible = true;
                //tool strip
                addConnectionToolStripMenuItem1.Visible = true;
                editConnectionToolStripMenuItem1.Visible = false;
                deleteConnectionToolStripMenuItem1.Visible = false;
                addFunctionCodeToolStripMenuItem1.Visible = true;
                editFunctionCodeToolStripMenuItem1.Visible = true;
                deleteFunctionCodeToolStripMenuItem1.Visible = true;
                startAllJobsToolStripMenuItem1.Visible = true;
                stopAllJobsToolStripMenuItem1.Visible = true;
                updateAllValuessingleReadToolStripMenuItem.Visible = true;
                toolStripSeparator4.Visible = true;
                toolStripSeparator5.Visible = true;
                toolStripSeparator6.Visible = true;
            }
			else 
			{
                //context menu strip
				stopCurrentJobToolStripMenuItem.Visible = false;
				stopAllJobsToolStripMenuItem.Visible = false;
				startAllJobsToolStripMenuItem.Visible = true;
				startSingleJobToolStripMenuItem.Visible = false;
				addConnectionToolStripMenuItem.Visible = true;
				editConnectionToolStripMenuItem.Visible = false;
				deleteConnectionToolStripMenuItem.Visible = false;
				addFunctionCodeToolStripMenuItem.Visible = false;	
				deleteConnectionToolStripMenuItem.Visible = false;
				editConnectionToolStripMenuItem.Visible = false;
				editFunctionCodeToolStripMenuItem.Visible = false;
				deleteFunctionCodeToolStripMenuItem.Visible = false;
				startAllJobsToolStripMenuItem.Visible = false;
				startSingleJobToolStripMenuItem.Visible = false;
				updateAllValuesToolStripMenuItem.Visible = true;
				updateValuesToolStripMenuItem.Visible = false;
				toolStripSeparator1.Visible = false;
				toolStripSeparator2.Visible = false;
				toolStripSeparator3.Visible = false;
                //tool strip
                addConnectionToolStripMenuItem1.Visible = true;
                editConnectionToolStripMenuItem1.Visible = false;
                deleteConnectionToolStripMenuItem1.Visible = false;
                addFunctionCodeToolStripMenuItem1.Visible = false;
                editFunctionCodeToolStripMenuItem1.Visible = false;
                deleteFunctionCodeToolStripMenuItem1.Visible = false;
                startAllJobsToolStripMenuItem1.Visible = true;
                stopAllJobsToolStripMenuItem1.Visible = false;
                updateAllValuessingleReadToolStripMenuItem.Visible = true;
                toolStripSeparator4.Visible = false;
                toolStripSeparator5.Visible = false;
                toolStripSeparator6.Visible = false;

            }
		}
		
		
		
		void AddFunctionCodeToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode != null)
			{
				AddFunctionCodeForm addFunctionCodeForm = new AddFunctionCodeForm(easyModbusManager, treeView1.SelectedNode.Index);
				addFunctionCodeForm.Show();
			}
		}
		
		void UpdateValuesToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
			{
				easyModbusManager.GetValues(easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index], treeView1.SelectedNode.Index);		
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, "Exception Reading values", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		void SplitContainer1SplitterMoved(object sender, SplitterEventArgs e)
		{
			treeView1.Width = splitContainer1.Panel1.Width - 5;
            tabControl1.Width = splitContainer1.Panel2.Width - 80;
            
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode == null) return;
			dataGridView1.AllowUserToAddRows = false;
			if (treeView1.SelectedNode.Level == 1)
			{
				dataGridView1.Rows.Add();
				dataGridView1[0,dataGridView1.Rows.Count-1].Value=easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Index].ConnectionName;				
			}
			if (treeView1.SelectedNode.Level == 2)
			{
				dataGridView1.Rows.Add();
				dataGridView1[0,dataGridView1.Rows.Count-1].Value=easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index].ConnectionName;				
			}
				dataGridView1.AllowUserToAddRows = true;
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode == null) return;
			if (treeView1.SelectedNode.Level == 2)
			{
				
				FunctionProperties functionProperty = easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index].FunctionPropertiesList[treeView1.SelectedNode.Index];
				ConnectionProperties connectionProperty = easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index];
				
				dataGridView1.AllowUserToAddRows = false;
				for (int i = 0; i < functionProperty.Quantity; i++)
				{
				    dataGridView1.Rows.Add();
					dataGridView1[0,dataGridView1.Rows.Count-1].Value = connectionProperty.ConnectionName;
					dataGridView1.ClearSelection();
					dataGridView1.CurrentCell = null;
					DataGridView1CellClick(null, null);
				
					// DataGridViewComboBoxCell cbCell = (DataGridViewComboBoxCell)dataGridView1.Rows[row].Cells[1];
					switch (functionProperty.FunctionCodeRead)
					{
        			case FunctionCodeRd.ReadCoils: dataGridView1[1,dataGridView1.Rows.Count-1].Value = "0x"+(functionProperty.StartingAdress+i+1).ToString();
						break;
					case FunctionCodeRd.ReadDiscreteInputs: dataGridView1[1,dataGridView1.Rows.Count-1].Value = "1x"+(functionProperty.StartingAdress+i+1).ToString();
						break;
					case FunctionCodeRd.ReadHoldingRegisters: dataGridView1[1,dataGridView1.Rows.Count-1].Value = "4x"+(functionProperty.StartingAdress+i+1).ToString();
						break;
					case FunctionCodeRd.ReadInputRegisters: dataGridView1[1,dataGridView1.Rows.Count-1].Value = "3x"+(functionProperty.StartingAdress+i+1).ToString();
						break;
					default: break;
					
			}
					
									
										
				}
				dataGridView1.AllowUserToAddRows = true;
			}
			
	
		}
		
		
		private void UpdateDataGridViewItems()
		{
		

			//*********************************************************************Set combo box for Connection
			string[] connectionNames = new string[dataGridView1.RowCount];
			//Save data from column "Connection name"
			for (int i = 0; i < dataGridView1.RowCount; i++) 
			{
				if (dataGridView1[0, i].Value != null)
					connectionNames[i] = dataGridView1[0, i].Value.ToString();
			}
			DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn();
			dataGridView1.Columns.RemoveAt(0);
            cmb.HeaderText = "Connection";
            if (easyModbusManager.connectionPropertiesList.Count > 0)
            	cmb.MaxDropDownItems = easyModbusManager.connectionPropertiesList.Count;
            else
            	cmb.MaxDropDownItems = 1;
            foreach (ConnectionProperties connectionProperty in easyModbusManager.connectionPropertiesList)
            {
            	cmb.Items.Add(connectionProperty.ConnectionName);
            }
            dataGridView1.Columns.Insert(0,cmb);
            //Restore  data to column "connection name"
            for (int i = 0; i < dataGridView1.RowCount; i++) 
			{
				dataGridView1[0, i].Value = connectionNames[i] ;
			}
            
 
            
            
            
           // 			DataGridViewComboBoxCell cbCell = (DataGridViewComboBoxCell)dataGridView1.Rows[0].Cells[3];
		//	cbCell.Items.Add("www");
		}

	     void DataGridView1CellClick(object sender, DataGridViewCellEventArgs e)
		{
	            //********************************************************************Set combo box for address
	            int row = 0;
	            if (dataGridView1.CurrentCell == null)
	            	row = dataGridView1.RowCount-1;
	            else 	
            row = dataGridView1.CurrentCell.RowIndex;
            DataGridViewComboBoxCell cbCell = (DataGridViewComboBoxCell)dataGridView1.Rows[row].Cells[1];
            string selectedCell = null;
            if (cbCell.Value != null)
             	selectedCell = cbCell.Value.ToString();
            cbCell.Value = null;
            cbCell.Items.Clear();
            for (int j = 0; j < easyModbusManager.connectionPropertiesList.Count; j++)
            {
            	if  (dataGridView1[0,row].Value != null)
            	if (easyModbusManager.connectionPropertiesList[j].ConnectionName == dataGridView1[0,row].Value.ToString())
            	{
            			
            		for (int k = 0; k < easyModbusManager.connectionPropertiesList[j].FunctionPropertiesList.Count; k++)
            		{
            			int currentAddress = 0;
            			for (int l = 0; l < easyModbusManager.connectionPropertiesList[j].FunctionPropertiesList[k].Quantity; l++)
            			{
            				currentAddress = easyModbusManager.connectionPropertiesList[j].FunctionPropertiesList[k].StartingAdress + l + 1;
            				switch (easyModbusManager.connectionPropertiesList[j].FunctionPropertiesList[k].FunctionCodeRead)
            				{
            						
            					case FunctionCodeRd.ReadCoils:
            						cbCell.Items.Add("0x" + currentAddress.ToString());
            						break;
            					case FunctionCodeRd.ReadDiscreteInputs:
            						cbCell.Items.Add("1x" + currentAddress.ToString());
            						break;
            					case FunctionCodeRd.ReadHoldingRegisters:
            						cbCell.Items.Add("4x" + currentAddress.ToString());
            						break;
            					case FunctionCodeRd.ReadInputRegisters:
            						cbCell.Items.Add("3x" + currentAddress.ToString());
            						break;
            					default: break;
            				
            				}
            				}
            			}
            		}
            }
            if (selectedCell != null)
            {
            	for (int i = 0; i < cbCell.Items.Count; i++)
            		if (cbCell.Items[i].ToString() == selectedCell)
            			cbCell.Value = selectedCell;
            }
            //********************************************************************Set combo box for datatype
            if (dataGridView1[1,row].Value != null)
            {
            cbCell = (DataGridViewComboBoxCell)dataGridView1.Rows[row].Cells[3];
            selectedCell = null;
            if (cbCell.Value != null)
             	selectedCell = cbCell.Value.ToString();
            cbCell.Value = null;
            cbCell.Items.Clear();
                if (dataGridView1[1, row].Value.ToString().StartsWith("0x") | dataGridView1[1, row].Value.ToString().StartsWith("1x"))
                {
                    cbCell.Items.Add("BOOL (FALSE...TRUE)");
                    cbCell.Value = "BOOL (FALSE...TRUE)";
                }
            if (dataGridView1[1,row].Value.ToString().StartsWith("3x")|dataGridView1[1,row].Value.ToString().StartsWith("4x"))
            {
            	cbCell.Items.Add("INT16 (-32768...32767)");
            	cbCell.Items.Add("UINT16 (0...65535)");
                cbCell.Items.Add("ASCII");
            }
            if (selectedCell != null)
            {
            	for (int i = 0; i < cbCell.Items.Count; i++)
            		if (cbCell.Items[i].ToString() == selectedCell)
            			cbCell.Value = selectedCell;
            }
          
            }
            
		}
		void DeleteConnectionToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode != null)
				if (treeView1.SelectedNode.Level == 1)
				{
					easyModbusManager.RemoveConnection(treeView1.SelectedNode.Index);
				}	
		}
		void EditConnectionToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode != null)
			{
			int index = treeView1.SelectedNode.Index;
			if (treeView1.SelectedNode.Level == 1)
			{
				AddConnectionForm addConnectionForm = new AddConnectionForm(easyModbusManager, index);
				addConnectionForm.ShowDialog();
			}
			}
		}
		void EditFunctionCodeToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (treeView1.SelectedNode == null) return;
			int indexFunction = treeView1.SelectedNode.Index;
			int indexConnection = treeView1.SelectedNode.Parent.Index;
			if (treeView1.SelectedNode.Level == 2)
			{
				AddFunctionCodeForm addFunctionCodeForm = new AddFunctionCodeForm(easyModbusManager, indexConnection, indexFunction);
				addFunctionCodeForm.ShowDialog();
			}
			
		}
		void UpdateAllValuesToolStripMenuItemClick(object sender, EventArgs e)
		{
			try
			{
				foreach(ConnectionProperties connectionProperty in easyModbusManager.connectionPropertiesList)
					for (int i = 0; i < connectionProperty.FunctionPropertiesList.Count; i++)
						easyModbusManager.GetValues(connectionProperty, i);		
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, "Exception Reading values", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		void DeleteFunctionCodeToolStripMenuItemClick(object sender, EventArgs e)
		{
				if (treeView1.SelectedNode != null)
				if (treeView1.SelectedNode.Level == 2)
				{
					easyModbusManager.RemoveFunctionProperty(treeView1.SelectedNode.Parent.Index, treeView1.SelectedNode.Index);
				}	
		}
		void MainFormSizeChanged(object sender, EventArgs e)
		{
			treeView1.Height = this.Height - 112;
            tabControl1.Height = this.Height - 70;
			dataGridView1.Height = this.tabControl1.Height - 30;
			dataGridView1.Width = this.splitContainer1.Panel2.Width - 85;
		}
		void Button3Click(object sender, EventArgs e)
		{
			this.AddConnectionToolStripMenuItemClick(null, null);
		}
		void Button4Click(object sender, EventArgs e)
		{
			this.DeleteConnectionToolStripMenuItemClick(null, null);
		}
		void Button5Click(object sender, EventArgs e)
		{
			this.EditConnectionToolStripMenuItemClick(null, null);
		}
		void Button6Click(object sender, EventArgs e)
		{
			this.AddFunctionCodeToolStripMenuItemClick(null, null);
		}
		void Button7Click(object sender, EventArgs e)
		{
			this.DeleteFunctionCodeToolStripMenuItemClick(null, null);
		}
		void Button8Click(object sender, EventArgs e)
		{
			this.EditFunctionCodeToolStripMenuItemClick(null, null);
		}
		void Button9Click(object sender, EventArgs e)
		{
			this.StopAllJobsToolStripMenuItemClick(null, null);
		}
		
		void StartSingleJobToolStripMenuItemClick(object sender, EventArgs e)
		{
			GetValuesThreadObject getValuesThreadObject = new GetValuesThreadObject();
			getValuesThreadObject.connectionProperty = easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index];
			getValuesThreadObject.functionPropertyIndex = treeView1.SelectedNode.Index;
            if (getValuesThreadObject.connectionProperty.timer == null)
			    getValuesThreadObject.connectionProperty.timer = new System.Threading.Timer(GetValuesThread, getValuesThreadObject , easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index].CycleTime, easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index].CycleTime);
            else
                getValuesThreadObject.connectionProperty.timer.Change(easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index].CycleTime, easyModbusManager.connectionPropertiesList[treeView1.SelectedNode.Parent.Index].CycleTime);
            getValuesThreadObject.connectionProperty.modbusClient.ReceiveDataChanged -= new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateTextBoxReceive);
            getValuesThreadObject.connectionProperty.modbusClient.SendDataChanged -= new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateTextBoxSend);

        }

        private bool locked;
		object locker = new object();
		private void GetValuesThread(object obj)
		{
			if (locked)
				System.Threading.Thread.Sleep(100);
            if (!locked)
            locked = true;
			lock (locker) {
                
				GetValuesThreadObject getValuesThreadObject = (GetValuesThreadObject)obj;
				try {
					if (getValuesThreadObject.functionPropertyIndex != 0)
						easyModbusManager.GetValues(getValuesThreadObject.connectionProperty, getValuesThreadObject.functionPropertyIndex);
					else
						easyModbusManager.GetValues(getValuesThreadObject.connectionProperty);
				} catch (Exception exc) {
					getValuesThreadObject.connectionProperty.timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
					getValuesThreadObject.connectionProperty.modbusClient.Disconnect();
					MessageBox.Show(exc.Message, "Exception Reading values", MessageBoxButtons.OK, MessageBoxIcon.Error);

				} finally {
					UpdateNodesConnectedStatus();
					locked = false;
				}
				locked = false;
			}
		}
		
		void StartAllJobsToolStripMenuItemClick(object sender, EventArgs e)
		{		
			foreach(ConnectionProperties connectionProperty in easyModbusManager.connectionPropertiesList)
			{
                connectionProperty.modbusClient.ReceiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateTextBoxReceive);
                connectionProperty.modbusClient.SendDataChanged += new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateTextBoxSend);
                GetValuesThreadObject getValuesThreadObject = new GetValuesThreadObject();
				getValuesThreadObject.connectionProperty = connectionProperty;
                if (connectionProperty.timer == null)
                    connectionProperty.timer = new System.Threading.Timer(GetValuesThread, getValuesThreadObject , connectionProperty.CycleTime, connectionProperty.CycleTime);
                else
                    connectionProperty.timer.Change(connectionProperty.CycleTime, connectionProperty.CycleTime);
            }
			UpdateNodesConnectedStatus();
		}
		void Button10Click(object sender, EventArgs e)
		{
			this.StartAllJobsToolStripMenuItemClick(null, null);
		}
		
		void UpdateNodesConnectedStatus()
		{	
            if (treeView1.Nodes.Count > 0)
			foreach (TreeNode tn1 in treeView1.Nodes[0].Nodes)
			{
				foreach (TreeNode tn2 in tn1.Nodes)
				{
					if (easyModbusManager.connectionPropertiesList[tn1.Index].modbusClient.Connected)
						tn2.BackColor = Color.Green;
					else
						tn2.BackColor = Color.White;			
				}
			}
			
		}
		void StopAllJobsToolStripMenuItemClick(object sender, EventArgs e)
		{
			foreach(ConnectionProperties connectionProperty in easyModbusManager.connectionPropertiesList)
			{
                connectionProperty.modbusClient.ReceiveDataChanged -= new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateTextBoxReceive);
                connectionProperty.modbusClient.SendDataChanged -= new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateTextBoxSend);
                connectionProperty.timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
				connectionProperty.modbusClient.Disconnect();
			}
			UpdateNodesConnectedStatus();		
		}
		
		
	public class GetValuesThreadObject
	{
		public ConnectionProperties connectionProperty;
		public int functionPropertyIndex;
	}

    private void stopCurrentJobToolStripMenuItem_Click(object sender, EventArgs e)
    {
        int indexJobToStop = 0;
        if (treeView1.SelectedNode.Level == 1)
        {
            indexJobToStop = treeView1.SelectedNode.Index;
            easyModbusManager.connectionPropertiesList[indexJobToStop].timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            easyModbusManager.connectionPropertiesList[indexJobToStop].modbusClient.Disconnect();
                easyModbusManager.connectionPropertiesList[indexJobToStop].modbusClient.ReceiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateTextBoxReceive);
                easyModbusManager.connectionPropertiesList[indexJobToStop].modbusClient.SendDataChanged += new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateTextBoxSend);
            }
        if (treeView1.SelectedNode.Level == 2)
            {
                indexJobToStop = treeView1.SelectedNode.Parent.Index;
                easyModbusManager.connectionPropertiesList[indexJobToStop].timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                easyModbusManager.connectionPropertiesList[indexJobToStop].modbusClient.Disconnect();
                easyModbusManager.connectionPropertiesList[indexJobToStop].modbusClient.ReceiveDataChanged += new EasyModbus.ModbusClient.ReceiveDataChangedHandler(UpdateTextBoxReceive);
                easyModbusManager.connectionPropertiesList[indexJobToStop].modbusClient.SendDataChanged += new EasyModbus.ModbusClient.SendDataChangedHandler(UpdateTextBoxSend);

            }
            UpdateNodesConnectedStatus();
     }

        private void saveWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            easyModbusManager.WriteXML(dataGridView1);
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            easyModbusManager.dataGridViewChanged += new EasyModbusManager.DataGridViewChanged(DataGridViewLinesChanged);
            easyModbusManager.ReadXML(dataGridView1);
        }

        private void DataGridViewLinesChanged(object sender)
        {
            dataGridView1.ClearSelection();
            dataGridView1.CurrentCell = null;

            DataGridView1CellClick(null, null);
        }

        delegate void UpdateReceiveDataCallback(EasyModbus.ModbusClient sender);
        void UpdateTextBoxReceive(object sender)
        {
            EasyModbus.ModbusClient modbusClient = (EasyModbus.ModbusClient)sender;
            if (textBox1.InvokeRequired)
            {
                UpdateReceiveDataCallback d = new UpdateReceiveDataCallback(UpdateTextBoxReceive);
                this.Invoke(d, new object[] { modbusClient });
            }
            else
            {
                textBox1.AppendText("From: "+modbusClient.IPAddress.ToString()+ " Rx: ");
                string hex = BitConverter.ToString(modbusClient.receiveData);
                hex = hex.Replace("-", " ");
                textBox1.AppendText(hex);
                textBox1.AppendText(System.Environment.NewLine);
            }
        }


        delegate void UpdateSendDataCallback(EasyModbus.ModbusClient sender);
        void UpdateTextBoxSend(object sender)
        {
            EasyModbus.ModbusClient modbusClient = (EasyModbus.ModbusClient)sender;
            if (textBox1.InvokeRequired)
            {
                UpdateSendDataCallback d = new UpdateSendDataCallback(UpdateTextBoxSend);
                this.Invoke(d, new object[] { modbusClient });
            }
            else
            {
                textBox1.AppendText("To: "+modbusClient.IPAddress.ToString() + "Tx: ");
                string hex = BitConverter.ToString(modbusClient.sendData);
                hex = hex.Replace("-", " ");
                textBox1.AppendText(hex);
                textBox1.AppendText(System.Environment.NewLine);
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // check if we are in column value
            if (e.ColumnIndex == 4)
            {
                int idx = e.RowIndex;
                FunctionProperties functionProperties= easyModbusManager.FindPropertyFromGrid(idx);
                if (functionProperties != null)
                {
                    string str = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    int[] values = EasyModbusManager.StrToValues(functionProperties, str);
                    easyModbusManager.WriteToServer(functionProperties, values);
                }
            }
            
        }
    }

}
