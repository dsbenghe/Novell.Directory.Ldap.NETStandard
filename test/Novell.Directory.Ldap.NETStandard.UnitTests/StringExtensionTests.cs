using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> DataSuccess =>
        new List<object[]>
        {
            // Empty strings
            new object[] { "", "", 0 },
            new object[] { "", "test", 0 },
            new object[] { "test", "", 0 }, 
            new object[] { "test", "", 4 }, 

            // Complete valid range
            new object[] { "abcd", "ab", 0 },
            new object[] { "abcd", "ab", 1 },
            new object[] { "abcd", "ab", 2 },
            new object[] { "abcd", "ab", 4 },

            // same length
            new object[] { "abcd", "abcd", 0 }, 
            new object[] { "abcd", "abcd", 1 },

            // Searchstring longer than baseString
            new object[] { "ab", "abcd", 0 }, 

            // Unicode
            new object[] { "大象牙膏", "象牙", 0 },
            new object[] { "大象牙膏", "象牙", 1 },
            new object[] { "大象牙膏", "象牙", 2 },
            new object[] { "大象牙膏", "象牙", 3 },
            new object[] { "大象牙膏", "象牙", 4 },
            new object[] { "зубная паста слона", "аста", 8 }
        };

        [Theory]
        [MemberData(nameof(DataException))]
        public void StartsWithStringAtOffset_Exception(string baseString, string searchString, int offset)
        {
            var substringStartsWithException = Assert.ThrowsAny<Exception>(() => baseString.Substring(offset).StartsWith(searchString));
            var startsWithStringAtOffsetException = Assert.ThrowsAny<Exception>(() => baseString.StartsWithStringAtOffset(searchString, offset));
            // Same exception type?
            Assert.IsType(substringStartsWithException.GetType(), startsWithStringAtOffsetException);
        }

        public static IEnumerable<object[]> DataException =>
        new List<object[]>
        {
            // Null Argument
            new object[] {"abcd", null, 0},
            
            // Invalid offset
            new object[] { "abcd", "abcd", 5 },
            new object[] { "", "abcd", 1 },
            new object[] { "abcd", "ab", 5 },
            new object[] { "abcd", "ab", -1 },
            new object[] { "大象牙膏", "象牙", 5 },
        };
    }
}
