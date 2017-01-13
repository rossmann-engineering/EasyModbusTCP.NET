/*
 * Attribution-NonCommercial-NoDerivatives 4.0 International (CC BY-NC-ND 4.0)
 * User: srossmann
 * Date: 25.11.2015
 * Time: 07:35
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
                    if (connectionProperties.ModbusTypeProperty == ModbusType.ModbusTCP)
                    {
						connectionProperties.modbusClient = new EasyModbus.ModbusClient();
						connectionProperties.modbusClient.UnitIdentifier = (byte)connectionProperties.SlaveID;
                    }
                    else
                    {
                    	connectionProperties.modbusClient = new EasyModbus.ModbusClient(connectionProperties.ComPort);
                    	connectionProperties.modbusClient.UnitIdentifier = (byte)connectionProperties.SlaveID;
                    }
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
		
