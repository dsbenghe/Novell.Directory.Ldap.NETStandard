using System.Threading;

namespace Novell.Directory.Ldap
{
    public interface IDebugIdentifier
    {
        DebugId DebugId { get; }
    }

    public struct DebugId
    {
        private static int _id;
        private static int GetNextId()
        {
            // Rollover is OK in case we somehow end up with more than 2147483647 calls to this...
            return Interlocked.Increment(ref _id);
        }

        /// <summary>
        /// A name that can identify an object instance.
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// An incrementing Id for every new object.
        /// Note: This Id increments for every newly generated DebugId,
        /// regardless of the Name.
        /// </summary>
        private int Id { get; }

        public override string ToString()
             => "[#" + Id + "] " + Name;

        public DebugId(string name)
        {
            Name = name;
            Id = GetNextId();
        }

        public static DebugId ForType<T>()
            => new DebugId(typeof(T).FullName);
    }
}
