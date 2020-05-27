using System.Collections.Generic;
using System.Linq;
using GeoNorgeAPI;
using Kartverket.Metadatakatalog.Models.Api;
using www.opengis.net;
using Kartverket.Metadatakatalog.Models.Translations;

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
        public const string DokFriluftsliv = "Friluftsliv";
        public const string DokAnnen = "Annen";
        public const string DokAnnenEnglish = "Other";
        public const string DokKystFiskeri = "Kyst / fiskeri";
        public const string DokLandskap = "Landskap";
        public const string DokKulturminner = "Kulturminner";

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
            {"human health and safety", DokSamfunnssikkerhet},
            {"utility and governmental services", DokSamfunnssikkerhet},
            {"environmental monitoring facilities", DokForurensning},
            {"production and industrial facilities", DokBasisGeodata},
            {"agricultural and aquaculture facilities", DokLandbruk},
            {"population distribution and demography", DokAnnen},
            {"area management / restriction / regulation zones & reporting units", DokPlan},
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

        private readonly Dictionary<string, string> _topicCategoryToTheme = new Dictionary<string, string>
        {
            {"farming", DokLandbruk},
            {"biota", DokNatur},
            {"boundaries", DokBasisGeodata},
            {"climatologyMeteorologyAtmosphere", DokNatur},
            {"economy", DokAnnen},
            {"elevation", DokBasisGeodata},
            {"environment", DokForurensning},
            {"geoscientificInformation", DokGeologi},
            {"health", DokSamfunnssikkerhet},
            {"imageryBaseMapsEarthCover", DokBasisGeodata},
            {"intelligenceMilitary", DokAnnen},
            {"inlandWaters", DokBasisGeodata},
            {"location", DokBasisGeodata},
            {"oceans", DokKystFiskeri},
            {"planningCadastre", DokPlan},
            {"society", DokKulturminner},
            {"structure", DokBasisGeodata},
            {"transportation", DokSamferdsel},
            {"utilitiesCommunication", DokEnergi},
        };

        private readonly Dictionary<string, string> _dokCategoryToTheme = new Dictionary<string, string>
        {
            {"basis geodata", DokBasisGeodata},
            {"samferdsel", DokSamferdsel},
            {"samfunnssikkerhet", DokSamfunnssikkerhet},
            {"forurensning", DokForurensning},
            {"friluftsliv", DokFriluftsliv},
            {"landskap", DokLandskap},
            {"natur", DokNatur},
            {"kulturminner", DokKulturminner},
            {"landbruk", DokLandbruk},
            {"energi", DokEnergi},
            {"geologi", DokGeologi},
            {"kyst", DokKystFiskeri},
            {"fiskeri", DokKystFiskeri},
            {"kyst / fiskeri", DokKystFiskeri},
            {"plan", DokPlan},
        };

        public string Resolve(SimpleMetadata metadata, string culture = Culture.NorwegianCode)
        {
            string theme = ResolveThemeFromDokKeywords(metadata, culture);
            if (string.IsNullOrWhiteSpace(theme))
            {
                if (culture == Culture.NorwegianCode)
                    theme = DokAnnen;
                else
                    theme = DokAnnenEnglish;

            }
            //if (string.IsNullOrWhiteSpace(theme))
            //{
            //    theme = ResolveThemeFromInspireKeywords(metadata);
            //    if (string.IsNullOrWhiteSpace(theme))
            //    {
            //        theme = ResolveThemeFromTopicCategory(metadata.TopicCategory);
            //        if (string.IsNullOrWhiteSpace(theme))
            //        {
            //            theme = ResolveCultureKeywords(metadata);
            //            if (string.IsNullOrWhiteSpace(theme))
            //            {
            //                theme = DokAnnen;
            //            }
            //        }
            //    }
            //}

            return theme;
        }

        public string ResolveAccess(string AccessConstraint, string OtherConstraintsAccess, string culture)
        {
            string dataaccess = null;
            if (culture == Culture.EnglishCode)
            {
                if (AccessConstraint == "restricted")
                    dataaccess = "Restricted data";
                else if (AccessConstraint == "norway digital restricted")
                    dataaccess = "Norway digital restricted";
                else if (AccessConstraint == "no restrictions")
                    dataaccess = "Open data";
                else if (AccessConstraint == "otherRestrictions")
                    if (OtherConstraintsAccess == "norway digital restricted")
                        dataaccess = "Norway digital restricted";
                    else if (OtherConstraintsAccess == "no restrictions")
                        dataaccess = "Open data";
            }
            else
            {
                if (AccessConstraint == "restricted")
                    dataaccess = "Skjermede data";
                else if (AccessConstraint == "norway digital restricted")
                    dataaccess = "Norge digitalt-begrenset";
                else if (AccessConstraint == "no restrictions")
                    dataaccess = "Åpne data";
                else if (AccessConstraint == "otherRestrictions")
                    if (OtherConstraintsAccess == "norway digital restricted")
                        dataaccess = "Norge digitalt-begrenset";
                    else if (OtherConstraintsAccess == "no restrictions")
                        dataaccess = "Åpne data";
            }
            return dataaccess;
        }

        private string ResolveThemeFromDokKeywords(SimpleMetadata metadata, string culture)
        {
            List<SimpleKeyword> keywordsDok = SimpleKeyword.Filter(metadata.Keywords, null, SimpleKeyword.THESAURUS_NATIONAL_THEME);
            foreach (var keyword in keywordsDok)
            {
                //string lowerCaseKeyword = keyword.Keyword.ToLower();
                //string theme;
                //if (_dokCategoryToTheme.TryGetValue(lowerCaseKeyword, out theme))
                //{
                //return theme;
                //}
                if (culture == Culture.EnglishCode && !string.IsNullOrEmpty(keyword.EnglishKeyword))
                    return keyword.EnglishKeyword;
                else
                return keyword.Keyword;
            }
            return null;
        }

        private string ResolveCultureKeywords(SimpleMetadata metadata)
        {
            foreach (var keyword in metadata.Keywords)
            {
                string lowerCaseKeyword = keyword.Keyword.ToLower();
                if (lowerCaseKeyword == "kultur" || lowerCaseKeyword == "kulturminne" || lowerCaseKeyword == "kulturminner")
                {
                    return DokKulturminner;
                }
            }
            return null;
        }

        private string ResolveThemeFromTopicCategory(string topicCategory)
        {
            if (!string.IsNullOrWhiteSpace(topicCategory))
            {
                string theme;
                if (_topicCategoryToTheme.TryGetValue(topicCategory, out theme))
                {
                    return theme;
                }    
            }
            return null;
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