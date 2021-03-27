using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    internal static class AsyncExtensions
    {
        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                await task.ConfigureAwait(false);

                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var tcs = new TaskCompletionSource<object>();
#if NETSTANDARD2_0
                using (cancellationToken.Register(Callback<object>, Tuple.Create(tcs, cancellationToken)))
#else
                await using (cancellationToken.Register(Callback<object>, Tuple.Create(tcs, cancellationToken)).ConfigureAwait(false))
#endif
                {
                    if (task == await Task.WhenAny(tcs.Task, task).ConfigureAwait(false))
                    {
                        await task.ConfigureAwait(false);
                    }
                    else
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                }
            }
            catch (AggregateException exception)
            {
                if (exception.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                }

                throw;
            }
        }

        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return await task.ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var tcs = new TaskCompletionSource<T>();
#if NETSTANDARD2_0
                using (cancellationToken.Register(Callback<T>, Tuple.Create(tcs, cancellationToken)))
#else
                await using (cancellationToken.Register(Callback<T>, Tuple.Create(tcs, cancellationToken)).ConfigureAwait(false))
#endif
                {
                    if (task == await Task.WhenAny(tcs.Task, task).ConfigureAwait(false))
                    {
                        return await task.ConfigureAwait(false);
                    }
                    else
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                }
            }
            catch (AggregateException exception)
            {
                if (exception.InnerException != null)
                {
                    ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                }

                throw;
            }
        }

        private static void Callback<T>(object state)
        {
            var tuple = (Tuple<TaskCompletionSource<T>, CancellationToken>)state;

            tuple.Item1.TrySetCanceled(tuple.Item2);
        }
#if !NET5_0
        public static Task ConnectAsync(this Socket socket, IPEndPoint ipEndPoint, CancellationToken cancellationToken)
        {
            return socket.ConnectAsync(ipEndPoint).WithCancellation(cancellationToken);
        }

        public static Task ConnectAsync(this TcpClient tcpClient, string host, int port, CancellationToken cancellationToken)
        {
            return tcpClient.ConnectAsync(host, port).WithCancellation(cancellationToken);
        }
#endif
    }
}
