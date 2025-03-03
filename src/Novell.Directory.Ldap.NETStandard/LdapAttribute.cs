﻿/******************************************************************************
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
using System.IO;
using System.Linq;
using System.Text;

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
    public class LdapAttribute : IComparable<LdapAttribute>
    {
        private readonly string _baseName; // cn of cn;lang-ja;phonetic

        private readonly string[] _subTypes; // lang-ja of cn;lang-ja
        private byte[][] _values; // Array of byte[] attribute values

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
                _subTypes = (string[])attr._subTypes.Clone();
            }

            // OK to just copy attributes, as the app only sees a deep copy of them
            if (attr._values != null)
            {
                _values = (byte[][])attr._values.Clone();
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
            Name = attrName ?? throw new ArgumentException("Attribute name cannot be null");
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
            var tmp = (byte[])attrBytes.Clone();
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
        ///     Returns an enumerable for the values of the attribute in byte format.
        /// </summary>
        /// <returns>
        ///     The values of the attribute in byte format.
        ///     Note: All string values will be UTF-8 encoded. To decode use the
        ///     String constructor. Example: new String( byteArray, "UTF-8" );.
        /// </returns>
        public ByteArrayView ByteValues => new ByteArrayView(_values);

        /// <summary>
        ///     Returns an enumerable for the string values of an attribute.
        /// </summary>
        /// <returns>
        ///     The string values of an attribute.
        /// </returns>
        public ByteArrayAsUtf8StringView StringValues => new ByteArrayAsUtf8StringView(_values);

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
                    return Array.Empty<byte[]>();
                }

                var size = _values.Length;
                var bva = new byte[size][];

                // Deep copy so application cannot change values
                for (int i = 0; i < size; i++)
                {
                    bva[i] = (byte[])_values[i].Clone();
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
                    return Array.Empty<string>();
                }

                var size = _values.Length;
                var sva = new string[size];
                for (var j = 0; j < size; j++)
                {
                    var valueBytes = _values[j];
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
        public string StringValue => _values?[0].ToUtf8String();

        /// <summary>
        ///     Returns the the first value of the attribute as a byte array.
        /// </summary>
        /// <returns>
        ///     The binary value of. <code>this</code> attribute or.
        ///     <code>null</code> if. <code>this</code> attribute doesn't have a value.
        ///     If the attribute has no values. <code>null</code> is returned.
        /// </returns>
        public byte[] ByteValue => (byte[])_values?[0].Clone();

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
                    foreach (var subType in _subTypes)
                    {
                        if (subType.StartsWith("lang-"))
                        {
                            return subType;
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
        public virtual int CompareTo(LdapAttribute attribute)
        {
            return Name.CompareTo(attribute.Name);
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
                    newObj._values = (byte[][])_values.Clone();
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

            Add(Convert.FromBase64String(attrString));
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

            Add(Convert.FromBase64String(new string(attrChars)));
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
                foreach (var subType in _subTypes)
                {
                    if (subType.EqualsOrdinalCI(subtype))
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
                foreach (var subType in _subTypes)
                {
                    if (subtypes[i] == null)
                    {
                        throw new ArgumentException("subtype at array index " + i + " cannot be null");
                    }

                    if (subType.EqualsOrdinalCI(subtypes[i]))
                    {
                        // We need to check the next entry of subtypes
                        goto gotSubType;
                    }
                }

                return false;
gotSubType:;
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

            RemoveValue(attrString.ToUtf8Bytes());
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
                if (attrBytes.SequenceEqual(_values[i]))
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
                        var tmp = new byte[_values.Length - 1][];
                        if (i != 0)
                        {
                            Array.Copy(_values, 0, tmp, 0, i);
                        }

                        if (moved != 0)
                        {
                            Array.Copy(_values, i + 1, tmp, i, moved);
                        }

                        _values = tmp;
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
            return _values?.Length ?? 0;
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
                _values = new byte[][] { bytes };
            }
            else
            {
                // Duplicate attribute values not allowed
                for (var i = 0; i < _values.Length; i++)
                {
                    if (bytes.SequenceEqual(_values[i]))
                    {
                        return; // Duplicate, don't add
                    }
                }

                Array.Resize(ref _values, _values.Length + 1);
                _values[_values.Length - 1] = bytes;
            }
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

            result.Append("{type='").Append(Name).Append('\'');
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

                    var valueBytes = _values[i];

                    if (valueBytes.Length == 0)
                    {
                        continue;
                    }

                    var sval = valueBytes.ToUtf8String();
                    if (sval.Length == 0)
                    {
                        // didn't decode well, must be binary
                        result.Append("<binary value, length:").Append(sval.Length).Append('>');
                        continue;
                    }

                    result.Append(sval);
                }

                result.Append('\'');
            }

            result.Append('}');
            return result.ToString();
        }
    }
}
