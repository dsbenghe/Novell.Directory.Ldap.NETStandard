/******************************************************************************
* The MIT License
* Copyright (c) 2006 Novell Inc.  www.novell.com
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
// Samples.ModifyACL.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

/*
 * ModifyACL.cs first modifies entryDN's ACL values to grant
 * trusteeDN the read, write, and delete entry rights. It then
 * displays entryDN's ACL values. Finally it removes entryDN's
 * modified ACL value.
 *
 * ACL (Access Control List) is a multivalued optional attribute.
 * An entry's ACL values define which other entries (trustees)
 * have what kinds of access to the entry itself and its
 * attributes.
 *
 * Each of the ACL value is in the format of
 *   "privileges#scope#subjectname#protectedattrname".
 *    privileges:        ORed bits to indicate the rights.
 *    scope:             either 'entry' or 'subtree'.
 *    subjectname:       trustee DN.
 *    protectedattrname: [Entry Rights], or [All Attributes Rights],
 *                       or a single attribute name.
 ******************************************************************************/

using System;
using System.Collections;
using System.Text;
using Novell.Directory.Ldap;

public class ModifyACL
{
	public static void Main( String[] args )
	{
		if (args.Length != 6) 
		{
			Console.Error.WriteLine(
				 "Usage:   mono ModifyACL <host name> <port number> <login dn>"
				 + " <password> \n         <entry dn> <trustee dn>");
			Console.Error.WriteLine(
				 "Example: mono ModifyACL Acme.com 389 \"cn=Admin,o=Acme\""
				 + "  secret \n         \"cn=test,ou=Sales,o=Acme\" "
				 + "\"cn=trustee,o=Acme\"");
			Environment.Exit(1);
		}
		int privileges   = 0;
		int ldapVersion  = LdapConnection.Ldap_V3;
		int ldapPort     = System.Convert.ToInt32(args[1]);
		String ldapHost  = args[0];
		String loginDN   = args[2];
		String password  = args[3];
		String entryDN   = args[4];
		String trusteeDN = args[5];

		LdapConnection lc = new LdapConnection();

		// encode ACL value
		privileges |= System.Convert.ToInt32(LdapDSConstants.LDAP_DS_ENTRY_BROWSE);
		privileges |= System.Convert.ToInt32(LdapDSConstants.LDAP_DS_ENTRY_ADD);
		privileges |= System.Convert.ToInt32(LdapDSConstants.LDAP_DS_ENTRY_DELETE);

		String aclValue = System.Convert.ToString(privileges)+ "#" + "entry" + "#"
							+ trusteeDN + "#" + "[Entry Rights]";
	
		try 
		{
			// connect to the server
			lc.Connect( ldapHost, ldapPort );
			// bind to the server
			lc.Bind(ldapVersion, loginDN, password);

			// modify entryDN's ACL attribute			
			Console.WriteLine( "    Entry DN: " + entryDN );
			Console.WriteLine( "    Trustee DN: " + trusteeDN );
			Console.WriteLine( "    Modifying entryDN's ACL value...");

			LdapAttribute acl = new LdapAttribute( "acl", aclValue);
			lc.Modify( entryDN, new LdapModification(LdapModification.ADD, acl));
			Console.WriteLine("    Modified ACL values to grant trusteeDN  the"
						+ "\n      'read', 'write', and 'delete' entry rights.\n");

			// display entryDN's ACL values
			findACLValues(lc, entryDN);

			// remove the Modified entryDN's ACL value
			Console.WriteLine( "\n    Removing the modified ACL value..." );
			lc.Modify( entryDN, new LdapModification(LdapModification.DELETE,acl));
			Console.WriteLine( "    Removed modified ACL value." );

			lc.Disconnect();
		}
		catch( LdapException e ) 
		{
			if ( e.ResultCode == LdapException.NO_SUCH_OBJECT )
				Console.Error.WriteLine( "Error: ModifyACL.java, No such entry" );
			else if ( e.ResultCode == LdapException.INSUFFICIENT_ACCESS_RIGHTS )
				Console.Error.WriteLine("Error: ModifyACL.java, Insufficient rights");
			else if ( e.ResultCode == LdapException.ATTRIBUTE_OR_VALUE_EXISTS )
				Console.Error.WriteLine("Error: ModifyACL.java, Attribute or value "
								+ "exists");
			else 
			{
				Console.WriteLine( "Error: ModifyACL.java, " + e.ToString() );
			}
			Environment.Exit(1);
		}
		catch( Exception e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
		}
		Environment.Exit(0);
	}

	// findACLValues() reads the entry to get it's ACL values
	public static void findACLValues(LdapConnection lc, String entry) 
	{
		String[] returnAttrs = { "acl" };
		String attributeName;
		IEnumerator allValues;
		LdapAttribute attribute;
		LdapAttributeSet attributeSet;

		try 
		{
			LdapEntry aclList = lc.Read( entry, returnAttrs );

			// printout entryDN's ACL values
			attributeSet = aclList.getAttributeSet();
			IEnumerator allAttributes = attributeSet.GetEnumerator();

			Console.WriteLine("    =========================================");
			Console.WriteLine("    entryDN's ACL values after modification:");
			Console.WriteLine("    =========================================");
			if (allAttributes.MoveNext()) 
			{
				attribute = (LdapAttribute)allAttributes.Current;
				attributeName = attribute.Name;
				allValues = attribute.StringValues;
				while(allValues.MoveNext()) 
				{
					PrintACLValue((String)allValues.Current);
				}
			}
		}
		catch( LdapException e ) 
		{
			Console.WriteLine( "Error: ModdifyACL, " + e.ToString() );
			Environment.Exit(1);
		}
	}

	// PrintACLValue() parses and prints the ACLValue
	public static void PrintACLValue( String ACLValue ) 
	{

		int    privileges;
		String scope, trusteeName, protName;

		// ACL value format: "privileges#scope#subjectname#protectedattrname".
		privileges = System.Convert.ToInt32(
						ACLValue.Substring( 0, ACLValue.IndexOf('#')) );
				
		protName = ACLValue.Substring(
			ACLValue.LastIndexOf("#")+1, ACLValue.Length-(ACLValue.LastIndexOf("#")+1));
		
		// truncate ACL value to "scope#subjectname"
		
		ACLValue = ACLValue.Substring(
			ACLValue.IndexOf('#') + 1, ACLValue.LastIndexOf('#')-(ACLValue.IndexOf('#') + 1) );

		scope = ACLValue.Substring( 0, (ACLValue.IndexOf('#')) );
		
		trusteeName = ACLValue.Substring(
			ACLValue.IndexOf('#') + 1, (ACLValue.Length-(ACLValue.IndexOf('#') + 1)) );

		
		StringBuilder privs = new StringBuilder();

		if ( protName.ToUpper().Equals("[Entry Rights]".ToUpper())) 
		{
			// decode [Entry Rights]rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_BROWSE) != 0?"BrowseEntry: true; ":"BrowseEntry: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ENTRY_ADD) != 0?"AddEntry: true; ":"AddEntry: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ENTRY_DELETE) != 0?"DeleteEntry: true; ":"DeleteEntry: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ENTRY_RENAME) != 0?"RenameEntry: true; ":"RenameEntry: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ENTRY_SUPERVISOR) != 0?"Supervisor: true; ":"Supervisor: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ENTRY_INHERIT_CTL) != 0?"Inherit_ctl: true.":"Inherit_ctl: false.");
		}
		else 
		{
			// decode attribute rights no matter it's for 
			// all attributes or a single attribute
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ATTR_COMPARE) != 0?"CompareAttributes: true; ":"CompareAttributes: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ATTR_READ) != 0?"ReadAttributes: true; ":"ReadAttributes: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ATTR_WRITE) != 0?"Write/Add/DeleteAttributes: true; ":"Write/Add/DeleteAttributes: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ATTR_SELF) != 0?"Add/DeleteSelf: true; ":"Add/DeleteSelf: false; ");
			privs.Append((privileges & LdapDSConstants.LDAP_DS_ATTR_SUPERVISOR) != 0?"Supervisor: true.":"Supervisor: false.");
		}

		Console.WriteLine("    Trustee name: " + trusteeName + "\n    scope: "
				   + scope + "\n    Protected attribute name: "
				   + protName + "\n    Privileges: " + privs);
		Console.WriteLine("    ---------------------------------------------");
	}
}
