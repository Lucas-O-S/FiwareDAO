using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leitor
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string host = "4.228.64.5";
            int n = 10;
            string lamp = "10x";

            if (await FiwareDAO.VerificarServer(host) == true)
            {
                Console.WriteLine("Conectado ao Server");
                await FiwareDAO.VerificarDados(host, lamp,n);
            }

            else Console.WriteLine("Não foi possivel se conectar ao server");
            Console.ReadKey();
        }

    }
}
