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
using Novell.Directory.Ldap.Events.Edir;
using Novell.Directory.Ldap.Events.Edir.EventData;

public class EdirEventsCallback
{
  public const string STARTING_PROMPT = 
  "Registering for eDirectory Events...";
  public const string QUIT_PROMPT = 
  "\nMonitoring changes... Enter \"q\" or \"Q\" to quit:";

  public static void Main(string[] args)
  {
    if (args.Length != 4)
    {
      Console.WriteLine("Usage:mono EdirEventsCallback.exe <host name> <port number> <login dn> <password> ");

	return;
    }

    EdirEventsCallback callback = new EdirEventsCallback();
    callback.Execute(args[0], args[1], args[2], args[3]);
  }

  protected void Execute(string ldapHost, 
			 string ldapPort,
			 string loginDN,
			 string password)
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

    EdirEventSpecifier[] specifier = new EdirEventSpecifier[1];
    specifier[0] = new EdirEventSpecifier(
					  EdirEventType.EVT_CREATE_ENTRY,
					  EdirEventResultType.EVT_STATUS_ALL
					  //, we could have optionally specified a filter here like "(attributeName=city)"
					  );
    
    // Make an object of EdirEventSource
    EdirEventSource objEventSource = new EdirEventSource(specifier, connection);

    // register for events
    objEventSource.EdirEvent += new EdirEventSource.EdirEventHandler(MyEdirEventHandler);

    // Another listener can be easily added
    objEventSource.EdirEvent += new EdirEventSource.EdirEventHandler(MyEdirEventHandler02);

    // Add a listener for generic directory event
    objEventSource.DirectoryEvent += new EdirEventSource.DirectoryEventHandler(MyDirectoryEventHandler);

    // Add a listener for exception event
    objEventSource.DirectoryExceptionEvent += new EdirEventSource.DirectoryExceptionEventHandler(MyDirectoryExceptionEventHandler);

    string input;
    bool bContinue;
    do
    {
      Console.WriteLine(QUIT_PROMPT);
      input = Console.ReadLine();
      bContinue = (input != null) && !(input.StartsWith("q")) && !( input.StartsWith("Q"));
    } while(bContinue);

    // time to unregister
    objEventSource.EdirEvent -= new EdirEventSource.EdirEventHandler(MyEdirEventHandler);

    objEventSource.EdirEvent -= new EdirEventSource.EdirEventHandler(MyEdirEventHandler02);

    objEventSource.DirectoryEvent -= new EdirEventSource.DirectoryEventHandler(MyDirectoryEventHandler);

    objEventSource.DirectoryExceptionEvent -= new EdirEventSource.DirectoryExceptionEventHandler(MyDirectoryExceptionEventHandler);

    // Disconnect
    try
    {
      connection.Disconnect();
    }
    catch(Exception e)
    {
    }
  }

  public void MyEdirEventHandler(object source, EdirEventArgs objEventArgs)
  {
    Console.WriteLine("EdirEventsCallback::MyEdirEventHandler Event classification = {0}", objEventArgs.EventClassification);    

    EdirEventIntermediateResponse objIR = objEventArgs.IntermediateResponse;
    if (null != objIR)
    {
      Console.WriteLine("EdirEventsCallback::MyEdirEventHandler EventType = {0}", objIR.EventType);

      // is there any data object associated with the event...
      BaseEdirEventData objResponseDataObject = objIR.EventResponseDataObject;
      if (null != objResponseDataObject)
      {
	Console.WriteLine("EdirEventsCallback::MyEdirEventHandler Type of data object = {0}", objResponseDataObject.EventDataType);
      }

      Console.WriteLine("EdirEventsCallback::MyEdirEventHandler EventResultType = {0}", objIR.EventResultType);

      // Now objResponseDataObject can be casted to appropriate type depending
      // upon the value of objResponseDataObject.EventDataType
    }

    Console.WriteLine(QUIT_PROMPT);  
  }

  public void MyEdirEventHandler02(object source, EdirEventArgs objEventArgs)
  {
    Console.WriteLine("EdirEventsCallback::MyEdirEventHandler02");
  }

  public void MyDirectoryEventHandler(object source, DirectoryEventArgs objEventArgs)
  {
    Console.WriteLine("EdirEventsCallback::MyDirectoryEventHandler Event classification = {0}", objEventArgs.EventClassification);

    Console.WriteLine(QUIT_PROMPT);
  }

  public void MyDirectoryExceptionEventHandler(object source, 
					       DirectoryExceptionEventArgs objEventArgs)
  {
    Console.WriteLine("EdirEventsCallback::MyDirectoryExceptionEventHandler DirectoryExceptionEvent = {0}", objEventArgs);
    Console.WriteLine("EdirEventsCallback::MyDirectoryExceptionEventHandler StackTrace = {0}", objEventArgs.LdapExceptionObject.StackTrace);
  }

}
