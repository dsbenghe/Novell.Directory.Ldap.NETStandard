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
// Novell.Directory.Ldap.Connection.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Events;

/// <summary> 
/// This class gives a sample implementation for using
/// PSearchEventSource to get Ldap events.
/// </summary>

public class PSearchCallback
{
  public const string STARTING_PROMPT = 
  "Registering for LDAP Persistent Search Events...";
  public const string QUIT_PROMPT = 
  "\nMonitoring changes... Enter \"q\" or \"Q\" to quit:";

  public static void Main(string[] args)
  {
    if (args.Length != 5)
    {
      Console.WriteLine("Usage: mono PSearchCallback.exe <host name> <port number> <login dn> <password>, <search base>");
      // TODO - mint
	return;
    }

    PSearchCallback callback = new PSearchCallback();
    callback.Execute(args[0], args[1], args[2], args[3], args[4]);
  }

  protected void Execute(string ldapHost, 
			 string ldapPort,
			 string loginDN,
			 string password,
			 string searchBase)
  {
    // Connect to the LDAP Server
    LdapConnection connection = new LdapConnection();

    try
    {
      connection.Connect(ldapHost, int.Parse(ldapPort));
      connection.Bind(loginDN, password);
    }
    catch(Exception e)
    {
      Console.WriteLine("Exception occurred: {0}", e.Message);
      try
      {
	connection.Disconnect();
      }
      catch(Exception e2)
      {
      }
      Environment.Exit(1);
    }

    Console.WriteLine(STARTING_PROMPT);
    
    string[] noAttrs = { LdapConnection.NO_ATTRS };
    
    // Make an object of PSearchEventSource
    PSearchEventSource objEventSource = 
      new PSearchEventSource(connection,
			     searchBase,
			     LdapConnection.SCOPE_SUB, // scope
			     "(objectClass=*)", // filter
			     noAttrs, // attrs
			     true, // typesOnly
			     null, // constraints
			     LdapEventType.LDAP_PSEARCH_ANY, // eventChangeType
			     true// changeonly
			     );
    
    // register MySearchResultEventHandler as the handler for the Search
    // result events...
    objEventSource.SearchResultEvent += new PSearchEventSource.SearchResultEventHandler(MySearchResultEventHandler);

    // Another listener could be added easily...
    objEventSource.SearchResultEvent += new PSearchEventSource.SearchResultEventHandler(MySearchResultEventHandler02);

    // Add a listener for Referral Event
    objEventSource.SearchReferralEvent += new PSearchEventSource.SearchReferralEventHandler(MySearchReferralEventHandler);
    
    // Add a listener for generic directory event
    objEventSource.DirectoryEvent += new PSearchEventSource.DirectoryEventHandler(MyDirectoryEventHandler);

    // Add a listener for exception event
    objEventSource.DirectoryExceptionEvent += new PSearchEventSource.DirectoryExceptionEventHandler(MyDirectoryExceptionEventHandler);
    
    string input;
    bool bContinue;
    do
    {
      Console.WriteLine(QUIT_PROMPT);
      input = Console.ReadLine();
      bContinue = (input != null) && !(input.StartsWith("q")) && !(input.StartsWith("Q"));
    } while(bContinue);

    // time to unregister
    objEventSource.SearchResultEvent -= new PSearchEventSource.SearchResultEventHandler(MySearchResultEventHandler);

    objEventSource.SearchResultEvent -= new PSearchEventSource.SearchResultEventHandler(MySearchResultEventHandler02);
    
    objEventSource.SearchReferralEvent -= new PSearchEventSource.SearchReferralEventHandler(MySearchReferralEventHandler);

    objEventSource.DirectoryEvent -= new LdapEventSource.DirectoryEventHandler(MyDirectoryEventHandler);

    objEventSource.DirectoryExceptionEvent -= new PSearchEventSource.DirectoryExceptionEventHandler(MyDirectoryExceptionEventHandler);
    
    // Disconnect
    try
    {
      connection.Disconnect();
    }
    catch(Exception e)
    {
    }
  }

  public void MySearchResultEventHandler(object source, SearchResultEventArgs objEventArgs)
  {
    Console.WriteLine("PSearchCallback::MySearchResultEventHandler Event classification = {0}", objEventArgs.EventClassification);
    Console.WriteLine("PSearchCallback::MySearchResultEventHandler Event type = {0}", objEventArgs.EventType);
    Console.WriteLine("PSearchCallback::MySearchResultEventHandler Entry DN = {0}", objEventArgs.Entry.DN);

    Console.WriteLine(QUIT_PROMPT);
  }

  public void MySearchResultEventHandler02(object source, SearchResultEventArgs objEventArgs)
  {
    Console.WriteLine("PSearchCallback::MySearchResultEventHandler02 Event type = {0}", objEventArgs.EventClassification);
    Console.WriteLine("PSearchCallback::MySearchResultEventHandler02 Event type = {0}", objEventArgs.EventType);
    Console.WriteLine("PSearchCallback::MySearchResultEventHandler02 Entry DN = {0}", objEventArgs.Entry.DN);

    Console.WriteLine(QUIT_PROMPT);
  }

  public void MySearchReferralEventHandler(object source, SearchReferralEventArgs objEventArgs)
  {
    Console.WriteLine("PSearchCallback::MySearchReferralEventHandler Event type = {0}", objEventArgs.EventClassification);
    Console.WriteLine("PSearchCallback::MySearchReferralEventHandler Event type = {0}", objEventArgs.EventType);

    Console.WriteLine(QUIT_PROMPT);
  }

  public void MyDirectoryEventHandler(object source, DirectoryEventArgs objEventArgs)
  {
    Console.WriteLine("PSearchCallback::MySearchDirectoryEventHandler Event classification = {0}", objEventArgs.EventClassification);

    Console.WriteLine(QUIT_PROMPT);
  }

  public void MyDirectoryExceptionEventHandler(object source, 
					       DirectoryExceptionEventArgs objEventArgs)
  {
    Console.WriteLine("PSearchCallback::MySearchDirectoryEventHandler DirectoryExceptionEvent = {0}", objEventArgs);
    Console.WriteLine("PSearchCallback::MySearchDirectoryEventHandler StackTrace = {0}", objEventArgs.LdapExceptionObject.StackTrace);
  }
}
