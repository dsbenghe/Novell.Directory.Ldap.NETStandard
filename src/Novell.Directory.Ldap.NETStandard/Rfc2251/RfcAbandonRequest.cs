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
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents the Ldap Abandon Request.
    ///     <pre>
    ///         AbandonRequest ::= [APPLICATION 16] MessageID
    ///     </pre>
    /// </summary>
    internal class RfcAbandonRequest : RfcMessageId, IRfcRequest
    {
        // *************************************************************************
        // Constructor for AbandonRequest
        // *************************************************************************

        /// <summary> Constructs an RfcAbandonRequest.</summary>
        public RfcAbandonRequest(int msgId)
            : base(msgId)
        {
        }

        public IRfcRequest DupRequest(string baseRenamed, string filter, bool reference)
        {
            throw new LdapException(ExceptionMessages.NoDupRequest, new object[] { "Abandon" },
                LdapException.LdapNotSupported, null);
        }

        public string GetRequestDn()
        {
            return null;
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 16. (0x50)
        ///     </pre>
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.Application, false, LdapMessage.AbandonRequest);
        }
    }
}
