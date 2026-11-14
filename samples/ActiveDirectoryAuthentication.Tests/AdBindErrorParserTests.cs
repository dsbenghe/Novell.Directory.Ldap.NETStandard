using Novell.Directory.Ldap.Samples.ActiveDirectoryAuthentication;
using Xunit;

namespace Novell.Directory.Ldap.Samples.Tests
{
    public class AdBindErrorParserTests
    {
        // Real-world diagnostic shape emitted by Windows Server domain controllers.
        private const string Template = "80090308: LdapErr: DSID-0C09044E, comment: AcceptSecurityContext error, data {0}, v4563";

        [Theory]
        [InlineData("525", AdBindResult.InvalidCredentials)]
        [InlineData("52e", AdBindResult.InvalidCredentials)]
        [InlineData("530", AdBindResult.LogonTimeRestriction)]
        [InlineData("531", AdBindResult.WorkstationRestriction)]
        [InlineData("532", AdBindResult.PasswordExpired)]
        [InlineData("533", AdBindResult.AccountDisabled)]
        [InlineData("701", AdBindResult.AccountExpired)]
        [InlineData("773", AdBindResult.MustChangePassword)]
        [InlineData("775", AdBindResult.AccountLocked)]
        public void Maps_known_data_codes(string code, AdBindResult expected)
        {
            var message = string.Format(Template, code);
            Assert.Equal(expected, AdBindErrorParser.Parse(message));
        }

        [Theory]
        [InlineData("DATA 52E")]
        [InlineData("data 0x52e")]
        [InlineData("comment: AcceptSecurityContext error, data 52e, v3839")]
        public void Is_case_and_prefix_insensitive(string message)
        {
            Assert.Equal(AdBindResult.InvalidCredentials, AdBindErrorParser.Parse(message));
        }

        [Fact]
        public void Falls_back_to_InvalidCredentials_when_data_token_is_missing()
        {
            Assert.Equal(AdBindResult.InvalidCredentials, AdBindErrorParser.Parse("Invalid Credentials"));
        }

        [Fact]
        public void Returns_Unknown_for_unrecognised_code()
        {
            Assert.Equal(AdBindResult.Unknown, AdBindErrorParser.Parse(string.Format(Template, "abc")));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Returns_Unknown_for_empty_input(string message)
        {
            Assert.Equal(AdBindResult.Unknown, AdBindErrorParser.Parse(message));
        }

        [Fact]
        public void Does_not_match_data_token_inside_other_words()
        {
            // "metadata 773" must not be read as an AD sub-error.
            Assert.Equal(AdBindResult.Unknown, AdBindErrorParser.Parse("metadata 773 rows"));
        }

        [Fact]
        public void Reads_diagnostic_from_LdapException()
        {
            var ex = new LdapException("Invalid Credentials", LdapException.InvalidCredentials, string.Format(Template, "773"));
            Assert.Equal(AdBindResult.MustChangePassword, AdBindErrorParser.Parse(ex));
        }

        [Fact]
        public void LdapException_with_code_49_and_no_data_token_is_InvalidCredentials()
        {
            var ex = new LdapException("Invalid Credentials", LdapException.InvalidCredentials, null);
            Assert.Equal(AdBindResult.InvalidCredentials, AdBindErrorParser.Parse(ex));
        }
    }
}
