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
    ///     The schema definition of an object class in a directory server.
    ///     The LdapObjectClassSchema class represents the definition of an object
    ///     class.  It is used to query the syntax of an object class.
    /// </summary>
    /// <seealso cref="LdapSchemaElement">
    /// </seealso>
    /// <seealso cref="LdapSchema">
    /// </seealso>
    public class LdapObjectClassSchema : LdapSchemaElement
    {
        /// <summary>
        ///     This class definition defines an abstract schema class.
        ///     This is equivalent to setting the Novell eDirectory effective class
        ///     flag to true.
        /// </summary>
        public const int Abstract = 0;

        /// <summary>
        ///     This class definition defines a structural schema class.
        ///     This is equivalent to setting the Novell eDirectory effective class
        ///     flag to true.
        /// </summary>
        public const int Structural = 1;

        /// <summary> This class definition defines an auxiliary schema class.</summary>
        public const int Auxiliary = 2;

        /// <summary>
        ///     Constructs an object class definition for adding to or deleting from
        ///     a directory's schema.
        /// </summary>
        /// <param name="names">
        ///     Name(s) of the object class.
        /// </param>
        /// <param name="oid">
        ///     Object Identifer of the object class - in
        ///     dotted-decimal format.
        /// </param>
        /// <param name="description">
        ///     Optional description of the object class.
        /// </param>
        /// <param name="superiors">
        ///     The object classes from which this one derives.
        /// </param>
        /// <param name="required">
        ///     A list of attributes required
        ///     for an entry with this object class.
        /// </param>
        /// <param name="optional">
        ///     A list of attributes acceptable but not required
        ///     for an entry with this object class.
        /// </param>
        /// <param name="type">
        ///     One of ABSTRACT, AUXILIARY, or STRUCTURAL. These
        ///     constants are defined in LdapObjectClassSchema.
        /// </param>
        /// <param name="obsolete">
        ///     true if this object is obsolete.
        /// </param>
        public LdapObjectClassSchema(string[] names, string oid, string[] superiors, string description,
            string[] required, string[] optional, int type, bool obsolete)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.ObjectClass])
        {
            Names = new string[names.Length];
            names.CopyTo(Names, 0);
            Id = oid;
            Description = description;
            Type = type;
            Obsolete = obsolete;
            if (superiors != null)
            {
                Superiors = new string[superiors.Length];
                superiors.CopyTo(Superiors, 0);
            }

            if (required != null)
            {
                RequiredAttributes = new string[required.Length];
                required.CopyTo(RequiredAttributes, 0);
            }

            if (optional != null)
            {
                OptionalAttributes = new string[optional.Length];
                optional.CopyTo(OptionalAttributes, 0);
            }

            Value = FormatString();
        }

        /// <summary>
        ///     Constructs an object class definition from the raw string value
        ///     returned from a directory query for "objectClasses".
        /// </summary>
        /// <param name="raw">
        ///     The raw string value returned from a directory
        ///     query for "objectClasses".
        /// </param>
        public LdapObjectClassSchema(string raw)
            : base(LdapSchema.SchemaTypeNames[LdapSchema.ObjectClass])
        {
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

                Obsolete = parser.Obsolete;
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

                if (parser.Superiors != null)
                {
                    Superiors = new string[parser.Superiors.Length];
                    parser.Superiors.CopyTo(Superiors, 0);
                }

                Type = parser.Type;
                var qualifiers = parser.Qualifiers;
                AttributeQualifier attrQualifier;
                while (qualifiers.MoveNext())
                {
                    attrQualifier = qualifiers.Current;
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
        ///     Returns the object classes from which this one derives.
        /// </summary>
        /// <returns>
        ///     The object classes superior to this class.
        /// </returns>
        public string[] Superiors { get; }

        /// <summary>
        ///     Returns a list of attributes required for an entry with this object
        ///     class.
        /// </summary>
        /// <returns>
        ///     The list of required attributes defined for this class.
        /// </returns>
        public string[] RequiredAttributes { get; }

        /// <summary>
        ///     Returns a list of optional attributes but not required of an entry
        ///     with this object class.
        /// </summary>
        /// <returns>
        ///     The list of optional attributes defined for this class.
        /// </returns>
        public string[] OptionalAttributes { get; }

        /// <summary>
        ///     Returns the type of object class.
        ///     The getType method returns one of the following constants defined in
        ///     LdapObjectClassSchema:
        ///     <ul>
        ///         <li>ABSTRACT</li>
        ///         <li>AUXILIARY</li>
        ///         <li>STRUCTURAL</li>
        ///     </ul>
        ///     See the LdapSchemaElement.getQualifier method for information on
        ///     obtaining the X-NDS flags.
        /// </summary>
        /// <returns>
        ///     The type of object class.
        /// </returns>
        public int Type { get; } = -1;

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
                        valueBuffer.Append(" $ ");
                    }

                    valueBuffer.Append(strArray[i]);
                }

                if (strArray.Length > 1)
                {
                    valueBuffer.Append(" )");
                }
            }

            if (Type != -1)
            {
                if (Type == Abstract)
                {
                    valueBuffer.Append(" ABSTRACT");
                }
                else if (Type == Auxiliary)
                {
                    valueBuffer.Append(" AUXILIARY");
                }
                else if (Type == Structural)
                {
                    valueBuffer.Append(" STRUCTURAL");
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
