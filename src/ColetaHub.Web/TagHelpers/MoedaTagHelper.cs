using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ColetaHub.Web.TagHelpers;

/// <summary>
/// Formata um valor monetário em reais (pt-BR), independentemente da cultura da requisição.
/// Uso: <c>&lt;moeda valor="@item.ValorDeclarado" /&gt;</c> → <c>&lt;span class="moeda"&gt;R$ 1.299,90&lt;/span&gt;</c>
/// </summary>
[HtmlTargetElement("moeda", TagStructure = TagStructure.WithoutEndTag)]
public class MoedaTagHelper : TagHelper
{
    private static readonly CultureInfo CulturaBrasileira = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Valor em reais.</summary>
    public decimal Valor { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("moeda", HtmlEncoder.Default);
        output.Content.SetContent(Valor.ToString("C", CulturaBrasileira));
    }
}
