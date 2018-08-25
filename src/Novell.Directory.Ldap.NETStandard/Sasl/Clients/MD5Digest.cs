/* Taken from Bouncy Castle
 * https://www.bouncycastle.org/
 * Copyright (c) 2000 - 2017 The Legion of the Bouncy Castle Inc. (http://www.bouncycastle.org)
 *
 * Licensed under MIT License
 * https://www.bouncycastle.org/csharp/licence.html
 */
using System;

namespace Novell.Directory.Ldap.Sasl.Clients
{
    internal sealed class MD5Digest
    {
        private const int BYTE_LENGTH = 64;

        private readonly byte[] xBuf;
        private int xBufOff;

        private long byteCount;

        private const int DigestLength = 16;

        private uint H1, H2, H3, H4;         // IV's

        private readonly uint[] X = new uint[16];
        private int xOff;

        internal MD5Digest()
        {
            xBuf = new byte[4];
            Reset();
        }

        internal MD5Digest(MD5Digest t)
        {
            xBuf = new byte[t.xBuf.Length];
            CopyIn(t);
            Reset();
        }

        private void CopyIn(MD5Digest t)
        {
            Array.Copy(t.xBuf, 0, xBuf, 0, t.xBuf.Length);

            xBufOff = t.xBufOff;
            byteCount = t.byteCount;

            H1 = t.H1;
            H2 = t.H2;
            H3 = t.H3;
            H4 = t.H4;

            Array.Copy(t.X, 0, X, 0, t.X.Length);
            xOff = t.xOff;
        }

        public void Update(byte input)
        {
            xBuf[xBufOff++] = input;

            if (xBufOff == xBuf.Length)
            {
                ProcessWord(xBuf, 0);
                xBufOff = 0;
            }

            byteCount++;
        }

        public void BlockUpdate(byte[] input)
            => BlockUpdate(input, 0, input.Length);

        public void BlockUpdate(byte[] input, int inOff, int length)
        {
            length = Math.Max(0, length);

            //
            // fill the current word
            //
            int i = 0;
            if (xBufOff != 0)
            {
                while (i < length)
                {
                    xBuf[xBufOff++] = input[inOff + i++];
                    if (xBufOff == 4)
                    {
                        ProcessWord(xBuf, 0);
                        xBufOff = 0;
                        break;
                    }
                }
            }

            //
            // process whole words.
            //
            int limit = ((length - i) & ~3) + i;
            for (; i < limit; i += 4)
            {
                ProcessWord(input, inOff + i);
            }

            //
            // load in the remainder.
            //
            while (i < length)
            {
                xBuf[xBufOff++] = input[inOff + i++];
            }

            byteCount += length;
        }

        public void Finish()
        {
            long bitLength = (byteCount << 3);

            //
            // add the pad bytes.
            //
            Update((byte)128);

            while (xBufOff != 0) Update((byte)0);
            ProcessLength(bitLength);
            ProcessBlock();
        }

        public void Reset()
        {
            byteCount = 0;
            xBufOff = 0;
            Array.Clear(xBuf, 0, xBuf.Length);

            H1 = 0x67452301;
            H2 = 0xefcdab89;
            H3 = 0x98badcfe;
            H4 = 0x10325476;

            xOff = 0;

            for (int i = 0; i != X.Length; i++)
            {
                X[i] = 0;
            }
        }

        public int GetByteLength()
        {
            return BYTE_LENGTH;
        }

        public string AlgorithmName => "MD5";
        public int GetDigestSize() => DigestLength;

        internal void ProcessWord(byte[] input, int inOff)
        {
            X[xOff] = LE_To_UInt32(input, inOff);

            if (++xOff == 16)
            {
                ProcessBlock();
            }
        }

        internal void ProcessLength(long bitLength)
        {
            if (xOff > 14)
            {
                if (xOff == 15)
                    X[15] = 0;

                ProcessBlock();
            }

            for (int i = xOff; i < 14; ++i)
            {
                X[i] = 0;
            }

            X[14] = (uint)((ulong)bitLength);
            X[15] = (uint)((ulong)bitLength >> 32);
        }

        public int DoFinal(byte[] output)
            => DoFinal(output, 0);

        public int DoFinal(byte[] output, int outOff)
        {
            Finish();

            UInt32_To_LE(H1, output, outOff);
            UInt32_To_LE(H2, output, outOff + 4);
            UInt32_To_LE(H3, output, outOff + 8);
            UInt32_To_LE(H4, output, outOff + 12);

            Reset();

            return DigestLength;
        }

        internal static void UInt32_To_LE(uint n, byte[] bs, int off)
        {
            bs[off] = (byte)(n);
            bs[off + 1] = (byte)(n >> 8);
            bs[off + 2] = (byte)(n >> 16);
            bs[off + 3] = (byte)(n >> 24);
        }

        internal static uint LE_To_UInt32(byte[] bs, int off)
        {
            return (uint)bs[off]
                | (uint)bs[off + 1] << 8
                | (uint)bs[off + 2] << 16
                | (uint)bs[off + 3] << 24;
        }

        //
        // round 1 left rotates
        //
        private const int S11 = 7;
        private const int S12 = 12;
        private const int S13 = 17;
        private const int S14 = 22;

        //
        // round 2 left rotates
        //
        private const int S21 = 5;
        private const int S22 = 9;
        private const int S23 = 14;
        private const int S24 = 20;

        //
        // round 3 left rotates
        //
        private const int S31 = 4;
        private const int S32 = 11;
        private const int S33 = 16;
        private const int S34 = 23;

        //
        // round 4 left rotates
        //
        private const int S41 = 6;
        private const int S42 = 10;
        private const int S43 = 15;
        private const int S44 = 21;

        /*
        * rotate int x left n bits.
        */
        private static uint RotateLeft(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        /*
        * F, G, H and I are the basic MD5 functions.
        */
        private static uint F(uint u, uint v, uint w)
        {
            return (u & v) | (~u & w);
        }

        private static uint G(uint u, uint v, uint w)
        {
            return (u & w) | (v & ~w);
        }

        private static uint H(uint u, uint v, uint w)
        {
            return u ^ v ^ w;
        }

        private static uint K(uint u, uint v, uint w)
        {
            return v ^ (u | ~w);
        }

#pragma warning disable RCS1032 // Remove redundant parentheses.
        internal void ProcessBlock()
        {
            uint a = H1;
            uint b = H2;
            uint c = H3;
            uint d = H4;

            //
            // Round 1 - F cycle, 16 times.
            //
            a = RotateLeft((a + F(b, c, d) + X[0] + 0xd76aa478), S11) + b;
            d = RotateLeft((d + F(a, b, c) + X[1] + 0xe8c7b756), S12) + a;
            c = RotateLeft((c + F(d, a, b) + X[2] + 0x242070db), S13) + d;
            b = RotateLeft((b + F(c, d, a) + X[3] + 0xc1bdceee), S14) + c;
            a = RotateLeft((a + F(b, c, d) + X[4] + 0xf57c0faf), S11) + b;
            d = RotateLeft((d + F(a, b, c) + X[5] + 0x4787c62a), S12) + a;
            c = RotateLeft((c + F(d, a, b) + X[6] + 0xa8304613), S13) + d;
            b = RotateLeft((b + F(c, d, a) + X[7] + 0xfd469501), S14) + c;
            a = RotateLeft((a + F(b, c, d) + X[8] + 0x698098d8), S11) + b;
            d = RotateLeft((d + F(a, b, c) + X[9] + 0x8b44f7af), S12) + a;
            c = RotateLeft((c + F(d, a, b) + X[10] + 0xffff5bb1), S13) + d;
            b = RotateLeft((b + F(c, d, a) + X[11] + 0x895cd7be), S14) + c;
            a = RotateLeft((a + F(b, c, d) + X[12] + 0x6b901122), S11) + b;
            d = RotateLeft((d + F(a, b, c) + X[13] + 0xfd987193), S12) + a;
            c = RotateLeft((c + F(d, a, b) + X[14] + 0xa679438e), S13) + d;
            b = RotateLeft((b + F(c, d, a) + X[15] + 0x49b40821), S14) + c;

            //
            // Round 2 - G cycle, 16 times.
            //
            a = RotateLeft((a + G(b, c, d) + X[1] + 0xf61e2562), S21) + b;
            d = RotateLeft((d + G(a, b, c) + X[6] + 0xc040b340), S22) + a;
            c = RotateLeft((c + G(d, a, b) + X[11] + 0x265e5a51), S23) + d;
            b = RotateLeft((b + G(c, d, a) + X[0] + 0xe9b6c7aa), S24) + c;
            a = RotateLeft((a + G(b, c, d) + X[5] + 0xd62f105d), S21) + b;
            d = RotateLeft((d + G(a, b, c) + X[10] + 0x02441453), S22) + a;
            c = RotateLeft((c + G(d, a, b) + X[15] + 0xd8a1e681), S23) + d;
            b = RotateLeft((b + G(c, d, a) + X[4] + 0xe7d3fbc8), S24) + c;
            a = RotateLeft((a + G(b, c, d) + X[9] + 0x21e1cde6), S21) + b;
            d = RotateLeft((d + G(a, b, c) + X[14] + 0xc33707d6), S22) + a;
            c = RotateLeft((c + G(d, a, b) + X[3] + 0xf4d50d87), S23) + d;
            b = RotateLeft((b + G(c, d, a) + X[8] + 0x455a14ed), S24) + c;
            a = RotateLeft((a + G(b, c, d) + X[13] + 0xa9e3e905), S21) + b;
            d = RotateLeft((d + G(a, b, c) + X[2] + 0xfcefa3f8), S22) + a;
            c = RotateLeft((c + G(d, a, b) + X[7] + 0x676f02d9), S23) + d;
            b = RotateLeft((b + G(c, d, a) + X[12] + 0x8d2a4c8a), S24) + c;

            //
            // Round 3 - H cycle, 16 times.
            //
            a = RotateLeft((a + H(b, c, d) + X[5] + 0xfffa3942), S31) + b;
            d = RotateLeft((d + H(a, b, c) + X[8] + 0x8771f681), S32) + a;
            c = RotateLeft((c + H(d, a, b) + X[11] + 0x6d9d6122), S33) + d;
            b = RotateLeft((b + H(c, d, a) + X[14] + 0xfde5380c), S34) + c;
            a = RotateLeft((a + H(b, c, d) + X[1] + 0xa4beea44), S31) + b;
            d = RotateLeft((d + H(a, b, c) + X[4] + 0x4bdecfa9), S32) + a;
            c = RotateLeft((c + H(d, a, b) + X[7] + 0xf6bb4b60), S33) + d;
            b = RotateLeft((b + H(c, d, a) + X[10] + 0xbebfbc70), S34) + c;
            a = RotateLeft((a + H(b, c, d) + X[13] + 0x289b7ec6), S31) + b;
            d = RotateLeft((d + H(a, b, c) + X[0] + 0xeaa127fa), S32) + a;
            c = RotateLeft((c + H(d, a, b) + X[3] + 0xd4ef3085), S33) + d;
            b = RotateLeft((b + H(c, d, a) + X[6] + 0x04881d05), S34) + c;
            a = RotateLeft((a + H(b, c, d) + X[9] + 0xd9d4d039), S31) + b;
            d = RotateLeft((d + H(a, b, c) + X[12] + 0xe6db99e5), S32) + a;
            c = RotateLeft((c + H(d, a, b) + X[15] + 0x1fa27cf8), S33) + d;
            b = RotateLeft((b + H(c, d, a) + X[2] + 0xc4ac5665), S34) + c;

            //
            // Round 4 - K cycle, 16 times.
            //
            a = RotateLeft((a + K(b, c, d) + X[0] + 0xf4292244), S41) + b;
            d = RotateLeft((d + K(a, b, c) + X[7] + 0x432aff97), S42) + a;
            c = RotateLeft((c + K(d, a, b) + X[14] + 0xab9423a7), S43) + d;
            b = RotateLeft((b + K(c, d, a) + X[5] + 0xfc93a039), S44) + c;
            a = RotateLeft((a + K(b, c, d) + X[12] + 0x655b59c3), S41) + b;
            d = RotateLeft((d + K(a, b, c) + X[3] + 0x8f0ccc92), S42) + a;
            c = RotateLeft((c + K(d, a, b) + X[10] + 0xffeff47d), S43) + d;
            b = RotateLeft((b + K(c, d, a) + X[1] + 0x85845dd1), S44) + c;
            a = RotateLeft((a + K(b, c, d) + X[8] + 0x6fa87e4f), S41) + b;
            d = RotateLeft((d + K(a, b, c) + X[15] + 0xfe2ce6e0), S42) + a;
            c = RotateLeft((c + K(d, a, b) + X[6] + 0xa3014314), S43) + d;
            b = RotateLeft((b + K(c, d, a) + X[13] + 0x4e0811a1), S44) + c;
            a = RotateLeft((a + K(b, c, d) + X[4] + 0xf7537e82), S41) + b;
            d = RotateLeft((d + K(a, b, c) + X[11] + 0xbd3af235), S42) + a;
            c = RotateLeft((c + K(d, a, b) + X[2] + 0x2ad7d2bb), S43) + d;
            b = RotateLeft((b + K(c, d, a) + X[9] + 0xeb86d391), S44) + c;

            H1 += a;
            H2 += b;
            H3 += c;
            H4 += d;

            xOff = 0;
        }
    }
}
