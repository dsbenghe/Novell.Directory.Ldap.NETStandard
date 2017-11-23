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
        public string Attribute { get; protected set; }
        public string ClassId { get; protected set; }
        public string Data { get; protected set; }
        public byte[] BinaryData { get; protected set; }
        public string Entry { get; protected set; }
        public string PerpetratorDN { get; protected set; }
        public string Syntax { get; protected set; }
        public DSETimeStamp TimeStamp { get; protected set; }
        public int Verb { get; protected set; }

        public ValueEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];
            Asn1OctetString octData;

            PerpetratorDN = ((Asn1OctetString)Decoder.Decode(DecodedData, length)).StringValue;
            Entry = ((Asn1OctetString)Decoder.Decode(DecodedData, length)).StringValue;
            Attribute = ((Asn1OctetString)Decoder.Decode(DecodedData, length)).StringValue;
            Syntax = ((Asn1OctetString)Decoder.Decode(DecodedData, length)).StringValue;
            ClassId = ((Asn1OctetString)Decoder.Decode(DecodedData, length)).StringValue;
            TimeStamp = new DSETimeStamp((Asn1Sequence)Decoder.Decode(DecodedData, length));
            octData = (Asn1OctetString)Decoder.Decode(DecodedData, length);
            Data = octData.StringValue;
            BinaryData = octData.ByteValue;
            Verb = ((Asn1Integer)Decoder.Decode(DecodedData, length)).IntValue;
            DataInitDone();
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.Append("[ValueEventData");
            buf.AppendFormat("(Attribute={0})", Attribute);
            buf.AppendFormat("(Classid={0})", ClassId);
            buf.AppendFormat("(Data={0})", Data);
            buf.AppendFormat("(Data={0})", BinaryData);
            buf.AppendFormat("(Entry={0})", Entry);
            buf.AppendFormat("(Perpetrator={0})", PerpetratorDN);
            buf.AppendFormat("(Syntax={0})", Syntax);
            buf.AppendFormat("(TimeStamp={0})", TimeStamp);
            buf.AppendFormat("(Verb={0})", Verb);
            buf.Append("]");

            return buf.ToString();
        }
    }
}