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
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents an Ldap ModifyDN request.
    /// </summary>
    /// <seealso cref="LdapConnection.SendRequest">
    /// </seealso>
    /*
     *       ModifyDNRequest ::= [APPLICATION 12] SEQUENCE {
     *               entry           LdapDN,
     *               newrdn          RelativeLdapDN,
     *               deleteoldrdn    BOOLEAN,
     *               newSuperior     [0] LdapDN OPTIONAL }
     */
    public class LdapModifyDnRequest : LdapMessage
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapModifyDnRequest>();

        /// <summary>
        ///     Constructs a ModifyDN (rename) Request.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="newParentdn">
        ///     The distinguished name of an existing entry which
        ///     is to be the new parent of the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the modifyDN request,
        ///     or null if none.
        /// </param>
        public LdapModifyDnRequest(string dn, string newRdn, string newParentdn, bool deleteOldRdn, LdapControl[] cont)
            : base(
                ModifyRdnRequest,
                new RfcModifyDnRequest(new RfcLdapDn(dn), new RfcRelativeLdapDn(newRdn), new Asn1Boolean(deleteOldRdn),
                    newParentdn != null ? new RfcLdapDn(newParentdn) : null), cont)
        {
        }

        /// <summary>
        ///     Returns the dn of the entry to rename or move in the directory.
        /// </summary>
        /// <returns>
        ///     the dn of the entry to rename or move.
        /// </returns>
        public string Dn => Asn1Object.RequestDn;

        /// <summary>
        ///     Returns the newRDN of the entry to rename or move in the directory.
        /// </summary>
        /// <returns>
        ///     the newRDN of the entry to rename or move.
        /// </returns>
        public string NewRdn
        {
            get
            {
                // Get the RFC request object for this request
                var req = (RfcModifyDnRequest)Asn1Object.GetRequest();
                var relDn = (RfcRelativeLdapDn)req.ToArray()[1];
                return relDn.StringValue();
            }
        }

        /// <summary>
        ///     Returns the DeleteOldRDN flag that applies to the entry to rename or
        ///     move in the directory.
        /// </summary>
        /// <returns>
        ///     the DeleteOldRDN flag for the entry to rename or move.
        /// </returns>
        public bool DeleteOldRdn
        {
            get
            {
                // Get the RFC request object for this request
                var req = (RfcModifyDnRequest)Asn1Object.GetRequest();
                var delOld = (Asn1Boolean)req.ToArray()[2];
                return delOld.BooleanValue();
            }
        }

        /// <summary>
        ///     Returns the ParentDN for the entry move in the directory.
        /// </summary>
        /// <returns>
        ///     the ParentDN for the entry to move, or <dd>null</dd>
        ///     if the request is not a move.
        /// </returns>
        public string ParentDn
        {
            get
            {
                // Get the RFC request object for this request
                var req = (RfcModifyDnRequest)Asn1Object.GetRequest();
                var seq = req.ToArray();
                if (seq.Length < 4 || seq[3] == null)
                {
                    return null;
                }

                var parentDn = (RfcLdapDn)req.ToArray()[3];
                return parentDn.StringValue();
            }
        }

        /// <summary>
        ///     Return an Asn1 representation of this mod DN request
        ///     #return an Asn1 representation of this object.
        /// </summary>
        public override string ToString()
        {
            return Asn1Object.ToString();
        }
    }
}
