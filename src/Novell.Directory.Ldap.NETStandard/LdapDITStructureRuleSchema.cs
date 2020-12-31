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
    ///     Represents the definition of a specific DIT (Directory Information Tree)
    ///     structure rule in the directory schema.
    ///     The LdapDITStructureRuleSchema class represents the definition of a DIT
    ///     Structure Rule.  It is used to discover or modify which
    ///     object classes a particular object class may be subordinate to in the DIT.
    /// </summary>
    public class LdapDitStructureRuleSchema : LdapSchemaElement
    {
        /// <summary>
        ///     Constructs a DIT structure rule for adding to or deleting from the
        ///     schema.
        /// </summary>
        /// <param name="names">
        ///     The names of the structure rule.
        /// </param>
        /// <param name="ruleId">
        ///     The unique identifier of the structure rule. NOTE:
        ///     this is an integer, not a dotted numerical
        ///     identifier. Structure rules aren't identified
        ///     by OID.
        /// </param>
        /// <param name="description">
        ///     An optional description of the structure rule.
        /// </param>
        /// <param name="obsolete">
        ///     True if the structure rule is obsolete.
        /// </param>
        /// <param name="nameForm">
        ///     Either the identifier or name of a name form.
        ///     This is used to indirectly refer to the object
        ///     class that this structure rule applies to.
        /// </param>
        /// <param name="superiorIDs">
        ///     A list of superior structure rules - specified
        ///     by their integer ID. The object class
        ///     specified by this structure rule (via the
        ///     nameForm parameter) may only be subordinate in
        ///     the DIT to object classes of those represented
        ///     by the structure rules here; it may be null.
        /// </param>
        public LdapDitStructureRuleSchema(string[] names, int ruleId, string description, bool obsolete,
            string nameForm,
            string[] superiorIDs)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.Ditstructure])
        {
            Names = new string[names.Length];
            names.CopyTo(Names, 0);
            RuleId = ruleId;
            Description = description;
            Obsolete = obsolete;
            NameForm = nameForm;
            Superiors = superiorIDs;
            Value = FormatString();
        }

        /// <summary>
        ///     Constructs a DIT structure rule from the raw string value returned from
        ///     a schema query for dITStructureRules.
        /// </summary>
        /// <param name="raw">
        ///     The raw string value returned from a schema
        ///     query for dITStructureRules.
        /// </param>
        public LdapDitStructureRuleSchema(string raw)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.Ditstructure])
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
                    RuleId = int.Parse(parser.Id);
                }

                if (parser.Description != null)
                {
                    Description = parser.Description;
                }

                if (parser.Superiors != null)
                {
                    Superiors = new string[parser.Superiors.Length];
                    parser.Superiors.CopyTo(Superiors, 0);
                }

                if (parser.NameForm != null)
                {
                    NameForm = parser.NameForm;
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
        ///     Returns the rule ID for this structure rule.
        ///     The getRuleID method returns an integer rather than a dotted
        ///     decimal OID. Objects of this class do not have an OID,
        ///     thus getID can return null.
        /// </summary>
        /// <returns>
        ///     The rule ID for this structure rule.
        /// </returns>
        public int RuleId { get; }

        /// <summary>
        ///     Returns the NameForm that this structure rule controls.
        ///     You can get the actual object class that this structure rule controls
        ///     by calling the getNameForm.getObjectClass method.
        /// </summary>
        /// <returns>
        ///     The NameForm that this structure rule controls.
        /// </returns>
        public string NameForm { get; } = string.Empty;

        /// <summary>
        ///     Returns a list of all structure rules that are superior to this
        ///     structure rule.
        ///     To resolve to an object class, you need to first
        ///     resolve the superior ID to another structure rule, then call
        ///     the getNameForm.getObjectClass method on that structure rule.
        /// </summary>
        /// <returns>
        ///     A list of all structure rules that are superior to this structure rule.
        /// </returns>
        public string[] Superiors { get; } = { string.Empty };

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

            var token = RuleId.ToString();
            valueBuffer.Append(token);

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

            if ((token = NameForm) != null)
            {
                valueBuffer.Append(" FORM ");
                valueBuffer.Append("'" + token + "'");
            }

            if ((strArray = Superiors) != null)
            {
                valueBuffer.Append(" SUP ");
                if (strArray.Length > 1)
                {
                    valueBuffer.Append("( ");
                }

                for (var i = 0; i < strArray.Length; i++)
                {
                    if (i > 0)
                    {
                        valueBuffer.Append(" ");
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
