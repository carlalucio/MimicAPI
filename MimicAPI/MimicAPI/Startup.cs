using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimicAPI.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MimicAPI.V1.Repositories;
using MimicAPI.V1.Repositories.Contracts;
using AutoMapper;
using MimicAPI.Helpers;
using Microsoft.AspNetCore.Mvc.Versioning;
using MimicAPI.Helpers.Swagger;
using Microsoft.Extensions.PlatformAbstractions;
using System.IO;

namespace MimicAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //o AutoMapper é um biblioteca que mapeia um objeto de uma classe para outro 
            #region AutoMapper-Config
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DTOMapperProfile());
            });
            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            #endregion

            services.AddDbContext<MimicContext>(opt =>
            {
                opt.UseSqlite("Data Source=Database\\Mimic.db");
            });

            services.AddMvc();

            //passa como parametro a Interface do serviço e depois a Implementação para usar a injeção de dependência no Controller
            services.AddScoped<IPalavraRepository, PalavraRepository>();

            //adicionar servico de versaionamento de API
            services.AddApiVersioning(cfg =>
            {
                //essa função retorna no cabeçalho quais as versões suportadas e disponíveis quando for feita uma requisição
                cfg.ReportApiVersions = true;

                cfg.ApiVersionReader = new HeaderApiVersionReader("api-version"); //adiciona o leitor de versão pelo cabeçalho da requisão
                cfg.AssumeDefaultVersionWhenUnspecified = true;  //direciona o usuário para a versão padrão caso não seja especificado isso na url
                cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0); //indica a versão padrão 


            });

            //configurando o Swagger
            services.AddSwaggerGen(cfg =>
            {
                cfg.ResolveConflictingActions(apiDescription => apiDescription.First() );//se tiver conflito de rota ele pega o primeiro

                //1º coloca a versão, 2º instancia calsse Info() e coloca parametros {titulo, versao}               
                cfg.SwaggerDoc("v1.1", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MimicAPI - V1.1",
                    Version = "v1.1"
                });
                cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info(){
                Title = "MimicAPI - V1.0",
                Version = "v1.0"
                });
                cfg.SwaggerDoc("v2.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "MimicAPI - V2.0",
                    Version = "v2.0"
                });

                //criando variáveis que usam o platformServices para pegar o caminho e o nome do arquivo xml com os comentários
                var CaminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
                var NomeProjeto = $"{PlatformServices.Default.Application.ApplicationName}.xml";
                var CaminhoAquivoXMLComentario = Path.Combine(CaminhoProjeto, NomeProjeto);

                //configuração para o swagger usar os comentários feitos no controller
                cfg.IncludeXmlComments(CaminhoAquivoXMLComentario);


                //configuração para selecionar qual versão quer exibir
                cfg.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });


                cfg.OperationFilter<ApiVersionOperationFilter>(); //passa a classe para filtrar a versão


            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseStatusCodePages();
            //app.UseHttpsRedirection();
            app.UseMvc();

            app.UseSwagger(); // cria o arquivo base --> /swagger/v1/swagger.json
            
            //gera a interface gráfica do Swagger e passa a configuração qual será o endpoint e o nome da API
            app.UseSwaggerUI(cfg=>
            {
                cfg.SwaggerEndpoint("/swagger/v1.1/swagger.json", "MimicAPI V1.1");
                cfg.SwaggerEndpoint(" /swagger/v1.0/swagger.json", "MimicAPI V1.0");
                cfg.SwaggerEndpoint("/swagger/v2.0/swagger.json", "MimicAPI V2.0");
                cfg.RoutePrefix = String.Empty; //configuração para que ao acessar a raiz da api direcione para o swaggerUI
            }); 
        }
    }
}
