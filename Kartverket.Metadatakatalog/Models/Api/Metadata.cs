﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Metadata
    {
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string @Abstract { get; set; }
        public string Type { get; set; }
        public string Theme { get; set; }
        public string Organization { get; set; }
        public string OrganizationLogo { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DistributionUrl { get; set; }
        public string DistributionProtocol { get; set; }
        public string ShowDetailsUrl { get; set; }
        public string OrganizationUrl { get; set; }
        public string DistributionName { get; set; }

        public Metadata(SearchResultItem item, UrlHelper urlHelper)
        {


            Uuid = item.Uuid;
            Title = item.Title;
            Abstract = item.Abstract;
            Type = item.Type;
            Theme = item.Theme;
            Organization = item.Organization;
            OrganizationLogo = item.OrganizationLogoUrl;
            ThumbnailUrl = item.ThumbnailUrl;
            DistributionUrl = item.DistributionUrl;
            DistributionProtocol = item.DistributionProtocol;
            DistributionName = item.DistributionName;
            ShowDetailsUrl = urlHelper.Action("Index", "Metadata", new {uuid = item.Uuid}, urlHelper.RequestContext.HttpContext.Request.Url.Scheme);
            string s = new SeoUrl(item.Organization, "").Organization;
            OrganizationUrl = urlHelper.RequestContext.HttpContext.Request.Url.Scheme + "://" + urlHelper.RequestContext.HttpContext.Request.Url.Authority + "/metadata/" + s;
        }

        public static List<Metadata> CreateFromList(IEnumerable<SearchResultItem> items, UrlHelper urlHelper)
        {
            return items.Select(item => new Metadata(item, urlHelper)).ToList();
        }
    }
}