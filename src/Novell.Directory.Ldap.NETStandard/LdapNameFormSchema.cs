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
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     A specific a name form in the directory schema.
    ///     The LdapNameFormSchema class represents the definition of a Name Form.  It
    ///     is used to discover or modify the allowed naming attributes for a particular
    ///     object class.
    /// </summary>
    /// <seealso cref="LdapSchemaElement">
    /// </seealso>
    /// <seealso cref="LdapSchema">
    /// </seealso>
    public class LdapNameFormSchema : LdapSchemaElement
    {
        /// <summary>
        ///     Constructs a name form for adding to or deleting from the schema.
        /// </summary>
        /// <param name="names">
        ///     The name(s) of the name form.
        /// </param>
        /// <param name="oid">
        ///     The unique object identifier of the name form - in
        ///     dotted numerical format.
        /// </param>
        /// <param name="description">
        ///     An optional description of the name form.
        /// </param>
        /// <param name="obsolete">
        ///     True if the name form is obsolete.
        /// </param>
        /// <param name="objectClass">
        ///     The object to which this name form applies.
        ///     This may be specified by either name or
        ///     numeric oid.
        /// </param>
        /// <param name="required">
        ///     A list of the attributes that must be present
        ///     in the RDN of an entry that this name form
        ///     controls. These attributes may be specified by
        ///     either name or numeric oid.
        /// </param>
        /// <param name="optional">
        ///     A list of the attributes that may be present
        ///     in the RDN of an entry that this name form
        ///     controls. These attributes may be specified by
        ///     either name or numeric oid.
        /// </param>
        public LdapNameFormSchema(string[] names, string oid, string description, bool obsolete, string objectClass,
            string[] required, string[] optional)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.NameForm])
        {
            this.names = new string[names.Length];
            names.CopyTo(this.names, 0);
            Oid = oid;
            Description = description;
            Obsolete = obsolete;
            ObjectClass = objectClass;
            RequiredNamingAttributes = new string[required.Length];
            required.CopyTo(RequiredNamingAttributes, 0);
            OptionalNamingAttributes = new string[optional.Length];
            optional.CopyTo(OptionalNamingAttributes, 0);
            Value = FormatString();
        }

        /*
        }

        /**
        * Constructs a Name Form from the raw string value returned on a
        * schema query for nameForms.
        *
        * @param raw        The raw string value returned on a schema
        *                   query for nameForms.
        */

        public LdapNameFormSchema(string raw)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.NameForm])
        {
            Obsolete = false;
            try
            {
                var parser = new SchemaParser(raw);

                if (parser.Names != null)
                {
                    names = new string[parser.Names.Length];
                    parser.Names.CopyTo(names, 0);
                }

                if (parser.Id != null)
                {
                    Oid = parser.Id;
                }

                if (parser.Description != null)
                {
                    Description = parser.Description;
                }

                if (parser.Required != null)
                {
                    RequiredNamingAttributes = new string[parser.Required.Length];
                    parser.Required.CopyTo(RequiredNamingAttributes, 0);
                }

                if (parser.Optional != null)
                {
                    OptionalNamingAttributes = new string[parser.Optional.Length];
                    parser.Optional.CopyTo(OptionalNamingAttributes, 0);
                }

                if (parser.ObjectClass != null)
                {
                    ObjectClass = parser.ObjectClass;
                }

                Obsolete = parser.Obsolete;
                var qualifiers = parser.Qualifiers;
                AttributeQualifier attrQualifier;
                while (qualifiers.MoveNext())
                {
                    attrQualifier = (AttributeQualifier)qualifiers.Current;
                    SetQualifier(attrQualifier.Name, attrQualifier.Values);
                }

                Value = FormatString();
            }
            catch (IOException ex)
            {
                Logger.Log.LogWarning("Exception swallowed", ex);
            }
        }

        /// <summary>
        ///     Returns the name of the object class which this name form applies to.
        /// </summary>
        /// <returns>
        ///     The name of the object class.
        /// </returns>
        public string ObjectClass { get; }

        /// <summary>
        ///     Returns the list of required naming attributes for an entry
        ///     controlled by this name form.
        /// </summary>
        /// <returns>
        ///     The list of required naming attributes.
        /// </returns>
        public string[] RequiredNamingAttributes { get; }

        /// <summary>
        ///     Returns the list of optional naming attributes for an entry
        ///     controlled by this content rule.
        /// </summary>
        /// <returns>
        ///     The list of the optional naming attributes.
        /// </returns>
        public string[] OptionalNamingAttributes { get; }

        /// <summary>
        ///     Returns a string in a format suitable for directly adding to a
        ///     directory, as a value of the particular schema element class.
        /// </summary>
        /// <returns>
        ///     A string representation of the class' definition.
        /// </returns>
        protected override string FormatString()
        {
            var valueBuffer = new StringBuilder("( ");
            string token;

            if ((token = Id) != null)
            {
                valueBuffer.Append(token);
            }

            var strArray = Names;
            if (strArray != null)
            {
                valueBuffer.Append(" NAME ");
                if (strArray.Length == 1)
                {
                    valueBuffer.Append("'" + strArray[0] + "'");
                }
                else
                {
                    valueBuffer.Append("( ");

                    for (var i = 0; i < strArray.Length; i++)
                    {
                        valueBuffer.Append(" '" + strArray[i] + "'");
                    }

                    valueBuffer.Append(" )");
                }
            }

            if ((token = Description) != null)
            {
                valueBuffer.Append(" DESC ");
                valueBuffer.Append("'" + token + "'");
            }

            if (Obsolete)
            {
                valueBuffer.Append(" OBSOLETE");
            }

            if ((token = ObjectClass) != null)
            {
                valueBuffer.Append(" OC ");
                valueBuffer.Append("'" + token + "'");
            }

            if ((strArray = RequiredNamingAttributes) != null)
            {
                valueBuffer.Append(" MUST ");
                if (strArray.Length > 1)
                {
                    valueBuffer.Append("( ");
                }

                for (var i = 0; i < strArray.Length; i++)
                {
                    if (i > 0)
                    {
                        valueBuffer.Append(" $ ");
                    }

                    valueBuffer.Append(strArray[i]);
                }

                if (strArray.Length > 1)
                {
                    valueBuffer.Append(" )");
                }
            }

            if ((strArray = OptionalNamingAttributes) != null)
            {
                valueBuffer.Append(" MAY ");
                if (strArray.Length > 1)
                {
                    valueBuffer.Append("( ");
                }

                for (var i = 0; i < strArray.Length; i++)
                {
                    if (i > 0)
                    {
                        valueBuffer.Append(" $ ");
                    }

                    valueBuffer.Append(strArray[i]);
                }

                if (strArray.Length > 1)
                {
                    valueBuffer.Append(" )");
                }
            }

            IEnumerator en;
            if ((en = QualifierNames) != null)
            {
                string qualName;
                string[] qualValue;
                while (en.MoveNext())
                {
                    qualName = (string)en.Current;
                    valueBuffer.Append(" " + qualName + " ");
                    if ((qualValue = GetQualifier(qualName)) != null)
                    {
                        if (qualValue.Length > 1)
                        {
                            valueBuffer.Append("( ");
                        }

                        for (var i = 0; i < qualValue.Length; i++)
                        {
                            if (i > 0)
                            {
                                valueBuffer.Append(" ");
                            }

                            valueBuffer.Append("'" + qualValue[i] + "'");
                        }

                        if (qualValue.Length > 1)
                        {
                            valueBuffer.Append(" )");
                        }
                    }
                }
            }

            valueBuffer.Append(" )");
            return valueBuffer.ToString();
        }
    }
}