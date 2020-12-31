using System.Net;
using System.Net.Sockets;

namespace Novell.Directory.Ldap
{
    internal static class ConnectTimeoutExtensions
    {
        internal static void Connect(this Socket socket, IPEndPoint ipEndPoint, int connectionTimeout)
        {
            // WaitAndUnwrap creates unnecessary threads if ConnectionTimeout is zero which
            // can impact performance.  Prefer the sync method call to avoid the overhead
            if (connectionTimeout != 0)
            {
                socket.ConnectAsync(ipEndPoint).WaitAndUnwrap(connectionTimeout);
            }
            else
            {
                socket.Connect(ipEndPoint);
            }
        }
    }
}
