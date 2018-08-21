// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// Source files:
// https://github.com/dotnet/corefx/blob/bffef76f6af208e2042a2f27bc081ee908bb390b/src/System.Collections.Specialized/src/System/Collections/Specialized/NameobjectCollectionBase.cs
// https://github.com/dotnet/corefx/blob/bffef76f6af208e2042a2f27bc081ee908bb390b/src/System.Collections.Specialized/src/System/Collections/Specialized/NameValueCollection.cs
// https://github.com/dotnet/corefx/blob/d19a246dfd5c638b6f8f2e66cef2a71e968ab0da/src/System.Web.HttpUtility/src/System/Web/HttpUtility.cs
// https://github.com/dotnet/corefx/blob/5a37188238e90d8566e6161b43ac483a671bb7d2/src/System.Collections.Specialized/src/Resources/Strings.resx
// https://raw.githubusercontent.com/dotnet/corefx/d19a246dfd5c638b6f8f2e66cef2a71e968ab0da/src/System.Web.HttpUtility/src/System/Web/Util/HttpEncoder.cs
//
// .net Standard 1.3 does not have a NameValueCollection, hence this.

using System;
using System.Collections;
using System.Text;

namespace Novell.Directory.Ldap.Utilclass
{
    public class HttpQSCollection : ICollection
    {
        private ArrayList _entriesArray;
        private volatile Hashtable _entriesTable;
        private volatile NameobjectEntry _nullKeyEntry;
        private KeysCollection _keys;
        private int _version;
        private object _syncRoot;
        private string[] _all;
        private string[] _allKeys;

        private static readonly StringComparer s_defaultComparer = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// <para> Creates an empty <see cref='HttpQSCollection'/> instance with the default initial capacity and using the default case-insensitive hash
        ///    code provider and the default case-insensitive comparer.</para>
        /// </summary>
        public HttpQSCollection() : this(s_defaultComparer)
        {
        }

        public HttpQSCollection(IEqualityComparer equalityComparer)
        {
            Comparer = equalityComparer ?? s_defaultComparer;
            Reset();
        }

        public HttpQSCollection(int capacity, IEqualityComparer equalityComparer) : this(equalityComparer)
        {
            Reset(capacity);
        }

        /// <summary>
        /// <para>Creates an empty <see cref='HttpQSCollection'/> instance with the specified
        ///    initial capacity and using the default case-insensitive hash code provider
        ///    and the default case-insensitive comparer.</para>
        /// </summary>
        public HttpQSCollection(int capacity)
        {
            Comparer = s_defaultComparer;
            Reset(capacity);
        }

        //
        // Private helpers
        //

        private void Reset()
        {
            _entriesArray = new ArrayList();
            _entriesTable = new Hashtable(Comparer);
            _nullKeyEntry = null;
            _version++;
        }

        private void Reset(int capacity)
        {
            _entriesArray = new ArrayList(capacity);
            _entriesTable = new Hashtable(capacity, Comparer);
            _nullKeyEntry = null;
            _version++;
        }

        private NameobjectEntry FindEntry(string key)
        {
            if (key != null)
                return (NameobjectEntry)_entriesTable[key];
            else
                return _nullKeyEntry;
        }

        internal IEqualityComparer Comparer { get; set; }

        /// <summary>
        /// <para>Gets or sets a value indicating whether the <see cref='HttpQSCollection'/> instance is read-only.</para>
        /// </summary>
        protected bool IsReadOnly { get; set; }

        /// <summary>
        /// <para>Gets a value indicating whether the <see cref='HttpQSCollection'/> instance contains entries whose
        ///    keys are not <see langword='null'/>.</para>
        /// </summary>
        protected bool BaseHasKeys() => _entriesTable.Count > 0;

        //
        // Methods to add / remove entries
        //

        /// <summary>
        ///    <para>Adds an entry with the specified key and value into the
        ///    <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected void BaseAdd(string name, object value)
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            var entry = new NameobjectEntry(name, value);

            // insert entry into hashtable
            if (name != null)
            {
                if (_entriesTable[name] == null)
                    _entriesTable.Add(name, entry);
            }
            else
            { // null key -- special case -- hashtable doesn't like null keys
                if (_nullKeyEntry == null)
                    _nullKeyEntry = entry;
            }

            // add entry to the list
            _entriesArray.Add(entry);

            _version++;
        }

        /// <summary>
        ///    <para>Removes the entries with the specified key from the
        ///    <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected void BaseRemove(string name)
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            if (name != null)
            {
                // remove from hashtable
                _entriesTable.Remove(name);

                // remove from array
                for (int i = _entriesArray.Count - 1; i >= 0; i--)
                {
                    if (Comparer.Equals(name, BaseGetKey(i)))
                        _entriesArray.RemoveAt(i);
                }
            }
            else
            { // null key -- special case
                // null out special 'null key' entry
                _nullKeyEntry = null;

                // remove from array
                for (int i = _entriesArray.Count - 1; i >= 0; i--)
                {
                    if (BaseGetKey(i) == null)
                        _entriesArray.RemoveAt(i);
                }
            }

            _version++;
        }

        /// <summary>
        ///    <para> Removes the entry at the specified index of the
        ///    <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected void BaseRemoveAt(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            string key = BaseGetKey(index);

            if (key != null)
            {
                // remove from hashtable
                _entriesTable.Remove(key);
            }
            else
            { // null key -- special case
                // null out special 'null key' entry
                _nullKeyEntry = null;
            }

            // remove from array
            _entriesArray.RemoveAt(index);

            _version++;
        }

        /// <summary>
        /// <para>Removes all entries from the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected void BaseClear()
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            Reset();
        }

        //
        // Access by name
        //

        /// <summary>
        ///    <para>Gets the value of the first entry with the specified key from
        ///       the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected object BaseGet(string name)
        {
            NameobjectEntry e = FindEntry(name);
            return e?.Value;
        }

        /// <summary>
        /// <para>Sets the value of the first entry with the specified key in the <see cref='HttpQSCollection'/>
        /// instance, if found; otherwise, adds an entry with the specified key and value
        /// into the <see cref='HttpQSCollection'/>
        /// instance.</para>
        /// </summary>
        protected void BaseSet(string name, object value)
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            NameobjectEntry entry = FindEntry(name);
            if (entry != null)
            {
                entry.Value = value;
                _version++;
            }
            else
            {
                BaseAdd(name, value);
            }
        }

        //
        // Access by index
        //

        /// <summary>
        ///    <para>Gets the value of the entry at the specified index of
        ///       the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected object BaseGet(int index)
        {
            NameobjectEntry entry = (NameobjectEntry)_entriesArray[index];
            return entry.Value;
        }

        /// <summary>
        ///    <para>Gets the key of the entry at the specified index of the
        ///    <see cref='HttpQSCollection'/>
        ///    instance.</para>
        /// </summary>
        protected string BaseGetKey(int index)
        {
            NameobjectEntry entry = (NameobjectEntry)_entriesArray[index];
            return entry.Key;
        }

        /// <summary>
        ///    <para>Sets the value of the entry at the specified index of
        ///       the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected void BaseSet(int index, object value)
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            NameobjectEntry entry = (NameobjectEntry)_entriesArray[index];
            entry.Value = value;
            _version++;
        }

        //
        // ICollection implementation
        //

        /// <summary>
        /// <para>Returns an enumerator that can iterate through the <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public virtual IEnumerator GetEnumerator()
        {
            return new NameobjectKeysEnumerator(this);
        }

        /// <summary>
        /// <para>Gets the number of key-and-value pairs in the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        public virtual int Count
        {
            get
            {
                return _entriesArray.Count;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.Arg_MultiRank, nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (array.Length - index < _entriesArray.Count)
            {
                throw new ArgumentException(SR.Arg_InsufficientSpace);
            }

            for (IEnumerator e = GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        //
        //  Helper methods to get arrays of keys and values
        //

        /// <summary>
        /// <para>Returns a <see cref='string' qualify='true'/> array containing all the keys in the
        /// <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected string[] BaseGetAllKeys()
        {
            int n = _entriesArray.Count;
            string[] allKeys = new string[n];

            for (int i = 0; i < n; i++)
                allKeys[i] = BaseGetKey(i);

            return allKeys;
        }

        /// <summary>
        /// <para>Returns an <see cref='object' qualify='true'/> array containing all the values in the
        /// <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected object[] BaseGetAllValues()
        {
            int n = _entriesArray.Count;
            object[] allValues = new object[n];

            for (int i = 0; i < n; i++)
                allValues[i] = BaseGet(i);

            return allValues;
        }

        /// <summary>
        ///    <para>Returns an array of the specified type containing
        ///       all the values in the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        protected object[] BaseGetAllValues(Type type)
        {
            int n = _entriesArray.Count;
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            object[] allValues = (object[])Array.CreateInstance(type, n);

            for (int i = 0; i < n; i++)
            {
                allValues[i] = BaseGet(i);
            }

            return allValues;
        }

        //
        // Keys property
        //

        /// <summary>
        /// <para>Returns a <see cref='KeysCollection'/> instance containing
        ///    all the keys in the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        public virtual KeysCollection Keys => _keys ?? (_keys = new KeysCollection(this));

        /// <summary>
        /// Simple entry class to allow substitution of values and indexed access to keys
        /// </summary>
        internal class NameobjectEntry
        {
            internal NameobjectEntry(string name, object value)
            {
                Key = name;
                Value = value;
            }

            internal string Key;
            internal object Value;
        }

        /// <summary>
        /// Enumerator over keys of NameobjectCollection
        /// </summary>
        internal class NameobjectKeysEnumerator : IEnumerator
        {
            private int _pos;
            private readonly HttpQSCollection _coll;
            private readonly int _version;

            internal NameobjectKeysEnumerator(HttpQSCollection coll)
            {
                _coll = coll;
                _version = _coll._version;
                _pos = -1;
            }

            public bool MoveNext()
            {
                if (_version != _coll._version)
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);

                if (_pos < _coll.Count - 1)
                {
                    _pos++;
                    return true;
                }
                else
                {
                    _pos = _coll.Count;
                    return false;
                }
            }

            public void Reset()
            {
                if (_version != _coll._version)
                    throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
                _pos = -1;
            }

            public object Current
            {
                get
                {
                    if (_pos >= 0 && _pos < _coll.Count)
                    {
                        return _coll.BaseGetKey(_pos);
                    }
                    else
                    {
                        throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
                    }
                }
            }
        }

        //
        // Keys collection
        //

        /// <summary>
        /// <para>Represents a collection of the <see cref='string' qualify='true'/> keys of a collection.</para>
        /// </summary>
        public class KeysCollection : ICollection
        {
            private readonly HttpQSCollection _coll;

            internal KeysCollection(HttpQSCollection coll)
            {
                _coll = coll;
            }

            // Indexed access

            /// <summary>
            ///    <para> Gets the key at the specified index of the collection.</para>
            /// </summary>
            public virtual string Get(int index)
            {
                return _coll.BaseGetKey(index);
            }

            /// <summary>
            ///    <para>Represents the entry at the specified index of the collection.</para>
            /// </summary>
            public string this[int index]
            {
                get
                {
                    return Get(index);
                }
            }

            // ICollection implementation

            /// <summary>
            ///    <para>Returns an enumerator that can iterate through the
            ///    <see cref='KeysCollection'/>.</para>
            /// </summary>
            public IEnumerator GetEnumerator()
            {
                return new NameobjectKeysEnumerator(_coll);
            }

            /// <summary>
            /// <para>Gets the number of keys in the <see cref='KeysCollection'/>.</para>
            /// </summary>
            public int Count
            {
                get
                {
                    return _coll.Count;
                }
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                if (array.Rank != 1)
                {
                    throw new ArgumentException(SR.Arg_MultiRank, nameof(array));
                }

                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < _coll.Count)
                {
                    throw new ArgumentException(SR.Arg_InsufficientSpace);
                }

                for (IEnumerator e = GetEnumerator(); e.MoveNext();)
                    array.SetValue(e.Current, index++);
            }

            object ICollection.SyncRoot
            {
                get { return ((ICollection)_coll).SyncRoot; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }
        }

        /// <summary>
        /// <para> Resets the cached arrays of the collection to <see langword='null'/>.</para>
        /// </summary>
        protected void InvalidateCachedArrays()
        {
            _all = null;
            _allKeys = null;
        }

        private static string GetAsOnestring(ArrayList list)
        {
            int n = list?.Count ?? 0;

            if (n == 1)
            {
                return (string)list[0];
            }
            else if (n > 1)
            {
                var s = new StringBuilder((string)list[0]);

                for (int i = 1; i < n; i++)
                {
                    s.Append(',');
                    s.Append((string)list[i]);
                }

                return s.ToString();
            }
            else
            {
                return null;
            }
        }

        private static string[] GetAsstringArray(ArrayList list)
        {
            int n = list?.Count ?? 0;
            if (n == 0)
                return null;

            string[] array = new string[n];
            list.CopyTo(0, array, 0, n);
            return array;
        }

        //
        // Misc public APIs
        //

        /// <summary>
        /// <para>Copies the entries in the specified <see cref='HttpQSCollection'/> to the current <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public void Add(HttpQSCollection c)
        {
            if (c == null)
            {
                throw new ArgumentNullException(nameof(c));
            }

            InvalidateCachedArrays();

            int n = c.Count;

            for (int i = 0; i < n; i++)
            {
                string key = c.GetKey(i);
                string[] values = c.GetValues(i);

                if (values != null)
                {
                    for (int j = 0; j < values.Length; j++)
                        Add(key, values[j]);
                }
                else
                {
                    Add(key, null);
                }
            }
        }

        /// <summary>
        ///    <para>Invalidates the cached arrays and removes all entries
        ///       from the <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public virtual void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            InvalidateCachedArrays();
            BaseClear();
        }

        public void CopyTo(Array dest, int index)
        {
            if (dest == null)
            {
                throw new ArgumentNullException(nameof(dest));
            }

            if (dest.Rank != 1)
            {
                throw new ArgumentException(SR.Arg_MultiRank, nameof(dest));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, SR.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (dest.Length - index < Count)
            {
                throw new ArgumentException(SR.Arg_InsufficientSpace);
            }

            int n = Count;
            if (_all == null)
            {
                string[] all = new string[n];
                for (int i = 0; i < n; i++)
                {
                    all[i] = Get(i);
                    dest.SetValue(all[i], i + index);
                }
                _all = all; // wait until end of loop to set _all reference in case Get throws
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    dest.SetValue(_all[i], i + index);
                }
            }
        }

        /// <summary>
        /// <para>Gets a value indicating whether the <see cref='HttpQSCollection'/> contains entries whose keys are not <see langword='null'/>.</para>
        /// </summary>
        public bool HasKeys()
        {
            return InternalHasKeys();
        }

        /// <summary>
        /// <para>Allows derived classes to alter HasKeys().</para>
        /// </summary>
        internal virtual bool InternalHasKeys()
        {
            return BaseHasKeys();
        }

        //
        // Access by name
        //

        /// <summary>
        ///    <para>Adds an entry with the specified name and value into the
        ///    <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public virtual void Add(string name, string value)
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            InvalidateCachedArrays();

            ArrayList values = (ArrayList)BaseGet(name);

            if (values == null)
            {
                // new key - add new key with single value
                values = new ArrayList(1);
                if (value != null)
                    values.Add(value);
                BaseAdd(name, values);
            }
            else
            {
                // old key -- append value to the list of values
                if (value != null)
                    values.Add(value);
            }
        }

        /// <summary>
        /// <para> Gets the values associated with the specified key from the <see cref='HttpQSCollection'/> combined into one comma-separated list.</para>
        /// </summary>
        public virtual string Get(string name)
        {
            ArrayList values = (ArrayList)BaseGet(name);
            return GetAsOnestring(values);
        }

        /// <summary>
        /// <para>Gets the values associated with the specified key from the <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public virtual string[] GetValues(string name)
        {
            ArrayList values = (ArrayList)BaseGet(name);
            return GetAsstringArray(values);
        }

        /// <summary>
        /// <para>Adds a value to an entry in the <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public virtual void Set(string name, string value)
        {
            if (IsReadOnly)
                throw new NotSupportedException(SR.CollectionReadOnly);

            InvalidateCachedArrays();

            var values = new ArrayList(1) { value };
            BaseSet(name, values);
        }

        /// <summary>
        /// <para>Removes the entries with the specified key from the <see cref='HttpQSCollection'/> instance.</para>
        /// </summary>
        public virtual void Remove(string name)
        {
            InvalidateCachedArrays();
            BaseRemove(name);
        }

        /// <summary>
        ///    <para> Represents the entry with the specified key in the
        ///    <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public string this[string name]
        {
            get
            {
                return Get(name);
            }

            set
            {
                Set(name, value);
            }
        }

        //
        // Indexed access
        //

        /// <summary>
        ///    <para>
        ///       Gets the values at the specified index of the <see cref='HttpQSCollection'/> combined into one
        ///       comma-separated list.</para>
        /// </summary>
        public virtual string Get(int index)
        {
            ArrayList values = (ArrayList)BaseGet(index);
            return GetAsOnestring(values);
        }

        /// <summary>
        ///    <para> Gets the values at the specified index of the <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public virtual string[] GetValues(int index)
        {
            ArrayList values = (ArrayList)BaseGet(index);
            return GetAsstringArray(values);
        }

        /// <summary>
        /// <para>Gets the key at the specified index of the <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public virtual string GetKey(int index) => BaseGetKey(index);

        /// <summary>
        /// <para>Represents the entry at the specified index of the <see cref='HttpQSCollection'/>.</para>
        /// </summary>
        public string this[int index] => Get(index);

        //
        // Access to keys and values as arrays
        //

        /// <summary>
        /// <para>Gets all the keys in the <see cref='HttpQSCollection'/>. </para>
        /// </summary>
        public virtual string[] AllKeys => _allKeys ?? (_allKeys = BaseGetAllKeys());

        public override string ToString()
        {
            int count = Count;
            if (count == 0)
            {
                return "";
            }

            var sb = new StringBuilder();
            string[] keys = AllKeys;
            for (int i = 0; i < count; i++)
            {
                sb.AppendFormat("{0}={1}&", keys[i], HtmlHelper.UrlEncode(this[keys[i]]));
            }

            return sb.ToString(0, sb.Length - 1);
        }

        // TODO: I18n/Resources class
        private static class SR
        {
            public const string CollectionReadOnly = "Collection is read-only.";
            public const string Arg_MultiRank = "Multi dimension array is not supported on this operation.";
            public const string ArgumentOutOfRange_NeedNonNegNum = "Index is less than zero.";
            public const string Arg_InsufficientSpace = "Insufficient space in the target location to copy the information.";
            public const string InvalidOperation_EnumOpCantHappen = "Enumeration has either not started or has already finished.";
            public const string InvalidOperation_EnumFailedVersion = "Collection was modified after the enumerator was instantiated.";
        }
    }
}
