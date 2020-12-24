using System;
using System.Collections.Generic;
using System.Linq;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// The result of calling <see cref="LdapConnectionExtensionMethods.GetRootDseInfo(ILdapConnection)"/>.
    /// </summary>
    public class RootDseInfo
    {
        public string ServerName { get; }
        public string DefaultNamingContext { get; }
        public IReadOnlyList<string> NamingContexts { get; }
        public IReadOnlyList<string> SupportedSaslMechanisms { get; }
        public IReadOnlyList<string> SupportedCapabilities { get; }
        public IReadOnlyList<string> SupportedControls { get; }
        public IReadOnlyList<string> SupportedExtensions { get; }
        public IReadOnlyList<string> SupportedLDAPPolicies { get; }

        /// <summary>
        /// All Root DSE Attributes that aren't already part of other properties of this class.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> OtherAttributes { get; }

        public RootDseInfo(LdapEntry rootDseEntry)
        {
            var otherAttributes = new Dictionary<string, IReadOnlyList<string>>();
            foreach (LdapAttribute attr in rootDseEntry.GetAttributeSet())
            {
                switch (attr.Name)
                {
                    case "serverName":
                        ServerName = attr.StringValue;
                        break;
                    case "defaultNamingContext":
                        DefaultNamingContext = attr.StringValue;
                        break;
                    case "supportedSASLMechanisms":
                        SupportedSaslMechanisms = attr.StringValueArray;
                        break;
                    case "namingContexts":
                        NamingContexts = attr.StringValueArray;
                        break;
                    case "supportedCapabilities":
                        SupportedCapabilities = attr.StringValueArray;
                        break;
                    case "supportedControl":
                        SupportedControls = attr.StringValueArray;
                        break;
                    case "supportedExtension":
                        SupportedExtensions = attr.StringValueArray;
                        break;
                    case "supportedLDAPPolicies":
                        SupportedLDAPPolicies = attr.StringValueArray;
                        break;
                    default:
                        otherAttributes[attr.Name] = attr.StringValueArray;
                        break;
                }
            }

            OtherAttributes = otherAttributes;

            // Don't want any of those properties be null
            ServerName = ServerName ?? string.Empty;
            DefaultNamingContext = DefaultNamingContext ?? string.Empty;
            NamingContexts = NamingContexts ?? Array.Empty<string>();
            SupportedSaslMechanisms = SupportedSaslMechanisms ?? Array.Empty<string>();
            SupportedCapabilities = SupportedCapabilities ?? Array.Empty<string>();
            SupportedControls = SupportedControls ?? Array.Empty<string>();
            SupportedExtensions = SupportedExtensions ?? Array.Empty<string>();
            SupportedLDAPPolicies = SupportedLDAPPolicies ?? Array.Empty<string>();
        }

        public bool SupportsExtension(string oid)
            => SupportedExtensions != null && SupportedExtensions.Contains(oid);
    }
}
