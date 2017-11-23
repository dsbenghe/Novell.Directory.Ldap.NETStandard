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
// Novell.Directory.Ldap.LdapCompareAttrNames.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Globalization;
using Novell.Directory.Ldap.Utilclass;
using System.Collections.Generic;
using System;
using System.Linq;

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
        private void InitBlock()
        {
            //			location = Locale.getDefault();
            _location = CultureInfo.CurrentCulture;
            _collator = CultureInfo.CurrentCulture.CompareInfo;
        }

        /// <summary>
        ///     Returns the locale to be used for sorting, if a locale has been
        ///     specified.
        ///     If locale is null, a basic String.compareTo method is used for
        ///     collation.  If non-null, a locale-specific collation is used.
        /// </summary>
        /// <returns>
        ///     The locale if one has been specified
        /// </returns>
        /// <summary>
        ///     Sets the locale to be used for sorting.
        /// </summary>
        /// <param name="locale">
        ///     The locale to be used for sorting.
        /// </param>
        public virtual CultureInfo Locale
        {
            get
            {
                //currently supports only English local.
                return _location;
            }

            set
            {
                _collator = value.CompareInfo;
                _location = value;
            }
        }

        private readonly IList<(string Name, bool Asceding)> _infos = new List<(string Name, bool Asceding)>();
        private CultureInfo _location;
        private CompareInfo _collator;

        /// <summary>
        ///     Constructs an object that sorts results by a single attribute, in
        ///     ascending order.
        /// </summary>
        /// <param name="attrName">
        ///     Name of an attribute by which to sort.
        /// </param>
        public LdapCompareAttrNames(string attrName)
            : this(attrName, true)
        {

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
            InitBlock();
            _infos.Add((attrName, ascendingFlag));
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
        public LdapCompareAttrNames(IEnumerable<string> attrNames)
        {
            InitBlock();
            foreach (var name in attrNames)
                _infos.Add((name, true));
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
        public LdapCompareAttrNames(IEnumerable<string> attrNames, IEnumerable<bool> ascendingFlags)
        {
            InitBlock();
            string[] names = attrNames.ToArray();
            bool[] ascending = ascendingFlags.ToArray();
            if (names.Length != ascending.Length)
            {
                throw new LdapException(ExceptionMessages.UNEQUAL_LENGTHS, LdapException.INAPPROPRIATE_MATCHING, null);
            }

            for (var i = 0; i < names.Length; i++)
            {
                _infos.Add((names[i], ascending[i]));
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
        public LdapCompareAttrNames(IEnumerable<(string, bool)> infos)
        {
            InitBlock();

            foreach (var info in infos)
                _infos.Add(info);
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
            LdapEntry entry1 = object1 as LdapEntry;
            LdapEntry entry2 = object2 as LdapEntry;
            string[] first; //multivalued attributes are ignored.
            string[] second; //we just use the first element
            int compare = 0, i =0;
            if (_collator == null)
            {
                //using default locale
                _collator = CultureInfo.CurrentCulture.CompareInfo;
            }
            for (i = 0; compare == 0 && i < _infos.Count; i++)
            {

                LdapAttribute one = entry1.GetAttribute(_infos[i].Name);
                LdapAttribute two = entry2.GetAttribute(_infos[i].Name);
                if (one != null && two != null)
                {
                    first = one.StringValueArray;
                    second = two.StringValueArray;
                    compare = _collator.Compare(first[0], second[0]);
                }
                //We could also use the other multivalued attributes to break ties.
                //one of the entries was null
                else
                {
                    if (one != null)
                        compare = -1;
                    //one is greater than two
                    else if (two != null)
                        compare = 1;
                    //one is lesser than two
                    else
                        compare = 0; //tie - break it with the next attribute name
                }
            }

            if (_infos[i - 1].Asceding)
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
        ///     true the comparators are equal
        /// </returns>
        public override bool Equals(object comparator)
        {
            if (!(comparator is LdapCompareAttrNames))
            {
                return false;
            }
            LdapCompareAttrNames comp = comparator as LdapCompareAttrNames;

            // Test to see if the attribute to compare are the same length
            if (comp._infos.Count != _infos.Count)
                return false;

            // Test to see if the attribute names and sorting orders are the same.
            for (var i = 0; i < _infos.Count; i++)
            {
                if (comp._infos[i].Asceding != _infos[i].Asceding)
                    return false;
                if (comp._infos[i].Name.Equals(_infos[i].Name, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }
            return true;
        }
    }
}