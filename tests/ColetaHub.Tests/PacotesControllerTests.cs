using ColetaHub.Web.Controllers;
using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ColetaHub.Tests;

public class PacotesControllerTests
{
    private readonly IPacoteRepository _repositorio = PacoteFabrica.RepositorioComSeed();

    private PacotesController CriarController() =>
        PacoteFabrica.ComTempData(new PacotesController(_repositorio, PacoteFabrica.Marketplaces()));

    [Fact]
    public void Create_ModelStateInvalido_RetornaViewComDropdownRepopulado()
    {
        var controller = CriarController();
        controller.ModelState.AddModelError("Pacote.CodigoRastreio", "Informe o código de rastreio.");
        var totalAntes = _repositorio.ObterTodos().Count;

        var resultado = controller.Create(new PacoteFormViewModel { Pacote = PacoteFabrica.Valido() });

        var view = Assert.IsType<ViewResult>(resultado);
        var modelo = Assert.IsType<PacoteFormViewModel>(view.Model);
        Assert.Equal(6, modelo.Marketplaces.Count());
        Assert.Contains(modelo.Marketplaces, item => item.Value == "Mercado Livre");
        Assert.Equal(totalAntes, _repositorio.ObterTodos().Count);
    }

    [Fact]
    public void Create_Valido_SalvaERedirecionaParaDetailsComMensagem()
    {
        var controller = CriarController();

        var resultado = controller.Create(new PacoteFormViewModel { Pacote = PacoteFabrica.Valido() });

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(PacotesController.Details), redirecionamento.ActionName);
        var id = Assert.IsType<int>(redirecionamento.RouteValues!["id"]);
        Assert.Equal("QB123456789BR", _repositorio.ObterPorId(id)!.CodigoRastreio);
        Assert.NotNull(controller.TempData[PacotesController.ChaveSucesso]);
    }

    [Fact]
    public void Create_MarketplaceForaDoCatalogo_RetornaViewComErro()
    {
        var controller = CriarController();

        var resultado = controller.Create(new PacoteFormViewModel { Pacote = PacoteFabrica.Valido(p => p.Marketplace = "Loja Inventada") });

        Assert.IsType<ViewResult>(resultado);
        Assert.True(controller.ModelState.ContainsKey("Pacote.Marketplace"));
    }

    [Fact]
    public void Create_CodigoDuplicado_RetornaViewComErro()
    {
        var controller = CriarController();

        var resultado = controller.Create(new PacoteFormViewModel { Pacote = PacoteFabrica.Valido(p => p.CodigoRastreio = "QB482917365BR") });

        Assert.IsType<ViewResult>(resultado);
        Assert.True(controller.ModelState.ContainsKey("Pacote.CodigoRastreio"));
    }

    [Fact]
    public void Delete_IdInexistente_RetornaNotFound()
    {
        var resultado = CriarController().Delete(9999);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public void Details_IdInexistente_RetornaNotFound()
    {
        Assert.IsType<NotFoundResult>(CriarController().Details(9999));
    }

    [Fact]
    public void Edit_IdInexistente_RetornaNotFound()
    {
        Assert.IsType<NotFoundResult>(CriarController().Edit(9999));
    }

    [Fact]
    public void Delete_IdExistente_MostraTelaDeConfirmacaoSemRemover()
    {
        var resultado = CriarController().Delete(1);

        var view = Assert.IsType<ViewResult>(resultado);
        Assert.Equal(1, Assert.IsType<Pacote>(view.Model).Id);
        Assert.NotNull(_repositorio.ObterPorId(1));
    }

    [Fact]
    public void DeleteConfirmed_RemoveERedirecionaParaIndex()
    {
        var controller = CriarController();

        var resultado = controller.DeleteConfirmed(1);

        var redirecionamento = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal(nameof(PacotesController.Index), redirecionamento.ActionName);
        Assert.Null(_repositorio.ObterPorId(1));
        Assert.NotNull(controller.TempData[PacotesController.ChaveSucesso]);
    }

    [Fact]
    public void DeleteConfirmed_IdInexistente_RedirecionaComErro()
    {
        var controller = CriarController();

        var resultado = controller.DeleteConfirmed(9999);

        Assert.IsType<RedirectToActionResult>(resultado);
        Assert.NotNull(controller.TempData[PacotesController.ChaveErro]);
    }

    [Fact]
    public void Edit_IdDaRotaDiferenteDoFormulario_RetornaBadRequest()
    {
        var resultado = CriarController().Edit(1, new PacoteFormViewModel { Pacote = PacoteFabrica.Valido(p => p.Id = 2) });

        Assert.IsType<BadRequestResult>(resultado);
    }

    [Fact]
    public void Search_FiltraPorStatusEMantemFiltrosNoModelo()
    {
        var pesquisa = new PacoteSearchViewModel { Status = StatusPacote.AguardandoRetirada, Texto = "a" };

        var view = Assert.IsType<ViewResult>(CriarController().Search(pesquisa));

        var modelo = Assert.IsType<PacoteSearchViewModel>(view.Model);
        Assert.Equal(StatusPacote.AguardandoRetirada, modelo.Status);
        Assert.Equal("a", modelo.Texto);
        Assert.NotEmpty(modelo.Resultados);
        Assert.All(modelo.Resultados, p => Assert.Equal(StatusPacote.AguardandoRetirada, p.Status));
        Assert.NotEmpty(modelo.Marketplaces);
    }

    [Fact]
    public void Search_IntervaloInvertido_RetornaErroSemResultados()
    {
        var controller = CriarController();
        var pesquisa = new PacoteSearchViewModel { RecebidoDe = DateTime.Today, RecebidoAte = DateTime.Today.AddDays(-5) };

        var view = Assert.IsType<ViewResult>(controller.Search(pesquisa));

        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(Assert.IsType<PacoteSearchViewModel>(view.Model).Resultados);
    }

    [Theory]
    [InlineData("codigo", false)]
    [InlineData("status", true)]
    [InlineData("data", false)]
    public void Index_OrdenaPelaColunaEscolhida(string coluna, bool decrescente)
    {
        var view = Assert.IsType<ViewResult>(CriarController().Index(coluna, decrescente));
        var pacotes = Assert.IsAssignableFrom<IReadOnlyList<Pacote>>(view.Model);

        IComparable Chave(Pacote p) => coluna switch
        {
            "codigo" => p.CodigoRastreio,
            "status" => p.Status,
            _ => p.DataRecebimento
        };

        var esperado = decrescente ? pacotes.OrderByDescending(Chave) : pacotes.OrderBy(Chave);
        Assert.Equal(esperado.Select(Chave), pacotes.Select(Chave));
    }
}
