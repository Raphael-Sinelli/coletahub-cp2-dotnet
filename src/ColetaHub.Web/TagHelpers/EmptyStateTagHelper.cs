using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ColetaHub.Web.TagHelpers;

/// <summary>
/// Bloco padronizado para listas vazias: ícone, título, mensagem e, opcionalmente, ações no conteúdo.
/// Uso: <c>&lt;empty-state mensagem="Nenhum pacote encontrado." icone="search"&gt;&lt;a ...&gt;Limpar filtros&lt;/a&gt;&lt;/empty-state&gt;</c>
/// </summary>
[HtmlTargetElement("empty-state")]
public class EmptyStateTagHelper : TagHelper
{
    /// <summary>Texto explicativo exibido abaixo do título.</summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Título em destaque (padrão: "Nada por aqui").</summary>
    public string Titulo { get; set; } = "Nada por aqui";

    /// <summary>Nome do ícone do Bootstrap Icons, sem o prefixo "bi-" (padrão: inbox).</summary>
    public string Icone { get; set; } = "inbox";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var acoes = await output.GetChildContentAsync();
        var codificador = HtmlEncoder.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("empty-state", codificador);
        output.Attributes.SetAttribute("role", "status");

        output.Content.SetHtmlContent(
            $"<div class=\"empty-state__icone\"><i class=\"bi bi-{codificador.Encode(Icone)}\" aria-hidden=\"true\"></i></div>" +
            $"<h3 class=\"empty-state__titulo\">{codificador.Encode(Titulo)}</h3>" +
            $"<p class=\"empty-state__mensagem\">{codificador.Encode(Mensagem)}</p>");

        if (!acoes.IsEmptyOrWhiteSpace)
        {
            output.Content.AppendHtml("<div class=\"empty-state__acoes\">");
            output.Content.AppendHtml(acoes);
            output.Content.AppendHtml("</div>");
        }
    }
}
