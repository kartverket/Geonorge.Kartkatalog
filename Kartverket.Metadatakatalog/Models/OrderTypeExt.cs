using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class OrderTypeExt : OrderType
    {
        public OrderLineTypeExt[] OrderLinesExt { get; set; }
    }
}