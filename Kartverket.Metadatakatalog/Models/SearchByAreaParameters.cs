using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchByAreaParameters : SearchParameters
    {
        public SearchByAreaParameters(IAiService aiService, ILogger<SearchParameters> logger, ISimpleMetadataUtil simpleMetadataUtil) : base(aiService: aiService, logger: logger, simpleMetadataUtil: simpleMetadataUtil)
        {
        }

        public string AreaCode { get; set; }

        public void CreateFacetOfArea()
        {
            Facets.Add(new FacetParameter { Name = "area", Value = AreaCode });
        }
    }
}