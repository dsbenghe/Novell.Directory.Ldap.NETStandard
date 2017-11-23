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
// Novell.Directory.Ldap.Events.Edir.EventData.DebugEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Text;
using Novell.Directory.Ldap.Asn1;
using System.Collections.Generic;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
    /// <summary>
    ///     This class represents the data for Debug Events.
    /// </summary>
    public class DebugEventData : BaseEdirEventData
    {
        public int DSTime { get; protected set; }
        public int MilliSeconds { get; protected set; }
        public string PerpetratorDN { get; protected set; }
        public string FormatString { get; protected set; }
        public int Verb { get; protected set; }

        public int ParameterCount { get; protected set; }
        public IList<DebugParameter> Parameters { get; protected set; }

        public DebugEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];

            DSTime = ((Asn1Integer)Decoder.Decode(DecodedData, length)).IntValue;
            MilliSeconds = ((Asn1Integer)Decoder.Decode(DecodedData, length)).IntValue;
            PerpetratorDN = ((Asn1OctetString)Decoder.Decode(DecodedData, length)).StringValue;
            FormatString = ((Asn1OctetString)Decoder.Decode(DecodedData, length)).StringValue;
            Verb = ((Asn1Integer)Decoder.Decode(DecodedData, length)).IntValue;
            ParameterCount = ((Asn1Integer)Decoder.Decode(DecodedData, length)).IntValue;
            Parameters = new List<DebugParameter>(ParameterCount);
            if (ParameterCount > 0)
            {
                Asn1Sequence seq = Decoder.Decode(DecodedData, length) as Asn1Sequence;
                for (var i = 0; i < ParameterCount; i++)
                {
                    Parameters.Add(new DebugParameter(seq[i] as Asn1Tagged));
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
            buf.Append("[DebugEventData");
            buf.AppendFormat("(Millseconds={0})", MilliSeconds);
            buf.AppendFormat("(DSTime={0})", DSTime);
            buf.AppendFormat("(PerpetratorDN={0})", PerpetratorDN);
            buf.AppendFormat("(Verb={0})", Verb);
            buf.AppendFormat("(ParameterCount={0})", ParameterCount);
            for (var i = 0; i < ParameterCount; i++)
            {
                buf.AppendFormat("(Parameter[{0}]={1})", i, Parameters[i]);
            }
            buf.Append("]");

            return buf.ToString();
        }
    }
}