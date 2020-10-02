using Novell.Directory.Ldap.Rfc2251;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
	public class RfcFilterTests
	{
		[Fact]
		public void RfcFilter_WithUtf16Surrogates_Success()
		{
			var filterString = "(sAMAccountName=user🐉👽✨)";
			var filter = new RfcFilter(filterString);

			Assert.Equal(filterString, filter.FilterToString());
		}
	}
}