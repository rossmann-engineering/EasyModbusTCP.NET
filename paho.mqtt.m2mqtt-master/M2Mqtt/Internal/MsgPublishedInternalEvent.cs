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

using uPLibrary.Networking.M2Mqtt.Messages;

namespace uPLibrary.Networking.M2Mqtt.Internal
{
    /// <summary>
    /// Internal event for a published message
    /// </summary>
    public class MsgPublishedInternalEvent : MsgInternalEvent
    {
        #region Properties...

        /// <summary>
        /// Message published (or failed due to retries)
        /// </summary>
        public bool IsPublished
        {
            get { return this.isPublished; }
            internal set { this.isPublished = value; }
        }

        #endregion

        // published flag
        bool isPublished;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="msg">Message published</param>
        /// <param name="isPublished">Publish flag</param>
        public MsgPublishedInternalEvent(MqttMsgBase msg, bool isPublished) 
            : base(msg)
        {
            this.isPublished = isPublished;
        }
    }
}
