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
// Novell.Directory.Ldap.Events.Edir.EventData.ValueEventData.cs
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
    ///     This class represents the data for Value Events.
    /// </summary>
    public class ValueEventData : BaseEdirEventData
    {
        protected string StrAttribute;

        public string Attribute => StrAttribute;

        protected string StrClassId;

        public string ClassId => StrClassId;

        protected string StrData;

        public string Data => StrData;

        protected byte[] BinData;

        public byte[] BinaryData => BinData;

        protected string StrEntry;

        public string Entry => StrEntry;

        protected string StrPerpetratorDn;

        public string PerpetratorDn => StrPerpetratorDn;

        // syntax
        protected string StrSyntax;

        public string Syntax => StrSyntax;

        protected DseTimeStamp TimeStampObj;

        public DseTimeStamp TimeStamp => TimeStampObj;

        protected int NVerb;

        public int Verb => NVerb;

        public ValueEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];
            Asn1OctetString octData;

            StrPerpetratorDn =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            StrEntry =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            StrAttribute =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            StrSyntax =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            StrClassId =
                ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            TimeStampObj =
                new DseTimeStamp((Asn1Sequence) Decoder.Decode(DecodedData, length));

            octData = (Asn1OctetString) Decoder.Decode(DecodedData, length);
            StrData = octData.StringValue();
            BinData = SupportClass.ToByteArray(octData.ByteValue());

            NVerb = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();

            DataInitDone();
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.Append("[ValueEventData");
            buf.AppendFormat("(Attribute={0})", StrAttribute);
            buf.AppendFormat("(Classid={0})", StrClassId);
            buf.AppendFormat("(Data={0})", StrData);
            buf.AppendFormat("(Data={0})", BinData);
            buf.AppendFormat("(Entry={0})", StrEntry);
            buf.AppendFormat("(Perpetrator={0})", StrPerpetratorDn);
            buf.AppendFormat("(Syntax={0})", StrSyntax);
            buf.AppendFormat("(TimeStamp={0})", TimeStampObj);
            buf.AppendFormat("(Verb={0})", NVerb);
            buf.Append("]");

            return buf.ToString();
        }
    }
}