using System.Collections.Generic;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    /// Provides a readonly list-view over a <c>byte[][]</c>-value.
    /// Returned values are UTF-8-decoded strings of the values of the backing array.
    /// </summary>
    public sealed class ByteArrayAsUtf8StringView : ViewBase<byte[], string>
    {
        public ByteArrayAsUtf8StringView(IReadOnlyList<byte[]> byteValues)
            : base(byteValues)
        {
        }

        public override string this[int index] => InnerValues[index].ToUtf8String();

        public override int IndexOf(string item)
        {
            if (item == null)
            {
                return -1;
            }

            for (int i = 0; i < InnerValues.Count; i++)
            {
                var value = this[i];
                if (string.Equals(value, item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
