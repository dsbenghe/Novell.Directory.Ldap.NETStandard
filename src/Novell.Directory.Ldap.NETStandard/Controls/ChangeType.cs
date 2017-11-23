using System;

namespace Novell.Directory.Ldap.NETStandard.Controls
{
    [Flags]
    public enum ChangeType : int
    {
        /// <summary>
        ///     Change type specifying that you want to track additions of new entries
        ///     to the directory.
        /// </summary>
        ADD = 1,

        /// <summary>
        ///     Change type specifying that you want to track removals of entries from
        ///     the directory.
        /// </summary>
        DELETE = 2,

        /// <summary>
        ///     Change type specifying that you want to track modifications of entries
        ///     in the directory.
        /// </summary>
        MODIFY = 4,

        /// <summary>
        ///     Change type specifying that you want to track modifications of the DNs
        ///     of entries in the directory.
        /// </summary>
        MODDN = 8,

        /// <summary>
        ///     Change type specifying that you want to track any of the above
        ///     modifications.
        /// </summary>
        ALL = ADD | DELETE | MODIFY | MODDN
    }
}
