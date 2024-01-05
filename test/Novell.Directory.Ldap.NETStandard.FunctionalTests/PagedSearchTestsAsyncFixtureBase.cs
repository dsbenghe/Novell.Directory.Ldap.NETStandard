// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests;

public class PagedSearchTestsAsyncFixtureBase : IAsyncLifetime
{
    public int Pages { get; }
    public int PageSize { get; }
    private readonly Random _random = new ();
    public string CnPrefix { get; }
    public IReadOnlyCollection<LdapEntry> Entries => _entriesTask.Result;

    private Task<LdapEntry[]> _entriesTask;

    protected PagedSearchTestsAsyncFixtureBase(int pages = 15, int pageSize = 20)
    {
        Pages = pages;
        PageSize = pageSize;
        CnPrefix = _random.Next().ToString();
    }

    public Task InitializeAsync()
    {
        _entriesTask = Task.WhenAll(
            Enumerable.Range(1, (Pages * PageSize) + (_random.Next() % PageSize))
                .Select(x => LdapOps.AddEntryAsync(CnPrefix)));
        return _entriesTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
