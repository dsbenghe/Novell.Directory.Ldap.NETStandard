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
// Novell.Directory.Ldap.Rfc2251.RfcBindRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.NETStandard.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents and Ldap Bind Request.
    ///     <pre>
    ///         BindRequest ::= [APPLICATION 0] SEQUENCE {
    ///         version                 INTEGER (1 .. 127),
    ///         name                    LdapDN,
    ///         authentication          AuthenticationChoice }
    ///     </pre>
    /// </summary>
    public class RfcBindRequest : Asn1Sequence, IRfcRequest
    {
        /// <summary> </summary>
        /// <summary> Sets the protocol version</summary>
        public virtual Asn1Integer Version
        {
            get => (Asn1Integer)this[0];
            set => this[0] = value;
        }

        /// <summary> </summary>
        /// <summary> </summary>
        public virtual RfcLdapDN Name
        {
            get => (RfcLdapDN)this[1];
            set => this[1] = value;
        }

        /// <summary> </summary>
        /// <summary> </summary>
        public virtual RfcAuthenticationChoice AuthenticationChoice
        {
            get => (RfcAuthenticationChoice)this[2];
            set => this[2] = value;
        }

        /// <summary>
        ///     ID is added for Optimization.
        ///     ID needs only be one Value for every instance,
        ///     thus we create it only once.
        /// </summary>
        private static readonly Asn1Identifier ID = new Asn1Identifier(TagClass.APPLICATION, true,
            LdapMessage.BIND_REQUEST);


        //*************************************************************************
        // Constructors for BindRequest
        //*************************************************************************

        /// <summary> </summary>
        public RfcBindRequest(Asn1Integer version, RfcLdapDN name, RfcAuthenticationChoice auth) : base(3)
        {
            Add(version);
            Add(name);
            Add(auth);
        }

        public RfcBindRequest(int version, string dn, string mechanism, byte[] credentials)
            : this(new Asn1Integer(version), new RfcLdapDN(dn), new RfcAuthenticationChoice(mechanism, credentials))
        {
        }

        /// <summary>
        ///     Constructs a new Bind Request copying the original data from
        ///     an existing request.
        /// </summary>
        internal RfcBindRequest(Asn1Object[] origRequest, string @base) : base(origRequest, origRequest.Length)
        {
            // Replace the dn if specified, otherwise keep original base
            if (@base != null)
            {
                this[1] = new RfcLdapDN(@base);
            }
        }

        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 0. (0x60)
        ///     </pre>
        /// </summary>
        public override Asn1Identifier Identifier
        {
            get => ID;
            set => base.Identifier = value;
        }

        public IRfcRequest DupRequest(string @base, string filter, bool request) => new RfcBindRequest(ToArray(), @base);

        public string RequestDN => ((RfcLdapDN)this[1]).StringValue;
    }
}