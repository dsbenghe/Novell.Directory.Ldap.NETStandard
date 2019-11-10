# Changelog

### 3.0.0 - pre release
* change ILdapConnection interface to improve testability & mockability
* changes to the public api of the library to improve usability and make it more .net style
* IPv6 support
* SASL support - thanks to Michael Stum - https://github.com/mstum
* Support for ReferralFollowing - thanks to Joseph Petersen - https://github.com/casz
* Add LocalCertificateSelectionCallback - thanks to bmoore - https://github.com/barry-r-moore

### 2.3.8
* Added connect timeout

### 2.3.7
* Disconnect method added to ILdapConnection

### 2.3.6
* The build matrix is running the stress tests in three ways: with no transport security, with SSL and with TLS.
* Improve TLS support - it used to throw exception in some rare conditions.
* Added new tests for connect with/without SSL/TLS 
* Added new tests for search
* Some smaller cleanups


### 2.3.5
* Added stress tests which are running the functional tests on multiple threads - running in Travis CI using OpenLDAP in a build matrix with different number of threads
* Added functional tests which are running in CI using OpenLDAP as ldap server
* Fixed reader thread hanging when disposing ldap connection after an unsuccesful bind (not sure if was happening in the original library as I didnt check it but very likely it did)
* Code cleanup: delete useless/incorrect finalizers and some general cleanup
* Free the write semaphore in finally

### 2.3.3 Fix two issues happening also in the original library
* Fix crashing of reader thread when stopping the thread because of unhandled exception (the reader thread was expecting IOException but not ObjectDisposedException)
* Fix race condition causing null reference on dispose of LdapConnection

### 2.3.1
* Built against the lowest possible version of .NET Standard: 1.3
* ILdapConnection introduced


### 2.3.0 Initial version built against the release version of .NET Core 1.0. The main changes for porting were around:
* Thread usage: the library was extensively using Abort, Interrupt, ThreadInterruptedException, ... - which is not recommended and also not supported in .NET Core.
* Serialization support for a limited number of types was deleted; not supported on .net core
* Ssl support: the library was using Mono.Security for this. Now is implemented using SslStream from NetStandard
* Implement IDisposable for LdapConnection to allow usage of "using" construct

## Original changelog before .NET Standard conversion

### 2009-07-14 Palaniappan N <npalaniappan@novell.com>
* The fix for the crash while disconnecting has been checked in. The fix is to implement iDisposambe interface and use 'Dispose' method to shut down the connection.

### 2009-07-07 Palaniappan N <npalaniappan@novell.com>
* New LDAP extension (specific to eDirectory) GetEffectivePrivilegesList has been added. eDirectory supports this from version 8.8 SP5. GetEffectivePrivilegesListRequest.cs processes the extended request. And GetEffectivePrivilegesListResponse.cs processes the extended response. GetEffectivePrivilegesList.cs is the sample that explains the extension.

### 2009-07-06 Palaniappan N <npalaniappan@novell.com>
* Changed version from 2.1.10 to 2.2.1 in Connection.cs
* Changed version from 2.1.10 to 2.2.1 in AssemblyInfo.cs
	
### 2008-03-14 Palaniappan N <npalaniappan@novell.com>
* Changed version from 2.1.9 to 2.1.10 in Connection.cs
* Changed version from 2.1.9 to 2.1.10 in AssemblyInfo.cs

### 2007-12-19 Palaniappan N <npalaniappan@novell.com>
* Fix for the issue of getting occasional -5875 error on the server when disconnecting the client.

### 2007-12-19 Palaniappan N <npalaniappan@novell.com>
* InteractiveSSL.cs has been made to check the	certificate store, if the certificate is present and valid before adding the new certificate to the Trust store.
		
### 2007-12-19 Palaniappan N <npalaniappan@novell.com>
* In the searchResultEventArgs.ToString(), the function classification was hard coded earlier. This has been fixed.
	
### 2007-12-05 Palaniappan N <npalaniappan@novell.com>
* Exception messages have been thrown in case of connection failure
	
### 2007-10-18 Palaniappan N <npalaniappan@novell.com>
* Changed version from 2.1.8 to 2.1.9 in Connection.cs
* Changed version from 2.1.8 to 2.1.9 in AssemblyInfo.cs

### 2007-09-24 Palaniappan N <npalaniappan@novell.com>
* conn.Disconnect() has been added in the samples InteractiveSSL.cs and SecureBind.cs to disconnect the connections properly even in case of the exceptions. The same will be done in all the samples in the upcoming release.

### 2007-09-19 Palaniappan N <npalaniappan@novell.com>
* Added a try/finally section in the file connection.cs to release the semId semaphore in case the connection fails.  	

### 2007-09-10 Palaniappan N <npalaniappan@novell.com>
* Exception has been thrown in case of supply of negative sleep interval.Change done in the file LdapEventSource.cs

### 2007-08-28 Palaniappan N <npalaniappan@novell.com>
* Assembly loader to load Mono.Security has been modified to pick up the proper one. Change done in the Connection.cs file

### 2007-03-07 Palaniappan N <npalaniappan@novell.com>
* Changed version from 2.1.7 to 2.1.8 in Connection.cs
* Changed version from 2.1.7 to 2.1.8 in AssemblyInfo.cs

### 2006-12-11 Palaniappan N <npalaniappan@novell.com>
* A fix for the bug which deals about the exceptions caused while using events with lots of create/modify events
* A new property BinaryData has been implemented in the ValueEventData.cs class to enable applications retrieve the binary data as such from the ASN1OctetString with out converting it in to a String

### 2006-11-22 Palaniappan N <npalaniappan@novell.com>
* Done a fix in DN.cs by correcting the misplaced decrement operator which caused malfunctioning of isDescendantOf() method

### 2006-10-12 Palaniappan N <npalaniappan@novell.com>
* Added a new sample ListGroups.cs
* Added a new sample SetPassword.cs
* Added a new sample AsynchronousSortControl.cs 
in Controls
* Added a new sample SimplePassword.cs in Controls

### 2006-09-05 Palaniappan N <npalaniappan@novell.com>
* Added a new sample ModifyACL.cs
* Added a new catch block in Connection.cs to catch the socket exceptions.
* Checked the condition, whether the sockets created by BOTH SSL and cleartext connections are open / null in Connection.cs
* Changes made in LdapResponse.cs so to monitor the events which caused problems with eDirectory 8.8 SP1 release, because of the LdapResponse structure.
* Modification done in RfcFilter.cs to parse the special charectes too.
* The usage of the command line arguments have been corrected in the samples AddEntry.cs, Bind.cs, CompareAttrs.cs, DelEntry.cs, GetAttributeSchema.cs, InteractiveSSL.cs, ModifyEntry.cs, ModifyPass.cs, RenameEntry.cs, Search.cs, SecureBind.cs, StartTLS.cs, PSearchCallback.cs, SearchPersist.cs, SortSearch.cs, VLVControl.cs
* Changed version from 2.1.6 to 2.1.7 in Connection.cs
* Changed version from 2.1.6 to 2.1.7 in AssemblyInfo.cs

### 2006-06-21 Palaniappan N <npalaniappan@novell.com>
* Added support for Backup-Restore of LDAP Objects
* Added new sample GetLdapBackupRestore.cs in the extensions to support Backup-Restore Feature
* Added new sample ClientSideSort.cs to support client side sorting of the entries
* Added new sample CompareAttrs.cs to compare the attributes
* Added new sample DynamicGroup.cs to support Dynamic Grouping of entries
* Added new sample List.cs to get all the entries of a specific container
* Changed version from 2.1.5 to 2.1.6 in Connection.cs
* Changed version from 2.1.5 to 2.1.6 in AssemblyInfo.cs

### 2006-03-27 Palaniappan N <npalaniappan@novell.com>
* Removed the assembly reference to Mono.Security.dll which caused the mismatch and added the appropriate reference

### 2006-03-03 Palaniappan N <npalaniappan@novell.com>
* Added new sample EdirEventSample.cs in the extensions to support event handling
* Modified the sample SearchPersist.cs to monitor the changes properly and unwanted namespaces are removed
* Changed version from 2.1.4 to 2.1.5 in Connection.cs.
* Changed version from 2.1.4 to 2.1.5 in AssemblyInfo.cs
	
### 2005-11-09 Palaniappan N <npalaniappan@novell.com>
* Connection.cs is changed so as to load the Mono.Security.dll even from a non-default location (path)

### 2005-09-23 Palaniappan N <npalaniappan@novell.com>
* Changes done in the samples PartitionEntryCount.cs,	VLVControl.cs, SearchPersist.cs and AddUserToGroup.cs to catch the uncaught exceptions as the fixes for the bugs #1291,#1292 and #1293 (from forge site)

### 2005-09-21 Palaniappan N <npalaniappan@novell.com>
* Changed version from 2.1.3 to 2.1.4 in Connection.cs
* Changed version from 2.1.3 to 2.1.4 in AssemblyInfo.cs

### 2005-09-14 Palaniappan N <npalaniappan@novell.com>
* The sample InteractiveSSL.cs is updated so as to give the client , the provision of seeing the server details to which it is trying to connect and to decide whether to proceed the SSL handshake or not. If it is to be proceeded, the server's certificate will be imported automatically to the client's Trust Store
 
### 2005-09-13 Palaniappan N <npalaniappan@novell.com>
* Fix for bugs #1174, #1175 (from forge site) made - Connection.cs class is modified by synchronizing the stream threads so as to avoid the memory consumption and handle consumption

### 2005-07-19 Anil Bhatia <banil@novell.com>
* Implementation and sample for Interactive SSL Support - provision for application to decide whether to proceed with SSL Handshake

### 2005-07-19 Anil Bhatia <banil@novell.com>
* fix for bug [#1022] from forge site. The fix seems to work for the time being

### 2005-07-11 Anil Bhatia <banil@novell.com>
* Support for Error Messages
* Removed hard coded dependency on Mono Security
* Fix for a race condition in Connection.cs

#### 2005-01-19 Anil Bhatia <banil@novell.com>
* Added support for subordinate subtree scope. Classes changed for this are LdapConnection and LdapUrl. Ref: http://www.ietf.org/internet-drafts/draft-sermersheim-ldap-subordinate-scope-01.txt
	
### 2005-01-14 Anil Bhatia <banil@novell.com>
* Changed version from 2.1.1 to 2.1.2 in Connection.cs.
* Changed version from 1.0.0. to 2.1.2 in AssemblyInfo.cs

### 2005-01-05 Anil Bhatia <banil@novell.com>
* Added support for error code 113 SSL_HANDSHAKE_FAILED

### 2004-09-16 Anil Bhatia <banil@novell.com>
* Added Support for LDAP and eDir events.
* Changes in Connection.cs regarding appropriate handling in method ServerCertificateValidation. This is required for proper execution of SecureBind sample

### 2004-04-14  Sunil Kumar  <sunilk@novell.com>
* Added Support for SSL/TLS

### 2004-03-31  Sunil Kumar  <sunilk@novell.com>
* Added support for LDAP Extension registrations

### 2003-14-12  Sunil Kumar  <sunilk@novell.com>
* Added Support for XML documentation

### 2003-10-12  Sunil Kumar  <sunilk@novell.com>
* Added Support for Schema Related Operations

### 2003-21-11  Sunil Kumar  <sunilk@novell.com>
* Changed the linux build script to replace corlib.dll with mscorlib.dll

### 2003-16-11  Sunil Kumar  <sunilk@novell.com>
* Added LDAP controls in Novell.Directory.Ldap.Controls
* Added Samples for LDAP controls

### 2003-15-11  Sunil Kumar  <sunilk@novell.com>	
* Changed the LdapConnection public Method name as per MS naming convention
* Added the Authors description	

### 2003-14-11  Sunil Kumar  <sunilk@novell.com>
* Changed the Directory structure and Class file names as per the Microsoft Standards	
* Changed the build scripts accordingly
