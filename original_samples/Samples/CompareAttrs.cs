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
// Samples.CompareAttrs.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

/* The CompareAttrs.cs example compares specified attribute values
 * with the entry's attribute values. 
 */

using System;
using Novell.Directory.Ldap;

public class CompareAttrs 
{
	public static void Main( String[] args ) 
	{        
		if (args.Length != 4) 
		{
			Console.Error.WriteLine("Usage:   mono CompareAttrs <host name> <login dn> "
				+ "<password> <compare dn> ");
			Console.Error.WriteLine("Example: mono CompareAttrs Acme.com \"cn=Admin,"
				+ "o=Acme\" secret\n         \"cn=JSmith,ou=Sales,o=Acme\"");
			Environment.Exit(1);
		}

		int ldapPort = LdapConnection.DEFAULT_PORT;
		int ldapVersion = LdapConnection.Ldap_V3;
		bool compareResults = false;        
		String ldapHost = args[0];
		String loginDN  = args[1];
		String password = args[2];
		String dn = args[3];
		LdapConnection lc = new LdapConnection();
		LdapAttribute attr = null;

		try 
		{
			// connect to the server
			lc.Connect( ldapHost, ldapPort );

			// authenticate to the server
			lc.Bind( ldapVersion, loginDN, password );				

			attr =new LdapAttribute( "objectclass", "inetOrgPerson" );
			System.Collections.IEnumerator allValues = attr.StringValues;
			allValues.MoveNext();
			// Compare the value of the objectclass attribute.
			if ( compareResults == lc.Compare(dn, attr))
				Console.WriteLine("\t" + (String)allValues.Current
						   + " is contained in the " + attr.Name + " attribute." );
			else
				Console.WriteLine("\t" + (String)allValues.Current
						   + " is not contained in the " + attr.Name + " attribute." );

			attr = new LdapAttribute( "sn", "Bunny" );			
			allValues = attr.StringValues;
			allValues.MoveNext();

			// Compare the value of the sn attribute.
			if ( compareResults == lc.Compare(dn, attr))
				Console.WriteLine("\t" + (String)allValues.Current
						   + " is contained in the " + attr.Name + " attribute." );
			else
				Console.WriteLine("\t" + (String)allValues.Current
						   + " is not contained in the " + attr.Name + " attribute." );

			// disconnect with the server
			lc.Disconnect();
		}
		catch( LdapException e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
		}
		catch( Exception e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
		}
		Environment.Exit(0);
	}
}
