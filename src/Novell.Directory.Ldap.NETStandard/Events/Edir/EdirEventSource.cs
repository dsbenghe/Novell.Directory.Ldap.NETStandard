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
        /// <summary>
        ///     EdirEventHandler is the delegate definition for EdirEvent.
        ///     The client (listener) has to register using this delegate in order to
        ///     get corresponding Edir events.
        /// </summary>
        public delegate
            void EdirEventHandler(
                object source,
                EdirEventArgs objEdirEventArgs);

        private readonly LdapConnection _mConnection;
        private readonly MonitorEventRequest _mRequestOperation;
        private EdirEventHandler _edirEvent;
        private LdapResponseQueue _mQueue;

        public EdirEventSource(EdirEventSpecifier[] specifier, LdapConnection conn)
        {
            if (specifier == null || conn == null)
            {
                throw new ArgumentException("Null argument specified");
            }

            _mRequestOperation = new MonitorEventRequest(specifier);
            _mConnection = conn;
        }

        /// <summary>
        ///     Caller has to register with this event in order to be notified of
        ///     corresponding Edir events.
        /// </summary>
        public event EdirEventHandler EdirEvent
        {
            add
            {
                _edirEvent += value;
                ListenerAdded();
            }

            remove
            {
                _edirEvent -= value;
                ListenerRemoved();
            }
        }

        protected override int GetListeners()
        {
            var nListeners = 0;
            if (_edirEvent != null)
            {
                nListeners = _edirEvent.GetInvocationList().Length;
            }

            return nListeners;
        }

        protected override void StartSearchAndPolling()
        {
            _mQueue = _mConnection.ExtendedOperation(_mRequestOperation, null, null);
            var ids = _mQueue.MessageIDs;

            if (ids.Length != 1)
            {
                throw new LdapException(
                    null,
                    LdapException.LocalError,
                    "Unable to Obtain Message Id");
            }

            StartEventPolling(_mQueue, _mConnection, ids[0]);
        }

        protected override void StopSearchAndPolling()
        {
            _mConnection.Abandon(_mQueue);
            StopEventPolling();
        }

        protected override bool NotifyEventListeners(
            LdapMessage sourceMessage,
            EventClassifiers aClassification,
            int nType)
        {
            var bListenersNotified = false;
            if (_edirEvent != null)
            {
                if (sourceMessage != null)
                {
                    if (sourceMessage.Type == LdapMessage.IntermediateResponse &&
                        sourceMessage is EdirEventIntermediateResponse)
                    {
                        _edirEvent(
                            this,
                            new EdirEventArgs(
                                sourceMessage,
                                EventClassifiers.ClassificationEdirEvent));
                        bListenersNotified = true;
                    }
                }
            }

            return bListenersNotified;
        }
    }
}