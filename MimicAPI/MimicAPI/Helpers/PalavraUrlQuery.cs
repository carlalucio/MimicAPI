using System;

namespace MimicAPI.Helpers
{
    //classe para auxiliar na inclusão de parâmetros no método ObterTodas()
    public class PalavraUrlQuery
    {
        public DateTime? Data { get; set; }
        public int? PagNumero { get; set; }
        public int? PagRegistro { get; set; }
    }
}
