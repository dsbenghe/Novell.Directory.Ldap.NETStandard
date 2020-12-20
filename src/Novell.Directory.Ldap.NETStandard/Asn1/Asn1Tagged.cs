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
    ///     The Asn1Tagged class can hold a base Asn1Object with a distinctive tag
    ///     describing the type of that base object. It also maintains a boolean value
    ///     indicating whether the value should be encoded by EXPLICIT or IMPLICIT
    ///     means. (Explicit is true by default.)
    ///     If the type is encoded IMPLICITLY, the base types form, length and content
    ///     will be encoded as usual along with the class type and tag specified in
    ///     the constructor of this Asn1Tagged class.
    ///     If the type is to be encoded EXPLICITLY, the base type will be encoded as
    ///     usual after the Asn1Tagged identifier has been encoded.
    /// </summary>
    public class Asn1Tagged : Asn1Object
    {
        private Asn1Object _content;

        /* Constructors for Asn1Tagged
        */

        /// <summary>
        ///     Constructs an Asn1Tagged object using the provided
        ///     AN1Identifier and the Asn1Object.
        ///     The explicit flag defaults to true as per the spec.
        /// </summary>
        public Asn1Tagged(Asn1Identifier identifier, Asn1Object objectRenamed)
            : this(identifier, objectRenamed, true)
        {
        }

        /// <summary> Constructs an Asn1Tagged object.</summary>
        public Asn1Tagged(Asn1Identifier identifier, Asn1Object objectRenamed, bool explicitRenamed)
            : base(identifier)
        {
            _content = objectRenamed;
            Explicit = explicitRenamed;

            if (!explicitRenamed && _content != null)
            {
                // replace object's id with new tag.
                _content.SetIdentifier(identifier);
            }
        }

        /// <summary>
        ///     Constructs an Asn1Tagged object by decoding data from an
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
        public Asn1Tagged(IAsn1Decoder dec, Stream inRenamed, int len, Asn1Identifier identifier)
            : base(identifier)
        {
            // If we are decoding an implicit tag, there is no way to know at this
            // low level what the base type really is. We can place the content
            // into an Asn1OctetString type and pass it back to the application who
            // will be able to create the appropriate ASN.1 type for this tag.
            _content = new Asn1OctetString(dec, inRenamed, len);
        }

        /// <summary> Sets the Asn1Object tagged value.</summary>
        public Asn1Object TaggedValue
        {
            get => _content;
            set
            {
                _content = value;
                if (!Explicit && value != null)
                {
                    // replace object's id with new tag.
                    value.SetIdentifier(GetIdentifier());
                }
            }
        }

        /// <summary>
        ///     Returns a boolean value indicating if this object uses
        ///     EXPLICIT tagging.
        /// </summary>
        public bool Explicit { get; }

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

        /* Asn1Tagged specific methods
        */

        /// <summary> Return a String representation of this Asn1Object.</summary>
        public override string ToString()
        {
            if (Explicit)
            {
                return base.ToString() + _content;
            }

            // implicit tagging
            return _content.ToString();
        }
    }
}
