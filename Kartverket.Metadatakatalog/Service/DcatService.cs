using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Schema;

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

        XmlDocument doc;

        public XmlDocument GenerateDcat()
        {
            XmlElement root = Setup();

            XmlElement catalog = CreateCatalog(root);

            CreateDatasets(root, catalog);

            return doc;
        }

        private void CreateDatasets(XmlElement root, XmlElement catalog)
        {

            for (int identifier = 1; identifier < 3; identifier++)
            {

                //Map dataset to catalog
                XmlElement catalogDataset = doc.CreateElement("dct", "dataset", xmlnsDct);
                catalogDataset.SetAttribute("resource", xmlnsRdf, "http://www.geonorge.no/geonetwork/resource/" + identifier);
                catalog.AppendChild(catalogDataset);

                XmlElement dataset = doc.CreateElement("dcat", "Dataset", xmlnsDcat);
                dataset.SetAttribute("about", xmlnsRdf, "http://www.geonorge.no/geonetwork/resource/" + identifier);
                root.AppendChild(dataset);

                XmlElement datasetIdentifier = doc.CreateElement("dct", "identifier", xmlnsDct);
                datasetIdentifier.InnerText = identifier.ToString();
                dataset.AppendChild(datasetIdentifier);

                XmlElement datasetTitle = doc.CreateElement("dct", "title", xmlnsDct);
                datasetTitle.InnerText = "Verdifulle kulturlandskap";
                dataset.AppendChild(datasetTitle);

                XmlElement datasetDescription = doc.CreateElement("dct", "description", xmlnsDct);
                datasetDescription.InnerText = "Datasettet viser forvaltningsmessig høyt prioriterte kulturlandskapsområder med både biologiske og kulturhistoriske verdier. Datasettet omfatter blant annet de høgest prioriterte områdene i Nasjonal registrering av verdifulle kulturlandskap, og utgjør 10-30 områder i hvert fylke, totalt ca. 300 områder i landet. Datasettet er ajourført pr. september 2011, og vil bli fortløpende komplettert i Naturbase. Datasettet innholder utvalgte opplysninger fra et separat fagsystem for kulturlandskap som forvaltes av Fylkesmannen. Nærmere opplysninger kan fås derfra.";
                dataset.AppendChild(datasetDescription);

                XmlElement datasetKeyword = doc.CreateElement("dct", "keyword", xmlnsDct);
                datasetKeyword.InnerText = "kulturlandskap";
                dataset.AppendChild(datasetKeyword);

                XmlElement datasetKeyword2 = doc.CreateElement("dct", "keyword", xmlnsDct);
                datasetKeyword2.InnerText = "biologiske og kulturhistoriske verdier";
                dataset.AppendChild(datasetKeyword2);

                //todo theme


                XmlElement datasetThumbnail = doc.CreateElement("foaf", "thumbnail", xmlnsFoaf);
                datasetThumbnail.SetAttribute("resource", xmlnsRdf, "https://www.geonorge.no/geonetwork/srv/nor/resources.get?uuid=a6368bed-4896-41d3-92aa-cc2b4261adc3&amp;access=public&amp;fname=kulturlandskap_dn_s.png");
                dataset.AppendChild(datasetThumbnail);

                XmlElement datasetUpdated = doc.CreateElement("dct", "updated", xmlnsDct);
                datasetUpdated.SetAttribute("datatype", xmlnsRdf, "http://www.w3.org/2001/XMLSchema#date");
                datasetUpdated.InnerText = "2016-01-26";
                dataset.AppendChild(datasetUpdated);

                XmlElement datasetPublisher = doc.CreateElement("dct", "publisher", xmlnsDct);
                datasetPublisher.SetAttribute("resource", xmlnsRdf, "https://register.geonorge.no/register/organisasjoner/kartverket/miljodirektoratet");
                dataset.AppendChild(datasetPublisher);

                XmlElement datasetContactPoint = doc.CreateElement("dcat", "contactPoint", xmlnsDcat);
                dataset.AppendChild(datasetContactPoint);

                XmlElement datasetKind = doc.CreateElement("vcard", "Kind", xmlnsVcard);
                datasetContactPoint.AppendChild(datasetKind);

                XmlElement datasetOrganizationName = doc.CreateElement("vcard", "organization-name", xmlnsVcard);
                datasetOrganizationName.SetAttribute("xml:lang", "");
                datasetOrganizationName.InnerText = "Miljødirektoratet";
                datasetKind.AppendChild(datasetOrganizationName);

                XmlElement datasetHasEmail = doc.CreateElement("vcard", "hasEmail", xmlnsVcard);
                datasetHasEmail.SetAttribute("resource", xmlnsRdf, "mailto:post@mdir.no");
                datasetKind.AppendChild(datasetHasEmail);

                XmlElement datasetAccrualPeriodicity = doc.CreateElement("dct", "accrualPeriodicity", xmlnsDct);
                datasetAccrualPeriodicity.InnerText = "notPlanned";
                dataset.AppendChild(datasetAccrualPeriodicity);

                XmlElement datasetGranularity = doc.CreateElement("dcat", "granularity", xmlnsDcat);
                datasetGranularity.InnerText = "25000";
                dataset.AppendChild(datasetGranularity);

                XmlElement datasetLicense = doc.CreateElement("dct", "license", xmlnsDct);
                datasetLicense.SetAttribute("resource", xmlnsRdf, "http://creativecommons.org/licenses/by/4.0/");
                dataset.AppendChild(datasetLicense);

                XmlElement datasetDataQuality = doc.CreateElement("dcat", "dataQuality", xmlnsDcat);
                datasetDataQuality.InnerText = "Ingen prosseshistorie tilgjenglig.";
                dataset.AppendChild(datasetDataQuality);

                //Distribution

                for (int distributionFormats = 1; distributionFormats < 3; distributionFormats++)
                {
                    //Map distribution to dataset
                    XmlElement distributionDataset = doc.CreateElement("dcat", "distribution", xmlnsDcat);
                    distributionDataset.SetAttribute("resource", xmlnsRdf, "https://www.geonorge.no/geonetwork/srv/nor/xml_iso19139?uuid=d1422d17-6d95-4ef1-96ab-8af31744dd63" + identifier + distributionFormats);
                    dataset.AppendChild(distributionDataset);

                    XmlElement distribution = doc.CreateElement("dcat", "Distribution", xmlnsDcat);
                    distribution.SetAttribute("about", xmlnsRdf, "https://www.geonorge.no/geonetwork/srv/nor/xml_iso19139?uuid=d1422d17-6d95-4ef1-96ab-8af31744dd63" + identifier + distributionFormats);
                    root.AppendChild(distribution);

                    XmlElement distributionTitle = doc.CreateElement("dct", "title", xmlnsDct);
                    distributionTitle.SetAttribute("xml:lang", "no");
                    distributionTitle.InnerText = "Filnedlastning";
                    distribution.AppendChild(distributionTitle);

                    XmlElement distributionDescription = doc.CreateElement("dct", "description", xmlnsDct);
                    distributionDescription.InnerText = "Nedlastning av Tur- og friluftsruter";
                    distribution.AppendChild(distributionDescription);

                    XmlElement distributionFormat = doc.CreateElement("dct", "format", xmlnsDct);
                    distributionFormat.InnerText = "application/gml+xml";
                    distribution.AppendChild(distributionFormat);

                    XmlElement distributionAccessURL = doc.CreateElement("dcat", "accessURL", xmlnsDcat);
                    distributionAccessURL.SetAttribute("resource", xmlnsRdf, "https://kartkatalog.geonorge.no/metadata/uuid/d1422d17-6d95-4ef1-96ab-8af31744dd63");
                    distribution.AppendChild(distributionAccessURL);

                    XmlElement distributionLicense = doc.CreateElement("dct", "license", xmlnsDct);
                    distributionLicense.SetAttribute("resource", xmlnsRdf, "http://creativecommons.org/licenses/by/4.0/");
                    distribution.AppendChild(distributionLicense);

                    XmlElement distributionStatus = doc.CreateElement("adms", "status", xmlnsAdms);
                    distributionStatus.SetAttribute("resource", xmlnsRdf, "http://purl.org/adms/status/Completed");
                    distribution.AppendChild(distributionStatus);

                }


                //Agent/publisher

                if (identifier == 1)
                {

                    XmlElement agent = doc.CreateElement("foaf", "Agent", xmlnsFoaf);
                    agent.SetAttribute("about", xmlnsRdf, "https://register.geonorge.no/register/organisasjoner/kartverket/miljodirektoratet");
                    root.AppendChild(agent);

                    XmlElement agentType = doc.CreateElement("dct", "type", xmlnsDct);
                    agentType.SetAttribute("resource", xmlnsRdf, "http://purl.org/adms/publishertype/NationalAuthority");
                    agent.AppendChild(agentType);

                    XmlElement agentName = doc.CreateElement("foaf", "name", xmlnsFoaf);
                    agentName.InnerText = "Miljødirektoratet";
                    agent.AppendChild(agentName);

                    XmlElement agentMbox = doc.CreateElement("foaf", "mbox", xmlnsFoaf);
                    agentMbox.InnerText = "post@miljodirektoratet.no";
                    agent.AppendChild(agentMbox);
                }
                else if (identifier == 2)
                {
                    XmlElement agent = doc.CreateElement("foaf", "Agent", xmlnsFoaf);
                    agent.SetAttribute("about", xmlnsRdf, "https://register.geonorge.no/register/organisasjoner/kartverket/kartverket");
                    root.AppendChild(agent);

                    XmlElement agentType = doc.CreateElement("dct", "type", xmlnsDct);
                    agentType.SetAttribute("resource", xmlnsRdf, "http://purl.org/adms/publishertype/NationalAuthority");
                    agent.AppendChild(agentType);

                    XmlElement agentName = doc.CreateElement("foaf", "name", xmlnsFoaf);
                    agentName.InnerText = "Kartverket";
                    agent.AppendChild(agentName);

                    XmlElement agentMbox = doc.CreateElement("foaf", "mbox", xmlnsFoaf);
                    agentMbox.InnerText = "post@kartverket.no";
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

            XmlElement catalogModified = doc.CreateElement("dct", "modified", xmlnsDct);
            catalogModified.SetAttribute("datatype", xmlnsRdf, "http://www.w3.org/2001/XMLSchema#date");
            catalogModified.InnerText = "2016-03-26";
            catalog.AppendChild(catalogModified);

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
    }
}