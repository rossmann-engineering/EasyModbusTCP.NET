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

            switch (connectionProperties.FunctionPropertiesList[functionPropertyID].FunctionCode)
            {
                case FunctionCode.ReadCoils:
                    connectionProperties.FunctionPropertiesList[functionPropertyID].values = modbusClient.ReadCoils(connectionProperties.FunctionPropertiesList[functionPropertyID].StartingAdress, connectionProperties.FunctionPropertiesList[functionPropertyID].Quantity);
                    break;
                case FunctionCode.ReadDiscreteInputs:
                    connectionProperties.FunctionPropertiesList[functionPropertyID].values = modbusClient.ReadDiscreteInputs(connectionProperties.FunctionPropertiesList[functionPropertyID].StartingAdress, connectionProperties.FunctionPropertiesList[functionPropertyID].Quantity);
                    break;
                case FunctionCode.ReadHoldingRegisters:
                    connectionProperties.FunctionPropertiesList[functionPropertyID].values = modbusClient.ReadHoldingRegisters(connectionProperties.FunctionPropertiesList[functionPropertyID].StartingAdress, connectionProperties.FunctionPropertiesList[functionPropertyID].Quantity);
                    break;
                case FunctionCode.ReadInputRegisters:
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
                switch (functionProperty.FunctionCode)
                {
                    case FunctionCode.ReadCoils:
                        functionProperty.values = modbusClient.ReadCoils(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    case FunctionCode.ReadDiscreteInputs:
                        functionProperty.values = modbusClient.ReadDiscreteInputs(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    case FunctionCode.ReadHoldingRegisters:
                        functionProperty.values = modbusClient.ReadHoldingRegisters(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    case FunctionCode.ReadInputRegisters:
                        functionProperty.values = modbusClient.ReadInputRegisters(functionProperty.StartingAdress, functionProperty.Quantity);
                        break;
                    default: break;
                }
            if (valuesChanged != null)
                valuesChanged(this);
        }

        public delegate void ConnectionPropertiesListChanged(object sender);
        public event ConnectionPropertiesListChanged connectionPropertiesListChanged;


        public static string getAddress(FunctionCode functionCode, int startingAddress, int quantity, int elementCount)
        {
            string returnValue = null;
            if ((startingAddress + elementCount) <= (startingAddress + quantity))
                switch (functionCode)
                {
                    case FunctionCode.ReadCoils:
                        returnValue = "0x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    case FunctionCode.ReadDiscreteInputs:
                        returnValue = "1x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    case FunctionCode.ReadHoldingRegisters:
                        returnValue = "4x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    case FunctionCode.ReadInputRegisters:
                        returnValue = "3x" + (startingAddress + elementCount + 1).ToString();
                        break;
                    default: break;
                }
            return returnValue;
        }

        public void WriteXML(DataGridView dataGridView)
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlNode xmlRoot;
            XmlNode xmlChild1;
            XmlNode xmlChild2, xmlChild3;
            xmlRoot = xmlDocument.CreateElement("ModbusConfiguration");
            for (int i = 0; i < this.connectionPropertiesList.Count; i++)
            {
                xmlChild1 = xmlDocument.CreateElement("connection");
                xmlChild2 = xmlDocument.CreateElement("connectionName");
                xmlChild2.InnerText = this.connectionPropertiesList[i].ConnectionName;
                xmlChild1.AppendChild(xmlChild2);
                xmlChild2 = xmlDocument.CreateElement("ipAddress");
                xmlChild2.InnerText = this.connectionPropertiesList[i].ModbusTCPAddress;
                xmlChild1.AppendChild(xmlChild2);
                xmlChild2 = xmlDocument.CreateElement("port");
                xmlChild2.InnerText = this.connectionPropertiesList[i].Port.ToString();
                xmlChild1.AppendChild(xmlChild2);
                xmlChild2 = xmlDocument.CreateElement("cyclicFlag");
                xmlChild2.InnerText = this.connectionPropertiesList[i].CyclicFlag.ToString();
                xmlChild1.AppendChild(xmlChild2);
                xmlChild2 = xmlDocument.CreateElement("cycleTime");
                xmlChild2.InnerText = this.connectionPropertiesList[i].CycleTime.ToString();
                xmlChild1.AppendChild(xmlChild2);
                for (int j = 0; j < this.connectionPropertiesList[i].FunctionPropertiesList.Count; j++)
                {
                    xmlChild2 = xmlDocument.CreateElement("functionCodes");
                    xmlChild3 = xmlDocument.CreateElement("functionCode");
                    xmlChild3.InnerText = this.connectionPropertiesList[i].FunctionPropertiesList[j].FunctionCode.ToString();
                    xmlChild2.AppendChild(xmlChild3);
                    xmlChild3 = xmlDocument.CreateElement("quantity");
                    xmlChild3.InnerText = this.connectionPropertiesList[i].FunctionPropertiesList[j].Quantity.ToString();
                    xmlChild2.AppendChild(xmlChild3);
                    xmlChild3 = xmlDocument.CreateElement("startingAddress");
                    xmlChild3.InnerText = this.connectionPropertiesList[i].FunctionPropertiesList[j].StartingAdress.ToString();
                    xmlChild2.AppendChild(xmlChild3);
                    xmlChild1.AppendChild(xmlChild2);
                }
                xmlRoot.AppendChild(xmlChild1);
                xmlChild1 = xmlDocument.CreateElement("dataGridView");
                for (int j = 0; j < dataGridView.Rows.Count; j++)
                {
                    if (dataGridView[0, j].Value != null & dataGridView[1, j].Value!= null & dataGridView[2, j].Value != null & dataGridView[3, j].Value != null)
                    xmlChild2 = xmlDocument.CreateElement("dataGridViewLines");
                    xmlChild3 = xmlDocument.CreateElement("columnConnection");
                    if (dataGridView[0, j].Value != null)
                        xmlChild3.InnerText = dataGridView[0, j].Value.ToString();
                    xmlChild2.AppendChild(xmlChild3);
                    xmlChild3 = xmlDocument.CreateElement("columnAddress");
                    if (dataGridView[1, j].Value != null)
                        xmlChild3.InnerText = dataGridView[1, j].Value.ToString();
                    xmlChild2.AppendChild(xmlChild3);
                    xmlChild3 = xmlDocument.CreateElement("columnTag");
                    if (dataGridView[2, j].Value != null)
                        xmlChild3.InnerText = dataGridView[2, j].Value.ToString();
                    xmlChild2.AppendChild(xmlChild3);
                    xmlChild3 = xmlDocument.CreateElement("columnDataType");
                    if (dataGridView[3, j].Value != null)
                        xmlChild3.InnerText = dataGridView[3, j].Value.ToString();
                    xmlChild2.AppendChild(xmlChild3);
                    xmlChild1.AppendChild(xmlChild2);

                }
                xmlRoot.AppendChild(xmlChild1);
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
            connectionPropertiesList = new List<ConnectionProperties>();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                ConnectionProperties connectionProperty = new ConnectionProperties();
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
                        if (xmlNode2.Name == "functionCode")
                            switch (xmlNode2.InnerText)
                            {
                                case "ReadCoils":
                                    functionProperty.FunctionCode = FunctionCode.ReadCoils;
                                    break;
                                case "ReadDiscreteInputs":
                                    functionProperty.FunctionCode = FunctionCode.ReadDiscreteInputs;
                                    break;
                                case "ReadHoldingRegisters":
                                    functionProperty.FunctionCode = FunctionCode.ReadHoldingRegisters;
                                    break;
                                case "ReadInputRegisters":
                                    functionProperty.FunctionCode = FunctionCode.ReadInputRegisters;
                                    break;
                            }
                        if (xmlNode2.Name == "startingAddress")
                            functionProperty.StartingAdress = Int32.Parse(xmlNode2.InnerText);
                        if (xmlNode2.Name == "quantity")
                            functionProperty.Quantity = Int32.Parse(xmlNode2.InnerText);
                    }
                    connectionProperty.FunctionPropertiesList.Add(functionProperty);
                    xmlNode3 = xmlNode3.NextSibling;
                }
                connectionPropertiesList.Add(connectionProperty);
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
            dataGridView.AllowUserToAddRows = true;
        }
    }
	
	
	public enum FunctionCode : int
	{
		ReadCoils = 1,
		ReadDiscreteInputs = 2,
		ReadHoldingRegisters = 3,
		ReadInputRegisters = 4,
	};
	
	
	public class FunctionProperties
	{
	
		FunctionCode funtionCode = FunctionCode.ReadCoils;
		
		[Browsable(true)]                       
   		[Category("Function code properties")] 
    	[Description("Function Code")]           
   		[DisplayName("Function Code")]     
		public FunctionCode FunctionCode
		{
			get {return funtionCode;}
			set {funtionCode = value;}
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
		
		public object values;
}
}
