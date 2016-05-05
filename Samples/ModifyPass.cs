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
// Samples.ModifyPass.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
/// description:    The ModifyPass.cs sample shows how to modify your own
/// password, giving both the old and new password.
/// 
/// Unless the caller has admin privileges, Novell eDirectory
/// requires both the old and new passwords in order to change
/// a password.

using System;
using Novell.Directory.Ldap;
namespace Samples
{

class AddEntry
{

	static void Main(string[] args)
	{

		if ( args.Length != 5)
		{
			Console.WriteLine("Usage:   mono ModifyPass <host name> <ldap port>  <login dn>" + " <old password> <new password>");
			Console.WriteLine("Example: mono ModifyPass Acme.com 389"  + " \"cn=tjhon,o=Acme\"" + " secret \"newpass\"");
			return;
        }

	    string ldapHost = args[0];
	    int ldapPort = System.Convert.ToInt32(args[1]);
	    String loginDN  = args[2];
	    String opassword = args[3];
	    String npassword = args[4];

        try
        {
            LdapConnection conn= new LdapConnection();
            Console.WriteLine("Connecting to:" + ldapHost);
            conn.Connect(ldapHost,ldapPort);
            conn.Bind(loginDN,opassword);
            LdapModification[] modifications = new LdapModification[2];
	    LdapAttribute deletePassword = new LdapAttribute("userPassword", opassword);
            modifications[0] = new LdapModification(LdapModification.DELETE, deletePassword);
            LdapAttribute addPassword = new LdapAttribute("userPassword", npassword);
            modifications[1] = new LdapModification(LdapModification.ADD, addPassword);
                                                                                
            conn.Modify(loginDN, modifications);
                                                                                
            System.Console.Out.WriteLine("Your password has been modified.");

	    conn.Disconnect();
        }
        catch(LdapException e)
        {
            Console.WriteLine("Error:" + e.LdapErrorMessage);
			return;
        }
        catch(Exception e)
        {
            Console.WriteLine("Error:" + e.Message);
            return;
        }
	}
}
}

