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
        private readonly int _result;
        private string _objectClass;
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

                RawString = newString.ToString();
            }
            else
            {
                RawString = aString;
            }

            var st2 = new SchemaTokenCreator(new StringReader(RawString));
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
                        Id = st2.StringValue;
                    }

                    while ((int) TokenTypes.Eof != st2.NextToken())
                    {
                        if (st2.Lastttype == (int) TokenTypes.Word)
                        {
                            if (st2.StringValue.ToUpper().Equals("NAME".ToUpper()))
                            {
                                if (st2.NextToken() == '\'')
                                {
                                    Names = new string[1];
                                    Names[0] = st2.StringValue;
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
                                            Names = new string[nameList.Count];
                                            SupportClass.ArrayListSupport.ToArray(nameList, Names);
                                        }
                                    }
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("DESC".ToUpper()))
                            {
                                if (st2.NextToken() == '\'')
                                {
                                    Description = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("SYNTAX".ToUpper()))
                            {
                                _result = st2.NextToken();
                                if (_result == (int) TokenTypes.Word || _result == '\'')
                                    //Test for non-standard schema
                                {
                                    Syntax = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("EQUALITY".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    Equality = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("ORDERING".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    Ordering = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("SUBSTR".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    Substring = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("FORM".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    NameForm = st2.StringValue;
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
                                    Superior = st2.StringValue;
                                }

                                if (values.Count > 0)
                                {
                                    Superiors = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, Superiors);
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("SINGLE-VALUE".ToUpper()))
                            {
                                Single = true;
                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("OBSOLETE".ToUpper()))
                            {
                                Obsolete = true;
                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("COLLECTIVE".ToUpper()))
                            {
                                Collective = true;
                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("NO-USER-MODIFICATION".ToUpper()))
                            {
                                UserMod = false;
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
                                    Required = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, Required);
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
                                    Optional = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, Optional);
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
                                    Precluded = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, Precluded);
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
                                    Auxiliary = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, Auxiliary);
                                }

                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("ABSTRACT".ToUpper()))
                            {
                                Type = LdapObjectClassSchema.Abstract;
                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("STRUCTURAL".ToUpper()))
                            {
                                Type = LdapObjectClassSchema.Structural;
                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("AUXILIARY".ToUpper()))
                            {
                                Type = LdapObjectClassSchema.Auxiliary;
                                continue;
                            }

                            if (st2.StringValue.ToUpper().Equals("USAGE".ToUpper()))
                            {
                                if (st2.NextToken() == (int) TokenTypes.Word)
                                {
                                    currName = st2.StringValue;
                                    if (currName.ToUpper().Equals("directoryOperation".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.DirectoryOperation;
                                    }
                                    else if (currName.ToUpper().Equals("distributedOperation".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.DistributedOperation;
                                    }
                                    else if (currName.ToUpper().Equals("dSAOperation".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.DsaOperation;
                                    }
                                    else if (currName.ToUpper().Equals("userApplications".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.UserApplications;
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
                                    Applies = new string[values.Count];
                                    SupportClass.ArrayListSupport.ToArray(values, Applies);
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

        public string RawString { get; set; }

        public string[] Names { get; }

        public IEnumerator Qualifiers => new ArrayEnumeration(_qualifiers.ToArray());

        public string Id { get; }

        public string Description { get; }

        public string Syntax { get; }

        public string Superior { get; }

        public bool Single { get; }

        public bool Obsolete { get; }

        public string Equality { get; }

        public string Ordering { get; }

        public string Substring { get; }

        public bool Collective { get; }

        public bool UserMod { get; } = true;

        public int Usage { get; private set; }

        public int Type { get; } = -1;

        public string[] Superiors { get; }

        public string[] Required { get; }

        public string[] Optional { get; }

        public string[] Auxiliary { get; }

        public string[] Precluded { get; }

        public string[] Applies { get; }

        public string NameForm { get; }

        public string ObjectClass => NameForm;

        private void InitBlock()
        {
            Usage = LdapAttributeSchema.UserApplications;
            _qualifiers = new ArrayList();
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