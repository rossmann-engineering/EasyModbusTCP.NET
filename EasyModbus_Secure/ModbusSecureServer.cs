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
using System.IO.Ports;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Linq;
using System.Text.RegularExpressions;
using Org.BouncyCastle;

namespace EasyModbusSecure
{
    #region class ModbusSecureProtocol
    /// <summary>
    /// Modbus Protocol informations.
    /// </summary>
    public class ModbusSecureProtocol
    {
        public enum ProtocolType { ModbusTCP = 0, ModbusUDP = 1, ModbusRTU = 2 };
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
        public SslStream stream;            //For TCP-Connection only
        public Byte[] bytes;
        public int portIn;                  //For UDP-Connection only
        public IPAddress ipAddressIn;       //For UDP-Connection only
    }
    #endregion

    #region TCPHandler class
    internal class TCPHandler
    {
        private static bool debug = false;
        public delegate void DataChanged(object networkConnectionParameter);
        public event DataChanged dataChanged;

        public delegate void NumberOfClientsChanged();
        public event NumberOfClientsChanged numberOfClientsChanged;

        TcpListener server = null;


        private List<Client> tcpClientLastRequestList = new List<Client>();

        public int NumberOfConnectedClients { get; set; }

        public string ipAddress = null;

        private X509Certificate2 serverCertificate;

        private bool mutualAuthentication { get; set; }

        private List<ValueTuple<string, List<byte>>> acceptableRoles { get; set; }

        public Role role { get; set; }

        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, Can write: {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
                if (debug) StoreLogData.Instance.Store($"Local certificate is null.", System.DateTime.Now);
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;



            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
                if (debug) StoreLogData.Instance.Store($"Remote certificate is null.", System.DateTime.Now);
            }
        }

        /// When making a server TCP listen socket, will listen to this IP address.
        public IPAddress LocalIPAddress
        {
            get { return localIPAddress; }
        }
        private IPAddress localIPAddress = IPAddress.Loopback; //Change that back to .Any

        /// <summary>
        /// Listen to all network interfaces.
        /// </summary>
        /// <param name="port">TCP port to listen</param>
        public TCPHandler(int port, string certificate, string certificatePassword, List<ValueTuple<string, List<byte>>> acceptableRoles, bool debug, bool mutualAuthentication = true)
        {
            server = new TcpListener(LocalIPAddress, port);
            serverCertificate = new X509Certificate2(certificate, certificatePassword, X509KeyStorageFlags.MachineKeySet);

            // By default everyone is denied access
            this.role = new Role("0");

            this.mutualAuthentication = mutualAuthentication;            

            this.acceptableRoles = new List<ValueTuple<string, List<byte>>>();
            this.acceptableRoles.AddRange(acceptableRoles);

            TCPHandler.debug = debug;

            server.Start();
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);

        }

        /// <summary>
        /// Listen to a specific network interface.
        /// </summary>
        /// <param name="localIPAddress">IP address of network interface to listen</param>
        /// <param name="port">TCP port to listen</param>
        public TCPHandler(IPAddress localIPAddress, int port, string certificate, string certificatePassword, List<ValueTuple<string, List<byte>>> acceptableRoles, bool debug, bool mutualAuthentication = true)
        {
            this.localIPAddress = localIPAddress;
            server = new TcpListener(LocalIPAddress, port);
            serverCertificate = new X509Certificate2(certificate, certificatePassword, X509KeyStorageFlags.MachineKeySet);

            // By default everyone is denied access
            this.role = new Role("0");

            this.mutualAuthentication = mutualAuthentication;
         
            this.acceptableRoles = new List<ValueTuple<string, List<byte>>>();
            this.acceptableRoles.AddRange(acceptableRoles);

            TCPHandler.debug = debug;

            server.Start();
            server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        public static bool ValidateClientCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            //    return true;

            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Just for testing for the localhost domain that does not match 127.0.0.1 with "localhost"
            // TODO: Fix that
            //if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            //    return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            if (debug) StoreLogData.Instance.Store($"Certificate error: { sslPolicyErrors}", System.DateTime.Now);

            // Do not allow this sever to communicate with unauthenticated clients.
            return false;
        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient = server.EndAcceptTcpClient(asyncResult);
                tcpClient.ReceiveTimeout = 4000;
                if (ipAddress != null)
                {
                    string ipEndpoint = tcpClient.Client.RemoteEndPoint.ToString();
                    ipEndpoint = ipEndpoint.Split(':')[0];
                    if (ipEndpoint != ipAddress)
                    {
                        tcpClient.Client.Disconnect(false);
                        return;
                    }
                }
            }
            catch (Exception) { }

            try
            {
                server.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
                Client client = new Client(tcpClient, mutualAuthentication);
                SslStream sslStream = client.SslStream;

                //With Mutual Authentication
                if (mutualAuthentication)
                {

                    try
                    {
                        sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Tls12, true);

                        if (sslStream.RemoteCertificate == null)
                        {
                            Console.WriteLine("Client did not send back a certificate");
                            if (debug) StoreLogData.Instance.Store($"Client did not send back a certificate", System.DateTime.Now);
                            //sslStream.Close();
                            //tcpClient.Close();

                            throw new AuthenticationException();
                        }

                        // Display the properties and settings for the authenticated stream.
                        DisplaySecurityLevel(sslStream);
                        DisplaySecurityServices(sslStream);
                        DisplayCertificateInformation(sslStream, client);
                        DisplayStreamProperties(sslStream);

                        CheckRoleInformation(sslStream, client);

                        this.role = new Role(client.getRole());

                    }
                    catch (AuthenticationException e)
                    {

                        Console.WriteLine("Exception: {0}", e.Message);
                        if (e.InnerException != null)
                        {
                            Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                        }
                        Console.WriteLine("Authentication failed - closing the connection.");
                        if (debug) StoreLogData.Instance.Store($"Authentication failed - closing the connection.", System.DateTime.Now);
                        sslStream.Close();
                        tcpClient.Close();
                        return;

                    }
                }
                else
                {
                    try
                    {
                        sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls12, true);

                        Console.WriteLine("NO CLIENT AUTH!!!");
                        if (debug) StoreLogData.Instance.Store($"Client authetication has not been enabled.", System.DateTime.Now);

                        // Display the properties and settings for the authenticated stream.
                        DisplaySecurityLevel(sslStream);
                        DisplaySecurityServices(sslStream);
                        DisplayCertificateInformation(sslStream, client);
                        DisplayStreamProperties(sslStream);

                        CheckRoleInformation(sslStream, client);

                        this.role = new Role(client.getRole());

                    }
                    catch (AuthenticationException e)
                    {

                        Console.WriteLine("Exception: {0}", e.Message);
                        if (e.InnerException != null)
                        {
                            Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                        }

                        Console.WriteLine("Authentication failed - closing the connection.");
                        if (debug) StoreLogData.Instance.Store($"Authentication failed - closing the connection.", System.DateTime.Now);

                        sslStream.Close();
                        tcpClient.Close();
                        return;

                    }
                }

                sslStream.ReadTimeout = 4000;
                sslStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
            }
            catch (Exception e)
            {
                // TODO: Populate that with an exception handling
                Console.WriteLine("Exception 2: {0}", e.Message);
                if (debug) StoreLogData.Instance.Store($"Exception occurred: {e.Message}", System.DateTime.Now);
            }
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

        private void ReadCallback(IAsyncResult asyncResult)
        {
            NetworkConnectionParameter networkConnectionParameter = new NetworkConnectionParameter();
            Client client = asyncResult.AsyncState as Client;
            client.Ticks = DateTime.Now.Ticks;
            NumberOfConnectedClients = GetAndCleanNumberOfConnectedClients(client);
            if (numberOfClientsChanged != null)
                numberOfClientsChanged();
            if (client != null)
            {
                int read;
                SslStream sslStream = null;
                try
                {
                    sslStream = client.SslStream;

                    read = sslStream.EndRead(asyncResult);
                }
                catch (Exception ex)
                {
                    return;
                }


                if (read == 0)
                {
                    //OnClientDisconnected(client.TcpClient);
                    //connectedClients.Remove(client);
                    return;
                }
                byte[] data = new byte[read];
                Buffer.BlockCopy(client.Buffer, 0, data, 0, read);
                networkConnectionParameter.bytes = data;
                networkConnectionParameter.stream = sslStream;
                if (dataChanged != null)
                    dataChanged(networkConnectionParameter);
                try
                {
                    sslStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);
                }
                catch (Exception)
                {
                }
            }
        }

        public void Disconnect()
        {
            try
            {
                foreach (Client clientLoop in tcpClientLastRequestList)
                {
                    clientLoop.SslStream.Close();
                }
            }
            catch (Exception) { }
            server.Stop();

        }

        public void DisplayCertificateInformation(SslStream stream, Client client)
        {
            // TODO: Maybe add logging here?
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificateX509 = stream.LocalCertificate;
            X509Certificate2 localCertificateX5092 = new X509Certificate2(localCertificateX509);
            if (stream.LocalCertificate != null)
            {               
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificateX5092.Subject,
                    localCertificateX5092.GetEffectiveDateString(),
                    localCertificateX5092.GetExpirationDateString());


                if (debug) StoreLogData.Instance.Store($"Local cert was issued to {localCertificateX5092.Subject}" +
                    $" and is valid from {localCertificateX5092.GetEffectiveDateString()} " +
                    $"until {localCertificateX5092.GetExpirationDateString()}", System.DateTime.Now);
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }

            // Display the properties of the client's certificate.
            X509Certificate remoteCertificateX509 = stream.RemoteCertificate;

            if (remoteCertificateX509 != null)
            {
                X509Certificate2 remoteCertificateX5092 = new X509Certificate2(remoteCertificateX509);

                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificateX5092.Subject,
                    remoteCertificateX5092.GetEffectiveDateString(),
                    remoteCertificateX5092.GetExpirationDateString());

                if (debug) StoreLogData.Instance.Store($"Remote cert was issued to {remoteCertificateX5092.Subject}" +
                   $" and is valid from {remoteCertificateX5092.GetEffectiveDateString()} " +
                   $"until {remoteCertificateX5092.GetExpirationDateString()}", System.DateTime.Now);
            
            }
            else
            {
                Console.WriteLine("Remote (client) certificate is null.");
                if (debug) StoreLogData.Instance.Store($"Remote (client) certificate is null.", System.DateTime.Now);
            }
        }

        public void CheckRoleInformation(SslStream stream, Client client)
        {           
            X509Certificate remoteCertificateX509 = stream.RemoteCertificate;

            int roleCount = 0;
            if (remoteCertificateX509 != null)
            {
                X509Certificate2 remoteCertificateX5092 = new X509Certificate2(remoteCertificateX509);

                var bcCert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(remoteCertificateX5092);

                var oids = bcCert.GetNonCriticalExtensionOids();

                foreach (var oid in oids)
                {

                    if (roleCount > 1)
                    {
                        Console.WriteLine("No valid role is provided - closing the connection.");
                        if (debug) StoreLogData.Instance.Store($"No valid role is provided - closing the connection.", System.DateTime.Now);
                        throw new AuthenticationException();
                    }

                    if (oid.Equals("1.3.6.1.4.1.50316.802.1"))
                    {
                        // We get the ASN.1 format from the data taht contains some special characters in the beggininig. 
                        // We then remove all visilbe and invisible control characters.

                        //string roleStr = Encoding.UTF8.GetString(asndata.RawData);
                        //roleStr = Regex.Replace(roleStr, @"\p{Cc}+", string.Empty);   

                        var oct = bcCert.GetExtensionValue(new Org.BouncyCastle.Asn1.DerObjectIdentifier(oid.ToString())).GetOctets();
                        //var extVal = bcCert.GetExtensionValue(oid.ToString());
                        //var oct = Org.BouncyCastle.Asn1.Asn1Object.FromByteArray(extVal.GetOctets());           
                        var aIn = new Org.BouncyCastle.Asn1.Asn1InputStream(oct);

                        var roleStr = aIn.ReadObject().ToString();


                        roleCount++;

                        if(acceptableRoles.Any(roleTuple => roleTuple.Item1.Equals(roleStr)))
                        {
                            client.setRole(roleStr);
                            Console.WriteLine("RoleOID:  {0}", client.getRole());
                        }
                        

                        if (client.getRole().Equals("0"))
                        {                            
                            Console.WriteLine("Attempted access with role {0}, has been identified", roleStr);
                            if (debug) StoreLogData.Instance.Store($"Attempted access with role {roleStr} has been identified", System.DateTime.Now);                            
                        }
                    }

                }

                if (roleCount == 0)
                {
                    Console.WriteLine("No role provided. Setting role to null");
                    if (debug) StoreLogData.Instance.Store($"No role provided. Setting role to null.", System.DateTime.Now);
                }
            }
            else
            {
                Console.WriteLine("Remote (client) certificate is null.");
                if (debug) StoreLogData.Instance.Store($"Remote (client) certificate is null.", System.DateTime.Now);
            }
        }


        internal class Client
        {
            private readonly TcpClient tcpClient;
            private readonly byte[] buffer;
            private SslStream stream;
            private Role role;
            private bool mutualAuthentication { get; set; }

            public long Ticks { get; set; }

            public Client(TcpClient tcpClient, bool mutualAuthentication)
            {
                // By default everyone is denied access
                this.role = new Role("0");
                this.tcpClient = tcpClient;
                int bufferSize = tcpClient.ReceiveBufferSize;
                buffer = new byte[bufferSize];

                this.mutualAuthentication = mutualAuthentication;
            }

            public string getRole()
            {
                return this.role.getRole();
            }
            public void setRole(string role)
            {
                this.role = new Role(role);
            }

            public TcpClient TcpClient
            {
                get { return tcpClient; }
            }

            public byte[] Buffer
            {
                get { return buffer; }
            }

            public SslStream SslStream
            {
                get
                {
                    if (stream == null)
                    {
                        if (mutualAuthentication)
                        {
                            stream = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateClientCertificate), null);
                            return stream;
                        }
                        else
                        {

                            stream = new SslStream(tcpClient.GetStream(), false, null, null);
                            return stream;
                        }
                    }
                    else
                    {
                        return stream;
                    }


                }

            }
        }
        public class Role
        {
            private string val;
            public Role(string val)
            {
                this.setRole(val);
            }
            public string getRole()
            {
                return this.val;
            }
            public void setRole(string role)
            {
                this.val = role;
            }

        };
    }
    #endregion



    /// <summary>
    /// Modbus TCP Server.
    /// </summary>
    public class ModbusSecureServer
    {
        private bool debug = false;
        Int32 port = 802;
        ModbusSecureProtocol receiveData;
        ModbusSecureProtocol sendData = new ModbusSecureProtocol();
        Byte[] bytes = new Byte[2100];
        //public Int16[] _holdingRegisters = new Int16[65535];
        public HoldingRegisters holdingRegisters;
        public InputRegisters inputRegisters;
        public Coils coils;
        public DiscreteInputs discreteInputs;
        private int numberOfConnections = 0;
        private bool udpFlag;
        private bool serialFlag;
        private int baudrate = 9600;
        private System.IO.Ports.Parity parity = Parity.Even;
        private System.IO.Ports.StopBits stopBits = StopBits.One;
        private string serialPort = "COM1";
        private SerialPort serialport;
        private byte unitIdentifier = 1;
        private int portIn;
        private IPAddress ipAddressIn;
        private UdpClient udpClient;
        private IPEndPoint iPEndPoint;
        private TCPHandler tcpHandler;
        Thread listenerThread;
        Thread clientConnectionThread;
        private ModbusSecureProtocol[] modbusLogData = new ModbusSecureProtocol[100];

        // TODO; Can all these be items of an array or a list?
        public bool FunctionCode1Disabled { get; set; }
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

        private IPAddress localIPAddress = IPAddress.Any;

        private string certificate { get; set; }
        private string certificatePassword { get; set; }

        private bool mutualAuthentication { get; set; }

        protected List<ValueTuple<string, List<byte>>> acceptableRoles { get; set; }
        protected string TcpHandlerRole
        {
            get
            {
                return tcpHandler?.role?.getRole();
            }
        }

        /// <summary>
        /// When creating a TCP or UDP socket, the local IP address to attach to.
        /// </summary>
        public IPAddress LocalIPAddress
        {
            get { return localIPAddress; }
            set { if (listenerThread == null) localIPAddress = value; }
        }

        public ModbusSecureServer(string certificate, string certificatePassword, bool mutualAuthentication, List<ValueTuple<string, List<byte>>> acceptableRoles)
        {
            holdingRegisters = new HoldingRegisters(this);
            inputRegisters = new InputRegisters(this);
            coils = new Coils(this);
            discreteInputs = new DiscreteInputs(this);

            this.certificate = certificate;

            this.certificatePassword = certificatePassword;

            this.mutualAuthentication = mutualAuthentication;

            this.acceptableRoles = acceptableRoles;

        }

        #region events
        public delegate void CoilsChangedHandler(int coil, int numberOfCoils);
        public event CoilsChangedHandler CoilsChanged;

        public delegate void HoldingRegistersChangedHandler(int register, int numberOfRegisters);
        public event HoldingRegistersChangedHandler HoldingRegistersChanged;

        public delegate void NumberOfConnectedClientsChangedHandler();
        public event NumberOfConnectedClientsChangedHandler NumberOfConnectedClientsChanged;

        public delegate void LogDataChangedHandler();
        public event LogDataChangedHandler LogDataChanged;
        #endregion

        public void Listen()
        {

            listenerThread = new Thread(ListenerThread);
            listenerThread.Start();
        }

        public void StopListening()
        {
            if (SerialFlag & (serialport != null))
            {
                if (serialport.IsOpen)
                    serialport.Close();
                shouldStop = true;
            }
            try
            {
                tcpHandler.Disconnect();
                listenerThread.Abort();

            }
            catch (Exception) { }
            listenerThread.Join();
            try
            {

                clientConnectionThread.Abort();
            }
            catch (Exception) { }
        }

        private void ListenerThread()
        {
            if (!udpFlag & !serialFlag)
            {
                if (udpClient != null)
                {
                    try
                    {
                        udpClient.Close();
                    }
                    catch (Exception) { }
                }
                tcpHandler = new TCPHandler(LocalIPAddress, port, certificate, certificatePassword, acceptableRoles ,debug, mutualAuthentication);
                if (debug) StoreLogData.Instance.Store($"EasyModbus Server listing for incomming data at Port {port}, local IP {LocalIPAddress}", System.DateTime.Now);
                tcpHandler.dataChanged += new TCPHandler.DataChanged(ProcessReceivedData);
                //tcpHandler.CheckRoleInformation(tcpHandler.)
                tcpHandler.numberOfClientsChanged += new TCPHandler.NumberOfClientsChanged(numberOfClientsChanged);
            }
            else if (serialFlag)
            {
                if (serialport == null)
                {
                    if (debug) StoreLogData.Instance.Store("EasyModbus RTU-Server listing for incomming data at Serial Port " + serialPort, System.DateTime.Now);
                    serialport = new SerialPort();
                    serialport.PortName = serialPort;
                    serialport.BaudRate = this.baudrate;
                    serialport.Parity = this.parity;
                    serialport.StopBits = stopBits;
                    serialport.WriteTimeout = 10000;
                    serialport.ReadTimeout = 1000;
                    serialport.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    serialport.Open();
                }
            }
            else
                while (!shouldStop)
                {
                    if (udpFlag)
                    {
                        if (udpClient == null | PortChanged)
                        {
                            IPEndPoint localEndoint = new IPEndPoint(LocalIPAddress, port);
                            udpClient = new UdpClient(localEndoint);
                            if (debug) StoreLogData.Instance.Store($"EasyModbus Server listing for incomming data at Port {port}, local IP {LocalIPAddress}", System.DateTime.Now);
                            udpClient.Client.ReceiveTimeout = 1000;
                            iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                            PortChanged = false;
                        }
                        if (tcpHandler != null)
                            tcpHandler.Disconnect();
                        try
                        {
                            bytes = udpClient.Receive(ref iPEndPoint);
                            portIn = iPEndPoint.Port;
                            NetworkConnectionParameter networkConnectionParameter = new NetworkConnectionParameter();
                            networkConnectionParameter.bytes = bytes;
                            ipAddressIn = iPEndPoint.Address;
                            networkConnectionParameter.portIn = portIn;
                            networkConnectionParameter.ipAddressIn = ipAddressIn;
                            ParameterizedThreadStart pts = new ParameterizedThreadStart(this.ProcessReceivedData);
                            Thread processDataThread = new Thread(pts);
                            processDataThread.Start(networkConnectionParameter);
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
        }

        #region SerialHandler
        private bool dataReceived = false;
        private byte[] readBuffer = new byte[2094];
        private DateTime lastReceive;
        private int nextSign = 0;
        private void DataReceivedHandler(object sender,
                        SerialDataReceivedEventArgs e)
        {
            int silence = 4000 / baudrate;
            if ((DateTime.Now.Ticks - lastReceive.Ticks) > TimeSpan.TicksPerMillisecond * silence)
                nextSign = 0;


            SerialPort sp = (SerialPort)sender;

            int numbytes = sp.BytesToRead;
            byte[] rxbytearray = new byte[numbytes];

            sp.Read(rxbytearray, 0, numbytes);

            Array.Copy(rxbytearray, 0, readBuffer, nextSign, rxbytearray.Length);
            lastReceive = DateTime.Now;
            nextSign = numbytes + nextSign;
            if (ModbusSecureClient.DetectValidModbusFrame(readBuffer, nextSign))
            {

                dataReceived = true;
                nextSign = 0;

                NetworkConnectionParameter networkConnectionParameter = new NetworkConnectionParameter();
                networkConnectionParameter.bytes = readBuffer;
                ParameterizedThreadStart pts = new ParameterizedThreadStart(this.ProcessReceivedData);
                Thread processDataThread = new Thread(pts);
                processDataThread.Start(networkConnectionParameter);
                dataReceived = false;

            }
            else
                dataReceived = false;
        }
        #endregion

        #region Method numberOfClientsChanged
        private void numberOfClientsChanged()
        {
            numberOfConnections = tcpHandler.NumberOfConnectedClients;
            if (NumberOfConnectedClientsChanged != null)
                NumberOfConnectedClientsChanged();
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
                SslStream sslStream = ((NetworkConnectionParameter)networkConnectionParameter).stream;
                int portIn = ((NetworkConnectionParameter)networkConnectionParameter).portIn;
                IPAddress ipAddressIn = ((NetworkConnectionParameter)networkConnectionParameter).ipAddressIn;


                Array.Copy(((NetworkConnectionParameter)networkConnectionParameter).bytes, 0, bytes, 0, ((NetworkConnectionParameter)networkConnectionParameter).bytes.Length);

                ModbusSecureProtocol receiveDataThread = new ModbusSecureProtocol();
                ModbusSecureProtocol sendDataThread = new ModbusSecureProtocol();

                try
                {
                    UInt16[] wordData = new UInt16[1];
                    byte[] byteData = new byte[2];
                    receiveDataThread.timeStamp = DateTime.Now;
                    receiveDataThread.request = true;
                    if (!serialFlag)
                    {
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
                    }

                    //Lese unit identifier
                    receiveDataThread.unitIdentifier = bytes[6 - 6 * Convert.ToInt32(serialFlag)];
                    //Check UnitIdentifier
                    if ((receiveDataThread.unitIdentifier != this.unitIdentifier) & (receiveDataThread.unitIdentifier != 0))
                        return;

                    // Lese function code
                    receiveDataThread.functionCode = bytes[7 - 6 * Convert.ToInt32(serialFlag)];

                    // Lese starting address 
                    byteData[1] = bytes[8 - 6 * Convert.ToInt32(serialFlag)];
                    byteData[0] = bytes[9 - 6 * Convert.ToInt32(serialFlag)];
                    Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                    receiveDataThread.startingAdress = wordData[0];

                    if (receiveDataThread.functionCode <= 4)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[11 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantity = wordData[0];
                    }
                    if (receiveDataThread.functionCode == 5)
                    {
                        receiveDataThread.receiveCoilValues = new ushort[1];
                        // Lese Value
                        byteData[1] = bytes[10 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[11 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveCoilValues, 0, 2);
                    }
                    if (receiveDataThread.functionCode == 6)
                    {
                        receiveDataThread.receiveRegisterValues = new ushort[1];
                        // Lese Value
                        byteData[1] = bytes[10 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[11 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveRegisterValues, 0, 2);
                    }
                    if (receiveDataThread.functionCode == 15)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[11 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantity = wordData[0];

                        receiveDataThread.byteCount = bytes[12 - 6 * Convert.ToInt32(serialFlag)];

                        if ((receiveDataThread.byteCount % 2) != 0)
                            receiveDataThread.receiveCoilValues = new ushort[receiveDataThread.byteCount / 2 + 1];
                        else
                            receiveDataThread.receiveCoilValues = new ushort[receiveDataThread.byteCount / 2];
                        // Lese Value
                        Buffer.BlockCopy(bytes, 13 - 6 * Convert.ToInt32(serialFlag), receiveDataThread.receiveCoilValues, 0, receiveDataThread.byteCount);
                    }
                    if (receiveDataThread.functionCode == 16)
                    {
                        // Lese quantity
                        byteData[1] = bytes[10 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[11 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantity = wordData[0];

                        receiveDataThread.byteCount = bytes[12 - 6 * Convert.ToInt32(serialFlag)];
                        receiveDataThread.receiveRegisterValues = new ushort[receiveDataThread.quantity];
                        for (int i = 0; i < receiveDataThread.quantity; i++)
                        {
                            // Lese Value
                            byteData[1] = bytes[13 + i * 2 - 6 * Convert.ToInt32(serialFlag)];
                            byteData[0] = bytes[14 + i * 2 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveRegisterValues, i * 2, 2);
                        }

                    }
                    if (receiveDataThread.functionCode == 23)
                    {
                        // Lese starting Address Read
                        byteData[1] = bytes[8 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[9 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.startingAddressRead = wordData[0];
                        // Lese quantity Read
                        byteData[1] = bytes[10 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[11 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantityRead = wordData[0];
                        // Lese starting Address Write
                        byteData[1] = bytes[12 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[13 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.startingAddressWrite = wordData[0];
                        // Lese quantity Write
                        byteData[1] = bytes[14 - 6 * Convert.ToInt32(serialFlag)];
                        byteData[0] = bytes[15 - 6 * Convert.ToInt32(serialFlag)];
                        Buffer.BlockCopy(byteData, 0, wordData, 0, 2);
                        receiveDataThread.quantityWrite = wordData[0];

                        receiveDataThread.byteCount = bytes[16 - 6 * Convert.ToInt32(serialFlag)];
                        receiveDataThread.receiveRegisterValues = new ushort[receiveDataThread.quantityWrite];
                        for (int i = 0; i < receiveDataThread.quantityWrite; i++)
                        {
                            // Lese Value
                            byteData[1] = bytes[17 + i * 2 - 6 * Convert.ToInt32(serialFlag)];
                            byteData[0] = bytes[18 + i * 2 - 6 * Convert.ToInt32(serialFlag)];
                            Buffer.BlockCopy(byteData, 0, receiveDataThread.receiveRegisterValues, i * 2, 2);
                        }
                    }
                }
                catch (Exception exc)
                { }
                this.CreateAnswer(receiveDataThread, sendDataThread, sslStream, portIn, ipAddressIn);
                //this.sendAnswer();
                this.CreateLogData(receiveDataThread, sendDataThread);

                if (LogDataChanged != null)
                    LogDataChanged();
            }
        }
        #endregion

        #region Method CreateAnswer
        private void CreateAnswer(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
                default:
                    sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                    sendData.exceptionCode = 1;
                    sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                    break;
            }
            sendData.timeStamp = DateTime.Now;
        }
        #endregion

        public virtual void ReadCoils(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[9 + sendData.byteCount + 2 * Convert.ToInt32(serialFlag)];

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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }
                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
            }
        }

        public virtual void ReadDiscreteInputs(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[9 + sendData.byteCount + 2 * Convert.ToInt32(serialFlag)];
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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }
                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
            }
        }

        public virtual void ReadHoldingRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
            if (((receiveData.startingAdress + 1 + receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))   //Invalid Starting adress or Starting address + quantity
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[9 + sendData.byteCount + 2 * Convert.ToInt32(serialFlag)];
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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }
                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
            }
        }

        public virtual void ReadInputRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
            if (((receiveData.startingAdress + 1 + receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))   //Invalid Starting adress or Starting address + quantity
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[9 + sendData.byteCount + 2 * Convert.ToInt32(serialFlag)];
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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
            }
        }

        public virtual void WriteSingleCoil(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
            if (((receiveData.startingAdress + 1) > 65535) | (receiveData.startingAdress < 0))    //Invalid Starting adress or Starting address + quantity
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[12 + 2 * Convert.ToInt32(serialFlag)];

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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
                if (CoilsChanged != null)
                    CoilsChanged(receiveData.startingAdress + 1, 1);
            }
        }

        public virtual void WriteSingleRegister(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
            if (((receiveData.startingAdress + 1) > 65535) | (receiveData.startingAdress < 0))    //Invalid Starting adress or Starting address + quantity
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[12 + 2 * Convert.ToInt32(serialFlag)];

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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
                if (HoldingRegistersChanged != null)
                    HoldingRegistersChanged(receiveData.startingAdress + 1, 1);
            }
        }

        public virtual void WriteMultipleCoils(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
            if ((((int)receiveData.startingAdress + 1 + (int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))    //Invalid Starting adress or Starting address + quantity
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[12 + 2 * Convert.ToInt32(serialFlag)];

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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
                if (CoilsChanged != null)
                    CoilsChanged(receiveData.startingAdress + 1, receiveData.quantity);
            }
        }

        public virtual void WriteMultipleRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
            if ((((int)receiveData.startingAdress + 1 + (int)receiveData.quantity) > 65535) | (receiveData.startingAdress < 0))   //Invalid Starting adress or Starting address + quantity
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[12 + 2 * Convert.ToInt32(serialFlag)];

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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
                if (HoldingRegistersChanged != null)
                    HoldingRegistersChanged(receiveData.startingAdress + 1, receiveData.quantity);
            }
        }

        public virtual void ReadWriteMultipleRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[9 + sendData.byteCount + 2 * Convert.ToInt32(serialFlag)];

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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }

                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
                if (HoldingRegistersChanged != null)
                    HoldingRegistersChanged(receiveData.startingAddressWrite + 1, receiveData.quantityWrite);
            }
        }

        protected void sendException(int errorCode, int exceptionCode, ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
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
                    data = new byte[9 + 2 * Convert.ToInt32(serialFlag)];
                else
                    data = new byte[9 + sendData.byteCount + 2 * Convert.ToInt32(serialFlag)];
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
                    if (serialFlag)
                    {
                        if (!serialport.IsOpen)
                            throw new EasyModbusSecure.Exceptions.SerialPortNotOpenedException("serial port not opened");
                        //Create CRC
                        sendData.crc = ModbusSecureClient.calculateCRC(data, Convert.ToUInt16(data.Length - 8), 6);
                        byteData = BitConverter.GetBytes((int)sendData.crc);
                        data[data.Length - 2] = byteData[0];
                        data[data.Length - 1] = byteData[1];
                        serialport.Write(data, 6, data.Length - 6);
                        if (debug)
                        {
                            byte[] debugData = new byte[data.Length - 6];
                            Array.Copy(data, 6, debugData, 0, data.Length - 6);
                            if (debug) StoreLogData.Instance.Store("Send Serial-Data: " + BitConverter.ToString(debugData), System.DateTime.Now);
                        }
                    }
                    else if (udpFlag)
                    {
                        //UdpClient udpClient = new UdpClient();
                        IPEndPoint endPoint = new IPEndPoint(ipAddressIn, portIn);
                        udpClient.Send(data, data.Length, endPoint);

                    }
                    else
                    {
                        stream.Write(data, 0, data.Length);
                        if (debug) StoreLogData.Instance.Store("Send Data: " + BitConverter.ToString(data), System.DateTime.Now);
                    }
                }
                catch (Exception) { }
            }
        }

        private void CreateLogData(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData)
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

        public ModbusSecureProtocol[] ModbusLogData
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

        public bool SerialFlag
        {
            get
            {
                return serialFlag;
            }
            set
            {
                serialFlag = value;
            }
        }

        public int Baudrate
        {
            get
            {
                return baudrate;
            }
            set
            {
                baudrate = value;
            }
        }

        public System.IO.Ports.Parity Parity
        {
            get
            {
                return parity;
            }
            set
            {
                parity = value;
            }
        }

        public System.IO.Ports.StopBits StopBits
        {
            get
            {
                return stopBits;
            }
            set
            {
                stopBits = value;
            }
        }

        public string SerialPort
        {
            get
            {
                return serialPort;
            }
            set
            {
                serialPort = value;
                if (serialPort != null)
                    serialFlag = true;
                else
                    serialFlag = false;
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




        public class HoldingRegisters
        {
            public Int16[] localArray = new Int16[65535];
            ModbusSecureServer ModbusSecureServer;

            public HoldingRegisters(EasyModbusSecure.ModbusSecureServer ModbusSecureServer)
            {
                this.ModbusSecureServer = ModbusSecureServer;
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
            ModbusSecureServer ModbusSecureServer;

            public InputRegisters(EasyModbusSecure.ModbusSecureServer ModbusSecureServer)
            {
                this.ModbusSecureServer = ModbusSecureServer;
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
            ModbusSecureServer ModbusSecureServer;

            public Coils(EasyModbusSecure.ModbusSecureServer ModbusSecureServer)
            {
                this.ModbusSecureServer = ModbusSecureServer;
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
            ModbusSecureServer ModbusSecureServer;

            public DiscreteInputs(EasyModbusSecure.ModbusSecureServer ModbusSecureServer)
            {
                this.ModbusSecureServer = ModbusSecureServer;
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

    public class ModbusSecureServerAuthZ : ModbusSecureServer
    {
        public bool FunctionCode1AuthZDisabled { get; set; }
        public bool FunctionCode2AuthZDisabled { get; set; }
        public bool FunctionCode3AuthZDisabled { get; set; }
        public bool FunctionCode4AuthZDisabled { get; set; }
        public bool FunctionCode5AuthZDisabled { get; set; }
        public bool FunctionCode6AuthZDisabled { get; set; }
        public bool FunctionCode15AuthZDisabled { get; set; }
        public bool FunctionCode16AuthZDisabled { get; set; }
        public bool FunctionCode23AuthZDisabled { get; set; }

        public ModbusSecureServerAuthZ(string certificate, string certificatePassword, bool mutualAuthentication, List<ValueTuple<string, List<byte>>> acceptableRoles)
            : base(certificate, certificatePassword, mutualAuthentication, acceptableRoles)
        {

            //secureServer = new ModbusSecureServer(certificate,certificatePassword, mutualAuthentication, acceptableRoles);
        }


        #region Method CheckRoleAccess
        private Boolean CheckRoleAccess(SslStream stream, string roleStr, byte functionCode)
        {
            // TODO: Maybe Modbus functions accessiblity should be checked

            //if (!this.acceptableRoles.Contains(ValueTuple.Create(roleStr, functionCode)))
            //{
            //    return false;
            //}
            //else if (this.acceptableRoles.Contains(ValueTuple.Create(roleStr, functionCode)))
            //{
            //    return true;
            //}

            if(acceptableRoles.Any(roleTuple => roleTuple.Item1.Equals(roleStr) && roleTuple.Item2.Contains(functionCode)))
            {
                return true;
            }

            return false;
        }
        #endregion


        public void HandleRoleCheck(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn, bool FunctionCodeNAuthZDisabled,
            Action<ModbusSecureProtocol, ModbusSecureProtocol, SslStream, int, IPAddress> ModbusFunction)
        {
            if (CheckRoleAccess(stream, this.TcpHandlerRole, receiveData.functionCode) && !FunctionCodeNAuthZDisabled)
            {
                //ModbusFunction(receiveData, sendData, stream, portIn, ipAddressIn);
                ModbusFunction(receiveData, sendData, stream, portIn, ipAddressIn);
            }
            else
            {
                sendData.errorCode = (byte)(receiveData.functionCode + 0x80);
                sendData.exceptionCode = 1;
                base.sendException(sendData.errorCode, sendData.exceptionCode, receiveData, sendData, stream, portIn, ipAddressIn);
                //TODO: Should this exception beeing logged as a client might try to access non-assigned functions?
            }
        }

        public override void ReadCoils(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode1AuthZDisabled, base.ReadCoils);
        }

        public override void ReadDiscreteInputs(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode2AuthZDisabled, base.ReadDiscreteInputs);
        }

        public override void ReadHoldingRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {            
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode3AuthZDisabled, base.ReadHoldingRegisters);
        }

        public override void ReadInputRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {            
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode4AuthZDisabled, base.ReadInputRegisters);
        }

        public override void WriteSingleCoil(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {          
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode5AuthZDisabled, base.WriteSingleCoil);
        }

        public override void WriteSingleRegister(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {        
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode6AuthZDisabled, base.WriteSingleRegister);
        }

        public override void WriteMultipleCoils(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {            
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode15AuthZDisabled, base.WriteMultipleCoils);
        }

        public override void WriteMultipleRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {            
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode16AuthZDisabled, base.WriteMultipleRegisters);
        }

        public override void ReadWriteMultipleRegisters(ModbusSecureProtocol receiveData, ModbusSecureProtocol sendData, SslStream stream, int portIn, IPAddress ipAddressIn)
        {           
            HandleRoleCheck(receiveData, sendData, stream, portIn, ipAddressIn, FunctionCode23AuthZDisabled, base.ReadWriteMultipleRegisters);
        }
    }

}
