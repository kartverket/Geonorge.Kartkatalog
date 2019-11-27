﻿using SolrNet;
using System.Collections.Generic;
using System.Linq;
using System;
using SolrNet.Commands.Parameters;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using Resources;

namespace Kartverket.Metadatakatalog.Models
{
    public struct OrderBy
    {
        private string value;
        private OrderBy(string value)
        {
            this.value = value;
        }

        public static readonly OrderBy score = new OrderBy("score");
        public static readonly OrderBy title = new OrderBy("title");
        public static readonly OrderBy title_desc = new OrderBy("title_desc");
        public static readonly OrderBy organization = new OrderBy("organization");
        public static readonly OrderBy organization_desc = new OrderBy("organization_desc");
        public static readonly OrderBy newest = new OrderBy("newest");
        public static readonly OrderBy updated = new OrderBy("updated");
        public static readonly OrderBy popularMetadata = new OrderBy("popularMetadata");

        public override string ToString()
        {
            return this.value;
        }

    }

    public class SearchParameters
    {
        public SearchParameters()
        {
            Facets = new List<FacetParameter>();
            Offset = 1;
            Limit = 30;
            orderby = Models.OrderBy.score.ToString();
        }

        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string orderby { get; set; }
        public List<FacetParameter> Facets { get; set; }

        public void AddComplexFacetsIfMissing()
        {
            AddDefaultFacetsIfMissing();
        }

        public void AddDefaultFacetsIfMissing()
        {
            List<FacetParameter> FacetList = new List<FacetParameter>(); 
            var facets = new List<string> { "type", "theme", "organization", "nationalinitiative", "placegroups", "DistributionProtocols", "area", "dataaccess", "spatialscope" };

            foreach (var defaultFacet in facets)
            {
                if (Facets != null)
                {
                    var selectedFacets = Facets.Where(f => f.Name == defaultFacet);
                    if (selectedFacets.Any()) 
                    {
                        foreach (var selectedFacet in selectedFacets)
                        {
                            FacetList.Add(new FacetParameter
                            {
                                Name = selectedFacet.Name,
                                NameTranslated = selectedFacet.NameTranslated,
                                Value = selectedFacet.Value
                            });
                        }
                    }
                    else { 
                        FacetList.Add(new FacetParameter
                        {
                            Name = defaultFacet,
                            NameTranslated = UI.ResourceManager.GetString("Facet_" + defaultFacet)
                        });
                    }
                }
            }
            Facets = FacetList;

        }


        /// <summary>
        /// Gets a sort order based on "Orderby" parameter
        /// </summary>
        /// <returns></returns>
        public SortOrder[] OrderBy()
        {
            var order = new[] { new SortOrder("score", Order.DESC) };
            if (orderby == Models.OrderBy.title.ToString())
            {
                order = new[] { new SortOrder("title", Order.ASC) };
            }
            else if (orderby == Models.OrderBy.title_desc.ToString())
            {
                order = new[] { new SortOrder("title", Order.DESC) };
            }
            else if (orderby == Models.OrderBy.organization.ToString())
            {
                order = new[] { new SortOrder("organization", Order.ASC) };
            }
            else if (orderby == Models.OrderBy.organization_desc.ToString())
            {
                order = new[] { new SortOrder("organization", Order.DESC) };
            }
            else if (orderby == Models.OrderBy.newest.ToString())
            {
                order = new[] { new SortOrder("date_published", Order.DESC) };
            }
            else if (orderby == Models.OrderBy.updated.ToString())
            {
                order = new[] { new SortOrder("date_updated", Order.DESC) };
            }
            else if (string.IsNullOrWhiteSpace(Text) && !HasNoFacetvalue())
            {
                order = new[] { new SortOrder("title", Order.ASC) };
            }
            else if (string.IsNullOrWhiteSpace(Text) && HasNoFacetvalue())
            {
                order = new[] { new SortOrder("popularMetadata", Order.DESC) };
            }
            else if (orderby == Models.OrderBy.score.ToString())
            {
                order = new[] { new SortOrder("score", Order.DESC) };
            }
            return order;
        }


        private bool HasNoFacetvalue()
        {
            bool hasnovalue = true;
            foreach (FacetParameter f in Facets)
            {
                if (!string.IsNullOrEmpty(f.Value))
                {
                    hasnovalue = false;
                    break;
                }
            }
            return hasnovalue;
        }

        /// <summary>
        /// Builds a Solr query based on "Text" parameter
        /// </summary>
        /// <returns></returns>
        public ISolrQuery BuildQuery()
        {
            log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            ISolrQuery query;
            var text = Text;
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace(":", " ");
                text = text.Replace("!", " ");
                text = text.Replace("{", " ");
                text = text.Replace("}", " ");
                text = text.Replace("[", " ");
                text = text.Replace("]", " ");
                text = text.Replace("(", " ");
                text = text.Replace(")", " ");
                text = text.Replace("^", " ");
                text = text.Replace("-", "\\-");

                if (text.Trim().Length == 0) query = SolrQuery.All;
                else if (text.Trim().Length < 5)
                {
                    query = new SolrMultipleCriteriaQuery(new[]
                    {
                        new SolrQuery("titleText:"+ text + "^50"),
                        new SolrQuery("titleText:"+ text + "*^40"),
                        new SolrQuery("allText:" + text + "^1.2"),
                        new SolrQuery("allText:" + text + "*^1.1"),
                        new SolrQuery("!boost b=typenumber")
                    });
                }
                else
                {
                    query = new SolrMultipleCriteriaQuery(new[]
                    {
                        new SolrQuery("titleText:"+ text + "^50"),
                        new SolrQuery("titleText:"+ text + "*^40"),
                        new SolrQuery("titleText:"+ text + "~2^1.1"),
                        new SolrQuery("allText:" + text + "^1.2"),
                        new SolrQuery("allText:" + text + "*^1.1"),
                        new SolrQuery("allText:\"" + text + "\"~1"),   //Fuzzy
                        new SolrQuery("allText2:" + text + ""), //Stemmer
                        new SolrQuery("!boost b=typenumber"),
                        //new SolrQuery("allText3:" + text)        //Fonetisk
                    });
                }
            }
            else query = SolrQuery.All;

            Log.Debug("Query: " + query.ToString());
            return query;
        }

        public void SetFacetOpenData()
        {
            FacetParameter dataAccess = Facets.Where(v => v.Name == "dataaccess").FirstOrDefault();
            if (dataAccess != null)
                Facets.Remove(dataAccess);
            if(CultureHelper.GetCurrentCulture() == Culture.EnglishCode)
                Facets.Add(new FacetParameter { Name = "dataaccess", Value = "Open data" });
            else
                Facets.Add(new FacetParameter { Name = "dataaccess", Value = "Åpne data" });
        }


        /// <summary>
        /// Builds a solr Filter query based on "Facets" parameter
        /// </summary>
        /// <returns></returns>
        public ICollection<ISolrQuery> BuildFilterQueries()
        {
            var queryList = new List<ISolrQuery>();

            Dictionary<string, string> facetQueries = new Dictionary<string, string>();

            var facets = Facets
                         .Where(f => !string.IsNullOrWhiteSpace(f.Value) && f.Name != "area")
                         .ToList();

            var facetsFylke = Facets
            .Where(f => !string.IsNullOrWhiteSpace(f.Value) && f.Name == "area")
            .Select(fa => fa.Value.Substring(0, 4))
            .Distinct()
            .ToList();

            if (facetsFylke.Count() > 0)
                facetQueries.Add("area", "");

            foreach (var facetFylke in facetsFylke)
            {
                var facetsKommune = Facets
                .Where(f => !string.IsNullOrWhiteSpace(f.Value) && f.Name == "area" && f.Value.Length > 4 && f.Value.StartsWith(facetFylke))
                .Distinct()
                .ToList();

                if (facetsKommune.Count > 0)
                {

                    foreach (var facet in facetsKommune)
                    {
                        var queryExpression = facetQueries["area"];
                        if (string.IsNullOrEmpty(queryExpression))
                        {
                            facetQueries["area"] = "area:\"" + facet.Value + "\"";
                        }
                        else
                        {
                            facetQueries["area"] = facetQueries["area"] + " OR  area:\"" + facet.Value + "\"";
                        }
                    }
                }
                else
                {
                    var queryExpression = facetQueries["area"];
                    if (string.IsNullOrEmpty(queryExpression))
                    {
                        facetQueries["area"] = "area:\"" + facetFylke + "\"";
                    }
                    else
                    {
                        facetQueries["area"] = facetQueries["area"] + " OR  area:\"" + facetFylke + "\"";
                    }
                }

            }


            foreach (var facet in facets)
            {
                if (!facetQueries.ContainsKey(facet.Name))
                    facetQueries.Add(facet.Name, "");

                var queryExpression = facetQueries[facet.Name];
                if (string.IsNullOrEmpty(queryExpression))
                {
                    facetQueries[facet.Name] = facet.Name + ":\"" + facet.Value + "\"";
                }
                else
                {
                    facetQueries[facet.Name] = facetQueries[facet.Name] + " OR " + facet.Name + ":\"" + facet.Value + "\"";
                }
            }

            foreach (var facetQuery in facetQueries)
            {
                queryList.Add(new SolrQuery(new LocalParams { { "tag", facetQuery.Key } } + facetQuery.Value));
            }

            return queryList;
        }

        public FacetParameters BuildFacetParameters()
        {
            var facetQueries = new List<ISolrFacetQuery>();

            List<string> facetsAdded = new List<string>();

            foreach (var facet in Facets)
            {
                if (facet.Value != null)
                {
                    if (!facetsAdded.Contains(facet.Name))
                        facetQueries.Add(new SolrFacetFieldQuery(new LocalParams { { "ex", facet.Name } } + facet.Name) { MinCount = 0, Limit = 550, Sort = false });

                    facetsAdded.Add(facet.Name);
                }
                else
                {
                    facetQueries.Add(new SolrFacetFieldQuery(facet.Name) { MinCount = 0, Limit = 550, Sort = false });
                }
            }

            var facets = new FacetParameters
            {
                Queries = facetQueries
            };

            return facets;
        }
    }
}