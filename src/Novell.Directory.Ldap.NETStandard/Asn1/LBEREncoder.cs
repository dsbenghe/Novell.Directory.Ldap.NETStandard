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
// Novell.Directory.Ldap.Asn1.LBEREncoder.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using Novell.Directory.Ldap.NETStandard.Asn1;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary>
    ///     This class provides LBER encoding routines for ASN.1 Types. LBER is a
    ///     subset of BER as described in the following taken from 5.1 of RFC 2251:
    ///     5.1. Mapping Onto BER-based Transport Services
    ///     The protocol elements of Ldap are encoded for exchange using the
    ///     Basic Encoding Rules (BER) [11] of ASN.1 [3]. However, due to the
    ///     high overhead involved in using certain elements of the BER, the
    ///     following additional restrictions are placed on BER-encodings of Ldap
    ///     protocol elements:
    ///     <li>(1) Only the definite form of length encoding will be used.</li>
    ///     <li>(2) OCTET STRING values will be encoded in the primitive form only.</li>
    ///     <li>
    ///         (3) If the value of a BOOLEAN type is true, the encoding MUST have
    ///         its contents octets set to hex "FF".
    ///     </li>
    ///     <li>
    ///         (4) If a value of a type is its default value, it MUST be absent.
    ///         Only some BOOLEAN and INTEGER types have default values in this
    ///         protocol definition.
    ///         These restrictions do not apply to ASN.1 types encapsulated inside of
    ///         OCTET STRING values, such as attribute values, unless otherwise
    ///         noted.
    ///     </li>
    ///     [3] ITU-T Rec. X.680, "Abstract Syntax Notation One (ASN.1) -
    ///     Specification of Basic Notation", 1994.
    ///     [11] ITU-T Rec. X.690, "Specification of ASN.1 encoding rules: Basic,
    ///     Canonical, and Distinguished Encoding Rules", 1994.
    /// </summary>
    public class LBEREncoder : IAsn1Encoder
    {

        /// <summary> 
        /// BER Encode an Asn1Boolean directly into the specified output stream.
        /// </summary>
        public virtual void Encode(Asn1Boolean b, Stream @out)
        {
            /* Encode the id */
            Encode(b.Identifier, @out);

            /* Encode the length */
            @out.WriteByte(0x01);

            /* Encode the boolean content*/
            @out.WriteByte(b.BooleanValue ? (byte)0xff : (byte)0x00);
        }

        /// <summary>
        ///     Encode an Asn1Numeric directly into the specified outputstream.
        ///     Use a two's complement representation in the fewest number of octets
        ///     possible.
        ///     Can be used to encode INTEGER and ENUMERATED values.
        /// </summary>
        public void Encode(Asn1Numeric n, Stream @out)
        {
            var octets = new byte[8];
            byte len;
            long value = n.LongValue;
            long endValue = value < 0 ? -1 : 0;
            long endSign = endValue & 0x80;

            for (len = 0; len == 0 || value != endValue || (octets[len - 1] & 0x80) != endSign; len++)
            {
                octets[len] = (byte)(value & 0xFF);
                value >>= 8;
            }

            Encode(n.Identifier, @out);
            @out.WriteByte(len); // Length
            for (var i = len - 1; i >= 0; i--)
                // Content
                @out.WriteByte(octets[i]);
        }

        /// <summary> 
        /// Encode an Asn1Null directly into the specified outputstream.
        /// </summary>
        public void Encode(Asn1Null n, Stream @out)
        {
            Encode(n.Identifier, @out);
            @out.WriteByte(0x00); // Length (with no Content)
        }

        /// <summary> 
        /// Encode an Asn1OctetString directly into the specified outputstream.
        /// </summary>
        public void Encode(Asn1OctetString os, Stream @out)
        {
            EncodeAsync(os, @out).Wait();
        }

        /// <summary> 
        /// Encode an Asn1OctetString directly into the specified outputstream.
        /// </summary>
        public async Task EncodeAsync(Asn1OctetString os, Stream @out, CancellationToken cancellation = default(CancellationToken))
        {
            Encode(os.Identifier, @out);
            EncodeLength(os.ByteValue.Length, @out);
            await @out.WriteAsync(os.ByteValue, 0, os.ByteValue.Length, cancellation);
        }


        /// <summary>
        ///     Encode an Asn1Structured into the specified outputstream.  This method
        ///     can be used to encode SET, SET_OF, SEQUENCE, SEQUENCE_OF
        /// </summary>
        public void Encode(Asn1Structured structured, Stream @out)
        {
            EncodeAsync(structured, @out).Wait();
        }

        /// <summary>
        ///     Encode an Asn1Structured into the specified outputstream.  This method
        ///     can be used to encode SET, SET_OF, SEQUENCE, SEQUENCE_OF
        /// </summary>
        public async Task EncodeAsync(Asn1Structured structured, Stream @out, CancellationToken cancellation = default(CancellationToken))
        {
            Encode(structured.Identifier, @out);

            Asn1Object[] array = structured.ToArray();

            using (var output = new MemoryStream())
            {
                /* Cycle through each element encoding each element */
                for (var i = 0; i < array.Length; i++)
                {
                    array[i].Encode(this, output);
                }

                /* Encode the length */
                EncodeLength((int)output.Length, @out);
                byte[] data = output.ToArray();
                await @out.WriteAsync(data, 0, data.Length, cancellation);
            }
        }

        /// <summary> 
        /// Encode an Asn1Tagged directly into the specified outputstream.
        /// </summary>
        public void Encode(Asn1Tagged tagged, Stream @out)
        {
            EncodeAsync(tagged, @out).Wait();
        }

        /// <summary> 
        /// Encode an Asn1Tagged directly into the specified outputstream.
        /// </summary>
        public async Task EncodeAsync(Asn1Tagged tagged, Stream @out, CancellationToken cancellation = default(CancellationToken))
        {
            if (tagged.Explicit)
            {
                Encode(tagged.Identifier, @out);

                /* determine the encoded length of the base type. */
                using (var encodedContent = new MemoryStream())
                {
                    tagged.TaggedValue.Encode(this, encodedContent);

                    EncodeLength((int)encodedContent.Length, @out);
                    byte[] data = encodedContent.ToArray();
                    await @out.WriteAsync(data, 0, data.Length);
                }
            }
            else
            {
                tagged.TaggedValue.Encode(this, @out);
            }
        }

        /// <summary> 
        /// Encode an Asn1Identifier directly into the specified outputstream.
        /// </summary>
        public void Encode(Asn1Identifier id, Stream @out)
        {
            TagClass tagClass = id.Asn1Class;
            var t = id.Tag;
            byte ccf = (byte)((((int)tagClass) << 6) | (id.Constructed ? 0x20 : 0));

            if (t < 30)
            {
                /* single octet */
                @out.WriteByte((byte)(ccf | t));
            }
            else
            {
                /* multiple octet */
                @out.WriteByte((byte)(ccf | 0x1F));
                EncodeTagInteger(t, @out);
            }
        }


        private void EncodeLength(int length, Stream @out)
        {
            if (length < 0x80)
            {
                @out.WriteByte((byte)length);
            }
            else
            {
                var octets = new byte[4]; // 4 bytes sufficient for 32 bit int.
                byte n;
                for (n = 0; length != 0; n++)
                {
                    octets[n] = (byte)(length & 0xFF);
                    length >>= 8;
                }

                @out.WriteByte((byte)(0x80 | n));

                for (var i = n - 1; i >= 0; i--)
                    @out.WriteByte(octets[i]);
            }
        }

        /// <summary> 
        /// Encodes the provided tag into the outputstream.
        /// </summary>
        private void EncodeTagInteger(int value, Stream @out)
        {
            var octets = new byte[5];
            int n;
            for (n = 0; value != 0; n++)
            {
                octets[n] = (byte)(value & 0x7F);
                value = value >> 7;
            }
            for (var i = n - 1; i > 0; i--)
            {
                @out.WriteByte((byte)(octets[i] | 0x80));
            }
            @out.WriteByte(octets[0]);
        }
    }
}