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
        private int _dsTime;

        public int DsTime => _dsTime;

        private int _milliSeconds;

        public int MilliSeconds => _milliSeconds;

        private int _nVerb;

        public int Verb => _nVerb;

        private int _currentProcess;

        public int CurrentProcess => _currentProcess;

        private string _strPerpetratorDn;

        public string PerpetratorDn => _strPerpetratorDn;

        private int[] _integerValues;

        public int[] IntegerValues => _integerValues;

        private string[] _stringValues;

        public string[] StringValues => _stringValues;

        public GeneralDsEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];

            _dsTime = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenDstime);
            _milliSeconds = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenMillisec);

            _nVerb = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenVerb);
            _currentProcess = GetTaggedIntValue(
                (Asn1Tagged) Decoder.Decode(DecodedData, length),
                GeneralEventField.EvtTagGenCurrproc);

            _strPerpetratorDn = GetTaggedStringValue(
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
                _integerValues = new int[intobject.Length];

                for (var i = 0; i < intobject.Length; i++)
                {
                    _integerValues[i] = ((Asn1Integer) intobject[i]).IntValue();
                }

                //second decoding for Strings.
                temptaggedvalue = (Asn1Tagged) Decoder.Decode(DecodedData, length);
            }
            else
            {
                _integerValues = null;
            }

            if (temptaggedvalue.GetIdentifier().Tag
                == (int) GeneralEventField.EvtTagGenStrings
                && temptaggedvalue.GetIdentifier().Constructed)
            {
                //String values.
                var inteseq =
                    GetTaggedSequence(temptaggedvalue, GeneralEventField.EvtTagGenStrings);
                var stringobject = inteseq.ToArray();
                _stringValues = new string[stringobject.Length];

                for (var i = 0; i < stringobject.Length; i++)
                {
                    _stringValues[i] =
                        ((Asn1OctetString) stringobject[i]).StringValue();
                }
            }
            else
            {
                _stringValues = null;
            }

            DataInitDone();
        }

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
            buf.AppendFormat("(DSTime={0})", _dsTime);
            buf.AppendFormat("(MilliSeconds={0})", _milliSeconds);
            buf.AppendFormat("(verb={0})", _nVerb);
            buf.AppendFormat("(currentProcess={0})", _currentProcess);
            buf.AppendFormat("(PerpetartorDN={0})", _strPerpetratorDn);
            buf.AppendFormat("(Integer Values={0})", _integerValues);
            buf.AppendFormat("(String Values={0})", _stringValues);
            buf.Append("]");

            return buf.ToString();
        }
    }
}