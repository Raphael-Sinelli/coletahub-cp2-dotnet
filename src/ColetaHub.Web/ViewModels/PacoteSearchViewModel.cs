using System.ComponentModel.DataAnnotations;
using ColetaHub.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ColetaHub.Web.ViewModels;

/// <summary>
/// Filtros da tela Pesquisar (vinculados pela query string do formulário GET) e os resultados.
/// Como a mesma instância volta para a View, os campos continuam preenchidos após a busca.
/// </summary>
public class PacoteSearchViewModel
{
    [Display(Name = "Código ou destinatário")]
    [StringLength(100, ErrorMessage = "Use no máximo 100 caracteres na busca.")]
    public string? Texto { get; set; }

    [Display(Name = "Status")]
    public StatusPacote? Status { get; set; }

    [Display(Name = "Marketplace")]
    public string? Marketplace { get; set; }

    [Display(Name = "Recebido de")]
    [DataType(DataType.Date)]
    public DateTime? RecebidoDe { get; set; }

    [Display(Name = "Recebido até")]
    [DataType(DataType.Date)]
    public DateTime? RecebidoAte { get; set; }

    [BindNever]
    [ValidateNever]
    public IEnumerable<SelectListItem> Marketplaces { get; set; } = [];

    [BindNever]
    [ValidateNever]
    public IReadOnlyList<Pacote> Resultados { get; set; } = [];

    public int TotalResultados => Resultados.Count;

    public bool PossuiFiltros =>
        !string.IsNullOrWhiteSpace(Texto) || Status.HasValue || !string.IsNullOrWhiteSpace(Marketplace) ||
        RecebidoDe.HasValue || RecebidoAte.HasValue;

    public FiltroPacote ParaFiltro() => new()
    {
        Texto = Texto,
        Status = Status,
        Marketplace = Marketplace,
        RecebidoDe = RecebidoDe,
        RecebidoAte = RecebidoAte
    };
}
