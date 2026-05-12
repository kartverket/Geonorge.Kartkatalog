using System.Collections.Generic;
using FluentAssertions;
using GeoNorgeAPI;
using Xunit;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class ThemeResolverTest
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void SimpleMetadata_ShouldBeCreated_ForTesting()
        {
            // Arrange & Act
            var metadata = CreateSimpleMetadata();

            // Assert
            metadata.Should().NotBeNull();
            metadata.Keywords.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MetadataWithDokKeyword_ShouldContainCorrectKeyword()
        {
            // Arrange
            const string expectedKeyword = "Landbruk";

            // Act
            var metadata = CreateMetadataWithDokKeyword(expectedKeyword);

            // Assert
            metadata.Keywords.Should().HaveCount(1);
            metadata.Keywords[0].Keyword.Should().Be(expectedKeyword);
            metadata.Keywords[0].Thesaurus.Should().Be(SimpleKeyword.THESAURUS_NATIONAL_THEME);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MetadataWithDokKeyword_ShouldSupportEnglishKeyword()
        {
            // Arrange
            const string norwegianKeyword = "Landbruk";
            const string englishKeyword = "Agriculture";

            // Act
            var metadata = CreateMetadataWithDokKeyword(norwegianKeyword, englishKeyword);

            // Assert
            metadata.Keywords[0].Keyword.Should().Be(norwegianKeyword);
            metadata.Keywords[0].EnglishKeyword.Should().Be(englishKeyword);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ThemeResolverConstants_ShouldHaveExpectedValues()
        {
            // This test validates the constant values that ThemeResolver uses
            // These should match the expected theme names

            // Act & Assert - Testing that the constants exist and have expected values
            SimpleKeyword.THESAURUS_NATIONAL_THEME.Should().NotBeNullOrEmpty();
            SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1.Should().NotBeNullOrEmpty();

            // Verify some known thesaurus values based on actual constants
            SimpleKeyword.THESAURUS_NATIONAL_THEME.Should().Contain("DOK-kategori");
            SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1.Should().Contain("INSPIRE");
        }

        [Fact]
        [Trait("Category", "Unit")]  
        public void KeywordFilter_ShouldWorkCorrectly_WithNationalThemeKeywords()
        {
            // Arrange
            var keywords = new List<SimpleKeyword>
            {
                new SimpleKeyword 
                { 
                    Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_THEME, 
                    Keyword = "Landbruk" 
                },
                new SimpleKeyword 
                { 
                    Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1, 
                    Keyword = "Transport networks" 
                },
                new SimpleKeyword 
                { 
                    Thesaurus = null, 
                    Keyword = "Other" 
                }
            };

            // Act
            var filteredKeywords = SimpleKeyword.Filter(keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME);

            // Assert
            filteredKeywords.Should().HaveCount(1);
            filteredKeywords[0].Keyword.Should().Be("Landbruk");
        }

        private static SimpleMetadata CreateSimpleMetadata()
        {
            var metadata = SimpleMetadata.CreateDataset();
            metadata.Keywords = new List<SimpleKeyword>();
            return metadata;
        }

        private static SimpleMetadata CreateMetadataWithDokKeyword(string norwegianKeyword, string? englishKeyword = null)
        {
            var metadata = SimpleMetadata.CreateDataset();
            var keywords = new List<SimpleKeyword>
            {
                new SimpleKeyword
                {
                    Thesaurus = SimpleKeyword.THESAURUS_NATIONAL_THEME,
                    Keyword = norwegianKeyword,
                    EnglishKeyword = englishKeyword
                }
            };
            metadata.Keywords = keywords;
            return metadata;
        }
    }
}