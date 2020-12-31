namespace Novell.Directory.Ldap.Sasl
{
    public class SaslDigestMd5Request : SaslRequest
    {
        public string Host { get; }

        public SaslDigestMd5Request()
            : base(SaslConstants.Mechanism.DigestMd5)
        {
        }

        public SaslDigestMd5Request(string username, string password, string realmName, string host)
            : this()
        {
            AuthorizationId = username;
            Credentials = password.ToUtf8Bytes();
            RealmName = realmName;
            Host = host;
        }
    }
}
