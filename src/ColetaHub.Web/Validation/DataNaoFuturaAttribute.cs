using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ColetaHub.Web.Validation;

/// <summary>
/// Atributo de validação customizado: impede datas posteriores ao dia de hoje.
/// Um pacote não pode ter sido recebido (ou retirado) amanhã.
/// No servidor valida em <see cref="IsValid(object?, ValidationContext)"/>; no navegador emite
/// <c>data-val-datanaofutura</c>, lido pelo adaptador em wwwroot/js/validacao-ptbr.js.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class DataNaoFuturaAttribute : ValidationAttribute, IClientModelValidator
{
    public DataNaoFuturaAttribute()
        : base("A {0} não pode estar no futuro.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateTime data || data.Date <= DateTime.Today)
        {
            return ValidationResult.Success;
        }

        var membros = validationContext.MemberName is null ? null : new[] { validationContext.MemberName };
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), membros);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-datanaofutura", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
    }
}
