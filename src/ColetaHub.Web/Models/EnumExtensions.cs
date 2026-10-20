using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ColetaHub.Web.Models;

/// <summary>Utilitários para exibir enums com o texto definido em [Display(Name = "...")].</summary>
public static class EnumExtensions
{
    /// <summary>Retorna o Display.Name do valor do enum ou, se não houver, o próprio nome do membro.</summary>
    public static string ObterNomeExibicao(this Enum valor)
    {
        var membro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
        return membro?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? valor.ToString();
    }
}
