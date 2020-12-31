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

using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Novell.Directory.Ldap
{
    /// <inheritdoc />
    public class LdapSearchResults : ILdapSearchResults
    {
        private readonly int _batchSize; // Application specified batch size
        private readonly LdapSearchConstraints _cons; // LdapSearchConstraints for search

        private readonly ArrayList _entries; // Search entries // TODO: Can't make Generic, holds different types
        private readonly LdapSearchQueue _queue;
        private readonly List<string[]> _references; // Search Result References
        private readonly LdapConnection _conn;
        private bool _completed; // All entries received
        private int _entryCount; // # Search entries in vector
        private int _entryIndex; // Current position in vector
        private int _referenceCount; // # Search Result Reference in vector

        private int _referenceIndex; // Current position in vector
        private ArrayList _referralConn; // Referral Connections

        /// <summary>
        ///     Constructs a queue object for search results.
        /// </summary>
        /// <param name="queue">
        ///     The queue for the search results.
        /// </param>
        /// <param name="cons">
        ///     The LdapSearchConstraints associated with this search.
        /// </param>
        internal LdapSearchResults(LdapConnection conn, LdapSearchQueue queue, LdapSearchConstraints cons)
        {
            // setup entry Vector
            _conn = conn;
            _cons = cons;
            var requestedBatchSize = cons.BatchSize;
            _entries = new ArrayList(requestedBatchSize == 0 ? 64 : requestedBatchSize);
            _entryCount = 0;
            _entryIndex = 0;

            // setup search reference Vector
            _references = new List<string[]>(5);
            _referenceCount = 0;
            _referenceIndex = 0;

            _queue = queue;
            _batchSize = requestedBatchSize == 0 ? int.MaxValue : requestedBatchSize;
        }

        /// <summary>
        ///     Returns the latest server controls returned by the server
        ///     in the context of this search request, or null
        ///     if no server controls were returned.
        /// </summary>
        /// <returns>
        ///     The server controls returned with the search request, or null
        ///     if none were returned.
        /// </returns>
        public LdapControl[] ResponseControls { get; private set; }

        /// <summary>
        ///     Collects batchSize elements from an LdapSearchQueue message
        ///     queue and places them in a Vector.
        ///     If the last message from the server,
        ///     the result message, contains an error, it will be stored in the Vector
        ///     for nextElement to process. (although it does not increment the search
        ///     result count) All search result entries will be placed in the Vector.
        ///     If a null is returned from getResponse(), it is likely that the search
        ///     was abandoned.
        /// </summary>
        /// <returns>
        ///     true if all search results have been placed in the vector.
        /// </returns>
        private bool BatchOfResults
        {
            get
            {
                LdapMessage msg;

                // <=batchSize so that we can pick up the result-done message
                for (var i = 0; i < _batchSize;)
                {
                    try
                    {
                        if ((msg = _queue.GetResponse()) != null)
                        {
                            // Only save controls if there are some
                            var ctls = msg.Controls;
                            if (ctls != null)
                            {
                                ResponseControls = ctls;
                            }

                            if (msg is LdapSearchResult)
                            {
                                // Search Entry
                                object entry = ((LdapSearchResult)msg).Entry;
                                _entries.Add(entry);
                                i++;
                                _entryCount++;
                            }
                            else if (msg is LdapSearchResultReference)
                            {
                                // Search Ref
                                var refs = ((LdapSearchResultReference)msg).Referrals;

                                if (_cons.ReferralFollowing)
                                {
                                    _referralConn = _conn.ChaseReferral(_queue, _cons, msg, refs, 0, true, _referralConn);
                                }
                                else
                                {
                                    _references.Add(refs);
                                    _referenceCount++;
                                }
                            }
                            else
                            {
                                // LdapResponse
                                var resp = (LdapResponse)msg;
                                var resultCode = resp.ResultCode;

                                // Check for an embedded exception
                                if (resp.HasException())
                                {
                                    // Fake it, results in an exception when msg read
                                    resultCode = LdapException.ConnectError;
                                }

                                if (resultCode == LdapException.Referral && _cons.ReferralFollowing)
                                {
                                    // Following referrals
                                    _referralConn = _conn.ChaseReferral(_queue, _cons, resp, resp.Referrals, 0, false, _referralConn);
                                }
                                else if (resultCode != LdapException.Success)
                                {
                                    // Results in an exception when message read
                                    _entries.Add(resp);
                                    _entryCount++;
                                }

                                // We are done only when we have read all messages
                                // including those received from following referrals
                                var msgIDs = _queue.MessageIDs;
                                var controls = _cons.GetControls();
                                if (msgIDs.Length == 0 && (controls == null || controls.Length == 0))
                                {
                                    // Release referral exceptions
                                    _conn.ReleaseReferralConnections(_referralConn);
                                    return true; // search completed
                                }
                            }
                        }
                        else
                        {
                            // We get here if the connection timed out
                            // we have no responses, no message IDs and no exceptions
                            var e = new LdapException(null, LdapException.LdapTimeout, null);
                            _entries.Add(e);
                            break;
                        }
                    }
                    catch (LdapException e)
                    {
                        // Hand exception off to user
                        _entries.Add(e);
                    }
                }

                return false; // search not completed
            }
        }

        /// <summary>
        ///     Returns a count of the items in the search result.
        ///     Returns a count of the entries and exceptions remaining in the object.
        ///     If the search was submitted with a batch size greater than zero,
        ///     getCount reports the number of results received so far but not enumerated
        ///     with next().  If batch size equals zero, getCount reports the number of
        ///     items received, since the application thread blocks until all results are
        ///     received.
        /// </summary>
        /// <returns>
        ///     The number of items received but not retrieved by the application.
        /// </returns>
        public int Count
        {
            get
            {
                var qCount = _queue.MessageAgent.Count;
                return _entryCount - _entryIndex + _referenceCount - _referenceIndex + qCount;
            }
        }

        /// <summary>
        ///     Reports if there are more search results.
        /// </summary>
        /// <returns>
        ///     true if there are more search results.
        /// </returns>
        public bool HasMore()
        {
            var ret = false;
            if (_entryIndex < _entryCount || _referenceIndex < _referenceCount)
            {
                // we have data
                ret = true;
            }
            else if (_completed == false)
            {
                // reload the Vector by getting more results
                ResetVectors();
                ret = _entryIndex < _entryCount || _referenceIndex < _referenceCount;
            }

            return ret;
        }

        /// <summary>
        ///     Returns the next result as an LdapEntry.
        ///     If automatic referral following is disabled or if a referral
        ///     was not followed, next() will throw an LdapReferralException
        ///     when the referral is received.
        /// </summary>
        /// <returns>
        ///     The next search result as an LdapEntry.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        /// <exception>
        ///     LdapReferralException A referral was received and not
        ///     followed.
        /// </exception>
        public LdapEntry Next()
        {
            if (_completed && _entryIndex >= _entryCount && _referenceIndex >= _referenceCount)
            {
                throw new ArgumentOutOfRangeException("LdapSearchResults.Next() no more results");
            }

            // Check if the enumeration is empty and must be reloaded
            ResetVectors();

            object element = null;

            // Check for Search References & deliver to app as they come in
            // We only get here if not following referrals/references
            if (_referenceIndex < _referenceCount)
            {
                var refs = _references[_referenceIndex++];
                var rex = new LdapReferralException(ExceptionMessages.ReferenceNofollow);
                rex.SetReferrals(refs);
                throw rex;
            }

            if (_entryIndex < _entryCount)
            {
                // Check for Search Entries and the Search Result
                element = _entries[_entryIndex++];
                if (element is LdapResponse)
                {
                    // Search done w/bad status
                    if (((LdapResponse)element).HasException())
                    {
                        var lr = (LdapResponse)element;
                        var ri = lr.ActiveReferral;

                        if (ri != null)
                        {
                            // Error attempting to follow a search continuation reference
                            var rex = new LdapReferralException(ExceptionMessages.ReferenceError, lr.Exception);
                            rex.SetReferrals(ri.ReferralList);
                            rex.FailedReferral = ri.ReferralUrl.ToString();
                            throw rex;
                        }
                    }

                    // Throw an exception if not success
                    ((LdapResponse)element).ChkResultCode();
                }
                else if (element is LdapException)
                {
                    throw (LdapException)element;
                }
            }
            else
            {
                // If not a Search Entry, Search Result, or search continuation
                // we are very confused.
                // LdapSearchResults.next(): No entry found & request is not complete
                throw new LdapException(ExceptionMessages.ReferralLocal, new object[] { "next" },
                    LdapException.LocalError, null);
            }

            return (LdapEntry)element;
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        /// <filterpriority>2.</filterpriority>
        public IEnumerator<LdapEntry> GetEnumerator()
        {
            while (HasMore())
            {
                yield return Next();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /*
        * If both of the vectors are empty, get more data for them.
        */
        private void ResetVectors()
        {
            // If we're done, no further checking needed
            if (_completed)
            {
                return;
            }

            // Checks if we have run out of references
            if (_referenceIndex != 0 && _referenceIndex >= _referenceCount)
            {
                _references.Clear();
                _referenceCount = 0;
                _referenceIndex = 0;
            }

            // Checks if we have run out of entries
            if (_entryIndex != 0 && _entryIndex >= _entryCount)
            {
                _entries.RemoveRange(0, _entries.Count);
                _entryCount = 0;
                _entryIndex = 0;
            }

            // If no data at all, must reload enumeration
            if (_referenceIndex == 0 && _referenceCount == 0 && _entryIndex == 0 && _entryCount == 0)
            {
                _completed = BatchOfResults;
            }
        }

        /// <summary> Cancels the search request and clears the message and enumeration.</summary>
        /*package*/
        internal void Abandon()
        {
            // first, remove message ID and timer and any responses in the queue
            _queue.MessageAgent.AbandonAll();

            // next, clear out enumeration
            ResetVectors();
            _completed = true;
        }

        /// <summary>
        /// Get referral connections.
        /// </summary>
        public ArrayList GetReferralConnections()
        {
            return _referralConn;
        }
    }
}
