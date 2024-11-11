using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leitor
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string host = "4.228.64.5";
            int n = 10;
            string lamp = "02y";

            if (await FiwareDAO.VerificarServer(host) == true)
            {
                while (true)
                {
                    Leitura l = await FiwareDAO.Ler(host,lamp);
                    Console.WriteLine($"Temperatura: {l.temperatura} \n Data: {l.data}");
                    Thread.Sleep(2000);


                }


            }

            else Console.WriteLine("Não foi possivel se conectar ao server");
            Console.ReadKey();
        }

    }
}
