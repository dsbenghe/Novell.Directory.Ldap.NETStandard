﻿/******************************************************************************
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

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents a single entry in a directory, consisting of
    ///     a distinguished name (DN) and zero or more attributes.
    ///     An instance of
    ///     LdapEntry is created in order to add an entry to a directory, and
    ///     instances of LdapEntry are returned on a search by enumerating an
    ///     LdapSearchResults.
    /// </summary>
    /// <seealso cref="LdapAttribute">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    public class LdapEntry : IComparable
    {
        private LdapAttributeSet Attrs { get; }

        /// <summary>
        ///     Constructs a new entry with the specified distinguished name and set
        ///     of attributes.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the new entry. The
        ///     value is not validated. An invalid distinguished
        ///     name will cause operations using this entry to fail.
        /// </param>
        /// <param name="attrs">
        ///     The initial set of attributes assigned to the
        ///     entry.
        /// </param>
        public LdapEntry(string? dn = null, LdapAttributeSet? attrs = null)
        {
            dn ??= string.Empty;

            attrs ??= new LdapAttributeSet();

            Dn = dn;
            Attrs = attrs;
        }

        /// <summary>
        ///     Returns the distinguished name of the entry.
        /// </summary>
        /// <returns>
        ///     The distinguished name of the entry.
        /// </returns>
        public string Dn { get; }

        /// <summary>
        ///     Compares this object with the specified object for order.
        ///     Ordering is determined by comparing normalized DN values
        ///     (see {@link LdapEntry#getDN() } and
        ///     {@link LdapDN#normalize(java.lang.String)}) using the
        ///     compareTo method of the String class.
        /// </summary>
        /// <param name="entry">
        ///     Entry to compare to.
        /// </param>
        /// <returns>
        ///     A negative integer, zero, or a positive integer as this
        ///     object is less than, equal to, or greater than the specified object.
        /// </returns>
        public virtual int CompareTo(object? entry)
        {
            if (entry == null)
            {
                return 1;
            }

            return LdapDn.Normalize(Dn).CompareTo(LdapDn.Normalize(((LdapEntry)entry).Dn));
        }

        /// <summary>
        ///     Returns if there is an attribute with given name.
        /// </summary>
        /// <param name="attrName">
        ///     The name of the attribute or attributes to return.
        /// </param>
        /// <returns>
        ///     true if attribute exists, false otherwise.
        /// </returns>
        public bool Contains(string attrName)
        {
            return Attrs.ContainsKey(attrName);
        }

        /// <summary>
        ///     Returns the attribute matching the specified attrName.
        /// </summary>
        /// <param name="attrName">
        ///     The name of the attribute to return.
        /// </param>
        /// <returns>
        ///     A LdapAttribute.
        /// </returns>
        public LdapAttribute Get(string attrName)
        {
            return Attrs.GetAttribute(attrName);
        }

        /// <summary>
        ///     Returns the attribute matching the specified attrName or the fallback value if no attribute was found.
        /// </summary>
        /// <param name="attributeName">
        ///     The name of the attribute to return.
        /// </param>
        /// <returns>
        ///     A LdapAttribute.
        /// </returns>
        [return: NotNullIfNotNull("fallback")]
        public LdapAttribute? GetOrDefault(string attributeName, LdapAttribute? fallback = default)
        {
            return !Attrs.TryGetValue(attributeName, out var attribute) ? fallback : attribute;
        }

        /// <summary>
        ///     Returns the attribute matching the specified attrName or the fallback value if no attribute was found.
        /// </summary>
        /// <param name="attributeName">
        ///     The name of the attribute to return.
        /// </param>
        /// <returns>
        ///     The string attribute value.
        /// </returns>
        [return: NotNullIfNotNull("fallback")]
        public string? GetStringValueOrDefault(string attributeName, string? fallback = default)
        {
            return GetOrDefault(attributeName)?.StringValue ?? fallback;
        }

        /// <summary>
        ///     Returns the attribute matching the specified attrName or the fallback value if no attribute was found.
        /// </summary>
        /// <param name="attributeName">
        ///     The name of the attribute to return.
        /// </param>
        /// <returns>
        ///     The byte[] attribute value.
        /// </returns>
        [return: NotNullIfNotNull("fallback")]
        public byte[]? GetBytesValueOrDefault(string attributeName, byte[]? fallback = default)
        {
            return GetOrDefault(attributeName)?.ByteValue ?? fallback;
        }

        /// <summary>
        ///     Returns the attribute set of the entry.
        ///     All base and subtype variants of all attributes are
        ///     returned. The LdapAttributeSet returned may be
        ///     empty if there are no attributes in the entry.
        /// </summary>
        /// <returns>
        ///     The attribute set of the entry.
        /// </returns>
        public LdapAttributeSet GetAttributeSet()
        {
            return Attrs;
        }

        /// <summary>
        ///     Returns an attribute set from the entry, consisting of only those
        ///     attributes matching the specified subtypes.
        ///     The getAttributeSet method can be used to extract only
        ///     a particular language variant subtype of each attribute,
        ///     if it exists. The "subtype" may be, for example, "lang-ja", "binary",
        ///     or "lang-ja;phonetic". If more than one subtype is specified, separated
        ///     with a semicolon, only those attributes with all of the named
        ///     subtypes will be returned. The LdapAttributeSet returned may be
        ///     empty if there are no matching attributes in the entry.
        /// </summary>
        /// <param name="subtype">
        ///     One or more subtype specification(s), separated
        ///     with semicolons. The "lang-ja" and
        ///     "lang-en;phonetic" are valid subtype
        ///     specifications.
        /// </param>
        /// <returns>
        ///     An attribute set from the entry with the attributes that
        ///     match the specified subtypes or an empty set if no attributes
        ///     match.
        /// </returns>
        public LdapAttributeSet GetAttributeSet(string subtype)
        {
            return Attrs.GetSubset(subtype);
        }

        /// <summary>
        ///     Returns a string representation of this LdapEntry.
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapEntry.
        /// </returns>
        public override string ToString()
        {
            var result = new StringBuilder("LdapEntry: ");
            if (Dn != null)
            {
                result.Append(Dn + "; ");
            }

            if (Attrs != null)
            {
                result.Append(Attrs);
            }

            return result.ToString();
        }
    }
}
