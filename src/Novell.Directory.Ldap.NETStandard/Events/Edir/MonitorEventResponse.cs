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
// Novell.Directory.Ldap.Events.Edir.MonitorEventResponse.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Events.Edir
{
    /// <summary>
    ///     This object represents the ExtendedResponse returned when Event
    ///     Registeration fails. This Extended Response structure is generated for
    ///     requests send as MonitorEventRequest.
    /// </summary>
    public class MonitorEventResponse : LdapExtendedResponse
    {
        public EdirEventSpecifier[] SpecifierList { get; protected set; }

        public MonitorEventResponse(RfcLdapMessage message)
            : base(message)
        {
            byte[] returnedValue = Value;

            if (returnedValue == null)
            {
                throw new LdapException(LdapException.ResultCodeToString(ResultCode),
                    ResultCode,
                    null);
            }

            var decoder = new LBERDecoder();

            var sequence = decoder.Decode(returnedValue) as Asn1Sequence;

            var length = (sequence[0] as Asn1Integer).IntValue;
            var sequenceSet = sequence[1] as Asn1Set;
            SpecifierList = new EdirEventSpecifier[length];

            for (var i = 0; i < length; i++)
            {
                var eventspecifiersequence = sequenceSet[i] as Asn1Sequence;
                var classfication = (eventspecifiersequence[0] as Asn1Integer).IntValue;
                var enumtype = (eventspecifiersequence[1] as Asn1Enumerated).IntValue;
                SpecifierList[i] = new EdirEventSpecifier((EdirEventType)classfication, (EdirEventResultType)enumtype);
            }
        }
    }
}