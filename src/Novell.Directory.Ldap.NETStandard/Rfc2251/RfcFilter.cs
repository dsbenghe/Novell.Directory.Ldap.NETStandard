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
using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Filter.
    ///     This filter object can be created from a String or can be built up
    ///     programatically by adding filter components one at a time.  Existing filter
    ///     components can be iterated though.
    ///     Each filter component has an integer identifier defined in this class.
    ///     The following are basic filter components: {@link #EQUALITY_MATCH},
    ///     {@link #GREATER_OR_EQUAL}, {@link #LESS_OR_EQUAL}, {@link #SUBSTRINGS},
    ///     {@link #PRESENT}, {@link #APPROX_MATCH}, {@link #EXTENSIBLE_MATCH}.
    ///     More filters can be nested together into more complex filters with the
    ///     following filter components: {@link #AND}, {@link #OR}, {@link #NOT}
    ///     Substrings can have three components:.
    ///     <pre>
    ///         Filter ::= CHOICE {
    ///         and             [0] SET OF Filter,
    ///         or              [1] SET OF Filter,
    ///         not             [2] Filter,
    ///         equalityMatch   [3] AttributeValueAssertion,
    ///         substrings      [4] SubstringFilter,
    ///         greaterOrEqual  [5] AttributeValueAssertion,
    ///         lessOrEqual     [6] AttributeValueAssertion,
    ///         present         [7] AttributeDescription,
    ///         approxMatch     [8] AttributeValueAssertion,
    ///         extensibleMatch [9] MatchingRuleAssertion }
    ///     </pre>
    /// </summary>
    public class RfcFilter : Asn1Choice
    {
        // *************************************************************************
        // Public variables for Filter
        // *************************************************************************

        /// <summary> Identifier for AND component.</summary>
        public const int And = LdapSearchRequest.And;

        /// <summary> Identifier for OR component.</summary>
        public const int Or = LdapSearchRequest.Or;

        /// <summary> Identifier for NOT component.</summary>
        public const int Not = LdapSearchRequest.Not;

        /// <summary> Identifier for EQUALITY_MATCH component.</summary>
        public const int EqualityMatch = LdapSearchRequest.EqualityMatch;

        /// <summary> Identifier for SUBSTRINGS component.</summary>
        public const int Substrings = LdapSearchRequest.Substrings;

        /// <summary> Identifier for GREATER_OR_EQUAL component.</summary>
        public const int GreaterOrEqual = LdapSearchRequest.GreaterOrEqual;

        /// <summary> Identifier for LESS_OR_EQUAL component.</summary>
        public const int LessOrEqual = LdapSearchRequest.LessOrEqual;

        /// <summary> Identifier for PRESENT component.</summary>
        public const int Present = LdapSearchRequest.Present;

        /// <summary> Identifier for APPROX_MATCH component.</summary>
        public const int ApproxMatch = LdapSearchRequest.ApproxMatch;

        /// <summary> Identifier for EXTENSIBLE_MATCH component.</summary>
        public const int ExtensibleMatch = LdapSearchRequest.ExtensibleMatch;

        /// <summary> Identifier for INITIAL component.</summary>
        public const int Initial = LdapSearchRequest.Initial;

        /// <summary> Identifier for ANY component.</summary>
        public const int Any = LdapSearchRequest.Any;

        /// <summary> Identifier for FINAL component.</summary>
        public const int Final = LdapSearchRequest.Final;

        private Stack _filterStack;
        private bool _finalFound;

        // *************************************************************************
        // Private variables for Filter
        // *************************************************************************
        private FilterTokenizer _ft;

        // *************************************************************************
        // Constructor for Filter
        // *************************************************************************

        /// <summary> Constructs a Filter object by parsing an RFC 2254 Search Filter String.</summary>
        public RfcFilter(string filter)
            : base(null)
        {
            ChoiceValue = Parse(filter);
        }

        /// <summary> Constructs a Filter object that will be built up piece by piece.   </summary>
        public RfcFilter()
            : base(null)
        {
            _filterStack = new Stack();

            // The choice value must be set later: setChoiceValue(rootFilterTag)
        }

        // *************************************************************************
        // Helper methods for RFC 2254 Search Filter parsing.
        // *************************************************************************

        /// <summary> Parses an RFC 2251 filter string into an ASN.1 Ldap Filter object.</summary>
        private Asn1Tagged Parse(string filterExpr)
        {
            if (string.IsNullOrEmpty(filterExpr))
            {
                filterExpr = "(objectclass=*)";
            }

            int idx;
            if ((idx = filterExpr.IndexOf('\\')) != -1)
            {
                var sb = new StringBuilder(filterExpr);
                var i = idx;
                while (i < sb.Length - 1)
                {
                    var c = sb[i++];
                    if (c == '\\')
                    {
                        // found '\' (backslash)
                        // If V2 escape, turn to a V3 escape
                        c = sb[i];
                        if (c == '*' || c == '(' || c == ')' || c == '\\')
                        {
                            // Ldap v2 filter, convert them into hex chars
                            sb.Remove(i, i + 1 - i);
                            sb.Insert(i, Convert.ToString(c, 16));
                            i += 2;
                        }
                    }
                }

                filterExpr = sb.ToString();
            }

            // missing opening and closing parentheses, must be V2, add parentheses
            if (filterExpr[0] != '(' && filterExpr[filterExpr.Length - 1] != ')')
            {
                filterExpr = "(" + filterExpr + ")";
            }

            var ch = filterExpr[0];
            var len = filterExpr.Length;

            // missing opening parenthesis ?
            if (ch != '(')
            {
                throw new LdapLocalException(ExceptionMessages.MissingLeftParen, LdapException.FilterError);
            }

            // missing closing parenthesis ?
            if (filterExpr[len - 1] != ')')
            {
                throw new LdapLocalException(ExceptionMessages.MissingRightParen, LdapException.FilterError);
            }

            // unmatched parentheses ?
            var parenCount = 0;
            for (var i = 0; i < len; i++)
            {
                if (filterExpr[i] == '(')
                {
                    parenCount++;
                }

                if (filterExpr[i] == ')')
                {
                    parenCount--;
                }
            }

            if (parenCount > 0)
            {
                throw new LdapLocalException(ExceptionMessages.MissingRightParen, LdapException.FilterError);
            }

            if (parenCount < 0)
            {
                throw new LdapLocalException(ExceptionMessages.MissingLeftParen, LdapException.FilterError);
            }

            _ft = new FilterTokenizer(this, filterExpr);

            return ParseFilter();
        }

        /// <summary> Parses an RFC 2254 filter.</summary>
        private Asn1Tagged ParseFilter()
        {
            _ft.GetLeftParen();

            var filter = ParseFilterComp();

            _ft.GetRightParen();

            return filter;
        }

        /// <summary> RFC 2254 filter helper method. Will Parse a filter component.</summary>
        private Asn1Tagged ParseFilterComp()
        {
            Asn1Tagged tag = null;
            var filterComp = _ft.OpOrAttr;

            switch (filterComp)
            {
                case And:
                case Or:
                    tag = new Asn1Tagged(
                        new Asn1Identifier(Asn1Identifier.Context, true, filterComp),
                        ParseFilterList(),
                        false);
                    break;

                case Not:
                    tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.Context, true, filterComp), ParseFilter(),
                        true);
                    break;

                default:
                    var filterType = _ft.FilterType;
                    var valueRenamed = _ft.Value;

                    switch (filterType)
                    {
                        case GreaterOrEqual:
                        case LessOrEqual:
                        case ApproxMatch:
                            tag = new Asn1Tagged(
                                new Asn1Identifier(Asn1Identifier.Context, true, filterType),
                                new RfcAttributeValueAssertion(
                                    new RfcAttributeDescription(_ft.Attr),
                                    new RfcAssertionValue(UnescapeString(valueRenamed))), false);
                            break;

                        case EqualityMatch:
                            if (valueRenamed.Equals("*"))
                            {
                                // present
                                tag = new Asn1Tagged(
                                    new Asn1Identifier(Asn1Identifier.Context, false, Present),
                                    new RfcAttributeDescription(_ft.Attr), false);
                            }
                            else if (valueRenamed.IndexOf('*') != -1)
                            {
                                // substrings parse:
                                //    [initial], *any*, [final] into an Asn1SequenceOf
                                var sub = new Tokenizer(valueRenamed, "*", true);

                                // SupportClass.Tokenizer sub = new SupportClass.Tokenizer(value_Renamed, "*");//, true);
                                var seq = new Asn1SequenceOf(5);
                                var tokCnt = sub.Count;
                                var cnt = 0;

                                var lastTok = string.Empty;

                                while (sub.HasMoreTokens())
                                {
                                    var subTok = sub.NextToken();
                                    cnt++;
                                    if (subTok.Equals("*"))
                                    {
                                        // if previous token was '*', and since the current
                                        // token is a '*', we need to insert 'any'
                                        if (lastTok.Equals(subTok))
                                        {
                                            // '**'
                                            seq.Add(
                                                new Asn1Tagged(
                                                    new Asn1Identifier(Asn1Identifier.Context, false, Any),
                                                    new RfcLdapString(UnescapeString(string.Empty)), false));
                                        }
                                    }
                                    else
                                    {
                                        // value (RfcLdapString)
                                        if (cnt == 1)
                                        {
                                            // initial
                                            seq.Add(
                                                new Asn1Tagged(
                                                    new Asn1Identifier(Asn1Identifier.Context, false, Initial),
                                                    new RfcLdapString(UnescapeString(subTok)), false));
                                        }
                                        else if (cnt < tokCnt)
                                        {
                                            // any
                                            seq.Add(
                                                new Asn1Tagged(
                                                    new Asn1Identifier(Asn1Identifier.Context, false, Any),
                                                    new RfcLdapString(UnescapeString(subTok)), false));
                                        }
                                        else
                                        {
                                            // final
                                            seq.Add(
                                                new Asn1Tagged(
                                                    new Asn1Identifier(Asn1Identifier.Context, false, Final),
                                                    new RfcLdapString(UnescapeString(subTok)), false));
                                        }
                                    }

                                    lastTok = subTok;
                                }

                                tag = new Asn1Tagged(
                                    new Asn1Identifier(Asn1Identifier.Context, true, Substrings),
                                    new RfcSubstringFilter(new RfcAttributeDescription(_ft.Attr), seq), false);
                            }
                            else
                            {
                                // simple
                                tag = new Asn1Tagged(
                                    new Asn1Identifier(Asn1Identifier.Context, true, EqualityMatch),
                                    new RfcAttributeValueAssertion(
                                        new RfcAttributeDescription(_ft.Attr),
                                        new RfcAssertionValue(UnescapeString(valueRenamed))), false);
                            }

                            break;

                        case ExtensibleMatch:
                            string type = null, matchingRule = null;
                            var dnAttributes = false;

                            // SupportClass.Tokenizer st = new StringTokenizer(ft.Attr, ":", true);
                            var st = new Tokenizer(_ft.Attr, ":"); // , true);

                            var first = true;
                            while (st.HasMoreTokens())
                            {
                                var s = st.NextToken().Trim();
                                if (first && !s.Equals(":"))
                                {
                                    type = s;
                                }

                                // dn must be lower case to be considered dn of the Entry.
                                else if (s.Equals("dn"))
                                {
                                    dnAttributes = true;
                                }
                                else if (!s.Equals(":"))
                                {
                                    matchingRule = s;
                                }

                                first = false;
                            }

                            tag = new Asn1Tagged(
                                new Asn1Identifier(Asn1Identifier.Context, true, ExtensibleMatch),
                                new RfcMatchingRuleAssertion(
                                    matchingRule == null ? null : new RfcMatchingRuleId(matchingRule),
                                    type == null ? null : new RfcAttributeDescription(type),
                                    new RfcAssertionValue(UnescapeString(valueRenamed)),
                                    dnAttributes == false ? null : new Asn1Boolean(true)), false);
                            break;
                    }

                    break;
            }

            return tag;
        }

        /// <summary> Must have 1 or more Filters.</summary>
        private Asn1SetOf ParseFilterList()
        {
            var setRenamed = new Asn1SetOf();

            setRenamed.Add(ParseFilter()); // must have at least 1 filter

            while (_ft.PeekChar() == '(')
            {
                // check for more filters
                setRenamed.Add(ParseFilter());
            }

            return setRenamed;
        }

        /// <summary>
        ///     Convert hex character to an integer. Return -1 if char is something
        ///     other than a hex char.
        /// </summary>
        internal static int Hex2Int(char c)
        {
            return c >= '0' && c <= '9'
                ? c - '0'
                : c >= 'A' && c <= 'F'
                    ? c - 'A' + 10
                    : c >= 'a' && c <= 'f'
                        ? c - 'a' + 10
                        : -1;
        }

        /// <summary>
        ///     Replace escaped hex digits with the equivalent binary representation.
        ///     Assume either V2 or V3 escape mechanisms:
        ///     V2: \*,  \(,  \),  \\.
        ///     V3: \2A, \28, \29, \5C, \00.
        /// </summary>
        /// <param name="string">
        ///     A part of the input filter string to be converted.
        /// </param>
        /// <returns>
        ///     octet-string encoding of the specified string.
        /// </returns>
        private byte[] UnescapeString(string stringRenamed)
        {
            // give octets enough space to grow
            var octets = new byte[stringRenamed.Length * 3];

            // index for string and octets
            int iString, iOctets;

            // escape==true means we are in an escape sequence.
            var escape = false;

            // escStart==true means we are reading the first character of an escape.
            var escStart = false;

            int ival, length = stringRenamed.Length;
            byte[] utf8Bytes;
            char ch; // Character we are adding to the octet string
            var temp = (char)0; // holds the value of the escaped sequence

            // loop through each character of the string and copy them into octets
            // converting escaped sequences when needed
            for (iString = 0, iOctets = 0; iString < length; iString++)
            {
                ch = stringRenamed[iString];
                var codePoint = char.ConvertToUtf32(stringRenamed, iString);
                if (codePoint > 0xffff)
                {
                    iString++;
                }

                if (escape)
                {
                    if ((ival = Hex2Int(ch)) < 0)
                    {
                        // Invalid escape value(not a hex character)
                        throw new LdapLocalException(ExceptionMessages.InvalidEscape, new object[] { ch },
                            LdapException.FilterError);
                    }

                    // V3 escaped: \\**
                    if (escStart)
                    {
                        temp = (char)(ival << 4); // high bits of escaped char
                        escStart = false;
                    }
                    else
                    {
                        temp |= (char)ival; // all bits of escaped char
                        octets[iOctets++] = (byte)temp;
                        escStart = escape = false;
                    }
                }
                else if (ch == '\\')
                {
                    escStart = escape = true;
                }
                else
                {
                    try
                    {
                        // place the character into octets.
                        if ((ch >= 0x01 && ch <= 0x27) || (ch >= 0x2B && ch <= 0x5B) || ch >= 0x5D)
                        {
                            // found valid char
                            if (ch <= 0x7f)
                            {
                                // char = %x01-27 / %x2b-5b / %x5d-7f
                                octets[iOctets++] = (byte)ch;
                            }
                            else
                            {
                                // char > 0x7f, could be encoded in 2, 3 or 4 bytes
                                utf8Bytes = Encoding.UTF8.GetBytes(char.ConvertFromUtf32(codePoint));

                                // copy utf8 encoded character into octets
                                Array.Copy(utf8Bytes, 0, octets, iOctets, utf8Bytes.Length);
                                iOctets = iOctets + utf8Bytes.Length;
                            }

                            escape = false;
                        }
                        else
                        {
                            // found invalid character
                            var escString = string.Empty;
                            utf8Bytes = Encoding.UTF8.GetBytes(char.ConvertFromUtf32(codePoint));

                            for (var i = 0; i < utf8Bytes.Length; i++)
                            {
                                var u = utf8Bytes[i];
                                if (u >= 0 && u < 0x10)
                                {
                                    escString = escString + "\\0" + Convert.ToString(u & 0xff, 16);
                                }
                                else
                                {
                                    escString = escString + "\\" + Convert.ToString(u & 0xff, 16);
                                }
                            }

                            throw new LdapLocalException(
                                ExceptionMessages.InvalidCharInFilter,
                                new object[] { ch, escString }, LdapException.FilterError);
                        }
                    }
                    catch (IOException ue)
                    {
                        // TODO: This can be removed? In Java, Encoding.GetEncoding("utf-8") might not work, but in .net we always have UTF-8
                        throw new Exception("UTF-8 String encoding not supported by JVM", ue);
                    }
                }
            }

            // Verify that any escape sequence completed
            if (escStart || escape)
            {
                throw new LdapLocalException(ExceptionMessages.ShortEscape, LdapException.FilterError);
            }

            var toReturn = new byte[iOctets];

            // Array.Copy((System.Array)SupportClass.ToByteArray(octets), 0, (System.Array)SupportClass.ToByteArray(toReturn), 0, iOctets);
            Array.Copy(octets, 0, toReturn, 0, iOctets);

            octets = null;
            return toReturn;
        }

        /* **********************************************************************
        *  The following methods aid in building filters sequentially,
        *  and is used by DSMLHandler:
        ***********************************************************************/

        /// <summary>
        ///     Called by sequential filter building methods to add to a filter
        ///     component.
        ///     Verifies that the specified Asn1Object can be added, then adds the
        ///     object to the filter.
        /// </summary>
        /// <param name="current">
        ///     Filter component to be added to the filter
        ///     @throws LdapLocalException Occurs when an invalid component is added, or
        ///     when the component is out of sequence.
        /// </param>
        private void AddObject(Asn1Object current)
        {
            if (_filterStack == null)
            {
                _filterStack = new Stack();
            }

            if (ChoiceValue == null)
            {
                // ChoiceValue is the root Asn1 node
                ChoiceValue = current;
            }
            else
            {
                var topOfStack = (Asn1Tagged)_filterStack.Peek();
                var valueRenamed = topOfStack.TaggedValue;
                if (valueRenamed == null)
                {
                    topOfStack.TaggedValue = current;
                    _filterStack.Push(current);

                    // filterStack.Add(current);
                }
                else if (valueRenamed is Asn1SetOf)
                {
                    ((Asn1SetOf)valueRenamed).Add(current);

                    // don't add this to the stack:
                }
                else if (valueRenamed is Asn1Set)
                {
                    ((Asn1Set)valueRenamed).Add(current);

                    // don't add this to the stack:
                }
                else if (valueRenamed.GetIdentifier().Tag == LdapSearchRequest.Not)
                {
                    throw new LdapLocalException(
                        "Attemp to create more than one 'not' sub-filter",
                        LdapException.FilterError);
                }
            }

            var type = current.GetIdentifier().Tag;
            if (type == And || type == Or || type == Not)
            {
                // filterStack.Add(current);
                _filterStack.Push(current);
            }
        }

        /// <summary>
        ///     Creates and addes a substrings filter component.
        ///     startSubstrings must be immediatly followed by at least one
        ///     {@link #addSubstring} method and one {@link #endSubstrings} method
        ///     @throws Novell.Directory.Ldap.LdapLocalException
        ///     Occurs when this component is created out of sequence.
        /// </summary>
        public void StartSubstrings(string attrName)
        {
            _finalFound = false;
            var seq = new Asn1SequenceOf(5);
            Asn1Object current = new Asn1Tagged(
                new Asn1Identifier(Asn1Identifier.Context, true, Substrings),
                new RfcSubstringFilter(new RfcAttributeDescription(attrName), seq), false);
            AddObject(current);
            _filterStack.Push(seq);
        }

        /// <summary>
        ///     Adds a Substring component of initial, any or final substring matching.
        ///     This method can be invoked only if startSubString was the last filter-
        ///     building method called.  A substring is not required to have an 'INITIAL'
        ///     substring.  However, when a filter contains an 'INITIAL' substring only
        ///     one can be added, and it must be the first substring added. Any number of
        ///     'ANY' substrings can be added. A substring is not required to have a
        ///     'FINAL' substrings either.  However, when a filter does contain a 'FINAL'
        ///     substring only one can be added, and it must be the last substring added.
        /// </summary>
        /// <param name="type">
        ///     Substring type: INITIAL | ANY | FINAL].
        /// </param>
        /// <param name="value">
        ///     Value to use for matching
        ///     @throws LdapLocalException   Occurs if this method is called out of
        ///     sequence or the type added is out of sequence.
        /// </param>
        public void AddSubstring(int type, byte[] valueRenamed)
        {
            try
            {
                var substringSeq = (Asn1SequenceOf)_filterStack.Peek();
                if (type != Initial && type != Any && type != Final)
                {
                    throw new LdapLocalException(
                        "Attempt to add an invalid " + "substring type",
                        LdapException.FilterError);
                }

                if (type == Initial && substringSeq.Size() != 0)
                {
                    throw new LdapLocalException(
                        "Attempt to add an initial " + "substring match after the first substring",
                        LdapException.FilterError);
                }

                if (_finalFound)
                {
                    throw new LdapLocalException(
                        "Attempt to add a substring " + "match after a final substring match",
                        LdapException.FilterError);
                }

                if (type == Final)
                {
                    _finalFound = true;
                }

                substringSeq.Add(new Asn1Tagged(
                    new Asn1Identifier(Asn1Identifier.Context, false, type),
                    new RfcLdapString(valueRenamed), false));
            }
            catch (InvalidCastException e)
            {
                throw new LdapLocalException(
                    "A call to addSubstring occured " + "without calling startSubstring",
                    LdapException.FilterError, e);
            }
        }

        /// <summary>
        ///     Completes a SubString filter component.
        ///     @throws LdapLocalException Occurs when this is called out of sequence,
        ///     or the substrings filter is empty.
        /// </summary>
        public void EndSubstrings()
        {
            try
            {
                _finalFound = false;
                var substringSeq = (Asn1SequenceOf)_filterStack.Peek();
                if (substringSeq.Size() == 0)
                {
                    throw new LdapLocalException("Empty substring filter", LdapException.FilterError);
                }
            }
            catch (InvalidCastException e)
            {
                throw new LdapLocalException("Missmatched ending of substrings", LdapException.FilterError, e);
            }

            _filterStack.Pop();
        }

        /// <summary>
        ///     Creates and adds an AttributeValueAssertion to the filter.
        /// </summary>
        /// <param name="rfcType">
        ///     Filter type: EQUALITY_MATCH | GREATER_OR_EQUAL
        ///     | LESS_OR_EQUAL | APPROX_MATCH ].
        /// </param>
        /// <param name="attrName">
        ///     Name of the attribute to be asserted.
        /// </param>
        /// <param name="value">
        ///     Value of the attribute to be asserted
        ///     @throws LdapLocalException
        ///     Occurs when the filter type is not a valid attribute assertion.
        /// </param>
        public void AddAttributeValueAssertion(int rfcType, string attrName, byte[] valueRenamed)
        {
            if (_filterStack != null && !(_filterStack.Count == 0) && _filterStack.Peek() is Asn1SequenceOf)
            {
                // If a sequenceof is on the stack then substring is left on the stack
                throw new LdapLocalException(
                    "Cannot insert an attribute assertion in a substring",
                    LdapException.FilterError);
            }

            if (rfcType != EqualityMatch && rfcType != GreaterOrEqual && rfcType != LessOrEqual &&
                rfcType != ApproxMatch)
            {
                throw new LdapLocalException(
                    "Invalid filter type for AttributeValueAssertion",
                    LdapException.FilterError);
            }

            Asn1Object current = new Asn1Tagged(
                new Asn1Identifier(Asn1Identifier.Context, true, rfcType),
                new RfcAttributeValueAssertion(
                    new RfcAttributeDescription(attrName),
                    new RfcAssertionValue(valueRenamed)), false);
            AddObject(current);
        }

        /// <summary>
        ///     Creates and adds a present matching to the filter.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute to check for presence.
        ///     @throws LdapLocalException
        ///     Occurs if addPresent is called out of sequence.
        /// </param>
        public void AddPresent(string attrName)
        {
            Asn1Object current = new Asn1Tagged(
                new Asn1Identifier(Asn1Identifier.Context, false, Present),
                new RfcAttributeDescription(attrName), false);
            AddObject(current);
        }

        /// <summary>
        ///     Adds an extensible match to the filter.
        /// </summary>
        /// <param name="">
        ///     matchingRule
        ///     OID or name of the matching rule to use for comparison.
        /// </param>
        /// <param name="attrName">
        ///     Name of the attribute to match.
        /// </param>
        /// <param name="value">
        ///     Value of the attribute to match against.
        /// </param>
        /// <param name="useDnMatching">
        ///     Indicates whether DN matching should be used.
        ///     @throws LdapLocalException
        ///     Occurs when addExtensibleMatch is called out of sequence.
        /// </param>
        public void AddExtensibleMatch(string matchingRule, string attrName, byte[] valueRenamed, bool useDnMatching)
        {
            Asn1Object current = new Asn1Tagged(
                new Asn1Identifier(Asn1Identifier.Context, true, ExtensibleMatch),
                new RfcMatchingRuleAssertion(
                    matchingRule == null ? null : new RfcMatchingRuleId(matchingRule),
                    attrName == null ? null : new RfcAttributeDescription(attrName),
                    new RfcAssertionValue(valueRenamed), useDnMatching == false ? null : new Asn1Boolean(true)), false);
            AddObject(current);
        }

        /// <summary>
        ///     Creates and adds the Asn1Tagged value for a nestedFilter: AND, OR, or
        ///     NOT.
        ///     Note that a Not nested filter can only have one filter, where AND
        ///     and OR do not.
        /// </summary>
        /// <param name="rfcType">
        ///     Filter type:
        ///     [AND | OR | NOT]
        ///     @throws Novell.Directory.Ldap.LdapLocalException.
        /// </param>
        public void StartNestedFilter(int rfcType)
        {
            Asn1Object current;
            if (rfcType == And || rfcType == Or)
            {
                current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.Context, true, rfcType), new Asn1SetOf(),
                    false);
            }
            else if (rfcType == Not)
            {
                current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.Context, true, rfcType), null, true);
            }
            else
            {
                throw new LdapLocalException(
                    "Attempt to create a nested filter other than AND, OR or NOT",
                    LdapException.FilterError);
            }

            AddObject(current);
        }

        /// <summary> Completes a nested filter and checks for the valid filter type.</summary>
        /// <param name="rfcType">
        ///     Type of filter to complete.
        ///     @throws Novell.Directory.Ldap.LdapLocalException  Occurs when the specified
        ///     type differs from the current filter component.
        /// </param>
        public void EndNestedFilter(int rfcType)
        {
            if (rfcType == Not)
            {
                // if this is a Not than Not should be the second thing on the stack
                _filterStack.Pop();
            }

            var topOfStackType = ((Asn1Object)_filterStack.Peek()).GetIdentifier().Tag;
            if (topOfStackType != rfcType)
            {
                throw new LdapLocalException("Missmatched ending of nested filter", LdapException.FilterError);
            }

            _filterStack.Pop();
        }

        /// <summary>
        ///     Creates an iterator over the preparsed segments of a filter.
        ///     The first object returned by an iterator is an integer indicating the
        ///     type of filter components.  Subseqence values are returned.  If a
        ///     component is of type 'AND' or 'OR' or 'NOT' then the value
        ///     returned is another iterator.  This iterator is used by ToString.
        /// </summary>
        /// <returns>
        ///     Iterator over filter segments.
        /// </returns>
        public IEnumerator GetFilterIterator()
        {
            return new FilterIterator(this, (Asn1Tagged)ChoiceValue);
        }

        /// <summary> Creates and returns a String representation of this filter.</summary>
        public string FilterToString()
        {
            var filter = new StringBuilder();
            StringFilter(GetFilterIterator(), filter);
            return filter.ToString();
        }

        /// <summary>
        ///     Uses a filterIterator to create a string representation of a filter.
        /// </summary>
        /// <param name="itr">
        ///     Iterator of filter components.
        /// </param>
        /// <param name="filter">
        ///     Buffer to place a string representation of the filter.
        /// </param>
        /// <seealso cref="FilterIterator">
        /// </seealso>
        private static void StringFilter(IEnumerator itr, StringBuilder filter)
        {
            var op = -1;
            filter.Append('(');
            while (itr.MoveNext())
            {
                var filterpart = itr.Current;
                if (filterpart is int)
                {
                    op = (int)filterpart;
                    switch (op)
                    {
                        case And:
                            filter.Append('&');
                            break;

                        case Or:
                            filter.Append('|');
                            break;

                        case Not:
                            filter.Append('!');
                            break;

                        case EqualityMatch:
                            {
                                filter.Append((string)itr.Current);
                                filter.Append('=');
                                var valueRenamed = (byte[])itr.Current;
                                filter.Append(ByteString(valueRenamed));
                                break;
                            }

                        case GreaterOrEqual:
                            {
                                filter.Append((string)itr.Current);
                                filter.Append(">=");
                                var valueRenamed = (byte[])itr.Current;
                                filter.Append(ByteString(valueRenamed));
                                break;
                            }

                        case LessOrEqual:
                            {
                                filter.Append((string)itr.Current);
                                filter.Append("<=");
                                var valueRenamed = (byte[])itr.Current;
                                filter.Append(ByteString(valueRenamed));
                                break;
                            }

                        case Present:
                            filter.Append((string)itr.Current);
                            filter.Append("=*");
                            break;

                        case ApproxMatch:
                            filter.Append((string)itr.Current);
                            filter.Append("~=");
                            var valueRenamed2 = (byte[])itr.Current;
                            filter.Append(ByteString(valueRenamed2));
                            break;

                        case ExtensibleMatch:
                            var oid = (string)itr.Current;

                            filter.Append((string)itr.Current);
                            filter.Append(':');
                            filter.Append(oid);
                            filter.Append(":=");
                            filter.Append((string)itr.Current);
                            break;

                        case Substrings:
                            {
                                filter.Append((string)itr.Current);
                                filter.Append('=');
                                var noStarLast = false;
                                while (itr.MoveNext())
                                {
                                    op = (int)itr.Current;
                                    switch (op)
                                    {
                                        case Initial:
                                            filter.Append((string)itr.Current);
                                            filter.Append('*');
                                            noStarLast = false;
                                            break;

                                        case Any:
                                            if (noStarLast)
                                            {
                                                filter.Append('*');
                                            }

                                            filter.Append((string)itr.Current);
                                            filter.Append('*');
                                            noStarLast = false;
                                            break;

                                        case Final:
                                            if (noStarLast)
                                            {
                                                filter.Append('*');
                                            }

                                            filter.Append((string)itr.Current);
                                            break;
                                    }
                                }

                                break;
                            }
                    }
                }
                else if (filterpart is IEnumerator)
                {
                    StringFilter((IEnumerator)filterpart, filter);
                }
            }

            filter.Append(')');
        }

        /// <summary>
        ///     Convert a UTF8 encoded string, or binary data, into a String encoded for
        ///     a string filter.
        /// </summary>
        private static string ByteString(byte[] valueRenamed)
        {
            if (Base64.IsValidUtf8(valueRenamed, true))
            {
                return valueRenamed.ToUtf8String();
            }

            var binary = new StringBuilder();
            for (var i = 0; i < valueRenamed.Length; i++)
            {
                // TODO repair binary output
                // Every octet needs to be escaped
                if (valueRenamed[i] >= 0)
                {
                    // one character hex string
                    binary.Append("\\0");
                    binary.Append(Convert.ToString(valueRenamed[i], 16));
                }
                else
                {
                    // negative (eight character) hex string
                    binary.Append("\\" + Convert.ToString(valueRenamed[i], 16).Substring(6));
                }
            }

            return binary.ToString();
        }

        /// <summary>
        ///     This inner class wrappers the Search Filter with an iterator.
        ///     This iterator will give access to all the individual components
        ///     preparsed.  The first call to next will return an Integer identifying
        ///     the type of filter component.  Then the component values will be returned
        ///     AND, NOT, and OR components values will be returned as Iterators.
        /// </summary>
        private class FilterIterator : IEnumerator
        {
            private readonly Asn1Tagged _root;

            private bool _hasMore = true;

            /// <summary>indexes the several parts a component may have. </summary>
            private int _index = -1;

            /// <summary>indicates if the identifier for a component has been returned yet. </summary>
            private bool _tagReturned;

            public FilterIterator(RfcFilter enclosingInstance, Asn1Tagged root)
            {
                EnclosingInstance = enclosingInstance;
                _root = root;
            }

            private RfcFilter EnclosingInstance { get; }

            public void Reset()
            {
            }

            /// <summary>
            ///     Returns filter identifiers and components of a filter.
            ///     The first object returned is an Integer identifying
            ///     its type.
            /// </summary>
            public object Current
            {
                get
                {
                    object toReturn = null;
                    if (!_tagReturned)
                    {
                        _tagReturned = true;
                        toReturn = _root.GetIdentifier().Tag;
                    }
                    else
                    {
                        var asn1 = _root.TaggedValue;

                        if (asn1 is RfcLdapString)
                        {
                            // one value to iterate
                            _hasMore = false;
                            toReturn = ((RfcLdapString)asn1).StringValue();
                        }
                        else if (asn1 is RfcSubstringFilter)
                        {
                            var sub = (RfcSubstringFilter)asn1;
                            if (_index == -1)
                            {
                                // return attribute name
                                _index = 0;
                                var attr = (RfcAttributeDescription)sub.get_Renamed(0);
                                toReturn = attr.StringValue();
                            }
                            else if (_index % 2 == 0)
                            {
                                // return substring identifier
                                var substrs = (Asn1SequenceOf)sub.get_Renamed(1);
                                toReturn = ((Asn1Tagged)substrs.get_Renamed(_index / 2)).GetIdentifier().Tag;
                                _index++;
                            }
                            else
                            {
                                // return substring value
                                var substrs = (Asn1SequenceOf)sub.get_Renamed(1);
                                var tag = (Asn1Tagged)substrs.get_Renamed(_index / 2);
                                var valueRenamed = (RfcLdapString)tag.TaggedValue;
                                toReturn = valueRenamed.StringValue();
                                _index++;
                            }

                            if (_index / 2 >= ((Asn1SequenceOf)sub.get_Renamed(1)).Size())
                            {
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is RfcAttributeValueAssertion)
                        {
                            // components: =,>=,<=,~=
                            var assertion = (RfcAttributeValueAssertion)asn1;

                            if (_index == -1)
                            {
                                toReturn = assertion.AttributeDescription;
                                _index = 1;
                            }
                            else if (_index == 1)
                            {
                                toReturn = assertion.AssertionValue;
                                _index = 2;
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is RfcMatchingRuleAssertion)
                        {
                            // Extensible match
                            var exMatch = (RfcMatchingRuleAssertion)asn1;
                            if (_index == -1)
                            {
                                _index = 0;
                            }

                            toReturn =
                                ((Asn1OctetString)((Asn1Tagged)exMatch.get_Renamed(_index++)).TaggedValue)
                                .StringValue();
                            if (_index > 2)
                            {
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is Asn1SetOf)
                        {
                            // AND and OR nested components
                            var setRenamed = (Asn1SetOf)asn1;
                            if (_index == -1)
                            {
                                _index = 0;
                            }

                            toReturn = new FilterIterator(
                                EnclosingInstance,
                                (Asn1Tagged)setRenamed.get_Renamed(_index++));
                            if (_index >= setRenamed.Size())
                            {
                                _hasMore = false;
                            }
                        }
                        else if (asn1 is Asn1Tagged)
                        {
                            // NOT nested component.
                            toReturn = new FilterIterator(EnclosingInstance, (Asn1Tagged)asn1);
                            _hasMore = false;
                        }
                    }

                    return toReturn;
                }
            }

            public bool MoveNext()
            {
                return _hasMore;
            }

            public void Remove()
            {
                throw new NotSupportedException("Remove is not supported on a filter iterator");
            }
        }

        /// <summary> This inner class will tokenize the components of an RFC 2254 search filter.</summary>
        private class FilterTokenizer
        {
            // *************************************************************************
            // Private variables
            // *************************************************************************
            private readonly string _filter; // The filter string to parse
            private readonly int _filterLength; // Length of the filter string to parse
            private int _offset; // Offset pointer into the filter string

            // *************************************************************************
            // Constructor
            // *************************************************************************

            /// <summary> Constructs a FilterTokenizer for a filter.</summary>
            public FilterTokenizer(RfcFilter enclosingInstance, string filter)
            {
                EnclosingInstance = enclosingInstance;
                _filter = filter;
                _offset = 0;
                _filterLength = filter.Length;
            }

            /// <summary>
            ///     Reads either an operator, or an attribute, whichever is
            ///     next in the filter string.
            ///     If the next component is an attribute, it is read and stored in the
            ///     attr field of this class which may be retrieved with getAttr()
            ///     and a -1 is returned. Otherwise, the int value of the operator read is
            ///     returned.
            /// </summary>
            public int OpOrAttr
            {
                get
                {
                    int index;

                    if (_offset >= _filterLength)
                    {
                        // "Unexpected end of filter",
                        throw new LdapLocalException(ExceptionMessages.UnexpectedEnd, LdapException.FilterError);
                    }

                    int ret;
                    int testChar = _filter[_offset];
                    if (testChar == '&')
                    {
                        _offset++;
                        ret = And;
                    }
                    else if (testChar == '|')
                    {
                        _offset++;
                        ret = Or;
                    }
                    else if (testChar == '!')
                    {
                        _offset++;
                        ret = Not;
                    }
                    else
                    {
                        if (_filter.StartsWithStringAtOffset(":=", _offset))
                        {
                            throw new LdapLocalException(ExceptionMessages.NoMatchingRule, LdapException.FilterError);
                        }

                        if (_filter.StartsWithStringAtOffset("::=", _offset) ||
                            _filter.StartsWithStringAtOffset(":::=", _offset))
                        {
                            throw new LdapLocalException(
                                ExceptionMessages.NoDnNorMatchingRule,
                                LdapException.FilterError);
                        }

                        // get first component of 'item' (attr or :dn or :matchingrule)
                        var delims = "=~<>()";
                        var sb = new StringBuilder();

                        while (delims.IndexOf(_filter[_offset]) == -1 &&
                               _filter.StartsWithStringAtOffset(":=", _offset) == false)
                        {
                            sb.Append(_filter[_offset++]);
                        }

                        Attr = sb.ToString().Trim();

                        // is there an attribute name specified in the filter ?
                        if (Attr.Length == 0 || Attr[0] == ';')
                        {
                            throw new LdapLocalException(ExceptionMessages.NoAttributeName, LdapException.FilterError);
                        }

                        for (index = 0; index < Attr.Length; index++)
                        {
                            var atIndex = Attr[index];
                            if (
                                !(char.IsLetterOrDigit(atIndex) || atIndex == '-' || atIndex == '.' || atIndex == ';' ||
                                  atIndex == ':'))
                            {
                                if (atIndex == '\\')
                                {
                                    throw new LdapLocalException(
                                        ExceptionMessages.InvalidEscInDescr,
                                        LdapException.FilterError);
                                }

                                throw new LdapLocalException(
                                    ExceptionMessages.InvalidCharInDescr,
                                    new object[] { atIndex }, LdapException.FilterError);
                            }
                        }

                        // is there an option specified in the filter ?
                        index = Attr.IndexOf(';');
                        if (index != -1 && index == Attr.Length - 1)
                        {
                            throw new LdapLocalException(ExceptionMessages.NoOption, LdapException.FilterError);
                        }

                        ret = -1;
                    }

                    return ret;
                }
            }

            /// <summary>
            ///     Reads an RFC 2251 filter type from the filter string and returns its
            ///     int value.
            /// </summary>
            public int FilterType
            {
                get
                {
                    if (_offset >= _filterLength)
                    {
                        // "Unexpected end of filter",
                        throw new LdapLocalException(ExceptionMessages.UnexpectedEnd, LdapException.FilterError);
                    }

                    int ret;
                    if (_filter.StartsWithStringAtOffset(">=", _offset))
                    {
                        _offset += 2;
                        ret = GreaterOrEqual;
                    }
                    else if (_filter.StartsWithStringAtOffset("<=", _offset))
                    {
                        _offset += 2;
                        ret = LessOrEqual;
                    }
                    else if (_filter.StartsWithStringAtOffset("~=", _offset))
                    {
                        _offset += 2;
                        ret = ApproxMatch;
                    }
                    else if (_filter.StartsWithStringAtOffset(":=", _offset))
                    {
                        _offset += 2;
                        ret = ExtensibleMatch;
                    }
                    else if (_filter[_offset] == '=')
                    {
                        _offset++;
                        ret = EqualityMatch;
                    }
                    else
                    {
                        // "Invalid comparison operator",
                        throw new LdapLocalException(
                            ExceptionMessages.InvalidFilterComparison,
                            LdapException.FilterError);
                    }

                    return ret;
                }
            }

            /// <summary> Reads a value from a filter string.</summary>
            public string Value
            {
                get
                {
                    if (_offset >= _filterLength)
                    {
                        // "Unexpected end of filter",
                        throw new LdapLocalException(ExceptionMessages.UnexpectedEnd, LdapException.FilterError);
                    }

                    var idx = _filter.IndexOf(')', _offset);
                    if (idx == -1)
                    {
                        idx = _filterLength;
                    }

                    var ret = _filter.Substring(_offset, idx - _offset);
                    _offset = idx;

                    return ret;
                }
            }

            /// <summary> Returns the current attribute identifier.</summary>
            public string Attr { get; private set; }

            public RfcFilter EnclosingInstance { get; }

            // *************************************************************************
            // Tokenizer methods
            // *************************************************************************

            /// <summary>
            ///     Reads the current char and throws an Exception if it is not a left
            ///     parenthesis.
            /// </summary>
            public void GetLeftParen()
            {
                if (_offset >= _filterLength)
                {
                    // "Unexpected end of filter",
                    throw new LdapLocalException(ExceptionMessages.UnexpectedEnd, LdapException.FilterError);
                }

                if (_filter[_offset++] != '(')
                {
                    // "Missing left paren",
                    throw new LdapLocalException(
                        ExceptionMessages.ExpectingLeftParen,
                        new object[] { _filter[_offset -= 1] }, LdapException.FilterError);
                }
            }

            /// <summary>
            ///     Reads the current char and throws an Exception if it is not a right
            ///     parenthesis.
            /// </summary>
            public void GetRightParen()
            {
                if (_offset >= _filterLength)
                {
                    // "Unexpected end of filter",
                    throw new LdapLocalException(ExceptionMessages.UnexpectedEnd, LdapException.FilterError);
                }

                if (_filter[_offset++] != ')')
                {
                    // "Missing right paren",
                    throw new LdapLocalException(
                        ExceptionMessages.ExpectingRightParen,
                        new object[] { _filter[_offset - 1] }, LdapException.FilterError);
                }
            }

            /// <summary>
            ///     Return the current char without advancing the offset pointer. This is
            ///     used by ParseFilterList when determining if there are any more
            ///     Filters in the list.
            /// </summary>
            public char PeekChar()
            {
                if (_offset >= _filterLength)
                {
                    // "Unexpected end of filter",
                    throw new LdapLocalException(ExceptionMessages.UnexpectedEnd, LdapException.FilterError);
                }

                return _filter[_offset];
            }
        }
    }
}
