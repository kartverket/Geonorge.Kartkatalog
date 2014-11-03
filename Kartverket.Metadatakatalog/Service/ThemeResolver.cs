using System.Collections.Generic;
using System.Linq;
using GeoNorgeAPI;

namespace Kartverket.Metadatakatalog.Service
{
    public class ThemeResolver
    {
        public const string DokBasisGeodata = "Basis geodata";
        public const string DokSamferdsel = "Samferdsel";
        public const string DokNatur = "Natur";
        public const string DokGeologi = "Geologi";
        public const string DokLandbruk = "Landbruk";
        public const string DokPlan = "Plan";
        public const string DokEnergi = "Energi";
        public const string DokSamfunnssikkerhet = "Samfunnssikkerhet";
        public const string DokForurensning = "Forurensning";
        public const string DokAnnen = "Annen";
        public const string DokKystFiskeri = "Kyst / fiskeri";
        public const string DokLandskap = "Landskap";

        // note use of lowercase keywords - comparison is also done with lower casing of input
        private readonly Dictionary<string, string> _inspireKeywordToTheme = new Dictionary<string, string>
        {
            {"coordinate reference systems", DokBasisGeodata},
            {"geographical grid systems", DokBasisGeodata},
            {"geographical names", DokBasisGeodata},
            {"administrative units", DokBasisGeodata},
            {"addresses", DokBasisGeodata},
            {"cadastral parcels", DokBasisGeodata},
            {"transport networks", DokSamferdsel},
            {"hydrography", DokBasisGeodata},
            {"protected sites", DokNatur},
            {"elevation", DokBasisGeodata},
            {"land cover", DokBasisGeodata},
            {"orthoimagery", DokBasisGeodata},
            {"geology", DokGeologi},
            {"statistical units", DokBasisGeodata},
            {"buildings", DokBasisGeodata},
            {"soil", DokLandbruk},
            {"land use", DokPlan},
            {"human health and safety", DokEnergi},
            {"utility and governmental services", DokSamfunnssikkerhet},
            {"environmental monitoring facilities", DokForurensning},
            {"production and industrial facilities", DokBasisGeodata},
            {"agricultural and aquaculture facilities", DokLandbruk},
            {"population distribution and demography", DokAnnen},
            {"area management / restriction / regulation zones & reporting units", DokAnnen},
            {"natural risk zones", DokNatur},
            {"atmospheric conditions", DokNatur},
            {"meteorological geographical features", DokNatur},
            {"oceanographic geographical features", DokKystFiskeri},
            {"sea regions", DokKystFiskeri},
            {"bio-geographical regions", DokLandskap},
            {"habitats and biotopes", DokNatur},
            {"species distribution", DokNatur},
            {"energy resources", DokEnergi},
            {"mineral resources", DokNatur},
        };

        public string Resolve(SimpleMetadata metadata)
        {
            return ResolveThemeFromInspireKeywords(metadata);
        }

        private string ResolveThemeFromInspireKeywords(SimpleMetadata metadata)
        {
            var inspireKeywords = SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_GEMET_INSPIRE_V1);
            if (inspireKeywords != null && inspireKeywords.Any())
            {
                foreach (var simpleKeyword in inspireKeywords)
                {
                    string lowerCaseKeyword = simpleKeyword.Keyword.ToLower();
                    string theme;
                    if (_inspireKeywordToTheme.TryGetValue(lowerCaseKeyword, out theme))
                    {
                        if (!string.IsNullOrWhiteSpace(theme))
                            return theme;
                    }
                }
            }
            return null;
        }
    }
}