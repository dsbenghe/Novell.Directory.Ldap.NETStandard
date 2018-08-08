using System.Collections.Generic;
using System.Text;

namespace Novell.Directory.Ldap
{
    internal static partial class ExtensionMethods
    {
        /// <summary>
        /// Is the given collection null, or Empty (0 elements)?
        /// </summary>
        internal static bool IsEmpty<T>(this IReadOnlyCollection<T> coll) => coll == null || coll.Count == 0;

        /// <summary>
        /// Is the given collection not null, and has at least 1 element?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <returns></returns>
        internal static bool IsNotEmpty<T>(this IReadOnlyCollection<T> coll) => !IsEmpty(coll);

        /// <summary>
        /// Shortcut for Encoding.UTF8.GetBytes
        /// </summary>
        internal static byte[] ToUtf8Bytes(this string input) => Encoding.UTF8.GetBytes(input);

        internal static LdapResultCode AsResultCode (this Asn1.Asn1Numeric asn1Num)
        {
            return (LdapResultCode)asn1Num.IntValue();
        }
    }
}
