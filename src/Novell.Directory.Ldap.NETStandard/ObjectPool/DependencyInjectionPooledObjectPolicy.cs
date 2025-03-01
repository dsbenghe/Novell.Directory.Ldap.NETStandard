using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System;

namespace Novell.Directory.Ldap.ObjectPool;

/// <summary>
/// An object pool policy that uses dependency injection to create pooled objects.
/// </summary>
/// <typeparam name="TService">The service type to be pooled.</typeparam>
/// <typeparam name="TImplementation">The implementation type to be pooled.</typeparam>
public sealed class DependencyInjectionPooledObjectPolicy<TService, TImplementation> :
    IPooledObjectPolicy<TService>
    where TService : class
    where TImplementation : class, TService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly bool _isResettable = typeof(IResettable).IsAssignableFrom(typeof(TImplementation));

    /// <summary>
    /// An object pool policy that uses dependency injection to create pooled objects.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to create pooled objects.</param>
    public DependencyInjectionPooledObjectPolicy(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public TService Create() => _serviceProvider.GetRequiredService<TService>();

    /// <inheritdoc />
    public bool Return(TService obj)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        return !_isResettable || ((IResettable)obj).TryReset();
    }
}
