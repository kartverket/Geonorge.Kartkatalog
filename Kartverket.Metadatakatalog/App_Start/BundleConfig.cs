using System.Configuration;
using System.Web;
using System.Web.Optimization;

namespace Kartverket.Metadatakatalog
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/shared-menu-scripts").Include(
            "~/KartverketSharedMenu/Scripts/angular.js",
            "~/KartverketSharedMenu/Scripts/jquery-1.11.3.min.js",
            "~/KartverketSharedMenu/Scripts/geonorge/app.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchOptions/" + ConfigurationManager.AppSettings["SearchOptionsFile"],
            "~/KartverketSharedMenu/Scripts/geonorge-top/menuTopController.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchTopController.js",
           // "~/KartverketSharedMenu/Scripts/bootstrap.js",
            "~/KartverketSharedMenu/Scripts/ui-bootstrap-0.14.3.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/baseUrl.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/common.js"

            ));


            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"
                        ));
            //"~/Scripts/jquery.typeahead.js"

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/bundles/shared-menu-styles")
           .Include("~/KartverketSharedMenu/Styles/bootstrap.css")
           .Include("~/KartverketSharedMenu/Styles/geonorge-top/menuTop.css")
           .Include("~/KartverketSharedMenu/Styles/geonorge-top/logoTop.css")
           .Include("~/KartverketSharedMenu/Styles/geonorge-top/searchTop.css")
           );


            bundles.Add(new StyleBundle("~/Content/download").Include(
                "~/Content/chosen.css",
                "~/Content/download.css",
                "~/Content/map-modal.css"));

            bundles.Add(new StyleBundle("~/Content/css_old").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/common.css",
                      "~/Content/navbar.css",
                      "~/Content/searchbar.css",
                      "~/Content/chosen.css",


                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/custom.css",
                      "~/Content/geonorge-colors.css"));
            // "~/Content/jquery.typeahead.css",
            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //          "~/Content/geonorge-default.css",
            //          "~/Content/site.css"));
            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
               "~/Scripts/visninger.js"));

            BundleTable.EnableOptimizations = false;
        }
    }
}
