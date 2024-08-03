#nullable enable
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
using System;

namespace Novell.Directory.Ldap
{
    /**
     *
     * Encapsulates the response returned by an LDAP server on an
     * asynchronous extended operation request.  It extends LdapResponse.
     *
     * The response can contain the OID of the extension, an octet string
     * with the operation's data, both, or neither.
     */
    public class LdapIntermediateResponse : LdapResponse
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapIntermediateResponse>();
        private static readonly RespExtensionSet<LdapIntermediateResponse> RegisteredResponses = new ();

        /**
         * Creates an LdapIntermediateResponse object which encapsulates
         * a server response to an asynchronous extended operation request.
         *
         * @param message  The RfcLdapMessage to convert to an
         * LdapIntermediateResponse object.
         */
        public LdapIntermediateResponse(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        /// Registers a class to be instantiated on receipt of a extendedresponse
        /// with the given OID.
        /// </summary>
        /// <remarks>
        /// Any previous registration for the OID is overridden. The
        /// extendedResponseClass object MUST be an extension of
        /// LdapIntermediateResponse.
        /// </remarks>
        /// <param name="oid">The object identifier of the control.</param>
        /// <param name="responseFactory">A factory to translate <see cref="RfcLdapMessage"/> into <typeparamref name="TIntermediateResponseClass"/>.</param>
        /// <typeparam name="TIntermediateResponseClass">A class extending <see cref="LdapIntermediateResponse"/>, that will be used for <paramref name="oid"/>.</typeparam>
        public static void Register<TIntermediateResponseClass>(string oid, Func<RfcLdapMessage, TIntermediateResponseClass> responseFactory)
            where TIntermediateResponseClass : LdapIntermediateResponse
        {
            RegisteredResponses.RegisterResponseExtension(oid, responseFactory);
        }

        public static RespExtensionSet<LdapIntermediateResponse> GetRegisteredResponses()
        {
            return RegisteredResponses;
        }

        /// <summary>Returns the message identifier of the response.</summary>
        /// <returns>OID of the response.</returns>
        public string? GetId()
        {
            var respOid =
                ((RfcIntermediateResponse)Message.Response).GetResponseName();

            return respOid?.StringValue();
        }

        /// <summary>Returns the value part of the response in raw bytes.</summary>
        /// <returns>The value of the response.</returns>
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public byte[]? GetValue()
#pragma warning restore SA1011
        {
            var tempString =
                ((RfcIntermediateResponse)Message.Response).GetResponse();

            return tempString?.ByteValue();
        }
    }
}
