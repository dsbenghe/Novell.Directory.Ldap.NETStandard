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
// Novell.Directory.Ldap.LdapSchema.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using Novell.Directory.Ldap.Utilclass;

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
    ///     retrieve schema elements from LdapSchema
    ///     <pre>
    ///         <code>
    /// .
    /// .
    /// .
    /// LdapSchema schema;
    /// LdapSchemaElement element;
    /// 
    /// // connect to the server
    /// lc.connect( ldapHost, ldapPort );
    /// lc.bind( ldapVersion, loginDN, password );
    /// 
    /// // read the schema from the directory
    /// schema = lc.fetchSchema( lc.getSchemaDN() );
    /// 
    /// // retrieve the definition of common name
    /// element = schema.getAttributeSchema( "cn" );
    /// System.out.println("The attribute cn has an oid of " + element.getID());
    /// .
    /// .
    /// .
    /// </code>
    ///     </pre>
    /// </summary>
    /// <seealso cref="LdapSchemaElement">
    /// </seealso>
    /// <seealso cref="LdapConnection.FetchSchema">
    /// </seealso>
    /// <seealso cref="LdapConnection.GetSchemaDN">
    /// </seealso>
    public class LdapSchema : LdapEntry
    {
        private void InitBlock()
        {
            _nameTable = new Hashtable[8];
            _idTable = new Hashtable[8];
        }

        /// <summary>
        ///     Returns an enumeration of attribute definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of attribute definitions.
        /// </returns>
        public IEnumerator AttributeSchemas => new EnumeratedIterator(_idTable[Attribute].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of DIT content rule definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT content rule definitions.
        /// </returns>
        public IEnumerator DitContentRuleSchemas => new EnumeratedIterator(_idTable[Ditcontent].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of DIT structure rule definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT structure rule definitions.
        /// </returns>
        public IEnumerator DitStructureRuleSchemas => new EnumeratedIterator(_idTable[Ditstructure].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of matching rule definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule definitions.
        /// </returns>
        public IEnumerator MatchingRuleSchemas => new EnumeratedIterator(_idTable[Matching].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of matching rule use definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule use definitions.
        /// </returns>
        public IEnumerator MatchingRuleUseSchemas => new EnumeratedIterator(_idTable[MatchingUse].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of name form definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of name form definitions.
        /// </returns>
        public IEnumerator NameFormSchemas => new EnumeratedIterator(_idTable[NameForm].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of object class definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of object class definitions.
        /// </returns>
        public IEnumerator ObjectClassSchemas => new EnumeratedIterator(_idTable[ObjectClass].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of syntax definitions.
        /// </summary>
        /// <returns>
        ///     An enumeration of syntax definitions.
        /// </returns>
        public IEnumerator SyntaxSchemas => new EnumeratedIterator(_idTable[Syntax].Values.GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of attribute names.
        /// </summary>
        /// <returns>
        ///     An enumeration of attribute names.
        /// </returns>
        public IEnumerator AttributeNames => new EnumeratedIterator(new SupportClass.SetSupport(_nameTable[Attribute].Keys).GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of DIT content rule names.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT content rule names.
        /// </returns>
        public IEnumerator DitContentRuleNames => new EnumeratedIterator(new SupportClass.SetSupport(_nameTable[Ditcontent].Keys).GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of DIT structure rule names.
        /// </summary>
        /// <returns>
        ///     An enumeration of DIT structure rule names.
        /// </returns>
        public IEnumerator DitStructureRuleNames => new EnumeratedIterator(new SupportClass.SetSupport(_nameTable[Ditstructure].Keys).GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of matching rule names.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule names.
        /// </returns>
        public IEnumerator MatchingRuleNames => new EnumeratedIterator(new SupportClass.SetSupport(_nameTable[Matching].Keys).GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of matching rule use names.
        /// </summary>
        /// <returns>
        ///     An enumeration of matching rule use names.
        /// </returns>
        public IEnumerator MatchingRuleUseNames => new EnumeratedIterator(new SupportClass.SetSupport(_nameTable[MatchingUse].Keys).GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of name form names.
        /// </summary>
        /// <returns>
        ///     An enumeration of name form names.
        /// </returns>
        public IEnumerator NameFormNames => new EnumeratedIterator(new SupportClass.SetSupport(_nameTable[NameForm].Keys).GetEnumerator());

        /// <summary>
        ///     Returns an enumeration of object class names.
        /// </summary>
        /// <returns>
        ///     An enumeration of object class names.
        /// </returns>
        public IEnumerator ObjectClassNames => new EnumeratedIterator(new SupportClass.SetSupport(_nameTable[ObjectClass].Keys).GetEnumerator());

        /// <summary>
        ///     The idTable hash on the oid (or integer ID for DITStructureRule) and
        ///     is used for retrieving enumerations
        /// </summary>
        private Hashtable[] _idTable;

        /// <summary>
        ///     The nameTable will hash on the names (if available). To insure
        ///     case-insensibility, the Keys for this table will be a String cast to
        ///     Uppercase.
        /// </summary>
        private Hashtable[] _nameTable;

        /*package*/

        /// <summary>
        ///     The following lists the Ldap names of subschema attributes for
        ///     schema elements (definitions):
        /// </summary>
        internal static readonly string[] SchemaTypeNames =
        {
            "attributeTypes", "objectClasses", "ldapSyntaxes",
            "nameForms", "dITContentRules", "dITStructureRules", "matchingRules", "matchingRuleUse"
        };

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int Attribute = 0;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int ObjectClass = 1;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int Syntax = 2;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int NameForm = 3;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int Ditcontent = 4;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int Ditstructure = 5;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int Matching = 6;

        /// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
        /*package*/
        internal const int MatchingUse = 7;


        /// <summary>
        ///     Constructs an LdapSchema object from attributes of an LdapEntry.
        ///     The object is empty if the entry parameter contains no schema
        ///     attributes.  The recognized schema attributes are the following:
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
        public LdapSchema(LdapEntry ent) : base(ent.Dn, ent.GetAttributeSet())
        {
            InitBlock();
            //reset all definitions
            for (var i = 0; i < SchemaTypeNames.Length; i++)
            {
                _idTable[i] = new Hashtable();
                _nameTable[i] = new Hashtable();
            }
            var itr = GetAttributeSet().GetEnumerator();
            while (itr.MoveNext())
            {
                var attr = (LdapAttribute) itr.Current;
                string valueRenamed, attrName = attr.Name;
                var enumString = attr.StringValues;

                if (attrName.ToUpper().Equals(SchemaTypeNames[ObjectClass].ToUpper()))
                {
                    LdapObjectClassSchema classSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        try
                        {
                            classSchema = new LdapObjectClassSchema(valueRenamed);
                        }
                        catch (Exception e)
                        {
                            Logger.Log.LogWarning("Exception swallowed", e);
                            continue; //Error parsing: do not add this definition
                        }
                        AddElement(ObjectClass, classSchema);
                    }
                }
                else if (attrName.ToUpper().Equals(SchemaTypeNames[Attribute].ToUpper()))
                {
                    LdapAttributeSchema attrSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        try
                        {
                            attrSchema = new LdapAttributeSchema(valueRenamed);
                        }
                        catch (Exception e)
                        {
                            Logger.Log.LogWarning("Exception swallowed", e);
                            continue; //Error parsing: do not add this definition
                        }
                        AddElement(Attribute, attrSchema);
                    }
                }
                else if (attrName.ToUpper().Equals(SchemaTypeNames[Syntax].ToUpper()))
                {
                    LdapSyntaxSchema syntaxSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        syntaxSchema = new LdapSyntaxSchema(valueRenamed);
                        AddElement(Syntax, syntaxSchema);
                    }
                }
                else if (attrName.ToUpper().Equals(SchemaTypeNames[Matching].ToUpper()))
                {
                    LdapMatchingRuleSchema matchingRuleSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        matchingRuleSchema = new LdapMatchingRuleSchema(valueRenamed, null);
                        AddElement(Matching, matchingRuleSchema);
                    }
                }
                else if (attrName.ToUpper().Equals(SchemaTypeNames[MatchingUse].ToUpper()))
                {
                    LdapMatchingRuleUseSchema matchingRuleUseSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        matchingRuleUseSchema = new LdapMatchingRuleUseSchema(valueRenamed);
                        AddElement(MatchingUse, matchingRuleUseSchema);
                    }
                }
                else if (attrName.ToUpper().Equals(SchemaTypeNames[Ditcontent].ToUpper()))
                {
                    LdapDitContentRuleSchema dItContentRuleSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        dItContentRuleSchema = new LdapDitContentRuleSchema(valueRenamed);
                        AddElement(Ditcontent, dItContentRuleSchema);
                    }
                }
                else if (attrName.ToUpper().Equals(SchemaTypeNames[Ditstructure].ToUpper()))
                {
                    LdapDitStructureRuleSchema dItStructureRuleSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        dItStructureRuleSchema = new LdapDitStructureRuleSchema(valueRenamed);
                        AddElement(Ditstructure, dItStructureRuleSchema);
                    }
                }
                else if (attrName.ToUpper().Equals(SchemaTypeNames[NameForm].ToUpper()))
                {
                    LdapNameFormSchema nameFormSchema;
                    while (enumString.MoveNext())
                    {
                        valueRenamed = (string) enumString.Current;
                        nameFormSchema = new LdapNameFormSchema(valueRenamed);
                        AddElement(NameForm, nameFormSchema);
                    }
                }
                //All non schema attributes are ignored.
            }
        }

        /// <summary>
        ///     Adds the schema definition to the idList and nameList HashMaps.
        ///     This method is used by the methods fetchSchema and add.
        ///     Note that the nameTable has all keys cast to Upper-case.  This is so we
        ///     can have a case-insensitive HashMap.  The getXXX (String key) methods
        ///     will also cast to uppercase.
        /// </summary>
        /// <param name="schemaType">
        ///     Type of schema definition, use one of the final
        ///     integers defined at the top of this class:
        ///     ATTRIBUTE, OBJECT_CLASS, SYNTAX, NAME_FORM,
        ///     DITCONTENT, DITSTRUCTURE, MATCHING, MATCHING_USE
        /// </param>
        /// <param name="element">
        ///     Schema element definition.
        /// </param>
        private void AddElement(int schemaType, LdapSchemaElement element)
        {
            SupportClass.PutElement(_idTable[schemaType], element.Id, element);
            var names = element.Names;
            for (var i = 0; i < names.Length; i++)
            {
                SupportClass.PutElement(_nameTable[schemaType], names[i].ToUpper(), element);
            }
        }

        // #######################################################################
        //   The following methods retrieve a SchemaElement given a Key name:
        // #######################################################################

        /// <summary>
        ///     This function abstracts retrieving LdapSchemaElements from the local
        ///     copy of schema in this LdapSchema class.  This is used by
        ///     <code>getXXX(String name)</code> functions.
        ///     Note that the nameTable has all keys cast to Upper-case.  This is so
        ///     we can have a case-insensitive HashMap.  The getXXX (String key)
        ///     methods will also cast to uppercase.
        ///     The first character of a NAME string can only be an alpha character
        ///     (see section 4.1 of rfc2252) Thus if the first character is a digit we
        ///     can conclude it is an OID.  Note that this digit is ASCII only.
        /// </summary>
        /// <param name="schemaType">
        ///     Specifies which list is to be used in schema
        ///     lookup.
        /// </param>
        /// <param name="key">
        ///     The key can be either an OID or a name string.
        /// </param>
        private LdapSchemaElement GetSchemaElement(int schemaType, string key)
        {
            if ((object) key == null || key.ToUpper().Equals("".ToUpper()))
                return null;
            var c = key[0];
            if (c >= '0' && c <= '9')
            {
                //oid lookup
                return (LdapSchemaElement) _idTable[schemaType][key];
            }
            //name lookup
            return (LdapSchemaElement) _nameTable[schemaType][key.ToUpper()];
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
            return (LdapAttributeSchema) GetSchemaElement(Attribute, name);
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
            return (LdapDitContentRuleSchema) GetSchemaElement(Ditcontent, name);
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
            return (LdapDitStructureRuleSchema) GetSchemaElement(Ditstructure, name);
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
            var idKey = id;
            return (LdapDitStructureRuleSchema) _idTable[Ditstructure][idKey];
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
            return (LdapMatchingRuleSchema) GetSchemaElement(Matching, name);
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
            return (LdapMatchingRuleUseSchema) GetSchemaElement(MatchingUse, name);
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
            return (LdapNameFormSchema) GetSchemaElement(NameForm, name);
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
            return (LdapObjectClassSchema) GetSchemaElement(ObjectClass, name);
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
            return (LdapSyntaxSchema) GetSchemaElement(Syntax, oid);
        }

        // ########################################################################
        // The following methods return an Enumeration of SchemaElements by schema type
        // ########################################################################

        // #######################################################################
        //  The following methods retrieve an Enumeration of Names of a schema type
        // #######################################################################


        /// <summary>
        ///     This helper function returns a number that represents the type of schema
        ///     definition the element represents.  The top of this file enumerates
        ///     these types.
        /// </summary>
        /// <param name="element">
        ///     A class extending LdapSchemaElement.
        /// </param>
        /// <returns>
        ///     a Number that identifies the type of schema element and
        ///     will be one of the following:
        ///     ATTRIBUTE, OBJECT_CLASS, SYNTAX, NAME_FORM,
        ///     DITCONTENT, DITSTRUCTURE, MATCHING, MATCHING_USE
        /// </returns>
        private int GetType(LdapSchemaElement element)
        {
            if (element is LdapAttributeSchema)
                return Attribute;
            if (element is LdapObjectClassSchema)
                return ObjectClass;
            if (element is LdapSyntaxSchema)
                return Syntax;
            if (element is LdapNameFormSchema)
                return NameForm;
            if (element is LdapMatchingRuleSchema)
                return Matching;
            if (element is LdapMatchingRuleUseSchema)
                return MatchingUse;
            if (element is LdapDitContentRuleSchema)
                return Ditcontent;
            if (element is LdapDitStructureRuleSchema)
                return Ditstructure;
            throw new ArgumentException("The specified schema element type is not recognized");
        }
    }
}