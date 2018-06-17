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
// Novell.Directory.Ldap.Events.Edir.EventData.GeneralDSEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using System.Text;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
    /// <summary>
    ///     The class represents the data for General DS Events.
    /// </summary>
    public class GeneralDsEventData : BaseEdirEventData
    {
        public GeneralDsEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];

            DsTime = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenDstime);
            MilliSeconds = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenMillisec);

            Verb = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenVerb);
            CurrentProcess = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenCurrproc);

            PerpetratorDn = GetTaggedStringValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenPerp);

            var temptaggedvalue =
                (Asn1Tagged) Decoder.Decode(DecodedData, length);

            if (temptaggedvalue.GetIdentifier().Tag
                == (int) GeneralEventField.EvtTagGenIntegers)
            {
                //Integer List.
                var inteseq = GetTaggedSequence(temptaggedvalue, GeneralEventField.EvtTagGenIntegers);
                var intobject = inteseq.ToArray();
                IntegerValues = new int[intobject.Length];

                for (var i = 0; i < intobject.Length; i++)
                {
                    IntegerValues[i] = ((Asn1Integer) intobject[i]).IntValue();
                }

                //second decoding for Strings.
                temptaggedvalue = (Asn1Tagged) Decoder.Decode(DecodedData, length);
            }
            else
            {
                IntegerValues = null;
            }

            if (temptaggedvalue.GetIdentifier().Tag
                == (int) GeneralEventField.EvtTagGenStrings
                && temptaggedvalue.GetIdentifier().Constructed)
            {
                //String values.
                var inteseq =
                    GetTaggedSequence(temptaggedvalue, GeneralEventField.EvtTagGenStrings);
                var stringobject = inteseq.ToArray();
                StringValues = new string[stringobject.Length];

                for (var i = 0; i < stringobject.Length; i++)
                {
                    StringValues[i] =
                        ((Asn1OctetString) stringobject[i]).StringValue();
                }
            }
            else
            {
                StringValues = null;
            }

            DataInitDone();
        }

        public int DsTime { get; }

        public int MilliSeconds { get; }

        public int Verb { get; }

        public int CurrentProcess { get; }

        public string PerpetratorDn { get; }

        public int[] IntegerValues { get; }

        public string[] StringValues { get; }

        protected int GetTaggedIntValue(Asn1Tagged tagvalue, GeneralEventField tagid)
        {
            var obj = tagvalue.TaggedValue;

            if ((int) tagid != tagvalue.GetIdentifier().Tag)
            {
                throw new IOException("Unknown Tagged Data");
            }

            var dbytes = SupportClass.ToByteArray(((Asn1OctetString) obj).ByteValue());
            var data = new MemoryStream(dbytes);

            var dec = new LberDecoder();

            var length = dbytes.Length;

            return (int) dec.DecodeNumeric(data, length);
        }

        protected string GetTaggedStringValue(Asn1Tagged tagvalue, GeneralEventField tagid)
        {
            var obj = tagvalue.TaggedValue;

            if ((int) tagid != tagvalue.GetIdentifier().Tag)
            {
                throw new IOException("Unknown Tagged Data");
            }

            var dbytes = SupportClass.ToByteArray(((Asn1OctetString) obj).ByteValue());
            var data = new MemoryStream(dbytes);

            var dec = new LberDecoder();

            var length = dbytes.Length;

            return (string) dec.DecodeCharacterString(data, length);
        }

        protected Asn1Sequence GetTaggedSequence(Asn1Tagged tagvalue, GeneralEventField tagid)
        {
            var obj = tagvalue.TaggedValue;

            if ((int) tagid != tagvalue.GetIdentifier().Tag)
            {
                throw new IOException("Unknown Tagged Data");
            }

            var dbytes = SupportClass.ToByteArray(((Asn1OctetString) obj).ByteValue());
            var data = new MemoryStream(dbytes);

            var dec = new LberDecoder();
            var length = dbytes.Length;

            return new Asn1Sequence(dec, data, length);
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.Append("[GeneralDSEventData");
            buf.AppendFormat("(DSTime={0})", DsTime);
            buf.AppendFormat("(MilliSeconds={0})", MilliSeconds);
            buf.AppendFormat("(verb={0})", Verb);
            buf.AppendFormat("(currentProcess={0})", CurrentProcess);
            buf.AppendFormat("(PerpetartorDN={0})", PerpetratorDn);
            buf.AppendFormat("(Integer Values={0})", IntegerValues);
            buf.AppendFormat("(String Values={0})", StringValues);
            buf.Append("]");

            return buf.ToString();
        }
    }
}