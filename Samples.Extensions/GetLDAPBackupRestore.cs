/******************************************************************************
* The MIT License
* Copyright (c) 2006 Novell Inc.  www.novell.com
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
// Samples.Extensions.GetLdapBackupRestore.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Extensions;

/**
 *  The following sample demonstrates how to do Object based backup and restore 
 *  on a LDAP server using LDAP extensions.
 *
 */

public class GetLdapBackupRestore 
{

	/**
	 *  Default constructor
	 */

	public GetLdapBackupRestore(): base()
	{
		return;
	}

	public static void Main(String[] args) 
	{
		if (!(args.Length == 5 || args.Length == 6 || args.Length == 7)) 
		{
            Console.Error.WriteLine("Usage: mono GetLdapBackupRestore <host Name> "
                              + "<port number> <login dn> <password>\n "
                              + " <object DN> [encrypted password (optional)]" +
                              		" [state Info (optional)]");
            Console.Error.WriteLine("\nFor Non encrypted objects::");
            Console.Error.WriteLine("--------------------------");
            Console.Error.WriteLine("	Example: mono GetLdapBackupRestore Acme.com 389 "
                              + "\"cn=Admin,o=Acme\" secret\n "
                              + "\"cn=TestUser,o=Acme\"");
            Console.Error.WriteLine("\nFor Encrypted objects::");
            Console.Error.WriteLine("----------------------");
            Console.Error.WriteLine("	Example: mono GetLdapBackupRestore Acme.com 389 "
                              + "\"cn=Admin,o=Acme\" secret\n "
                              + "\"cn=TestUser,o=Acme\" testpassword");
            Environment.Exit(1);
        }

		int ldapVersion = LdapConnection.Ldap_V3;
		String ldapHost = args[0];
		int ldapPort = int.Parse(args[1]);
		String loginDN = args[2];
		String password = args[3];
		String objectDN = args[4];
		String encPasswd = null; 
		String stateInfo = null; 
		
		if(args.Length == 6 && args[5] != null)
		{
			encPasswd = args[5];
		}
		if(args.Length == 7 && args[6] != null)
		{
			stateInfo = args[6];
		}
		

		//Create a LdapConnection object
		LdapConnection ld = new LdapConnection();
		
		if(ldapPort == 636)
			ld.SecureSocketLayer = true;

		try 
		{
			// connect to the server
			ld.Connect(ldapHost, ldapPort);
		
			// bind to the server
			ld.Bind(ldapVersion, loginDN, password.ToString());
			Console.WriteLine("\nLogin succeeded");
			Console.WriteLine("\n Object DN =" + objectDN);
			
			//Call backup method
			ArrayList objectBuffer;
			if(encPasswd == null)
				objectBuffer = backup(ld, objectDN, null, stateInfo);
			else
				objectBuffer = backup(ld, objectDN, SupportClass.ToByteArray(encPasswd), stateInfo);
						
			//Call restore method
			if(encPasswd == null)
				restore(ld, objectDN, null, objectBuffer);
			else
				restore(ld, objectDN, SupportClass.ToByteArray(encPasswd), objectBuffer);
			
			/* Done, so disconnect */
			if (ld.Connected)
				ld.Disconnect();

		} 
		catch (LdapException e) 
		{
			Console.WriteLine("Error: " + e.ToString());
		} 
		catch (System.Exception e) 
		{
			Console.WriteLine("Error: " + e.ToString());
		}

	}

	/**
	 *
	 * Constructs an extended operation object for getting data about any Object
	 * and make a call to ld.ExtendedOperation to get the response <br>
	 * 
	 */

	public static ArrayList backup(LdapConnection ld, String objectDN,
			byte[] passwd, String stateInfo) 
	{
		int intInfo;
		String strInfo;
		byte[] returnedBuffer; //Actual data blob returned as byte[]
		ArrayList objectBuffer = new ArrayList(4);
		objectBuffer.Insert(0, new Integer32(-1)); //Mark the rc default as failed backup
		try 
		{
			LdapExtendedOperation request = new LdapBackupRequest(objectDN,
					passwd, stateInfo);

			LdapExtendedResponse response = ld.ExtendedOperation(request);
			
			int result = response.ResultCode;
			objectBuffer.Remove(0);
			objectBuffer.Insert(0, new Integer32(result));			

			if ((result == LdapException.SUCCESS) && (response is LdapBackupResponse)) 
			{			
				Console.WriteLine("Backup Info:");

				strInfo = ((LdapBackupResponse) response).getStatusInfo();
				Console.WriteLine("    Status Info: " + strInfo);

				intInfo = ((LdapBackupResponse) response).getBufferLength();
				Console.WriteLine("    Buffer length: " + intInfo);
				objectBuffer.Insert(1, new Integer32(intInfo));

				strInfo = ((LdapBackupResponse) response).getChunkSizesString();
				Console.WriteLine("    Chunk sizes: " + strInfo);
				objectBuffer.Insert(2, strInfo);	
				
				returnedBuffer = ((LdapBackupResponse) response).getReturnedBuffer();
				objectBuffer.Insert(3, returnedBuffer);

				Console.WriteLine("\nInformation backed up successfully\n");
			
			} 
			else 
			{
				Console.WriteLine("Could not backup the information.\n");
				throw new LdapException(response.ErrorMessage, response.ResultCode, (String) null);
			}
			
		} 
		catch (LdapException e) 
		{
			Console.WriteLine("Error: " + e.ToString());
		}
		catch (System.Exception e) 
		{
			Console.WriteLine("Error: " + e.ToString());
		}
		return objectBuffer;
	}
	
	/**
	 *
	 * Constructs an extended operation object for restoring data of retreived 
	 * Object and make a call to ld.extendedOperation to get the response <br>
	 * 
	 */
	
	public static void restore(LdapConnection ld, String objectDN, 
			byte[] passwd, ArrayList objectBuffer) 
	{
		try 
		{			
			if(((Integer32)objectBuffer[0]).intValue != 0)
			{
				Console.WriteLine("Note: The test program did not proceed " +
						"with restore since backup was not proper");
				Environment.Exit(0);
			}
						
			LdapExtendedOperation request = new LdapRestoreRequest(
					objectDN, passwd,
					(((Integer32)objectBuffer[1]).intValue),
					(string)objectBuffer[2],
					(byte[])objectBuffer[3]);

			LdapExtendedResponse response = ld.ExtendedOperation(request);
			
			if ( response.ResultCode == LdapException.SUCCESS )
			    Console.WriteLine("Object restored successfully\n");
			else 
			{
			    Console.WriteLine("Restore Request Failed");
			    throw new LdapException( response.ErrorMessage,
			                             response.ResultCode,
			                             (string)null);
			}
				
		} 
		catch (LdapException e) 
		{
			Console.WriteLine("Error: " + e.ToString());
		}
		catch (System.Exception e) 
		{
			Console.WriteLine("Error: " + e.ToString());
		}
	}
}