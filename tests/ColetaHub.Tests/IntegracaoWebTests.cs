using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ColetaHub.Tests;

/// <summary>
/// Testes de ponta a ponta pelo pipeline HTTP real (model binding, antiforgery, validação e páginas de status),
/// simulando um navegador sem JavaScript: só o servidor valida.
/// </summary>
public class IntegracaoWebTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public IntegracaoWebTests(WebApplicationFactory<Program> fabrica)
    {
        _fabrica = fabrica;
    }

    private HttpClient CriarCliente() =>
        _fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static string CodigoUnico() => $"IT{Random.Shared.Next(0, 1_000_000_000):D9}BR";

    /// <summary>Abre o formulário (o cookie antiforgery fica no cliente) e devolve o token do campo oculto.</summary>
    private static async Task<string> ObterTokenAsync(HttpClient cliente)
    {
        var html = await cliente.GetStringAsync("/Pacotes/Create");
        return Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
    }

    private static Dictionary<string, string> FormularioValido(string token, string codigo) => new()
    {
        ["__RequestVerificationToken"] = token,
        ["Pacote.Id"] = "0",
        ["Pacote.CodigoRastreio"] = codigo,
        ["Pacote.Marketplace"] = "Shopee",
        ["Pacote.TipoOperacao"] = "RetiradaCliente",
        ["Pacote.NomeDestinatario"] = "Teste de Integração",
        ["Pacote.CpfDestinatario"] = "123.456.789-09",
        ["Pacote.PesoKg"] = "2,5",
        ["Pacote.ValorDeclarado"] = "149,90",
        ["Pacote.Fragil"] = "false",
        ["Pacote.Status"] = "Recebido",
        ["Pacote.DataRecebimento"] = DateTime.Today.ToString("yyyy-MM-dd")
    };

    private static async Task<string> LerHtmlAsync(HttpResponseMessage resposta) =>
        WebUtility.HtmlDecode(await resposta.Content.ReadAsStringAsync());

    [Fact]
    public async Task Create_FormularioVazio_MostraErrosSemExcecaoERepopulaDropdown()
    {
        var cliente = CriarCliente();
        var token = await ObterTokenAsync(cliente);
        // Um navegador envia todos os campos do formulário, vazios quando não preenchidos.
        var vazio = FormularioValido(token, codigo: "").ToDictionary(c => c.Key, c => c.Key == "__RequestVerificationToken" ? c.Value : "");

        var resposta = await cliente.PostAsync("/Pacotes/Create", new FormUrlEncodedContent(vazio));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var html = await LerHtmlAsync(resposta);
        Assert.Contains("Informe o código de rastreio.", html);
        Assert.Contains("Informe o CPF do destinatário.", html);
        Assert.Contains("O campo Peso (kg) é obrigatório.", html);
        Assert.Contains("<option value=\"Mercado Livre\">Mercado Livre</option>", html);
    }

    [Fact]
    public async Task Create_DecimalComVirgula_SalvaValorCorreto()
    {
        var cliente = CriarCliente();
        var token = await ObterTokenAsync(cliente);

        var resposta = await cliente.PostAsync("/Pacotes/Create", new FormUrlEncodedContent(FormularioValido(token, CodigoUnico())));

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        var id = resposta.Headers.Location!.OriginalString.Split('/').Last();
        var pacote = await cliente.GetFromJsonAsync<JsonElement>($"/api/pacotes/{id}");
        Assert.Equal(2.5m, pacote.GetProperty("pesoKg").GetDecimal());
        Assert.Equal(149.90m, pacote.GetProperty("valorDeclarado").GetDecimal());
    }

    [Theory]
    [InlineData("Pacote.PesoKg", "2.5")]
    [InlineData("Pacote.ValorDeclarado", "149.90")]
    public async Task Create_DecimalComPonto_RecusaSemSalvar(string campo, string valor)
    {
        var cliente = CriarCliente();
        var token = await ObterTokenAsync(cliente);
        var codigo = CodigoUnico();
        var formulario = FormularioValido(token, codigo);
        formulario[campo] = valor;

        var resposta = await cliente.PostAsync("/Pacotes/Create", new FormUrlEncodedContent(formulario));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("deve ser um número com vírgula nas casas decimais", await LerHtmlAsync(resposta));
        var encontrados = await cliente.GetFromJsonAsync<JsonElement>($"/api/pacotes/pesquisa?texto={codigo}");
        Assert.Equal(0, encontrados.GetArrayLength());
    }

    [Fact]
    public async Task Delete_ViaGet_SoMostraConfirmacaoENaoRemove()
    {
        var cliente = CriarCliente();

        var resposta = await cliente.GetAsync("/Pacotes/Delete/1");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/pacotes/1")).StatusCode);
    }

    [Fact]
    public async Task Delete_PostSemTokenAntiforgery_Retorna400ENaoRemove()
    {
        var cliente = CriarCliente();

        var resposta = await cliente.PostAsync("/Pacotes/Delete/2", new FormUrlEncodedContent(new Dictionary<string, string>()));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync("/api/pacotes/2")).StatusCode);
    }

    [Fact]
    public async Task Details_IdInexistente_RetornaPagina404Amigavel()
    {
        var resposta = await CriarCliente().GetAsync("/Pacotes/Details/99999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Contains("Não encontramos este pacote ou página", await LerHtmlAsync(resposta));
    }

    [Fact]
    public async Task Search_StatusInvalidoNaUrl_MostraMensagem()
    {
        var resposta = await CriarCliente().GetAsync("/Pacotes/Search?Status=abc");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("O valor 'abc' não é válido para Status.", await LerHtmlAsync(resposta));
    }

    [Fact]
    public async Task Api_CpfNulo_Retorna400ComTituloEmPortugues()
    {
        var resposta = await CriarCliente().PostAsJsonAsync("/api/pacotes", new
        {
            codigoRastreio = CodigoUnico(),
            nomeDestinatario = "Teste de Integração",
            cpfDestinatario = (string?)null,
            marketplace = "Amazon",
            pesoKg = 1.2m,
            valorDeclarado = 99.9m,
            dataRecebimento = DateTime.Today.ToString("yyyy-MM-dd"),
            status = "Recebido",
            tipoOperacao = "RetiradaCliente",
            fragil = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Um ou mais campos são inválidos.", problema.GetProperty("title").GetString());
        Assert.True(problema.GetProperty("errors").TryGetProperty("CpfDestinatario", out _));
    }

    [Theory]
    [InlineData("123.456.789-08")]
    [InlineData("111.111.111-11")]
    public async Task Create_CpfComDigitoErradoOuRepetido_RecusaSemSalvar(string cpf)
    {
        var cliente = CriarCliente();
        var token = await ObterTokenAsync(cliente);
        var codigo = CodigoUnico();
        var formulario = FormularioValido(token, codigo);
        formulario["Pacote.CpfDestinatario"] = cpf;

        var resposta = await cliente.PostAsync("/Pacotes/Create", new FormUrlEncodedContent(formulario));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("Informe um CPF válido no formato 000.000.000-00.", await LerHtmlAsync(resposta));
        var encontrados = await cliente.GetFromJsonAsync<JsonElement>($"/api/pacotes/pesquisa?texto={codigo}");
        Assert.Equal(0, encontrados.GetArrayLength());
    }

    [Fact]
    public async Task Api_CpfComDigitoErrado_Retorna400()
    {
        var resposta = await CriarCliente().PostAsJsonAsync("/api/pacotes", new
        {
            codigoRastreio = CodigoUnico(),
            nomeDestinatario = "Teste de Integração",
            cpfDestinatario = "391.552.708-60",
            marketplace = "Amazon",
            pesoKg = 1.2m,
            valorDeclarado = 99.9m,
            dataRecebimento = DateTime.Today.ToString("yyyy-MM-dd"),
            status = "Recebido",
            tipoOperacao = "RetiradaCliente",
            fragil = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var erros = (await resposta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors");
        Assert.Equal("Informe um CPF válido no formato 000.000.000-00.", erros.GetProperty("CpfDestinatario")[0].GetString());
    }
}
