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
// Samples.Extensions.GetReplicaInfo.cs
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
using GetReplicaInfoRequest = Novell.Directory.Ldap.Extensions.GetReplicaInfoRequest;
using GetReplicaInfoResponse = Novell.Directory.Ldap.Extensions.GetReplicaInfoResponse;

/// <summary>  The following sample demonstrates how to get information about a
/// replica that resides on a specific server.
/// 
/// </summary>
public class GetReplicaInfo
{
	
	[STAThread]
	public static void  Main(System.String[] args)
	{
		
		if (args.Length != 6)
		{
			System.Console.Error.WriteLine("Usage:    mono GetReplicaInfo <host Name> " + "<port number> <login dn> <password>\n        " + " <partition DN> <server ND>");
			System.Console.Error.WriteLine("Example:  mono GetReplicaInfo Acme.com 389 " + "\"cn=Admin,o=Acme\" secret\n         " + "\"ou=Sales,o=Acme\" \"cn=myServer,o=Acme\"");
			System.Environment.Exit(1);
		}
		
		int ldapVersion = LdapConnection.Ldap_V3;
		System.String ldapHost = args[0];
		int ldapPort = System.Int32.Parse(args[1]);
		System.String loginDN = args[2];
		System.String password = args[3];
		System.String partitionDN = args[4];
		System.String serverDN = args[5];
		int intInfo;
		System.String strInfo;
		LdapConnection ld = new LdapConnection();
		
		try
		{
			// connect to the server
			ld.Connect(ldapHost, ldapPort);
			// bind to the server
			ld.Bind(ldapVersion, loginDN, password);
			System.Console.Out.WriteLine("\nLogin succeeded");
			
			LdapExtendedOperation request = new GetReplicaInfoRequest(serverDN, partitionDN);
			
			LdapExtendedResponse response = ld.ExtendedOperation(request);
			
			if ((response.ResultCode == LdapException.SUCCESS) && (response is GetReplicaInfoResponse))
			{
				System.Console.Out.WriteLine("Repica Info:");
				strInfo = ((GetReplicaInfoResponse) response).getpartitionDN();
				System.Console.Out.WriteLine("    Partition DN: " + strInfo);
				intInfo = ((GetReplicaInfoResponse) response).getpartitionID();
				System.Console.Out.WriteLine("    Partition ID: " + intInfo);
				intInfo = ((GetReplicaInfoResponse) response).getreplicaState();
				System.Console.Out.WriteLine("    Replica state: " + intInfo);
				intInfo = ((GetReplicaInfoResponse) response).getmodificationTime();
				System.Console.Out.WriteLine("    Modification Time: " + intInfo);
				intInfo = ((GetReplicaInfoResponse) response).getpurgeTime();
				System.Console.Out.WriteLine("    Purge Time: " + intInfo);
				intInfo = ((GetReplicaInfoResponse) response).getlocalPartitionID();
				System.Console.Out.WriteLine("    Local partition ID: " + intInfo);
				intInfo = ((GetReplicaInfoResponse) response).getreplicaType();
				System.Console.Out.WriteLine("    Replica Type: " + intInfo);
				intInfo = ((GetReplicaInfoResponse) response).getflags();
				System.Console.Out.WriteLine("    Flags: " + intInfo);
				System.Console.Out.WriteLine("\nget replica information succeeded\n");
			}
			else
			{
				System.Console.Out.WriteLine("Could not get replica information.\n");
				throw new LdapException(response.ErrorMessage, response.ResultCode, (System.String) null);
			}
			
			/* Done, so disconnect */
			if (ld.Connected)
				ld.Disconnect();
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine("Error: " + e.ToString());
		}
	}
}
