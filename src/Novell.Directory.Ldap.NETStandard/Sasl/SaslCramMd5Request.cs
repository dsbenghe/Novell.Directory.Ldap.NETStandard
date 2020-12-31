namespace Novell.Directory.Ldap.Sasl
{
    public class SaslCramMd5Request : SaslRequest
    {
        public SaslCramMd5Request()
            : base(SaslConstants.Mechanism.CramMd5)
        {
        }

        public SaslCramMd5Request(string username, string password)
            : this()
        {
            AuthorizationId = username;
            Credentials = password.IsNotEmpty() ? password.ToUtf8Bytes() : null;
        }
    }
}
