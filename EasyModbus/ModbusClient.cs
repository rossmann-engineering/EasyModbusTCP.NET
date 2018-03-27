/*
 * Copyright (c) 2017 Stefan Roßmann.
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 * http://www.rossmann-engineering.de
 * Datum: 16.06.2011
 * Zeit: 21:35
 */
using System;
using System.Net.Sockets;
using System.Net;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace EasyModbus
{
	/// <summary>
	/// Implements a ModbusClient.
	/// </summary>
	public partial class ModbusClient
	{
        public enum RegisterOrder { LowHigh = 0, HighLow = 1 };
        private bool debug=false;
		private TcpClient tcpClient;
		private string ipAddress = "127.0.0.1";
		private int port = 502;
        private uint transactionIdentifierInternal = 0;
		private byte [] transactionIdentifier = new byte[2];
		private byte [] protocolIdentifier = new byte[2];
        private byte[] crc = new byte[2];
		private byte [] length = new byte[2];
		private byte unitIdentifier = 0x01;
		private byte functionCode;
		private byte [] startingAddress = new byte[2];
		private byte [] quantity = new byte[2];
        private bool udpFlag = false;
        private int portOut;
        private int baudRate = 9600;
        private int connectTimeout = 1000;
        public byte[] receiveData;
        public byte[] sendData; 
        private SerialPort serialport;
        private Parity parity = Parity.Even;
        private StopBits stopBits = StopBits.One;
        private bool connected = false;
        private bool[] mqttCoilsOldValues;
        private bool[] mqttDiscreteInputsOldValues;
        private int[] mqttInputRegistersOldValues;
        private int[] mqttHoldingRegistersOldValues;
        private EasyModbus2Mqtt easyModbus2Mqtt;
        private bool mqttRetainMessages;
        public int NumberOfRetries { get; set; } = 3;
        private int countRetries = 0;
        string mqttRootTopic = "easymodbusclient";
        public string MqttUserName { get; set; }
        public string MqttPassword { get; set; }
        public bool MqttPushOnChange { get; set; } = true;

        public int MqttBrokerPort { get; set; } = 1883;

        public delegate void ReceiveDataChangedHandler(object sender);
        public event ReceiveDataChangedHandler ReceiveDataChanged;
        
        public delegate void SendDataChangedHandler(object sender);
        public event SendDataChangedHandler SendDataChanged;

        public delegate void ConnectedChangedHandler(object sender);
        public event ConnectedChangedHandler ConnectedChanged;

        NetworkStream stream;
		
		/// <summary>
		/// Constructor which determines the Master ip-address and the Master Port.
		/// </summary>
		/// <param name="ipAddress">IP-Address of the Master device</param>
		/// <param name="port">Listening port of the Master device (should be 502)</param>
		public ModbusClient(string ipAddress, int port)
		{
			if (debug) StoreLogData.Instance.Store("EasyModbus library initialized for Modbus-TCP, IPAddress: " + ipAddress + ", Port: "+port ,System.DateTime.Now);
#if (!COMMERCIAL)
            Console.WriteLine("EasyModbus Client Library Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("Copyright (c) Stefan Rossmann Engineering Solutions");
            Console.WriteLine();
#endif
            this.ipAddress = ipAddress;
			this.port = port;
		}

        /// <summary>
        /// Constructor which determines the Serial-Port
        /// </summary>
        /// <param name="serialPort">Serial-Port Name e.G. "COM1"</param>
        public ModbusClient(string serialPort)
        {
        	if (debug) StoreLogData.Instance.Store("EasyModbus library initialized for Modbus-RTU, COM-Port: " + serialPort ,System.DateTime.Now);
#if (!COMMERCIAL)
            Console.WriteLine("EasyModbus Client Library Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("Copyright (c) Stefan Rossmann Engineering Solutions");
            Console.WriteLine();
#endif
            this.serialport = new SerialPort();
            serialport.PortName = serialPort;
            serialport.BaudRate = baudRate;
            serialport.Parity = parity;
            serialport.StopBits = stopBits;
            serialport.WriteTimeout = 10000;
            serialport.ReadTimeout = connectTimeout;
           
            serialport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);      
        }

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public ModbusClient()
        {
        	if (debug) StoreLogData.Instance.Store("EasyModbus library initialized for Modbus-TCP" ,System.DateTime.Now);
#if (!COMMERCIAL)
            Console.WriteLine("EasyModbus Client Library Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("Copyright (c) Stefan Rossmann Engineering Solutions");
            Console.WriteLine();
#endif
        }
		
		/// <summary>
		/// Establish connection to Master device in case of Modbus TCP. Opens COM-Port in case of Modbus RTU
		/// </summary>
		public void Connect()
		{
            if (serialport != null)
            {
                if (!serialport.IsOpen)
                {
                	if (debug) StoreLogData.Instance.Store("Open Serial port " + serialport.PortName,System.DateTime.Now);
                    serialport.BaudRate = baudRate;
                    serialport.Parity = parity;
                    serialport.StopBits = stopBits;
                    serialport.WriteTimeout = 10000;
                    serialport.ReadTimeout = connectTimeout;
                    serialport.Open();
                    connected = true;
                    
                   
                }
                if (ConnectedChanged != null)
                    try
                    {
                        ConnectedChanged(this);
                    }
                    catch
                    {

                    }
                return;
            }
            if (!udpFlag)
            {
                if (debug) StoreLogData.Instance.Store("Open TCP-Socket, IP-Address: " + ipAddress + ", Port: " + port, System.DateTime.Now);
                tcpClient = new TcpClient();
                var result = tcpClient.BeginConnect(ipAddress, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(connectTimeout);
                if (!success)
                {
                    throw new EasyModbus.Exceptions.ConnectionException("connection timed out");
                }
                tcpClient.EndConnect(result);

                //tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();
                stream.ReadTimeout = connectTimeout;
                connected = true;
            }
            else
            {
                tcpClient = new TcpClient();
                connected = true;
            }
            if (ConnectedChanged != null)
                try
                {
                    ConnectedChanged(this);
                }
                catch
                {

                }
        }
		
		/// <summary>
		/// Establish connection to Master device in case of Modbus TCP.
		/// </summary>
		public void Connect(string ipAddress, int port)
		{
            if (!udpFlag)
            {
                if (debug) StoreLogData.Instance.Store("Open TCP-Socket, IP-Address: " + ipAddress + ", Port: " + port, System.DateTime.Now);
                tcpClient = new TcpClient();
                var result = tcpClient.BeginConnect(ipAddress, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(connectTimeout);
                if (!success)
                {
                    throw new EasyModbus.Exceptions.ConnectionException("connection timed out");
                }
                tcpClient.EndConnect(result);

                //tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();
                stream.ReadTimeout = connectTimeout;
                connected = true;
            }
            else
            {
                tcpClient = new TcpClient();
                connected = true;
            }

            if (ConnectedChanged != null)
                ConnectedChanged(this);
        }

        /// <summary>
        /// Converts two ModbusRegisters to Float - Example: EasyModbus.ModbusClient.ConvertRegistersToFloat(modbusClient.ReadHoldingRegisters(19,2))
        /// </summary>
        /// <param name="registers">Two Register values received from Modbus</param>
        /// <returns>Connected float value</returns>
        public static float ConvertRegistersToFloat(int[] registers)
        {
            if (registers.Length != 2)
                throw new ArgumentException("Input Array length invalid - Array langth must be '2'");
            int highRegister = registers[1];
            int lowRegister = registers[0];
            byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
            byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
            byte[] floatBytes = {
                                    lowRegisterBytes[0],
                                    lowRegisterBytes[1],
                                    highRegisterBytes[0],
                                    highRegisterBytes[1]
                                };
            return BitConverter.ToSingle(floatBytes, 0);
        }

        /// <summary>
        /// Converts two ModbusRegisters to Float, Registers can by swapped
        /// </summary>
        /// <param name="registers">Two Register values received from Modbus</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Connected float value</returns>
        public static float ConvertRegistersToFloat(int[] registers, RegisterOrder registerOrder)
        {
            int [] swappedRegisters = {registers[0],registers[1]};
            if (registerOrder == RegisterOrder.HighLow) 
                swappedRegisters = new int[] {registers[1],registers[0]};
            return ConvertRegistersToFloat(swappedRegisters);
        }

        /// <summary>
        /// Converts two ModbusRegisters to 32 Bit Integer value
        /// </summary>
        /// <param name="registers">Two Register values received from Modbus</param>
        /// <returns>Connected 32 Bit Integer value</returns>
        public static Int32 ConvertRegistersToInt(int[] registers)
        {
            if (registers.Length != 2)
                throw new ArgumentException("Input Array length invalid - Array langth must be '2'");
            int highRegister = registers[1];
            int lowRegister = registers[0];
            byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
            byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
            byte[] doubleBytes = {
                                    lowRegisterBytes[0],
                                    lowRegisterBytes[1],
                                    highRegisterBytes[0],
                                    highRegisterBytes[1]
                                };
            return BitConverter.ToInt32(doubleBytes, 0);
        }

        /// <summary>
        /// Converts two ModbusRegisters to 32 Bit Integer Value - Registers can be swapped
        /// </summary>
        /// <param name="registers">Two Register values received from Modbus</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Connecteds 32 Bit Integer value</returns>
        public static Int32 ConvertRegistersToInt(int[] registers, RegisterOrder registerOrder)
        {
            int[] swappedRegisters = { registers[0], registers[1] };
            if (registerOrder == RegisterOrder.HighLow)
                swappedRegisters = new int[] { registers[1], registers[0] };
            return ConvertRegistersToInt(swappedRegisters);
        }

        /// <summary>
        /// Convert four 16 Bit Registers to 64 Bit Integer value Register Order "LowHigh": Reg0: Low Word.....Reg3: High Word, "HighLow": Reg0: High Word.....Reg3: Low Word
        /// </summary>
        /// <param name="registers">four Register values received from Modbus</param>
        /// <returns>64 bit value</returns>
        public static Int64 ConvertRegistersToLong(int[] registers)
        {
            if (registers.Length != 4)
                throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
            int highRegister = registers[3];
            int highLowRegister = registers[2];
            int lowHighRegister = registers[1];
            int lowRegister = registers[0];
            byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
            byte[] highLowRegisterBytes = BitConverter.GetBytes(highLowRegister);
            byte[] lowHighRegisterBytes = BitConverter.GetBytes(lowHighRegister);
            byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
            byte[] longBytes = {
                                    lowRegisterBytes[0],
                                    lowRegisterBytes[1],
                                    lowHighRegisterBytes[0],
                                    lowHighRegisterBytes[1],
                                    highLowRegisterBytes[0],
                                    highLowRegisterBytes[1],
                                    highRegisterBytes[0],
                                    highRegisterBytes[1]
                                };
            return BitConverter.ToInt64(longBytes, 0);
        }

        /// <summary>
        /// Convert four 16 Bit Registers to 64 Bit Integer value - Registers can be swapped
        /// </summary>
        /// <param name="registers">four Register values received from Modbus</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Connected 64 Bit Integer value</returns>
        public static Int64 ConvertRegistersToLong(int[] registers, RegisterOrder registerOrder)
        {
            if (registers.Length != 4)
                throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
            int[] swappedRegisters = { registers[0], registers[1], registers[2], registers[3] };
            if (registerOrder == RegisterOrder.HighLow)
                swappedRegisters = new int[] { registers[3], registers[2], registers[1], registers[0] };
            return ConvertRegistersToLong(swappedRegisters);
        }

        /// <summary>
        /// Convert four 16 Bit Registers to 64 Bit double prec. value Register Order "LowHigh": Reg0: Low Word.....Reg3: High Word, "HighLow": Reg0: High Word.....Reg3: Low Word
        /// </summary>
        /// <param name="registers">four Register values received from Modbus</param>
        /// <returns>64 bit value</returns>
        public static double ConvertRegistersToDouble(int[] registers)
        {
            if (registers.Length != 4)
                throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
            int highRegister = registers[3];
            int highLowRegister = registers[2];
            int lowHighRegister = registers[1];
            int lowRegister = registers[0];
            byte[] highRegisterBytes = BitConverter.GetBytes(highRegister);
            byte[] highLowRegisterBytes = BitConverter.GetBytes(highLowRegister);
            byte[] lowHighRegisterBytes = BitConverter.GetBytes(lowHighRegister);
            byte[] lowRegisterBytes = BitConverter.GetBytes(lowRegister);
            byte[] longBytes = {
                                    lowRegisterBytes[0],
                                    lowRegisterBytes[1],
                                    lowHighRegisterBytes[0],
                                    lowHighRegisterBytes[1],
                                    highLowRegisterBytes[0],
                                    highLowRegisterBytes[1],
                                    highRegisterBytes[0],
                                    highRegisterBytes[1]
                                };
            return BitConverter.ToDouble(longBytes, 0);
        }

        /// <summary>
        /// Convert four 16 Bit Registers to 64 Bit double prec. value - Registers can be swapped
        /// </summary>
        /// <param name="registers">four Register values received from Modbus</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Connected double prec. float value</returns>
        public static double ConvertRegistersToDouble(int[] registers, RegisterOrder registerOrder)
        {
            if (registers.Length != 4)
                throw new ArgumentException("Input Array length invalid - Array langth must be '4'");
            int[] swappedRegisters = { registers[0], registers[1], registers[2], registers[3] };
            if (registerOrder == RegisterOrder.HighLow)
                swappedRegisters = new int[] { registers[3], registers[2], registers[1], registers[0] };
            return ConvertRegistersToDouble(swappedRegisters);
        }

        /// <summary>
        /// Converts float to two ModbusRegisters - Example:  modbusClient.WriteMultipleRegisters(24, EasyModbus.ModbusClient.ConvertFloatToTwoRegisters((float)1.22));
        /// </summary>
        /// <param name="floatValue">Float value which has to be converted into two registers</param>
        /// <returns>Register values</returns>
        public static int[] ConvertFloatToRegisters(float floatValue)
        {
            byte[] floatBytes = BitConverter.GetBytes(floatValue);
            byte[] highRegisterBytes = 
            {
                floatBytes[2],
                floatBytes[3],
                0,
                0
            };
            byte[] lowRegisterBytes = 
            {
                
                floatBytes[0],
                floatBytes[1],
                0,
                0
            };
            int[] returnValue =
            {
                BitConverter.ToInt32(lowRegisterBytes,0),
                BitConverter.ToInt32(highRegisterBytes,0)
            };
            return returnValue;
        }

        /// <summary>
        /// Converts float to two ModbusRegisters Registers - Registers can be swapped
        /// </summary>
        /// <param name="floatValue">Float value which has to be converted into two registers</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Register values</returns>
        public static int[] ConvertFloatToRegisters(float floatValue, RegisterOrder registerOrder)
        {
            int[] registerValues = ConvertFloatToRegisters(floatValue);
            int[] returnValue = registerValues;
            if (registerOrder == RegisterOrder.HighLow)
                returnValue = new Int32[] { registerValues[1], registerValues[0] };
            return returnValue;
        }

        /// <summary>
        /// Converts 32 Bit Value to two ModbusRegisters
        /// </summary>
        /// <param name="intValue">Int value which has to be converted into two registers</param>
        /// <returns>Register values</returns>
        public static int[] ConvertIntToRegisters(Int32 intValue)
        {
            byte[] doubleBytes = BitConverter.GetBytes(intValue);
            byte[] highRegisterBytes = 
            {
                doubleBytes[2],
                doubleBytes[3],
                0,
                0
            };
            byte[] lowRegisterBytes = 
            {
                
                doubleBytes[0],
                doubleBytes[1],
                0,
                0
            };
            int[] returnValue =
            {
                BitConverter.ToInt32(lowRegisterBytes,0),
                BitConverter.ToInt32(highRegisterBytes,0)
            };
            return returnValue;
        }

        /// <summary>
        /// Converts 32 Bit Value to two ModbusRegisters Registers - Registers can be swapped
        /// </summary>
        /// <param name="intValue">Double value which has to be converted into two registers</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Register values</returns>
        public static int[] ConvertIntToRegisters(Int32 intValue, RegisterOrder registerOrder)
        {
            int[] registerValues = ConvertIntToRegisters(intValue);
            int[] returnValue = registerValues;
            if (registerOrder == RegisterOrder.HighLow)
                returnValue = new Int32[] { registerValues[1], registerValues[0] };
            return returnValue;
        }

        /// <summary>
        /// Converts 64 Bit Value to four ModbusRegisters
        /// </summary>
        /// <param name="longValue">long value which has to be converted into four registers</param>
        /// <returns>Register values</returns>
        public static int[] ConvertLongToRegisters(Int64 longValue)
        {
            byte[] longBytes = BitConverter.GetBytes(longValue);
            byte[] highRegisterBytes =
            {
                longBytes[6],
                longBytes[7],
                0,
                0
            };
            byte[] highLowRegisterBytes =
            {
                longBytes[4],
                longBytes[5],
                0,
                0
            };
            byte[] lowHighRegisterBytes =
            {
                longBytes[2],
                longBytes[3],
                0,
                0
            };
            byte[] lowRegisterBytes =
            {

                longBytes[0],
                longBytes[1],
                0,
                0
            };
            int[] returnValue =
            {
                BitConverter.ToInt32(lowRegisterBytes,0),
                BitConverter.ToInt32(lowHighRegisterBytes,0),
                BitConverter.ToInt32(highLowRegisterBytes,0),
                BitConverter.ToInt32(highRegisterBytes,0)
            };
            return returnValue;
        }

        /// <summary>
        /// Converts 64 Bit Value to four ModbusRegisters - Registers can be swapped
        /// </summary>
        /// <param name="longValue">long value which has to be converted into four registers</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Register values</returns>
        public static int[] ConvertLongToRegisters(Int64 longValue, RegisterOrder registerOrder)
        {
            int[] registerValues = ConvertLongToRegisters(longValue);
            int[] returnValue = registerValues;
            if (registerOrder == RegisterOrder.HighLow)
                returnValue = new int[] { registerValues[3], registerValues[2], registerValues[1], registerValues[0] };
            return returnValue;
        }

        /// <summary>
        /// Converts 64 Bit double prec Value to four ModbusRegisters
        /// </summary>
        /// <param name="doubleValue">double value which has to be converted into four registers</param>
        /// <returns>Register values</returns>
        public static int[] ConvertDoubleToRegisters(double doubleValue)
        {
            byte[] doubleBytes = BitConverter.GetBytes(doubleValue);
            byte[] highRegisterBytes =
            {
                doubleBytes[6],
                doubleBytes[7],
                0,
                0
            };
            byte[] highLowRegisterBytes =
            {
                doubleBytes[4],
                doubleBytes[5],
                0,
                0
            };
            byte[] lowHighRegisterBytes =
            {
                doubleBytes[2],
                doubleBytes[3],
                0,
                0
            };
            byte[] lowRegisterBytes =
            {

                doubleBytes[0],
                doubleBytes[1],
                0,
                0
            };
            int[] returnValue =
            {
                BitConverter.ToInt32(lowRegisterBytes,0),
                BitConverter.ToInt32(lowHighRegisterBytes,0),
                BitConverter.ToInt32(highLowRegisterBytes,0),
                BitConverter.ToInt32(highRegisterBytes,0)
            };
            return returnValue;
        }

        /// <summary>
        /// Converts 64 Bit double prec. Value to four ModbusRegisters - Registers can be swapped
        /// </summary>
        /// <param name="doubleValue">double value which has to be converted into four registers</param>
        /// <param name="registerOrder">Desired Word Order (Low Register first or High Register first</param>
        /// <returns>Register values</returns>
        public static int[] ConvertDoubleToRegisters(double doubleValue, RegisterOrder registerOrder)
        {
            int[] registerValues = ConvertDoubleToRegisters(doubleValue);
            int[] returnValue = registerValues;
            if (registerOrder == RegisterOrder.HighLow)
                returnValue = new int[] { registerValues[3], registerValues[2], registerValues[1], registerValues[0] };
            return returnValue;
        }

        /// <summary>
        /// Converts 16 - Bit Register values to String
        /// </summary>
        /// <param name="registers">Register array received via Modbus</param>
        /// <param name="offset">First Register containing the String to convert</param>
        /// <param name="stringLength">number of characters in String (must be even)</param>
        /// <returns>Converted String</returns>
        public static string ConvertRegistersToString(int[] registers, int offset, int stringLength)
        { 
        byte[] result = new byte[stringLength];
        byte[] registerResult = new byte[2];
        
            for (int i = 0; i < stringLength/2; i++)
            {
                registerResult = BitConverter.GetBytes(registers[offset + i]);
                result[i * 2] = registerResult[0];
                result[i * 2 + 1] = registerResult[1];
            }
            return System.Text.Encoding.Default.GetString(result);
        }

        /// <summary>
        /// Converts a String to 16 - Bit Registers
        /// </summary>
        /// <param name="registers">Register array received via Modbus</param>
        /// <returns>Converted String</returns>
        public static int[] ConvertStringToRegisters(string stringToConvert)
        {
            byte[] array = System.Text.Encoding.ASCII.GetBytes(stringToConvert);
            int[] returnarray = new int[stringToConvert.Length / 2 + stringToConvert.Length % 2];
            for (int i = 0; i < returnarray.Length; i++)
            {
                returnarray[i] = array[i * 2];
                if (i*2 +1< array.Length)
                {
                    returnarray[i] = returnarray[i] | ((int)array[i * 2 + 1] << 8);
                }
            }
            return returnarray;
        }


        /// <summary>
        /// Calculates the CRC16 for Modbus-RTU
        /// </summary>
        /// <param name="data">Byte buffer to send</param>
        /// <param name="numberOfBytes">Number of bytes to calculate CRC</param>
        /// <param name="startByte">First byte in buffer to start calculating CRC</param>
        public static UInt16 calculateCRC(byte[] data, UInt16 numberOfBytes, int startByte)
        { 
            byte[] auchCRCHi = {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40
            };
		
            byte[] auchCRCLo = {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4,
            0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
            0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD,
            0x1D, 0x1C, 0xDC, 0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7,
            0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
            0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE,
            0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2,
            0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
            0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB,
            0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0, 0x50, 0x90, 0x91,
            0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
            0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88,
            0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83, 0x41, 0x81, 0x80,
            0x40
            };
            UInt16 usDataLen = numberOfBytes;
            byte  uchCRCHi = 0xFF ; 
            byte uchCRCLo = 0xFF ; 
            int i = 0;
            int uIndex ;
            while (usDataLen>0) 
            {
                usDataLen--;
                if ((i + startByte) < data.Length)
                {
                    uIndex = uchCRCLo ^ data[i + startByte];
                    uchCRCLo = (byte)(uchCRCHi ^ auchCRCHi[uIndex]);
                    uchCRCHi = auchCRCLo[uIndex];
                }
                i++;
            }
            return (UInt16)((UInt16)uchCRCHi << 8 | uchCRCLo);           
        }

        private bool dataReceived = false;
        private bool receiveActive = false;
        private byte[] readBuffer = new byte[256];
        private int bytesToRead = 0;
        private int akjjjctualPositionToRead = 0;
        DateTime dateTimeLastRead;
/*
        private void DataReceivedHandler(object sender,
                        SerialDataReceivedEventArgs e)
        {
            long ticksWait = TimeSpan.TicksPerMillisecond * 2000;
            SerialPort sp = (SerialPort)sender;
            
            if (bytesToRead == 0 || sp.BytesToRead == 0)
            {
                actualPositionToRead = 0;
                sp.DiscardInBuffer();
                dataReceived = false;
                receiveActive = false;
                return;
            }

            if (actualPositionToRead == 0 && !dataReceived)
                readBuffer = new byte[256];

            //if ((DateTime.Now.Ticks - dateTimeLastRead.Ticks) > ticksWait)
            //{
            //    readBuffer = new byte[256];
            //    actualPositionToRead = 0;
            //}
            int numberOfBytesInBuffer = sp.BytesToRead;
            sp.Read(readBuffer, actualPositionToRead, ((numberOfBytesInBuffer + actualPositionToRead) > readBuffer.Length) ? 0 : numberOfBytesInBuffer);
            actualPositionToRead = actualPositionToRead + numberOfBytesInBuffer;
            //sp.DiscardInBuffer();
            //if (DetectValidModbusFrame(readBuffer, (actualPositionToRead < readBuffer.Length) ? actualPositionToRead : readBuffer.Length) | bytesToRead <= actualPositionToRead)
            if (actualPositionToRead >= bytesToRead)
            {

                    dataReceived = true;
                    bytesToRead = 0;
                    actualPositionToRead = 0;
                    if (debug) StoreLogData.Instance.Store("Received Serial-Data: " + BitConverter.ToString(readBuffer), System.DateTime.Now);

            }


            //dateTimeLastRead = DateTime.Now;
        }
 */       

        
        private void DataReceivedHandler(object sender,
                        SerialDataReceivedEventArgs e)
        {
            serialport.DataReceived -= DataReceivedHandler;

            //while (receiveActive | dataReceived)
        	//	System.Threading.Thread.Sleep(10);
        	receiveActive = true;
        	
        	const long ticksWait = TimeSpan.TicksPerMillisecond * 2000;//((40*10000000) / this.baudRate);
        	
        	
        	SerialPort sp = (SerialPort)sender;
            if (bytesToRead == 0)
            {
                sp.DiscardInBuffer();
                receiveActive = false;
                serialport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                return;
            }
            readBuffer = new byte[256];
        	int numbytes=0;
            int actualPositionToRead = 0;
            DateTime dateTimeLastRead = DateTime.Now;
            do{
            	try {
            		dateTimeLastRead = DateTime.Now;  
            		while ((sp.BytesToRead) == 0) 
            		{
            			System.Threading.Thread.Sleep(10);
            			if  ((DateTime.Now.Ticks - dateTimeLastRead.Ticks) > ticksWait) 
            				break;
            		}
            		numbytes=sp.BytesToRead;
            		
            	
            	byte[] rxbytearray = new byte[numbytes];
            	sp.Read(rxbytearray, 0, numbytes);
                Array.Copy(rxbytearray,0, readBuffer,actualPositionToRead, (actualPositionToRead + rxbytearray.Length) <= bytesToRead ? rxbytearray.Length : bytesToRead - actualPositionToRead); 
            	
            	actualPositionToRead = actualPositionToRead + rxbytearray.Length;
            	
            	}
            	catch (Exception){
            	
            	}

                if (bytesToRead <= actualPositionToRead)
                    break;

            	if (DetectValidModbusFrame(readBuffer, (actualPositionToRead < readBuffer.Length) ? actualPositionToRead : readBuffer.Length) | bytesToRead <= actualPositionToRead)
                    break;
            }
            while ((DateTime.Now.Ticks - dateTimeLastRead.Ticks) < ticksWait) ;
            
            //10.000 Ticks in 1 ms

            receiveData = new byte[actualPositionToRead];
            Array.Copy(readBuffer, 0, receiveData, 0, (actualPositionToRead < readBuffer.Length) ? actualPositionToRead: readBuffer.Length);
            if (debug) StoreLogData.Instance.Store("Received Serial-Data: "+BitConverter.ToString(readBuffer) ,System.DateTime.Now);
            bytesToRead = 0;


         
            
            dataReceived = true;
            receiveActive = false;
            serialport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            if (ReceiveDataChanged != null)
            {

                ReceiveDataChanged(this);

            }
            
            //sp.DiscardInBuffer();
        }

        public static bool DetectValidModbusFrame(byte[] readBuffer, int length)
        {
        	// minimum length 6 bytes
        	if (length < 6)
        		return false;
        	//SlaveID correct
        	if ((readBuffer[0] < 1) | (readBuffer[0] > 247))
        		return false;
            //CRC correct?
            byte[] crc = new byte[2];
            crc = BitConverter.GetBytes(calculateCRC(readBuffer, (ushort)(length-2), 0));
                if (crc[0] != readBuffer[length-2] | crc[1] != readBuffer[length-1])
                	return false;
            return true;
        }

        /// <summary>
        /// Read Discrete Inputs from Server device (FC2) and publishes the values to a MQTT-Broker.
        /// The Topic will be easymodbusclient/discreteinputs/'address' e.g. easymodbusclient/discreteinputs/0 for address "0".
        /// Note that the Address that will be publishes is "0"-Based. The Root topic can be changed using the Parameter
        /// By default we are using the Standard-Port 1883. This Port can be changed using the Property "MqttBrokerPort"
        /// A Username and Passowrd can be provided using the Properties "MqttUserName" and "MqttPassword"
        /// 'MqttRootTopic' Default is 'easymodbusclient'
        /// </summary>
        /// <param name="startingAddress">First discrete input to read</param>
        /// <param name="quantity">Number of discrete Inputs to read</param>
        /// <param name="mqttBrokerAddress">Broker address the values will be published to</param>
        /// <returns>Boolean Array which contains the discrete Inputs</returns>
        public bool[] ReadDiscreteInputs(int startingAddress, int quantity, string mqttBrokerAddress)
        {
            bool[] returnValue = this.ReadDiscreteInputs(startingAddress, quantity);
            List<String> topic = new List<String>();
            List<String> payload = new List<String>();
            if (MqttPushOnChange && mqttDiscreteInputsOldValues == null)
                mqttDiscreteInputsOldValues = new bool[65535];
            for (int i = 0; i < returnValue.Length; i++)
            {
                if (mqttDiscreteInputsOldValues == null ? true : (mqttDiscreteInputsOldValues[i] != returnValue[i]))
                {
                    topic.Add(mqttRootTopic + "/discreteinputs/" + (i + startingAddress).ToString());
                    payload.Add(returnValue[i].ToString());
                    mqttDiscreteInputsOldValues[i] = returnValue[i];
                }

            }
            if (easyModbus2Mqtt == null)
                easyModbus2Mqtt = new EasyModbus2Mqtt();
            easyModbus2Mqtt.MqttBrokerPort = this.MqttBrokerPort;
            easyModbus2Mqtt.MqttUserName = this.MqttUserName;
            easyModbus2Mqtt.MqttPassword = this.MqttPassword;
            easyModbus2Mqtt.RetainMessages = this.mqttRetainMessages;
            easyModbus2Mqtt.publish(topic.ToArray(), payload.ToArray(), mqttBrokerAddress);
    
            return returnValue;
        }


        /// <summary>
        /// Read Discrete Inputs from Server device (FC2).
        /// </summary>
        /// <param name="startingAddress">First discrete input to read</param>
        /// <param name="quantity">Number of discrete Inputs to read</param>
        /// <returns>Boolean Array which contains the discrete Inputs</returns>
        public bool[] ReadDiscreteInputs(int startingAddress, int quantity)
		{
            if (debug) StoreLogData.Instance.Store("FC2 (Read Discrete Inputs from Master device), StartingAddress: "+ startingAddress+", Quantity: " +quantity, System.DateTime.Now);
            transactionIdentifierInternal ++;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
			if (tcpClient == null & !udpFlag & serialport==null)
			{
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
			if (startingAddress > 65535 | quantity >2000)
			{
				if (debug) StoreLogData.Instance.Store("ArgumentException Throwed", System.DateTime.Now);
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
			}
			bool[] response;
			this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.functionCode = 0x02;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
            Byte[] data = new byte[]
                            {	
                            this.transactionIdentifier[1],
							this.transactionIdentifier[0],
							this.protocolIdentifier[1],
							this.protocolIdentifier[0],
							this.length[1],
							this.length[0],
							this.unitIdentifier,
							this.functionCode,
							this.startingAddress[1],
							this.startingAddress[0],
							this.quantity[1],
							this.quantity[0],
                            this.crc[0],
                            this.crc[1]
                            };
                crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
                data[12] = crc[0];
                data[13] = crc[1];

            if (serialport != null)
            {
                dataReceived = false;
                if (quantity % 8 == 0)
                    bytesToRead = 5 + quantity / 8;
                else
                    bytesToRead = 6 + quantity / 8;
 //               serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, 8);
                if (debug)
                {
                	byte [] debugData = new byte[8];
            		Array.Copy(data, 6, debugData, 0, 8);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                if (SendDataChanged != null)
            	{
            		sendData = new byte[8];
            		Array.Copy(data, 6, sendData, 0, 8);
            		SendDataChanged(this);
                    
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;


                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);       
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                }
                if (receivedUnitIdentifier != this.unitIdentifier)
                	data = new byte[2100];
                else
                    countRetries = 0;
            }
            else if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length-2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length-2);
                if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                    if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
            		}
                    data = new Byte[2100];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
            }
            if (data[7] == 0x82 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x82 & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x82 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x82 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
            crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data[8]+3), 6));
                if ((crc[0] != data[data[8] + 9] | crc[1] != data[data[8] + 10]) & dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        return ReadDiscreteInputs(startingAddress, quantity);
                    }
                }
                else if (!dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");
                    }
                    else
                    {
                        countRetries++;
                        return ReadDiscreteInputs(startingAddress, quantity);
                    }
                }
            }
			response = new bool[quantity];
			for (int i = 0; i < quantity; i++)
			{
				int intData = data[9+i/8];
				int mask = Convert.ToInt32(Math.Pow(2, (i%8)));
				response[i] = Convert.ToBoolean((intData & mask)/mask);
			}    		
    		return (response);
		}

        /// <summary>
        /// Read coils from Server device (FC1) and publishes the values to a MQTT-Broker.
        /// The Topic will be easymodbusclient/coils/'address' e.g. easymodbusclient/coils/0 for address "0".
        /// Note that the Address that will be publishes is "0"-Based. The Root topic can be changed using the Parameter
        /// 'MqttRootTopic' Default is 'easymodbusclient'
        /// By default we are using the Standard-Port 1883. This Port can be changed using the Property "MqttBrokerPort"
        /// A Username and Passowrd can be provided using the Properties "MqttUserName" and "MqttPassword"
        /// </summary>
        /// <param name="startingAddress">First coil to read</param>
        /// <param name="quantity">Number of coils to read</param>
        /// <param name="mqttBrokerAddress">Broker address 8the values will be published to</param>
        /// <returns>Boolean Array which contains the coild</returns>
        public bool[] ReadCoils(int startingAddress, int quantity, string mqttBrokerAddress)
        {
            
            bool[] returnValue = this.ReadCoils(startingAddress, quantity);
            List<String> topic = new List<String>();
            List<String> payload = new List<String>();
            if (MqttPushOnChange && mqttCoilsOldValues == null)
                mqttCoilsOldValues = new bool[65535];
            for (int i = 0; i < returnValue.Length; i++)
            {
                if (mqttCoilsOldValues == null ? true : (mqttCoilsOldValues[i] != returnValue[i]))
                {
                    topic.Add(mqttRootTopic + "/coils/" + (i + startingAddress).ToString());
                    payload.Add(returnValue[i].ToString());
                    mqttCoilsOldValues[i] = returnValue[i];
                }
            }
            if (easyModbus2Mqtt == null)
                easyModbus2Mqtt = new EasyModbus2Mqtt();
            easyModbus2Mqtt.MqttBrokerPort = this.MqttBrokerPort;
            easyModbus2Mqtt.MqttUserName = this.MqttUserName;
            easyModbus2Mqtt.MqttPassword = this.MqttPassword;
            easyModbus2Mqtt.RetainMessages = this.mqttRetainMessages;
            easyModbus2Mqtt.publish(topic.ToArray(), payload.ToArray(), mqttBrokerAddress);


            return returnValue;
        }

        /// <summary>
        /// Read Coils from Server device (FC1).
        /// </summary>
        /// <param name="startingAddress">First coil to read</param>
        /// <param name="quantity">Numer of coils to read</param>
        /// <returns>Boolean Array which contains the coils</returns>
        public bool[] ReadCoils(int startingAddress, int quantity)
		{
            if (debug) StoreLogData.Instance.Store("FC1 (Read Coils from Master device), StartingAddress: "+ startingAddress+", Quantity: " +quantity, System.DateTime.Now);
            transactionIdentifierInternal++;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
			if (startingAddress > 65535 | quantity >2000)
			{
				if (debug) StoreLogData.Instance.Store("ArgumentException Throwed", System.DateTime.Now);
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
			}
			bool[] response;
			this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.functionCode = 0x01;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
			Byte[] data = new byte[]{	
                            this.transactionIdentifier[1],
							this.transactionIdentifier[0],
							this.protocolIdentifier[1],
							this.protocolIdentifier[0],
							this.length[1],
							this.length[0],
							this.unitIdentifier,
							this.functionCode,
							this.startingAddress[1],
							this.startingAddress[0],
							this.quantity[1],
							this.quantity[0],
                            this.crc[0],
                            this.crc[1]
            };

            crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
           data[12] = crc[0];
                data[13] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                if (quantity % 8 == 0)
                    bytesToRead = 5 + quantity/8;
                else
                    bytesToRead = 6 + quantity/8;
 //               serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, 8);
                if (debug)
                {
                	byte [] debugData = new byte[8];
            		Array.Copy(data, 6, debugData, 0, 8);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[8];
            		Array.Copy(data, 6, sendData, 0, 8);
            		SendDataChanged(this);
                    
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
          
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                }
                if (receivedUnitIdentifier != this.unitIdentifier)
                	data = new byte[2100];
                else
                    countRetries = 0;
            }
			else if (tcpClient.Client.Connected | udpFlag)
			{
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length-2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length-2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send MocbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                    if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
                        
                    }
                    data = new Byte[2100];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
			}
            if (data[7] == 0x81 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x81 & data[8] == 0x02)
           {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x81 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x81 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
            crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data[8]+3), 6));
                if ((crc[0] != data[data[8]+9] | crc[1] != data[data[8]+10]) & dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        return ReadCoils(startingAddress, quantity);
                    }
                }
                else if (!dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");
                    }
                    else
                    {
                        countRetries++;
                        return ReadCoils(startingAddress, quantity);
                    }
                }
            }
			response = new bool[quantity];
			for (int i = 0; i < quantity; i++)
			{
				int intData = data[9+i/8];
				int mask = Convert.ToInt32(Math.Pow(2, (i%8)));
				response[i] = Convert.ToBoolean((intData & mask)/mask);
			}   		
    		return (response);
		}

        /// <summary>
        /// Read Holding Registers from Server device (FC3) and publishes the values to a MQTT-Broker.
        /// The Topic will be easymodbusclient/holdingregisters/'address' e.g. easymodbusclient/holdingregisters/0 for address "0".
        /// Note that the Address that will be publishes is "0"-Based. The Root topic can be changed using the Parameter
        /// 'MqttRootTopic' Default is 'easymodbusclient'
        /// By default we are using the Standard-Port 1883. This Port can be changed using the Property "MqttBrokerPort"
        /// A Username and Passowrd can be provided using the Properties "MqttUserName" and "MqttPassword"
        /// </summary>
        /// <param name="startingAddress">First Holding Register to read</param>
        /// <param name="quantity">Number of Holding Registers to read</param>
        /// <param name="mqttBrokerAddress">Broker address the values will be published to</param>
        /// <returns>Boolean Array which contains the Holding Registers</returns>
        public int[] ReadHoldingRegisters(int startingAddress, int quantity, string mqttBrokerAddress)
        {
            int[] returnValue = this.ReadHoldingRegisters(startingAddress, quantity);
            List<String> topic = new List<String>();
            List<String> payload = new List<String>();
            if (MqttPushOnChange && mqttHoldingRegistersOldValues == null)
                mqttHoldingRegistersOldValues = new int[65535];
            for (int i = 0; i < returnValue.Length; i++)
            {
                if (mqttHoldingRegistersOldValues == null ? true : (mqttHoldingRegistersOldValues[i] != returnValue[i]))
                {
                    topic.Add(mqttRootTopic + "/holdingregisters/" + (i + startingAddress).ToString());
                    payload.Add(returnValue[i].ToString());
                    mqttHoldingRegistersOldValues[i] = returnValue[i];
                }
            }
            if (easyModbus2Mqtt == null)
                easyModbus2Mqtt = new EasyModbus2Mqtt();
            easyModbus2Mqtt.MqttBrokerPort = this.MqttBrokerPort;
            easyModbus2Mqtt.MqttUserName = this.MqttUserName;
            easyModbus2Mqtt.MqttPassword = this.MqttPassword;
            easyModbus2Mqtt.RetainMessages = this.mqttRetainMessages;
            easyModbus2Mqtt.publish(topic.ToArray(), payload.ToArray(), mqttBrokerAddress);
            return returnValue;
        }

        /// <summary>
        /// Read Holding Registers from Master device (FC3).
        /// </summary>
        /// <param name="startingAddress">First holding register to be read</param>
        /// <param name="quantity">Number of holding registers to be read</param>
        /// <returns>Int Array which contains the holding registers</returns>
        public int[] ReadHoldingRegisters(int startingAddress, int quantity)
		{
            if (debug) StoreLogData.Instance.Store("FC3 (Read Holding Registers from Master device), StartingAddress: "+ startingAddress+", Quantity: " +quantity, System.DateTime.Now);
            transactionIdentifierInternal++;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
			if (startingAddress > 65535 | quantity >125)
			{
				if (debug) StoreLogData.Instance.Store("ArgumentException Throwed", System.DateTime.Now);
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
			}
			int[] response;
			this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.functionCode = 0x03;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
			Byte[] data = new byte[]{	this.transactionIdentifier[1],
							this.transactionIdentifier[0],
							this.protocolIdentifier[1],
							this.protocolIdentifier[0],
							this.length[1],
							this.length[0],
							this.unitIdentifier,
							this.functionCode,
							this.startingAddress[1],
							this.startingAddress[0],
							this.quantity[1],
							this.quantity[0],
                            this.crc[0],
                            this.crc[1]
            };
            crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = crc[0];
            data[13] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                bytesToRead = 5 + 2 * quantity;
//                serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, 8);
                if (debug)
                {
                	byte [] debugData = new byte[8];
            		Array.Copy(data, 6, debugData, 0, 8);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[8];
            		Array.Copy(data, 6, sendData, 0, 8);
            		SendDataChanged(this);
                    
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	
                	receivedUnitIdentifier = data[6];
                }
                if (receivedUnitIdentifier != this.unitIdentifier)
                	data = new byte[2100];
                else
                    countRetries = 0;
            }
			else if (tcpClient.Client.Connected | udpFlag)
			{
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length-2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length-2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                    if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
                       
                    }
                    data = new Byte[256];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
			}
            if (data[7] == 0x83 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x83 & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x83 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x83 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
            crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data[8]+3), 6));
                if ((crc[0] != data[data[8]+9] | crc[1] != data[data[8]+10])& dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        return ReadHoldingRegisters(startingAddress, quantity);
                    }
                }
                else if (!dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");
                    }
                    else
                    {
                        countRetries++;
                        return ReadHoldingRegisters(startingAddress, quantity);
                    }
                    

                }
            }
			response = new int[quantity];
			for (int i = 0; i < quantity; i++)
			{
				byte lowByte;
				byte highByte;
				highByte = data[9+i*2];
				lowByte = data[9+i*2+1];
				
				data[9+i*2] = lowByte;
				data[9+i*2+1] = highByte;
				
				response[i] = BitConverter.ToInt16(data,(9+i*2));
			}			
    		return (response);			
		}

        /// <summary>
        /// Read Input Registers from Server device (FC4) and publishes the values to a MQTT-Broker.
        /// The Topic will be easymodbusclient/inputregisters/'address' e.g. easymodbusclient/inputregisters/0 for address "0".
        /// Note that the Address that will be publishes is "0"-Based. The Root topic can be changed using the Parameter
        /// 'MqttRootTopic' Default is 'easymodbusclient'
        /// By default we are using the Standard-Port 1883. This Port can be changed using the Property "MqttBrokerPort"
        /// A Username and Passowrd can be provided using the Properties "MqttUserName" and "MqttPassword"
        /// </summary>
        /// <param name="startingAddress">First Input Register to read</param>
        /// <param name="quantity">Number of Input Registers to read</param>
        /// <param name="mqttBrokerAddress">Broker address 8the values will be published to</param>
        /// <returns>Boolean Array which contains the Input Registers</returns>
        public int[] ReadInputRegisters(int startingAddress, int quantity, string mqttBrokerAddress)
        {
            int[] returnValue = this.ReadInputRegisters(startingAddress, quantity);
            List<String> topic = new List<String>();
            List<String> payload = new List<String>();
            if (MqttPushOnChange && mqttInputRegistersOldValues == null)
                mqttInputRegistersOldValues = new int[65535];
            for (int i = 0; i < returnValue.Length; i++)
            {
                if (mqttInputRegistersOldValues == null ? true : (mqttInputRegistersOldValues[i] != returnValue[i]))
                {
                    topic.Add(mqttRootTopic + "/inputregisters/" + (i + startingAddress).ToString());
                    payload.Add(returnValue[i].ToString());
                    mqttInputRegistersOldValues[i] = returnValue[i];
                }
            }
            if (easyModbus2Mqtt == null)
                easyModbus2Mqtt = new EasyModbus2Mqtt();
            easyModbus2Mqtt.MqttBrokerPort = this.MqttBrokerPort;
            easyModbus2Mqtt.MqttUserName = this.MqttUserName;
            easyModbus2Mqtt.MqttPassword = this.MqttPassword;
            easyModbus2Mqtt.RetainMessages = this.mqttRetainMessages;
            easyModbus2Mqtt.publish(topic.ToArray(), payload.ToArray(), mqttBrokerAddress);
            return returnValue;
        }

        /// <summary>
        /// Read Input Registers from Master device (FC4).
        /// </summary>
        /// <param name="startingAddress">First input register to be read</param>
        /// <param name="quantity">Number of input registers to be read</param>
        /// <returns>Int Array which contains the input registers</returns>
        public int[] ReadInputRegisters(int startingAddress, int quantity)
		{
            
            if (debug) StoreLogData.Instance.Store("FC4 (Read Input Registers from Master device), StartingAddress: "+ startingAddress+", Quantity: " +quantity, System.DateTime.Now);
            transactionIdentifierInternal++;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
			if (startingAddress > 65535 | quantity >125)
			{
				if (debug) StoreLogData.Instance.Store("ArgumentException Throwed", System.DateTime.Now);
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
			}
			int[] response;
			this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.functionCode = 0x04;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
			Byte[] data = new byte[]{	this.transactionIdentifier[1],
							this.transactionIdentifier[0],
							this.protocolIdentifier[1],
							this.protocolIdentifier[0],
							this.length[1],
							this.length[0],
							this.unitIdentifier,
							this.functionCode,
							this.startingAddress[1],
							this.startingAddress[0],
							this.quantity[1],
							this.quantity[0],
                            this.crc[0],
                            this.crc[1]        
            };
            crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = crc[0];
            data[13] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                bytesToRead = 5 + 2 * quantity;


 //               serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, 8);
                if (debug)
                {
                	byte [] debugData = new byte[8];
            		Array.Copy(data, 6, debugData, 0, 8);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[8];
            		Array.Copy(data, 6, sendData, 0, 8);
            		SendDataChanged(this);
                    
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                }
               
                if (receivedUnitIdentifier != this.unitIdentifier)
              	 	data = new byte[2100];   
                else
                    countRetries = 0;
            }
			else if (tcpClient.Client.Connected | udpFlag)
			{
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length-2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length-2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                     if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
            		}
                    data = new Byte[2100];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
			}
            if (data[7] == 0x84 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x84 & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x84 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x84 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
            crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data[8]+3), 6));
                if ((crc[0] != data[data[8]+9] | crc[1] != data[data[8]+10]) & dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        return ReadInputRegisters(startingAddress, quantity);
                    }
                }
                else if (!dataReceived)
                {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");
                        
                    }
                    else
                    {
                        countRetries++;
                        return ReadInputRegisters(startingAddress, quantity);
                    }
                    
                }
            }
			response = new int[quantity];
			for (int i = 0; i < quantity; i++)
			{
				byte lowByte;
				byte highByte;
				highByte = data[9+i*2];
				lowByte = data[9+i*2+1];
				
				data[9+i*2] = lowByte;
				data[9+i*2+1] = highByte;
				
				response[i] = BitConverter.ToInt16(data,(9+i*2));
			}
    		return (response);
		}
	
	
		/// <summary>
		/// Write single Coil to Master device (FC5).
		/// </summary>
        /// <param name="startingAddress">Coil to be written</param>
		/// <param name="value">Coil Value to be written</param>
        public void WriteSingleCoil(int startingAddress, bool value)
        {
           
            if (debug) StoreLogData.Instance.Store("FC5 (Write single coil to Master device), StartingAddress: "+ startingAddress+", Value: " + value, System.DateTime.Now);
            transactionIdentifierInternal++;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
            byte[] coilValue = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)0x0006);
            this.functionCode = 0x05;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
            if (value == true)
            {
                coilValue = BitConverter.GetBytes((int)0xFF00);
            }
            else
            {
                coilValue = BitConverter.GetBytes((int)0x0000);
            }
            Byte[] data = new byte[]{	this.transactionIdentifier[1],
							this.transactionIdentifier[0],
							this.protocolIdentifier[1],
							this.protocolIdentifier[0],
							this.length[1],
							this.length[0],
							this.unitIdentifier,
							this.functionCode,
							this.startingAddress[1],
							this.startingAddress[0],
							coilValue[1],
							coilValue[0],
                            this.crc[0],
                            this.crc[1]    
                            };
            crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = crc[0];
            data[13] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                bytesToRead = 8;
 //               serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, 8);
                if (debug)
                {
                	byte [] debugData = new byte[8];
            		Array.Copy(data, 6, debugData, 0, 8);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[8];
            		Array.Copy(data, 6, sendData, 0, 8);
            		SendDataChanged(this);
                   
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                    countRetries = 0;
                }
              
            }
            else if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length - 2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length - 2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                    if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
                       
                    }                    
                    data = new Byte[2100];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
            }
            if (data[7] == 0x85 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x85 & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x85 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x85 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
             crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));           
             if ((crc[0] != data[12] | crc[1] != data[13]) & dataReceived)
             {
                	if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        WriteSingleCoil(startingAddress, value);
                    }
                }
             else if (!dataReceived)
             {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");

                    }
                    else
                    {
                        countRetries++;
                        WriteSingleCoil(startingAddress, value);
                    }
                }
            }
        }


        /// <summary>
        /// Write single Register to Master device (FC6).
        /// </summary>
        /// <param name="startingAddress">Register to be written</param>
        /// <param name="value">Register Value to be written</param>
        public void WriteSingleRegister(int startingAddress, int value)
        {
            if (debug) StoreLogData.Instance.Store("FC6 (Write single register to Master device), StartingAddress: "+ startingAddress+", Value: " + value, System.DateTime.Now);
            transactionIdentifierInternal++;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
            byte[] registerValue = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)0x0006);
            this.functionCode = 0x06;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
                registerValue = BitConverter.GetBytes((int)value);

            Byte[] data = new byte[]{	this.transactionIdentifier[1],
							this.transactionIdentifier[0],
							this.protocolIdentifier[1],
							this.protocolIdentifier[0],
							this.length[1],
							this.length[0],
							this.unitIdentifier,
							this.functionCode,
							this.startingAddress[1],
							this.startingAddress[0],
							registerValue[1],
							registerValue[0],
                            this.crc[0],
                            this.crc[1]    
                            };
            crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));
            data[12] = crc[0];
            data[13] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                bytesToRead = 8;
//                serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, 8);
                if (debug)
                {
                	byte [] debugData = new byte[8];
            		Array.Copy(data, 6, debugData, 0, 8);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[8];
            		Array.Copy(data, 6, sendData, 0, 8);
            		SendDataChanged(this);
                    
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                }
                if (receivedUnitIdentifier != this.unitIdentifier)
                	data = new byte[2100];   
                else
                    countRetries = 0;
            }
            else if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length - 2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length - 2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                     if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
                        
                    }                   
                    data = new Byte[2100];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
            }
            if (data[7] == 0x86 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x86 & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x86 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x86 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
             crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));           
             if ((crc[0] != data[12] | crc[1] != data[13]) & dataReceived)
             {
                if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        WriteSingleRegister(startingAddress, value);
                    }
                }
             else if (!dataReceived)
             {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");

                    }
                    else
                    {
                        countRetries++;
                        WriteSingleRegister(startingAddress, value);
                    }
                }
            }
        }

        /// <summary>
        /// Write multiple coils to Master device (FC15).
        /// </summary>
        /// <param name="startingAddress">First coil to be written</param>
        /// <param name="values">Coil Values to be written</param>
        public void WriteMultipleCoils(int startingAddress, bool[] values)
        {
            string debugString = "";
        	for (int i = 0; i < values.Length;i++)
        		debugString = debugString + values[i] + " ";
        	if (debug) StoreLogData.Instance.Store("FC15 (Write multiple coils to Master device), StartingAddress: "+ startingAddress+", Values: " + debugString, System.DateTime.Now);
            transactionIdentifierInternal++;
            byte byteCount = (byte)((values.Length % 8 != 0 ? values.Length / 8 + 1: (values.Length / 8)));
            byte[] quantityOfOutputs = BitConverter.GetBytes((int)values.Length);
            byte singleCoilValue = 0;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
            this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)(7+(byteCount)));
            this.functionCode = 0x0F;
            this.startingAddress = BitConverter.GetBytes(startingAddress);



            Byte[] data = new byte[14 +2 + (values.Length % 8 != 0 ? values.Length/8 : (values.Length / 8)-1)];
            data[0] = this.transactionIdentifier[1];
            data[1] = this.transactionIdentifier[0];
            data[2] = this.protocolIdentifier[1];
	        data[3] = this.protocolIdentifier[0];
			data[4] = this.length[1];
			data[5] = this.length[0];
			data[6] = this.unitIdentifier;
			data[7] = this.functionCode;
			data[8] = this.startingAddress[1];
			data[9] = this.startingAddress[0];
            data[10] = quantityOfOutputs[1];
            data[11] = quantityOfOutputs[0];
            data[12] = byteCount;
            for (int i = 0; i < values.Length; i++)
            {
                if ((i % 8) == 0)
                    singleCoilValue = 0;
                byte CoilValue;
                if (values[i] == true)
                    CoilValue = 1;
                else
                    CoilValue = 0;


                singleCoilValue = (byte)((int)CoilValue<<(i%8) | (int)singleCoilValue);

                data[13 + (i / 8)] = singleCoilValue;            
            }
            crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data.Length - 8), 6));
            data[data.Length - 2] = crc[0];
            data[data.Length - 1] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                bytesToRead = 8;
 //               serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, data.Length - 6);
                if (debug)
                {
                	byte [] debugData = new byte[data.Length - 6];
            		Array.Copy(data, 6, debugData, 0, data.Length - 6);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[data.Length - 6];
            		Array.Copy(data, 6, sendData, 0, data.Length - 6);
            		SendDataChanged(this);
                    
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                }
                if (receivedUnitIdentifier != this.unitIdentifier)
                	data = new byte[2100];     
                else
                    countRetries = 0;
            }
            else if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length-2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);                   
                }
                else
                {
                    stream.Write(data, 0, data.Length-2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                    if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
                        
                    }                    
                    data = new Byte[2100];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
            }
            if (data[7] == 0x8F & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x8F & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x8F & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x8F & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
             crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));           
             if ((crc[0] != data[12] | crc[1] != data[13]) & dataReceived)
             {
                if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        WriteMultipleCoils(startingAddress, values);
                    }
                }
             else if (!dataReceived)
              {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");

                    }
                    else
                    {
                        countRetries++;
                        WriteMultipleCoils(startingAddress, values);
                    }
                }
            }
        }

        /// <summary>
        /// Write multiple registers to Master device (FC16).
        /// </summary>
        /// <param name="startingAddress">First register to be written</param>
        /// <param name="values">register Values to be written</param>
        public void WriteMultipleRegisters(int startingAddress, int[] values)
        {
            string debugString = "";
        	for (int i = 0; i < values.Length;i++)
        		debugString = debugString + values[i] + " ";
        	if (debug) StoreLogData.Instance.Store("FC16 (Write multiple Registers to Server device), StartingAddress: "+ startingAddress+", Values: " + debugString, System.DateTime.Now);
            transactionIdentifierInternal++;
            byte byteCount = (byte)(values.Length * 2);
            byte[] quantityOfOutputs = BitConverter.GetBytes((int)values.Length);
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
            this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)(7+values.Length*2));
            this.functionCode = 0x10;
            this.startingAddress = BitConverter.GetBytes(startingAddress);

            Byte[] data = new byte[13+2 + values.Length*2];
            data[0] = this.transactionIdentifier[1];
            data[1] = this.transactionIdentifier[0];
            data[2] = this.protocolIdentifier[1];
            data[3] = this.protocolIdentifier[0];
            data[4] = this.length[1];
            data[5] = this.length[0];
            data[6] = this.unitIdentifier;
            data[7] = this.functionCode;
            data[8] = this.startingAddress[1];
            data[9] = this.startingAddress[0];
            data[10] = quantityOfOutputs[1];
            data[11] = quantityOfOutputs[0];
            data[12] = byteCount;
            for (int i = 0; i < values.Length; i++)
            {
                byte[] singleRegisterValue = BitConverter.GetBytes((int)values[i]);
                data[13 + i*2] = singleRegisterValue[1];
                data[14 + i*2] = singleRegisterValue[0];
            }
            crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data.Length - 8), 6));
            data[data.Length - 2] = crc[0];
            data[data.Length - 1] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                bytesToRead = 8;
//                serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, data.Length - 6);

                if (debug)
                {
                	byte [] debugData = new byte[data.Length - 6];
            		Array.Copy(data, 6, debugData, 0, data.Length - 6);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[data.Length - 6];
            		Array.Copy(data, 6, sendData, 0, data.Length - 6);
            		SendDataChanged(this);
                   
                }
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                    	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                }
                if (receivedUnitIdentifier != this.unitIdentifier)
                	data = new byte[2100];    
                else
                    countRetries = 0;
            }
            else if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length-2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length-2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                     if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
            		}                   
                    data = new Byte[2100];
                    int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
            }
            if (data[7] == 0x90 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x90 & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x90 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x90 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            if (serialport != null)
            {
             crc = BitConverter.GetBytes(calculateCRC(data, 6, 6));           
             if ((crc[0] != data[12] | crc[1] != data[13])  &dataReceived)
             {
                if (debug) StoreLogData.Instance.Store("CRCCheckFailedException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new EasyModbus.Exceptions.CRCCheckFailedException("Response CRC check failed");
                    }
                    else
                    {
                        countRetries++;
                        WriteMultipleRegisters(startingAddress, values);
                    }
                }
             else if (!dataReceived)
             {
                	if (debug) StoreLogData.Instance.Store("TimeoutException Throwed", System.DateTime.Now);
                    if (NumberOfRetries <= countRetries)
                    {
                        countRetries = 0;
                        throw new TimeoutException("No Response from Modbus Slave");

                    }
                    else
                    {
                        countRetries++;
                        WriteMultipleRegisters(startingAddress, values);
                    }
                }
            }
        }

        /// <summary>
        /// Read/Write Multiple Registers (FC23).
        /// </summary>
        /// <param name="startingAddressRead">First input register to read</param>
        /// <param name="quantityRead">Number of input registers to read</param>
        /// <param name="startingAddressWrite">First input register to write</param>
        /// <param name="values">Values to write</param>
        /// <returns>Int Array which contains the Holding registers</returns>
        public int[] ReadWriteMultipleRegisters(int startingAddressRead, int quantityRead, int startingAddressWrite, int[] values)
        {

            string debugString = "";
        	for (int i = 0; i < values.Length;i++)
        		debugString = debugString + values[i] + " ";
        	if (debug) StoreLogData.Instance.Store("FC23 (Read and Write multiple Registers to Server device), StartingAddress Read: "+ startingAddressRead+ ", Quantity Read: "+quantityRead+", startingAddressWrite: " + startingAddressWrite +", Values: " + debugString, System.DateTime.Now);
            transactionIdentifierInternal++;
            byte [] startingAddressReadLocal = new byte[2];
		    byte [] quantityReadLocal = new byte[2];
            byte[] startingAddressWriteLocal = new byte[2];
            byte[] quantityWriteLocal = new byte[2];
            byte writeByteCountLocal = 0;
            if (serialport != null)
                if (!serialport.IsOpen)
            	{
            		if (debug) StoreLogData.Instance.Store("SerialPortNotOpenedException Throwed", System.DateTime.Now);
                    throw new EasyModbus.Exceptions.SerialPortNotOpenedException("serial port not opened");
            	}
            if (tcpClient == null & !udpFlag & serialport == null)
            {
				if (debug) StoreLogData.Instance.Store("ConnectionException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ConnectionException("connection error");
			}
            if (startingAddressRead > 65535 | quantityRead > 125 | startingAddressWrite > 65535 | values.Length > 121)
            {
				if (debug) StoreLogData.Instance.Store("ArgumentException Throwed", System.DateTime.Now);
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
			}
            int[] response;
            this.transactionIdentifier = BitConverter.GetBytes((uint)transactionIdentifierInternal);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)0x0006);
            this.functionCode = 0x17;
            startingAddressReadLocal = BitConverter.GetBytes(startingAddressRead);
            quantityReadLocal = BitConverter.GetBytes(quantityRead);
            startingAddressWriteLocal = BitConverter.GetBytes(startingAddressWrite);
            quantityWriteLocal = BitConverter.GetBytes(values.Length);
            writeByteCountLocal = Convert.ToByte(values.Length * 2);
            Byte[] data = new byte[17 +2+ values.Length*2];
            data[0] =               this.transactionIdentifier[1];
            data[1] =   		    this.transactionIdentifier[0];
			data[2] =   			this.protocolIdentifier[1];
			data[3] =   			this.protocolIdentifier[0];
			data[4] =   			this.length[1];
			data[5] =   			this.length[0];
			data[6] =   			this.unitIdentifier;
			data[7] =   		    this.functionCode;
			data[8] =   			startingAddressReadLocal[1];
			data[9] =   			startingAddressReadLocal[0];
			data[10] =   			quantityReadLocal[1];
			data[11] =   			quantityReadLocal[0];
            data[12] =               startingAddressWriteLocal[1];
			data[13] =   			startingAddressWriteLocal[0];
			data[14] =   			quantityWriteLocal[1];
			data[15] =   			quantityWriteLocal[0];
            data[16] =              writeByteCountLocal;

            for (int i = 0; i < values.Length; i++)
            {
                byte[] singleRegisterValue = BitConverter.GetBytes((int)values[i]);
                data[17 + i*2] = singleRegisterValue[1];
                data[18 + i*2] = singleRegisterValue[0];
            }
            crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data.Length - 8), 6));
            data[data.Length - 2] = crc[0];
            data[data.Length - 1] = crc[1];
            if (serialport != null)
            {
                dataReceived = false;
                bytesToRead = 5 + 2*quantityRead;
 //               serialport.ReceivedBytesThreshold = bytesToRead;
                serialport.Write(data, 6, data.Length - 6);
                if (debug)
                {
                	byte [] debugData = new byte[data.Length - 6];
            		Array.Copy(data, 6, debugData, 0, data.Length - 6);
            		if (debug) StoreLogData.Instance.Store("Send Serial-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
               if (SendDataChanged != null)
            	{
            		sendData = new byte[data.Length - 6];
            		Array.Copy(data, 6, sendData, 0, data.Length - 6);
            		SendDataChanged(this);
            	}
                data = new byte[2100];
                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.Now;
                byte receivedUnitIdentifier = 0xFF;
                while (receivedUnitIdentifier != this.unitIdentifier & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                {
                	while (dataReceived == false & !((DateTime.Now.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * this.connectTimeout))
                   	 	System.Threading.Thread.Sleep(1);  
                	data = new byte[2100];
                	Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                	receivedUnitIdentifier = data[6];
                }
                if (receivedUnitIdentifier != this.unitIdentifier)
              	 	data = new byte[2100];               
                else
                    countRetries = 0;
            }
            else if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length-2, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length-2);
                    if (debug)
                {
                	byte [] debugData = new byte[data.Length-2];
            		Array.Copy(data, 0, debugData, 0, data.Length-2);
            		if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: "+BitConverter.ToString(debugData) ,System.DateTime.Now);          		
                }
                     if (SendDataChanged != null)
            		{
            			sendData = new byte[data.Length-2];
            			Array.Copy(data, 0, sendData, 0, data.Length-2);
            			SendDataChanged(this);
                        
                    }                   
                    data = new Byte[2100];
                     int NumberOfBytes = stream.Read(data, 0, data.Length);
                    if (ReceiveDataChanged != null)
            		{
            			receiveData = new byte[NumberOfBytes];
            			Array.Copy(data, 0, receiveData, 0, NumberOfBytes);
                        if (debug) StoreLogData.Instance.Store("Receive ModbusTCP-Data: " + BitConverter.ToString(receiveData), System.DateTime.Now);
                        ReceiveDataChanged(this);
            		}
                }
            }
            if (data[7] == 0x97 & data[8] == 0x01)
            {
            	if (debug) StoreLogData.Instance.Store("FunctionCodeNotSupportedException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.FunctionCodeNotSupportedException("Function code not supported by master");
            }
            if (data[7] == 0x97 & data[8] == 0x02)
            {
            	if (debug) StoreLogData.Instance.Store("StartingAddressInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.StartingAddressInvalidException("Starting address invalid or starting address + quantity invalid");
            }
            if (data[7] == 0x97 & data[8] == 0x03)
            {
            	if (debug) StoreLogData.Instance.Store("QuantityInvalidException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.QuantityInvalidException("quantity invalid");
            }
            if (data[7] == 0x97 & data[8] == 0x04)
            {
            	if (debug) StoreLogData.Instance.Store("ModbusException Throwed", System.DateTime.Now);
                throw new EasyModbus.Exceptions.ModbusException("error reading");
            }
            response = new int[quantityRead];
            for (int i = 0; i < quantityRead; i++)
            {
                byte lowByte;
                byte highByte;
                highByte = data[9 + i * 2];
                lowByte = data[9 + i * 2 + 1];

                data[9 + i * 2] = lowByte;
                data[9 + i * 2 + 1] = highByte;

                response[i] = BitConverter.ToInt16(data, (9 + i * 2));
            }
            return (response);
        }
	
		/// <summary>
		/// Close connection to Master Device.
		/// </summary>
		public void Disconnect()
		{
			if (debug) StoreLogData.Instance.Store("Disconnect", System.DateTime.Now);
            if (serialport != null)
            {
                if (serialport.IsOpen & !this.receiveActive)
                    serialport.Close();
                if (ConnectedChanged != null)
                    ConnectedChanged(this);
                return;
            }
            if (stream != null)
			    stream.Close();
            if (tcpClient != null)
			    tcpClient.Close();
            connected = false;
            if (ConnectedChanged != null)
                ConnectedChanged(this);

        }

        /// <summary>
        /// Destructor - Close connection to Master Device.
        /// </summary>
		~ ModbusClient()
		{
			if (debug) StoreLogData.Instance.Store("Destructor called - automatically disconnect", System.DateTime.Now);
            if (serialport != null)
            {
                if (serialport.IsOpen)
                    serialport.Close();
                return;
            }
			if (tcpClient != null & !udpFlag)
			{
                if (stream !=null)
				stream.Close();
			    tcpClient.Close();
			}
		}

        /// <summary>
        /// Returns "TRUE" if Client is connected to Server and "FALSE" if not. In case of Modbus RTU returns if COM-Port is opened
        /// </summary>
		public bool Connected
		{
			get
			{
                if (serialport != null)
                {
                    return (serialport.IsOpen);
                }

                if (udpFlag & tcpClient != null)
                    return true;
                if (tcpClient == null)
                    return false;
                else
                {
                    return connected;

                }

			}
		}

        public bool Available(int timeout)
        {
            // Ping's the local machine.
            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            IPAddress address = System.Net.IPAddress.Parse(ipAddress);

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);

            // Wait 10 seconds for a reply.
            System.Net.NetworkInformation.PingReply reply = pingSender.Send(address, timeout, buffer);

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets or Sets the IP-Address of the Server.
        /// </summary>
		public string IPAddress
		{
			get
			{
				return ipAddress;
			}
			set
			{
				ipAddress = value;
			}
		}

        /// <summary>
        /// Gets or Sets the Port were the Modbus-TCP Server is reachable (Standard is 502).
        /// </summary>
		public int Port
		{
			get
			{
				return port;
			}
			set
			{
				port = value;
			}
		}

        /// <summary>
        /// Gets or Sets the UDP-Flag to activate Modbus UDP.
        /// </summary>
        public bool UDPFlag
        {
            get
            {
                return udpFlag;
            }
            set
            {
                udpFlag = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Unit identifier in case of serial connection (Default = 0)
        /// </summary>
        public byte UnitIdentifier
        {
            get
            {
                return unitIdentifier;
            }
            set
            {
                unitIdentifier = value;
            }
        }


        /// <summary>
        /// Gets or Sets the Baudrate for serial connection (Default = 9600)
        /// </summary>
        public int Baudrate
        {
            get
            {
                return baudRate;
            }
            set
            {
                baudRate = value;
            }
        }

        /// <summary>
        /// Gets or Sets the of Parity in case of serial connection
        /// </summary>
        public Parity Parity
        {
            get
            {
                if (serialport != null)
                    return parity;
                else
                    return Parity.Even;
            }
            set
            {
                if (serialport != null)
                    parity = value;
            }
        }


        /// <summary>
        /// Gets or Sets the number of stopbits in case of serial connection
        /// </summary>
        public StopBits StopBits
        {
            get
            {
                if (serialport != null)
                    return stopBits;
                else
                    return StopBits.One;
            }
            set
            {
                if (serialport != null)
                    stopBits = value;
            }
        }

        /// <summary>
        /// Gets or Sets the connection Timeout in case of ModbusTCP connection
        /// </summary>
        public int ConnectionTimeout
        {
            get
            {
                return connectTimeout;
            }
            set
            {
                connectTimeout = value;
            }
        }

        /// <summary>
        /// Gets or Sets the serial Port
        /// </summary>
        public string SerialPort
        {
            get
            {

                return serialport.PortName;
            }
            set
            {
                if (value == null)
                {
                    serialport = null;
                    return;
                }
                if (serialport != null)
                    serialport.Close();
                this.serialport = new SerialPort();
                this.serialport.PortName = value;               
                serialport.BaudRate = baudRate;
                serialport.Parity = parity;
                serialport.StopBits = stopBits;
                serialport.WriteTimeout = 10000;
                serialport.ReadTimeout = connectTimeout;
                serialport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            }
        }

        /// <summary>
        /// Gets or Sets the Filename for the LogFile
        /// </summary>
        public string LogFileFilename
        {
            get
            {
                return StoreLogData.Instance.Filename;
            }
            set
            {
                StoreLogData.Instance.Filename = value;
                if (StoreLogData.Instance.Filename != null)
                    debug = true;
                else
                    debug = false;
            }
        }

        /// <summary>
        /// Gets or Sets the Mqtt Root Topic
        /// </summary>
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

        /// <summary>
        /// Disables or Enables to Retain the Messages in the Broker - default is false (Enabled)
        /// </summary>
        public bool MqttRetainMessages
        {
            get
            {
                return this.mqttRetainMessages;
            }
            set
            {
                this.mqttRetainMessages = value;
            }
        }

    }
}
