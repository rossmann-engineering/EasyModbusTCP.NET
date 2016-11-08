/*
 * Created by SharpDevelop.
 * User: srossmann
 * Date: 25.11.2015
 * Time: 07:35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
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
                    easyModbusManager.AddConnection(connectionProperties);
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
    	
    	public EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient();
    	public System.Threading.Timer timer;
	}	
}
		
