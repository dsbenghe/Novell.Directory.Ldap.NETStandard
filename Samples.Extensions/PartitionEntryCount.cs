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
// Samples.Extensions.PartitionEntryCount.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
using System;
using LdapConnection = Novell.Directory.Ldap.LdapConnection;
using LdapException = Novell.Directory.Ldap.LdapException;
using LdapExtendedOperation = Novell.Directory.Ldap.LdapExtendedOperation;
using LdapExtendedResponse = Novell.Directory.Ldap.LdapExtendedResponse;
using PartitionEntryCountRequest = Novell.Directory.Ldap.Extensions.PartitionEntryCountRequest;
using PartitionEntryCountResponse = Novell.Directory.Ldap.Extensions.PartitionEntryCountResponse;

/// <summary>  The following sample demonstrates how to count the number of
/// objects in a partition using Novell Ldap Extensions.
/// 
/// </summary>
public class PartitionEntryCount
{
	
	[STAThread]
	public static void  Main(System.String[] args)
	{
		
		if (args.Length != 5)
		{
			System.Console.Error.WriteLine("Usage:   mono PartitionEntryCount <host Name> " + "<port number> <login dn> <password>" + "\n         <partition dn>");
			System.Console.Error.WriteLine("Example: mono PartitionEntryCount Acme.com 389 " + "\"cn=Admin,o=Acme\" secret" + "\n         \"ou=Sales,o=Acme\"");
			System.Environment.Exit(1);
		}
		
		int LdapVersion = LdapConnection.Ldap_V3;
		System.String LdapHost = args[0];
		int LdapPort = System.Int32.Parse(args[1]);
		System.String loginDN = args[2];
		System.String password = args[3];
		System.String partitionDN = args[4];
		int count = 0;
		LdapConnection ld = new LdapConnection();
		
		try
		{
			// connect to the server
			ld.Connect(LdapHost, LdapPort);
			// bind to the server
			ld.Bind(LdapVersion, loginDN, password);
			System.Console.Out.WriteLine("\nLogin succeeded");
			
			LdapExtendedOperation request = new PartitionEntryCountRequest(partitionDN);
			
			LdapExtendedResponse response = ld.ExtendedOperation(request);
			
			if ((response.ResultCode == LdapException.SUCCESS) && (response is PartitionEntryCountResponse))
			{
				count = ((PartitionEntryCountResponse) response).Count;
				System.Console.Out.WriteLine("\n    Entry count of partition " + partitionDN + " is: " + count);
				
				System.Console.Out.WriteLine("\nPartitionEntryCount succeeded\n");
			}
			else
			{
				System.Console.Out.WriteLine("\nPartitionEntryCount Failed");
				throw new LdapException(response.ErrorMessage, response.ResultCode, (System.String) null);
			}
			
			/* Done, so disconnect */
			if (ld.Connected)
				ld.Disconnect();
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine("Error: " + e.LdapErrorMessage);
		}
		catch(Exception e)
		{
			Console.WriteLine("Error:" + e.Message);
			return;
		}
	}
}
