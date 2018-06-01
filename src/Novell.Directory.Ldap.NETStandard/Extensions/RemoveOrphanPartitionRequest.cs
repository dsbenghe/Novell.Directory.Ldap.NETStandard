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
// Novell.Directory.Ldap.Extensions.RemoveOrphanPartitionRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap.Extensions
{
    /// <summary>
    ///     Deletes an orphan partition.
    ///     To delete an orphan partition, you must create an instance of this
    ///     class and then call the extendedOperation method with this
    ///     object as the required LdapExtendedOperation parameter.
    ///     The RemoveOrphanPartitionRequest extension uses the following OID:
    ///     2.16.840.1.113719.1.27.100.41
    ///     The requestValue has the following format:
    ///     requestValue ::=
    ///     serverDN     LdapDN
    ///     contextName  LdapDN
    /// </summary>
    public class RemoveOrphanPartitionRequest : LdapExtendedOperation
    {
        /// <summary>
        ///     Constructs an extended operation object for deleting an orphan partition.
        /// </summary>
        /// <param name="serverDn">
        ///     The distinguished name of the server
        ///     on which the orphan partition resides.
        /// </param>
        /// <param name="contextName">
        ///     The distinguished name of the orphan
        ///     partition to delete.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error message
        ///     and an Ldap error code.
        /// </exception>
        public RemoveOrphanPartitionRequest(string serverDn, string contextName)
            : base(ReplicationConstants.RemoveOrphanNamingContextReq, null)
        {
            try
            {
                if ((object) serverDn == null || (object) contextName == null)
                    throw new ArgumentException(ExceptionMessages.ParamError);

                var encodedData = new MemoryStream();
                var encoder = new LberEncoder();

                var asn1ServerDn = new Asn1OctetString(serverDn);
                var asn1ContextName = new Asn1OctetString(contextName);

                asn1ServerDn.Encode(encoder, encodedData);
                asn1ContextName.Encode(encoder, encodedData);

                SetValue(SupportClass.ToSByteArray(encodedData.ToArray()));
            }
            catch (IOException ioe)
            {
                throw new LdapException(ExceptionMessages.EncodingError, LdapException.EncodingError, null, ioe);
            }
        }
    }
}