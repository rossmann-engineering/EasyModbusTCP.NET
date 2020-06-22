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
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;

namespace EasyModbus
{
#region class ModbusProtocol
    /// <summary>
    /// Modbus Protocol informations.
    /// </summary>
    public class ModbusProtocol
    {
    	public enum ProtocolType { ModbusTCP = 0, ModbusUDP = 1, ModbusRTU = 2};
        public DateTime timeStamp;
        public bool request;
        public bool response;
        public UInt16 transactionIdentifier;
        public UInt16 protocolIdentifier;
        public UInt16 length;
        public byte unitIdentifier;
        public byte functionCode;
        public UInt16 startingAdress;
        public UInt16 startingAddressRead;
        public UInt16 startingAddressWrite;
        public UInt16 quantity;
        public UInt16 quantityRead;
        public UInt16 quantityWrite;
        public byte byteCount;
        public byte exceptionCode;
        public byte errorCode;
        public UInt16[] receiveCoilValues;
        public UInt16[] receiveRegisterValues;
        public Int16[] sendRegisterValues;
        public bool[] sendCoilValues;    
        public UInt16 crc;
    }
#endregion

#region structs
    struct NetworkConnectionParameter
    {
        public NetworkStream stream;        //For TCP-Connection only
        public Byte[] bytes;
        public int portIn;                  //For UDP-Connection only
        public IPAddress ipAddressIn;       //For UDP-Connection only
    }
#endregion

#region TCPHandler class
    internal class TCPHandler
    {
        public delegate void DataChanged(object networkConnectionParameter);
        public event DataChanged dataChanged;

        public delegate void NumberOfClientsChanged();
        public event NumberOfClientsChanged numberOfClientsChanged;

        TcpListener server = null;
        

        private List<Client> tcpClientLastRequestList = new List<Client>();

        public int NumberOfConnectedClients { get; set; }

        public string ipAddress = null;

        public TCPHandler(int port)
        {
            IPAddress localAddr = IPAddress.Any;
            server = new TcpListener(localAddr, port);
            server.Start();
            server.AcceptTcpClientAsync().Start();
        }

        public TCPHandler(string ipAddress, int port)
        {
            this.ipAddress = ipAddress;
            IPAddress localAddr = IPAddress.Any;
            server = new TcpListener(localAddr, port);
            server.Start();
            server.AcceptTcpClientAsync().Start();
        }


        private async void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
               
                tcpClient.ReceiveTimeout = 4000;
                if (ipAddress != null)
                {
                    string ipEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                    ipEndpoint = ipEndpoint.Split(':')[0];
                    if (ipEndpoint != ipAddress)
                    {
                        tcpClient.Client.Dispose();
                        return;
                    }
                }
            }
            catch (Exception) { }
            try
            {
                
                Client client = new Client(tcpClient);
                NetworkStream networkStream = client.NetworkStream;
                networkStream.ReadTimeout = 4000;
                byte[] result = new byte[client.Buffer.Length];
                await networkStream.ReadAsync(result, 0, client.Buffer.Length);
                this.ReadCallback(result);
            }
            catch (Exception) { }
        }

        private int GetAndCleanNumberOfConnectedClients(Client client)
        {
            lock (this)
            {
                int i = 0;
                bool objetExists = false;
                foreach (Client clientLoop in tcpClientLastRequestList)
                {
                    if (client.Equals(clientLoop))
                        objetExists = true;
                }
                try
                {
                    tcpClientLastRequestList.RemoveAll(delegate (Client c)
                        {
                            return ((DateTime.Now.Ticks - c.Ticks) > 40000000);
                        }

                        );
                }
                catch (Exception) { }
                if (!objetExists)
                    tcpClientLastRequestList.Add(client);


                return tcpClientLastRequestList.Count;
            }
        }

        private void ReadCallback(byte[] result)
        {
            NetworkConnectionParameter networkConnectionParameter = new NetworkConnectionParameter();
            //NumberOfConnectedClients = GetAndCleanNumberOfConnectedClients(client);
            if (numberOfClientsChanged != null)
                numberOfClientsChanged();

                //networkConnectionParameter.bytes = data;
                //networkConnectionParameter.stream = networkStream;
                if (dataChanged != null)
                    dataChanged(networkConnectionParameter);
               
            
        }

        public void Disconnect()
        {
            try
            {
                foreach (Client clientLoop in tcpClientLastRequestList)
                {
                    clientLoop.NetworkStream.Dispose();
                }
            }
            catch (Exception) { }
            server.Stop();
            
        }


        internal class Client
        {
            private readonly TcpClient tcpClient;
            private readonly byte[] buffer;
            public long Ticks { get; set; }

            public Client(TcpClient tcpClient)
            {
                this.tcpClient = tcpClient;
                int bufferSize = tcpClient.ReceiveBufferSize;
                buffer = new byte[bufferSize];
            }

            public TcpClient TcpClient
            {
                get { return tcpClient; }
            }

            public byte[] Buffer
            {
                get { return buffer; }
            }

            public NetworkStream NetworkStream
            {
                get {
                    
                        return tcpClient.GetStream();

                }
            }
        }
    }
#endregion
    
    /// <summary>
    /// Modbus TCP Server.
    /// </summary>
    public class ModbusServer
    {
        private bool debug = false;
        Int32 port = 502;
        ModbusProtocol receiveData;
        ModbusProtocol sendData =  new ModbusProtocol();
        Byte[] bytes = new Byte[2100];
        //public Int16[] _holdingRegisters = new Int16[65535];
        public HoldingRegisters holdingRegisters;      
        public InputRegisters inputRegisters;
        public Coils coils;
        public DiscreteInputs discreteInputs;
        private int numberOfConnections = 0;
        private byte unitIdentifier = 1;
        private int portIn;
        private IPAddress ipAddressIn;
        private UdpClient udpClient;
        private IPEndPoint iPEndPoint;
        private TCPHandler tcpHandler;
        Thread listenerThread;
        Thread clientConnectionThread;
        private ModbusProtocol[] modbusLogData = new ModbusProtocol[100];
        public bool FunctionCode1Disabled {get; set;}
        public bool FunctionCode2Disabled { get; set; }
        public bool FunctionCode3Disabled { get; set; }
        public bool FunctionCode4Disabled { get; set; }
        public bool FunctionCode5Disabled { get; set; }
        public bool FunctionCode6Disabled { get; set; }
        public bool FunctionCode15Disabled { get; set; }
        public bool FunctionCode16Disabled { get; set; }
        public bool FunctionCode23Disabled { get; set; }
        public bool PortChanged { get; set; }
        object lockCoils = new object();
        object lockHoldingRegisters = new object();
        private volatile bool shouldStop;
        



        public ModbusServer()
        {

            holdingRegisters = new HoldingRegisters(this);
            inputRegisters = new InputRegisters(this);
            coils = new Coils(this);
            discreteInputs = new DiscreteInputs(this);
 

        }

        #region events
        public delegate void CoilsChanged(int coil, int numberOfCoils);
        public event CoilsChanged coilsChanged;

        public delegate void HoldingRegistersChanged(int register, int numberOfRegisters);
        public event HoldingRegistersChanged holdingRegistersChanged;

        public delegate void NumberOfConnectedClientsChanged();
        public event NumberOfConnectedClientsChanged numberOfConnectedClientsChanged;

        public delegate void LogDataChanged();
        public event LogDataChanged logDataChanged;
        #endregion

        public void Listen()
        {
            
            listenerThread = new Thread(ListenerThread);
            listenerThread.Start();
        }

        public void StopListening()
        {

            try
            {
                tcpHandler.Disconnect();
                //listenerThread.Abort();
                
            }
            catch (Exception) { }
            listenerThread.Join();
            try
            {

               // clientConnectionThread.();
            }
            catch (Exception) { }
        }
        
        private void ListenerThread()
        {
    
                tcpHandler = new TCPHandler(port);
                if (debug) StoreLogData.Instance.Store("EasyModbus Server listing for incomming data at Port " + port, System.DateTime.Now);
                tcpHandler.dataChanged += new TCPHandler.DataChanged(ProcessReceivedData);
                tcpHandler.numberOfClientsChanged += new TCPHandler.NumberOfClientsChanged(numberOfClientsChanged);       
          
        }
    
 
		#region Method numberOfClientsChanged
        private void numberOfClientsChanged()
        {
            numberOfConnections = tcpHandler.NumberOfConnectedClients;
            if (numberOfConnectedClientsChanged != null)
                numberOfConnectedClientsChanged();
        }
        #endregion

        object lockProcessReceivedData = new object();
        #region Method ProcessReceivedData
        private void ProcessReceivedData(object networkConnectionParameter)
        {
            lock (lockProcessReceivedData)
            {
                Byte[] bytes = new byte[((NetworkConnectionParameter)networkConnectionParameter).bytes.Length];
                if (debug) StoreLogData.Instance.Store("Received Data: " + BitConverter.ToString(bytes), System.DateTime.Now);
                NetworkStream stream = ((NetworkConnectionParameter)networkConnectionParameter).stream;
                int portIn = ((NetworkConnectionParameter)networkConnectionParameter).portIn;
                IPAddress ipAddressIn = ((NetworkConnectionParameter)networkConnectionParameter).ipAddressIn;


                Array.Copy(((NetworkConnectionParameter)networkConnectionParameter).bytes, 0, bytes, 0, ((NetworkConnectionParameter)networkConnectionParameter).bytes.Length);

                ModbusProtocol receiveDataThread = new ModbusProtocol();
                ModbusProtocol sendDataThread = new ModbusProtocol();

                try
                {
                    UInt16[] wordData = new UInt16[1];
                    byte[] byteData = new byte[2];
                    receiveDataThread.timeStamp = DateTime.Now;
                    receiveDataThread.request = true;
                        //Lese Transaction identifier
                        byteData[1] = bytes[0];
                        byteData[0] = bytes[1];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.transactionIdentifier = wordData[0];

                        //Lese Protocol identifier
                        byteData[1] = bytes[2];
                        byteData[0] = bytes[3];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.protocolIdentifier = wordData[0];

                        //Lese length
                        byteData[1] = bytes[4];
                        byteData[0] = bytes[5];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.length = wordData[0];
                    

                    //Lese unit identifier
                    receiveDataThread.unitIdentifier = bytes[6];
                    //Check UnitIdentifier
                    if ((receiveDataThread.unitIdentifier != this.unitIdentifier) & (receiveDataThread.unitIdentifier != 0))
                        return;

                    // Lese function code
                    receiveDataThread.functionCode = bytes[7];

                    // Lese starting address 
                    byteData[1] = bytes[8];
                    byteData[0] = bytes[9];
                    Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                    receiveDataThread.startingAdress = wordData[0];

                    if (receiveDataThread.functionCode <= 4)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantity = wordData[0];
                    }
                    if (receiveDataThread.functionCode == 5)
                    {
                        receiveDataThread.receiveCoilValues = new ushort[1];
                        // Lese Value
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveCoilValues, 0, 2);
                    }
                    if (receiveDataThread.functionCode == 6)
                    {
                        receiveDataThread.receiveRegisterValues = new ushort[1];
                        // Lese Value
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveRegisterValues, 0, 2);
                    }
                    if (receiveDataThread.functionCode == 15)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantity = wordData[0];

                        receiveDataThread.byteCount = bytes[12];

                        if ((receiveDataThread.byteCount % 2) != 0)
                            receiveDataThread.receiveCoilValues = new ushort[receiveDataThread.byteCount / 2 + 1];
                        else
                            receiveDataThread.receiveCoilValues = new ushort[receiveDataThread.byteCount / 2];
                        // Lese Value
                        Buffer.BlockCopy(bytes, 13, receiveDataThread.receiveCoilValues, 0, receiveDataThread.byteCount);
                    }
                    if (receiveDataThread.functionCode == 16)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantity = wordData[0];

                        receiveDataThread.byteCount = bytes[12];
                        receiveDataThread.receiveRegisterValues = new ushort[receiveDataThread.quantity];
                        for (int i = 0; i < receiveDataThread.quantity; i++)
                        {
                            // Lese Value
                            byteData[1] = bytes[13 + i * 2];
                            byteData[0] = bytes[14 + i * 2];
                            Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveRegisterValues, i * 2, 2);
                        }

                    }
                    if (receiveDataThread.functionCode == 23)
                    {
                        // Lese starting Address Read
                        byteData[1] = bytes[8];
                        byteData[0] = bytes[9];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.startingAddressRead = wordData[0];
                        // Lese quantity Read
                        byteData[1] = bytes[10];
                        byteData[0] = bytes[11];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantityRead = wordData[0];
                        // Lese starting Address Write
                        byteData[1] = bytes[12];
                        byteData[0] = bytes[13];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.startingAddressWrite = wordData[0];
                        // Lese quantity Write
                        byteData[1] = bytes[14];
                        byteData[0] = bytes[15];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantityWrite = wordData[0];

                        receiveDataThread.byteCount = bytes[16];
                        receiveDataThread.receiveRegisterValues = new ushort[receiveDataThread.quantityWrite];
                        for (int i = 0; i < receiveDataThread.quantityWrite; i++)
                        {
                            // Lese Value
                            byteData[1] = bytes[17 + i * 2];
                            byteData[0] = bytes[18 + i * 2];
                            Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveRegisterValues, i * 2, 2);
                        }
                    }
                }
                catch (Exception exc)
                { }
                this.CreateAnswer(receiveDataThread, sendDataThread, stream, portIn, ipAddressIn);
                //this.sendAnswer();
                this.CreateLogData(receiveDataThread, sendDataThread);

                if (logDataChanged != null)
                    logDataChanged();
            }
        }
        #endregion
         
        #region Method CreateAnswer
        private void CreateAnswer(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {

            switch (receiveData.functionCode)
            {
                // Read Coils
                case 1:
                    if (!FunctionCode1Disabled)
                        this.ReadCoils(receiveData, sendData, stream, portIn, ipAddressIn);
                    else
                    {
                        sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    break;
                // Read Input Registers
                case 2:
                    if (!FunctionCode2Disabled)
                        this.ReadDiscreteInputs(receiveData, sendData, stream, portIn, ipAddressIn);
                    else
                    {
                        sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    
                    break;
                // Read Holding Registers
                case 3:
                    if (!FunctionCode3Disabled)
                        this.ReadHoldingRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                    else
                    {
                        sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    
                    break;
                // Read Input Registers
                case 4:
                    if (!FunctionCode4Disabled)
                        this.ReadInputRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                    else
                    {
                        sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    
                    break;
                // Write single coil
                case 5:
                    if (!FunctionCode5Disabled)
                        this.WriteSingleCoil(receiveData, sendData, stream, portIn, ipAddressIn);
                    else
                    {
                        sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    
                    break;
                // Write single register
                case 6:
                    if (!FunctionCode6Disabled)
                        this.WriteSingleRegister(receiveData, sendData, stream, portIn, ipAddressIn);
                    else
                    {
                        sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    }
                    
                        break;
                // Write Multiple coils
                case 15:
                        if (!FunctionCode15Disabled)
                            this.WriteMultipleCoils(receiveData, sendData, stream, portIn, ipAddressIn);
                        else
                        {
                            sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                            sendData.exceptionCode = 1;
                            sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        }

                        break;
                // Write Multiple registers
                case 16:
                        if (!FunctionCode16Disabled)
                            this.WriteMultipleRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                        else
                        {
                            sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                            sendData.exceptionCode = 1;
                            sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        }

                        break;
                // Error: Function Code not supported
                case 23:
                        if (!FunctionCode23Disabled)
                            this.ReadWriteMultipleRegisters(receiveData, sendData, stream, portIn, ipAddressIn);
                        else
                        {
                            sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                            sendData.exceptionCode = 1;
                            sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        }

                        break;
                // Error: Function Code not supported
                default: sendData.errorCode = (byte) (receiveData.functionCode + 0x80);
                        sendData.exceptionCode = 1;
                        sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                        break;
            }
            sendData.timeStamp = DateTime.Now;
        }
        #endregion
         
        private void ReadCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x07D0))  //Invalid quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1 + receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))     //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                if ((receiveData.quantity % 8) == 0)
                    sendData.byteCount = (byte)(receiveData.quantity / 8);
                else
                    sendData.byteCount = (byte)(receiveData.quantity / 8 + 1);

                sendData.sendCoilValues = new bool[receiveData.quantity];
                lock (lockCoils)
                    Array.Copy(coils.localArray, receiveData.startingAdress + 1, sendData.sendCoilValues, 0, receiveData.quantity);
            }
            if (true)
            {
                Byte[] data;

                if (sendData.exceptionCode > 0)
                	data = new byte[9];
                else
                   	data = new byte[9];
              
                Byte[] byteData = new byte[2];

                sendData.length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];
                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;

                //ByteCount
                data[8] = sendData.byteCount;

                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendCoilValues = null;
                }

                if (sendData.sendCoilValues != null)
                    for (int i = 0; i < (sendData.byteCount); i++)
                    {
                        byteData = new byte[2];
                        for (int j = 0; j < 8; j++)
                        {

                            byte boolValue;
                            if (sendData.sendCoilValues[i * 8 + j] == true)
                                boolValue = 1;
                            else
                                boolValue = 0;
                            byteData[1] = (byte)((byteData[1]) | (boolValue << j));
                            if ((i * 8 + j + 1) >= sendData.sendCoilValues.Length)
                                break;
                        }
                        data[9 + i] = byteData[1];
                    }
                try
                {

                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
            }  
        }

        private void ReadDiscreteInputs(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x07D0))  //Invalid quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1 + receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))   //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                if ((receiveData.quantity % 8) == 0)
                    sendData.byteCount = (byte)(receiveData.quantity / 8);
                else
                    sendData.byteCount = (byte)(receiveData.quantity / 8 + 1);

                sendData.sendCoilValues = new bool[receiveData.quantity];
                Array.Copy(discreteInputs.localArray, receiveData.startingAdress + 1, sendData.sendCoilValues, 0, receiveData.quantity);
            }
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.byteCount];
                Byte[] byteData = new byte[2];
                sendData.length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;

                //ByteCount
                data[8] = sendData.byteCount;


                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendCoilValues = null;
                }

                if (sendData.sendCoilValues != null)
                    for (int i = 0; i < (sendData.byteCount); i++)
                    {
                        byteData = new byte[2];
                        for (int j = 0; j < 8; j++)
                        {

                            byte boolValue;
                            if (sendData.sendCoilValues[i * 8 + j] == true)
                                boolValue = 1;
                            else
                                boolValue = 0;
                            byteData[1] = (byte)((byteData[1]) | (boolValue << j));
                            if ((i * 8 + j + 1) >= sendData.sendCoilValues.Length)
                                break;
                        }
                        data[9 + i] = byteData[1];
                    }

                try
                {
                  
                        stream.Write(data, 0, data.Length);
                        if(debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
            }
        }

        private void ReadHoldingRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x007D))  //Invalid quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1 + receiveData.quantity) > 65535)  | (receiveData.startingAdress < 0))   //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                sendData.byteCount = (byte)(2 * receiveData.quantity);
                sendData.sendRegisterValues = new Int16[receiveData.quantity];
                lock (lockHoldingRegisters)
                    Buffer.BlockCopy(holdingRegisters.localArray, receiveData.startingAdress * 2 + 2, sendData.sendRegisterValues, 0, receiveData.quantity * 2);
            }
                if (sendData.exceptionCode > 0)
                    sendData.length = 0x03;
                else
                    sendData.length = (ushort)(0x03 + sendData.byteCount);
            
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.byteCount];
                Byte[] byteData = new byte[2];
                sendData.length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;

                //ByteCount
                data[8] = sendData.byteCount;

                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }
   

                if (sendData.sendRegisterValues != null)
                    for (int i = 0; i < (sendData.byteCount / 2); i++)
                    {
                        byteData = BitConverter.GetBytes((Int16)sendData.sendRegisterValues[i]);
                        data[9 + i * 2] = byteData[1];
                        data[10 + i * 2] = byteData[0];
                    }
                try
                {
                    
                    
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
            }       
        }

        private void ReadInputRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            if ((receiveData.quantity < 1) | (receiveData.quantity > 0x007D))  //Invalid quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1 + receiveData.quantity) > 65535)  | (receiveData.startingAdress < 0))   //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                sendData.byteCount = (byte)(2 * receiveData.quantity);
                sendData.sendRegisterValues = new Int16[receiveData.quantity];
                Buffer.BlockCopy(inputRegisters.localArray, receiveData.startingAdress * 2 + 2, sendData.sendRegisterValues, 0, receiveData.quantity * 2);
            }
                if (sendData.exceptionCode > 0)
                    sendData.length = 0x03;
                else
                    sendData.length = (ushort)(0x03 + sendData.byteCount);
            
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.byteCount];
                Byte[] byteData = new byte[2];
                sendData.length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;

                //ByteCount
                data[8] = sendData.byteCount;

                
                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }


                if (sendData.sendRegisterValues != null)
                    for (int i = 0; i < (sendData.byteCount / 2); i++)
                    {
                        byteData = BitConverter.GetBytes((Int16)sendData.sendRegisterValues[i]);
                        data[9 + i * 2] = byteData[1];
                        data[10 + i * 2] = byteData[0];
                    }
                try
                {
                    
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
            }
        }

        private void WriteSingleCoil(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.receiveCoilValues = receiveData.receiveCoilValues;
            if ((receiveData.receiveCoilValues[0] != 0x0000) & (receiveData.receiveCoilValues[0] != 0xFF00))  //Invalid Value
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1) > 65535)  | (receiveData.startingAdress < 0))    //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                if (receiveData.receiveCoilValues[0] == 0xFF00)
                {
                    lock (lockCoils)
                        coils[receiveData.startingAdress + 1] = true;
                }
                if (receiveData.receiveCoilValues[0] == 0x0000)
                {
                    lock (lockCoils)
                        coils[receiveData.startingAdress + 1] = false;
                }
            }
                if (sendData.exceptionCode > 0)
                    sendData.length = 0x03;
                else
                    sendData.length = 0x06;
            
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                Byte[] byteData = new byte[2];
                sendData.length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;



                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.startingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.receiveCoilValues[0]);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }


                try
                {
                  
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
                if (coilsChanged != null)
                    coilsChanged(receiveData.startingAdress+1, 1);
            }
        }

        private void WriteSingleRegister(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.receiveRegisterValues = receiveData.receiveRegisterValues;
           
            if ((receiveData.receiveRegisterValues[0] < 0x0000) | (receiveData.receiveRegisterValues[0] > 0xFFFF))  //Invalid Value
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if (((receiveData.startingAdress + 1) > 65535)  | (receiveData.startingAdress < 0))    //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                lock (lockHoldingRegisters)
                    holdingRegisters[receiveData.startingAdress + 1] = unchecked((short)receiveData.receiveRegisterValues[0]);
            }
                if (sendData.exceptionCode > 0)
                    sendData.length = 0x03;
                else
                    sendData.length = 0x06;
            
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                Byte[] byteData = new byte[2];
                sendData.length = (byte)(data.Length - 6);


                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;



                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.startingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.receiveRegisterValues[0]);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }


                try
                {
                   
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
                if (holdingRegistersChanged != null)
                    holdingRegistersChanged(receiveData.startingAdress+1, 1);
            }
        }

        private void WriteMultipleCoils(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.quantity = receiveData.quantity;
            
            if ((receiveData.quantity == 0x0000) | (receiveData.quantity > 0x07B0))  //Invalid Quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((int)receiveData.startingAdress + 1 + (int)receiveData.quantity) > 65535)  | (receiveData.startingAdress < 0))    //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                lock (lockCoils)
                    for (int i = 0; i < receiveData.quantity; i++)
                    {
                        int shift = i % 16;
                    /*                if ((i == receiveData.quantity - 1) & (receiveData.quantity % 2 != 0))
                                    {
                                        if (shift < 8)
                                            shift = shift + 8;
                                        else
                                            shift = shift - 8;
                                    }*/
                        int mask = 0x1;
                        mask = mask << (shift);
                        if ((receiveData.receiveCoilValues[i / 16] & (ushort)mask) == 0)
                        
                            coils[receiveData.startingAdress + i + 1] = false;
                        else
                        
                            coils[receiveData.startingAdress + i + 1] = true;

                    }
            }
            if (sendData.exceptionCode > 0)
                sendData.length = 0x03;
            else
                sendData.length = 0x06;
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                Byte[] byteData = new byte[2];
                sendData.length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;



                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.startingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.quantity);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }


                try
                {
                
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
                if (coilsChanged != null)
                    coilsChanged(receiveData.startingAdress+1, receiveData.quantity);
            }
        }

        private void WriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;
            sendData.startingAdress = receiveData.startingAdress;
            sendData.quantity = receiveData.quantity;

            if ((receiveData.quantity == 0x0000) | (receiveData.quantity > 0x07B0))  //Invalid Quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((int)receiveData.startingAdress + 1 + (int)receiveData.quantity) > 65535)  | (receiveData.startingAdress < 0))   //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                lock (lockHoldingRegisters)
                    for (int i = 0; i < receiveData.quantity; i++)
                    {
                        holdingRegisters[receiveData.startingAdress + i + 1] = unchecked((short)receiveData.receiveRegisterValues[i]);
                    }
            }
            if (sendData.exceptionCode > 0)
                sendData.length = 0x03;
            else
                sendData.length = 0x06;
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[12];

                Byte[] byteData = new byte[2];
                sendData.length = (byte)(data.Length - 6);

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;



                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }
                else
                {
                    byteData = BitConverter.GetBytes((int)receiveData.startingAdress);
                    data[8] = byteData[1];
                    data[9] = byteData[0];
                    byteData = BitConverter.GetBytes((int)receiveData.quantity);
                    data[10] = byteData[1];
                    data[11] = byteData[0];
                }


                try
                {
                   
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                    }
                catch (Exception) { }
                if (holdingRegistersChanged != null)
                    holdingRegistersChanged(receiveData.startingAdress+1, receiveData.quantity);
            }
        }

        private void ReadWriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = this.unitIdentifier;
            sendData.functionCode = receiveData.functionCode;


            if ((receiveData.quantityRead < 0x0001) | (receiveData.quantityRead > 0x007D) | (receiveData.quantityWrite < 0x0001) | (receiveData.quantityWrite > 0x0079) | (receiveData.byteCount != (receiveData.quantityWrite * 2)))  //Invalid Quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 3;
            }
            if ((((int)receiveData.startingAddressRead + 1 + (int)receiveData.quantityRead) > 65535) | (((int)receiveData.startingAddressWrite + 1 + (int)receiveData.quantityWrite) > 65535) | (receiveData.quantityWrite < 0) | (receiveData.quantityRead < 0))    //Invalid Starting adress or Starting address + quantity
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 2;
            }
            if (sendData.exceptionCode == 0)
            {
                sendData.sendRegisterValues = new Int16[receiveData.quantityRead];
                lock (lockHoldingRegisters)
                    Buffer.BlockCopy(holdingRegisters.localArray, receiveData.startingAddressRead * 2 + 2, sendData.sendRegisterValues, 0, receiveData.quantityRead * 2);

                lock (holdingRegisters)
                    for (int i = 0; i < receiveData.quantityWrite; i++)
                    {
                        holdingRegisters[receiveData.startingAddressWrite + i + 1] = unchecked((short)receiveData.receiveRegisterValues[i]);
                    }
                sendData.byteCount = (byte)(2 * receiveData.quantityRead);
            }
            if (sendData.exceptionCode > 0)
                sendData.length = 0x03;
            else
                sendData.length = Convert.ToUInt16(3 + 2 * receiveData.quantityRead);
            if (true)
            {
                Byte[] data;
                if (sendData.exceptionCode > 0)
                    data = new byte[9];
                else
                    data = new byte[9 + sendData.byteCount];

                Byte[] byteData = new byte[2];

                //Send Transaction identifier
                byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                data[0] = byteData[1];
                data[1] = byteData[0];

                //Send Protocol identifier
                byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                data[2] = byteData[1];
                data[3] = byteData[0];

                //Send length
                byteData = BitConverter.GetBytes((int)sendData.length);
                data[4] = byteData[1];
                data[5] = byteData[0];

                //Unit Identifier
                data[6] = sendData.unitIdentifier;

                //Function Code
                data[7] = sendData.functionCode;

                //ByteCount
                data[8] = sendData.byteCount;


                if (sendData.exceptionCode > 0)
                {
                    data[7] = sendData.errorCode;
                    data[8] = sendData.exceptionCode;
                    sendData.sendRegisterValues = null;
                }
                else
                {
                    if (sendData.sendRegisterValues != null)
                        for (int i = 0; i < (sendData.byteCount / 2); i++)
                        {
                            byteData = BitConverter.GetBytes((Int16)sendData.sendRegisterValues[i]);
                            data[9 + i * 2] = byteData[1];
                            data[10 + i * 2] = byteData[0];
                        }

                }


                try
                {
                   
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                }
                catch (Exception) { }
                if (holdingRegistersChanged != null)
                    holdingRegistersChanged(receiveData.startingAddressWrite+1, receiveData.quantityWrite);
            }
        }

        private void sendException(int errorCode, int exceptionCode, ModbusProtocol receiveData, ModbusProtocol sendData, NetworkStream stream, int portIn, IPAddress ipAddressIn)
        {
            sendData.response = true;

            sendData.transactionIdentifier = receiveData.transactionIdentifier;
            sendData.protocolIdentifier = receiveData.protocolIdentifier;

            sendData.unitIdentifier = receiveData.unitIdentifier;
            sendData.errorCode = (byte)errorCode;
            sendData.exceptionCode = (byte)exceptionCode;

             if (sendData.exceptionCode > 0)
                sendData.length = 0x03;
            else
                sendData.length = (ushort)(0x03 + sendData.byteCount);

             if (true)
             {
                 Byte[] data;
                 if (sendData.exceptionCode > 0)
                     data = new byte[9];
                 else
                     data = new byte[9 + sendData.byteCount];
                 Byte[] byteData = new byte[2];
                 sendData.length = (byte)(data.Length - 6);

                 //Send Transaction identifier
                 byteData = BitConverter.GetBytes((int)sendData.transactionIdentifier);
                 data[0] = byteData[1];
                 data[1] = byteData[0];

                 //Send Protocol identifier
                 byteData = BitConverter.GetBytes((int)sendData.protocolIdentifier);
                 data[2] = byteData[1];
                 data[3] = byteData[0];

                 //Send length
                 byteData = BitConverter.GetBytes((int)sendData.length);
                 data[4] = byteData[1];
                 data[5] = byteData[0];

                 //Unit Identifier
                 data[6] = sendData.unitIdentifier;


                 data[7] = sendData.errorCode;
                 data[8] = sendData.exceptionCode;


                 try
                 {
                   
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    
                 }
                 catch (Exception) { }
             }
        }

        private void CreateLogData(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            for (int i = 0; i < 98; i++)
            {
                modbusLogData[99 - i] = modbusLogData[99 - i - 2];

            }
            modbusLogData[0] = receiveData;
            modbusLogData[1] = sendData;

        }



        public int NumberOfConnections
        {
            get
            {
                return numberOfConnections;
            }
        }

        public ModbusProtocol[] ModbusLogData
        {
            get
            {
                return modbusLogData;
            }
        }

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

    public class HoldingRegisters
    {
        public Int16[] localArray = new Int16[65535];
        ModbusServer modbusServer;
     
        public HoldingRegisters(EasyModbus.ModbusServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }

        public Int16 this[int x]
        {
            get { return this.localArray[x]; }
            set
            {              
                this.localArray[x] = value;
            }
        }

    }

    public class InputRegisters
    {
        public Int16[] localArray = new Int16[65535];
        ModbusServer modbusServer;

        public InputRegisters(EasyModbus.ModbusServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }

        public Int16 this[int x]
        {
            get { return this.localArray[x]; }
            set
            {
                this.localArray[x] = value;

            }
        }

    }

    public class Coils
    {
        public bool[] localArray = new bool[65535];
        ModbusServer modbusServer;

        public Coils(EasyModbus.ModbusServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }

        public bool this[int x]
        {
            get { return this.localArray[x]; }
            set
            {
                this.localArray[x] = value;

            }
        }

    }

    public class DiscreteInputs
    {
        public bool[] localArray = new bool[65535];
        ModbusServer modbusServer;

        public DiscreteInputs(EasyModbus.ModbusServer modbusServer)
        {
            this.modbusServer = modbusServer;
        }

        public bool this[int x]
        {
            get { return this.localArray[x]; }
            set
            {
                this.localArray[x] = value;
            }
        }

    }
}
   