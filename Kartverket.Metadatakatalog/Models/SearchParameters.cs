using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Service.Search;
using Microsoft.Extensions.Logging;
using Resources;
using SolrNet;
using SolrNet.Commands.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IAiService _aiService;
        private readonly ILogger<SearchParameters> _logger;
        public SearchParameters(IAiService aiService, ILogger<SearchParameters> logger)
        {
            Facets = new List<FacetParameter>();
            Offset = 1;
            Limit = 30;
            orderby = Models.OrderBy.score.ToString();
            _aiService = aiService;
            _logger = logger;
        }

        public SearchParameters()
        {
        }

        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string orderby { get; set; }
        public bool listhidden { get; set; }
        public List<FacetParameter> Facets { get; set; }

        public void AddComplexFacetsIfMissing()
        {
            AddDefaultFacetsIfMissing();
        }

        public void AddDefaultFacetsIfMissing()
        {
            List<FacetParameter> FacetList = new List<FacetParameter>();
            var facets = new List<string> { "type", "theme", "organizations", "nationalinitiative", "DistributionProtocols", "area", "dataaccess", "spatialscope" };

            foreach (var defaultFacet in facets)
            {
                if (Facets != null)
                {
                    var selectedFacets = Facets.Where(f => string.Equals(f.Name, defaultFacet, StringComparison.OrdinalIgnoreCase));
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
                    else
                    {
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

        public static string EscapeSolrQuery(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            // List of Solr special characters
            char[] specialChars = { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '*', '?', ':', '\\', '/' };
            var sb = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (specialChars.Contains(c))
                    sb.Append('\\');
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Builds a Solr query based on "Text" parameter
        /// </summary>
        /// <returns></returns>
        public ISolrQuery BuildQuery(ref QueryOptions options)
        {
            ISolrQuery query = null;
            try
            {
                var text = Text;
                if (!string.IsNullOrEmpty(text))
                {
                    text = text.Trim();
                    text = EscapeSolrQuery(text);

                    var titleText = text.Replace(" ", "*");
                    var textAll = text.Replace(" ", "*");
                    var queryString = "";

                    // Per-word fuzzy terms (e.g. "løs~1 jord~1"). Built here so the allText fuzzy
                    // clause below can be wrapped in parentheses and stay scoped to its field. Without
                    // the parentheses, Lucene binds "field:" to only the FIRST token of a multi-word
                    // query and the remaining words fall through to the default field (allText),
                    // carrying the clause's boost with them (e.g. uuid:^81 leaking onto allText:jord).
                    var fuzzyWords = string.Join(" ",
                        text.Split(' ').Where(x => !string.IsNullOrEmpty(x)).Select(x => x + "~1"));

                    if (text.Contains(" "))
                    {
                        var words = text.Split(' ');
                        string textOr = "";
                        for (int w = 0; w < words.Count(); w++)
                            if (!string.IsNullOrEmpty(words[w]))
                            {
                                textOr = textOr + "(type:dataset AND titleText:*" + words[w] + "*)^0.5";
                                textOr = textOr + " titleText:*" + words[w] + "*^0.4";
                                if (w != words.Count() - 1) textOr = textOr + " OR ";
                            }
                        queryString = textOr;
                    }

                    if (text.Trim().Length == 0)
                    {
                        var criteriaQueries = new List<ISolrQuery>
                        {
                        listhidden ? SolrQuery.All : new SolrQuery("!serie:*series_historic*"),
                        listhidden ? null : new SolrQuery("!serie:*series_time*"),
                        };
                        criteriaQueries.RemoveAll(q => q == null);
                        query = new SolrMultipleCriteriaQuery(criteriaQueries);
                    }
                    else
                    {
                        string vectorSearchString = null;
                        if (Text.Length > 2)
                        {
                            if (SimpleMetadataUtil.StaticUseVectorSearch)
                            {
                                var vectorSw = System.Diagnostics.Stopwatch.StartNew();
                                var embedding = _aiService.GetPredictions(Text);
                                vectorSw.Stop();
                                _logger?.LogInformation("Vertex embedding ms={EmbeddingMs} text={Text}", vectorSw.ElapsedMilliseconds, Text);
                                if (embedding != null && embedding.Length > 0)
                                {
                                    var formattedEmbedding = embedding.Select(f => f.ToString("F8", System.Globalization.CultureInfo.InvariantCulture));
                                    vectorSearchString = "[" + string.Join(",", formattedEmbedding) + "]";
                                }
                            }
                        }

                    var criteriaQueries = new List<ISolrQuery>
                    {
                        new SolrQuery("uuid:(" + text + ")^81"),
                        new SolrQuery("(type:dataset AND titleText:" + titleText + ")^79  titleText:" + titleText + "^78"),
                        new SolrQuery("(type:dataset AND titleText:" + titleText + "*)^77  titleText:" + titleText + "*^76"),
                        new SolrQuery("(type:dataset AND title_lowercase:*" + titleText + "*)^75  titleText:" + titleText + "*^74"),
                        new SolrQuery("(type:dataset AND titleText:*" + titleText + "*)^73  titleText:*" + titleText + "*^72"),
                        new SolrQuery("(type:dataset AND allText:*" + textAll + "*)^71 allText:*" + textAll + "*^70"),
                        !string.IsNullOrEmpty(queryString) ? new SolrQuery(queryString) : null,
                        new SolrQuery("allText:(" + fuzzyWords + ")^1"),
                        new SolrQuery("allText2:(" + text + ")"),
                        listhidden ? null : new SolrQuery("!serie:*series_historic*"),
                        listhidden ? null : new SolrQuery("!serie:*series_time*"),
                        new SolrQuery("!boost b=typenumber")
                    };

                    criteriaQueries.RemoveAll(q => q == null);
                    query = new SolrMultipleCriteriaQuery(criteriaQueries);

                    // --- VEKTORSØK FILTER LOGIKK ---
                    if (SimpleMetadataUtil.StaticUseVectorSearch && vectorSearchString != null)
                    {
                        // topK = kandidatpool (recall). Et lavt topK kapper hele resultatsettet
                        // når knn brukes i et filter, så vi holder poolen romslig og lar
                        // frange-terskelen (l) styre presisjonen.
                        string knnString = "{!knn f=vector topK=200}" + vectorSearchString;

                        // Solr skalerer KNN-scoren ulikt per similarityFunction, så en fast l blir
                        // feilkalibrert hvis kjernen ikke bruker dot_product. Vi uttrykker terskelen
                        // som cosinus (default 0.56) og oversetter til riktig frange-l for kjernens
                        // similarityFunction (default dot_product → l=0.78, euklidsk → l≈0.53).
                        // Leksikalske treff beholdes uansett via venstre side av OR, så terskelen
                        // rammer kun rene vektor-treff (f.eks. stedsnavn-kollisjonen «Kirkenes»).
                        double frangeThreshold = SimpleMetadataUtil.CosineToFrangeThreshold(
                            SimpleMetadataUtil.StaticVectorCosineThreshold,
                            SimpleMetadataUtil.StaticVectorSimilarityFunction);
                        string frangeL = frangeThreshold.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

                        var currentFilters = options.FilterQueries != null
                            ? options.FilterQueries.ToList()
                            : new List<ISolrQuery>();

                        // Hybrid: behold leksikalske treff (inkl. direkte uuid-oppslag), OG legg
                        // til semantisk nære dokumenter over terskel. uuid må stå i fq-en fordi
                        // et rent uuid-treff ellers filtreres bort før ^81-boosten i q rekker å
                        // virke (uuid ligger ikke i allText og er ikke vektor-nært).
                        // _query_-magifeltet lar oss neste en local-params-spørring (frange)
                        // inne i et boolsk uttrykk; {!...} kan ikke stå inline ellers.
                        currentFilters.Add(new SolrQuery(
                            "uuid:(" + text + ") OR allText:*" + textAll + "* OR _query_:\"{!frange l=" + frangeL + "}query($knn_q)\""));
                        options.FilterQueries = currentFilters;

                        // Legg til ExtraParams uten å slette det som eventuelt lå der
                        var extraParams = options.ExtraParams != null
                            ? new Dictionary<string, string>(options.ExtraParams.ToDictionary(p => p.Key, p => p.Value))
                            : new Dictionary<string, string>();
                        extraParams["knn_q"] = knnString;
                        // ReRank reordner de øverste reRankDocs leksikalske treffene etter
                        // vektor-likhet. reRankWeight * knnScore legges til original-scoren,
                        // og gir mer forutsigbar kontroll enn additivt bq mot ^70+-boostene.
                        // reRankWeight=80: ved 20 ble semantiske treff (f.eks. «Løsmasser» for
                        // «løs jord») liggende nederst fordi de leksikalske first-pass-scorene
                        // dominerte; 80 løfter dem til topp 2-4 uten å forstyrre presise treff
                        // (tittelmatch gir first-pass i hundreder, så rerank-bonusen blir støy der).
                        //
                        // ReRank er en relevans-operasjon og skal kun brukes når resultatet
                        // sorteres på score. Ved eksplisitt sortering (title, organization,
                        // dato) ville rq overstyre den valgte sorteringen, så da hopper vi
                        // over den. Hybrid-filteret over beholdes uansett (påvirker kun
                        // hvilke dokumenter som matcher, ikke rekkefølgen).
                        bool sortByScore = string.IsNullOrEmpty(orderby)
                            || orderby == Models.OrderBy.score.ToString();
                        if (sortByScore)
                        {
                            // reRankWeight er kalibrert mot dot_product-scoreskalaen [0,1]. Euklidsk
                            // gir et komprimert område (~[0.2,1] for unit-vektorer), så vekten bør
                            // kunne justeres per kjerne via config (default 80).
                            extraParams["rq"] = "{!rerank reRankQuery=$knn_q reRankDocs=200 reRankWeight="
                                + SimpleMetadataUtil.StaticVectorReRankWeight + "}";
                        }
                        options.ExtraParams = extraParams;
                        }
                    }
                }
                else
                {
                    var criteriaQueries = new List<ISolrQuery>
                    {
                     listhidden ? SolrQuery.All : new SolrQuery("!serie:*series_historic*"),
                     listhidden ? SolrQuery.All : new SolrQuery("!serie:*series_time*")
                    };
                    criteriaQueries.RemoveAll(q => q == null);
                    query = new SolrMultipleCriteriaQuery(criteriaQueries);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in BuildQuery: " + ex.Message);
            }

            return query;
        }



        public void SetFacetOpenData()
        {
            FacetParameter dataAccess = Facets.Where(v => v.Name == "dataaccess").FirstOrDefault();
            if (dataAccess != null)
                Facets.Remove(dataAccess);
            if (CultureHelper.GetCurrentCulture() == Culture.EnglishCode)
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
                    if(facet.Value != "High value dataset")
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