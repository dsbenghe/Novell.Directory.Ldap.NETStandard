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

using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents a queue of incoming asynchronous messages from the server.
    ///     It is the common interface for {@link LdapResponseQueue} and
    ///     {@link LdapSearchQueue}.
    /// </summary>
    public abstract class LdapMessageQueue : IDebugIdentifier
    {
        public virtual DebugId DebugId { get; } = DebugId.ForType<LdapMessageQueue>();

        /// <summary> The message agent object associated with this queue.</summary>
        private readonly MessageAgent _agent;

        /// <summary>
        ///     Constructs a response queue using the specified message agent.
        /// </summary>
        /// <param name="agent">
        ///     The message agent to associate with this connection.
        /// </param>
        internal LdapMessageQueue(string myname, MessageAgent agent)
        {
            _agent = agent;
        }

        /// <summary>
        ///     Returns the internal client message agent.
        /// </summary>
        /// <returns>
        ///     The internal client message agent.
        /// </returns>
        internal MessageAgent MessageAgent => _agent;

        /// <summary>
        ///     Returns the message IDs for all outstanding requests. These are requests
        ///     for which a response has not been received from the server or which
        ///     still have messages to be retrieved with getResponse.
        ///     The last ID in the array is the messageID of the last submitted
        ///     request.
        /// </summary>
        /// <returns>
        ///     The message IDs for all outstanding requests.
        /// </returns>
        public int[] MessageIDs => _agent.MessageIDs;

        /// <summary>
        ///     Returns the response from an Ldap request.
        ///     The getResponse method blocks until a response is available, or until
        ///     all operations associated with the object have completed or been
        ///     canceled, and then returns the response.
        ///     The application is responsible to determine the type of message
        ///     returned.
        /// </summary>
        /// <returns>
        ///     The response.
        /// </returns>
        /// <seealso cref="LdapResponse">
        /// </seealso>
        /// <seealso cref="LdapSearchResult">
        /// </seealso>
        /// <seealso cref="LdapSearchResultReference">
        /// </seealso>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public LdapMessage GetResponse()
        {
            return GetResponse(null);
        }

        /// <summary>
        ///     Returns the response from an Ldap request for a particular message ID.
        ///     The getResponse method blocks until a response is available
        ///     for a particular message ID, or until all operations associated
        ///     with the object have completed or been canceled, and
        ///     then returns the response.  If there is no outstanding operation for
        ///     the message ID (or if it is zero or a negative number),
        ///     IllegalArgumentException is thrown.
        ///     The application is responsible to determine the type of message
        ///     returned.
        /// </summary>
        /// <param name="msgid">
        ///     query for responses for a specific message request.
        /// </param>
        /// <returns>
        ///     The response from the server.
        /// </returns>
        /// <seealso cref="LdapResponse">
        /// </seealso>
        /// <seealso cref="LdapSearchResult">
        /// </seealso>
        /// <seealso cref="LdapSearchResultReference">
        /// </seealso>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public LdapMessage GetResponse(int msgid)
        {
            return GetResponse((int?)msgid);
        }

        /// <summary>
        ///     Private implementation of getResponse.
        ///     Has an Integer object as a parameter so we can distinguish
        ///     the null and the message number case.
        /// </summary>
        private LdapMessage GetResponse(int? msgid)
        {
            object resp;
            LdapMessage response;
            if ((resp = _agent.GetLdapMessage(msgid)) == null)
            {
                // blocks
                return null; // no messages from this agent
            }

            // Local error occurred, contains a LocalException
            if (resp is LdapResponse ldapResponse)
            {
                return ldapResponse;
            }

            // Normal message handling
            var message = (RfcLdapMessage)resp;
            switch (message.Type)
            {
                case LdapMessage.SearchResponse:
                    response = new LdapSearchResult(message);
                    break;

                case LdapMessage.SearchResultReference:
                    response = new LdapSearchResultReference(message);
                    break;

                case LdapMessage.ExtendedResponse:
                    response = ExtResponseFactory.ConvertToExtendedResponse(message);
                    break;

                case LdapMessage.IntermediateResponse:
                    response = IntermediateResponseFactory.ConvertToIntermediateResponse(message);
                    break;

                default:
                    response = new LdapResponse(message);
                    break;
            }

            return response;
        }

        /// <summary>
        ///     Reports true if any response has been received from the server and not
        ///     yet retrieved with getResponse.  If getResponse has been used to
        ///     retrieve all messages received to this point, then isResponseReceived
        ///     returns false.
        /// </summary>
        /// <returns>
        ///     true if a response is available to be retrieved via getResponse,
        ///     otherwise false.
        /// </returns>
        public bool IsResponseReceived()
        {
            return _agent.IsResponseReceived();
        }

        /// <summary>
        ///     Reports true if a response has been received from the server for
        ///     a particular message ID but not yet retrieved with getResponse.  If
        ///     there is no outstanding operation for the message ID (or if it is
        ///     zero or a negative number), IllegalArgumentException is thrown.
        /// </summary>
        /// <param name="msgid">
        ///     A particular message ID to query for available responses.
        /// </param>
        /// <returns>
        ///     true if a response is available to be retrieved via getResponse
        ///     for the specified message ID, otherwise false.
        /// </returns>
        public bool IsResponseReceived(int msgid)
        {
            return _agent.IsResponseReceived(msgid);
        }

        /// <summary>
        ///     Reports true if all results have been received for a particular
        ///     message id.
        ///     If the search result done has been received from the server for the
        ///     message id, it reports true.  There may still be messages waiting to be
        ///     retrieved by the applcation with getResponse.
        ///     @throws IllegalArgumentException if there is no outstanding operation
        ///     for the message ID,.
        /// </summary>
        public bool IsComplete(int msgid)
        {
            return _agent.IsComplete(msgid);
        }
    }
}
