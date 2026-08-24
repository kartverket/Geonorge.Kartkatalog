using FluentAssertions;
using Kartverket.Metadatakatalog.Models;
using Kartverket.Metadatakatalog.Service;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Kartverket.Metadatakatalog.Tests.Models
{
    public class SurveyAreaMapLinkTest
    {
        private const string NorgeskartUrl = "https://norgeskart-preview-geonorge.atkv3-dev.kartverket.cloud/";

        public SurveyAreaMapLinkTest()
        {
            // Constructing SimpleMetadataUtil registers it as the static instance used by MetadataViewModel
            new SimpleMetadataUtil(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Norgeskart"] = NorgeskartUrl })
                .Build());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldBuildLinkWithCoverageAndSurveyAreaLayers()
        {
            var metadata = new MetadataViewModel
            {
                CoverageUrl = "TYPE:GEONORGE-WMS@PATH:https://wms.geonorge.no/skwms1/wms.geonorge_dekningskart?@LAYER:nve_flomsoner",
                CoverageGridUrl = "TYPE:GEONORGE-WMS@PATH:https://wms.geonorge.no/skwms1/wms.geonorge_dekningskart?@LAYER:nve_flomsoner",
                SurveyAreaMapUrlWms = "@PATH:https://kart.nve.no/enterprise/services/Flomsoner2/MapServer/WMSServer?request=GetCapabilities&service=WMS@LAYER:flomsoner",
                SurveyAreaMapUrl = "https://testnedlasting.geonorge.no/geonorge/Basisdata/DOKFullstendighetsdekningskart/Kartkatalogen/dekning_flomsoner.geojson"
            };

            metadata.GetSurveyAreaMapLink().Should().Be(
                NorgeskartUrl
                + "?zoom=4&lat=7194396.01&lon=511194.69000000006&backgroundLayer=topograatone"
                + "&wmsUrl=https%3A%2F%2Fwms.geonorge.no%2Fskwms1%2Fwms.geonorge_dekningskart%3Fdatasett%3Dnve_flomsoner"
                + "&wmsUrl=https%3A%2F%2Fwms.geonorge.no%2Fskwms1%2Fwms.gp_dek_oversikt%3Fdatasett%3Dnve_flomsoner"
                + "&wmsUrl=https%3A%2F%2Fkart.nve.no%2Fenterprise%2Fservices%2FFlomsoner2%2FMapServer%2FWMSServer%3Frequest%3DGetCapabilities%26service%3DWMS"
                + "&geojsonUrl=https%3A%2F%2Ftestnedlasting.geonorge.no%2Fgeonorge%2FBasisdata%2FDOKFullstendighetsdekningskart%2FKartkatalogen%2Fdekning_flomsoner.geojson"
                + "&rotation=0&showMenu=false");
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ShouldReturnNullWhenNoCoverage()
        {
            new MetadataViewModel().GetSurveyAreaMapLink().Should().BeNull();
        }
    }
}
