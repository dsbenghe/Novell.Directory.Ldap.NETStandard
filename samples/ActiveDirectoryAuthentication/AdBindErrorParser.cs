using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Novell.Directory.Ldap.Samples.ActiveDirectoryAuthentication
{
    /// <summary>
    /// Decodes the Active Directory specific sub-error that accompanies an
    /// <c>invalidCredentials (49)</c> bind failure.
    /// </summary>
    /// <remarks>
    /// AD does not use distinct LDAP result codes for "wrong password", "password
    /// expired", "account locked", etc. — every one of them is result code 49.
    /// The real reason is only available in the diagnostic message, in the form:
    /// <code>80090308: LdapErr: DSID-0C09044E, comment: AcceptSecurityContext error, data 52e, v4563</code>
    /// This class extracts the <c>data</c> token and maps it. It has no dependency on a
    /// live directory, so it is fully unit-testable.
    /// </remarks>
    public static class AdBindErrorParser
    {
        // "data 52e", "data 0x52e", "DATA 773" — hex token, optional 0x prefix
        private static readonly Regex DataToken = new Regex(
            @"\bdata\s+(?:0x)?([0-9a-fA-F]{2,4})\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly IReadOnlyDictionary<int, AdBindResult> Map = new Dictionary<int, AdBindResult>
        {
            [0x525] = AdBindResult.InvalidCredentials,     // user not found
            [0x52e] = AdBindResult.InvalidCredentials,     // invalid credentials
            [0x530] = AdBindResult.LogonTimeRestriction,
            [0x531] = AdBindResult.WorkstationRestriction,
            [0x532] = AdBindResult.PasswordExpired,
            [0x533] = AdBindResult.AccountDisabled,
            [0x701] = AdBindResult.AccountExpired,
            [0x773] = AdBindResult.MustChangePassword,
            [0x775] = AdBindResult.AccountLocked,
        };

        /// <summary>
        /// Classifies a bind failure thrown by <c>LdapConnection.BindAsync</c>.
        /// </summary>
        public static AdBindResult Parse(LdapException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            // The AD diagnostic text may surface in either property depending on server/version.
            var diagnostic = string.Concat(exception.LdapErrorMessage, " ", exception.Message);
            var result = Parse(diagnostic);

            // No "data" token but the result code is still invalidCredentials (49):
            // treat as plain bad credentials rather than Unknown.
            if (result == AdBindResult.Unknown && exception.ResultCode == LdapException.InvalidCredentials)
            {
                return AdBindResult.InvalidCredentials;
            }

            return result;
        }

        /// <summary>
        /// Classifies a raw diagnostic message. Returns <see cref="AdBindResult.Unknown"/>
        /// when no recognised <c>data</c> token is present.
        /// </summary>
        public static AdBindResult Parse(string diagnosticMessage)
        {
            if (string.IsNullOrWhiteSpace(diagnosticMessage))
            {
                return AdBindResult.Unknown;
            }

            var match = DataToken.Match(diagnosticMessage);
            if (match.Success
                && int.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out var code)
                && Map.TryGetValue(code, out var mapped))
            {
                return mapped;
            }

            // Some servers omit the data token entirely.
            return diagnosticMessage.IndexOf("Invalid Credentials", StringComparison.OrdinalIgnoreCase) >= 0
                ? AdBindResult.InvalidCredentials
                : AdBindResult.Unknown;
        }
    }
}
