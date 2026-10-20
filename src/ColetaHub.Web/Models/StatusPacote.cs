using System.ComponentModel.DataAnnotations;

namespace ColetaHub.Web.Models;

/// <summary>
/// Etapas do ciclo de vida de um pacote dentro do ponto de coleta.
/// O [Display] fornece o rótulo em português usado por Html.GetEnumSelectList e pelo &lt;status-badge&gt;.
/// </summary>
public enum StatusPacote
{
    [Display(Name = "Recebido")]
    Recebido,

    [Display(Name = "Aguardando retirada")]
    AguardandoRetirada,

    [Display(Name = "Aguardando coleta")]
    AguardandoColeta,

    [Display(Name = "Retirado pelo cliente")]
    Retirado,

    [Display(Name = "Coletado pela transportadora")]
    Coletado,

    [Display(Name = "Devolvido ao remetente")]
    Devolvido
}
