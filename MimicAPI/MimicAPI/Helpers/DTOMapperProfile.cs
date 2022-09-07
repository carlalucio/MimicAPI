using AutoMapper;
using MimicAPI.V1.Models;
using MimicAPI.V1.Models.DTO;

namespace MimicAPI.Helpers
{   /*AutoMapper é a biblioteca que nos ajuda a copiar um objeto de um tipo, para outro. Ele ignora atributos que não tenham equivalência
     ex = Palavra > PalavraDTO 
     Aqui no Profile vamos determinar quais objetos ele irá mapear
    Podse usar o .ForMember() para determinar a cópia de  atributos com nomes diferentes*/

    public class DTOMapperProfile: Profile
    {
        public DTOMapperProfile()
        {
            //primeiro recebe o objeto origem, dps o que vai receber os valores
            CreateMap<Palavra, PalavraDTO>();
            CreateMap<PaginationList<Palavra>, PaginationList<PalavraDTO>>();
        }
    }
}
