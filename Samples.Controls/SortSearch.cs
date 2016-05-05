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
// Samples.Controls.SortSearch.cs
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

class SortSearch
{

	static void Main(string[] args)
	{

		if ( args.Length != 6)
		{
			Console.WriteLine("Usage:   mono SortSearch <host name> <ldap port>  <login dn>" + " <password> <search base>" + " <search filter>");
			Console.WriteLine("Example: mono SortSearch Acme.com 389"  + " \"cn=admin,o=Acme\"" + " secret \"ou=sales,o=Acme\"" + "         \"(objectclass=*)\"");
			return;
        }

	    string ldapHost = args[0];
		int ldapPort = System.Convert.ToInt32(args[1]);
	    String loginDN  = args[2];
	    String password = args[3];
        String searchBase = args[4];
	    String searchFilter = args[5];

		String[] attrs = new String[1];            
		attrs[0] = "sn";

		LdapSortKey[] keys = new LdapSortKey[1];
		keys[0] = new LdapSortKey( "sn" );
          
        try
        {

			LdapConnection conn= new LdapConnection();
			conn.Connect(ldapHost,ldapPort);
			conn.Bind(loginDN,password);


			// Create a LDAPSortControl object - Fail if cannot sort
			LdapSortControl sort = new LdapSortControl( keys, true );
            
			// Set the Sort control to be sent as part of search request
			LdapSearchConstraints cons = conn.SearchConstraints;
			cons.setControls( sort );
			conn.Constraints = cons;

			Console.WriteLine("Connecting to:" + ldapHost);
			LdapSearchResults lsc=conn.Search(	searchBase,
												LdapConnection.SCOPE_SUB,
												searchFilter,
												attrs,
												false,
												(LdapSearchConstraints) null );

			while (lsc.hasMore())
			{
				LdapEntry nextEntry = null;
				try 
				{
					nextEntry = lsc.next();
				}
				catch(LdapException e) 
				{
					Console.WriteLine("Error: " + e.LdapErrorMessage);
					// Exception is thrown, go for next entry
				continue;
				}
				Console.WriteLine("\n" + nextEntry.DN);
				LdapAttributeSet attributeSet = nextEntry.getAttributeSet();
				System.Collections.IEnumerator ienum=attributeSet.GetEnumerator();
				while(ienum.MoveNext())
				{
					LdapAttribute attribute=(LdapAttribute)ienum.Current;
       				string attributeName = attribute.Name;
					string attributeVal = attribute.StringValue;
       		        Console.WriteLine( attributeName + "value:" + attributeVal);
				}
			}

			conn.Disconnect();
		}
        catch(LdapException e)
        {
			Console.WriteLine("Error:" + e.LdapErrorMessage);
			Console.WriteLine("Error:" + e.ToString());
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

