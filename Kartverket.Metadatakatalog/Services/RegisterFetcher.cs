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
using Microsoft.Extensions.Configuration;

namespace Kartverket.Metadatakatalog.Service
{
    public class RegisterFetcher
    {
        MemoryCacher memCacher = new MemoryCacher();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        IDictionary<string, CodeListValue> TopicCategories = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> TopicCategoriesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> SpatialRepresentations = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> SpatialRepresentationsEnglish = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> MaintenanceFrequencyValues = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> MaintenanceFrequencyValuesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfStatusValues = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfStatusValuesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfClassificationValues = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfClassificationValuesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfRestrictionValues = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfRestrictionValuesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        public IDictionary<string, CodeListValue> ListOfRestrictionInspireValues = new ConcurrentDictionary<string, CodeListValue>();
        public IDictionary<string, CodeListValue> ListOfRestrictionInspireValuesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfCoordinatesystemNameValues = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfCoordinatesystemNameValuesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        public IDictionary<string, CodeListValue> ListOfDistributionTypes = new ConcurrentDictionary<string, CodeListValue>();
        public IDictionary<string, CodeListValue> ListOfDistributionTypesEnglish = new ConcurrentDictionary<string, CodeListValue>();
        public IDictionary<string, CodeListValue> OrganizationShortNames = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfOrderingInstructions = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> ListOfOrderingInstructionsEnglish = new ConcurrentDictionary<string, CodeListValue>();

        IDictionary<string, CodeListValue> DownloadUseGroups = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> DownloadUseGroupsEnglish = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> DownloadPurposes = new ConcurrentDictionary<string, CodeListValue>();
        IDictionary<string, CodeListValue> DownloadPurposesEnglish = new ConcurrentDictionary<string, CodeListValue>();

        IDictionary<string, CodeListValue> ListOfInspire = new ConcurrentDictionary<string, CodeListValue>();
        

        public RegisterFetcher(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

        private HttpClient CreateHttpClient()
        {
            return _httpClientFactory.CreateClient();
        }

        public IDictionary<string, string> GetDownloadUseGroups()
        {
            var culture = CultureHelper.GetCurrentCulture();
            var source = culture == Culture.NorwegianCode ? DownloadUseGroups : DownloadUseGroupsEnglish;
            return source.ToDictionary(o => o.Key, o => o.Value.Value);
        }

        public IDictionary<string, string> GetDownloadPurposes()
        {
            var culture = CultureHelper.GetCurrentCulture();
            var source = culture == Culture.NorwegianCode ? DownloadPurposes : DownloadPurposesEnglish;
            return source.ToDictionary(o => o.Key, o => o.Value.Value);
        }

        public IDictionary<string, CodeListValue> GetListOfOrganizations()
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("organizations");

            IDictionary<string, CodeListValue> Organizations = new ConcurrentDictionary<string, CodeListValue>();

            if (cache != null)
            {
                Organizations = cache as IDictionary<string, CodeListValue>;
            }

            if (Organizations == null && Organizations.Count < 1)
            {
                var url = _configuration?["RegistryUrl"] + "api/register/organisasjoner";
                using var httpClient = CreateHttpClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", Culture.NorwegianCode);

                var responseResult = httpClient.GetAsync(url).Result;
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
                            Organizations.Add(name, new CodeListValue(shortName));
                        }
                    }
                }

                Organizations = Organizations.OrderBy(o => o.Value.Value).ToDictionary(o => o.Key, o => o.Value);
                memCacher.Add("organizations", Organizations, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return Organizations;
        }


        public string GetCoordinatesystemName(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? ListOfCoordinatesystemNameValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfCoordinatesystemNameValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }
        public string GetDistributionType(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? ListOfDistributionTypes.Where(p => p.Key == value).FirstOrDefault() 
                : ListOfDistributionTypesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public string GetDistributionTypeDescription(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? ListOfDistributionTypes.Where(p => p.Key == value).FirstOrDefault()
                : ListOfDistributionTypesEnglish.Where(p => p.Key == value).FirstOrDefault();

            return dic.Value?.Description;
        }

        public string GetSpatialRepresentation(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? SpatialRepresentations.Where(p => p.Key == value).FirstOrDefault()
                : SpatialRepresentationsEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public string GetTopicCategory(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? TopicCategories.Where(p => p.Key == value).FirstOrDefault()
                : TopicCategoriesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public string GetMaintenanceFrequency(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? MaintenanceFrequencyValues.Where(p => p.Key == value).FirstOrDefault()
                : MaintenanceFrequencyValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public string GetStatus(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? ListOfStatusValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfStatusValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public string GetInspire(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = 
                 ListOfInspire.Where(p => p.Key == value).FirstOrDefault();
                
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public string GetClassification(string value)
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? ListOfClassificationValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfClassificationValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public string GetRestriction(string value, string OtherConstraintsAccess ="")
        {
            var culture = CultureHelper.GetCurrentCulture();
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? ListOfRestrictionValues.Where(p => p.Key == value).FirstOrDefault()
                : ListOfRestrictionValuesEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
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
            KeyValuePair<string, CodeListValue> dic = culture == Culture.NorwegianCode
                ? ListOfOrderingInstructions.Where(p => p.Key == value).FirstOrDefault()
                : ListOfOrderingInstructionsEnglish.Where(p => p.Key == value).FirstOrDefault();
            if (!dic.Equals(default(KeyValuePair<String, CodeListValue>)))
                value = dic.Value;

            return value;
        }

        public IDictionary<string, CodeListValue> GetCodeList(string systemid, string culture = Culture.NorwegianCode)
        {
            var cacheId = systemid + "_" + culture;
            var cache = memCacher.GetValue(cacheId);

            IDictionary<string, CodeListValue> CodeValues = new ConcurrentDictionary<string, CodeListValue>();

            if (cache != null)
            {
                CodeValues = cache as IDictionary<string, CodeListValue>;
            }
            else
            {
                object lockObj = new object();
                string url = _configuration?["RegistryUrl"] + "api/kodelister/" + systemid;

                using var httpClient = CreateHttpClient();
                var message = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                message.Headers.Add("Accept-Language", culture);

                var responseResult = httpClient.SendAsync(message).Result;
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
                                CodeValues.Add(codevalue, new CodeListValue(code["label"].ToString(), code["description"]?.ToString()));
                            }
                        }
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

        public IDictionary<string, CodeListValue> GetCodeListByName(string name, string culture = Culture.NorwegianCode)
        {
            var cacheId = name + "_" + culture;
            var cache = memCacher.GetValue(cacheId);

            IDictionary<string, CodeListValue> CodeValues = new ConcurrentDictionary<string, CodeListValue>();

            if (cache != null)
            {
                CodeValues = cache as IDictionary<string, CodeListValue>;
            }
            else
            {

                string url = _configuration?["RegistryUrl"] + "api/metadata-kodelister/" + name;

                using var httpClient = CreateHttpClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var data = httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

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
                            CodeValues.Add(codevalue, new CodeListValue(code["label"].ToString(), code["description"]?.ToString()));
                        }
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

        public IDictionary<string, CodeListValue> GetSubRegister(string registername, string culture = Culture.NorwegianCode)
        {
            MemoryCacher memCacher = new MemoryCacher();

            var cache = memCacher.GetValue("subregisteritem-" + registername);

            IDictionary<string, CodeListValue> RegisterItems = new ConcurrentDictionary<string, CodeListValue>();

            if (cache != null)
            {
                RegisterItems = cache as IDictionary<string, CodeListValue>;
            }

            if (RegisterItems ==  null && RegisterItems.Count < 1)
            {
                var url = _configuration?["RegistryUrl"] + "api/subregister/" + registername;

                using var httpClient = CreateHttpClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var responseResult = httpClient.GetAsync(url).Result;
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
                            RegisterItems.Add(id, new CodeListValue(item["label"].ToString(), item["description"]?.ToString()));
                        }
                    }
                }

                RegisterItems = RegisterItems.OrderBy(o => o.Value.Value).ToDictionary(o => o.Key, o => o.Value);
                memCacher.Add("subregisteritem-" + registername, RegisterItems, new DateTimeOffset(DateTime.Now.AddYears(1)));
            }

            return RegisterItems;
        }

        public IDictionary<string, CodeListValue> GetEPSGCodeList(string systemid, string culture = Culture.NorwegianCode)
        {
            var cacheId = systemid + "_" + culture;

            var cache = memCacher.GetValue(cacheId);

            IDictionary<string, CodeListValue> CodeValues = new ConcurrentDictionary<string, CodeListValue>();

            if (cache != null)
            {
                CodeValues = cache as IDictionary<string, CodeListValue>;
            }
            else
            {

                string url = _configuration?["RegistryUrl"] + "api/kodelister/" + systemid;
                using var httpClient = CreateHttpClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", culture);

                var responseResult = httpClient.GetAsync(url).Result;
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
                            CodeValues.Add(codevalue, new CodeListValue(code["label"].ToString(), code["description"]?.ToString()));
                        }
                    }
                }

                CodeValues = CodeValues.OrderBy(o => o.Value.Value).ToDictionary(o => o.Key, o => o.Value);

                memCacher.Add(cacheId, CodeValues, new DateTimeOffset(DateTime.Now.AddHours(12)));

            }

            return CodeValues;
        }

    }
}