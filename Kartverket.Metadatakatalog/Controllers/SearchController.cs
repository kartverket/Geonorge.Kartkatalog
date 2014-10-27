using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Commands.Parameters;
using SolrNet.DSL;
using System.Web;
using Kartverket.Metadatakatalog.Models.Api;

namespace Kartverket.Metadatakatalog.Controllers.Api
{
    public class SearchController : ApiController
    {

        public string Get() {

            return "Search string missing";
        }

        public SearchResult Get(string search)
        {
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();

            string qstr = search;


            var query = Query.Field("title").Is(qstr).Boost(4)
                || Query.Field("abstract").Is(qstr).Boost(3)
                || Query.Field("purpose").Is(qstr)
                || Query.Field("type").Is(qstr)
                || Query.Field("topic_category").Is(qstr)
                || Query.Field("contact_metadata_name").Is(qstr)
                || Query.Field("contact_metadata_organization").Is(qstr)
                || Query.Field("contact_metadata_email").Is(qstr)
                || Query.Field("contact_owner_name").Is(qstr)
                || Query.Field("contact_owner_organization").Is(qstr)
                || Query.Field("contact_owner_email").Is(qstr)
                || Query.Field("contact_publisher_name").Is(qstr)
                || Query.Field("contact_publisher_organization").Is(qstr)
                || Query.Field("contact_publisher_email").Is(qstr)
                || Query.Field("keyword").Is(qstr)
                ;


            
            var metadataIndexDocs = solr.Query(query, new QueryOptions
            {
                Rows = 20 ,
                Start = 0,
                Facet = new FacetParameters
                {
                    Queries = new[] { new SolrFacetFieldQuery("type") , new SolrFacetFieldQuery("title")}
                }
            });


            return SearchResultOutput(metadataIndexDocs);

        }


        private static SearchResult SearchResultOutput(SolrQueryResults<MetadataIndexDoc> metadataIndexDocs)
        {
            List<Metadata> metadataList = new List<Metadata>(); // map'e fra metadataIndexDocs...

            foreach (MetadataIndexDoc metadataIndexDoc in metadataIndexDocs)
            {

                Metadata metadata = new Metadata
                {
                    Uuid = metadataIndexDoc.Uuid,
                    Title = metadataIndexDoc.Title,
                    @Abstract = metadataIndexDoc.Abstract,
                    Purpose = metadataIndexDoc.Purpose,
                    Type = metadataIndexDoc.Type,
                    TopicCategory = metadataIndexDoc.TopicCategory,
                    ContactMetadata = new Kartverket.Metadatakatalog.Models.Api.Contact
                    {
                        Name = metadataIndexDoc.ContactMetadataName,
                        Email = metadataIndexDoc.ContactMetadataEmail,
                        Organization = metadataIndexDoc.ContactMetadataOrganization
                    },
                    ContactOwner = new Kartverket.Metadatakatalog.Models.Api.Contact
                    {
                        Name = metadataIndexDoc.ContactMetadataName,
                        Email = metadataIndexDoc.ContactMetadataEmail,
                        Organization = metadataIndexDoc.ContactMetadataOrganization
                    },
                    ContactPublisher = new Kartverket.Metadatakatalog.Models.Api.Contact
                    {
                        Name = metadataIndexDoc.ContactMetadataName,
                        Email = metadataIndexDoc.ContactMetadataEmail,
                        Organization = metadataIndexDoc.ContactMetadataOrganization
                    },

                    DatePublished = metadataIndexDoc.DatePublished,
                    DateUpdated = metadataIndexDoc.DateUpdated,
                    LegendDescriptionUrl = metadataIndexDoc.LegendDescriptionUrl,
                    ProductPageUrl = metadataIndexDoc.ProductPageUrl,
                    ProductSheetUrl = metadataIndexDoc.ProductSheetUrl,
                    ProductSpecificationUrl = metadataIndexDoc.ProductSpecificationUrl,
                    ThumbnailUrl = metadataIndexDoc.ThumbnailUrl,
                    DistributionUrl = metadataIndexDoc.DistributionUrl,
                    DistributionProtocol = metadataIndexDoc.DistributionProtocol,
                    MaintenanceFrequency = metadataIndexDoc.MaintenanceFrequency

                };

                metadataList.Add(metadata);
            }

            List<Facet> facets = new List<Facet>();

            foreach (var facetField in metadataIndexDocs.FacetFields)
            {
                System.Diagnostics.Debug.WriteLine("{0}", facetField.Key);

                Facet facet = new Facet();
                facet.Name = facetField.Key;
                facet.Values = new List<Facet.FacetValue>();

                foreach (var facetvalueFromIndex in metadataIndexDocs.FacetFields[facetField.Key])
                {

                    System.Diagnostics.Debug.WriteLine("{0}: {1}", facetvalueFromIndex.Key, facetvalueFromIndex.Value);

                    Facet.FacetValue facetvalue = new Facet.FacetValue
                    {
                        Name = facetvalueFromIndex.Key,
                        Count = facetvalueFromIndex.Value,
                    };
                    facet.Values.Add(facetvalue);
                }
                facets.Add(facet);
            }

            SearchResult SResult = new SearchResult
            {
                NumFound = metadataIndexDocs.Count,
                //Limit = 
                //Offset = 
                MetadataList = metadataList,
                Facets = facets
            };
            return SResult;
        } 
    }
}
