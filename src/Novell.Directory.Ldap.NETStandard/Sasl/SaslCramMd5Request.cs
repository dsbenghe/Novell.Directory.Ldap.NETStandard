namespace Novell.Directory.Ldap.Sasl
{
    public class SaslCramMd5Request : SaslRequest
    {
        public SaslCramMd5Request() : base (SaslConstants.Mechanism.CramMd5)
        {
        }
    }
}
