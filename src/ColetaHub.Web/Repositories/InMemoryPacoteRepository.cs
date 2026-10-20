using System.Collections.Concurrent;
using ColetaHub.Web.Models;

namespace ColetaHub.Web.Repositories;

/// <summary>
/// Persistência em memória (sem banco de dados). Registrado como Singleton no Program.cs,
/// então uma única instância vive enquanto a aplicação estiver rodando e é compartilhada
/// entre o site MVC e a API. Ao reiniciar, os dados voltam ao seed.
/// O ConcurrentDictionary e o Interlocked tornam o acesso seguro entre requisições simultâneas.
/// </summary>
public class InMemoryPacoteRepository : IPacoteRepository
{
    private readonly ConcurrentDictionary<int, Pacote> _pacotes = new();
    private int _ultimoId;

    public InMemoryPacoteRepository(IEnumerable<Pacote> pacotesIniciais)
    {
        foreach (var pacote in pacotesIniciais)
        {
            Adicionar(pacote);
        }
    }

    public IReadOnlyList<Pacote> ObterTodos() =>
        _pacotes.Values
            .OrderByDescending(p => p.DataRecebimento)
            .ThenByDescending(p => p.Id)
            .Select(p => p.Clonar())
            .ToList();

    public Pacote? ObterPorId(int id) =>
        _pacotes.TryGetValue(id, out var pacote) ? pacote.Clonar() : null;

    public Pacote Adicionar(Pacote pacote)
    {
        var novo = pacote.Clonar();
        novo.Id = Interlocked.Increment(ref _ultimoId);
        _pacotes[novo.Id] = novo;
        return novo.Clonar();
    }

    public bool Atualizar(Pacote pacote)
    {
        if (!_pacotes.TryGetValue(pacote.Id, out var atual))
        {
            return false;
        }

        return _pacotes.TryUpdate(pacote.Id, pacote.Clonar(), atual);
    }

    public bool Remover(int id) => _pacotes.TryRemove(id, out _);

    public IReadOnlyList<Pacote> Pesquisar(FiltroPacote filtro)
    {
        IEnumerable<Pacote> consulta = _pacotes.Values;

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            consulta = consulta.Where(p =>
                p.CodigoRastreio.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                p.NomeDestinatario.Contains(texto, StringComparison.OrdinalIgnoreCase));
        }

        if (filtro.Status.HasValue)
        {
            consulta = consulta.Where(p => p.Status == filtro.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Marketplace))
        {
            consulta = consulta.Where(p => p.Marketplace.Equals(filtro.Marketplace, StringComparison.OrdinalIgnoreCase));
        }

        if (filtro.RecebidoDe.HasValue)
        {
            consulta = consulta.Where(p => p.DataRecebimento.Date >= filtro.RecebidoDe.Value.Date);
        }

        if (filtro.RecebidoAte.HasValue)
        {
            consulta = consulta.Where(p => p.DataRecebimento.Date <= filtro.RecebidoAte.Value.Date);
        }

        return consulta
            .OrderByDescending(p => p.DataRecebimento)
            .ThenByDescending(p => p.Id)
            .Select(p => p.Clonar())
            .ToList();
    }

    public bool CodigoJaCadastrado(string codigoRastreio, int? ignorarId = null) =>
        _pacotes.Values.Any(p =>
            p.Id != ignorarId &&
            p.CodigoRastreio.Equals(codigoRastreio, StringComparison.OrdinalIgnoreCase));
}
