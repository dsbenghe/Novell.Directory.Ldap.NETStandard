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
// Samples.ClientSideSort.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

/* The ClientSideSort.cs example demonstrates the sorting
 * capabilities for search results, i.e., LDAPEntries, and
 * LDAPAttributes.  The example sorts and prints entries in the
 * specified container (search base).
 */ 

using System;
using System.Collections;
using Novell.Directory.Ldap;

public class ClientSideSort
{
	public static void Main( String[] args )
	{
		if (args.Length != 5) 
		{
			Console.WriteLine("Usage:   mono ClientSideSort <host name> "+
					   "<login dn> <password> <search base>\n"
					   + "         <search filter>");
			Console.WriteLine("Example: mono ClientSideSort Acme.com"
					   + " \"cn=admin,o=Acme\""
					   + " secret \"ou=sales,o=Acme\"\n"
					   + "         \"(objectclass=*)\"");
			Environment.Exit(0);
		}

		int ldapPort = LdapConnection.DEFAULT_PORT;
		int searchScope = LdapConnection.SCOPE_ONE;
		int ldapVersion  = LdapConnection.Ldap_V3;
		String ldapHost = args[0];
		String loginDN  = args[1];
		String password = args[2];
		String searchBase = args[3];
		String searchFilter = args[4];
		LdapConnection conn = new LdapConnection();

		try 
		{
			// connect to the server
			conn.Connect( ldapHost, ldapPort );

			// bind to the server
			conn.Bind( ldapVersion, loginDN, password);

			LdapSearchResults searchResults = conn.Search(  searchBase,
															searchScope,
															searchFilter,
															new String[] {"cn", "uid", "sn"}, //attributes
															false);        // return attrs and values

			/* sortedResults will sort the entries according to the natural
			 * ordering of LDAPEntry (by distiguished name).
			 */

			ArrayList sortedResults = new ArrayList();
			while ( searchResults.hasMore()) 
			{
				try 
				{
					sortedResults.Add( searchResults.next() );
				}
				catch(LdapException e) 
				{
					Console.WriteLine("Error: " + e.ToString());
					// Exception is thrown, go for next entry
					continue;
				}
			}

			// print the sorted results
			Console.WriteLine( "\n"+
					   "****************************\n"+
					   "Search results sorted by DN:\n"+
					   "****************************");
			sortedResults.Sort();
			IEnumerator i = sortedResults.GetEnumerator(0,sortedResults.Count-1);
			while (i.MoveNext())
			{
				PrintEntry( (LdapEntry)(i.Current) );
			}

			/* resort the results an an array using a specific comparator */
			String[] namesToSortBy  = { "sn", "uid", "cn"  };
			bool[] sortAscending = { true, false, true };
			LdapCompareAttrNames myComparator = new LdapCompareAttrNames( namesToSortBy, sortAscending );

			Object[] sortedSpecial = sortedResults.ToArray();
			Array.Sort(sortedSpecial, myComparator);

			// print the re-sorted results
			Console.WriteLine( "\n" +
				   "*****************************************************\n" +
				   "Search results sorted by sn, uid(Descending), and cn:\n" +
				   "*****************************************************");
			for(int j=0; j< sortedSpecial.Length; j++)
			{
				PrintEntry( (LdapEntry) sortedSpecial[j] );
			}
			// disconnect with the server
			conn.Disconnect();
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

	/**
	 * Prints the DN and attributes in an LDAPEntry to System.out.
	 * This method used TreeSet to sort the attributes by name.
	 */
	public static void PrintEntry(LdapEntry entry)
	{
		/* To print an entry,
		 *   -- Loop through all the attributes
		 *   -- Loop through all the attribute values
		 */

		Console.WriteLine(entry.DN);
		Console.WriteLine("\tAttributes: ");

		LdapAttributeSet attributeSet = entry.getAttributeSet();
		IEnumerator allAttributes = attributeSet.GetEnumerator();

		while(allAttributes.MoveNext()) 
		{
			LdapAttribute attribute = (LdapAttribute)(allAttributes.Current);
			string attributeName = attribute.Name;

			Console.WriteLine("\t\t" + attributeName);

			IEnumerator allValues = attribute.StringValues;

			if( allValues != null) 
			{
				while(allValues.MoveNext()) 
				{
					String Value = (String) allValues.Current;
					Console.WriteLine("\t\t\t" + Value);
				}
			}
		}
		return;
	}
}

