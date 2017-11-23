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
// Novell.Directory.Ldap.Asn1.Asn1Structured.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary>
    ///     This class serves as the base type for all ASN.1
    ///     structured types.
    /// </summary>
    public abstract class Asn1Structured : Asn1Object, IList<Asn1Object>,  ICollection<Asn1Object>
    {
        protected List<Asn1Object> Content { get; set; } =  new List<Asn1Object>();

        /// <summary>
        /// Create a an Asn1 structured type with default size of 10
        /// </summary>
        /// <param name="id">the Asn1Identifier containing the tag for this structured type</param>
        protected internal Asn1Structured(Asn1Identifier id) 
            : this(id, 10)
        {
        }

        /// <summary>
        /// Create a an Asn1 structured type with the designated size
        /// </summary>
        /// <param name="id">id the Asn1Identifier containing the tag for this structured type</param>
        /// <param name="size">size the size to allocate</param>
        protected internal Asn1Structured(Asn1Identifier id, int size) 
            : base(id)
        {
            Content = new List<Asn1Object>(size);
        }

        /// <summary>
        /// Create a an Asn1 structured type with default size of 10
        /// </summary>
        /// <param name="id">id the Asn1Identifier containing the tag for this structured type</param>
        /// <param name="newContent">content an array containing the content</param>
        /// <param name="size">size the number of items of content in the array</param>
        protected internal Asn1Structured(Asn1Identifier id, Asn1Object[] newContent, int size) : base(id)
        {
            Content = new List<Asn1Object>(size);
            Content.AddRange(newContent);
        }

        /// <summary>
        ///     Encodes the contents of this Asn1Structured directly to an output
        ///     stream.
        /// </summary>
        public override void Encode(IAsn1Encoder enc, Stream @out)
        {
            enc.Encode(this, @out);
        }

        /// <summary>
        /// Decode an Asn1Structured type from an InputStream.
        /// </summary>
        protected internal void DecodeStructured(IAsn1Decoder dec, Stream @in, int len)
        {
            var componentLen = new int[1]; // collects length of component

            while (len > 0)
            {
                Add(dec.Decode(@in, componentLen));
                len -= componentLen[0];
            }
        }

        /// <summary>
        ///     Returns an array containing the individual ASN.1 elements
        ///     of this Asn1Structed object.
        /// </summary>
        /// <returns>
        ///     an array of Asn1Objects
        /// </returns>
        public Asn1Object[] ToArray() => Content.ToArray();

        /// <summary>
        ///     Adds a new Asn1Object to the end of this Asn1Structured
        ///     object.
        /// </summary>
        /// <param name="value">
        ///     The Asn1Object to add to this Asn1Structured
        ///     object.
        /// </param>
        public virtual void Add(Asn1Object value)
        {
            Content.Add(value);
        }

        public virtual void AddRange(IEnumerable<Asn1Object> values)
        {
            Content.AddRange(values);
        }

        public void Clear() => Content.Clear();

        public bool Contains(Asn1Object item) => Content.Contains(item);

        public void CopyTo(Asn1Object[] array, int arrayIndex) => Content.CopyTo(array, arrayIndex);

        public bool Remove(Asn1Object item) => Content.Remove(item);

        public IEnumerator<Asn1Object> GetEnumerator() => Content.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(Asn1Object item) => Content.IndexOf(item);

        public void Insert(int index, Asn1Object item) => Content.Insert(index, item);

        public void RemoveAt(int index) => Content.RemoveAt(index);

        public Asn1Object this[int index]
        {
            get => Content[index];
            set => Content[index] = value;
        }

        /// <summary>
        ///     Returns the number of Asn1Obejcts that have been encoded
        ///     into this Asn1Structured class.
        /// </summary>
        public int Count => Content.Count;

        public bool IsReadOnly => false;
    }
}