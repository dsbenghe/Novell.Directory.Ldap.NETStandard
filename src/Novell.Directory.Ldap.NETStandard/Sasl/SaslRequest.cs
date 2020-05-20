using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Novell.Directory.Ldap.Sasl
{
    public abstract class SaslRequest
    {
        public string SaslMechanism { get; }
        public string AuthorizationId { get; set; }
        public byte[] Credentials { get; set; }
        public LdapConstraints Constraints { get; set; }
        public virtual string RealmName { get; set; }
        protected List<LdapControl> Controls { get; }
        public void AddControl(LdapControl control) => Controls.Add(control);
        public void AddAllControls(IEnumerable<LdapControl> controls) => Controls.AddRange(controls ?? Enumerable.Empty<LdapControl>());
        public IReadOnlyCollection<LdapControl> GetAllControls() => Controls.Select(c => c.Clone() as LdapControl).ToArray();

        // Not supporting auth-int and auth-conf yet
        public QualityOfProtection QualityOfProtection => QualityOfProtection.AuthenticationOnly;

        // Not sure if there's any point supporting Medium and Low, ever?
        public ProtectionStrength ProtectionStrength => ProtectionStrength.High;

        public Hashtable SaslBindProperties { get; set; }

        protected SaslRequest(string saslMechanism)
        {
            SaslMechanism = saslMechanism;
            Controls = new List<LdapControl>();
        }
    }
}
