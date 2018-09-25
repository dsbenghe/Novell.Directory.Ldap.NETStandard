using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// RFC 3062: LDAP Password Modify Extended Operation
    /// https://tools.ietf.org/html/rfc3062
    /// </summary>
    public class LdapModifyPasswordOperation : LdapExtendedOperation
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapModifyPasswordOperation>();

        private string _userIdentity, _oldPasswd, _newPasswd;

        /// <summary>
        /// The userIdentity field, if present, SHALL contain an octet string
        /// representation of the user associated with the request.  This string
        /// may or may not be an LDAPDN [RFC2253].  If no userIdentity field is
        /// present, the request acts up upon the password of the user currently
        /// associated with the LDAP session.
        /// </summary>
        public string UserIdentity
        {
            get => _userIdentity;
            set
            {
                _userIdentity = value;
                UpdateValues();
            }
        }

        /// <summary>
        /// The oldPasswd field, if present, SHALL
        /// contain the user's current password.
        /// </summary>
        public string OldPassword
        {
            get => _oldPasswd;
            set
            {
                _oldPasswd = value;
                UpdateValues();
            }
        }

        /// <summary>
        /// The newPasswd field, if present, SHALL
        /// contain the desired password for this user.
        /// </summary>
        public string NewPassword
        {
            get => _newPasswd;
            set
            {
                _newPasswd = value;
                UpdateValues();
            }
        }

        public LdapModifyPasswordOperation()
            : base(LdapKnownOids.Extensions.ModifyPassword, null)
        {
        }

        private void UpdateValues()
        {
            var seq = new Asn1Sequence();
            if (!string.IsNullOrEmpty(_userIdentity))
            {
                var tag = new Asn1Identifier(Asn1Identifier.Context, true, 0);
                seq.Add(new Asn1Tagged(tag, new Asn1OctetString(_userIdentity)));
            }
            if (!string.IsNullOrEmpty(_oldPasswd))
            {
                var tag = new Asn1Identifier(Asn1Identifier.Context, true, 1);
                seq.Add(new Asn1Tagged(tag, new Asn1OctetString(_oldPasswd)));
            }
            if (!string.IsNullOrEmpty(_newPasswd))
            {
                var tag = new Asn1Identifier(Asn1Identifier.Context, true, 2);
                seq.Add(new Asn1Tagged(tag, new Asn1OctetString(_newPasswd)));
            }
            var values = seq.GetEncoding(new LberEncoder());
            SetValue(values);
        }
    }

    public class LdapModifyPasswordResponse : LdapExtendedResponse
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapModifyPasswordResponse>();

        /// <summary>
        /// The genPasswd field, if present, SHALL
        /// contain a generated password for the user.
        /// </summary>
        public string GeneratedPassword { get; }

        public LdapModifyPasswordResponse(RfcLdapMessage message)
            : base(message)
        {
            GeneratedPassword = ((RfcExtendedResponse)message.Response)?.Response?.StringValue();
        }
    }
}
