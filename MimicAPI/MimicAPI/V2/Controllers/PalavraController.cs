using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MimicAPI.V2.Controllers
{
    // /api/v2.o/palavras => a versao será definida na url da chamada

    [ApiController]
    // [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    //inidica que essa versão está obsoleta
    [ApiVersion("2.0", Deprecated = true)]
    public class PalavraController : ControllerBase
    {
       /// <summary>
       /// Operação que pega o nome da Versão 
       /// </summary>
       /// <returns></returns>
        [MapToApiVersion("2.0")]
        [HttpGet("")]        
        public string ObterTodas()
        {
            return "Versão 2.0";
        }
    }
}
