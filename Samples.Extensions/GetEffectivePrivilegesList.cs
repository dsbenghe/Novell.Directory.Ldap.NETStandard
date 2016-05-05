/******************************************************************************
* The MIT License
* Copyright (c) 2009 Novell Inc.  www.novell.com
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
// Samples.Extensions.GetEffectivePrivilegesList.cs
//
// Author:
//   Arpit Rastogi (Rarpit@novell.com)
//
// (C) 2009 Novell, Inc (http://www.novell.com)
//
using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Extensions;

namespace Samples.Extensions
{
	class GetEffectivePrevilegesList 
	{
		static void Main( string[] args ) 
		{
			if (args.Length != 6)
			{
				System.Console.Error.WriteLine("Usage:   mono GetEffectivePrivilegesList " + "<host Name> <port number> <login dn> " + "\n         <password> <object dn> <trustee dn>");
				System.Console.Error.WriteLine("Example: mono GetEffectivePrivilegesList Acme.com 389 " + "\"cn=Admin,o=Acme\" secret\n         " + "\"cn=james,o=Acme\" " + "\"cn=admin,o=Acme\"");
				System.Environment.Exit(1);
			}
			int    ldapVersion = LdapConnection.Ldap_V3;
			System.String ldapHost    = args[0];
			int    ldapPort    = System.Int32.Parse(args[1]);
			System.String loginDN     = args[2];
			System.String password    = args[3];
			System.String objectDN    = args[4];
			System.String trusteeDN   = args[5];
			int[]    iRight      = {0};
			System.String[] sRight      = null;
			LdapConnection ld  = new LdapConnection();
			try 
			{
				// connect to the server
				ld.Connect(ldapHost, ldapPort);
				// bind to the server
				ld.Bind(ldapVersion, loginDN, password);
				System.Console.Out.WriteLine("\nLogin succeeded");
				// user can choose from:
				//   1. object rights(represented as [Entry Rights]);
				//   2. attribute rights(represented as [All Attributes Rights];
				//   3. a single attribute name like 'acl'
				//String rightName = "{[Entry Rights],null}"
				//String rightName = "{[All Attributes Rights],null}";
				//String rightName = "{attr1,attr2,attr3,.... ,null}"
				System.String[] rightName = {"acl","cn","dn",null};
				LdapExtendedOperation request = new GetEffectivePrivilegesListRequest(objectDN,trusteeDN,rightName);
				LdapExtendedResponse response = ld.ExtendedOperation(request); 
			            
				if ( response.ResultCode == LdapException.SUCCESS && 
					( response is GetEffectivePrivilegesListResponse )) 
				{
					iRight = ((GetEffectivePrivilegesListResponse)response).getPrivileges();
					if(iRight.Length == (rightName.Length-1))
					{
						sRight = new System.String[iRight.Length];
						for ( int i =0 ; rightName[i] != null ; i++)
						{
							if ( rightName[i].ToUpper().Equals("[Entry Rights]".ToUpper()) )
								sRight[i] = "object rights";
							else if ( rightName[i].ToUpper().Equals("[All Attributes Rights]".ToUpper()))
								sRight[i] = "attribute rights";
							else
								sRight[i] = rightName[i];
						}
						System.Console.WriteLine("\"" + trusteeDN + "\" has the following rights on \""+ objectDN+"\'s ");
						for(int i=0;rightName[i]!=null;i++)
						{
							System.Console.WriteLine("'" + sRight[i] + "':");
							PrintRights( rightName[i], iRight[i] );
							System.Console.WriteLine("\nGet Effective Privileges succeeded");
						}
					}
					else
					{
						System.Console.WriteLine("You have provided the wrong input in terms of attribute list");
					}
				}                   
				else 
				{                
					System.Console.WriteLine("Get Effective Privileges List Failed");
					throw new LdapException( response.ErrorMessage, response.ResultCode, (System.String) null);
				}
				
				/* Done, so disconnect */
				if ( ld.Connected )
					ld.Disconnect();
			}
			catch( LdapException e ) 
			{
				System.Console.Out.WriteLine("Error: " + e.LdapErrorMessage);
			}
			
		}
		// PrintRights() parses and prints the effective rights one by one
		public static void PrintRights( String aName, int rights )
		{
			System.Text.StringBuilder rString = new System.Text.StringBuilder();
			if ( aName.ToUpper().Equals("[Entry Rights]".ToUpper())) 
			{
				// decode object rights
				rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_BROWSE) != 0 ? "BrowseEntry: true; ":"BrowseEntry: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_ADD) != 0 ? "AddEntry: true; ":"AddEntry: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_DELETE) != 0 ? "DeleteEntry: true; ":"DeleteEntry: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_RENAME) != 0 ? "RenameEntry: true; ":"RenameEntry: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_SUPERVISOR) != 0 ? "Supervisor: true; ":"Supervisor: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ENTRY_INHERIT_CTL) != 0 ? "Inherit_ctl: true.":"Inherit_ctl: false.");
			}
			else 
			{
				// decode attribute rights no matter it's for 
				// all attributes or a single attribute
				rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_COMPARE) != 0 ? "CompareAttributes: true; ": "CompareAttributes: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_READ) != 0 ? "ReadAttributes: true; ":"ReadAttributes: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_WRITE) != 0 ? "Write/Add/DeleteAttributes: true; ":"Write/Add/DeleteAttributes: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_SELF) != 0 ? "Add/DeleteSelf: true; ":"Add/DeleteSelf: false; ");
				rString.Append((rights & LdapDSConstants.LDAP_DS_ATTR_SUPERVISOR) != 0 ? "Supervisor: true.":"Supervisor: false.");            
			}         
			System.Console.WriteLine(rString);
		}
	}
}
