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
// Novell.Directory.Ldap.Events.Edir.EventData.EntryEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Text;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
    /// <summary>
    ///     The class represents the data for Entry Events.
    /// </summary>
    public class EntryEventData : BaseEdirEventData
    {
        private readonly int _nFlags;

        private readonly int _nVerb;

        private readonly string _strClassId;

        private readonly string _strEntry;

        private readonly string _strNewDn;
        private readonly string _strPerpetratorDn;

        private readonly DseTimeStamp _timeStampObj;

        public EntryEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];
            _strPerpetratorDn =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            _strEntry =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            _strClassId =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            _timeStampObj =
                new DseTimeStamp((Asn1Sequence) Decoder.Decode(DecodedData, length));
            _nVerb = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _nFlags = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _strNewDn =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            DataInitDone();
        }

        public string PerpetratorDn => _strPerpetratorDn;

        public string Entry => _strEntry;

        public string NewDn => _strNewDn;

        public string ClassId => _strClassId;

        public int Verb => _nVerb;

        public int Flags => _nFlags;

        public DseTimeStamp TimeStamp => _timeStampObj;

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("EntryEventData[");
            buf.AppendFormat("(Entry={0})", _strEntry);
            buf.AppendFormat("(Prepetrator={0})", _strPerpetratorDn);
            buf.AppendFormat("(ClassId={0})", _strClassId);
            buf.AppendFormat("(Verb={0})", _nVerb);
            buf.AppendFormat("(Flags={0})", _nFlags);
            buf.AppendFormat("(NewDN={0})", _strNewDn);
            buf.AppendFormat("(TimeStamp={0})", _timeStampObj);
            buf.Append("]");

            return buf.ToString();
        }
    }
}