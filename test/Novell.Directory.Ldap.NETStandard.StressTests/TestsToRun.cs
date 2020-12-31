using Novell.Directory.Ldap.NETStandard.FunctionalTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.StressTests
{
    public static class TestsToRun
    {
        static TestsToRun()
        {
            Tests.AddRange(LoadTests());
        }

        private static List<Action> LoadTests()
        {
            return GetMethods()
                .Select(m =>
                {
                    return new Action(() =>
                    {
                        var testType = m.DeclaringType;
                        var testInstance = Activator.CreateInstance(testType);
                        try
                        {
                            m.Invoke(testInstance, null);
                        }
                        finally
                        {
                            (testInstance as IDisposable)?.Dispose();
                        }
                    });
                }).ToList();
        }

        public static List<MethodInfo> GetMethods()
        {
            var testsAssembly = typeof(AddEntryTests).GetTypeInfo().Assembly;
            return
                testsAssembly.DefinedTypes.Where(x => x.Name.EndsWith("Tests"))
                    .SelectMany(t => t.GetMethods())
                    .Where(m => m.IsPublic)
                    .Where(m => m.CustomAttributes.Any(attr => attr.AttributeType.Name == nameof(FactAttribute)
                        && attr.NamedArguments.Count(na => na.MemberName == "Skip") == 0))
                    .Where(m => m.CustomAttributes.All(attr => attr.AttributeType.Name != nameof(LongRunningAttribute)))
                    .ToList();
        }

        public static List<Action> Tests { get; } = new List<Action>();
    }
}
