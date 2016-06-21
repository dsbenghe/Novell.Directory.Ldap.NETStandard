# LDAP client library for .NET Standard

[![Build status](https://ci.appveyor.com/api/projects/status/nabbc061vlumiivs/branch/master?svg=true)](https://ci.appveyor.com/project/dsbenghe/novell-directory-ldap-netstandard/branch/master)

The library is originaly coming from Novell (https://www.novell.com/developer/ndk/ldap_libraries_for_c_sharp.html) - really old code base (looks like a tool-based conversion from Java as it has really odd coding style) - but it works. First commit is the original source code from Novell.

Ported to .NET Core. The main changes were around:
- thread usage: the library was extensively using Abort, Interrupt, ... - which is not recommended ans also not supported in .NET Core.
- serialization support for a limited number of types: deleted
- ssl support: the library was using Mono.Security for this. I commented this for the moment and is throwing an exception.

The library has some sample which are not included in the solution and are in the original state - they may or may not compile on .NET Core - but they should be compilable on .NET Core with minimal work.
