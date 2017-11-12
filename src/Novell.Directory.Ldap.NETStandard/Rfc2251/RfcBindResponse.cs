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
// Novell.Directory.Ldap.Rfc2251.RfcBindResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.NETStandard.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents and Ldap Bind Response.
    ///     <pre>
    ///         BindResponse ::= [APPLICATION 1] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         serverSaslCreds    [7] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcBindResponse : Asn1Sequence, IRfcResponse
    {
        /// <summary>
        ///     Returns the OPTIONAL serverSaslCreds of a BindResponse if it exists
        ///     otherwise null.
        /// </summary>
        public virtual Asn1OctetString ServerSaslCreds
        {
            get
            {
                if (Count == 5)
                    return (this[4] as Asn1Tagged).TaggedValue as Asn1OctetString;
                else if (Count == 4 && this[3] is Asn1Tagged tag)
                    return tag.TaggedValue as Asn1OctetString;
                return null;
            }
        }

        //*************************************************************************
        // Constructors for BindResponse
        //*************************************************************************

        /// <summary>
        ///     The only time a client will create a BindResponse is when it is
        ///     decoding it from an InputStream
        ///     Note: If serverSaslCreds is included in the BindResponse, it does not
        ///     need to be decoded since it is already an OCTET STRING.
        /// </summary>
        public RfcBindResponse(IAsn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Count > 3)
            {
                var obj = this[3] as Asn1Tagged;
                if (obj.Identifier.Tag == RfcLdapResult.REFERRAL)
                {
                    var content = (obj.TaggedValue as Asn1OctetString).ByteValue;
                    using (var bais = new MemoryStream(content))
                    {
                        this[3] = new RfcReferral(dec, bais, content.Length);
                    }
                }
            }
        }

        //*************************************************************************
        // Accessors
        //*************************************************************************

        /// <summary> </summary>
        public Asn1Enumerated ResultCode => this[0] as Asn1Enumerated;

        /// <summary> </summary>
        public RfcLdapDN MatchedDN => new RfcLdapDN((this[1] as Asn1OctetString).ByteValue);

        /// <summary> </summary>
        public RfcLdapString ErrorMessage => new RfcLdapString((this[2] as Asn1OctetString).ByteValue);

        /// <summary> </summary>
        public RfcReferral Referral
        {
            get
            {
                if (Count > 3)
                {
                    if (this[3] is RfcReferral ret)
                        return ret;
                }
                return null;
            }
        }

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier Identifier
        {
            set => base.Identifier = value;
            get => new Asn1Identifier(TagClass.APPLICATION, true, LdapMessage.BIND_RESPONSE);
        }
    }
}