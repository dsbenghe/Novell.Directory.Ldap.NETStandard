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
// Samples.ListGroups.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

/* ListGroups.cs sample lists the members of the specified
 * group.  If the object is a dynamic group, the search filter,
 * identity, and excluded member list are also displayed.
 * 
 *  Group Attributes:
 *	  member (or it's synonym uniqueMember):  A list of the DNs of the
 *	  members of the group.   For dynamic groups, DN's added to this
 *    attribute will be automatically included in the group regardless
 *    of the search filter.
 *
 *  Dynamic Group attributes:
 *    memberQueryURL:  Specifies parameters for the implied search when
 *    reading members of the group.  The query is in URL form:
 *    ldap:///<base dn>??<scope>?<filter>[?[!]x-chain]
 *    where:
 *       <base dn> is the starting point of the search
 *       <scope> is "one" for one-level, or "sub" for subtree search
 *       <filter> is the search filter
 *       x-chain indicates that the search will chain if necessary.
 *       (Use with care, since it degrades performance.)
 *
 *  excludedMember:  
 *    A list of the DN's specifically excluded from the group 
 *    regardless of the search filter.
 *
 *  dgIdentity:  
 *    Specifies the identity to use for the implicit search.
 *    If this attribute is not present, it uses the public/private key
 *    of the group object, if present.
 *    Otherwise use the anonymous identity.
 *
 *  dgAllowDuplicates:  
 *    Boolean attribute.  "true" speeds up the search, but the members 
 *    list may have duplicates.
 *
 *  dgTimeout:  
 *    Number of seconds to wait to get results from another server 
 *    when chaining.
 ******************************************************************************/

using System;
using System.Collections;

using Novell.Directory.Ldap;

public class ListGroups
{
	public static void Main( String[] args )
	{
		if (args.Length != 4) 
		{
			Console.WriteLine("Usage:   mono ListGroups <host name> <login dn>"
					   + " <password> <group dn>\n");
			Console.WriteLine("Example: mono ListGroups Acme.com"
					   + " \"cn=admin,o=Acme\" secret "
					   + " cn=salesGroup,ou=sales,o=acme\n");
			Environment.Exit(0);
		}

		int ldapPort = LdapConnection.DEFAULT_PORT;
		int searchScope = LdapConnection.SCOPE_BASE;
		int ldapVersion  = LdapConnection.Ldap_V3;
		int i;
		IEnumerator objClass =  null;
		IEnumerator queryURL =  null;
		IEnumerator identity =  null;
		IEnumerator excludedMember = null;
		IEnumerator member = null;
		bool isGroup=false, isDynamicGroup=false;
		String[] attrs  = new String[] {   "objectClass",
										   "memberQueryURL",
										   "dgIdentity",
										   "excludedMember",
										   "member"};

		/* Since reading members of a dynamic group could potentially involve
		 * a significant directory search, we use a timeout. Setting
		 * time out to 10 seconds
		 */
		LdapSearchConstraints cons = new LdapSearchConstraints();
		cons.TimeLimit = 10000 ;

		String ldapHost = args[0];
		String loginDN  = args[1];
		String password = args[2];
		String groupDN  = args[3];

		LdapConnection lc = new LdapConnection();

		try 
		{
			// connect to the server
			lc.Connect( ldapHost, ldapPort );
			// bind to the server
			lc.Bind( ldapVersion, loginDN, password );

			Console.WriteLine("\n\tReading object :" + groupDN);
			LdapSearchResults searchResults =
				lc.Search(  groupDN,       // object to read
				searchScope,   // scope - read single object
				null,          // search filter
				attrs,         // return only required attributes
				false,         // return attrs and values
				cons );        // time out value

			// Examine the attributes that were returned and extract the data

			LdapEntry nextEntry = null;
			try 
			{
				nextEntry = searchResults.next();
			}
			catch(LdapException e) 
			{
				Console.WriteLine("Error: " + e.ToString());
				Environment.Exit(1);
			}

			LdapAttributeSet attributeSet = nextEntry.getAttributeSet();
			IEnumerator allAttributes = attributeSet.GetEnumerator();

			while(allAttributes.MoveNext()) 
			{
				LdapAttribute attribute = (LdapAttribute)allAttributes.Current;
				String attributeName = attribute.Name;
				// Save objectclass values
				if (attributeName.ToUpper().Equals( "objectClass".ToUpper() ) ) 
				{
					objClass =  attribute.StringValues;
				}

					// Save the memberQueryURL attribute if present
				else if (attributeName.ToUpper().Equals( "memberQueryURL".ToUpper() ))
				{
					queryURL =  attribute.StringValues;
				}

					// Save the dgIdentity attribute if present
				else if (attributeName.ToUpper().Equals( "dgIdentity".ToUpper() ) ) 
				{
					identity =  attribute.StringValues;
				}

					// Save the excludedMember attribute if present
				else if (attributeName.ToUpper().Equals( "excludedMember".ToUpper() )) 
				{
					excludedMember =  attribute.StringValues;
				}

					/* Save the member attribute.  This may also show up
					 * as uniqueMember
					 */
				else if ( attributeName.ToUpper().Equals ( "member".ToUpper() ) ||
					attributeName.ToUpper().Equals ( "uniqueMember".ToUpper() ) ) 
				{
					member =  attribute.StringValues;
				}
			}

			/* Verify that this is a group object  (i.e. objectClass contains
			 * the value "group", "groupOfNames", or "groupOfUniqueNames").
			 * Also determine if this is a dynamic group object
			 * (i.e. objectClass contains the value "dynamicGroup" or
			 * "dynamicGroupAux").
			 */
			while(objClass.MoveNext()) 
			{
				String objectName = (String) objClass.Current;
				if ( objectName.ToUpper().Equals( "group".ToUpper() ) ||
					objectName.ToUpper().Equals( "groupOfNames".ToUpper() ) ||
					objectName.ToUpper().Equals( "groupOfUniqueNames".ToUpper()) )
					isGroup = true;
				else if ( objectName.ToUpper().Equals( "dynamicGroup".ToUpper() ) ||
					objectName.ToUpper().Equals( "dynamicGroupAux".ToUpper() ) )
					isGroup = isDynamicGroup = true;
			}

			if (!isGroup) 
			{
				Console.WriteLine("\tThis object is NOT a group object."
						   + "Exiting.\n");
				Environment.Exit(0);
			}

			/* If this is a dynamic group, display its memberQueryURL, identity
			 * and excluded member list.
			 */
			if ( isDynamicGroup )  
			{
				if ( (queryURL != null)&& (queryURL.MoveNext()) ) 
				{
					Console.WriteLine("\tMember Query URL:");
					while (queryURL.MoveNext())
						Console.WriteLine("\t\t" + queryURL.Current);
				}

				if ( (identity != null) && (identity.MoveNext()) ) 
				{
					Console.WriteLine("\tIdentity for search:"
							   + identity.Current);
				}

				if ( (excludedMember != null) &&
					(excludedMember.MoveNext()) ) 
				{
					Console.WriteLine("\tExcluded member list:");
					while (excludedMember.MoveNext())
						Console.WriteLine("\t\t"
								   + excludedMember.Current);
				}
			}

			// Print the goup's member list
			if( member != null && member.MoveNext() )
			{
				Console.WriteLine("\n\tMember list:");
				while ( member.MoveNext() )
					Console.WriteLine("\t\t" + member.Current);
			}

			// disconnect with the server
			lc.Disconnect();
		}
		catch( LdapException e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
			Environment.Exit(1);
		}
		catch( Exception e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
		}
		Environment.Exit(0);
	}
}
