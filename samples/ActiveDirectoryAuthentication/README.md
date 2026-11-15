# Sample: Active Directory authentication with decoded bind errors

Verifies a user's password against Active Directory with a simple bind and tells you
**why** the bind failed — expired password, locked account, disabled account, must-change,
etc. — instead of a bare `LdapException`.

## Why this matters

AD returns `invalidCredentials (49)` for *every* logon failure. The actual reason is only
in the diagnostic message:

```
80090308: LdapErr: DSID-0C09044E, comment: AcceptSecurityContext error, data 52e, v4563
                                                                        ^^^^^^^^
```

`AdBindErrorParser` extracts the `data` token and maps it:

| data | Meaning                         | `AdBindResult`           |
|------|---------------------------------|--------------------------|
| 525  | user not found                  | `InvalidCredentials`     |
| 52e  | invalid credentials             | `InvalidCredentials`     |
| 530  | logon time restriction          | `LogonTimeRestriction`   |
| 531  | workstation restriction         | `WorkstationRestriction` |
| 532  | password expired                | `PasswordExpired`        |
| 533  | account disabled                | `AccountDisabled`        |
| 701  | account expired                 | `AccountExpired`         |
| 773  | must change password            | `MustChangePassword`     |
| 775  | account locked out              | `AccountLocked`          |

## Run

```bash
cd samples/ActiveDirectoryAuthentication
export AD_PASSWORD='...'                     # or omit to be prompted
dotnet run -- dc01.example.com 389 jdoe@example.com
dotnet run -- dc01.example.com 636 jdoe@example.com --ssl
dotnet run -- dc01.example.com 389 'EXAMPLE\jdoe' --starttls
```

Exit code `0` means the credentials are valid.

## Use in your own code

```csharp
var authenticator = new ActiveDirectoryAuthenticator(new ActiveDirectoryOptions
{
    Host = "dc01.example.com",
    Port = 636,
    UseSsl = true,
    DefaultUpnSuffix = "example.com",   // "jdoe" -> "jdoe@example.com"
});

var result = await authenticator.AuthenticateAsync(userName, password);
if (result != AdBindResult.Success)
{
    // map result to a user-facing message; see Program.Describe()
}
```

## Notes

* Uses UPN / `DOMAIN\user` binds, which AD accepts directly — no need to search for the
  user's DN first.
* An empty password is rejected before binding: AD treats a bind with an empty password
  as an anonymous bind and would report success.
* `ActiveDirectoryOptions.Timeout` (default 10 s) bounds connect + TLS + bind; an unreachable
  host returns `ConnectionError` instead of hanging for the OS TCP timeout.
* `AdBindErrorParser` is a pure function with no server dependency; see
  `../ActiveDirectoryAuthentication.Tests` for its unit tests.
