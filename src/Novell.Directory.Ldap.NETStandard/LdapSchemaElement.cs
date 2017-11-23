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
// Novell.Directory.Ldap.LdapSchemaElement.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using Novell.Directory.Ldap.Utilclass;
using System.Collections.Generic;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The LdapSchemaElement class is the base class representing schema
    ///     elements (definitions) in Ldap.
    ///     An LdapSchemaElement is read-only, single-valued LdapAttribute.
    ///     Therefore, it does not support the addValue and removeValue methods from
    ///     LdapAttribute.  This class overrides those methods and throws
    ///     <code>UnsupportedOperationException</code> if either of those methods are
    ///     invoked by an application.
    /// </summary>
    /// <seealso cref="LdapSchema">
    /// </seealso>
    /// <seealso cref="LdapConnection.FetchSchema">
    /// </seealso>
    public abstract class LdapSchemaElement : LdapAttribute
    {

        /// <summary>
        ///     Returns an array of names for the element, or null if
        ///     none is found.
        ///     The getNames method accesses the NAME qualifier (from the BNF
        ///     descriptions of Ldap schema definitions). The array consists of all
        ///     values of the NAME qualifier.
        /// </summary>
        /// <returns>
        ///     An array of names for the element, or null if none
        ///     is found.
        /// </returns>
        public virtual string[] Names { get; protected internal set; } = { "" };


        /// <summary>
        ///     Returns the description of the element.
        ///     The getDescription method returns the value of the DESC qualifier
        ///     (from the BNF descriptions of Ldap schema definitions).
        /// </summary>
        /// <returns>
        ///     The description of the element.
        /// </returns>
        public virtual string Description { get; protected internal set; }

        /// <summary>
        ///     Returns the unique object identifier (OID) of the element.
        /// </summary>
        /// <returns>
        ///     The OID of the element.
        /// </returns>
        public virtual string Id { get; protected internal set; } = string.Empty;

        /// <summary>
        ///     Returns an enumeration of all qualifiers of the element which are
        ///     vendor specific (begin with "X-").
        /// </summary>
        /// <returns>
        ///     An enumeration of all qualifiers of the element.
        /// </returns>
        public virtual IEnumerable<string> QualifierNames => HashQualifier.Keys;

        /// <summary>
        ///     Returns whether the element has the OBSOLETE qualifier
        ///     in its Ldap definition.
        /// </summary>
        /// <returns>
        ///     True if the Ldap definition contains the OBSOLETE qualifier;
        ///     false if OBSOLETE qualifier is not present.
        /// </returns>
        public virtual bool Obsolete { get; protected internal set; }

        /// <summary>
        ///     Creates an LdapSchemaElement by setting the name of the LdapAttribute.
        ///     Because this is the only constructor, all extended classes are expected
        ///     to call this constructor.  The value of the LdapAttribute must be set
        ///     by the setValue method.
        /// </summary>
        /// <param name="attrName">
        ///     The attribute name of the schema definition. Valid
        ///     names are one of the following:
        ///     "attributeTypes", "objectClasses", "ldapSyntaxes",
        ///     "nameForms", "dITContentRules", "dITStructureRules",
        ///     "matchingRules", or "matchingRuleUse"
        /// </param>
        protected internal LdapSchemaElement(string attrName) : base(attrName)
        {
            
        }



        /// <summary>
        ///     A string array of optional, or vendor-specific, qualifiers for the
        ///     schema element.
        ///     These optional qualifiers begin with "X-"; the Novell eDirectory
        ///     specific qualifiers begin with "X-NDS".
        /// </summary>
        protected internal string[] qualifier = { "" };

        /// <summary>
        ///     A hash table that contains the vendor-specific qualifiers (for example,
        ///     the X-NDS flags).
        /// </summary>
        private IDictionary<string, AttributeQualifier> HashQualifier { get; set; } = new Dictionary<string, AttributeQualifier>();

        /// <summary>
        ///     Returns an array of all values of a specified optional or non-
        ///     standard qualifier of the element.
        ///     The getQualifier method may be used to access the values of
        ///     vendor-specific qualifiers (which begin with "X-").
        /// </summary>
        /// <param name="name">
        ///     The name of the qualifier, case-sensitive.
        /// </param>
        /// <returns>
        ///     An array of values for the specified non-standard qualifier.
        /// </returns>
        public virtual string[] GetQualifier(string name)
        {
            if (HashQualifier.ContainsKey(name))
            {
                return HashQualifier[name].Values;
            }
            return null;
        }

        /// <summary>
        ///     Returns a string in a format suitable for directly adding to a directory,
        ///     as a value of the particular schema element.
        /// </summary>
        /// <returns>
        ///     A string that can be used to add the element to the directory.
        /// </returns>
        public override string ToString() => FormatString();

        /// <summary>
        ///     Implementations of formatString format a schema element into a string
        ///     suitable for using in a modify (ADD) operation to the directory.
        ///     ToString uses this method.  This method is needed because a call to
        ///     setQualifier requires reconstructing the string value of the schema
        ///     element.
        /// </summary>
        protected internal abstract string FormatString();

        /// <summary>
        ///     Sets the values of a specified optional or non-standard qualifier of
        ///     the element.
        ///     The setQualifier method is used to set the values of vendor-
        ///     specific qualifiers (which begin with "X-").
        /// </summary>
        /// <param name="name">
        ///     The name of the qualifier, case-sensitive.
        /// </param>
        /// <param name="values">
        ///     The values to set for the qualifier.
        /// </param>
        public virtual void SetQualifier(string name, string[] values)
        {
            var attrQualifier = new AttributeQualifier(name, values);
            if (HashQualifier.ContainsKey(name))
                HashQualifier[name] = attrQualifier;
            else
                HashQualifier.Add(name, attrQualifier);
            Value = FormatString();
        }

        /// <summary>
        ///     LdapSchemaElement is read-only and this method is over-ridden to
        ///     throw an exception.
        ///     @throws UnsupportedOperationException always thrown since
        ///     LdapSchemaElement is read-only
        /// </summary>
        public override void AddValue(string value)
        {
            throw new NotSupportedException("addValue is not supported by LdapSchemaElement");
        }

        /// <summary>
        ///     LdapSchemaElement is read-only and this method is over-ridden to
        ///     throw an exception.
        ///     @throws UnsupportedOperationException always thrown since
        ///     LdapSchemaElement is read-only
        /// </summary>
        public virtual void AddValue(byte[] value)
        {
            throw new NotSupportedException("addValue is not supported by LdapSchemaElement");
        }

        /// <summary>
        ///     LdapSchemaElement is read-only and this method is over-ridden to
        ///     throw an exception.
        ///     @throws UnsupportedOperationException always thrown since
        ///     LdapSchemaElement is read-only
        /// </summary>
        public override void RemoveValue(string value)
        {
            throw new NotSupportedException("removeValue is not supported by LdapSchemaElement");
        }

        /// <summary>
        ///     LdapSchemaElement is read-only and this method is over-ridden to
        ///     throw an exception.
        ///     @throws UnsupportedOperationException always thrown since
        ///     LdapSchemaElement is read-only
        /// </summary>
        public virtual void RemoveValue(byte[] value)
        {
            throw new NotSupportedException("removeValue is not supported by LdapSchemaElement");
        }
    }
}