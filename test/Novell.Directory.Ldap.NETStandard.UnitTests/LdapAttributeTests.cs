using System;
using System.Linq;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    /// <summary>
    /// Value-list semantics of <see cref="LdapAttribute"/>: duplicate handling,
    /// insertion order, exact-length views, removal — including around the internal
    /// linear-scan/hash-set threshold and with large value counts, where adding used
    /// to cost O(n^2) comparisons and array copies.
    /// </summary>
    public class LdapAttributeTests
    {
        [Fact]
        public void AddValue_DropsDuplicates_AndPreservesInsertionOrder()
        {
            var attr = new LdapAttribute("member");

            // Cross the internal dedup threshold (8) to exercise both the linear
            // and the hash-set duplicate checks.
            for (var i = 0; i < 12; i++)
            {
                attr.AddValue("value" + i);
                attr.AddValue("value" + i); // immediate duplicate
            }

            attr.AddValue("value0");  // late duplicate, checked via hash set
            attr.AddValue("value11");

            Assert.Equal(12, attr.Size());
            Assert.Equal(Enumerable.Range(0, 12).Select(i => "value" + i).ToArray(), attr.StringValueArray);
        }

        [Fact]
        public void ByteAndStringViews_ReportExactCount_NoSpareCapacity()
        {
            var attr = new LdapAttribute("cn");

            // 9 values: above the dedup threshold, so the internal array over-allocates.
            for (var i = 0; i < 9; i++)
            {
                attr.AddValue("v" + i);
            }

            Assert.Equal(9, attr.Size());
            Assert.Equal(9, attr.StringValueArray.Length);
            Assert.Equal(9, attr.ByteValueArray.Length);
            Assert.Equal(9, attr.StringValues.Count());
            Assert.Equal(9, attr.ByteValues.Count());
            Assert.All(attr.ByteValueArray, v => Assert.NotNull(v));
        }

        [Fact]
        public void DuplicateCheck_ComparesContent_NotReference()
        {
            var attr = new LdapAttribute("jpegPhoto");
            attr.AddValue(new byte[] { 1, 2, 3 });
            attr.AddValue(new byte[] { 1, 2, 3 }); // equal content, different instance

            Assert.Equal(1, attr.Size());
        }

        [Fact]
        public void RemoveValue_AfterBulkAdds_RemovesAndAllowsReAdd()
        {
            var attr = new LdapAttribute("member");
            for (var i = 0; i < 20; i++)
            {
                attr.AddValue("m" + i);
            }

            attr.RemoveValue("m7");
            Assert.Equal(19, attr.Size());
            Assert.DoesNotContain("m7", attr.StringValueArray);

            attr.AddValue("m7"); // not a duplicate anymore
            Assert.Equal(20, attr.Size());

            attr.AddValue("m8"); // still a duplicate
            Assert.Equal(20, attr.Size());
        }

        [Fact]
        public void RemoveValue_LastValue_EmptiesAttribute()
        {
            var attr = new LdapAttribute("cn", "single");
            attr.RemoveValue("single");

            Assert.Equal(0, attr.Size());
            Assert.Empty(attr.StringValueArray);
        }

        [Fact]
        public void Clone_IsIndependentOfOriginal()
        {
            var attr = new LdapAttribute("member");
            for (var i = 0; i < 10; i++)
            {
                attr.AddValue("m" + i);
            }

            var clone = attr.Clone();
            clone.AddValue("clone-only");
            clone.AddValue("m0"); // duplicate within clone

            Assert.Equal(10, attr.Size());
            Assert.Equal(11, clone.Size());
            Assert.DoesNotContain("clone-only", attr.StringValueArray);
        }

        [Fact]
        public void CopyConstructor_CopiesExactValues()
        {
            var attr = new LdapAttribute("member");
            for (var i = 0; i < 10; i++)
            {
                attr.AddValue("m" + i);
            }

            var copy = new LdapAttribute(attr);
            Assert.Equal(10, copy.Size());
            Assert.Equal(attr.StringValueArray, copy.StringValueArray);
        }

        [Fact]
        public void AddValue_ManyValues_CompletesQuickly()
        {
            // 100K unique values. The former per-value duplicate scan plus
            // Array.Resize(+1) growth made this take minutes to hours; with the
            // hash-set dedup and geometric growth it finishes in well under the
            // test timeout.
            const int n = 100_000;
            var attr = new LdapAttribute("member");
            for (var i = 0; i < n; i++)
            {
                attr.AddValue("cn=user" + i.ToString("D6") + ",ou=people,dc=example,dc=com");
            }

            Assert.Equal(n, attr.Size());
            Assert.Equal(n, attr.StringValueArray.Length);
        }
    }
}
