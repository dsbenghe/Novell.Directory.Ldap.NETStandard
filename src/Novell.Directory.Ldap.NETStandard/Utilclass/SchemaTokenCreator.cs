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
using System.IO;

namespace Novell.Directory.Ldap.Utilclass
{
    public class SchemaTokenCreator
    {
        private readonly bool _ccomments = false; // C style comments enabled
        private readonly bool _cppcomments = false; // C++ style comments enabled
        private readonly Stream _input;
        private readonly bool _iseolsig = false;

        private readonly StreamReader _reader;
        private readonly StringReader _sreader;
        private char[] _buf;
        private readonly bool _cidtolower = false;
        private byte[] _ctype;
        private double _numberValue;
        private int _peekchar;
        private bool _pushedback;
        public int LastType { get; set; }

        public string StringValue { get; set; }

        public SchemaTokenCreator(Stream instream)
        {
            Initialise();

            _input = instream ?? throw new NullReferenceException();
        }

        public SchemaTokenCreator(StreamReader r)
        {
            Initialise();

            _reader = r ?? throw new NullReferenceException();
        }

        public SchemaTokenCreator(StringReader r)
        {
            Initialise();

            _sreader = r ?? throw new NullReferenceException();
        }

        public int CurrentLine { get; private set; } = 1;

        private void Initialise()
        {
            _ctype = new byte[256];
            _buf = new char[20];
            _peekchar = int.MaxValue;
            WordCharacters('a', 'z');
            WordCharacters('A', 'Z');
            WordCharacters(128 + 32, 255);
            WhitespaceCharacters(0, ' ');
            CommentCharacter('/');
            QuoteCharacter('"');
            QuoteCharacter('\'');
            ParseNumbers();
        }

        public void PushBack()
        {
            _pushedback = true;
        }

        public string ToStringValue()
        {
            string strval;
            switch (LastType)
            {
                case (int)TokenTypes.Eof:
                    strval = "EOF";
                    break;

                case (int)TokenTypes.Eol:
                    strval = "EOL";
                    break;

                case (int)TokenTypes.Word:
                    strval = StringValue;
                    break;

                case (int)TokenTypes.String:
                    strval = StringValue;
                    break;

                case (int)TokenTypes.Number:
                case (int)TokenTypes.Real:
                    strval = "n=" + _numberValue;
                    break;

                default:
                    {
                        if (LastType < 256 && (_ctype[LastType] & (byte)CharacterTypes.Stringquote) != 0)
                        {
                            strval = StringValue;
                            break;
                        }

                        var s = new char[3];
                        s[0] = s[2] = '\'';
                        s[1] = (char)LastType;
                        strval = new string(s);
                        break;
                    }
            }

            return strval;
        }

        public void WordCharacters(int min, int max)
        {
            if (min < 0)
            {
                min = 0;
            }

            if (max >= _ctype.Length)
            {
                max = _ctype.Length - 1;
            }

            while (min <= max)
            {
                _ctype[min++] |= (byte)CharacterTypes.Alphabetic;
            }
        }

        public void WhitespaceCharacters(int min, int max)
        {
            if (min < 0)
            {
                min = 0;
            }

            if (max >= _ctype.Length)
            {
                max = _ctype.Length - 1;
            }

            while (min <= max)
            {
                _ctype[min++] = (byte)CharacterTypes.Whitespace;
            }
        }

        public void OrdinaryCharacters(int min, int max)
        {
            if (min < 0)
            {
                min = 0;
            }

            if (max >= _ctype.Length)
            {
                max = _ctype.Length - 1;
            }

            while (min <= max)
            {
                _ctype[min++] = 0;
            }
        }

        public void OrdinaryCharacter(int ch)
        {
            if (ch >= 0 && ch < _ctype.Length)
            {
                _ctype[ch] = 0;
            }
        }

        public void CommentCharacter(int ch)
        {
            if (ch >= 0 && ch < _ctype.Length)
            {
                _ctype[ch] = (byte)CharacterTypes.Commentchar;
            }
        }

        public void InitTable()
        {
            for (var i = _ctype.Length; --i >= 0;)
            {
                _ctype[i] = 0;
            }
        }

        public void QuoteCharacter(int ch)
        {
            if (ch >= 0 && ch < _ctype.Length)
            {
                _ctype[ch] = (byte)CharacterTypes.Stringquote;
            }
        }

        public void ParseNumbers()
        {
            for (int i = '0'; i <= '9'; i++)
            {
                _ctype[i] |= (byte)CharacterTypes.Numeric;
            }

            _ctype['.'] |= (byte)CharacterTypes.Numeric;
            _ctype['-'] |= (byte)CharacterTypes.Numeric;
        }

        private int Read()
        {
            if (_sreader != null)
            {
                return _sreader.Read();
            }

            if (_reader != null)
            {
                return _reader.Read();
            }

            if (_input != null)
            {
                return _input.ReadByte();
            }

            throw new Exception();
        }

        public int NextToken()
        {
            if (_pushedback)
            {
                _pushedback = false;
                return LastType;
            }

            StringValue = null;

            var curc = _peekchar;
            if (curc < 0)
            {
                curc = int.MaxValue;
            }

            if (curc == int.MaxValue - 1)
            {
                curc = Read();
                if (curc < 0)
                {
                    return LastType = (int)TokenTypes.Eof;
                }

                if (curc == '\n')
                {
                    curc = int.MaxValue;
                }
            }

            if (curc == int.MaxValue)
            {
                curc = Read();
                if (curc < 0)
                {
                    return LastType = (int)TokenTypes.Eof;
                }
            }

            LastType = curc;
            _peekchar = int.MaxValue;

            int ctype = curc < 256 ? _ctype[curc] : (byte)CharacterTypes.Alphabetic;
            while ((ctype & (byte)CharacterTypes.Whitespace) != 0)
            {
                if (curc == '\r')
                {
                    CurrentLine++;
                    if (_iseolsig)
                    {
                        _peekchar = int.MaxValue - 1;
                        return LastType = (int)TokenTypes.Eol;
                    }

                    curc = Read();
                    if (curc == '\n')
                    {
                        curc = Read();
                    }
                }
                else
                {
                    if (curc == '\n')
                    {
                        CurrentLine++;
                        if (_iseolsig)
                        {
                            return LastType = (int)TokenTypes.Eol;
                        }
                    }

                    curc = Read();
                }

                if (curc < 0)
                {
                    return LastType = (int)TokenTypes.Eof;
                }

                ctype = curc < 256 ? _ctype[curc] : (byte)CharacterTypes.Alphabetic;
            }

            if ((ctype & (byte)CharacterTypes.Numeric) != 0)
            {
                var checkb = false;
                if (curc == '-')
                {
                    curc = Read();
                    if (curc != '.' && (curc < '0' || curc > '9'))
                    {
                        _peekchar = curc;
                        return LastType = '-';
                    }

                    checkb = true;
                }

                double dvar = 0;
                var tempvar = 0;
                var checkdec = 0;
                while (true)
                {
                    if (curc == '.' && checkdec == 0)
                    {
                        checkdec = 1;
                    }
                    else if (curc >= '0' && curc <= '9')
                    {
                        dvar = (dvar * 10) + (curc - '0');
                        tempvar += checkdec;
                    }
                    else
                    {
                        break;
                    }

                    curc = Read();
                }

                _peekchar = curc;
                if (tempvar != 0)
                {
                    double divby = 10;
                    tempvar--;
                    while (tempvar > 0)
                    {
                        divby *= 10;
                        tempvar--;
                    }

                    dvar = dvar / divby;
                }

                _numberValue = checkb ? -dvar : dvar;
                return LastType = (int)TokenTypes.Number;
            }

            if ((ctype & (byte)CharacterTypes.Alphabetic) != 0)
            {
                var i = 0;
                do
                {
                    if (i >= _buf.Length)
                    {
                        var nb = new char[_buf.Length * 2];
                        Array.Copy(_buf, 0, nb, 0, _buf.Length);
                        _buf = nb;
                    }

                    _buf[i++] = (char)curc;
                    curc = Read();
                    ctype = curc < 0
                        ? (byte)CharacterTypes.Whitespace
                        : curc < 256
                            ? _ctype[curc]
                            : (byte)CharacterTypes.Alphabetic;
                }
                while ((ctype & ((byte)CharacterTypes.Alphabetic | (byte)CharacterTypes.Numeric)) != 0);

                _peekchar = curc;
                StringValue = new string(_buf, 0, i);
                if (_cidtolower)
                {
                    StringValue = StringValue.ToLower();
                }

                return LastType = (int)TokenTypes.Word;
            }

            if ((ctype & (byte)CharacterTypes.Stringquote) != 0)
            {
                LastType = curc;
                var i = 0;
                var rc = Read();
                while (rc >= 0 && rc != LastType && rc != '\n' && rc != '\r')
                {
                    if (rc == '\\')
                    {
                        curc = Read();
                        var first = curc;
                        if (curc >= '0' && curc <= '7')
                        {
                            curc = curc - '0';
                            var loopchar = Read();
                            if (loopchar >= '0' && loopchar <= '7')
                            {
                                curc = (curc << 3) + (loopchar - '0');
                                loopchar = Read();
                                if (loopchar >= '0' && loopchar <= '7' && first <= '3')
                                {
                                    curc = (curc << 3) + (loopchar - '0');
                                    rc = Read();
                                }
                                else
                                {
                                    rc = loopchar;
                                }
                            }
                            else
                            {
                                rc = loopchar;
                            }
                        }
                        else
                        {
                            switch (curc)
                            {
                                case 'f':
                                    curc = 0xC;
                                    break;

                                case 'a':
                                    curc = 0x7;
                                    break;

                                case 'b':
                                    curc = '\b';
                                    break;

                                case 'v':
                                    curc = 0xB;
                                    break;

                                case 'n':
                                    curc = '\n';
                                    break;

                                case 'r':
                                    curc = '\r';
                                    break;

                                case 't':
                                    curc = '\t';
                                    break;
                            }

                            rc = Read();
                        }
                    }
                    else
                    {
                        curc = rc;
                        rc = Read();
                    }

                    if (i >= _buf.Length)
                    {
                        var nb = new char[_buf.Length * 2];
                        Array.Copy(_buf, 0, nb, 0, _buf.Length);
                        _buf = nb;
                    }

                    _buf[i++] = (char)curc;
                }

                _peekchar = rc == LastType ? int.MaxValue : rc;

                StringValue = new string(_buf, 0, i);
                return LastType;
            }

            if (curc == '/' && (_cppcomments || _ccomments))
            {
                curc = Read();
                if (curc == '*' && _ccomments)
                {
                    var prevc = 0;
                    while ((curc = Read()) != '/' || prevc != '*')
                    {
                        if (curc == '\r')
                        {
                            CurrentLine++;
                            curc = Read();
                            if (curc == '\n')
                            {
                                curc = Read();
                            }
                        }
                        else
                        {
                            if (curc == '\n')
                            {
                                CurrentLine++;
                                curc = Read();
                            }
                        }

                        if (curc < 0)
                        {
                            return LastType = (int)TokenTypes.Eof;
                        }

                        prevc = curc;
                    }

                    return NextToken();
                }

                if (curc == '/' && _cppcomments)
                {
                    while ((curc = Read()) != '\n' && curc != '\r' && curc >= 0)
                    {
                    }

                    _peekchar = curc;
                    return NextToken();
                }

                if ((_ctype['/'] & (byte)CharacterTypes.Commentchar) != 0)
                {
                    while ((curc = Read()) != '\n' && curc != '\r' && curc >= 0)
                    {
                    }

                    _peekchar = curc;
                    return NextToken();
                }

                _peekchar = curc;
                return LastType = '/';
            }

            if ((ctype & (byte)CharacterTypes.Commentchar) != 0)
            {
                while ((curc = Read()) != '\n' && curc != '\r' && curc >= 0)
                {
                }

                _peekchar = curc;
                return NextToken();
            }

            return LastType = curc;
        }
    }
}
