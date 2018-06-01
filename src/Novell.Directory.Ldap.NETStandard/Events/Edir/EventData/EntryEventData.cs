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
        protected string StrPerpetratorDn;

        public string PerpetratorDn => StrPerpetratorDn;

        protected string StrEntry;

        public string Entry => StrEntry;

        protected string StrNewDn;

        public string NewDn => StrNewDn;

        protected string StrClassId;

        public string ClassId => StrClassId;

        protected int NVerb;

        public int Verb => NVerb;

        protected int NFlags;

        public int Flags => NFlags;

        protected DseTimeStamp TimeStampObj;

        public DseTimeStamp TimeStamp => TimeStampObj;

        public EntryEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];
            StrPerpetratorDn =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            StrEntry =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            StrClassId =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            TimeStampObj =
                new DseTimeStamp((Asn1Sequence) Decoder.Decode(DecodedData, length));
            NVerb = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            NFlags = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            StrNewDn =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            DataInitDone();
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("EntryEventData[");
            buf.AppendFormat("(Entry={0})", StrEntry);
            buf.AppendFormat("(Prepetrator={0})", StrPerpetratorDn);
            buf.AppendFormat("(ClassId={0})", StrClassId);
            buf.AppendFormat("(Verb={0})", NVerb);
            buf.AppendFormat("(Flags={0})", NFlags);
            buf.AppendFormat("(NewDN={0})", StrNewDn);
            buf.AppendFormat("(TimeStamp={0})", TimeStampObj);
            buf.Append("]");

            return buf.ToString();
        }
    }
}