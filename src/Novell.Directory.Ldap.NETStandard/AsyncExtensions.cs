using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    public static class AsyncExtensions
    {
        public static async Task TimeoutAfterAsync(this Task task, int timeout)
        {
            try
            {
                if (timeout == 0)
                {
                    await task.ConfigureAwait(false);
                }
                else
                {
                    if (task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false))
                    {
                        await task.ConfigureAwait(false);
                    }
                    else
                    {
                        throw new SocketException(258); // WAIT_TIMEOUT
                    }
                }
            }
            catch (AggregateException exception)
            {
                if (exception.InnerExceptions.Count == 1)
                {
                    throw exception.InnerException;
                }

                throw;
            }
        }
    }
}
