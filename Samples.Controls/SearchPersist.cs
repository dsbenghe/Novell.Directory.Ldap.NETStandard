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
// Samples.Controls.SearchPersist.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;

namespace Samples.Controls
{
	class SearchPersist
	{
		const double TIME_OUT_IN_MINUTES = 0.5;
		static DateTime timeOut;

		static void Main(string[] args)
		{

			if ( args.Length != 5)
			{
				Console.WriteLine("Usage:   mono SearchPersist <host name> <ldap port>  <login dn>" + " <password> <search base>" );
				Console.WriteLine("Example: mono SearchPersist Acme.com 389"  + " \"cn=admin,o=Acme\"" + " secret \"ou=sales,o=Acme\"");
				return;
			}

			int ldapVersion  = LdapConnection.Ldap_V3;        
			String ldapHost = args[0];
			int ldapPort = Convert.ToInt32(args[1]);;
			String loginDN = args[2];
			String password = args[3];
			String searchBase = args[4];
			LdapSearchQueue queue = null;
			LdapSearchConstraints constraints;
			LdapPersistSearchControl psCtrl;
			LdapConnection lc = new LdapConnection();
			constraints =  new LdapSearchConstraints();

			try 
			{
				// connect to the server
				lc.Connect( ldapHost, ldapPort );
				// authenticate to the server
				lc.Bind(ldapVersion, loginDN, password);

				//Create the persistent search control
				psCtrl = new LdapPersistSearchControl(
					LdapPersistSearchControl.ANY, // any change
					true,                         //only get changes
					true,                         //return entry change controls
					true);                        //control is critcal

				// add the persistent search control to the search constraints
				constraints.setControls( psCtrl );

				// perform the search with no attributes returned
				String[] noAttrs = {LdapConnection.NO_ATTRS};
				queue = lc.Search(
					searchBase,                // container to search
					LdapConnection.SCOPE_SUB,  // search container's subtree
					"(objectClass=*)",         // search filter, all objects
					noAttrs,                   // don't return attributes
					false,                     // return attrs and values, ignored
					null,                      // use default search queue
					constraints);              // use default search constraints
			}
			catch( LdapException e ) 
			{
				Console.WriteLine( "Error: " + e.ToString() );
				try { lc.Disconnect(); } 
				catch(LdapException e2) {  }
				Environment.Exit(1);
			}
			catch(Exception e)
			{
				Console.WriteLine( "Error: " + e.Message );
				return;
			}
						
			Console.WriteLine("Monitoring the events for {0} minutes..", TIME_OUT_IN_MINUTES );
			Console.WriteLine();

			//Set the timeout value
			timeOut= DateTime.Now.AddMinutes(TIME_OUT_IN_MINUTES);

			try 
			{
				//Monitor till the timeout happens
				while (DateTime.Now.CompareTo(timeOut) < 0) 
				{
					if (!checkForAChange(queue))
						break;					
					System.Threading.Thread.Sleep(10);		
				}
			} 			
			catch (System.IO.IOException e)
			{
				System.Console.Out.WriteLine(e.Message);
			}
			catch (System.Threading.ThreadInterruptedException e)
			{
			}
		
			//Disconnect from the server before exiting
			try
			{
				lc.Abandon(queue); //abandon the search
				lc.Disconnect();
			}
			catch (LdapException e)
			{
				Console.Out.WriteLine();
				Console.Out.WriteLine("Error: " + e.ToString());
			}
		
			Environment.Exit(0);
		}
		
		/// <summary> Check the queue for a response. If a response has been received,
		/// print the response information.
		/// </summary>
		
		static private bool checkForAChange(LdapSearchQueue queue)
		{
			LdapMessage message;
			bool result = true;
			try
			{
				//check if a response has been received so we don't block
				//when calling getResponse()
				if (queue.isResponseReceived())
				{
					message = queue.getResponse();
					if (message != null)
					{
						// is the response a search result reference?
						if (message is LdapSearchResultReference)
						{
							String[] urls = ((LdapSearchResultReference) message).Referrals;
							Console.Out.WriteLine("\nSearch result references:");
							for (int i = 0; i < urls.Length; i++)
								Console.Out.WriteLine(urls[i]);							
						}
							// is the response a search result?
						else if (message is LdapSearchResult)
						{
							LdapControl[] controls = message.Controls;
							for (int i = 0; i < controls.Length; i++)
							{
								if (controls[i] is LdapEntryChangeControl)
								{
									LdapEntryChangeControl ecCtrl = (LdapEntryChangeControl) controls[i];
								
									int changeType = ecCtrl.ChangeType;
										Console.Out.WriteLine("\n\nchange type: " + getChangeTypeString(changeType));
									if (changeType == LdapPersistSearchControl.MODDN)
										Console.Out.WriteLine("Prev. DN: " + ecCtrl.PreviousDN);
									if (ecCtrl.HasChangeNumber)
										Console.Out.WriteLine("Change Number: " + ecCtrl.ChangeNumber);
								
									LdapEntry entry = ((LdapSearchResult) message).Entry;
								
									Console.Out.WriteLine("entry: " + entry.DN);									
								}
							}
						}
							// the message is a search response
						else
						{
							LdapResponse response = (LdapResponse) message;
							int resultCode = response.ResultCode;
							if (resultCode == LdapException.SUCCESS)
							{
								Console.Out.WriteLine("\nUnexpected success response.");
								result = false;
							}
							else if (resultCode == LdapException.REFERRAL)
							{
								String[] urls = ((LdapResponse) message).Referrals;
								Console.Out.WriteLine("\n\nReferrals:");
								for (int i = 0; i < urls.Length; i++)
									Console.Out.WriteLine(urls[i]);
							}
							else
							{
								Console.Out.WriteLine("Persistent search failed.");
								throw new LdapException(response.ErrorMessage, resultCode, response.MatchedDN);
							}
						}
					}
				}
			}
			catch (LdapException e)
			{
				Console.Out.WriteLine("Error: " + e.ToString());
				result = false;
			}
		
			return result;
		}
	
		/// <summary> Return a string indicating the type of change represented by the
		/// changeType parameter.
		/// </summary>
		private static String getChangeTypeString(int changeType)
		{
			String changeTypeString;
		
			switch (changeType)
			{
			
				case LdapPersistSearchControl.ADD: 
					changeTypeString = "ADD";
					break;
			
				case LdapPersistSearchControl.MODIFY: 
					changeTypeString = "MODIFY";
					break;
			
				case LdapPersistSearchControl.MODDN: 
					changeTypeString = "MODDN";
					break;
			
				case LdapPersistSearchControl.DELETE: 
					changeTypeString = "DELETE";
					break;
			
				default: 
					changeTypeString = "Unknown change type: " + changeType.ToString();
					break;
			
			}
		
			return changeTypeString;
		}
	} //end class SearchPersist
}