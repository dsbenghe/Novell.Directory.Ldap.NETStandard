using System;
using System.Collections;

namespace Novell.Directory.Ldap
{
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
