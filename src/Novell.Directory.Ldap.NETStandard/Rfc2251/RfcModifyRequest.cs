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
    ///     Represents an Ldap Modify Request.
    ///     <pre>
    ///         ModifyRequest ::= [APPLICATION 6] SEQUENCE {
    ///         object          LdapDN,
    ///         modification    SEQUENCE OF SEQUENCE {
    ///         operation       ENUMERATED {
    ///         add     (0),
    ///         delete  (1),
    ///         replace (2) },
    ///         modification    AttributeTypeAndValues } }
    ///     </pre>
    /// </summary>
    public class RfcModifyRequest : Asn1Sequence, IRfcRequest
    {
        // *************************************************************************
        // Constructor for ModifyRequest
        // *************************************************************************

        /// <summary> </summary>
        public RfcModifyRequest(RfcLdapDn objectRenamed, Asn1SequenceOf modification)
            : base(2)
        {
            Add(objectRenamed);
            Add(modification);
        }

        /// <summary>
        ///     Constructs a new Modify Request copying from the ArrayList of
        ///     an existing request.
        /// </summary>
        internal RfcModifyRequest(Asn1Object[] origRequest, string baseRenamed)
            : base(origRequest, origRequest.Length)
        {
            // Replace the base if specified, otherwise keep original base
            if (baseRenamed != null)
            {
                set_Renamed(0, new RfcLdapDn(baseRenamed));
            }
        }

        /// <summary>
        ///     Return the Modifications for this request.
        /// </summary>
        /// <returns>
        ///     the modifications for this request.
        /// </returns>
        public Asn1SequenceOf Modifications => (Asn1SequenceOf)get_Renamed(1);

        public IRfcRequest DupRequest(string baseRenamed, string filter, bool request)
        {
            return new RfcModifyRequest(ToArray(), baseRenamed);
        }

        /// <summary>
        ///     Return the String value of the DN associated with this request.
        /// </summary>
        /// <returns>
        ///     the DN for this request.
        /// </returns>
        public string GetRequestDn()
        {
            return ((RfcLdapDn)get_Renamed(0)).StringValue();
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, true, LdapMessage.ModifyRequest);
        }
    }
}
