using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using System;
using System.Collections.Concurrent;
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

        IDictionary<string, string> TopicCategories = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> TopicCategoriesEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> SpatialRepresentations = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> SpatialRepresentationsEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> MaintenanceFrequencyValues = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> MaintenanceFrequencyValuesEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfStatusValues = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfStatusValuesEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfClassificationValues = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfClassificationValuesEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfRestrictionValues = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfRestrictionValuesEnglish = new ConcurrentDictionary<string, string>();
        public IDictionary<string, string> ListOfRestrictionInspireValues = new ConcurrentDictionary<string, string>();
        public IDictionary<string, string> ListOfRestrictionInspireValuesEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfCoordinatesystemNameValues = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfCoordinatesystemNameValuesEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfDistributionTypes = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfDistributionTypesEnglish = new ConcurrentDictionary<string, string>();
        public IDictionary<string, string> OrganizationShortNames = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfOrderingInstructions = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> ListOfOrderingInstructionsEnglish = new ConcurrentDictionary<string, string>();

        IDictionary<string, string> DownloadUseGroups = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> DownloadUseGroupsEnglish = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> DownloadPurposes = new ConcurrentDictionary<string, string>();
        IDictionary<string, string> DownloadPurposesEnglish = new ConcurrentDictionary<string, string>();

        IDictionary<string, string> ListOfInspire = new ConcurrentDictionary<string, string>();
        

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

        public IDictionary<string, string> GetDownloadUseGroups()
        {
            var culture = CultureHelper.GetCurrentCulture();
            if (culture == Culture.NorwegianCode)
                return DownloadUseGroups;
            else
                return DownloadUseGroupsEnglish;
        }

        public IDictionary<string, string> GetDownloadPurposes()
        {
            var culture = CultureHelper.GetCurrentCulture();
            if (culture == Culture.NorwegianCode)
                return DownloadPurposes;
            else
                return DownloadPurposesEnglish;
        }

        public IDictionary<string, string> GetListOfOrganizations()
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("organizations");

            IDictionary<string, string> Organizations = new ConcurrentDictionary<string, string>();

            if (cache != null)
            {
                Organizations = cache as IDictionary<string, string>;
            }

            if (Organizations == null && Organizations.Count < 1)
            {
                var url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/organisasjoner";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", Culture.NorwegianCode);

                var responseResult = _httpClient.GetAsync(url).Result;
                HttpContent content = responseResult.Content;
                var data = content.ReadAsStringAsync().Result;

                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var orgs = response["containeditems"];
                lock (Organizations)
                { 
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
                    value = ListOfRestrictionInspireValues["http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1b"]; // "Skjermede data"
                if (value == "no restrictions" || OtherConstraintsAccess == "no restrictions")
                    value = ListOfRestrictionInspireValues["http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/noLimitations"];  //"Åpne data"
                else if (value == "norway digital restricted" || OtherConstraintsAccess == "norway digital restricted")
                    value = ListOfRestrictionInspireValues["http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d"]; //"Norge digitalt-begrenset"
            }
            else
            {
                if (value == "restricted")
                    value = ListOfRestrictionInspireValuesEnglish["http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1b"]; // "Restricted data";
                if (value == "no restrictions" || OtherConstraintsAccess == "no restrictions")
                    value = ListOfRestrictionInspireValuesEnglish["http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/noLimitations"]; // "Open data"
                else if (value == "norway digital restricted" || OtherConstraintsAccess == "norway digital restricted")
                    value = ListOfRestrictionInspireValuesEnglish["http://inspire.ec.europa.eu/metadata-codelist/LimitationsOnPublicAccess/INSPIRE_Directive_Article13_1d"];  //"Norway digitalt restricted"
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

        public IDictionary<string, string> GetCodeList(string systemid, string culture = Culture.NorwegianCode)
        {
            var cacheId = systemid + "_" + culture;
            var cache = memCacher.GetValue(cacheId);

            IDictionary<string, string> CodeValues = new ConcurrentDictionary<string, string>();

            if (cache != null)
            {
                CodeValues = cache as IDictionary<string, string>;
            }
            else
            {
                object lockObj = new object();
                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;

                var message = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                message.Headers.Add("Accept-Language", culture);

                var responseResult = _httpClient.SendAsync(message).Result;
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
                        lock (lockObj)
                        {
                            if (!CodeValues.ContainsKey(codevalue))
                            {
                                CodeValues.Add(codevalue, code["label"].ToString());
                            }
                        }
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

        public IDictionary<string, string> GetCodeListByName(string name, string culture = Culture.NorwegianCode)
        {
            var cacheId = name + "_" + culture;
            var cache = memCacher.GetValue(cacheId);

            IDictionary<string, string> CodeValues = new ConcurrentDictionary<string, string>();

            if (cache != null)
            {
                CodeValues = cache as IDictionary<string, string>;
            }
            else
            {

                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/metadata-kodelister/" + name;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var data = _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

                var response = Newtonsoft.Json.Linq.JObject.Parse(data);
                var codeList = response["containeditems"];
                lock (CodeValues)
                {
                    foreach (var code in codeList)
                    {
                        var codevalue = code["codevalue"].ToString();
                        if (name == "inspire-tilgangsrestriksjoner" && !string.IsNullOrWhiteSpace(codevalue))
                            codevalue = codevalue.Replace("https","http");
                        if (string.IsNullOrWhiteSpace(codevalue))
                            codevalue = code["label"].ToString();

                        if (!CodeValues.ContainsKey(codevalue))
                        {
                            CodeValues.Add(codevalue, code["label"].ToString());
                        }
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

        public IDictionary<string, string> GetSubRegister(string registername, string culture = Culture.NorwegianCode)
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("subregisteritem-" + registername);

            IDictionary<string, string> RegisterItems = new ConcurrentDictionary<string, string>();

            if (cache != null)
            {
                RegisterItems = cache as IDictionary<string, string>;
            }

            if (RegisterItems ==  null && RegisterItems.Count < 1)
            {
                var url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/subregister/" + registername;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var responseResult = _httpClient.GetAsync(url).Result;
                HttpContent content = responseResult.Content;
                var data = content.ReadAsStringAsync().Result;

                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var items = response["containeditems"];
                lock (RegisterItems)
                {
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
                }

                RegisterItems = RegisterItems.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
                memCacher.Add("subregisteritem-" + registername, RegisterItems, new DateTimeOffset(DateTime.Now.AddYears(1)));
            }

            return RegisterItems;
        }

        public IDictionary<string, string> GetEPSGCodeList(string systemid, string culture = Culture.NorwegianCode)
        {
            var cacheId = systemid + "_" + culture;

            var cache = memCacher.GetValue(cacheId);

            IDictionary<string, string> CodeValues = new ConcurrentDictionary<string, string>();

            if (cache != null)
            {
                CodeValues = cache as IDictionary<string, string>;
            }
            else
            {

                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/kodelister/" + systemid;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var responseResult = _httpClient.GetAsync(url).Result;
                HttpContent content = responseResult.Content;
                var data = content.ReadAsStringAsync().Result;
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                var codeList = response["containeditems"];
                lock (CodeValues)
                { 
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
                }

                CodeValues = CodeValues.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

    }
}