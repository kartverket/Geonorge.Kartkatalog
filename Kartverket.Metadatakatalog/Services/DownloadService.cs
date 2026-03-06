using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kartverket.Metadatakatalog.Service
{
    public class DownloadService
    {
        private readonly ILogger<DownloadService> _logger;
        private readonly IConfiguration _configuration;

        public DownloadService(IConfiguration configuration, ILogger<DownloadService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OrderReceiptType> Order(OrderType o, string orderUrl)
        {
                if (string.IsNullOrEmpty(orderUrl))
                orderUrl = _configuration["DownloadUrl"] + "api/order";

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

                _logger.LogInformation("Sending request to download api: {OrderUrl}\r\n{Content}", orderUrl, content);

                HttpResponseMessage response = client.PostAsync(orderUrl, content).Result;
                _logger.LogInformation("Response code: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var resultAsString = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Response:\r\n{ResultAsString}", resultAsString);
                    var order = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderReceiptType>(resultAsString);

                return order;
                }

            return null;
        }
    }
}