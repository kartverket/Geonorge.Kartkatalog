using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Kartverket.Metadatakatalog.Service
{
    public class RegisterFetcher
    {
        MemoryCacher memCacher = new MemoryCacher();
        private static readonly HttpClient _httpClient = new HttpClient();

        Dictionary<string, string> TopicCategories = new Dictionary<string, string>();
        Dictionary<string, string> TopicCategoriesEnglish = new Dictionary<string, string>();
        Dictionary<string, string> SpatialRepresentations = new Dictionary<string, string>();
        Dictionary<string, string> SpatialRepresentationsEnglish = new Dictionary<string, string>();
        Dictionary<string, string> MaintenanceFrequencyValues = new Dictionary<string, string>();
        Dictionary<string, string> MaintenanceFrequencyValuesEnglish = new Dictionary<string, string>();
        Dictionary<string, string> ListOfStatusValues = new Dictionary<string, string>();
        Dictionary<string, string> ListOfStatusValuesEnglish = new Dictionary<string, string>();
        Dictionary<string, string> ListOfClassificationValues = new Dictionary<string, string>();
        Dictionary<string, string> ListOfClassificationValuesEnglish = new Dictionary<string, string>();
        Dictionary<string, string> ListOfRestrictionValues = new Dictionary<string, string>();
        Dictionary<string, string> ListOfRestrictionValuesEnglish = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfRestrictionInspireValues = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfRestrictionInspireValuesEnglish = new Dictionary<string, string>();
        Dictionary<string, string> ListOfCoordinatesystemNameValues = new Dictionary<string, string>();
        Dictionary<string, string> ListOfCoordinatesystemNameValuesEnglish = new Dictionary<string, string>();
        Dictionary<string, string> ListOfDistributionTypes = new Dictionary<string, string>();
        Dictionary<string, string> ListOfDistributionTypesEnglish = new Dictionary<string, string>();
        public Dictionary<string, string> OrganizationShortNames = new Dictionary<string, string>();
        Dictionary<string, string> ListOfOrderingInstructions = new Dictionary<string, string>();
        Dictionary<string, string> ListOfOrderingInstructionsEnglish = new Dictionary<string, string>();

        Dictionary<string, string> DownloadUseGroups = new Dictionary<string, string>();
        Dictionary<string, string> DownloadUseGroupsEnglish = new Dictionary<string, string>();
        Dictionary<string, string> DownloadPurposes = new Dictionary<string, string>();
        Dictionary<string, string> DownloadPurposesEnglish = new Dictionary<string, string>();

        Dictionary<string, string> ListOfInspire = new Dictionary<string, string>();
        

        public RegisterFetcher()
        {
            TopicCategories = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB100");
            TopicCategoriesEnglish = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB100", Culture.EnglishCode);
            SpatialRepresentations = GetCodeList("4C54EB31-714E-4457-AF6A-44FE6DBE76C1");
            SpatialRepresentationsEnglish = GetCodeList("4C54EB31-714E-4457-AF6A-44FE6DBE76C1", Culture.EnglishCode);
            MaintenanceFrequencyValues = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB124");
            MaintenanceFrequencyValuesEnglish = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB124", Culture.EnglishCode);
            ListOfStatusValues = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB137");
            ListOfStatusValuesEnglish = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB137", Culture.EnglishCode);
            ListOfClassificationValues = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB145");
            ListOfClassificationValuesEnglish = GetCodeList("9A46038D-16EE-4562-96D2-8F6304AAB145", Culture.EnglishCode);
            ListOfRestrictionValues = GetCodeList("D23E9F2F-66AB-427D-8AE4-5B6FD3556B57");
            ListOfRestrictionValuesEnglish = GetCodeList("D23E9F2F-66AB-427D-8AE4-5B6FD3556B57", Culture.EnglishCode);
            ListOfRestrictionInspireValues = GetCodeListByName("inspire-tilgangsrestriksjoner");
            ListOfRestrictionInspireValuesEnglish = GetCodeListByName("inspire-tilgangsrestriksjoner", Culture.EnglishCode);
            ListOfCoordinatesystemNameValues = GetEPSGCodeList("37B9DC41-D868-4CBC-84F9-39557041FB2C");
            ListOfCoordinatesystemNameValuesEnglish = GetEPSGCodeList("37B9DC41-D868-4CBC-84F9-39557041FB2C", Culture.EnglishCode);
            ListOfDistributionTypes = GetCodeList("94B5A165-7176-4F43-B6EC-1063F7ADE9EA");
            ListOfDistributionTypesEnglish = GetCodeList("94B5A165-7176-4F43-B6EC-1063F7ADE9EA", Culture.EnglishCode );
            OrganizationShortNames = GetListOfOrganizations();
            ListOfOrderingInstructions = GetSubRegister("metadata-kodelister/kartverket/norge-digitalt-tjenesteerklaering");
            ListOfOrderingInstructionsEnglish = GetSubRegister("metadata-kodelister/kartverket/norge-digitalt-tjenesteerklaering", Culture.EnglishCode);

            DownloadUseGroups = GetCodeListByName("brukergrupper");
            DownloadUseGroupsEnglish = GetCodeListByName("brukergrupper", Culture.EnglishCode);
            DownloadPurposes = GetCodeListByName("formal");
            DownloadPurposesEnglish = GetCodeListByName("formal", Culture.EnglishCode);

            ListOfInspire = GetCodeList("E7E48BC6-47C6-4E37-BE12-08FB9B2FEDE6");

        }

        public Dictionary<string, string> GetDownloadUseGroups()
        {
            var culture = CultureHelper.GetCurrentCulture();
            if (culture == Culture.NorwegianCode)
                return DownloadUseGroups;
            else
                return DownloadUseGroupsEnglish;
        }

        public Dictionary<string, string> GetDownloadPurposes()
        {
            var culture = CultureHelper.GetCurrentCulture();
            if (culture == Culture.NorwegianCode)
                return DownloadPurposes;
            else
                return DownloadPurposesEnglish;
        }

        public Dictionary<string, string> GetListOfOrganizations()
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("organizations");

            Dictionary<string, string> Organizations = new Dictionary<string, string>();

            if (cache != null)
            {
                Organizations = cache as Dictionary<string, string>;
            }

            if (Organizations.Count < 1)
            {
                var url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/organisasjoner";
                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", Culture.NorwegianCode);

                var responseResult = _httpClient.GetAsync(url).Result;
                HttpContent content = responseResult.Content;
                var data = content.ReadAsStringAsync().Result;

                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var orgs = response["containeditems"];

                foreach (var org in orgs)
                {
                    var name = org["label"].ToString();
                    var shortName = name;
                    if (org["ShortName"] != null)
                        shortName = org["ShortName"].ToString();

                    if (!Organizations.ContainsKey(name))
                    {
                        Organizations.Add(name, shortName);
                    }
                }

                Organizations = Organizations.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
                memCacher.Add("organizations", Organizations, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return Organizations;
        }


        public string GetCoordinatesystemName(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? ListOfCoordinatesystemNameValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfCoordinatesystemNameValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }
        public string GetDistributionType(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? ListOfDistributionTypes.Where(p => p.Key == value).FirstOrDefault() 
                : ListOfDistributionTypesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public string GetSpatialRepresentation(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? SpatialRepresentations.Where(p => p.Key == value).FirstOrDefault()
                : SpatialRepresentationsEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public string GetTopicCategory(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? TopicCategories.Where(p => p.Key == value).FirstOrDefault()
                : TopicCategoriesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public string GetMaintenanceFrequency(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? MaintenanceFrequencyValues.Where(p => p.Key == value).FirstOrDefault()
                : MaintenanceFrequencyValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public string GetStatus(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? ListOfStatusValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfStatusValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public string GetInspire(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = 
                 ListOfInspire.Where(p => p.Key == value).FirstOrDefault();
                
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public string GetClassification(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? ListOfClassificationValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfClassificationValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public string GetRestriction(string value, string OtherConstraintsAccess ="")
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? ListOfRestrictionValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfRestrictionValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;
            if (culture == Culture.NorwegianCode)
            {
                if (value == "restricted")
                    value = ListOfRestrictionInspireValues["https://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1b"]; // "Skjermede data"
                if (value == "no restrictions" || OtherConstraintsAccess == "no restrictions")
                    value = ListOfRestrictionInspireValues["https://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/noLimitations"];  //"Åpne data"
                else if (value == "norway digital restricted" || OtherConstraintsAccess == "norway digital restricted")
                    value = ListOfRestrictionInspireValues["https://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d"]; //"Norge digitalt-begrenset"
            }
            else
            {
                if (value == "restricted")
                    value = ListOfRestrictionInspireValuesEnglish["https://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1b"]; // "Restricted data";
                if (value == "no restrictions" || OtherConstraintsAccess == "no restrictions")
                    value = ListOfRestrictionInspireValuesEnglish["https://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/noLimitations"]; // "Open data"
                else if (value == "norway digital restricted" || OtherConstraintsAccess == "norway digital restricted")
                    value = ListOfRestrictionInspireValuesEnglish["https://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d"];  //"Norway digitalt restricted"
            }

            return value;
        }

        public string GetServiceDeclaration(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, string> dic = culture == Culture.NorwegianCode
                ? ListOfOrderingInstructions.Where(p => p.Key == value).FirstOrDefault()
                : ListOfOrderingInstructionsEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, String>)))
                value = dic.Value;

            return value;
        }

        public Dictionary<string, string> GetCodeList(string systemid, string culture = Culture.NorwegianCode)
        {
            var cacheId = systemid + "_" + culture;
            var cache = memCacher.GetValue(cacheId);

            Dictionary<string, string> CodeValues = new Dictionary<string, string>();

            if (cache != null)
            {
                CodeValues = cache as Dictionary<string, string>;
            }
            else
            {
                
                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;

                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var responseResult = _httpClient.GetAsync(url).Result;
                HttpContent content = responseResult.Content;
                var data = content.ReadAsStringAsync().Result;

                var response = Newtonsoft.Json.Linq.JObject.Parse(data);
                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    if (string.IsNullOrWhiteSpace(codevalue))
                        codevalue = code["label"].ToString();

                    if (!CodeValues.ContainsKey(codevalue))
                    {
                        CodeValues.Add(codevalue, code["label"].ToString());
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

        public Dictionary<string, string> GetCodeListByName(string name, string culture = Culture.NorwegianCode)
        {
            var cacheId = name + "_" + culture;
            var cache = memCacher.GetValue(cacheId);

            Dictionary<string, string> CodeValues = new Dictionary<string, string>();

            if (cache != null)
            {
                CodeValues = cache as Dictionary<string, string>;
            }
            else
            {

                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/metadata-kodelister/" + name;

                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var data = _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

                var response = Newtonsoft.Json.Linq.JObject.Parse(data);
                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["codevalue"].ToString();
                    if (string.IsNullOrWhiteSpace(codevalue))
                        codevalue = code["label"].ToString();

                    if (!CodeValues.ContainsKey(codevalue))
                    {
                        CodeValues.Add(codevalue, code["label"].ToString());
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

        public Dictionary<string, string> GetSubRegister(string registername, string culture = Culture.NorwegianCode)
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("subregisteritem-" + registername);

            Dictionary<string, string> RegisterItems = new Dictionary<string, string>();

            if (cache != null)
            {
                RegisterItems = cache as Dictionary<string, string>;
            }

            if (RegisterItems.Count < 1)
            {
                var url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/" + registername;

                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var responseResult = _httpClient.GetAsync(url).Result;
                HttpContent content = responseResult.Content;
                var data = content.ReadAsStringAsync().Result;

                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var items = response["containeditems"];

                foreach (var item in items)
                {
                    var id = item["id"].ToString();
                    var owner = item["owner"].ToString();
                    string organization = item["owner"].ToString();

                    if (!RegisterItems.ContainsKey(id))
                    {
                        RegisterItems.Add(id, item["label"].ToString());
                    }
                }

                RegisterItems = RegisterItems.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
                memCacher.Add("subregisteritem-" + registername, RegisterItems, new DateTimeOffset(DateTime.Now.AddYears(1)));
            }

            return RegisterItems;
        }

        public Dictionary<string, string> GetEPSGCodeList(string systemid, string culture = Culture.NorwegianCode)
        {
            var cacheId = systemid + "_" + culture;

            var cache = memCacher.GetValue(cacheId);

            Dictionary<string, string> CodeValues = new Dictionary<string, string>();

            if (cache != null)
            {
                CodeValues = cache as Dictionary<string, string>;
            }
            else
            {

                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;
                _httpClient.DefaultRequestHeaders.Remove("Accept-Language");
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var responseResult = _httpClient.GetAsync(url).Result;
                HttpContent content = responseResult.Content;
                var data = content.ReadAsStringAsync().Result;
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var codeList = response["containeditems"];

                foreach (var code in codeList)
                {
                    var codevalue = code["documentreference"].ToString();
                    if (string.IsNullOrWhiteSpace(codevalue))
                        codevalue = code["label"].ToString();

                    if (!CodeValues.ContainsKey(codevalue))
                    {
                        CodeValues.Add(codevalue, code["label"].ToString());
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

    }
}