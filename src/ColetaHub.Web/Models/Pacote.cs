using System.ComponentModel.DataAnnotations;
using ColetaHub.Web.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ColetaHub.Web.Models;

/// <summary>
/// Pacote de marketplace que está (ou esteve) sob a guarda do ponto de coleta.
/// Reúne os tipos exigidos no checkpoint: string, número (int/decimal), data, enum, bool
/// e a string Marketplace, escolhida em uma lista montada pelo Controller.
/// </summary>
public class Pacote : IValidatableObject
{
    /// <summary>Limite, em dias, a partir do qual o pacote é considerado parado no ponto.</summary>
    public const int DiasLimiteNoPonto = 7;

    // [Required] explícito em Id e Fragil: sem ele o MVC cria um implícito (tipos não anuláveis)
    // com a mensagem padrão em inglês no atributo data-val-required.
    [Required(ErrorMessage = "O identificador do pacote é obrigatório.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o código de rastreio.")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "O código de rastreio deve ter exatamente 13 caracteres.")]
    [RegularExpression(@"^[A-Z]{2}\d{9}BR$", ErrorMessage = "Use o formato AA123456789BR: 2 letras, 9 dígitos e BR.")]
    [Display(Name = "Código de rastreio")]
    public string CodigoRastreio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome do destinatário.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    [Display(Name = "Destinatário")]
    public string NomeDestinatario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF do destinatário.")]
    [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "Use o formato 000.000.000-00.")]
    [Display(Name = "CPF do destinatário")]
    public string CpfDestinatario { get; set; } = string.Empty;

    /// <summary>
    /// String escolhida pelo usuário em um dropdown cujas opções vêm do Controller
    /// (IMarketplaceService → PacoteFormViewModel.Marketplaces).
    /// </summary>
    [Required(ErrorMessage = "Selecione o marketplace de origem.")]
    [StringLength(30, ErrorMessage = "O marketplace deve ter no máximo 30 caracteres.")]
    [Display(Name = "Marketplace")]
    public string Marketplace { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o peso.")]
    [Range(0.01, 30.0, ErrorMessage = "O peso deve estar entre 0,01 kg e 30 kg.")]
    [Display(Name = "Peso (kg)")]
    public decimal PesoKg { get; set; }

    [Required(ErrorMessage = "Informe o valor declarado.")]
    [DataType(DataType.Currency)]
    [Range(0.0, 50000.0, ErrorMessage = "O valor declarado deve estar entre R$ 0,00 e R$ 50.000,00.")]
    [Display(Name = "Valor declarado")]
    public decimal ValorDeclarado { get; set; }

    [Required(ErrorMessage = "Informe a data de recebimento.")]
    [DataType(DataType.Date)]
    [DataNaoFutura]
    [Display(Name = "Data de recebimento")]
    public DateTime DataRecebimento { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [DataNaoFutura]
    [Display(Name = "Data de saída")]
    public DateTime? DataRetirada { get; set; }

    [Required(ErrorMessage = "Selecione o status.")]
    [Display(Name = "Status")]
    public StatusPacote Status { get; set; } = StatusPacote.Recebido;

    [Required(ErrorMessage = "Selecione o tipo de operação.")]
    [Display(Name = "Tipo de operação")]
    public TipoOperacao TipoOperacao { get; set; } = TipoOperacao.RetiradaCliente;

    [Required(ErrorMessage = "Informe se o pacote é frágil.")]
    [Display(Name = "Frágil")]
    public bool Fragil { get; set; }

    [StringLength(300, ErrorMessage = "As observações devem ter no máximo 300 caracteres.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Observações")]
    public string? Observacoes { get; set; }

    /// <summary>Indica se o pacote ainda está fisicamente no ponto (não saiu nem foi devolvido).</summary>
    [ValidateNever]
    public bool EstaNoPonto => Status is StatusPacote.Recebido or StatusPacote.AguardandoRetirada or StatusPacote.AguardandoColeta;

    /// <summary>Dias entre o recebimento e a saída (ou hoje, se não há data de saída). Calculado, não persistido.</summary>
    [ValidateNever]
    [Display(Name = "Dias no ponto")]
    public int DiasNoPonto => Math.Max(0, ((DataRetirada ?? DateTime.Today).Date - DataRecebimento.Date).Days);

    /// <summary>CPF parcialmente oculto para listagens (LGPD): ***.456.789-**.</summary>
    /// <remarks>Tolera CPF nulo: o model binding converte o campo vazio em null antes da validação.</remarks>
    [ValidateNever]
    public string CpfMascarado => CpfDestinatario is { Length: 14 } cpf ? $"***{cpf[3..11]}-**" : CpfDestinatario ?? string.Empty;

    /// <summary>
    /// Regras que envolvem mais de uma propriedade. O MVC executa este método depois que
    /// todas as Data Annotations das propriedades passaram.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var saiuDoPonto = Status is StatusPacote.Retirado or StatusPacote.Coletado;

        if (DataRetirada.HasValue && DataRetirada.Value.Date < DataRecebimento.Date)
        {
            yield return new ValidationResult(
                "A data de saída não pode ser anterior à data de recebimento.",
                [nameof(DataRetirada)]);
        }

        if (DataRetirada.HasValue && !saiuDoPonto)
        {
            yield return new ValidationResult(
                "A data de saída só pode ser informada quando o status é Retirado ou Coletado.",
                [nameof(DataRetirada)]);
        }

        if (!DataRetirada.HasValue && saiuDoPonto)
        {
            yield return new ValidationResult(
                "Informe a data de saída para pacotes retirados ou coletados.",
                [nameof(DataRetirada)]);
        }
    }

    /// <summary>Cópia rasa, usada pelo repositório para não expor a instância armazenada.</summary>
    public Pacote Clonar() => (Pacote)MemberwiseClone();
}
