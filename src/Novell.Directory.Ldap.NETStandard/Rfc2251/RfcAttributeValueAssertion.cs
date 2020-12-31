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

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Attribute Value Assertion.
    ///     <pre>
    ///         AttributeValueAssertion ::= SEQUENCE {
    ///         attributeDesc   AttributeDescription,
    ///         assertionValue  AssertionValue }
    ///     </pre>
    /// </summary>
    public class RfcAttributeValueAssertion : Asn1Sequence
    {
        /// <summary>
        ///     Creates an Attribute Value Assertion.
        /// </summary>
        /// <param name="ad">
        ///     The assertion description.
        /// </param>
        /// <param name="av">
        ///     The assertion value.
        /// </param>
        public RfcAttributeValueAssertion(RfcAttributeDescription ad, RfcAssertionValue av)
            : base(2)
        {
            Add(ad);
            Add(av);
        }

        /// <summary>
        ///     Returns the attribute description.
        /// </summary>
        /// <returns>
        ///     the attribute description.
        /// </returns>
        public string AttributeDescription => ((RfcAttributeDescription)get_Renamed(0)).StringValue();

        /// <summary>
        ///     Returns the assertion value.
        /// </summary>
        /// <returns>
        ///     the assertion value.
        /// </returns>
        public byte[] AssertionValue => ((RfcAssertionValue)get_Renamed(1)).ByteValue();
    }
}
