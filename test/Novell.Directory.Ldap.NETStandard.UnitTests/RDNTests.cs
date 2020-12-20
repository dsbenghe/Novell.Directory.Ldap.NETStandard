using Novell.Directory.Ldap.Utilclass;
using System;
using System.Reflection;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class RDNTests
    {
        private readonly MethodInfo _equalAttrTypeMethod = typeof(Rdn).GetMethod("EqualAttrType", BindingFlags.Instance | BindingFlags.NonPublic);
        private bool EqualAttrTypeCheck(string val1, string val2) => (bool)_equalAttrTypeMethod.Invoke(new Rdn(), new object[] { val1, val2 });

        [Fact]
        public void EqualAttrType_OIDs_CompareTrue()
        {
            var val1 = "1.3.6.1.4.1.1466.20036";
            var val2 = "1.3.6.1.4.1.1466.20036";

            var result = EqualAttrTypeCheck(val1, val2);

            Assert.True(result);
        }

        [Fact]
        public void EqualAttrType_Names_CompareTrue()
        {
            var val1 = "Name";
            var val2 = "Name";

            var result = EqualAttrTypeCheck(val1, val2);

            Assert.True(result);
        }

        [Fact]
        public void EqualAttrType_Names_DifferentCase_CompareTrue()
        {
            var val1 = "NAME";
            var val2 = "Name";

            var result = EqualAttrTypeCheck(val1, val2);

            Assert.True(result);
        }

        [Fact]
        public void EqualAttrType_OidAndName_Exception()
        {
            var val1 = "Name";
            var val2 = "1.3.6.1.4.1.1466.20036";

            var ex = Assert.Throws<TargetInvocationException>(() => { EqualAttrTypeCheck(val1, val2); });
            Assert.IsType<ArgumentException>(ex.InnerException);
        }

        [Fact]
        public void Equals_SameInstance_True()
        {
            var rdn1 = new Rdn("cn=admin");

            Assert.True(rdn1.Equals(rdn1));
        }

        [Fact]
        public void Equals_SameRdn_True()
        {
            var rdn1 = new Rdn("cn=admin");
            var rdn2 = new Rdn("cn=admin");

            Assert.True(rdn1.Equals(rdn2));
        }

        [Fact]
        public void Equals_SameRdn_DifferentCasing_True()
        {
            var rdn1 = new Rdn("cn=admin");
            var rdn2 = new Rdn("CN=Admin");

            Assert.True(rdn1.Equals(rdn2));
        }

        [Fact]
        public void Equals_DifferentRdn_False()
        {
            var rdn1 = new Rdn("cn=admin1");
            var rdn2 = new Rdn("CN=Admin2");

            Assert.False(rdn1.Equals(rdn2));
        }

        [Fact]
        public void Ctor_MoreThanOneDn_Exception()
        {
            Assert.Throws<ArgumentException>(() => new Rdn("cn=admin, ou=marketing, o=corporation"));
        }

        [Fact]
        public void Ctor_EmptyDn_Exception()
        {
            Assert.Throws<ArgumentException>(() => new Rdn(string.Empty));
        }

        [Fact]
        public void Ctor_Null_Exception()
        {
            Assert.Throws<NullReferenceException>(() => new Rdn(null));
        }
    }
}
