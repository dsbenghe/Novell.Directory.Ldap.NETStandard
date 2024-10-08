﻿/******************************************************************************
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
using System;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Encapsulates the response returned by an Ldap server on an
    ///     asynchronous extended operation request.  It extends LdapResponse.
    ///     The response can contain the OID of the extension, an octet string
    ///     with the operation's data, both, or neither.
    /// </summary>
    public class LdapExtendedResponse : LdapResponse
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapExtendedResponse>();

        /// <summary>
        ///     Creates an LdapExtendedResponse object which encapsulates
        ///     a server response to an asynchronous extended operation request.
        /// </summary>
        /// <param name="message">
        ///     The RfcLdapMessage to convert to an
        ///     LdapExtendedResponse object.
        /// </param>
        public LdapExtendedResponse(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        ///     Returns the message identifier of the response.
        /// </summary>
        /// <returns>
        ///     OID of the response.
        /// </returns>
        public string Id
        {
            get
            {
                var respOid = ((RfcExtendedResponse)Message.Response).ResponseName;
                return respOid?.StringValue();
            }
        }

        public static RespExtensionSet<LdapExtendedResponse> RegisteredResponses { get; } = new ();

        /// <summary>
        ///     Returns the value part of the response in raw bytes.
        /// </summary>
        /// <returns>
        ///     The value of the response.
        /// </returns>
        public byte[] Value
        {
            get
            {
                var tempString = ((RfcExtendedResponse)Message.Response).Response;
                return tempString?.ByteValue();
            }
        }

        /// <summary>
        ///     Registers a class to be instantiated on receipt of a extendedresponse
        ///     with the given OID.
        ///     <p>
        ///         Any previous registration for the OID is overridden. The
        ///         extendedResponseClass object MUST be an extension of
        ///         LDAPExtendedResponse.
        ///     </p>
        /// </summary>
        /// <param name="oid">
        ///     The object identifier of the control.
        /// </param>
        /// <param name="responseFactory">
        ///     A delegate which can instantiate a <see cref="LdapExtendedResponse"/>.
        /// </param>
        /// <typeparam name="TExtendedResponseClass">
        ///     A class extending <see cref="LdapExtendedResponse"/>.
        /// </typeparam>
        public static void Register<TExtendedResponseClass>(string oid, Func<RfcLdapMessage, TExtendedResponseClass> responseFactory)
            where TExtendedResponseClass : LdapExtendedResponse
        {
            RegisteredResponses.RegisterResponseExtension(oid, responseFactory);
        }
    }
}
