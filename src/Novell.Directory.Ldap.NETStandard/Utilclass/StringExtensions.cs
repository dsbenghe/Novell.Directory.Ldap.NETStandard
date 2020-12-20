using System;

namespace Novell.Directory.Ldap.Utilclass
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces string.Substring(offset).StartsWith(value) and avoids memory allocations.
        /// </summary>
        public static bool StartsWithStringAtOffset(this string baseString, string value, int offset)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (offset > baseString.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be larger than length of string");
            }

            if (offset + value.Length > baseString.Length)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (baseString[offset + i] != value[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
