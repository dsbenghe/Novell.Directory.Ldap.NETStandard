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
using System.Globalization;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     A DN encapsulates a Distinguished Name (an ldap name with context). A DN
    ///     does not need to be fully distinguished, or extend to the Root of a
    ///     directory.  It provides methods to get information about the DN and to
    ///     manipulate the DN.
    ///     The following are examples of valid DN:
    ///     <ul>
    ///         <li>cn=admin,ou=marketing,o=corporation</li>
    ///         <li>cn=admin,ou=marketing</li>
    ///         <li>2.5.4.3=admin,ou=marketing</li>
    ///         <li>oid.2.5.4.3=admin,ou=marketing</li>
    ///     </ul>
    ///     Note: Multivalued attributes are all considered to be one
    ///     component and are represented in one RDN (see RDN).
    /// </summary>
    /// <seealso cref="Rdn">
    /// </seealso>
    public class Dn
    {
        // parser state identifiers.
        private const int LookForRdnAttrType = 1;
        private const int AlphaAttrType = 2;
        private const int OidAttrType = 3;
        private const int LookForRdnValue = 4;
        private const int QuotedRdnValue = 5;
        private const int HexRdnValue = 6;
        private const int UnquotedRdnValue = 7;

        /* State transition table:  Parsing starts in state 1.

        State   COMMA   DIGIT   "Oid."  ALPHA   EQUAL   QUOTE   SHARP   HEX
        --------------------------------------------------------------------
        1       Err     3       3       2       Err     Err     Err     Err
        2       Err     Err     Err     2       4       Err     Err     Err
        3       Err     3       Err     Err     4       Err     Err     Err
        4       Err     7       Err     7       Err     5       6       7
        5       1       5       Err     5       Err     1       Err     7
        6       1       6       Err     Err     Err     Err     Err     6
        7       1       7       Err     7       Err     Err     Err     7

        */

        private List<Rdn> _rdnList = new List<Rdn>();

        public Dn()
        {
        }

        /// <summary>
        ///     Constructs a new DN based on the specified string representation of a
        ///     distinguished name. The syntax of the DN must conform to that specified
        ///     in RFC 2253.
        /// </summary>
        /// <param name="dnString">
        ///     a string representation of the distinguished name.
        /// </param>
        /// <exception>
        ///     IllegalArgumentException  if the the value of the dnString
        ///     parameter does not adhere to the syntax described in
        ///     RFC 2253.
        /// </exception>
        public Dn(string dnString)
        {
            /* the empty string is a valid DN */
            if (dnString.Length == 0)
            {
                return;
            }

            var tokenBuf = new char[dnString.Length];
            var trailingSpaceCount = 0;
            var attrType = string.Empty;
            var attrValue = string.Empty;
            var rawValue = string.Empty;
            var hexDigitCount = 0;
            var currRdn = new Rdn();

            // indicates whether an OID number has a first digit of ZERO
            var tokenIndex = 0;
            var currIndex = 0;
            var valueStart = 0;
            var state = LookForRdnAttrType;
            var lastIndex = dnString.Length - 1;
            while (currIndex <= lastIndex)
            {
                var currChar = dnString[currIndex];
                char nextChar;
                switch (state)
                {
                    case LookForRdnAttrType:
                        while (currChar == ' ' && currIndex < lastIndex)
                        {
                            currChar = dnString[++currIndex];
                        }

                        if (IsAlpha(currChar))
                        {
                            if (dnString.Substring(currIndex).StartsWith("oid.") ||
                                dnString.Substring(currIndex).StartsWith("OID."))
                            {
                                // form is "oid.###.##.###... or OID.###.##.###...
                                currIndex += 4; // skip oid. prefix and get to actual oid
                                if (currIndex > lastIndex)
                                {
                                    throw new ArgumentException(dnString);
                                }

                                currChar = dnString[currIndex];
                                if (IsDigit(currChar))
                                {
                                    tokenBuf[tokenIndex++] = currChar;
                                    state = OidAttrType;
                                }
                                else
                                {
                                    throw new ArgumentException(dnString);
                                }
                            }
                            else
                            {
                                tokenBuf[tokenIndex++] = currChar;
                                state = AlphaAttrType;
                            }
                        }
                        else if (IsDigit(currChar))
                        {
                            --currIndex;
                            state = OidAttrType;
                        }
                        else if (CharUnicodeInfo.GetUnicodeCategory(currChar) != UnicodeCategory.SpaceSeparator)
                        {
                            throw new ArgumentException(dnString);
                        }

                        break;

                    case AlphaAttrType:
                        if (IsAlpha(currChar) || IsDigit(currChar) || currChar == '-')
                        {
                            tokenBuf[tokenIndex++] = currChar;
                        }
                        else
                        {
                            // skip any spaces
                            while (currChar == ' ' && currIndex < lastIndex)
                            {
                                currChar = dnString[++currIndex];
                            }

                            if (currChar == '=')
                            {
                                attrType = new string(tokenBuf, 0, tokenIndex);
                                tokenIndex = 0;
                                state = LookForRdnValue;
                            }
                            else
                            {
                                throw new ArgumentException(dnString);
                            }
                        }

                        break;

                    case OidAttrType:
                        if (!IsDigit(currChar))
                        {
                            throw new ArgumentException(dnString);
                        }

                        var firstDigitZero = currChar == '0' ? true : false;
                        tokenBuf[tokenIndex++] = currChar;
                        currChar = dnString[++currIndex];

                        if ((IsDigit(currChar) && firstDigitZero) || (currChar == '.' && firstDigitZero))
                        {
                            throw new ArgumentException(dnString);
                        }

                        // consume all numbers.
                        while (IsDigit(currChar) && currIndex < lastIndex)
                        {
                            tokenBuf[tokenIndex++] = currChar;
                            currChar = dnString[++currIndex];
                        }

                        if (currChar == '.')
                        {
                            tokenBuf[tokenIndex++] = currChar;

                            // The state remains at OID_ATTR_TYPE
                        }
                        else
                        {
                            // skip any spaces
                            while (currChar == ' ' && currIndex < lastIndex)
                            {
                                currChar = dnString[++currIndex];
                            }

                            if (currChar == '=')
                            {
                                attrType = new string(tokenBuf, 0, tokenIndex);
                                tokenIndex = 0;
                                state = LookForRdnValue;
                            }
                            else
                            {
                                throw new ArgumentException(dnString);
                            }
                        }

                        break;

                    case LookForRdnValue:
                        while (currChar == ' ')
                        {
                            if (currIndex < lastIndex)
                            {
                                currChar = dnString[++currIndex];
                            }
                            else
                            {
                                throw new ArgumentException(dnString);
                            }
                        }

                        if (currChar == '"')
                        {
                            state = QuotedRdnValue;
                            valueStart = currIndex;
                        }
                        else if (currChar == '#')
                        {
                            hexDigitCount = 0;
                            tokenBuf[tokenIndex++] = currChar;
                            valueStart = currIndex;
                            state = HexRdnValue;
                        }
                        else
                        {
                            valueStart = currIndex;

                            // check this character again in the UNQUOTED_RDN_VALUE state
                            currIndex--;
                            state = UnquotedRdnValue;
                        }

                        break;

                    case UnquotedRdnValue:
                        if (currChar == '\\')
                        {
                            if (!(currIndex < lastIndex))
                            {
                                throw new ArgumentException(dnString);
                            }

                            currChar = dnString[++currIndex];
                            if (IsHexDigit(currChar))
                            {
                                if (!(currIndex < lastIndex))
                                {
                                    throw new ArgumentException(dnString);
                                }

                                nextChar = dnString[++currIndex];
                                if (IsHexDigit(nextChar))
                                {
                                    tokenBuf[tokenIndex++] = HexToChar(currChar, nextChar);
                                    trailingSpaceCount = 0;
                                }
                                else
                                {
                                    throw new ArgumentException(dnString);
                                }
                            }
                            else if (NeedsEscape(currChar) || currChar == '#' || currChar == '=' || currChar == ' ')
                            {
                                tokenBuf[tokenIndex++] = currChar;
                                trailingSpaceCount = 0;
                            }
                            else
                            {
                                throw new ArgumentException(dnString);
                            }
                        }
                        else if (currChar == ' ')
                        {
                            trailingSpaceCount++;
                            tokenBuf[tokenIndex++] = currChar;
                        }
                        else if (currChar == ',' || currChar == ';' || currChar == '+')
                        {
                            attrValue = new string(tokenBuf, 0, tokenIndex - trailingSpaceCount);
                            rawValue = dnString.Substring(valueStart, currIndex - trailingSpaceCount - valueStart);

                            currRdn.Add(attrType, attrValue, rawValue);
                            if (currChar != '+')
                            {
                                _rdnList.Add(currRdn);
                                currRdn = new Rdn();
                            }

                            trailingSpaceCount = 0;
                            tokenIndex = 0;
                            state = LookForRdnAttrType;
                        }
                        else if (NeedsEscape(currChar))
                        {
                            throw new ArgumentException(dnString);
                        }
                        else
                        {
                            trailingSpaceCount = 0;
                            tokenBuf[tokenIndex++] = currChar;
                        }

                        break; // end UNQUOTED RDN VALUE

                    case QuotedRdnValue:
                        if (currChar == '"')
                        {
                            rawValue = dnString.Substring(valueStart, currIndex + 1 - valueStart);
                            if (currIndex < lastIndex)
                            {
                                currChar = dnString[++currIndex];
                            }

                            // skip any spaces
                            while (currChar == ' ' && currIndex < lastIndex)
                            {
                                currChar = dnString[++currIndex];
                            }

                            if (currChar == ',' || currChar == ';' || currChar == '+' || currIndex == lastIndex)
                            {
                                attrValue = new string(tokenBuf, 0, tokenIndex);

                                currRdn.Add(attrType, attrValue, rawValue);
                                if (currChar != '+')
                                {
                                    _rdnList.Add(currRdn);
                                    currRdn = new Rdn();
                                }

                                trailingSpaceCount = 0;
                                tokenIndex = 0;
                                state = LookForRdnAttrType;
                            }
                            else
                            {
                                throw new ArgumentException(dnString);
                            }
                        }
                        else if (currChar == '\\')
                        {
                            currChar = dnString[++currIndex];
                            if (IsHexDigit(currChar))
                            {
                                nextChar = dnString[++currIndex];
                                if (IsHexDigit(nextChar))
                                {
                                    tokenBuf[tokenIndex++] = HexToChar(currChar, nextChar);
                                    trailingSpaceCount = 0;
                                }
                                else
                                {
                                    throw new ArgumentException(dnString);
                                }
                            }
                            else if (NeedsEscape(currChar) || currChar == '#' || currChar == '=' || currChar == ' ')
                            {
                                tokenBuf[tokenIndex++] = currChar;
                                trailingSpaceCount = 0;
                            }
                            else
                            {
                                throw new ArgumentException(dnString);
                            }
                        }
                        else
                        {
                            tokenBuf[tokenIndex++] = currChar;
                        }

                        break; // end QUOTED RDN VALUE

                    case HexRdnValue:
                        if (!IsHexDigit(currChar) || currIndex > lastIndex)
                        {
                            // check for odd number of hex digits
                            if (hexDigitCount % 2 != 0 || hexDigitCount == 0)
                            {
                                throw new ArgumentException(dnString);
                            }

                            rawValue = dnString.Substring(valueStart, currIndex - valueStart);

                            // skip any spaces
                            while (currChar == ' ' && currIndex < lastIndex)
                            {
                                currChar = dnString[++currIndex];
                            }

                            if (currChar == ',' || currChar == ';' || currChar == '+' || currIndex == lastIndex)
                            {
                                attrValue = new string(tokenBuf, 0, tokenIndex);

                                // added by cameron
                                currRdn.Add(attrType, attrValue, rawValue);
                                if (currChar != '+')
                                {
                                    _rdnList.Add(currRdn);
                                    currRdn = new Rdn();
                                }

                                tokenIndex = 0;
                                state = LookForRdnAttrType;
                            }
                            else
                            {
                                throw new ArgumentException(dnString);
                            }
                        }
                        else
                        {
                            tokenBuf[tokenIndex++] = currChar;
                            hexDigitCount++;
                        }

                        break; // end HEX RDN VALUE
                } // end switch

                currIndex++;
            } // end while

            // check ending state
            if (state == UnquotedRdnValue || (state == HexRdnValue && hexDigitCount % 2 == 0 && hexDigitCount != 0))
            {
                attrValue = new string(tokenBuf, 0, tokenIndex - trailingSpaceCount);
                rawValue = dnString.Substring(valueStart, currIndex - trailingSpaceCount - valueStart);
                currRdn.Add(attrType, attrValue, rawValue);
                _rdnList.Add(currRdn);
            }
            else if (state == LookForRdnValue)
            {
                // empty value is valid
                attrValue = string.Empty;
                rawValue = dnString.Substring(valueStart);
                currRdn.Add(attrType, attrValue, rawValue);
                _rdnList.Add(currRdn);
            }
            else
            {
                throw new ArgumentException(dnString);
            }
        } // end DN constructor (string dn)

        /// <summary> Retrieves a list of RDN Objects, or individual names of the DN.</summary>
        /// <returns>
        ///     list of RDNs.
        /// </returns>
        public IReadOnlyList<Rdn> RdNs => new List<Rdn>(_rdnList);

        /// <summary> Returns the Parent of this DN.</summary>
        /// <returns>
        ///     Parent DN.
        /// </returns>
        public Dn Parent
        {
            get
            {
                var parent = new Dn
                {
                    _rdnList = new List<Rdn>(_rdnList),
                };

                if (parent._rdnList.Count >= 1)
                {
                    parent._rdnList.RemoveAt(0); // remove first object
                }

                return parent;
            }
        }

        /// <summary>
        ///     Checks a character to see if it is an ascii alphabetic character in
        ///     ranges 65-90 or 97-122.
        /// </summary>
        /// <param name="ch">
        ///     the character to be tested.
        /// </param>
        /// <returns>
        ///     <code>true</code> if the character is an ascii alphabetic
        ///     character.
        /// </returns>
        private bool IsAlpha(char ch)
        {
            // ASCII A-Z
            if ((ch < 91 && ch > 64) || (ch < 123 && ch > 96))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks a character to see if it is an ascii digit (0-9) character in
        ///     the ascii value range 48-57.
        /// </summary>
        /// <param name="ch">
        ///     the character to be tested.
        /// </param>
        /// <returns>
        ///     <code>true</code> if the character is an ascii alphabetic
        ///     character.
        /// </returns>
        private bool IsDigit(char ch)
        {
            if (ch < 58 && ch > 47)
            {
                // ASCII 0-9
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks a character to see if it is valid hex digit 0-9, a-f, or
        ///     A-F (ASCII value ranges 48-47, 65-70, 97-102).
        /// </summary>
        /// <param name="ch">
        ///     the character to be tested.
        /// </param>
        /// <returns>
        ///     <code>true</code> if the character is a valid hex digit.
        /// </returns>
        private static bool IsHexDigit(char ch)
        {
            // ASCII A-F
            if ((ch < 58 && ch > 47) || (ch < 71 && ch > 64) || (ch < 103 && ch > 96))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks a character to see if it must always be escaped in the
        ///     string representation of a DN.  We must tests for space, sharp, and
        ///     equals individually.
        /// </summary>
        /// <param name="ch">
        ///     the character to be tested.
        /// </param>
        /// <returns>
        ///     <code>true</code> if the character needs to be escaped in at
        ///     least some instances.
        /// </returns>
        private bool NeedsEscape(char ch)
        {
            if (ch == ',' || ch == '+' || ch == '\"' || ch == ';' || ch == '<' || ch == '>' || ch == '\\')
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Converts two valid hex digit characters that form the string
        ///     representation of an ascii character value to the actual ascii
        ///     character.
        /// </summary>
        /// <param name="hex1">
        ///     the hex digit for the high order byte.
        /// </param>
        /// <param name="hex0">
        ///     the hex digit for the low order byte.
        /// </param>
        /// <returns>
        ///     the character whose value is represented by the parameters.
        /// </returns>
        private static char HexToChar(char hex1, char hex0)
        {
            int result;

            if (hex1 < 58 && hex1 > 47)
            {
                // ASCII 0-9
                result = (hex1 - 48) * 16;
            }
            else if (hex1 < 71 && hex1 > 64)
            {
                // ASCII a-f
                result = (hex1 - 55) * 16;
            }
            else if (hex1 < 103 && hex1 > 96)
            {
                // ASCII A-F
                result = (hex1 - 87) * 16;
            }
            else
            {
                throw new ArgumentException("Not hex digit");
            }

            if (hex0 < 58 && hex0 > 47)
            {
                // ASCII 0-9
                result += hex0 - 48;
            }
            else if (hex0 < 71 && hex0 > 64)
            {
                // ASCII a-f
                result += hex0 - 55;
            }
            else if (hex0 < 103 && hex0 > 96)
            {
                // ASCII A-F
                result += hex0 - 87;
            }
            else
            {
                throw new ArgumentException("Not hex digit");
            }

            return (char)result;
        }

        /// <summary>
        ///     Creates and returns a string that represents this DN.  The string
        ///     follows RFC 2253, which describes String representation of DN's and
        ///     RDN's.
        /// </summary>
        /// <returns>
        ///     A DN string.
        /// </returns>
        public override string ToString()
        {
            var length = _rdnList.Count;
            var dn = string.Empty;
            if (length < 1)
            {
                return null;
            }

            dn = _rdnList[0].ToString();
            for (var i = 1; i < length; i++)
            {
                dn += "," + _rdnList[i];
            }

            return dn;
        }

        public IReadOnlyList<Rdn> GetRdnList() => _rdnList;

        public override bool Equals(object toDn)
        {
            return Equals((Dn)toDn);
        }

        public bool Equals(Dn toDn)
        {
            var aList = toDn.GetRdnList();
            var length = aList.Count;

            if (_rdnList.Count != length)
            {
                return false;
            }

            for (var i = 0; i < length; i++)
            {
                if (!_rdnList[i].Equals(toDn.GetRdnList()[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     return a string array of the individual RDNs contained in the DN.
        /// </summary>
        /// <param name="noTypes">
        ///     If true, returns only the values of the
        ///     components, and not the names, e.g. "Babs
        ///     Jensen", "Accounting", "Acme", "us" - instead of
        ///     "cn=Babs Jensen", "ou=Accounting", "o=Acme", and
        ///     "c=us".
        /// </param>
        /// <returns>
        ///     <code>String[]</code> containing the rdns in the DN with
        ///     the leftmost rdn in the first element of the array.
        /// </returns>
        public string[] ExplodeDn(bool noTypes)
        {
            var length = _rdnList.Count;
            var rdns = new string[length];
            for (var i = 0; i < length; i++)
            {
                rdns[i] = _rdnList[i].ToString(noTypes);
            }

            return rdns;
        }

        /// <summary> Retrieves the count of RDNs, or individule names, in the Distinguished name.</summary>
        /// <returns>
        ///     the count of RDN.
        /// </returns>
        public int CountRdNs()
        {
            return _rdnList.Count;
        }

        /// <summary>
        ///     Determines if this DN is <I>contained</I> by the DN passed in.  For
        ///     example:  "cn=admin, ou=marketing, o=corporation" is contained by
        ///     "o=corporation", "ou=marketing, o=corporation", and "ou=marketing"
        ///     but <B>not</B> by "cn=admin" or "cn=admin,ou=marketing,o=corporation"
        ///     Note: For users of Netscape's SDK this method is comparable to contains.
        /// </summary>
        /// <param name="containerDn">
        ///     of a container.
        /// </param>
        /// <returns>
        ///     true if containerDN contains this DN.
        /// </returns>
        public bool IsDescendantOf(Dn containerDn)
        {
            var i = containerDn._rdnList.Count - 1; // index to an RDN of the ContainerDN
            var j = _rdnList.Count - 1; // index to an RDN of the ContainedDN

            // Search from the end of the DN for an RDN that matches the end RDN of
            // containerDN.
            while (!_rdnList[j].Equals(containerDn._rdnList[i]))
            {
                j--;
                if (j <= 0)
                {
                    return false;
                }

                // if the end RDN of containerDN does not have any equal
                // RDN in rdnList, then containerDN does not contain this DN
            }

            i--; // avoid a redundant compare
            j--;

            // step backwards to verify that all RDNs in containerDN exist in this DN
            for (; i >= 0 && j >= 0; i--, j--)
            {
                if (!_rdnList[j].Equals(containerDn._rdnList[i]))
                {
                    return false;
                }
            }

            // the DNs are identical and thus not contained
            if (j == 0 && i == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary> Adds the RDN to the beginning of the current DN.</summary>
        /// <param name="rdn">
        ///     an RDN to be added.
        /// </param>
        public void AddRdn(Rdn rdn)
        {
            _rdnList.Insert(0, rdn);
        }

        /// <summary> Adds the RDN to the beginning of the current DN.</summary>
        /// <param name="rdn">
        ///     an RDN to be added.
        /// </param>
        public void AddRdnToFront(Rdn rdn)
        {
            _rdnList.Insert(0, rdn);
        }

        /// <summary> Adds the RDN to the end of the current DN.</summary>
        /// <param name="rdn">
        ///     an RDN to be added.
        /// </param>
        public void AddRdnToBack(Rdn rdn)
        {
            _rdnList.Add(rdn);
        }

        public override int GetHashCode()
        {
            var hashCode = 5751775;
            hashCode = (hashCode * -1521134295) + EqualityComparer<List<Rdn>>.Default.GetHashCode(_rdnList);
            return hashCode;
        }
    }
}
