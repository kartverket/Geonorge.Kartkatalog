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
using System.Web.Mvc;


namespace Kartverket.Metadatakatalog.Controllers.Api
{
    public class SearchController : ApiController
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


         public ISolrQuery BuildQuery(SearchParameters parameters) {

             if (!string.IsNullOrEmpty(parameters.text)) {
                 var qstr = parameters.text;
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
             var queriesFromFacets = from p in parameters.facets
                                     where p.value != null && p.value != ""
                                     select (ISolrQuery)Query.Field(p.name).Is(p.value); 
             return queriesFromFacets.ToList(); 
         }


        string[] AllFacetFields;


        public SearchResult Get([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))]  SearchParameters parameters)
        {

            try{
                
                if (parameters == null)
                    parameters = new SearchParameters();
                    

                var AllFacetFieldsQuery = parameters.facets.GroupBy(f => f.name).Select(group => group.FirstOrDefault());

                List<FacetInput> AllFacetFieldsList = AllFacetFieldsQuery.ToList();

                if (AllFacetFieldsList.Count > 0)
                {

                    AllFacetFields = new string[AllFacetFieldsList.Count];

                    int fcounter = 0;

                    foreach (var f in AllFacetFieldsList)
                    {
                        AllFacetFields[fcounter] = f.name;
                        fcounter++;
                    }

                }

                else {
                    //Default facet
                    AllFacetFields = new[] { "type" };
                
                }
             
                var solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();

                var start = parameters.offset - 1; //(parameters.offset - 1) * parameters.limit;
                var metadataIndexDocs = solr.Query(BuildQuery(parameters), new QueryOptions
                { 
                         FilterQueries = BuildFilterQueries(parameters),
                         Rows = parameters.limit,
                         StartOrCursor = new StartOrCursor.Start(start), 
                         Facet = new FacetParameters { 
                             Queries = AllFacetFields.Select(f => new SolrFacetFieldQuery(f) {MinCount = 1}) 
                                                     .Cast<ISolrFacetQuery>()
                                                     .ToList(),
                         }, 
                     });
                return SearchResultOutput(metadataIndexDocs, parameters.limit, parameters.offset);
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
                    Type = metadataIndexDoc.Type,
                    Theme = "TODO",
                    Organization = metadataIndexDoc.Organization,
                    OrganizationLogo = "TODO",
                    ThumbnailUrl = metadataIndexDoc.ThumbnailUrl,
                    DistributionUrl = metadataIndexDoc.DistributionUrl,
                    DistributionProtocol = metadataIndexDoc.DistributionProtocol,
                    ShowDetailsUrl = "http://metadata.dev.geonorge.no/metadata/?uuid=" + metadataIndexDoc.Uuid

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
