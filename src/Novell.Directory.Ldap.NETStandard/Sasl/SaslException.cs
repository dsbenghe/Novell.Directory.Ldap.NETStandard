using System;

namespace Novell.Directory.Ldap.Sasl
{
    public class SaslException : Exception
    {
        public SaslException()
        {
        }

        public SaslException(string message)
            : base(message)
        {
        }

        public SaslException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
