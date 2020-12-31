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
using System.IO;

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
        /// <summary> Context-specific TAG for optional responseName.</summary>
        public const int ResponseNameTag = 10;

        /// <summary> Context-specific TAG for optional response.</summary>
        public const int ResponseTag = 11;

        private readonly int _referralIndex;
        private readonly int _responseIndex;
        private readonly int _responseNameIndex;

        // *************************************************************************
        // Constructors for ExtendedResponse
        // *************************************************************************

        /// <summary>
        ///     The only time a client will create a ExtendedResponse is when it is
        ///     decoding it from an InputStream.
        /// </summary>
        public RfcExtendedResponse(IAsn1Decoder dec, Stream inRenamed, int len)
            : base(dec, inRenamed, len)
        {
            // decode optional tagged elements
            if (Size() > 3)
            {
                for (var i = 3; i < Size(); i++)
                {
                    var obj = (Asn1Tagged)get_Renamed(i);
                    var id = obj.GetIdentifier();
                    switch (id.Tag)
                    {
                        case RfcLdapResult.Referral:
                            var content = ((Asn1OctetString)obj.TaggedValue).ByteValue();
                            var bais = new MemoryStream(content);
                            set_Renamed(i, new RfcReferral(dec, bais, content.Length));
                            _referralIndex = i;
                            break;

                        case ResponseNameTag:
                            set_Renamed(i, new RfcLdapOid(((Asn1OctetString)obj.TaggedValue).ByteValue()));
                            _responseNameIndex = i;
                            break;

                        case ResponseTag:
                            set_Renamed(i, obj.TaggedValue);
                            _responseIndex = i;
                            break;
                    }
                }
            }
        }

        /// <summary> </summary>
        public RfcLdapOid ResponseName => _responseNameIndex != 0 ? (RfcLdapOid)get_Renamed(_responseNameIndex) : null;

        /// <summary> </summary>
        public Asn1OctetString Response => _responseIndex != 0 ? (Asn1OctetString)get_Renamed(_responseIndex) : null;

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary> </summary>
        public Asn1Enumerated GetResultCode()
        {
            return (Asn1Enumerated)get_Renamed(0);
        }

        /// <summary> </summary>
        public RfcLdapDn GetMatchedDn()
        {
            return new RfcLdapDn(((Asn1OctetString)get_Renamed(1)).ByteValue());
        }

        /// <summary> </summary>
        public RfcLdapString GetErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString)get_Renamed(2)).ByteValue());
        }

        /// <summary> </summary>
        public RfcReferral GetReferral()
        {
            return _referralIndex != 0 ? (RfcReferral)get_Renamed(_referralIndex) : null;
        }

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, true, LdapMessage.ExtendedResponse);
        }
    }
}
