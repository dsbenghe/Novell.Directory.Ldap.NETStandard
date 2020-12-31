namespace Novell.Directory.Ldap
{
    public static class ExtensionRegistrations
    {
        static ExtensionRegistrations()
        {
            LdapExtendedResponse.Register(LdapKnownOids.Extensions.WhoAmI, typeof(LdapWhoAmIResponse));
        }

        public static LdapWhoAmIResponse WhoAmI(this LdapConnection conn, LdapConstraints cons = null)
        {
            var result = conn.ExtendedOperation(new LdapWhoAmIOperation(), cons);
            if (result is LdapWhoAmIResponse whoami)
            {
                return whoami;
            }

            return new LdapWhoAmIResponse(result.Message);
        }
    }
}
