using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kartverket.Metadatakatalog.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Service
{
    public class DownloadService
    {

        public OrderReceiptType Order()
        {


                var client = new HttpClient();
                client.BaseAddress = new Uri("http://download.dev.geonorge.no/"); //http://localhost:61236/
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                OrderType o = new OrderType();
                o.email = "dagolav@arkitektum.no";

                List<OrderLineType> orderLines = new List<OrderLineType>();

                OrderLineType oL1 = new OrderLineType();
                oL1.metadataUuid = "58e0dbf8-0d47-47c8-8086-107a3fa2dfa4";
                oL1.projections = new ProjectionType[] { new ProjectionType{ code = "UTM32" }};
                oL1.areas = new AreaType[] { new AreaType { type="kommune",  name = "Oslo" } };

                orderLines.Add(oL1);

                o.orderLines = orderLines.ToArray();

                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(o, 
                    new Newtonsoft.Json.JsonSerializerSettings 
                    { ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver() }
                    );

                HttpResponseMessage response = client.PostAsJsonAsync("api/order", json).Result;


                if (response.IsSuccessStatusCode)
                {

                    var result = response.Content.ReadAsAsync<object>().Result;
                    var order = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderReceiptType>(result.ToString());

                return order;
                }

            return null;
        }
    }
}