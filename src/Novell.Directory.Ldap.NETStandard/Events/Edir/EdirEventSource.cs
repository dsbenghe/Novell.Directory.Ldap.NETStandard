/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Events.Edir.EdirEventSource.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
using System;

namespace Novell.Directory.Ldap.Events.Edir
{
    /// <summary>
    ///     This is the source class for Edir events.
    /// </summary>
    public class EdirEventSource : LdapEventSource
    {
        protected EventHandler<EdirEventArgs> edir_event;

        /// <summary>
        ///     Caller has to register with this event in order to be notified of
        ///     corresponding Edir events.
        /// </summary>
        public event EventHandler<EdirEventArgs> EdirEvent
        {
            add
            {
                edir_event += value;
                ListenerAdded();
            }
            remove
            {
                edir_event -= value;
                ListenerRemoved();
            }
        }

        protected override int GetListeners()
        {
            var nListeners = 0;
            if (null != edir_event)
                nListeners = edir_event.GetInvocationList().Length;

            return nListeners;
        }

        protected LdapConnection Connection { get; set; }
        protected MonitorEventRequest RequestOperation { get; set; }
        protected LdapResponseQueue Queue { get; set; }

        public EdirEventSource(EdirEventSpecifier[] specifier, LdapConnection conn)
        {
            if (specifier == null)
                throw new ArgumentNullException(nameof(specifier));
            RequestOperation = new MonitorEventRequest(specifier);
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
        }

        protected override void StartSearchAndPolling()
        {
            Queue = Connection.ExtendedOperation(RequestOperation, null, null);
            var ids = Queue.MessageIDs;

            if (ids.Length != 1)
            {
                throw new LdapException(
                    null,
                    LdapException.LOCAL_ERROR,
                    "Unable to Obtain Message Id"
                );
            }

            StartEventPolling(Queue, Connection, ids[0]);
        }

        protected override void StopSearchAndPolling()
        {
            Connection.Abandon(Queue);
            StopEventPolling();
        }

        protected override bool NotifyEventListeners(LdapMessage sourceMessage,
            EventClassifiers aClassification,
            int nType)
        {
            var bListenersNotified = false;
            if (null != edir_event)
            {
                if (null != sourceMessage)
                {
                    if (sourceMessage.Type == LdapMessage.INTERMEDIATE_RESPONSE &&
                        sourceMessage is EdirEventIntermediateResponse)
                    {
                        edir_event(this,
                            new EdirEventArgs(sourceMessage,
                                EventClassifiers.CLASSIFICATION_EDIR_EVENT));
                        bListenersNotified = true;
                    }
                }
            }

            return bListenersNotified;
        }
    }
}