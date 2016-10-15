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
// Samples.Controls.SimplePassword.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//


/*  
 *  The sample SimplePassword.cs shows how to set the simple password 
 *  of an entry.  
 *  
 *  The simple password is set by modifying the 'userpassword'
 *  attribute and attaching the Simple Password control to
 *  the Ldap add or modify operation.
 *  
 *  The purpose of the Simple Password is to allow migration 
 *  of an object with a hashed password into eDirectory.
 *  The object may then be accessed using the same password as 
 *  in the orginal system.
 *   
 */


using System;

using Novell.Directory.Ldap;

public class SimplePassword 
{

	private static String  simplePassOID = "2.16.840.1.113719.1.27.101.5";
  
	public static void Main( String[] args )
	{
		if (args.Length != 6) 
		{
			Console.Error.WriteLine("Usage:   mono SimplePassword <host Name> "
				+ "<port number> <login dn> <password> <user dn>"
				+ " <new user password>");
			Console.Error.WriteLine("\n Example: mono SimplePassword Acme.com 389"
				+ " \"cn=Admin,o=Acme\" secret\n"
				+ "         \"cn=JSmith,ou=sales,o=Acme\" userPWD");
			Environment.Exit(1);
		}

		int    ldapVersion = LdapConnection.Ldap_V3;
		String ldapHost    = args[0];
		int    ldapPort    = int.Parse(args[1]);
		String loginDN     = args[2];
		String password    = args[3];
		String userDN      = args[4];
		String userPWD     = args[5];


		/* Simple Password control.  There is no value  associated with this control,
		 * just an OID and criticality. Setting the criticality to TRUE means the
		 * server will return an error if it does not recognize or is unable to
		 * perform the control. 
		 */

		LdapControl cont = new LdapControl(simplePassOID,
			true,
			null);  
		LdapConstraints lcons = new LdapConstraints();
		lcons.setControls(cont);
        
		LdapConnection lc  = new LdapConnection();

		try 
		{
			// connect to the server
			lc.Connect( ldapHost, ldapPort );
			// bind to the server
			lc.Bind( ldapVersion, loginDN, password );

			//  Modify the 'userpassword' attribute, with the Simple
			// Password control.
			LdapModification[] modifications = new LdapModification[1];             
			LdapAttribute sPassword = new LdapAttribute( "userPassword",userPWD);
			modifications[0] =
				new LdapModification( LdapModification.REPLACE, sPassword);

			lc.Modify( userDN, modifications,lcons);

			Console.WriteLine("Your Simple password has been modified.");

			lc.Disconnect();
		}
		catch( LdapException e ) 
		{
			Console.Error.WriteLine("SimplePassword example failed");
			Console.Error.WriteLine( "Error: " + e.ToString() );
			Environment.Exit(1);
		}
		catch( Exception e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
		}
		Environment.Exit(0);
	}
}            
