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

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Novell.Directory.Ldap.Utilclass
{
    public class SchemaParser
    {
        private readonly int _result;
        private string _objectClass;
        private readonly List<AttributeQualifier> _qualifiers = new List<AttributeQualifier>();

        public SchemaParser(string aString)
        {
            Usage = LdapAttributeSchema.UserApplications;

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

            // First parse out the OID
            string currName;
            if (st2.NextToken() != (int)TokenTypes.Eof)
            {
                if (st2.LastType == '(')
                {
                    if (st2.NextToken() == (int)TokenTypes.Word)
                    {
                        Id = st2.StringValue;
                    }

                    while (st2.NextToken() != (int)TokenTypes.Eof)
                    {
                        if (st2.LastType == (int)TokenTypes.Word)
                        {
                            if (st2.StringValue.EqualsOrdinalCI("NAME"))
                            {
                                if (st2.NextToken() == '\'')
                                {
                                    Names = new string[1];
                                    Names[0] = st2.StringValue;
                                }
                                else
                                {
                                    if (st2.LastType == '(')
                                    {
                                        var nameList = new List<string>();
                                        while (st2.NextToken() == '\'')
                                        {
                                            if (st2.StringValue != null)
                                            {
                                                nameList.Add(st2.StringValue);
                                            }
                                        }

                                        if (nameList.Count > 0)
                                        {
                                            Names = nameList.ToArray();
                                        }
                                    }
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("DESC"))
                            {
                                if (st2.NextToken() == '\'')
                                {
                                    Description = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("SYNTAX"))
                            {
                                _result = st2.NextToken();

                                // Test for non-standard schema
                                if (_result == (int)TokenTypes.Word || _result == '\'')
                                {
                                    Syntax = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("EQUALITY"))
                            {
                                if (st2.NextToken() == (int)TokenTypes.Word)
                                {
                                    Equality = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("ORDERING"))
                            {
                                if (st2.NextToken() == (int)TokenTypes.Word)
                                {
                                    Ordering = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("SUBSTR"))
                            {
                                if (st2.NextToken() == (int)TokenTypes.Word)
                                {
                                    Substring = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("FORM"))
                            {
                                if (st2.NextToken() == (int)TokenTypes.Word)
                                {
                                    NameForm = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("OC"))
                            {
                                if (st2.NextToken() == (int)TokenTypes.Word)
                                {
                                    _objectClass = st2.StringValue;
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("SUP"))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.LastType == '(')
                                {
                                    st2.NextToken();
                                    while (st2.LastType != ')')
                                    {
                                        if (st2.LastType != '$')
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
                                    Superiors = values.ToArray();
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("SINGLE-VALUE"))
                            {
                                Single = true;
                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("OBSOLETE"))
                            {
                                Obsolete = true;
                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("COLLECTIVE"))
                            {
                                Collective = true;
                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("NO-USER-MODIFICATION"))
                            {
                                UserMod = false;
                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("MUST"))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.LastType == '(')
                                {
                                    st2.NextToken();
                                    while (st2.LastType != ')')
                                    {
                                        if (st2.LastType != '$')
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
                                    Required = values.ToArray();
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("MAY"))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.LastType == '(')
                                {
                                    st2.NextToken();
                                    while (st2.LastType != ')')
                                    {
                                        if (st2.LastType != '$')
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
                                    Optional = values.ToArray();
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("NOT"))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.LastType == '(')
                                {
                                    st2.NextToken();
                                    while (st2.LastType != ')')
                                    {
                                        if (st2.LastType != '$')
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
                                    Precluded = values.ToArray();
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("AUX"))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.LastType == '(')
                                {
                                    st2.NextToken();
                                    while (st2.LastType != ')')
                                    {
                                        if (st2.LastType != '$')
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
                                    Auxiliary = values.ToArray();
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("ABSTRACT"))
                            {
                                Type = LdapObjectClassSchema.Abstract;
                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("STRUCTURAL"))
                            {
                                Type = LdapObjectClassSchema.Structural;
                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("AUXILIARY"))
                            {
                                Type = LdapObjectClassSchema.Auxiliary;
                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("USAGE"))
                            {
                                if (st2.NextToken() == (int)TokenTypes.Word)
                                {
                                    currName = st2.StringValue;
                                    if (currName.EqualsOrdinalCI("directoryOperation"))
                                    {
                                        Usage = LdapAttributeSchema.DirectoryOperation;
                                    }
                                    else if (currName.EqualsOrdinalCI("distributedOperation"))
                                    {
                                        Usage = LdapAttributeSchema.DistributedOperation;
                                    }
                                    else if (currName.EqualsOrdinalCI("dSAOperation"))
                                    {
                                        Usage = LdapAttributeSchema.DsaOperation;
                                    }
                                    else if (currName.EqualsOrdinalCI("userApplications"))
                                    {
                                        Usage = LdapAttributeSchema.UserApplications;
                                    }
                                }

                                continue;
                            }

                            if (st2.StringValue.EqualsOrdinalCI("APPLIES"))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.LastType == '(')
                                {
                                    st2.NextToken();
                                    while (st2.LastType != ')')
                                    {
                                        if (st2.LastType != '$')
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
                                    Applies = values.ToArray();
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

        public IEnumerator<AttributeQualifier> Qualifiers => _qualifiers.GetEnumerator();

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

        private AttributeQualifier ParseQualifier(SchemaTokenCreator st, string name)
        {
            var values = new List<string>(5);
            if (st.NextToken() == '\'')
            {
                values.Add(st.StringValue);
            }
            else
            {
                if (st.LastType == '(')
                {
                    while (st.NextToken() == '\'')
                    {
                        values.Add(st.StringValue);
                    }
                }
            }

            return new AttributeQualifier(name, values.ToArray());
        }
    }
}
