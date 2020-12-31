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

using System;
using System.IO;

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary> This class encapsulates the OCTET STRING type.</summary>
    public class Asn1OctetString : Asn1Object
    {
        /// <summary> ASN.1 OCTET STRING tag definition.</summary>
        public const int Tag = 0x04;

        /// <summary>
        ///     ID is added for Optimization.
        ///     Id needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier Id = new Asn1Identifier(Asn1Identifier.Universal, false, Tag);

        private readonly byte[] _content;

        /* Constructors for Asn1OctetString
                */

        /// <summary>
        ///     Call this constructor to construct an Asn1OctetString
        ///     object from a byte array.
        /// </summary>
        /// <param name="content">
        ///     A byte array representing the string that
        ///     will be contained in the this Asn1OctetString object.
        /// </param>
        public Asn1OctetString(byte[] content)
            : base(Id)
        {
            _content = content;
        }

        /// <summary>
        ///     Call this constructor to construct an Asn1OctetString
        ///     object from a String object.
        /// </summary>
        /// <param name="content">
        ///     A string value that will be contained
        ///     in the this Asn1OctetString object.
        /// </param>
        public Asn1OctetString(string content)
            : base(Id)
        {
            try
            {
                var ibytes = content.ToUtf8Bytes();
                _content = ibytes;
            }
            catch (IOException uee)
            {
                throw new Exception(uee.ToString());
            }
        }

        /// <summary>
        ///     Constructs an Asn1OctetString object by decoding data from an
        ///     input stream.
        /// </summary>
        /// <param name="dec">
        ///     The decoder object to use when decoding the
        ///     input stream.  Sometimes a developer might want to pass
        ///     in his/her own decoder object.
        /// </param>
        /// <param name="in">
        ///     A byte stream that contains the encoded ASN.1.
        /// </param>
        public Asn1OctetString(IAsn1Decoder dec, Stream inRenamed, int len)
            : base(Id)
        {
            _content = len > 0 ? (byte[])dec.DecodeOctetString(inRenamed, len) : new byte[0];
        }

        /* Asn1Object implementation
        */

        /// <summary>
        ///     Call this method to encode the current instance into the
        ///     specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">
        ///     Encoder object to use when encoding self.
        /// </param>
        /// <param name="out">
        ///     The output stream onto which the encoded byte
        ///     stream is written.
        /// </param>
        public override void Encode(IAsn1Encoder enc, Stream outRenamed)
        {
            enc.Encode(this, outRenamed);
        }

        /*Asn1OctetString specific methods
        */

        /// <summary> Returns the content of this Asn1OctetString as a byte array.</summary>
        public byte[] ByteValue()
        {
            return _content;
        }

        /// <summary> Returns the content of this Asn1OctetString as a String.</summary>
        public string StringValue()
        {
            string s = null;
            try
            {
                s = _content.ToUtf8String();
            }
            catch (IOException uee)
            {
                // TODO: Why? Just remove the try..catch?
                throw new Exception(uee.ToString());
            }

            return s;
        }

        /// <summary> Return a String representation of this Asn1Object.</summary>
        public override string ToString()
        {
            return base.ToString() + "OCTET STRING: " + StringValue();
        }
    }
}
