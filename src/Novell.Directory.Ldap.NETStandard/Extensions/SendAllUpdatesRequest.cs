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
// Novell.Directory.Ldap.Extensions.SendAllUpdatesRequest.cs
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
    ///     Schedules an updated request to be sent to all directory servers in a
    ///     replica ring.
    ///     The sendAllUpdatesRequest extension uses the following OID:
    ///     2.16.840.1.113719.1.27.100.23
    ///     The requestValue has the following format:
    ///     requestValue ::=
    ///     partitionRoot   LdapDN
    ///     origServerDN    LdapDN
    /// </summary>
    public class SendAllUpdatesRequest : LdapExtendedOperation
    {
        /// <summary>
        ///     Constructs an extended operation object for sending updates to a replica ring.
        /// </summary>
        /// <param name="partitionRoot">
        ///     The distinguished name of the replica
        ///     that will be updated.
        /// </param>
        /// <param name="origServerDn">
        ///     The distinguished name of the server that sends the
        ///     updates to the replica ring.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error message
        ///     and an Ldap error code.
        /// </exception>
        public SendAllUpdatesRequest(string partitionRoot, string origServerDn)
            : base(ReplicationConstants.SendAllUpdatesReq, null)
        {
            try
            {
                if ((object) partitionRoot == null || (object) origServerDn == null)
                {
                    throw new ArgumentException(ExceptionMessages.ParamError);
                }

                var encodedData = new MemoryStream();
                var encoder = new LberEncoder();

                var asn1PartitionRoot = new Asn1OctetString(partitionRoot);
                var asn1OrigServerDn = new Asn1OctetString(origServerDn);

                asn1PartitionRoot.Encode(encoder, encodedData);
                asn1OrigServerDn.Encode(encoder, encodedData);

                SetValue(SupportClass.ToSByteArray(encodedData.ToArray()));
            }
            catch (IOException ioe)
            {
                throw new LdapException(ExceptionMessages.EncodingError, LdapException.EncodingError, null, ioe);
            }
        }
    }
}