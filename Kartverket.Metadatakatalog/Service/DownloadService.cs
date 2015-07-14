﻿using System;
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
using System.Web.Configuration;

namespace Kartverket.Metadatakatalog.Service
{
    public class DownloadService
    {

        public OrderReceiptType Order(OrderType o)
        {


                var client = new HttpClient();
                client.BaseAddress = new Uri(WebConfigurationManager.AppSettings["DownloadUrl"]); // http://localhost:61236/
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
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