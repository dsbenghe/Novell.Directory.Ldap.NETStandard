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

using Novell.Directory.Ldap.Asn1;
using System.IO;

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
        // *************************************************************************
        // Constructors for Control
        // *************************************************************************

        /// <summary> </summary>
        public RfcControl(RfcLdapOid controlType)
            : this(controlType, new Asn1Boolean(false), null)
        {
        }

        /// <summary> </summary>
        public RfcControl(RfcLdapOid controlType, Asn1Boolean criticality)
            : this(controlType, criticality, null)
        {
        }

        /// <summary>
        ///     Note: criticality is only added if true, as per RFC 2251 sec 5.1 part
        ///     (4): If a value of a type is its default value, it MUST be
        ///     absent.
        /// </summary>
        public RfcControl(RfcLdapOid controlType, Asn1Boolean criticality, Asn1OctetString controlValue)
            : base(3)
        {
            Add(controlType);
            if (criticality.BooleanValue())
            {
                Add(criticality);
            }

            if (controlValue != null)
            {
                Add(controlValue);
            }
        }

        /// <summary> Constructs a Control object by decoding it from an InputStream.</summary>
        public RfcControl(IAsn1Decoder dec, Stream inRenamed, int len)
            : base(dec, inRenamed, len)
        {
        }

        /// <summary> Constructs a Control object by decoding from an Asn1Sequence.</summary>
        public RfcControl(Asn1Sequence seqObj)
            : base(3)
        {
            var len = seqObj.Size();
            for (var i = 0; i < len; i++)
            {
                Add(seqObj.get_Renamed(i));
            }
        }

        /// <summary> </summary>
        public Asn1OctetString ControlType => (Asn1OctetString)get_Renamed(0);

        /// <summary>
        ///     Returns criticality.
        ///     If no value present, return the default value of FALSE.
        /// </summary>
        public Asn1Boolean Criticality
        {
            get
            {
                if (Size() > 1)
                {
                    // MAY be a criticality
                    var obj = get_Renamed(1);
                    if (obj is Asn1Boolean)
                    {
                        return (Asn1Boolean)obj;
                    }
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
        public Asn1OctetString ControlValue
        {
            get
            {
                if (Size() > 2)
                {
                    // MUST be a control value
                    return (Asn1OctetString)get_Renamed(2);
                }

                if (Size() > 1)
                {
                    // MAY be a control value
                    var obj = get_Renamed(1);
                    if (obj is Asn1OctetString)
                    {
                        return (Asn1OctetString)obj;
                    }
                }

                return null;
            }

            set
            {
                if (value == null)
                {
                    return;
                }

                if (Size() == 3)
                {
                    // We already have a control value, replace it
                    set_Renamed(2, value);
                    return;
                }

                if (Size() == 2)
                {
                    // Get the second element
                    var obj = get_Renamed(1);

                    // Is this a control value
                    if (obj is Asn1OctetString)
                    {
                        // replace this one
                        set_Renamed(1, value);
                    }
                    else
                    {
                        // add a new one at the end
                        Add(value);
                    }
                }
            }
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************
    }
}
