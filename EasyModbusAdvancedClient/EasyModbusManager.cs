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
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using EasyModbus;

namespace EasyModbusAdvancedClient
{
    /// <summary>
    /// Description of EasyModbusManager.
    /// </summary>
    public class EasyModbusManager
    {

        EasyModbus.ModbusClient modbusClient = new EasyModbus.ModbusClient();
        public List<ConnectionProperties> connectionPropertiesList = new List<ConnectionProperties>();

        public EasyModbusManager()
        {
        }

        public void AddConnection(ConnectionProperties connectionProperties)
        {
            foreach (ConnectionProperties connectionProperty in connectionPropertiesList)
            {
                if (connectionProperties.ConnectionName == connectionProperty.ConnectionName)
                {
                    throw new Exception("Duplicate connection Name detected");
                }
            }

            // create modbus client accordingly
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

            connectionPropertiesList.Add(connectionProperties);
            if (connectionPropertiesListChanged != null)
                connectionPropertiesListChanged(this);
        }

        public void RemoveConnection(int connectionNumber)
        {
            connectionPropertiesList.RemoveAt(connectionNumber);
            if (connectionPropertiesListChanged != null)
                connectionPropertiesListChanged(this);
        }

        public void EditConnection(ConnectionProperties connectionProperty, int connectionNumber)
        {
            connectionPropertiesList[connectionNumber] = connectionProperty;
            if (connectionPropertiesListChanged != null)
                connectionPropertiesListChanged(this);
            foreach (ConnectionProperties connectionPropertyListEntry in connectionPropertiesList)
            {
                if (connectionPropertyListEntry.ConnectionName == connectionProperty.ConnectionName)
                    throw new Exception("Duplicate connection Name detcted");
                return;
            }
        }

        public void AddFunctionProperty(FunctionProperties functionProperty, int connectionNumber)
        {
            // create link to connection
            functionProperty.Connection = connectionPropertiesList[connectionNumber];
            // add to list
            connectionPropertiesList[connectionNumber].FunctionPropertiesList.Add(functionProperty);
            if (connectionPropertiesListChanged != null)
                connectionPropertiesListChanged(this);
        }

        public void RemoveFunctionProperty(int connectionNumber, int functionNumber)
        {
            connectionPropertiesList[connectionNumber].FunctionPropertiesList.RemoveAt(functionNumber);
            if (connectionPropertiesListChanged != null)
                connectionPropertiesListChanged(this);
        }

        public void EditFunctionProperty(FunctionProperties functionProperty, int connectionNumber, int functionNumber)
        {
            connectionPropertiesList[connectionNumber].FunctionPropertiesList[functionNumber] = functionProperty;
            if (connectionPropertiesListChanged != null)
                connectionPropertiesListChanged(this);
        }

        public delegate void ValuesChanged(object sender);
        public event ValuesChanged valuesChanged;
        public void GetValues(ConnectionProperties connectionProperties, int functionPropertyID)
        {

            modbusClient = connectionProperties.modbusClient;          
            if (!modbusClient.Connected)
            {
                modbusClient.IPAddress = connectionProperties.ModbusTCPAddress;
                modbusClient.Port = connectionProperties.Port;
                modbusClient.Connect();
            }

            switch (connectionProperties.FunctionPropertiesList[functionPropertyID].FunctionCodeRead)
            {
                case FunctionCodeRd.ReadCoils:
                    connectionProperties.FunctionPropertiesList[functionPropertyID].values = modbusClient.ReadCoils(connectionProperties.FunctionPropertiesList[functionPropertyID].StartingAdress, connectionProperties.FunctionPropertiesList[functionPropertyID].Quantity);
                    break;
                case FunctionCodeRd.ReadDiscreteInputs:
                    connectionProperties.FunctionPropertiesList[functionPropertyID].values = modbusClient.ReadDiscreteInputs(connectionProperties.FunctionPropertiesList[functionPropertyID].StartingAdress, connectionProperties.FunctionPropertiesList[functionPropertyID].Quantity);
                    break;
                case FunctionCodeRd.ReadHoldingRegisters:
                    connectionProperties.FunctionPropertiesList[functionPropertyID].values = modbusClient.ReadHoldingRegisters(connectionProperties.FunctionPropertiesList[functionPropertyID].StartingAdress, connectionProperties.FunctionPropertiesList[functionPropertyID].Quantity);
                    break;
                case FunctionCodeRd.ReadInputRegisters:
                    connectionProperties.FunctionPropertiesList[functionPropertyID].values = modbusClient.ReadInputRegisters(connectionProperties.FunctionPropertiesList[functionPropertyID].StartingAdress, connectionProperties.FunctionPropertiesList[functionPropertyID].Quantity);
                    break;
                default: break;
            }
            if (valuesChanged != null)
                valuesChanged(this);
        }

        public void GetValues(ConnectionProperties connectionProperties)
        {

            modbusClient = connectionProperties.modbusClient;
            if (!modbusClient.Connected)
            {
                modbusClient.IPAddress = connectionProperties.ModbusTCPAddress;
                modbusClient.Port = connectionProperties.Port;
                modbusClient.Connect();
            }
            foreach (FunctionProperties functionProperty in connectionProperties.FunctionPropertiesList)
                switch (functionProperty.FunctionCodeRead)
                {
                    case FunctionCodeRd.ReadCoils:
                        functionProperty.values = modbusClient.ReadCoils(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    case FunctionCodeRd.ReadDiscreteInputs:
                        functionProperty.values = modbusClient.ReadDiscreteInputs(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    case FunctionCodeRd.ReadHoldingRegisters:
                        functionProperty.values = modbusClient.ReadHoldingRegisters(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    case FunctionCodeRd.ReadInputRegisters:
                        functionProperty.values = modbusClient.ReadInputRegisters(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    default: break;
                }
            if (valuesChanged != null)
                valuesChanged(this);
        }

        public delegate void ConnectionPropertiesListChanged(object sender);
        public event ConnectionPropertiesListChanged connectionPropertiesListChanged;


        public static string getAddress(FunctionCodeRd functionCode, int startingAddress, int quantity, int elementCount)
        {
            string returnValue = null;
            if ((startingAddress + elementCount) <= (startingAddress + quantity))
                switch (functionCode)
                {
                    case FunctionCodeRd.ReadCoils:
                        returnValue = "0x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    case FunctionCodeRd.ReadDiscreteInputs:
                        returnValue = "1x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    case FunctionCodeRd.ReadHoldingRegisters:
                        returnValue = "4x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    case FunctionCodeRd.ReadInputRegisters:
                        returnValue = "3x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    default: break;
                }
            return returnValue;
        }

        public static int[] StrToValues(FunctionProperties functionProperties, string str)
        {
            int[] values = { };
            switch (functionProperties.FunctionCodeWrite)
            {
                case FunctionCodeWr.WriteHoldingRegisters:
                    int value = 0;
                    if (Int32.TryParse(str, out value))
                    {
                        // add 
                        int[] x = { value };
                        return x;

                    }

                    break;
            }
            return values;
        }

        public FunctionProperties FindPropertyFromGrid( int gridRow)
        {
            foreach (ConnectionProperties connection in connectionPropertiesList)
            {
                foreach (FunctionProperties functionProperty in connection.FunctionPropertiesList)
                {
                    if (functionProperty.DataGridRow == gridRow)
                    {
                        return functionProperty;
                    }
                }
            }

            return null;
        }

        public void WriteToServer(FunctionProperties prop, int[] values)
        {
            /*
            string text = "";
            text += "property " + prop.StartingAdress + "\n" + "type " + prop.FunctionCodeWrite.ToString() + "\n" + "new value: " + values.ToString() + "\n";
            text += "connection " + prop.Connection.ConnectionName;
            MessageBox.Show(text, "updating register");
            */

            int startingAddress = prop.StartingAdress;
            switch(prop.FunctionCodeWrite)
            {
                case FunctionCodeWr.WriteHoldingRegisters:
                                            prop.Connection.modbusClient.WriteMultipleRegisters(startingAddress, values);
                                            break;

            }
            

        }

        public void WriteXML(DataGridView dataGridView)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlNode xmlRoot;
            XmlNode xmlNodeConnection, xmlNodeConnectionProp;
            XmlNode xmlNodeFunctionCodes, xmlNodeFunctionCodesProp;
            XmlNode xmlNodeDataGrid, xmlNodeDataGridLines, xmlNodeDataGridLinesProp;
            xmlRoot = xmlDocument.CreateElement("ModbusConfiguration");
            for (int i = 0; i < this.connectionPropertiesList.Count; i++)
            {
                xmlNodeConnection = xmlDocument.CreateElement("connection");
                xmlNodeConnectionProp = xmlDocument.CreateElement("connectionName");
                xmlNodeConnectionProp.InnerText = this.connectionPropertiesList[i].ConnectionName;
                xmlNodeConnection.AppendChild(xmlNodeConnectionProp);
                xmlNodeConnectionProp = xmlDocument.CreateElement("ipAddress");
                xmlNodeConnectionProp.InnerText = this.connectionPropertiesList[i].ModbusTCPAddress;
                xmlNodeConnection.AppendChild(xmlNodeConnectionProp);
                xmlNodeConnectionProp = xmlDocument.CreateElement("port");
                xmlNodeConnectionProp.InnerText = this.connectionPropertiesList[i].Port.ToString();
                xmlNodeConnection.AppendChild(xmlNodeConnectionProp);
                xmlNodeConnectionProp = xmlDocument.CreateElement("cyclicFlag");
                xmlNodeConnectionProp.InnerText = this.connectionPropertiesList[i].CyclicFlag.ToString();
                xmlNodeConnection.AppendChild(xmlNodeConnectionProp);
                xmlNodeConnectionProp = xmlDocument.CreateElement("cycleTime");
                xmlNodeConnectionProp.InnerText = this.connectionPropertiesList[i].CycleTime.ToString();
                xmlNodeConnection.AppendChild(xmlNodeConnectionProp);
                for (int j = 0; j < this.connectionPropertiesList[i].FunctionPropertiesList.Count; j++)
                {
                    xmlNodeFunctionCodes = xmlDocument.CreateElement("functionCodes");
                    xmlNodeFunctionCodesProp = xmlDocument.CreateElement("functionCodeRead");
                    xmlNodeFunctionCodesProp.InnerText = this.connectionPropertiesList[i].FunctionPropertiesList[j].FunctionCodeRead.ToString();
                    xmlNodeFunctionCodes.AppendChild(xmlNodeFunctionCodesProp);
                    xmlNodeFunctionCodesProp = xmlDocument.CreateElement("functionCodeWrite");
                    xmlNodeFunctionCodesProp.InnerText = this.connectionPropertiesList[i].FunctionPropertiesList[j].FunctionCodeWrite.ToString();
                    xmlNodeFunctionCodes.AppendChild(xmlNodeFunctionCodesProp);
                    xmlNodeFunctionCodesProp = xmlDocument.CreateElement("quantity");
                    xmlNodeFunctionCodesProp.InnerText = this.connectionPropertiesList[i].FunctionPropertiesList[j].Quantity.ToString();
                    xmlNodeFunctionCodes.AppendChild(xmlNodeFunctionCodesProp);
                    xmlNodeFunctionCodesProp = xmlDocument.CreateElement("startingAddress");
                    xmlNodeFunctionCodesProp.InnerText = this.connectionPropertiesList[i].FunctionPropertiesList[j].StartingAdress.ToString();
                    xmlNodeFunctionCodes.AppendChild(xmlNodeFunctionCodesProp);
                    xmlNodeConnection.AppendChild(xmlNodeFunctionCodes);
                }
                xmlRoot.AppendChild(xmlNodeConnection);
                xmlNodeDataGrid = xmlDocument.CreateElement("dataGridView");
                for (int j = 0; j < dataGridView.Rows.Count; j++)
                {
                    if (dataGridView[0, j].Value != null & dataGridView[1, j].Value != null & dataGridView[3, j].Value != null)
                    {
                        xmlNodeDataGridLines = xmlDocument.CreateElement("dataGridViewLines");

                        xmlNodeDataGridLinesProp = xmlDocument.CreateElement("columnConnection");
                        xmlNodeDataGridLinesProp.InnerText = dataGridView[0, j].Value.ToString();
                        xmlNodeDataGridLines.AppendChild(xmlNodeDataGridLinesProp);

                        xmlNodeDataGridLinesProp = xmlDocument.CreateElement("columnAddress");
                        xmlNodeDataGridLinesProp.InnerText = dataGridView[1, j].Value.ToString();
                        xmlNodeDataGridLines.AppendChild(xmlNodeDataGridLinesProp);

                        xmlNodeDataGridLinesProp = xmlDocument.CreateElement("columnTag");
                        if (dataGridView[2, j].Value != null)
                            xmlNodeDataGridLinesProp.InnerText = dataGridView[2, j].Value.ToString();
                        else
                            xmlNodeDataGridLinesProp.InnerText = "n.a.";
                        xmlNodeDataGridLines.AppendChild(xmlNodeDataGridLinesProp);

                        xmlNodeDataGridLinesProp = xmlDocument.CreateElement("columnDataType");
                        xmlNodeDataGridLinesProp.InnerText = dataGridView[3, j].Value.ToString();
                        xmlNodeDataGridLines.AppendChild(xmlNodeDataGridLinesProp);

                        xmlNodeDataGrid.AppendChild(xmlNodeDataGridLines);
                    }
                }
                xmlRoot.AppendChild(xmlNodeDataGrid);

                xmlDocument.AppendChild(xmlRoot);
                xmlDocument.Save("textWriter.xml");
            }



        }

        public delegate void DataGridViewChanged(object sender);
        public event DataGridViewChanged dataGridViewChanged;
        public void ReadXML(DataGridView dataGridView)
        {
            XmlNodeList xmlNodeList, xmlNodeList2;
            XmlNode xmlNode3;
            System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.Load("textWriter.xml");
            xmlNodeList = xmlDocument.GetElementsByTagName("connection");
            //connectionPropertiesList = new List<ConnectionProperties>();
            this.connectionPropertiesList.Clear();
            int slotId = 0;

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                ConnectionProperties connectionProperty = new ConnectionProperties();
                AddConnection(connectionProperty);

                connectionProperty.ConnectionName = (xmlNode["connectionName"].InnerText);
                connectionProperty.ModbusTCPAddress = (xmlNode["ipAddress"].InnerText);
                connectionProperty.Port = Int32.Parse(xmlNode["port"].InnerText);
                connectionProperty.CyclicFlag = bool.Parse(xmlNode["cyclicFlag"].InnerText);
                connectionProperty.CycleTime = Int32.Parse(xmlNode["cycleTime"].InnerText);
                xmlNode3 = xmlNode["functionCodes"];
                while (xmlNode3 != null)
                {
                    xmlNodeList2 = xmlNode3.ChildNodes;
                    FunctionProperties functionProperty = new FunctionProperties();

                    foreach (XmlNode xmlNode2 in xmlNodeList2)
                    {
                        if (xmlNode2.Name == "functionCodeRead")
                            switch (xmlNode2.InnerText)
                            {
                                case "ReadCoils":
                                    functionProperty.FunctionCodeRead = FunctionCodeRd.ReadCoils;
                                    break;
                                case "ReadDiscreteInputs":
                                    functionProperty.FunctionCodeRead = FunctionCodeRd.ReadDiscreteInputs;
                                    break;
                                case "ReadHoldingRegisters":
                                    functionProperty.FunctionCodeRead = FunctionCodeRd.ReadHoldingRegisters;
                                    break;
                                case "ReadInputRegisters":
                                    functionProperty.FunctionCodeRead = FunctionCodeRd.ReadInputRegisters;
                                    break;
                            }
                        if (xmlNode2.Name == "functionCodeWrite")
                        {
                            functionProperty.FunctionCodeWrite = FunctionCodeWr.WriteNone;
                            switch (xmlNode2.InnerText)
                            {
                                case "WriteCoils":
                                    functionProperty.FunctionCodeWrite = FunctionCodeWr.WriteNone;
                                    break;
                                case "WriteDiscreteInputs":
                                    functionProperty.FunctionCodeWrite = FunctionCodeWr.WriteNone;
                                    break;
                                case "WriteHoldingRegisters":
                                    functionProperty.FunctionCodeWrite = FunctionCodeWr.WriteHoldingRegisters;
                                    break;
                                case "WriteInputRegisters":
                                    functionProperty.FunctionCodeWrite = FunctionCodeWr.WriteNone;
                                    break;
                            }

                        }
                        if (xmlNode2.Name == "startingAddress")
                            functionProperty.StartingAdress = Int32.Parse(xmlNode2.InnerText);
                        if (xmlNode2.Name == "quantity")
                            functionProperty.Quantity = Int32.Parse(xmlNode2.InnerText);
                    }
                    //connectionProperty.FunctionPropertiesList.Add(functionProperty);
                    this.AddFunctionProperty(functionProperty, slotId);

                    xmlNode3 = xmlNode3.NextSibling;
                }
                slotId++;
                //this.connectionPropertiesList.Add(connectionProperty);
            }
            if (connectionPropertiesListChanged != null)
                connectionPropertiesListChanged(this);

            xmlNodeList = xmlDocument.GetElementsByTagName("dataGridViewLines");
            dataGridView.Rows.Clear();
            dataGridView.AllowUserToAddRows = false;
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                dataGridView.Rows.Add();
                if  (xmlNode["columnConnection"] != null)
                    dataGridView[0, dataGridView.Rows.Count - 1].Value = xmlNode["columnConnection"].InnerText;
                dataGridView.ClearSelection();
                dataGridView.CurrentCell = null;
                
                if (xmlNode["columnAddress"] != null)
                    dataGridView[1, dataGridView.Rows.Count - 1].Value = xmlNode["columnAddress"].InnerText;
                if (dataGridViewChanged != null)
                    dataGridViewChanged(this);
                if (xmlNode["columnTag"] != null)
                    dataGridView[2, dataGridView.Rows.Count - 1].Value = xmlNode["columnTag"].InnerText;
                if (xmlNode["columnDataType"] != null)
                    dataGridView[3, dataGridView.Rows.Count - 1].Value = xmlNode["columnDataType"].InnerText;
            }

            // trigger update of values manually
            //this.valuesChanged(this);

            dataGridView.AllowUserToAddRows = true;
        }
    }
	
	
	public enum FunctionCodeRd : int
	{
		ReadCoils = 1,
		ReadDiscreteInputs = 2,
		ReadHoldingRegisters = 3,
		ReadInputRegisters = 4,
	};

    public enum FunctionCodeWr : int
    {
        WriteNone = 0,
        WriteCoils = 1,
        WriteDiscreteInputs = 2,
        WriteHoldingRegisters = 3,
        WriteInputRegisters = 4,
    };


    public class FunctionProperties
	{
	
		FunctionCodeRd functionCodeRd = FunctionCodeRd.ReadCoils;
        [Browsable(true)]                       
   		[Category("Function code properties")] 
    	[Description("Function Code Read")]           
   		[DisplayName("Function Code Read")]     
		public FunctionCodeRd FunctionCodeRead
		{
			get {return functionCodeRd; }
			set { functionCodeRd = value;}
		}

        FunctionCodeWr functionCodeWr = FunctionCodeWr.WriteNone;
        [Browsable(true)]
        [Category("Function code properties")]
        [Description("Function Code Write")]
        [DisplayName("Function Code Write")]
        public FunctionCodeWr FunctionCodeWrite
        {
            get { return functionCodeWr; }
            set { functionCodeWr = value; }
        }

        int startingAdress = 0;
		[Browsable(true)]                       
   		[Category("Function code properties")] 
    	[Description("Starting Address")]           
   		[DisplayName("Starting Address")]  
		public int StartingAdress
		{
			get {return startingAdress;}
			set {startingAdress = value;}
		}
		

		int quantity = 1;
		[Browsable(true)]                       
   		[Category("Function code properties")] 
    	[Description("Quantity")]           
   		[DisplayName("Quantity")]    
		public int Quantity
		{
			get {return quantity;}
			set {quantity = value;}
		}

        int DataGridRowIdx = -1;
        [Browsable(false)]
        [Category("Function code properties")]
        [Description("Data Grid Row Idx")]
        [DisplayName("Data Grid Row Idx")]
        public int DataGridRow
        {
            get { return DataGridRowIdx; }
            set { DataGridRowIdx = value; }
        }

        ConnectionProperties connection= null;
        [Browsable(false)]
        [Category("Function code properties")]
        [Description("connection")]
        [DisplayName("connection")]
        public ConnectionProperties Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        public object values;
}
}
