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
// Novell.Directory.Ldap.Utilclass.DN.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

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
    ///     component and are represented in one RDN (see RDN)
    /// </summary>
    /// <seealso cref="RDN">
    /// </seealso>
    public class DN
    {

        /// <summary> Retrieves a list of RDN Objects, or individual names of the DN</summary>
        /// <returns>
        ///     list of RDNs
        /// </returns>
        public virtual IList<RDN> RDNs { get; private set; } = new List<RDN>();

        /// <summary> Returns the Parent of this DN</summary>
        /// <returns>
        ///     Parent DN
        /// </returns>
        public virtual DN Parent
        {
            get
            {
                var parent = new DN
                {
                    RDNs = new List<RDN>(RDNs)
                };
                if (parent.RDNs.Count >= 1)
                    parent.RDNs.Remove(RDNs[0]); //remove first object
                return parent;
            }
        }

        //parser state identifiers.
        private const int LOOK_FOR_RDN_ATTR_TYPE = 1;
        private const int ALPHA_ATTR_TYPE = 2;
        private const int OID_ATTR_TYPE = 3;
        private const int LOOK_FOR_RDN_VALUE = 4;
        private const int QUOTED_RDN_VALUE = 5;
        private const int HEX_RDN_VALUE = 6;
        private const int UNQUOTED_RDN_VALUE = 7;

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

        public DN()
        {
        }

        /// <summary>
        ///     Constructs a new DN based on the specified string representation of a
        ///     distinguished name. The syntax of the DN must conform to that specified
        ///     in RFC 2253.
        /// </summary>
        /// <param name="dnString">
        ///     a string representation of the distinguished name
        /// </param>
        /// <exception>
        ///     IllegalArgumentException  if the the value of the dnString
        ///     parameter does not adhere to the syntax described in
        ///     RFC 2253
        /// </exception>
        public DN(string dnString)
        {
            /* the empty string is a valid DN */
            if (dnString.Length == 0)
                return;

            char currChar;
            char nextChar = default(char);
            int currIndex;
            var tokenBuf = new char[dnString.Length];
            int tokenIndex;
            int lastIndex;
            int valueStart;
            int state;
            var trailingSpaceCount = 0;
            var attrType = "";
            var attrValue = "";
            var rawValue = "";
            var hexDigitCount = 0;
            var currRDN = new RDN();

            //indicates whether an OID number has a first digit of ZERO
            var firstDigitZero = false;

            tokenIndex = 0;
            currIndex = 0;
            valueStart = 0;
            state = LOOK_FOR_RDN_ATTR_TYPE;
            lastIndex = dnString.Length - 1;
            while (currIndex <= lastIndex)
            {
                currChar = dnString[currIndex];
                switch (state)
                {
                    case LOOK_FOR_RDN_ATTR_TYPE:
                        LookForRdnAttribute(dnString, ref currChar, ref currIndex, tokenBuf, ref tokenIndex, lastIndex, ref state);
                        break;

                    case ALPHA_ATTR_TYPE:
                        AlphaAttribute(dnString, ref currChar, ref currIndex, tokenBuf, ref tokenIndex, lastIndex, ref state, ref attrType);
                        break;

                    case OID_ATTR_TYPE:
                        firstDigitZero = OidAttribute(dnString, ref currChar, ref currIndex, tokenBuf, ref tokenIndex, lastIndex, ref state, ref attrType);
                        break;


                    case LOOK_FOR_RDN_VALUE:
                        LookForRdnValue(dnString, ref currChar, ref currIndex, tokenBuf, ref tokenIndex, lastIndex, out valueStart, out state, ref hexDigitCount);
                        break;


                    case UNQUOTED_RDN_VALUE:
                        UnquotedRndValue(dnString, ref currChar, ref nextChar, ref currIndex, tokenBuf, ref tokenIndex, lastIndex, valueStart, ref state, ref trailingSpaceCount, attrType, ref attrValue, ref rawValue, ref currRDN);
                        break;


                    case QUOTED_RDN_VALUE:
                        QuotedRdnValue(dnString, ref currChar, ref nextChar, ref currIndex, tokenBuf, ref tokenIndex, lastIndex, valueStart, ref state, ref trailingSpaceCount, attrType, ref attrValue, ref rawValue, ref currRDN);
                        break;


                    case HEX_RDN_VALUE:
                        HexRdnValue(dnString, ref currChar, ref currIndex, tokenBuf, ref tokenIndex, lastIndex, valueStart, ref state, attrType, ref attrValue, ref rawValue, ref hexDigitCount, ref currRDN);
                        break;
                }
                currIndex++;
            }

            if (state == UNQUOTED_RDN_VALUE || state == HEX_RDN_VALUE && hexDigitCount % 2 == 0 && hexDigitCount != 0)
            {
                attrValue = new string(tokenBuf, 0, tokenIndex - trailingSpaceCount);
                rawValue = dnString.Substring(valueStart, currIndex - trailingSpaceCount - valueStart);
                currRDN.Add(attrType, attrValue, rawValue);
                RDNs.Add(currRDN);
            }
            else if (state == LOOK_FOR_RDN_VALUE)
            {
                attrValue = string.Empty;
                rawValue = dnString.Substring(valueStart);
                currRDN.Add(attrType, attrValue, rawValue);
                RDNs.Add(currRDN);
            }
            else
            {
                throw new ArgumentException(dnString);
            }
        } //end DN constructor (string dn)

        private void HexRdnValue(string dnString, ref char currChar, ref int currIndex, char[] tokenBuf, ref int tokenIndex, int lastIndex, int valueStart, ref int state, string attrType, ref string attrValue, ref string rawValue, ref int hexDigitCount, ref RDN currRDN)
        {
            if (!IsHexDigit(currChar) || currIndex > lastIndex)
            {
                //check for odd number of hex digits
                if (hexDigitCount % 2 != 0 || hexDigitCount == 0)
                    throw new ArgumentException(dnString);
                rawValue = dnString.Substring(valueStart, currIndex - valueStart);
                //skip any spaces
                while (currChar == ' ' && currIndex < lastIndex)
                    currChar = dnString[++currIndex];
                if (currChar == ',' || currChar == ';' || currChar == '+' || currIndex == lastIndex)
                {
                    attrValue = new string(tokenBuf, 0, tokenIndex);

                    //added by cameron
                    currRDN.Add(attrType, attrValue, rawValue);
                    if (currChar != '+')
                    {
                        RDNs.Add(currRDN);
                        currRDN = new RDN();
                    }
                    tokenIndex = 0;
                    state = LOOK_FOR_RDN_ATTR_TYPE;
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
        }

        private void QuotedRdnValue(string dnString, ref char currChar, ref char nextChar, ref int currIndex, char[] tokenBuf, ref int tokenIndex, int lastIndex, int valueStart, ref int state, ref int trailingSpaceCount, string attrType, ref string attrValue, ref string rawValue, ref RDN currRDN)
        {
            if (currChar == '"')
            {
                rawValue = dnString.Substring(valueStart, currIndex + 1 - valueStart);
                if (currIndex < lastIndex)
                    currChar = dnString[++currIndex];
                //skip any spaces
                while (currChar == ' ' && currIndex < lastIndex)
                    currChar = dnString[++currIndex];
                if (currChar == ',' || currChar == ';' || currChar == '+' || currIndex == lastIndex)
                {
                    attrValue = new string(tokenBuf, 0, tokenIndex);

                    currRDN.Add(attrType, attrValue, rawValue);
                    if (currChar != '+')
                    {
                        RDNs.Add(currRDN);
                        currRDN = new RDN();
                    }
                    trailingSpaceCount = 0;
                    tokenIndex = 0;
                    state = LOOK_FOR_RDN_ATTR_TYPE;
                }
                else
                    throw new ArgumentException(dnString);
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
                        throw new ArgumentException(dnString);
                }
                else if (NeedsEscape(currChar) || currChar == '#' || currChar == '=' || currChar == ' ')
                {
                    tokenBuf[tokenIndex++] = currChar;
                    trailingSpaceCount = 0;
                }
                else
                    throw new ArgumentException(dnString);
            }
            else
                tokenBuf[tokenIndex++] = currChar;
        }

        private void UnquotedRndValue(string dnString, ref char currChar, ref char nextChar, ref int currIndex, char[] tokenBuf, ref int tokenIndex, int lastIndex, int valueStart, ref int state, ref int trailingSpaceCount, string attrType, ref string attrValue, ref string rawValue, ref RDN currRDN)
        {
            if (currChar == '\\')
            {
                if (!(currIndex < lastIndex))
                    throw new ArgumentException(dnString);
                currChar = dnString[++currIndex];
                if (IsHexDigit(currChar))
                {
                    if (!(currIndex < lastIndex))
                        throw new ArgumentException(dnString);
                    nextChar = dnString[++currIndex];
                    if (IsHexDigit(nextChar))
                    {
                        tokenBuf[tokenIndex++] = HexToChar(currChar, nextChar);
                        trailingSpaceCount = 0;
                    }
                    else
                        throw new ArgumentException(dnString);
                }
                else if (NeedsEscape(currChar) || currChar == '#' || currChar == '=' || currChar == ' ')
                {
                    tokenBuf[tokenIndex++] = currChar;
                    trailingSpaceCount = 0;
                }
                else
                    throw new ArgumentException(dnString);
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

                currRDN.Add(attrType, attrValue, rawValue);
                if (currChar != '+')
                {
                    RDNs.Add(currRDN);
                    currRDN = new RDN();
                }

                trailingSpaceCount = 0;
                tokenIndex = 0;
                state = LOOK_FOR_RDN_ATTR_TYPE;
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
        }

        private static void LookForRdnValue(string dnString, ref char currChar, ref int currIndex, char[] tokenBuf, ref int tokenIndex, int lastIndex, out int valueStart, out int state, ref int hexDigitCount)
        {
            while (currChar == ' ')
            {
                if (currIndex < lastIndex)
                    currChar = dnString[++currIndex];
                else
                    throw new ArgumentException(dnString);
            }
            if (currChar == '"')
            {
                state = QUOTED_RDN_VALUE;
                valueStart = currIndex;
            }
            else if (currChar == '#')
            {
                hexDigitCount = 0;
                tokenBuf[tokenIndex++] = currChar;
                valueStart = currIndex;
                state = HEX_RDN_VALUE;
            }
            else
            {
                valueStart = currIndex;
                //check this character again in the UNQUOTED_RDN_VALUE state
                currIndex--;
                state = UNQUOTED_RDN_VALUE;
            }
        }

        private static bool OidAttribute(string dnString, ref char currChar, ref int currIndex, char[] tokenBuf, ref int tokenIndex, int lastIndex, ref int state, ref string attrType)
        {
            bool firstDigitZero;
            if (!char.IsDigit(currChar))
                throw new ArgumentException(dnString);
            firstDigitZero = currChar == '0';
            tokenBuf[tokenIndex++] = currChar;
            currChar = dnString[++currIndex];

            if (char.IsDigit(currChar) && firstDigitZero || currChar == '.' && firstDigitZero)
            {
                throw new ArgumentException(dnString);
            }

            //consume all numbers.
            while (char.IsDigit(currChar) && currIndex < lastIndex)
            {
                tokenBuf[tokenIndex++] = currChar;
                currChar = dnString[++currIndex];
            }
            if (currChar == '.')
            {
                tokenBuf[tokenIndex++] = currChar;
                //The state remains at OID_ATTR_TYPE
            }
            else
            {
                //skip any spaces
                while (currChar == ' ' && currIndex < lastIndex)
                    currChar = dnString[++currIndex];
                if (currChar == '=')
                {
                    attrType = new string(tokenBuf, 0, tokenIndex);
                    tokenIndex = 0;
                    state = LOOK_FOR_RDN_VALUE;
                }
                else
                    throw new ArgumentException(dnString);
            }

            return firstDigitZero;
        }

        private static void AlphaAttribute(string dnString, ref char currChar, ref int currIndex, char[] tokenBuf, ref int tokenIndex, int lastIndex, ref int state, ref string attrType)
        {
            if (char.IsLetter(currChar) || char.IsDigit(currChar) || currChar == '-')
                tokenBuf[tokenIndex++] = currChar;
            else
            {
                //skip any spaces
                while (currChar == ' ' && currIndex < lastIndex)
                    currChar = dnString[++currIndex];
                if (currChar == '=')
                {
                    attrType = new string(tokenBuf, 0, tokenIndex);
                    tokenIndex = 0;
                    state = LOOK_FOR_RDN_VALUE;
                }
                else
                    throw new ArgumentException(dnString);
            }
        }

        private static void LookForRdnAttribute(string dnString, ref char currChar, ref int currIndex, char[] tokenBuf, ref int tokenIndex, int lastIndex, ref int state)
        {
            while (currChar == ' ' && currIndex < lastIndex)
                currChar = dnString[++currIndex];
            if (char.IsLetter(currChar))
            {
                if (dnString.Substring(currIndex).StartsWith("oid.", StringComparison.InvariantCultureIgnoreCase))
                {
                    //form is "oid.###.##.###... or OID.###.##.###...
                    currIndex += 4; //skip oid. prefix and get to actual oid
                    if (currIndex > lastIndex)
                        throw new ArgumentException(dnString);
                    currChar = dnString[currIndex];
                    if (char.IsDigit(currChar))
                    {
                        tokenBuf[tokenIndex++] = currChar;
                        state = OID_ATTR_TYPE;
                    }
                    else
                        throw new ArgumentException(dnString);
                }
                else
                {
                    tokenBuf[tokenIndex++] = currChar;
                    state = ALPHA_ATTR_TYPE;
                }
            }
            else if (char.IsDigit(currChar))
            {
                --currIndex;
                state = OID_ATTR_TYPE;
            }
            else if (!(CharUnicodeInfo.GetUnicodeCategory(currChar) == UnicodeCategory.SpaceSeparator))
                throw new ArgumentException(dnString);
        }


        /// <summary>
        ///     Checks a character to see if it is valid hex digit 0-9, a-f, or
        ///     A-F (ASCII value ranges 48-47, 65-70, 97-102).
        /// </summary>
        /// <param name="ch">
        ///     the character to be tested.
        /// </param>
        /// <returns>
        ///     <code>true</code> if the character is a valid hex digit
        /// </returns>
        private static bool IsHexDigit(char ch)
        {
            if (ch < 58 && ch > 47 || ch < 71 && ch > 64 || ch < 103 && ch > 96)
                //ASCII A-F
                return true;
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
                return true;
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
                //ASCII 0-9
                result = (hex1 - 48) * 16;
            else if (hex1 < 71 && hex1 > 64)
                //ASCII a-f
                result = (hex1 - 55) * 16;
            else if (hex1 < 103 && hex1 > 96)
                //ASCII A-F
                result = (hex1 - 87) * 16;
            else
                throw new ArgumentException("Not hex digit");

            if (hex0 < 58 && hex0 > 47)
                //ASCII 0-9
                result += hex0 - 48;
            else if (hex0 < 71 && hex0 > 64)
                //ASCII a-f
                result += hex0 - 55;
            else if (hex0 < 103 && hex0 > 96)
                //ASCII A-F
                result += hex0 - 87;
            else
                throw new ArgumentException("Not hex digit");

            return (char)result;
        }

        /// <summary>
        ///     Creates and returns a string that represents this DN.  The string
        ///     follows RFC 2253, which describes String representation of DN's and
        ///     RDN's
        /// </summary>
        /// <returns>
        ///     A DN string.
        /// </returns>
        public override string ToString()
        {
            var length = RDNs.Count;
            var dn = "";
            if (length < 1)
                return null;
            dn = RDNs[0].ToString();
            for (var i = 1; i < length; i++)
            {
                dn += "," + RDNs[i];
            }
            return dn;
        }


        public override bool Equals(object toDN)
        {
            return Equals((DN)toDN);
        }

        public bool Equals(DN toDN)
        {
            var aList = toDN.RDNs;
            var length = aList.Count;

            if (RDNs.Count != length)
                return false;

            for (var i = 0; i < length; i++)
            {
                if (!RDNs[i].Equals(toDN.RDNs[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        ///     return a string array of the individual RDNs contained in the DN
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
        ///     the leftmost rdn in the first element of the array
        /// </returns>
        public virtual string[] ExplodeDN(bool noTypes)
        {
            var length = RDNs.Count;
            var rdns = new string[length];
            for (var i = 0; i < length; i++)
                rdns[i] = RDNs[i].ToString(noTypes);
            return rdns;
        }

        /// <summary> Retrieves the count of RDNs, or individule names, in the Distinguished name</summary>
        /// <returns>
        ///     the count of RDN
        /// </returns>
        public virtual int CountRDNs => RDNs.Count;

        /// <summary>
        ///     Determines if this DN is <I>contained</I> by the DN passed in.  For
        ///     example:  "cn=admin, ou=marketing, o=corporation" is contained by
        ///     "o=corporation", "ou=marketing, o=corporation", and "ou=marketing"
        ///     but <B>not</B> by "cn=admin" or "cn=admin,ou=marketing,o=corporation"
        ///     Note: For users of Netscape's SDK this method is comparable to contains
        /// </summary>
        /// <param name="containerDN">
        ///     of a container
        /// </param>
        /// <returns>
        ///     true if containerDN contains this DN
        /// </returns>
        public virtual bool IsDescendantOf(DN containerDN)
        {
            var i = containerDN.RDNs.Count - 1; //index to an RDN of the ContainerDN
            var j = RDNs.Count - 1; //index to an RDN of the ContainedDN
            //Search from the end of the DN for an RDN that matches the end RDN of
            //containerDN.
            while (!RDNs[j].Equals(containerDN.RDNs[i]))
            {
                j--;
                if (j <= 0)
                    return false;
                //if the end RDN of containerDN does not have any equal
                //RDN in rdnList, then containerDN does not contain this DN
            }
            i--; //avoid a redundant compare
            j--;
            //step backwards to verify that all RDNs in containerDN exist in this DN
            for (; i >= 0 && j >= 0; i--, j--)
            {
                if (!RDNs[j].Equals(containerDN.RDNs[i]))
                    return false;
            }
            if (j == 0 && i == 0)
                //the DNs are identical and thus not contained
                return false;

            return true;
        }

        /// <summary> Adds the RDN to the beginning of the current DN.</summary>
        /// <param name="rdn">
        ///     an RDN to be added
        /// </param>
        public virtual void AddRDN(RDN rdn)
        {
            RDNs.Insert(0, rdn);
        }

        /// <summary> Adds the RDN to the beginning of the current DN.</summary>
        /// <param name="rdn">
        ///     an RDN to be added
        /// </param>
        public virtual void AddRDNToFront(RDN rdn)
        {
            RDNs.Insert(0, rdn);
        }

        /// <summary> Adds the RDN to the end of the current DN</summary>
        /// <param name="rdn">
        ///     an RDN to be added
        /// </param>
        public virtual void AddRDNToBack(RDN rdn)
        {
            RDNs.Add(rdn);
        }
    } //end class DN
}