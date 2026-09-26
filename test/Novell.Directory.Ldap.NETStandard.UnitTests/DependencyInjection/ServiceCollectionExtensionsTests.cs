// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Novell.Directory.Ldap.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.DependencyInjection
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddLdapConnectionPool_when_serviceCollection_null_throws_ArgumentNullException()
        {
            IServiceCollection serviceCollection = null;

            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddLdapConnectionPool());
        }

        [Fact]
        public void AddLdapConnectionPool_with_maximumRetained_when_serviceCollection_null_throws_ArgumentNullException()
        {
            IServiceCollection serviceCollection = null;

            Assert.Throws<ArgumentNullException>(() => serviceCollection.AddLdapConnectionPool(10));
        }

        [Fact]
        public void AddLdapConnectionPool_registers_ILdapConnection_as_transient()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLdapConnectionPool();

            var descriptor = serviceCollection.Single(d => d.ServiceType == typeof(ILdapConnection));
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void AddLdapConnectionPool_resolves_ILdapConnection_as_LdapConnection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLdapConnectionPool();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var connection = serviceProvider.GetRequiredService<ILdapConnection>();

            Assert.IsType<LdapConnection>(connection);
        }

        [Fact]
        public void AddLdapConnectionPool_registers_ObjectPool_as_singleton()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLdapConnectionPool();

            var descriptor = serviceCollection.Single(d => d.ServiceType == typeof(ObjectPool<ILdapConnection>));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void AddLdapConnectionPool_without_maximumRetained_resolves_pool_using_default_based_on_processor_count()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLdapConnectionPool();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Resolving forces the TryAddSingleton factory to run, exercising the
            // Environment.ProcessorCount * 2 default and the delegation to the
            // (serviceCollection, maximumRetained) overload.
            var pool = serviceProvider.GetRequiredService<ObjectPool<ILdapConnection>>();

            Assert.IsType<DefaultObjectPool<ILdapConnection>>(pool);
        }

        [Fact]
        public void AddLdapConnectionPool_with_maximumRetained_resolves_pool_successfully()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLdapConnectionPool(1);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var pool = serviceProvider.GetRequiredService<ObjectPool<ILdapConnection>>();

            Assert.IsType<DefaultObjectPool<ILdapConnection>>(pool);
        }

        [Fact]
        public void AddLdapConnectionPool_pool_returns_connections_resolved_from_container()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLdapConnectionPool(1);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var pool = serviceProvider.GetRequiredService<ObjectPool<ILdapConnection>>();

            var connection = pool.Get();

            Assert.IsType<LdapConnection>(connection);
        }
    }
}
