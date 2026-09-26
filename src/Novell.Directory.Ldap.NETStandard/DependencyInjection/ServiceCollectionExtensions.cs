using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using Novell.Directory.Ldap.ObjectPool;
using System;

namespace Novell.Directory.Ldap.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an LDAP connection pool to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the LDAP connection pool to.</param>
    public static void AddLdapConnectionPool(this IServiceCollection serviceCollection)
    {
        if (serviceCollection is null)
        {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

        var maximumRetained = Environment.ProcessorCount * 2;
        serviceCollection.AddLdapConnectionPool(maximumRetained);
    }

    /// <summary>
    /// Adds an LDAP connection pool to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to add the LDAP connection pool to.</param>
    /// <param name="maximumRetained">The maximum number of objects to retain in the pool.</param>
    public static void AddLdapConnectionPool(this IServiceCollection serviceCollection, int maximumRetained)
    {
        if (serviceCollection is null)
        {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

        serviceCollection.AddTransient<ILdapConnection, LdapConnection>(_ => new LdapConnection());
        serviceCollection.TryAddSingleton<ObjectPool<ILdapConnection>>(serviceProvider =>
        {
            var policy = new DependencyInjectionPooledObjectPolicy<ILdapConnection, LdapConnection>(serviceProvider);
            return new DefaultObjectPool<ILdapConnection>(policy, maximumRetained);
        });
    }
}
