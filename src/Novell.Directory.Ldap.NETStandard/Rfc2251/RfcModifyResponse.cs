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

using Novell.Directory.Ldap.Asn1;
using System.IO;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Modify Response.
    ///     <pre>
    ///         ModifyResponse ::= [APPLICATION 7] LdapResult
    ///     </pre>
    /// </summary>
    public class RfcModifyResponse : RfcLdapResult
    {
        // *************************************************************************
        // Constructor for ModifyResponse
        // *************************************************************************

        /// <summary>
        ///     The only time a client will create a ModifyResponse is when it is
        ///     decoding it from an InputStream.
        /// </summary>
        public RfcModifyResponse(IAsn1Decoder dec, Stream input, int len)
            : base(dec, input, len)
        {
        }

        /// <summary>
        ///     Constructs an RfcModifyResponse from parameters.
        /// </summary>
        /// <param name="resultCode">
        ///     the result code of the operation.
        /// </param>
        /// <param name="matchedDn">
        ///     the matched DN returned from the server.
        /// </param>
        /// <param name="errorMessage">
        ///     the diagnostic message returned from the server.
        /// </param>
        /// <param name="referral">
        ///     the referral(s) returned by the server.
        /// </param>
        public RfcModifyResponse(Asn1Enumerated resultCode, RfcLdapDn matchedDn, RfcLdapString errorMessage,
            RfcReferral referral)
            : base(resultCode, matchedDn, errorMessage, referral)
        {
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, true, LdapMessage.ModifyResponse);
        }
    }
}
