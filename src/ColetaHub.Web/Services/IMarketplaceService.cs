namespace ColetaHub.Web.Services;

/// <summary>Catálogo de marketplaces atendidos pelo ponto de coleta.</summary>
public interface IMarketplaceService
{
    IReadOnlyList<string> ObterTodos();

    /// <summary>Confere se o nome pertence ao catálogo (evita valores forjados no POST).</summary>
    bool Existe(string? nome);
}
