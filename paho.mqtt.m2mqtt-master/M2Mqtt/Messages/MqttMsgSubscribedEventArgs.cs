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
    /// Event Args class for subscribed topics
    /// </summary>
    public class MqttMsgSubscribedEventArgs : EventArgs
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
        /// List of granted QOS Levels
        /// </summary>
        public byte[] GrantedQoSLevels
        {
            get { return this.grantedQosLevels; }
            internal set { this.grantedQosLevels = value; }
        }

        #endregion

        // message identifier
        ushort messageId;
        // granted QOS levels
        byte[] grantedQosLevels;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageId">Message identifier for subscribed topics</param>
        /// <param name="grantedQosLevels">List of granted QOS Levels</param>
        public MqttMsgSubscribedEventArgs(ushort messageId, byte[] grantedQosLevels)
        {
            this.messageId = messageId;
            this.grantedQosLevels = grantedQosLevels;
        }
    }
}
