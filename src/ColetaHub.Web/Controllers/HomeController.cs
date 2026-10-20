using System.Diagnostics;
using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ColetaHub.Web.Controllers;

/// <summary>Painel inicial, página Sobre e páginas de erro amigáveis.</summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    private readonly IPacoteRepository _repositorio;

    public HomeController(IPacoteRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public IActionResult Index()
    {
        var pacotes = _repositorio.ObterTodos();
        var noPonto = pacotes.Where(p => p.EstaNoPonto).ToList();

        var painel = new HomeDashboardViewModel
        {
            TotalPacotes = pacotes.Count,
            PacotesNoPonto = noPonto.Count,
            PacotesFrageis = noPonto.Count(p => p.Fragil),
            PacotesParados = noPonto.Count(p => p.DiasNoPonto > Pacote.DiasLimiteNoPonto),
            ValorSobGuarda = noPonto.Sum(p => p.ValorDeclarado),
            PorStatus = Enum.GetValues<StatusPacote>()
                .Select(status => new ContagemStatus(status, pacotes.Count(p => p.Status == status)))
                .ToList(),
            PorMarketplace = pacotes
                .GroupBy(p => p.Marketplace)
                .Select(g => new ContagemMarketplace(g.Key, g.Count()))
                .OrderByDescending(c => c.Quantidade)
                .ThenBy(c => c.Marketplace)
                .ToList(),
            Parados = noPonto
                .Where(p => p.DiasNoPonto > Pacote.DiasLimiteNoPonto)
                .OrderByDescending(p => p.DiasNoPonto)
                .ToList(),
            RecebidosRecentemente = pacotes.Take(5).ToList()
        };

        return View(painel);
    }

    public IActionResult Sobre()
    {
        return View();
    }

    /// <summary>Destino do UseStatusCodePagesWithReExecute: 404 ganha página própria, demais códigos caem na de erro.</summary>
    [Route("erro/{codigo:int}")]
    public IActionResult ErroHttp(int codigo)
    {
        return codigo == StatusCodes.Status404NotFound
            ? View("NaoEncontrado")
            : View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
