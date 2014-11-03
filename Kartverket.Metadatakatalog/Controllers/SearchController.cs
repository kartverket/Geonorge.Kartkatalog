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


// Metadata search api examples

// Return all documents:
// api/search/

// Input only searchstring, returns limit=10 , offset=1 and default facet field=type:
// api/search/?text=Norge

// Limit hits:
// ?text=norge&limit=2

// Set offset:
// ?text=norge&limit=2&offset=3

// Get facet=type with value=service:
// ?text=norge&facets[0]name=type&facets[0]value=service

// For more facets limitations:
// ?text=norge&facets[0]name=type&facets[0]value=service&facets[1]name=organization&facets[1]value=kartverket

// Only define facets without facet limitation:
// ?text=norge&facets[0]name=type&facets[1]name=organization

// Set facets with some limitations:
// ?text=norge&facets[0]name=type&facets[1]name=organization&facets[1]value=kartverket


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
                     || Query.Field("theme").Is(qstr)
                     || Query.Field("organization").Is(qstr)
                     || Query.Field("topic_category").Is(qstr)
                     || Query.Field("keyword").Is(qstr)
                     || Query.Field("uuid").Is(qstr)
                     ;
                 return query;
             }
                 
             return SolrQuery.All; 
         } 
 
 
         public ICollection<ISolrQuery> BuildFilterQueries(SearchParameters parameters) { 
             var queriesFromFacets = from p in facetsInternal
                                     where p.value != null && p.value != ""
                                     select (ISolrQuery)Query.Field(p.name).In(p.value.Split(',')); 
             return queriesFromFacets.ToList(); 
         }


        string[] AllFacetFields;
        List<FacetInput.FacetMap> facetsInternal;


        public SearchResult Get([System.Web.Http.ModelBinding.ModelBinder(typeof(SM.General.Api.FieldValueModelBinder))]  SearchParameters parameters)
        {

            try{
                
                if (parameters == null)
                    parameters = new SearchParameters();

                facetsInternal = new List<FacetInput.FacetMap>();

                foreach (var facetinput in parameters.facets) {

                    if (facetinput.name == "type")
                        facetsInternal.Add(new FacetInput.FacetMap("type", facetinput.value, facetinput.name));
                    if(facetinput.name == "organization")
                        facetsInternal.Add(new FacetInput.FacetMap("organization", UppercaseFirst(facetinput.value), facetinput.name));
                    if (facetinput.name == "theme")
                        facetsInternal.Add(new FacetInput.FacetMap("theme", facetinput.value, facetinput.name));
                
                }

                var AllFacetFieldsQuery = facetsInternal.GroupBy(f => f.name).Select(group => group.FirstOrDefault());

                List<FacetInput.FacetMap> AllFacetFieldsList = AllFacetFieldsQuery.ToList();

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

                var start = parameters.offset-1; 
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
                return SearchResultOutput(metadataIndexDocs, parameters.limit, parameters.offset, facetsInternal);
            }
            catch(Exception ex){
            
                Log.Error(ex.Message);
            }

            return null;

        }


        private static SearchResult SearchResultOutput(SolrQueryResults<MetadataIndexDoc> metadataIndexDocs, int Limit, int Offset, List<FacetInput.FacetMap> facetsInternal)
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
                    Theme = metadataIndexDoc.Theme,
                    Organization = metadataIndexDoc.Organization,
                    OrganizationLogo = metadataIndexDoc.OrganizationLogoUrl,
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

                var mapedFacet = facetsInternal.FirstOrDefault(fi => fi.name == facetField.Key);
                if (mapedFacet != null)
                    facet.FacetField = mapedFacet.map;


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

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            string[] moreValues = s.Split(',');
            string strReturn = "";
            for (int st = 0; st < moreValues.Length;st++ )
            {
                strReturn = strReturn + char.ToUpper(moreValues[st][0]) + moreValues[st].Substring(1);
                if (st < moreValues.Length-1)
                    strReturn = strReturn + ",";
            }

            return strReturn;
        }

    }
}
