using System.Collections.Generic;
using FluentAssertions;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Service;
using Xunit;

namespace Kartverket.Metadatakatalog.Tests.Service
{
    public class ThemeResolverTest
    {
        //[Fact]
        //public void InspireCoordinateReferenceSystemsResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Coordinate reference systems", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireGeographicalNamesResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Geographical names", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireAdministrativeUnitsResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Administrative units", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireAddressesResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Addresses", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireCadastralParcelsResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Cadastral parcels", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireTransportNetworksResolveToSamferdsel()
        //{
        //    ResolveInspireKeyword("Transport networks", ThemeResolver.DokSamferdsel);
        //}

        //[Fact]
        //public void InspireHydrographyResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Hydrography", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireProtectedSitesResolveToNatur()
        //{
        //    ResolveInspireKeyword("Protected sites", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void InspireElevationResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Elevation", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireLandCoverResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Land cover", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireOrthoimageryResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Orthoimagery", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireGeologyResolveToGeologi()
        //{
        //    ResolveInspireKeyword("Geology", ThemeResolver.DokGeologi);
        //}

        //[Fact]
        //public void InspireStatisticalUnitsResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Statistical units", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireBuildingsResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Buildings", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireSoilResolveToLandbruk()
        //{
        //    ResolveInspireKeyword("Soil", ThemeResolver.DokLandbruk);
        //}

        //[Fact]
        //public void InspireLandUseResolveToPlan()
        //{
        //    ResolveInspireKeyword("Land use", ThemeResolver.DokPlan);
        //}

        //[Fact]
        //public void InspireHumanHealthResolveToSamfunnssikkerhet()
        //{
        //    ResolveInspireKeyword("Human health and safety", ThemeResolver.DokSamfunnssikkerhet);
        //}

        //[Fact]
        //public void InspireUtilityAndGovernmentalServicesResolveToSamfunnssikkerhet()
        //{
        //    ResolveInspireKeyword("Utility and governmental services", ThemeResolver.DokSamfunnssikkerhet);
        //}

        //[Fact]
        //public void InspireEnvironmentalMonitoringFacilitiesResolveToForurensning()
        //{
        //    ResolveInspireKeyword("Environmental monitoring facilities", ThemeResolver.DokForurensning);
        //}

        //[Fact]
        //public void InspireProductionAndIndustrialFacilitiesResolveToBasisGeodata()
        //{
        //    ResolveInspireKeyword("Production and industrial facilities", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void InspireAgriculturalAndAquacultureFacilitiesResolveToLandbruk()
        //{
        //    ResolveInspireKeyword("Agricultural and aquaculture facilities", ThemeResolver.DokLandbruk);
        //}

        //[Fact]
        //public void InspirePopulationDistributionResolveToAnnen()
        //{
        //    ResolveInspireKeyword("Population distribution and demography", ThemeResolver.DokAnnen);
        //}

        //[Fact]
        //public void InspireAreaManagementResolveToPlan()
        //{
        //    ResolveInspireKeyword("Area management / restriction / regulation zones & reporting units", ThemeResolver.DokPlan);
        //}

        //[Fact]
        //public void InspireNaturalRiskZonesResolveToNatur()
        //{
        //    ResolveInspireKeyword("Natural risk zones", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void InspireAtmosphericConditionsResolveToNatur()
        //{
        //    ResolveInspireKeyword("Atmospheric conditions", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void InspireMeteorologicalGeographicalFeaturesResolveToNatur()
        //{
        //    ResolveInspireKeyword("Meteorological geographical features", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void InspireOceanographicGeographicalFeaturesResolveToKystFiskeri()
        //{
        //    ResolveInspireKeyword("Oceanographic geographical features", ThemeResolver.DokKystFiskeri);
        //}

        //[Fact]
        //public void InspireSeaRegionsResolveToKystFiskeri()
        //{
        //    ResolveInspireKeyword("Sea regions", ThemeResolver.DokKystFiskeri);
        //}

        //[Fact]
        //public void InspireBioGeographicalRegionsResolveToLandskap()
        //{
        //    ResolveInspireKeyword("Bio-geographical regions", ThemeResolver.DokLandskap);
        //}

        //[Fact]
        //public void InspireHabitatsAndBiotopesResolveToNatur()
        //{
        //    ResolveInspireKeyword("Habitats and biotopes", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void InspireSpeciesDistributionResolveToNatur()
        //{
        //    ResolveInspireKeyword("Species distribution", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void InspireEnergyResourcesResolveToEnergi()
        //{
        //    ResolveInspireKeyword("Energy resources", ThemeResolver.DokEnergi);
        //}

        //[Fact]
        //public void InspireMineralResourcesResolveToNatur()
        //{
        //    ResolveInspireKeyword("Mineral resources", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void ShouldUseTopicCategoryWhenNoMappingExistsForInspireKeyword()
        //{
        //    SimpleMetadata metadata = createMetadataWithInspireKeyword("this inspire keyword does not exist");
        //    metadata.TopicCategory = "farming";
        //    ResolveTheme(metadata).Should().Be(ThemeResolver.DokLandbruk);
        //}

        //[Fact]
        //public void TopicCategoryFarmingResolveToLandbruk()
        //{
        //    ResolveTopicCategory("farming", ThemeResolver.DokLandbruk);
        //}

        //[Fact]
        //public void TopicCategoryBiotaResolveToNatur()
        //{
        //    ResolveTopicCategory("biota", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void TopicCategoryBoundariesResolveToBasisGeodata()
        //{
        //    ResolveTopicCategory("boundaries", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void TopicCategoryClimatologyMeteorologyAtmosphereResolveToNatur()
        //{
        //    ResolveTopicCategory("climatologyMeteorologyAtmosphere", ThemeResolver.DokNatur);
        //}

        //[Fact]
        //public void TopicCategoryEconomyResolveToAnnen()
        //{
        //    ResolveTopicCategory("economy", ThemeResolver.DokAnnen);
        //}

        //[Fact]
        //public void TopicCategoryElevationResolveToBasisGeodata()
        //{
        //    ResolveTopicCategory("elevation", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void TopicCategoryEnvironmentResolveToForurensning()
        //{
        //    ResolveTopicCategory("environment", ThemeResolver.DokForurensning);
        //}

        //[Fact]
        //public void TopicCategoryGeoscientificInformationResolveToGeologi()
        //{
        //    ResolveTopicCategory("geoscientificInformation", ThemeResolver.DokGeologi);
        //}

        //[Fact]
        //public void TopicCategoryHealthResolveToSamfunnssikkerhet()
        //{
        //    ResolveTopicCategory("health", ThemeResolver.DokSamfunnssikkerhet);
        //}

        //[Fact]
        //public void TopicCategoryImageryBaseMapsEarthCoverResolveToBasisGeodata()
        //{
        //    ResolveTopicCategory("imageryBaseMapsEarthCover", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void TopicCategoryintelligenceMilitaryResolveToAnnen()
        //{
        //    ResolveTopicCategory("intelligenceMilitary", ThemeResolver.DokAnnen);
        //}

        //[Fact]
        //public void TopicCategoryinlandWatersResolveToBasisGeodata()
        //{
        //    ResolveTopicCategory("inlandWaters", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void TopicCategoryLocationResolveToBasisGeodata()
        //{
        //    ResolveTopicCategory("location", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void TopicCategoryOceansResolveToKystFiskeri()
        //{
        //    ResolveTopicCategory("oceans", ThemeResolver.DokKystFiskeri);
        //}

        //[Fact]
        //public void TopicCategoryPlanningCadastreResolveToPlan()
        //{
        //    ResolveTopicCategory("planningCadastre", ThemeResolver.DokPlan);
        //}

        //[Fact]
        //public void TopicCategorySocietyResolveToKulturminner()
        //{
        //    ResolveTopicCategory("society", ThemeResolver.DokKulturminner);
        //}

        //[Fact]
        //public void TopicCategoryStructureResolveToBasisGeodata()
        //{
        //    ResolveTopicCategory("structure", ThemeResolver.DokBasisGeodata);
        //}

        //[Fact]
        //public void TopicCategoryTransportationResolveToSamferdsel()
        //{
        //    ResolveTopicCategory("transportation", ThemeResolver.DokSamferdsel);
        //}

        //[Fact]
        //public void TopicCategoryUtilitiesCommunicationResolveToEnergi()
        //{
        //    ResolveTopicCategory("utilitiesCommunication", ThemeResolver.DokEnergi);
        //}

        //[Fact]
        //public void ShouldResolveKeywordKulturToKulturminner()
        //{
        //    ResolveTheme(CreateMetadataWithKeyword("Kultur")).Should().Be(ThemeResolver.DokKulturminner);
        //    ResolveTheme(CreateMetadataWithKeyword("kultur")).Should().Be(ThemeResolver.DokKulturminner);
        //}

        //[Fact]
        //public void ShouldResolveKeywordKulturminneToKulturminner()
        //{
        //    ResolveTheme(CreateMetadataWithKeyword("Kulturminne")).Should().Be(ThemeResolver.DokKulturminner);
        //    ResolveTheme(CreateMetadataWithKeyword("kulturminne")).Should().Be(ThemeResolver.DokKulturminner);
        //}

        [Fact]
        public void ShouldResolveKeywordKulturminnerToKulturminner()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Kulturminner")).Should().Be(ThemeResolver.DokKulturminner);
        }

        [Fact]
        public void ShouldResolveToAnnenWhenNoKeywordOrTopicCategoryWithMappingExists()
        {
            ResolveTheme(SimpleMetadata.CreateDataset()).Should().Be(ThemeResolver.DokAnnen);
        }

        [Fact]
        public void ShouldResolveKeywordBasisGeodataRegardslessOfCase()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Basis geodata")).Should().Be(ThemeResolver.DokBasisGeodata);
        }

        [Fact]
        public void ShouldResolveKeywordSamferdsel()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Samferdsel")).Should().Be(ThemeResolver.DokSamferdsel);
        }

        [Fact]
        public void ShouldResolveKeywordSamfunnssikkerhet()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Samfunnssikkerhet")).Should().Be(ThemeResolver.DokSamfunnssikkerhet);
        }

        [Fact]
        public void ShouldResolveKeywordForurensning()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Forurensning")).Should().Be(ThemeResolver.DokForurensning);
        }

        [Fact]
        public void ShouldResolveKeywordFriluftsliv()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Friluftsliv")).Should().Be(ThemeResolver.DokFriluftsliv);
        }

        [Fact]
        public void ShouldResolveKeywordLandskap()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Landskap")).Should().Be(ThemeResolver.DokLandskap);
        }

        [Fact]
        public void ShouldResolveKeywordNatur()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Natur")).Should().Be(ThemeResolver.DokNatur);
        }

        [Fact]
        public void ShouldResolveKeywordKulturminner()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Kulturminner")).Should().Be(ThemeResolver.DokKulturminner);
        }

        [Fact]
        public void ShouldResolveKeywordLandbruk()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Landbruk")).Should().Be(ThemeResolver.DokLandbruk);
        }

        [Fact]
        public void ShouldResolveKeywordEnergi()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Energi")).Should().Be(ThemeResolver.DokEnergi);
        }

        [Fact]
        public void ShouldResolveKeywordGeologi()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Geologi")).Should().Be(ThemeResolver.DokGeologi);
        }

        //[Fact]
        //public void ShouldResolveKeywordKystFiskeri()
        //{
        //    ResolveTheme(CreateMetadataWithDokKeyword("Kyst / Fiskeri")).Should().Be(ThemeResolver.DokKystFiskeri);
        //    ResolveTheme(CreateMetadataWithDokKeyword("Kyst")).Should().Be(ThemeResolver.DokKystFiskeri);
        //    ResolveTheme(CreateMetadataWithDokKeyword("Fiskeri")).Should().Be(ThemeResolver.DokKystFiskeri);
        //}

        [Fact]
        public void ShouldResolveKeywordPlan()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Plan")).Should().Be(ThemeResolver.DokPlan);
        }

        private static SimpleMetadata CreateMetadataWithKeyword(string keyword, string thesaurus = null)
        {
            var metadata = SimpleMetadata.CreateDataset();
            var keywords = new List<SimpleKeyword>();
            keywords.Add(new SimpleKeyword
            {
                Keyword = keyword,
                Thesaurus = thesaurus
            });
            metadata.Keywords = keywords;
            return metadata;
        }

        private static SimpleMetadata CreateMetadataWithDokKeyword(string keyword)
        {
            return CreateMetadataWithKeyword(keyword, SimpleKeyword.THESAURUS_NATIONAL_THEME);
        }

        private static void ResolveTopicCategory(string topicCategory, string expectedTheme)
        {
            ResolveTheme(CreateMetadataWithTopicCategory(topicCategory)).Should().Be(expectedTheme);
        }

        private static SimpleMetadata CreateMetadataWithTopicCategory(string topicCategory)
        {
            var metadata = SimpleMetadata.CreateDataset();
            metadata.TopicCategory = topicCategory;
            return metadata;
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
            var metadata = SimpleMetadata.CreateDataset();
            var keywords = new List<SimpleKeyword>();
            keywords.Add(new SimpleKeyword
            {
                Thesaurus = SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1,
                Keyword = inspireKeyword
            });
            metadata.Keywords = keywords;

            return metadata;
        }
    }
}