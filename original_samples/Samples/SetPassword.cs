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
// Samples.SetPassword.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

 /*
 *   The SetPassword.cs sample shows how to set the password
 *   of an entry by setting the userPassword attribute
 *   of the entry.
 *
 *   In Novell eDirectory, only an admin can set a password
 *   without supplying the old password.  Consequently this
 *   method works on any Novell Ldap server, but only when the
 *   caller has admin privileges. 
 */


using System;
using Novell.Directory.Ldap;

public class SetPassword 
{
	public static void Main( String[] args ) 
	{        
		if (args.Length != 5) 
		{
			Console.Error.WriteLine("Usage:   mono SetPassword <host name> "
				+ "<login dn> <password>\n"
				+ "         <modify dn> <new password>");
			Console.Error.WriteLine("Example: mono SetPassword Acme.com "
				+ "\"cn=Admin,o=Acme secret\"\n"
				+ "         \"cn=JSmith,ou=Sales,o=Acme\"" 
				+ " newPassword");
			Environment.Exit(1);
		}

		int ldapPort = LdapConnection.DEFAULT_PORT;
		int ldapVersion = LdapConnection.Ldap_V3;        
		String ldapHost = args[0];
		String loginDN = args[1];
		String password = args[2];
		String modifyDN = args[3];
		String newPassword = args[4];
		LdapConnection lc = new LdapConnection();

		/* To set a user's password,
		  *   -- User should have administrator privileges
		  *   -- Specify the new password value to be set
		  *   -- Specify the modify type (replace for this operation)
		  *   -- Add the new value and type to the modification set
		  *   -- Call LdapConnection modify method to set the password
		  */

		try 
		{
			// connect to the server
			lc.Connect( ldapHost, ldapPort );
			// authenticate to the server
			lc.Bind( ldapVersion, loginDN, password );

			LdapAttribute attributePassword = new LdapAttribute( "userPassword",
				newPassword);
			lc.Modify( modifyDN, new LdapModification(
				LdapModification.REPLACE, attributePassword) );

			Console.WriteLine( "Successfully set the user's password" );

			// disconnect with the server
			lc.Disconnect();
		}
		catch( LdapException e ) 
		{
			if ( e.ResultCode == LdapException.NO_SUCH_OBJECT ) 
			{
				Console.Error.WriteLine( "Error: No such entry" );
			} 
			else if ( e.ResultCode ==
				LdapException.INSUFFICIENT_ACCESS_RIGHTS ) 
			{
				Console.Error.WriteLine( "Error: Insufficient rights" );
			} 
			else 
			{
				Console.Error.WriteLine( "Error: " + e.ToString() );
			}        
		}
		catch( Exception e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
		}
		Environment.Exit(0);
	}
}
