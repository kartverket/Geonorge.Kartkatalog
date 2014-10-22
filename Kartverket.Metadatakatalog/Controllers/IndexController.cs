using System;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Models;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using SolrNet.Exceptions;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class IndexController : Controller
    {
        public ActionResult Index()
        {

            var doc = new MetadataIndexDoc()
            {
                Uuid = "e461747b-8482-4b2b-88af-cf920570de2d",
                Title = "ABAS wms"
            };

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<MetadataIndexDoc>>();
            solr.Add(doc);
            try
            {
                solr.Commit();
            }
            catch (SolrConnectionException exception)
            {
                Console.WriteLine(exception.Message);
            }
            

            return View();
        }
    }
}