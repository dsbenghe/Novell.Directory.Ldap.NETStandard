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
// Novell.Directory.Ldap.Rfc2251.RfcControl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Control.
    ///     <pre>
    ///         Control ::= SEQUENCE {
    ///         controlType             LdapOID,
    ///         criticality             BOOLEAN DEFAULT FALSE,
    ///         controlValue            OCTET STRING OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcControl : Asn1Sequence
    {
        /// <summary> </summary>
        public virtual Asn1OctetString ControlType => this[0] as Asn1OctetString;

        /// <summary>
        ///     Returns criticality.
        ///     If no value present, return the default value of FALSE.
        /// </summary>
        public virtual Asn1Boolean Criticality
        {
            get
            {
                if (Count > 1)
                {
                    // MAY be a criticality
                    if (this[1] is Asn1Boolean ret)
                        return ret;
                }

                return new Asn1Boolean(false);
            }
        }

        /// <summary>
        ///     Since controlValue is an OPTIONAL component, we need to check
        ///     to see if one is available. Remember that if criticality is of default
        ///     value, it will not be present.
        /// </summary>
        /// <summary>
        ///     Called to set/replace the ControlValue.  Will normally be called by
        ///     the child classes after the parent has been instantiated.
        /// </summary>
        public virtual Asn1OctetString ControlValue
        {
            get
            {
                if (Count > 2)
                {
                    // MUST be a control value
                    return this[2] as Asn1OctetString;
                }
                if (Count > 1)
                {
                    // MAY be a control value
                    if (this[1] is Asn1OctetString ret)
                        return ret;
                }
                return null;
            }

            set
            {
                if (value == null)
                    return;

                if (Count == 3)
                {
                    // We already have a control value, replace it
                    this[2] = value;
                }
                else if (Count == 2)
                {
                    // Is this a control value
                    if (this[1] is Asn1OctetString)
                    {
                        // replace this one
                        this[1] = value;
                    }
                    else
                    {
                        // add a new one at the end
                        Add(value);
                    }
                }
            }
        }

        /// <summary> </summary>
        public RfcControl(RfcLdapOID controlType)
            : this(controlType, new Asn1Boolean(false), null)
        {
        }

        /// <summary> </summary>
        public RfcControl(RfcLdapOID controlType, Asn1Boolean criticality)
            : this(controlType, criticality, null)
        {
        }

        /// <summary>
        ///     Note: criticality is only added if true, as per RFC 2251 sec 5.1 part
        ///     (4): If a value of a type is its default value, it MUST be
        ///     absent.
        /// </summary>
        public RfcControl(RfcLdapOID controlType, Asn1Boolean criticality, Asn1OctetString controlValue)
            : base(3)
        {
            Add(controlType);
            if (criticality.BooleanValue)
                Add(criticality);
            if (controlValue != null)
                Add(controlValue);
        }

        /// <summary> 
        /// Constructs a Control object by decoding it from an InputStream.
        /// </summary>
        public RfcControl(IAsn1Decoder dec, Stream in_Renamed, int len)
            : base(dec, in_Renamed, len)
        {
        }

        /// <summary> 
        /// Constructs a Control object by decoding from an Asn1Sequence
        /// </summary>
        public RfcControl(Asn1Sequence seqObj)
            : base(3)
        {
            AddRange(seqObj);
        }
    }
}