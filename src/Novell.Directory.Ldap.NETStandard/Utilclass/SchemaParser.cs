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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Novell.Directory.Ldap.Utilclass
{
    public class SchemaParser
    {

        public virtual string RawString { get; set; }
        public virtual string[] Names { get; internal set; }
        public virtual ICollection<AttributeQualifier> Qualifiers { get; internal set; }
        public virtual string Id { get; internal set; }
        public virtual string Description { get; internal set; }
        public virtual string Syntax { get; internal set; }
        public virtual string Superior { get; internal set; }
        public virtual bool Single { get; internal set; }
        public virtual bool Obsolete { get; internal set; }
        public virtual string Equality { get; internal set; }
        public virtual string Ordering { get; internal set; }
        public virtual string Substring { get; internal set; }
        public virtual bool Collective { get; internal set; }
        public virtual bool UserMod { get; internal set; } = true;
        public virtual int Usage { get; internal set; }
        public virtual int Type { get; internal set; }
        public virtual string[] Superiors { get; internal set; }
        public virtual string[] Required { get; internal set; }
        public virtual string[] Optional { get; internal set; }
        public virtual string[] Auxiliary { get; internal set; }
        public virtual string[] Precluded { get; internal set; }
        public virtual string[] Applies { get; internal set; }
        public virtual string NameForm { get; internal set; }
        public virtual string ObjectClass { get; internal set; }


        public SchemaParser(string @string)
        {

            Usage = LdapAttributeSchema.USER_APPLICATIONS;
            Qualifiers = new List<AttributeQualifier>();

            RawString = new StringBuilder(@string)
                            .Replace(@"\", @"\\")
                            .ToString();

            int result = 0;
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
            if ((int)TokenTypes.EOF != st2.NextToken())
            {
                if (st2.lastttype == '(')
                {
                    if ((int)TokenTypes.WORD == st2.NextToken())
                    {
                        Id = st2.StringValue;
                    }
                    while ((int)TokenTypes.EOF != st2.NextToken())
                    {
                        if (st2.lastttype == (int)TokenTypes.WORD)
                        {
                            if (st2.StringValue.Equals("NAME", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (st2.NextToken() == '\'')
                                {
                                    Names = new string[1];
                                    Names[0] = st2.StringValue;
                                }
                                else
                                {
                                    if (st2.lastttype == '(')
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
                                result = st2.NextToken();
                                if (result == (int)TokenTypes.WORD || result == '\'')
                                //Test for non-standard schema
                                {
                                    Syntax = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("EQUALITY".ToUpper()))
                            {
                                if (st2.NextToken() == (int)TokenTypes.WORD)
                                {
                                    Equality = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("ORDERING".ToUpper()))
                            {
                                if (st2.NextToken() == (int)TokenTypes.WORD)
                                {
                                    Ordering = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("SUBSTR".ToUpper()))
                            {
                                if (st2.NextToken() == (int)TokenTypes.WORD)
                                {
                                    Substring = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("FORM".ToUpper()))
                            {
                                if (st2.NextToken() == (int)TokenTypes.WORD)
                                {
                                    NameForm = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("OC".ToUpper()))
                            {
                                if (st2.NextToken() == (int)TokenTypes.WORD)
                                {
                                    ObjectClass = st2.StringValue;
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("SUP".ToUpper()))
                            {
                                var values = new ArrayList();
                                st2.NextToken();
                                if (st2.lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.lastttype != ')')
                                    {
                                        if (st2.lastttype != '$')
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
                                if (st2.lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.lastttype != ')')
                                    {
                                        if (st2.lastttype != '$')
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
                                if (st2.lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.lastttype != ')')
                                    {
                                        if (st2.lastttype != '$')
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
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.lastttype != ')')
                                    {
                                        if (st2.lastttype != '$')
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
                            if (st2.StringValue.ToUpper().Equals("AUX".ToUpper()))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.lastttype != ')')
                                    {
                                        if (st2.lastttype != '$')
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
                            if (st2.StringValue.ToUpper().Equals("ABSTRACT".ToUpper()))
                            {
                                Type = LdapObjectClassSchema.ABSTRACT;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("STRUCTURAL".ToUpper()))
                            {
                                Type = LdapObjectClassSchema.STRUCTURAL;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("AUXILIARY".ToUpper()))
                            {
                                Type = LdapObjectClassSchema.AUXILIARY;
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("USAGE".ToUpper()))
                            {
                                if (st2.NextToken() == (int)TokenTypes.WORD)
                                {
                                    currName = st2.StringValue;
                                    if (currName.ToUpper().Equals("directoryOperation".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.DIRECTORY_OPERATION;
                                    }
                                    else if (currName.ToUpper().Equals("distributedOperation".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.DISTRIBUTED_OPERATION;
                                    }
                                    else if (currName.ToUpper().Equals("dSAOperation".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.DSA_OPERATION;
                                    }
                                    else if (currName.ToUpper().Equals("userApplications".ToUpper()))
                                    {
                                        Usage = LdapAttributeSchema.USER_APPLICATIONS;
                                    }
                                }
                                continue;
                            }
                            if (st2.StringValue.ToUpper().Equals("APPLIES".ToUpper()))
                            {
                                var values = new List<string>();
                                st2.NextToken();
                                if (st2.lastttype == '(')
                                {
                                    st2.NextToken();
                                    while (st2.lastttype != ')')
                                    {
                                        if (st2.lastttype != '$')
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
                                Qualifiers.Add(q);
                            }
                        }
                    }
                }
            }
        }

        private AttributeQualifier ParseQualifier(SchemaTokenCreator st, string name)
        {
            var values = new List<string>(5);
            if (st.NextToken() == '\'')
            {
                values.Add(st.StringValue);
            }
            else
            {
                if (st.lastttype == '(')
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