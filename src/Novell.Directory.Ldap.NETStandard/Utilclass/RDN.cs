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
// Novell.Directory.Ldap.Utilclass.RDN.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     A RDN encapsulates a single object's name of a Distinguished Name(DN).
    ///     The object name represented by this class contains no context.  Thus a
    ///     Relative Distinguished Name (RDN) could be relative to anywhere in the
    ///     Directories tree.
    ///     For example, of following DN, 'cn=admin, ou=marketing, o=corporation', all
    ///     possible RDNs are 'cn=admin', 'ou=marketing', and 'o=corporation'.
    ///     Multivalued attributes are encapsulated in this class.  For example the
    ///     following could be represented by an RDN: 'cn=john + l=US', or
    ///     'cn=juan + l=ES'
    /// </summary>
    /// <seealso cref="DN">
    /// </seealso>
    public class RDN
    {
        /// <summary>
        ///     Returns the actually Raw String before Normalization
        /// </summary>
        /// <returns>
        ///     The raw string
        /// </returns>
        protected internal virtual string RawValue { get; private set; } = string.Empty;

        /// <summary>
        ///     Returns the type of this RDN.  This method assumes that only one value
        ///     is used, If multivalues attributes are used only the first Type is
        ///     returned.  Use GetTypes.
        /// </summary>
        /// <returns>
        ///     Type of attribute
        /// </returns>
        public virtual string Type => Types[0];

        /// <summary> Returns all the types for this RDN.</summary>
        /// <returns>
        ///     list of types
        /// </returns>
        public virtual IList<string> Types { get; } = new List<string>();

        /// <summary>
        ///     Returns the values of this RDN.  If multivalues attributes are used only
        ///     the first Type is returned.  Use GetTypes.
        /// </summary>
        /// <returns>
        ///     Type of attribute
        /// </returns>
        public virtual string Value => Values[0];

        /// <summary> Returns all the types for this RDN.</summary>
        /// <returns>
        ///     list of types
        /// </returns>
        public virtual IList<string> Values { get; } = new List<string>();

        /// <summary> Determines if this RDN is multivalued or not</summary>
        /// <returns>
        ///     true if this RDN is multivalued
        /// </returns>
        public virtual bool Multivalued => Values.Count > 1;


        /// <summary>
        ///     Creates an RDN object from the DN component specified in the string RDN
        /// </summary>
        /// <param name="rdn">
        ///     the DN component
        /// </param>
        public RDN(string rdn)
        {
            RawValue = rdn;
            var dn = new DN(rdn);
            var rdns = dn.RDNs;
            //there should only be one rdn
            if (rdns.Count != 1)
                throw new ArgumentException("Invalid RDN: see API documentation");
            var thisRDN = (RDN)rdns[0];
            Types = thisRDN.Types;
            Values = thisRDN.Values;
            RawValue = thisRDN.RawValue;
        }

        public RDN()
        {

        }

        /// <summary>
        ///     Compares the RDN to the rdn passed.  Note: If an there exist any
        ///     mulivalues in one RDN they must all be present in the other.
        /// </summary>
        /// <param name="rdn">
        ///     the RDN to compare to
        ///     @throws IllegalArgumentException if the application compares a name
        ///     with an OID.
        /// </param>
        public virtual bool Equals(RDN rdn)
        {
            if (Values.Count != rdn.Values.Count)
            {
                return false;
            }
            int j, i;
            for (i = 0; i < Values.Count; i++)
            {
                //verify that the current value and type exists in the other list
                j = 0;
                //May need a more intellegent compare
                while (j < Values.Count &&
                       (!Values[i].ToUpper().Equals(rdn.Values[j].ToUpper()) ||
                        !EqualAttrType(Types[i], rdn.Types[j])))
                {
                    j++;
                }
                if (j >= rdn.Values.Count)
                    //couldn't find first value
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     Internal function used by equal to compare Attribute types.  Because
        ///     attribute types could either be an OID or a name.  There needs to be a
        ///     Translation mechanism.  This function will absract this functionality.
        ///     Currently if types differ (Oid and number) then UnsupportedOperation is
        ///     thrown, either one or the other must used.  In the future an OID to name
        ///     translation can be used.
        /// </summary>
        private bool EqualAttrType(string attr1, string attr2)
        {
            if (char.IsDigit(attr1[0]) ^ char.IsDigit(attr2[0]))
                //isDigit tests if it is an OID
                throw new ArgumentException("OID numbers are not currently compared to attribute names");

            return attr1.ToUpper().Equals(attr2.ToUpper());
        }

        /// <summary>
        ///     Adds another value to the RDN.  Only one attribute type is allowed for
        ///     the RDN.
        /// </summary>
        /// <param name="attrType">
        ///     Attribute type, could be an OID or String
        /// </param>
        /// <param name="attrValue">
        ///     Attribute Value, must be normalized and escaped
        /// </param>
        /// <param name="rawValue">
        ///     or text before normalization, can be Null
        /// </param>
        public virtual void Add(string attrType, string attrValue, string rawValue)
        {
            Types.Add(attrType);
            Values.Add(attrValue);
            RawValue += rawValue;
        }

        /// <summary>
        ///     Creates a string that represents this RDN, according to RFC 2253
        /// </summary>
        /// <returns>
        ///     An RDN string
        /// </returns>
        public override string ToString() => ToString(false);

        /// <summary>
        ///     Creates a string that represents this RDN.
        ///     If noTypes is true then Atribute types will be ommited.
        /// </summary>
        /// <param name="noTypes">
        ///     true if attribute types will be omitted.
        /// </param>
        /// <returns>
        ///     An RDN string
        /// </returns>
        public virtual string ToString(bool noTypes)
        {
            var length = Types.Count;
            var sb = new StringBuilder();
            if (length < 1)
                return null;
            if (!noTypes)
            {
                sb.Append(Types[0])
                    .Append("=");
            }
            sb.Append(Values[0]);

            for (var i = 1; i < length; i++)
            {
                sb.Append("+");
                if (!noTypes)
                {
                    sb.Append(Types[i])
                       .Append("=");
                }
                sb.Append(Values[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Returns each multivalued name in the current RDN as an array of Strings.
        /// </summary>
        /// <param name="noTypes">
        ///     Specifies whether Attribute types are included. The attribute
        ///     type names will be ommitted if the parameter noTypes is true.
        /// </param>
        /// <returns>
        ///     List of multivalued Attributes
        /// </returns>
        public virtual string[] ExplodeRDN(bool noTypes)
        {
            var length = Types.Count;
            if (length < 1)
                return null;
            var toReturn = new string[Types.Count];

            if (!noTypes)
            {
                toReturn[0] = Types[0] + "=";
            }
            toReturn[0] += Values[0];

            for (var i = 1; i < length; i++)
            {
                if (!noTypes)
                {
                    toReturn[i] += Types[i] + "=";
                }
                toReturn[i] += Values[i];
            }

            return toReturn;
        }
    }
}