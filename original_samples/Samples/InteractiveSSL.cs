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
// Samples.InteractiveSSL.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//
using System;
using Syscert = System.Security.Cryptography.X509Certificates;

using Novell.Directory.Ldap;

using Novell.Directory.Ldap.Utilclass;
using Mono.Security.X509;
using Mono.Security.Cryptography;

namespace Samples
{
class InteractiveSSL
{
	protected static bool bHowToProceed, quit = false, removeFlag = false;
	protected static int bindCount = 0;

	InteractiveSSL() {}

	static void Main( string[] args )
	{

		if ( args.Length != 4 )
		{
			Console.WriteLine("Usage:   mono InteractiveSSL <host name> <ldap port>  <login dn> <password>\n");
			Console.WriteLine("Example: mono InteractiveSSL Acme.com 636"  + " \"cn=admin,o=Acme\"" + " secret\n");
			return;
		}
		LdapConnection conn=null;
		string ldapHost = args[0];
		int ldapPort = System.Convert.ToInt32(args[1]);
		String loginDN  = args[2];
		String password = args[3];
		bHowToProceed = true;
		String continueBind;
				
		try
		{
			do
			{
				bindCount++;
				conn= new LdapConnection();
				conn.SecureSocketLayer=true;
				Console.WriteLine( "Connecting to:" + ldapHost );
			
				conn.UserDefinedServerCertValidationDelegate += new
					CertificateValidationCallback(MySSLHandler);
				if(bHowToProceed == false)
					conn.Disconnect();
				if(bHowToProceed == true) 
				{
					conn.Connect(ldapHost,ldapPort);
					conn.Bind(loginDN,password);
					Console.WriteLine( " SSL Bind Successfull " );
					conn.Disconnect();
				}	
				
				Console.WriteLine ( "\nDo you wish to Bind again to the server (y/n)?" );
				continueBind = Console.ReadLine();
				
				if(continueBind == "y" || continueBind == "Y")
					quit = false;
				if(continueBind == "n" || continueBind == "N")
					quit = true;
					
			}while(quit == false);
		}
		catch(LdapException ee)
		{
			Console.WriteLine(ee.LdapErrorMessage);			
		}
		catch(Exception e) 
		{
			Console.WriteLine(e.StackTrace);
		}
		conn.Disconnect();
	}
	
	public static bool MySSLHandler(Syscert.X509Certificate certificate,
					int[] certificateErrors)
	{
		
		X509Store store = null;
		X509Stores stores = X509StoreManager.CurrentUser;
		String input;
		store = stores.TrustedRoot;
		
		
		//Import the details of the certificate from the server.
		
		X509Certificate x509 = null;
		X509CertificateCollection coll = new X509CertificateCollection ();
		byte[] data = certificate.GetRawCertData();
		if (data != null)			
			x509 = new X509Certificate (data);
		
		//List the details of the Server
		
		//check for ceritficate in store
		X509CertificateCollection check = store.Certificates;
		if(!check.Contains(x509))
		{
			if(bindCount == 1)
			{
				Console.WriteLine ( " \n\nCERTIFICATE DETAILS: \n" );
				Console.WriteLine ( " {0}X.509 v{1} Certificate", (x509.IsSelfSigned ? "Self-signed " : String.Empty), x509.Version);
				Console.WriteLine ( "  Serial Number: {0}", CryptoConvert.ToHex (x509.SerialNumber));
				Console.WriteLine ( "  Issuer Name:   {0}", x509.IssuerName);
				Console.WriteLine ( "  Subject Name:  {0}", x509.SubjectName);
				Console.WriteLine ( "  Valid From:    {0}", x509.ValidFrom);
				Console.WriteLine ( "  Valid Until:   {0}", x509.ValidUntil);
				Console.WriteLine ( "  Unique Hash:   {0}", CryptoConvert.ToHex (x509.Hash));
				Console.WriteLine ();			
			}	
			
			//Get the response from the Client
			do
			{
				Console.WriteLine("\nDo you want to proceed with the connection (y/n)?");
				input = Console.ReadLine();
				if(input=="y" || input == "Y")
					bHowToProceed = true;
				if(input=="n" || input == "N")
					bHowToProceed = false;			
			}while(input!="y" && input != "Y" && input !="n" && input != "N");
		}	
		else
		{
			if(bHowToProceed == true)
			{
				//Add the certificate to the store.
			
				if (x509 != null)
					coll.Add (x509);
				store.Import (x509);
				if(bindCount == 1)
					removeFlag = true;			
			}
		}
		if(bHowToProceed == false)
		{
			//Remove the certificate added from the store.
			
			if(removeFlag == true && bindCount > 1)
			{				
				foreach (X509Certificate xt509 in store.Certificates) {
					if (CryptoConvert.ToHex (xt509.Hash) == CryptoConvert.ToHex (x509.Hash)) {
						store.Remove (x509);
					}				
				}
			}	
			Console.WriteLine("SSL Bind Failed.");
		}	
		return bHowToProceed;
	}
}	
}