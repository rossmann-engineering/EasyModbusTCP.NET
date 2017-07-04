/*
Copyright (c) 2013, 2014 Paolo Patierno

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution. 

The Eclipse Public License is available at 
   http://www.eclipse.org/legal/epl-v10.html
and the Eclipse Distribution License is available at 
   http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Paolo Patierno - initial API and implementation and/or initial documentation
*/

#if SSL
#if (MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3)
using Microsoft.SPOT.Net.Security;
#else
using System.Net.Security;
using System.Security.Authentication;
#endif
#endif
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System;

namespace uPLibrary.Networking.M2Mqtt
{
    /// <summary>
    /// Channel to communicate over the network
    /// </summary>
    public class MqttNetworkChannel : IMqttNetworkChannel
    {
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3 || COMPACT_FRAMEWORK)
        private readonly RemoteCertificateValidationCallback userCertificateValidationCallback;
        private readonly LocalCertificateSelectionCallback userCertificateSelectionCallback;
#endif
        // remote host information
        private string remoteHostName;
        private IPAddress remoteIpAddress;
        private int remotePort;

        // socket for communication
        private Socket socket;
        // using SSL
        private bool secure;

        // CA certificate (on client)
        private X509Certificate caCert;
        // Server certificate (on broker)
        private X509Certificate serverCert;
        // client certificate (on client)
        private X509Certificate clientCert;

        // SSL/TLS protocol version
        private MqttSslProtocols sslProtocol;

        /// <summary>
        /// Remote host name
        /// </summary>
        public string RemoteHostName { get { return this.remoteHostName; } }

        /// <summary>
        /// Remote IP address
        /// </summary>
        public IPAddress RemoteIpAddress { get { return this.remoteIpAddress; } }

        /// <summary>
        /// Remote port
        /// </summary>
        public int RemotePort { get { return this.remotePort; } }

#if SSL
        // SSL stream
        private SslStream sslStream;
#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
        private NetworkStream netStream;
#endif
#endif

        /// <summary>
        /// Data available on the channel
        /// </summary>
        public bool DataAvailable
        {
            get
            {
#if SSL
#if (MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3)
                if (secure)
                    return this.sslStream.DataAvailable;
                else
                    return (this.socket.Available > 0);
#else
                if (secure)
                    return this.netStream.DataAvailable;
                else
                    return (this.socket.Available > 0);
#endif
#else
                return (this.socket.Available > 0);
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">Socket opened with the client</param>
        public MqttNetworkChannel(Socket socket)
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3 || COMPACT_FRAMEWORK)
            : this(socket, false, null, MqttSslProtocols.None, null, null)
#else
            : this(socket, false, null, MqttSslProtocols.None)
#endif
        {

        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">Socket opened with the client</param>
        /// <param name="secure">Secure connection (SSL/TLS)</param>
        /// <param name="serverCert">Server X509 certificate for secure connection</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3 || COMPACT_FRAMEWORK)
        /// <param name="userCertificateSelectionCallback">A RemoteCertificateValidationCallback delegate responsible for validating the certificate supplied by the remote party</param>
        /// <param name="userCertificateValidationCallback">A LocalCertificateSelectionCallback delegate responsible for selecting the certificate used for authentication</param>
        public MqttNetworkChannel(Socket socket, bool secure, X509Certificate serverCert, MqttSslProtocols sslProtocol,
            RemoteCertificateValidationCallback userCertificateValidationCallback,
            LocalCertificateSelectionCallback userCertificateSelectionCallback)
#else
        public MqttNetworkChannel(Socket socket, bool secure, X509Certificate serverCert, MqttSslProtocols sslProtocol)
#endif
        {
            this.socket = socket;
            this.secure = secure;
            this.serverCert = serverCert;
            this.sslProtocol = sslProtocol;
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3 || COMPACT_FRAMEWORK)
            this.userCertificateValidationCallback = userCertificateValidationCallback;
            this.userCertificateSelectionCallback = userCertificateSelectionCallback;
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        public MqttNetworkChannel(string remoteHostName, int remotePort)
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3 || COMPACT_FRAMEWORK)
            : this(remoteHostName, remotePort, false, null, null, MqttSslProtocols.None, null, null)
#else
            : this(remoteHostName, remotePort, false, null, null, MqttSslProtocols.None)
#endif
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        /// <param name="secure">Using SSL</param>
        /// <param name="caCert">CA certificate</param>
        /// <param name="clientCert">Client certificate</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3 || COMPACT_FRAMEWORK)
        /// <param name="userCertificateSelectionCallback">A RemoteCertificateValidationCallback delegate responsible for validating the certificate supplied by the remote party</param>
        /// <param name="userCertificateValidationCallback">A LocalCertificateSelectionCallback delegate responsible for selecting the certificate used for authentication</param>
        public MqttNetworkChannel(string remoteHostName, int remotePort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol,
            RemoteCertificateValidationCallback userCertificateValidationCallback,
            LocalCertificateSelectionCallback userCertificateSelectionCallback)
#else
        public MqttNetworkChannel(string remoteHostName, int remotePort, bool secure, X509Certificate caCert, X509Certificate clientCert, MqttSslProtocols sslProtocol)
#endif
        {
            IPAddress remoteIpAddress = null;
            try
            {
                // check if remoteHostName is a valid IP address and get it
                remoteIpAddress = IPAddress.Parse(remoteHostName);
            }
            catch
            {
            }

            // in this case the parameter remoteHostName isn't a valid IP address
            if (remoteIpAddress == null)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(remoteHostName);
                if ((hostEntry != null) && (hostEntry.AddressList.Length > 0))
                {
                    // check for the first address not null
                    // it seems that with .Net Micro Framework, the IPV6 addresses aren't supported and return "null"
                    int i = 0;
                    while (hostEntry.AddressList[i] == null) i++;
                    remoteIpAddress = hostEntry.AddressList[i];
                }
                else
                {
                    throw new Exception("No address found for the remote host name");
                }
            }

            this.remoteHostName = remoteHostName;
            this.remoteIpAddress = remoteIpAddress;
            this.remotePort = remotePort;
            this.secure = secure;
            this.caCert = caCert;
            this.clientCert = clientCert;
            this.sslProtocol = sslProtocol;
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3 || COMPACT_FRAMEWORK)
            this.userCertificateValidationCallback = userCertificateValidationCallback;
            this.userCertificateSelectionCallback = userCertificateSelectionCallback;
#endif
        }

        /// <summary>
        /// Connect to remote server
        /// </summary>
        public void Connect()
        {
            this.socket = new Socket(this.remoteIpAddress.GetAddressFamily(), SocketType.Stream, ProtocolType.Tcp);
            // try connection to the broker
            this.socket.Connect(new IPEndPoint(this.remoteIpAddress, this.remotePort));

#if SSL
            // secure channel requested
            if (secure)
            {
                // create SSL stream
#if (MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3)
                this.sslStream = new SslStream(this.socket);
#else
                this.netStream = new NetworkStream(this.socket);
                this.sslStream = new SslStream(this.netStream, false, this.userCertificateValidationCallback, this.userCertificateSelectionCallback);
#endif

                // server authentication (SSL/TLS handshake)
#if (MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3)
                this.sslStream.AuthenticateAsClient(this.remoteHostName,
                    this.clientCert,
                    new X509Certificate[] { this.caCert },
                    SslVerification.CertificateRequired,
                    MqttSslUtility.ToSslPlatformEnum(this.sslProtocol));
#else
                X509CertificateCollection clientCertificates = null;
                // check if there is a client certificate to add to the collection, otherwise it's null (as empty)
                if (this.clientCert != null)
                    clientCertificates = new X509CertificateCollection(new X509Certificate[] { this.clientCert });

                this.sslStream.AuthenticateAsClient(this.remoteHostName,
                    clientCertificates,
                    MqttSslUtility.ToSslPlatformEnum(this.sslProtocol),
                    false);
                
#endif
            }
#endif
        }

        /// <summary>
        /// Send data on the network channel
        /// </summary>
        /// <param name="buffer">Data buffer to send</param>
        /// <returns>Number of byte sent</returns>
        public int Send(byte[] buffer)
        {
#if SSL
            if (this.secure)
            {
                this.sslStream.Write(buffer, 0, buffer.Length);
                this.sslStream.Flush();
                return buffer.Length;
            }
            else
                return this.socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
#else
            return this.socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
#endif
        }

        /// <summary>
        /// Receive data from the network
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer)
        {
#if SSL
            if (this.secure)
            {
                // read all data needed (until fill buffer)
                int idx = 0, read = 0;
                while (idx < buffer.Length)
                {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = this.sslStream.Read(buffer, idx, buffer.Length - idx);
                    if (read == 0)
                        return 0;
                    idx += read;
                }
                return buffer.Length;
            }
            else
            {
                // read all data needed (until fill buffer)
                int idx = 0, read = 0;
                while (idx < buffer.Length)
                {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.
                    read = this.socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                    if (read == 0)
                        return 0;
                    idx += read;
                }
                return buffer.Length;
            }
#else
            // read all data needed (until fill buffer)
            int idx = 0, read = 0;
            while (idx < buffer.Length)
            {
                // fixed scenario with socket closed gracefully by peer/broker and
                // Read return 0. Avoid infinite loop.
                read = this.socket.Receive(buffer, idx, buffer.Length - idx, SocketFlags.None);
                if (read == 0)
                    return 0;
                idx += read;
            }
            return buffer.Length;
#endif
        }

        /// <summary>
        /// Receive data from the network channel with a specified timeout
        /// </summary>
        /// <param name="buffer">Data buffer for receiving data</param>
        /// <param name="timeout">Timeout on receiving (in milliseconds)</param>
        /// <returns>Number of bytes received</returns>
        public int Receive(byte[] buffer, int timeout)
        {
            // check data availability (timeout is in microseconds)
            if (this.socket.Poll(timeout * 1000, SelectMode.SelectRead))
            {
                return this.Receive(buffer);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Close the network channel
        /// </summary>
        public void Close()
        {
#if SSL
            if (this.secure)
            {
#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
                this.netStream.Close();
#endif
                this.sslStream.Close();
            }
            this.socket.Close();
#else
            this.socket.Close();
#endif
        }

        /// <summary>
        /// Accept connection from a remote client
        /// </summary>
        public void Accept()
        {
#if SSL
            // secure channel requested
            if (secure)
            {
#if !(MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3)

                this.netStream = new NetworkStream(this.socket);
                this.sslStream = new SslStream(this.netStream, false, this.userCertificateValidationCallback, this.userCertificateSelectionCallback);

                this.sslStream.AuthenticateAsServer(this.serverCert, false, MqttSslUtility.ToSslPlatformEnum(this.sslProtocol), false);
#endif
            }

            return;
#else
            return;
#endif
        }
    }

    /// <summary>
    /// IPAddress Utility class
    /// </summary>
    public static class IPAddressUtility
    {
        /// <summary>
        /// Return AddressFamily for the IP address
        /// </summary>
        /// <param name="ipAddress">IP address to check</param>
        /// <returns>Address family</returns>
        public static AddressFamily GetAddressFamily(this IPAddress ipAddress)
        {
#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
            return ipAddress.AddressFamily;
#else
            return (ipAddress.ToString().IndexOf(':') != -1) ? 
                AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
#endif
        }
    }

    /// <summary>
    /// MQTT SSL utility class
    /// </summary>
    public static class MqttSslUtility
    {
#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3 && !COMPACT_FRAMEWORK)
        public static SslProtocols ToSslPlatformEnum(MqttSslProtocols mqttSslProtocol)
        {
            switch (mqttSslProtocol)
            {
                case MqttSslProtocols.None:
                    return SslProtocols.None;
                case MqttSslProtocols.SSLv3:
                    return SslProtocols.Ssl3;
                case MqttSslProtocols.TLSv1_0:
                    return SslProtocols.Tls;
                case MqttSslProtocols.TLSv1_1:
                    return SslProtocols.Tls11;
                case MqttSslProtocols.TLSv1_2:
                    return SslProtocols.Tls12;
                default:
                    throw new ArgumentException("SSL/TLS protocol version not supported");
            }
        }
#elif (MF_FRAMEWORK_VERSION_V4_2 || MF_FRAMEWORK_VERSION_V4_3)
        public static SslProtocols ToSslPlatformEnum(MqttSslProtocols mqttSslProtocol)
        {
            switch (mqttSslProtocol)
            {
                case MqttSslProtocols.None:
                    return SslProtocols.None;
                case MqttSslProtocols.SSLv3:
                    return SslProtocols.SSLv3;
                case MqttSslProtocols.TLSv1_0:
                    return SslProtocols.TLSv1;
                case MqttSslProtocols.TLSv1_1:
                case MqttSslProtocols.TLSv1_2:
                default:
                    throw new ArgumentException("SSL/TLS protocol version not supported");
            }
        }
#endif
    }
}
