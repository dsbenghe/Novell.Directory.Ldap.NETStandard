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
// Samples.Extensions.GetEffectivePrivileges.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
using System;
using  Novell.Directory.Ldap;
using GetEffectivePrivilegesRequest = Novell.Directory.Ldap.Extensions.GetEffectivePrivilegesRequest;
using GetEffectivePrivilegesResponse = Novell.Directory.Ldap.Extensions.GetEffectivePrivilegesResponse;

/// <summary>  The following sample demonstrates how get the rights that
/// a trustee object has on the object.
/// 
/// </summary>
public class GetEffectivePrivileges
{
	
	public static void  Main(System.String[] args)
	{
		
		if (args.Length != 6)
		{
			System.Console.Error.WriteLine("Usage:   mono GetEffectivePrivileges " + "<host Name> <port number> <login dn> " + "\n         <password> <object dn> <trustee dn>");
			System.Console.Error.WriteLine("Example: mono GetEffectivePrivileges Acme.com 389 " + "\"cn=Admin,o=Acme\" secret\n         " + "\"cn=james,o=Acme\" " + "\"cn=admin,o=Acme\"");
			System.Environment.Exit(1);
		}
		
		int LdapVersion = LdapConnection.Ldap_V3;
		System.String LdapHost = args[0];
		int LdapPort = System.Int32.Parse(args[1]);
		System.String loginDN = args[2];
		System.String password = args[3];
		System.String objectDN = args[4];
		System.String trusteeDN = args[5];
		int iRight = 0;
		System.String sRight = null;
		LdapConnection ld = new LdapConnection();
		
		try
		{
			// connect to the server
			ld.Connect(LdapHost, LdapPort);
			// bind to the server
			ld.Bind(LdapVersion, loginDN, password);
			System.Console.Out.WriteLine("\nLogin succeeded");
			
			// user can choose from:
			//   1. object rights(represented as [Entry Rights]);
			//   2. attribute rights(represented as [All Attributes Rights];
			//   3. a single attribute name like 'acl'
			//String rightName = "[Entry Rights]"
			//String rightName = "[All Attributes Rights]";
			System.String rightName = "acl";
			
			LdapExtendedOperation request = new GetEffectivePrivilegesRequest(objectDN, trusteeDN, rightName);
			
			LdapExtendedResponse response = ld.ExtendedOperation(request);
			
			if (response.ResultCode == LdapException.SUCCESS && (response is GetEffectivePrivilegesResponse))
			{
				iRight = ((GetEffectivePrivilegesResponse) response).Privileges;
				
				if (rightName.ToUpper().Equals("[Entry Rights]".ToUpper()))
					sRight = "object rights";
				else if (rightName.ToUpper().Equals("[All Attributes Rights]".ToUpper()))
					sRight = "attribute rights";
				else
					sRight = rightName;
				
				System.Console.Out.WriteLine("\"" + trusteeDN + "\" has the following" + " rights on \"" + objectDN + "\"s '" + sRight + "':");
				PrintRights(rightName, iRight);
				System.Console.Out.WriteLine("\nGet Effective Privileges succeeded");
			}
			else
			{
				System.Console.Out.WriteLine("Get Effective Privileges Failed");
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
	}
	
	// PrintRights() parses and prints the effective rights
	public static void  PrintRights(System.String aName, int rights)
	{
		
		System.Text.StringBuilder rString = new System.Text.StringBuilder();
		
		if (aName.ToUpper().Equals("[Entry Rights]".ToUpper()))
		{
			// decode object rights
			rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_BROWSE) != 0?"BrowseEntry: true; ":"BrowseEntry: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_ADD) != 0?"AddEntry: true; ":"AddEntry: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_DELETE) != 0?"DeleteEntry: true; ":"DeleteEntry: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_RENAME) != 0?"RenameEntry: true; ":"RenameEntry: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_SUPERVISOR) != 0?"Supervisor: true; ":"Supervisor: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_INHERIT_CTL) != 0?"Inherit_ctl: true.":"Inherit_ctl: false.");
		}
		else
		{
			// decode attribute rights no matter it's for 
			// all attributes or a single attribute
			rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_COMPARE) != 0?"CompareAttributes: true; ":"CompareAttributes: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_READ) != 0?"ReadAttributes: true; ":"ReadAttributes: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_WRITE) != 0?"Write/Add/DeleteAttributes: true; ":"Write/Add/DeleteAttributes: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_SELF) != 0?"Add/DeleteSelf: true; ":"Add/DeleteSelf: false; ");
			rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_SUPERVISOR) != 0?"Supervisor: true.":"Supervisor: false.");
		}
		
//		System.Console.Out.WriteLine(rString);
	}
}

