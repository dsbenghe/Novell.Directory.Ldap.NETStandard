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
using System.Collections.Generic;
using System.Globalization;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Represents a schema entry that controls one or more entries held by a
    ///     Directory Server.
    ///     <code>LdapSchema</code> Contains methods to parse schema attributes into
    ///     individual schema definitions, represented by subclasses of
    ///     {@link LdapSchemaElement}.  Schema may be retrieved from a Directory server
    ///     with the fetchSchema method of LdapConnection or by creating an LdapEntry
    ///     containing schema attributes.  The following sample code demonstrates how to
    ///     retrieve schema elements from LdapSchema.
    ///     <pre>
    ///         <code>
    /// .
    /// .
    /// .
    /// LdapSchema schema;
    /// LdapSchemaElement element;
    /// // connect to the server
    /// lc.connect( ldapHost, ldapPort );
    /// lc.bind( ldapVersion, loginDN, password );
    /// // read the schema from the directory
    /// schema = lc.fetchSchema( lc.getSchemaDN() );
    /// // retrieve the definition of common name
    /// element = schema.getAttributeSchema( "cn" );
    /// System.out.println("The attribute cn has an oid of " + element.getID());
    /// .
    /// .
    /// .
    /// </code>
    ///     </pre>
    /// </summary>
    /// <seealso cref="LdapSchemaElement" />
    /// <seealso cref="LdapConnection.FetchSchemaAsync" />
    /// <seealso cref="LdapConnection.GetSchemaDnAsync()" />
    /// <seealso cref="LdapConnection.GetSchemaDnAsync(string)" />
    public class LdapSchema : LdapEntry
    {
        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int Attribute = 0;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int ObjectClass = 1;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int Syntax = 2;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int NameForm = 3;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int Ditcontent = 4;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int Ditstructure = 5;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int Matching = 6;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable. </summary>
        /*package*/
        internal const int MatchingUse = 7;

        /*package*/

        /// <summary>
        ///     The following lists the Ldap names of subschema attributes for
        ///     schema elements (definitions):.
        /// </summary>
        internal static readonly string[] SchemaTypeNames =
        {
            "attributeTypes", "objectClasses", "ldapSyntaxes",
            "nameForms", "dITContentRules", "dITStructureRules", "matchingRules", "matchingRuleUse",
        };

        /// <summary>
        ///     The idTable hash on the oid of Attribute and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapAttributeSchema> _attributeIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of Attribute (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapAttributeSchema> _attributeNameTable;

        /// <summary>
        ///     The idTable hash on the oid of ObjectClass and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapObjectClassSchema> _objectClassIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of ObjectClass (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapObjectClassSchema> _objectClassNameTable;

        /// <summary>
        ///     The idTable hash on the oid of Syntax and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapSyntaxSchema> _syntaxIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of Syntax (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapSyntaxSchema> _syntaxNameTable;

        /// <summary>
        ///     The idTable hash on the oid of NameForm and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapNameFormSchema> _nameFormIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of NameForm (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapNameFormSchema> _nameFormNameTable;

        /// <summary>
        ///     The idTable hash on the oid of Ditcontent and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapDitContentRuleSchema> _ditcontentIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of Ditcontent (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapDitContentRuleSchema> _ditcontentNameTable;

        /// <summary>
        ///     The idTable hash on the integer ID of Ditstructure and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapDitStructureRuleSchema> _ditstructureIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of Ditstructure (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapDitStructureRuleSchema> _ditstructureNameTable;

        /// <summary>
        ///     The idTable hash on the oid of Matching and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapMatchingRuleSchema> _matchingIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of Matching (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapMatchingRuleSchema> _matchingNameTable;

        /// <summary>
        ///     The idTable hash on the oid of MatchingUse and
        ///     is used for retrieving enumerations.
        /// </summary>
        private readonly Dictionary<string, LdapMatchingRuleUseSchema> _matchingUseIdTable;

        /// <summary>
        ///     The nameTable will hash on the names of MatchingUse (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private readonly Dictionary<string, LdapMatchingRuleUseSchema> _matchingUseNameTable;

        /// <summary>
        ///     Constructs an LdapSchema object from attributes of an LdapEntry.
        ///     The object is empty if the entry parameter contains no schema
        ///     attributes.  The recognized schema attributes are the following:.
        ///     <pre>
        ///         <code>
        /// "attributeTypes", "objectClasses", "ldapSyntaxes",
        /// "nameForms", "dITContentRules", "dITStructureRules",
        /// "matchingRules","matchingRuleUse"
        /// </code>
        ///     </pre>
        /// </summary>
        /// <param name="ent">
        ///     An LdapEntry containing schema information.
        /// </param>
        public LdapSchema(LdapEntry ent)
            : base(ent.Dn, ent.GetAttributeSet())
        {
            // reset all definitions
            _attributeIdTable = new Dictionary<string, LdapAttributeSchema>(StringComparer.Ordinal);
            _attributeNameTable = new Dictionary<string, LdapAttributeSchema>(StringComparer.OrdinalIgnoreCase);
            _objectClassIdTable = new Dictionary<string, LdapObjectClassSchema>(StringComparer.Ordinal);
            _objectClassNameTable = new Dictionary<string, LdapObjectClassSchema>(StringComparer.OrdinalIgnoreCase);
            _syntaxIdTable = new Dictionary<string, LdapSyntaxSchema>(StringComparer.Ordinal);
            _syntaxNameTable = new Dictionary<string, LdapSyntaxSchema>(StringComparer.OrdinalIgnoreCase);
            _nameFormIdTable = new Dictionary<string, LdapNameFormSchema>(StringComparer.Ordinal);
            _nameFormNameTable = new Dictionary<string, LdapNameFormSchema>(StringComparer.OrdinalIgnoreCase);
            _ditcontentIdTable = new Dictionary<string, LdapDitContentRuleSchema>(StringComparer.Ordinal);
            _ditcontentNameTable = new Dictionary<string, LdapDitContentRuleSchema>(StringComparer.OrdinalIgnoreCase);
            _ditstructureIdTable = new Dictionary<string, LdapDitStructureRuleSchema>(StringComparer.Ordinal);
            _ditstructureNameTable = new Dictionary<string, LdapDitStructureRuleSchema>(StringComparer.OrdinalIgnoreCase);
            _matchingIdTable = new Dictionary<string, LdapMatchingRuleSchema>(StringComparer.Ordinal);
            _matchingNameTable = new Dictionary<string, LdapMatchingRuleSchema>(StringComparer.OrdinalIgnoreCase);
            _matchingUseIdTable = new Dictionary<string, LdapMatchingRuleUseSchema>(StringComparer.Ordinal);
            _matchingUseNameTable = new Dictionary<string, LdapMatchingRuleUseSchema>(StringComparer.OrdinalIgnoreCase);

            foreach (var attr in GetAttributeSet())
            {
                var attrName = attr.Name;

                if (attrName.EqualsOrdinalCI(SchemaTypeNames[ObjectClass]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        LdapObjectClassSchema classSchema;
                        try
                        {
                            classSchema = new LdapObjectClassSchema(valueRenamed);
                        }
                        catch (Exception e)
                        {
                            Logger.Log.LogWarning("Exception swallowed", e);
                            continue; // Error parsing: do not add this definition
                        }

                        AddElement(_objectClassIdTable, _objectClassNameTable, classSchema);
                    }
                }
                else if (attrName.EqualsOrdinalCI(SchemaTypeNames[Attribute]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        LdapAttributeSchema attrSchema;
                        try
                        {
                            attrSchema = new LdapAttributeSchema(valueRenamed);
                        }
                        catch (Exception e)
                        {
                            Logger.Log.LogWarning("Exception swallowed", e);
                            continue; // Error parsing: do not add this definition
                        }

                        AddElement(_attributeIdTable, _attributeNameTable, attrSchema);
                    }
                }
                else if (attrName.EqualsOrdinalCI(SchemaTypeNames[Syntax]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        var syntaxSchema = new LdapSyntaxSchema(valueRenamed);
                        AddElement(_syntaxIdTable, _syntaxNameTable, syntaxSchema);
                    }
                }
                else if (attrName.EqualsOrdinalCI(SchemaTypeNames[Matching]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        var matchingRuleSchema = new LdapMatchingRuleSchema(valueRenamed, null);
                        AddElement(_matchingIdTable, _matchingNameTable, matchingRuleSchema);
                    }
                }
                else if (attrName.EqualsOrdinalCI(SchemaTypeNames[MatchingUse]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        var matchingRuleUseSchema = new LdapMatchingRuleUseSchema(valueRenamed);
                        AddElement(_matchingUseIdTable, _matchingUseNameTable, matchingRuleUseSchema);
                    }
                }
                else if (attrName.EqualsOrdinalCI(SchemaTypeNames[Ditcontent]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        var dItContentRuleSchema = new LdapDitContentRuleSchema(valueRenamed);
                        AddElement(_ditcontentIdTable, _ditcontentNameTable, dItContentRuleSchema);
                    }
                }
                else if (attrName.EqualsOrdinalCI(SchemaTypeNames[Ditstructure]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        var dItStructureRuleSchema = new LdapDitStructureRuleSchema(valueRenamed);
                        AddElement(_ditstructureIdTable, _ditstructureNameTable, dItStructureRuleSchema);
                    }
                }
                else if (attrName.EqualsOrdinalCI(SchemaTypeNames[NameForm]))
                {
                    foreach (var valueRenamed in attr.StringValueArray)
                    {
                        var nameFormSchema = new LdapNameFormSchema(valueRenamed);
                        AddElement(_nameFormIdTable, _nameFormNameTable, nameFormSchema);
                    }
                }

                // All non schema attributes are ignored.
            }
        }

        /// <summary>
        ///     Returns an enumeration of attribute definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of attribute definitions.
        /// </returns>
        public IEnumerable<LdapAttributeSchema> AttributeSchemas => _attributeIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of DIT content rule definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT content rule definitions.
        /// </returns>
        public IEnumerable<LdapDitContentRuleSchema> DitContentRuleSchemas => _ditcontentIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of DIT structure rule definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT structure rule definitions.
        /// </returns>
        public IEnumerable<LdapDitStructureRuleSchema> DitStructureRuleSchemas => _ditstructureIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of matching rule definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule definitions.
        /// </returns>
        public IEnumerable<LdapMatchingRuleSchema> MatchingRuleSchemas => _matchingIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of matching rule use definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule use definitions.
        /// </returns>
        public IEnumerable<LdapMatchingRuleUseSchema> MatchingRuleUseSchemas => _matchingUseIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of name form definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of name form definitions.
        /// </returns>
        public IEnumerable<LdapNameFormSchema> NameFormSchemas => _nameFormIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of object class definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of object class definitions.
        /// </returns>
        public IEnumerable<LdapObjectClassSchema> ObjectClassSchemas => _objectClassIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of syntax definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of syntax definitions.
        /// </returns>
        public IEnumerable<LdapSchemaElement> SyntaxSchemas => _syntaxIdTable.Values;

        /// <summary>
        ///     Returns an enumeration of attribute names.
        /// </summary>
        /// <returns>
        ///     An enumeration of attribute names.
        /// </returns>
        public IEnumerable<string> AttributeNames => _attributeNameTable.Keys;

        /// <summary>
        ///     Returns an enumeration of DIT content rule names.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT content rule names.
        /// </returns>
        public IEnumerable<string> DitContentRuleNames => _ditcontentNameTable.Keys;

        /// <summary>
        ///     Returns an enumeration of DIT structure rule names.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT structure rule names.
        /// </returns>
        public IEnumerable<string> DitStructureRuleNames => _ditstructureNameTable.Keys;

        /// <summary>
        ///     Returns an enumeration of matching rule names.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule names.
        /// </returns>
        public IEnumerable<string> MatchingRuleNames => _matchingNameTable.Keys;

        /// <summary>
        ///     Returns an enumeration of matching rule use names.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule use names.
        /// </returns>
        public IEnumerable<string> MatchingRuleUseNames => _matchingUseNameTable.Keys;

        /// <summary>
        ///     Returns an enumeration of name form names.
        /// </summary>
        /// <returns>
        ///     An enumeration of name form names.
        /// </returns>
        public IEnumerable<string> NameFormNames => _nameFormNameTable.Keys;

        /// <summary>
        ///     Returns an enumeration of object class names.
        /// </summary>
        /// <returns>
        ///     An enumeration of object class names.
        /// </returns>
        public IEnumerable<string> ObjectClassNames => _objectClassNameTable.Keys;

        /// <summary>
        ///     Adds the schema definition to the idTable and nameTable Dictionary.
        ///     This method is used by the methods fetchSchema and add.
        ///     Note that the nameTable has all keys cast to Upper-case.  This is so we
        ///     can have a case-insensitive Dictionary. The getXXX (String key) methods
        ///     will also cast to uppercase.
        /// </summary>
        /// <param name="idTable">
        ///     Table to store the element by Id.
        /// </param>
        /// <param name="nameTable">
        ///     Table to store the element by Name(s).
        /// </param>
        /// <param name="element">
        ///     Schema element definition.
        /// </param>
        private void AddElement<T>(Dictionary<string, T> idTable, Dictionary<string, T> nameTable, T element)
            where T : LdapSchemaElement
        {
            idTable[element.Id] = element;
            var names = element.GetNames();
            foreach (var name in names)
            {
                nameTable[name.ToUpper(CultureInfo.InvariantCulture)] = element;
            }
        }

        // #######################################################################
        //   The following methods retrieve a SchemaElement given a Key name:
        // #######################################################################

        /// <summary>
        ///     This function abstracts retrieving LdapSchemaElements from the local
        ///     copy of schema in this LdapSchema class.  This is used by.
        ///     <code>getXXX(String name)</code> functions.
        ///     Note that the nameTable has all keys cast to Upper-case.  This is so
        ///     we can have a case-insensitive HashMap.  The getXXX (String key)
        ///     methods will also cast to uppercase.
        ///     The first character of a NAME string can only be an alpha character
        ///     (see section 4.1 of rfc2252) Thus if the first character is a digit we
        ///     can conclude it is an OID.  Note that this digit is ASCII only.
        /// </summary>
        /// <param name="idTable">
        ///     Table to store the element by Id.
        /// </param>
        /// <param name="nameTable">
        ///     Table to store the element by Name(s).
        /// </param>
        /// <param name="key">
        ///     The key can be either an OID or a name string.
        /// </param>
        private T GetSchemaElement<T>(Dictionary<string, T> idTable, Dictionary<string, T> nameTable, string key)
            where T : LdapSchemaElement
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var c = key[0];
            if (c >= '0' && c <= '9')
            {
                // oid lookup
                return idTable[key];
            }

            // name lookup
            return nameTable[key.ToUpper(CultureInfo.InvariantCulture)];
        }

        /// <summary>
        ///     Returns a particular attribute definition, or null if not found.
        /// </summary>
        /// <param name="name">
        ///     Name or OID of the attribute for which a definition is
        ///     to be returned.
        /// </param>
        /// <returns>
        ///     The attribute definition, or null if not found.
        /// </returns>
        public LdapAttributeSchema GetAttributeSchema(string name)
        {
            return GetSchemaElement(_attributeIdTable, _attributeNameTable, name);
        }

        /// <summary>
        ///     Returns a particular DIT content rule definition, or null if not found.
        /// </summary>
        /// <param name="name">
        ///     The name of the DIT content rule use for which a
        ///     definition is to be returned.
        /// </param>
        /// <returns>
        ///     The DIT content rule definition, or null if not found.
        /// </returns>
        public LdapDitContentRuleSchema GetDitContentRuleSchema(string name)
        {
            return GetSchemaElement(_ditcontentIdTable, _ditcontentNameTable, name);
        }

        /// <summary>
        ///     Returns a particular DIT structure rule definition, or null if not found.
        /// </summary>
        /// <param name="name">
        ///     The name of the DIT structure rule use for which a
        ///     definition is to be returned.
        /// </param>
        /// <returns>
        ///     The DIT structure rule definition, or null if not found.
        /// </returns>
        public LdapDitStructureRuleSchema GetDitStructureRuleSchema(string name)
        {
            return GetSchemaElement(_ditstructureIdTable, _ditstructureNameTable, name);
        }

        /// <summary>
        ///     Returns a particular DIT structure rule definition, or null if not found.
        /// </summary>
        /// <param name="id">
        ///     The ID of the DIT structure rule use for which a
        ///     definition is to be returned.
        /// </param>
        /// <returns>
        ///     The DIT structure rule definition, or null if not found.
        /// </returns>
        public LdapDitStructureRuleSchema GetDitStructureRuleSchema(int id)
        {
            return _ditstructureIdTable[id.ToString()];
        }

        /// <summary>
        ///     Returns a particular matching rule definition, or null if not found.
        /// </summary>
        /// <param name="name">
        ///     The name of the matching rule for which a definition
        ///     is to be returned.
        /// </param>
        /// <returns>
        ///     The matching rule definition, or null if not found.
        /// </returns>
        public LdapMatchingRuleSchema GetMatchingRuleSchema(string name)
        {
            return GetSchemaElement(_matchingIdTable, _matchingNameTable, name);
        }

        /// <summary>
        ///     Returns a particular matching rule use definition, or null if not found.
        /// </summary>
        /// <param name="name">
        ///     The name of the matching rule use for which a definition
        ///     is to be returned.
        /// </param>
        /// <returns>
        ///     The matching rule use definition, or null if not found.
        /// </returns>
        public LdapMatchingRuleUseSchema GetMatchingRuleUseSchema(string name)
        {
            return GetSchemaElement(_matchingUseIdTable, _matchingUseNameTable, name);
        }

        /// <summary>
        ///     Returns a particular name form definition, or null if not found.
        /// </summary>
        /// <param name="name">
        ///     The name of the name form for which a definition
        ///     is to be returned.
        /// </param>
        /// <returns>
        ///     The name form definition, or null if not found.
        /// </returns>
        public LdapNameFormSchema GetNameFormSchema(string name)
        {
            return GetSchemaElement(_nameFormIdTable, _nameFormNameTable, name);
        }

        /// <summary>
        ///     Returns a particular object class definition, or null if not found.
        /// </summary>
        /// <param name="name">
        ///     The name or OID of the object class for which a
        ///     definition is to be returned.
        /// </param>
        /// <returns>
        ///     The object class definition, or null if not found.
        /// </returns>
        public LdapObjectClassSchema GetObjectClassSchema(string name)
        {
            return GetSchemaElement(_objectClassIdTable, _objectClassNameTable, name);
        }

        /// <summary>
        ///     Returns a particular syntax definition, or null if not found.
        /// </summary>
        /// <param name="oid">
        ///     The oid of the syntax for which a definition
        ///     is to be returned.
        /// </param>
        /// <returns>
        ///     The syntax definition, or null if not found.
        /// </returns>
        public LdapSyntaxSchema GetSyntaxSchema(string oid)
        {
            return GetSchemaElement(_syntaxIdTable, _syntaxNameTable, oid);
        }
    }
}
