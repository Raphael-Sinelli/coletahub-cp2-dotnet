using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ColetaHub.Web.Controllers.Api;

/// <summary>
/// API REST de pacotes. Usa o mesmo repositório Singleton do site MVC, então uma alteração
/// feita pelo Postman aparece imediatamente no navegador (e vice-versa).
/// Com [ApiController], um modelo inválido devolve 400 com ValidationProblemDetails automaticamente.
/// </summary>
[ApiController]
[Route("api/pacotes")]
[Produces("application/json")]
public class PacotesApiController : ControllerBase
{
    private readonly IPacoteRepository _repositorio;
    private readonly IMarketplaceService _marketplaces;

    public PacotesApiController(IPacoteRepository repositorio, IMarketplaceService marketplaces)
    {
        _repositorio = repositorio;
        _marketplaces = marketplaces;
    }

    /// <summary>Lista todos os pacotes, do recebimento mais recente para o mais antigo.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<Pacote>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Pacote>> ObterTodos()
    {
        return Ok(_repositorio.ObterTodos());
    }

    /// <summary>Obtém um pacote pelo Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<Pacote>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Pacote> ObterPorId(int id)
    {
        var pacote = _repositorio.ObterPorId(id);
        return pacote is null ? NotFound() : Ok(pacote);
    }

    /// <summary>Pesquisa por texto (código ou destinatário), status, marketplace e intervalo de recebimento.</summary>
    /// <remarks>Exemplo: GET /api/pacotes/pesquisa?texto=silva&amp;status=AguardandoRetirada&amp;recebidoDe=2026-10-01</remarks>
    [HttpGet("pesquisa")]
    [ProducesResponseType<IEnumerable<Pacote>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Pacote>> Pesquisar([FromQuery] FiltroPacote filtro)
    {
        return Ok(_repositorio.Pesquisar(filtro));
    }

    /// <summary>Registra um novo pacote. O Id enviado no corpo é ignorado.</summary>
    [HttpPost]
    [ProducesResponseType<Pacote>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<Pacote> Criar(Pacote pacote)
    {
        pacote.Id = 0;
        ValidarRegrasDoPonto(pacote);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var criado = _repositorio.Adicionar(pacote);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza todos os campos de um pacote existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Atualizar(int id, Pacote pacote)
    {
        if (pacote.Id != 0 && pacote.Id != id)
        {
            ModelState.AddModelError(nameof(Pacote.Id), "O Id do corpo difere do Id da URL.");
            return ValidationProblem(ModelState);
        }

        if (_repositorio.ObterPorId(id) is null)
        {
            return NotFound();
        }

        pacote.Id = id;
        ValidarRegrasDoPonto(pacote);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return _repositorio.Atualizar(pacote) ? NoContent() : NotFound();
    }

    /// <summary>Remove um pacote.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Remover(int id)
    {
        return _repositorio.Remover(id) ? NoContent() : NotFound();
    }

    private void ValidarRegrasDoPonto(Pacote pacote)
    {
        if (!_marketplaces.Existe(pacote.Marketplace))
        {
            ModelState.AddModelError(nameof(Pacote.Marketplace),
                $"Marketplace inválido. Opções: {string.Join(", ", _marketplaces.ObterTodos())}.");
        }

        if (_repositorio.CodigoJaCadastrado(pacote.CodigoRastreio, pacote.Id))
        {
            ModelState.AddModelError(nameof(Pacote.CodigoRastreio), "Já existe um pacote com este código de rastreio.");
        }
    }
}
