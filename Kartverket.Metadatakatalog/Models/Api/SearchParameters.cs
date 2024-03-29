﻿using System;
using System.Collections.Generic;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class SearchParameters
    {
        /// <summary>
        /// The text to search for
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// This param indicates an offset into the list of constraints to allow paging. Default is 1.
        /// </summary>
        public int offset { get; set; }
        /// <summary>
        /// This param indicates the maximum number of constraint counts that should be returned. Default is 10.
        /// </summary>
        public int limit { get; set; }
        /// <summary>
        /// List hidden metadata like series_historic, series_time
        /// </summary>
        public bool listhidden { get; set; }
        /// <summary>
        /// Limits the list to facets. Ex. <![CDATA[ &facets[0]name=organization&facets[0]value=Kartverket]]>. Facets result is grouped by  "theme" , "type", "organization".
        /// </summary>
        /// <summary>
        /// Field to order by, ex: orderby=title, default order is score desc.
        /// </summary>
        public string orderby { get; set; }
        public DateTime? datefrom { get; set; }
        public DateTime? dateto { get; set; }
        public List<FacetInput> facets { get; set; }

        public SearchParameters()
        {
            facets = new List<FacetInput>();
            
            limit = 10;
            offset = 1;
        } 
    }

    
}