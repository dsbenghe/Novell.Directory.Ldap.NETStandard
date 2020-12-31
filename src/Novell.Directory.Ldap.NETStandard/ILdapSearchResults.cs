using System.Collections.Generic;

namespace Novell.Directory.Ldap
{
    /// <inheritdoc />
    /// <summary>
    ///     An ILdapSearchResults interface is returned from a synchronous search
    ///     operation. It provides access to all results received during the
    ///     operation (entries and exceptions).
    /// </summary>
    /// <seealso cref="!:LdapConnection.Search">
    /// </seealso>
    public interface ILdapSearchResults : IEnumerable<LdapEntry>
    {
        /// <summary>
        ///     Returns a count of the items in the search result.
        ///     Returns a count of the entries and exceptions remaining in the object.
        ///     If the search was submitted with a batch size greater than zero,
        ///     getCount reports the number of results received so far but not enumerated
        ///     with next().  If batch size equals zero, getCount reports the number of
        ///     items received, since the application thread blocks until all results are
        ///     received.
        /// </summary>
        /// <returns>
        ///     The number of items received but not retrieved by the application.
        /// </returns>
        int Count { get; }

        /// <summary>
        ///     Reports if there are more search results.
        /// </summary>
        /// <returns>
        ///     true if there are more search results.
        /// </returns>
        bool HasMore();

        /// <summary>
        ///     Returns the next result as an LdapEntry.
        ///     If automatic referral following is disabled or if a referral
        ///     was not followed, next() will throw an LdapReferralException
        ///     when the referral is received.
        /// </summary>
        /// <returns>
        ///     The next search result as an LdapEntry.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        /// <exception>
        ///     LdapReferralException A referral was received and not
        ///     followed.
        /// </exception>
        LdapEntry Next();

        /// <summary>
        ///     Returns the latest server controls returned by the server
        ///     in the context of this search request, or null
        ///     if no server controls were returned.
        /// </summary>
        /// <returns>
        ///     The server controls returned with the search request, or null
        ///     if none were returned.
        /// </returns>
        LdapControl[] ResponseControls { get; }
    }
}
