using System.Configuration;
using System.Web.Optimization;

namespace Geonorge.KartverketSharedMenu.Bundles
{
    public static class SharedMenu
    {
        /// <summary>
        ///     Add this bundle to your BundleCollection in order to regster it. At the buttom of the <head></head>, add
        ///     @Scripts.Render("~/bundles/shared-menu-scripts")
        /// </summary>
        public static Bundle Scripts => new ScriptBundle("~/bundles/shared-menu-scripts").Include(
            "~/KartverketSharedMenu/Scripts/angular.js",
            "~/KartverketSharedMenu/Scripts/jquery-1.11.3.min.js",
            "~/KartverketSharedMenu/Scripts/geonorge/app.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchOptions/" + ConfigurationManager.AppSettings["SearchOptionsFile"],
            "~/KartverketSharedMenu/Scripts/geonorge-top/menuTopController.js",
            "~/KartverketSharedMenu/Scripts/geonorge-top/searchTopController.js",
            "~/KartverketSharedMenu/Scripts/bootstrap.js",
            "~/KartverketSharedMenu/Scripts/ui-bootstrap.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/baseUrl.js",
            "~/KartverketSharedMenu/Scripts/geonorge-common/common.js",
            "~/Scripts/angular-ui/ui-bootstrap.min.js");

        /// <summary>
        ///     Add this bundle to your BundleCollection in order to regster it. At the top of the <head></head>, add
        ///     @Styles.Render("~/bundles/shared-menu-styles")
        /// </summary>
        public static Bundle Styles => new StyleBundle("~/bundles/shared-menu-styles")
            .Include("~/KartverketSharedMenu/Styles/bootstrap.css")
            .Include("~/KartverketSharedMenu/Styles/geonorge-top/menuTop.css")
            .Include("~/KartverketSharedMenu/Styles/geonorge-top/searchTop.css");
    }
}