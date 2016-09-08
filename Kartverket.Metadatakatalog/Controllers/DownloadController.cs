using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kartverket.Metadatakatalog.Service;
using Kartverket.Metadatakatalog.Models;

namespace Kartverket.Metadatakatalog.Controllers
{
    public class DownloadController : Controller
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // GET: Download
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Order(FormCollection order)
        {
            OrderTypeExt o = GetOrderForm(order);
            List<OrderReceiptType> model = new List<OrderReceiptType>();
            DownloadService download = new DownloadService();

            var OrderUrls = o.OrderLinesExt.Select(oU => oU.OrderUrl).Distinct().ToList();

            foreach (var orderUrl in OrderUrls)
            {
                List<OrderLineTypeExt> oLinesExt = o.OrderLinesExt.Where(l => l.OrderUrl == orderUrl).ToList();
                OrderType orderInfo = new OrderType();
                orderInfo.email = o.email;
                List<OrderLineType> oLines = new List<OrderLineType>();
                foreach (var line in oLinesExt)
                {
                    OrderLineType oLine = new OrderLineType();
                    oLine.areas = line.areas;
                    oLine.coordinates = line.coordinates;
                    oLine.formats = line.formats;
                    oLine.metadataUuid = line.metadataUuid;
                    oLine.projections = line.projections;
                    oLines.Add(oLine);
                }
                orderInfo.orderLines = oLines.ToArray();
                try
                {
                    OrderReceiptType result = download.Order(orderInfo, orderUrl);
                    model.Add(result);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }

            return View(model);
        }

        private OrderTypeExt GetOrderForm(FormCollection order)
        {
            OrderTypeExt o;
            List<OrderLineTypeExt> orderLines;

            List<string> uuids = new List<string>();

            foreach (var key in order.AllKeys)
            {
                if (key.Length > 36)
                {

                    string uuid = key.Substring(0, 36);
                    if (!uuids.Contains(uuid))
                    {
                        uuids.Add(uuid);
                    }
                }
            }


            o = new OrderTypeExt();

            if (!string.IsNullOrWhiteSpace(order["email"]))
                o.email = order["email"];

            orderLines = new List<OrderLineTypeExt>();


            foreach (var id in uuids)
            {

                OrderLineTypeExt oL = new OrderLineTypeExt();
                oL.metadataUuid = id;

                oL.OrderUrl = order[id + "-orderUrl"];

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

                string coordinates = order[id + "-coordinates"];
                if (!string.IsNullOrWhiteSpace(coordinates))
                    oL.coordinates = coordinates;


                var area = order[id + "-areas"];
                List<OrderAreaType> areaList = new List<OrderAreaType>();
                if (area != null)
                {
                    var areas = area.Split(',');

                    for (int j = 0; j < areas.Length; j++)
                    {
                        var areaType = areas[j].Split('_');
                        if(areaType.Count() == 2)
                            areaList.Add(new OrderAreaType { type = areaType[0], code = areaType[1] });
                    }
                    oL.areas = areaList.ToArray();
                }



                orderLines.Add(oL);

            }

            o.orderLines = orderLines.ToArray();
            o.OrderLinesExt = orderLines.ToArray();

            return o;
        }
    }
}