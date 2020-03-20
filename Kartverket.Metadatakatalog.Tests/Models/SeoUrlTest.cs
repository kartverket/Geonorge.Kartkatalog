using FluentAssertions;
using Kartverket.Metadatakatalog.Models;
using Xunit;

namespace Kartverket.Metadatakatalog.Tests.Models
{
    public class SeoUrlTest
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldLowerCase()
        {
            var seoUrl = new SeoUrl("SkogOgLandskap", "Ar50");
            seoUrl.Organization.Should().Be("skogoglandskap");
            seoUrl.Title.Should().Be("ar50");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldRemoveApostroph()
        {
            var seoUrl = new SeoUrl("skog'", "a'r");
            seoUrl.Organization.Should().Be("skog");
            seoUrl.Title.Should().Be("ar");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldReplaceNorwegianCharacters()
        {
            var seoUrl = new SeoUrl("SkØgÅLÆndskap", "årøæ5");
            seoUrl.Organization.Should().Be("skogalaendskap");
            seoUrl.Title.Should().Be("aroae5");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldReplaceSpecialCharacters()
        {
            var seoUrl = new SeoUrl("skog$og@landska%p", "a~r 5");
            seoUrl.Organization.Should().Be("skog-og-landska-p");
            seoUrl.Title.Should().Be("a-r-5");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldRemoveDuplicateHyphens()
        {
            var seoUrl = new SeoUrl("skog og -landskap", "ar - 5");
            seoUrl.Organization.Should().Be("skog-og-landskap");
            seoUrl.Title.Should().Be("ar-5");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldRemoveLeadingAndTrailingHyphens()
        {
            var seoUrl = new SeoUrl("-skog og landskap-", "-ar5-");
            seoUrl.Organization.Should().Be("skog-og-landskap");
            seoUrl.Title.Should().Be("ar5");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldReturnTrueWhenBothOrganizationAndTitleMatches()
        {
            var seoUrl = new SeoUrl("Skog og landskap", "WMS AR-5");
            seoUrl.Matches("skog-og-landskap", "wms-ar-5").Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldReturnFalseWhenOrganizationDoesNotMatch()
        {
            var seoUrl = new SeoUrl("Skog og landskap", "AR-5");
            seoUrl.Matches("skogoglandskap", "ar-5").Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldReturnFalseWhenTitleDoesNotMatch()
        {
            var seoUrl = new SeoUrl("Skog og landskap", "AR-5");
            seoUrl.Matches("skog-og-landskap", "ar5sadfsdf").Should().BeFalse();
        }
    }
}
