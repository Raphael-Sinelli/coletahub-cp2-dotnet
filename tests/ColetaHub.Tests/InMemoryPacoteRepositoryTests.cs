using ColetaHub.Web.Models;
using ColetaHub.Web.Services;

namespace ColetaHub.Tests;

public class InMemoryPacoteRepositoryTests
{
    [Fact]
    public void Adicionar_AtribuiIdsIncrementais()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();

        var primeiro = repositorio.Adicionar(PacoteFabrica.Valido());
        var segundo = repositorio.Adicionar(PacoteFabrica.Valido(p => p.CodigoRastreio = "OY987654321BR"));

        Assert.Equal(1, primeiro.Id);
        Assert.Equal(2, segundo.Id);
        Assert.Equal(2, repositorio.ObterTodos().Count);
    }

    [Fact]
    public void ObterPorId_RetornaPacoteCadastrado()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();
        var criado = repositorio.Adicionar(PacoteFabrica.Valido());

        var encontrado = repositorio.ObterPorId(criado.Id);

        Assert.NotNull(encontrado);
        Assert.Equal("QB123456789BR", encontrado.CodigoRastreio);
        Assert.Equal("Maria Aparecida Souza", encontrado.NomeDestinatario);
    }

    [Fact]
    public void ObterPorId_IdInexistente_RetornaNull()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();

        Assert.Null(repositorio.ObterPorId(42));
    }

    [Fact]
    public void ObterPorId_RetornaCopia_AlteracaoSemAtualizarNaoPersiste()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();
        var criado = repositorio.Adicionar(PacoteFabrica.Valido());

        var copia = repositorio.ObterPorId(criado.Id)!;
        copia.NomeDestinatario = "Alterado sem salvar";

        Assert.Equal("Maria Aparecida Souza", repositorio.ObterPorId(criado.Id)!.NomeDestinatario);
    }

    [Fact]
    public void Atualizar_PersisteAlteracoes()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();
        var pacote = repositorio.Adicionar(PacoteFabrica.Valido());

        pacote.Status = StatusPacote.Retirado;
        pacote.DataRetirada = DateTime.Today;
        var atualizou = repositorio.Atualizar(pacote);

        Assert.True(atualizou);
        var salvo = repositorio.ObterPorId(pacote.Id)!;
        Assert.Equal(StatusPacote.Retirado, salvo.Status);
        Assert.Equal(DateTime.Today, salvo.DataRetirada);
    }

    [Fact]
    public void Atualizar_IdInexistente_RetornaFalse()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();

        var atualizou = repositorio.Atualizar(PacoteFabrica.Valido(p => p.Id = 99));

        Assert.False(atualizou);
        Assert.Empty(repositorio.ObterTodos());
    }

    [Fact]
    public void Remover_ExcluiPacote()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();
        var pacote = repositorio.Adicionar(PacoteFabrica.Valido());

        Assert.True(repositorio.Remover(pacote.Id));
        Assert.Null(repositorio.ObterPorId(pacote.Id));
        Assert.False(repositorio.Remover(pacote.Id));
    }

    [Theory]
    [InlineData("qb482917365br")]
    [InlineData("4829173")]
    [InlineData("QB48")]
    public void Pesquisar_PorTrechoDoCodigo_IgnoraMaiusculas(string texto)
    {
        var repositorio = PacoteFabrica.RepositorioComSeed();

        var resultado = repositorio.Pesquisar(new FiltroPacote { Texto = texto });

        Assert.Single(resultado);
        Assert.Equal("QB482917365BR", resultado[0].CodigoRastreio);
    }

    [Fact]
    public void Pesquisar_PorNomeDoDestinatario()
    {
        var repositorio = PacoteFabrica.RepositorioComSeed();

        var resultado = repositorio.Pesquisar(new FiltroPacote { Texto = "  MARTINS " });

        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, p => Assert.Contains("Martins", p.NomeDestinatario));
    }

    [Fact]
    public void Pesquisar_FiltraPorStatus()
    {
        var repositorio = PacoteFabrica.RepositorioComSeed();

        var resultado = repositorio.Pesquisar(new FiltroPacote { Status = StatusPacote.Devolvido });

        Assert.NotEmpty(resultado);
        Assert.All(resultado, p => Assert.Equal(StatusPacote.Devolvido, p.Status));
    }

    [Fact]
    public void Pesquisar_CombinaMarketplaceEIntervaloDeDatas()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();
        repositorio.Adicionar(PacoteFabrica.Valido(p => { p.CodigoRastreio = "AA000000001BR"; p.Marketplace = "Amazon"; p.DataRecebimento = DateTime.Today.AddDays(-10); }));
        repositorio.Adicionar(PacoteFabrica.Valido(p => { p.CodigoRastreio = "AA000000002BR"; p.Marketplace = "Amazon"; p.DataRecebimento = DateTime.Today.AddDays(-3); }));
        repositorio.Adicionar(PacoteFabrica.Valido(p => { p.CodigoRastreio = "AA000000003BR"; p.Marketplace = "Shein"; p.DataRecebimento = DateTime.Today.AddDays(-3); }));

        var resultado = repositorio.Pesquisar(new FiltroPacote
        {
            Marketplace = "amazon",
            RecebidoDe = DateTime.Today.AddDays(-5),
            RecebidoAte = DateTime.Today
        });

        var unico = Assert.Single(resultado);
        Assert.Equal("AA000000002BR", unico.CodigoRastreio);
    }

    [Fact]
    public void Pesquisar_SemFiltros_RetornaTodos()
    {
        var repositorio = PacoteFabrica.RepositorioComSeed();

        Assert.Equal(repositorio.ObterTodos().Count, repositorio.Pesquisar(new FiltroPacote()).Count);
    }

    [Fact]
    public void CodigoJaCadastrado_IgnoraOProprioPacoteNaEdicao()
    {
        var repositorio = PacoteFabrica.RepositorioVazio();
        var pacote = repositorio.Adicionar(PacoteFabrica.Valido());

        Assert.True(repositorio.CodigoJaCadastrado("qb123456789br"));
        Assert.False(repositorio.CodigoJaCadastrado("QB123456789BR", pacote.Id));
    }

    [Fact]
    public void Seed_TemAoMenos12PacotesCobrindoStatusMarketplacesEParados()
    {
        var pacotes = PacoteFabrica.RepositorioComSeed().ObterTodos();

        Assert.True(pacotes.Count >= 12);
        Assert.All(Enum.GetValues<StatusPacote>(), status => Assert.Contains(pacotes, p => p.Status == status));
        Assert.All(new MarketplaceService().ObterTodos(), nome => Assert.Contains(pacotes, p => p.Marketplace == nome));
        Assert.Contains(pacotes, p => p.Fragil);
        Assert.True(pacotes.Count(p => p.EstaNoPonto && p.DiasNoPonto > Pacote.DiasLimiteNoPonto) >= 2);
    }
}
