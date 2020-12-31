using System.Collections.Generic;
using System.Text;

namespace Novell.Directory.Ldap
{
    /// <remarks>
    /// Taken from Bouncy Castle
    /// https://www.bouncycastle.org/
    /// Copyright (c) 2000 - 2017 The Legion of the Bouncy Castle Inc. (http://www.bouncycastle.org)
    ///
    /// Licensed under MIT License
    /// https://www.bouncycastle.org/csharp/licence.html.
    /// </remarks>
    /// <summary>
    /// Helper extension methods.
    /// </summary>
    internal static partial class ExtensionMethods
    {
        private static readonly byte[] HexEncodingTable =
        {
            (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
            (byte)'8', (byte)'9', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f',
        };

        /// <summary>
        /// Convert the byte array to an Hex string, e.g., { 0x00, 0xFF, 0xAB } => "00ffab".
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <param name="offset">The offset in the <paramref name="bytes"/> array to start converting (defaults to 0).</param>
        /// <param name="length">
        /// How many bytes in the <paramref name="bytes"/> to convert?
        /// (default to -1, which means "from <paramref name="offset"/> to the end".
        /// </param>
        internal static string ToHexString(this IReadOnlyList<byte> bytes, int offset = 0, int length = -1)
        {
            if (bytes == null || bytes.Count == 0)
            {
                return string.Empty;
            }

            if (length == -1)
            {
                length = bytes.Count;
            }

            var result = new byte[length * 2];
            var ix = 0;
            for (int i = offset; i < (offset + length); i++)
            {
                int v = bytes[i];
                result[ix++] = HexEncodingTable[v >> 4];
                result[ix++] = HexEncodingTable[v & 0xf];
            }

            return Encoding.ASCII.GetString(result, 0, result.Length);
        }
    }
}
