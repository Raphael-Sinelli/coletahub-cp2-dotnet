using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.Services;
using ColetaHub.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ColetaHub.Web.Controllers;

/// <summary>
/// CRUD de pacotes e pesquisa. Todo POST valida o token antiforgery e termina em
/// RedirectToAction (Post/Redirect/Get), com a mensagem de resultado em TempData.
/// </summary>
public class PacotesController : Controller
{
    public const string ChaveSucesso = "Sucesso";
    public const string ChaveErro = "Erro";

    private readonly IPacoteRepository _repositorio;
    private readonly IMarketplaceService _marketplaces;

    public PacotesController(IPacoteRepository repositorio, IMarketplaceService marketplaces)
    {
        _repositorio = repositorio;
        _marketplaces = marketplaces;
    }

    // GET /Pacotes?ordenarPor=codigo&decrescente=false
    public IActionResult Index(string ordenarPor = "data", bool decrescente = true)
    {
        var pacotes = _repositorio.ObterTodos();

        IEnumerable<Pacote> ordenados = ordenarPor switch
        {
            "codigo" => decrescente ? pacotes.OrderByDescending(p => p.CodigoRastreio) : pacotes.OrderBy(p => p.CodigoRastreio),
            "status" => decrescente ? pacotes.OrderByDescending(p => p.Status) : pacotes.OrderBy(p => p.Status),
            _ => decrescente ? pacotes.OrderByDescending(p => p.DataRecebimento) : pacotes.OrderBy(p => p.DataRecebimento)
        };

        ViewData["OrdenarPor"] = ordenarPor is "codigo" or "status" ? ordenarPor : "data";
        ViewData["Decrescente"] = decrescente;
        return View(ordenados.ToList());
    }

    // GET /Pacotes/Details/5
    public IActionResult Details(int id)
    {
        var pacote = _repositorio.ObterPorId(id);
        return pacote is null ? NotFound() : View(pacote);
    }

    // GET /Pacotes/Create
    public IActionResult Create()
    {
        return View(CriarFormulario(new Pacote()));
    }

    // POST /Pacotes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PacoteFormViewModel formulario)
    {
        ValidarRegrasDoPonto(formulario.Pacote);

        if (!ModelState.IsValid)
        {
            return View(CriarFormulario(formulario.Pacote));
        }

        var criado = _repositorio.Adicionar(formulario.Pacote);
        TempData[ChaveSucesso] = $"Pacote {criado.CodigoRastreio} registrado para {criado.NomeDestinatario}.";
        return RedirectToAction(nameof(Details), new { id = criado.Id });
    }

    // GET /Pacotes/Edit/5
    public IActionResult Edit(int id)
    {
        var pacote = _repositorio.ObterPorId(id);
        return pacote is null ? NotFound() : View(CriarFormulario(pacote));
    }

    // POST /Pacotes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, PacoteFormViewModel formulario)
    {
        if (id != formulario.Pacote.Id)
        {
            return BadRequest();
        }

        ValidarRegrasDoPonto(formulario.Pacote);

        if (!ModelState.IsValid)
        {
            return View(CriarFormulario(formulario.Pacote));
        }

        if (!_repositorio.Atualizar(formulario.Pacote))
        {
            TempData[ChaveErro] = "O pacote não existe mais. Ele pode ter sido removido por outro operador.";
            return RedirectToAction(nameof(Index));
        }

        TempData[ChaveSucesso] = $"Pacote {formulario.Pacote.CodigoRastreio} atualizado.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET /Pacotes/Delete/5 → primeira camada de confirmação (página com resumo)
    public IActionResult Delete(int id)
    {
        var pacote = _repositorio.ObterPorId(id);
        return pacote is null ? NotFound() : View(pacote);
    }

    // POST /Pacotes/Delete/5 → a exclusão só acontece aqui, com token antiforgery
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var pacote = _repositorio.ObterPorId(id);

        if (pacote is null || !_repositorio.Remover(id))
        {
            TempData[ChaveErro] = "Não foi possível remover: o pacote não foi encontrado.";
            return RedirectToAction(nameof(Index));
        }

        TempData[ChaveSucesso] = $"Pacote {pacote.CodigoRastreio} removido.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Pacotes/Search?Texto=silva&Status=AguardandoRetirada&Marketplace=Shopee&RecebidoDe=...&RecebidoAte=...
    public IActionResult Search(PacoteSearchViewModel pesquisa)
    {
        if (pesquisa.RecebidoDe > pesquisa.RecebidoAte)
        {
            ModelState.AddModelError(nameof(pesquisa.RecebidoAte), "A data final deve ser igual ou posterior à data inicial.");
        }

        pesquisa.Marketplaces = MontarOpcoesMarketplace();
        pesquisa.Resultados = ModelState.IsValid ? _repositorio.Pesquisar(pesquisa.ParaFiltro()) : [];
        return View(pesquisa);
    }

    /// <summary>
    /// Monta as opções do dropdown de marketplace a partir do serviço. É daqui que vêm os dados
    /// da string "Marketplace" escolhida pelo usuário (requisito do checkpoint).
    /// </summary>
    private IEnumerable<SelectListItem> MontarOpcoesMarketplace() =>
        _marketplaces.ObterTodos().Select(nome => new SelectListItem(nome, nome)).ToList();

    private PacoteFormViewModel CriarFormulario(Pacote pacote) => new()
    {
        Pacote = pacote,
        Marketplaces = MontarOpcoesMarketplace()
    };

    /// <summary>Regras que dependem de dados externos ao modelo: catálogo e unicidade do código.</summary>
    private void ValidarRegrasDoPonto(Pacote pacote)
    {
        var prefixo = nameof(PacoteFormViewModel.Pacote);

        if (!string.IsNullOrEmpty(pacote.Marketplace) && !_marketplaces.Existe(pacote.Marketplace))
        {
            ModelState.AddModelError($"{prefixo}.{nameof(Pacote.Marketplace)}", "Selecione um marketplace da lista.");
        }

        if (!string.IsNullOrEmpty(pacote.CodigoRastreio) && _repositorio.CodigoJaCadastrado(pacote.CodigoRastreio, pacote.Id))
        {
            ModelState.AddModelError($"{prefixo}.{nameof(Pacote.CodigoRastreio)}", "Já existe um pacote com este código de rastreio.");
        }
    }
}
