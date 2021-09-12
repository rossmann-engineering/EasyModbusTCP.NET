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
        public int NumberOfRetries { get; set; } = 3;
        private int countRetries = 0;

        private bool dataReceived = false;
        private bool receiveActive = false;
        private byte[] readBuffer = new byte[256];
        private int bytesToRead = 0;

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
            this.length = BitConverter.GetBytes((int)11 + values.Length * 2);
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

    }
}
