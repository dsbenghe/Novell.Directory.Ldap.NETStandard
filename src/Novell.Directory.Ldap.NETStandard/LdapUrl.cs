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

using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections.Generic;
using System.Text;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Encapsulates parameters of an Ldap URL query as defined in RFC2255.
    ///     An LdapUrl object can be passed to LdapConnection.search to retrieve
    ///     search results.
    /// </summary>
    /// <seealso cref="LdapConnection.Search">
    /// </seealso>
    public class LdapUrl
    {
        private const int DefaultScope = LdapConnection.ScopeBase;
        private readonly bool _ipV6 = false; // TCP/IP V6
        private string _dn; // Base DN
        private int _port; // Port

        // Broken out parts of the URL

        /// <summary>
        ///     Constructs a URL object with the specified string as the URL.
        /// </summary>
        /// <param name="url">
        ///     An Ldap URL string, e.g.
        ///     "ldap://ldap.example.com:80/dc=example,dc=com?cn,
        ///     sn?sub?(objectclass=inetOrgPerson)".
        /// </param>
        /// <exception>
        ///     MalformedURLException The specified URL cannot be parsed.
        /// </exception>
        public LdapUrl(string url)
        {
            ParseUrl(url);
        }

        /// <summary>
        ///     Constructs a URL object with the specified host, port, and DN.
        ///     This form is used to create URL references to a particular object
        ///     in the directory.
        /// </summary>
        /// <param name="host">
        ///     Host identifier of Ldap server, or null for
        ///     "localhost".
        /// </param>
        /// <param name="port">
        ///     The port number for Ldap server (use
        ///     LdapConnection.DEFAULT_PORT for default port).
        /// </param>
        /// <param name="dn">
        ///     Distinguished name of the base object of the search.
        /// </param>
        public LdapUrl(string host, int port, string dn)
        {
            Host = host;
            _port = port;
            _dn = dn;
        }

        /// <summary>
        ///     Constructs an Ldap URL with all fields explicitly assigned, to
        ///     specify an Ldap search operation.
        /// </summary>
        /// <param name="host">
        ///     Host identifier of Ldap server, or null for
        ///     "localhost".
        /// </param>
        /// <param name="port">
        ///     The port number for Ldap server (use
        ///     LdapConnection.DEFAULT_PORT for default port).
        /// </param>
        /// <param name="dn">
        ///     Distinguished name of the base object of the search.
        /// </param>
        /// <param name="attrNames">
        ///     Names or OIDs of attributes to retrieve.  Passing a
        ///     null array signifies that all user attributes are to be
        ///     retrieved. Passing a value of "*" allows you to specify
        ///     that all user attributes as well as any specified
        ///     operational attributes are to be retrieved.
        /// </param>
        /// <param name="scope">
        ///     Depth of search (in DN namespace). Use one of
        ///     SCOPE_BASE, SCOPE_ONE, SCOPE_SUB from LdapConnection.
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        /// </param>
        /// <param name="extensions">
        ///     Extensions provide a mechanism to extend the
        ///     functionality of Ldap URLs. Currently no
        ///     Ldap URL extensions are defined. Each extension
        ///     specification is a type=value expression, and  may
        ///     be. <code>null</code> or empty.  The =value part may be
        ///     omitted. The expression may be prefixed with '!' if it
        ///     is mandatory for the evaluation of the URL.
        /// </param>
        public LdapUrl(string host, int port, string dn, string[] attrNames, int scope, string filter,
            string[] extensions)
        {
            Host = host;
            _port = port;
            _dn = dn;
            AttributeArray = new string[attrNames.Length];
            attrNames.CopyTo(AttributeArray, 0);
            Scope = scope;
            Filter = filter;
            Extensions = new string[extensions.Length];
            extensions.CopyTo(Extensions, 0);
        }

        /// <summary>
        ///     Constructs an Ldap URL with all fields explicitly assigned, including
        ///     isSecure, to specify an Ldap search operation.
        /// </summary>
        /// <param name="host">
        ///     Host identifier of Ldap server, or null for
        ///     "localhost".
        /// </param>
        /// <param name="port">
        ///     The port number for Ldap server (use
        ///     LdapConnection.DEFAULT_PORT for default port).
        /// </param>
        /// <param name="dn">
        ///     Distinguished name of the base object of the search.
        /// </param>
        /// <param name="attrNames">
        ///     Names or OIDs of attributes to retrieve.  Passing a
        ///     null array signifies that all user attributes are to be
        ///     retrieved. Passing a value of "*" allows you to specify
        ///     that all user attributes as well as any specified
        ///     operational attributes are to be retrieved.
        /// </param>
        /// <param name="scope">
        ///     Depth of search (in DN namespace). Use one of
        ///     SCOPE_BASE, SCOPE_ONE, SCOPE_SUB from LdapConnection.
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        ///     from LdapConnection: SCOPE_BASE, SCOPE_ONE, SCOPE_SUB.
        /// </param>
        /// <param name="extensions">
        ///     Extensions provide a mechanism to extend the
        ///     functionality of Ldap URLs. Currently no
        ///     Ldap URL extensions are defined. Each extension
        ///     specification is a type=value expression, and  may
        ///     be. <code>null</code> or empty.  The =value part may be
        ///     omitted. The expression may be prefixed with '!' if it
        ///     is mandatory for the evaluation of the URL.
        /// </param>
        /// <param name="secure">
        ///     If true creates an Ldap URL of the ldaps type.
        /// </param>
        public LdapUrl(string host, int port, string dn, string[] attrNames, int scope, string filter,
            string[] extensions, bool secure)
        {
            Host = host;
            _port = port;
            _dn = dn;
            AttributeArray = attrNames;
            Scope = scope;
            Filter = filter;
            Extensions = new string[extensions.Length];
            extensions.CopyTo(Extensions, 0);
            Secure = secure;
        }

        /// <summary>
        ///     Returns an array of attribute names specified in the URL.
        /// </summary>
        /// <returns>
        ///     An array of attribute names in the URL.
        /// </returns>
        public string[] AttributeArray { get; private set; }

        /// <summary>
        ///     Returns an enumerator for the attribute names specified in the URL.
        /// </summary>
        /// <returns>
        ///     An enumeration of attribute names.
        /// </returns>
        public IEnumerator<string> Attributes => new ArrayEnumeration<string>(AttributeArray);

        /// <summary>
        ///     Returns any Ldap URL extensions specified, or null if none are
        ///     specified. Each extension is a type=value expression. The =value part
        ///     MAY be omitted. The expression MAY be prefixed with '!' if it is
        ///     mandatory for evaluation of the URL.
        /// </summary>
        /// <returns>
        ///     string array of extensions.
        /// </returns>
        public string[] Extensions { get; private set; }

        /// <summary>
        ///     Returns the search filter or. <code>null</code> if none was specified.
        /// </summary>
        /// <returns>
        ///     The search filter.
        /// </returns>
        public string Filter { get; private set; }

        /// <summary>
        ///     Returns the name of the Ldap server in the URL.
        /// </summary>
        /// <returns>
        ///     The host name specified in the URL.
        /// </returns>
        public string Host { get; private set; }

        /// <summary>
        ///     Returns the port number of the Ldap server in the URL.
        /// </summary>
        /// <returns>
        ///     The port number in the URL.
        /// </returns>
        public int Port
        {
            get
            {
                if (_port == 0)
                {
                    return LdapConnection.DefaultPort;
                }

                return _port;
            }
        }

        /// <summary>
        ///     Returns the depth of search. It returns one of the following from
        ///     LdapConnection: SCOPE_BASE, SCOPE_ONE, or SCOPE_SUB.
        /// </summary>
        /// <returns>
        ///     The search scope.
        /// </returns>
        public int Scope { get; private set; } = DefaultScope;

        /// <summary>
        ///     Returns true if the URL is of the type ldaps (Ldap over SSL, a predecessor
        ///     to startTls).
        /// </summary>
        /// <returns>
        ///     whether this is a secure Ldap url or not.
        /// </returns>
        public bool Secure { get; private set; }

        /// <summary>
        ///     Returns a clone of this URL object.
        /// </summary>
        /// <returns>
        ///     clone of this URL object.
        /// </returns>
        public object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        ///     Decodes a URL-encoded string.
        ///     Any occurences of %HH are decoded to the hex value represented.
        ///     However, this method does NOT decode "+" into " ".
        /// </summary>
        /// <param name="urlEncoded">
        ///     String to decode.
        /// </param>
        /// <returns>
        ///     The decoded string.
        /// </returns>
        /// <exception>
        ///     MalformedURLException The URL could not be parsed.
        /// </exception>
        public static string Decode(string urlEncoded)
        {
            var searchStart = 0;
            var fieldStart = urlEncoded.IndexOf("%", searchStart);

            // Return now if no encoded data
            if (fieldStart < 0)
            {
                return urlEncoded;
            }

            // Decode the %HH value and copy to new string buffer
            var fieldEnd = 0; // end of previous field
            var dataLen = urlEncoded.Length;
            var decoded = new StringBuilder(dataLen);

            while (true)
            {
                if (fieldStart > dataLen - 3)
                {
                    throw new UriFormatException(
                        "LdapUrl.decode: must be two hex characters following escape character '%'");
                }

                if (fieldStart < 0)
                {
                    fieldStart = dataLen;
                }

                // Copy to string buffer from end of last field to start of next
                decoded.Append(urlEncoded.Substring(fieldEnd, fieldStart - fieldEnd));
                fieldStart += 1;

                if (fieldStart >= dataLen)
                {
                    break;
                }

                fieldEnd = fieldStart + 2;
                try
                {
                    decoded.Append((char)Convert.ToInt32(urlEncoded.Substring(fieldStart, fieldEnd - fieldStart), 16));
                }
                catch (FormatException ex)
                {
                    throw new UriFormatException("LdapUrl.decode: error converting hex characters to integer \"" +
                                                 ex.Message + "\"");
                }

                searchStart = fieldEnd;
                if (searchStart == dataLen)
                {
                    break;
                }

                fieldStart = urlEncoded.IndexOf("%", searchStart);
            }

            return decoded.ToString();
        }

        /// <summary>
        ///     Encodes an arbitrary string using the URL encoding rules.
        ///     Any illegal characters are encoded as %HH.
        /// </summary>
        /// <param name="toEncode">
        ///     The string to encode.
        /// </param>
        /// <returns>
        ///     The URL-encoded string.
        ///     Comment: An illegal character consists of any non graphical US-ASCII character, Unsafe, or reserved characters.
        /// </returns>
        public static string Encode(string toEncode)
        {
            var buffer = new StringBuilder(toEncode.Length); // empty but initial capicity of 'length'
            string temp;
            char currChar;

            for (var i = 0; i < toEncode.Length; i++)
            {
                currChar = toEncode[i];

                if (currChar <= 0x1F || currChar == 0x7F || (currChar >= 0x80 && currChar <= 0xFF) || currChar == '<' ||
                    currChar == '>' || currChar == '\"' || currChar == '#' || currChar == '%' || currChar == '{' ||
                    currChar == '}' || currChar == '|' || currChar == '\\' || currChar == '^' || currChar == '~' ||
                    currChar == '[' || currChar == '\'' || currChar == ';' || currChar == '/' || currChar == '?' ||
                    currChar == ':' || currChar == '@' || currChar == '=' || currChar == '&')
                {
                    temp = Convert.ToString(currChar, 16);
                    if (temp.Length == 1)
                    {
                        buffer.Append("%0" + temp);
                    }

                    // if(temp.length()==2) this can only be two or one digit long.
                    else
                    {
                        buffer.Append("%" + Convert.ToString(currChar, 16));
                    }
                }
                else
                {
                    buffer.Append(currChar);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        ///     Returns the base distinguished name encapsulated in the URL.
        /// </summary>
        /// <returns>
        ///     The base distinguished name specified in the URL, or null if none.
        /// </returns>
        public string GetDn()
        {
            return _dn;
        }

        /// <summary> Sets the base distinguished name encapsulated in the URL.</summary>
        internal void SetDn(string dn)
        {
            _dn = dn;
        }

        /// <summary>
        ///     Returns a valid string representation of this Ldap URL.
        /// </summary>
        /// <returns>
        ///     The string representation of the Ldap URL.
        /// </returns>
        public override string ToString()
        {
            var url = new StringBuilder(256);

            // Scheme
            if (Secure)
            {
                url.Append("ldaps://");
            }
            else
            {
                url.Append("ldap://");
            }

            // Host:port/dn
            if (_ipV6)
            {
                url.Append("[" + Host + "]");
            }
            else
            {
                url.Append(Host);
            }

            // Port not specified
            if (_port != 0)
            {
                url.Append(":" + _port);
            }

            if (_dn == null && AttributeArray == null && Scope == DefaultScope && Filter == null &&
                Extensions == null)
            {
                return url.ToString();
            }

            url.Append("/");

            if (_dn != null)
            {
                url.Append(_dn);
            }

            if (AttributeArray == null && Scope == DefaultScope && Filter == null && Extensions == null)
            {
                return url.ToString();
            }

            // attributes
            url.Append("?");
            if (AttributeArray != null)
            {
                // should we check also for attrs != "*"
                for (var i = 0; i < AttributeArray.Length; i++)
                {
                    url.Append(AttributeArray[i]);

                    if (i < AttributeArray.Length - 1)
                    {
                        url.Append(",");
                    }
                }
            }

            if (Scope == DefaultScope && Filter == null && Extensions == null)
            {
                return url.ToString();
            }

            // scope
            url.Append("?");
            if (Scope != DefaultScope)
            {
                if (Scope == LdapConnection.ScopeOne)
                {
                    url.Append("one");
                }
                else
                {
                    url.Append("sub");
                }
            }

            if (Filter == null && Extensions == null)
            {
                return url.ToString();
            }

            // filter
            if (Filter == null)
            {
                url.Append("?");
            }
            else
            {
                url.Append("?" + Filter);
            }

            if (Extensions == null)
            {
                return url.ToString();
            }

            // extensions
            url.Append("?");
            if (Extensions != null)
            {
                for (var i = 0; i < Extensions.Length; i++)
                {
                    url.Append(Extensions[i]);

                    if (i < Extensions.Length - 1)
                    {
                        url.Append(",");
                    }
                }
            }

            return url.ToString();
        }

        private string[] ParseList(string listStr, char delimiter, int listStart, int listEnd)

        // end of list + 1
        {
            // TODO: This is just string.Split with the given delimiter on a Substring?

            // Check for and empty string
            if (listEnd - listStart < 1)
            {
                return null;
            }

            // First count how many items are specified
            var itemStart = listStart;
            int itemEnd;
            var itemCount = 0;

            while (itemStart > 0)
            {
                // itemStart == 0 if no delimiter found
                itemCount += 1;
                itemEnd = listStr.IndexOf(delimiter, itemStart);

                if (itemEnd > 0 && itemEnd < listEnd)
                {
                    itemStart = itemEnd + 1;
                }
                else
                {
                    break;
                }
            }

            // Now fill in the array with the attributes
            itemStart = listStart;
            var list = new string[itemCount];
            itemCount = 0;

            while (itemStart > 0)
            {
                if (itemStart >= listStr.Length)
                {
                    break;
                }

                itemEnd = listStr.IndexOf(delimiter, itemStart);

                if (itemStart <= listEnd)
                {
                    if (itemEnd < 0)
                    {
                        itemEnd = listEnd;
                    }

                    if (itemEnd > listEnd)
                    {
                        itemEnd = listEnd;
                    }

                    list[itemCount] = listStr.Substring(itemStart, itemEnd - itemStart);
                    itemStart = itemEnd + 1;
                    itemCount += 1;
                }
                else
                {
                    break;
                }
            }

            return list;
        }

        private void ParseUrl(string url)
        {
            if (url == null)
            {
                throw new UriFormatException("LdapUrl: URL cannot be null");
            }

            // TODO: There are WAY too many Substring calls here, causing unneccessary garbage
            var scanStart = 0;
            var scanEnd = url.Length;

            // Check if URL is enclosed by < & >
            if (url[scanStart] == '<')
            {
                if (url[scanEnd - 1] != '>')
                {
                    throw new UriFormatException("LdapUrl: URL bad enclosure");
                }

                scanStart += 1;
                scanEnd -= 1;
            }

            // Determine the URL scheme and set appropriate default port
            if (url.Substring(scanStart, scanStart + 4 - scanStart).EqualsOrdinalCI("URL:"))
            {
                scanStart += 4;
            }

            if (url.Substring(scanStart, scanStart + 7 - scanStart).EqualsOrdinalCI("ldap://"))
            {
                scanStart += 7;
                _port = LdapConnection.DefaultPort;
            }
            else if (url.Substring(scanStart, scanStart + 8 - scanStart).EqualsOrdinalCI("ldaps://"))
            {
                Secure = true;
                scanStart += 8;
                _port = LdapConnection.DefaultSslPort;
            }
            else
            {
                throw new UriFormatException("LdapUrl: URL scheme is not ldap");
            }

            // Find where host:port ends and dn begins
            var dnStart = url.IndexOf("/", scanStart);
            var hostPortEnd = scanEnd;
            var novell = false;
            if (dnStart < 0)
            {
                /*
                * Kludge. check for ldap://111.222.333.444:389??cn=abc,o=company
                *
                * Check for broken Novell referral format.  The dn is in
                * the scope position, but the required slash is missing.
                * This is illegal syntax but we need to account for it.
                * Fortunately it can't be confused with anything real.
                */
                dnStart = url.IndexOf("?", scanStart);
                if (dnStart > 0)
                {
                    if (url[dnStart + 1] == '?')
                    {
                        hostPortEnd = dnStart;
                        dnStart += 1;
                        novell = true;
                    }
                    else
                    {
                        dnStart = -1;
                    }
                }
            }
            else
            {
                hostPortEnd = dnStart;
            }

            // Check for IPV6 "[ipaddress]:port"
            int portStart;
            var hostEnd = hostPortEnd;
            if (url[scanStart] == '[')
            {
                hostEnd = url.IndexOf(']', scanStart + 1);
                if (hostEnd >= hostPortEnd || hostEnd == -1)
                {
                    throw new UriFormatException("LdapUrl: \"]\" is missing on IPV6 host name");
                }

                // Get host w/o the [ & ]
                Host = url.Substring(scanStart + 1, hostEnd - (scanStart + 1));
                portStart = url.IndexOf(":", hostEnd);
                if (portStart < hostPortEnd && portStart != -1)
                {
                    // port is specified
                    _port = int.Parse(url.Substring(portStart + 1, hostPortEnd - (portStart + 1)));
                }
            }
            else
            {
                portStart = url.IndexOf(":", scanStart);

                // Isolate the host and port
                if (portStart < 0 || portStart > hostPortEnd)
                {
                    // no port is specified, we keep the default
                    Host = url.Substring(scanStart, hostPortEnd - scanStart);
                }
                else
                {
                    // port specified in URL
                    Host = url.Substring(scanStart, portStart - scanStart);
                    _port = int.Parse(url.Substring(portStart + 1, hostPortEnd - (portStart + 1)));
                }
            }

            scanStart = hostPortEnd + 1;
            if (scanStart >= scanEnd || dnStart < 0)
            {
                return;
            }

            // Parse out the base dn
            scanStart = dnStart + 1;
            var attrsStart = url.IndexOf('?', scanStart);
            if (attrsStart < 0)
            {
                _dn = url.Substring(scanStart, scanEnd - scanStart);
            }
            else
            {
                _dn = url.Substring(scanStart, attrsStart - scanStart);
            }

            scanStart = attrsStart + 1;

            // Wierd novell syntax can have nothing beyond the dn
            if (scanStart >= scanEnd || attrsStart < 0 || novell)
            {
                return;
            }

            // Parse out the attributes
            var scopeStart = url.IndexOf('?', scanStart);
            if (scopeStart < 0)
            {
                scopeStart = scanEnd - 1;
            }

            AttributeArray = ParseList(url, ',', attrsStart + 1, scopeStart);
            scanStart = scopeStart + 1;
            if (scanStart >= scanEnd)
            {
                return;
            }

            // Parse out the scope
            var filterStart = url.IndexOf('?', scanStart);
            string scopeStr;
            if (filterStart < 0)
            {
                scopeStr = url.Substring(scanStart, scanEnd - scanStart);
            }
            else
            {
                scopeStr = url.Substring(scanStart, filterStart - scanStart);
            }

            if (scopeStr.IsEmpty() || scopeStr.EqualsOrdinalCI("base"))
            {
                Scope = LdapConnection.ScopeBase;
            }
            else if (scopeStr.EqualsOrdinalCI("one"))
            {
                Scope = LdapConnection.ScopeOne;
            }
            else if (scopeStr.EqualsOrdinalCI("sub"))
            {
                Scope = LdapConnection.ScopeSub;
            }
            else
            {
                throw new UriFormatException("LdapUrl: URL invalid scope");
            }

            scanStart = filterStart + 1;
            if (scanStart >= scanEnd || filterStart < 0)
            {
                return;
            }

            // Parse out the filter
            scanStart = filterStart + 1;
            string filterStr;
            var extStart = url.IndexOf('?', scanStart);
            if (extStart < 0)
            {
                filterStr = url.Substring(scanStart, scanEnd - scanStart);
            }
            else
            {
                filterStr = url.Substring(scanStart, extStart - scanStart);
            }

            if (filterStr.IsNotEmpty())
            {
                Filter = filterStr; // Only modify if not the default filter
            }

            scanStart = extStart + 1;
            if (scanStart >= scanEnd || extStart < 0)
            {
                return;
            }

            // Parse out the extensions
            var end = url.IndexOf('?', scanStart);
            if (end > 0)
            {
                throw new UriFormatException("LdapUrl: URL has too many ? fields");
            }

            Extensions = ParseList(url, ',', scanStart, scanEnd);
        }
    }
}
