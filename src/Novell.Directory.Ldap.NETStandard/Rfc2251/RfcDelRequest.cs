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

using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Delete Request.
    ///     <pre>
    ///         DelRequest ::= [APPLICATION 10] LdapDN
    ///     </pre>
    /// </summary>
    public class RfcDelRequest : RfcLdapDn, IRfcRequest
    {
        // *************************************************************************
        // Constructor for DelRequest
        // *************************************************************************

        /// <summary>
        ///     Constructs an Ldapv3 delete request protocol operation.
        /// </summary>
        /// <param name="dn">
        ///     The Distinguished Name of the entry to delete.
        /// </param>
        public RfcDelRequest(string dn)
            : base(dn)
        {
        }

        /// <summary>
        ///     Constructs an Ldapv3 delete request protocol operation.
        /// </summary>
        /// <param name="dn">
        ///     The Distinguished Name of the entry to delete.
        /// </param>
        public RfcDelRequest(byte[] dn)
            : base(dn)
        {
        }

        public IRfcRequest DupRequest(string baseRenamed, string filter, bool request)
        {
            if (baseRenamed == null)
            {
                return new RfcDelRequest(ByteValue());
            }

            return new RfcDelRequest(baseRenamed);
        }

        public string GetRequestDn()
        {
            return StringValue();
        }

        /// <summary>
        ///     Override getIdentifier() to return the appropriate application-wide id
        ///     representing this delete request. The getIdentifier() method is called
        ///     when this object is encoded.
        ///     Identifier = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 10.
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, false, LdapMessage.DelRequest);
        }
    }
}
