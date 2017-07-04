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
using uPLibrary.Networking.M2Mqtt.Exceptions;

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Class for PINGRESP message from client to broker
    /// </summary>
    public class MqttMsgPingResp : MqttMsgBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MqttMsgPingResp()
        {
            this.type = MQTT_MSG_PINGRESP_TYPE;
        }

        /// <summary>
        /// Parse bytes for a PINGRESP message
        /// </summary>
        /// <param name="fixedHeaderFirstByte">First fixed header byte</param>
        /// <param name="protocolVersion">Protocol Version</param>
        /// <param name="channel">Channel connected to the broker</param>
        /// <returns>PINGRESP message instance</returns>
        public static MqttMsgPingResp Parse(byte fixedHeaderFirstByte, byte protocolVersion, IMqttNetworkChannel channel)
        {
            MqttMsgPingResp msg = new MqttMsgPingResp();

            if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1)
            {
                // [v3.1.1] check flag bits
                if ((fixedHeaderFirstByte & MSG_FLAG_BITS_MASK) != MQTT_MSG_PINGRESP_FLAG_BITS)
                    throw new MqttClientException(MqttClientErrorCode.InvalidFlagBits);
            }

            // already know remaininglength is zero (MQTT specification),
            // so it isn't necessary to read other data from socket
            int remainingLength = MqttMsgBase.decodeRemainingLength(channel);
            
            return msg;
        }

        public override byte[] GetBytes(byte protocolVersion)
        {
            byte[] buffer = new byte[2];
            int index = 0;

            // first fixed header byte
            if (protocolVersion == MqttMsgConnect.PROTOCOL_VERSION_V3_1_1)
                buffer[index++] = (MQTT_MSG_PINGRESP_TYPE << MSG_TYPE_OFFSET) | MQTT_MSG_PINGRESP_FLAG_BITS; // [v.3.1.1]
            else
                buffer[index++] = (MQTT_MSG_PINGRESP_TYPE << MSG_TYPE_OFFSET);
            buffer[index++] = 0x00;

            return buffer;
        }

        public override string ToString()
        {
#if TRACE
            return this.GetTraceString(
                "PINGRESP",
                null,
                null);
#else
            return base.ToString();
#endif
        }
    }
}
