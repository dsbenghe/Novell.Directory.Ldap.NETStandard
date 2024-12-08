using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;
using System.IO;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// RFC 3063: LDAP Password Modify Extended Operation
    /// https://tools.ietf.org/html/rfc3062.
    ///   <pre>
    ///     PasswdModifyRequestValue::= SEQUENCE {
    ///       userIdentity[0]  OCTET STRING OPTIONAL,
    ///       oldPasswd[1]     OCTET STRING OPTIONAL,
    ///       newPasswd[2]     OCTET STRING OPTIONAL }
    ///   </pre>
    /// </summary>
    public class LdapPasswordModifyOperation : LdapExtendedOperation
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapPasswordModifyOperation>();

        /// <summary> Context-specific tag for optional userIdentity.</summary>
        private const int UserIdentityTag = 0;

        /// <summary> Context-specific tag for optional oldPasswd.</summary>
        private const int OldPasswdTag = 1;

        /// <summary> Context-specific tag for optional newPasswd.</summary>
        private const int NewPasswdTag = 2;

        /// <summary>
        /// According to RFC 4511 Section 5.1:
        /// The OCTET STRING type must always be encoded in the primitive (not constructed) form.
        /// </summary>
        private const bool ConstructedType = false;

        /// <summary>
        /// According to the complete ASN.1 definition in RFC 4511, Appendix B:
        /// Tags are always implicit (not explicit) unless otherwise stated.
        /// </summary>
        private const bool ExplicitTag = false;

        /// <summary>
        /// Constructs an LDAP Password Modify Extended Operation.
        /// </summary>
        /// <param name="userIdentity">
        /// A string that identifies the user whose password will be changed.
        /// This does not have to be a Distinguished Name but usually it is.
        /// Can be null or empty in which case the LDAP server is supposed to
        /// use the currently bound user.</param>
        /// <param name="oldPasswd">
        /// The user's current password. Used to authenticate the operation.
        /// Can be null or empty in which case the behavior is unspecified.
        /// Usually a null or empty oldPasswd will result in an error but some
        /// servers may allow it if certain conditions are met. For example,
        /// if the currently bound user is the RootDN user.
        /// </param>
        /// <param name="newPasswd">
        /// The desired new password. Can be null or empty in which case the
        /// LDAP server is supposed to generate a new password and send it
        /// with the response message (the 'genPasswd' field).  Some servers
        /// may not support or allow password generation and will send an
        /// error response instead.
        /// </param>
        public LdapPasswordModifyOperation(string userIdentity, string oldPasswd, string newPasswd)
            : base(LdapKnownOids.Extensions.PasswordModify, null)
        {
            var seq = new Asn1Sequence(3);

            if (!string.IsNullOrEmpty(userIdentity))
            {
                var octetString = new Asn1OctetString(userIdentity);
                var id = new Asn1Identifier(Asn1Identifier.Context, ConstructedType, UserIdentityTag);

                seq.Add(new Asn1Tagged(id, octetString, ExplicitTag));
            }

            if (!string.IsNullOrEmpty(oldPasswd))
            {
                var octetString = new Asn1OctetString(oldPasswd);
                var id = new Asn1Identifier(Asn1Identifier.Context, ConstructedType, OldPasswdTag);

                seq.Add(new Asn1Tagged(id, octetString, ExplicitTag));
            }

            if (!string.IsNullOrEmpty(newPasswd))
            {
                var octetString = new Asn1OctetString(newPasswd);
                var id = new Asn1Identifier(Asn1Identifier.Context, ConstructedType, NewPasswdTag);

                seq.Add(new Asn1Tagged(id, octetString, ExplicitTag));
            }

            var stream = new MemoryStream();
            seq.Encode(new LberEncoder(), stream);

            SetValue(stream.ToArray());
        }
    }

    /// <summary>
    /// RFC 3063: LDAP Password Modify Extended Operation
    /// https://tools.ietf.org/html/rfc3062.
    ///   <pre>
    ///     PasswdModifyResponseValue ::= SEQUENCE {
    ///       genPasswd[0] OCTET STRING OPTIONAL }
    ///   </pre>
    /// </summary>
    public class LdapPasswordModifyResponse : LdapExtendedResponse
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapPasswordModifyResponse>();

        public string genPasswd { get; }

        public LdapPasswordModifyResponse(RfcLdapMessage message)
            : base(message)
        {
            genPasswd = ((RfcExtendedResponse)message.Response)?.Response?.StringValue();
        }
    }
}
