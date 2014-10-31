using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models.Api
{
    
    public class FacetInput
    {
        public FacetInput()
        {
        }
        public FacetInput(string name,string value)
        {
            this.name = name;
            this.value = value;
        }
        public string name { get; set; }
        public string value { get; set; }


        public class FacetMap : FacetInput
        {
            public FacetMap(string name, string value, string map) : base (name, value)
            {    
            this.map = map;
            }

            public string map { get; set; }
        
        }
    }
}