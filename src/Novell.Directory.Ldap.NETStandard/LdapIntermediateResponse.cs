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
        private static readonly RespExtensionSet RegisteredResponses = new RespExtensionSet();

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

        /**
         * Registers a class to be instantiated on receipt of a extendedresponse
         * with the given OID.
         *
         * <p>Any previous registration for the OID is overridden. The
         *  extendedResponseClass object MUST be an extension of
         *  LdapIntermediateResponse. </p>
         *
         * @param oid            The object identifier of the control.
         * <br><br>
         * @param extendedResponseClass  A class which can instantiate an
         *                                LdapIntermediateResponse.
         */
        public static void Register(string oid, Type extendedResponseClass)
        {
            RegisteredResponses.RegisterResponseExtension(oid, extendedResponseClass);
        }

        public static RespExtensionSet GetRegisteredResponses()
        {
            return RegisteredResponses;
        }

        /**
         * Returns the message identifier of the response.
         *
         * @return OID of the response.
         */
        public string GetId()
        {
            var respOid =
                ((RfcIntermediateResponse)Message.Response).GetResponseName();
            if (respOid == null)
            {
                return null;
            }

            return respOid.StringValue();
        }

        /**
         * Returns the value part of the response in raw bytes.
         *
         * @return The value of the response.
         */
        public byte[] GetValue()
        {
            var tempString =
                ((RfcIntermediateResponse)Message.Response).GetResponse();
            if (tempString == null)
            {
                return null;
            }

            return tempString.ByteValue();
        }
    }
}
