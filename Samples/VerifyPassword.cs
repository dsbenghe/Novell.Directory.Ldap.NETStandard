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
// Samples.Bind.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
/// VerifyPassword.cs
/// Description:    The VerifyPassword.cs sample verifies that a password is
/// correct for the given entry.
/// 
/// We simply compare the password with the "userPassword"
/// attribute of the entry using the Ldap compare function.
/// 
using System;
using Novell.Directory.Ldap;

public class VerifyPassword
{
	public static void  Main(System.String[] args)
	{
		if (args.Length != 5)
		{
			System.Console.Out.WriteLine("Usage:   mono VerifyPassword <host name>" + " <login dn> <password> <object dn>\n" + "         <test password>");
			System.Console.Out.WriteLine("Example: mono VerifyPassword Acme.com " + "\"cn=Admin,o=Acme\" secret\n" + "         \"cn=JSmith,ou=Sales,o=Acme\" testPassword");
			System.Environment.Exit(0);
		}
		
		int ldapPort = LdapConnection.DEFAULT_PORT;
		int ldapVersion = LdapConnection.Ldap_V3;
		System.String ldapHost = args[0];
		System.String loginDN = args[1];
		System.String password = args[2];
		System.String objectDN = args[3];
		System.String testPassword = args[4];
		LdapConnection conn = new LdapConnection();
		
		try
		{
			// connect to the server
			conn.Connect(ldapHost, ldapPort);
			
			// authenticate to the server
			conn.Bind(ldapVersion, loginDN, password);
			
			LdapAttribute attr = new LdapAttribute("userPassword", testPassword);
			bool correct = conn.Compare(objectDN, attr);
			
			System.Console.Out.WriteLine(correct?"The password is correct.":"The password is incorrect.\n");
			
			// disconnect with the server
			conn.Disconnect();
		}
		catch (LdapException e)
		{
			if (e.ResultCode == LdapException.NO_SUCH_OBJECT)
			{
				System.Console.Error.WriteLine("Error: No such entry");
			}
			else if (e.ResultCode == LdapException.NO_SUCH_ATTRIBUTE)
			{
				System.Console.Error.WriteLine("Error: No such attribute");
			}
			else
			{
				System.Console.Error.WriteLine("Error: " + e.ToString());
			}
		}
		catch (System.IO.IOException e)
		{
			System.Console.Out.WriteLine("Error: " + e.ToString());
		}
		System.Environment.Exit(0);
	}
}
