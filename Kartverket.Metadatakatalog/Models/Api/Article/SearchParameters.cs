using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api.Article
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
        /// Field to order by, ex: orderby=title, default order is score desc.
        /// </summary>
        public string orderby { get; set; }

        public SearchParameters()
        {

            limit = 10;
            offset = 1;
        }
    }


}