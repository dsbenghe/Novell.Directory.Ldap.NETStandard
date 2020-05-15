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
// Novell.Directory.Ldap.LdapAttributeSet.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     A set of {@link LdapAttribute} objects.
    ///     An. <code>LdapAttributeSet</code> is a collection of. <code>LdapAttribute</code>
    ///     classes as returned from an. <code>LdapEntry</code> on a search or read
    ///     operation. <code>LdapAttributeSet</code> may be also used to contruct an entry
    ///     to be added to a directory.  If the. <code>add()</code> or. <code>addAll()</code>
    ///     methods are called and one or more of the objects to be added is not
    ///     an. <code>LdapAttribute, ClassCastException</code> is thrown (as discussed in the
    ///     documentation for. <code>java.util.Collection</code>).
    /// </summary>
    /// <seealso cref="LdapAttribute">
    /// </seealso>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    public class LdapAttributeSet : Dictionary<string, LdapAttribute>
    {
        /// <summary> Constructs an empty set of attributes.</summary>
        public LdapAttributeSet() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        ///     Returns the attribute matching the specified attrName.
        ///     For example:
        ///     <ul>
        ///         <li><code>getAttribute("cn")</code>      returns only the "cn" attribute</li>
        ///         <li>
        ///             <code>getAttribute("cn;lang-en")</code> returns only the "cn;lang-en"
        ///             attribute.
        ///         </li>
        ///     </ul>
        ///     In both cases,. <code>null</code> is returned if there is no exact match to
        ///     the specified attrName.
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </summary>
        /// <param name="attrName">
        ///     The name of an attribute to retrieve, with or without
        ///     subtype specifications. For example, "cn", "cn;phonetic", and
        ///     "cn;binary" are valid attribute names.
        /// </param>
        /// <returns>
        ///     The attribute matching the specified attrName.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown when the attribute is not found</exception>
        public LdapAttribute GetAttribute(string attrName)
        {
            return this[attrName];
        }

        /// <summary>
        ///     Returns a single best-match attribute, or. <code>null</code> if no match is
        ///     available in the entry.
        ///     Ldap version 3 allows adding a subtype specification to an attribute
        ///     name. For example, "cn;lang-ja" indicates a Japanese language
        ///     subtype of the "cn" attribute and "cn;lang-ja-JP-kanji" may be a subtype
        ///     of "cn;lang-ja". This feature may be used to provide multiple
        ///     localizations in the same directory. For attributes which do not vary
        ///     among localizations, only the base attribute may be stored, whereas
        ///     for others there may be varying degrees of specialization.
        ///     For example,. <code>getAttribute(attrName,lang)</code> returns the.
        ///     <code>LdapAttribute</code> that exactly matches attrName and that
        ///     best matches lang.
        ///     If there are subtypes other than "lang" subtypes included
        ///     in attrName, for example, "cn;binary", only attributes with all of
        ///     those subtypes are returned. If lang is. <code>null</code> or empty, the
        ///     method behaves as getAttribute(attrName). If there are no matching
        ///     attributes,. <code>null</code> is returned.
        ///     Assume the entry contains only the following attributes:
        ///     <ul>
        ///         <li>cn;lang-en</li>
        ///         <li>cn;lang-ja-JP-kanji</li>
        ///         <li>sn</li>
        ///     </ul>
        ///     Examples:
        ///     <ul>
        ///         <li><code>getAttribute( "cn" )</code>       returns <code>null</code>.</li>
        ///         <li><code>getAttribute( "sn" )</code>       returns the "sn" attribute.</li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-en-us" )</code>
        ///             returns the "cn;lang-en" attribute.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-en" )</code>
        ///             returns the "cn;lang-en" attribute.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "cn", "lang-ja" )</code>
        ///             returns <code>null</code>.
        ///         </li>
        ///         <li>
        ///             <code>getAttribute( "sn", "lang-en" )</code>
        ///             returns the "sn" attribute.
        ///         </li>
        ///     </ul>
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </summary>
        /// <param name="attrName">
        ///     The name of an attribute to retrieve, with or without
        ///     subtype specifications. For example, "cn", "cn;phonetic", and
        ///     cn;binary" are valid attribute names.
        /// </param>
        /// <param name="lang">
        ///     A language specification with optional subtypes
        ///     appended using "-" as separator. For example, "lang-en", "lang-en-us",
        ///     "lang-ja", and "lang-ja-JP-kanji" are valid language specification.
        /// </param>
        /// <returns>
        ///     A single best-match. <code>LdapAttribute</code>
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown when the attribute is not found</exception>
        public LdapAttribute GetAttribute(string attrName, string lang)
        {
            var key = attrName + ";" + lang;
            return this[key];
        }

        /// <summary>
        ///     Creates a new attribute set containing only the attributes that have
        ///     the specified subtypes.
        ///     For example, suppose an attribute set contains the following
        ///     attributes:
        ///     <ul>
        ///         <li>    cn</li>
        ///         <li>    cn;lang-ja</li>
        ///         <li>    sn;phonetic;lang-ja</li>
        ///         <li>    sn;lang-us</li>
        ///     </ul>
        ///     Calling the. <code>getSubset</code> method and passing lang-ja as the
        ///     argument, the method returns an attribute set containing the following
        ///     attributes:.
        ///     <ul>
        ///         <li>cn;lang-ja</li>
        ///         <li>sn;phonetic;lang-ja</li>
        ///     </ul>
        /// </summary>
        /// <param name="subtype">
        ///     Semi-colon delimited list of subtypes to include. For
        ///     example:
        ///     <ul>
        ///         <li> "lang-ja" specifies only Japanese language subtypes</li>
        ///         <li> "binary" specifies only binary subtypes</li>
        ///         <li>
        ///             "binary;lang-ja" specifies only Japanese language subtypes
        ///             which also are binary
        ///         </li>
        ///     </ul>
        ///     Note: Novell eDirectory does not currently support language subtypes.
        ///     It does support the "binary" subtype.
        /// </param>
        /// <returns>
        ///     An attribute set containing the attributes that match the
        ///     specified subtype.
        /// </returns>
        public LdapAttributeSet GetSubset(string subtype)
        {
            // Create a new tempAttributeSet
            var tempAttributeSet = new LdapAttributeSet();
            var i = GetEnumerator();

            // Cycle throught this.attributeSet
            while (i.MoveNext())
            {
                var attr = i.Current;

                // Does this attribute have the subtype we are looking for. If
                // yes then add it to our AttributeSet, else next attribute
                if (attr.HasSubtype(subtype))
                {
                    tempAttributeSet.Add(attr.Clone());
                }
            }

            return tempAttributeSet;
        }

        // --- methods defined in set ---

        /// <summary>
        ///     Returns an iterator over the attributes in this set.  The attributes
        ///     returned from this iterator are not in any particular order.
        /// </summary>
        /// <returns>
        ///     iterator over the attributes in this set.
        /// </returns>
        public new IEnumerator<LdapAttribute> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        ///     Returns. <code>true</code> if this set contains no elements.
        /// </summary>
        /// <returns>
        ///     <code>true</code> if this set contains no elements.
        /// </returns>
        public bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        ///     Adds the specified attribute to this set if it is not already present.
        ///     If an attribute with the same name already exists in the set then the
        ///     specified attribute will not be added.
        /// </summary>
        /// <param name="attribute">
        ///     Object of type. <code>LdapAttribute</code>
        /// </param>
        /// <returns>
        ///     true if the attribute was added.
        ///     @throws ClassCastException occurs the specified Object
        ///     is not of type. <code>LdapAttribute</code>.
        /// </returns>
        public bool Add(LdapAttribute attribute)
        {
            var name = attribute.Name;
            if (ContainsKey(name))
            {
                return false;
            }

            this[name] = attribute;
            return true;
        }

        /// <summary>
        ///     Returns a string representation of this LdapAttributeSet.
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttributeSet.
        /// </returns>
        public override string ToString()
        {
            var retValue = new StringBuilder("LdapAttributeSet: ");
            var attrs = GetEnumerator();
            var first = true;
            while (attrs.MoveNext())
            {
                if (!first)
                {
                    retValue.Append(" ");
                }

                first = false;
                var attr = attrs.Current;
                retValue.Append(attr);
            }

            return retValue.ToString();
        }
    }
}