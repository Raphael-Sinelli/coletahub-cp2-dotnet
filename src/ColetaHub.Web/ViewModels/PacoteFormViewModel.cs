using ColetaHub.Web.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ColetaHub.Web.ViewModels;

/// <summary>
/// Dados da partial _Form (Create e Edit): o pacote editado e as opções do dropdown de marketplace.
/// </summary>
public class PacoteFormViewModel
{
    public Pacote Pacote { get; set; } = new();

    /// <summary>
    /// Requisito "string escolhida pelo usuário com dados vindos do Controller": a lista é
    /// montada no PacotesController a partir do IMarketplaceService e renderizada com asp-items.
    /// Não vem do formulário, por isso não é vinculada nem validada no POST.
    /// </summary>
    [BindNever]
    [ValidateNever]
    public IEnumerable<SelectListItem> Marketplaces { get; set; } = [];
}
