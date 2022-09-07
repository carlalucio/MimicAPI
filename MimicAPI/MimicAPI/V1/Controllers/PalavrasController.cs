using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MimicAPI.Helpers;
using MimicAPI.V1.Models;
using MimicAPI.V1.Models.DTO;
using MimicAPI.V1.Repositories.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MimicAPI.V1.Controllers
{
    [ApiController]
    // [Route("api/v{version:apiVersion}/[controller]")] --> recebe a versão dentro do template da  URL 
    
    [Route("api/[controller]")] // --> pode acessar a versão passando a QueryString --> /api/palavras?api-version=1
    [ApiVersion("1.0", Deprecated = true)]

    //indica que nesse controller tem uma subversão onde eu vou indicar os metodos dessa nova versão  com o atributo MapToApiVersion()
    [ApiVersion("1.1")]


    public class PalavrasController : ControllerBase
    {
        //injeção de dependência da Interface ao invés de injetar diretamente a dependência do banco      
        private readonly IPalavraRepository _repository;
        private readonly IMapper _mapper;

          public PalavrasController(IPalavraRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        //rota padrão para acessar todos elementos cadastrados  url --> api/palavras
        //será utilizado na instalação do app para baixar todas as palavras cadastradas
        //essa rota tbm pode receber parametro do tipo DateTime para filtrar palavras criadas ou alteradas baseado na data de atualização
        //utiliza a querystring no padrao americano na url --> api/palavras?data=2022-09-01
        //passa como parâmetro a rota + o nome da rota
        //comentáro de /// é usado para documentação

        /// <summary>
        /// Operação que pega do banco de dados todas as Palavras existentes
        /// </summary>
        /// <param name="query"> Filtros de pesquisa</param>
        /// <returns>Listagem de Palavras</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")] // indica q esse método é da versão 1.0 e 1.1
        [HttpGet("", Name ="ObterTodas")]

        //Um método action é utilizado para processar uma requisição HTTP e ele pode conter ou não conter argumentos
        //opcionalmente, essa rota pode receber Data, numero da pagina e quantidade de registro por pagina - parametros da Classe PalavraUrlQuuery
        //parametro do tipo DateTime para filtrar palavras criadas ou alteradas baseado na data de atualização
        //utiliza a querystring no padrao americano na url --> api/palavras?Data=2022-09-01
        //pode enviar o numero de paginas ou não --> api/palavras?PagNumero=1&PagRegistro=2
        //Usa o [FromQuery] para indicar que vai vir da QueryString
        public ActionResult ObterTodas([FromQuery]PalavraUrlQuery query)
        {
            var item = _repository.ObterPalavras(query);

            //primeiro verifica se existem registros
            if (item.Results.Count == 0)
                return NotFound();

            //método para criar links nas palavras e de paginação
            PaginationList<PalavraDTO> lista = CriarLinksListPalavraDTO(query, item);

            return Ok(lista);
        }


        /// <summary>
        /// Operação que pega uma única Palavra da base de dados
        /// </summary>
        /// <param name="id">Código identificador da Palavra</param>
        /// <returns>Um objeto de Plavra</returns>      
        
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")] // indica q esse método é da versão 1.0 e 1.1

        [HttpGet("{id}", Name = "ObterPalavra")] //passar a rota com id do elemento a obter, como parâmetro -- /api/palavras/1  //dar um nome para a rota pro controller poder usar o Url.Link    
        public ActionResult Obter(int id)
        {
            //verifica se o elemento existe através do método Obter da Interface/Repository ou retonar um Http Status Code 404
            var obj = _repository.Obter(id);
            if (obj == null)
                return NotFound();
            //aqui o automapper vai copiar todos os atributos de palavra em uma palavraDTO, eú só preencher o atributo Link
            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(obj);

                     
            palavraDTO.Links.Add(
                //Url.Link recebe o nome da rota e o objeto - no caso o id do objeto na URI
                new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }),"GET")
                ); 
            palavraDTO.Links.Add(
                new LinkDTO("update",Url.Link( "AtualizarPalavra", new { id = palavraDTO.Id }),"PUT")
                );
            palavraDTO.Links.Add(
                new LinkDTO("delete", Url.Link("ExcluirPalavra", new { id = palavraDTO.Id }), "DELETE")
                );
            return Ok(palavraDTO);

        }

        /// <summary>
        /// Operação que realiza o Cadastro da Palavra
        /// </summary>
        /// <param name="palavra">Um objeto Palavra</param>
        /// <returns>Um objeto Palavra com seu Id</returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")] // indica q esse método é da versão 1.0 e 1.1

        [Route("")] //passar a rota padrão -- /api/palavras/ (POST: id, nome, pontuação, criação)
        [HttpPost]        
        public ActionResult Cadastrar([FromBody]Palavra palavra)
        {
            //primeira validação: verifica se tem os dados para criação da entidade no banco
            if (palavra == null)
                return BadRequest();

            //usa o ModelState pra validar o objeto de entrada -  se os dados não foram preenchidos
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            palavra.Ativo = true;
            palavra.Criado = DateTime.Now;            
            _repository.Cadastrar(palavra);

            PalavraDTO  palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                //Url.Link recebe o nome da rota e o objeto - no caso o id do objeto na URI
                new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")
                );
            //retona o http status 201 (parametros: url , objeto)
            //usa o $ para interpolação
            return Created($"/api/palavras/{palavra.Id}", palavraDTO);
        }

        /// <summary>
        /// Operação que realiza a substituição de dados de uma Palavra específica
        /// </summary>
        /// <param name="id">Codigo identificador da Palavra a ser alterada</param>
        /// <param name="palavra">Objeto Palavra com dados para alteração</param>
        /// <returns></returns>
        [MapToApiVersion("1.0")]
        [MapToApiVersion("1.1")] // indica q esse método é da versão 1.0 e 1.1
       
        [HttpPut("{id}", Name = "AtualizarPalavra")]  //passar a rota com o id do elemento a atualizar e o nome da rota como parâmetro-- /api/palavras/1 (PUT: id, nome, pontuação, criação)        
        public ActionResult Atualizar(int id, [FromBody]Palavra palavra)
        {
            //verifica se o elemento existe, usando o método Obter, e retonar um Http Status Code 404 caso não exista            
            var obj =  _repository.Obter(id);
            if (obj == null)
                return NotFound();

            // verifica se tem os dados para criação da entidade no banco
            if (palavra == null)
                return BadRequest();

            //usa o ModelState pra validar o objeto de entrada -  se os dados não foram preenchidos
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            //conserva as propriedades que não serão alteradas pelo usuário
            palavra.Id = id;
            palavra.Ativo = obj.Ativo;
            palavra.Criado = obj.Criado;
            palavra.Atualizado = DateTime.Now;
            _repository.Aualizar(palavra);


            PalavraDTO palavraDTO = _mapper.Map<Palavra, PalavraDTO>(palavra);
            palavraDTO.Links.Add(
                //Url.Link recebe o nome da rota e o objeto - no caso o id do objeto na URI
                new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavraDTO.Id }), "GET")
                );
            return Ok();
        }

        /// <summary>
        /// Operação que desativa uma Palavra do Sistema
        /// </summary>
        /// <param name="id">Código identificador da palavra</param>
        /// <returns></returns>
        [MapToApiVersion("1.1")] // indica q esse método é da versão 1.1
        [HttpDelete("{id}", Name = "ExcluirPalavra")] //passar a rota com o id do elemento a excluir e o nome da rota como parâmetro -- /api/palavras/1 (DELETE)
        public ActionResult Deletar(int id)
        {
            //verifica se o elemento existe e retonar um Http Status Code 404
            var palavra = _repository.Obter(id);
            if (palavra == null)
                return NotFound();

            _repository.Deletar(id);

            //retorna Http Status Code 204
            return NoContent();
        }


        //método usado no ObterTodas() para criar links nas palavras e na pagina
        private PaginationList<PalavraDTO> CriarLinksListPalavraDTO(PalavraUrlQuery query, PaginationList<Palavra> item)
        {
            //o mapper vai converter uma lista de palavras em palavraDTO
            var lista = _mapper.Map<PaginationList<Palavra>, PaginationList<PalavraDTO>>(item);

            //para cada palavra dessa lista ele vai adicionar o link 
            foreach (var palavra in lista.Results)
            {
                palavra.Links = new List<LinkDTO>();
                palavra.Links.Add(new LinkDTO("self", Url.Link("ObterPalavra", new { id = palavra.Id }), "GET"));
            }
            //links q serão exibidos referentes a página
            //passa o nome da rota e a query, pq se tiver parâmetro no método ele já reconhece, se não, ignora  a query
            lista.Links.Add(new LinkDTO("self", Url.Link("ObterTodas", query), "GET"));

            //verifica se foi passado o parametro Paginação
            if (item.Paginacao != null)
            {
                //retorna no cabeçalho da requisição  usando o parametro X-Pagination
                //usa o JsonCovert.SerializeObject() para transformar o objeto em string
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(item.Paginacao));

                //lógica para verificar se existe próxima página, se existir ele cria o link para proxima pagina                
                if (query.PagNumero + 1 <= item.Paginacao.TotalPaginas)
                {
                    //instância de PalavraQuery que vai receber o numero da página se for passado lá no parametro
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero + 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("next", Url.Link("ObterTodas", queryString), "GET"));
                }

                //lógica para verificar se existe pagina anterior, se existir ele cria o link para pagina anterior
                if (query.PagNumero - 1 > 0)
                {
                    //instância de PalavraQuery que vai receber o numero da página se for passado lá no parametro
                    var queryString = new PalavraUrlQuery() { PagNumero = query.PagNumero - 1, PagRegistro = query.PagRegistro, Data = query.Data };
                    lista.Links.Add(new LinkDTO("prev", Url.Link("ObterTodas", queryString), "GET"));
                }
            }

            return lista;
        }
    }
}
