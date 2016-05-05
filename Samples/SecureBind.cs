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
// Samples.SecureBind.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//
using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;

namespace Samples
{
class SecureBind
{

	SecureBind(){}

	static void Main(string[] args)
	{

		if ( args.Length != 4)
		{
			Console.WriteLine("Usage:   mono SecureBind <host name> <ldap port>  <login dn>" + " <password> \n");
			Console.WriteLine("Example: mono SecureBind Acme.com 636"  + " \"cn=admin,o=Acme\"" + " secret \n");
			Console.WriteLine("Import the server Trusted Root Certificate in Mono trust store using certmgr.exe utility e.g.\n");
			Console.WriteLine("certmgr -add -c Trust /home/exports/TrustedRootCert.cer\n");
			return;
		}

		string ldapHost = args[0];
		int ldapPort = System.Convert.ToInt32(args[1]);
		String loginDN  = args[2];
		String password = args[3];
		LdapConnection conn=null;
		try
		{
			conn= new LdapConnection();
			conn.SecureSocketLayer=true;
			Console.WriteLine("Connecting to:" + ldapHost);
			conn.Connect(ldapHost,ldapPort);
			conn.Bind(loginDN,password);
			Console.WriteLine(" SSL Bind Successfull");			
		}
		catch(Exception e)
		{
			Console.WriteLine("Error:" + e.Message);			
	        }
		conn.Disconnect();
	}
}
}


