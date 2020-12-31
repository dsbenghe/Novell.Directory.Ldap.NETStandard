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
using System.Text;

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary>
    ///     This class serves as the base type for all ASN.1
    ///     structured types.
    /// </summary>
    public abstract class Asn1Structured : Asn1Object
    {
        private Asn1Object[] _content;

        private int _contentIndex;

        /*
        * Create a an Asn1 structured type with default size of 10
        *
        * @param the Asn1Identifier containing the tag for this structured type
        */

        protected internal Asn1Structured(Asn1Identifier id)
            : this(id, 10)
        {
        }

        /*
        * Create a an Asn1 structured type with the designated size
        *
        * @param id the Asn1Identifier containing the tag for this structured type
        *
        * @param size the size to allocate
        */

        protected internal Asn1Structured(Asn1Identifier id, int size)
            : base(id)
        {
            _content = new Asn1Object[size];
        }

        /*
        * Create a an Asn1 structured type with default size of 10
        *
        * @param id the Asn1Identifier containing the tag for this structured type
        *
        * @param content an array containing the content
        *
        * @param size the number of items of content in the array
        */

        protected internal Asn1Structured(Asn1Identifier id, Asn1Object[] newContent, int size)
            : base(id)
        {
            _content = newContent;
            _contentIndex = size;
        }

        /// <summary>
        ///     Encodes the contents of this Asn1Structured directly to an output
        ///     stream.
        /// </summary>
        public override void Encode(IAsn1Encoder enc, Stream outRenamed)
        {
            enc.Encode(this, outRenamed);
        }

        /// <summary> Decode an Asn1Structured type from an InputStream.</summary>
        protected internal void DecodeStructured(IAsn1Decoder dec, Stream inRenamed, int len)
        {
            var componentLen = new int[1]; // collects length of component

            while (len > 0)
            {
                Add(dec.Decode(inRenamed, componentLen));
                len -= componentLen[0];
            }
        }

        /// <summary>
        ///     Returns an array containing the individual ASN.1 elements
        ///     of this Asn1Structed object.
        /// </summary>
        /// <returns>
        ///     an array of Asn1Objects.
        /// </returns>
        public Asn1Object[] ToArray()
        {
            var cloneArray = new Asn1Object[_contentIndex];
            Array.Copy(_content, 0, cloneArray, 0, _contentIndex);
            return cloneArray;
        }

        /// <summary>
        ///     Adds a new Asn1Object to the end of this Asn1Structured
        ///     object.
        /// </summary>
        /// <param name="value">
        ///     The Asn1Object to add to this Asn1Structured
        ///     object.
        /// </param>
        public void Add(Asn1Object valueRenamed)
        {
            if (_contentIndex == _content.Length)
            {
                // Array too small, need to expand it, double length
                var newSize = _contentIndex + _contentIndex;
                var newArray = new Asn1Object[newSize];
                Array.Copy(_content, 0, newArray, 0, _contentIndex);
                _content = newArray;
            }

            _content[_contentIndex++] = valueRenamed;
        }

        /// <summary>
        ///     Replaces the Asn1Object in the specified index position of
        ///     this Asn1Structured object.
        /// </summary>
        /// <param name="index">
        ///     The index into the Asn1Structured object where
        ///     this new ANS1Object will be placed.
        /// </param>
        /// <param name="value">
        ///     The Asn1Object to set in this Asn1Structured
        ///     object.
        /// </param>
        public void set_Renamed(int index, Asn1Object valueRenamed)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException("Asn1Structured: get: index " + index + ", size " + _contentIndex);
            }

            _content[index] = valueRenamed;
        }

        /// <summary>
        ///     Gets a specific Asn1Object in this structred object.
        /// </summary>
        /// <param name="index">
        ///     The index of the Asn1Object to get from
        ///     this Asn1Structured object.
        /// </param>
        public Asn1Object get_Renamed(int index)
        {
            if (index >= _contentIndex || index < 0)
            {
                throw new IndexOutOfRangeException("Asn1Structured: set: index " + index + ", size " + _contentIndex);
            }

            return _content[index];
        }

        /// <summary>
        ///     Returns the number of Asn1Obejcts that have been encoded
        ///     into this Asn1Structured class.
        /// </summary>
        public int Size()
        {
            return _contentIndex;
        }

        /// <summary>
        ///     Creates a String representation of this Asn1Structured.
        ///     object.
        /// </summary>
        /// <param name="type">
        ///     the Type to put in the String representing this structured object.
        /// </param>
        /// <returns>
        ///     the String representation of this object.
        /// </returns>
        public string ToString(string type)
        {
            var sb = new StringBuilder();

            sb.Append(type);

            for (var i = 0; i < _contentIndex; i++)
            {
                sb.Append(_content[i]);
                if (i != _contentIndex - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(" }");

#pragma warning disable SA1100 // Do not prefix calls with base unless local implementation exists
            return base.ToString() + sb; // TODO: improve this so we can get rid of the disabling
#pragma warning restore SA1100 // Do not prefix calls with base unless local implementation exists
        }
    }
}
