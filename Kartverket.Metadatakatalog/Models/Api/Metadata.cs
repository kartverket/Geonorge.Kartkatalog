using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Models.Api
{
    public class Metadata
    {
        /// <summary>
        /// The uniqueidentifier
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The abstract
        /// </summary>
        public string @Abstract { get; set; }
        /// <summary>
        /// The type of metadata
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The theme
        /// </summary>
        public string Theme { get; set; }
        /// <summary>
        /// The owner of the metadata
        /// </summary>
        public string Organization { get; set; }
        /// <summary>
        /// The logo for the organization
        /// </summary>
        public string OrganizationLogo { get; set; }
        /// <summary>
        /// Illustrative image of the metadata
        /// </summary>
        public string ThumbnailUrl { get; set; }
        /// <summary>
        /// Url for downloading dataset/service
        /// </summary>
        public string DistributionUrl { get; set; }
        /// <summary>
        /// The protocol used for downloading
        /// </summary>
        public string DistributionProtocol { get; set; }
        /// <summary>
        /// Url to metadata details page
        /// </summary>
        public string ShowDetailsUrl { get; set; }
        /// <summary>
        /// Url to metadata owner organization details page
        /// </summary>
        public string OrganizationUrl { get; set; }
        /// <summary>
        /// The layer for services
        /// </summary>
        public string DistributionName { get; set; }

        /// <summary>
        /// True if one of the nationalinitiativs(Samarbeid og lover) is "Åpne data"
        /// </summary>
        public bool IsOpenData { get; set; }
        /// <summary>
        /// Url for legend/drawing rules
        /// </summary>
        public string LegendDescriptionUrl { get; set; }
        /// <summary>
        /// Url for productsheet
        /// </summary>
        public string ProductSheetUrl { get; set; }
        /// <summary>
        /// Url for detailed spesifications
        /// </summary>
        public string ProductSpecificationUrl { get; set; }
        /// <summary>
        /// Services for dataset
        /// </summary>
        public List<string> DatasetServices { get; set; }

        public Metadata() { 
        }
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
            if (urlHelper != null)
            {
                ShowDetailsUrl = WebConfigurationManager.AppSettings["KartkatalogenUrl"] + "metadata/uuid/" + item.Uuid;
                string s = new SeoUrl(item.Organization, "").Organization;
                OrganizationUrl = WebConfigurationManager.AppSettings["KartkatalogenUrl"] + "metadata/" + s;
            }
           
            if (item.NationalInitiative != null && item.NationalInitiative.Contains("Åpne data"))
                IsOpenData = true;
            else IsOpenData = false;

            LegendDescriptionUrl = item.LegendDescriptionUrl;
            ProductSheetUrl = item.ProductSheetUrl;
            ProductSpecificationUrl = item.ProductSpecificationUrl;
            DatasetServices = item.DatasetServices;
        }

        public static List<Metadata> CreateFromList(IEnumerable<SearchResultItem> items, UrlHelper urlHelper)
        {
            return items.Select(item => new Metadata(item, urlHelper)).ToList();
        }
    }
}