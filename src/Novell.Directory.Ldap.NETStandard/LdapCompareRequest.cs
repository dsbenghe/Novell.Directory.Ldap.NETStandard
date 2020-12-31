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

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents an Ldap Compare Request.
    /// </summary>
    /// <seealso cref="LdapConnection.SendRequest">
    /// </seealso>
    /*
     *       CompareRequest ::= [APPLICATION 14] SEQUENCE {
     *               entry           LdapDN,
     *               ava             AttributeValueAssertion }
     */
    public class LdapCompareRequest : LdapMessage
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapCompareRequest>();

        /// <summary>
        ///     Constructs an LdapCompareRequest Object.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry containing an
        ///     attribute to compare.
        /// </param>
        /// <param name="name">
        ///     The name of the attribute to compare.
        /// </param>
        /// <param name="value">
        ///     The value of the attribute to compare.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the compare request,
        ///     or null if none.
        /// </param>
        public LdapCompareRequest(string dn, string name, byte[] valueRenamed, LdapControl[] cont)
            : base(
                CompareRequest,
                new RfcCompareRequest(
                    new RfcLdapDn(dn),
                    new RfcAttributeValueAssertion(
                        new RfcAttributeDescription(name),
                        new RfcAssertionValue(valueRenamed))), cont)
        {
        }

        /// <summary>
        ///     Returns the LdapAttribute associated with this request.
        /// </summary>
        /// <returns>
        ///     the LdapAttribute.
        /// </returns>
        public string AttributeDescription
        {
            get
            {
                var req = (RfcCompareRequest)Asn1Object.GetRequest();
                return req.AttributeValueAssertion.AttributeDescription;
            }
        }

        /// <summary>
        ///     Returns the LdapAttribute associated with this request.
        /// </summary>
        /// <returns>
        ///     the LdapAttribute.
        /// </returns>
        public byte[] AssertionValue
        {
            get
            {
                var req = (RfcCompareRequest)Asn1Object.GetRequest();
                return req.AttributeValueAssertion.AssertionValue;
            }
        }

        /// <summary>
        ///     Returns of the dn of the entry to compare in the directory.
        /// </summary>
        /// <returns>
        ///     the dn of the entry to compare.
        /// </returns>
        public string Dn => Asn1Object.RequestDn;
    }
}
