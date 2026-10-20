namespace ColetaHub.Web.Services;

/// <summary>
/// Fonte das opções do dropdown de Marketplace. O PacotesController consulta este serviço
/// e envia a lista para a View dentro do PacoteFormViewModel.
/// </summary>
public class MarketplaceService : IMarketplaceService
{
    private static readonly string[] Marketplaces =
    [
        "Mercado Livre",
        "Shopee",
        "Amazon",
        "Magalu",
        "Shein",
        "AliExpress"
    ];

    public IReadOnlyList<string> ObterTodos() => Marketplaces;

    public bool Existe(string? nome) =>
        nome is not null && Marketplaces.Contains(nome, StringComparer.OrdinalIgnoreCase);
}
