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
// Novell.Directory.Ldap.Events.PSearchEventSource.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//


using System;
using Novell.Directory.Ldap.Controls;

namespace Novell.Directory.Ldap.Events
{
    /// <summary>
    ///     This is the source class for Ldap events.
    /// </summary>
    public class PSearchEventSource : LdapEventSource
    {
        protected EventHandler<SearchResultEventArgs> _SearchResultEvent;

        /// <summary>
        ///     Caller has to register with this event in order to be notified of
        ///     corresponding Ldap search result event.
        /// </summary>
        public event EventHandler<SearchResultEventArgs> SearchResultEvent
        {
            add
            {
                _SearchResultEvent += value;
                ListenerAdded();
            }
            remove
            {
                _SearchResultEvent -= value;
                ListenerRemoved();
            }
        }

        protected EventHandler<SearchReferralEventArgs> _searchReferralEvent;

        /// <summary>
        ///     Caller has to register with this event in order to be notified of
        ///     corresponding Ldap search reference event.
        /// </summary>
        public event EventHandler<SearchReferralEventArgs> SearchReferralEvent
        {
            add
            {
                _searchReferralEvent += value;
                ListenerAdded();
            }
            remove
            {
                _searchReferralEvent -= value;
                ListenerRemoved();
            }
        }

        protected override int GetListeners()
        {
            var nListeners = 0;
            if (null != _SearchResultEvent)
                nListeners = _SearchResultEvent.GetInvocationList().Length;

            if (null != _searchReferralEvent)
                nListeners += _searchReferralEvent.GetInvocationList().Length;

            return nListeners;
        }

        protected LdapConnection Connection { get; set; }
        protected string SearchBase { get; set; }
        protected int Scope { get; set; }
        protected string[] Attrs { get; set; }
        protected string Filter { get; set; }
        protected bool TypesOnly { get; set; }
        protected LdapSearchConstraints SearchConstraints { get; set; }
        protected LdapEventType EventChangeType { get; set; }
        protected LdapSearchQueue Queue { get; set; }

        // Constructor
        public PSearchEventSource(LdapConnection conn,
                                  string searchBase,
                                  int scope,
                                  string filter,
                                  string[] attrs,
                                  bool typesOnly,
                                  LdapSearchConstraints constraints,
                                  LdapEventType eventchangetype,
                                  bool changeonly)
        {
            Connection = conn ?? throw new ArgumentNullException(nameof(conn));
            SearchBase = searchBase ?? throw new ArgumentNullException(nameof(searchBase));
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            Attrs = attrs ?? throw new ArgumentNullException(nameof(attrs));
            TypesOnly = typesOnly;
            Scope = scope;
            EventChangeType = eventchangetype;

            // make things ready for starting a search operation
            if (constraints == null)
            {
                SearchConstraints = new LdapSearchConstraints();
            }
            else
            {
                SearchConstraints = constraints;
            }

            //Create the persistent search control
            var psCtrl = new LdapPersistSearchControl((int)eventchangetype, // any change 
                            changeonly, //only get changes
                            true, //return entry change controls
                            true); //control is critcal

            // add the persistent search control to the search constraints
            SearchConstraints.Controls = new LdapControl[] { psCtrl };
        }

        protected override void StartSearchAndPolling()
        {
            // perform the search with no attributes returned
            Queue = Connection.Search(SearchBase, // container to search
                                      Scope, // search container's subtree
                                      Filter, // search filter, all objects
                                      Attrs, // don't return attributes
                                      TypesOnly, // return attrs and values or attrs only.
                                      null, // use default search queue
                                      SearchConstraints); // use default search constraints

            int[] ids = Queue.MessageIDs;

            if (ids.Length != 1)
            {
                throw new LdapException(null, LdapException.LOCAL_ERROR, "Unable to Obtain Message Id");
            }

            StartEventPolling(Queue, Connection, ids[0]);
        }

        protected override void StopSearchAndPolling()
        {
            Connection.Abandon(Queue);
            StopEventPolling();
        }

        protected override bool NotifyEventListeners(LdapMessage sourceMessage,
            EventClassifiers classification,
            int nType)
        {
            bool listenersNotified = false;
            if (sourceMessage == null)
            {
                return listenersNotified;
            }

            switch (sourceMessage.Type)
            {
                case LdapMessage.SEARCH_RESULT_REFERENCE:
                    if (_searchReferralEvent != null)
                    {
                        _searchReferralEvent(this,
                            new SearchReferralEventArgs(
                                sourceMessage,
                                classification,
                                (LdapEventType)nType)
                        );
                        listenersNotified = true;
                    }
                    break;

                case LdapMessage.SEARCH_RESPONSE:
                    if (_SearchResultEvent != null)
                    {
                        LdapEventType changeType = LdapEventType.TYPE_UNKNOWN;
                        var controls = sourceMessage.Controls;
                        foreach (var control in controls)
                        {
                            if (control is LdapEntryChangeControl con)
                            {
                                changeType = (LdapEventType)con.ChangeType;
                                // TODO: Why is this continue here..? (from Java code..)
                                // TODO: Why are we interested only in the last changeType..?
                            }
                        }
                        // if no changeType then value is TYPE_UNKNOWN
                        _SearchResultEvent(this,
                            new SearchResultEventArgs(
                                sourceMessage,
                                classification,
                                changeType)
                        );
                        listenersNotified = true;
                    }
                    break;

                case LdapMessage.SEARCH_RESULT:
                    // This is a generic LDAP Event
                    // TODO: Why the type is ANY...? (java code)
                    NotifyDirectoryListeners(new LdapEventArgs(sourceMessage,
                        EventClassifiers.CLASSIFICATION_LDAP_PSEARCH,
                        LdapEventType.LDAP_PSEARCH_ANY));
                    listenersNotified = true;
                    break;

                default:
                    // This seems to be some unknown event.
                    // Let this be notified to generic DirectoryListeners in the base class...
                    break;
            }

            return listenersNotified;
        }
    }
}