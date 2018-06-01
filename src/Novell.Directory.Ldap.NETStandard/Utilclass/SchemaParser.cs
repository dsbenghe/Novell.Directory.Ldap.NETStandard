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
// Novell.Directory.Ldap.LdapAttributeSchema.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.IO;
using System.Text;

namespace Novell.Directory.Ldap.Utilclass
{
    public class SchemaParser
    {
        private void InitBlock()
        {
            _usage = LdapAttributeSchema.UserApplications;
            _qualifiers = new ArrayList();
        }

        public virtual string RawString
        {
            get => _rawString;

            set => _rawString = value;
        }

        public virtual string[] Names => _names;

        public virtual IEnumerator Qualifiers => new ArrayEnumeration(_qualifiers.ToArray());

        public virtual string Id => _id;

        public virtual string Description => _description;

        public virtual string Syntax => _syntax;

        public virtual string Superior => _superior;

        public virtual bool Single => _single;

        public virtual bool Obsolete => _obsolete;

        public virtual string Equality => _equality;

        public virtual string Ordering => _ordering;

        public virtual string Substring => _substring;

        public virtual bool Collective => _collective;

        public virtual bool UserMod => _userMod;

        public virtual int Usage => _usage;

        public virtual int Type => _type;

        public virtual string[] Superiors => _superiors;

        public virtual string[] Required => _required;

        public virtual string[] Optional => _optional;

        public virtual string[] Auxiliary => _auxiliary;

        public virtual string[] Precluded => _precluded;

        public virtual string[] Applies => _applies;

        public virtual string NameForm => _nameForm;

        public virtual string ObjectClass => _nameForm;

        private string _rawString;
        private string[] _names;
        private string _id;
        private string _description;
        private string _syntax;
        private string _superior;
        private string _nameForm;
        private string _objectClass;
        private string[] _superiors;
        private string[] _required;
        private string[] _optional;
        private string[] _auxiliary;
        private string[] _precluded;
        private string[] _applies;
        private bool _single;
        private bool _obsolete;
        private string _equality;
        private string _ordering;
        private string _substring;
        private bool _collective;
        private bool _userMod = true;
        private int _usage;
        private int _type = -1;
        private int _result;
        private ArrayList _qualifiers;

        public SchemaParser(string aString)
        {
            InitBlock();

            int index;

            if ((index = aString.IndexOf('\\')) != -1)
            {
                /*
                * Unless we escape the slash, StreamTokenizer will interpret the
                * single slash and convert it assuming octal values.
                * Two successive back slashes are intrepreted as one backslash.
                */
                var newString = new StringBuilder(aString.Substring(0, index - 0));
                for (var i = index; i < aString.Length; i++)
                {
                    newString.Append(aString[i]);
                    if (aString[i] == '\\')
                    {
                        newString.Append('\\');
                    }
                }
                _rawString = newString.ToString();
            }
            else
            {
                _rawString = aString;
            }

            var st2 = new SchemaTokenCreator(new StringReader(_rawString));
            st2.OrdinaryCharacter('.');
            st2.OrdinaryCharacters('0', '9');
            st2.OrdinaryCharacter('{');
            st2.OrdinaryCharacter('}');
            st2.OrdinaryCharacter('_');
            st2.OrdinaryCharacter(';');
            st2.WordCharacters('.', '9');
            st2.WordCharacters('{', '}');
            st2.WordCharacters('_', '_');
            st2.WordCharacters(';', ';');
            //First parse out the OID
            string currName;
            if ((int) TokenTypes.Eof != st2.NextToken())
            {
                if (st2.Lastttype == '(')
                {
                    if ((int) TokenTypes.Word == st2.NextToken())
                    {
                        _id = st2.StringValue;
                    }
                    while ((int) TokenTypes.Eof != st2.NextToken())
                    {
                        if (st2.Lastttype == (int) TokenTypes.Word)
                        {
                            if (st2.StringValue.ToUpper().Equals("NAME".ToUpper()))
                            {
                                if (st2.NextToken() == '\'')
                                {
                                    _names = new string[1];
                                    _names[0] = st2.StringValue;
                                }
                                else
                                {
                                    if (st2.Lastttype == '(')
                                    {
                                        var nameList = new ArrayList();
                                        while (st2.NextToken() == '\'')
                                        {
                                            if ((object) st2.StringValue != null)
                                            {
                                                nameList.Add(st2.StringValue);
                                            }
                                        }
                                        if (nameList.Count > 0)
                                        {
                                            _names = new string[nameList.Count];
                                            SupportClass.ArrayListSupport.ToArray(nameList, _names);
                                        }
                                    }
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("DESC".ToUpper()))
                            {
                                if (st2.NextToken() == '\'')
                                {
                                    _description = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("SYNTAX".ToUpper()))
                            {
                                _result = st2.NextToken();
                                if (_result == (int) TokenTypes.Word || _result == '\'')
                                    //Test for non-standard schema
                                {
                                    _syntax = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("EQUALITY".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    _equality = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("ORDERING".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    _ordering = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("SUBSTR".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    _substring = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("FORM".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    _nameForm = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("OC".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    _objectClass = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("SUP".ToUpper()))
                            {
                                var values = new ArrayList();
                                st2.NextToken();
                                if (st2.Lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.Lastttype != ')')
                                    {
                                        if (st2.Lastttype != '$')
                                        {
                                            values.Add(st2.StringValue);
                                        }
                                        st2.NextToken();
                                    }
                                }
                                else
                                {
                                    values.Add(st2.StringValue);
                                    _superior = st2.StringValue;
                                }
                                if (values.Count > 0)
                                {
                                    _superiors = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, _superiors);
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("SINGLE-VALUE".ToUpper()))
                            {
                                _single = true;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("OBSOLETE".ToUpper()))
                            {
                                _obsolete = true;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("COLLECTIVE".ToUpper()))
                            {
                                _collective = true;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("NO-USER-MODIFICATION".ToUpper()))
                            {
                                _userMod = false;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("MUST".ToUpper()))
                            {
                                var values = new ArrayList();
                                st2.NextToken();
                                if (st2.Lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.Lastttype != ')')
                                    {
                                        if (st2.Lastttype != '$')
                                        {
                                            values.Add(st2.StringValue);
                                        }
                                        st2.NextToken();
                                    }
                                }
                                else
                                {
                                    values.Add(st2.StringValue);
                                }
                                if (values.Count > 0)
                                {
                                    _required = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, _required);
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("MAY".ToUpper()))
                            {
                                var values = new ArrayList();
                                st2.NextToken();
                                if (st2.Lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.Lastttype != ')')
                                    {
                                        if (st2.Lastttype != '$')
                                        {
                                            values.Add(st2.StringValue);
                                        }
                                        st2.NextToken();
                                    }
                                }
                                else
                                {
                                    values.Add(st2.StringValue);
                                }
                                if (values.Count > 0)
                                {
                                    _optional = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, _optional);
                                }
                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("NOT".ToUpper()))
                            {
                                var values = new ArrayList();
                                st2.NextToken();
                                if (st2.Lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.Lastttype != ')')
                                    {
                                        if (st2.Lastttype != '$')
                                        {
                                            values.Add(st2.StringValue);
                                        }
                                        st2.NextToken();
                                    }
                                }
                                else
                                {
                                    values.Add(st2.StringValue);
                                }
                                if (values.Count > 0)
                                {
                                    _precluded = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, _precluded);
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("AUX".ToUpper()))
                            {
                                var values = new ArrayList();
                                st2.NextToken();
                                if (st2.Lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.Lastttype != ')')
                                    {
                                        if (st2.Lastttype != '$')
                                        {
                                            values.Add(st2.StringValue);
                                        }
                                        st2.NextToken();
                                    }
                                }
                                else
                                {
                                    values.Add(st2.StringValue);
                                }
                                if (values.Count > 0)
                                {
                                    _auxiliary = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, _auxiliary);
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("ABSTRACT".ToUpper()))
                            {
                                _type = LdapObjectClassSchema.Abstract;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("STRUCTURAL".ToUpper()))
                            {
                                _type = LdapObjectClassSchema.Structural;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("AUXILIARY".ToUpper()))
                            {
                                _type = LdapObjectClassSchema.Auxiliary;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("USAGE".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    currName = st2.StringValue;
                                    if (currName.ToUpper().Equals("directoryOperation".ToUpper()))
                                    {
                                        _usage = LdapAttributeSchema.DirectoryOperation;
                                    }
                                    else if (currName.ToUpper().Equals("distributedOperation".ToUpper()))
                                    {
                                        _usage = LdapAttributeSchema.DistributedOperation;
                                    }
                                    else if (currName.ToUpper().Equals("dSAOperation".ToUpper()))
                                    {
                                        _usage = LdapAttributeSchema.DsaOperation;
                                    }
                                    else if (currName.ToUpper().Equals("userApplications".ToUpper()))
                                    {
                                        _usage = LdapAttributeSchema.UserApplications;
                                    }
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("APPLIES".ToUpper()))
                            {
                                var values = new ArrayList();
                                st2.NextToken();
                                if (st2.Lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.Lastttype != ')')
                                    {
                                        if (st2.Lastttype != '$')
                                        {
                                            values.Add(st2.StringValue);
                                        }
                                        st2.NextToken();
                                    }
                                }
                                else
                                {
                                    values.Add(st2.StringValue);
                                }
                                if (values.Count > 0)
                                {
                                    _applies = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, _applies);
                                }
                                continue;
                            }
                            currName = st2.StringValue;
                            var q = ParseQualifier(st2, currName);
                            if (q != null)
                            {
                                _qualifiers.Add(q);
                            }
                        }
                    }
                }
            }
        }

        private AttributeQualifier ParseQualifier(SchemaTokenCreator st, string name)
        {
            var values = new ArrayList(5);
            if (st.NextToken() == '\'')
            {
                values.Add(st.StringValue);
            }
            else
            {
                if (st.Lastttype == '(')
                {
                    while (st.NextToken() == '\'')
                    {
                        values.Add(st.StringValue);
                    }
                }
            }
            var valArray = new string[values.Count];
            valArray = (string[]) SupportClass.ArrayListSupport.ToArray(values, valArray);
            return new AttributeQualifier(name, valArray);
        }
    }
}