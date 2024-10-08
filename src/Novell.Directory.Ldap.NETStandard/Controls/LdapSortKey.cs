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

namespace Novell.Directory.Ldap.Controls
{
    /// <summary>
    ///     Encapsulates parameters for sorting search results.
    /// </summary>
    public class LdapSortKey
    {
        // Constructors

        /// <summary>
        ///     Constructs a new LdapSortKey object using an attribute as the sort key.
        /// </summary>
        /// <param name="keyDescription">
        ///     The single attribute to use for sorting. If the
        ///     name is preceded by a minus sign (-), the sorting
        ///     is done in reverse (descending) order.
        ///     An OID for a matching rule may be appended
        ///     following a ":".
        ///     Examples:.
        ///     <ul>
        ///         <li> "cn" (sorts in ascending order by the cn attribute)</li>
        ///         <li> "-cn" (sorts in descending order by the cn attribute) </li>
        ///         <li>
        ///             "cn:1.2.3.4.5" (sorts in ascending order by the cn attribute
        ///             using the matching rule 1.2.3.4.5)
        ///         </li>
        ///     </ul>
        /// </param>
        public LdapSortKey(string keyDescription)
        {
            MatchRule = null;
            Reverse = false;
            var myKey = keyDescription;
            if (myKey[0] == '-')
            {
                myKey = myKey.Substring(1);
                Reverse = true;
            }

            var pos = myKey.IndexOf(':');
            if (pos != -1)
            {
                Key = myKey.Substring(0, pos - 0);
                MatchRule = myKey.Substring(pos + 1);
            }
            else
            {
                Key = myKey;
            }
        }

        /// <summary>
        ///     Constructs a new LdapSortKey object with the specified attribute name
        ///     and sort order.
        /// </summary>
        /// <param name="key">
        ///     The single attribute to use for sorting.
        /// </param>
        /// <param name="reverse">
        ///     If true, sorting is done in descending order. If false,
        ///     sorting is done in ascending order.
        /// </param>
        public LdapSortKey(string key, bool reverse)
            : this(key, reverse, null)
        {
        }

        /// <summary>
        ///     Constructs a new LdapSortKey object with the specified attribute name,
        ///     sort order, and a matching rule.
        /// </summary>
        /// <param name="key">
        ///     The attribute name (for example, "cn") to use for sorting.
        /// </param>
        /// <param name="reverse">
        ///     If true, sorting is done in descending order. If false,
        ///     sorting is done in ascending order.
        /// </param>
        /// <param name="matchRule">
        ///     The object ID (OID) of a matching rule used for
        ///     collation. If the object will be used to request
        ///     server-side sorting of search results, it should
        ///     be the OID of a matching rule known to be
        ///     supported by that server.
        /// </param>
        public LdapSortKey(string key, bool reverse, string matchRule)
        {
            Key = key;
            Reverse = reverse;
            MatchRule = matchRule;
        }

        /// <summary>
        ///     Returns the attribute to used for sorting.
        /// </summary>
        /// <returns>
        ///     The name of the attribute used for sorting.
        /// </returns>
        public string Key { get; }

        /// <summary>
        ///     Returns the sorting order, ascending or descending.
        /// </summary>
        /// <returns>
        ///     True if the sorting is done is descending order; false, if the
        ///     sorting is done is ascending order.
        /// </returns>
        public bool Reverse { get; }

        /// <summary>
        ///     Returns the OID to be used as a matching rule.
        /// </summary>
        /// <returns>
        ///     The OID to be used as matching rule, or null if none is to be
        ///     used.
        /// </returns>
        public string MatchRule { get; }
    }
}
