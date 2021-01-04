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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO.Ports;





namespace EasyModbus
{

    public partial class ModbusClient
    { 


        #region Class Variables
        private bool debug = false;
        private TcpClient tcpClient;
        private bool udpFlag = false;
        private string ipAddress = "127.0.0.1";
        private int port = 502;
        private int baudRate = 9600;
        private int connectTimeout = 1000;
        private bool connected;

        private byte[] protocolIdentifier = new byte[2];
        private byte[] crc = new byte[2];
        private byte[] length = new byte[2];
        private byte unitIdentifier = 0x01;
        private byte functionCode;
        private byte[] startingAddress = new byte[2];
        private byte[] quantity = new byte[2];

        private bool dataReceived = false;
        private int bytesToRead = 0;
        public byte[] sendData;
        public byte[] receiveData;
        private byte[] readBuffer = new byte[256];
        private int portOut;

        public int NumberOfRetries { get; set; } = 3;
        private int countRetries = 0;

        private uint transactionIdentifierInternal = 0;
        private byte[] transactionIdentifier = new byte[2];

        private SerialPort serialport;
        private bool receiveActive = false;

        public delegate void ConnectedChangedHandler(object sender);
        public event ConnectedChangedHandler ConnectedChanged;

        public delegate void ReceiveDataChangedHandler(object sender);
        public event ReceiveDataChangedHandler ReceiveDataChanged;

        public delegate void SendDataChangedHandler(object sender);
        public event SendDataChangedHandler SendDataChanged;

        NetworkStream stream;
        #endregion


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
        /// Close connection to Master Device.
        /// </summary>
        public void Disconnect()
        {
            if (debug) StoreLogData.Instance.Store("Disconnect", System.DateTime.Now);
            if (serialport != null)
            {
                if (serialport.IsOpen & !receiveActive)
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
        /// Read Discrete Inputs from Server device (FC2).
        /// </summary>
        /// <param name="startingAddress">First discrete input to read</param>
        /// <param name="quantity">Number of discrete Inputs to read</param>
        /// <returns>Boolean Array which contains the discrete Inputs</returns>
        public bool[] ReadDiscreteInputs(int startingAddress, int quantity)
        {
            if (debug) StoreLogData.Instance.Store("FC2 (Read Discrete Inputs from Master device), StartingAddress: " + startingAddress + ", Quantity: " + quantity, System.DateTime.Now);
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
            if (startingAddress > 65535 | quantity > 2000)
            {
                if (debug) StoreLogData.Instance.Store("ArgumentException Throwed", System.DateTime.Now);
                throw new ArgumentException("Starting address must be 0 - 65535; quantity must be 0 - 2000");
            }

            // Create Request
            ApplicationDataUnit request = new ApplicationDataUnit(2);
            request.QuantityRead = (ushort)quantity;
            request.StartingAddressRead = (ushort)startingAddress;
            request.TransactionIdentifier = (ushort)transactionIdentifierInternal;


            ApplicationDataUnit response = new ApplicationDataUnit(2);
            response.QuantityRead = (ushort)quantity;

            byte[] data = new byte[255];
            if (serialport != null)
            {
                dataReceived = false;
                if (quantity % 8 == 0)
                    bytesToRead = 5 + quantity / 8;
                else
                    bytesToRead = 6 + quantity / 8;

                serialport.Write(request.Payload, 6, 8);
                if (debug)
                {
                    byte[] debugData = new byte[8];
                    Array.Copy(request.Payload, 6, debugData, 0, 8);
                    if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                }
                if (SendDataChanged != null)
                {
                    sendData = new byte[8];
                    Array.Copy(request.Payload, 6, sendData, 0, 8);
                    SendDataChanged(this);

                }

                

                readBuffer = new byte[256];
                DateTime dateTimeSend = DateTime.UtcNow;

                response.UnitIdentifier = 0xFF;

                while (response.UnitIdentifier != unitIdentifier & !((DateTime.UtcNow.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * connectTimeout))
                {
                    while (dataReceived == false & !((DateTime.UtcNow.Ticks - dateTimeSend.Ticks) > TimeSpan.TicksPerMillisecond * connectTimeout))
                        System.Threading.Thread.Sleep(1);
                    data = new byte[255];
                    Array.Copy(readBuffer, 0, data, 6, readBuffer.Length);
                 
                }
                if (response.UnitIdentifier != unitIdentifier)
                    data = new byte[255];
                else
                    countRetries = 0;
            }
            else if (tcpClient.Client.Connected | udpFlag)
            {
                if (udpFlag)
                {
                    UdpClient udpClient = new UdpClient();
                    System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    udpClient.Send(data, data.Length - 2, endPoint);
                    portOut = ((System.Net.IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    udpClient.Client.ReceiveTimeout = 5000;
                    endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipAddress), portOut);
                    data = udpClient.Receive(ref endPoint);
                }
                else
                {
                    stream.Write(request.Payload, 0, request.Payload.Length - 2);
                    if (debug)
                    {
                        byte[] debugData = new byte[data.Length - 2];
                        Array.Copy(data, 0, debugData, 0, data.Length - 2);
                        if (debug) StoreLogData.Instance.Store("Send ModbusTCP-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                    }
                    if (SendDataChanged != null)
                    {
                        sendData = new byte[data.Length - 2];
                        Array.Copy(data, 0, sendData, 0, data.Length - 2);
                        SendDataChanged(this);
                    }
                    data = new Byte[255];
                    int NumberOfBytes = stream.Read(response.Payload, 0, response.Payload.Length);
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
                crc = BitConverter.GetBytes(calculateCRC(data, (ushort)(data[8] + 3), 6));
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

            return response.RegisterDataBool;
        }

 


    }
}
