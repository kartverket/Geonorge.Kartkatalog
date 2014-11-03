using System.Collections.Generic;
using FluentAssertions;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Service;
using NUnit.Framework;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    class ThemeResolverTest
    {
        [Test]
        public void ShouldReturnNullWhenNoKeywordsExist()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();

            string theme = new ThemeResolver().Resolve(metadata);
            theme.Should().BeNull();
        }

        [Test]
        public void ShouldReturnNullWhenInspireKeywordsDoesNotExist()
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            metadata.Keywords.Add(new SimpleKeyword() { Keyword = "testing" });

            string theme = new ThemeResolver().Resolve(metadata);
            theme.Should().BeNull();
        }

        [Test]
        public void InspireCoordinateReferenceSystemsResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Coordinate reference systems", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireGeographicalNamesResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Geographical names", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireAdministrativeUnitsResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Administrative units", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireAddressesResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Addresses", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireCadastralParcelsResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Cadastral parcels", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireTransportNetworksResolveToSamferdsel()
        {
            ResolveInspireKeyword("Transport networks", ThemeResolver.DokSamferdsel);
        }

        [Test]
        public void InspireHydrographyResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Hydrography", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireProtectedSitesResolveToNatur()
        {
            ResolveInspireKeyword("Protected sites", ThemeResolver.DokNatur);
        }

        [Test]
        public void InspireElevationResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Elevation", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireLandCoverResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Land cover", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireOrthoimageryResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Orthoimagery", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireGeologyResolveToGeologi()
        {
            ResolveInspireKeyword("Geology", ThemeResolver.DokGeologi);
        }
        
        [Test]
        public void InspireStatisticalUnitsResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Statistical units", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireBuildingsResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Buildings", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireSoilResolveToLandbruk()
        {
            ResolveInspireKeyword("Soil", ThemeResolver.DokLandbruk);
        }

        [Test]
        public void InspireLandUseResolveToPlan()
        {
            ResolveInspireKeyword("Land use", ThemeResolver.DokPlan);
        }

        [Test]
        public void InspireHumanHealthResolveToEnergi()
        {
            ResolveInspireKeyword("Human health and safety", ThemeResolver.DokEnergi);
        }

        [Test]
        public void InspireUtilityAndGovernmentalServicesResolveToSamfunnssikkerhet()
        {
            ResolveInspireKeyword("Utility and governmental services", ThemeResolver.DokSamfunnssikkerhet);
        }

        [Test]
        public void InspireEnvironmentalMonitoringFacilitiesResolveToForurensning()
        {
            ResolveInspireKeyword("Environmental monitoring facilities", ThemeResolver.DokForurensning);
        }

        [Test]
        public void InspireProductionAndIndustrialFacilitiesResolveToBasisGeodata()
        {
            ResolveInspireKeyword("Production and industrial facilities", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void InspireAgriculturalAndAquacultureFacilitiesResolveToLandbruk()
        {
            ResolveInspireKeyword("Agricultural and aquaculture facilities", ThemeResolver.DokLandbruk);
        }

        [Test]
        public void InspirePopulationDistributionResolveToLandbruk()
        {
            ResolveInspireKeyword("Population distribution and demography", ThemeResolver.DokAnnen);
        }

        private static void ResolveInspireKeyword(string inspireKeyword, string expectedTheme)
        {
            ResolveTheme(createMetadataWithInspireKeyword(inspireKeyword)).Should().Be(expectedTheme);
        }

        private static string ResolveTheme(SimpleMetadata metadata)
        {
            return new ThemeResolver().Resolve(metadata);
        }

        private static SimpleMetadata createMetadataWithInspireKeyword(string inspireKeyword)
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            var keywords = new List<SimpleKeyword>();
            keywords.Add(new SimpleKeyword()
            {
                Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1,
                Keyword = inspireKeyword
            });
            metadata.Keywords = keywords;

            return metadata;
        }
    }
}
