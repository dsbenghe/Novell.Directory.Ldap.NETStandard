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
// Novell.Directory.Ldap.Events.Edir.EventData.BinderyObjectEventData.cs
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
    ///     This class represents the data for Bindery Events.
    /// </summary>
    public class BinderyObjectEventData : BaseEdirEventData
    {
        private readonly string _strEntryDn;

        public string EntryDn => _strEntryDn;

        private readonly int _nType;

        public int ValueType => _nType;

        private readonly int _nEmuObjFlags;

        public int EmuObjFlags => _nEmuObjFlags;

        private readonly int _nSecurity;

        public int Security => _nSecurity;

        private readonly string _strName;

        public string Name => _strName;

        public BinderyObjectEventData(EdirEventDataType eventDataType, Asn1Object message)
            : base(eventDataType, message)
        {
            var length = new int[1];

            _strEntryDn = ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();
            _nType = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _nEmuObjFlags = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _nSecurity = ((Asn1Integer) Decoder.Decode(DecodedData, length)).IntValue();
            _strName = ((Asn1OctetString) Decoder.Decode(DecodedData, length)).StringValue();

            DataInitDone();
        }

        /// <summary>
        ///     Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("[BinderyObjectEvent");
            buf.AppendFormat("(EntryDn={0})", _strEntryDn);
            buf.AppendFormat("(Type={0})", _nType);
            buf.AppendFormat("(EnumOldFlags={0})", _nEmuObjFlags);
            buf.AppendFormat("(Secuirty={0})", _nSecurity);
            buf.AppendFormat("(Name={0})", _strName);
            buf.Append("]");

            return buf.ToString();
        }
    }
}