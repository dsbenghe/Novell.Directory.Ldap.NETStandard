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
    ///     Represents an LDAM MOdify DN Request.
    ///     <pre>
    ///         ModifyDNRequest ::= [APPLICATION 12] SEQUENCE {
    ///         entry           LdapDN,
    ///         newrdn          RelativeLdapDN,
    ///         deleteoldrdn    BOOLEAN,
    ///         newSuperior     [0] LdapDN OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcModifyDnRequest : Asn1Sequence, IRfcRequest
    {
        // *************************************************************************
        // Constructors for ModifyDNRequest
        // *************************************************************************

        /// <summary> </summary>
        public RfcModifyDnRequest(RfcLdapDn entry, RfcRelativeLdapDn newrdn, Asn1Boolean deleteoldrdn)
            : this(entry, newrdn, deleteoldrdn, null)
        {
        }

        /// <summary> </summary>
        public RfcModifyDnRequest(RfcLdapDn entry, RfcRelativeLdapDn newrdn, Asn1Boolean deleteoldrdn,
            RfcLdapDn newSuperior)
            : base(4)
        {
            Add(entry);
            Add(newrdn);
            Add(deleteoldrdn);
            if (newSuperior != null)
            {
                newSuperior.SetIdentifier(new Asn1Identifier(Asn1Identifier.Context, false, 0));
                Add(newSuperior);
            }
        }

        /// <summary>
        ///     Constructs a new Delete Request copying from the ArrayList of
        ///     an existing request.
        /// </summary>
        internal RfcModifyDnRequest(Asn1Object[] origRequest, string baseRenamed)
            : base(origRequest, origRequest.Length)
        {
            // Replace the base if specified, otherwise keep original base
            if (baseRenamed != null)
            {
                set_Renamed(0, new RfcLdapDn(baseRenamed));
            }
        }

        public IRfcRequest DupRequest(string baseRenamed, string filter, bool request)
        {
            return new RfcModifyDnRequest(ToArray(), baseRenamed);
        }

        public string GetRequestDn()
        {
            return ((RfcLdapDn)get_Renamed(0)).StringValue();
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 12.
        ///     </pre>
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, true, LdapMessage.ModifyRdnRequest);
        }
    }
}
