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
// Samples.Controls.AsynchronousSortControl.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//



/*
 *  The sample AsynchronousSortControl.cs demonstrates how to use 
 *  the server side control with Asynchronous search requests.
 * 
 *  The program is hard coded to sort based on the common name
 *  attribute, and it searches for all objects at the specified
 *  searchBase.
 * 
 */

using System;
using System.Collections;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;


public class AsyncSortControl 
{
	public static void Main( String[] args ) 
	{           
		// Verify correct number of parameters
		if (args.Length != 4) 
		{
			Console.WriteLine("Usage:   mono AsynchronousSortControl <host name> "
					   + "<login dn> <password> <container>");
			Console.WriteLine("Example: mono AsynchronousSortControl Acme.com"
					   + " \"cn=admin,o=Acme\" secret \"ou=Sales,o=Acme\"");
			Environment.Exit(0);
		}
           
		// Read command line arguments  
		String  ldapHost    = args[0];       
		String  loginDN     = args[1];
		String  password    = args[2];
		String  searchBase  = args[3];
		int MY_PORT = 389;
		int ldapVersion  = LdapConnection.Ldap_V3;        

		try 
		{
			// Create a LdapConnection object
			LdapConnection lc = new LdapConnection();
            
			// Connect to server
			lc.Connect( ldapHost, MY_PORT);
			lc.Bind(ldapVersion, loginDN, password );
			Console.WriteLine( "Login succeeded");

			// We will be searching for all objects 
			String MY_FILTER = "(objectClass=*)";

			//  Results of the search should include givenname and cn
			String[] attrs = new String[2];
			attrs[0] = "givenname";
			attrs[1] = "cn";

			// The results should be sorted using the cn attribute
			LdapSortKey[] keys = new LdapSortKey[1];
			keys[0] = new LdapSortKey( "cn" );
          
			// Create a LdapSortControl object - Fail if cannot sort
			LdapSortControl sort = new LdapSortControl( keys, true );
            
			// Set the Sort control to be sent as part of search request
			LdapSearchConstraints cons = lc.SearchConstraints;
			cons.setControls( sort );
			lc.Constraints = cons;

			// Perform the search - ASYNCHRONOUS SEARCH USED HERE
			Console.WriteLine( "Calling search request");
			LdapSearchQueue queue = lc.Search( searchBase, 
				LdapConnection.SCOPE_SUB, 
				MY_FILTER, 
				attrs, 
				false, 
				(LdapSearchQueue)null,
				(LdapSearchConstraints) null );
  
			LdapMessage message; 
			while (( message = queue.getResponse()) != null ) 
			{
        
				// OPTION 1: the message is a search result reference
				if ( message is LdapSearchResultReference ) 
				{                    
					// Not following referrals to keep things simple
					String[] urls = ((LdapSearchResultReference)message).Referrals;
					Console.WriteLine("Search result references:");                        
					for ( int i = 0; i < urls.Length; i++ )
						Console.WriteLine(urls[i]); 
				} 
                
					// OPTION 2:the message is a search result
				else if ( message is LdapSearchResult ) 
				{
					// Get the object name
					LdapEntry entry = ((LdapSearchResult)message).Entry;
                       
					Console.WriteLine("\n" + entry.DN);
					Console.WriteLine("\tAttributes: ");

					// Get the attributes and print them out
					LdapAttributeSet attributeSet = entry.getAttributeSet();
					IEnumerator allAttributes = attributeSet.GetEnumerator();
  
					while(allAttributes.MoveNext()) 
					{
						LdapAttribute attribute = (LdapAttribute)allAttributes.Current;
						String attributeName = attribute.Name;
     
						Console.WriteLine("\t\t" + attributeName);

						// Print all values of the attribute
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
				}                
                
					// OPTION 3: The message is a search response
				else 
				{
					LdapResponse response = (LdapResponse)message;
					int status = response.ResultCode;
                    
					// the return code is Ldap success
					if ( status == LdapException.SUCCESS ) 
					{
						Console.WriteLine("Asynchronous search succeeded.");
					}
                    
						// the return code is referral exception
					else if ( status == LdapException.REFERRAL ) 
					{                        
						String[] urls=((LdapResponse)message).Referrals;
						Console.WriteLine("Referrals:");                        
						for ( int i = 0; i < urls.Length; i++ )
							Console.WriteLine(urls[i]);                    
					}                    
					else 
					{                        
						Console.WriteLine("Asynchronous search failed.");
						Console.WriteLine( response.ErrorMessage);
					}
                    
					// Server should send back a control irrespective of the 
					// status of the search request
					LdapControl[] controls = response.Controls;
					if ( controls != null ) 
					{
                
						// Theoritically we could have multiple controls returned
						for( int i = 0; i < controls.Length; i++ ) 
						{
    
							// We are looking for the LdapSortResponse Control class - the control
							// sent back in response to LdapSortControl
							if ( controls[i] is LdapSortResponse ) 
							{
								
								Console.WriteLine("Received Ldap Sort Control fromserver");
						     
								// We must have an error code and maybe a string identifying
								// erring attribute in the response control.  Get these.
								String bad = ((LdapSortResponse)controls[i]).FailedAttribute;
								int result = ((LdapSortResponse)controls[i]).ResultCode;

								// Print out error ccode (0 if no error) and any returned
								// attribute
								Console.WriteLine( "Error code: " + result );
								if ( bad != null )
									Console.WriteLine( "Offending " + "attribute: " + bad );
								else
									Console.WriteLine( "No offending " + "attribute " + "returned" );
							}
						}
					}
                    
				}                            
			}                                                                                     
		
				// All done - disconnect
			if ( lc.Connected == true )
					lc.Disconnect();
		}
        
		catch( LdapException e ) 
		{
			Console.WriteLine( e.ToString() );
		}
		catch( Exception e ) 
		{
			Console.WriteLine( "Error: " + e.ToString() );
		}
	}
}
