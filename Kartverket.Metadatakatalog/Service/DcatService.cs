using GeoNorgeAPI;
using Kartverket.Geonorge.Utilities.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Schema;
using www.opengis.net;

namespace Kartverket.Metadatakatalog.Service
{
    public class DcatService
    {

        const string xmlnsRdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        const string xmlnsFoaf = "http://xmlns.com/foaf/0.1/";
        const string xmlnsGco = "http://www.isotc211.org/2005/gco";
        const string xmlnsVoid = "http://www.w3.org/TR/void/";
        const string xmlnsSkos = "http://www.w3.org/2004/02/skos/core#";
        const string xmlnsDc = "http://purl.org/dc/elements/1.1/";
        const string xmlnsDct = "http://purl.org/dc/terms/";
        const string xmlnsDctype = "http://purl.org/dc/dcmitype/";
        const string xmlnsDcat = "http://www.w3.org/ns/dcat#";
        const string xmlnsVcard = "http://www.w3.org/2006/vcard/ns#";
        const string xmlnsAdms = "http://www.w3.org/ns/adms#";
        const string xmlnsXslUtils = "java:org.fao.geonet.util.XslUtil";
        const string xmlnsGmd = "http://www.isotc211.org/2005/gmd";
        const string xmlnsRdfs = "http://www.w3.org/2000/01/rdf-schema#";

        string kartkatalogenUrl = WebConfigurationManager.AppSettings["KartkatalogenUrl"];

        XmlDocument doc;
        SearchResultsType metadataSets;

        GeoNorge geoNorge = new GeoNorge("", "", WebConfigurationManager.AppSettings["GeoNetworkUrl"]);

        DateTime? catalogLastModified;

        private readonly OrganizationService _organizationService = new OrganizationService(WebConfigurationManager.AppSettings["RegistryUrl"], new HttpClientFactory());

        Dictionary<string, string> OrganizationsLink;

        public XmlDocument GenerateDcat()
        {
            OrganizationsLink = GetOrganizationsLink();
            metadataSets = GetDatasets();

            XmlElement root = Setup();

            XmlElement catalog = CreateCatalog(root);

            CreateDatasets(root, catalog);

            Finalize(root, catalog);

            doc.Save(System.Web.HttpContext.Current.Request.MapPath("~\\dcat\\geonorge_dcat.rdf"));

            return doc;
        }


        private void CreateDatasets(XmlElement root, XmlElement catalog)
        {

            for (int d = 0; d < metadataSets.Items.Length; d++)
            {
                string uuid = ((www.opengis.net.DCMIRecordType)(metadataSets.Items[d])).Items[0].Text[0];
                MD_Metadata_Type md = geoNorge.GetRecordByUuid(uuid);
                var data = new SimpleMetadata(md);

                //Map dataset to catalog
                XmlElement catalogDataset = doc.CreateElement("dct", "dataset", xmlnsDct);
                catalogDataset.SetAttribute("resource", xmlnsRdf, kartkatalogenUrl + "Metadata/uuid/" + data.Uuid);
                catalog.AppendChild(catalogDataset);

                XmlElement dataset = doc.CreateElement("dcat", "Dataset", xmlnsDcat);
                dataset.SetAttribute("about", xmlnsRdf, kartkatalogenUrl + "Metadata/uuid/" + data.Uuid);
                root.AppendChild(dataset);

                XmlElement datasetIdentifier = doc.CreateElement("dct", "identifier", xmlnsDct);
                datasetIdentifier.InnerText = data.Uuid.ToString();
                dataset.AppendChild(datasetIdentifier);

                XmlElement datasetTitle = doc.CreateElement("dct", "title", xmlnsDct);
                datasetTitle.InnerText = data.Title;
                dataset.AppendChild(datasetTitle);


                XmlElement datasetDescription = doc.CreateElement("dct", "description", xmlnsDct);
                if (!string.IsNullOrEmpty(data.Abstract))
                    datasetDescription.InnerText = data.Abstract;
                dataset.AppendChild(datasetDescription);
                    
                foreach(var keyword in data.Keywords) { 

                XmlElement datasetKeyword = doc.CreateElement("dct", "keyword", xmlnsDct);
                datasetKeyword.InnerText = keyword.Keyword;
                dataset.AppendChild(datasetKeyword);

                }

                //Todo theme

                if (data.Thumbnails != null && data.Thumbnails.Count > 0) { 
                    XmlElement datasetThumbnail = doc.CreateElement("foaf", "thumbnail", xmlnsFoaf);
                    datasetThumbnail.SetAttribute("resource", xmlnsRdf, data.Thumbnails[0].URL);
                    dataset.AppendChild(datasetThumbnail);
                }


                XmlElement datasetUpdated = doc.CreateElement("dct", "updated", xmlnsDct);
                datasetUpdated.SetAttribute("datatype", xmlnsRdf, "http://www.w3.org/2001/XMLSchema#date");
                if (data.DateUpdated.HasValue) { 
                    datasetUpdated.InnerText = data.DateUpdated.Value.ToString("yyyy-MM-dd");
                    if(!catalogLastModified.HasValue || data.DateUpdated > catalogLastModified)
                        catalogLastModified = data.DateUpdated;
                }
                dataset.AppendChild(datasetUpdated);


                XmlElement datasetPublisher = doc.CreateElement("dct", "publisher", xmlnsDct);
                if(data.ContactOwner != null && !string.IsNullOrEmpty(data.ContactOwner.Organization) && OrganizationsLink[data.ContactOwner.Organization] != null )
                    datasetPublisher.SetAttribute("resource", xmlnsRdf, OrganizationsLink[data.ContactOwner.Organization]);
                dataset.AppendChild(datasetPublisher);

                Organization organization = null;

                if (data.ContactOwner != null)
                {
                    Task<Organization> getOrganizationTask = _organizationService.GetOrganizationByName(data.ContactOwner.Organization);
                    organization = getOrganizationTask.Result;
                }

                XmlElement datasetContactPoint = doc.CreateElement("dcat", "contactPoint", xmlnsDcat);
                dataset.AppendChild(datasetContactPoint);

                XmlElement datasetKind = doc.CreateElement("vcard", "Kind", xmlnsVcard);
                datasetContactPoint.AppendChild(datasetKind);

                XmlElement datasetOrganizationName = doc.CreateElement("vcard", "organization-name", xmlnsVcard);
                datasetOrganizationName.SetAttribute("xml:lang", "");
                if(organization != null )
                    datasetOrganizationName.InnerText = organization.Name;
                datasetKind.AppendChild(datasetOrganizationName);

                if (data.ContactOwner != null && !string.IsNullOrEmpty(data.ContactOwner.Email))
                { 
                    XmlElement datasetHasEmail = doc.CreateElement("vcard", "hasEmail", xmlnsVcard);
                    datasetHasEmail.SetAttribute("resource", xmlnsRdf, "mailto:" + data.ContactOwner.Email);
                    datasetKind.AppendChild(datasetHasEmail);
                }


                XmlElement datasetAccrualPeriodicity = doc.CreateElement("dct", "accrualPeriodicity", xmlnsDct);
                if (!string.IsNullOrEmpty(data.MaintenanceFrequency))
                    datasetAccrualPeriodicity.InnerText = data.MaintenanceFrequency;
                dataset.AppendChild(datasetAccrualPeriodicity);
     
                XmlElement datasetGranularity = doc.CreateElement("dcat", "granularity", xmlnsDcat);
                if (!string.IsNullOrEmpty(data.ResolutionScale))
                    datasetGranularity.InnerText = data.ResolutionScale;
                dataset.AppendChild(datasetGranularity);
                
                XmlElement datasetLicense = doc.CreateElement("dct", "license", xmlnsDct);
                if(data.Constraints != null && !string.IsNullOrEmpty(data.Constraints.OtherConstraintsLink))
                    datasetLicense.SetAttribute("resource", xmlnsRdf, data.Constraints.OtherConstraintsLink);
                dataset.AppendChild(datasetLicense);


                XmlElement datasetDataQuality = doc.CreateElement("dcat", "dataQuality", xmlnsDcat);
                if (!string.IsNullOrEmpty(data.ProcessHistory))
                    datasetDataQuality.InnerText = data.ProcessHistory;
                dataset.AppendChild(datasetDataQuality);

                //Distribution
                if(data.DistributionFormats != null)
                {                     
                    foreach (var distro in data.DistributionFormats)
                    {
                        //Map distribution to dataset
                        XmlElement distributionDataset = doc.CreateElement("dcat", "distribution", xmlnsDcat);
                        distributionDataset.SetAttribute("resource", xmlnsRdf, kartkatalogenUrl + "/Metadata/uuid/" + data.Uuid + "/"+  distro.Name);
                        dataset.AppendChild(distributionDataset);

                        XmlElement distribution = doc.CreateElement("dcat", "Distribution", xmlnsDcat);
                        distribution.SetAttribute("about", xmlnsRdf, kartkatalogenUrl + "/Metadata/uuid/" + data.Uuid + "/" + distro.Name);
                        root.AppendChild(distribution);

                        XmlElement distributionTitle = doc.CreateElement("dct", "title", xmlnsDct);
                        distributionTitle.SetAttribute("xml:lang", "no");
                        if(data.DistributionDetails != null && !string.IsNullOrEmpty(data.DistributionDetails.Protocol))
                            distributionTitle.InnerText = data.DistributionDetails.Protocol;
                        distribution.AppendChild(distributionTitle);

                        XmlElement distributionDescription = doc.CreateElement("dct", "description", xmlnsDct);
                        if (data.DistributionDetails != null && !string.IsNullOrEmpty(data.DistributionDetails.Name))
                            distributionDescription.InnerText = data.DistributionDetails.Name;
                        distribution.AppendChild(distributionDescription);

                        XmlElement distributionFormat = doc.CreateElement("dct", "format", xmlnsDct);
                        distributionFormat.InnerText = distro.Name;
                        distribution.AppendChild(distributionFormat);

                        XmlElement distributionAccessURL = doc.CreateElement("dcat", "accessURL", xmlnsDcat);
                        distributionAccessURL.SetAttribute("resource", xmlnsRdf, kartkatalogenUrl + "metadata/uuid/" + uuid);
                        distribution.AppendChild(distributionAccessURL);

                        XmlElement distributionLicense = doc.CreateElement("dct", "license", xmlnsDct);
                        if (data.Constraints != null && !string.IsNullOrEmpty(data.Constraints.OtherConstraintsLink))
                            distributionLicense.SetAttribute("resource", xmlnsRdf, data.Constraints.OtherConstraintsLink);
                        distribution.AppendChild(distributionLicense);

                        XmlElement distributionStatus = doc.CreateElement("adms", "status", xmlnsAdms);
                        if (!string.IsNullOrEmpty(data.Status))
                            distributionStatus.SetAttribute("resource", xmlnsRdf, "http://purl.org/adms/status/" + data.Status);
                        distribution.AppendChild(distributionStatus);

                    }

                }


                //Agent/publisher

                XmlElement agent = doc.CreateElement("foaf", "Agent", xmlnsFoaf);
                if (data.ContactOwner != null && !string.IsNullOrEmpty(data.ContactOwner.Organization) && OrganizationsLink[data.ContactOwner.Organization] != null)
                    agent.SetAttribute("about", xmlnsRdf, OrganizationsLink[data.ContactOwner.Organization]);
                root.AppendChild(agent);

                XmlElement agentType = doc.CreateElement("dct", "type", xmlnsDct);
                agentType.SetAttribute("resource", xmlnsRdf, "http://purl.org/adms/publishertype/NationalAuthority");
                agent.AppendChild(agentType);

                XmlElement agentName = doc.CreateElement("foaf", "name", xmlnsFoaf);
                if (organization != null)
                    agentName.InnerText = organization.Name;
                agent.AppendChild(agentName);

                if (data.ContactOwner != null && !string.IsNullOrEmpty(data.ContactOwner.Email))
                {
                    XmlElement agentMbox = doc.CreateElement("foaf", "mbox", xmlnsFoaf);
                    agentMbox.InnerText = data.ContactOwner.Email;
                    agent.AppendChild(agentMbox);
                }

            }
        }

        private XmlElement CreateCatalog(XmlElement root)
        {
            //Catalog info
            XmlElement catalog = doc.CreateElement("dcat", "Catalog", xmlnsDcat);
            catalog.SetAttribute("about", xmlnsRdf, "http://www.geonorge.no/geonetwork");
            root.AppendChild(catalog);

            XmlElement catalogTitle = doc.CreateElement("dct", "title", xmlnsDct);
            catalogTitle.SetAttribute("xml:lang", "no");
            catalogTitle.InnerText = "Geonorge";
            catalog.AppendChild(catalogTitle);

            XmlElement catalogDescription = doc.CreateElement("dct", "description", xmlnsDct);
            catalogDescription.InnerText = "GeoNorge er den nasjonale katalogen for geografisk informasjon";
            catalog.AppendChild(catalogDescription);

            XmlElement catalogIssued = doc.CreateElement("dct", "issued", xmlnsDct);
            catalogIssued.SetAttribute("datatype", xmlnsRdf, "http://www.w3.org/2001/XMLSchema#date");
            catalogIssued.InnerText = DateTime.Now.ToString("yyyy-MM-dd");
            catalog.AppendChild(catalogIssued);


            XmlElement catalogLabel = doc.CreateElement("rdfs", "label", xmlnsRdfs);
            catalogLabel.SetAttribute("xml:lang", "no");
            catalogLabel.InnerText = "GeoNorge";
            catalog.AppendChild(catalogLabel);

            XmlElement catalogHomePage = doc.CreateElement("foaf", "homepage", xmlnsFoaf);
            catalogHomePage.InnerText = "http://www.geonorge.no/geonetwork";
            catalog.AppendChild(catalogHomePage);

            XmlElement catalogOpenSearchDescription = doc.CreateElement("void", "openSearchDescription", xmlnsVoid);
            catalogOpenSearchDescription.InnerText = "http://www.geonorge.no/geonetwork/srv/nor/portal.opensearch";
            catalog.AppendChild(catalogOpenSearchDescription);

            XmlElement catalogUriLookupEndpoint = doc.CreateElement("void", "uriLookupEndpoint", xmlnsVoid);
            catalogUriLookupEndpoint.InnerText = "http://www.geonorge.no/geonetwork/srv/nor/rdf.search?any=";
            catalog.AppendChild(catalogUriLookupEndpoint);

            XmlElement catalogPublisher = doc.CreateElement("dct", "publisher", xmlnsDct);
            catalogPublisher.SetAttribute("resource", xmlnsRdf, "https://register.geonorge.no/register/organisasjoner/kartverket/kartverket");
            catalog.AppendChild(catalogPublisher);

            XmlElement catalogLicense = doc.CreateElement("dct", "license", xmlnsDct);
            catalogLicense.SetAttribute("resource", xmlnsRdf, "http://creativecommons.org/licenses/by/4.0/");
            catalog.AppendChild(catalogLicense);

            XmlElement catalogLanguage = doc.CreateElement("dct", "language", xmlnsDct);
            catalogLanguage.InnerText = "nor";
            catalog.AppendChild(catalogLanguage);

            XmlElement catalogThemeTaxonomy = doc.CreateElement("dct", "themeTaxonomy", xmlnsDct);
            catalogThemeTaxonomy.SetAttribute("resource", xmlnsRdf, "http://www.eionet.europa.eu/gemet/inspire_themes");
            catalog.AppendChild(catalogThemeTaxonomy);
            return catalog;
        }

        private XmlElement Setup()
        {
            doc = new XmlDocument();

            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);
            XmlElement root = doc.CreateElement("rdf", "RDF", xmlnsRdf);
            root.SetAttribute("xmlns:foaf", xmlnsFoaf);
            root.SetAttribute("xmlns:gco", xmlnsGco);
            root.SetAttribute("xmlns:void", xmlnsVoid);
            root.SetAttribute("xmlns:skos", xmlnsSkos);
            root.SetAttribute("xmlns:dc", xmlnsDc);
            root.SetAttribute("xmlns:dct", xmlnsDct);
            root.SetAttribute("xmlns:dctype", xmlnsDctype);
            root.SetAttribute("xmlns:dcat", xmlnsDcat);
            root.SetAttribute("xmlns:vcard", xmlnsVcard);
            root.SetAttribute("xmlns:adms", xmlnsAdms);
            root.SetAttribute("xmlns:xslUtils", xmlnsXslUtils);
            root.SetAttribute("xmlns:gmd", xmlnsGmd);
            root.SetAttribute("xmlns:rdfs", xmlnsRdfs);

            doc.AppendChild(root);
            return root;
        }

        private void Finalize(XmlElement root, XmlElement catalog)
        {
            XmlElement catalogModified = doc.CreateElement("dct", "modified", xmlnsDct);
            catalogModified.SetAttribute("datatype", xmlnsRdf, "http://www.w3.org/2001/XMLSchema#date");
            if (catalogLastModified.HasValue)
                catalogModified.InnerText = catalogLastModified.Value.ToString("yyyy-MM-dd");
            catalog.AppendChild(catalogModified);
        }


        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SearchResultsType GetDatasets()
        {
            GeoNorge _geoNorge = new GeoNorge("", "", WebConfigurationManager.AppSettings["GeoNetworkUrl"] + "srv/nor/csw-dataset?");
            _geoNorge.OnLogEventDebug += new GeoNorgeAPI.LogEventHandlerDebug(LogEventsDebug);
            _geoNorge.OnLogEventError += new GeoNorgeAPI.LogEventHandlerError(LogEventsError);
            var filters = new object[]
            {
                    new PropertyIsLikeType
                        {
                            escapeChar = "\\",
                            singleChar = "_",
                            wildCard = "%",
                            PropertyName = new PropertyNameType {Text = new[] {"Subject"}},
                            Literal = new LiteralType {Text = new[] {"åpne data"}}
                        }
            };

            var filterNames = new ItemsChoiceType23[]
            {
                        ItemsChoiceType23.PropertyIsLike,
            };

            var result = _geoNorge.SearchWithFilters(filters, filterNames, 1, 1000, false);
            return result;
        }

        private void LogEventsDebug(string log)
        {

            System.Diagnostics.Debug.Write(log);
            Log.Debug(log);
        }

        private void LogEventsError(string log, Exception ex)
        {
            Log.Error(log, ex);
        }

        public Dictionary<string, string>GetOrganizationsLink()
        {
            Dictionary<string, string> Organizations = new Dictionary<string, string>();

            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/organisasjoner");
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var orgs = response["containeditems"];

            foreach (var org in orgs)
            {
                if (!Organizations.ContainsKey(org["label"].ToString()))
                {
                    Organizations.Add(org["label"].ToString(), org["id"].ToString());
                }
            }

            return Organizations;
        }

    }
}