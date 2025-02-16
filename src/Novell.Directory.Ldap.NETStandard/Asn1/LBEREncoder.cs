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

using System.IO;

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
    public class LberEncoder : IAsn1Encoder
    {
        /* Encoders for ASN.1 simple type Contents
                */

        /// <summary> BER Encode an Asn1Boolean directly into the specified output stream.</summary>
        public void Encode(Asn1Boolean b, Stream output)
        {
            /* Encode the id */
            Encode(b.GetIdentifier(), output);

            /* Encode the length */
            output.WriteByte(0x01);

            /* Encode the boolean content*/
            output.WriteByte(b.BooleanValue() ? (byte)0xff : (byte)0x00);
        }

        /// <summary>
        ///     Encode an Asn1Numeric directly into the specified output stream.
        ///     Use a two's complement representation in the fewest number of octets
        ///     possible.
        ///     Can be used to encode INTEGER and ENUMERATED values.
        /// </summary>
        public void Encode(Asn1Numeric n, Stream output)
        {
            var octets = new byte[8];
            byte len;
            var value = n.LongValue();
            long endValue = value < 0 ? -1 : 0;
            var endSign = endValue & 0x80;

            for (len = 0; len == 0 || value != endValue || (octets[len - 1] & 0x80) != endSign; len++)
            {
                octets[len] = (byte)(value & 0xFF);
                value >>= 8;
            }

            Encode(n.GetIdentifier(), output);
            output.WriteByte(len); // Length

            // Content
            for (var i = len - 1; i >= 0; i--)
            {
                output.WriteByte(octets[i]);
            }
        }

        /* Asn1 TYPE NOT YET SUPPORTED
        * Encode an Asn1Real directly to a stream.
        public void encode(Asn1Real r, OutputStream out)
        throws IOException
        {
        throw new IOException("LBEREncoder: Encode to a stream not implemented");
        }
        */

        /// <summary> Encode an Asn1Null directly into the specified outputstream.</summary>
        public void Encode(Asn1Null n, Stream output)
        {
            Encode(n.GetIdentifier(), output);
            output.WriteByte(0x00); // Length (with no Content)
        }

        /* Asn1 TYPE NOT YET SUPPORTED
        * Encode an Asn1BitString directly to a stream.
        public void encode(Asn1BitString bs, OutputStream out)
        throws IOException
        {
        throw new IOException("LBEREncoder: Encode to a stream not implemented");
        }
        */

        /// <summary> Encode an Asn1OctetString directly into the specified outputstream.</summary>
        public void Encode(Asn1OctetString os, Stream output)
        {
            Encode(os.GetIdentifier(), output);
            EncodeLength(os.ByteValue().Length, output);
            var array = os.ByteValue();
            output.Write(array, 0, array.Length);
        }

        /* Asn1 TYPE NOT YET SUPPORTED
        * Encode an Asn1ObjectIdentifier directly to a stream.
        * public void encode(Asn1ObjectIdentifier oi, OutputStream out)
        * throws IOException
        * {
        * throw new IOException("LBEREncoder: Encode to a stream not implemented");
        * }
        */

        /* Asn1 TYPE NOT YET SUPPORTED
        * Encode an Asn1CharacterString directly to a stream.
        * public void encode(Asn1CharacterString cs, OutputStream out)
        * throws IOException
        * {
        * throw new IOException("LBEREncoder: Encode to a stream not implemented");
        * }
        */

        /* Encoders for ASN.1 structured types
        */

        /// <summary>
        ///     Encode an Asn1Structured into the specified outputstream.  This method
        ///     can be used to encode SET, SET_OF, SEQUENCE, SEQUENCE_OF.
        /// </summary>
        public void Encode(Asn1Structured c, Stream output)
        {
            Encode(c.GetIdentifier(), output);

            var value = c.ToArray();

            var buffer = new MemoryStream();

            /* Cycle through each element encoding each element */
            for (var i = 0; i < value.Length; i++)
            {
                value[i].Encode(this, buffer);
            }

            /* Encode the length */
            EncodeLength((int)buffer.Length, output);

            /* Add each encoded element into the output stream */
            var array = buffer.GetBuffer();
            output.Write(array, 0, (int)buffer.Length);
        }

        /// <summary> Encode an Asn1Tagged directly into the specified outputstream.</summary>
        public void Encode(Asn1Tagged t, Stream output)
        {
            if (t.Explicit)
            {
                Encode(t.GetIdentifier(), output);

                /* determine the encoded length of the base type. */
                var encodedContent = new MemoryStream();
                t.TaggedValue.Encode(this, encodedContent);

                EncodeLength((int)encodedContent.Length, output);
                var array = encodedContent.ToArray();
                output.Write(array, 0, array.Length);
            }
            else
            {
                t.TaggedValue.Encode(this, output);
            }
        }

        /* Encoders for ASN.1 useful types
        */
        /* Encoder for ASN.1 Identifier
        */

        /// <summary> Encode an Asn1Identifier directly into the specified outputstream.</summary>
        public void Encode(Asn1Identifier id, Stream output)
        {
            var c = id.Asn1Class;
            var t = id.Tag;
            var ccf = (byte)((c << 6) | (id.Constructed ? 0x20 : 0));

            if (t < 30)
            {
                /* single octet */
                output.WriteByte((byte)(ccf | t));
            }
            else
            {
                /* multiple octet */
                output.WriteByte((byte)(ccf | 0x1F));
                EncodeTagInteger(t, output);
            }
        }

        /* Private helper methods
        */

        /*
        *  Encodes the specified length into the the outputstream
        */

        private void EncodeLength(int length, Stream output)
        {
            if (length < 0x80)
            {
                output.WriteByte((byte)length);
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

                output.WriteByte((byte)(0x80 | n));

                for (var i = n - 1; i >= 0; i--)
                {
                    output.WriteByte(octets[i]);
                }
            }
        }

        /// <summary> Encodes the provided tag into the outputstream.</summary>
        private void EncodeTagInteger(int value, Stream output)
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
                output.WriteByte((byte)(octets[i] | 0x80));
            }

            output.WriteByte(octets[0]);
        }
    }
}
