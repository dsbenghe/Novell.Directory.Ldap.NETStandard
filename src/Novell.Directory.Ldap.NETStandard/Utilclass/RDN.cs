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

using System;
using System.Collections.Generic;

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
    ///     'cn=juan + l=ES'.
    /// </summary>
    /// <seealso cref="Dn">
    /// </seealso>
    public class Rdn : object
    {
        private readonly List<string> _types;
        private readonly List<string> _values;

        /// <summary>
        ///     Creates an RDN object from the DN component specified in the string RDN.
        /// </summary>
        /// <param name="rdn">
        ///     the DN component.
        /// </param>
        public Rdn(string rdn)
        {
            RawValue = rdn;
            var dn = new Dn(rdn);
            var rdns = dn.RdNs;

            // there should only be one rdn
            if (rdns.Count != 1)
            {
                throw new ArgumentException("Invalid RDN: see API " + "documentation");
            }

            var thisRdn = rdns[0];
            _types = thisRdn._types;
            _values = thisRdn._values;
            RawValue = thisRdn.RawValue;
        }

        public Rdn()
        {
            _types = new List<string>();
            _values = new List<string>();
            RawValue = string.Empty;
        }

        /// <summary>
        ///     Returns the actually Raw String before Normalization.
        /// </summary>
        /// <returns>
        ///     The raw string.
        /// </returns>
        internal string RawValue { get; private set; }

        /// <summary>
        ///     Returns the type of this RDN.  This method assumes that only one value
        ///     is used, If multivalues attributes are used only the first Type is
        ///     returned.  Use GetTypes.
        /// </summary>
        /// <returns>
        ///     Type of attribute.
        /// </returns>
        public string Type => _types[0];

        /// <summary> Returns all the types for this RDN.</summary>
        /// <returns>
        ///     list of types.
        /// </returns>
        public string[] Types => _types.ToArray();

        /// <summary>
        ///     Returns the values of this RDN.  If multivalues attributes are used only
        ///     the first Type is returned.  Use GetTypes.
        /// </summary>
        /// <returns>
        ///     Type of attribute.
        /// </returns>
        public string Value => _values[0];

        /// <summary> Returns all the values for this RDN.</summary>
        /// <returns>
        ///     list of values.
        /// </returns>
        public string[] Values => _values.ToArray();

        /// <summary> Determines if this RDN is multivalued or not.</summary>
        /// <returns>
        ///     true if this RDN is multivalued.
        /// </returns>
        public bool Multivalued => _values.Count > 1 ? true : false;

        /// <summary>
        ///     Compares the RDN to the rdn passed.  Note: If an there exist any
        ///     mulivalues in one RDN they must all be present in the other.
        /// </summary>
        /// <param name="rdn">
        ///     the RDN to compare to
        ///     @throws IllegalArgumentException if the application compares a name
        ///     with an OID.
        /// </param>
        public bool Equals(Rdn rdn)
        {
            if (_values.Count != rdn._values.Count)
            {
                return false;
            }

            for (var i = 0; i < _values.Count; i++)
            {
                // verify that the current value and type exists in the other list
                var j = 0;

                var valuesEqual = _values[i].EqualsOrdinalCI(rdn._values[j]);
                var isEqualAttrType = EqualAttrType(_types[i], rdn._types[j]);

                while (j < _values.Count && (!valuesEqual || !isEqualAttrType))
                {
                    j++;
                }

                // couldn't find first value
                if (j >= rdn._values.Count)
                {
                    return false;
                }
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
            // If it starts with a number, it's considered an OID
            if (char.IsDigit(attr1[0]) != char.IsDigit(attr2[0]))
            {
                throw new ArgumentException("OID numbers can not be compared to attribute names");
            }

            return attr1.EqualsOrdinalCI(attr2);
        }

        /// <summary>
        ///     Adds another value to the RDN.  Only one attribute type is allowed for
        ///     the RDN.
        /// </summary>
        /// <param name="attrType">
        ///     Attribute type, could be an OID or String.
        /// </param>
        /// <param name="attrValue">
        ///     Attribute Value, must be normalized and escaped.
        /// </param>
        /// <param name="rawValue">
        ///     or text before normalization, can be Null.
        /// </param>
        public void Add(string attrType, string attrValue, string rawValue)
        {
            _types.Add(attrType);
            _values.Add(attrValue);
            RawValue += rawValue;
        }

        /// <summary>
        ///     Creates a string that represents this RDN, according to RFC 2253.
        /// </summary>
        /// <returns>
        ///     An RDN string.
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
        ///     An RDN string.
        /// </returns>
        public string ToString(bool noTypes)
        {
            var length = _types.Count;
            var toReturn = string.Empty;
            if (length < 1)
            {
                return null;
            }

            if (!noTypes)
            {
                toReturn = _types[0] + "=";
            }

            toReturn += _values[0];

            for (var i = 1; i < length; i++)
            {
                toReturn += "+";
                if (!noTypes)
                {
                    toReturn += _types[i] + "=";
                }

                toReturn += _values[i];
            }

            return toReturn;
        }

        /// <summary>
        ///     Returns each multivalued name in the current RDN as an array of Strings.
        /// </summary>
        /// <param name="noTypes">
        ///     Specifies whether Attribute types are included. The attribute
        ///     type names will be ommitted if the parameter noTypes is true.
        /// </param>
        /// <returns>
        ///     List of multivalued Attributes.
        /// </returns>
        public string[] ExplodeRdn(bool noTypes)
        {
            var length = _types.Count;
            if (length < 1)
            {
                return null;
            }

            var toReturn = new string[_types.Count];

            if (!noTypes)
            {
                toReturn[0] = _types[0] + "=";
            }

            toReturn[0] += _values[0];

            for (var i = 1; i < length; i++)
            {
                if (!noTypes)
                {
                    toReturn[i] += _types[i] + "=";
                }

                toReturn[i] += _values[i];
            }

            return toReturn;
        }
    }
}
