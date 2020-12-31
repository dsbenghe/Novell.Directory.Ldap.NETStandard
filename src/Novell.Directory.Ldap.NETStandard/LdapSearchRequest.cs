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
using Novell.Directory.Ldap.Rfc2251;
using System.Collections;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents an Ldap Search request.
    /// </summary>
    /// <seealso cref="LdapConnection.SendRequest">
    /// </seealso>
    /*
     *       SearchRequest ::= [APPLICATION 3] SEQUENCE {
     *               baseObject      LdapDN,
     *               scope           ENUMERATED {
     *                       baseObject              (0),
     *                       singleLevel             (1),
     *                       wholeSubtree            (2) },
     *               derefAliases    ENUMERATED {
     *                       neverDerefAliases       (0),
     *                       derefInSearching        (1),
     *                       derefFindingBaseObj     (2),
     *                       derefAlways             (3) },
     *               sizeLimit       INTEGER (0 .. maxInt),
     *               timeLimit       INTEGER (0 .. maxInt),
     *               typesOnly       BOOLEAN,
     *               filter          Filter,
     *               attributes      AttributeDescriptionList }
     */
    public class LdapSearchRequest : LdapMessage
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapSearchRequest>();

        // *************************************************************************
        // Public variables for Filter
        // *************************************************************************

        /// <summary> Search Filter Identifier for an AND component.</summary>
        public const int And = 0;

        /// <summary> Search Filter Identifier for an OR component.</summary>
        public const int Or = 1;

        /// <summary> Search Filter Identifier for a NOT component.</summary>
        public const int Not = 2;

        /// <summary> Search Filter Identifier for an EQUALITY_MATCH component.</summary>
        public const int EqualityMatch = 3;

        /// <summary> Search Filter Identifier for a SUBSTRINGS component.</summary>
        public const int Substrings = 4;

        /// <summary> Search Filter Identifier for a GREATER_OR_EQUAL component.</summary>
        public const int GreaterOrEqual = 5;

        /// <summary> Search Filter Identifier for a LESS_OR_EQUAL component.</summary>
        public const int LessOrEqual = 6;

        /// <summary> Search Filter Identifier for a PRESENT component.</summary>
        public const int Present = 7;

        /// <summary> Search Filter Identifier for an APPROX_MATCH component.</summary>
        public const int ApproxMatch = 8;

        /// <summary> Search Filter Identifier for an EXTENSIBLE_MATCH component.</summary>
        public const int ExtensibleMatch = 9;

        /// <summary>
        ///     Search Filter Identifier for an INITIAL component of a SUBSTRING.
        ///     Note: An initial SUBSTRING is represented as "value*".
        /// </summary>
        public const int Initial = 0;

        /// <summary>
        ///     Search Filter Identifier for an ANY component of a SUBSTRING.
        ///     Note: An ANY SUBSTRING is represented as "*value*".
        /// </summary>
        public const int Any = 1;

        /// <summary>
        ///     Search Filter Identifier for a FINAL component of a SUBSTRING.
        ///     Note: A FINAL SUBSTRING is represented as "*value".
        /// </summary>
        public const int Final = 2;

        /// <summary>
        ///     Constructs an Ldap Search Request.
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:.
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     The names of attributes to retrieve.
        ///     operation exceeds the time limit.
        /// </param>
        /// <param name="dereference">
        ///     Specifies when aliases should be dereferenced.
        ///     Must be one of the constants defined in
        ///     LdapConstraints, which are DEREF_NEVER,
        ///     DEREF_FINDING, DEREF_SEARCHING, or DEREF_ALWAYS.
        /// </param>
        /// <param name="maxResults">
        ///     The maximum number of search results to return
        ///     for a search request.
        ///     The search operation will be terminated by the server
        ///     with an LdapException.SIZE_LIMIT_EXCEEDED if the
        ///     number of results exceed the maximum.
        /// </param>
        /// <param name="serverTimeLimit">
        ///     The maximum time in seconds that the server
        ///     should spend returning search results. This is a
        ///     server-enforced limit.  A value of 0 means
        ///     no time limit.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found.  If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the search request.
        ///     or null if none.
        /// </param>
        /// <seealso cref="Novell.Directory.Ldap.LdapConnection.Search">
        /// </seealso>
        /// <seealso cref="Novell.Directory.Ldap.LdapSearchConstraints">
        /// </seealso>
        public LdapSearchRequest(string baseRenamed, int scope, string filter, string[] attrs, int dereference,
            int maxResults, int serverTimeLimit, bool typesOnly, LdapControl[] cont)
            : base(
                SearchRequest,
                new RfcSearchRequest(new RfcLdapDn(baseRenamed), new Asn1Enumerated(scope),
                    new Asn1Enumerated(dereference), new Asn1Integer(maxResults), new Asn1Integer(serverTimeLimit),
                    new Asn1Boolean(typesOnly), new RfcFilter(filter), new RfcAttributeDescriptionList(attrs)), cont)
        {
        }

        /// <summary>
        ///     Constructs an Ldap Search Request with a filter in Asn1 format.
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:.
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     The names of attributes to retrieve.
        ///     operation exceeds the time limit.
        /// </param>
        /// <param name="dereference">
        ///     Specifies when aliases should be dereferenced.
        ///     Must be either one of the constants defined in
        ///     LdapConstraints, which are DEREF_NEVER,
        ///     DEREF_FINDING, DEREF_SEARCHING, or DEREF_ALWAYS.
        /// </param>
        /// <param name="maxResults">
        ///     The maximum number of search results to return
        ///     for a search request.
        ///     The search operation will be terminated by the server
        ///     with an LdapException.SIZE_LIMIT_EXCEEDED if the
        ///     number of results exceed the maximum.
        /// </param>
        /// <param name="serverTimeLimit">
        ///     The maximum time in seconds that the server
        ///     should spend returning search results. This is a
        ///     server-enforced limit.  A value of 0 means
        ///     no time limit.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found.  If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the search request.
        ///     or null if none.
        /// </param>
        /// <seealso cref="Novell.Directory.Ldap.LdapConnection.Search">
        /// </seealso>
        /// <seealso cref="Novell.Directory.Ldap.LdapSearchConstraints">
        /// </seealso>
        public LdapSearchRequest(string baseRenamed, int scope, RfcFilter filter, string[] attrs, int dereference,
            int maxResults, int serverTimeLimit, bool typesOnly, LdapControl[] cont)
            : base(
                SearchRequest,
                new RfcSearchRequest(new RfcLdapDn(baseRenamed), new Asn1Enumerated(scope),
                    new Asn1Enumerated(dereference), new Asn1Integer(maxResults), new Asn1Integer(serverTimeLimit),
                    new Asn1Boolean(typesOnly), filter, new RfcAttributeDescriptionList(attrs)), cont)
        {
        }

        /// <summary>
        ///     Retrieves the Base DN for a search request.
        /// </summary>
        /// <returns>
        ///     the base DN for a search request.
        /// </returns>
        public string Dn => Asn1Object.RequestDn;

        /// <summary> Retrieves the scope of a search request.</summary>
        /// <returns>
        ///     scope of a search request.
        /// </returns>
        /// <seealso cref="LdapConnection.ScopeBase">
        /// </seealso>
        /// <seealso cref="LdapConnection.ScopeOne">
        /// </seealso>
        /// <seealso cref="LdapConnection.ScopeSub">
        /// </seealso>
        public int Scope => ((Asn1Enumerated)((RfcSearchRequest)Asn1Object.get_Renamed(1)).get_Renamed(1)).IntValue();

        /// <summary> Retrieves the behaviour of dereferencing aliases on a search request.</summary>
        /// <returns>
        ///     integer representing how to dereference aliases.
        /// </returns>
        /// <seealso cref="LdapSearchConstraints.DerefAlways">
        /// </seealso>
        /// <seealso cref="LdapSearchConstraints.DerefFinding">
        /// </seealso>
        /// <seealso cref="LdapSearchConstraints.DerefNever">
        /// </seealso>
        /// <seealso cref="LdapSearchConstraints.DerefSearching">
        /// </seealso>
        public int Dereference =>
            ((Asn1Enumerated)((RfcSearchRequest)Asn1Object.get_Renamed(1)).get_Renamed(2)).IntValue();

        /// <summary>
        ///     Retrieves the maximum number of entries to be returned on a search.
        /// </summary>
        /// <returns>
        ///     Maximum number of search entries.
        /// </returns>
        public int MaxResults =>
            ((Asn1Integer)((RfcSearchRequest)Asn1Object.get_Renamed(1)).get_Renamed(3)).IntValue();

        /// <summary>
        ///     Retrieves the server time limit for a search request.
        /// </summary>
        /// <returns>
        ///     server time limit in nanoseconds.
        /// </returns>
        public int ServerTimeLimit =>
            ((Asn1Integer)((RfcSearchRequest)Asn1Object.get_Renamed(1)).get_Renamed(4)).IntValue();

        /// <summary>
        ///     Retrieves whether attribute values or only attribute types(names) should
        ///     be returned in a search request.
        /// </summary>
        /// <returns>
        ///     true if only attribute types (names) are returned, false if
        ///     attributes types and values are to be returned.
        /// </returns>
        public bool TypesOnly =>
            ((Asn1Boolean)((RfcSearchRequest)Asn1Object.get_Renamed(1)).get_Renamed(5)).BooleanValue();

        /// <summary> Retrieves an array of attribute names to request for in a search.</summary>
        /// <returns>
        ///     Attribute names to be searched.
        /// </returns>
        public string[] Attributes
        {
            get
            {
                var attrs = (RfcAttributeDescriptionList)((RfcSearchRequest)Asn1Object.get_Renamed(1)).get_Renamed(7);

                var rAttrs = new string[attrs.Size()];
                for (var i = 0; i < rAttrs.Length; i++)
                {
                    rAttrs[i] = ((RfcAttributeDescription)attrs.get_Renamed(i)).StringValue();
                }

                return rAttrs;
            }
        }

        /// <summary> Creates a string representation of the filter in this search request.</summary>
        /// <returns>
        ///     filter string for this search request.
        /// </returns>
        public string StringFilter => RfcFilter.FilterToString();

        /// <summary> Retrieves an SearchFilter object representing a filter for a search request.</summary>
        /// <returns>
        ///     filter object for a search request.
        /// </returns>
        private RfcFilter RfcFilter => (RfcFilter)((RfcSearchRequest)Asn1Object.get_Renamed(1)).get_Renamed(6);

        /// <summary>
        ///     Retrieves an Iterator object representing the parsed filter for
        ///     this search request.
        ///     The first object returned from the Iterator is an Integer indicating
        ///     the type of filter component. One or more values follow the component
        ///     type as subsequent items in the Iterator. The pattern of Integer
        ///     component type followed by values continues until the end of the
        ///     filter.
        ///     Values returned as a byte array may represent UTF-8 characters or may
        ///     be binary values. The possible Integer components of a search filter
        ///     and the associated values that follow are:.
        ///     <ul>
        ///         <li>AND - followed by an Iterator value</li>
        ///         <li>OR - followed by an Iterator value</li>
        ///         <li>NOT - followed by an Iterator value</li>
        ///         <li>
        ///             EQUALITY_MATCH - followed by the attribute name represented as a
        ///             String, and by the attribute value represented as a byte array
        ///         </li>
        ///         <li>
        ///             GREATER_OR_EQUAL - followed by the attribute name represented as a
        ///             String, and by the attribute value represented as a byte array
        ///         </li>
        ///         <li>
        ///             LESS_OR_EQUAL - followed by the attribute name represented as a
        ///             String, and by the attribute value represented as a byte array
        ///         </li>
        ///         <li>
        ///             APPROX_MATCH - followed by the attribute name represented as a
        ///             String, and by the attribute value represented as a byte array
        ///         </li>
        ///         <li>PRESENT - followed by a attribute name respresented as a String</li>
        ///         <li>
        ///             EXTENSIBLE_MATCH - followed by the name of the matching rule
        ///             represented as a String, by the attribute name represented
        ///             as a String, and by the attribute value represented as a
        ///             byte array.
        ///         </li>
        ///         <li>
        ///             SUBSTRINGS - followed by the attribute name represented as a
        ///             String, by one or more SUBSTRING components (INITIAL, ANY,
        ///             or FINAL) followed by the SUBSTRING value.
        ///         </li>
        ///     </ul>
        /// </summary>
        /// <returns>
        ///     Iterator representing filter components.
        /// </returns>
        public IEnumerator SearchFilter => RfcFilter.GetFilterIterator();
    }
}
