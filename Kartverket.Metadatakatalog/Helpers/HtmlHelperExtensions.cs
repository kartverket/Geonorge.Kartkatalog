using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Kartverket.Metadatakatalog.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static string ApplicationVersionNumber(this HtmlHelper helper)
        {
            string versionNumber = WebConfigurationManager.AppSettings["BuildVersionNumber"];
            return versionNumber;
        }
    }
}