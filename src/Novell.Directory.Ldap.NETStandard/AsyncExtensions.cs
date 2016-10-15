using System;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    public static class AsyncExtensions
    {
        public static T ResultAndUnwrap<T>(this Task<T> task)
        {
            try
            {
                return task.Result;
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

        public static void WaitAndUnwrap(this Task task)
        {
            try
            {
                task.Wait();
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
