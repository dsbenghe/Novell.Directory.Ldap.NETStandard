﻿/******************************************************************************
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
    /// <summary> This class encapsulates the ASN.1 ENUMERATED type.</summary>
    public class Asn1Enumerated : Asn1Numeric
    {
        /// <summary> ASN.1 tag definition for ENUMERATED.</summary>
        public const int Tag = 0x0a;

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier Id = new Asn1Identifier(Asn1Identifier.Universal, false, Tag);

        /* Constructors for Asn1Enumerated
                */

        /// <summary>
        ///     Call this constructor to construct an Asn1Enumerated
        ///     object from an integer value.
        /// </summary>
        /// <param name="content">
        ///     The integer value to be contained in the
        ///     this Asn1Enumerated object.
        /// </param>
        public Asn1Enumerated(int content)
            : base(Id, content)
        {
        }

        /// <summary>
        ///     Call this constructor to construct an Asn1Enumerated
        ///     object from a long value.
        /// </summary>
        /// <param name="content">
        ///     The long value to be contained in the
        ///     this Asn1Enumerated object.
        /// </param>
        public Asn1Enumerated(long content)
            : base(Id, content)
        {
        }

        /// <summary>
        ///     Constructs an Asn1Enumerated object by decoding data from an
        ///     input stream.
        /// </summary>
        /// <param name="dec">
        ///     The decoder object to use when decoding the
        ///     input stream.  Sometimes a developer might want to pass
        ///     in his/her own decoder object.
        /// </param>
        /// <param name="input">
        ///     A byte stream that contains the encoded ASN.1.
        /// </param>
        public Asn1Enumerated(IAsn1Decoder dec, Stream input, int len)
            : base(Id, dec.DecodeNumeric(input, len))
        {
        }

        /// <summary>
        ///     Call this method to encode the current instance into the
        ///     specified output stream using the specified encoder object.
        /// </summary>
        /// <param name="enc">
        ///     Encoder object to use when encoding self.
        /// </param>
        /// <param name="output">
        ///     The output stream onto which the encoded byte
        ///     stream is written.
        /// </param>
        public override void Encode(IAsn1Encoder enc, Stream output)
        {
            enc.Encode(this, output);
        }

        /// <summary> Return a String representation of this Asn1Enumerated.</summary>
        public override string ToString()
        {
            return base.ToString() + "ENUMERATED: " + LongValue();
        }
    }
}
