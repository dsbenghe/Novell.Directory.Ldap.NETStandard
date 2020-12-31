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
    ///     Represents and Ldap Bind Response.
    ///     <pre>
    ///         BindResponse ::= [APPLICATION 1] SEQUENCE {
    ///         COMPONENTS OF LdapResult,
    ///         serverSaslCreds    [7] OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcBindResponse : Asn1Sequence, IRfcResponse
    {
        // *************************************************************************
        // Constructors for BindResponse
        // *************************************************************************

        /// <summary>
        ///     The only time a client will create a BindResponse is when it is
        ///     decoding it from an InputStream
        ///     Note: If serverSaslCreds is included in the BindResponse, it does not
        ///     need to be decoded since it is already an OCTET STRING.
        /// </summary>
        public RfcBindResponse(IAsn1Decoder dec, Stream inRenamed, int len)
            : base(dec, inRenamed, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() > 3)
            {
                var obj = (Asn1Tagged)get_Renamed(3);
                var id = obj.GetIdentifier();
                if (id.Tag == RfcLdapResult.Referral)
                {
                    var content = ((Asn1OctetString)obj.TaggedValue).ByteValue();
                    var bais = new MemoryStream(content);
                    set_Renamed(3, new RfcReferral(dec, bais, content.Length));
                }
            }
        }

        /// <summary>
        ///     Returns the OPTIONAL serverSaslCreds of a BindResponse if it exists
        ///     otherwise null.
        /// </summary>
        public Asn1OctetString ServerSaslCreds
        {
            get
            {
                if (Size() == 5)
                {
                    return (Asn1OctetString)((Asn1Tagged)get_Renamed(4)).TaggedValue;
                }

                if (Size() == 4)
                {
                    // could be referral or serverSaslCreds
                    var obj = get_Renamed(3);
                    if (obj is Asn1Tagged)
                    {
                        return (Asn1OctetString)((Asn1Tagged)obj).TaggedValue;
                    }
                }

                return null;
            }
        }

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
            if (Size() > 3)
            {
                var obj = get_Renamed(3);
                if (obj is RfcReferral)
                {
                    return (RfcReferral)obj;
                }
            }

            return null;
        }

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, true, LdapMessage.BindResponse);
        }
    }
}
