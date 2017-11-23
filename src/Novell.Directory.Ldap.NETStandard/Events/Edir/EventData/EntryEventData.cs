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
        public string PerpetratorDN { get; protected set; }
        public string Entry { get; protected set; }
        public string NewDN { get; protected set; }
        public string ClassId { get; protected set; }
        public int Verb { get; protected set; }
        public int Flags { get; protected set; }
        public DSETimeStamp TimeStamp { get; protected set; }

        public EntryEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];
            PerpetratorDN = (Decoder.Decode(DecodedData, length) as Asn1OctetString).StringValue;
            Entry = (Decoder.Decode(DecodedData, length) as Asn1OctetString).StringValue;
            ClassId = (Decoder.Decode(DecodedData, length) as Asn1OctetString).StringValue;
            TimeStamp = new DSETimeStamp(Decoder.Decode(DecodedData, length) as Asn1Sequence);
            Verb = (Decoder.Decode(DecodedData, length) as Asn1Integer).IntValue;
            Flags = (Decoder.Decode(DecodedData, length) as Asn1Integer).IntValue;
            NewDN = (Decoder.Decode(DecodedData, length) as Asn1OctetString).StringValue;

            DataInitDone();
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("EntryEventData[");
            buf.AppendFormat("(Entry={0})", Entry);
            buf.AppendFormat("(Prepetrator={0})", PerpetratorDN);
            buf.AppendFormat("(ClassId={0})", ClassId);
            buf.AppendFormat("(Verb={0})", Verb);
            buf.AppendFormat("(Flags={0})", Flags);
            buf.AppendFormat("(NewDN={0})", NewDN);
            buf.AppendFormat("(TimeStamp={0})", TimeStamp);
            buf.Append("]");

            return buf.ToString();
        }
    }
}