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
// Novell.Directory.Ldap.LdapAttribute.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The name and values of one attribute of a directory entry.
    ///     LdapAttribute objects are used when searching for, adding,
    ///     modifying, and deleting attributes from the directory.
    ///     LdapAttributes are often used in conjunction with an
    ///     {@link LdapAttributeSet} when retrieving or adding multiple
    ///     attributes to an entry.
    /// </summary>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    /// <seealso cref="LdapModification">
    /// </seealso>
    public class LdapAttribute : IComparable
    {
        private readonly string _baseName; // cn of cn;lang-ja;phonetic

        private readonly string[] _subTypes; // lang-ja of cn;lang-ja
        private object[] _values; // Array of byte[] attribute values

        /// <summary>
        ///     Constructs an attribute with copies of all values of the input
        ///     attribute.
        /// </summary>
        /// <param name="attr">
        ///     An LdapAttribute to use as a template.
        ///     @throws IllegalArgumentException if attr is null.
        /// </param>
        public LdapAttribute(LdapAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentException("LdapAttribute class cannot be null");
            }

            // Do a deep copy of the LdapAttribute template
            Name = attr.Name;
            _baseName = attr._baseName;
            if (attr._subTypes != null)
            {
                _subTypes = new string[attr._subTypes.Length];
                Array.Copy(attr._subTypes, 0, _subTypes, 0, _subTypes.Length);
            }

            // OK to just copy attributes, as the app only sees a deep copy of them
            if (attr._values != null)
            {
                _values = new object[attr._values.Length];
                Array.Copy(attr._values, 0, _values, 0, _values.Length);
            }
        }

        /// <summary>
        ///     Constructs an attribute with no values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null.
        /// </param>
        public LdapAttribute(string attrName)
        {
            if (attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }

            Name = attrName;
            _baseName = GetBaseName(attrName);
            _subTypes = GetSubtypes(attrName);
        }

        /// <summary>
        ///     Constructs an attribute with a byte-formatted value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrName or attrBytes is null.
        /// </param>
        public LdapAttribute(string attrName, byte[] attrBytes)
            : this(attrName)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            // Make our own copy of the byte array to prevent app from changing it
            var tmp = new byte[attrBytes.Length];
            Array.Copy(attrBytes, 0, tmp, 0, attrBytes.Length);
            Add(tmp);
        }

        /// <summary>
        ///     Constructs an attribute with a single string value.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     @throws IllegalArgumentException if attrName or attrString is null.
        /// </param>
        public LdapAttribute(string attrName, string attrString)
            : this(attrName)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            try
            {
                var ibytes = attrString.ToUtf8Bytes();
                Add(ibytes);
            }
            catch (IOException e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        ///     Constructs an attribute with an array of string values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        /// </param>
        /// <param name="attrStrings">
        ///     Array of values as strings.
        ///     @throws IllegalArgumentException if attrName, attrStrings, or a member
        ///     of attrStrings is null.
        /// </param>
        public LdapAttribute(string attrName, string[] attrStrings)
            : this(attrName)
        {
            if (attrStrings == null)
            {
                throw new ArgumentException("Attribute values array cannot be null");
            }

            for (int i = 0, u = attrStrings.Length; i < u; i++)
            {
                try
                {
                    if (attrStrings[i] == null)
                    {
                        throw new ArgumentException("Attribute value " + "at array index " + i + " cannot be null");
                    }

                    var ibytes = attrStrings[i].ToUtf8Bytes();
                    Add(ibytes);
                }
                catch (IOException e)
                {
                    throw new Exception(e.ToString());
                }
            }
        }

        /// <summary>
        ///     Returns an enumerator for the values of the attribute in byte format.
        /// </summary>
        /// <returns>
        ///     The values of the attribute in byte format.
        ///     Note: All string values will be UTF-8 encoded. To decode use the
        ///     String constructor. Example: new String( byteArray, "UTF-8" );.
        /// </returns>
        public IEnumerator<byte[]> ByteValues => new ArrayEnumeration<byte[]>(ByteValueArray);

        /// <summary>
        ///     Returns an enumerator for the string values of an attribute.
        /// </summary>
        /// <returns>
        ///     The string values of an attribute.
        /// </returns>
        public IEnumerator<string> StringValues => new ArrayEnumeration<string>(StringValueArray);

        /// <summary>
        ///     Returns the values of the attribute as an array of bytes.
        /// </summary>
        /// <returns>
        ///     The values as an array of bytes or an empty array if there are
        ///     no values.
        /// </returns>
        public byte[][] ByteValueArray
        {
            get
            {
                if (_values == null)
                {
                    return new byte[0][];
                }

                var size = _values.Length;
                var bva = new byte[size][];

                // Deep copy so application cannot change values
                for (int i = 0, u = size; i < u; i++)
                {
                    bva[i] = new byte[((byte[])_values[i]).Length];
                    Array.Copy((Array)_values[i], 0, bva[i], 0, bva[i].Length);
                }

                return bva;
            }
        }

        /// <summary>
        ///     Returns the values of the attribute as an array of strings.
        /// </summary>
        /// <returns>
        ///     The values as an array of strings or an empty array if there are
        ///     no values.
        /// </returns>
        public string[] StringValueArray
        {
            get
            {
                if (_values == null)
                {
                    return new string[0];
                }

                var size = _values.Length;
                var sva = new string[size];
                for (var j = 0; j < size; j++)
                {
                    var valueBytes = (byte[])_values[j];
                    sva[j] = valueBytes.ToUtf8String();
                }

                return sva;
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a. <code>String</code>.
        /// </summary>
        /// <returns>
        ///     The UTF-8 encoded.<code>String</code> value of the attribute's
        ///     value.  If the value wasn't a UTF-8 encoded. <code>String</code>
        ///     to begin with the value of the returned. <code>String</code> is
        ///     non deterministic.
        ///     If. <code>this</code> attribute has more than one value the
        ///     first value is converted to a UTF-8 encoded. <code>String</code>
        ///     and returned. It should be noted, that the directory may
        ///     return attribute values in any order, so that the first
        ///     value may vary from one call to another.
        ///     If the attribute has no values. <code>null</code> is returned.
        /// </returns>
        public string StringValue
        {
            get
            {
                string rval = null;
                if (_values != null)
                {
                    var valueBytes = (byte[])_values[0];
                    rval = valueBytes.ToUtf8String();
                }
                return rval;
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a byte array.
        /// </summary>
        /// <returns>
        ///     The binary value of. <code>this</code> attribute or.
        ///     <code>null</code> if. <code>this</code> attribute doesn't have a value.
        ///     If the attribute has no values. <code>null</code> is returned.
        /// </returns>
        public byte[] ByteValue
        {
            get
            {
                byte[] bva = null;
                if (_values != null)
                {
                    // Deep copy so app can't change the value
                    bva = new byte[((byte[])_values[0]).Length];
                    Array.Copy((Array)_values[0], 0, bva, 0, bva.Length);
                }

                return bva;
            }
        }

        /// <summary>
        ///     Returns the language subtype of the attribute, if any.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns the string, lang-ja.
        /// </summary>
        /// <returns>
        ///     The language subtype of the attribute or null if the attribute
        ///     has none.
        /// </returns>
        public string LangSubtype
        {
            get
            {
                if (_subTypes != null)
                {
                    for (var i = 0; i < _subTypes.Length; i++)
                    {
                        if (_subTypes[i].StartsWith("lang-"))
                        {
                            return _subTypes[i];
                        }
                    }
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns the name of the attribute.
        /// </summary>
        /// <returns>
        ///     The name of the attribute.
        /// </returns>
        public string Name { get; }

        /// <summary>
        ///     Replaces all values with the specified value. This protected method is
        ///     used by sub-classes of LdapSchemaElement because the value cannot be set
        ///     with a contructor.
        /// </summary>
        protected string Value
        {
            set
            {
                _values = null;
                try
                {
                    Add(value.ToUtf8Bytes());
                }
                catch (IOException ue)
                {
                    throw new Exception(ue.ToString());
                }
            }
        }

        /// <summary>
        ///     Compares this object with the specified object for order.
        ///     Ordering is determined by comparing attribute names (see
        ///     {@link #getName() }) using the method compareTo() of the String class.
        /// </summary>
        /// <param name="attribute">
        ///     The LdapAttribute to be compared to this object.
        /// </param>
        /// <returns>
        ///     Returns a negative integer, zero, or a positive
        ///     integer as this object is less than, equal to, or greater than the
        ///     specified object.
        /// </returns>
        public virtual int CompareTo(object attribute)
        {
            return Name.CompareTo(((LdapAttribute)attribute).Name);
        }

        /// <summary>
        ///     Returns a clone of this LdapAttribute.
        /// </summary>
        /// <returns>
        ///     clone of this LdapAttribute.
        /// </returns>
        public LdapAttribute Clone()
        {
            try
            {
                var newObj = (LdapAttribute)MemberwiseClone();
                if (_values != null)
                {
                    Array.Copy(_values, 0, newObj._values, 0, _values.Length);
                }

                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        ///     Adds a string value to the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null.
        /// </param>
        public virtual void AddValue(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(attrString.ToUtf8Bytes());
        }

        /// <summary>
        ///     Adds a byte-formatted value to the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrBytes is null.
        /// </param>
        public virtual void AddValue(byte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(attrBytes);
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  String
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null.
        /// </param>
        public void AddBase64Value(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(Base64.Decode(attrString));
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrString">
        ///     The base64 value of the attribute as a StringBuffer.
        /// </param>
        /// <param name="start">
        ///     The start index of base64 encoded part, inclusive.
        /// </param>
        /// <param name="end">
        ///     The end index of base encoded part, exclusive.
        ///     @throws IllegalArgumentException if attrString is null.
        /// </param>
        public void AddBase64Value(StringBuilder attrString, int start, int end)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(Base64.Decode(attrString, start, end));
        }

        /// <summary>
        ///     Adds a base64 encoded value to the attribute.
        ///     The value will be decoded and stored as bytes.  Character
        ///     data encoded as a base64 value must be UTF-8 characters.
        /// </summary>
        /// <param name="attrChars">
        ///     The base64 value of the attribute as an array of
        ///     characters.
        ///     @throws IllegalArgumentException if attrString is null.
        /// </param>
        public void AddBase64Value(char[] attrChars)
        {
            if (attrChars == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            Add(Base64.Decode(attrChars));
        }


        /// <summary>
        ///     Returns the base name of the attribute.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <returns>
        ///     The base name of the attribute.
        /// </returns>
        public string GetBaseName()
        {
            return _baseName;
        }

        /// <summary>
        ///     Returns the base name of the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract the
        ///     base name.
        /// </param>
        /// <returns>
        ///     The base name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null.
        /// </returns>
        public static string GetBaseName(string attrName)
        {
            if (attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }

            var idx = attrName.IndexOf(';');
            if (idx == -1)
            {
                return attrName;
            }

            return attrName.Substring(0, idx - 0);
        }

        /// <summary>
        ///     Extracts the subtypes from the attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        /// </returns>
        public string[] GetSubtypes()
        {
            return _subTypes;
        }

        /// <summary>
        ///     Extracts the subtypes from the specified attribute name.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns an array containing lang-ja and phonetic.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute from which to extract
        ///     the subtypes.
        /// </param>
        /// <returns>
        ///     An array subtypes or null if the attribute has none.
        ///     @throws IllegalArgumentException if attrName is null.
        /// </returns>
        public static string[] GetSubtypes(string attrName)
        {
            if (attrName == null)
            {
                throw new ArgumentException("Attribute name cannot be null");
            }

            var st = new Tokenizer(attrName, ";");
            string[] subTypes = null;
            var cnt = st.Count;
            if (cnt > 0)
            {
                st.NextToken(); // skip over basename
                subTypes = new string[cnt - 1];
                var i = 0;
                while (st.HasMoreTokens())
                {
                    subTypes[i++] = st.NextToken();
                }
            }

            return subTypes;
        }

        /// <summary>
        ///     Reports if the attribute name contains the specified subtype.
        ///     For example, if you check for the subtype lang-en and the
        ///     attribute name is cn;lang-en, this method returns true.
        /// </summary>
        /// <param name="subtype">
        ///     The single subtype to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has the specified subtype;
        ///     false, if it doesn't.
        ///     @throws IllegalArgumentException if subtype is null.
        /// </returns>
        public bool HasSubtype(string subtype)
        {
            if (subtype == null)
            {
                throw new ArgumentException("subtype cannot be null");
            }

            if (_subTypes != null)
            {
                for (var i = 0; i < _subTypes.Length; i++)
                {
                    if (_subTypes[i].EqualsOrdinalCI(subtype))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Reports if the attribute name contains all the specified subtypes.
        ///     For example, if you check for the subtypes lang-en and phonetic
        ///     and if the attribute name is cn;lang-en;phonetic, this method
        ///     returns true. If the attribute name is cn;phonetic or cn;lang-en,
        ///     this method returns false.
        /// </summary>
        /// <param name="subtypes">
        ///     An array of subtypes to check for.
        /// </param>
        /// <returns>
        ///     True, if the attribute has all the specified subtypes;
        ///     false, if it doesn't have all the subtypes.
        ///     @throws IllegalArgumentException if subtypes is null or if array member
        ///     is null.
        /// </returns>
        public bool HasSubtypes(string[] subtypes)
        {
            if (subtypes == null)
            {
                throw new ArgumentException("subtypes cannot be null");
            }

            for (var i = 0; i < subtypes.Length; i++)
            {
                for (var j = 0; j < _subTypes.Length; j++)
                {
                    if (_subTypes[j] == null)
                    {
                        throw new ArgumentException("subtype " + "at array index " + i + " cannot be null");
                    }

                    if (_subTypes[j].EqualsOrdinalCI(subtypes[i]))
                    {
                        goto gotSubType;
                    }
                }

                return false;
                gotSubType: ;
            }

            return true;
        }

        /// <summary>
        ///     Removes a string value from the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a string.
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrString is null.
        /// </param>
        public virtual void RemoveValue(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            try
            {
                RemoveValue(attrString.ToUtf8Bytes());
            }
            catch (IOException uee)
            {
                // This should NEVER happen but just in case ...
                throw new Exception(uee.ToString());
            }
        }

        /// <summary>
        ///     Removes a byte-formatted value from the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     Example: <code>String.getBytes("UTF-8");</code>
        ///     Note: Removing a value which is not present in the attribute has
        ///     no effect.
        ///     @throws IllegalArgumentException if attrBytes is null.
        /// </param>
        public virtual void RemoveValue(byte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentException("Attribute value cannot be null");
            }

            for (var i = 0; i < _values.Length; i++)
            {
                if (Equals(attrBytes, (byte[])_values[i]))
                {
                    if (i == 0 && _values.Length == 1)
                    {
                        // Optimize if first element of a single valued attr
                        _values = null;
                        return;
                    }

                    if (_values.Length == 1)
                    {
                        _values = null;
                    }
                    else
                    {
                        var moved = _values.Length - i - 1;
                        var tmp = new object[_values.Length - 1];
                        if (i != 0)
                        {
                            Array.Copy(_values, 0, tmp, 0, i);
                        }

                        if (moved != 0)
                        {
                            Array.Copy(_values, i + 1, tmp, i, moved);
                        }

                        _values = tmp;
                        tmp = null;
                    }

                    break;
                }
            }
        }

        /// <summary>
        ///     Returns the number of values in the attribute.
        /// </summary>
        /// <returns>
        ///     The number of values in the attribute.
        /// </returns>
        public int Size()
        {
            return _values == null ? 0 : _values.Length;
        }

        /// <summary>
        ///     Adds an object to. <code>this</code> object's list of attribute values.
        /// </summary>
        /// <param name="bytes">
        ///     Ultimately all of this attribute's values are treated
        ///     as binary data so we simplify the process by requiring
        ///     that all data added to our list is in binary form.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        /// </param>
        private void Add(byte[] bytes)
        {
            if (_values == null)
            {
                _values = new object[] {bytes };
            }
            else
            {
                // Duplicate attribute values not allowed
                for (var i = 0; i < _values.Length; i++)
                {
                    if (Equals(bytes, (byte[])_values[i]))
                    {
                        return; // Duplicate, don't add
                    }
                }

                var tmp = new object[_values.Length + 1];
                Array.Copy(_values, 0, tmp, 0, _values.Length);
                tmp[_values.Length] = bytes;
                _values = tmp;
                tmp = null;
            }
        }

        /// <summary>
        ///     Returns true if the two specified arrays of bytes are equal to each
        ///     another.  Matches the logic of Arrays.equals which is not available
        ///     in jdk 1.1.x.
        /// </summary>
        /// <param name="e1">
        ///     the first array to be tested.
        /// </param>
        /// <param name="e2">
        ///     the second array to be tested.
        /// </param>
        /// <returns>
        ///     true if the two arrays are equal.
        /// </returns>
        private bool Equals(byte[] e1, byte[] e2)
        {
            // If same object, they compare true
            if (e1 == e2)
            {
                return true;
            }

            // If either but not both are null, they compare false
            if (e1 == null || e2 == null)
            {
                return false;
            }

            // If arrays have different length, they compare false
            var length = e1.Length;
            if (e2.Length != length)
            {
                return false;
            }

            // If any of the bytes are different, they compare false
            for (var i = 0; i < length; i++)
            {
                if (e1[i] != e2[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns a string representation of this LdapAttribute.
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttribute.
        /// </returns>
        public override string ToString()
        {
            var result = new StringBuilder("LdapAttribute: ");

            result.Append("{type='" + Name + "'");
            if (_values != null)
            {
                result.Append(", ");
                if (_values.Length == 1)
                {
                    result.Append("value='");
                }
                else
                {
                    result.Append("values='");
                }

                for (var i = 0; i < _values.Length; i++)
                {
                    if (i != 0)
                    {
                        result.Append("','");
                    }

                    var valueBytes = (byte[])_values[i];

                    if (valueBytes.Length == 0)
                    {
                        continue;
                    }

                    var sval = valueBytes.ToUtf8String();
                    if (sval.Length == 0)
                    {
                        // didn't decode well, must be binary
                        result.Append("<binary value, length:" + sval.Length);
                        continue;
                    }

                    result.Append(sval);
                }

                result.Append("'");
            }

            result.Append("}");

            return result.ToString();
        }

        private class UrlData
        {
            private readonly byte[] _data;

            private readonly int _length;

            public UrlData(LdapAttribute enclosingInstance, byte[] data, int length)
            {
                EnclosingInstance = enclosingInstance;
                _length = length;
                _data = data;
            }

            public LdapAttribute EnclosingInstance { get; }

            public int GetLength()
            {
                return _length;
            }

            public byte[] GetData()
            {
                return _data;
            }
        }
    }
}