/******************************************************************************
* The MIT License
* Copyright (c) 2009 Novell Inc.  www.novell.com
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
// Novell.Directory.Ldap.Extensions.GetEffectivePrivilegesResponse.cs
//
// Author:
//   Arpit Rastogi (Rarpit@novell.com)
//
// (C) 2009 Novell, Inc (http://www.novell.com)
//

using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Extensions
{
    public class GetEffectivePrivilegesListResponse : LdapExtendedResponse
    {
        /// <summary>
        ///     Retrieves the effective rights from an GetEffectivePrivilegesListResponse object.
        ///     An object in this class is generated from an ExtendedResponse object
        ///     using the ExtendedResponseFactory class.
        ///     The getEffectivePrivilegesListResponse extension uses the following OID:
        ///     2.16.840.1.113719.1.27.100.104
        /// </summary>

        public int[] Privileges { get; } = { 0 };

        public GetEffectivePrivilegesListResponse(RfcLdapMessage rfcMessage) : base(rfcMessage)
        {
            /// <summary> Constructs an object from the responseValue which contains the effective
            /// Privileges.
            /// 
            /// The constructor parses the responseValue which has the following
            /// format:
            /// responseValue ::=<br>
            /// <p>SEQUENCE numberofresponses ::= INTEGER <br>
            /// SET of [<br>
            /// SEQUENCES of {Privileges INTEGER}]<br>
            ///  
            /// </summary>
            /// <exception> IOException The responseValue could not be decoded.
            /// </exception>
            if (ResultCode == LdapException.SUCCESS)
            {
                // parse the contents of the reply
                var returnedValue = Value;
                if (returnedValue == null)
                    throw new IOException("No returned value");

                //Create a decoder object
                var decoder = new LBERDecoder();

                var asn1_seq1 = decoder.Decode(returnedValue) as Asn1Sequence;
                if (asn1_seq1 == null)
                    throw new IOException("Decoding error");
                var asn1_seq2 = asn1_seq1[0] as Asn1Sequence;
                int no_Privileges = (asn1_seq2[0] as Asn1Integer).IntValue;

                 
                Asn1Set set_privileg_response = (Asn1Set)asn1_seq1[1];
                Asn1Sequence seq2 = null;
                Privileges = new int[no_Privileges];
                for (var index = 0; index < no_Privileges; index++)
                {
                    seq2 = set_privileg_response[index] as Asn1Sequence;
                    Privileges[index] = (seq2[0] as Asn1Integer).IntValue;
                }
            }
        }
    }
}