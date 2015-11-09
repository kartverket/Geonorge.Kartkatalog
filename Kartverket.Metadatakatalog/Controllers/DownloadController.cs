using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class DownloadController : Controller
    {
        // GET: Download
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Order(FormCollection order)
        {


            OrderType o = GetOrderForm(order);

            DownloadService download = new DownloadService();

            var model = download.Order(o);

            return View(model);
        }

        private OrderType GetOrderForm(FormCollection order)
        {
            OrderType o;
            List<OrderLineType> orderLines;

            List<string> uuids = new List<string>();

            foreach (var key in order.AllKeys)
            {
                string uuid = key.Substring(0, 36);
                if (!uuids.Contains(uuid))
                {
                    uuids.Add(uuid);
                }

            }


            o = new OrderType();

            if (!string.IsNullOrWhiteSpace(order["email"]))
                o.email = order["email"];

            orderLines = new List<OrderLineType>();


            foreach (var id in uuids)
            {

                OrderLineType oL = new OrderLineType();
                oL.metadataUuid = id;

                var projections = order[id + "-projection"];
                string[] projectionsList;
                if (projections != null)
                {
                    List<ProjectionType> projectionTypes = new List<ProjectionType>();
                    projectionsList = projections.Split(',');
                    foreach (var p in projectionsList)
                    {
                        projectionTypes.Add(new ProjectionType { code = p });
                    }
                    oL.projections = projectionTypes.ToArray();
                }

                var formats = order[id + "-formats"];
                string[] formatList;
                if (formats != null)
                {
                    List<FormatType> formatTypes = new List<FormatType>();
                    formatList = formats.Split(',');
                    foreach (var f in formatList)
                    {
                        formatTypes.Add(new FormatType { name = f });
                    }
                    oL.formats = formatTypes.ToArray();
                }

                var area = order[id + "-areas"];
                List<AreaType> areaList = new List<AreaType>();
                if (area != null)
                {
                    var areas = area.Split(',');

                    for (int j = 0; j < areas.Length; j++)
                    {
                        var areaType = areas[j].Split('_');

                        areaList.Add(new AreaType { type = areaType[0], code = areaType[1] });
                    }
                    oL.areas = areaList.ToArray();
                }

                string coordinates = order[id + "-coordinates"];
                if (!string.IsNullOrWhiteSpace(coordinates))
                    oL.coordinates = coordinates;

               orderLines.Add(oL);     

            }

            o.orderLines = orderLines.ToArray();

            return o;
        }
    }
}