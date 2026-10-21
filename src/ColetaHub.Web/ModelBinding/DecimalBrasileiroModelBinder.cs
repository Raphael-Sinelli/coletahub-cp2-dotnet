using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ColetaHub.Web.ModelBinding;

/// <summary>
/// Lê decimais de formulários e query strings no formato brasileiro (2,5 · 149,90 · 1.234,56).
/// O binder padrão, em pt-BR, trata o ponto como separador de milhar e transforma "2.5" em 25;
/// aqui qualquer ponto fora de um agrupamento de milhar válido é recusado com mensagem clara.
/// Não afeta a API, cujo corpo JSON é lido pelo System.Text.Json (ponto decimal).
/// </summary>
public sealed partial class DecimalBrasileiroModelBinder : IModelBinder
{
    private static readonly CultureInfo CulturaBrasileira = CultureInfo.GetCultureInfo("pt-BR");

    [GeneratedRegex(@"^-?(\d{1,3}(\.\d{3})+|\d+)(,\d+)?$")]
    private static partial Regex FormatoBrasileiro();

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var resultado = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (resultado == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, resultado);
        var texto = resultado.FirstValue?.Trim();
        var campo = bindingContext.ModelMetadata.GetDisplayName();

        if (string.IsNullOrEmpty(texto))
        {
            if (bindingContext.ModelMetadata.IsReferenceOrNullableType)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"O campo {campo} é obrigatório.");
            }

            return Task.CompletedTask;
        }

        if (FormatoBrasileiro().IsMatch(texto) &&
            decimal.TryParse(texto, NumberStyles.Number, CulturaBrasileira, out var valor))
        {
            bindingContext.Result = ModelBindingResult.Success(valor);
        }
        else
        {
            // Mesma mensagem configurada no Program.cs e exibida pelo jQuery Validate (data-val-number).
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustBeANumberAccessor(campo));
        }

        return Task.CompletedTask;
    }
}

/// <summary>Usa o <see cref="DecimalBrasileiroModelBinder"/> para propriedades decimal e decimal?.</summary>
public sealed class DecimalBrasileiroModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var tipo = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return tipo == typeof(decimal) ? new DecimalBrasileiroModelBinder() : null;
    }
}
