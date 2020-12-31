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
using System;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents and Ldap Compare Request.
    ///     <pre>
    ///         CompareRequest ::= [APPLICATION 14] SEQUENCE {
    ///         entry           LdapDN,
    ///         ava             AttributeValueAssertion }
    ///     </pre>
    /// </summary>
    public class RfcCompareRequest : Asn1Sequence, IRfcRequest
    {
        // *************************************************************************
        // Constructor for CompareRequest
        // *************************************************************************

        /// <summary> </summary>
        public RfcCompareRequest(RfcLdapDn entry, RfcAttributeValueAssertion ava)
            : base(2)
        {
            Add(entry);
            Add(ava);
            if (ava.AssertionValue == null)
            {
                throw new ArgumentException("compare: Attribute must have an assertion value");
            }
        }

        /// <summary>
        ///     Constructs a new Compare Request copying from the data of
        ///     an existing request.
        /// </summary>
        internal RfcCompareRequest(Asn1Object[] origRequest, string baseRenamed)
            : base(origRequest, origRequest.Length)
        {
            // Replace the base if specified, otherwise keep original base
            if (baseRenamed != null)
            {
                set_Renamed(0, new RfcLdapDn(baseRenamed));
            }
        }

        public RfcAttributeValueAssertion AttributeValueAssertion => (RfcAttributeValueAssertion)get_Renamed(1);

        public IRfcRequest DupRequest(string baseRenamed, string filter, bool request)
        {
            return new RfcCompareRequest(ToArray(), baseRenamed);
        }

        public string GetRequestDn()
        {
            return ((RfcLdapDn)get_Renamed(0)).StringValue();
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, true, LdapMessage.CompareRequest);
        }
    }
}
