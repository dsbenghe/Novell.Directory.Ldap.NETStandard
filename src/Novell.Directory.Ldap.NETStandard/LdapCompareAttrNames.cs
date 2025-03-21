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

using Novell.Directory.Ldap.Utilclass;
using System.Collections;
using System.Globalization;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Compares Ldap entries based on attribute name.
    ///     An object of this class defines ordering when sorting LdapEntries,
    ///     usually from search results.  When using this Comparator, LdapEntry objects
    ///     are sorted by the attribute names(s) passed in on the
    ///     constructor, in ascending or descending order.  The object is typically
    ///     supplied to an implementation of the collection interfaces such as
    ///     java.util.TreeSet which performs sorting.
    ///     Comparison is performed via locale-sensitive Java String comparison,
    ///     which may not correspond to the Ldap ordering rules by which an Ldap server
    ///     would sort them.
    /// </summary>
    public class LdapCompareAttrNames : IComparer
    {
        private readonly bool[] _sortAscending; // true if sorting ascending

        private readonly string[] _sortByNames; // names to to sort by.
        private CompareInfo _collator = CultureInfo.CurrentCulture.CompareInfo;
        private CultureInfo _location = CultureInfo.CurrentCulture;

        /// <summary>
        ///     Constructs an object that sorts results by a single attribute, in
        ///     ascending order.
        /// </summary>
        /// <param name="attrName">
        ///     Name of an attribute by which to sort.
        /// </param>
        public LdapCompareAttrNames(string attrName)
        {
            _sortByNames = new string[1];
            _sortByNames[0] = attrName;
            _sortAscending = new bool[1];
            _sortAscending[0] = true;
        }

        /// <summary>
        ///     Constructs an object that sorts results by a single attribute, in
        ///     either ascending or descending order.
        /// </summary>
        /// <param name="attrName">
        ///     Name of an attribute to sort by.
        /// </param>
        /// <param name="ascendingFlag">
        ///     True specifies ascending order; false specifies
        ///     descending order.
        /// </param>
        public LdapCompareAttrNames(string attrName, bool ascendingFlag)
        {
            _sortByNames = new string[1];
            _sortByNames[0] = attrName;
            _sortAscending = new bool[1];
            _sortAscending[0] = ascendingFlag;
        }

        /// <summary>
        ///     Constructs an object that sorts by one or more attributes, in the
        ///     order provided, in ascending order.
        ///     Note: Novell eDirectory allows sorting by one attribute only. The
        ///     direcctory server must also be configured to index the specified
        ///     attribute.
        /// </summary>
        /// <param name="attrNames">
        ///     Array of names of attributes to sort by.
        /// </param>
        public LdapCompareAttrNames(string[] attrNames)
        {
            _sortByNames = new string[attrNames.Length];
            _sortAscending = new bool[attrNames.Length];
            for (var i = 0; i < attrNames.Length; i++)
            {
                _sortByNames[i] = attrNames[i];
                _sortAscending[i] = true;
            }
        }

        /// <summary>
        ///     Constructs an object that sorts by one or more attributes, in the
        ///     order provided, in either ascending or descending order for each
        ///     attribute.
        ///     Note: Novell eDirectory supports only ascending sort order (A,B,C ...)
        ///     and allows sorting only by one attribute. The directory server must be
        ///     configured to index this attribute.
        /// </summary>
        /// <param name="attrNames">
        ///     Array of names of attributes to sort by.
        /// </param>
        /// <param name="ascendingFlags">
        ///     Array of flags, one for each attrName, where
        ///     true specifies ascending order and false specifies
        ///     descending order. An LdapException is thrown if
        ///     the length of ascendingFlags is not greater than
        ///     or equal to the length of attrNames.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        public LdapCompareAttrNames(string[] attrNames, bool[] ascendingFlags)
        {
            if (attrNames.Length != ascendingFlags.Length)
            {
                throw new LdapException(ExceptionMessages.UnequalLengths, LdapException.InappropriateMatching, null);

                // "Length of attribute Name array does not equal length of Flags array"
            }

            _sortByNames = new string[attrNames.Length];
            _sortAscending = new bool[ascendingFlags.Length];
            for (var i = 0; i < attrNames.Length; i++)
            {
                _sortByNames[i] = attrNames[i];
                _sortAscending[i] = ascendingFlags[i];
            }
        }

        /// <summary>
        ///     Returns the locale to be used for sorting, if a locale has been
        ///     specified.
        ///     If locale is null, a basic String.compareTo method is used for
        ///     collation.  If non-null, a locale-specific collation is used.
        /// </summary>
        public virtual CultureInfo Locale
        {
            get => _location;

            set
            {
                _collator = value.CompareInfo;
                _location = value;
            }
        }

        /// <summary>
        ///     Compares the the attributes of the first LdapEntry to the second.
        ///     Only the values of the attributes named at the construction of this
        ///     object will be compared.  Multi-valued attributes compare on the first
        ///     value only.
        /// </summary>
        /// <param name="object1">
        ///     Target entry for comparison.
        /// </param>
        /// <param name="object2">
        ///     Entry to be compared to.
        /// </param>
        /// <returns>
        ///     Negative value if the first entry is less than the second and
        ///     positive if the first is greater than the second.  Zero is returned if all
        ///     attributes to be compared are the same.
        /// </returns>
        public virtual int Compare(object object1, object object2)
        {
            var entry1 = (LdapEntry)object1;
            var entry2 = (LdapEntry)object2;
            LdapAttribute one, two;
            string[] first; // multivalued attributes are ignored.
            string[] second; // we just use the first element
            int compare, i = 0;
            if (_collator == null)
            {
                // using default locale
                _collator = CultureInfo.CurrentCulture.CompareInfo;
            }

            do
            {
                // while first and second are equal
                one = entry1.Get(_sortByNames[i]);
                two = entry2.Get(_sortByNames[i]);
                if (one != null && two != null)
                {
                    first = one.StringValueArray;
                    second = two.StringValueArray;
                    compare = _collator.Compare(first[0], second[0]);
                }

                // We could also use the other multivalued attributes to break ties.
                // one of the entries was null
                else
                {
                    if (one != null)
                    {
                        compare = -1;
                    }

                    // one is greater than two
                    else if (two != null)
                    {
                        compare = 1;
                    }

                    // one is lesser than two
                    else
                    {
                        compare = 0; // tie - break it with the next attribute name
                    }
                }

                i++;
            }
            while (compare == 0 && i < _sortByNames.Length);

            if (_sortAscending[i - 1])
            {
                // return the normal ascending comparison.
                return compare;
            }

            // negate the comparison for a descending comparison.
            return -compare;
        }

        /// <summary>
        ///     Determines if this comparator is equal to the comparator passed in.
        ///     This will return true if the comparator is an instance of
        ///     LdapCompareAttrNames and compares the same attributes names in the same
        ///     order.
        /// </summary>
        /// <returns>
        ///     true the comparators are equal.
        /// </returns>
        public override bool Equals(object comparator)
        {
            if (comparator is not LdapCompareAttrNames comp)
            {
                return false;
            }

            // Test to see if the attribute to compare are the same length
            if (comp._sortByNames.Length != _sortByNames.Length || comp._sortAscending.Length != _sortAscending.Length)
            {
                return false;
            }

            // Test to see if the attribute names and sorting orders are the same.
            for (var i = 0; i < _sortByNames.Length; i++)
            {
                if (comp._sortAscending[i] != _sortAscending[i])
                {
                    return false;
                }

                if (!comp._sortByNames[i].EqualsOrdinalCI(_sortByNames[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _sortAscending != null ? _sortAscending.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (_sortByNames != null ? _sortByNames.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_collator != null ? _collator.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_location != null ? _location.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
