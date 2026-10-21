using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ColetaHub.Web.Validation;

/// <summary>
/// Valida CPF no formato 000.000.000-00 com dígitos verificadores corretos (módulo 11),
/// rejeitando sequências repetidas como 111.111.111-11. Valor vazio é aceito: a obrigatoriedade
/// fica com o [Required]. No navegador emite <c>data-val-cpf</c>, lido pelo adaptador em
/// wwwroot/js/validacao-ptbr.js, que aplica o mesmo algoritmo.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed partial class CpfAttribute : ValidationAttribute, IClientModelValidator
{
    public CpfAttribute()
        : base("Informe um CPF válido no formato 000.000.000-00.")
    {
    }

    [GeneratedRegex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$")]
    private static partial Regex FormatoComMascara();

    /// <summary>Confere formato e dígitos verificadores de um CPF com máscara.</summary>
    public static bool EhValido(string cpf)
    {
        if (!FormatoComMascara().IsMatch(cpf))
        {
            return false;
        }

        var digitos = cpf.Where(char.IsDigit).Select(c => c - '0').ToArray();
        if (digitos.All(d => d == digitos[0]))
        {
            return false;
        }

        return digitos[9] == CalcularDigito(digitos, 9) && digitos[10] == CalcularDigito(digitos, 10);
    }

    // Soma ponderada dos "quantidade" primeiros dígitos (pesos decrescentes a partir de quantidade + 1), módulo 11.
    private static int CalcularDigito(int[] digitos, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
        {
            soma += digitos[i] * (quantidade + 1 - i);
        }

        var resto = soma * 10 % 11;
        return resto == 10 ? 0 : resto;
    }

    public override bool IsValid(object? value) =>
        value is not string cpf || string.IsNullOrEmpty(cpf) || EhValido(cpf);

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.TryAdd("data-val", "true");
        context.Attributes.TryAdd("data-val-cpf", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
    }
}
