// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Novell.Directory.Ldap.ObjectPool;
using System;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.ObjectPool
{
    public class DependencyInjectionPooledObjectPolicyTests
    {
        [Fact]
        public void Constructor_when_serviceProvider_null_throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                () => new DependencyInjectionPooledObjectPolicy<IPooledService, PooledServiceImplementation>(null));
        }

        [Fact]
        public void Create_returns_instance_resolved_from_serviceProvider()
        {
            var serviceProvider = NewServiceProvider();
            var policy = new DependencyInjectionPooledObjectPolicy<IPooledService, PooledServiceImplementation>(serviceProvider);

            var result = policy.Create();

            Assert.IsType<PooledServiceImplementation>(result);
        }

        [Fact]
        public void Return_when_obj_null_throws_ArgumentNullException()
        {
            var serviceProvider = NewServiceProvider();
            var policy = new DependencyInjectionPooledObjectPolicy<IPooledService, PooledServiceImplementation>(serviceProvider);

            Assert.Throws<ArgumentNullException>(() => policy.Return(null));
        }

        [Fact]
        public void Return_when_implementation_not_resettable_returns_true()
        {
            var serviceProvider = NewServiceProvider();
            var policy = new DependencyInjectionPooledObjectPolicy<IPooledService, PooledServiceImplementation>(serviceProvider);
            var obj = new PooledServiceImplementation();

            var result = policy.Return(obj);

            Assert.True(result);
        }

        [Fact]
        public void Return_when_implementation_resettable_and_TryReset_succeeds_returns_true()
        {
            var serviceProvider = NewServiceProvider();
            var policy = new DependencyInjectionPooledObjectPolicy<IPooledService, ResettablePooledServiceImplementation>(serviceProvider);
            var obj = new ResettablePooledServiceImplementation(resetResult: true);

            var result = policy.Return(obj);

            Assert.True(result);
            Assert.True(obj.TryResetWasCalled);
        }

        [Fact]
        public void Return_when_implementation_resettable_and_TryReset_fails_returns_false()
        {
            var serviceProvider = NewServiceProvider();
            var policy = new DependencyInjectionPooledObjectPolicy<IPooledService, ResettablePooledServiceImplementation>(serviceProvider);
            var obj = new ResettablePooledServiceImplementation(resetResult: false);

            var result = policy.Return(obj);

            Assert.False(result);
            Assert.True(obj.TryResetWasCalled);
        }

        private static IServiceProvider NewServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IPooledService, PooledServiceImplementation>();
            serviceCollection.AddTransient<ResettablePooledServiceImplementation>();
            return serviceCollection.BuildServiceProvider();
        }

        private interface IPooledService
        {
        }

        private class PooledServiceImplementation : IPooledService
        {
        }

        private class ResettablePooledServiceImplementation : IPooledService, IResettable
        {
            private readonly bool _resetResult;

            public ResettablePooledServiceImplementation()
                : this(resetResult: true)
            {
            }

            public ResettablePooledServiceImplementation(bool resetResult)
            {
                _resetResult = resetResult;
            }

            public bool TryResetWasCalled { get; private set; }

            public bool TryReset()
            {
                TryResetWasCalled = true;
                return _resetResult;
            }
        }
    }
}
