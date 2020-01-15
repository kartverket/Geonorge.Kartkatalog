using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Kartverket.Metadatakatalog.Models.ViewModels
{
    public class SearchByAreaViewModel : SearchViewModel
    {
        public ICollection<Area> Municipalities { get; set; }
        public ICollection<Area> Counties { get; set; }
        public string AreaCode { get; set; }

        public SearchByAreaViewModel(SearchByAreaParameters parameters, SearchResult searchResult)
            : base(parameters, searchResult)
        {
            EnabledFacets = new List<string> { "themes", "types", "nationalinitiatives", "organizations", "distributionProtocols", "dataAccesses" };
            if (parameters.AreaCode != null)
            {
                AreaCode = parameters.AreaCode;
            }
            if (parameters.Facets != null)
            {
                foreach (var facet in parameters.Facets)
                {
                    if (facet.Name == "area")
                    {
                        facet.Value = parameters.AreaCode;
                    }
                }
            }
            Municipalities = GetMunicipalities();
            Counties = GetCounties();
        }

        private ICollection<Area> GetCounties()
        {
            Counties = new Collection<Area>();
            foreach (var area in areaDictionary.OrderBy(n => n.Value))
            {
                if (!IsMunicipal(area.Key))
                {
                    Counties.Add(new Area(area));
                }
            }
            return Counties;
        }


        private ICollection<Area> GetMunicipalities()
        {
            Municipalities = new Collection<Area>();
            foreach (var area in areaDictionary.OrderBy(n => n.Value))
            {
                if (IsMunicipal(area.Key))
                {
                    Municipalities.Add(new Area(area)); ;
                }
            }
            return Municipalities;
        }

        private bool IsMunicipal(string key)
        {
            string[] parts = key.Split('/');
            if (parts.Count() > 2)
            {
                return true;
            }
            return false;
        }

        public RouteValueDictionary AreaRouteValues(RouteValueDictionary routeValues)
        {
            routeValues["areacode"] = AreaCode;
            return routeValues;

        }
    }

    public class Area
    {
        public Area(KeyValuePair<string, string> area)
        {
            Code = area.Key;
            Name = area.Value;
        }

        public string Code { get; set; }
        public string Name { get; set; }
    }
}