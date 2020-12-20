using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// RFC 4532: LDAP "Who am I?" Operation
    /// https://tools.ietf.org/html/rfc4532.
    /// </summary>
    public class LdapWhoAmIOperation : LdapExtendedOperation
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapWhoAmIOperation>();

        public LdapWhoAmIOperation()
            : base(LdapKnownOids.Extensions.WhoAmI, null)
        {
        }
    }

    public class LdapWhoAmIResponse : LdapExtendedResponse
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapWhoAmIResponse>();

        public string AuthzId { get; }

        public AuthzType AuthzIdType { get; }

        public string AuthzIdWithoutType { get; }

        public LdapWhoAmIResponse(RfcLdapMessage message)
            : base(message)
        {
            AuthzId = ((RfcExtendedResponse)message.Response)?.Response?.StringValue();
            AuthzIdWithoutType = AuthzId;
            AuthzIdType = AuthzType.None;

            if (!string.IsNullOrEmpty(AuthzId))
            {
                // Seems like sometimes, control characters are prepended to the actual value
                var cutIndex = 0;
                foreach (var c in AuthzId)
                {
                    if (char.IsControl(c) || c == '"')
                    {
                        cutIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (cutIndex > 0)
                {
                    AuthzId = AuthzId.Substring(cutIndex);
                }

                if (AuthzId.StartsWith("u:"))
                {
                    AuthzIdType = AuthzType.User;
                    AuthzIdWithoutType = AuthzId.Substring(2);
                }
                else if (AuthzId.StartsWith("dn:"))
                {
                    AuthzIdType = AuthzType.DistinguishedName;
                    AuthzIdWithoutType = AuthzId.Substring(3);
                }
                else
                {
                    AuthzIdType = AuthzType.Unknown;
                }
            }
        }

        // RFC 4513
        public enum AuthzType
        {
            None = 0,
            Unknown,
            User,
            DistinguishedName,
        }
    }
}
