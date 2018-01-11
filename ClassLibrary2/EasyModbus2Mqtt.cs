using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.IO.Ports;
using System.Diagnostics;

namespace EasyModbus
{

    public class EasyModbus2Mqtt
    {
        private ModbusClient modbusClient = new ModbusClient();
        List<ReadOrder> readOrders = new List<ReadOrder>();
        string mqttBrokerAddress = "www.mqtt-dashboard.com";
        int mqttBrokerPort = 1883;
        string mqttRootTopic = "easymodbusclient";
        uPLibrary.Networking.M2Mqtt.MqttClient mqttClient;
        public bool AutomaticReconnect { get; set; } = true;
        public string MqttUserName { get; set; }
        public string MqttPassword { get; set; }
        public bool RetainMessages { get; set; }


        public EasyModbus2Mqtt()
        {
            
        }

        public void AddReadOrder(ReadOrder readOrder)
        {
            if (readOrder.FunctionCode == 0)
                throw new ArgumentOutOfRangeException("FunctionCode must be initialized");
            if (readOrder.Quantity == 0)
                throw new ArgumentOutOfRangeException("Quantity cannot be 0");
            if (readOrder.Topic != null)
                if (readOrder.Topic.Length != readOrder.Quantity)
                    throw new ArgumentOutOfRangeException("Size of the Topic array must mach with quantity");
            if (readOrder.Retain != null)
                if (readOrder.Retain.Length != readOrder.Quantity)
                    throw new ArgumentOutOfRangeException("Size of the Retain array must mach with quantity");
            if (readOrder.Hysteresis != null)
                if (readOrder.Hysteresis.Length != readOrder.Quantity)
                    throw new ArgumentOutOfRangeException("Size of the Hysteresis array must mach with quantity");
            if (readOrder.Scale != null)
                if (readOrder.Scale.Length != readOrder.Quantity)
                    throw new ArgumentOutOfRangeException("Size of the Scale array must mach with quantity");
            if (readOrder.Retain != null)
                if (readOrder.Retain.Length != readOrder.Quantity)
                    throw new ArgumentOutOfRangeException("Size of the Retain array must mach with quantity");
            if (readOrder.CylceTime == 0)
                readOrder.CylceTime = 500;
            if (readOrder.Topic == null)
            {
                readOrder.Topic = new string[readOrder.Quantity];
                for (int i = 0; i < readOrder.Quantity; i++)
                {
                    if (readOrder.FunctionCode == FunctionCode.ReadCoils)
                    {
                        readOrder.Topic[i] = mqttRootTopic+"/coils/" + (i + readOrder.StartingAddress).ToString();
                        
                    }
                    if (readOrder.FunctionCode == FunctionCode.ReadDiscreteInputs)
                    {
                        readOrder.Topic[i] = mqttRootTopic+"/discreteinputs/" + (i + readOrder.StartingAddress).ToString();
                        
                    }
                    if (readOrder.FunctionCode == FunctionCode.ReadHoldingRegisters)
                    {
                        readOrder.Topic[i] = mqttRootTopic+"/holdingregisters/" + (i + readOrder.StartingAddress).ToString();
                        
                    }
                    if (readOrder.FunctionCode == FunctionCode.ReadInputRegisters)
                    {
                        readOrder.Topic[i] = mqttRootTopic+"/inputregisters/" + (i + readOrder.StartingAddress).ToString();
                        
                    }
                }
            }
            readOrder.oldvalue = new object[readOrder.Quantity];
            readOrders.Add(readOrder);
        }

        public void AddReadOrder(FunctionCode functionCode, int quantity, int startingAddress, int cycleTime)
        {
            ReadOrder readOrder = new ReadOrder();
            readOrder.CylceTime = cycleTime;
            readOrder.FunctionCode = functionCode;
            readOrder.Quantity = quantity;
            readOrder.StartingAddress = startingAddress;
            readOrder.Topic = new string[quantity];
            readOrder.Retain = new bool[quantity];
            readOrder.oldvalue = new object[quantity];
            for (int i = 0; i < quantity; i++)
            {
                if (functionCode == FunctionCode.ReadCoils)
                {
                    readOrder.Topic[i] = mqttRootTopic+ "/coils/" + (i + readOrder.StartingAddress).ToString();
                    
                }
                if (functionCode == FunctionCode.ReadDiscreteInputs)
                {
                    readOrder.Topic[i] = mqttRootTopic + "/discreteinputs/" + (i + readOrder.StartingAddress).ToString();
                    
                }
                if (functionCode == FunctionCode.ReadHoldingRegisters)
                {
                    readOrder.Topic[i] = mqttRootTopic + "/holdingregisters/" + (i + readOrder.StartingAddress).ToString();
                   
                }
                if (functionCode == FunctionCode.ReadInputRegisters)
                {
                    readOrder.Topic[i] = mqttRootTopic + "/inputregisters/" + (i + readOrder.StartingAddress).ToString();
                 
                }
            }
            readOrders.Add(readOrder);
        }

        public void AddReadOrder(FunctionCode functionCode, int quantity, int startingAddress, int cycleTime, string[] topic)
        {
            ReadOrder readOrder = new ReadOrder();
            readOrder.FunctionCode = functionCode;
            readOrder.Quantity = quantity;
            readOrder.StartingAddress = startingAddress;
            readOrder.CylceTime = cycleTime;
            readOrder.Topic = topic;
            readOrder.Retain = new bool[quantity];
            this.AddReadOrder(readOrder);
        }

        public void start()
        {

            this.shouldStop = false;
            if (mqttBrokerAddress == null)
                throw new ArgumentOutOfRangeException("Mqtt Broker Address not initialized");
            mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient(mqttBrokerAddress,mqttBrokerPort,false,null,null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);
     
            string clientID = new Guid().ToString();
            try
            {
                if (MqttUserName == null || MqttPassword == null)
                    mqttClient.Connect(clientID);
                else
                    mqttClient.Connect(clientID, MqttUserName, MqttPassword);
                if (!modbusClient.Connected)
                    modbusClient.Connect();
            }
            catch (Exception exc)
            {
                if (!this.AutomaticReconnect)
                    throw exc;
            }
            for (int i = 0; i < readOrders.Count; i++)
            {
                readOrders[i].thread = new System.Threading.Thread(new ParameterizedThreadStart(ProcessData));
                readOrders[i].thread.Start(readOrders[i]);
            }
        }

        string MqttBrokerAddressPublish = "";
        public void publish(string[] topic, string[] payload, string mqttBrokerAddress)
        {
            if (mqttClient != null)
                if (!mqttBrokerAddress.Equals(this.MqttBrokerAddressPublish) & mqttClient.IsConnected)
                    mqttClient.Disconnect();
            if (topic.Length != payload.Length)
                throw new ArgumentOutOfRangeException("Array topic and payload must be the same size");
            mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient(mqttBrokerAddress, mqttBrokerPort, false, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);
            string clientID = Guid.NewGuid().ToString();
            if (!mqttClient.IsConnected)
            {
                if (MqttUserName == null || MqttPassword == null)
                    mqttClient.Connect(clientID);
                else
                    mqttClient.Connect(clientID, MqttUserName, MqttPassword);
            }
                
            for (int i = 0; i < payload.Length; i++)
                mqttClient.Publish(topic[i], Encoding.UTF8.GetBytes(payload[i]), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, RetainMessages);
         
        }

        public void publish(string topic, string payload, string mqttBrokerAddress)
        {
            if (mqttClient != null)
                if (!mqttBrokerAddress.Equals(this.MqttBrokerAddressPublish) & mqttClient.IsConnected)
                    mqttClient.Disconnect();
            mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient(mqttBrokerAddress, mqttBrokerPort, false, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);
            string clientID = Guid.NewGuid().ToString();
            if (!mqttClient.IsConnected)
            {
                if (MqttUserName == null || MqttPassword == null)
                    mqttClient.Connect(clientID);
                else
                    mqttClient.Connect(clientID, MqttUserName, MqttPassword);
            }

                if (payload != null)
                    mqttClient.Publish(topic, Encoding.UTF8.GetBytes(payload), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, RetainMessages);
                else
                    mqttClient.Publish(topic, new byte[0], 0, RetainMessages);
        }

        public void Disconnect()
        {
            mqttClient.Disconnect();
        }


        public void stop()
        {
            modbusClient.Disconnect();
            mqttClient.Disconnect();
            this.shouldStop = true;
        }

        private volatile bool shouldStop;
        private object lockProcessData = new object();
        private void ProcessData(object param)
        {
            while (!shouldStop)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient(mqttBrokerAddress, mqttBrokerPort, false, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);
                        string clientID = Guid.NewGuid().ToString();
                        if (MqttUserName == null || MqttPassword == null)
                            mqttClient.Connect(clientID);
                        else
                            mqttClient.Connect(clientID, MqttUserName, MqttPassword);
                    }
                }
                catch (Exception exc)
                {
                    if (!this.AutomaticReconnect)
                        throw exc;
                }
                ReadOrder readOrder = (ReadOrder)param;
                lock (lockProcessData)
                {
                    try
                    {
                        if (readOrder.FunctionCode == FunctionCode.ReadCoils)
                        {
                            bool[] value = modbusClient.ReadCoils(readOrder.StartingAddress, readOrder.Quantity);
                            for (int i = 0; i < value.Length; i++)
                            {
                                if (readOrder.oldvalue[i] == null)
                                    mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                else if ((bool)readOrder.oldvalue[i] != value[i])
                                    mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                readOrder.oldvalue[i] = value[i];
                            }

                        }
                        if (readOrder.FunctionCode == FunctionCode.ReadDiscreteInputs)
                        {
                            bool[] value = modbusClient.ReadDiscreteInputs(readOrder.StartingAddress, readOrder.Quantity);
                            for (int i = 0; i < value.Length; i++)
                            {
                                if (readOrder.oldvalue[i] == null)
                                    mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                else if ((bool)readOrder.oldvalue[i] != value[i])
                                    mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                readOrder.oldvalue[i] = value[i];
                            }
                        }
                        if (readOrder.FunctionCode == FunctionCode.ReadHoldingRegisters)
                        {
                            int[] value = modbusClient.ReadHoldingRegisters(readOrder.StartingAddress, readOrder.Quantity);
                            for (int i = 0; i < value.Length; i++)
                            {
                                float scale = readOrder.Scale != null ? (readOrder.Scale[i] == 0) ? 1 : readOrder.Scale[i] : 1;
                                if (readOrder.oldvalue[i] == null)
                                {
                                    mqttClient.Publish(readOrder.Topic[i], (readOrder.Unit == null ? Encoding.UTF8.GetBytes(((float)value[i] * scale).ToString()) : Encoding.UTF8.GetBytes(((float)value[i] * scale) + " " + readOrder.Unit[i])), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                    readOrder.oldvalue[i] = value[i];
                                }
                                else if (((int)readOrder.oldvalue[i] != value[i]) && (readOrder.Hysteresis != null ? ((value[i] < (int)readOrder.oldvalue[i] - (int)readOrder.Hysteresis[i]) | (value[i] > (int)readOrder.oldvalue[i] + (int)readOrder.Hysteresis[i])) : true))
                                {
                                    mqttClient.Publish(readOrder.Topic[i], (readOrder.Unit == null ? Encoding.UTF8.GetBytes(((float)value[i] * scale).ToString()) : Encoding.UTF8.GetBytes(((float)value[i] * scale).ToString() + " " + readOrder.Unit[i])), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                    readOrder.oldvalue[i] = value[i];
                                }
                            }
                        }

                        if (readOrder.FunctionCode == FunctionCode.ReadInputRegisters)
                        {
                            int[] value = modbusClient.ReadInputRegisters(readOrder.StartingAddress, readOrder.Quantity);
                            for (int i = 0; i < value.Length; i++)
                            {
                                float scale = readOrder.Scale != null ? (readOrder.Scale[i] == 0) ? 1 : readOrder.Scale[i] : 1;
                                if (readOrder.oldvalue[i] == null)
                                {
                                    mqttClient.Publish(readOrder.Topic[i], (readOrder.Unit == null ? Encoding.UTF8.GetBytes(((float)value[i] * scale).ToString()) : Encoding.UTF8.GetBytes(((float)value[i] * scale).ToString() + " " + readOrder.Unit[i])), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                    readOrder.oldvalue[i] = value[i];
                                }
                                else if (((int)readOrder.oldvalue[i] != value[i]) && (readOrder.Hysteresis != null ? ((value[i] < (int)readOrder.oldvalue[i] - (int)readOrder.Hysteresis[i]) | (value[i] > (int)readOrder.oldvalue[i] + (int)readOrder.Hysteresis[i])) : true))
                                {
                                    mqttClient.Publish(readOrder.Topic[i], (readOrder.Unit == null ? Encoding.UTF8.GetBytes(((float)value[i] * scale).ToString()) : Encoding.UTF8.GetBytes(((float)value[i] * scale).ToString() + " " + readOrder.Unit[i])), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, (readOrder.Retain != null) ? readOrder.Retain[i] : false);
                                    readOrder.oldvalue[i] = value[i];
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        modbusClient.Disconnect();
                        Thread.Sleep(2000);
                        if (!AutomaticReconnect)
                            throw exc;
                        Debug.WriteLine(exc.StackTrace);

                        if (!modbusClient.Connected)
                        {
                            try
                            {

                                modbusClient.Connect();
                            }
                            catch (Exception) { }
                        }
                    }
                 }
                System.Threading.Thread.Sleep(readOrder.CylceTime);
            }
        }


        public string MqttBrokerAddress
        {
            get
            {
                return this.mqttBrokerAddress;
            }
            set
            {
                this.mqttBrokerAddress = value;
            }
        }

        public int MqttBrokerPort
        {
            get
            {
                return this.mqttBrokerPort;
            }
            set
            {
                this.mqttBrokerPort = value;
            }
        }

        public string MqttRootTopic
        {
            get
            {
                return this.mqttRootTopic;
            }
            set
            {
                this.mqttRootTopic = value;
            }
        }

        public string IPAddress
        {
            get
            {
                return modbusClient.IPAddress;
            }
            set
            {
                modbusClient.IPAddress = value;
            }
        }

        public string ModbusIPAddress
        {
            get
            {
                return modbusClient.IPAddress;
            }
            set
            {
                modbusClient.IPAddress = value;
            }
        }

        public int Port
        {
            get
            {
                return modbusClient.Port;
            }
            set
            {
                modbusClient.Port = value;
            }
        }

        public int ModbusPort
        {
            get
            {
                return modbusClient.Port;
            }
            set
            {
                modbusClient.Port = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Unit identifier in case of serial connection (Default = 0)
        /// </summary>
        public byte UnitIdentifier
        {
            get
            {
                return modbusClient.UnitIdentifier;
            }
            set
            {
                modbusClient.UnitIdentifier = value;
            }
        }


        /// <summary>
        /// Gets or Sets the Baudrate for serial connection (Default = 9600)
        /// </summary>
        public int Baudrate
        {
            get
            {
                return modbusClient.Baudrate;
            }
            set
            {
                modbusClient.Baudrate = value;
            }
        }

        /// <summary>
        /// Gets or Sets the of Parity in case of serial connection
        /// </summary>
        public Parity Parity
        {
            get
            {
                if (modbusClient.SerialPort != null)
                    return modbusClient.Parity;
                else
                    return Parity.Even;
            }
            set
            {
                if (modbusClient.SerialPort != null)
                    modbusClient.Parity = value;
            }
        }


        /// <summary>
        /// Gets or Sets the number of stopbits in case of serial connection
        /// </summary>
        public StopBits StopBits
        {
            get
            {
                if (modbusClient.SerialPort != null)
                    return modbusClient.StopBits;
                else
                    return StopBits.One;
            }
            set
            {
                if (modbusClient.SerialPort != null)
                    modbusClient.StopBits = value;
            }
        }

        /// <summary>
        /// Gets or Sets the connection Timeout in case of ModbusTCP connection
        /// </summary>
        public int ConnectionTimeout
        {
            get
            {
                return modbusClient.ConnectionTimeout;
            }
            set
            {
                modbusClient.ConnectionTimeout = value;
            }
        }

        /// <summary>
        /// Gets or Sets the serial Port
        /// </summary>
        public string SerialPort
        {
            get
            {

                return modbusClient.SerialPort;
            }
            set
            {
                this.SerialPort = value;
            }
        }

    }

    public class ReadOrder
    {
        public FunctionCode FunctionCode;       //Function Code to execute
        public int CylceTime = 500;             //Polling intervall in ms
        public int StartingAddress;             //First Modbus Register to Read (0-based)
        public int Quantity;
        public string[] Topic;                  //Symbolnames can by replaced, by default we Push to the topic e.g. for Coils: /modbusclient/coils/1
        public int[] Hysteresis;                //Values for 16-Bit Registers will be published of the dieffreence is greater than Hysteresis
        public string[] Unit;                   //Unit for Analog Values (Holding Registers and Input Registers)
        public float[] Scale;                    //Scale for Analog Values (Holding Registers and Input Registers)
        public bool[] Retain;                   //Retain last Value in Broker
        internal System.Threading.Thread thread;
        internal object[] oldvalue;             
    }

    public enum FunctionCode
    {
        ReadDiscreteInputs = 2,
        ReadCoils = 1,
        ReadHoldingRegisters = 3,
        ReadInputRegisters = 4
    }

}
