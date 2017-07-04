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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Threading;

namespace uPLibrary.Networking.M2Mqtt
{
    public class MqttNetworkChannel : IMqttNetworkChannel
    {
        // stream socket for communication
        private StreamSocket socket;

        // remote host information
        private HostName remoteHostName;
        private int remotePort;

        // using SSL
        private bool secure;

        // SSL/TLS protocol version
        private MqttSslProtocols sslProtocol;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">Socket opened with the client</param>
        public MqttNetworkChannel(StreamSocket socket)
        {
            this.socket = socket;
            this.sslProtocol = MqttSslProtocols.None;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="remoteHostName">Remote Host name</param>
        /// <param name="remotePort">Remote port</param>
        /// <param name="secure">Using SSL</param>
        /// <param name="sslProtocol">SSL/TLS protocol version</param>
        public MqttNetworkChannel(string remoteHostName, int remotePort, bool secure, MqttSslProtocols sslProtocol)
        {
            this.remoteHostName = new HostName(remoteHostName);
            this.remotePort = remotePort;
            this.secure = secure;
            this.sslProtocol = sslProtocol;

            if (secure && (sslProtocol == MqttSslProtocols.None))
                throw new ArgumentException("For secure connection, an SSL/TLS protocol version is needed");
        }

        public bool DataAvailable
        {
            get { return true; }
        }

        public int Receive(byte[] buffer)
        {
            IBuffer result;

            // read all data needed (until fill buffer)
            int idx = 0;
            while (idx < buffer.Length)
            {
                // fixed scenario with socket closed gracefully by peer/broker and
                // Read return 0. Avoid infinite loop.

                // read is executed synchronously
                result = this.socket.InputStream.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.None).AsTask().Result;
                if (result.Length == 0)
                    return 0;
                idx += (int)result.Length;
            }
            return buffer.Length;
        }

        public int Receive(byte[] buffer, int timeout)
        {
            CancellationTokenSource cts = new CancellationTokenSource(timeout);

            try
            {
                IBuffer result;

                // read all data needed (until fill buffer)
                int idx = 0;
                while (idx < buffer.Length)
                {
                    // fixed scenario with socket closed gracefully by peer/broker and
                    // Read return 0. Avoid infinite loop.

                    // read is executed synchronously
                    result = this.socket.InputStream.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.None).AsTask(cts.Token).Result;
                    if (result.Length == 0)
                        return 0;
                    idx += (int)result.Length;
                }
                return buffer.Length;
            }
            catch (TaskCanceledException)
            {
                return 0;
            }
        }

        public int Send(byte[] buffer)
        {
            // send is executed synchronously
            return (int)this.socket.OutputStream.WriteAsync(buffer.AsBuffer()).AsTask().Result;
        }

        public void Close()
        {
            this.socket.Dispose();
        }

        public void Connect()
        {
            this.socket = new StreamSocket();

            // connection is executed synchronously
            this.socket.ConnectAsync(this.remoteHostName,
                this.remotePort.ToString(),
                MqttSslUtility.ToSslPlatformEnum(this.sslProtocol)).AsTask().Wait();
        }

        public void Accept()
        {
            // TODO : SSL support with StreamSocket / StreamSocketListener seems to be NOT supported
            return;
        }
    }

    /// <summary>
    /// MQTT SSL utility class
    /// </summary>
    public static class MqttSslUtility
    {
        public static SocketProtectionLevel ToSslPlatformEnum(MqttSslProtocols mqttSslProtocol)
        {
            switch (mqttSslProtocol)
            {
                case MqttSslProtocols.None:
                    return SocketProtectionLevel.PlainSocket;
                case MqttSslProtocols.SSLv3:
                    return SocketProtectionLevel.SslAllowNullEncryption;
                case MqttSslProtocols.TLSv1_0:
                    return SocketProtectionLevel.Tls10;
                case MqttSslProtocols.TLSv1_1:
                    return SocketProtectionLevel.Tls11;
                case MqttSslProtocols.TLSv1_2:
                    return SocketProtectionLevel.Tls12;
                default:
                    throw new ArgumentException("SSL/TLS protocol version not supported");
            }
        }
    }
}
