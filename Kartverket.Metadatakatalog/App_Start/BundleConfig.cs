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
            //***************** complete bundle - remember to update this as well as the separate bundles ****************//

            bundles.Add(new ScriptBundle("~/bundles/kartkatalog-scripts").Include(
            "~/KartverketSharedMenu/Scripts/angular-1.4.9.min.js",
            "~/KartverketSharedMenu/Scripts/jquery-1.11.3.js",
            "~/KartverketSharedMenu/Scripts/geonorge/app.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchOptions/" + ConfigurationManager.AppSettings["SearchOptionsFile"],
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchTopController.js",
            "~/KartverketSharedMenu/Scripts/ui-bootstrap-1.1.0.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/baseUrl.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/common.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/menuTopController.js",
            "~/Scripts/bootstrap.js",
            "~/Scripts/site.js",
            "~/Scripts/jquery-ui.js",
            "~/Scripts/respond.js",
            "~/Scripts/js.cookie.js",
            "~/Scripts/jquery.validate*",
            "~/Scripts/chosen.jquery.js",
            "~/Scripts/easyXDM.js",
            "~/Scripts/visninger.js",
            "~/Scripts/init.js",
            "~/Scripts/shopping-cart-jscookie.js",
            "~/Scripts/site.js"
            ));


            bundles.Add(new StyleBundle("~/Content/kartkatalog-styles").Include(
                "~/KartverketSharedMenu/Styles/bootstrap.css",
                "~/KartverketSharedMenu/Styles/geonorge-top/menuTop.css",
                "~/KartverketSharedMenu/Styles/geonorge-top/logoTop.css",
                "~/KartverketSharedMenu/Styles/geonorge-top/searchTop.css",
                "~/Content/site.css",
                "~/Content/jquery-ui.css",
                "~/Content/jquery-ui.structure.css",
                "~/Content/jquery-ui.theme.css",
                "~/Content/themes/base/core.css",
                "~/Content/themes/base/datepicker.css",
                "~/Content/themes/base/theme.css",
                "~/Content/custom.css",
                "~/Content/ie-10-only.css",
                "~/Content/ie-11-only.css",
                "~/Content/geonorge-colors.css",
                "~/Content/searchresult-layout.css",
                "~/Content/custom-icons.css",
                "~/Content/font-awesome.min.css",
                "~/Content/forms.css",
                "~/Content/chosen.css",
                "~/Content/download.css",
                "~/Content/map-modal.css",
                "~/Content/shopping-cart.css"
           ));


            bundles.Add(new StyleBundle("~/Content/bower_components/kartverket-felleskomponenter/assets/css/styles").Include(
                "~/Content/bower_components/kartverket-felleskomponenter/assets/css/vendor.min.css",
                "~/Content/bower_components/kartverket-felleskomponenter/assets/css/main.min.css"
                ));

            bundles.Add(new ScriptBundle("~/Content/bower_components/kartverket-felleskomponenter/assets/js/scripts").Include(
               "~/Content/bower_components/kartverket-felleskomponenter/assets/js/vendor.min.js",
               "~/Content/bower_components/kartverket-felleskomponenter/assets/js/main.min.js",
               "~/Scripts/site.js",
               "~/Scripts/visninger.js",
               "~/Scripts/easyXDM.min.js",
               "~/Scripts/download.js"
           ));

            //***************** separate bundles - used in other applications ****************//

            bundles.Add(new ScriptBundle("~/bundles/shared-menu-scripts").Include(
            "~/KartverketSharedMenu/Scripts/angular-1.4.9.min.js",
            "~/KartverketSharedMenu/Scripts/jquery-1.11.3.js",
            "~/KartverketSharedMenu/Scripts/geonorge/app.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchOptions/" + ConfigurationManager.AppSettings["SearchOptionsFile"],
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchTopController.js",
            "~/KartverketSharedMenu/Scripts/ui-bootstrap-1.1.0.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/baseUrl.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/common.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/menuTopController.js"
            ));


            bundles.Add(new StyleBundle("~/bundles/shared-menu-styles")
           .Include("~/KartverketSharedMenu/Styles/bootstrap.css")
           .Include("~/KartverketSharedMenu/Styles/geonorge-top/menuTop.css")
           .Include("~/KartverketSharedMenu/Styles/geonorge-top/logoTop.css")
           .Include("~/KartverketSharedMenu/Styles/geonorge-top/searchTop.css")
           );


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/jquery-ui.css",
                      "~/Content/jquery-ui.structure.css",
                      "~/Content/jquery-ui.theme.css",
                      "~/Content/themes/base/core.css",
                      "~/Content/themes/base/datepicker.css",
                      "~/Content/themes/base/theme.css",
                      "~/Content/custom.css",
                      "~/Content/ie-10-only.css",
                      "~/Content/ie-11-only.css",
                      "~/Content/geonorge-colors.css",
                      "~/Content/searchresult-layout.css",
                      "~/Content/custom-icons.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/forms.css",
                      "~/Content/chosen.css",
                      "~/Content/download.css",
                      "~/Content/map-modal.css"
                      ));


            bundles.Add(new ScriptBundle("~/bundles/js").Include(
               "~/Scripts/bootstrap.js",
               "~/Scripts/site.js",
               "~/Scripts/jquery-ui-datepicker.js",
               "~/Scripts/respond.js",
               "~/Scripts/jquery.cookie.js",
               "~/Scripts/jquery.validate*",
               "~/Scripts/chosen.jquery.js",
               "~/Scripts/easyXDM.js",
               "~/Scripts/visninger.js",
               "~/Scripts/init.js"));

            bundles.Add(new StyleBundle("~/Content/shopping-cart").Include(
                "~/Content/shopping-cart.css"));

            bundles.Add(new ScriptBundle("~/bundles/shopping-cart").Include(
                "~/Scripts/shopping-cart.js"));

            BundleTable.EnableOptimizations = true;
        }
    }
}
