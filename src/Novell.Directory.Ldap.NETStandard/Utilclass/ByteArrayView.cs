using System;
using System.Collections.Generic;
using System.Linq;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    /// Provides a readonly list-view over a <c>byte[][]</c>-value.
    /// Returned values are copies of the values of the backing array, and hence modifing them does not change the original value.
    /// </summary>
    public sealed class ByteArrayView : ViewBase<byte[], byte[]>
    {
        public ByteArrayView(IReadOnlyList<byte[]> byteValues)
            : base(byteValues)
        {
        }

        public override byte[] this[int index] => (byte[])InnerValues[index].Clone();

        public ReadOnlySpan<byte> GetAsSpan(int index)
        {
            return InnerValues[index];
        }

        public override int IndexOf(byte[] item)
        {
            if (item == null)
            {
                return -1;
            }

            for (int i = 0; i < InnerValues.Count; i++)
            {
                if (InnerValues[i].SequenceEqual(item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
