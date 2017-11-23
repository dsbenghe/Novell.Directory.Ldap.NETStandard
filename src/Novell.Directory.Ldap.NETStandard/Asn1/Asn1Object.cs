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
// Novell.Directory.Ldap.Asn1.Asn1Object.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using System.Text;

namespace Novell.Directory.Ldap.Asn1
{
    /// <summary> This is the base class for all other Asn1 types.</summary>
    public abstract class Asn1Object
    {
        public virtual Asn1Identifier Identifier { get; set; }

        public Asn1Object(Asn1Identifier id)
        {
            Identifier = id;
        }

        /// <summary>
        ///     Abstract method that must be implemented by each child
        ///     class to encode itself ( an Asn1Object) directly intto
        ///     a output stream.
        /// </summary>
        /// <param name="out">
        ///     The output stream onto which the encoded
        ///     Asn1Object will be placed.
        /// </param>
        public abstract void Encode(IAsn1Encoder enc, Stream @out);


        /// <summary>
        ///     This method returns a byte array representing the encoded
        ///     Asn1Object.  It in turn calls the encode method that is
        ///     defined in Asn1Object but will usually be implemented
        ///     in the child Asn1 classses.
        /// </summary>
        public byte[] Encoding(IAsn1Encoder enc)
        {
            using (var @out = new MemoryStream())
            {
                Encode(enc, @out);
                return @out.ToArray();
            }
        }

        /// <summary> Return a String representation of this Asn1Object.</summary>
        public override string ToString()
        {
                var sb = new StringBuilder();

                sb.Append(Identifier.Asn1Class.ToString())
                  .Append(Identifier.Tag)
                  .Append("] ");

                return sb.ToString();
        }
    }
}