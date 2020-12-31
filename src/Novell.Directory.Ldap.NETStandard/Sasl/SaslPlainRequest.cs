namespace Novell.Directory.Ldap.Sasl
{
    public class SaslPlainRequest : SaslRequest
    {
        public SaslPlainRequest()
            : base(SaslConstants.Mechanism.Plain)
        {
        }

        public SaslPlainRequest(string username, string password)
            : this()
        {
            AuthorizationId = username;
            Credentials = password.IsNotEmpty() ? password.ToUtf8Bytes() : null;
        }
    }
}
