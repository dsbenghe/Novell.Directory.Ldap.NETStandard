using System;
using System.Collections.Generic;
using System.Text;

namespace Novell.Directory.Ldap
{
    internal static partial class ExtensionMethods
    {
        /// <summary>
        /// Shortcut for <see cref="string.IsNullOrEmpty"/>.
        /// </summary>
        internal static bool IsEmpty(this string input) => string.IsNullOrEmpty(input);

        /// <summary>
        /// Shortcut for negative <see cref="string.IsNullOrEmpty"/>.
        /// </summary>
        internal static bool IsNotEmpty(this string input) => !IsEmpty(input);

        /// <summary>
        /// Is the given collection null, or Empty (0 elements)?.
        /// </summary>
        internal static bool IsEmpty<T>(this IReadOnlyCollection<T> coll) => coll == null || coll.Count == 0;

        /// <summary>
        /// Is the given collection not null, and has at least 1 element?.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        internal static bool IsNotEmpty<T>(this IReadOnlyCollection<T> coll) => !IsEmpty(coll);

        /// <summary>
        /// Shortcut for <see cref="UTF8Encoding.GetBytes"/>.
        /// </summary>
        internal static byte[] ToUtf8Bytes(this string input) => Encoding.UTF8.GetBytes(input);

        /// <summary>
        /// Shortcut for <see cref="UTF8Encoding.GetString"/>
        /// Will return an empty string if <paramref name="input"/> is null or empty.
        /// </summary>
        internal static string ToUtf8String(this byte[] input)
            => input.IsNotEmpty() ? Encoding.UTF8.GetString(input) : string.Empty;

        /// <summary>
        /// Compare two strings using <see cref="StringComparison.Ordinal"/>.
        /// </summary>
        internal static bool EqualsOrdinal(this string input, string other)
            => string.Equals(input, other, StringComparison.Ordinal);

        /// <summary>
        /// Compare two strings using <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        internal static bool EqualsOrdinalCI(this string input, string other)
            => string.Equals(input, other, StringComparison.OrdinalIgnoreCase);
    }
}
