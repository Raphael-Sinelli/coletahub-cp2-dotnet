using ColetaHub.Web.Models;

namespace ColetaHub.Web.Repositories;

/// <summary>
/// Contrato de acesso aos pacotes. Controllers dependem desta interface (Repository + DI),
/// então trocar a memória por um banco no futuro não exige mudar nenhum Controller.
/// </summary>
public interface IPacoteRepository
{
    IReadOnlyList<Pacote> ObterTodos();

    Pacote? ObterPorId(int id);

    /// <summary>Armazena o pacote com um novo Id incremental e retorna a versão salva.</summary>
    Pacote Adicionar(Pacote pacote);

    /// <summary>Substitui o pacote de mesmo Id. Retorna false se o Id não existir.</summary>
    bool Atualizar(Pacote pacote);

    /// <summary>Remove o pacote. Retorna false se o Id não existir.</summary>
    bool Remover(int id);

    IReadOnlyList<Pacote> Pesquisar(FiltroPacote filtro);

    /// <summary>Verifica se o código já pertence a outro pacote (ignorando o próprio, na edição).</summary>
    bool CodigoJaCadastrado(string codigoRastreio, int? ignorarId = null);
}
