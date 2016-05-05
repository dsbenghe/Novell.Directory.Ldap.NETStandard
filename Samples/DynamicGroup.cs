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
// Samples.DynamicGroup.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//


/*  The DynamicGroup.cs sample demonstrates how to:
 *  1) create the Dynamic Group Entry cn=myDynamicGroup
 *     in a specified container with a specified memberQueryURL
 *  2) read and print the values of the "member" attribute of cn=myDynamicGroup
 *  3) delete the dynamic group
 *
 *  Notes on Dynamic Groups:
 *
 *  Dynamic groups are supported in Novell eDirectory version 8.6.1 or later.
 *
 *  A dynamic group is similar to a group entry, but has a search URL
 *  attribute.  Entries satisfying the search URL are considered members of
 *  the group. The DN of each member will be returned when reading the
 *  "member" (or its synonym "uniqueMember") attribute.
 *
 *  A dynamic group is created either with objectClass="dynamicGroup" or by
 *  adding the auxiliary class "dynamicGroupAux" to an existing group entry.
 *
 *  The search URL is specified in the "memberQueryURL" attribute.
 *  The value of "memberQueryURL" is encoded as an "Ldap URL".
 *
 *  An entry may be statically added to a dynamic group by adding its DN
 *  to the "member" (or its synonym "uniqueMember") attribute.  Similarly
 *  an entry may be statically excluded from the group by adding its DN to the
 *  "excludedMember" attribute. These entries will be included or excluded
 *  from the group regardless of the search URL.
 *
 *  Note: at the present time, the only way to view only the static members
 *  of a dynamic group is to delete the memberQueryURL attribute and
 *  then read the member attribute.
 *
 *  In order to provide consistent results when processing the search URL,
 *  the authorization identity DN used to determine group membership is based
 *  on the following criteria:
 *  1) If the "dgIdentity" attribute is present, its value is the identity DN.
 *  2) If the above is false, and if the dynamic group entry contains a
 *     public/private key, then the DN of the group entry is the identity DN.
 *  3) If neither of the above are true, then the anonymous identity is the
 *     identity DN.
 *
 *  The creator of the group cannot set the "dgIdentity" attribute to a DN
 *  to which he or she does not already have rights.  The dynamic group entry
 *  and the DN specified by dgIdentity must be on the same server.
 *
 *  The "dgAllowDuplicates" attribute enables or disables the presence
 *  of duplicate values in the "membership" attribute. The default is false.
 *  Setting this attribute to true results in a faster search, but some values
 *  in the "membership" attribute may be duplicates.
 *
 *  The "dgTimeout" attribute determines the number of seconds to wait to get
 *  results from another server during dynamic group member search, when the
 *  search spans multiple servers.
 *
 *  The format the search URL in the memberQueryUrl attribute is:
 *      Ldap:///<base dn>??<scope>?<filter>[?[!]x-chain]
 *
 *  The optional extension "x-chain" causes the server to chain to other
 *  servers if necessary to complete the search. When present, the search
 *  will NOT be limited to the host server.  This extension should be used
 *  carefully. The exclamation indicates it is a critical extension, and if set
 *  the server will return an error if chaining is not supported or enabled.
 *
 *  For example, to create a dynamic group consisting of all entries in the
 *  "ou=sales,o=acme" subtree with the title "Manager", set memberQueryURL to:
 *      "Ldap:///ou=sales,o=acme??sub?(title=Manager)"
 */


using Novell.Directory.Ldap;
using System;
using System.Collections;

public class DynamicGroup
{
	public static void Main( String[] args )
	{
		if (args.Length != 6) 
		{
			Console.Error.WriteLine("Usage:   mono DynamicGroup <host name>"
				+ " <port number> <login dn> <password> <container name>"
				+ " <queryURL>");
			Console.Error.WriteLine("Example: mono DynamicGroup Acme.com"
				+ " 389 \"cn=admin, o=Acme\" secret \"o=Acme\" \n             "
				+ " Ldap:///ou=Sales,o=Acme??sub?(title=*Manager*)\n");
			Environment.Exit(1);
		}

		// Set Ldap version to 3 */
		int LdapVersion  = LdapConnection.Ldap_V3;
		
		String ldapHost = args[0];
		int ldapPort = Convert.ToInt32(args[1]);
		String loginDN = args[2];
		String password = args[3];
		String containerName = args[4];
		String queryURL = args[5];
		
		/* Construct the entry's dn using the container name from the
		 * command line and the name of the new dynamic group entry to create.
		 */
		String dn = "cn=myDynamicGroup," + containerName;

		LdapConnection lc = new LdapConnection();

		try 
		{
			if(ldapPort == LdapConnection.DEFAULT_SSL_PORT)
				lc.SecureSocketLayer = true;
			// connect to the server
			lc.Connect( ldapHost, ldapPort );
			// bind to the server
			lc.Bind( LdapVersion, loginDN, password );

			// Adding dynamic group entry to the tree
			Console.WriteLine( "\tAdding dynamic group entry...");

			/* add a dynamic group entry to the directory tree in the
			 * specified container
			 */
			addDynamicGroupEntry( lc, loginDN, dn, queryURL);

			/* Reading the member attribute of dynamic group entry and
			 * printing the values
			 */
			Console.WriteLine("\n\tReading the \"member\" "
					   + " attribute of dynamic group ojbect ...");
			searchDynamicGroupEntry ( lc, dn );


			// Removing the dynamic group entry from the specified container
			Console.WriteLine("\n\tDeleting dynamic group entry...");
			deleteDynamicGroupEntry( lc, dn );

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


	// add dynamic group entry
	public static bool addDynamicGroupEntry ( LdapConnection lc,
		String loginDN, String entryDN, String queryURL) 
	{

		bool status = true;
		LdapAttributeSet  attributeSet = new LdapAttributeSet();

		//The objectclass "dynamicGroup is used to create dynamic group entries
		attributeSet.Add( new LdapAttribute( "objectclass", "dynamicGroup" ));

		/* The memberQueryURL attribute describes the membership of the list
		 * using an LdapURL, which is defined in RFC2255
		 */
		attributeSet.Add( new LdapAttribute( "memberQueryURL", queryURL ) );

		/* Set the identity to use for the implied search.  loginDN is used
		 * as the dgIdentity in this sample.
		 */
		attributeSet.Add( new LdapAttribute( "dgIdentity", loginDN ) );

		LdapEntry newEntry = new LdapEntry( entryDN, attributeSet );

		try 
		{
			lc.Add( newEntry );
			Console.WriteLine("\tEntry: " + entryDN + " added successfully." );
		}
		catch( LdapException e ) 
		{
			Console.WriteLine( "\t\tFailed to add dynamic group entry " +
					   entryDN);
			Console.WriteLine( "Error: " + e.ToString() );
			status = false;
		}
		return status;
	}

	// read and print search results
	public static bool searchDynamicGroupEntry ( LdapConnection lc,
		String searchBase ) 
	{
		bool status = true;
		int searchScope = LdapConnection.SCOPE_BASE;
		String[] attrList = new String[]{"member"};
		String searchFilter = "(objectclass=*)";


		/* Since reading members of a dynamic group could potentially involve
		 * a significant directory search, we use a timeout. Setting
		 * time out to 10 seconds
		 */
		LdapSearchConstraints cons = new LdapSearchConstraints();
		cons.TimeLimit = 10000 ;

		try 
		{
			LdapSearchResults searchResults =
				lc.Search(  searchBase,
				searchScope,
				searchFilter,
				attrList,          // return only "member" attr
				false,             // return attrs and values
				cons );            // time out value

			LdapEntry nextEntry = null ;
			// Read and print search results.  We expect only one entry */
			if (( nextEntry = searchResults.next()) != null ) 
			{
				LdapAttributeSet attributeSet = nextEntry.getAttributeSet();
				IEnumerator allAttributes = attributeSet.GetEnumerator();

				if ( allAttributes.MoveNext() ) 
				{
					// found member(s) in this group
					LdapAttribute attribute =
						(LdapAttribute)allAttributes.Current;
					String attributeName = attribute.Name;

					IEnumerator allValues = attribute.StringValues;

					if( allValues != null) 
					{
						while(allValues.MoveNext()) 
						{
							String Value = (String) allValues.Current;
							Console.WriteLine("            " + attributeName
									   + " : " + Value);
						}
					}
				}
				else 
				{
					// no member(s) found in this group
					Console.WriteLine("            No objects matched the "
							   + " memberQueryURL filter.\n  ");
				}
			}
		}
		catch( LdapException e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
			status = false;
		}
		return status;
	}

	// delete the  dynamic group entry
	public static bool deleteDynamicGroupEntry( LdapConnection lc,
		String deleteDN ) 
	{

		bool status = true;

		try 
		{
			// Deletes the entry from the directory
			lc.Delete( deleteDN );
			Console.WriteLine("\tEntry: " + deleteDN + " was deleted." );
		}
		catch( LdapException e ) 
		{
			Console.WriteLine( "\t\tFailed to remove dynamic group entry." );
			Console.WriteLine( "Error: " + e.ToString() );
			status = false;
		}
		return status;
	}
}
