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
// Novell.Directory.Ldap.Events.Edir.EventData.DebugParameter.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Text;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
    /// <summary>
    ///     This class represents the Debug Paramenter that is part of
    ///     the DebugEventData.
    /// </summary>
    public class DebugParameter
    {
        protected object ObjData;

        public DebugParameter(Asn1Tagged dseObject)
        {
            switch ((DebugParameterType) dseObject.GetIdentifier().Tag)
            {
                case DebugParameterType.Entryid:
                case DebugParameterType.Integer:
                    ObjData = GetTaggedIntValue(dseObject);
                    break;

                case DebugParameterType.Binary:
                    ObjData = ((Asn1OctetString) dseObject.TaggedValue).ByteValue();
                    break;

                case DebugParameterType.String:
                    ObjData = ((Asn1OctetString) dseObject.TaggedValue).StringValue();
                    break;

                case DebugParameterType.Timestamp:
                    ObjData = new DseTimeStamp(GetTaggedSequence(dseObject));
                    break;

                case DebugParameterType.Timevector:
                    var timeVector = new ArrayList();
                    var seq = GetTaggedSequence(dseObject);
                    var count = ((Asn1Integer) seq.get_Renamed(0)).IntValue();
                    if (count > 0)
                    {
                        var timeSeq = (Asn1Sequence) seq.get_Renamed(1);

                        for (var i = 0; i < count; i++)
                        {
                            timeVector.Add(new DseTimeStamp((Asn1Sequence) timeSeq.get_Renamed(i)));
                        }
                    }

                    ObjData = timeVector;
                    break;

                case DebugParameterType.Address:
                    ObjData = new ReferralAddress(GetTaggedSequence(dseObject));
                    break;

                default:
                    throw new IOException("Unknown Tag in DebugParameter..");
            }

            DebugType = (DebugParameterType) dseObject.GetIdentifier().Tag;
        }

        public DebugParameterType DebugType { get; }

        public object Data => ObjData;

        protected int GetTaggedIntValue(Asn1Tagged tagVal)
        {
            var obj = tagVal.TaggedValue;
            var dataBytes = SupportClass.ToByteArray(((Asn1OctetString) obj).ByteValue());

            var decodedData = new MemoryStream(dataBytes);
            var decoder = new LberDecoder();

            return (int) decoder.DecodeNumeric(
                decodedData,
                dataBytes.Length);
        }

        protected Asn1Sequence GetTaggedSequence(Asn1Tagged tagVal)
        {
            var obj = tagVal.TaggedValue;
            var dataBytes = SupportClass.ToByteArray(((Asn1OctetString) obj).ByteValue());

            var decodedData = new MemoryStream(dataBytes);
            var decoder = new LberDecoder();

            return new Asn1Sequence(decoder, decodedData, dataBytes.Length);
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("[DebugParameter");
            if (Enum.IsDefined(DebugType.GetType(), DebugType))
            {
                buf.AppendFormat("(type={0},", DebugType);
                buf.AppendFormat("value={0})", ObjData);
            }
            else
            {
                buf.Append("(type=Unknown)");
            }

            buf.Append("]");

            return buf.ToString();
        }
    }
}