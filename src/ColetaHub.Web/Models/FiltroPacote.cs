namespace ColetaHub.Web.Models;

/// <summary>Critérios de pesquisa aceitos pelo repositório. Campos nulos ou vazios são ignorados.</summary>
public class FiltroPacote
{
    /// <summary>Trecho do código de rastreio ou do nome do destinatário (sem diferenciar maiúsculas).</summary>
    public string? Texto { get; set; }

    public StatusPacote? Status { get; set; }

    public string? Marketplace { get; set; }

    public DateTime? RecebidoDe { get; set; }

    public DateTime? RecebidoAte { get; set; }
}
