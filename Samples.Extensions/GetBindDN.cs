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
// Samples.Extensions.GetBindDN.cs
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
using GetBindDNRequest = Novell.Directory.Ldap.Extensions.GetBindDNRequest;
using GetBindDNResponse = Novell.Directory.Ldap.Extensions.GetBindDNResponse;

/// <summary>  The following sample demonstrates how to get the distinguished name
/// of the object you used when making the Ldap_bind call.
/// 
/// </summary>
public class GetBindDN
{
	
	[STAThread]
	public static void  Main(System.String[] args)
	{
		
		if (args.Length != 4)
		{
			System.Console.Error.WriteLine("Usage:   mono GetBindDN " + "<host Name> <port number> <login dn>" + "\n              <password>");
			System.Console.Error.WriteLine("Example: mono GetBindDN Acme.com " + "389 \"cn=Admin,o=Acme\" secret");
			System.Environment.Exit(1);
		}
		
		int LdapVersion = LdapConnection.Ldap_V3;
		System.String LdapHost = args[0];
		int LdapPort = System.Int32.Parse(args[1]);
		System.String loginDN = args[2];
		System.String password = args[3];
		LdapConnection ld = new LdapConnection();
		
		try
		{
			// connect to the server
			ld.Connect(LdapHost, LdapPort);
			// bind to the server
			ld.Bind(LdapVersion, loginDN, password);
			System.Console.Out.WriteLine("\nLogin succeeded");
			
			LdapExtendedOperation request = new GetBindDNRequest();
			
			LdapExtendedResponse response = ld.ExtendedOperation(request);
			
			if (((response.ResultCode) == LdapException.SUCCESS) && (response is GetBindDNResponse))
			{
				System.Console.Out.WriteLine("You were logged in as: " + ((GetBindDNResponse) response).Identity);
				System.Console.Out.WriteLine("\nGetBindDN succeeded.\n");
			}
			else
			{
				System.Console.Out.WriteLine("GetBindDN failed.\n");
				throw new LdapException(response.ErrorMessage, response.ResultCode, (System.String) null);
			}
			
			/* Done, so disconnect */
			if (ld.Connected)
				ld.Disconnect();
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine("\nError: " + e.LdapErrorMessage);
		}
	}
}
