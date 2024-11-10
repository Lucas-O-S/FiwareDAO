using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Leitor
{
    public static class FiwareDAO
    {
        public static async Task<bool> VerificarServer(string host)
        {
            try
            {
                var options = new RestClientOptions($"http://{host}:4041")
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest("/iot/about", Method.Get);
                RestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public static async Task VerificarDados(string host, string lamp, int n)
        {
            var options = new RestClientOptions($"http://{host}:8666")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/STH/v1/contextEntities/type/Lamp/id/urn:ngsi-ld:Lamp:{lamp}/attributes/temperature?lastN={n}", Method.Get);
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
        }

    }
}
