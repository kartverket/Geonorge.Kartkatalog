using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class SearchController : ApiController
    {
        public IEnumerable<MetadataIndexDoc> Get()
        {
            
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();

            var results = solr.Query(new SolrQueryByField("uuid", "e461747b-8482-4b2b-88af-cf920570de2d"));

            return results.ToList();
        } 

    }
}
