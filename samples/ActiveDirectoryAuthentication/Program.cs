using System;
using System.Text;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap.Samples.ActiveDirectoryAuthentication
{
    /// <summary>
    /// Usage:
    ///   dotnet run -- &lt;host&gt; &lt;port&gt; &lt;user@domain | DOMAIN\user&gt; [--ssl | --starttls]
    ///
    /// The password is read from the AD_PASSWORD environment variable, or prompted
    /// without echo if the variable is not set. Exit code 0 = valid credentials.
    /// </summary>
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: ActiveDirectoryAuthentication <host> <port> <user@domain | DOMAIN\\user> [--ssl | --starttls]");
                return 2;
            }

            var options = new ActiveDirectoryOptions
            {
                Host = args[0],
                Port = int.Parse(args[1]),
                UseSsl = Array.IndexOf(args, "--ssl") >= 0,
                UseStartTls = Array.IndexOf(args, "--starttls") >= 0,
                Timeout = TimeSpan.FromSeconds(10),
            };

            var password = Environment.GetEnvironmentVariable("AD_PASSWORD") ?? ReadPassword();

            var authenticator = new ActiveDirectoryAuthenticator(options);
            var result = await authenticator.AuthenticateAsync(args[2], password).ConfigureAwait(false);

            Console.WriteLine($"{result}: {Describe(result)}");
            return result == AdBindResult.Success ? 0 : 1;
        }

        /// <summary>Human-readable text you would typically show on a login page.</summary>
        private static string Describe(AdBindResult result)
        {
            switch (result)
            {
                case AdBindResult.Success: return "Credentials are valid.";
                case AdBindResult.InvalidCredentials: return "User name or password is incorrect.";
                case AdBindResult.PasswordExpired: return "Your password has expired. Contact your administrator.";
                case AdBindResult.MustChangePassword: return "You must change your password before logging in.";
                case AdBindResult.AccountDisabled: return "This account is disabled.";
                case AdBindResult.AccountLocked: return "This account is locked out.";
                case AdBindResult.AccountExpired: return "This account has expired.";
                case AdBindResult.LogonTimeRestriction: return "Logon is not permitted at this time.";
                case AdBindResult.WorkstationRestriction: return "Logon is not permitted from this workstation.";
                case AdBindResult.ConnectionError: return "The directory server could not be reached.";
                default: return "Authentication failed for an unrecognised reason.";
            }
        }

        private static string ReadPassword()
        {
            Console.Error.Write("Password: ");
            var sb = new StringBuilder();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                }
            }

            Console.Error.WriteLine();
            return sb.ToString();
        }
    }
}
