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

        public IEnumerable<OrderReceiptType> Order()
        {

            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                client.BaseAddress = new Uri("http://download.dev.geonorge.no/"); //http://localhost:61236/
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                //client.DefaultRequestHeaders.Add("Accept", "*/*");
                //client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                //client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                
                
                //client.DefaultRequestHeaders.ConnectionClose = true;

                OrderType o = new OrderType();
                o.email = "dagolav@arkitektum.no";

                List<OrderLineType> orderLines = new List<OrderLineType>();

                OrderLineType oL1 = new OrderLineType();
                oL1.metadataUuid = "58e0dbf8-0d47-47c8-8086-107a3fa2dfa4";
                oL1.projections = new ProjectionType[] { new ProjectionType{ code = "UTM32" }};

                orderLines.Add(oL1);

                o.orderLines = orderLines.ToArray();

                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(o, 
                    new Newtonsoft.Json.JsonSerializerSettings 
                    { ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver() }
                    );

                HttpResponseMessage response = client.PostAsJsonAsync("api/order", json).Result;
                //HttpResponseMessage response = client.PostAsync("api/order", new StringContent(json)).Result;


                if (response.IsSuccessStatusCode)
                {
                var order = response.Content.ReadAsAsync
                <IEnumerable<OrderReceiptType>>().Result;
                return order;
                }

               
            }

            return null;
        }
    }
}