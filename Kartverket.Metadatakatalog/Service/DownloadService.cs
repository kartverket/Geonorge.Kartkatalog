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
using System.Web.Configuration;

namespace Kartverket.Metadatakatalog.Service
{
    public class DownloadService
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public OrderReceiptType Order(OrderType o, string orderUrl)
        {
                if (string.IsNullOrEmpty(orderUrl))
                orderUrl = WebConfigurationManager.AppSettings["DownloadUrl"] + "api/order";

            //Disable SSL sertificate errors
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
            delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true; // **** Always accept
                        };

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(o, 
                    new Newtonsoft.Json.JsonSerializerSettings 
                    { ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver() }
                    );
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Log.Info($"Sending request to download api: {orderUrl}\r\n{content}");

                HttpResponseMessage response = client.PostAsync(orderUrl, content).Result;
                Log.Info($"Response code: " + response.StatusCode);

                if (response.IsSuccessStatusCode)
                {

                    var result = response.Content.ReadAsAsync<object>().Result;
                    var resultAsString = result.ToString();
                    Log.Info($"Response:\r\n{resultAsString}");
                    var order = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderReceiptType>(resultAsString);

                return order;
                }

            return null;
        }
    }
}