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
using System.Globalization;
using System.Text;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Helpers for building LDAP search filters from untrusted input safely (RFC 4515).
    ///     Use <see cref="EscapeValue(string)"/> or <see cref="Format(string, object[])"/> whenever a value
    ///     that did not originate in your own code (a user name, a DN read from the directory, a GUID)
    ///     is placed inside an assertion value, so it cannot change the structure of the filter.
    /// </summary>
    public static class LdapFilter
    {
        /// <summary>
        ///     Escapes an assertion value for use inside a filter: <c>*</c> <c>(</c> <c>)</c> <c>\</c> and NUL
        ///     (the characters RFC 4515 section 3 requires), plus other control characters, are replaced by
        ///     their <c>\xx</c> hexadecimal form. Other characters, including non-ASCII, pass through unchanged.
        /// </summary>
        /// <param name="value">The raw value. <c>null</c> is treated as an empty string.</param>
        /// <returns>The escaped value, safe to embed between <c>=</c> and <c>)</c> in a filter.</returns>
        public static string EscapeValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder sb = null;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (NeedsEscape(c))
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder(value.Length + 8).Append(value, 0, i);
                    }

                    AppendHex(sb, (byte)c);
                }
                else
                {
                    sb?.Append(c);
                }
            }

            return sb == null ? value : sb.ToString();
        }

        /// <summary>
        ///     Escapes a binary value (for example an Active Directory <c>objectGUID</c> or <c>objectSid</c>)
        ///     by encoding every byte as <c>\xx</c>.
        /// </summary>
        /// <param name="value">The raw bytes. <c>null</c> is treated as empty.</param>
        /// <returns>The escaped value.</returns>
        public static string EscapeValue(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length * 3);
            foreach (var b in value)
            {
                AppendHex(sb, b);
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Builds a filter from a template with <see cref="string.Format(string, object[])"/> placeholders,
        ///     escaping every argument first. Strings and <c>byte[]</c> are escaped with
        ///     <see cref="EscapeValue(string)"/> / <see cref="EscapeValue(byte[])"/>; any other argument is
        ///     converted with <see cref="CultureInfo.InvariantCulture"/> and then escaped.
        ///     <code>LdapFilter.Format("(&amp;(objectClass=person)(sAMAccountName={0}))", userInput)</code>
        /// </summary>
        /// <param name="template">The filter template. Only the placeholders are substituted; the template itself is not escaped.</param>
        /// <param name="args">The values to escape and substitute.</param>
        /// <returns>The complete filter.</returns>
        public static string Format(string template, params object[] args)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (args == null || args.Length == 0)
            {
                return template;
            }

            var escaped = new object[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case null:
                        escaped[i] = string.Empty;
                        break;
                    case string s:
                        escaped[i] = EscapeValue(s);
                        break;
                    case byte[] b:
                        escaped[i] = EscapeValue(b);
                        break;
                    default:
                        escaped[i] = EscapeValue(Convert.ToString(args[i], CultureInfo.InvariantCulture));
                        break;
                }
            }

            return string.Format(CultureInfo.InvariantCulture, template, escaped);
        }

        private static bool NeedsEscape(char c)
        {
            return c == '*' || c == '(' || c == ')' || c == '\\' || c < ' ' || c == '\x7f';
        }

        private static void AppendHex(StringBuilder sb, byte b)
        {
            sb.Append('\\').Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }
    }
}
