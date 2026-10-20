using ColetaHub.Web.Controllers.Api;
using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ColetaHub.Tests;

public class PacotesApiControllerTests
{
    private readonly IPacoteRepository _repositorio = PacoteFabrica.RepositorioComSeed();

    // ValidationProblem() resolve o ProblemDetailsFactory pelo HttpContext, por isso os serviços do MVC são registrados.
    private PacotesApiController CriarController() => new(_repositorio, PacoteFabrica.Marketplaces())
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection().AddLogging().AddControllers().Services.BuildServiceProvider()
            }
        }
    };

    [Fact]
    public void ObterPorId_Existente_Retorna200()
    {
        var resultado = CriarController().ObterPorId(1);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(1, Assert.IsType<Pacote>(ok.Value).Id);
    }

    [Fact]
    public void ObterPorId_Inexistente_Retorna404()
    {
        Assert.IsType<NotFoundResult>(CriarController().ObterPorId(9999).Result);
    }

    [Fact]
    public void Criar_Valido_Retorna201ComRotaDoNovoPacote()
    {
        var resultado = CriarController().Criar(PacoteFabrica.Valido(p => p.Id = 500));

        var criado = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        Assert.Equal(nameof(PacotesApiController.ObterPorId), criado.ActionName);
        var pacote = Assert.IsType<Pacote>(criado.Value);
        Assert.NotEqual(500, pacote.Id);
        Assert.Equal(pacote.Id, criado.RouteValues!["id"]);
    }

    [Fact]
    public void Criar_MarketplaceInvalido_Retorna400()
    {
        var resultado = CriarController().Criar(PacoteFabrica.Valido(p => p.Marketplace = "Loja X"));

        var erro = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.IsType<ValidationProblemDetails>(erro.Value);
    }

    [Fact]
    public void Atualizar_Existente_Retorna204()
    {
        var pacote = _repositorio.ObterPorId(1)!;
        pacote.Status = StatusPacote.Retirado;
        pacote.DataRetirada = DateTime.Today;

        Assert.IsType<NoContentResult>(CriarController().Atualizar(1, pacote));
        Assert.Equal(StatusPacote.Retirado, _repositorio.ObterPorId(1)!.Status);
    }

    [Fact]
    public void Atualizar_Inexistente_Retorna404()
    {
        Assert.IsType<NotFoundResult>(CriarController().Atualizar(9999, PacoteFabrica.Valido()));
    }

    [Fact]
    public void Remover_Existente_Retorna204EDepois404()
    {
        var controller = CriarController();

        Assert.IsType<NoContentResult>(controller.Remover(2));
        Assert.IsType<NotFoundResult>(controller.Remover(2));
    }

    [Fact]
    public void Pesquisar_PorStatus_RetornaSomenteOsDoStatus()
    {
        var resultado = CriarController().Pesquisar(new FiltroPacote { Status = StatusPacote.AguardandoColeta });

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var pacotes = Assert.IsAssignableFrom<IEnumerable<Pacote>>(ok.Value);
        Assert.All(pacotes, p => Assert.Equal(StatusPacote.AguardandoColeta, p.Status));
    }
}
