// Uncomment the following to provide samples for PageResult<T>. Must also add the Microsoft.AspNet.WebApi.OData
// package to your project.
////#define Handle_PageResultOfT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
#if Handle_PageResultOfT
using System.Web.Http.OData;
#endif

namespace Kartverket.Metadatakatalog.Areas.HelpPage
{
    /// <summary>
    /// Use this class to customize the Help Page.
    /// For example you can set a custom <see cref="System.Web.Http.Description.IDocumentationProvider"/> to supply the documentation
    /// or you can provide the samples for the requests/responses.
    /// </summary>
    public static class HelpPageConfig
    {
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Kartverket.Metadatakatalog.Areas.HelpPage.TextSample.#ctor(System.String)",
            Justification = "End users may choose to merge this string with existing localized resources.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly",
            MessageId = "bsonspec",
            Justification = "Part of a URI.")]
        public static void Register(HttpConfiguration config)
        {
            //// Uncomment the following to use the documentation from XML documentation file.
            config.SetDocumentationProvider(new XmlDocumentationProvider(HttpContext.Current.Server.MapPath("~/App_Data/XmlDocument.xml")));

            //// Uncomment the following to use "sample string" as the sample for all actions that have string as the body parameter or return type.
            //// Also, the string arrays will be used for IEnumerable<string>. The sample objects will be serialized into different media type 
            //// formats by the available formatters.
            Kartverket.Metadatakatalog.Models.Api.SearchResult s2 = new Kartverket.Metadatakatalog.Models.Api.SearchResult(new Kartverket.Metadatakatalog.Models.SearchResult() { Limit = 10, NumFound=1, Offset =0 });
            s2.Results = new List<Metadatakatalog.Models.Api.Metadata>();
            Metadatakatalog.Models.Api.Metadata m = new Metadatakatalog.Models.Api.Metadata() { Abstract = "Adresser med geografisk punkt og kretsinformasjon. Kopiert ukentlig fra Matikkelen.", Organization = "Kartverket", Title = "Adresse", Theme = "Annen", IsOpenData = false, DistributionProtocol = "WWW:DOWNLOAD-1.0-http--download", DistributionUrl = "https://download.geonorge.no/skdl2/nl2prot/nl2", Type = "dataset", OrganizationUrl = "https://kartkatalog.geonorge.no/metadata/kartverket", OrganizationLogo = "https://register.geonorge.no/data/organizations/971040238_kv_logo100.png", ShowDetailsUrl = "https://kartkatalog.geonorge.no/metadata/kartverket/adresse/0674c9d0-a72e-43b9-a9d4-327f374b2a32", ThumbnailUrl = "http://www.geonorge.no:80/geonetwork/srv/eng/resources.get?uuid=0674c9d0-a72e-43b9-a9d4-327f374b2a32&amp;fname=Matrikkelen_s.png", Uuid = "0674c9d0-a72e-43b9-a9d4-327f374b2a32" };
            s2.Results.Add(m);

            s2.Facets = new List<Metadatakatalog.Models.Api.Facet>();
            Metadatakatalog.Models.Api.Facet f = new Metadatakatalog.Models.Api.Facet();
            f.FacetField = "theme";
            f.FacetResults = new List<Metadatakatalog.Models.Api.Facet.FacetValue>();
            f.FacetResults.Add(new Metadatakatalog.Models.Api.Facet.FacetValue() { Name = "Annen", Count = 1 });
            s2.Facets.Add(f);
            Metadatakatalog.Models.Api.Facet f2 = new Metadatakatalog.Models.Api.Facet();
            f2.FacetField = "type";
            f2.FacetResults = new List<Metadatakatalog.Models.Api.Facet.FacetValue>();
            f2.FacetResults.Add(new Metadatakatalog.Models.Api.Facet.FacetValue() { Name = "dataset", Count = 1 });
            s2.Facets.Add(f2);
            Metadatakatalog.Models.Api.Facet f3 = new Metadatakatalog.Models.Api.Facet();
            f3.FacetField = "organization";
            f3.FacetResults = new List<Metadatakatalog.Models.Api.Facet.FacetValue>();
            f3.FacetResults.Add(new Metadatakatalog.Models.Api.Facet.FacetValue() { Name = "Kartverket", Count = 1 });
            s2.Facets.Add(f3);

            config.SetSampleObjects(new Dictionary<Type, object>
            {
                {typeof(string), "sample string"},
                {typeof(Kartverket.Metadatakatalog.Models.Api.SearchResult), s2}
            });

            // Extend the following to provide factories for types not handled automatically (those lacking parameterless
            // constructors) or for which you prefer to use non-default property values. Line below provides a fallback
            // since automatic handling will fail and GeneratePageResult handles only a single type.
#if Handle_PageResultOfT
            config.GetHelpPageSampleGenerator().SampleObjectFactories.Add(GeneratePageResult);
#endif

            // Extend the following to use a preset object directly as the sample for all actions that support a media
            // type, regardless of the body parameter or return type. The lines below avoid display of binary content.
            // The BsonMediaTypeFormatter (if available) is not used to serialize the TextSample object.
            config.SetSampleForMediaType(
                new TextSample("Binary JSON content. See http://bsonspec.org for details."),
                new MediaTypeHeaderValue("application/bson"));

            //// Uncomment the following to use "[0]=foo&[1]=bar" directly as the sample for all actions that support form URL encoded format
            //// and have IEnumerable<string> as the body parameter or return type.
            //config.SetSampleForType("[0]=foo&[1]=bar", new MediaTypeHeaderValue("application/x-www-form-urlencoded"), typeof(IEnumerable<string>));
            //config.SetSampleForType("[0]=foo&[1]=bar", new MediaTypeHeaderValue("application/json"), typeof(IEnumerable<string>));
            //// Uncomment the following to use "1234" directly as the request sample for media type "text/plain" on the controller named "Values"
            //// and action named "Put".
            //config.SetSampleRequest("/api/search/?text=adresse", new MediaTypeHeaderValue("application/x-www-form-urlencoded"), "APISearch", "Get");
            config.SetSampleRequest("/api/search/?text=adresse&facets[0]name=organization&facets[0]value=Kartverket", new MediaTypeHeaderValue("application/x-www-form-urlencoded"), "APISearch", "Get");
            //config.SetSampleRequest("/api/search/?text=adresse&facets[0]name=organization&facets[0]value=Kartverket&Offset=10&Limit=15", new MediaTypeHeaderValue("application/x-www-form-urlencoded"), "APISearch", "Get");
            //// Uncomment the following to use the image on "../images/aspNetHome.png" directly as the response sample for media type "image/png"
            //// on the controller named "Values" and action named "Get" with parameter "id".

            //var urlHelper = new System.Web.Mvc.UrlHelper(HttpContext.Current.Request.RequestContext);
           

            //// Uncomment the following to correct the sample request when the action expects an HttpRequestMessage with ObjectContent<string>.
            //// The sample will be generated as if the controller named "Values" and action named "Get" were having string as the body parameter.
            //config.SetActualRequestType(typeof(string), "Values", "Get");

            //// Uncomment the following to correct the sample response when the action returns an HttpResponseMessage with ObjectContent<string>.
            //// The sample will be generated as if the controller named "Values" and action named "Post" were returning a string.
            //config.SetActualResponseType(typeof(string), "Values", "Post");
        }

#if Handle_PageResultOfT
        private static object GeneratePageResult(HelpPageSampleGenerator sampleGenerator, Type type)
        {
            if (type.IsGenericType)
            {
                Type openGenericType = type.GetGenericTypeDefinition();
                if (openGenericType == typeof(PageResult<>))
                {
                    // Get the T in PageResult<T>
                    Type[] typeParameters = type.GetGenericArguments();
                    Debug.Assert(typeParameters.Length == 1);

                    // Create an enumeration to pass as the first parameter to the PageResult<T> constuctor
                    Type itemsType = typeof(List<>).MakeGenericType(typeParameters);
                    object items = sampleGenerator.GetSampleObject(itemsType);

                    // Fill in the other information needed to invoke the PageResult<T> constuctor
                    Type[] parameterTypes = new Type[] { itemsType, typeof(Uri), typeof(long?), };
                    object[] parameters = new object[] { items, null, (long)ObjectGenerator.DefaultCollectionSize, };

                    // Call PageResult(IEnumerable<T> items, Uri nextPageLink, long? count) constructor
                    ConstructorInfo constructor = type.GetConstructor(parameterTypes);
                    return constructor.Invoke(parameters);
                }
            }

            return null;
        }
#endif
    }
}