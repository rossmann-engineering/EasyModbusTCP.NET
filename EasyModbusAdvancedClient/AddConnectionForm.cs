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
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace EasyModbusAdvancedClient
{
	/// <summary>
	/// Description of AddConnectionForm.
	/// </summary>
	public partial class AddConnectionForm : Form
	{
		private EasyModbusManager easyModbusManager;
		private ConnectionProperties connectionProperties = new ConnectionProperties();
		private bool editMode = false;
		private int indexToEdit;
		public AddConnectionForm(EasyModbusManager easyModbusManager)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			this.easyModbusManager = easyModbusManager;
			InitializeComponent();
			connectionProperties.ConnectionName = "Connection #"+(easyModbusManager.connectionPropertiesList.Count+1).ToString();
			propertyGrid1.SelectedObject = connectionProperties;
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		public AddConnectionForm(EasyModbusManager easyModbusManager, int indexToEdit)
		{
			this.easyModbusManager = easyModbusManager;
			InitializeComponent();
			connectionProperties.ConnectionName = easyModbusManager.connectionPropertiesList[indexToEdit].ConnectionName;
			connectionProperties.CycleTime = easyModbusManager.connectionPropertiesList[indexToEdit].CycleTime;
			connectionProperties.CyclicFlag = easyModbusManager.connectionPropertiesList[indexToEdit].CyclicFlag;
			connectionProperties.ModbusTCPAddress = easyModbusManager.connectionPropertiesList[indexToEdit].ModbusTCPAddress;
			connectionProperties.Port = easyModbusManager.connectionPropertiesList[indexToEdit].Port;
			connectionProperties.ComPort = easyModbusManager.connectionPropertiesList[indexToEdit].ComPort;		
			connectionProperties.SlaveID = easyModbusManager.connectionPropertiesList[indexToEdit].SlaveID;		
			connectionProperties.ModbusTypeProperty = easyModbusManager.connectionPropertiesList[indexToEdit].ModbusTypeProperty;
			connectionProperties.modbusClient =  easyModbusManager.connectionPropertiesList[indexToEdit].modbusClient;
			propertyGrid1.SelectedObject = connectionProperties;
			editMode = true;
			this.indexToEdit = indexToEdit;
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			this.Close();
		}
		
		void Button1Click(object sender, EventArgs e)
		{
            try
            {
                if (!editMode)
                {
                    easyModbusManager.AddConnection(connectionProperties);
                }
                else
                    easyModbusManager.EditConnection(connectionProperties, indexToEdit);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Exception Adding connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Close();
		}
	}
	
	public enum ModbusType
	{
		ModbusTCP = 0,
		ModbusRTU = 1
	}
	
	public class ConnectionProperties
	{
		    	
		ModbusType modbusType;
    
   		[Browsable(true)]                       
   		[Category("Modbus Type")] 
    	[Description("Modbus TCP or Modbus RTU")]      
   		[DisplayName("Modbus Type")]       
    	public ModbusType ModbusTypeProperty
    	{
        	get { return modbusType; }
        	set { modbusType = value; }
    	}
		
		string connectionName = "Connection #1"; 
    
   		[Browsable(true)]                       
   		[Category("Connection properties")] 
    	[Description("Unique Connection Name")]      
   		[DisplayName("Connection Name")]       
    	public string ConnectionName
    	{
        	get { return connectionName; }
        	set { connectionName = value; }
    	}
	
		string modbusTCPAddress = "127.0.0.1"; 
    
   		[Browsable(true)]                        
   		[Category("Connection properties")] 
    	[Description("IP-Address of Modbus-TCP Server")]           
   		[DisplayName("IP Address of Modbus-TCP Server")]      
    	public string ModbusTCPAddress
    	{
        	get { return modbusTCPAddress; }
        	set { modbusTCPAddress = value; }
    	}
    	
    	
		int port = 502;   
   		[Browsable(true)]                       
   		[Category("Connection properties")] 
    	[Description("Port")]           
   		[DisplayName("Port")]       
    	public int Port
    	{
        	get { return port; }
        	set { port = value; }
    	}
    	
 		string comPort = "COM1";   
   		[Browsable(true)]                       
   		[Category("Connection properties")] 
    	[Description("Serial COM-Port")]           
   		[DisplayName("Serial COM-Port")]       
    	public string ComPort
    	{
        	get { return comPort; }
        	set { comPort = value; }
    	}
    	
 		int slaveID = 1;   
   		[Browsable(true)]                       
   		[Category("Connection properties")] 
    	[Description("Slave ID")]           
   		[DisplayName("Slave ID")]       
    	public int SlaveID
    	{
        	get { return slaveID; }
        	set { slaveID = value; }
    	}
    	  	    
        
		bool cyclicFlag = false;
		[Browsable(true)]                       
   		[Category("Connection properties")] 
    	[Description("Enable cyclic data exchange")]           
   		[DisplayName("Enable cyclic data exchange")]   
		public bool CyclicFlag
		{
			get {return cyclicFlag;}
			set {cyclicFlag = value;}
		}
		
		int cycleTime = 100;
		[Browsable(true)]                       
   		[Category("Connection properties")] 
    	[Description("Cycle time for cyclic data exchange")]           
   		[DisplayName("Cycle time")]   
		public int CycleTime
		{
			get {return cycleTime;}
			set {cycleTime = value;}
		}             
   		
    	System.Collections.Generic.List<FunctionProperties> functionPropertiesList = new System.Collections.Generic.List<FunctionProperties>();
    	[Browsable(false)]   
    	public System.Collections.Generic.List<FunctionProperties> FunctionPropertiesList
    	{
    		get { return functionPropertiesList; }
        	set { functionPropertiesList = value; }
    	}	
    	
    	public EasyModbus.ModbusClient modbusClient;
    	public System.Threading.Timer timer;
	}	
}
		
