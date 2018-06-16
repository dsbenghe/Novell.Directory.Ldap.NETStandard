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
// Novell.Directory.Ldap.Extensions.GetBindDNResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Extensions
{
    /// <summary>
    ///     Retrieves the identity from an GetBindDNResponse object.
    ///     An object in this class is generated from an LdapExtendedResponse object
    ///     using the ExtendedResponseFactory class.
    ///     The GetBindDNResponse extension uses the following OID:
    ///     2.16.840.1.113719.1.27.100.32
    /// </summary>
    public sealed class GetBindDnResponse : LdapExtendedResponse
    {
        /// <summary>
        ///     Returns the identity of the object.
        /// </summary>
        /// <returns>
        ///     A string value specifying the bind dn returned by the server.
        /// </returns>
        public string Identity => _identity;

        // Identity returned by the server
        private readonly string _identity;

        /// <summary>
        ///     Constructs an object from the responseValue which contains the bind dn.
        ///     The constructor parses the responseValue which has the following
        ///     format:
        ///     responseValue ::=
        ///     identity   OCTET STRING
        /// </summary>
        /// <exception>
        ///     IOException The return value could not be decoded.
        /// </exception>
        public GetBindDnResponse(RfcLdapMessage rfcMessage) : base(rfcMessage)
        {
            if (ResultCode == LdapException.Success)
            {
                // parse the contents of the reply
                var returnedValue = Value;
                if (returnedValue == null)
                    throw new IOException("No returned value");

                // Create a decoder object
                var decoder = new LberDecoder();
                if (decoder == null)
                    throw new IOException("Decoding error");

                // The only parameter returned should be an octet string
                var asn1Identity = (Asn1OctetString) decoder.Decode(returnedValue);
                if (asn1Identity == null)
                    throw new IOException("Decoding error");

                // Convert to normal string object
                _identity = asn1Identity.StringValue();
                if ((object) _identity == null)
                    throw new IOException("Decoding error");
            }
            else
            {
                _identity = "";
            }
        }
    }
}