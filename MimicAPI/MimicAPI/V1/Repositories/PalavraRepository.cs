using Microsoft.EntityFrameworkCore;
using MimicAPI.Database;
using MimicAPI.Helpers;
using MimicAPI.V1.Models;
using MimicAPI.V1.Repositories.Contracts;
using System;
using System.Linq;

namespace MimicAPI.V1.Repositories
{
    public class PalavraRepository : IPalavraRepository
    {
        private readonly MimicContext _banco;
        public PalavraRepository(MimicContext banco)
        {
            _banco = banco;

        }

        public PaginationList<Palavra> ObterPalavras(PalavraUrlQuery query)
        {
            //intancia um objeto do tipo PaginationList
            var lista = new PaginationList<Palavra>();

            //AsNoTracking para não armazenar em cache
            //usa o AsQueryble() pra transformar o objeto em IQueryable e ter acesso aos filtros
            var item = _banco.Palavras.AsNoTracking().AsQueryable();

            //se o parametro data tiver valor:
            if (query.Data.HasValue)
            {
                //se ele tiver valor o Where vai buscar todos os registros que foram criados ou atualizados a partir daquela data
                item = item.Where(a => a.Criado > query.Data.Value || a.Atualizado > query.Data.Value);
            }

            //verifica se o parametro pagNumero tem valor
            if (query.PagNumero.HasValue)
            {
                //variável que vai receber a quantidade total de registros do banco
                var quantidadeTotalRegistros = item.Count();

                //usa o Skip() para pular uma quantidade de registro e o Take() para pegar uma quantidade
                //skip vai calular a quantidade de itens q vai pular baseado na pagina solicitada e quantidade de itens por pagina
                //o take vai pegar a quantidade de registro solicitada após pular 
                //usa o .Value porque pode ser q a requisição não receba esse parâmetro
                item = item.Skip((query.PagNumero.Value - 1) * query.PagRegistro.Value).Take(query.PagRegistro.Value);

                //intância da classe paginação
                var paginacao = new Paginacao();
                paginacao.NumeroPagina = query.PagNumero.Value;
                paginacao.RegistroPorPagina = query.PagRegistro.Value;
                paginacao.TotalRegistros = quantidadeTotalRegistros;

                //usa o método Math.Ceiling() para arredondar para cima, pois  totalregistro / quantidadeExibida pode resultar em 2,1 por ex e precisar de 3 paginas para exibir  
                //faz o casting de (int) pq o método só recebe decimal ou double e casting (double) pq ele não define sozinho
                paginacao.TotalPaginas = (int)Math.Ceiling((double)quantidadeTotalRegistros / query.PagRegistro.Value);  
                
                lista.Paginacao = paginacao;
            }

            //passa o resultado com todas as palavras para a propriedade Results que é uma lista da classe PaginationList
            lista.Results.AddRange(item.ToList());

            return lista;

        }


        public Palavra Obter(int id)
        {
            //AsNoTracking para não armazenar em cache
            //FirstOrDefault() para retornar o primeiro elemento de uma sequência ou um valor padrão se o elemento não estiver lá.
            return _banco.Palavras.AsNoTracking().FirstOrDefault(a => a.Id == id);  
        }

        public void Cadastrar(Palavra palavra)
        {
            _banco.Palavras.Add(palavra);
            _banco.SaveChanges();
        }

        public void Aualizar(Palavra palavra)
        {
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();
        }

        

        public void Deletar(int id)
        {
            //utiliza o método obter da própria classe para atualizar o status do elemento
            var palavra = Obter(id);

            //exclusão lógica - vai inativar o elemento
            palavra.Ativo = false;
            _banco.Palavras.Update(palavra);
            _banco.SaveChanges();

        }

        
    }
}
