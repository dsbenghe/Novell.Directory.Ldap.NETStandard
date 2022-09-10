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
// Samples.AddUserToGroup.cs
//
// Author:
//   Vincent Boots (vincent.boots@innovative.nl)
//
//


using Novell.Directory.Ldap;

namespace Samples;

internal static class AddUserToGroup
{
    private static void Main(string[] args)
    {
        if (args.Length != 6)
        {
            Console.WriteLine("Usage:   sampleApp  <host name> <ldap port>  <login dn>  <password> <userdn> <groupdn>");
            Console.WriteLine("Example sampleApp example.com 389 CN=Administrator,CN=Users,DC=example,DC=com StrongPassword CN=exampleUser,CN=Users,DC=example,DC=com CN=Users,DC=example,DC=com CN=ExampleGroup,CN=Users,DC=example,DC=com CN=Users,DC=example,DC=com"" );
            return;
        }

        string ldapHost = args[0];
        int    ldapPort = Convert.ToInt32(args[1]);
        string loginDN  = args[2];
        string password = args[3];
        string userDn   = args[4];
        string groupDn  = args[5];


        var ldapConnection = SetLdapConnection(ldapHost, ldapPort, loginDN, password).GetAwaiter().GetResult();
        AddUserToGroupAsync(ldapConnection, userDn, groupDn);
        ldapConnection.Disconnect();
    }

    private static async Task<LdapConnection> SetLdapConnection(string ldapHost, int ldapPort, string loginDN, string password)
    {
        var conn = new LdapConnection();
        try
        {
            Console.WriteLine("Connecting to:" + ldapHost);
            await conn.ConnectAsync(ldapHost, ldapPort);
            await conn.BindAsync(loginDN, password);
            Console.WriteLine(" Bind Successfull");
        }
        catch (LdapException e)
        {
            throw new LdapException(e.LdapErrorMessage);
        }
        catch (Exception e)
        {
            throw new LdapException(e.Message);
        }

        return conn;
    }

    private static async Task<bool> AddUserToGroupAsync(LdapConnection conn, System.String userdn, System.String groupdn)
    {
        // modifications for group
        LdapModification[] modGroup = new LdapModification[1];

        // Add modifications to modGroup
        LdapAttribute member = new LdapAttribute("member", userdn);
        modGroup[0] = new LdapModification(LdapModification.Add, member);
        try
        {
            // Modify the group's attributes
            await conn.ModifyAsync(groupdn, modGroup);
            Console.WriteLine("Modified the group's attribute.");
        }
        catch (LdapException e)
        {
            Console.WriteLine("Failed to modify group's attributes: " + e.LdapErrorMessage);
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error:" + e.Message);
            return false;
        }

        return true;
    }
}
