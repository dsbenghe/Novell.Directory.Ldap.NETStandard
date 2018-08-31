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
// Novell.Directory.Ldap.SupportClass.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

// Support classes replicate the functionality of the original code, but in some cases they are
// substantially different architecturally. Although every effort is made to preserve the
// original architecture of the application in the converted project, the user should be aware that
// the primary goal of these support classes is to replicate functionality, and that at times
// the architecture of the resulting solution may differ somewhat.
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     This interface should be implemented by any class whose instances are intended
    ///     to be executed by a thread.
    /// </summary>
    public interface IThreadRunnable
    {
        /// <summary>
        ///     This method has to be implemented in order that starting of the thread causes the object's
        ///     run method to be called in that separately executing thread.
        /// </summary>
        void Run();
    }

    /// <summary>
    ///     Contains conversion support elements such as classes, interfaces and static methods.
    /// </summary>
    public partial class SupportClass
    {
        /// <summary>
        ///     Converts a string to an array of bytes.
        /// </summary>
        /// <param name="sourceString">The string to be converted.</param>
        /// <returns>The new array of bytes.</returns>
        public static byte[] ToByteArray(string sourceString)
        {
            var byteArray = new byte[sourceString.Length];
            for (var index = 0; index < sourceString.Length; index++)
            {
                byteArray[index] = (byte)sourceString[index];
            }

            return byteArray;
        }

        /// <summary>
        ///     Converts a array of object-type instances to a byte-type array.
        /// </summary>
        /// <param name="tempObjectArray">Array to convert.</param>
        /// <returns>An array of byte type elements.</returns>
        public static byte[] ToByteArray(object[] tempObjectArray)
        {
            var byteArray = new byte[tempObjectArray.Length];
            for (var index = 0; index < tempObjectArray.Length; index++)
            {
                byteArray[index] = (byte)tempObjectArray[index];
            }

            return byteArray;
        }

        /*******************************/

        /// <summary>
        ///     Reads a number of characters from the current source Stream and writes the data to the target array at the
        ///     specified index.
        /// </summary>
        /// <param name="sourceStream">The source Stream to read from.</param>
        /// <param name="target">Contains the array of characteres read from the source Stream.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source Stream.</param>
        /// <returns>
        ///     The number of characters read. The number will be less than or equal to count depending on the data available
        ///     in the source Stream. Returns -1 if the end of the stream is reached.
        /// </returns>
        public static int ReadInput(Stream sourceStream, ref byte[] target, int start, int count)
        {
            // Returns 0 bytes if not enough space in target
            if (target.Length == 0)
            {
                return 0;
            }

            var receiver = new byte[target.Length];
            var bytesRead = 0;
            var startIndex = start;
            var bytesToRead = count;
            while (bytesToRead > 0)
            {
                var n = sourceStream.Read(receiver, startIndex, bytesToRead);
                if (n == 0)
                {
                    break;
                }

                bytesRead += n;
                startIndex += n;
                bytesToRead -= n;
            }

            // Returns -1 if EOF
            if (bytesRead == 0)
            {
                return -1;
            }

            for (var i = start; i < start + bytesRead; i++)
            {
                target[i] = (byte)receiver[i];
            }

            return bytesRead;
        }

        /// <summary>
        ///     Reads a number of characters from the current source TextReader and writes the data to the target array at the
        ///     specified index.
        /// </summary>
        /// <param name="sourceTextReader">The source TextReader to read from.</param>
        /// <param name="target">Contains the array of characteres read from the source TextReader.</param>
        /// <param name="start">The starting index of the target array.</param>
        /// <param name="count">The maximum number of characters to read from the source TextReader.</param>
        /// <returns>
        ///     The number of characters read. The number will be less than or equal to count depending on the data available
        ///     in the source TextReader. Returns -1 if the end of the stream is reached.
        /// </returns>
        public static int ReadInput(TextReader sourceTextReader, ref byte[] target, int start, int count)
        {
            // Returns 0 bytes if not enough space in target
            if (target.Length == 0)
            {
                return 0;
            }

            var charArray = new char[target.Length];
            var bytesRead = sourceTextReader.Read(charArray, start, count);

            // Returns -1 if EOF
            if (bytesRead == 0)
            {
                return -1;
            }

            for (var index = start; index < start + bytesRead; index++)
            {
                target[index] = (byte)charArray[index];
            }

            return bytesRead;
        }

        /*******************************/

        /// <summary>
        ///     This method returns the literal value received.
        /// </summary>
        /// <param name="literal">The literal to return.</param>
        /// <returns>The received value.</returns>
        public static long Identity(long literal)
        {
            return literal;
        }

        /// <summary>
        ///     This method returns the literal value received.
        /// </summary>
        /// <param name="literal">The literal to return.</param>
        /// <returns>The received value.</returns>
        public static ulong Identity(ulong literal)
        {
            return literal;
        }

        /// <summary>
        ///     This method returns the literal value received.
        /// </summary>
        /// <param name="literal">The literal to return.</param>
        /// <returns>The received value.</returns>
        public static float Identity(float literal)
        {
            return literal;
        }

        /// <summary>
        ///     This method returns the literal value received.
        /// </summary>
        /// <param name="literal">The literal to return.</param>
        /// <returns>The received value.</returns>
        public static double Identity(double literal)
        {
            return literal;
        }

        /*******************************/

        /// <summary>
        ///     Gets the DateTimeFormat instance and date instance to obtain the date with the format passed.
        /// </summary>
        /// <param name="format">The DateTimeFormat to obtain the time and date pattern.</param>
        /// <param name="date">The date instance used to get the date.</param>
        /// <returns>A string representing the date with the time and date patterns.</returns>
        public static string FormatDateTime(DateTimeFormatInfo format, DateTime date)
        {
            var timePattern = DateTimeFormatManager.Manager.GetTimeFormatPattern(format);
            var datePattern = DateTimeFormatManager.Manager.GetDateFormatPattern(format);
            return date.ToString(datePattern + " " + timePattern, format);
        }

        /*******************************/

        /// <summary>
        ///     Removes the first occurrence of an specific object from an ArrayList instance.
        /// </summary>
        /// <param name="arrayList">The ArrayList instance.</param>
        /// <param name="element">The element to remove.</param>
        /// <returns>True if item is found in the ArrayList; otherwise, false.</returns>
        public static bool VectorRemoveElement(ArrayList arrayList, object element)
        {
            var containsItem = arrayList.Contains(element);
            arrayList.Remove(element);
            return containsItem;
        }

        /*******************************/

        /// <summary>
        ///     Adds an element to the top end of a Stack instance.
        /// </summary>
        /// <param name="stack">The Stack instance.</param>
        /// <param name="element">The element to add.</param>
        /// <returns>The element added.</returns>
        public static object StackPush(Stack stack, object element)
        {
            stack.Push(element);
            return element;
        }

        /*******************************/

        /// <summary>
        ///     Copies an array of chars obtained from a String into a specified array of chars.
        /// </summary>
        /// <param name="sourceString">The String to get the chars from.</param>
        /// <param name="sourceStart">Position of the String to start getting the chars.</param>
        /// <param name="sourceEnd">Position of the String to end getting the chars.</param>
        /// <param name="destinationArray">Array to return the chars.</param>
        /// <param name="destinationStart">Position of the destination array of chars to start storing the chars.</param>
        /// <returns>An array of chars.</returns>
        public static void GetCharsFromString(string sourceString, int sourceStart, int sourceEnd,
            ref char[] destinationArray, int destinationStart)
        {
            var sourceCounter = sourceStart;
            var destinationCounter = destinationStart;
            while (sourceCounter < sourceEnd)
            {
                destinationArray[destinationCounter] = sourceString[sourceCounter];
                sourceCounter++;
                destinationCounter++;
            }
        }
     
        /*******************************/

        /// <summary>
        ///     Determines whether two Collections instances are equals.
        /// </summary>
        /// <param name="source">The first Collections to compare. </param>
        /// <param name="target">The second Collections to compare. </param>
        /// <returns>Return true if the first collection is the same instance as the second collection, otherwise return false.</returns>
        public static bool EqualsSupport(ICollection source, ICollection target)
        {
            var sourceEnumerator = ReverseStack(source);
            var targetEnumerator = ReverseStack(target);

            if (source.Count != target.Count)
            {
                return false;
            }

            while (sourceEnumerator.MoveNext() && targetEnumerator.MoveNext())
            {
                if (!sourceEnumerator.Current.Equals(targetEnumerator.Current))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Determines if a Collection is equal to the Object.
        /// </summary>
        /// <param name="source">The first Collections to compare.</param>
        /// <param name="target">The Object to compare.</param>
        /// <returns>Return true if the first collection contains the same values of the second Object, otherwise return false.</returns>
        public static bool EqualsSupport(ICollection source, object target)
        {
            if (target.GetType() != typeof(ICollection))
            {
                return false;
            }

            return EqualsSupport(source, (ICollection)target);
        }

        /// <summary>
        ///     Determines if a IDictionaryEnumerator is equal to the Object.
        /// </summary>
        /// <param name="source">The first IDictionaryEnumerator to compare.</param>
        /// <param name="target">The second Object to compare.</param>
        /// <returns>
        ///     Return true if the first IDictionaryEnumerator contains the same values of the second Object, otherwise return
        ///     false.
        /// </returns>
        public static bool EqualsSupport(IDictionaryEnumerator source, object target)
        {
            if (target.GetType() != typeof(IDictionaryEnumerator))
            {
                return false;
            }

            return EqualsSupport(source, (IDictionaryEnumerator)target);
        }

        /// <summary>
        ///     Determines whether two IDictionaryEnumerator instances are equals.
        /// </summary>
        /// <param name="source">The first IDictionaryEnumerator to compare.</param>
        /// <param name="target">The second IDictionaryEnumerator to compare.</param>
        /// <returns>
        ///     Return true if the first IDictionaryEnumerator contains the same values as the second IDictionaryEnumerator,
        ///     otherwise return false.
        /// </returns>
        public static bool EqualsSupport(IDictionaryEnumerator source, IDictionaryEnumerator target)
        {
            while (source.MoveNext() && target.MoveNext())
            {
                if (source.Key.Equals(target.Key))
                {
                    if (source.Value.Equals(target.Value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Reverses the Stack Collection received.
        /// </summary>
        /// <param name="collection">The collection to reverse.</param>
        /// <returns>
        ///     The collection received in reverse order if it was a System.Collections.Stack type, otherwise it does
        ///     nothing to the collection.
        /// </returns>
        public static IEnumerator ReverseStack(ICollection collection)
        {
            if (collection.GetType() == typeof(Stack))
            {
                var collectionStack = new ArrayList(collection);
                collectionStack.Reverse();
                return collectionStack.GetEnumerator();
            }

            return collection.GetEnumerator();
        }

        /*******************************/

        /// <summary>
        ///     The class performs token processing from strings.
        /// </summary>
        public class Tokenizer
        {
            private readonly bool _returnDelims;

            // The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
            private string _delimiters = " \t\n\r";

            // Element list identified
            private ArrayList _elements;

            // Source string to use
            private string _source;

            /// <summary>
            ///     Initializes a new class instance with a specified string to process.
            /// </summary>
            /// <param name="source">String to tokenize.</param>
            public Tokenizer(string source)
            {
                _elements = new ArrayList();
                _elements.AddRange(source.Split(_delimiters.ToCharArray()));
                RemoveEmptyStrings();
                _source = source;
            }

            /// <summary>
            ///     Initializes a new class instance with a specified string to process
            ///     and the specified token delimiters to use.
            /// </summary>
            /// <param name="source">String to tokenize.</param>
            /// <param name="delimiters">String containing the delimiters.</param>
            public Tokenizer(string source, string delimiters)
            {
                _elements = new ArrayList();
                _delimiters = delimiters;
                _elements.AddRange(source.Split(_delimiters.ToCharArray()));
                RemoveEmptyStrings();
                _source = source;
            }

            public Tokenizer(string source, string delimiters, bool retDel)
            {
                _elements = new ArrayList();
                _delimiters = delimiters;
                _source = source;
                _returnDelims = retDel;
                if (_returnDelims)
                {
                    Tokenize();
                }
                else
                {
                    _elements.AddRange(source.Split(_delimiters.ToCharArray()));
                }

                RemoveEmptyStrings();
            }

            /// <summary>
            ///     Current token count for the source string.
            /// </summary>
            public int Count => _elements.Count;

            private void Tokenize()
            {
                var tempstr = _source;
                var toks = string.Empty;
                if (tempstr.IndexOfAny(_delimiters.ToCharArray()) < 0 && tempstr.Length > 0)
                {
                    _elements.Add(tempstr);
                }
                else if (tempstr.IndexOfAny(_delimiters.ToCharArray()) < 0 && tempstr.Length <= 0)
                {
                    return;
                }

                while (tempstr.IndexOfAny(_delimiters.ToCharArray()) >= 0)
                {
                    if (tempstr.IndexOfAny(_delimiters.ToCharArray()) == 0)
                    {
                        if (tempstr.Length > 1)
                        {
                            _elements.Add(tempstr.Substring(0, 1));
                            tempstr = tempstr.Substring(1);
                        }
                        else
                        {
                            tempstr = string.Empty;
                        }
                    }
                    else
                    {
                        toks = tempstr.Substring(0, tempstr.IndexOfAny(_delimiters.ToCharArray()));
                        _elements.Add(toks);
                        _elements.Add(tempstr.Substring(toks.Length, 1));
                        if (tempstr.Length > toks.Length + 1)
                        {
                            tempstr = tempstr.Substring(toks.Length + 1);
                        }
                        else
                        {
                            tempstr = string.Empty;
                        }
                    }
                }

                if (tempstr.Length > 0)
                {
                    _elements.Add(tempstr);
                }
            }

            /// <summary>
            ///     Determines if there are more tokens to return from the source string.
            /// </summary>
            /// <returns>True or false, depending if there are more tokens.</returns>
            public bool HasMoreTokens()
            {
                return _elements.Count > 0;
            }

            /// <summary>
            ///     Returns the next token from the token list.
            /// </summary>
            /// <returns>The string value of the token.</returns>
            public string NextToken()
            {
                string result;
                if (_source == string.Empty)
                {
                    throw new Exception();
                }

                if (_returnDelims)
                {
// Tokenize();
                    RemoveEmptyStrings();
                    result = (string)_elements[0];
                    _elements.RemoveAt(0);
                    return result;
                }

                _elements = new ArrayList();
                _elements.AddRange(_source.Split(_delimiters.ToCharArray()));
                RemoveEmptyStrings();
                result = (string)_elements[0];
                _elements.RemoveAt(0);
                _source = _source.Remove(_source.IndexOf(result), result.Length);
                _source = _source.TrimStart(_delimiters.ToCharArray());
                return result;
            }

            /// <summary>
            ///     Returns the next token from the source string, using the provided
            ///     token delimiters.
            /// </summary>
            /// <param name="delimiters">String containing the delimiters to use.</param>
            /// <returns>The string value of the token.</returns>
            public string NextToken(string delimiters)
            {
                _delimiters = delimiters;
                return NextToken();
            }

            /// <summary>
            ///     Removes all empty strings from the token list.
            /// </summary>
            private void RemoveEmptyStrings()
            {
                for (var index = 0; index < _elements.Count; index++)
                {
                    if ((string)_elements[index] == string.Empty)
                    {
                        _elements.RemoveAt(index);
                        index--;
                    }
                }
            }
        }

        /*******************************/

        /// <summary>
        ///     Provides support for DateFormat.
        /// </summary>
        public class DateTimeFormatManager
        {
            public static DateTimeFormatHashTable Manager = new DateTimeFormatHashTable();

            /// <summary>
            ///     Hashtable class to provide functionality for dateformat properties.
            /// </summary>
            public class DateTimeFormatHashTable : Hashtable
            {
                /// <summary>
                ///     Sets the format for datetime.
                /// </summary>
                /// <param name="format">DateTimeFormat instance to set the pattern.</param>
                /// <param name="newPattern">A string with the pattern format.</param>
                public void SetDateFormatPattern(DateTimeFormatInfo format, string newPattern)
                {
                    if (this[format] != null)
                    {
                        ((DateTimeFormatProperties)this[format]).DateFormatPattern = newPattern;
                    }
                    else
                    {
                        var tempProps = new DateTimeFormatProperties
                        {
                            DateFormatPattern = newPattern
                        };
                        Add(format, tempProps);
                    }
                }

                /// <summary>
                ///     Gets the current format pattern of the DateTimeFormat instance.
                /// </summary>
                /// <param name="format">The DateTimeFormat instance which the value will be obtained.</param>
                /// <returns>The string representing the current datetimeformat pattern.</returns>
                public string GetDateFormatPattern(DateTimeFormatInfo format)
                {
                    if (this[format] == null)
                    {
                        return "d-MMM-yy";
                    }

                    return ((DateTimeFormatProperties)this[format]).DateFormatPattern;
                }

                /// <summary>
                ///     Sets the datetimeformat pattern to the giving format.
                /// </summary>
                /// <param name="format">The datetimeformat instance to set.</param>
                /// <param name="newPattern">The new datetimeformat pattern.</param>
                public void SetTimeFormatPattern(DateTimeFormatInfo format, string newPattern)
                {
                    if (this[format] != null)
                    {
                        ((DateTimeFormatProperties)this[format]).TimeFormatPattern = newPattern;
                    }
                    else
                    {
                        var tempProps = new DateTimeFormatProperties
                        {
                            TimeFormatPattern = newPattern
                        };
                        Add(format, tempProps);
                    }
                }

                /// <summary>
                ///     Gets the current format pattern of the DateTimeFormat instance.
                /// </summary>
                /// <param name="format">The DateTimeFormat instance which the value will be obtained.</param>
                /// <returns>The string representing the current datetimeformat pattern.</returns>
                public string GetTimeFormatPattern(DateTimeFormatInfo format)
                {
                    if (this[format] == null)
                    {
                        return "h:mm:ss tt";
                    }

                    return ((DateTimeFormatProperties)this[format]).TimeFormatPattern;
                }

                /// <summary>
                ///     Internal class to provides the DateFormat and TimeFormat pattern properties on .NET.
                /// </summary>
                private class DateTimeFormatProperties
                {
                    public string DateFormatPattern = "d-MMM-yy";
                    public string TimeFormatPattern = "h:mm:ss tt";
                }
            }
        }
    }
}