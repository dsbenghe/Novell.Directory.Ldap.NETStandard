using System;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class LongRunningAttribute : Attribute
    {
    }
}