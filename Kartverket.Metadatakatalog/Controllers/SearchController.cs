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
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


         public ISolrQuery BuildQuery(SearchParameters parameters) {

             if (!string.IsNullOrEmpty(parameters.FreeSearch)) {
                 var qstr = parameters.FreeSearch;
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
                 return query;
             }
                 
             return SolrQuery.All; 
         } 
 
 
         public ICollection<ISolrQuery> BuildFilterQueries(SearchParameters parameters) { 
             var queriesFromFacets = from p in parameters.Facets 
                                     select (ISolrQuery)Query.Field(p.Key).Is(p.Value); 
             return queriesFromFacets.ToList(); 
         }

        private static readonly string[] AllFacetFields = new[] { "type" };


        public IEnumerable<string> SelectedFacetFields(SearchParameters parameters) { 
             return parameters.Facets.Select(f => f.Key); 
         }



        public SearchResult Get([FromUri] SearchParameters parameters)
        {
            
            try{

                if (string.IsNullOrWhiteSpace(parameters.FreeSearch)){
                    throw new ArgumentException("FreeSearch parameter missing");
                }
                
                
                IDictionary<string, string> FacetParam = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(parameters.Type))
                    FacetParam.Add("type", parameters.Type);
                if (!string.IsNullOrWhiteSpace(parameters.Organization))
                    FacetParam.Add("contact_metadata_organization", parameters.Organization);

                parameters.Facets = FacetParam;

                var solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();

                var start = (parameters.Offset - 1) * parameters.Limit;
                var metadataIndexDocs = solr.Query(BuildQuery(parameters), new QueryOptions
                { 
                         FilterQueries = BuildFilterQueries(parameters), 
                         Rows = parameters.Limit,
                         StartOrCursor = new StartOrCursor.Start(start), 
                         Facet = new FacetParameters { 
                             Queries = AllFacetFields.Select(f => new SolrFacetFieldQuery(f) {MinCount = 1}) 
                                                     .Cast<ISolrFacetQuery>()
                                                     .ToList(),
                         }, 
                     });
                return SearchResultOutput(metadataIndexDocs, parameters.Limit, parameters.Offset);
            }
            catch(Exception ex){
            
                Log.Error(ex.Message);
            }

            return null;

        }


        private static SearchResult SearchResultOutput(SolrQueryResults<MetadataIndexDoc> metadataIndexDocs, int Limit, int Offset)
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

                Facet facet = new Facet();
                facet.FacetField = facetField.Key;
                facet.FacetResults = new List<Facet.FacetValue>();

                foreach (var facetvalueFromIndex in metadataIndexDocs.FacetFields[facetField.Key])
                {

                    Facet.FacetValue facetvalue = new Facet.FacetValue
                    {
                        Name = facetvalueFromIndex.Key,
                        Count = facetvalueFromIndex.Value,
                    };
                    facet.FacetResults.Add(facetvalue);
                }
                facets.Add(facet);
            }

            SearchResult SResult = new SearchResult
            {
                NumFound = metadataIndexDocs.NumFound,
                Limit = Limit,
                Offset = Offset,
                Results = metadataList,
                Facets = facets
            };
            return SResult;
        } 
    }
}
