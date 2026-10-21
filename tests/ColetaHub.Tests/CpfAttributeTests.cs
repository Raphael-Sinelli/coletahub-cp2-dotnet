using System.ComponentModel.DataAnnotations;
using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.Validation;

namespace ColetaHub.Tests;

public class CpfAttributeTests
{
    private readonly CpfAttribute _atributo = new();

    [Theory]
    [InlineData("123.456.789-09")]
    [InlineData("428.193.057-41")]
    [InlineData("391.552.708-40")]
    [InlineData("842.160.937-87")]
    public void CpfValido_Aceito(string cpf)
    {
        Assert.True(_atributo.IsValid(cpf));
    }

    [Theory]
    [InlineData("123.456.789-08")] // segundo dígito errado
    [InlineData("123.456.789-19")] // primeiro dígito errado
    [InlineData("391.552.708-60")]
    [InlineData("428.193.057-14")] // dígitos trocados
    public void DigitoVerificadorErrado_Recusado(string cpf)
    {
        Assert.False(_atributo.IsValid(cpf));
    }

    [Theory]
    [InlineData("000.000.000-00")]
    [InlineData("111.111.111-11")]
    [InlineData("555.555.555-55")]
    [InlineData("999.999.999-99")]
    public void SequenciaRepetida_Recusada(string cpf)
    {
        // Sequências repetidas passam no cálculo do módulo 11, mas não são CPFs emitidos.
        Assert.False(_atributo.IsValid(cpf));
    }

    [Theory]
    [InlineData("12345678909")]      // dígitos certos, mas sem máscara
    [InlineData("123.456.789-0")]
    [InlineData("123-456-789.09")]
    [InlineData("123.456.789/09")]
    [InlineData("abc.def.ghi-jk")]
    public void FormatoErrado_Recusado(string cpf)
    {
        Assert.False(_atributo.IsValid(cpf));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Vazio_DeixaParaORequired(string? cpf)
    {
        Assert.True(_atributo.IsValid(cpf));
    }

    [Fact]
    public void Pacote_ComDigitoErrado_TemErroNoCpfComMensagemEmPortugues()
    {
        var pacote = PacoteFabrica.Valido(p => p.CpfDestinatario = "123.456.789-08");
        var erros = new List<ValidationResult>();

        Validator.TryValidateObject(pacote, new ValidationContext(pacote), erros, validateAllProperties: true);

        var erro = Assert.Single(erros);
        Assert.Contains(nameof(Pacote.CpfDestinatario), erro.MemberNames);
        Assert.Equal("Informe um CPF válido no formato 000.000.000-00.", erro.ErrorMessage);
    }

    [Fact]
    public void Seed_TodosOsCpfsTemDigitosVerificadoresCorretos()
    {
        Assert.All(PacoteSeed.Criar(), p => Assert.True(CpfAttribute.EhValido(p.CpfDestinatario), p.CpfDestinatario));
    }
}
