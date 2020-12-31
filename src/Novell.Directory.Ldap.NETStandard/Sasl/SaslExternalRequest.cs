namespace Novell.Directory.Ldap.Sasl
{
    public class SaslExternalRequest : SaslRequest
    {
        public SaslExternalRequest()
            : base(SaslConstants.Mechanism.External)
        {
        }
    }
}
