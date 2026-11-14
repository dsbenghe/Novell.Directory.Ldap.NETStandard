namespace Novell.Directory.Ldap.Samples.ActiveDirectoryAuthentication
{
    /// <summary>
    /// Outcome of a simple bind against Active Directory, with the AD-specific
    /// sub-error (the <c>data NNN</c> token in the diagnostic message) decoded.
    /// </summary>
    public enum AdBindResult
    {
        /// <summary>Bind succeeded — the credentials are valid.</summary>
        Success,

        /// <summary>Wrong password or unknown user (data 52e / 525).</summary>
        InvalidCredentials,

        /// <summary>The password has expired (data 532).</summary>
        PasswordExpired,

        /// <summary>The user must change the password at next logon (data 773).</summary>
        MustChangePassword,

        /// <summary>The account is disabled (data 533).</summary>
        AccountDisabled,

        /// <summary>The account is locked out (data 775).</summary>
        AccountLocked,

        /// <summary>The account has expired (data 701).</summary>
        AccountExpired,

        /// <summary>Logon not permitted at this time (data 530).</summary>
        LogonTimeRestriction,

        /// <summary>Logon not permitted from this workstation (data 531).</summary>
        WorkstationRestriction,

        /// <summary>The server rejected the bind for a reason we did not decode.</summary>
        Unknown,

        /// <summary>The server could not be reached (DNS, TCP, TLS, timeout).</summary>
        ConnectionError,
    }
}
