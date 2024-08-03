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

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     The. <code>MessageVector</code> class implements extends the
    ///     existing Vector class so that it can be used to maintain a
    ///     list of currently registered control responses.
    /// </summary>
    public class RespControlVector
    {
        public delegate LdapControl LdapControlFactory(string oid, bool critical, byte[] value);

        private readonly ConcurrentDictionary<string, LdapControlFactory> _controls = new (StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds a control to the current list of registered response controls.
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="controlFactory"></param>
        public void RegisterResponseControl(string oid, LdapControlFactory controlFactory)
        {
            _controls[oid] = controlFactory;
        }

        /// <summary>
        /// Searches the list of registered controls for a matching control.
        /// We search using the OID string.
        /// If a match is found we return the Class name that was provided to us on registration.
        /// </summary>
        /// <param name="searchOid"></param>
        /// <returns></returns>
        public bool TryFindResponseControlFactory(string searchOid, [MaybeNullWhen(false)] out LdapControlFactory responseFactory)
        {
            return _controls.TryGetValue(searchOid, out responseFactory);
        }
    }
}
