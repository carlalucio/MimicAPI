using MimicAPI.Helpers;
using MimicAPI.V1.Models;


namespace MimicAPI.V1.Repositories.Contracts
{
    //interface para diminuir o acoplamento
    public interface IPalavraRepository
    {
        PaginationList<Palavra> ObterPalavras(PalavraUrlQuery query);
        Palavra Obter(int id);
        void Cadastrar(Palavra palavra);
        void Aualizar( Palavra palavra);
        void Deletar(int id);

    }
}
