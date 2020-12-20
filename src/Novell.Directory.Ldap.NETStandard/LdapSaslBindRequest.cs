using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
    public class LdapSaslBindRequest : LdapMessage
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapSaslBindRequest>();

        /// <summary>
        ///     Constructs a simple bind request.
        /// </summary>
        /// <param name="version">
        ///     The Ldap protocol version, use Ldap_V3.
        ///     Ldap_V2 is not supported.
        /// </param>
        /// <param name="dn">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name.
        /// </param>
        /// <param name="passwd">
        ///     If non-null and non-empty, specifies that the
        ///     connection and all operations through it should
        ///     be authenticated with dn as the distinguished
        ///     name and passwd as password.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the simple bind request,
        ///     or null if none.
        /// </param>
        public LdapSaslBindRequest(int version, string mechanism, LdapControl[] cont)
            : this(version, mechanism, cont, null)
        {
        }

        public LdapSaslBindRequest(int version, string mechanism, LdapControl[] cont, byte[] credentials)
            : base(BindRequest, new RfcBindRequest(version, string.Empty, mechanism, credentials), cont)
        {
        }

        /// <summary>
        ///     Return an Asn1 representation of this add request.
        ///     #return an Asn1 representation of this object.
        /// </summary>
        public override string ToString()
        {
            return Asn1Object.ToString();
        }
    }
}
