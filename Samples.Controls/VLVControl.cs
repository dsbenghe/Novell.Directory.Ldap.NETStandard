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
// Samples.Controls.VLVControl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;

/// <summary>  The following sample demonstrates how to use the VLV
/// control with Synchronous search requests.  As required a 
/// Server Side Sort Control is also included in the request.
/// 
/// The program is hard coded to sort based on the common name
/// attribute, and it searches for all objects at the specified
/// searchBase.
/// 
/// Usage: Usage: VLVControl <host name> <login dn> <password> 
/// <searchBase>
/// 
/// </summary>
public class VLVControl
{
	
	public static void  Main(System.String[] args)
	{
		
		/* Check if we have the correct number of command line arguments */
		if (args.Length != 4)
		{
			System.Console.Error.WriteLine("Usage:   mono VLVControl <host name> <login dn>" + " <password> <container>");
			System.Console.Error.WriteLine("Example: mono VLVControl Acme.com \"cn=admin,o=Acme\" secret" + " \"ou=Sales,o=Acme\"");
			System.Environment.Exit(1);
		}
		
		/* Parse the command line arguments  */
		System.String LdapHost = args[0];
		System.String loginDN = args[1];
		System.String password = args[2];
		System.String searchBase = args[3];
		int LdapPort = LdapConnection.DEFAULT_PORT;
		int LdapVersion = LdapConnection.Ldap_V3;
		LdapConnection conn = new LdapConnection();
		
		try
		{
			// connect to the server
			conn.Connect(LdapHost, LdapPort);
			// bind to the server
			conn.Bind(LdapVersion, loginDN, password);
			System.Console.Out.WriteLine("Succesfully logged in to server: " + LdapHost);
			
			/* Set default filter - Change this line if you need a different set
			* of search restrictions. Read the "NDS and Ldap Integration Guide"
			* for information on support by Novell eDirectory of this
			* functionaliry.
			*/
			System.String MY_FILTER = "cn=*";
			
			/* We are requesting that the givenname and cn fields for each 
			* object be returned
			*/
			System.String[] attrs = new System.String[2];
			attrs[0] = "givenname";
			attrs[1] = "cn";
			
			// We will be sending two controls to the server 
			LdapControl[] requestControls = new LdapControl[2];
			
			/* Create the sort key to be used by the sort control 
			* Results should be sorted based on the cn attribute. 
			* See the "NDS and Ldap Integration Guide" for information on
			* Novell eDirectory support of this functionaliry.
			*/
			LdapSortKey[] keys = new LdapSortKey[1];
			keys[0] = new LdapSortKey("cn");
			
			// Create the sort control 
			requestControls[0] = new LdapSortControl(keys, true);
			
			/* Create the VLV Control.
			* These two fields in the VLV Control identify the before and 
			* after count of entries to be returned 
			*/
			int beforeCount = 0;
			int afterCount = 2;
			
			/* The VLV control request can specify the index
			* using one of the two methods described below:
			* 
			* TYPED INDEX: Here we request all objects that have cn greater
			* than or equal to the letter "a" 
			*/
			requestControls[1] = new LdapVirtualListControl("a", beforeCount, afterCount);
			
			/* The following code needs to be enabled to specify the index 
			* directly 
			*   int offset = 0; - offset of the index
			*   int contentCount = 3; - our estimate of the search result size
			*   requestControls[1] = new LdapVirtualListControl(offset, 
			*                          beforeCount, afterCount, contentCount);
			*/
			
			// Set the controls to be sent as part of search request
			LdapSearchConstraints cons = conn.SearchConstraints;
			cons.setControls(requestControls);
			conn.Constraints = cons;
			
			// Send the search request - Synchronous Search is being used here 
			System.Console.Out.WriteLine("Calling Asynchronous Search...");
			LdapSearchResults res = conn.Search(searchBase, LdapConnection.SCOPE_SUB, MY_FILTER, attrs, false, (LdapSearchConstraints) null);
			
			// Loop through the results and print them out
			while (res.hasMore())
			{
				
				/* Get next returned entry.  Note that we should expect a Ldap-
				*Exception object as well just in case something goes wrong
				*/
				LdapEntry nextEntry=null;
				try
				{
					nextEntry = res.next();
				}
				catch (LdapException e)
				{
					if (e is LdapReferralException)
						continue;
					else
					{
						System.Console.Out.WriteLine("Search stopped with exception " + e.ToString());
						break;
					}
				}
				
				/* Print out the returned Entries distinguished name.  */
				System.Console.Out.WriteLine();
				System.Console.Out.WriteLine(nextEntry.DN);
				
				/* Get the list of attributes for the current entry */
				LdapAttributeSet findAttrs = nextEntry.getAttributeSet();
				
				/* Convert attribute list to Enumeration */
				System.Collections.IEnumerator enumAttrs = findAttrs.GetEnumerator();
				System.Console.Out.WriteLine("Attributes: ");
				
				/* Loop through all attributes in the enumeration */
				while (enumAttrs.MoveNext())
				{
					
					LdapAttribute anAttr = (LdapAttribute) enumAttrs.Current;
					
					/* Print out the attribute name */
					System.String attrName = anAttr.Name;
					System.Console.Out.WriteLine("" + attrName);
					
					// Loop through all values for this attribute and print them
					System.Collections.IEnumerator enumVals = anAttr.StringValues;
					while (enumVals.MoveNext())
					{
						System.String aVal = (System.String) enumVals.Current;
						System.Console.Out.WriteLine("" + aVal);
					}
				}
			}
			
			// Server should send back a control irrespective of the 
			// status of the search request
			LdapControl[] controls = res.ResponseControls;
			if (controls == null)
			{
				System.Console.Out.WriteLine("No controls returned");
			}
			else
			{
				
				// We are likely to have multiple controls returned 
				for (int i = 0; i < controls.Length; i++)
				{
					
					/* Is this the Sort Response Control. */
					if (controls[i] is LdapSortResponse)
					{
						
						System.Console.Out.WriteLine("Received Ldap Sort Control from " + "Server");
						
						/* We could have an error code and maybe a string 
						* identifying erring attribute in the response control.
						*/
						System.String bad = ((LdapSortResponse) controls[i]).FailedAttribute;
						int result = ((LdapSortResponse) controls[i]).ResultCode;
						
						// Print out error code (0 if no error) and any 
						// returned attribute
						System.Console.Out.WriteLine("Error code: " + result);
						if ((System.Object) bad != null)
							System.Console.Out.WriteLine("Offending " + "attribute: " + bad);
						else
							System.Console.Out.WriteLine("No offending " + "attribute " + "returned");
					}
					
					/* Is this a VLV Response Control */
					if (controls[i] is LdapVirtualListResponse)
					{
						
						System.Console.Out.WriteLine("Received VLV Response Control from " + "Server...");
						
						/* Get all returned fields */
						int firstPosition = ((LdapVirtualListResponse) controls[i]).FirstPosition;
						int ContentCount = ((LdapVirtualListResponse) controls[i]).ContentCount;
						int resultCode = ((LdapVirtualListResponse) controls[i]).ResultCode;
						System.String context = ((LdapVirtualListResponse) controls[i]).Context;
						
						
						/* Print out the returned fields.  Typically you would 
						* have used these fields to reissue another VLV request
						* or to display the list on a GUI 
						*/
						System.Console.Out.WriteLine("Result Code    => " + resultCode);
						System.Console.Out.WriteLine("First Position => " + firstPosition);
						System.Console.Out.WriteLine("Content Count  => " + ContentCount);
						if ((System.Object) context != null)
							System.Console.Out.WriteLine("Context String => " + context);
						else
							System.Console.Out.WriteLine("No Context String in returned" + " control");
					}
				}
			}
			
			/* We are done - disconnect */
			if (conn.Connected)
				conn.Disconnect();
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine(e.ToString());
		}
		catch (System.IO.IOException e)
		{
			System.Console.Out.WriteLine("Error: " + e.ToString());
		}
		catch(Exception e)
		{
			System.Console.WriteLine("Error: " + e.Message);
		}
	}
}
