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
// Novell.Directory.Ldap.Events.Edir.EventData.ChangeAddressEventData.cs
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
    ///     This class represents the data for Change Address.
    /// </summary>
    public class ChangeAddressEventData : BaseEdirEventData
    {
        private int _nFlags;

        public int Flags
        {
            get { return _nFlags; }
        }

        private int _nProto;

        public int Proto
        {
            get { return _nProto; }
        }

        private int _addressFamily;

        public int AddressFamily
        {
            get { return _addressFamily; }
        }

        private string _strAddress;

        public string Address
        {
            get { return _strAddress; }
        }

        private string _pstkName;

        public string PstkName
        {
            get { return _pstkName; }
        }

        private string _sourceModule;

        public string SourceModule
        {
            get { return _sourceModule; }
        }

        public ChangeAddressEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];

            _nFlags = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _nProto = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _addressFamily = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _strAddress = ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            _pstkName = ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            _sourceModule = ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            DataInitDone();
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("[ChangeAddresssEvent");
            buf.AppendFormat("(flags={0})", +_nFlags);
            buf.AppendFormat("(proto={0})", _nProto);
            buf.AppendFormat("(addrFamily={0})", _addressFamily);
            buf.AppendFormat("(address={0})", _strAddress);
            buf.AppendFormat("(pstkName={0})", _pstkName);
            buf.AppendFormat("(source={0})", _sourceModule);
            buf.Append("]");

            return buf.ToString();
        }
    }
}