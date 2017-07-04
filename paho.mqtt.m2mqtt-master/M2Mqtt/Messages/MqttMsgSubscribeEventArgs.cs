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

#if (!MF_FRAMEWORK_VERSION_V4_2 && !MF_FRAMEWORK_VERSION_V4_3)
using System;
#else
using Microsoft.SPOT;
#endif

namespace uPLibrary.Networking.M2Mqtt.Messages
{
    /// <summary>
    /// Event Args class for subscribe request on topics
    /// </summary>
    public class MqttMsgSubscribeEventArgs : EventArgs
    {
        #region Properties...

        /// <summary>
        /// Message identifier
        /// </summary>
        public ushort MessageId
        {
            get { return this.messageId; }
            internal set { this.messageId = value; }
        }

        /// <summary>
        /// Topics requested to subscribe
        /// </summary>
        public string[] Topics
        {
            get { return this.topics; }
            internal set { this.topics = value; }
        }

        /// <summary>
        /// List of QOS Levels requested
        /// </summary>
        public byte[] QoSLevels
        {
            get { return this.qosLevels; }
            internal set { this.qosLevels = value; }
        }

        #endregion

        // message identifier
        ushort messageId;
        // topics requested to subscribe
        string[] topics;
        // QoS levels requested
        byte[] qosLevels;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribe topics request</param>
        /// <param name="topics">Topics requested to subscribe</param>
        /// <param name="qosLevels">List of QOS Levels requested</param>
        public MqttMsgSubscribeEventArgs(ushort messageId, string[] topics, byte[] qosLevels)
        {
            this.messageId = messageId;
            this.topics = topics;
            this.qosLevels = qosLevels;
        }
    }
}
