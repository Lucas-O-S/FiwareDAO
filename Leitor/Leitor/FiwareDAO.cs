using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
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

        public static async Task<List<Leitura>> VerificarDados(string host, string lamp, int n)
        {
            List<Leitura> leituras = new List<Leitura>();
            var options = new RestClientOptions($"http://{host}:8666")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/STH/v1/contextEntities/type/Lamp/id/urn:ngsi-ld:Lamp:{lamp}/attributes/temperature?lastN={n}", Method.Get);
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                JsonDocument doc = JsonDocument.Parse(response.Content);
                var contexto = doc.RootElement.GetProperty("contextResponses");
                foreach (var contextResponse in contexto.EnumerateArray())
                {
                    // Acessando o objeto 'contextElement' dentro de cada 'contextResponse'
                    var contextElement = contextResponse.GetProperty("contextElement");

                    // Acessando o array 'attributes' dentro de 'contextElement'
                    if (contextElement.TryGetProperty("attributes", out var attributes))
                    {
                        // Iterando sobre o array 'attributes' para encontrar o atributo 'temperature'
                        foreach (var attribute in attributes.EnumerateArray())
                        {
                            // Verificando se o nome do atributo é 'temperature'
                            if (attribute.GetProperty("name").GetString() == "temperature")
                            {
                                // Acessando o array 'values' dentro do atributo 'temperature'
                                var values = attribute.GetProperty("values");

                                // Iterando sobre o array 'values' para extrair os dados de temperatura
                                foreach (var value in values.EnumerateArray())
                                {
                                    // Extraindo os dados do valor
                                    DateTime recvTime = DateTime.Parse(value.GetProperty("recvTime").GetString());
                                    float attrValue = value.GetProperty("attrValue").GetSingle();
                                    leituras.Add(new Leitura(attrValue, recvTime));
                                }
                            }
                        }
                    }
                }
                return leituras;

            }
            return null;

        }

        public static async Task<Leitura> Ler(string host, string lamp)
        {

            Leitura leitura;
            var options = new RestClientOptions($"http://{host}:1026")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/v2/entities/urn:ngsi-ld:Lamp:{lamp}/attrs/temperature", Method.Get);
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");
            request.AddHeader("accept", "application/json");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                JsonDocument doc = JsonDocument.Parse(response.Content);
                float temp = doc.RootElement.GetProperty("value").GetSingle();
                string dataString = doc.RootElement.GetProperty("metadata").GetProperty("TimeInstant").GetProperty("value").ToString();
                DateTime data = DateTime.Parse(dataString);
                leitura = new Leitura(temp, data);
                return leitura;
            }
            return leitura = new Leitura();
        }

        public static async Task CriarLamp(string host, string lamp)
        {
            await Provisioning(host, lamp);
            await Registering(host, lamp);
            await Subscribe(host, lamp);

        }

        //cria a lamp escolhida
        private static async Task Provisioning(string host, string lamp)
        {
            var options = new RestClientOptions($"http://{host}:4041")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/iot/devices", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");

            var body = new
            {
                devices = new[]
                {
                    new
                    {
                        device_id = $"lamp{lamp}",
                        entity_name = $"urn:ngsi-ld:Lamp:{lamp}",
                        entity_type = "Lamp",
                        protocol = "PDI-IoTA-UltraLight",
                        transport = "MQTT",
                        commands = new[]
                        {
                            new { name = "on", type = "command" },
                            new { name = "off", type = "command" }
                        },
                        attributes = new[]
                        {
                            new { object_id = "s", name = "state", type = "Text" },
                            new { object_id = "t", name = "temperature", type = "float" }
                        }
                    }
                }
            };

            // Convertendo o objeto para JSON
            string jsonBody = JsonSerializer.Serialize(body);

            // Adicionando o corpo JSON na requisição
            request.AddStringBody(jsonBody, DataFormat.Json);

            // Enviando a requisição
            var response = await client.ExecuteAsync(request);

            // Exibindo o conteúdo da resposta
            Console.WriteLine(response.Content);
        }


        //Registra a lamp
        private static async Task Registering(string host, string lamp)
        {
            var options = new RestClientOptions($"http://{host}:1026")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/v2/registrations", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");

            // Construindo o corpo JSON com objetos anônimos
            var body = new
            {
                description = "Lamp Commands",
                dataProvided = new
                {
                    entities = new[]
                    {
                new
                {
                    id = $"urn:ngsi-ld:Lamp:{lamp}",
                    type = "Lamp"
                }
            },
                    attrs = new[] { "on", "off" }
                },
                provider = new
                {
                    http = new
                    {
                        url = $"http://{host}:4041"
                    },
                    legacyForwarding = true
                }
            };

            // Convertendo o objeto para JSON
            string jsonBody = JsonSerializer.Serialize(body);

            // Adicionando o corpo JSON na requisição
            request.AddStringBody(jsonBody, DataFormat.Json);

            // Enviando a requisição
            var response = await client.ExecuteAsync(request);

            // Exibindo o conteúdo da resposta
            Console.WriteLine(response.Content);
        }


        //Inscreve a lamp ao sth comet
        private static async Task Subscribe(string host, string lamp)
        {
            var options = new RestClientOptions($"http://{host}:1026")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/v2/subscriptions", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");

            // Criando o corpo JSON com objetos anônimos
            var body = new
            {
                description = "Notify STH-Comet of all Motion Sensor count changes",
                subject = new
                {
                    entities = new[]
                    {
                new
                {
                    id = $"urn:ngsi-ld:Lamp:{lamp}",
                    type = "Lamp"
                }
            },
                    condition = new
                    {
                        attrs = new[] { "temperature" }
                    }
                },
                notification = new
                {
                    http = new
                    {
                        url = $"http://{host}:8666/notify"
                    },
                    attrs = new[] { "temperature" },
                    attrsFormat = "legacy"
                }
            };

            // Convertendo o objeto para JSON
            string jsonBody = JsonSerializer.Serialize(body);

            // Adicionando o corpo JSON na requisição
            request.AddStringBody(jsonBody, DataFormat.Json);

            // Enviando a requisição
            var response = await client.ExecuteAsync(request);

            // Exibindo o conteúdo da resposta
            Console.WriteLine(response.Content);
        }




    }

}

