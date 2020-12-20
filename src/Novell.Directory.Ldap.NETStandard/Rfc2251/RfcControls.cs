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
    ///     Represents Ldap Contreols.
    ///     <pre>
    ///         Controls ::= SEQUENCE OF Control
    ///     </pre>
    /// </summary>
    public class RfcControls : Asn1SequenceOf
    {
        /// <summary> Controls context specific tag.</summary>
        public const int Controls = 0;

        // *************************************************************************
        // Constructors for Controls
        // *************************************************************************

        /// <summary>
        ///     Constructs a Controls object. This constructor is used in combination
        ///     with the add() method to construct a set of Controls to send to the
        ///     server.
        /// </summary>
        public RfcControls()
            : base(5)
        {
        }

        /// <summary> Constructs a Controls object by decoding it from an InputStream.</summary>
        public RfcControls(IAsn1Decoder dec, Stream inRenamed, int len)
            : base(dec, inRenamed, len)
        {
            // Convert each SEQUENCE element to a Control
            for (var i = 0; i < Size(); i++)
            {
                var tempControl = new RfcControl((Asn1Sequence)get_Renamed(i));
                set_Renamed(i, tempControl);
            }
        }

        // *************************************************************************
        // Mutators
        // *************************************************************************

        /// <summary> Override add() of Asn1SequenceOf to only accept a Control type.</summary>
        public void Add(RfcControl control)
        {
            base.Add(control);
        }

        /// <summary> Override set() of Asn1SequenceOf to only accept a Control type.</summary>
        public void set_Renamed(int index, RfcControl control)
        {
            base.set_Renamed(index, control);
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary> Override getIdentifier to return a context specific id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Context, true, Controls);
        }
    }
}
