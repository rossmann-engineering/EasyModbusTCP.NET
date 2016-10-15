/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Stefan Roßmann
 * Datum: 16.06.2011
 * Zeit: 21:35
 */
using System;
using System.Net.Sockets;
using System.Net;
namespace SRModbusTCP
{
	/// <summary>
	/// SR_Modbus_TCP implements a ModbusClient.
	/// </summary>
	public class ModbusTCP
	{
		private TcpClient tcpClient;
		private string ipAddress = "127.0.0.1";
		private int port = 502;
		private byte [] transactionIdentifier = new byte[2];
		private byte [] protocolIdentifier = new byte[2];
		private byte [] length = new byte[2];
		private byte unitIdentifier;
		private byte functionCode;
		private byte [] startingAddress = new byte[2];
		private byte [] quantity = new byte[2];
        private bool udpFlag = false;
        private int portOut;
		NetworkStream stream;
		
		/// <summary>
		/// Constructor which determines the Master ip-address and the Master Port.
		/// </summary>
		/// <param name="ipAddress">IP-Address of the Master device</param>
		/// <param name="port">Listening port of the Master device (should be 502)</param>
		public ModbusTCP(string ipAddress, int port)
		{
			this.ipAddress = ipAddress;
			this.port = port;
		}

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public ModbusTCP()
        {
        }
		
		/// <summary>
		/// Establish connection to Master device in case of Modbus TCP.
		/// </summary>
		public void Connect()
		{
            if (!udpFlag)
            {
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();
            }
            else
                tcpClient = new TcpClient();
		}
		
		/// <summary>
		/// Establish connection to Master device in case of Modbus TCP.
		/// </summary>
		public void Connect(string ipAddress, int port)
		{
            if (!udpFlag)
            {
                this.ipAddress = ipAddress;
                this.port = port;
                tcpClient = new TcpClient(ipAddress, port);
                stream = tcpClient.GetStream();
            }
            else
                tcpClient = new TcpClient();
		}
		
		/// <summary>
		/// Read Discrete Inputs from Master device (FC2).
		/// </summary>
		/// <param name="startingAddress">First discrete input to be read</param>
		/// <param name="quantity">Numer of discrete Inputs to be read</param>
		/// <returns>Boolean Array which contains the discrete Inputs</returns>
		public bool[] ReadDiscreteInputs(int startingAddress, int quantity)
		{
			if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
			if (startingAddress > 65535 | quantity >2000)
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
			bool[] response;
			this.transactionIdentifier = BitConverter.GetBytes((int) 0x0001);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.unitIdentifier = 0x00;
			this.functionCode = 0x02;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
			Byte[] data = new byte[12]
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
							this.quantity[0]
                            };
            
			if (tcpClient.Client.Connected | udpFlag)
			{
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
			if (data[7] == 0x82 & data[8] == 0x01)
				throw new Exception("Function code not supported by master");
			if (data[7] == 0x82 & data[8] == 0x02)
				throw new Exception("Starting address invalid or starting address + quantity invalid");
			if (data[7] == 0x82 & data[8] == 0x03)
				throw new Exception("quantity invalid");
			if (data[7] == 0x82 & data[8] == 0x04)
				throw new Exception("error reading");
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
		/// Read Coils from Master device (FC1).
		/// </summary>
		/// <param name="startingAddress">First coil to be read</param>
		/// <param name="quantity">Numer of coils to be read</param>
		/// <returns>Boolean Array which contains the coils</returns>
		public bool[] ReadCoils(int startingAddress, int quantity)
		{
            if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
			if (startingAddress > 65535 | quantity >2000)
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
			bool[] response;
			this.transactionIdentifier = BitConverter.GetBytes((int) 0x0001);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.unitIdentifier = 0x00;
			this.functionCode = 0x01;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
			Byte[] data = new byte[12]{	this.transactionIdentifier[1],
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
							this.quantity[0]};
			if (tcpClient.Client.Connected | udpFlag)
			{
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
			if (data[7] == 0x81 & data[8] == 0x01)
				throw new Exception("Function code not supported by master");
			if (data[7] == 0x81 & data[8] == 0x02)
				throw new Exception("Starting address invalid or starting address + quantity invalid");
			if (data[7] == 0x81 & data[8] == 0x03)
				throw new Exception("quantity invalid");
			if (data[7] == 0x81 & data[8] == 0x04)
				throw new Exception("error reading");
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
		/// <param name="quantity">Numer of holding registers to be read</param>
		/// <returns>Int Array which contains the holding registers</returns>
		public int[] ReadHoldingRegisters(int startingAddress, int quantity)
		{
            if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
			if (startingAddress > 65535 | quantity >125)
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
			int[] response;
			this.transactionIdentifier = BitConverter.GetBytes((int) 0x0001);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.unitIdentifier = 0x00;
			this.functionCode = 0x03;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
			Byte[] data = new byte[12]{	this.transactionIdentifier[1],
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
							this.quantity[0]};
			if (tcpClient.Client.Connected | udpFlag)
			{
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
			if (data[7] == 0x83 & data[8] == 0x01)
				throw new Exception("Function code not supported by master");
			if (data[7] == 0x83 & data[8] == 0x02)
				throw new Exception("Starting address invalid or starting address + quantity invalid");
			if (data[7] == 0x83 & data[8] == 0x03)
				throw new Exception("quantity invalid");
			if (data[7] == 0x83 & data[8] == 0x04)
				throw new Exception("error reading");
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
		/// <param name="quantity">Numer of input registers to be read</param>
		/// <returns>Int Array which contains the input registers</returns>
		public int[] ReadInputRegisters(int startingAddress, int quantity)
		{
            if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
			if (startingAddress > 65535 | quantity >125)
				throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 125");
			int[] response;
			this.transactionIdentifier = BitConverter.GetBytes((int) 0x0001);
			this.protocolIdentifier = BitConverter.GetBytes((int) 0x0000);
			this.length = BitConverter.GetBytes((int)0x0006);
			this.unitIdentifier = 0x00;
			this.functionCode = 0x04;
			this.startingAddress = BitConverter.GetBytes(startingAddress);
			this.quantity = BitConverter.GetBytes(quantity);
			Byte[] data = new byte[12]{	this.transactionIdentifier[1],
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
							this.quantity[0]};
			if (tcpClient.Client.Connected | udpFlag)
			{
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
			if (data[7] == 0x84 & data[8] == 0x01)
				throw new Exception("Function code not supported by master");
			if (data[7] == 0x84 & data[8] == 0x02)
				throw new Exception("Starting address invalid or starting address + quantity invalid");
			if (data[7] == 0x84 & data[8] == 0x03)
				throw new Exception("quantity invalid");
			if (data[7] == 0x84 & data[8] == 0x04)
				throw new Exception("error reading");
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
            if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
            byte[] coilValue = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes((int)0x0001);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)0x0006);
            this.unitIdentifier = 0x00;
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
            Byte[] data = new byte[12]{	this.transactionIdentifier[1],
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
							coilValue[0]
                            };
            if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
                if (data[7] == 0x85 & data[8] == 0x01)
                    throw new Exception("Function code not supported by master");
                if (data[7] == 0x85 & data[8] == 0x02)
                    throw new Exception("Starting address invalid or starting address + quantity invalid");
                if (data[7] == 0x85 & data[8] == 0x03)
                    throw new Exception("quantity invalid");
                if (data[7] == 0x85 & data[8] == 0x04)
                    throw new Exception("error reading");
            }
        }


        /// <summary>
        /// Write single Register to Master device (FC6).
        /// </summary>
        /// <param name="startingAddress">Register to be written</param>
        /// <param name="value">Register Value to be written</param>
        public void WriteSingleRegister(int startingAddress, int value)
        {
            if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
            byte[] registerValue = new byte[2];
            this.transactionIdentifier = BitConverter.GetBytes((int)0x0001);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)0x0006);
            this.unitIdentifier = 0x00;
            this.functionCode = 0x06;
            this.startingAddress = BitConverter.GetBytes(startingAddress);
                registerValue = BitConverter.GetBytes((int)value);

            Byte[] data = new byte[12]{	this.transactionIdentifier[1],
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
							registerValue[0]
                            };
            if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
                if (data[7] == 0x86 & data[8] == 0x01)
                    throw new Exception("Function code not supported by master");
                if (data[7] == 0x86 & data[8] == 0x02)
                    throw new Exception("Starting address invalid or starting address + quantity invalid");
                if (data[7] == 0x86 & data[8] == 0x03)
                    throw new Exception("quantity invalid");
                if (data[7] == 0x86 & data[8] == 0x04)
                    throw new Exception("error reading");
            }
        }

        /// <summary>
        /// Write multiple coils to Master device (FC15).
        /// </summary>
        /// <param name="startingAddress">First coil to be written</param>
        /// <param name="values">Coil Values to be written</param>
        public void WriteMultipleCoils(int startingAddress, bool[] values)
        {
            byte byteCount = (byte)(values.Length/8+1);
            byte[] quantityOfOutputs = BitConverter.GetBytes((int)values.Length);
            byte singleCoilValue = 0;
            if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
            this.transactionIdentifier = BitConverter.GetBytes((int)0x0001);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)(7+(values.Length/8+1)));
            this.unitIdentifier = 0x00;
            this.functionCode = 0x0F;
            this.startingAddress = BitConverter.GetBytes(startingAddress);

            Byte[] data = new byte[14 + values.Length/8];
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
            if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                    
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
                if (data[7] == 0x8F & data[8] == 0x01)
                    throw new Exception("Function code not supported by master");
                if (data[7] == 0x8F & data[8] == 0x02)
                    throw new Exception("Starting address invalid or starting address + quantity invalid");
                if (data[7] == 0x8F & data[8] == 0x03)
                    throw new Exception("quantity invalid");
                if (data[7] == 0x8F & data[8] == 0x04)
                    throw new Exception("error reading");
            }
        }

        /// <summary>
        /// Write multiple registers to Master device (FC16).
        /// </summary>
        /// <param name="startingAddress">First register to be written</param>
        /// <param name="values">register Values to be written</param>
        public void WriteMultipleRegisters(int startingAddress, int[] values)
        {
            byte byteCount = (byte)(values.Length * 2);
            byte[] quantityOfOutputs = BitConverter.GetBytes((int)values.Length);
            if (tcpClient == null & !udpFlag)
                throw new Exception("connection error");
            this.transactionIdentifier = BitConverter.GetBytes((int)0x0001);
            this.protocolIdentifier = BitConverter.GetBytes((int)0x0000);
            this.length = BitConverter.GetBytes((int)(7+values.Length*2));
            this.unitIdentifier = 0x00;
            this.functionCode = 0x10;
            this.startingAddress = BitConverter.GetBytes(startingAddress);

            Byte[] data = new byte[13 + values.Length*2];
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
            if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    IPEndPoint endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length, endPoint);
                    portOut = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(data, 0, data.Length);
                    data = new Byte[2100];
                    stream.Read(data, 0, data.Length);
                }
                if (data[7] == 0x10 & data[8] == 0x01)
                    throw new Exception("Function code not supported by master");
                if (data[7] == 0x10 & data[8] == 0x02)
                    throw new Exception("Starting address invalid or starting address + quantity invalid");
                if (data[7] == 0x10 & data[8] == 0x03)
                    throw new Exception("quantity invalid");
                if (data[7] == 0x10 & data[8] == 0x04)
                    throw new Exception("error reading");
            }
        }
		
		/// <summary>
		/// Close connection to Master Device.
		/// </summary>
		public void Disconnect()
		{
            if (stream != null)
			    stream.Close();
            if (tcpClient != null)
			    tcpClient.Close();		    
		}

        /// <summary>
        /// Destructor - Close connection to Master Device.
        /// </summary>
		~ ModbusTCP()
		{
			if (tcpClient != null & !udpFlag)
			{
				stream.Close();
			tcpClient.Close();
			}
		}

        /// <summary>
        /// Returns "TRUE" if Client is connected to Server and "FALSE" if not.
        /// </summary>
		public bool Connected
		{
			get
			{
                if (udpFlag & tcpClient != null)
                    return true;
				if (tcpClient == null)
					return false;
				else
				return tcpClient.Connected;
			}
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
	}
}
