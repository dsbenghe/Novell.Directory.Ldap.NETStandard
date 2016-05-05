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
// Samples.Extensions.EdirEventSample.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Events;
using Novell.Directory.Ldap.Events.Edir;
using Novell.Directory.Ldap.Events.Edir.EventData;

public class EdirEventSample 
{
	public const double TIME_OUT_IN_MINUTES = 5;
	public static DateTime timeOut;
	
	/**
	 * Check the queue for a response. If a response has been received,
	 * print the response information.
	 */

	static private bool checkForAChange(LdapResponseQueue queue) 
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
			
					if (message is MonitorEventResponse) 
					{
						MonitorEventResponse eventerrorresponse = (MonitorEventResponse) message;
						Console.WriteLine("\nError in Registration ResultCode  = " + eventerrorresponse.ResultCode);
						EdirEventSpecifier[] specifiers = eventerrorresponse.SpecifierList;
						for (int i = 0; i < specifiers.Length; i++) 
						{
							Console.WriteLine("Specifier:" + "EventType  = " + specifiers[i].EventType);
						}
						Environment.Exit(-1);

					}

						// is the response a event response ?
					else if ( message is EdirEventIntermediateResponse) 
					{
						Console.WriteLine("Edir Event Occured");
						EdirEventIntermediateResponse eventresponse = (EdirEventIntermediateResponse) message;
						
						//process the eventresponse Data, depending on the
						// type of response 
						processEventData(eventresponse.EventResponseDataObject,	eventresponse.EventType);
                        
					}
					
						// the message is a Unknown response
					else 
					{
						Console.WriteLine("UnKnown Message =" + message);
					}
				}
			}
		} 

		catch (LdapException e) 
		{
			Console.WriteLine("Error: " + e.ToString());
			result = false;
		}

		return result;
	}

	public static void Main(String[] args) 
	{
		if (args.Length != 3) 
		{
			Console.WriteLine(
				"Usage:   mono EdirEventSample <host name> <login dn>"
				+ " <password> ");
			Console.WriteLine(
				"Example: mono EdirEventSample Acme.com \"cn=admin,o=Acme\""
				+ " secret ");
			Environment.Exit(0);
		}
		
		int ldapPort = LdapConnection.DEFAULT_PORT;
		int ldapVersion = LdapConnection.Ldap_V3;
		String ldapHost = args[0];
		String loginDN = args[1];
		String password = args[2];

		LdapResponseQueue queue = null;

		LdapConnection lc = new LdapConnection();
		
		try 
		{
			// connect to the server
			lc.Connect(ldapHost, ldapPort);

			// authenticate to the server
			lc.Bind(ldapVersion, loginDN, password);

			//Create an Array of EdirEventSpecifier
			EdirEventSpecifier[] specifier = new EdirEventSpecifier[1];

			//Register for all Add Value events.
			specifier[0] =
				new EdirEventSpecifier(EdirEventType.EVT_CREATE_ENTRY,
				//Generate an Value Event of Type Add Value 
				EdirEventResultType.EVT_STATUS_ALL
				//Generate Event for all status
				);

			//Create an MonitorEventRequest using the specifiers.        
			MonitorEventRequest requestoperation =
				new MonitorEventRequest(specifier);
			
			//Send the request to server and get the response queue.
			queue = lc.ExtendedOperation(requestoperation, null, null);

		} 

		catch (LdapException e) 
		{
			Console.WriteLine("Error: " + e.ToString());
			try 
			{
				lc.Disconnect();
			} 
			catch (LdapException e2) 
			{
				Console.WriteLine("Error: " + e2.ToString());
			}
			Environment.Exit(1);
		} 

		catch (Exception e) 
		{
			Console.WriteLine("Error: " + e.ToString());
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
			Console.WriteLine(e.Message);
		} 

		catch (System.Threading.ThreadInterruptedException e) 
		{
			Console.WriteLine(e.Message);
		}

		//disconnect from the server before exiting
		try 
		{
			lc.Abandon(queue); //abandon the search
			lc.Disconnect();
		} 

		catch (LdapException e) 
		{
			Console.WriteLine();
			Console.WriteLine("Error: " + e.ToString());
		}

		Environment.Exit(0);

	} // end main

	/**
	 * Processes the Event Data depending on the Type.
	 * @param data EventResponseData.
	 * @param type Type of Data.
	 */
	
	static private void processEventData( BaseEdirEventData data, EdirEventType type) 
	{
		switch (type) 
		{	
			case EdirEventType.EVT_CREATE_ENTRY :
				// Value event.
				//Output the relevant Data.
				EntryEventData valueevent = (EntryEventData) data;
				Console.WriteLine("Entry         = " + valueevent.Entry);
				Console.WriteLine("PrepetratorDN = " + valueevent.PerpetratorDN);
				Console.WriteLine("TimeStamp     = " + valueevent.TimeStamp);
				Console.WriteLine();
				break;
			
			default :
				//Unknow Event.				
				break;
		}

	}
}
