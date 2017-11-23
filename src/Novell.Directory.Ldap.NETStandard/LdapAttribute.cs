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
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using Novell.Directory.Ldap.Utilclass;
using System.Collections.Generic;
using System.Linq;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The name and values of one attribute of a directory entry.
    ///     LdapAttribute objects are used when searching for, Adding,
    ///     modifying, and deleting attributes from the directory.
    ///     LdapAttributes are often used in conjunction with an
    ///     {@link LdapAttributeSet} when retrieving or Adding multiple
    ///     attributes to an entry.
    /// </summary>
    /// <seealso cref="LdapEntry">
    /// </seealso>
    /// <seealso cref="LdapAttributeSet">
    /// </seealso>
    /// <seealso cref="LdapModification">
    /// </seealso>
    public class LdapAttribute : IComparable, ICloneable
    {
        private class URLData
        {
            private void InitBlock(LdapAttribute enclosingInstance) => EnclosingInstance = enclosingInstance;

            public LdapAttribute EnclosingInstance { get; private set; }

            public URLData(LdapAttribute enclosingInstance, byte[] data, int length)
            {
                InitBlock(enclosingInstance);
                Length = length;
                Data = data;
            }

            public int Length { get; }

            public byte[] Data { get; }
        }

        /// <summary>
        ///     Returns an enumerator for the values of the attribute in byte format.
        /// </summary>
        /// <returns>
        ///     The values of the attribute in byte format.
        ///     Note: All string values will be UTF-8 encoded. To decode use the
        ///     String constructor. Example: new String( byteArray, "UTF-8" );
        /// </returns>
        public virtual byte[][] ByteValues => ByteValueArray;

        /// <summary>
        ///     Returns an enumerator for the string values of an attribute.
        /// </summary>
        /// <returns>
        ///     The string values of an attribute.
        /// </returns>
        public virtual string[] StringValues => StringValueArray;

        /// <summary>
        ///     Returns the values of the attribute as an array of bytes.
        /// </summary>
        /// <returns>
        ///     The values as an array of bytes or an empty array if there are
        ///     no values.
        /// </returns>
        public virtual byte[][] ByteValueArray
        {
            get
            {
                if (values == null)
                    return new byte[0][];
                var size = values.Count;
                var bva = new byte[size][];
                // Deep copy so application cannot change values
                for (int i = 0, u = size; i < u; i++)
                {
                    bva[i] = new byte[values[i].Length];
                    Array.Copy(values[i], 0, bva[i], 0, bva[i].Length);
                }
                return bva;
            }
        }

        /// <summary>
        ///     Returns the values of the attribute as an array of strings.
        /// </summary>
        /// <returns>
        ///     The values as an array of strings or an empty array if there are
        ///     no values
        /// </returns>
        public virtual string[] StringValueArray
        {
            get
            {
                if (values == null)
                    return new string[0];
                var size = values.Count;
                var sva = new List<string>(size);
                foreach(byte[] value in values)
                    sva.Add(Encoding.UTF8.GetString(value));
                return sva.ToArray();
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a <code>String</code>.
        /// </summary>
        /// <returns>
        ///     The UTF-8 encoded<code>String</code> value of the attribute's
        ///     value.  If the value wasn't a UTF-8 encoded <code>String</code>
        ///     to begin with the value of the returned <code>String</code> is
        ///     non deterministic.
        ///     If <code>this</code> attribute has more than one value the
        ///     first value is converted to a UTF-8 encoded <code>String</code>
        ///     and returned. It should be noted, that the directory may
        ///     return attribute values in any order, so that the first
        ///     value may vary from one call to another.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public virtual string StringValue
        {
            get
            {
                if (values != null)
                    return Encoding.UTF8.GetString((byte[])values[0]);
                return null;
            }
        }

        /// <summary>
        ///     Returns the the first value of the attribute as a byte array.
        /// </summary>
        /// <returns>
        ///     The binary value of <code>this</code> attribute or
        ///     <code>null</code> if <code>this</code> attribute doesn't have a value.
        ///     If the attribute has no values <code>null</code> is returned
        /// </returns>
        public virtual byte[] ByteValue
        {
            get
            {
                byte[] bva = null;
                if (values != null)
                {
                    // Deep copy so app can't change the value
                    bva = new byte[values.FirstOrDefault().Length];
                    Array.Copy(values.FirstOrDefault(), 0, bva, 0, bva.Length);
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
        public virtual string LangSubtype
        {
            get
            {
                if (subTypes != null)
                {
                    return subTypes.FirstOrDefault(x => x.StartsWith("lang-"));
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
        public virtual string Name { get; }

        /// <summary>
        ///     Replaces all values with the specified value. This protected method is
        ///     used by sub-classes of LdapSchemaElement because the value cannot be set
        ///     with a contructor.
        /// </summary>
        protected internal virtual string Value
        {
            set
            {
                values = null;
                Add(Encoding.UTF8.GetBytes(value));
            }
        }

        private readonly IList<string> subTypes; // lang-ja of cn;lang-ja
        private IList<byte[]> values; // Array of byte[] attribute values

        /// <summary>
        ///     Constructs an attribute with copies of all values of the input
        ///     attribute.
        /// </summary>
        /// <param name="attr">
        ///     An LdapAttribute to use as a template.
        ///     @throws IllegalArgumentException if attr is null
        /// </param>
        public LdapAttribute(LdapAttribute attr)
        {
            if (attr == null)
            {
                throw new ArgumentNullException(nameof(attr));
            }
            // Do a deep copy of the LdapAttribute template
            Name = attr.Name;
            BaseName = attr.BaseName;
            if (attr.subTypes != null)
            {
                subTypes = new List<string>(attr.subTypes);
            }
            // OK to just copy attributes, as the app only sees a deep copy of them
            if (attr.values != null)
            {
                values = new List<byte[]>(attr.values);
            }
        }

        /// <summary>
        ///     Constructs an attribute with no values.
        /// </summary>
        /// <param name="attrName">
        ///     Name of the attribute.
        ///     @throws IllegalArgumentException if attrName is null
        /// </param>
        public LdapAttribute(string attrName)
        {
            Name = attrName ?? throw new ArgumentNullException(nameof(attrName));
            BaseName = GetBaseName(attrName);
            subTypes = GetSubtypes(attrName);
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
        ///     @throws IllegalArgumentException if attrName or attrBytes is null
        /// </param>
        public LdapAttribute(string attrName, byte[] attrBytes) : this(attrName)
        {
            if (attrBytes == null)
            {
                throw new ArgumentNullException(nameof(attrBytes));
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
        ///     @throws IllegalArgumentException if attrName or attrString is null
        /// </param>
        public LdapAttribute(string attrName, string attrString) : this(attrName)
        {
            if (attrString == null)
            {
                throw new ArgumentNullException(nameof(attrString));
            }
            Add(Encoding.UTF8.GetBytes(attrString));
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
        ///     of attrStrings is null
        /// </param>
        public LdapAttribute(string attrName, string[] attrStrings) : this(attrName)
        {
            if (attrStrings == null)
            {
                throw new ArgumentNullException(nameof(attrStrings));
            }
            for (int i = 0, u = attrStrings.Length; i < u; i++)
            {
                if (attrStrings[i] == null)
                    throw new ArgumentNullException($"Attribute value at array index {i} cannot be null");
                Add(Encoding.UTF8.GetBytes(attrStrings[i]));
            }
        }

        /// <summary>
        ///     Returns a clone of this LdapAttribute.
        /// </summary>
        /// <returns>
        ///     clone of this LdapAttribute.
        /// </returns>
        public object Clone()
        {
            var newObj = MemberwiseClone() as LdapAttribute;
            if (values != null)
                newObj.values = new List<byte[]>(values);
            return newObj;
        }

        /// <summary>
        ///     Adds a string value to the attribute.
        /// </summary>
        /// <param name="attrString">
        ///     Value of the attribute as a String.
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void AddValue(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentNullException(nameof(attrString));
            }

            Add(Encoding.UTF8.GetBytes(attrString));
        }

        /// <summary>
        ///     Adds a byte-formatted value to the attribute.
        /// </summary>
        /// <param name="attrBytes">
        ///     Value of the attribute as raw bytes.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        public virtual void AddValue(byte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentNullException(nameof(attrBytes));
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
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void AddBase64Value(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentNullException(nameof(attrString));
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
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void AddBase64Value(StringBuilder attrString, int start, int end)
        {
            if (attrString == null)
            {
                throw new ArgumentNullException(nameof(attrString));
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
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void AddBase64Value(char[] attrChars)
        {
            if (attrChars == null)
            {
                throw new ArgumentNullException(nameof(attrChars));
            }

            Add(Base64.Decode(attrChars));
        }

        /// <summary>
        ///     Adds a URL, indicating a file or other resource that contains
        ///     the value of the attribute.
        /// </summary>
        /// <param name="url">
        ///     String value of a URL pointing to the resource containing
        ///     the value of the attribute.
        ///     @throws IllegalArgumentException if url is null
        /// </param>
        public virtual void AddURLValue(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }
            AddURLValue(new Uri(url));
        }

        /// <summary>
        ///     Adds a URL, indicating a file or other resource that contains
        ///     the value of the attribute.
        /// </summary>
        /// <param name="url">
        ///     A URL class pointing to the resource containing the value
        ///     of the attribute.
        ///     @throws IllegalArgumentException if url is null
        /// </param>
        public virtual void AddURLValue(Uri url)
        {
            // Class to encapsulate the data bytes and the length
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            // Get InputStream from the URL
            var webRequest = WebRequest.Create(url);
            var @in = webRequest.GetResponseAsync().ResultAndUnwrap().GetResponseStream();
            // Read the bytes into buffers and store the them in an arraylist
            var bufs = new ArrayList();
            var buf = new byte[4096];
            int len, totalLength = 0;
            while ((len = SupportClass.ReadInput(@in, buf, 0, 4096)) != -1)
            {
                bufs.Add(new URLData(this, buf, len));
                buf = new byte[4096];
                totalLength += len;
            }
            /*
            * Now that the length is known, allocate an array to hold all
            * the bytes of data and copy the data to that array, store
            * it in this LdapAttribute
            */
            var data = new byte[totalLength];
            var offset = 0; //
            for (var i = 0; i < bufs.Count; i++)
            {
                var b = (URLData)bufs[i];
                len = b.Length;
                Array.Copy(b.Data, 0, data, offset, len);
                offset += len;
            }
            Add(data);
        }

        /// <summary>
        ///     Returns the base name of the attribute.
        ///     For example, if the attribute name is cn;lang-ja;phonetic,
        ///     this method returns cn.
        /// </summary>
        /// <returns>
        ///     The base name of the attribute.
        /// </returns>
        public virtual string BaseName { get; }

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
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string GetBaseName(string attrName)
        {
            if (attrName == null)
            {
                throw new ArgumentNullException(nameof(attrName));
            }
            var idx = attrName.IndexOf(';');
            if (-1 == idx)
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
        public virtual IEnumerable<string> Subtypes => subTypes;

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
        ///     @throws IllegalArgumentException if attrName is null
        /// </returns>
        public static string[] GetSubtypes(string attrName)
        {
            if (attrName == null)
            {
                throw new ArgumentNullException(nameof(attrName));
            }
            var st = new SupportClass.Tokenizer(attrName, ";");
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
        ///     @throws IllegalArgumentException if subtype is null
        /// </returns>
        public virtual bool HasSubtype(string subtype)
        {
            if (subtype == null)
            {
                throw new ArgumentNullException(nameof(subtype));
            }
            if (subTypes != null)
            {
                return subTypes.Any(x => x.Equals(subtype, StringComparison.InvariantCultureIgnoreCase));
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
        public virtual bool HasSubtypes(string[] subtypes)
        {
            if (subtypes == null)
            {
                throw new ArgumentNullException(nameof(subtypes));
            }
            for (var i = 0; i < subtypes.Length; i++)
            {
                bool found = false;
                for (var j = 0; j < subTypes.Count; j++)
                {
                    if (subTypes[j] == null)
                        throw new ArgumentNullException($"subtype at array index {j} cannot be null");
                    if (subTypes[j].ToUpper().Equals(subtypes[i].ToUpper()))
                    {
                        found = false;
                        break;
                    }
                }

                if (!found)
                    return false;
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
        ///     @throws IllegalArgumentException if attrString is null
        /// </param>
        public virtual void RemoveValue(string attrString)
        {
            if (attrString == null)
            {
                throw new ArgumentNullException(nameof(attrString));
            }
            RemoveValue(Encoding.UTF8.GetBytes(attrString));
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
        ///     @throws IllegalArgumentException if attrBytes is null
        /// </param>
        public virtual void RemoveValue(byte[] attrBytes)
        {
            if (attrBytes == null)
            {
                throw new ArgumentNullException(nameof(attrBytes));
            }
            byte[] remover = values.FirstOrDefault(x => x.SequenceEqual(attrBytes));
            if (remover != default(byte[]))
                values.Remove(remover);
        }

        /// <summary>
        ///     Returns the number of values in the attribute.
        /// </summary>
        /// <returns>
        ///     The number of values in the attribute.
        /// </returns>
        public virtual int Size => values == null ? 0 : values.Count;

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
        ///     Adds an object to <code>this</code> object's list of attribute values
        /// </summary>
        /// <param name="bytes">
        ///     Ultimately all of this attribute's values are treated
        ///     as binary data so we simplify the process by requiring
        ///     that all data Added to our list is in binary form.
        ///     Note: If attrBytes represents a string it should be UTF-8 encoded.
        /// </param>
        private void Add(byte[] bytes)
        {
            if (values == null)
            {
                values = new List<byte[]> { bytes };
            }
            else
            {
                if (!values.Any(x => x.SequenceEqual(bytes)))
                    values.Add(bytes);
            }
        }

        /// <summary>
        ///     Returns a string representation of this LdapAttribute
        /// </summary>
        /// <returns>
        ///     a string representation of this LdapAttribute
        /// </returns>
        public override string ToString()
        {
            var result = new StringBuilder("LdapAttribute: ");

            result.Append("{type='" + Name + "'");
            if (values != null)
            {
                result.Append(", ");
                if (values.Count == 1)
                {
                    result.Append("value='");
                }
                else
                {
                    result.Append("values='");
                }

                int i = 0;
                foreach (var value in values)
                {
                    if (i != 0)
                    {
                        result.Append("','");
                    }

                    if (value.Length == 0)
                    {
                        continue;
                    }

                    string sval = Encoding.UTF8.GetString(value);
                    if (sval.Length == 0)
                    {
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
    }
}