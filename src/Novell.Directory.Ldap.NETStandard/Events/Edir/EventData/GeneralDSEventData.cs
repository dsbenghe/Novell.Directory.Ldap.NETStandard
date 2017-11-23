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
    public class GeneralDSEventData : BaseEdirEventData
    {
        public int DSTime { get; protected set; }
        public int MilliSeconds { get; protected set; }
        public int Verb { get; protected set; }
        public int CurrentProcess { get; protected set; }
        public string PerpetratorDN { get; protected set; }
        public int[] IntegerValues { get; protected set; }
        public string[] StringValues { get; protected set; }

        public GeneralDSEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];

            DSTime = GetTaggedIntValue((Asn1Tagged)Decoder.Decode(DecodedData, length), GeneralEventField.EVT_TAG_GEN_DSTIME);
            MilliSeconds = GetTaggedIntValue((Asn1Tagged)Decoder.Decode(DecodedData, length), GeneralEventField.EVT_TAG_GEN_MILLISEC);

            Verb = GetTaggedIntValue((Asn1Tagged)Decoder.Decode(DecodedData, length), GeneralEventField.EVT_TAG_GEN_VERB);
            CurrentProcess = GetTaggedIntValue((Asn1Tagged)Decoder.Decode(DecodedData, length), GeneralEventField.EVT_TAG_GEN_CURRPROC);

            PerpetratorDN = GetTaggedStringValue((Asn1Tagged)Decoder.Decode(DecodedData, length), GeneralEventField.EVT_TAG_GEN_PERP);

            var temptaggedvalue = Decoder.Decode(DecodedData, length) as Asn1Tagged;

            if (temptaggedvalue.Identifier.Tag == (int)GeneralEventField.EVT_TAG_GEN_INTEGERS)
            {
                //Integer List.
                var inteseq = GetTaggedSequence(temptaggedvalue, GeneralEventField.EVT_TAG_GEN_INTEGERS);
                var intobject = inteseq.ToArray();
                IntegerValues = new int[intobject.Length];

                for (var i = 0; i < intobject.Length; i++)
                {
                    IntegerValues[i] = (intobject[i] as Asn1Integer).IntValue;
                }

                //second decoding for Strings.
                temptaggedvalue = Decoder.Decode(DecodedData, length) as Asn1Tagged;
            }
            else
            {
                IntegerValues = null;
            }

            if (temptaggedvalue.Identifier.Tag == (int)GeneralEventField.EVT_TAG_GEN_STRINGS
                && temptaggedvalue.Identifier.Constructed)
            {
                //String values.
                var inteseq = GetTaggedSequence(temptaggedvalue, GeneralEventField.EVT_TAG_GEN_STRINGS);
                var stringobject = inteseq.ToArray();
                StringValues = new string[stringobject.Length];

                for (var i = 0; i < stringobject.Length; i++)
                {
                    StringValues[i] = (stringobject[i] as Asn1OctetString).StringValue;
                }
            }
            else
            {
                StringValues = null;
            }

            DataInitDone();
        }

        protected int GetTaggedIntValue(Asn1Tagged tagvalue, GeneralEventField tagid)
        {
            var obj = tagvalue.TaggedValue;

            if ((int)tagid != tagvalue.Identifier.Tag)
            {
                throw new IOException("Unknown Tagged Data");
            }

            var dbytes = (obj as Asn1OctetString).ByteValue;
            using (var data = new MemoryStream(dbytes))
            {
                var dec = new LBERDecoder();
                var length = dbytes.Length;
                return (int)dec.DecodeNumeric(data, length);
            }
        }

        protected string GetTaggedStringValue(Asn1Tagged tagvalue, GeneralEventField tagid)
        {
            var obj = tagvalue.TaggedValue;

            if ((int)tagid != tagvalue.Identifier.Tag)
            {
                throw new IOException("Unknown Tagged Data");
            }

            var dbytes = (obj as Asn1OctetString).ByteValue;
            using (var data = new MemoryStream(dbytes))
            {
                var dec = new LBERDecoder();
                var length = dbytes.Length;
                return dec.DecodeCharacterString(data, length);
            }
        }

        protected Asn1Sequence GetTaggedSequence(Asn1Tagged tagvalue, GeneralEventField tagid)
        {
            var obj = tagvalue.TaggedValue;

            if ((int)tagid != tagvalue.Identifier.Tag)
            {
                throw new IOException("Unknown Tagged Data");
            }

            var dbytes = (obj as Asn1OctetString).ByteValue;
            using (var data = new MemoryStream(dbytes))
            {
                var dec = new LBERDecoder();
                var length = dbytes.Length;
                return new Asn1Sequence(dec, data, length);
            }
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.Append("[GeneralDSEventData");
            buf.AppendFormat("(DSTime={0})", DSTime);
            buf.AppendFormat("(MilliSeconds={0})", MilliSeconds);
            buf.AppendFormat("(verb={0})", Verb);
            buf.AppendFormat("(currentProcess={0})", CurrentProcess);
            buf.AppendFormat("(PerpetartorDN={0})", PerpetratorDN);
            buf.AppendFormat("(Integer Values={0})", IntegerValues);
            buf.AppendFormat("(String Values={0})", StringValues);
            buf.Append("]");

            return buf.ToString();
        }
    }
}