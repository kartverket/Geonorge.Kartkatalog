using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Kartverket.Metadatakatalog.Service
{
    public class RegisterFetcher
    {
        MemoryCacher memCacher = new MemoryCacher();
        private static readonly WebClient _webClient = new WebClient();

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
        Dictionary<string, string> ListOfCoordinatesystemNameValues = new Dictionary<string, string>();
        Dictionary<string, string> ListOfCoordinatesystemNameValuesEnglish = new Dictionary<string, string>();
        Dictionary<string, string> ListOfDistributionTypes = new Dictionary<string, string>();
        Dictionary<string, string> ListOfDistributionTypesEnglish = new Dictionary<string, string>();
        public Dictionary<string, string> OrganizationShortNames = new Dictionary<string, string>();


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
            ListOfCoordinatesystemNameValues = GetEPSGCodeList("37B9DC41-D868-4CBC-84F9-39557041FB2C");
            ListOfCoordinatesystemNameValuesEnglish = GetEPSGCodeList("37B9DC41-D868-4CBC-84F9-39557041FB2C", Culture.EnglishCode);
            ListOfDistributionTypes = GetCodeList("94B5A165-7176-4F43-B6EC-1063F7ADE9EA");
            ListOfDistributionTypesEnglish = GetCodeList("94B5A165-7176-4F43-B6EC-1063F7ADE9EA", Culture.EnglishCode );
            OrganizationShortNames = GetListOfOrganizations();

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
                _webClient.Encoding = System.Text.Encoding.UTF8;
                var data = _webClient.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/organisasjoner");
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
                    value = "Skjermede data";
                if (OtherConstraintsAccess == "no restrictions")
                    value = "Åpne data";
                else if (OtherConstraintsAccess == "norway digital restricted")
                    value = "Norge digitalt-begrenset";
            }
            else
            {
                if (value == "restricted")
                    value = "Restricted data";
                if (OtherConstraintsAccess == "no restrictions")
                    value = "Open data";
                else if (OtherConstraintsAccess == "norway digital restricted")
                    value = "Norway digitalt restricted";
            }

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
                _webClient.Headers.Add("Accept-Language", culture);
                _webClient.Encoding = System.Text.Encoding.UTF8;
                var data = _webClient.DownloadString(url);
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
                _webClient.Encoding = System.Text.Encoding.UTF8;
                var data = _webClient.DownloadString(url);
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