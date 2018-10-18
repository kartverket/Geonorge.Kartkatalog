using SolrNet;
using System.Collections.Generic;
using System.Linq;
using System;
using SolrNet.Commands.Parameters;
using Kartverket.Metadatakatalog.Helpers;
using Kartverket.Metadatakatalog.Models.Translations;

namespace Kartverket.Metadatakatalog.Models.Article
{
    public struct OrderBy
    {
        private string value;
        private OrderBy(string value)
        {
            this.value = value;
        }


        public override string ToString()
        {
            return this.value;
        }

    }

    public class SearchParameters
    {
        public SearchParameters()
        {
            Offset = 1;
            Limit = 30;
            orderby = Models.OrderBy.score.ToString();
        }

        public string Text { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string orderby { get; set; }


        /// <summary>
        /// Gets a sort order based on "Orderby" parameter
        /// </summary>
        /// <returns></returns>
        public SortOrder[] OrderBy()
        {
            var order = new[] { new SortOrder("score", Order.DESC) };
            if (orderby == "StartPublish")
            {
                order = new[] { new SortOrder("StartPublish", Order.DESC) };
            }
            return order;
        }

        /// <summary>
        /// Builds a Solr query based on "Text" parameter
        /// </summary>
        /// <returns></returns>
        public ISolrQuery BuildQuery()
        {
            log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            ISolrQuery query;
            if (!string.IsNullOrEmpty(Text))
            {
                Text = Text.Replace(":", " ");
                Text = Text.Replace("!", " ");
                Text = Text.Replace("{", " ");
                Text = Text.Replace("}", " ");
                Text = Text.Replace("[", " ");
                Text = Text.Replace("]", " ");
                Text = Text.Replace("(", " ");
                Text = Text.Replace(")", " ");
                Text = Text.Replace("^", " ");
                Text = Text.Replace("-", "\\-");

                if (Text.Trim().Length == 0) query = SolrQuery.All;
                else if (Text.Trim().Length < 5)
                {
                    query = new SolrMultipleCriteriaQuery(new[]
                    {
                        new SolrQuery("TitleText:"+ Text + "^50"),
                        new SolrQuery("TitleText:"+ Text + "*^40"),
                        new SolrQuery("allText:" + Text + "^1.2"),
                        new SolrQuery("allText:" + Text + "*^1.1"),
                        new SolrQuery("!boost b=typenumber")
                    });
                }
                else
                {
                    query = new SolrMultipleCriteriaQuery(new[]
                    {
                        new SolrQuery("TitleText:"+ Text + "^50"),
                        new SolrQuery("TitleText:"+ Text + "*^40"),
                        new SolrQuery("TitleText:"+ Text + "~2^1.1"),
                        new SolrQuery("allText:" + Text + "^1.2"),
                        new SolrQuery("allText:" + Text + "*^1.1"),
                        new SolrQuery("allText:\"" + Text + "\"~1"),   //Fuzzy
                        new SolrQuery("allText2:" + Text + ""), //Stemmer
                        new SolrQuery("!boost b=typenumber")
                    });
                }
            }
            else query = SolrQuery.All;

            Log.Debug("Query: " + query.ToString());
            return query;
        }
    }
}