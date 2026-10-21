using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using ColetaHub.Web.Controllers.Api;
using ColetaHub.Web.ModelBinding;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews(opcoes =>
    {
        // Mensagens de model binding em português (ex.: "O campo Peso (kg) deve ser um número.").
        var mensagens = opcoes.ModelBindingMessageProvider;
        mensagens.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
        mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
        mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
        // Este acessor recebe o valor digitado (vazio), não o nome do campo.
        mensagens.SetValueMustNotBeNullAccessor(_ => "Este campo é obrigatório.");
        mensagens.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} não foi informado.");
        mensagens.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado para {campo} é inválido.");
        mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
        mensagens.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
        mensagens.SetMissingRequestBodyRequiredValueAccessor(() => "O corpo da requisição é obrigatório.");

        // Decimais de formulário sempre no padrão brasileiro (recusa "2.5", que viraria 25).
        opcoes.ModelBinderProviders.Insert(0, new DecimalBrasileiroModelBinderProvider());
    })
    .AddJsonOptions(opcoes => opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(opcoes =>
    {
        // Títulos dos ProblemDetails da API em português.
        opcoes.ClientErrorMapping[StatusCodes.Status400BadRequest].Title = "Requisição inválida.";
        opcoes.ClientErrorMapping[StatusCodes.Status404NotFound].Title = "Recurso não encontrado.";
        opcoes.InvalidModelStateResponseFactory = contexto =>
        {
            var problema = contexto.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>()
                .CreateValidationProblemDetails(contexto.HttpContext, contexto.ModelState, title: PacotesApiController.TituloValidacao);
            return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
        };
    });

// Injeção de dependência: Singleton mantém os dados em memória durante toda a execução.
builder.Services.AddSingleton<IMarketplaceService, MarketplaceService>();
builder.Services.AddSingleton<IPacoteRepository>(_ => new InMemoryPacoteRepository(PacoteSeed.Criar()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ColetaHub API",
        Version = "v1",
        Description = "API REST do ponto de coleta ColetaHub (Checkpoint 2, FIAP). Persistência em memória."
    });
    var arquivoXml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    opcoes.IncludeXmlComments(arquivoXml);
});

// Cultura pt-BR fixa: datas dd/MM/yyyy, vírgula decimal e moeda R$ na leitura e na exibição.
var culturaPtBr = new CultureInfo("pt-BR");
builder.Services.Configure<RequestLocalizationOptions>(opcoes =>
{
    opcoes.DefaultRequestCulture = new RequestCulture(culturaPtBr);
    opcoes.SupportedCultures = [culturaPtBr];
    opcoes.SupportedUICultures = [culturaPtBr];
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opcoes =>
    {
        opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "ColetaHub API v1");
        opcoes.DocumentTitle = "ColetaHub API";
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization();

// Respostas 404/4xx/5xx sem corpo nas páginas MVC são re-executadas em /erro/{codigo}.
// A API fica de fora para continuar respondendo JSON (ProblemDetails).
app.UseWhen(
    contexto => !contexto.Request.Path.StartsWithSegments("/api"),
    ramo => ramo.UseStatusCodePagesWithReExecute("/erro/{0}"));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Exposto para os testes de integração (WebApplicationFactory<Program>).
public partial class Program;
