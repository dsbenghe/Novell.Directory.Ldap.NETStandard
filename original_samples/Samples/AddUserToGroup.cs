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
// Samples.AddUserToGroup.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
/// Description: The AddUserToGroup sample shows how to add a user to a group
/// on Novell eDirectory. This includes four attribute
/// modification steps:
/// 1. add group's dn to user's groupMemberShip attribute.
/// 2. add group's dn to user's securityEquals attribute.
/// 3. add user's dn to group's uniqueMember attribute.
/// 4. add user's dn to group's equivalentToMe attribute.
/// After the modifications, the security privileges that are
/// granted to the group are now inherited by the user. 
/// ****************************************************************************
using System;
using Novell.Directory.Ldap;

public class AddUserToGroup
{
	[STAThread]
	public static void  Main(System.String[] args)
	{
		
		if (args.Length != 5)
		{
			usage();
			System.Environment.Exit(1);
		}
		
		int ldapPort = LdapConnection.DEFAULT_PORT;
		int ldapVersion = LdapConnection.Ldap_V3;
		bool status = false;
		LdapConnection conn = new LdapConnection();
		System.String ldapHost = args[0];
		System.String loginDN = args[1];
		System.String password = args[2];
		System.String userDN = args[3];
		System.String groupDN = args[4];
		
		try
		{
			// connect to the server
			conn.Connect(ldapHost, ldapPort);
			// bind to the server
			conn.Bind(ldapVersion, loginDN, password);
			
			// call _AddUseToGroup() to add the user to the group
			status = _AddUserToGroup(conn, userDN, groupDN);
			
			if (status)
				System.Console.Out.WriteLine("User: " + userDN + " was enrolled in group: " + groupDN);
			else
				System.Console.Out.WriteLine("User: " + userDN + " could not be enrolled in group: " + groupDN);
			
			// disconnect with the server
			conn.Disconnect();
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine("Error: " + e.LdapErrorMessage);
		}
		catch(Exception e)
		{
			Console.WriteLine("Error:" + e.Message);
			return;
		}
		System.Environment.Exit(0);
	}
	
	public static void  usage()
	{
		System.Console.Error.WriteLine("Usage:   mono AddUserToGroup <ldap host>" + " <login dn> <password> <user dn >\n" + "              <group dn>");
		System.Console.Error.WriteLine("Example: mono AddUserToGroup Acme.com" + " \"cn=Admin,o=Acme\" secret\n" + "              \"cn=James,ou=Sales,o=Acme\"" + " \"cn=salesGroup,ou=Sales,o=Acme\"");
	}
	
	public static bool _AddUserToGroup(LdapConnection conn, System.String userdn, System.String groupdn)
	{
		
		// modifications for group and user
		LdapModification[] modGroup = new LdapModification[2];
		LdapModification[] modUser = new LdapModification[2];
		
		// Add modifications to modUser
		LdapAttribute membership = new LdapAttribute("groupMembership", groupdn);
		modUser[0] = new LdapModification(LdapModification.ADD, membership);
		LdapAttribute security = new LdapAttribute("securityEquals", groupdn);
		modUser[1] = new LdapModification(LdapModification.ADD, security);
		
		// Add modifications to modGroup
		LdapAttribute member = new LdapAttribute("uniqueMember", userdn);
		modGroup[0] = new LdapModification(LdapModification.ADD, member);
		LdapAttribute equivalent = new LdapAttribute("equivalentToMe", userdn);
		modGroup[1] = new LdapModification(LdapModification.ADD, equivalent);
		
		try
		{
			// Modify the user's attributes
			conn.Modify(userdn, modUser);
			System.Console.Out.WriteLine("Modified the user's attribute.");
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine("Failed to modify user's attributes: " + e.LdapErrorMessage);
			return false;
		}
		
		try
		{
			// Modify the group's attributes
			conn.Modify(groupdn, modGroup);
			System.Console.Out.WriteLine("Modified the group's attribute.");
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine("Failed to modify group's attributes: " + e.LdapErrorMessage);
			doCleanup(conn, userdn, groupdn);
			return false;
		}
		catch(Exception e)
		{
			Console.WriteLine("Error:" + e.Message);
			return false;
		}
		return true;
	}
	
	public static void  doCleanup(LdapConnection conn, System.String userdn, System.String groupdn)
	{
		// since we have modified the user's attributes and failed to
		// modify the group's attribute, we need to delete the modified
		// user's attribute values.
		
		// modifications for user
		LdapModification[] modUser = new LdapModification[2];
		
		// Delete the groupdn from the user's attributes
		LdapAttribute membership = new LdapAttribute("groupMembership", groupdn);
		modUser[0] = new LdapModification(LdapModification.DELETE, membership);
		LdapAttribute security = new LdapAttribute("securityEquals", groupdn);
		modUser[1] = new LdapModification(LdapModification.DELETE, security);
		
		try
		{
			// Modify the user's attributes
			conn.Modify(userdn, modUser);
			
			System.Console.Out.WriteLine("Deleted the modified user's attribute values.");
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine("Could not delete modified user's attributes: " + e.LdapErrorMessage);
		}
		catch(Exception e)
		{
			Console.WriteLine("Error:" + e.Message);
			return;
		}

		return ;
	}
}
