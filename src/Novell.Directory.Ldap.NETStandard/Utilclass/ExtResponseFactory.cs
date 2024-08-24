using Novell.Directory.Ldap.Rfc2251;
using System;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     Takes an LdapExtendedResponse and returns an object
    ///     (that implements the base class ParsedExtendedResponse)
    ///     based on the OID.
    ///     <p>
    ///         You can then call methods defined in the child
    ///         class to parse the contents of the response.  The methods available
    ///         depend on the child class. All child classes inherit from the
    ///         ParsedExtendedResponse.
    ///     </p>
    /// </summary>
    public static class ExtResponseFactory
    {
        /// <summary>
        ///     Used to Convert an RfcLdapMessage object to the appropriate
        ///     LdapExtendedResponse object depending on the operation being performed.
        /// </summary>
        /// <param name="inResponse">
        ///     The LdapExtendedReponse object as returned by the
        ///     extendedOperation method in the LdapConnection object.
        /// </param>
        /// <returns>
        ///     An object of base class LdapExtendedResponse.  The actual child
        ///     class of this returned object depends on the operation being
        ///     performed.
        /// </returns>
        public static LdapExtendedResponse ConvertToExtendedResponse(RfcLdapMessage inResponse)
        {
            var tempResponse = new LdapExtendedResponse(inResponse);

            // Get the oid stored in the Extended response
            var inOid = tempResponse.Id;
            if (inOid == null)
            {
                return tempResponse;
            }

            var regExtResponses = LdapExtendedResponse.RegisteredResponses;
            if (regExtResponses.TryFindResponseExtension(inOid, out var responseFactory))
            {
                try
                {
                    return responseFactory(inResponse);
                }
                catch (Exception e)
                {
                    // Could not create the ResponseControl object
                    // All possible exceptions are ignored. We fall through
                    // and create a default LdapControl object
                    Logger.Log.LogWarning("Exception swallowed", e);
                }
            }

            // If we get here we did not have a registered extendedresponse
            // for this oid.  Return a default LdapExtendedResponse object.
            return tempResponse;
        }
    }
}
