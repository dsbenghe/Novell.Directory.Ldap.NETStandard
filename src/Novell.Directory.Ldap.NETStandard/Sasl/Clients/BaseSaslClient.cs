using System;
using System.Collections;

namespace Novell.Directory.Ldap.Sasl.Clients
{
    public abstract class BaseSaslClient : ISaslClient
    {
        protected string ServerName { get; }
        protected Hashtable Props { get; }

        protected BaseSaslClient(string serverName, Hashtable props)
        {
            ServerName = serverName;
            Props = props;
        }

        public abstract string MechanismName { get; }
        public abstract bool HasInitialResponse { get; }
        public abstract bool IsComplete { get; }
        public abstract DebugId DebugId { get; }

        public abstract byte[] EvaluateChallenge(byte[] challenge);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
