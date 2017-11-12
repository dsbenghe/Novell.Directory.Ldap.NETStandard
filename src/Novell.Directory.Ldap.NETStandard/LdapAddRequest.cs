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
// Novell.Directory.Ldap.LdapAddRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents an Ldap Add Request.
    /// </summary>
    /// <seealso cref="LdapConnection.SendRequest">
    /// </seealso>
    /*
     *       AddRequest ::= [APPLICATION 8] SEQUENCE {
     *               entry           LdapDN,
     *               attributes      AttributeList }
     */
    public class LdapAddRequest : LdapMessage
    {
        /// <summary>
        ///     Constructs an LdapEntry that represents the add request
        /// </summary>
        /// <returns>
        ///     an LdapEntry that represents the add request.
        /// </returns>
        public virtual LdapEntry Entry
        {
            get
            {
                RfcAddRequest addreq = Asn1Object.Request as RfcAddRequest;

                var attrs = new LdapAttributeSet();

                // Build the list of attributes
                var seqArray = addreq.Attributes;
                foreach(RfcAttributeTypeAndValues seq in addreq.Attributes)
                {
                    var attr = new LdapAttribute((seq[0] as Asn1OctetString).StringValue);

                    // Add the values to the attribute
                    var set_Renamed = (Asn1SetOf)seq[1];
                    foreach (var item in set_Renamed)
                    {
                        attr.AddValue((item as Asn1OctetString).ByteValue);
                    }
                    attrs.Add(attr);
                }

                return new LdapEntry(Asn1Object.RequestDN, attrs);
            }
        }

        /// <summary>
        ///     Constructs a request to add an entry to the directory.
        /// </summary>
        /// <param name="entry">
        ///     The LdapEntry to add to the directory.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the add request,
        ///     or null if none.
        /// </param>
        public LdapAddRequest(LdapEntry entry, LdapControl[] cont)
            : base(ADD_REQUEST, new RfcAddRequest(new RfcLdapDN(entry.DN), MakeRfcAttrList(entry)), cont)
        {
        }

        /// <summary>
        ///     Build the attribuite list from an LdapEntry.
        /// </summary>
        /// <param name="entry">
        ///     The LdapEntry associated with this add request.
        /// </param>
        private static RfcAttributeList MakeRfcAttrList(LdapEntry entry)
        {
            // convert Java-API LdapEntry to RFC2251 AttributeList
            var attrSet = entry.AttributeSet;
            var attrList = new RfcAttributeList(attrSet.Count);
            foreach(LdapAttribute item in attrSet)
            {
                var vals = new Asn1SetOf(item.Size);
                foreach(byte[] info in item.ByteValues)
                {
                    vals.Add(new RfcAttributeValue(info));
                }
                attrList.Add(new RfcAttributeTypeAndValues(new RfcAttributeDescription(item.Name), vals));
            }
            return attrList;
        }

        /// <summary>
        ///     Return an Asn1 representation of this add request.
        ///     #return an Asn1 representation of this object.
        /// </summary>
        public override string ToString() => Asn1Object.ToString();
    }
}