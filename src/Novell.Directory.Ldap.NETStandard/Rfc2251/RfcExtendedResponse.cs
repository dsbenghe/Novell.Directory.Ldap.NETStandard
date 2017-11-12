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
// Novell.Directory.Ldap.Rfc2251.RfcExtendedResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.NETStandard.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Extended Response.
    ///     <pre>
    ///         ExtendedResponse ::= [APPLICATION 24] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         responseName     [10] LdapOID OPTIONAL,
    ///         response         [11] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcExtendedResponse : Asn1Sequence, IRfcResponse
    {
        /// <summary> </summary>
        public virtual RfcLdapOID ResponseName => responseNameIndex != 0 ? this[responseNameIndex] as RfcLdapOID : null;

        /// <summary> </summary>
        public virtual Asn1OctetString Response => responseIndex != 0 ? this[responseIndex] as Asn1OctetString : null;

        /// <summary> Context-specific TAG for optional responseName.</summary>
        public const int RESPONSE_NAME = 10;

        /// <summary> Context-specific TAG for optional response.</summary>
        public const int RESPONSE = 11;

        private readonly int referralIndex;
        private readonly int responseNameIndex;
        private readonly int responseIndex;

        //*************************************************************************
        // Constructors for ExtendedResponse
        //*************************************************************************

        /// <summary>
        ///     The only time a client will create a ExtendedResponse is when it is
        ///     decoding it from an InputStream
        /// </summary>
        public RfcExtendedResponse(IAsn1Decoder dec, Stream @in, int len) : base(dec, @in, len)
        {
            // decode optional tagged elements
            if (Count > 3)
            {
                for (var i = 3; i < Count; i++)
                {
                    var obj = this[i] as Asn1Tagged;
                    var id = obj.Identifier;
                    switch (id.Tag)
                    {
                        case RfcLdapResult.REFERRAL:
                            var content = ((Asn1OctetString)obj.TaggedValue).ByteValue;
                            using (var bais = new MemoryStream(content))
                                this[i] = new RfcReferral(dec, bais, content.Length);
                            referralIndex = i;
                            break;

                        case RESPONSE_NAME:
                            this[i] = new RfcLdapOID((obj.TaggedValue as Asn1OctetString).ByteValue);
                            responseNameIndex = i;
                            break;

                        case RESPONSE:
                            this[i] = obj.TaggedValue;
                            responseIndex = i;
                            break;
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
        public RfcReferral Referral => referralIndex != 0 ? this[referralIndex] as RfcReferral : null;

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier Identifier
        {
            set => base.Identifier = value;
            get =>new Asn1Identifier(TagClass.APPLICATION, true, LdapMessage.EXTENDED_RESPONSE); 
        }
    }
}