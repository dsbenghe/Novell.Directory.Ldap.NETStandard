using System.Collections.Generic;
using System.Threading.Tasks;

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
    public interface ILdapSearchResults : IAsyncEnumerable<LdapEntry>
    {
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
