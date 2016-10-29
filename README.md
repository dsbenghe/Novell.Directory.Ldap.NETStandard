# LDAP client library for .NET Standard 1.3

.NET Core, .NET Framework 4.6, Universal Windows Platform

[![Build status](https://ci.appveyor.com/api/projects/status/nabbc061vlumiivs/branch/master?svg=true)](https://ci.appveyor.com/project/dsbenghe/novell-directory-ldap-netstandard/branch/master) - Windows Build<br />
[![Build Status](https://travis-ci.org/dsbenghe/Novell.Directory.Ldap.NETStandard.svg?branch=master)](https://travis-ci.org/dsbenghe/Novell.Directory.Ldap.NETStandard) - Linux Build (including tests on OpenLDAP) <br />
[![NuGet](https://img.shields.io/nuget/v/Novell.Directory.Ldap.NETStandard.svg)](https://www.nuget.org/packages/Novell.Directory.Ldap.NETStandard/)

The library is originaly coming from Novell (https://www.novell.com/developer/ndk/ldap_libraries_for_c_sharp.html) - really old code base (looks like a tool-based conversion from Java - this seems to be the original java code repo http://www.openldap.org/devel/gitweb.cgi?p=openldap-jldap.git;a=summary - first commit 2000 :)). 

First commit in this repo is the original source code from Novell.

Ported to .NET Standard 1.3 (https://docs.microsoft.com/en-us/dotnet/articles/standard/library): works on .NET Core, .NET Framework 4.6, Universal Windows Platform.

The main changes were around:
- thread usage: the library was extensively using Abort, Interrupt, ThreadInterruptedException, ... - which is not recommended and also not supported in .NET Core.
- serialization support for a limited number of types was deleted
- ssl support: the library was using Mono.Security for this. Now is implemented using SslStream from NetStandard

The library has some samples which are not included in the solution and are in the original state - they may or may not compile on .NET Core - but they should be compilable on .NET Core with minimal work.