#nullable enable
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

using Novell.Directory.Ldap.Rfc2251;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     This  class implements a Set
    ///     so that it can be used to maintain a list of currently
    ///     registered extended responses.
    /// </summary>
    /// <typeparam name="TOut">The base-class of the output.</typeparam>
    public class RespExtensionSet<TOut> : IEnumerable<KeyValuePair<string, Func<RfcLdapMessage, TOut>>>
    {
        private readonly ConcurrentDictionary<string, Func<RfcLdapMessage, TOut>> _map;

        public RespExtensionSet()
        {
            _map = new ConcurrentDictionary<string, Func<RfcLdapMessage, TOut>>();
        }

        /// <summary>
        ///     Returns the number of extensions in this set.
        /// </summary>
        /// <returns>
        ///     number of extensions in this set.
        /// </returns>
        public int Count => _map.Count;

        /// <summary>
        ///     Adds or replaces <paramref name="responseFactory"/> to the current list of registered responses.
        /// </summary>
        public void RegisterResponseExtension(string oid, Func<RfcLdapMessage, TOut> responseFactory)
        {
            _map[oid] = responseFactory;
        }

        /// <summary>
        ///     Returns an iterator over the responses in this set.  The responses
        ///     returned from this iterator are not in any particular order.
        /// </summary>
        /// <returns>
        ///     iterator over the responses in this set.
        /// </returns>
        public IEnumerator<KeyValuePair<string, Func<RfcLdapMessage, TOut>>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Searches the list of registered responses for a matching response.  We
        /// search using the OID string.  If a match is found we return the
        /// Class name that was provided to us on registration.
        /// </summary>
        public bool TryFindResponseExtension(string searchOid, [MaybeNullWhen(false)] out Func<RfcLdapMessage, TOut> responseFactory)
        {
            return _map.TryGetValue(searchOid, out responseFactory);
        }
    }
}
