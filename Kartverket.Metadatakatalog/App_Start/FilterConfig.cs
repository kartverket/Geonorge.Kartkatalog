using System.Web;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.ActionFilters;

namespace Kartverket.Metadatakatalog
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new WhitespaceFilter());
        }
    }
}
