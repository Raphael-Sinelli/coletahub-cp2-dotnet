using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ColetaHub.Tests;

/// <summary>Monta objetos válidos para os testes, permitindo ajustar só o que interessa a cada cenário.</summary>
internal static class PacoteFabrica
{
    public static Pacote Valido(Action<Pacote>? ajustar = null)
    {
        var pacote = new Pacote
        {
            CodigoRastreio = "QB123456789BR",
            NomeDestinatario = "Maria Aparecida Souza",
            CpfDestinatario = "123.456.789-09",
            Marketplace = "Shopee",
            PesoKg = 1.5m,
            ValorDeclarado = 120m,
            DataRecebimento = DateTime.Today.AddDays(-1),
            Status = StatusPacote.AguardandoRetirada,
            TipoOperacao = TipoOperacao.RetiradaCliente
        };
        ajustar?.Invoke(pacote);
        return pacote;
    }

    public static InMemoryPacoteRepository RepositorioVazio() => new([]);

    public static InMemoryPacoteRepository RepositorioComSeed() => new(PacoteSeed.Criar());

    /// <summary>Controller pronto para uso fora do pipeline HTTP, com TempData funcional.</summary>
    public static T ComTempData<T>(T controller) where T : Controller
    {
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), new TempDataEmMemoria());
        return controller;
    }

    public static IMarketplaceService Marketplaces() => new MarketplaceService();

    private sealed class TempDataEmMemoria : ITempDataProvider
    {
        private IDictionary<string, object> _valores = new Dictionary<string, object>();

        public IDictionary<string, object> LoadTempData(HttpContext context) => _valores;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) => _valores = values;
    }
}
