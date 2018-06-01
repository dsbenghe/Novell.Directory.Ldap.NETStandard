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
// Novell.Directory.Ldap.Events.Edir.EventData.SecurityEquivalenceEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Text;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
    /// <summary>
    ///     This class represents the data for Security Equivalence Events.
    /// </summary>
    public class SecurityEquivalenceEventData : BaseEdirEventData
    {
        private readonly string _strEntryDn;

        public string EntryDn => _strEntryDn;

        private readonly int _retryCount;

        public int RetryCount => _retryCount;

        private readonly string _strValueDn;

        public string ValueDn => _strValueDn;

        private readonly int _referralCount;

        public int ReferralCount => _referralCount;

        private readonly ArrayList _referralList;

        public ArrayList ReferralList => _referralList;

        public SecurityEquivalenceEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];

            _strEntryDn = ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            _retryCount = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _strValueDn = ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            var referalseq = (Asn1Sequence) Decoder.Decode(DecodedData, length);

            _referralCount = ((Asn1Integer) referalseq.get_Renamed(0)).IntValue();
            _referralList = new ArrayList();
            if (_referralCount > 0)
            {
                var referalseqof = (Asn1Sequence) referalseq.get_Renamed(1);

                for (var i = 0; i < _referralCount; i++)
                {
                    _referralList.Add(new ReferralAddress((Asn1Sequence) referalseqof.get_Renamed(i)));
                }
            }

            DataInitDone();
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("[SecurityEquivalenceEventData");
            buf.AppendFormat("(EntryDN={0})", _strEntryDn);
            buf.AppendFormat("(RetryCount={0})", _retryCount);
            buf.AppendFormat("(valueDN={0})", _strValueDn);
            buf.AppendFormat("(referralCount={0})", _referralCount);
            buf.AppendFormat("(Referral Lists={0})", _referralList);
            buf.Append("]");

            return buf.ToString();
        }
    }
}