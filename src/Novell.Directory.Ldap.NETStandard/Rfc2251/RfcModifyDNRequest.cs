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
// Novell.Directory.Ldap.Rfc2251.RfcModifyDNRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.NETStandard.Asn1;

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
    public class RfcModifyDNRequest : Asn1Sequence, IRfcRequest
    {
        //*************************************************************************
        // Constructors for ModifyDNRequest
        //*************************************************************************

        /// <summary> </summary>
        public RfcModifyDNRequest(RfcLdapDN entry, RfcRelativeLdapDN newrdn, Asn1Boolean deleteoldrdn)
            : this(entry, newrdn, deleteoldrdn, null)
        {
        }

        /// <summary> </summary>
        public RfcModifyDNRequest(RfcLdapDN entry, RfcRelativeLdapDN newrdn, Asn1Boolean deleteoldrdn,
            RfcLdapDN newSuperior) : base(4)
        {
            Add(entry);
            Add(newrdn);
            Add(deleteoldrdn);
            if (newSuperior != null)
            {
                newSuperior.Identifier = new Asn1Identifier(TagClass.CONTEXT, false, 0);
                Add(newSuperior);
            }
        }

        /// <summary>
        ///     Constructs a new Delete Request copying from the ArrayList of
        ///     an existing request.
        /// </summary>
        internal RfcModifyDNRequest(Asn1Object[] origRequest, string base_Renamed)
            : base(origRequest, origRequest.Length)
        {
            // Replace the base if specified, otherwise keep original base
            if (base_Renamed != null)
            {
                this[0] = new RfcLdapDN(base_Renamed);
            }
        }

        //*************************************************************************
        // Accessors
        //*************************************************************************

        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 12.
        ///     </pre>
        /// </summary>
        public override Asn1Identifier Identifier
        {
            set => base.Identifier = value;
            get => new Asn1Identifier(TagClass.APPLICATION, true, LdapMessage.MODIFY_RDN_REQUEST);
        }

        public IRfcRequest DupRequest(string @base, string filter, bool request) => new RfcModifyDNRequest(ToArray(), @base);

        public string RequestDN => (this[0] as RfcLdapDN).StringValue;
    }
}