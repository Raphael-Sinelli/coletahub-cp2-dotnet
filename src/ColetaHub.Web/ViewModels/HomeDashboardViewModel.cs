using ColetaHub.Web.Models;

namespace ColetaHub.Web.ViewModels;

/// <summary>Indicadores exibidos no painel inicial.</summary>
public class HomeDashboardViewModel
{
    public int TotalPacotes { get; init; }

    public int PacotesNoPonto { get; init; }

    public int PacotesFrageis { get; init; }

    public int PacotesParados { get; init; }

    public decimal ValorSobGuarda { get; init; }

    /// <summary>Quantidade por status, na ordem do enum (inclui status com zero pacotes).</summary>
    public IReadOnlyList<ContagemStatus> PorStatus { get; init; } = [];

    /// <summary>Quantidade por marketplace, do maior para o menor.</summary>
    public IReadOnlyList<ContagemMarketplace> PorMarketplace { get; init; } = [];

    /// <summary>Pacotes no ponto há mais de <see cref="Pacote.DiasLimiteNoPonto"/> dias, mais antigos primeiro.</summary>
    public IReadOnlyList<Pacote> Parados { get; init; } = [];

    public IReadOnlyList<Pacote> RecebidosRecentemente { get; init; } = [];
}

public record ContagemStatus(StatusPacote Status, int Quantidade);

public record ContagemMarketplace(string Marketplace, int Quantidade);
