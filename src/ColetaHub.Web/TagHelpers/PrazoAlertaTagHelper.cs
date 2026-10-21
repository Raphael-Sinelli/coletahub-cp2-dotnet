using System.Text.Encodings.Web;
using ColetaHub.Web.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ColetaHub.Web.TagHelpers;

/// <summary>
/// Mostra há quantos dias o pacote está no ponto, com alerta visual quando passa do limite
/// (padrão: <see cref="Pacote.DiasLimiteNoPonto"/> dias). Faixas: ok, atenção (a 2 dias do limite) e alerta.
/// Uso: <c>&lt;prazo-alerta dias="@item.DiasNoPonto" encerrado="@(!item.EstaNoPonto)" /&gt;</c>
/// </summary>
[HtmlTargetElement("prazo-alerta", TagStructure = TagStructure.WithoutEndTag)]
public class PrazoAlertaTagHelper : TagHelper
{
    /// <summary>Dias desde o recebimento.</summary>
    public int Dias { get; set; }

    /// <summary>Quantidade de dias a partir da qual o pacote é considerado parado.</summary>
    public int Limite { get; set; } = Pacote.DiasLimiteNoPonto;

    /// <summary>Quando true (pacote já saiu ou foi devolvido), exibe apenas o tempo de permanência, sem alerta.</summary>
    public bool Encerrado { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var texto = Dias switch
        {
            0 => "Hoje",
            1 => "1 dia",
            _ => $"{Dias} dias"
        };

        var (modificador, icone, dica) = (Encerrado, Dias) switch
        {
            (true, _) => ("encerrado", "bi-clock-history", "Pacote já saiu do ponto: sem alerta de prazo"),
            (false, var d) when d > Limite => ("alerta", "bi-exclamation-triangle-fill", $"Parado há mais de {Limite} dias"),
            (false, var d) when d >= Limite - 2 => ("atencao", "bi-hourglass-bottom", $"Perto do limite de {Limite} dias"),
            _ => ("ok", "bi-clock", "Dentro do prazo")
        };

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("prazo", HtmlEncoder.Default);
        output.AddClass($"prazo--{modificador}", HtmlEncoder.Default);
        output.Attributes.SetAttribute("title", dica);

        output.Content.SetHtmlContent($"<i class=\"bi {icone}\" aria-hidden=\"true\"></i>");
        output.Content.Append(modificador == "alerta" ? $"Parado há {texto}" : texto);
    }
}
