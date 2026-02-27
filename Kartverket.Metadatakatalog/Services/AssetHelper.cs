using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Kartverket.Metadatakatalog.Services
{
    public static class AssetHelper
    {
        // CSS file groups
        public static readonly string[] MainStyleFiles = new[]
        {
            "~/Content/bower_components/kartverket-felleskomponenter/assets/css/vendor.min.css",
            "~/Content/bower_components/kartverket-felleskomponenter/assets/css/vendorfonts.min.css",
            "~/Content/bower_components/kartverket-felleskomponenter/assets/css/main.min.css",
            "~/Content/temp.css",
            "~/Content/Blocks/SurveyBlock/SurveyBlockStyle.css"
        };

        public static readonly string[] ShoppingCartStyleFiles = new[]
        {
            "~/Content/shopping-cart.css"
        };

        // JavaScript file groups
        public static readonly string[] MainScriptFiles = new[]
        {
            "~/Scripts/jquery-3.7.1.js", // Specify exact version for production
            "~/Content/bower_components/kartverket-felleskomponenter/assets/js/vendor.min.js",
            "~/Content/bower_components/kartverket-felleskomponenter/assets/js/main.min.js"
        };

        public static readonly string[] DownloadScriptFiles = new[]
        {
            "~/Scripts/vue-download.js"
        };

        public static readonly string[] ShoppingCartScriptFiles = new[]
        {
            "~/Scripts/shopping-cart.js"
        };

        // Environment-based optimization settings
        public static bool EnableOptimizations(IWebHostEnvironment env)
        {
            return !env.IsDevelopment();
        }
    }
}