namespace MimicAPI.Helpers
{
    public class Paginacao
    {
        //classe para auxiliar passar no cabeçalho quantos registros tem, quantas páginas tem e todos os dados para o usuario navegar para o proximo registro
        public int NumeroPagina { get; set; }
        public int RegistroPorPagina{ get; set; }
        public int TotalRegistros { get; set; }
        public int TotalPaginas { get; set; }
    }
}
