namespace Novell.Directory.Ldap.Sasl
{
    public class SaslDigestMd5Request : SaslRequest
    {
        public SaslDigestMd5Request() : base(SaslConstants.Mechanism.DigestMd5)
        {
        }
    }
}
