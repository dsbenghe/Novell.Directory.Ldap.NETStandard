using Microsoft.Extensions.Configuration;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public class LdapServerConfiguration
    {
        public string ServerAddress { get; set; }

        public int ServerPort { get; set; }

        public int ServerPortSsl { get; set; }

        public int AlternateServerPort { get; set; }

        public int AlternateServerPortSsl { get; set; }

        public string BaseDn { get; set; }

        public string RootUserDn { get; set; }

        public string RootUserPassword { get; set; }
    }

    public class TestsConfig
    {
        public static LdapServerConfiguration LdapServer { get; }

        public const string DefaultObjectClass = "inetOrgPerson";
        public const string DefaultPassword = "password";

        static TestsConfig()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = configurationBuilder.Build();
            var ldapServerConfigurationSection = configuration.GetSection(nameof(LdapServerConfiguration));
            var ldapServerConfiguration = new LdapServerConfiguration();
            ldapServerConfigurationSection.Bind(ldapServerConfiguration);
            LdapServer = ldapServerConfiguration;
        }
    }
}
