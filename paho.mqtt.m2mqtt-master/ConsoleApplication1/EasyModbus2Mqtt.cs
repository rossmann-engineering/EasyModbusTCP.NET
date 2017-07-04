using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Mqtt
{

    class EasyModbus2Mqtt : EasyModbus.ModbusClient
    {
        List<ReadOrder> readOrders = new List<ReadOrder>();
        string mqttBrokerAddress;
        string mqttRootTopic = "easymodbusclient";
        uPLibrary.Networking.M2Mqtt.MqttClient mqttClient;

        public EasyModbus2Mqtt()
        {
        }

        public void AddReadOrder(ReadOrder readOrder)
        {
            if (readOrder.FunctionCode == 0)
                throw new ArgumentOutOfRangeException("FunctionCode must be initialized");
            if (readOrder.Quantity == 0)
                throw new ArgumentOutOfRangeException("Quantity cannot be 0");
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

        public void start()
        {
            this.shouldStop = false;
            if (mqttBrokerAddress == null)
                throw new ArgumentOutOfRangeException("Mqtt Broker Address not initialized");
            if (!this.Connected)
                this.Connect();
            mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient(mqttBrokerAddress);
            string clientID = new Guid().ToString();
            mqttClient.Connect(clientID);
            for (int i = 0; i < readOrders.Count; i++)
            {
                readOrders[i].thread = new System.Threading.Thread(new ParameterizedThreadStart(ProcessData));
                readOrders[i].thread.Start(readOrders[i]);
            }
        }

        public void stop()
        {
            this.Disconnect();
            mqttClient.Disconnect();
            this.shouldStop = true;
        }

        private volatile bool shouldStop;
        private void ProcessData(object param)
        {
            while (!shouldStop)
            {
                ReadOrder readOrder = (ReadOrder)param;
                if (readOrder.FunctionCode == FunctionCode.ReadCoils)
                {
                    bool[] value = this.ReadCoils(readOrder.StartingAddress, readOrder.Quantity);
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (readOrder.oldvalue[i] == null)
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()) , MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        else if ((bool)readOrder.oldvalue[i] != value[i])
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        readOrder.oldvalue[i] = value[i];
                    }

                }
                if (readOrder.FunctionCode == FunctionCode.ReadDiscreteInputs)
                {
                    bool[] value = this.ReadDiscreteInputs(readOrder.StartingAddress, readOrder.Quantity);
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (readOrder.oldvalue[i] == null)
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        else if ((bool)readOrder.oldvalue[i] != value[i])
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        readOrder.oldvalue[i] = value[i];
                    }
                }
                if (readOrder.FunctionCode == FunctionCode.ReadHoldingRegisters)
                {
                    int[] value = this.ReadHoldingRegisters(readOrder.StartingAddress, readOrder.Quantity);
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (readOrder.oldvalue[i] == null)
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        else if ((int)readOrder.oldvalue[i] != value[i])
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        readOrder.oldvalue[i] = value[i];
                    }
                }

                if (readOrder.FunctionCode == FunctionCode.ReadInputRegisters)
                {
                    int[] value = this.ReadInputRegisters(readOrder.StartingAddress, readOrder.Quantity);
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (readOrder.oldvalue[i] == null)
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        else if ((int)readOrder.oldvalue[i] != value[i])
                            mqttClient.Publish(readOrder.Topic[i], Encoding.UTF8.GetBytes(value[i].ToString()), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                        readOrder.oldvalue[i] = value[i];
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

    }

    public class ReadOrder
    {
        public FunctionCode FunctionCode;   //Function Code to execute
        public int CylceTime = 500;               //Polling intervall in ms
        public int StartingAddress;         //First Modbus Register to Read (0-based)
        public int Quantity;
        public string[] Topic;        //Symbolnames can by replaced, by default we Push to the topic e.g. for Coils: /modbusclient/coils/1
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
