﻿/******************************************************************************
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

using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     Takes an LDAPIntermediateResponse and returns an object
    ///     (that implements the base class LDAPIntermediateResponse)
    ///     based on the OID.
    ///     You can then call methods defined in the child
    ///     class to parse the contents of the response.  The methods available
    ///     depend on the child class. All child classes inherit from the
    ///     LdapIntermediateResponse.
    /// </summary>
    public static class IntermediateResponseFactory
    {
        /// <summary>
        /// Used to Convert an RfcLDAPMessage object to the appropriate
        /// LDAPIntermediateResponse object depending on the operation being performed.
        /// </summary>
        /// <param name="inResponse">The LDAPIntermediateResponse object as returned by the
        /// extendedOperation method in the LDAPConnection object.</param>
        /// <returns>An object of base class LDAPIntermediateResponse.  The actual child
        /// class of this returned object depends on the operation being
        /// performed.</returns>
        /// <exception cref="LdapException">A general exception which includes an error message
        /// and an LDAP error code.</exception>
        public static LdapIntermediateResponse ConvertToIntermediateResponse(RfcLdapMessage inResponse)
        {
            var tempResponse = new LdapIntermediateResponse(inResponse);

            // Get the oid stored in the Extended response
            var inOid = tempResponse.GetId();
            if (inOid == null)
            {
                return tempResponse;
            }

            var regExtResponses =
                LdapIntermediateResponse.GetRegisteredResponses();
            if (regExtResponses.TryFindResponseExtension(inOid, out var extRespFactory))
            {
                return extRespFactory(inResponse);
            }

            // If we get here we did not have a registered extendedresponse
            // for this oid.  Return a default LDAPIntermediateResponse object.
            return tempResponse;
        }
    }
}
