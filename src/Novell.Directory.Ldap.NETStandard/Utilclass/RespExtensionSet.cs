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
// Novell.Directory.Ldap.Utilclass.RespExtensionSet.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     This  class  extends the AbstractSet and Implements the Set
    ///     so that it can be used to maintain a list of currently
    ///     registered extended responses.
    /// </summary>
    public class RespExtensionSet : IDictionary<string, Type>
    {
        private readonly ConcurrentDictionary<string, Type> _map = new ConcurrentDictionary<string, Type>();

        /// <summary>
        ///     Returns the number of extensions in this set.
        /// </summary>
        /// <returns>
        ///     number of extensions in this set.
        /// </returns>
        public int Count => _map.Count;

        public ICollection<string> Keys => _map.Keys;

        public ICollection<Type> Values => _map.Values;

        public bool IsReadOnly => false;

        public Type this[string key]
        {
            get
            {
                if (_map.ContainsKey(key))
                    return _map[key];
                return null;
            }
            set
            {
                if (_map.ContainsKey(key))
                    _map[key] = value;
                _map.TryAdd(key, value);
            }
        }

        public void Add(string key, Type value) => _map.TryAdd(key, value);

        public bool ContainsKey(string key) => _map.ContainsKey(key);

        public bool Remove(string key) => _map.TryRemove(key, out var ret);

        public bool TryGetValue(string key, out Type value) => _map.TryGetValue(key, out value);

        public void Add(KeyValuePair<string, Type> item) => _map.TryAdd(item.Key, item.Value);

        public void Clear() => _map.Clear();

        public bool Contains(KeyValuePair<string, Type> item) => _map.ContainsKey(item.Key) && _map[item.Key] == item.Value;

        private static object locker = new object();
        public void CopyTo(KeyValuePair<string, Type>[] array, int arrayIndex)
        {
            lock (locker)
            {
                var tmp = new List<KeyValuePair<string, Type>>(_map.Count);
                foreach (var item in _map)
                    tmp.Add(item);
                array = tmp.ToArray();
            }
        }

        public bool Remove(KeyValuePair<string, Type> item) => Remove(item.Key);

        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => _map.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}