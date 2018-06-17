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
// Novell.Directory.Ldap.Events.LdapEventSource.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;

namespace Novell.Directory.Ldap.Events
{
    /// <summary>
    ///     This is the base class for any EventSource.
    /// </summary>
    /// <seealso cref='Novell.Directory.Ldap.Events.PSearchEventSource' />
    /// <seealso cref='Novell.Directory.Ldap.Events.Edir.EdirEventSource' />
    public abstract class LdapEventSource
    {
        /// <summary>
        ///     DirectoryEventHandler is the delegate definition for DirectoryEvent.
        ///     The client (listener) has to register using this delegate in order to
        ///     get events that may not be recognized by the actual event source.
        /// </summary>
        public delegate void DirectoryEventHandler(object source, DirectoryEventArgs objDirectoryEventArgs);

        /// <summary>
        ///     DirectoryEventHandler is the delegate definition for DirectoryExceptionEvent.
        /// </summary>
        public delegate void DirectoryExceptionEventHandler(object source,
            DirectoryExceptionEventArgs objDirectoryExceptionEventArgs);

        protected internal const int EventTypeUnknown = -1;
        protected const int DefaultSleepTime = 1000;

        private DirectoryEventHandler _directoryEvent;

        private DirectoryExceptionEventHandler _directoryExceptionEvent;

        private int _sleepInterval = DefaultSleepTime;

        protected EventsGenerator MObjEventsGenerator;

        /// <summary>
        ///     SleepInterval controls the duration after which event polling is repeated.
        /// </summary>
        public int SleepInterval
        {
            get => _sleepInterval;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("SleepInterval", "cannot take the negative or zero values ");
                }

                _sleepInterval = value;
            }
        }

        protected abstract int GetListeners();

        protected ListenersCount GetCurrentListenersState()
        {
            var nListeners = 0;

            // Get Listeners registered with Actual EventSource
            nListeners += GetListeners();

            // Get Listeners registered for generic events
            if (null != _directoryEvent)
            {
                nListeners += _directoryEvent.GetInvocationList().Length;
            }

            // Get Listeners registered for exception events
            if (null != _directoryExceptionEvent)
            {
                nListeners += _directoryExceptionEvent.GetInvocationList().Length;
            }

            if (0 == nListeners)
            {
                return ListenersCount.Zero;
            }

            if (1 == nListeners)
            {
                return ListenersCount.One;
            }

            return ListenersCount.MoreThanOne;
        }

        protected void ListenerAdded()
        {
            // Get current state
            var lc = GetCurrentListenersState();

            switch (lc)
            {
                case ListenersCount.One:
                    // start search and polling if not already started
                    StartSearchAndPolling();
                    break;

                case ListenersCount.Zero:
                case ListenersCount.MoreThanOne:
                default:
                    break;
            }
        }

        protected void ListenerRemoved()
        {
            // Get current state
            var lc = GetCurrentListenersState();

            switch (lc)
            {
                case ListenersCount.Zero:
                    // stop search and polling if not already stopped
                    StopSearchAndPolling();
                    break;

                case ListenersCount.One:
                case ListenersCount.MoreThanOne:
                default:
                    break;
            }
        }

        protected abstract void StartSearchAndPolling();
        protected abstract void StopSearchAndPolling();

        /// <summary>
        ///     DirectoryEvent represents a generic Directory event.
        ///     If any event is not recognized by the actual
        ///     event sources, an object of corresponding DirectoryEventArgs
        ///     class is passed as part of the notification.
        /// </summary>
        public event DirectoryEventHandler DirectoryEvent
        {
            add
            {
                _directoryEvent += value;
                ListenerAdded();
            }
            remove
            {
                _directoryEvent -= value;
                ListenerRemoved();
            }
        }

        /// <summary>
        ///     DirectoryEvent represents a generic Directory exception event.
        /// </summary>
        public event DirectoryExceptionEventHandler DirectoryExceptionEvent
        {
            add
            {
                _directoryExceptionEvent += value;
                ListenerAdded();
            }
            remove
            {
                _directoryExceptionEvent -= value;
                ListenerRemoved();
            }
        }

        protected void StartEventPolling(
            LdapMessageQueue queue,
            LdapConnection conn,
            int msgid)
        {
            // validate the argument values
            if (queue == null
                || conn == null)
            {
                throw new ArgumentException("No parameter can be Null.");
            }

            if (null == MObjEventsGenerator)
            {
                MObjEventsGenerator = new EventsGenerator(this, queue, conn, msgid);
                MObjEventsGenerator.SleepTime = _sleepInterval;

                MObjEventsGenerator.StartEventPolling();
            }
        } // end of method StartEventPolling

        protected void StopEventPolling()
        {
            if (null != MObjEventsGenerator)
            {
                MObjEventsGenerator.StopEventPolling();
                MObjEventsGenerator = null;
            }
        } // end of method StopEventPolling

        protected abstract bool
            NotifyEventListeners(LdapMessage sourceMessage,
                EventClassifiers aClassification,
                int nType);

        protected void NotifyListeners(LdapMessage sourceMessage,
            EventClassifiers aClassification,
            int nType)
        {
            // first let the actual source Notify the listeners with
            // appropriate EventArgs

            var bListenersNotified = NotifyEventListeners(sourceMessage,
                aClassification,
                nType);

            if (!bListenersNotified)
            {
                // Actual EventSource could not recognize the event
                // Just notify the listeners for generic directory events
                NotifyDirectoryListeners(sourceMessage, aClassification);
            }
        }

        protected void NotifyDirectoryListeners(LdapMessage sourceMessage,
            EventClassifiers aClassification)
        {
            NotifyDirectoryListeners(new DirectoryEventArgs(sourceMessage,
                aClassification));
        }

        protected void NotifyDirectoryListeners(DirectoryEventArgs objDirectoryEventArgs)
        {
            if (null != _directoryEvent)
            {
                _directoryEvent(this, objDirectoryEventArgs);
            }
        }

        protected void NotifyExceptionListeners(LdapMessage sourceMessage, LdapException ldapException)
        {
            if (null != _directoryExceptionEvent)
            {
                _directoryExceptionEvent(this, new DirectoryExceptionEventArgs(sourceMessage, ldapException));
            }
        }

        protected enum ListenersCount
        {
            Zero,
            One,
            MoreThanOne
        }


        /// <summary>
        ///     This is a nested class that is supposed to monitor
        ///     LdapMessageQueue for events generated by the LDAP Server.
        /// </summary>
        protected class EventsGenerator
        {
            private readonly int _messageid;
            private readonly LdapEventSource _mObjLdapEventSource;
            private readonly LdapMessageQueue _searchqueue;
            private volatile bool _isrunning = true;
            private LdapConnection _ldapconnection;


            public EventsGenerator(LdapEventSource objEventSource,
                LdapMessageQueue queue,
                LdapConnection conn,
                int msgid)
            {
                _mObjLdapEventSource = objEventSource;
                _searchqueue = queue;
                _ldapconnection = conn;
                _messageid = msgid;
                SleepTime = DefaultSleepTime;
            } // end of Constructor

            /// <summary>
            ///     SleepTime controls the duration after which event polling is repeated.
            /// </summary>
            public int SleepTime { get; set; }

            protected void Run()
            {
                while (_isrunning)
                {
                    LdapMessage response = null;
                    try
                    {
                        while (_isrunning
                               && !_searchqueue.IsResponseReceived(_messageid))
                        {
                            Thread.Sleep(SleepTime);
                        }

                        if (_isrunning)
                        {
                            response = _searchqueue.GetResponse(_messageid);
                        }

                        if (response != null)
                        {
                            Processmessage(response);
                        }
                    }
                    catch (LdapException e)
                    {
                        _mObjLdapEventSource.NotifyExceptionListeners(response, e);
                    }
                }
            } // end of method run

            protected void Processmessage(LdapMessage response)
            {
                if (response is LdapResponse)
                {
                    try
                    {
                        ((LdapResponse) response).ChkResultCode();

                        _mObjLdapEventSource.NotifyEventListeners(response,
                            EventClassifiers.ClassificationUnknown,
                            EventTypeUnknown);
                    }
                    catch (LdapException e)
                    {
                        _mObjLdapEventSource.NotifyExceptionListeners(response, e);
                    }
                }
                else
                {
                    _mObjLdapEventSource.NotifyEventListeners(response,
                        EventClassifiers.ClassificationUnknown,
                        EventTypeUnknown);
                }
            } // end of method processmessage

            public void StartEventPolling()
            {
                _isrunning = true;
                new Thread(Run).Start();
            }

            public void StopEventPolling()
            {
                _isrunning = false;
            } // end of method stopEventGeneration
        } // end of class EventsGenerator
    } // end of class LdapEventSource
} // end of namespace