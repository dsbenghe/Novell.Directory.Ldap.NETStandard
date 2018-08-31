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
    }
}