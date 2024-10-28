﻿using Kartverket.Metadatakatalog.Service.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class SearchByAreaParameters : SearchParameters
    {
        public SearchByAreaParameters(IAiService aiService) : base(aiService: aiService)
        {
        }

        public string AreaCode { get; set; }

        public void CreateFacetOfArea()
        {
            Facets.Add(new FacetParameter { Name = "area", Value = AreaCode });
        }
    }
}