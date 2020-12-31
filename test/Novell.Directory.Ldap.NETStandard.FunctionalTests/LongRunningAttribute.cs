using System;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LongRunningAttribute : Attribute
    {
    }
}
