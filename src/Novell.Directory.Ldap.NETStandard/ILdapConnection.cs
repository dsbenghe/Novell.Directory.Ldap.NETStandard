﻿using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Interface for all the minimal operations.
    /// </summary>
    public interface ILdapConnection : IDisposable, IDebugIdentifier
    {
        /// <summary>
        ///     Starts Transport Layer Security (TLS) protocol on this connection
        ///     to enable session privacy.
        ///     This affects the LdapConnection object and all cloned objects. A
        ///     socket factory that implements LdapTLSSocketFactory must be set on the
        ///     connection.
        /// </summary>
        /// <exception>
        ///     LdapException Thrown if TLS cannot be started.  If a
        ///     SocketFactory has been specified that does not implement
        ///     LdapTLSSocketFactory an LdapException is thrown.
        /// </exception>
        Task StartTlsAsync(CancellationToken ct = default);

        /// <summary>
        ///     Stops Transport Layer Security(TLS) on the LDAPConnection and reverts
        ///     back to an anonymous state.
        ///     @throws LDAPException This can occur for the following reasons:.
        ///     <UL>
        ///         <LI>StartTLS has not been called before stopTLS</LI>
        ///         <LI>
        ///             There exists outstanding messages that have not received all
        ///             responses
        ///         </LI>
        ///         <LI>The sever was not able to support the operation</LI>
        ///     </UL>
        ///     <p>
        ///         Note: The Sun and IBM implementions of JSSE do not currently allow
        ///         stopping TLS on an open Socket.  In order to produce the same results
        ///         this method currently disconnects the socket and reconnects, giving
        ///         the application an anonymous connection to the server, as required
        ///         by StopTLS
        ///     </p>
        /// </summary>
        Task StopTlsAsync(CancellationToken ct = default);

        /// <summary>
        ///     Synchronously adds an entry to the directory.
        /// </summary>
        /// <param name="entry">
        ///     LdapEntry object specifying the distinguished
        ///     name and attributes of the new entry.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task AddAsync(LdapEntry entry, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously adds an entry to the directory, using the specified
        ///     constraints.
        /// </summary>
        /// <param name="entry">
        ///     LdapEntry object specifying the distinguished
        ///     name and attributes of the new entry.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task AddAsync(LdapEntry entry, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) as an Ldapv3 bind, using the specified name and
        ///     password.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task BindAsync(string dn, string passwd, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password,
        ///     and Ldap version.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task BindAsync(int version, string dn, string passwd, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) as an Ldapv3 bind, using the specified name,
        ///     password, and constraints.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task BindAsync(string dn, string passwd, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password, Ldap version,
        ///     and constraints.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        ///     Note: the application should use care in the use
        ///     of String password objects.  These are long lived
        ///     objects, and may expose a security risk, especially
        ///     in objects that are serialized.  The LdapConnection
        ///     keeps no long lived instances of these objects.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task BindAsync(int version, string dn, string passwd, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password,
        ///     and Ldap version.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The version of the Ldap protocol to use
        ///     in the bind, use Ldap_V3.  Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task BindAsync(int version, string dn, byte[] passwd, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously authenticates to the Ldap server (that the object is
        ///     currently connected to) using the specified name, password, Ldap version,
        ///     and constraints.
        ///     If the object has been disconnected from an Ldap server,
        ///     this method attempts to reconnect to the server. If the object
        ///     has already authenticated, the old authentication is discarded.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task BindAsync(int version, string dn, byte[] passwd, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        /// Bind via a SASL Mechanism.
        /// </summary>
        /// <param name="saslRequest"></param>
        Task BindAsync(SaslRequest saslRequest, CancellationToken ct = default);

        IReadOnlyCollection<ISaslClientFactory> GetRegisteredSaslClientFactories();

        void RegisterSaslClientFactory(ISaslClientFactory saslClientFactory);

        bool IsSaslMechanismSupported(string mechanism);

        /// <summary>
        ///     Connects to the specified host and port.
        ///     If this LdapConnection object represents an open connection, the
        ///     connection is closed first before the new connection is opened.
        ///     At this point, there is no authentication, and any operations are
        ///     conducted as an anonymous client.
        ///     When more than one host name is specified, each host is contacted
        ///     in turn until a connection can be established.
        /// </summary>
        /// <param name="host">
        ///     A host name.
        /// </param>
        /// <param name="port">
        ///     The TCP or UDP port number to connect to or contact.
        ///     The default Ldap port is 389.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task ConnectAsync(string host, int port, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously deletes the entry with the specified distinguished name
        ///     from the directory.
        ///     Note: A Delete operation will not remove an entry that contains
        ///     subordinate entries, nor will it dereference alias entries.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to delete.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task DeleteAsync(string dn, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously deletes the entry with the specified distinguished name
        ///     from the directory, using the specified constraints.
        ///     Note: A Delete operation will not remove an entry that contains
        ///     subordinate entries, nor will it dereference alias entries.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to delete.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task DeleteAsync(string dn, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously disconnects from the Ldap server.
        ///     Before the object can perform Ldap operations again, it must
        ///     reconnect to the server by calling connect.
        ///     The disconnect method abandons any outstanding requests, issues an
        ///     unbind request to the server, and then closes the socket.
        /// </summary>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        void Disconnect();

        /// <summary>
        ///     Provides a synchronous means to access extended, non-mandatory
        ///     operations offered by a particular Ldapv3 compliant server.
        /// </summary>
        /// <param name="op">
        ///     The object which contains (1) an identifier of an extended
        ///     operation which should be recognized by the particular Ldap
        ///     server this client is connected to and (2)
        ///     an operation-specific sequence of octet strings
        ///     or BER-encoded values.
        /// </param>
        /// <returns>
        ///     An operation-specific object, containing an ID and either an octet
        ///     string or BER-encoded values.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task<LdapExtendedResponse> ExtendedOperationAsync(LdapExtendedOperation op, CancellationToken ct = default);

        /// <summary>
        ///     Provides a synchronous means to access extended, non-mandatory
        ///     operations offered by a particular Ldapv3 compliant server.
        /// </summary>
        /// <param name="op">
        ///     The object which contains (1) an identifier of an extended
        ///     operation which should be recognized by the particular Ldap
        ///     server this client is connected to and (2) an
        ///     operation-specific sequence of octet strings
        ///     or BER-encoded values.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     An operation-specific object, containing an ID and either an
        ///     octet string or BER-encoded values.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task<LdapExtendedResponse> ExtendedOperationAsync(LdapExtendedOperation op, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously makes a single change to an existing entry in the
        ///     directory.
        ///     For example, this modify method changes the value of an attribute,
        ///     adds a new attribute value, or removes an existing attribute value.
        ///     The LdapModification object specifies both the change to be made and
        ///     the LdapAttribute value to be changed.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modification.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mod">
        ///     A single change to be made to the entry.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task ModifyAsync(string dn, LdapModification mod, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously makes a single change to an existing entry in the
        ///     directory, using the specified constraints.
        ///     For example, this modify method changes the value of an attribute,
        ///     adds a new attribute value, or removes an existing attribute value.
        ///     The LdapModification object specifies both the change to be
        ///     made and the LdapAttribute value to be changed.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modification.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mod">
        ///     A single change to be made to the entry.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task ModifyAsync(string dn, LdapModification mod, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously makes a set of changes to an existing entry in the
        ///     directory.
        ///     For example, this modify method changes attribute values, adds
        ///     new attribute values, or removes existing attribute values.
        ///     Because the server applies all changes in an LdapModification array
        ///     atomically, the application can expect that no changes
        ///     have been performed if an error is returned.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modifications.
        /// </summary>
        /// <param name="dn">
        ///     Distinguished name of the entry to modify.
        /// </param>
        /// <param name="mods">
        ///     The changes to be made to the entry.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task ModifyAsync(string dn, LdapModification[] mods, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously makes a set of changes to an existing entry in the
        ///     directory, using the specified constraints.
        ///     For example, this modify method changes attribute values, adds new
        ///     attribute values, or removes existing attribute values.
        ///     Because the server applies all changes in an LdapModification array
        ///     atomically, the application can expect that no changes
        ///     have been performed if an error is returned.
        ///     If the request fails with {@link LdapException.CONNECT_ERROR},
        ///     it is indeterminate whether or not the server made the modifications.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to modify.
        /// </param>
        /// <param name="mods">
        ///     The changes to be made to the entry.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an
        ///     error message and an Ldap error code.
        /// </exception>
        Task ModifyAsync(string dn, LdapModification[] mods, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously reads the entry for the specified distiguished name (DN)
        ///     and retrieves all attributes for the entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server.
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found.
        /// </exception>
        Task<LdapEntry> ReadAsync(string dn, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously reads the entry for the specified distiguished name (DN),
        ///     using the specified constraints, and retrieves all attributes for the
        ///     entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server.
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found.
        /// </exception>
        Task<LdapEntry> ReadAsync(string dn, LdapSearchConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously reads the entry for the specified distinguished name (DN)
        ///     and retrieves only the specified attributes from the entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <param name="attrs">
        ///     The names of the attributes to retrieve.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server.
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found.
        /// </exception>
        Task<LdapEntry> ReadAsync(string dn, string[] attrs, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously reads the entry for the specified distinguished name (DN),
        ///     using the specified constraints, and retrieves only the specified
        ///     attributes from the entry.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to retrieve.
        /// </param>
        /// <param name="attrs">
        ///     The names of the attributes to retrieve.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     the LdapEntry read from the server.
        /// </returns>
        /// <exception>
        ///     LdapException if the object was not found.
        /// </exception>
        Task<LdapEntry> ReadAsync(string dn, string[] attrs, LdapSearchConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task RenameAsync(string dn, string newRdn, bool deleteOldRdn, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory, using the
        ///     specified constraints.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task RenameAsync(string dn, string newRdn, bool deleteOldRdn, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory, possibly
        ///     repositioning the entry in the directory tree.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="newParentdn">
        ///     The distinguished name of an existing entry which
        ///     is to be the new parent of the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task RenameAsync(string dn, string newRdn, string newParentdn, bool deleteOldRdn, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously renames an existing entry in the directory, using the
        ///     specified constraints and possibly repositioning the entry in the
        ///     directory tree.
        /// </summary>
        /// <param name="dn">
        ///     The current distinguished name of the entry.
        /// </param>
        /// <param name="newRdn">
        ///     The new relative distinguished name for the entry.
        /// </param>
        /// <param name="newParentdn">
        ///     The distinguished name of an existing entry which
        ///     is to be the new parent of the entry.
        /// </param>
        /// <param name="deleteOldRdn">
        ///     If true, the old name is not retained as an
        ///     attribute value. If false, the old name is
        ///     retained as an attribute value.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the operation.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task RenameAsync(string dn, string newRdn, string newParentdn, bool deleteOldRdn, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously performs the search specified by the parameters.
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:.
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     Search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     Names of attributes to retrieve.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found. If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task<ILdapSearchResults> SearchAsync(string @base, int scope, string filter, string[] attrs, bool typesOnly, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously performs the search specified by the parameters,
        ///     using the specified search constraints (such as the
        ///     maximum number of entries to find or the maximum time to wait for
        ///     search results).
        ///     As part of the search constraints, the method allows specifying
        ///     whether or not the results are to be delivered all at once or in
        ///     smaller batches. If specified that the results are to be delivered in
        ///     smaller batches, each iteration blocks only until the next batch of
        ///     results is returned.
        /// </summary>
        /// <param name="base">
        ///     The base distinguished name to search from.
        /// </param>
        /// <param name="scope">
        ///     The scope of the entries to search. The following
        ///     are the valid options:.
        ///     <ul>
        ///         <li>SCOPE_BASE - searches only the base DN</li>
        ///         <li>SCOPE_ONE - searches only entries under the base DN</li>
        ///         <li>
        ///             SCOPE_SUB - searches the base DN and all entries
        ///             within its subtree
        ///         </li>
        ///     </ul>
        /// </param>
        /// <param name="filter">
        ///     The search filter specifying the search criteria.
        /// </param>
        /// <param name="attrs">
        ///     The names of attributes to retrieve.
        /// </param>
        /// <param name="typesOnly">
        ///     If true, returns the names but not the values of
        ///     the attributes found.  If false, returns the
        ///     names and values for attributes found.
        /// </param>
        /// <param name="cons">
        ///     The constraints specific to the search.
        /// </param>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task<ILdapSearchResults> SearchAsync(string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously checks to see if an entry contains an attribute
        ///     with a specified value.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to use in the
        ///     comparison.
        /// </param>
        /// <param name="attr">
        ///     The attribute to compare against the entry. The
        ///     method checks to see if the entry has an
        ///     attribute with the same name and value as this
        ///     attribute.
        /// </param>
        /// <returns>
        ///     True if the entry has the value,
        ///     and false if the entry does not
        ///     have the value or the attribute.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task<bool> CompareAsync(string dn, LdapAttribute attr, CancellationToken ct = default);

        /// <summary>
        ///     Synchronously checks to see if an entry contains an attribute with a
        ///     specified value, using the specified constraints.
        /// </summary>
        /// <param name="dn">
        ///     The distinguished name of the entry to use in the
        ///     comparison.
        /// </param>
        /// <param name="attr">
        ///     The attribute to compare against the entry. The
        ///     method checks to see if the entry has an
        ///     attribute with the same name and value as this
        ///     attribute.
        /// </param>
        /// <param name="cons">
        ///     Constraints specific to the operation.
        /// </param>
        /// <returns>
        ///     True if the entry has the value,
        ///     and false if the entry does not
        ///     have the value or the attribute.
        /// </returns>
        /// <exception>
        ///     LdapException A general exception which includes an error
        ///     message and an Ldap error code.
        /// </exception>
        Task<bool> CompareAsync(string dn, LdapAttribute attr, LdapConstraints cons, CancellationToken ct = default);

        /// <summary>
        ///     Indicates whether the object has authenticated to the connected Ldap
        ///     server.
        /// </summary>
        /// <returns>
        ///     True if the object has authenticated; false if it has not
        ///     authenticated.
        /// </returns>
        bool Bound { get; }

        /// <summary>
        ///     Indicates whether the connection represented by this object is open
        ///     at this time.
        /// </summary>
        /// <returns>
        ///     True if connection is open; false if the connection is closed.
        /// </returns>
        bool Connected { get; }

        /// <summary>
        ///     Returns a copy of the set of search constraints associated with this
        ///     connection. These constraints apply to search operations performed
        ///     through this connection (unless a different set of
        ///     constraints is specified when calling the search operation method).
        /// </summary>
        /// <returns>
        ///     The set of default search constraints that apply to
        ///     this connection.
        /// </returns>
        /// <seealso cref="SearchConstraints">
        /// </seealso>
        /// <seealso cref="LdapSearchConstraints">
        /// </seealso>
        LdapSearchConstraints SearchConstraints { get; }
    }
}
