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
// Samples.List.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

/*The List.cs example returns all the entries in the specified
 *container (search base). No attributes are returned.
 */

using Novell.Directory.Ldap;
using System;

public class List 
{
	public static void Main( String[] args ) 
	{       
		if (args.Length != 5) 
		{
			Console.Error.WriteLine("Usage:   mono List <host name> <login dn>"
				+ " <password> <search base>\n"
				+ "         <search filter>");
			Console.Error.WriteLine("Example: mono List Acme.com \"cn=admin,o=Acme\""
				+ " secret \"ou=sales,o=Acme\"\n"
				+ "         \"(objectclass=*)\"");
			Environment.Exit(1);
		}

		int LdapPort = LdapConnection.DEFAULT_PORT;
		int searchScope = LdapConnection.SCOPE_ONE;
		int LdapVersion  = LdapConnection.Ldap_V3;;
		bool attributeOnly = true;
		String[] attrs = {LdapConnection.NO_ATTRS};                
		String ldapHost = args[0];
		String loginDN = args[1];
		String password = args[2];
		String searchBase = args[3];
		String searchFilter = args[4];
		LdapConnection lc = new LdapConnection();

		try 
		{
			// connect to the server
			lc.Connect( ldapHost, LdapPort );
			// bind to the server
			lc.Bind( LdapVersion, loginDN, password );

			LdapSearchResults searchResults =
				lc.Search(  searchBase,      // container to search
				searchScope,     // search scope
				searchFilter,    // search filter
				attrs,           // "1.1" returns entry name only
				attributeOnly);  // no attributes are returned

			// print out all the objects
			while ( searchResults.hasMore() ) 
			{
				LdapEntry nextEntry = null;
				try 
				{
					nextEntry = searchResults.next();
				}
				catch(LdapException e) 
				{
					Console.WriteLine("Error: " + e.ToString());

					// Exception is thrown, go for next entry
					continue;
				}

				Console.WriteLine("\n" + nextEntry.DN);
			}
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
