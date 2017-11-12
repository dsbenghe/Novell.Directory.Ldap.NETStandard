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
using System.Threading.Tasks;

namespace Novell.Directory.Ldap.Events
{
    /// <summary>
    ///     This is the base class for any EventSource.
    /// </summary>
    /// <seealso cref='Novell.Directory.Ldap.Events.PSearchEventSource' />
    /// <seealso cref='Novell.Directory.Ldap.Events.Edir.EdirEventSource' />
    public abstract class LdapEventSource
    {
        protected enum LISTENERS_COUNT
        {
            ZERO,
            ONE,
            MORE_THAN_ONE
        }

        protected internal const int EVENT_TYPE_UNKNOWN = -1;
        protected const int DEFAULT_SLEEP_TIME = 1000;

        private int _sleepInterval = DEFAULT_SLEEP_TIME;

        /// <summary>
        ///     SleepInterval controls the duration after which event polling is repeated.
        /// </summary>
        public int SleepInterval
        {
            get { return _sleepInterval; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("SleepInterval", "cannot take the negative or zero values ");
                _sleepInterval = value;
            }
        }

        protected abstract int GetListeners();

        protected LISTENERS_COUNT GetCurrentListenersState()
        {
            var nListeners = 0;

            // Get Listeners registered with Actual EventSource
            nListeners += GetListeners();

            // Get Listeners registered for generic events
            if (null != _directoryEvent)
                nListeners += _directoryEvent.GetInvocationList().Length;

            // Get Listeners registered for exception events
            if (null != _directoryExceptionEvent)
                nListeners += _directoryExceptionEvent.GetInvocationList().Length;

            if (0 == nListeners)
                return LISTENERS_COUNT.ZERO;

            if (1 == nListeners)
                return LISTENERS_COUNT.ONE;

            return LISTENERS_COUNT.MORE_THAN_ONE;
        }

        protected void ListenerAdded()
        {
            switch (GetCurrentListenersState())
            {
                case LISTENERS_COUNT.ONE:
                    // start search and polling if not already started
                    StartSearchAndPolling();
                    break;

                case LISTENERS_COUNT.ZERO:
                case LISTENERS_COUNT.MORE_THAN_ONE:
                default:
                    break;
            }
        }

        protected void ListenerRemoved()
        {
            switch (GetCurrentListenersState())
            {
                case LISTENERS_COUNT.ZERO:
                    // stop search and polling if not already stopped
                    StopSearchAndPolling();
                    break;

                case LISTENERS_COUNT.ONE:
                case LISTENERS_COUNT.MORE_THAN_ONE:
                default:
                    break;
            }
        }

        protected abstract void StartSearchAndPolling();
        protected abstract void StopSearchAndPolling();

        protected EventHandler<DirectoryEventArgs> _directoryEvent;

        /// <summary>
        ///     DirectoryEvent represents a generic Directory event.
        ///     If any event is not recognized by the actual
        ///     event sources, an object of corresponding DirectoryEventArgs
        ///     class is passed as part of the notification.
        /// </summary>
        public event EventHandler<DirectoryEventArgs> DirectoryEvent
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


        private EventHandler<DirectoryExceptionEventArgs> _directoryExceptionEvent;

        /// <summary>
        ///     DirectoryEvent represents a generic Directory exception event.
        /// </summary>
        public event EventHandler<DirectoryExceptionEventArgs> DirectoryExceptionEvent
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

        protected EventsGenerator _objEventsGenerator;

        protected void StartEventPolling(
            LdapMessageQueue queue,
            LdapConnection conn,
            int msgid)
        {
            // validate the argument values
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));
            if (conn == null)
                throw new ArgumentNullException(nameof(conn));

            if (_objEventsGenerator == null)
            {
                _objEventsGenerator = new EventsGenerator(this, queue, conn, msgid)
                {
                    SleepTime = _sleepInterval
                };

                _objEventsGenerator.StartEventPolling();
            }
        } // end of method StartEventPolling

        protected void StopEventPolling()
        {
            _objEventsGenerator?.StopEventPolling();
            _objEventsGenerator = null;
        } // end of method StopEventPolling

        protected abstract bool NotifyEventListeners(LdapMessage sourceMessage, EventClassifiers classification, int type);

        protected void NotifyListeners(LdapMessage sourceMessage, EventClassifiers classification, int type)
        {
            // first let the actual source Notify the listeners with
            // appropriate EventArgs

            bool listenersNotified = NotifyEventListeners(sourceMessage, classification, type);

            if (!listenersNotified)
            {
                // Actual EventSource could not recognize the event
                // Just notify the listeners for generic directory events
                NotifyDirectoryListeners(sourceMessage, classification);
            }
        }

        protected void NotifyDirectoryListeners(LdapMessage sourceMessage, EventClassifiers classification)
        {
            NotifyDirectoryListeners(new DirectoryEventArgs(sourceMessage, classification));
        }

        protected void NotifyDirectoryListeners(DirectoryEventArgs objDirectoryEventArgs)
        {
            _directoryEvent?.Invoke(this, objDirectoryEventArgs);
        }

        protected void NotifyExceptionListeners(LdapMessage sourceMessage, LdapException ldapException)
        {
            _directoryExceptionEvent?.Invoke(this, new DirectoryExceptionEventArgs(sourceMessage, ldapException));
        }


        /// <summary>
        ///     This is a nested class that is supposed to monitor
        ///     LdapMessageQueue for events generated by the LDAP Server.
        /// </summary>
        protected class EventsGenerator
        {
            private readonly LdapEventSource _objLdapEventSource;
            private readonly LdapMessageQueue _searchqueue;
            private readonly int _messageid;
            private LdapConnection _ldapconnection;
            private CancellationTokenSource _source;

            /// <summary>
            ///     SleepTime controls the duration after which event polling is repeated.
            /// </summary>
            public int SleepTime { get; set; }


            public EventsGenerator(LdapEventSource objEventSource,
                                    LdapMessageQueue queue,
                                    LdapConnection conn,
                                    int msgid)
            {
                _objLdapEventSource = objEventSource;
                _searchqueue = queue;
                _ldapconnection = conn;
                _messageid = msgid;
                SleepTime = DEFAULT_SLEEP_TIME;
                _source = new CancellationTokenSource();
            }

            protected Task Run()
            {
                return Run(_source.Token);
            }

            protected async Task Run(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    LdapMessage response = null;
                    try
                    {
                        while (!token.IsCancellationRequested && !_searchqueue.IsResponseReceived(_messageid))
                        {
                            await Task.Delay(SleepTime);
                        }

                        if (!token.IsCancellationRequested)
                        {
                            response = _searchqueue.GetResponse(_messageid );
                        }

                        if (response != null)
                        {
                            Processmessage(response);
                        }
                    }
                    catch (LdapException e)
                    {
                        _objLdapEventSource.NotifyExceptionListeners(response, e);
                    }
                }
            }


            protected void Processmessage(LdapMessage response)
            {
                if (response is LdapResponse resp)
                {
                    try
                    {
                        resp.ChkResultCode();

                        _objLdapEventSource.NotifyEventListeners(response, EventClassifiers.CLASSIFICATION_UNKNOWN, EVENT_TYPE_UNKNOWN);
                    }
                    catch (LdapException e)
                    {
                        _objLdapEventSource.NotifyExceptionListeners(response, e);
                    }
                }
                else
                {
                    _objLdapEventSource.NotifyEventListeners(response, EventClassifiers.CLASSIFICATION_UNKNOWN, EVENT_TYPE_UNKNOWN);
                }
            }

            public Task StartEventPolling(CancellationTokenSource tokenSource)
            {
                _source?.Cancel();
                _source = tokenSource;
                return Run();
            }

            public Task StartEventPolling()
            {
                _source?.Cancel();
                _source = new CancellationTokenSource();
                return Run();
            }

            public void StopEventPolling()
            {
                _source?.Cancel();
            }
        }
    }
}