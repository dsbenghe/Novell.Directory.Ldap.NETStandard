/******************************************************************************
* The MIT License
* Copyright (c) 2020 Miroslav Adamec
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

using System;
using System.IO;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
    /// <summary>
    /// LDAP_SERVER_EXTENDED_DN_OID  ( 1.2.840.113556.1.4.529 ) - This causes an 
    /// LDAP server to return an extended form of the objects DN: <GUID=guid_value>;dn.
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-adts/57056773-932c-4e55-9491-e13f49ba580c"/>
    public class ExtendedDnControl : LdapControl
    {
        private const string ExtendedDnControlOID = "1.2.840.113556.1.4.529";

        private readonly LberEncoder encoder = new LberEncoder();
        private readonly Asn1Sequence controlValue = new Asn1Sequence();

        /// <summary>
        /// Creates a new ExtendedDnControl using the specified flag.
        /// </summary>
        /// <param name="flag">The format of the GUID that will be returned.</param>
        /// <param name="critical">True if the LDAP operation should be discarded if the
        /// control is not supported. False if the operation can be processed without
        /// the control.</param>
        public ExtendedDnControl(GuidFormatFlag flag, bool critical)
            : base(ExtendedDnControlOID, critical, null)
        {
            controlValue.Add(new Asn1Integer((int)flag));

            try
            {
                using (var encodedData = new MemoryStream())
                {
                    controlValue.Encode(encoder, encodedData);
                    SetValue(encodedData.ToArray());
                }
            }
            catch (IOException e)
            {
                //Shouldn't occur unless there is a serious failure
                throw new InvalidOperationException("Unable to create instance of ExtendedDnControl", e);
            }
        }

        /// <summary>
        /// LDAP GUID format in HEX or string dashed format.
        /// </summary>
        public enum GuidFormatFlag
        {
            Hex,
            String
        }
    }
}
