using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kartverket.Metadatakatalog.Models
{
    public class OrderLineTypeExt : OrderLineType
    {
        public string OrderUrl { get; set; }
    }
}