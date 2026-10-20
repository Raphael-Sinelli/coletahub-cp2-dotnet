using System.ComponentModel.DataAnnotations;

namespace ColetaHub.Web.Models;

/// <summary>
/// Motivo pelo qual o pacote está no ponto: o cliente vem buscar, a transportadora vem coletar
/// (envio feito por um vendedor) ou o pacote voltará ao remetente.
/// </summary>
public enum TipoOperacao
{
    [Display(Name = "Retirada pelo cliente")]
    RetiradaCliente,

    [Display(Name = "Coleta pela transportadora")]
    ColetaTransportadora,

    [Display(Name = "Devolução")]
    Devolucao
}
