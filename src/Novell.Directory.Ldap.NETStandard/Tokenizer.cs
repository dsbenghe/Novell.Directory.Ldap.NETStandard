using System;
using System.Collections.Generic;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The class performs token processing from strings.
    /// </summary>
    internal class Tokenizer
    {
        // Element list identified
        private readonly List<string> _elements;

        /// <summary>
        ///     Initializes a new class instance with a specified string to process
        ///     and the specified token delimiters to use.
        /// </summary>
        /// <param name="source">String to tokenize.</param>
        /// <param name="delimiters">String containing the delimiters.</param>
        /// <param name="returnDelimiters"><c>true</c>, to return the found delimiter as token.</param>
        public Tokenizer(string source, string delimiters, bool returnDelimiters = false)
        {
            _elements = new List<string>();
            if (returnDelimiters)
            {
                Tokenize(source, delimiters.ToCharArray());
            }
            else
            {
                _elements.AddRange(source.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>
        ///     Current token count for the source string.
        /// </summary>
        public int Count => _elements.Count;

        private void Tokenize(string tempstr, char[] delimiters)
        {
            int nextIndex;
            while ((nextIndex = tempstr.IndexOfAny(delimiters)) >= 0)
            {
                if (nextIndex == 0)
                {
                    if (tempstr.Length > 1)
                    {
                        _elements.Add(tempstr.Substring(0, 1));
                        tempstr = tempstr.Substring(1);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    _elements.Add(tempstr.Substring(0, nextIndex));
                    _elements.Add(tempstr.Substring(nextIndex, 1));
                    if (tempstr.Length > nextIndex + 1)
                    {
                        tempstr = tempstr.Substring(nextIndex + 1);
                    }
                    else
                    {
                        return;
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
            if (_elements.Count < 1)
            {
                throw new Exception();
            }

            var result = _elements[0];
            _elements.RemoveAt(0);
            return result;
        }
    }
}
