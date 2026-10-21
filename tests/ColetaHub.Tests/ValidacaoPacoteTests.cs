using System.ComponentModel.DataAnnotations;
using ColetaHub.Web.Models;
using ColetaHub.Web.Repositories;
using ColetaHub.Web.Validation;

namespace ColetaHub.Tests;

public class ValidacaoPacoteTests
{
    /// <summary>Executa Data Annotations e, se elas passarem, o IValidatableObject (mesma ordem do MVC).</summary>
    private static List<ValidationResult> Validar(Pacote pacote)
    {
        var erros = new List<ValidationResult>();
        Validator.TryValidateObject(pacote, new ValidationContext(pacote), erros, validateAllProperties: true);
        return erros;
    }

    private static bool TemErroEm(IEnumerable<ValidationResult> erros, string propriedade) =>
        erros.Any(e => e.MemberNames.Contains(propriedade));

    [Fact]
    public void PacoteValido_NaoTemErros()
    {
        Assert.Empty(Validar(PacoteFabrica.Valido()));
    }

    [Fact]
    public void Seed_TodosOsPacotesSaoValidos()
    {
        Assert.All(PacoteSeed.Criar(), pacote => Assert.Empty(Validar(pacote)));
    }

    // ---------- [DataNaoFutura] ----------

    [Fact]
    public void DataNaoFutura_DataDeAmanha_Invalida()
    {
        var erros = Validar(PacoteFabrica.Valido(p => p.DataRecebimento = DateTime.Today.AddDays(1)));

        Assert.True(TemErroEm(erros, nameof(Pacote.DataRecebimento)));
    }

    [Fact]
    public void DataNaoFutura_Hoje_Valida()
    {
        var atributo = new DataNaoFuturaAttribute();

        Assert.True(atributo.IsValid(DateTime.Today));
        Assert.True(atributo.IsValid(DateTime.Today.AddYears(-1)));
    }

    [Fact]
    public void DataNaoFutura_Nulo_Valido()
    {
        Assert.True(new DataNaoFuturaAttribute().IsValid(null));
    }

    [Fact]
    public void DataNaoFutura_MensagemUsaNomeDeExibicao()
    {
        var erro = Validar(PacoteFabrica.Valido(p => p.DataRecebimento = DateTime.Today.AddDays(3))).Single();

        Assert.Equal("Data de recebimento não pode ser uma data futura.", erro.ErrorMessage);
    }

    // ---------- IValidatableObject ----------

    [Fact]
    public void DataRetiradaAnteriorAoRecebimento_Invalida()
    {
        var erros = Validar(PacoteFabrica.Valido(p =>
        {
            p.Status = StatusPacote.Retirado;
            p.DataRecebimento = DateTime.Today.AddDays(-2);
            p.DataRetirada = DateTime.Today.AddDays(-5);
        }));

        var erro = Assert.Single(erros);
        Assert.Contains(nameof(Pacote.DataRetirada), erro.MemberNames);
        Assert.Contains("anterior", erro.ErrorMessage);
    }

    [Theory]
    [InlineData(StatusPacote.Recebido)]
    [InlineData(StatusPacote.AguardandoRetirada)]
    [InlineData(StatusPacote.AguardandoColeta)]
    [InlineData(StatusPacote.Devolvido)]
    public void DataRetiradaComStatusQueNaoSaiuDoPonto_Invalida(StatusPacote status)
    {
        var erros = Validar(PacoteFabrica.Valido(p =>
        {
            p.Status = status;
            p.DataRetirada = DateTime.Today;
        }));

        Assert.True(TemErroEm(erros, nameof(Pacote.DataRetirada)));
    }

    [Theory]
    [InlineData(StatusPacote.Retirado)]
    [InlineData(StatusPacote.Coletado)]
    public void StatusDeSaidaComDataRetiradaValida_Valido(StatusPacote status)
    {
        var erros = Validar(PacoteFabrica.Valido(p =>
        {
            p.Status = status;
            p.DataRetirada = DateTime.Today;
        }));

        Assert.Empty(erros);
    }

    [Fact]
    public void StatusRetiradoSemDataDeSaida_Invalido()
    {
        var erros = Validar(PacoteFabrica.Valido(p => p.Status = StatusPacote.Retirado));

        Assert.True(TemErroEm(erros, nameof(Pacote.DataRetirada)));
    }

    // ---------- Data Annotations de formato e faixa ----------

    [Theory]
    [InlineData("qb123456789br")]
    [InlineData("QB12345678BR")]
    [InlineData("QB1234567890")]
    [InlineData("1B123456789BR")]
    public void CodigoRastreioForaDoFormato_Invalido(string codigo)
    {
        Assert.True(TemErroEm(Validar(PacoteFabrica.Valido(p => p.CodigoRastreio = codigo)), nameof(Pacote.CodigoRastreio)));
    }

    [Theory]
    [InlineData("12345678909")]
    [InlineData("123.456.789-0")]
    [InlineData("123-456-789.09")]
    public void CpfForaDoFormato_Invalido(string cpf)
    {
        Assert.True(TemErroEm(Validar(PacoteFabrica.Valido(p => p.CpfDestinatario = cpf)), nameof(Pacote.CpfDestinatario)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(30.5)]
    public void PesoForaDaFaixa_Invalido(double peso)
    {
        Assert.True(TemErroEm(Validar(PacoteFabrica.Valido(p => p.PesoKg = (decimal)peso)), nameof(Pacote.PesoKg)));
    }

    [Fact]
    public void ObservacoesAcimaDe300Caracteres_Invalida()
    {
        Assert.True(TemErroEm(Validar(PacoteFabrica.Valido(p => p.Observacoes = new string('x', 301))), nameof(Pacote.Observacoes)));
    }

    // ---------- Propriedades calculadas ----------

    [Fact]
    public void DiasNoPonto_ContaAteHojeEnquantoEstaNoPonto()
    {
        var pacote = PacoteFabrica.Valido(p => p.DataRecebimento = DateTime.Today.AddDays(-9));

        Assert.Equal(9, pacote.DiasNoPonto);
        Assert.True(pacote.EstaNoPonto);
    }

    [Fact]
    public void DiasNoPonto_ContaAteADataDeSaida()
    {
        var pacote = PacoteFabrica.Valido(p =>
        {
            p.Status = StatusPacote.Coletado;
            p.DataRecebimento = DateTime.Today.AddDays(-9);
            p.DataRetirada = DateTime.Today.AddDays(-6);
        });

        Assert.Equal(3, pacote.DiasNoPonto);
        Assert.False(pacote.EstaNoPonto);
    }

    [Fact]
    public void CpfMascarado_OcultaPrimeirosEUltimosDigitos()
    {
        Assert.Equal("***.456.789-**", PacoteFabrica.Valido().CpfMascarado);
    }

    [Fact]
    public void CpfMascarado_CpfNulo_RetornaVazioSemExcecao()
    {
        // O model binding converte o campo vazio em null; antes da correção isto lançava NullReferenceException.
        var pacote = PacoteFabrica.Valido(p => p.CpfDestinatario = null!);

        Assert.Equal(string.Empty, pacote.CpfMascarado);
        Assert.Contains(Validar(pacote), e => e.MemberNames.Contains(nameof(Pacote.CpfDestinatario)));
    }
}
