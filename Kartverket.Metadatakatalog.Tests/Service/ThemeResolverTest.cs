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
        public void InspirePopulationDistributionResolveToAnnen()
        {
            ResolveInspireKeyword("Population distribution and demography", ThemeResolver.DokAnnen);
        }

        [Test]
        public void InspireAreaManagementResolveToAnnen()
        {
            ResolveInspireKeyword("Area management / restriction / regulation zones & reporting units", ThemeResolver.DokAnnen);
        }

        [Test]
        public void InspireNaturalRiskZonesResolveToNatur()
        {
            ResolveInspireKeyword("Natural risk zones", ThemeResolver.DokNatur);
        }

        [Test]
        public void InspireAtmosphericConditionsResolveToNatur()
        {
            ResolveInspireKeyword("Atmospheric conditions", ThemeResolver.DokNatur);
        }

        [Test]
        public void InspireMeteorologicalGeographicalFeaturesResolveToNatur()
        {
            ResolveInspireKeyword("Meteorological geographical features", ThemeResolver.DokNatur);
        }

        [Test]
        public void InspireOceanographicGeographicalFeaturesResolveToKystFiskeri()
        {
            ResolveInspireKeyword("Oceanographic geographical features", ThemeResolver.DokKystFiskeri);
        }

        [Test]
        public void InspireSeaRegionsResolveToKystFiskeri()
        {
            ResolveInspireKeyword("Sea regions", ThemeResolver.DokKystFiskeri);
        }

        [Test]
        public void InspireBioGeographicalRegionsResolveToLandskap()
        {
            ResolveInspireKeyword("Bio-geographical regions", ThemeResolver.DokLandskap);
        }

        [Test]
        public void InspireHabitatsAndBiotopesResolveToNatur()
        {
            ResolveInspireKeyword("Habitats and biotopes", ThemeResolver.DokNatur);
        }

        [Test]
        public void InspireSpeciesDistributionResolveToNatur()
        {
            ResolveInspireKeyword("Species distribution", ThemeResolver.DokNatur);
        }

        [Test]
        public void InspireEnergyResourcesResolveToEnergi()
        {
            ResolveInspireKeyword("Energy resources", ThemeResolver.DokEnergi);
        }

        [Test]
        public void InspireMineralResourcesResolveToNatur()
        {
            ResolveInspireKeyword("Mineral resources", ThemeResolver.DokNatur);
        }

        [Test]
        public void ShouldUseTopicCategoryWhenNoMappingExistsForInspireKeyword()
        {
            SimpleMetadata metadata = createMetadataWithInspireKeyword("this inspire keyword does not exist");
            metadata.TopicCategory = "farming";
            ResolveTheme(metadata).Should().Be(ThemeResolver.DokLandbruk);
        }

        [Test]
        public void TopicCategoryFarmingResolveToLandbruk()
        {
            ResolveTopicCategory("farming", ThemeResolver.DokLandbruk);
        }

        [Test]
        public void TopicCategoryBiotaResolveToNatur()
        {
            ResolveTopicCategory("biota", ThemeResolver.DokNatur);
        }

        [Test]
        public void TopicCategoryBoundariesResolveToBasisGeodata()
        {
            ResolveTopicCategory("boundaries", ThemeResolver.DokBasisGeodata);
        }
        
        [Test]
        public void TopicCategoryClimatologyMeteorologyAtmosphereResolveToNatur()
        {
            ResolveTopicCategory("climatologyMeteorologyAtmosphere", ThemeResolver.DokNatur);
        }

        [Test]
        public void TopicCategoryEconomyResolveToAnnen()
        {
            ResolveTopicCategory("economy", ThemeResolver.DokAnnen);
        }

        [Test]
        public void TopicCategoryElevationResolveToBasisGeodata()
        {
            ResolveTopicCategory("elevation", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void TopicCategoryEnvironmentResolveToForurensning()
        {
            ResolveTopicCategory("environment", ThemeResolver.DokForurensning);
        }

        [Test]
        public void TopicCategoryGeoscientificInformationResolveToGeologi()
        {
            ResolveTopicCategory("geoscientificInformation", ThemeResolver.DokGeologi);
        }

        [Test]
        public void TopicCategoryHealthResolveToSamfunnssikkerhet()
        {
            ResolveTopicCategory("health", ThemeResolver.DokSamfunnssikkerhet);
        }

        [Test]
        public void TopicCategoryImageryBaseMapsEarthCoverResolveToBasisGeodata()
        {
            ResolveTopicCategory("imageryBaseMapsEarthCover", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void TopicCategoryintelligenceMilitaryResolveToAnnen()
        {
            ResolveTopicCategory("intelligenceMilitary", ThemeResolver.DokAnnen);
        }

        [Test]
        public void TopicCategoryinlandWatersResolveToBasisGeodata()
        {
            ResolveTopicCategory("inlandWaters", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void TopicCategoryLocationResolveToBasisGeodata()
        {
            ResolveTopicCategory("location", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void TopicCategoryOceansResolveToKystFiskeri()
        {
            ResolveTopicCategory("oceans", ThemeResolver.DokKystFiskeri);
        }

        [Test]
        public void TopicCategoryPlanningCadastreResolveToPlan()
        {
            ResolveTopicCategory("planningCadastre", ThemeResolver.DokPlan);
        }

        [Test]
        public void TopicCategorySocietyResolveToKulturminner()
        {
            ResolveTopicCategory("society", ThemeResolver.DokKulturminner);
        }

        [Test]
        public void TopicCategoryStructureResolveToBasisGeodata()
        {
            ResolveTopicCategory("structure", ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void TopicCategoryTransportationResolveToSamferdsel()
        {
            ResolveTopicCategory("transportation", ThemeResolver.DokSamferdsel);
        }

        [Test]
        public void TopicCategoryUtilitiesCommunicationResolveToEnergi()
        {
            ResolveTopicCategory("utilitiesCommunication", ThemeResolver.DokEnergi);
        }

        [Test]
        public void ShouldResolveKeywordKulturToKulturminner()
        {
            ResolveTheme(CreateMetadataWithKeyword("Kultur")).Should().Be(ThemeResolver.DokKulturminner);
            ResolveTheme(CreateMetadataWithKeyword("kultur")).Should().Be(ThemeResolver.DokKulturminner);
        }

        [Test]
        public void ShouldResolveKeywordKulturminneToKulturminner()
        {
            ResolveTheme(CreateMetadataWithKeyword("Kulturminne")).Should().Be(ThemeResolver.DokKulturminner);
            ResolveTheme(CreateMetadataWithKeyword("kulturminne")).Should().Be(ThemeResolver.DokKulturminner);
        }

        [Test]
        public void ShouldResolveKeywordKulturminnerToKulturminner()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Kulturminner")).Should().Be(ThemeResolver.DokKulturminner);
            ResolveTheme(CreateMetadataWithDokKeyword("kulturminner")).Should().Be(ThemeResolver.DokKulturminner);
        }

        [Test]
        public void ShouldResolveToAnnenWhenNoKeywordOrTopicCategoryWithMappingExists()
        {
            ResolveTheme(SimpleMetadata.CreateDataset()).Should().Be(ThemeResolver.DokAnnen);
        }

        [Test]
        public void ShouldResolveKeywordBasisGeodataRegardslessOfCase()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Basis geodata")).Should().Be(ThemeResolver.DokBasisGeodata);
            ResolveTheme(CreateMetadataWithDokKeyword("basis Geodata")).Should().Be(ThemeResolver.DokBasisGeodata);
        }

        [Test]
        public void ShouldResolveKeywordSamferdsel()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Samferdsel")).Should().Be(ThemeResolver.DokSamferdsel);
        }

        [Test]
        public void ShouldResolveKeywordSamfunnssikkerhet()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Samfunnssikkerhet")).Should().Be(ThemeResolver.DokSamfunnssikkerhet);
        }

        [Test]
        public void ShouldResolveKeywordForurensning()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Forurensning")).Should().Be(ThemeResolver.DokForurensning);
        }

        [Test]
        public void ShouldResolveKeywordFriluftsliv()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Friluftsliv")).Should().Be(ThemeResolver.DokFriluftsliv);
        }

        [Test]
        public void ShouldResolveKeywordLandskap()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Landskap")).Should().Be(ThemeResolver.DokLandskap);
        }

        [Test]
        public void ShouldResolveKeywordNatur()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Natur")).Should().Be(ThemeResolver.DokNatur);
        }

        [Test]
        public void ShouldResolveKeywordKulturminner()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Kulturminner")).Should().Be(ThemeResolver.DokKulturminner);
        }

        [Test]
        public void ShouldResolveKeywordLandbruk()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Landbruk")).Should().Be(ThemeResolver.DokLandbruk);
        }

        [Test]
        public void ShouldResolveKeywordEnergi()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Energi")).Should().Be(ThemeResolver.DokEnergi);
        }

        [Test]
        public void ShouldResolveKeywordGeologi()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Geologi")).Should().Be(ThemeResolver.DokGeologi);
        }

        [Test]
        public void ShouldResolveKeywordKystFiskeri()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Kyst / Fiskeri")).Should().Be(ThemeResolver.DokKystFiskeri);
            ResolveTheme(CreateMetadataWithDokKeyword("Kyst")).Should().Be(ThemeResolver.DokKystFiskeri);
            ResolveTheme(CreateMetadataWithDokKeyword("Fiskeri")).Should().Be(ThemeResolver.DokKystFiskeri);
        }

        [Test]
        public void ShouldResolveKeywordPlan()
        {
            ResolveTheme(CreateMetadataWithDokKeyword("Plan")).Should().Be(ThemeResolver.DokPlan);
        }

        private static SimpleMetadata CreateMetadataWithKeyword(string keyword, string thesaurus = null)
        {
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
            var keywords = new List<SimpleKeyword>();
            keywords.Add(new SimpleKeyword()
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
            SimpleMetadata metadata = SimpleMetadata.CreateDataset();
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
