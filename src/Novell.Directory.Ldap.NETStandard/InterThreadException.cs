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

using System;

namespace Novell.Directory.Ldap
{
    public class InterThreadException : LdapException
    {
        private readonly Message _request;

        /// <summary>
        ///     Constructs a InterThreadException with its associated message.
        /// </summary>
        /// <param name="message">
        ///     The text providign additional error information.
        /// </param>
        /// <param name="resultCode">
        ///     The error result code.
        /// </param>
        /// <param name="request">
        ///     The Message class associated with this exception.
        /// </param>
        internal InterThreadException(string message, object[] arguments, int resultCode, Exception rootException,
            Message request)
            : base(message, arguments, resultCode, null, rootException)
        {
            _request = request;
        }

        /// <summary>
        ///     Returns the message ID of this message request.
        /// </summary>
        /// <returns>
        ///     the message ID.  Returns -1 if no message
        ///     is associated with this exception.
        /// </returns>
        internal int MessageId
        {
            get
            {
                if (_request == null)
                {
                    return -1;
                }

                return _request.MessageId;
            }
        }

        /// <summary>
        ///     Returns the message type expected as a reply to
        ///     the message associated with this message's request type.
        /// </summary>
        /// <returns>
        ///     the message type of the expected reply.  Returns -1
        ///     if no reply expected.
        /// </returns>
        internal int ReplyType
        {
            get
            {
                if (_request == null)
                {
                    return -1;
                }

                var reqType = _request.MessageType;
                var responseType = -1;
                switch (reqType)
                {
                    case LdapMessage.BindRequest:
                        responseType = LdapMessage.BindResponse;
                        break;

                    case LdapMessage.UnbindRequest:
                        responseType = -1;
                        break;

                    case LdapMessage.SearchRequest:
                        responseType = LdapMessage.SearchResult;
                        break;

                    case LdapMessage.ModifyRequest:
                        responseType = LdapMessage.ModifyResponse;
                        break;

                    case LdapMessage.AddRequest:
                        responseType = LdapMessage.AddResponse;
                        break;

                    case LdapMessage.DelRequest:
                        responseType = LdapMessage.DelResponse;
                        break;

                    case LdapMessage.ModifyRdnRequest:
                        responseType = LdapMessage.ModifyRdnResponse;
                        break;

                    case LdapMessage.CompareRequest:
                        responseType = LdapMessage.CompareResponse;
                        break;

                    case LdapMessage.AbandonRequest:
                        responseType = -1;
                        break;

                    case LdapMessage.ExtendedRequest:
                        responseType = LdapMessage.ExtendedResponse;
                        break;
                }

                return responseType;
            }
        }
    }
}
