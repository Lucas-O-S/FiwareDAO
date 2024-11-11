using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace Leitor
{
    public struct Leitura
    {
        public float temperatura { get;}
        public DateTime data {get;}
        public Leitura(float temperatura, DateTime data)
        {
            this.data = data;
            this.temperatura = temperatura;
        }
    }
}
