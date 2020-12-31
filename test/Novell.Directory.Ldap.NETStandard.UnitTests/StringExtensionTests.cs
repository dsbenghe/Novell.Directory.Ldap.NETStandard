using Novell.Directory.Ldap.Utilclass;
using System;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class StringExtensionTests
    {
        [Theory]
        [MemberData(nameof(DataSuccess))]
        public void StartsWithStringAtOffset_Success(string baseString, string searchString, int offset)
        {
            Assert.Equal(baseString.StartsWithStringAtOffset(searchString, offset), baseString.Substring(offset).StartsWith(searchString));
        }

        public static TheoryData<string, string, int> DataSuccess
        {
            get
            {
                return new TheoryData<string, string, int>
                {
                    // Empty strings
                    { string.Empty, string.Empty, 0 },
                    { string.Empty, "test", 0 },
                    { "test", string.Empty, 0 },
                    { "test", string.Empty, 4 },

                    // Complete valid range
                    { "abcd", "ab", 0 },
                    { "abcd", "ab", 1 },
                    { "abcd", "ab", 2 },
                    { "abcd", "ab", 4 },

                    // same length
                    { "abcd", "abcd", 0 },
                    { "abcd", "abcd", 1 },

                    // Searchstring longer than baseString
                    { "ab", "abcd", 0 },

                    // Unicode
                    { "大象牙膏", "象牙", 0 },
                    { "大象牙膏", "象牙", 1 },
                    { "大象牙膏", "象牙", 2 },
                    { "大象牙膏", "象牙", 3 },
                    { "大象牙膏", "象牙", 4 },
                    { "зубная паста слона", "аста", 8 },
                };
            }
        }

        [Theory]
        [MemberData(nameof(DataException))]
        public void StartsWithStringAtOffset_Exception(string baseString, string searchString, int offset)
        {
            var substringStartsWithException = Assert.ThrowsAny<Exception>(() => baseString.Substring(offset).StartsWith(searchString));
            var startsWithStringAtOffsetException = Assert.ThrowsAny<Exception>(() => baseString.StartsWithStringAtOffset(searchString, offset));

            // Same exception type?
            Assert.IsType(substringStartsWithException.GetType(), startsWithStringAtOffsetException);
        }

        public static TheoryData<string, string, int> DataException
        {
            get
            {
                return new TheoryData<string, string, int>
                {
                    // Null Argument
                    { "abcd", null, 0 },

                    // Invalid offset
                    { "abcd", "abcd", 5 },
                    { string.Empty, "abcd", 1 },
                    { "abcd", "ab", 5 },
                    { "abcd", "ab", -1 },
                    { "大象牙膏", "象牙", 5 },
                };
            }
        }
    }
}
