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

using Novell.Directory.Ldap.Utilclass;
using System.Collections;
using System.IO;
using System.Text;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents a DIT (Directory Information Tree) content rule
    ///     in a directory schema.
    ///     The LdapDITContentRuleSchema class is used to discover or modify
    ///     additional auxiliary classes, mandatory and optional attributes, and
    ///     restricted attributes in effect for an object class.
    /// </summary>
    public class LdapDitContentRuleSchema : LdapSchemaElement
    {
        /// <summary>
        ///     Constructs a DIT content rule for adding to or deleting from the
        ///     schema.
        /// </summary>
        /// <param name="names">
        ///     The names of the content rule.
        /// </param>
        /// <param name="oid">
        ///     The unique object identifier of the content rule -
        ///     in dotted numerical format.
        /// </param>
        /// <param name="description">
        ///     The optional description of the content rule.
        /// </param>
        /// <param name="obsolete">
        ///     True if the content rule is obsolete.
        /// </param>
        /// <param name="auxiliary">
        ///     A list of auxiliary object classes allowed for
        ///     an entry to which this content rule applies.
        ///     These may either be specified by name or
        ///     numeric oid.
        /// </param>
        /// <param name="required">
        ///     A list of attributes that an entry
        ///     to which this content rule applies must
        ///     contain in addition to its normal set of
        ///     mandatory attributes. These attributes may be
        ///     specified by either name or numeric oid.
        /// </param>
        /// <param name="optional">
        ///     A list of attributes that an entry
        ///     to which this content rule applies may contain
        ///     in addition to its normal set of optional
        ///     attributes. These attributes may be specified by
        ///     either name or numeric oid.
        /// </param>
        /// <param name="precluded">
        ///     A list, consisting of a subset of the optional
        ///     attributes of the structural and
        ///     auxiliary object classes which are precluded
        ///     from an entry to which this content rule
        ///     applies. These may be specified by either name
        ///     or numeric oid.
        /// </param>
        public LdapDitContentRuleSchema(string[] names, string oid, string description, bool obsolete,
            string[] auxiliary, string[] required, string[] optional, string[] precluded)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.Ditcontent])
        {
            Names = new string[names.Length];
            names.CopyTo(Names, 0);
            Id = oid;
            Description = description;
            Obsolete = obsolete;
            AuxiliaryClasses = auxiliary;
            RequiredAttributes = required;
            OptionalAttributes = optional;
            PrecludedAttributes = precluded;
            Value = FormatString();
        }

        /// <summary>
        ///     Constructs a DIT content rule from the raw string value returned from a
        ///     schema query for dITContentRules.
        /// </summary>
        /// <param name="raw">
        ///     The raw string value returned from a schema query
        ///     for content rules.
        /// </param>
        public LdapDitContentRuleSchema(string raw)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.Ditcontent])
        {
            Obsolete = false;
            try
            {
                var parser = new SchemaParser(raw);

                if (parser.Names != null)
                {
                    Names = new string[parser.Names.Length];
                    parser.Names.CopyTo(Names, 0);
                }

                if (parser.Id != null)
                {
                    Id = parser.Id;
                }

                if (parser.Description != null)
                {
                    Description = parser.Description;
                }

                if (parser.Auxiliary != null)
                {
                    AuxiliaryClasses = new string[parser.Auxiliary.Length];
                    parser.Auxiliary.CopyTo(AuxiliaryClasses, 0);
                }

                if (parser.Required != null)
                {
                    RequiredAttributes = new string[parser.Required.Length];
                    parser.Required.CopyTo(RequiredAttributes, 0);
                }

                if (parser.Optional != null)
                {
                    OptionalAttributes = new string[parser.Optional.Length];
                    parser.Optional.CopyTo(OptionalAttributes, 0);
                }

                if (parser.Precluded != null)
                {
                    PrecludedAttributes = new string[parser.Precluded.Length];
                    parser.Precluded.CopyTo(PrecludedAttributes, 0);
                }

                Obsolete = parser.Obsolete;
                var qualifiers = parser.Qualifiers;
                AttributeQualifier attrQualifier;
                while (qualifiers.MoveNext())
                {
                    attrQualifier = qualifiers.Current;
                    SetQualifier(attrQualifier.Name, attrQualifier.Values);
                }

                Value = FormatString();
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        ///     Returns the list of allowed auxiliary classes.
        /// </summary>
        /// <returns>
        ///     The list of allowed auxiliary classes.
        /// </returns>
        public string[] AuxiliaryClasses { get; } = { string.Empty };

        /// <summary>
        ///     Returns the list of additional required attributes for an entry
        ///     controlled by this content rule.
        /// </summary>
        /// <returns>
        ///     The list of additional required attributes.
        /// </returns>
        public string[] RequiredAttributes { get; } = { string.Empty };

        /// <summary>
        ///     Returns the list of additional optional attributes for an entry
        ///     controlled by this content rule.
        /// </summary>
        /// <returns>
        ///     The list of additional optional attributes.
        /// </returns>
        public string[] OptionalAttributes { get; } = { string.Empty };

        /// <summary>
        ///     Returns the list of precluded attributes for an entry controlled by
        ///     this content rule.
        /// </summary>
        /// <returns>
        ///     The list of precluded attributes.
        /// </returns>
        public string[] PrecludedAttributes { get; } = { string.Empty };

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

            var strArray = GetNames();
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

            if ((strArray = AuxiliaryClasses) != null)
            {
                valueBuffer.Append(" AUX ");
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

            if ((strArray = RequiredAttributes) != null)
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

            if ((strArray = OptionalAttributes) != null)
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

            if ((strArray = PrecludedAttributes) != null)
            {
                valueBuffer.Append(" NOT ");
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
