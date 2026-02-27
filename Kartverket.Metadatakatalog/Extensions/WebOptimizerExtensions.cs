// This file is temporarily disabled until WebOptimizer package is available for .NET 10
// TODO: Re-enable when WebOptimizer supports .NET 10 or find alternative bundling solution
namespace Kartverket.Metadatakatalog.Extensions
{
    /*
    public static class WebOptimizerExtensions
    {
        public static void ConfigureBundles(this IWebHostEnvironment env, IAssetPipeline pipeline)
        {
            // CSS Bundles
            pipeline.AddCssBundle("/css/styles.css",
                "/Content/bower_components/kartverket-felleskomponenter/assets/css/vendor.min.css",
                "/Content/bower_components/kartverket-felleskomponenter/assets/css/vendorfonts.min.css",
                "/Content/bower_components/kartverket-felleskomponenter/assets/css/main.min.css",
                "/Content/temp.css",
                "/Content/Blocks/SurveyBlock/SurveyBlockStyle.css");

            pipeline.AddCssBundle("/css/shopping-cart.css",
                "/Content/shopping-cart.css");

            // JavaScript Bundles
            pipeline.AddJavaScriptBundle("/js/bundle.js",
                "/Scripts/jquery-3.7.1.js", // Updated to specific version
                "/Content/bower_components/kartverket-felleskomponenter/assets/js/vendor.min.js",
                "/Content/bower_components/kartverket-felleskomponenter/assets/js/main.min.js");

            pipeline.AddJavaScriptBundle("/js/download.js",
                "/Scripts/vue-download.js");

            pipeline.AddJavaScriptBundle("/js/shopping-cart.js",
                "/Scripts/shopping-cart.js");

            // Disable optimization in development
            if (env.IsDevelopment())
            {
                pipeline.MinifyCss = false;
                pipeline.MinifyJavaScript = false;
                pipeline.EnableTagHelperBundling = false;
            }
        }
    }
    */
}