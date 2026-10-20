using System.Text.Encodings.Web;
using ColetaHub.Web.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ColetaHub.Web.TagHelpers;

/// <summary>
/// Renderiza o status do pacote como um selo colorido com ícone do Bootstrap Icons.
/// Uso: <c>&lt;status-badge status="@item.Status" /&gt;</c>
/// Saída: <c>&lt;span class="badge-status badge-status--aguardando-retirada"&gt;&lt;i class="bi bi-hourglass-split"&gt;&lt;/i&gt; Aguardando retirada&lt;/span&gt;</c>
/// </summary>
[HtmlTargetElement("status-badge", TagStructure = TagStructure.WithoutEndTag)]
public class StatusBadgeTagHelper : TagHelper
{
    private static readonly Dictionary<StatusPacote, (string Modificador, string Icone)> Aparencia = new()
    {
        [StatusPacote.Recebido] = ("recebido", "bi-box-seam"),
        [StatusPacote.AguardandoRetirada] = ("aguardando-retirada", "bi-hourglass-split"),
        [StatusPacote.AguardandoColeta] = ("aguardando-coleta", "bi-truck"),
        [StatusPacote.Retirado] = ("retirado", "bi-person-check"),
        [StatusPacote.Coletado] = ("coletado", "bi-box-arrow-right"),
        [StatusPacote.Devolvido] = ("devolvido", "bi-arrow-return-left")
    };

    /// <summary>Status a ser exibido.</summary>
    public StatusPacote Status { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var (modificador, icone) = Aparencia[Status];

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("badge-status", HtmlEncoder.Default);
        output.AddClass($"badge-status--{modificador}", HtmlEncoder.Default);

        output.Content.SetHtmlContent($"<i class=\"bi {icone}\" aria-hidden=\"true\"></i>");
        output.Content.Append(Status.ObterNomeExibicao());
    }
}
