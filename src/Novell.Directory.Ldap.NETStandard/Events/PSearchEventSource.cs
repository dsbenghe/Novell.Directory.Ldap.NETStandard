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

using Novell.Directory.Ldap.Controls;
using System;

namespace Novell.Directory.Ldap.Events
{
    /// <summary>
    ///     This is the source class for Ldap events.
    /// </summary>
    public class PSearchEventSource : LdapEventSource
    {
        /// <summary>
        ///     SearchReferralEventHandler is the delegate definition for SearchReferralEvent.
        ///     The client (listener) has to register using this delegate in order to
        ///     get corresponding Ldap events.
        /// </summary>
        public delegate
            void SearchReferralEventHandler(
                object source,
                SearchReferralEventArgs objArgs);

        /// <summary>
        ///     SearchResultEventHandler is the delegate definition for SearchResultEvent.
        ///     The client (listener) has to register using this delegate in order to
        ///     get corresponding Ldap events.
        /// </summary>
        public delegate
            void SearchResultEventHandler(
                object source,
                SearchResultEventArgs objArgs);

        private readonly string[] _mAttrs;

        private readonly LdapConnection _mConnection;
        private readonly string _mFilter;
        private readonly int _mScope;
        private readonly string _mSearchBase;
        private readonly LdapSearchConstraints _mSearchConstraints;
        private readonly bool _mTypesOnly;
        private LdapEventType _mEventChangeType;

        private LdapSearchQueue _mQueue;

        private SearchReferralEventHandler _searchReferralEvent;
        private SearchResultEventHandler _searchResultEvent;

        // Constructor
        public PSearchEventSource(
            LdapConnection conn,
            string searchBase,
            int scope,
            string filter,
            string[] attrs,
            bool typesOnly,
            LdapSearchConstraints constraints,
            LdapEventType eventchangetype,
            bool changeonly)
        {
            // validate the input arguments
            if (conn == null
                || searchBase == null
                || filter == null
                || attrs == null)
            {
                throw new ArgumentException("Null argument specified");
            }

            _mConnection = conn;
            _mSearchBase = searchBase;
            _mScope = scope;
            _mFilter = filter;
            _mAttrs = attrs;
            _mTypesOnly = typesOnly;
            _mEventChangeType = eventchangetype;

            // make things ready for starting a search operation
            if (constraints == null)
            {
                _mSearchConstraints = new LdapSearchConstraints();
            }
            else
            {
                _mSearchConstraints = constraints;
            }

            // Create the persistent search control
            var psCtrl =
                new LdapPersistSearchControl(
                    (int)eventchangetype, // any change
                    changeonly, // only get changes
                    true, // return entry change controls
                    true); // control is critcal

            // add the persistent search control to the search constraints
            _mSearchConstraints.SetControls(psCtrl);
        } // end of Constructor

        /// <summary>
        ///     Caller has to register with this event in order to be notified of
        ///     corresponding Ldap search result event.
        /// </summary>
        public event SearchResultEventHandler SearchResultEvent
        {
            add
            {
                _searchResultEvent += value;
                ListenerAdded();
            }

            remove
            {
                _searchResultEvent -= value;
                ListenerRemoved();
            }
        }

        /// <summary>
        ///     Caller has to register with this event in order to be notified of
        ///     corresponding Ldap search reference event.
        /// </summary>
        public event SearchReferralEventHandler SearchReferralEvent
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
            if (_searchResultEvent != null)
            {
                nListeners = _searchResultEvent.GetInvocationList().Length;
            }

            if (_searchReferralEvent != null)
            {
                nListeners += _searchReferralEvent.GetInvocationList().Length;
            }

            return nListeners;
        }

        protected override void StartSearchAndPolling()
        {
            // perform the search with no attributes returned
            _mQueue =
                _mConnection.Search(
                    _mSearchBase, // container to search
                    _mScope, // search container's subtree
                    _mFilter, // search filter, all objects
                    _mAttrs, // don't return attributes
                    _mTypesOnly, // return attrs and values or attrs only.
                    null, // use default search queue
                    _mSearchConstraints); // use default search constraints

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
            if (sourceMessage == null)
            {
                return bListenersNotified;
            }

            switch (sourceMessage.Type)
            {
                case LdapMessage.SearchResultReference:
                    if (_searchReferralEvent != null)
                    {
                        _searchReferralEvent(
                            this,
                            new SearchReferralEventArgs(
                                sourceMessage,
                                aClassification,
                                (LdapEventType)nType));
                        bListenersNotified = true;
                    }

                    break;

                case LdapMessage.SearchResponse:
                    if (_searchResultEvent != null)
                    {
                        var changeType = LdapEventType.TypeUnknown;
                        var controls = sourceMessage.Controls;
                        foreach (var control in controls)
                        {
                            if (control is LdapEntryChangeControl)
                            {
                                changeType = (LdapEventType)((LdapEntryChangeControl)control).ChangeType;

                                // TODO: Why is this continue here..? (from Java code..)
                                // TODO: Why are we interested only in the last changeType..?
                            }
                        }

                        // if no changeType then value is TYPE_UNKNOWN
                        _searchResultEvent(
                            this,
                            new SearchResultEventArgs(
                                sourceMessage,
                                aClassification,
                                changeType));
                        bListenersNotified = true;
                    }

                    break;

                case LdapMessage.SearchResult:
                    // This is a generic LDAP Event
                    // TODO: Why the type is ANY...? (java code)
                    NotifyDirectoryListeners(new LdapEventArgs(
                        sourceMessage,
                        EventClassifiers.ClassificationLdapPsearch,
                        LdapEventType.LdapPsearchAny));
                    bListenersNotified = true;
                    break;
            }

            return bListenersNotified;
        }
    } // end of class PSearchEventSource
}
