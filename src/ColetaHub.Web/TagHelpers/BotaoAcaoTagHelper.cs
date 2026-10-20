using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ColetaHub.Web.TagHelpers;

/// <summary>
/// Padroniza os botões de ação de um registro (ícone, cor, texto acessível e rota).
/// Uso: <c>&lt;botao-acao tipo="editar" id="@item.Id" /&gt;</c>
/// Tipos aceitos: <c>detalhes</c>, <c>editar</c> e <c>excluir</c>.
/// Com <c>modal="true"</c>, o botão excluir abre o modal de confirmação (#modalExclusao) em vez de navegar;
/// sem JavaScript, o link continua levando à página de confirmação.
/// </summary>
[HtmlTargetElement("botao-acao", TagStructure = TagStructure.WithoutEndTag)]
public class BotaoAcaoTagHelper : TagHelper
{
    private static readonly Dictionary<string, (string Acao, string Rotulo, string Icone, string Estilo)> Tipos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["detalhes"] = ("Details", "Detalhes", "bi-eye", "btn-outline-secondary"),
            ["editar"] = ("Edit", "Editar", "bi-pencil-square", "btn-outline-primary"),
            ["excluir"] = ("Delete", "Excluir", "bi-trash3", "btn-outline-danger")
        };

    private readonly IUrlHelperFactory _urlHelperFactory;

    public BotaoAcaoTagHelper(IUrlHelperFactory urlHelperFactory)
    {
        _urlHelperFactory = urlHelperFactory;
    }

    /// <summary>detalhes | editar | excluir</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Id do registro usado na rota.</summary>
    public int Id { get; set; }

    /// <summary>Controller de destino (padrão: Pacotes).</summary>
    public string Controlador { get; set; } = "Pacotes";

    /// <summary>Texto que identifica o registro no tooltip e no modal (ex.: o código de rastreio).</summary>
    public string? Descricao { get; set; }

    /// <summary>Exibe só o ícone (o rótulo fica disponível para leitores de tela).</summary>
    public bool Compacto { get; set; }

    /// <summary>Para o tipo excluir: abre o modal de confirmação da página.</summary>
    public bool Modal { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!Tipos.TryGetValue(Tipo, out var configuracao))
        {
            throw new InvalidOperationException(
                $"<botao-acao> tipo \"{Tipo}\" inválido. Use: {string.Join(", ", Tipos.Keys)}.");
        }

        var url = _urlHelperFactory.GetUrlHelper(ViewContext).Action(configuracao.Acao, Controlador, new { id = Id });
        var textoAcessivel = string.IsNullOrWhiteSpace(Descricao) ? configuracao.Rotulo : $"{configuracao.Rotulo} {Descricao}";

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("href", url);
        output.Attributes.SetAttribute("title", textoAcessivel);
        output.AddClass("btn", HtmlEncoder.Default);
        output.AddClass("btn-sm", HtmlEncoder.Default);
        output.AddClass(configuracao.Estilo, HtmlEncoder.Default);
        output.AddClass("botao-acao", HtmlEncoder.Default);

        if (Modal && configuracao.Acao == "Delete")
        {
            output.Attributes.SetAttribute("data-bs-toggle", "modal");
            output.Attributes.SetAttribute("data-bs-target", "#modalExclusao");
            output.Attributes.SetAttribute("data-descricao", Descricao ?? string.Empty);
        }

        var rotulo = Compacto
            ? $"<span class=\"visually-hidden\">{HtmlEncoder.Default.Encode(textoAcessivel)}</span>"
            : $"<span>{configuracao.Rotulo}</span>";

        output.Content.SetHtmlContent($"<i class=\"bi {configuracao.Icone}\" aria-hidden=\"true\"></i>{rotulo}");
    }
}
